
using System;

using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

	/// <summary>
	/// A table adapter class for a F3S Document Catalog SP Action record to isolate the data access from the consumer.
	/// Promotes loose couplings against the data layer.
	/// </summary>
	public partial class f3s_DocumentCatalogSPAction : IDocumentCatalogSPactionRecord
	{
		#region Interrogation Properties
		#endregion

		#region Data Properties

		public string Action
		{
			get
			{
				if (f3s_Action == null)
				{
					return null;
				}
				return f3s_Action.ToString();
			}
            set
            {
				if (!string.IsNullOrEmpty(value))
				{
					f3s_DocumentCatalogSPAction_f3s_Action action;
					if (Enum.TryParse<f3s_DocumentCatalogSPAction_f3s_Action>(value, out action))
					{
						f3s_Action = action;
					}
				}
				else
				{
					f3s_Action = null;
				}
            }
		}

		public string CatalogName
        {
            get => f3s_CatalogName;
            set
            {
                f3s_CatalogName = value;
            }
        }

        public string NewCatalogName
        {
            get => f3s_NewCatalogName;
            set
            {
                f3s_NewCatalogName = value;
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

