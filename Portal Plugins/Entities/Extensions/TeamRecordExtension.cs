
using System;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities
{

    /// <summary>
    /// A table adapter class for a UserRoleAssociation record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class Team : ITeamRecord
    {
        #region Interrogation Properties

        public bool HasUpdatedEmail
        {
            get
            {
                return Attributes.Contains("emailaddress");
            }
        }

        public bool HasUpdatedName
        {
            get
            {
                return Attributes.Contains("name");
            }
        }

        #endregion

        #region Data Properties

        public string EmailAddress
        {
            get => EMailAddress;
            set
            {
                EmailAddress = value;
            }
        }
        public string Type
        {
            get
            {
                return this.TeamType.ToString();
            }
            set
            {
                Team_TeamType type;
                if (Enum.TryParse<Team_TeamType>(value, out type))
                {
                    this.TeamType = type;
                }
                else
                {
                    this.TeamType = null;
                }
            }
        }

        public string TypeMembership
        {
            get
            {
                return MembershipType.ToString();
            }
            set
            {
                Team_MembershipType type;
                if (Enum.TryParse<Team_MembershipType>(value, out type))
                {
                    MembershipType = type;
                }
                else
                {
                    MembershipType = null;
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

