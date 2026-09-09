using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities;


using Microsoft.Xrm.Sdk;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk.Messages;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class TemplatesAdapter : ITemplatesAdapter
	{
		private IOrganizationService orgService;
		private IRepository dbService;
		private EXConnect_ServiceContext serviceContext;
		private ITracingService tracer;

		public TemplatesAdapter( IRepository dbService, ITracingService tracer )
		{
			if (dbService == null)
			{
				throw new ArgumentNullException("Business DB Service");
			}
			if (tracer == null)
			{
				throw new ArgumentNullException("Tracing Service");
			}

			this.tracer = tracer;
			this.dbService = dbService;
			this.orgService = dbService.OrganizationService;
			serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public ITemplateRecord CreateRecord()
		{
			ITemplateRecord newRecord = new rcade_CARTTemplates();
			newRecord.Initialize(orgService, tracer);
			return newRecord;
		}

		public ITemplateRecord GetRecordFromEntity( Entity entity )
		{
			ITemplateRecord templateRecord = entity.ToEntity<rcade_CARTTemplates>();
			templateRecord.Initialize(orgService, tracer);
			return templateRecord;
		}

		public ITemplateRecord GetRecordFromId( Guid id )
		{
			var templateRecord = GetAllRecords().Where(b => b.Id == id).FirstOrDefault();
			if (templateRecord != null)
			{
				templateRecord.Initialize(orgService, tracer);
			}
			return templateRecord;
		}

		public IEnumerable<ITemplateRecord> GetAllRecords()
		{
			return serviceContext.rcade_CARTTemplatesSet as IEnumerable<ITemplateRecord>;
		}

		/// <summary>
		/// Sets the Needs Update flag for all active template records
		/// </summary>
		/// <param name="needsUpdate"></param>
		public bool SetNeedsUpdateFlag( bool needsUpdate = true )
		{
			var recordIdList = GetAllRecords().Where(t => t.IsActive).Select(t => t.Id).ToList();

			if (recordIdList.Count > 0)
			{
				// Set the ground work for a multipleRequest
				var multipleRequest = new ExecuteMultipleRequest()
				{
					// Assign settings that define execution behavior: continue on error, return responses.
					Settings = new ExecuteMultipleSettings()
					{
						ContinueOnError = true,
						ReturnResponses = true
					},
					// Create an empty organization request collection.
					Requests = new OrganizationRequestCollection()
				};

				// Update the records found
				foreach (var recordId in recordIdList)
				{
					var entity = new Entity("rcade_carttemplates", recordId);

					var updRequest = new UpdateRequest { Target = entity };
					// add the contactlookup field with the greator's contactid reference
					entity.Attributes["eca_needsupdate"] = needsUpdate;
					multipleRequest.Requests.Add(updRequest);
				}

				var multipleResponse = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);
				return !multipleResponse.IsFaulted;
			}
			else
			{
				tracer.Trace("No records found in the EXConnect Templates table.");
				return true;
			}
		}

		/// <summary>
		/// Translate a given string into a TemplateType, if possible
		/// </summary>
		/// <param name="templateType">Either the Template Type name or Choice value (OptionSet)</param>
		/// <returns>string</returns>
		public string ValidateTemplateTypeChoice( string templateType )
		{
			// Try to convert the incoming string into a Transaction Type Enum
			// and return the string representation.

			rcade_CARTTemplates_eca_TemplateType type;
			if (Enum.TryParse<rcade_CARTTemplates_eca_TemplateType>(templateType, out type))
			{
				return type.ToString();
			}
			return string.Empty;
		}

	}
}
