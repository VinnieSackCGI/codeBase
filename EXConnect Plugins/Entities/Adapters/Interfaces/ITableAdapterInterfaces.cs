using System;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;

using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Data;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{

	#region Reference Tables 

	public interface IAppropriationAdapter : ITableAdapter<IAppropriationRecord>
	{
		string GetCodeFromId( Guid id );
		IAppropriationRecord GetRecordFromId( Guid postId );
		EntityReference GetExtendedEntityReference( EntityReference appropRef );
		EntityReference GetEntityReferenceFromCode( string name );
		EntityReference GetEntityReferenceFromId( Guid id );
	}

	public interface IReferenceTableAdapter<T> : ITableAdapter<T>
	{
		string GetNameFromId( Guid id );
		EntityReference GetEntityReferenceFromId( Guid id );
	}

	public interface IObligationTypeAdapter : IReferenceTableAdapter<IObligationTypeRecord>
	{
		EntityReference GetEntityReferenceFromName( string name );
		EntityReference GetExtendedEntityReference( EntityReference fundingTypeRef );
	}
	public interface IPostAdapter : IReferenceTableAdapter<IPostRecord>
	{
		EntityReference GetEntityReferenceFromName( string name );
		EntityReference GetExtendedEntityReference( EntityReference fundingTypeRef );
		IPostRecord GetRecordFromId( Guid postId );
	}

	public interface IFundingTypeAdapter : IReferenceTableAdapter<IFundingTypeRecord>
	{
		EntityReference GetEntityReferenceFromName( string name );
		EntityReference GetExtendedEntityReference( EntityReference fundingTypeRef );
	}

	public class FundingSrceRecord
	{
		public Guid FundingSrcId = Guid.Empty;
		public string FundingSrcName = string.Empty;
		public string FundingSrcCode = string.Empty;
		public string FundingType = string.Empty;
		public Guid FundingTypeId = Guid.Empty;
		public string Appropriation = string.Empty;
		public Guid AppropriationId = Guid.Empty;
	}

	public interface IFundingSourceAdapter : IReferenceTableAdapter<IFundingSourceRecord>
	{
		EntityReference GetEntityReferenceFromName( string name );
		EntityReference GetExtendedEntityReference( EntityReference fundingSourceRef );
		IList<FundingSrceRecord> GetAppropriationFundingDataList();
	}

	public interface INeighborhoodAdapter : ITableAdapter<INeighborhoodRecord>
	{
	}

	public interface IDivisionAdapter : ITableAdapter<IDivisionRecord>
	{
	}

	public interface IProgramAdapter : ITableAdapter<IProgramRecord>
	{
	}

	public interface ISubProgramAdapter : ITableAdapter<ISubProgramRecord>
	{
	}

	public interface ITemplatesAdapter : ITableAdapter<ITemplateRecord>
	{
		ITemplateRecord GetRecordFromId( Guid id );
		bool SetNeedsUpdateFlag( bool needsUpdate );
		string ValidateTemplateTypeChoice( string templateType );
	}

	#endregion

	public interface ICARTuserRoleAdapter : ITableAdapter<ICARTuserRoleRecord>
    {
        ICARTuserRoleRecord GetRecordFromId(Guid id);
        ICARTuserRoleRecord GetRecordFromName(string name);
    }

	public interface ICARTtransactionAdapter : ITableAdapter<ICARTtransactionRecord>
	{
		ICARTtransactionRecord GetRecordFromId( Guid id );
		ICARTtransactionRecord GetRecordFromName( string name );
		int GetHighestDocumentNumberByObligationCodeString( int fiscalYear, string codeString, int countLimit );
        int GetHighestDocumentNumberByCommitmentCodeString( int fiscalYear, string codeString, int countLimit );
		bool CanBeAutoApproved( ICARTtransactionRecord transaction );
		ICARTtransactionRecord ComposeTransactionRecordFromStaged( IStagedBulkUploadRecord stagedRecord );
		string GetLogicalName( string fieldName );
	}

	public interface IStagedBulkUploadRecordAdapter : ITableAdapter<IStagedBulkUploadRecord>
	{
		IStagedBulkUploadRecord GetRecordFromId( Guid id );
		IStagedBulkUploadRecord GetRecordFromName( string name );
		IStagedBulkUploadRecord ComposeStagedRecordFromTransaction( ICARTtransactionRecord cartTransaction );
		IStagedBulkUploadRecord ComposeStagedRecordFromSpendPlanRequest( ISpendPlanRequestRecord spenPlanRequest );
	}

	public interface ICUFFaccountAdapter : ITableAdapter<ICUFFaccountRecord>
	{
		ICUFFaccountRecord GetRecordFromId( Guid id );
		EntityReference GetEntityReferenceFromId( Guid id );
		EntityReference GetEntityReferenceFromName( string name );
		EntityReference GetExtendedEntityReference( EntityReference accountRef );
		DateTime GetLastRevisionDate();
	}

	public interface IAppropriationYearlyRollupAdapter : ITableAdapter<IAppropriationYearlyRollupRecord>
	{
		IAppropriationYearlyRollupRecord GetRecordFromId( Guid id );
		IAppropriationYearlyRollupRecord GetRecordFromKeys( int fiscalYear, EntityReference refAppropriation );
	}

	public interface IAllotmentAdapter
	{
		string GetCodeFromId( Guid id );
		string GetNameFromId( Guid id );
	}

	public interface ISpendPlanRequestAdapter : ITableAdapter<ISpendPlanRequestRecord>
	{
		ISpendPlanRequestRecord GetRecordFromId( Guid id );
		ISpendPlanRequestRecord GetRecordFromName( string name );
		ISpendPlanRequestRecord ComposeSpendPlanRequestRecordFromStaged( IStagedBulkUploadRecord stagedRecord );
	}

}
