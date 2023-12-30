using Domain.Seedwork;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Infrastructure.Seedwork
{
    public static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, MongoDbContext ctx)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.DomainEvents != null && x.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities.SelectMany(x => x.DomainEvents).ToList();

            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            var tasks = domainEvents.Select(async domainEvent => { await mediator.Publish(domainEvent); });

            await Task.WhenAll(tasks);
        }
    }

    public static class MongoDbContextExtension
    {
    }

    public interface IUnitOfWork : IDisposable
    {
        Task<bool> Commit();
    }

    public interface IMongoContext : IDisposable
    {
        void AddCommand(Func<Task> func, object entry = null);
        IMongoCollection<T> GetCollection<T>(string name);
        Task<int> SaveChanges();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMongoContext _context;

        public UnitOfWork(IMongoContext context)
        {
            _context = context;
        }

        public async Task<bool> Commit()
        {
            var changeAmount = await _context.SaveChanges();
            return changeAmount > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public class ChangeTracker
    {
        private readonly List<Object> _entries;

        public ChangeTracker()
        {
            _entries = new List<object>();
        }

        public void Add(object entry) => _entries.Add(entry);

        public IEnumerable<T> Entries<T>() => _entries.OfType<T>();
    }

    public class MongoDbContext : IMongoContext
    {
        public readonly MongoClient MongoClient;
        public readonly ChangeTracker ChangeTracker;
        private readonly IMongoDatabase _database;
        private readonly List<Func<Task>> _commands;
        private readonly IMediator _mediator;

        public IClientSessionHandle Session { get; private set; }

        public MongoDbContext(string connectionUrl, string database, IMediator mediator)
        {
            _mediator = mediator;
            var mongoClient = new MongoClient(connectionUrl);
            _database = mongoClient.GetDatabase(database);
            MongoClient = mongoClient;
            ChangeTracker = new ChangeTracker();
            _commands = new List<Func<Task>>(); //Every command will be stored and it'll be processed at SaveChanges
        }

        public async Task<int> SaveChanges()
        {
            try
            {
                await _mediator.DispatchDomainEventsAsync(this);

                using (Session = await MongoClient.StartSessionAsync())
                {
                    Session.StartTransaction();
                    var commandTasks = _commands.Select(c => c());
                    await Task.WhenAll(commandTasks);
                    await Session.CommitTransactionAsync();
                }

                return _commands.Count;
            }
            catch (Exception ex)
            {
                var aggregatedException = ex as AggregateException;
                throw;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);

        public void AddCommand(Func<Task> func, object entry = null)
        {
            _commands.Add(func);
            if (entry != null)
                ChangeTracker.Add(entry);
        }

        public void Dispose()
        {
            Session?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public abstract class MongoDbRepository<T> :
        IRepository<T> where T : Entity, IAggregateRoot
    {
        public readonly IMongoCollection<T> DbSet;
        protected readonly IMongoContext Context;

        protected MongoDbRepository(IMongoContext context, string collectionName = "")
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                var name = typeof(T).Name;
                collectionName = char.ToLowerInvariant(name[0]) + name.Substring(1);
            }

            Context = context;
            DbSet = Context.GetCollection<T>(collectionName);
        }

        public virtual void Add(T obj)
        {
            if (obj == null)
                return;
            Context.AddCommand(() => DbSet.InsertOneAsync(obj), obj);
        }

        public virtual void AddMany(IEnumerable<T> documents)
        {
            if (documents == null)
                return;
            Context.AddCommand(() => DbSet.InsertManyAsync(documents), documents);
        }

        public virtual void Update(T obj, Expression<Func<T, bool>> filter)
        {
            Context.AddCommand(() => DbSet.ReplaceOneAsync(filter, obj), obj);
        }

        public virtual void Remove(Expression<Func<T, bool>> filter)
        {
            Context.AddCommand(() => DbSet.DeleteOneAsync(filter));
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        protected ProjectionDefinition<T> DefaultProjectionDefinition => Builders<T>.Projection.Exclude("_id");

        public virtual async Task<IEnumerable<T>> Query()
        {
            var options = new FindOptions<T, BsonDocument>
            {
                Projection = DefaultProjectionDefinition,
            };

            var data = await DbSet.FindAsync(FilterDefinition<T>.Empty, options);
            var documents = await data.ToListAsync();
            var results = documents.Select(ConvertBsonToEntity);
            return results;
        }

        public virtual async Task<IEnumerable<T>> Query(Expression<Func<T, bool>> filter)
        {
            var options = new FindOptions<T, BsonDocument>
            {
                Projection = DefaultProjectionDefinition,
            };

            var data = await DbSet.FindAsync(FilterDefinition<T>.Empty, options);
            var documents = await data.ToListAsync();
            var results = documents.Select(ConvertBsonToEntity);
            return results;
        }

        protected async Task<IEnumerable<T>> Query(JObject filter, FindOptions<T, BsonDocument> options = null)
        {
            var data = await DbSet.FindAsync<BsonDocument>(filter.ToString(), options);
            var documents = await data.ToListAsync();
            var results = documents.Select(ConvertBsonToEntity);
            return results;
        }

        public virtual async Task<IEnumerable<TResult>> Query<TResult>(FilterDefinition<T> filter,
            FindOptions<T, BsonDocument> options = null)
        {
            var data = await DbSet.FindAsync<BsonDocument>(filter, options);
            var documents = await data.ToListAsync();
            var test = documents.Where(d => d != null);
            var results = documents.Where(d => d != null)
                .Select(d => JsonConvert.DeserializeObject<TResult>(ToJson(d)));
            return results;
        }

        public virtual async Task<IEnumerable<T>> Query(FilterDefinition<T> filter,
            FindOptions<T, BsonDocument> options = null)
        {
            var data = await DbSet.FindAsync<BsonDocument>(filter, options);
            var documents = await data.ToListAsync();
            var test = documents.Where(d => d != null);
            var results = documents.Where(d => d != null)
                .Select(d => JsonConvert.DeserializeObject<T>(ToJson(d)));
            return results;
        }

        public virtual async Task<T> QueryOne(Expression<Func<T, bool>> filter)
        {
            var options = new FindOptions<T, BsonDocument>
            {
                Projection = DefaultProjectionDefinition
            };

            var data = await DbSet.FindAsync(filter, options);
            var document = await data.SingleOrDefaultAsync();
            return document == null ? null : ConvertBsonToEntity(document);
        }

        public virtual async Task<T> QueryOne(FilterDefinition<T> filter, FindOptions<T, BsonDocument> options)
        {
            var results = await DbSet.FindAsync(filter, options);
            var document = await results.SingleOrDefaultAsync();
            return document == null ? null : ConvertBsonToEntity(document);
        }

        protected virtual T ConvertBsonToEntity(BsonDocument document) =>
            document == null ? default : JsonConvert.DeserializeObject<T>(ToJson(document));

        protected virtual TResults ConvertBsonToEntity<TResults>(BsonDocument document) =>
            document == null ? default : JsonConvert.DeserializeObject<TResults>(ToJson(document));

        private string ToJson(BsonDocument bson)
        {
            using var stream = new MemoryStream();
            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, typeof(BsonDocument), bson);
            }

            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new Newtonsoft.Json.Bson.BsonReader(stream);
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            using (var jWriter = new JsonTextWriter(sw))
            {
                jWriter.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                jWriter.WriteToken(reader);
            }

            return sb.ToString();
        }
    }
}