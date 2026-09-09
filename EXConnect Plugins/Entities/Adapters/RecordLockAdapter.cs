using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters
{
	using System.Threading;
	using Interfaces;
	using Microsoft.Xrm.Sdk.Query;
	using Plugins_CommonLibrary.Services;

	public class RecordLockAdapter : IRecordLockAdapter
	{
		private IRepository dbService;
		private ITracingService tracer;
		private EXConnect_ServiceContext serviceContext;
		private IWebAPIService webApiService;

		private Guid? lockedRecord;

		public RecordLockAdapter( IRepository dbService, ITracingService tracer )
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
			serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;

			webApiService = new WebAPIService(dbService.EnvironmentVariables, tracer);

			lockedRecord = null;
		}

		public bool LockRollupRecord( Guid requesterId, string appropriation, string account, string fundingType, string fiscalYear, DateTime? dateTimeLocal = null )
		{
#if UNITTEST
            return true;
#endif
			// Try placing a lock for the given transaction budget string. If the transaction is an adjustment,
			// we will need to lock both the source and target budget strings

			// But first, remove any locks we already have
			// including any old ones that exist due to any unexpected errors
			UnlockRecord(true);
			if (dateTimeLocal != null)
			{
				RemoveOldLocks(dateTimeLocal);
			}

			var recordKey = string.Concat(appropriation, ":", account, ":", fundingType, ":", fiscalYear);

			tracer.Trace($"Trying to get a lock on rollup string \"{recordKey}\".");

			lockedRecord = GetRollupKeyRecordLock(recordKey, requesterId);

			return (lockedRecord != null);
		}

		public bool UnlockRecord( bool useWebAPI = false )
		{
			if (lockedRecord != null)
			{
				RemoveLockedRecord(lockedRecord, useWebAPI);
				lockedRecord = null;
			}
			return (lockedRecord == null);
		}

		private void RemoveLockedRecord( Guid? id, bool useWebAPI = false )
		{
			if (id == null) return;

			var lockRecord = serviceContext.eca_RollupRecordLockSet.Where(r => r.Id == id).FirstOrDefault();
			if (lockRecord == null)
			{
				tracer.Trace($"Record lock for Id = {id.GetValueOrDefault()} cannot be found.");
			}
			else
			{
				if (useWebAPI)
				{
					webApiService.DeleteRecord(eca_RollupRecordLock.EntityLogicalCollectionName, lockRecord.Id);
				}
				else
				{
					dbService.Delete(lockRecord);
				}
			}
		}

		private void RemoveOldLocks( DateTime? dateTimeLocal )
		{
			// Remove any locks present more than 5 minutes ago
			var threeMinutesAgo = dateTimeLocal.GetValueOrDefault().AddMinutes(-3);

			tracer.Trace($"==> Removing records earlier than {threeMinutesAgo}.");

			var locksList = serviceContext.eca_RollupRecordLockSet
											.Where(r => !r.eca_LockedRecordKey.StartsWith("Settings")
													  && r.CreatedOn.GetValueOrDefault() < threeMinutesAgo)
											.ToList();
			if (locksList.Count > 0)
			{
				tracer.Trace($"==> Removing {locksList.Count} locks that exceed 3 minutes.");
				foreach (var lockRecord in locksList)
				{
					webApiService.DeleteRecord(eca_RollupRecordLock.EntityLogicalCollectionName, lockRecord.Id);
				}
			}
		}

		static int maxIterations = 50;

		private Guid? GetRollupKeyRecordLock( string rollupKey, Guid requestorId )
		{
			// Try to obtain a lock for the given budget string.
			// If lock already exists, sleep for 1 second and try again
			// If lock cannot be obtained within 50 seconds, bail out.

			var iteration = 0;
            while (iteration < maxIterations)
            {
                var doTrace = iteration % 10 == 0;
                var lockRecord = GetFirstRecord(rollupKey);
                if (lockRecord == null)
                {
                    if (doTrace)
                    {
                        tracer.Trace($"Attempting to create Record Lock on {rollupKey} - iteration # {iteration}");
                    }
                    // Add this key info to lock the record
                    if (webApiService.CreateRecord(eca_RollupRecordLock.EntityLogicalCollectionName,
                                            new { eca_lockedrecordkey = $"{rollupKey}", eca_requestid = $"{requestorId}" }))
                    {
                        // Verify that the created record is the first one created
                        if (doTrace)
                        {
                            tracer.Trace($"Created Record on iteration # {iteration}");
                        }
                        var firstLock = GetFirstRecord(rollupKey);
                        if (firstLock != null && firstLock.eca_RequestId == requestorId.ToString())
                        {
                            tracer.Trace($"Record lock successful at {DateTime.Now.ToString("ddMMMyyyy hh:mm:ss.ffff")} on iteration # {iteration}.");
                            return firstLock.Id;
                        }
                        else 
                        {
                            if (doTrace) 
                            {
                                tracer.Trace($"Record lock failed on iteration # {iteration}\nFirst Lock is null = {firstLock == null}" +
                                    $"\nFirst Lock Request Id = {firstLock?.eca_RequestId}\nRequestor Id = {requestorId.ToString()}");
                            }
                        }
                    }
                }
                else if (lockRecord.eca_RequestId == requestorId.ToString())
                {
                    // If the first lock is from the same requestor, use it.
                    return lockRecord.Id;
                }
                else
                {
                    if (doTrace) 
                    {
                        tracer.Trace($"Lock record is not null and Lock Record Request Id ({lockRecord?.eca_RequestId}) does not equal" +
                            $" Requestor Id ({requestorId.ToString()}) for iteration # {iteration}");
                    }
                }

                    // If we get here, we need to try again.
                    // But first, we need to delete the lock for this requestor, if one exists.
                    lockRecord = serviceContext.eca_RollupRecordLockSet
                                        .Where(r => r.eca_LockedRecordKey == rollupKey && r.eca_RequestId == requestorId.ToString())
                                        .FirstOrDefault();

				if (lockRecord != null)
				{
					tracer.Trace("Lock request failed due to collision.");
					RemoveLockedRecord(lockRecord.Id, true);
				}

				if (doTrace)
				{
					tracer.Trace($"sleeping... Iteration # {iteration} : {DateTime.Now.ToString("G")}");
				}
				Thread.Sleep(500);
				iteration++;
			}

			if (iteration >= maxIterations)
			{
				var errMsg = $"Could not obtain a record lock within {maxIterations} ierations to perform the update for fiscal strip \"{rollupKey}\". Operation aborted.";
				throw (new InvalidPluginExecutionException(OperationStatus.Failed, -2147220685, errMsg));
			}

			return null;
		}

		private eca_RollupRecordLock GetFirstRecord( string recordKey )
		{
			// =============================================================
			// Cannot use Linq to retrieve the data becuase the results are
			// cached. Need to use FetchXml to get any updated data
			// =============================================================
			/*
			var result = serviceContext.eca_RollupRecordLockSet
								.Where(r => r.eca_LockedRecordKey == recordKey)
								.OrderBy(r => r.ModifiedBy)
								.FirstOrDefault();
			*/

			if (recordKey.Contains("&"))
			{
				recordKey = recordKey.Replace("&", "&amp;");
			}
			var fetchXml = $@"
<fetch>
  <entity name='eca_rolluprecordlock'>
    <attribute name='eca_lockedrecordkey' />
    <attribute name='eca_requestid' />
    <filter>
      <condition attribute='eca_lockedrecordkey' operator='eq' value='{recordKey}' />
    </filter>
    <order attribute='createdon' />
  </entity>
</fetch>";
			var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));


			if (results.Entities.Count > 0)
			{
				var entity = results.Entities[0];

				var result = new eca_RollupRecordLock();
				result.Id = entity.Id;
				result.eca_RequestId = entity["eca_requestid"].ToString();
				result.eca_LockedRecordKey = entity["eca_lockedrecordkey"].ToString();

				return result;
			}
			return null;
		}

	}
}

