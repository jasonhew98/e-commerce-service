using Api.Application.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Application.IntegrationEvents
{
    public class ProductAddedIntegrationEvent : IntegrationEvent
    {
        public string ProductId { get; set; }

        public ProductAddedIntegrationEvent(
            string productId,
            string actionBy,
            string actionByName,
            DateTime actionByUTCDateTime) : base(actionBy, actionByName, actionByUTCDateTime)
        {
            ProductId = productId;
        }
    }

    public class ProductAddedIntegrationEventHandler : IIntegrationEventHandler<ProductAddedIntegrationEvent>
    {
        private readonly ILogger<ProductAddedIntegrationEvent> _logger;

        public ProductAddedIntegrationEventHandler(
            ILogger<ProductAddedIntegrationEvent> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ProductAddedIntegrationEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
