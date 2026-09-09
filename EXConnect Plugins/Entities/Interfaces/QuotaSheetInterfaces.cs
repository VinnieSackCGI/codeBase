using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{

	/// <summary>
	///  Exposes the necessary fields by the Domain layer from the Quata Sheet tables
	/// </summary>
	public interface IQuotaSheetAccountMapRecord : ITableRecord
	{
		#region Interrogation Fields

		bool IsActive { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string Category { get; set; }

		string Region { get; set; }

		EntityReference CUFFaccount { get; set; }

		DateTime? CompletedOn { get; set; }
		bool UsedForUSStudents { get; set; }
		bool UsedForForeignStudents { get; set; }
		bool UsedForUSScholars { get; set; }
		bool UsedForVisitingScholars { get; set; }

		int Version { get; set; }

		#endregion

		#region Methods
		#endregion
	}

	public interface IQuotaSheetHeaderRecord : ITableRecord
	{
		#region Interrogation Fields

		bool IsActive { get; }

		#endregion

		#region Data fields

		string Name { get; set; }
		string AwardNumber { get; set; }
		string SubProgramCode { get; set; }
		int FiscalYear { get; set; }
		string Region { get; set; }
		string SubRegion { get; set; }
		string GranteeType { get; set; }
		string PhaseType { get; set; }

		EntityReference Appropriation { get; set; }

		string Notes { get; set; }

		int? Version { get; set; }

        int? Status { get; set;  }

		#endregion

		#region Methods

		void UpdateFromNewVersion( IQuotaSheetHeaderRecord newRecord );

		#endregion
	}

	public interface IQuotaSheetDataRecord : ITableRecord
	{
		// General interface to expose the common fields between Quota Sheets
		string Name { get; set; }
		EntityReference CUFFaccount { get; set; }
		EntityReference ParentHeader { get; set; }
		int? Version { get; set; }
        int? QSSortOrder { get; set; }
	}

	public interface IQuotaSheetRegionalDataRecord : IQuotaSheetDataRecord
	{
		#region Interrogation Fields

		bool IsActive { get; }

		#endregion

		#region Data fields

		int? Base_NewCount { get; set; }
		int? Base_LectureCount { get; set; }
		int? Base_ResearchCount { get; set; }
		int? Base_IEACount { get; set; }
		decimal? Base_NewAmount { get; set; }
		int? Base_RenewalCount { get; set; }
		decimal? Base_RenewalAmount { get; set; }
		int? Base_SR_Count { get; set; }
		int? Base_SR_RenewalCount { get; set; }
		decimal? Base_SR_Amount { get; set; }
		int? Base_NewOtherCount { get; set; }
		int? Base_CentrallyFundedCount { get; set; }
		decimal? Base_NewOtherAmount { get; set; }
		int? Base_RenewalOtherCount { get; set; }
		decimal? Base_RenewalOtherAmount { get; set; }

		int? Carryover_NewCount { get; set; }
		int? Carryover_LectureCount { get; set; }
		int? Carryover_ResearchCount { get; set; }
		int? Carryover_IEACount { get; set; }
		decimal? Carryover_NewAmount { get; set; }
		int? Carryover_RenewalCount { get; set; }
		decimal? Carryover_RenewalAmount { get; set; }

		int? Recovery_NewCount { get; set; }
		int? Recovery_LectureCount { get; set; }
		int? Recovery_ResearchCount { get; set; }
		int? Recovery_IEACount { get; set; }
		decimal? Recovery_NewAmount { get; set; }
		int? Recovery_RenewalCount { get; set; }
		decimal? Recovery_RenewalAmount { get; set; }

		int? OtherFunding_NewCount { get; set; }
		int? OtherFunding_LectureCount { get; set; }
		int? OtherFunding_ResearchCount { get; set; }
		int? OtherFunding_IEACount { get; set; }
		decimal? OtherFunding_NewAmount { get; set; }
		int? OtherFunding_RenewalCount { get; set; }
		decimal? OtherFunding_RenewalAmount { get; set; }

		int? OtherFunding2_NewCount { get; set; }
		int? OtherFunding2_LectureCount { get; set; }
		int? OtherFunding2_ResearchCount { get; set; }
		int? OtherFunding2_IEACount { get; set; }
		decimal? OtherFunding2_NewAmount { get; set; }
		int? OtherFunding2_RenewalCount { get; set; }
		decimal? OtherFunding2_RenewalAmount { get; set; }

		#endregion

		#region Methods

		Entity GetUpdateEntityFromNewVersion( IQuotaSheetRegionalDataRecord newRecord, EntityReference accountRef, int version );

		#endregion
	}

	public interface IQuotaSheetFrontOfficeDataRecord : IQuotaSheetDataRecord
	{
		#region Interrogation Fields

		bool IsActive { get; }

		#endregion

		#region Data fields

		decimal? Preliminary_BaseFunding { get; set; }

		decimal? Initial_BaseFunding { get; set; }
		decimal? Initial_CarryoverFunding { get; set; }
		decimal? Initial_RecoveryFunding { get; set; }
		decimal? Initial_AfghanistanFunding { get; set; }
		decimal? Initial_OtherFunding1 { get; set; }
		decimal? Initial_OtherFunding2 { get; set; }

		decimal? Final_BaseFunding { get; set; }
		decimal? Final_CarryoverFunding { get; set; }
		decimal? Final_RecoveryFunding { get; set; }
		decimal? Final_AfghanistanFunding { get; set; }
		decimal? Final_OtherFunding1 { get; set; }
        decimal? Final_OtherFunding2 { get; set; }

		decimal? FinalFinal_BaseFunding { get; set; }
		decimal? FinalFinal_CarryoverFunding { get; set; }
		decimal? FinalFinal_RecoveryFunding { get; set; }
		decimal? FinalFinal_AfghanistanFunding { get; set; }
		decimal? FinalFinal_OtherFunding1 { get; set; }
		decimal? FinalFinal_OtherFunding2 { get; set; }

        decimal? Amendment5_BaseFunding { get; set; }
        decimal? Amendment5_CarryoverFunding { get; set; }
        decimal? Amendment5_RecoveryFunding { get; set; }
        decimal? Amendment5_OtherFunding1 { get; set; }
        decimal? Amendment5_OtherFunding2 { get; set; }

        decimal? Amendment6_BaseFunding { get; set; }
        decimal? Amendment6_CarryoverFunding { get; set; }
        decimal? Amendment6_RecoveryFunding { get; set; }
        decimal? Amendment6_OtherFunding1 { get; set; }
        decimal? Amendment6_OtherFunding2 { get; set; }
        #endregion

        #region Methods

        Entity GetUpdateEntityFromNewVersion( IQuotaSheetFrontOfficeDataRecord newRecord, EntityReference accountRef, int version);


		#endregion

	}

}
