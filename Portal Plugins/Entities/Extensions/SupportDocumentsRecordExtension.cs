
using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a F3S Support Documents record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class f3s_SupportDocuments : ISupportDocumentsRecord
	{
		#region Interrogation Properties

		public bool HasUpdatedName
		{
			get
			{
				return this.Attributes.Contains("f3s_name");
			}
		}

		#endregion

		#region Data Properties

		public string CatalogName
        {
            get => f3s_Name;

            set
            {
                f3s_Name = value;
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

