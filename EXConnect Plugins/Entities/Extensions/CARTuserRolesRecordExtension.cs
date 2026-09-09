using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
    public partial class rcade_CARTUserRoles : ICARTuserRoleRecord
    {
        #region Data Properties

        public string Name
        {
            get => rcade_Name;

            set
            {
                rcade_Name = value;
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
