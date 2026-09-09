using Plugins_CommonLibrary.Entities.Interfaces;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using Portal_Plugins.Entities.Adapters.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
	public interface IRepository : IBaseRepository
    {
 		#region System tables

		ISystemUserAdapter SystemUser { get; }
        IContactAdapter Contact { get; }
//        IEnvironmentVariablesAdapter EnvironmentVariables { get; }
        ITeamAdapter Team { get; }

		#endregion

		IWebRoleAdapter WebRole { get; }
		IPortalWebRoleAdapter PortalWebRole { get; }
		ISharePointDocumentLocationAdapter SPDocumentLocation { get; }
		ISupportDocumentsAdapter SupportDoc { get; }
		IDocumentCatalogSPactionAdapter CatalogSPaction { get; }

		ISupportDocumentsUserSPactionAdapter UserSPaction { get; }

	}
}
