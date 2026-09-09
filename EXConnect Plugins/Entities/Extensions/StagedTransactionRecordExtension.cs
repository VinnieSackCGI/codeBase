using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_StagedTransaction : IStagedTransactionRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_StagedTransactionState.Active;
		}

		public bool IsAppropriationAllocation
		{
			get => eca_TransactionType == eca_ECATransactionTypes.AppropriationAllocation;
		}
		public bool IsAOA
		{
			get => eca_TransactionType == eca_ECATransactionTypes.AOA;
		}

		public bool IsCommitment
		{
			get => eca_TransactionType == eca_ECATransactionTypes.Commitment;
		}

		public bool IsObligation
		{
			get => eca_TransactionType == eca_ECATransactionTypes.Obligation;
		}
		public bool IsProgramTransfer
		{
			get => eca_TransactionType == eca_ECATransactionTypes.ProgramTransfer;
		}

		public bool IsReimbursement
		{
			get => eca_FundingType.Name == "Reimbursement";
		}

		#endregion

		#region Data Properties

		public string Name
		{
			get => eca_Name;

			set
			{
				eca_Name = value;
			}
		}

		public EntityReference Appropriation
		{
			get => eca_Appropriation;
			set
			{
				eca_Appropriation = value;
			}
		}

		public EntityReference CUFFaccount
		{
			get => eca_CUFFAccount;
			set
			{
				eca_CUFFAccount = value;
			}
		}

		public EntityReference FromCUFFaccount
		{
			get => eca_FromCUFFAccount;
			set
			{
				eca_FromCUFFAccount = value;
			}
		}

		public int? FiscalYear
		{
			get => eca_FiscalYear;
			set
			{
				eca_FiscalYear = value;
			}
		}

		public EntityReference FundingType
		{
			get => eca_FundingType;
			set
			{
				eca_FundingType = value;
			}
		}

		public string Type
		{
			get
			{
				return eca_TransactionType.ToString();
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_ECATransactionTypes type;
					if (Enum.TryParse<eca_ECATransactionTypes>(value, out type))
					{
						eca_TransactionType = type;
					}
				}
				else
				{
					eca_TransactionType = null;
				}
			}
		}

		public string IBIS_RequestCode
		{
			get => eca_IBISRequestCode;
			set => eca_IBISRequestCode = value;
		}

		public string ProjectCode 
		{
			get => eca_ProjectCode;
			set => eca_ProjectCode = value;
		}

		public string State 
		{
			get => StateCode.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_StagedTransactionState type;
					if (Enum.TryParse<eca_StagedTransactionState>(value, out type))
					{
						StateCode = type;
					}
				}
				else
				{
					StateCode = null;
				}
			}
		}

		public string Status
		{
			get => StatusCode.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_StagedTransaction_StatusCode type;
					if (Enum.TryParse<eca_StagedTransaction_StatusCode>(value, out type))
					{
						StatusCode = type;
					}
				}
				else
				{
					StatusCode = null;
				}
			}
		}

		public string Description
		{
			get => eca_Description;
			set => eca_Description = value;
		}

		public decimal? Amount
		{
			get
			{
				if (eca_Amount == null)
				{
					return null;
				}
				return eca_Amount.Value;
			}
			set
			{
				if (value != null)
				{
					eca_Amount = new Money(value ?? 0);
				}
			}
		}

		public EntityReference FundingSource
		{
			get => eca_FundingSource;
			set => eca_FundingSource = value;
		}

		public bool IsValid
		{
			get => eca_IsValid.GetValueOrDefault();
			set => eca_IsValid = value;
		}

		public string ValidationErrors
		{
			get => eca_ValidationErrors;
			set => eca_ValidationErrors = value;
		}

		public Entity Entity
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region Methods

		public IStagedTransactionRecord GetCombinedTransactionRecord( IStagedTransactionRecord transaction2 )
		{
			// Generate a combined transaction record fromjm "this" transaction and another one. Emphasis on "this" transaction.

			var combinedRecord = new eca_StagedTransaction();

			combinedRecord.Id = this.Id;
			combinedRecord.FundingSource = (this.FundingSource != null ? this.FundingSource : transaction2.FundingSource);
			combinedRecord.FundingType = (this.FundingType != null ? this.FundingType : transaction2.FundingType);
			combinedRecord.Appropriation = (this.Appropriation != null ? this.Appropriation : transaction2.Appropriation);
			combinedRecord.CUFFaccount = (this.CUFFaccount != null ? this.CUFFaccount : transaction2.CUFFaccount);
			combinedRecord.FromCUFFaccount = (this.FromCUFFaccount != null ? this.FromCUFFaccount : transaction2.FromCUFFaccount);
			combinedRecord.FiscalYear = (this.FiscalYear != null ? this.FiscalYear : transaction2.FiscalYear);
			combinedRecord.Type = (!string.IsNullOrEmpty(this.Type) ? this.Type : transaction2.Type);
			combinedRecord.Description = (!string.IsNullOrEmpty(this.Description) ? this.Description : transaction2.Description);
			combinedRecord.IBIS_RequestCode = (!string.IsNullOrEmpty(this.IBIS_RequestCode) ? this.IBIS_RequestCode : transaction2.IBIS_RequestCode);
			combinedRecord.ProjectCode = (!string.IsNullOrEmpty(this.ProjectCode) ? this.ProjectCode : transaction2.ProjectCode);
			combinedRecord.Amount = (this.Attributes.Contains("eca_amount") ? this.Amount : transaction2.Amount);

			return combinedRecord;
		}


		#endregion

	}
}
