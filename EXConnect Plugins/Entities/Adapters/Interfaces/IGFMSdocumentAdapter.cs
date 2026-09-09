using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{
	public class ChildTransaction
	{
		public Guid Id;
		public string Name;
		public EntityReference Appropriation;
		public EntityReference Account;
		public EntityReference FundingType;
		public decimal Amount;
		public string Status;
	}

	public class ChildRollupAmount
	{
		public EntityReference Appropriation;
		public EntityReference Account;
		public EntityReference FundingType;
		public decimal CommitmentAmount;
		public decimal CommitmentDeltaAmount;
		public decimal ObligationAmount;
		public decimal ObligationDeltaAmount;
	}


	public interface IGFMSdocumentAdapter : ITableAdapter<IGFMSdocumentRecord>
	{
		IGFMSdocumentRecord GetRecordFromId( Guid id );
		IList<ICARTtransactionRecord> GetChildTransactionsList( Guid id );
		int GetHighestDocumentNumberByObligationCodeString( int fiscalYear, string codeString, int countLimit );
		int GetHighestDocumentNumberByCommitmentCodeString( int fiscalYear, string codeString, int countLimit );
	}

}
