using MediatR;
using System;
using System.Collections.Generic;

namespace Domain.Seedwork
{
    public abstract class Entity
    {
        private List<INotification> _domainEvents;

        public List<INotification> DomainEvents
        {
            get
            {
                if (_domainEvents == null)
                {
                    _domainEvents = new List<INotification>();
                }
                return _domainEvents;
            }
        }

        protected Entity()
        {
            _domainEvents = new List<INotification>();
        }

        public void AddDomainEvent(INotification eventItem)
        {
            if (eventItem == null)
                return;

            DomainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            DomainEvents.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            DomainEvents.Clear();
        }
    }

    public abstract class AuditableEntity : Entity
    {
        public string CreatedBy { get; protected set; }
        public string CreatedByName { get; protected set; }
        public DateTime? CreatedUTCDateTime { get; protected set; }
        public string ModifiedBy { get; protected set; }
        public string ModifiedByName { get; protected set; }
        public DateTime? ModifiedUTCDateTime { get; protected set; }

        private DateTime? _originalModifiedUTCDateTime;
        public DateTime? OriginalModifiedUTCDateTime => _originalModifiedUTCDateTime;

        public bool IsNew => (DateTime.UtcNow - CreatedUTCDateTime)?.TotalDays < 3;
        public bool IsUpdated => !IsNew && ((DateTime.UtcNow - ModifiedUTCDateTime)?.TotalDays < 3);

        protected AuditableEntity(
            string createdBy = "",
            string createdByName = "",
            DateTime? createdUTCDateTime = null,
            string modifiedBy = "",
            string modifiedByName = "",
            DateTime? modifiedUTCDateTime = null)
        {
            CreatedBy = createdBy;
            CreatedByName = createdByName;
            CreatedUTCDateTime = createdUTCDateTime;

            if (string.IsNullOrEmpty(modifiedBy))
            {
                ModifiedBy = createdBy;
                ModifiedByName = createdByName;
                ModifiedUTCDateTime = createdUTCDateTime;
            }
            else
            {
                ModifiedBy = modifiedBy;
                ModifiedByName = modifiedByName;
                ModifiedUTCDateTime = modifiedUTCDateTime ?? createdUTCDateTime;
            }
        }

        public void SetModified((string id, string name) user)
        {
            var (id, name) = user;
            ModifiedBy = id;
            ModifiedByName = name;
            _originalModifiedUTCDateTime = ModifiedUTCDateTime;
            ModifiedUTCDateTime = DateTime.Now;
        }
    }
}
