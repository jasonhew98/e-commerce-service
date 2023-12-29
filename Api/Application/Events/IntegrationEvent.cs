using System;

namespace Api.Application.Events
{
    public abstract class IntegrationEvent
    {
        public string IntegrationEventId { get; set; } = Guid.NewGuid().ToString();
        public string ActionBy { get; set; }
        public string ActionByName { get; set; }
        public DateTime ActionByUTCDateTime { get; set; }

        public IntegrationEvent(
            string actionBy,
            string actionByName,
            DateTime actionByUTCDateTime)
        {
            ActionBy = actionBy;
            ActionByName = actionByName;
            ActionByUTCDateTime = actionByUTCDateTime;
        }
    }
}
