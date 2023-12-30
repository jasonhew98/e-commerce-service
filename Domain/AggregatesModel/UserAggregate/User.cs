using Domain.Model;
using Domain.Seedwork;
using System;
using System.Collections.Generic;

namespace Domain.AggregatesModel.UserAggregate
{
    public class User : AuditableEntity, IAggregateRoot
    {
        public string UserId { get; private set; }
        public string UserName { get; private set; }
        public string FullName { get; private set; }
        public string Password { get; private set; }
        public string Email { get; private set; }
        public List<Attachment> ProfilePictures { get; private set; }

        public User(
            string fullName,
            string email,
            string userName = null,
            List<Attachment> profilePictures = null,
            string userId = null,
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
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Password = password;
            Email = email;
            ProfilePictures = profilePictures;
        }

        public void UpdateUserDetails(User user)
        {
            UserName = user.UserName;
            FullName = user.FullName;
            Email = user.Email;
            ProfilePictures = user.ProfilePictures;
        }
    }
}
