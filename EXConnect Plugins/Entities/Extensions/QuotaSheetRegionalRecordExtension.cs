
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_QuotaSheetRegionalData : IQuotaSheetRegionalDataRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_QuotaSheetRegionalDataState.Active;
		}

		#endregion

		#region Data fields

		public string Name
		{
			get => eca_Name;
			set => eca_Name = value;
		}

		public EntityReference CUFFaccount
		{
			get => eca_CUFFAccount;
			set => eca_CUFFAccount = value;
		}

		public EntityReference ParentHeader
		{
			get => eca_ParentHeaderRecord;
			set => eca_ParentHeaderRecord = value;
		}

		public int? Version
		{
			get => eca_Version;
			set => eca_Version = value;
		}

		// Base Funding
		public int? Base_NewCount
		{
			get => eca_BaseFundingNewCount;
			set => eca_BaseFundingNewCount = value;
		}
		public int? Base_LectureCount
		{
			get => eca_BaseFundingNewLectureCount;
			set => eca_BaseFundingNewLectureCount = value;
		}
		public int? Base_ResearchCount
		{
			get => eca_BaseFundingNewResearchCount;
			set => eca_BaseFundingNewResearchCount = value;
		}
		public int? Base_IEACount
		{
			get => eca_BaseFundingNewIEACount;
			set => eca_BaseFundingNewIEACount = value;
		}
		public decimal? Base_NewAmount
		{
			get
			{
				if (eca_BaseFundingNewAmount != null)
				{
					return eca_BaseFundingNewAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_BaseFundingNewAmount = new Money(value.Value);
				}
				else
				{
					eca_BaseFundingNewAmount = null;
				}
			}
		}
		public int? Base_NewOtherCount
		{
			get => eca_BaseFundingNewOtherCount;
			set => eca_BaseFundingNewOtherCount = value;
		}
		public decimal? Base_NewOtherAmount
		{
			get
			{
				if (eca_BaseFundingNewOtherAmount != null)
				{
					return eca_BaseFundingNewOtherAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_BaseFundingNewOtherAmount = new Money(value.Value);
				}
				else
				{
					eca_BaseFundingNewOtherAmount = null;
				}
			}
		}
		public int? Base_RenewalCount
		{
			get => eca_BaseFundingRenewalCount;
			set => eca_BaseFundingRenewalCount = value;
		}
		public decimal? Base_RenewalAmount
		{
			get
			{
				if (eca_BaseFundingRenewalAmount != null)
				{
					return eca_BaseFundingRenewalAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_BaseFundingRenewalAmount = new Money(value.Value);
				}
				else
				{
					eca_BaseFundingRenewalAmount = null;
				}
			}
		}
		public int? Base_RenewalOtherCount
		{
			get => eca_BaseFundingRenewalOtherCount;
			set => eca_BaseFundingRenewalOtherCount = value;
		}
		public decimal? Base_RenewalOtherAmount
		{
			get
			{
				if (eca_BaseFundingRenewalOtherAmount != null)
				{
					return eca_BaseFundingRenewalOtherAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_BaseFundingRenewalOtherAmount = new Money(value.Value);
				}
				else
				{
					eca_BaseFundingRenewalOtherAmount = null;
				}
			}
		}

		public int? Base_SR_Count
		{
			get => eca_BaseFundingSRCount;
			set => eca_BaseFundingSRCount = value;
		}
		public int? Base_SR_RenewalCount
		{
			get => eca_BaseFundingSRRenewalCount;
			set => eca_BaseFundingSRRenewalCount = value;
		}

		public decimal? Base_SR_Amount
		{
			get
			{
				if (eca_BaseFundingSRAmount != null)
				{
					return eca_BaseFundingSRAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_BaseFundingSRAmount = new Money(value.Value);
				}
				else
				{
					eca_BaseFundingSRAmount = null;
				}
			}
		}
		public int? Base_CentrallyFundedCount
		{
			get => eca_BaseFundingCentrallyFundedCount;
			set => eca_BaseFundingCentrallyFundedCount = value;
		}

		// Carryover Funding
		public int? Carryover_NewCount
		{
			get => eca_CarryoverFundingNewCount;
			set => eca_CarryoverFundingNewCount = value;
		}
		public int? Carryover_LectureCount
		{
			get => eca_CarryoverFundingNewLectureCount;
			set => eca_CarryoverFundingNewLectureCount = value;
		}
		public int? Carryover_ResearchCount
		{
			get => eca_CarryoverFundingNewResearchCount;
			set => eca_CarryoverFundingNewResearchCount = value;
		}
		public int? Carryover_IEACount
		{
			get => eca_CarryoverFundingNewIEACount;
			set => eca_CarryoverFundingNewIEACount = value;
		}
		public decimal? Carryover_NewAmount
		{
			get
			{
				if (eca_CarryoverFundingNewAmount != null)
				{
					return eca_CarryoverFundingNewAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_CarryoverFundingNewAmount = new Money(value.Value);
				}
				else
				{
					eca_CarryoverFundingNewAmount = null;
				}
			}
		}
		public int? Carryover_RenewalCount
		{
			get => eca_CarryoverFundingRenewalCount;
			set => eca_CarryoverFundingRenewalCount = value;
		}
		public decimal? Carryover_RenewalAmount
		{
			get
			{
				if (eca_CarryoverFundingRenewalAmount != null)
				{
					return eca_CarryoverFundingRenewalAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_CarryoverFundingRenewalAmount = new Money(value.Value);
				}
				else
				{
					eca_CarryoverFundingRenewalAmount = null;
				}
			}
		}

		// Recovery Funding

		public int? Recovery_NewCount
		{
			get => eca_RecoveryFundingNewCount;
			set => eca_RecoveryFundingNewCount = value;
		}
		public int? Recovery_LectureCount
		{
			get => eca_RecoveryFundingNewLectureCount;
			set => eca_RecoveryFundingNewLectureCount = value;
		}
		public int? Recovery_ResearchCount
		{
			get => eca_RecoveryFundingNewResearchCount;
			set => eca_RecoveryFundingNewResearchCount = value;
		}
		public int? Recovery_IEACount
		{
			get => eca_RecoveryFundingNewIEACount;
			set => eca_RecoveryFundingNewIEACount = value;
		}
		public decimal? Recovery_NewAmount
		{
			get
			{
				if (eca_RecoveryFundingNewAmount != null)
				{
					return eca_RecoveryFundingNewAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_RecoveryFundingNewAmount = new Money(value.Value);
				}
				else
				{
					eca_RecoveryFundingNewAmount = null;
				}
			}
		}
		public int? Recovery_RenewalCount
		{
			get => eca_RecoveryFundingRenewalCount;
			set => eca_RecoveryFundingRenewalCount = value;
		}
		public decimal? Recovery_RenewalAmount
		{
			get
			{
				if (eca_RecoveryFundingRenewalAmount != null)
				{
					return eca_RecoveryFundingRenewalAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_RecoveryFundingRenewalAmount = new Money(value.Value);
				}
				else
				{
					eca_RecoveryFundingRenewalAmount = null;
				}
			}
		}

		// Other Funding

		public int? OtherFunding_NewCount
		{
			get => eca_OtherFundingNewCount;
			set => eca_OtherFundingNewCount = value;
		}
		public int? OtherFunding_LectureCount
		{
			get => eca_OtherFundingNewLectureCount;
			set => eca_OtherFundingNewLectureCount = value;
		}
		public int? OtherFunding_ResearchCount
		{
			get => eca_OtherFundingNewResearchCount;
			set => eca_OtherFundingNewResearchCount = value;
		}
		public int? OtherFunding_IEACount
		{
			get => eca_OtherFundingNewIEACount;
			set => eca_OtherFundingNewIEACount = value;
		}
		public decimal? OtherFunding_NewAmount
		{
			get
			{
				if (eca_OtherFundingNewAmount != null)
				{
					return eca_OtherFundingNewAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_OtherFundingNewAmount = new Money(value.Value);
				}
				else
				{
					eca_OtherFundingNewAmount = null;
				}
			}
		}
		public int? OtherFunding_RenewalCount
		{
			get => eca_OtherFundingRenewalCount;
			set => eca_OtherFundingRenewalCount = value;
		}
		public decimal? OtherFunding_RenewalAmount
		{
			get
			{
				if (eca_OtherFundingRenewalAmount != null)
				{
					return eca_OtherFundingRenewalAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_OtherFundingRenewalAmount = new Money(value.Value);
				}
				else
				{
					eca_OtherFundingRenewalAmount = null;
				}
			}
		}

		// Other Funding 2

		public int? OtherFunding2_NewCount
		{
			get => eca_OtherFunding2NewCount;
			set => eca_OtherFunding2NewCount = value;
		}
		public int? OtherFunding2_LectureCount
		{
			get => eca_OtherFunding2NewLectureCount;
			set => eca_OtherFunding2NewLectureCount = value;
		}
		public int? OtherFunding2_ResearchCount
		{
			get => eca_OtherFunding2NewResearchCount;
			set => eca_OtherFunding2NewResearchCount = value;
		}
		public int? OtherFunding2_IEACount
		{
			get => eca_OtherFunding2NewIEACount;
			set => eca_OtherFunding2NewIEACount = value;
		}
		public decimal? OtherFunding2_NewAmount
		{
			get
			{
				if (eca_OtherFunding2NewAmount != null)
				{
					return eca_OtherFunding2NewAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_OtherFunding2NewAmount = new Money(value.Value);
				}
				else
				{
					eca_OtherFunding2NewAmount = null;
				}
			}
		}
		public int? OtherFunding2_RenewalCount
		{
			get => eca_OtherFunding2RenewalCount;
			set => eca_OtherFunding2RenewalCount = value;
		}
		public decimal? OtherFunding2_RenewalAmount
		{
			get
			{
				if (eca_OtherFunding2RenewalAmount != null)
				{
					return eca_OtherFunding2RenewalAmount.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_OtherFunding2RenewalAmount = new Money(value.Value);
				}
				else
				{
					eca_OtherFunding2RenewalAmount = null;
				}
			}
		}

		public Entity Entity
		{
			get => this;
		}
        public int? QSSortOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        #region Public Methods

        public Entity GetUpdateEntityFromNewVersion( IQuotaSheetRegionalDataRecord record, EntityReference accountRef, int version )
		{
            // Update the current record with the incoming data, if different
            // Name and Parent Header should not have changed.

            var updatedEntity = new Entity( this.LogicalName, this.Id);

			// Start with the CUFF Account if different
			if (CUFFaccount == null || CUFFaccount?.Id != accountRef.Id)
			{
				updatedEntity["eca_cuffaccount"] = accountRef;
			}

			if (Base_NewCount != record.Base_NewCount)
			{
				updatedEntity["eca_basefundingnewcount"] = record.Base_NewCount;
			}
			if (Base_LectureCount != record.Base_LectureCount)
			{
				updatedEntity["eca_basefundingnewlecturecount"] = record.Base_LectureCount;
			}
			if (Base_ResearchCount != record.Base_ResearchCount)
			{
				updatedEntity["eca_basefundingnewresearchcount"] = record.Base_ResearchCount;
			}
			if (Base_IEACount != record.Base_IEACount)
			{
				updatedEntity["eca_basefundingnewieacount"] = record.Base_IEACount;
			}
			if (Base_NewAmount != record.Base_NewAmount)
			{
				updatedEntity["eca_basefundingnewamount"] = new Money(record.Base_NewAmount.GetValueOrDefault(0));
			}
			if (Base_NewOtherCount != record.Base_NewOtherCount)
			{
				updatedEntity["eca_basefundingnewothercount"] = record.Base_NewOtherCount;
			}
			if (Base_NewOtherAmount != record.Base_NewOtherAmount)
			{
				updatedEntity["eca_basefundingnewotheramount"] = new Money(record.Base_NewOtherAmount.GetValueOrDefault(0));
			}
			if (Base_RenewalCount != record.Base_RenewalCount)
			{
				updatedEntity["eca_basefundingrenewalcount"] = record.Base_RenewalCount;
			}
			if (Base_RenewalAmount != record.Base_RenewalAmount)
			{
				updatedEntity["eca_basefundingrenewalamount"] = new Money(record.Base_RenewalAmount.GetValueOrDefault(0));
			}
			if (Base_RenewalOtherCount != record.Base_RenewalOtherCount)
			{
				updatedEntity["eca_basefundingrenewalothercount"] = record.Base_RenewalOtherCount;
			}
			if (Base_RenewalOtherAmount != record.Base_RenewalOtherAmount)
			{
				updatedEntity["eca_basefundingrenewalotheramount"] = new Money(record.Base_RenewalOtherAmount.GetValueOrDefault(0));
			}

			if (Base_SR_Count != record.Base_SR_Count)
			{
				updatedEntity["eca_basefundingsrcount"] = record.Base_SR_Count;
			}
			if (Base_SR_RenewalCount != record.Base_SR_RenewalCount)
			{
				updatedEntity["eca_basefundingsrrenewalcount"] = record.Base_SR_RenewalCount;
			}
			if (Base_SR_Amount != record.Base_SR_Amount)
			{
				updatedEntity["eca_basefundingsramount"] = new Money(record.Base_SR_Amount.GetValueOrDefault(0));
			}
			if (Base_CentrallyFundedCount != record.Base_CentrallyFundedCount)
			{
				updatedEntity["eca_basefundingcentrallyfundedcount"] = record.Base_CentrallyFundedCount;
			}

			if (Carryover_NewCount != record.Carryover_NewCount)
			{
				updatedEntity["eca_carryoverfundingnewcount"] = record.Carryover_NewCount;
			}
			if (Carryover_LectureCount != record.Carryover_LectureCount)
			{
				updatedEntity["eca_carryoverfundingnewlecturecount"] = record.Carryover_LectureCount;
			}
			if (Carryover_ResearchCount != record.Carryover_ResearchCount)
			{
				updatedEntity["eca_carryoverfundingnewresearchcount"] = record.Carryover_ResearchCount;
			}
			if (Carryover_IEACount != record.Carryover_IEACount)
			{
				updatedEntity["eca_carryoverfundingnewieacount"] = record.Carryover_IEACount;
			}
			if (Carryover_NewAmount != record.Carryover_NewAmount)
			{
				updatedEntity["eca_carryoverfundingnewamount"] = new Money(record.Carryover_NewAmount.GetValueOrDefault(0));
			}
			if (Carryover_RenewalCount != record.Carryover_RenewalCount)
			{
				updatedEntity["eca_carryoverfundingrenewalcount"] = record.Carryover_RenewalCount;
			}
			if (Carryover_RenewalAmount != record.Carryover_RenewalAmount)
			{
				updatedEntity["eca_carryoverfundingrenewalamount"] = new Money(record.Carryover_RenewalAmount.GetValueOrDefault(0));
			}

			if (Recovery_NewCount != record.Recovery_NewCount)
			{
				updatedEntity["eca_recoveryfundingnewcount"] = record.Recovery_NewCount;
			}
			if (Recovery_LectureCount != record.Recovery_LectureCount)
			{
				updatedEntity["eca_recoveryfundingnewlecturecount"] = record.Recovery_LectureCount;
			}
			if (Recovery_ResearchCount != record.Recovery_ResearchCount)
			{
				updatedEntity["eca_recoveryfundingnewresearchcount"] = record.Recovery_ResearchCount;
			}
			if (Recovery_IEACount != record.Recovery_IEACount)
			{
				updatedEntity["eca_recoveryfundingnewieacount"] = record.Recovery_IEACount;
			}
			if (Recovery_NewAmount != record.Recovery_NewAmount)
			{
				updatedEntity["eca_recoveryfundingnewamount"] = new Money(record.Recovery_NewAmount.GetValueOrDefault(0));
			}
			if (Recovery_RenewalCount != record.Recovery_RenewalCount)
			{
				updatedEntity["eca_recoveryfundingrenewalcount"] = record.Recovery_RenewalCount;
			}
			if (Recovery_RenewalAmount != record.Recovery_RenewalAmount)
			{
				updatedEntity["eca_recoveryfundingrenewalamount"] = new Money(record.Recovery_RenewalAmount.GetValueOrDefault(0));
			}

			if (OtherFunding_NewCount != record.OtherFunding_NewCount)
			{
				updatedEntity["eca_otherfundingnewcount"] = record.OtherFunding_NewCount;
			}
			if (OtherFunding_LectureCount != record.OtherFunding_LectureCount)
			{
				updatedEntity["eca_otherfundingnewlecturecount"] = record.OtherFunding_LectureCount;
			}
			if (OtherFunding_ResearchCount != record.OtherFunding_ResearchCount)
			{
				updatedEntity["eca_otherfundingnewresearchcount"] = record.OtherFunding_ResearchCount;
			}
			if (OtherFunding_IEACount != record.OtherFunding_IEACount)
			{
				updatedEntity["eca_otherfundingnewieacount"] = record.OtherFunding_IEACount;
			}
			if (OtherFunding_NewAmount != record.OtherFunding_NewAmount)
			{
				updatedEntity["eca_otherfundingnewamount"] = new Money(record.OtherFunding_NewAmount.GetValueOrDefault(0));
			}
			if (OtherFunding_RenewalCount != record.OtherFunding_RenewalCount)
			{
				updatedEntity["eca_otherfundingrenewalcount"] = record.OtherFunding_RenewalCount;
			}
			if (OtherFunding_RenewalAmount != record.OtherFunding_RenewalAmount)
			{
				updatedEntity["eca_otherfundingrenewalamount"] = new Money(record.OtherFunding_RenewalAmount.GetValueOrDefault(0));
			}

			if (OtherFunding2_NewCount != record.OtherFunding2_NewCount)
			{
				updatedEntity["eca_otherfunding2newcount"] = record.OtherFunding2_NewCount;
			}
			if (OtherFunding2_LectureCount != record.OtherFunding2_LectureCount)
			{
				updatedEntity["eca_otherfunding2newlecturecount"] = record.OtherFunding2_LectureCount;
			}
			if (OtherFunding2_ResearchCount != record.OtherFunding2_ResearchCount)
			{
				updatedEntity["eca_otherfunding2newresearchcount"] = record.OtherFunding2_ResearchCount;
			}
			if (OtherFunding2_IEACount != record.OtherFunding2_IEACount)
			{
				updatedEntity["eca_otherfunding2newieacount"] = record.OtherFunding2_IEACount;
			}
			if (OtherFunding2_NewAmount != record.OtherFunding2_NewAmount)
			{
				updatedEntity["eca_otherfunding2newamount"] = new Money(record.OtherFunding2_NewAmount.GetValueOrDefault(0));
			}
			if (OtherFunding2_RenewalCount != record.OtherFunding2_RenewalCount)
			{
				updatedEntity["eca_otherfunding2renewalcount"] = record.OtherFunding2_RenewalCount;
			}
			if (OtherFunding2_RenewalAmount != record.OtherFunding2_RenewalAmount)
			{
				updatedEntity["eca_otherfunding2renewalamount"] = new Money(record.OtherFunding2_RenewalAmount.GetValueOrDefault(0));
			}

			//Set the version
			updatedEntity["eca_version"] = version;

			// Return the entity if we have changes
			if (updatedEntity.Attributes.Count > 0)
			{
				return updatedEntity;
			}

			return null;
		}


		#endregion

	}
}
