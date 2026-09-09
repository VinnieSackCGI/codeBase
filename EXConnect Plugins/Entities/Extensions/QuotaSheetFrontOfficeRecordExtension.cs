
using System;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_FrontOfficeQuotaSheet : IQuotaSheetFrontOfficeDataRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_FrontOfficeQuotaSheetState.Active;
		}

		#endregion

		#region Data fields

		public string Name
		{
			get => eca_FundingSource;
			set => eca_FundingSource = value;
		}

		public EntityReference CUFFaccount
		{
			get => eca_CUFFAccount;
			set => eca_CUFFAccount = value;
		}

		public EntityReference ParentHeader
		{
			get => eca_ParentHeader;
			set => eca_ParentHeader = value;
		}

		public int? Version
		{
			get => eca_Version;
			set => eca_Version = value;
		}

        public int? QSSortOrder
        {
            get { return GetAttributeValue<int?>("eca_qssortorder"); }
            set { SetAttributeValue("eca_qssortorder", value); }
        }

        // Preliminary
        public decimal? Preliminary_BaseFunding
		{
			get
			{
				if (eca_PreliminaryBaseFunding != null)
				{
					return eca_PreliminaryBaseFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_PreliminaryBaseFunding = new Money(value.Value);
				}
				else
				{
					eca_PreliminaryBaseFunding = null;
				}
			}
		}

		// Initial
		public decimal? Initial_BaseFunding
		{
			get
			{
				if (eca_InitialBaseFunding != null)
				{
					return eca_InitialBaseFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialBaseFunding = new Money(value.Value);
				}
				else
				{
					eca_InitialBaseFunding = null;
				}
			}
		}
		public decimal? Initial_CarryoverFunding
		{
			get
			{
				if (eca_InitialCarryOverFunding != null)
				{
					return eca_InitialCarryOverFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialCarryOverFunding = new Money(value.Value);
				}
				else
				{
					eca_InitialCarryOverFunding = null;
				}
			}
		}
		public decimal? Initial_RecoveryFunding
		{
			get
			{
				if (eca_InitialRecoveryFunding != null)
				{
					return eca_InitialRecoveryFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialRecoveryFunding = new Money(value.Value);
				}
				else
				{
					eca_InitialRecoveryFunding = null;
				}
			}
		}
		public decimal? Initial_AfghanistanFunding
		{
			get
			{
				if (eca_InitialAfghanistanFunding != null)
				{
					return eca_InitialAfghanistanFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialAfghanistanFunding = new Money(value.Value);
				}
				else
				{
					eca_InitialAfghanistanFunding = null;
				}
			}
		}
		public decimal? Initial_OtherFunding1
		{
			get
			{
				if (eca_InitialOtherFunding1 != null)
				{
					return eca_InitialOtherFunding1.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialOtherFunding1 = new Money(value.Value);
				}
				else
				{
					eca_InitialOtherFunding1 = null;
				}
			}
		}
		public decimal? Initial_OtherFunding2
		{
			get
			{
				if (eca_InitialOtherFunding2 != null)
				{
					return eca_InitialOtherFunding2.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_InitialOtherFunding2 = new Money(value.Value);
				}
				else
				{
					eca_InitialOtherFunding2 = null;
				}
			}
		}

		// Final
		public decimal? Final_BaseFunding
		{
			get
			{
				if (eca_FinalBaseFunding != null)
				{
					return eca_FinalBaseFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalBaseFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalBaseFunding = null;
				}
			}
		}
		public decimal? Final_CarryoverFunding
		{
			get
			{
				if (eca_FinalCarryOverFunding != null)
				{
					return eca_FinalCarryOverFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalCarryOverFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalCarryOverFunding = null;
				}
			}
		}
		public decimal? Final_RecoveryFunding
		{
			get
			{
				if (eca_FinalRecoveryFunding != null)
				{
					return eca_FinalRecoveryFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalRecoveryFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalRecoveryFunding = null;
				}
			}
		}
		public decimal? Final_AfghanistanFunding
		{
			get
			{
				if (eca_FinalAfghanistanFunding != null)
				{
					return eca_FinalAfghanistanFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalAfghanistanFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalAfghanistanFunding = null;
				}
			}
		}
		public decimal? Final_OtherFunding1
		{
			get
			{
				if (eca_FinalOtherFunding1 != null)
				{
					return eca_FinalOtherFunding1.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalOtherFunding1 = new Money(value.Value);
				}
				else
				{
					eca_FinalOtherFunding1 = null;
				}
			}
		}

        public decimal? Final_OtherFunding2
		{
			get
			{
				if (eca_FinalOtherFunding2 != null)
				{
					return eca_FinalOtherFunding2.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalOtherFunding2 = new Money(value.Value);
				}
				else
				{
					eca_FinalOtherFunding2 = null;
				}
			}
		}

		// Final-Final
		public decimal? FinalFinal_BaseFunding
		{
			get
			{
				if (eca_FinalFinalBaseFunding != null)
				{
					return eca_FinalFinalBaseFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalBaseFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalBaseFunding = null;
				}
			}
		}
		public decimal? FinalFinal_CarryoverFunding
		{
			get
			{
				if (eca_FinalFinalCarryOverFunding != null)
				{
					return eca_FinalFinalCarryOverFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalCarryOverFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalCarryOverFunding = null;
				}
			}
		}
		public decimal? FinalFinal_RecoveryFunding
		{
			get
			{
				if (eca_FinalFinalRecoveryFunding != null)
				{
					return eca_FinalFinalRecoveryFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalRecoveryFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalRecoveryFunding = null;
				}
			}
		}
		public decimal? FinalFinal_AfghanistanFunding
		{
			get
			{
				if (eca_FinalFinalAfghanistanFunding != null)
				{
					return eca_FinalFinalAfghanistanFunding.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalAfghanistanFunding = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalAfghanistanFunding = null;
				}
			}
		}
		public decimal? FinalFinal_OtherFunding1
		{
			get
			{
				if (eca_FinalFinalOtherFunding1 != null)
				{
					return eca_FinalFinalOtherFunding1.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalOtherFunding1 = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalOtherFunding1 = null;
				}
			}
		}
		public decimal? FinalFinal_OtherFunding2
		{
			get
			{
				if (eca_FinalFinalOtherFunding2 != null)
				{
					return eca_FinalFinalOtherFunding2.Value;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					eca_FinalFinalOtherFunding2 = new Money(value.Value);
				}
				else
				{
					eca_FinalFinalOtherFunding2 = null;
				}
			}
		}
        // Amendment 5
        public decimal? Amendment5_BaseFunding
        {
            get
            {
                if (eca_amendment5basefunding != null)
                {
                    return eca_amendment5basefunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_amendment5basefunding = new Money(value.Value);
                }
                else
                {
                    eca_amendment5basefunding = null;
                }
            }
        }
        public decimal? Amendment5_CarryoverFunding
        {
            get
            {
                if (eca_Amendment5CarryoverFunding != null)
                {
                    return eca_Amendment5CarryoverFunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment5CarryoverFunding = new Money(value.Value);
                }
                else
                {
                    eca_Amendment5CarryoverFunding = null;
                }
            }
        }
        public decimal? Amendment5_RecoveryFunding
        {
            get
            {
                if (eca_Amendment5RecoveryFunding != null)
                {
                    return eca_Amendment5RecoveryFunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment5RecoveryFunding = new Money(value.Value);
                }
                else
                {
                    eca_Amendment5RecoveryFunding = null;
                }
            }
        }
        public decimal? Amendment5_OtherFunding1
        {
            get
            {
                if (eca_Amendment5OtherFunding1 != null)
                {
                    return eca_Amendment5OtherFunding1.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment5OtherFunding1 = new Money(value.Value);
                }
                else
                {
                    eca_Amendment5OtherFunding1 = null;
                }
            }
        }
        public decimal? Amendment5_OtherFunding2
        {
            get
            {
                if (eca_Amendment5OtherFunding2 != null)
                {
                    return eca_Amendment5OtherFunding2.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment5OtherFunding2 = new Money(value.Value);
                }
                else
                {
                    eca_Amendment5OtherFunding2 = null;
                }
            }
        }

        // Amendment 6
        public decimal? Amendment6_BaseFunding
        {
            get
            {
                if (eca_Amendment6BaseFunding != null)
                {
                    return eca_Amendment6BaseFunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment6BaseFunding = new Money(value.Value);
                }
                else
                {
                    eca_Amendment6BaseFunding = null;
                }
            }
        }
        public decimal? Amendment6_CarryoverFunding
        {
            get
            {
                if (eca_Amendment6CarryoverFunding != null)
                {
                    return eca_Amendment6CarryoverFunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment6CarryoverFunding = new Money(value.Value);
                }
                else
                {
                    eca_Amendment6CarryoverFunding = null;
                }
            }
        }
        public decimal? Amendment6_RecoveryFunding
        {
            get
            {
                if (eca_Amendment6RecoveryFunding != null)
                {
                    return eca_Amendment6RecoveryFunding.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment6RecoveryFunding = new Money(value.Value);
                }
                else
                {
                    eca_Amendment6RecoveryFunding = null;
                }
            }
        }
        public decimal? Amendment6_OtherFunding1
        {
            get
            {
                if (eca_Amendment6OtherFunding1 != null)
                {
                    return eca_Amendment6OtherFunding1.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment6OtherFunding1 = new Money(value.Value);
                }
                else
                {
                    eca_Amendment6OtherFunding1 = null;
                }
            }
        }
        public decimal? Amendment6_OtherFunding2
        {
            get
            {
                if (eca_Amendment6OtherFunding2 != null)
                {
                    return eca_Amendment6OtherFunding2.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    eca_Amendment6OtherFunding2 = new Money(value.Value);
                }
                else
                {
                    eca_Amendment6OtherFunding2 = null;
                }
            }
        }

        public Entity Entity
		{
			get => this;
		}
        
        #endregion

        #region Public Methods

        public Entity GetUpdateEntityFromNewVersion( IQuotaSheetFrontOfficeDataRecord record, EntityReference accountRef, int version)
		{
			// Update the current record with the incoming data, if different
			// Name and Parent Header, should not have changed.

			// Keep track of changes in an Entity object
			var updatedEntity = new Entity(this.LogicalName, this.Id);

            // Start with the CUFF Account if different
            if (CUFFaccount == null || CUFFaccount.Id != accountRef.Id)
            {
                updatedEntity["eca_cuffaccount"] = accountRef;
            }

            if (Preliminary_BaseFunding != record.Preliminary_BaseFunding)
			{
				updatedEntity["eca_preliminarybasefunding"] = new Money(record.Preliminary_BaseFunding.GetValueOrDefault(0));
			}
			if (Initial_BaseFunding != record.Initial_BaseFunding)
			{
				updatedEntity["eca_initialbasefunding"] = new Money(record.Initial_BaseFunding.GetValueOrDefault(0));
			}
			if (Initial_CarryoverFunding != record.Initial_CarryoverFunding)
			{
				updatedEntity["eca_initialcarryoverfunding"] = new Money(record.Initial_CarryoverFunding.GetValueOrDefault(0));
			}
			if (Initial_RecoveryFunding != record.Initial_RecoveryFunding)
			{
				updatedEntity["eca_initialrecoveryfunding"] = new Money(record.Initial_RecoveryFunding.GetValueOrDefault(0));
			}
			if (Initial_AfghanistanFunding != record.Initial_AfghanistanFunding)
			{
				updatedEntity["eca_initialafghanistanfunding"] = new Money(record.Initial_AfghanistanFunding.GetValueOrDefault(0));
			}
			if (Initial_OtherFunding1 != record.Initial_OtherFunding1)
			{
				updatedEntity["eca_initialotherfunding1"] = new Money(record.Initial_OtherFunding1.GetValueOrDefault(0));
			}
			if (Initial_OtherFunding2 != record.Initial_OtherFunding2)
			{
				updatedEntity["eca_initialotherfunding2"] = new Money(record.Initial_OtherFunding2.GetValueOrDefault(0));
			}

			if (Final_BaseFunding != record.Final_BaseFunding)
			{
				updatedEntity["eca_finalbasefunding"] = new Money(record.Final_BaseFunding.GetValueOrDefault(0));
			}
			if (Final_CarryoverFunding != record.Final_CarryoverFunding)
			{
				updatedEntity["eca_finalcarryoverfunding"] = new Money(record.Final_CarryoverFunding.GetValueOrDefault(0));
			}
			if (Final_RecoveryFunding != record.Final_RecoveryFunding)
			{
				updatedEntity["eca_finalrecoveryfunding"] = new Money(record.Final_RecoveryFunding.GetValueOrDefault(0));
			}
			if (Final_AfghanistanFunding != record.Final_AfghanistanFunding)
			{
				updatedEntity["eca_finalafghanistanfunding"] = new Money(record.Final_AfghanistanFunding.GetValueOrDefault(0));
			}
			if (Final_OtherFunding1 != record.Final_OtherFunding1)
			{
				updatedEntity["eca_finalotherfunding1"] = new Money(record.Final_OtherFunding1.GetValueOrDefault(0));
			}
			if (Final_OtherFunding2 != record.Final_OtherFunding2)
			{
				updatedEntity["eca_finalotherfunding2"] = new Money(record.Final_OtherFunding2.GetValueOrDefault(0));
			}

			if (FinalFinal_BaseFunding != record.FinalFinal_BaseFunding)
			{
				updatedEntity["eca_finalfinalbasefunding"] = new Money(record.FinalFinal_BaseFunding.GetValueOrDefault(0));
			}
			if (FinalFinal_CarryoverFunding != record.FinalFinal_CarryoverFunding)
			{
				updatedEntity["eca_finalfinalcarryoverfunding"] = new Money(record.FinalFinal_CarryoverFunding.GetValueOrDefault(0));
			}
			if (FinalFinal_RecoveryFunding != record.FinalFinal_RecoveryFunding)
			{
				updatedEntity["eca_finalfinalrecoveryfunding"] = new Money(record.FinalFinal_RecoveryFunding.GetValueOrDefault(0));
			}
			if (FinalFinal_AfghanistanFunding != record.FinalFinal_AfghanistanFunding)
			{
				updatedEntity["eca_finalfinalafghanistanfunding"] = new Money(record.FinalFinal_AfghanistanFunding.GetValueOrDefault(0));
			}
			if (FinalFinal_OtherFunding1 != record.FinalFinal_OtherFunding1)
			{
				updatedEntity["eca_finalfinalotherfunding1"] = new Money(record.FinalFinal_OtherFunding1.GetValueOrDefault(0));
			}
			if (FinalFinal_OtherFunding2 != record.FinalFinal_OtherFunding2)
			{
				updatedEntity["eca_finalfinalotherfunding2"] = new Money(record.FinalFinal_OtherFunding2.GetValueOrDefault(0));
			}
            if (Amendment5_BaseFunding != record.Amendment5_BaseFunding)
            {
                updatedEntity["eca_amendment5basefunding"] = new Money(record.Amendment5_BaseFunding.GetValueOrDefault(0));
            }
            if (Amendment5_CarryoverFunding != record.Amendment5_CarryoverFunding)
            {
                updatedEntity["eca_amendment5carryoverfunding"] = new Money(record.Amendment5_CarryoverFunding.GetValueOrDefault(0));
            }
            if (Amendment5_RecoveryFunding != record.Amendment5_RecoveryFunding)
            {
                updatedEntity["eca_amendment5recoveryfunding"] = new Money(record.Amendment5_RecoveryFunding.GetValueOrDefault(0));
            }
            if (Amendment5_OtherFunding1 != record.Amendment5_OtherFunding1)
            {
                updatedEntity["eca_amendment5otherfunding1"] = new Money(record.Amendment5_OtherFunding1.GetValueOrDefault(0));
            }
            if (Amendment5_OtherFunding2 != record.Amendment5_OtherFunding2)
            {
                updatedEntity["eca_amendment5otherfunding2"] = new Money(record.Amendment5_OtherFunding2.GetValueOrDefault(0));
            }

            if (Amendment6_BaseFunding != record.Amendment6_BaseFunding)
            {
                updatedEntity["eca_amendment6basefunding"] = new Money(record.Amendment6_BaseFunding.GetValueOrDefault(0));
            }
            if (Amendment6_CarryoverFunding != record.Amendment6_CarryoverFunding)
            {
                updatedEntity["eca_amendment6carryoverfunding"] = new Money(record.Amendment6_CarryoverFunding.GetValueOrDefault(0));
            }
            if (Amendment6_RecoveryFunding != record.Amendment6_RecoveryFunding)
            {
                updatedEntity["eca_amendment6recoveryfunding"] = new Money(record.Amendment6_RecoveryFunding.GetValueOrDefault(0));
            }
            if (Amendment6_OtherFunding1 != record.Amendment6_OtherFunding1)
            {
                updatedEntity["eca_amendment6otherfunding1"] = new Money(record.Amendment6_OtherFunding1.GetValueOrDefault(0));
            }
            if (Amendment6_OtherFunding2 != record.Amendment6_OtherFunding2)
            {
                updatedEntity["eca_amendment6otherfunding2"] = new Money(record.Amendment6_OtherFunding2.GetValueOrDefault(0));
            }
            //Set the version
            updatedEntity["eca_version"] = version;

			// Return the updated Entity only if we have updates
			if (updatedEntity.Attributes.Count > 0)
			{
				return updatedEntity;
			}

			return null;

		}

		#endregion

	}
}
