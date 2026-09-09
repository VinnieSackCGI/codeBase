
using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a Web Role record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class f3s_WebRoles : IWebRolesRecord
    {
		#region Interrogation Properties

		public bool HasUpdatedName
		{
			get
			{
				return Attributes.Contains("f3s_rolename");
			}
		}

		public bool HasUpdatedDescription
		{
			get
			{
				return Attributes.Contains("f3s_description");
			}
		}

		#endregion

		#region Data Properties

		public string Name
        {
            get => f3s_RoleName;

            set
            {
				f3s_RoleName = value;
            }
        }

        public string Description
        {
            get => f3s_Description;

            set
            {
				f3s_Description = value;
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

