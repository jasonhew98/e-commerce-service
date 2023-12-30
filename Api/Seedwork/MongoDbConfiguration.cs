using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Infrastructure.Seedwork
{
    public static class MongoDbConfiguration
    {
        public static void RegisterDefault()
        {
            var conventionPack = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention(),
                new ImmutableTypeClassMapConvention()
            };

            ConventionRegistry.Register("defaultConvention", conventionPack, t => true);

            BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.UtcInstance);
            BsonSerializer.RegisterSerializer(typeof(JObject), new JObjectSerializer());
            BsonSerializer.RegisterSerializer(typeof(JToken), new JTokenSerializer());
            BsonSerializer.RegisterSerializer(typeof(JArray), new JArraySerializer());
        }
    }

    public class JObjectSerializer : SerializerBase<JObject>
    {
        public override JObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context, args);
            return JObject.Parse(bsonDocument.ToString());
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JObject value)
        {
            if (value == null)
            {
                BsonDocumentSerializer.Instance.Serialize(context, args, new BsonDocument());
            }
            else
            {
                var bsonDocument = BsonDocument.Parse(value?.ToString());
                BsonDocumentSerializer.Instance.Serialize(context, args, bsonDocument);
            }
        }
    }

    public class JArraySerializer : SerializerBase<JArray>
    {
        public override JArray Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context, args);
            return JArray.Parse(bsonDocument.ToString());
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JArray value)
        {
            var bsonArray = BsonSerializer.Deserialize<BsonArray>(value?.ToString());
            BsonArraySerializer.Instance.Serialize(context, args, bsonArray);
        }
    }

    public class JTokenSerializer : SerializerBase<JToken>
    {
        public override JToken Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context, args);
            return JToken.Parse(bsonDocument.ToString());
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JToken value)
        {
            switch (value)
            {
                case JValue jValue:
                    {
                        var jObject = new JObject { { "result", jValue } };
                        var bsonDocument = BsonDocument.Parse(jObject.ToString());
                        BsonDocumentSerializer.Instance.Serialize(context, args, bsonDocument);
                        return;
                    }
                default:
                case JContainer jContainer:
                    {
                        var bsonDocument = RawBsonDocumentHelper.FromJson(value.ToString());
                        BsonDocumentSerializer.Instance.Serialize(context, args, bsonDocument);
                        return;
                    }
            }
        }
    }

    public static class RawBsonDocumentHelper
    {
        public static RawBsonDocument FromBsonDocument(BsonDocument document)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var bsonWriter = new BsonBinaryWriter(memoryStream, BsonBinaryWriterSettings.Defaults))
                {
                    var context = BsonSerializationContext.CreateRoot(bsonWriter);
                    BsonDocumentSerializer.Instance.Serialize(context, document);
                }

                return new RawBsonDocument(memoryStream.ToArray());
            }
        }

        public static RawBsonDocument FromJson(string json)
        {
            return FromBsonDocument(BsonDocument.Parse(json));
        }
    }
}