using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface IReferenceTableRecord : IKeyTableRecord
	{
		bool IsActive { get; }
	}
	public interface IAppropriationRecord : IReferenceTableRecord
	{
		bool IsFundsControlled { get; }
		string Allotment { get; set; }
		int? CRDays { get; set; }
		decimal? CRFactor { get; set; }
		bool? AllowNegativeAccountBalances { get; set; }
	}

	public interface IFundingTypeRecord : IReferenceTableRecord { }

	public interface IFundingSourceRecord : IReferenceTableRecord
	{ 
		EntityReference FundingType { get; set; }
		EntityReference Appropriation { get; }
	}

	public interface IObligationTypeRecord : IReferenceTableRecord { }

	public interface IPostRecord : IReferenceTableRecord
	{
		string Region { get; set; }

	}
	public interface INeighborhoodRecord : IReferenceTableRecord
	{
		IEnumerable<SystemUser> BudgetAnalystsList { get; set; }
	}
	public interface IDivisionRecord : IReferenceTableRecord
	{
	}
	public interface IProgramRecord : IReferenceTableRecord
	{
	}
	public interface ISubProgramRecord : IReferenceTableRecord
	{
	}

}
