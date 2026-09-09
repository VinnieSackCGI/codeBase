using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface ICUFFaccountRecord : ITableRecord
	{
		#region Interrogation Properties

		bool IsActive { get; }
		bool IsSpendingEligible { get; }
		bool IsAOAeligible { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string Description { get; set; }
		EntityReference Neighborhood { get; set; }
		EntityReference Division { get; set; }
		EntityReference Program { get; set; }
		EntityReference SubProgram { get; set; }
		EntityReference FunctionCode { get; set; }
		EntityReference ParentAccount { get; set; }

		#endregion
	}
}
