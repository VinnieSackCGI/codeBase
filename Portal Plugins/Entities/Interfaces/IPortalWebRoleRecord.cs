
using System;

using Microsoft.Xrm.Sdk;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Data layer from the Portal Web Role table (mspp_webrole)
    /// </summary>
    public interface IPortalWebRoleRecord
    {

		#region Interrogation Properties
		#endregion

		#region Data fields

		Guid Id { get; set; }

		string Name  { get; set; }
        string Description { get; set; }

		EntityReference WebsiteRef { get; set; }

		//		string WebsiteName { get; set; }

		Entity Entity { get; }

		#endregion

	}

}
