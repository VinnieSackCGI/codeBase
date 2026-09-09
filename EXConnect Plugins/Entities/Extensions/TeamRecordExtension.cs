using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
    public partial class Team : ITeamRecord
    {
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
