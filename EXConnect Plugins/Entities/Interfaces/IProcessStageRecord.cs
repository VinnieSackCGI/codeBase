using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	/// <summary>
	///  Exposes the necessary fields by the Domain layer from the Process Stage table
	/// </summary>
	public interface IProcessStageRecord : ITableRecord
    {
        void Initialize(IRepository dbService, ITracingService tracer);

        #region Interrogation Fields

        bool IsDraft { get; }
		bool IsPendingApproval { get; }
		bool IsApproved { get; }
        bool IsNeedsRevision { get; }
        #endregion

        #region Data fields

#if UNITTEST
        EntityReference ProcessId { get; set; }
        string PrimaryEntityType { get; set; }
        Guid? StageId { get; set; }
        string StageName { get; set; }
#else
        EntityReference ProcessId { get; }
        string PrimaryEntityType { get; }
        Guid? StageId { get; }
        string StageName { get; }

#endif
        #endregion

        #region Methods
        #endregion
    }

}
