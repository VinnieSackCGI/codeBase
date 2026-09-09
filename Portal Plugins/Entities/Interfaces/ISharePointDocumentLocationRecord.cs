
using System;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the F3S SharePoint Document Location table
    /// </summary>
    public interface ISharePointDocumentLocationRecord : ITableRecord
    {

		#region Interrogation Properties
		#endregion

		#region Data fields

		string Name  { get; set; }
        EntityReference ParentSiteOrLocation { get; set; }
        EntityReference RegardingObjectId { get; set; }
        string RelativeUrl { get; set; }
        Guid? SiteCollectionId { get; set; }

        #endregion

    }

}
