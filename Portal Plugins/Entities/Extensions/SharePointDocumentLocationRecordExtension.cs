
using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

    /// <summary>
    /// A table adapter class for a F3S SharePoint Document Location record to isolate the data access from the consumer.
    /// Promotes loose couplings against the data layer.
    /// </summary>
    public partial class SharePointDocumentLocation : ISharePointDocumentLocationRecord
	{
		#region Interrogation Properties
		#endregion

		#region Data Properties

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

