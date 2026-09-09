
using System;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Plugins_CommonLibrary.Plugin_Handling;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

using Portal_Plugins.Plugin_Handling;

namespace Portal_Plugins.Plugins
{
	[CrmPluginRegistration(MessageNameEnum.Create,
	"f3s_supportdocuments",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "",
	"Support Documents Catalog Name Duplicate Prevention - Create", 1,
	IsolationModeEnum.Sandbox,
	Description = "Prevent Duplicate Support Documents Catalog name on Record Create",
	Id = "1e50b797-a991-42c0-81e7-4ff7fd70ffde")]

	[CrmPluginRegistration(MessageNameEnum.Create,
	"f3s_supportdocuments",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Support Documents Catalog Name Creation", 1,
	IsolationModeEnum.Sandbox,
	Description = "Modify the default SharePoint Catalog name on Record Create",
	Id = "414adf65-fa5e-4c47-a050-c9fbcca29e22")]

	// Since SharePoint folder Deletion or Renaming does not appear to work (or not available)
	// within the Power Pages SharePoint Integration feature, we have to trigger a Power Automate
	// flow to perform these actions

	[CrmPluginRegistration(MessageNameEnum.Update,
	"f3s_supportdocuments",
	StageEnum.PreOperation,
	ExecutionModeEnum.Synchronous, "f3s_name",
	"Support Documents Catalog Name Duplicate Prevention - Update", 2,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_name,statecode",
	Description = "Prevent Duplicate Support Documents Catalog name on Record Update",
	Id = "82e471ad-7b18-44ab-a56b-bc7203292af0")]

	[CrmPluginRegistration(MessageNameEnum.Update,
	"f3s_supportdocuments",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "f3s_name",
	"Support Documents Catalog Name Update", 2,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_name,statecode",
	Description = "Trigger a flow to update the SharePoint Support Documents Catalog name on Record Update",
	Id = "1901603f-946c-4a09-8345-11a0b44e5b55")]

	[CrmPluginRegistration(MessageNameEnum.Delete,
	"f3s_supportdocuments",
	StageEnum.PostOperation,
	ExecutionModeEnum.Synchronous, "",
	"Support Documents Catalog Delete", 3,
	IsolationModeEnum.Sandbox,
	Image1Type = ImageTypeEnum.PreImage,
	Image1Name = ImageTypeName.PreImage,
	Image1Attributes = "f3s_name",
	Description = "Trigger a flow to delete the SharePoint Support Documents Catalog on Record Delete",
	Id = "513f0534-ac5b-43e0-871a-76f8fcafdb5d")]

