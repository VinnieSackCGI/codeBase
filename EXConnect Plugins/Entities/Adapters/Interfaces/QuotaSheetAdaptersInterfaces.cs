using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Adapters.Interfaces
{

	public enum GranteeType
	{
		USStudent = 1,
		ForeignStudent,
		USScholar,
		VisitingScholar,
        AMIDEAST,
		Scholar
	}

	public interface IQuotaSheetHeaderAdapter : ITableAdapter<IQuotaSheetHeaderRecord>
	{
		IQuotaSheetHeaderRecord GetRecordFromId( Guid id );
		EntityReference GetEntityReferenceFromId( Guid id );
		string GetMatchingGranteeTypeFromString( string strValue );
        string GetMatchingRevisionTypeFromString(string strValue);
		string GetMatchingRevisionNameFromType( string typeValue );
		GranteeType? GetGranteeTypeEnumFromString(string granteeType );
	}

	public interface IQuotaSheetRegionalDataAdapter : ITableAdapter<IQuotaSheetRegionalDataRecord>
	{
		//IRegionalQuotaSheetRecord GetRecordFromId( Guid id );
		bool IsRegionalDataType( IQuotaSheetDataRecord dataRecord );
	}

	public interface IQuotaSheetFrontOfficeDataAdapter : ITableAdapter<IQuotaSheetFrontOfficeDataRecord>
	{
		//IRegionalQuotaSheetRecord GetRecordFromId( Guid id );
		bool IsFrontOfficeDataType( IQuotaSheetDataRecord dataRecord );
	}

	public interface IQuotaSheetAccountMapAdapter 
	{
		IList<QuotaSheetAccountMapItem> GetRecordsList();
	}


}

