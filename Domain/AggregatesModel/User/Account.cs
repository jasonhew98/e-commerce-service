using Domain.Model;
using Domain.Seedwork;
using System;
using System.Collections.Generic;

namespace Domain.AggregatesModel.AccountAggregate
{
    public class Account : AuditableEntity, IAggregateRoot
    {
        public string AccountId { get; private set; }
        public string FullName { get; private set; }
        public string Password { get; private set; }
        public string Email { get; private set; }
        public List<Attachment> ProfilePictures { get; private set; }

        public Account(
            string fullName,
            string email,
            List<Attachment> profilePictures = null,
            string accountId = null,
            string password = null,
            string createdBy = null,
            string createdByName = null,
            DateTime? createdUTCDateTime = null,
            string modifiedBy = null,
            string modifiedByName = null,
            DateTime? modifiedUTCDateTime = null)
            : base(
                  createdBy: createdBy,
                  createdByName: createdByName,
                  createdUTCDateTime: createdUTCDateTime,
                  modifiedBy: modifiedBy,
                  modifiedByName: modifiedByName,
                  modifiedUTCDateTime: modifiedUTCDateTime)
        {
            AccountId = accountId;
            FullName = fullName;
            Password = password;
            Email = email;
            ProfilePictures = profilePictures;
        }

        public void UpdateAccountDetails(Account account)
        {
            FullName = account.FullName;
            Email = account.Email;
            ProfilePictures = account.ProfilePictures;
        }
    }
}
