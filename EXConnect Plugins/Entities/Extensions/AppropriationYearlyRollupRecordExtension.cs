using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
    public partial class rcade_AppropriationYearlyRollup : IAppropriationYearlyRollupRecord
    {
		#region Interrogation Properties
        #endregion
		
        #region Data Properties

		public string Name
        {
            get => rcade_Name;

            set
            {
				rcade_Name = value;
            }
        }

		public int? FiscalYear
		{
			get => rcade_FiscalYear;
			set
			{
				rcade_FiscalYear = value;
			}
		}

		public EntityReference Appropriation
        {
            get => rcade_Appropriation;
            set
            {
                rcade_Appropriation = value;
            }
        }

        public decimal? AllocatedAppropriated
        {
            get
            {
                if (rcade_AllocatedAppropriated == null)
                {
                    return null;
                }
                return rcade_AllocatedAppropriated.Value;
            }
            set
            {
                if (rcade_AllocatedAppropriated != null)
                {
					rcade_AllocatedAppropriated = new Money(value ?? 0);
				}
			}
        }

		public decimal? AllocatedCarryover
		{
			get
			{
				if (rcade_AllocatedCarryover == null)
				{
					return null;
				}
				return rcade_AllocatedCarryover.Value;
			}
			set
			{
				if (rcade_AllocatedCarryover != null)
				{
					rcade_AllocatedCarryover = new Money(value ?? 0);
				}
			}
		}

		public decimal? AllocatedRecoveries
		{
			get
			{
				if (rcade_AllocatedRecoveries== null)
				{
					return null;
				}
				return rcade_AllocatedRecoveries.Value;
			}
			set
			{
				if (rcade_AllocatedRecoveries != null)
				{
					rcade_AllocatedRecoveries = new Money(value ?? 0);
				}
			}
		}

		public decimal? AllocatedReimbursements
		{
			get
			{
				if (rcade_AllocatedReimbursements == null)
				{
					return null;
				}
				return rcade_AllocatedReimbursements.Value;
			}
			set
			{
				if (rcade_AllocatedReimbursements != null)
				{
					rcade_AllocatedReimbursements = new Money(value ?? 0);
				}
			}
		}

		public Entity Entity
        {
            get
            {
                return this;
            }
        }

		public decimal? PendingTotalAllocated
		{
			get => (eca_PendingTotalAllocated == null ? 0 : eca_PendingTotalAllocated.Value);
			set => eca_PendingTotalAllocated = new Money(value ?? 0);
		}

		public decimal? TotalAllocated
		{
			get => (eca_TotalAllocated == null ? 0 : eca_TotalAllocated.Value);
			set => eca_TotalAllocated = new Money(value ?? 0);
		}

		public decimal? TotalResources
		{
			get => (eca_TotalResources == null ? 0 : eca_TotalResources.Value);
		}

		public decimal? AppropriatedBalance
		{
			get => (rcade_AppropriatedBalance == null ? 0 : rcade_AppropriatedBalance.Value);
		}

		#endregion
	}
}
