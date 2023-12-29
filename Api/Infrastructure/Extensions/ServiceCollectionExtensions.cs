using Api.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace Api.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddRabbitMQEventBus(this IServiceCollection services, string connectionUrl, string brokerName, string queueName, int timeoutBeforeReconnecting = 15)
		{
			services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
			services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
			{
				var connectionFactory = new ConnectionFactory
				{
					Uri = new Uri(connectionUrl),
					DispatchConsumersAsync = true,
				};

				var logger = factory.GetService<ILogger<RabbitMQPersistentConnection>>();
				return new RabbitMQPersistentConnection(connectionFactory, logger, timeoutBeforeReconnecting);
			});

			services.AddSingleton<IIntegrationEventService, IntegrationEventService>(factory =>
			{
				var persistentConnection = factory.GetService<IPersistentConnection>();
				var subscriptionManager = factory.GetService<IEventBusSubscriptionManager>();
				var logger = factory.GetService<ILogger<IntegrationEventService>>();

				return new IntegrationEventService(persistentConnection, subscriptionManager, factory, logger, brokerName, queueName);
			});
		}
	}
}
