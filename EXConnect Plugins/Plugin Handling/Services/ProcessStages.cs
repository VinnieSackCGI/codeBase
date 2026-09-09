using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Entities.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace EXConnect_Plugins.Services
{
	public static class ProcessStages
	{
		public static Dictionary<string, Guid> TransactionStages { get; set; }
		public static Dictionary<string, Guid> DocumentStages { get; set; }
		public static Dictionary<string, Guid> Stages { get; private set; }

		internal class StageDecode
		{
			internal int Order;
			internal string RecordStatus;
			internal string WorkflowStage;
		}
		private static IList<StageDecode> StageSequence { get; set; }

		public static void Initialize( IRepository dbService )
		{
			TransactionStages = dbService.ProcessStage.GetAllProcessStagesForPrimaryEntityName(rcade_CARTTransaction.EntityLogicalName);
			DocumentStages = dbService.ProcessStage.GetAllProcessStagesForPrimaryEntityName(eca_GFMSDocument.EntityLogicalName);

			StageSequence = new List<StageDecode>();
			StageSequence.Add(new StageDecode { Order = 1, RecordStatus = "Draft", WorkflowStage = "Draft" });
			StageSequence.Add(new StageDecode { Order = 2, RecordStatus = "Draft", WorkflowStage = "Needs Revision" });
			StageSequence.Add(new StageDecode { Order = 3, RecordStatus = "Pending", WorkflowStage = "Pending Approval" });
			StageSequence.Add(new StageDecode { Order = 4, RecordStatus = "Approved", WorkflowStage = "Approved" });
		}

		#region Static Functions

		public static void SetWorkflowStage( IRepository dbService, ITracingService tracer, IWorkflowRecord bpfRecord, string stageName, bool patchChanges = false )
		{
			// Get the stage associated with the incoming step index
			//			var newStageRecord = dbService.ProcessStage.GetAllProcessRecords(twRecord.ProcessRef.Id).Where(s => s.StageName == stageName)
			//												.FirstOrDefault(); 

			if (TransactionStages == null || DocumentStages == null)
			{
				Initialize(dbService);
			}

			// Point to the corresponding Stages
			if ( bpfRecord is eca_documentworkflow)
			{
				Stages = DocumentStages;
			}
			else if (bpfRecord is eca_transactionworkflow)
			{
				Stages = TransactionStages;
			}

			if (!Stages.Keys.Contains(stageName))
			{
				string msg = $"Could not reconcile the new stage name \"{stageName}\".";
				throw (new InvalidPluginExecutionException(OperationStatus.Failed, stageName));
			}

			tracer.Trace($"Setting Active Stage to \"{stageName}\". ");

			// Set the process workflow record with the new step
			bpfRecord.ActiveStageRef = new EntityReference("processstage", Stages[stageName]);

			tracer.Trace($"Current TraversePath = \"{bpfRecord.TraversedPath}\", new ID = \"{Stages[stageName]}\".");

			// Reconstruct the TraversePath based on the stage to set

			var traversedPath = string.Empty;

			for (var s = 0; s < StageSequence.Count(); s++)
			{
				var stage = StageSequence[s].WorkflowStage;

				if (stage == "Needs Revision")
				{
					// Consider "Needs Revision" ONLY if it's the last stage 
					if (s < StageSequence.Count() - 1)
					{
						continue;
					}
				}

				if (s > 0)
				{
					traversedPath += ",";
				}
				traversedPath += $"{Stages[stage]}";
				if ( stage == stageName)
				{
					break;
				}
			}
			bpfRecord.TraversedPath = traversedPath;

			if (!patchChanges)
			{
				// Make sure the record is Active
				if (bpfRecord.IsFinished && !bpfRecord.IsActive)
				{
					bpfRecord.Reactivate();
				}
			}
			else
			{
				// Patch changes to avoid collisions

				var bpfEntity = new Entity(bpfRecord.FieldLogicalNames["TableLogicalName"], bpfRecord.Id);

				if (bpfRecord.IsFinished && !bpfRecord.IsActive)
				{
					// Make sure we make this request first
					bpfEntity[bpfRecord.FieldLogicalNames["StatusCode"]] = new OptionSetValue(1);
					bpfEntity[bpfRecord.FieldLogicalNames["StateCode"]] = new OptionSetValue(0);
				}

				bpfEntity[bpfRecord.FieldLogicalNames["TraversedPath"]] = bpfRecord.TraversedPath;
				bpfEntity[bpfRecord.FieldLogicalNames["ActiveStageRef"]] = bpfRecord.ActiveStageRef;

				dbService.OrgServiceContext.Detach(bpfRecord.Entity);

				tracer.Trace("BPF Record has been detached.");

				var updResponse = dbService.Patch(bpfEntity); 

				if (updResponse.Results.Count > 0)
				{
					tracer.Trace($"Response from Update call is : {updResponse.Results.ToString()}");
				}
				else
				{
					tracer.Trace($"Response from Update call has no results.");
				}
			}
		}

		public static string WorkflowStageFromRecordStatus( IRepository dbService, string statusName )
		{
			// Translate the incoming Transaction stage to a Workflow Stage; 

			if (StageSequence == null)
			{
				Initialize(dbService);
			}

			if (string.IsNullOrEmpty(statusName))
			{
				return null;
			}

			var bpfStageName = StageSequence.Where(s => s.RecordStatus == statusName).Select(s => s.WorkflowStage).FirstOrDefault();

			if (string.IsNullOrEmpty(bpfStageName))
			{
				string msg = $"Could not find a corresponding BPF Stage Name for Status = \"{statusName}\".";
			}

			return bpfStageName;
		}

		public static string WorkflowStageToRecordStage( IRepository dbService, string stageName )
		{
			// Translate the incoming Transaction stage to a Workflow Stage; 

			if (StageSequence == null)
			{
				Initialize(dbService);
			}

			if (string.IsNullOrEmpty(stageName))
			{
				return null;
			}

			var recordStatusName = StageSequence.Where(s => s.WorkflowStage == stageName).Select(s => s.RecordStatus).FirstOrDefault();

			if (string.IsNullOrEmpty(recordStatusName))
			{
				string msg = $"Could not find a corresponding record Status for BPF Stage Name = \"{stageName}\".";
			}

			return recordStatusName;
		}

		#endregion

	}
}
