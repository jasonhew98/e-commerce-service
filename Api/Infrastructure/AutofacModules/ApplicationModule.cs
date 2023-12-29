using Api.Application.Events;
using Api.Infrastructure.Services;
using Api.Seedwork.AesEncryption;
using Autofac;
using Autofac.Features.ResolveAnything;
using Domain.AggregatesModel.AccountAggregate;
using Domain.AggregatesModel.ProductAggregate;
using Infrastructure.Repositories;
using Infrastructure.Seedwork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Api.Infrastructure.AutofacModules
{
    public class ApplicationModule : Autofac.Module
    {
        private readonly ServiceConfiguration _serviceConfiguration;

        public ApplicationModule(ServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CoreMongoDbContext>().As<IMongoContext>().InstancePerLifetimeScope()
                .WithParameter((pi, ctx) => pi.Name == "mongoDbUrl",
                    (p, c) => c.Resolve<IOptions<CoreRepositoryOptions>>().Value.MongoDbUrl)
                .WithParameter((pi, ctx) => pi.Name == "database",
                    (p, c) => c.Resolve<IOptions<CoreRepositoryOptions>>().Value.Database);
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();

            Func<ParameterInfo, IComponentContext, bool> parameterSelector = (pi, ctx) => pi.Name == "collectionName";

            builder.RegisterType<MongoDbProductRepository>().As<IProductRepository>().InstancePerLifetimeScope()
                .WithParameter(parameterSelector,
                    (p, c) => c.Resolve<IOptions<CoreRepositoryOptions>>().Value.ProductCollectionName);

            builder.RegisterType<MongoDbAccountRepository>().As<IAccountRepository>().InstancePerLifetimeScope()
                .WithParameter(parameterSelector,
                    (p, c) => c.Resolve<IOptions<CoreRepositoryOptions>>().Value.AccountCollectionName);

            builder.RegisterType<AesSecurity>().As<IAesSecurity>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ApiGoogleDriveService>().As<GoogleDriveService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ApiFileService>().As<FileService>()
                .InstancePerLifetimeScope();

            builder.Register((c, p) =>
            {
                var accessor = c.Resolve<IHttpContextAccessor>();
                var logger = c.Resolve<ILogger<ContainerBuilder>>();

                /*var headerAuthorization =
                    accessor.HttpContext.Request.Headers["authorization"].ToString() ?? string.Empty;
                var token = "";

                if (!string.IsNullOrEmpty(headerAuthorization))
                {
                    logger.LogInformation("Request authorization header value: {value}", headerAuthorization);
                    token = headerAuthorization.Replace("Bearer", "").Trim();
                }*/

                return true;
            }).InstancePerDependency();

            builder.RegisterSource(
                new AnyConcreteTypeNotAlreadyRegisteredSource(type => type.Namespace != null && !type.Namespace.Contains("Microsoft"))
                .WithRegistrationsAs(b => b.InstancePerDependency()));
        }
    }
}
