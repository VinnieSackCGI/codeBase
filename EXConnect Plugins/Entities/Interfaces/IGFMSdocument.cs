using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface IGFMSdocumentRecord : ITableRecord
	{
		#region Interrogation Properties

		bool IsActive { get; }
		bool IsApproved { get; }

		bool IsCommitment { get; }
		bool IsObligation { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string DocumentNumber { get; set; }
		string DocumentType { get; set; }

		string TransactionType { get; set; }
		EntityReference ObligationType { get; set; }
		bool NeedsValidation { get; set; }
		string Status { get; set; }

		#endregion

		#region Public Methods
		#endregion

	}
}
