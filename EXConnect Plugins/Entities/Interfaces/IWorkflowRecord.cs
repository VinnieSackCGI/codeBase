using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{

    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the Transaction or Document Workflow Record table
    /// </summary>
	public interface IWorkflowRecord : ITableRecord
    {
        void Initialize(IRepository dbService, ITracingService tracer);

        #region Interrogation Fields

        bool IsActive { get; }
        
        bool IsFinished { get; }

        #endregion

        #region Data fields
        Dictionary<string, string> FieldLogicalNames { get; }

		EntityReference ActiveStageRef { get; set; }

        DateTime? ActiveStageStartedOn { get; set; }
        DateTime? CompletedOn { get; set; }

        string TraversedPath { get; set; }

        EntityReference ProcessRef { get; set; }

        #endregion

        #region Methods

        void Reactivate(bool withUpdate = true);

        #endregion
    }

    public interface IDocumentWorkflowRecord : IWorkflowRecord
    {
    	EntityReference DocumentRef { get; set; }
	}

	public interface ITransactionWorkflowRecord : IWorkflowRecord
	{
		EntityReference TransactionRef { get; set; }
	}
}
