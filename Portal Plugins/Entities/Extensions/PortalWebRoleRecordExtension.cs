
using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a Portal Web Role record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class mspp_webrole : IPortalWebRoleRecord
    {
		#region Interrogation Properties
        #endregion

        #region Data Properties

        public string Name
        {
            get => mspp_name;
            set
            {
				mspp_name = value;
            }
        }

        public string Description
        {
            get => mspp_description;

            set
            {
				mspp_description = value;
            }
        }

        public EntityReference WebsiteRef
        {
            get => mspp_websiteid;
            set
            {
                mspp_websiteid = value;
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