	public class SharePointSupportDocumentsCatalogPlugin : MainPlugin
	{
		protected override void ExecutePlugin(IExtendedPluginContext executeContext)
		{
			InitializeContext(executeContext);

			tracer.Trace("Beginning execution of F3S Suppport Documents Catalog");

			if (this.context.MessageName == MessageNameEnum.Delete.ToString())
			{

				// A Deleted record does not provide a TargetEntity but a TargetEntityReference
				// so we can deal with the PreImage directly.
				var oldSupportDocRecord = dbService.SupportDoc.GetRecordFromEntity(context.GetPreImage());

				tracer.Trace($"Processing a Delete request for Support Documents folder \"{oldSupportDocRecord.CatalogName}\".");

				// Store the action into the Document Catalog SP Action table to trigger the Power Automate flow
				var catalogSPactionRecord = dbService.CatalogSPaction.CreateRecord();
				catalogSPactionRecord.Action = "Delete";
				catalogSPactionRecord.CatalogName = oldSupportDocRecord.CatalogName;
				if ( dbService.Create(catalogSPactionRecord.Entity, true) == null)
				{
					tracer.Trace("The Document Catalog SP Action record could not be created.");
				}
			}
			else
			{
				// Get the Support Documents record object from the target entity
				var newSupportDocRecord = dbService.SupportDoc.GetRecordFromEntity(context.GetTargetEntity());

				tracer.Trace($"Processing \"{this.context.MessageName}\" action for Catalog Name \"{newSupportDocRecord.CatalogName}\" with Id = {newSupportDocRecord.Id}.");

				if (this.context.MessageName == MessageNameEnum.Create.ToString())
				{
					if (context.ExecutionContext.Stage == (int)StageEnum.PreOperation)
					{
						// Check that the name is unique
						var recordExists = (dbService.SupportDoc.GetRecordFromName(newSupportDocRecord.CatalogName) != null);
						if (recordExists)
						{
							var msg = $"A Support Document Catalog with name \"{newSupportDocRecord.CatalogName}\" already exists. Duplicate Catalog Names are not allowed.";
							tracer.Trace(msg);
							throw (new InvalidPluginExecutionException(OperationStatus.Failed, msg));
						}
						return;
					}

					// Check for the existance of a corresponding SharePoint Document Location (by id)
					var spDocLocation = dbService.SPDocumentLocation.GetRecordFromDocumentId(newSupportDocRecord.Id);
					if (spDocLocation != null)
					{
						tracer.Trace($"The \"{newSupportDocRecord.CatalogName}\" Catalog Name already exists in the SharePoint Document Location table. Process terminated.");
						return;
					}

					tracer.Trace($"A SharePoint Document Location for the Catalog Name cannot be found, a new record will be created...");

					// Get the Parent Site
					var parentSite = GetDefaultParentSite();

					if (parentSite == null)
					{
						tracer.Trace($"The default SharePoint site cannot be found. Process will terminate");
						return;
					}

					// Get the Document Location record for the default site
					var defaultUrl = "f3s_supportdocuments";
					var parentSiteRef = new EntityReference("sharepointsite", parentSite.Id);

					var defaultSPdocLoc = dbService.SPDocumentLocation.GetRecordFromRelativeUrl(defaultUrl);
					if (defaultSPdocLoc == null)
					{
						tracer.Trace("The default Support Documents Document Location record cannot be found. Creating a new one.");
						defaultSPdocLoc = dbService.SPDocumentLocation.CreateRecord();
						defaultSPdocLoc.Name = "Support Documents on Default Site";
						defaultSPdocLoc.ParentSiteOrLocation = parentSiteRef;
						defaultSPdocLoc.RelativeUrl = defaultUrl;
						defaultSPdocLoc.RegardingObjectId = null;
						defaultSPdocLoc.SiteCollectionId = parentSite.Id;
						defaultSPdocLoc.Id = Guid.NewGuid();
						if(dbService.Create(defaultSPdocLoc.Entity, true) == null)
						{
							tracer.Trace($"The default Support Documents Document Location record could not be created. Process will terminate.");
							return;
						}
					}

					// Add the Document Location record for this catalog

					spDocLocation = dbService.SPDocumentLocation.CreateRecord();
					spDocLocation.Id = Guid.NewGuid();
					spDocLocation.Name = "Support Document on Default Site";
					spDocLocation.ParentSiteOrLocation = defaultSPdocLoc.Entity.ToEntityReference();
					spDocLocation.RegardingObjectId = newSupportDocRecord.Entity.ToEntityReference();
					spDocLocation.RelativeUrl = newSupportDocRecord.CatalogName;
					spDocLocation.SiteCollectionId = parentSite.Id;
					dbService.Create(spDocLocation.Entity, true);
				}
				else if (this.context.MessageName == MessageNameEnum.Update.ToString())
				{
					var oldSupportDocRecord = dbService.SupportDoc.GetRecordFromEntity(context.GetPreImage());

					tracer.Trace($"Updating Support Document Catalog Name from \"{oldSupportDocRecord.CatalogName}\".");

					if (context.ExecutionContext.Stage == (int)StageEnum.PreOperation )
					{
						tracer.Trace("This is Pre-Operation");

						if (newSupportDocRecord.HasUpdatedName)
						{
							tracer.Trace($"Checking for a duplicate for Catalog name \"{newSupportDocRecord.CatalogName}\".");

							// Check that the name is unique if the name is updated
							var recordExists = (dbService.SupportDoc.GetRecordFromName(newSupportDocRecord.CatalogName) != null);
							tracer.Trace("Back from checking");
							if (recordExists)
							{
								var msg = $"A Support DocumentCatalog with name \"{newSupportDocRecord.CatalogName}\" already exists. Duplicate Catalogs are not allowed.";
								tracer.Trace(msg);
								throw (new InvalidPluginExecutionException(OperationStatus.Failed, msg));
							}
						}
						return;
					}

					// Check for the existance of a corresponding SharePoint Document Location (by id)
					var spDocLocation = dbService.SPDocumentLocation.GetRecordFromDocumentId(newSupportDocRecord.Id);
					if (spDocLocation == null)
					{
						tracer.Trace($"The \"{newSupportDocRecord.CatalogName}\" Catalog Name cannot be found in the SharePoint Document Location table. Process terminated.");
						return;
					}

					// Update the record accordingly
					spDocLocation.RelativeUrl = (newSupportDocRecord.HasUpdatedName ? newSupportDocRecord.CatalogName : oldSupportDocRecord.CatalogName);

					dbService.Update(spDocLocation.Entity, true);

					// Although updates to the Document Location table sare taken care of here, we need to deal with updating the SharePoint folder.
					// Store the action into the Document Catalog SP Action table to trigger the Power Automate flow
					var catalogSPactionRecord = dbService.CatalogSPaction.CreateRecord();
					catalogSPactionRecord.Action = "Update";
					catalogSPactionRecord.CatalogName = oldSupportDocRecord.CatalogName;
					catalogSPactionRecord.NewCatalogName = newSupportDocRecord.CatalogName;
					if (dbService.Create(catalogSPactionRecord.Entity, true) == null)
					{
						tracer.Trace("The Document Catalog SP Action record could not be created.");
					}

				}
			}
		}

		#region Auxiliary Methods

		internal class Site
		{
			internal string URL = string.Empty;
			internal Guid Id = Guid.Empty;
		}

		private Site GetDefaultParentSite()
		{
			var fetchXml = $@"
<fetch>
  <entity name='sharepointsite'>
    <attribute name='absoluteurl' />
    <attribute name='sharepointsiteid' />
    <filter>
      <condition attribute='isdefault' operator='eq' value='1' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";

			var defaultSite = new Site();

			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

			if (results.Entities.Count > 0)
			{
				var entity = results.Entities[0];
				if (entity.Id != null)
				{
					defaultSite.Id = entity.Id;
					defaultSite.URL = entity["absoluteurl"].ToString();
				}
			}
			else
			{
				tracer.Trace("No Contact records returned during retrieval of Contact-WebRoles Association list.");
				return null;
			}

			return defaultSite;
		}

        #endregion

    }
}
