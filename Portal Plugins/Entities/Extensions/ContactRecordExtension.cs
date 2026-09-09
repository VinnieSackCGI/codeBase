
using System;

using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a UserRoleAssociation record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class Contact : IContactRecord
    {
        #region Interrogation Properties

        public bool HasUpdatedEmail
        {
            get
            {
                return Attributes.Contains("emailaddress1");
            }
        }

        public bool HasUpdatedName
        {
            get
            {
                return Attributes.Contains("fullname");
            }
        }

        public bool IsActive
        {
            get
            {
                return StateCode == ContactState.Active;
            }
        }

        public bool IsEmailConfirmed
        {
            get
            {
                return adx_identity_emailaddress1confirmed.GetValueOrDefault() == true;
            }
        }

        #endregion

        #region Data Properties

        public string Name
        {
            get
            {
                return FullName;
            }
#if UNITTEST
            set
            {
                FullName = value;
            }
#endif
        }


        public string EmailAddress
        {
            get
            {
                return EMailAddress1;
            }
#if UNITTEST
            set
            {
                EMailAddress1 = value;
            }
#endif
        }

        public string State
        {
            get
            {
                return StateCode.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    ContactState state;
                    if (Enum.TryParse<ContactState>(value, out state))
                    {
                        StateCode = state;
                    }
                }
                else
                {
                    StateCode = null;
                }
            }
        }

        public Entity Entity
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}

