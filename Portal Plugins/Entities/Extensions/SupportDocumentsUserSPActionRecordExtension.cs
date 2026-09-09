using System;

using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities
{
    using Interfaces;

	/// <summary>
	/// A table adapter class for a F3S Support Documents User SP Action record to isolate the data access from the consumer.
	/// Promotes loose couplings against the data layer.
	/// </summary>
	public partial class f3s_SupportDocumentsUserSPAction : ISupportDocumentsUserSPactionRecord
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
					f3s_SupportDocumentsUserSPAction_f3s_Action action;
					if (Enum.TryParse<f3s_SupportDocumentsUserSPAction_f3s_Action>(value, out action))
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

		public string EmailAddress
        {
            get => f3s_UserName;
            set
            {
				f3s_UserName = value;
            }
        }

        public string PermissionLevel
        {
            get
			{
				if (f3s_PermissionLevel == null)
				{
					return null;
				}
				else
				{
					return f3s_PermissionLevel.ToString();
				}

			} 
            set
            {
				if (!string.IsNullOrEmpty(value))
				{
					f3s_SupportDocumentsUserSPAction_f3s_PermissionLevel level;
					if (Enum.TryParse<f3s_SupportDocumentsUserSPAction_f3s_PermissionLevel>(value, out level))
					{
						f3s_PermissionLevel = level;
					}
				}
				else
				{
					f3s_PermissionLevel = null;
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

