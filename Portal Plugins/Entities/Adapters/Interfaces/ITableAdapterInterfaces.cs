using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Portal_Plugins.Entities.Interfaces;
using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

namespace Portal_Plugins.Entities.Adapters.Interfaces
{
	public interface IContactAdapter : IUserTableAdapter, ITableAdapter<IContactRecord> { }

	public interface IWebRoleAdapter : ITableAdapter<IWebRolesRecord>
	{
		IWebRolesRecord GetRecordFromId(Guid id);
		IWebRolesRecord GetRecordFromName(string name);
	}
	public interface IPortalWebRoleAdapter : ITableAdapter<IPortalWebRoleRecord>
	{
		IPortalWebRoleRecord GetRecordFromId(Guid id);
		IPortalWebRoleRecord GetRecordFromName(string name);
		EntityReference F3S_WebSiteRef { get; }
	}
	public interface ISharePointDocumentLocationAdapter : ITableAdapter<ISharePointDocumentLocationRecord>
	{
		ISharePointDocumentLocationRecord GetRecordFromId(Guid id);
		ISharePointDocumentLocationRecord GetRecordFromRelativeUrl(string name);
		ISharePointDocumentLocationRecord GetRecordFromDocumentId(Guid docId);
	}

	public interface ISupportDocumentsAdapter : ITableAdapter<ISupportDocumentsRecord>
	{
		ISupportDocumentsRecord GetRecordFromId(Guid id);
		ISupportDocumentsRecord GetRecordFromName(string catalogName);
	}

	public interface IDocumentCatalogSPactionAdapter : ITableAdapter<IDocumentCatalogSPactionRecord> { }

	public interface ISupportDocumentsUserSPactionAdapter : ITableAdapter<ISupportDocumentsUserSPactionRecord> { }

}
