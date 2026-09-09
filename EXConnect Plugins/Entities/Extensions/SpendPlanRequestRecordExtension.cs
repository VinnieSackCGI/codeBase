using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_FormulationRequest : ISpendPlanRequestRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_FormulationRequestState.Active;
		}

		#endregion

		#region Data Properties

		public string Name
		{
			get => eca_FormulationRequest1;

			set
			{
				eca_FormulationRequest1 = value;
			}
		}

		public EntityReference FunctionCode
		{
			get => eca_FunctionCode;
			set
			{
				eca_FunctionCode = value;
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
			get => eca_Account;
			set
			{
				eca_Account = value;
			}
		}

		public string FiscalYear
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
				return eca_RequestType.ToString();
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					msdyn_flow_approval_RequestType type;
					if (Enum.TryParse<msdyn_flow_approval_RequestType>(value, out type))
					{
						eca_RequestType = type;
					}
				}
			}
		}

		public string Justification
		{
			get => eca_Justification;
			set => eca_Justification = value;
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
			}
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
					eca_Amount = value;
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
					eca_FormulationRequest_StatusCode type;
					if (Enum.TryParse<eca_FormulationRequest_StatusCode>(value, out type))
					{
						StatusCode = type;
					}
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

		#endregion

		#region Public Methods

		public ISpendPlanRequestRecord GetCombinedRecord( ISpendPlanRequestRecord sprRecord )
		{
			// Generate a combined transaction record fromjm "this" transaction and another one. Emphasis on "this" transaction.

			var combinedRecord = new eca_FormulationRequest();

			combinedRecord.Id = this.Id;
			combinedRecord.Appropriation = (this.Appropriation != null ? this.Appropriation : sprRecord.Appropriation);
			combinedRecord.CUFFaccount = (this.CUFFaccount != null ? this.CUFFaccount : sprRecord.CUFFaccount);
			combinedRecord.FundingType = (this.FundingType != null ? this.FundingType : sprRecord.FundingType);
			combinedRecord.FiscalYear = (this.FiscalYear != null ? this.FiscalYear : sprRecord.FiscalYear);
			combinedRecord.Type = (!string.IsNullOrEmpty(this.Type) ? this.Type : sprRecord.Type);
			combinedRecord.Status = (!string.IsNullOrEmpty(this.Status) ? this.Status : sprRecord.Status);
			combinedRecord.Name = (this.Name != null ? this.Name : sprRecord.Name);
			combinedRecord.Amount = (this.Attributes.Contains("eca_amount") ? this.Amount : sprRecord.Amount);
			combinedRecord.Justification = (this.Justification != null ? this.Justification : sprRecord.Justification);

			return combinedRecord;
		}

		#endregion
	}
}
