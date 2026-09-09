using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_StagedBulkUploadRecords : IStagedBulkUploadRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_StagedBulkUploadRecordsState.Active;
		}

		public bool IsAppropriationAllocation
		{
			get => eca_TemplateType == eca_TemplateTypes.AppropriationAllocation;
		}
		public bool IsAOA
		{
			get => eca_TemplateType == eca_TemplateTypes.AOA;
		}

		public bool IsTransaction
		{
			get => IsAOA || IsAppropriationAllocation;
		}

		public bool IsSpendPlanRequest
		{
			get => eca_TemplateType == eca_TemplateTypes.SpendPlanRequest;
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

		public string TemplateType
		{
			get
			{
				return eca_TemplateType.ToString();
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_TemplateTypes type;
					if (Enum.TryParse<eca_TemplateTypes>(value, out type))
					{
						eca_TemplateType = type;
					}
				}
				else
				{
					eca_TemplateType = null;
				}
			}
		}

		public string IBIS_RequestCode
		{
			get => eca_IBISRequestCode;
			set => eca_IBISRequestCode = value;
		}

		public string RequestName
		{
			get => eca_RequestName;
			set => eca_RequestName = value;
		}

		public string ProjectCode
		{
			get => eca_ProjectCode;
			set => eca_ProjectCode = value;
		}
		public string Priority
		{
			get => eca_Priority.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_Priority type;
					if (Enum.TryParse<eca_Priority>(value, out type))
					{
						eca_Priority = type;
					}
				}
				else
				{
					eca_Priority = null;
				}
			}
		}

		public string State
		{
			get => StateCode.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_StagedBulkUploadRecordsState type;
					if (Enum.TryParse<eca_StagedBulkUploadRecordsState>(value, out type))
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
					eca_StagedBulkUploadRecords_StatusCode type;
					if (Enum.TryParse<eca_StagedBulkUploadRecords_StatusCode>(value, out type))
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

		public string Description_Justification
		{
			get => eca_DescriptionJustification;
			set => eca_DescriptionJustification = value;
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

		public IStagedBulkUploadRecord GetCombinedRecord( IStagedBulkUploadRecord record )
		{
			// Generate a combined transaction record fromjm "this" transaction and another one. Emphasis on "this" transaction.

			var combinedRecord = new eca_StagedBulkUploadRecords();

			combinedRecord.Id = this.Id;
			combinedRecord.FundingSource = (this.FundingSource != null ? this.FundingSource : record.FundingSource);
			combinedRecord.FundingType = (this.FundingType != null ? this.FundingType : record.FundingType);
			combinedRecord.Appropriation = (this.Appropriation != null ? this.Appropriation : record.Appropriation);
			combinedRecord.CUFFaccount = (this.CUFFaccount != null ? this.CUFFaccount : record.CUFFaccount);
			combinedRecord.FromCUFFaccount = (this.FromCUFFaccount != null ? this.FromCUFFaccount : record.FromCUFFaccount);
			combinedRecord.FiscalYear = (this.FiscalYear != null ? this.FiscalYear : record.FiscalYear);
			combinedRecord.TemplateType = (!string.IsNullOrEmpty(this.TemplateType) ? this.TemplateType : record.TemplateType);
			combinedRecord.Description_Justification = (!string.IsNullOrEmpty(this.Description_Justification) ? this.Description_Justification : record.Description_Justification);
			combinedRecord.IBIS_RequestCode = (!string.IsNullOrEmpty(this.IBIS_RequestCode) ? this.IBIS_RequestCode : record.IBIS_RequestCode);
			combinedRecord.RequestName = (!string.IsNullOrEmpty(this.RequestName) ? this.RequestName : record.RequestName);
			combinedRecord.ProjectCode = (!string.IsNullOrEmpty(this.ProjectCode) ? this.ProjectCode : record.ProjectCode);
			combinedRecord.Priority = (!string.IsNullOrEmpty(this.Priority) ? this.Priority : record.Priority);
			combinedRecord.Amount = (this.Attributes.Contains("eca_amount") ? this.Amount : record.Amount);

			return combinedRecord;
		}


		#endregion
	}
}
