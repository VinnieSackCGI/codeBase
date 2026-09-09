
using System;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_QuotaSheetHeader : IQuotaSheetHeaderRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_QuotaSheetHeaderState.Active;
		}

		#endregion

		#region Data Properties

		public string Name
		{
			get => eca_Name;
			set => eca_Name = value;
		}

		public string AwardNumber
		{
			get => eca_AwardNumber;
			set => eca_AwardNumber = value;
		}

		public string SubProgramCode
		{
			get => eca_SubProgramCode;
			set => eca_SubProgramCode = value;
		}

		public int FiscalYear
		{
			get => eca_FiscalYear.GetValueOrDefault(0);
			set => eca_FiscalYear = value;
		}

		public EntityReference Appropriation
		{
			get => eca_Appropriation;
			set => eca_Appropriation = value;
		}

		public string Region
		{
			get => eca_Region;
			set => eca_Region = value;
		}

		public string SubRegion
		{
			get => eca_SubRegion;
			set => eca_SubRegion = value;
		}

		public string GranteeType
		{
			get => eca_GranteeType.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_GranteeType type;
					if (Enum.TryParse<eca_GranteeType>(value, out type))
					{
						eca_GranteeType = type;
					}
				}
				else
				{
					eca_GranteeType = null;
				}
			}
		}

		public string Notes
		{
			get => eca_Notes;
			set => eca_Notes = value;
		}

		public string PhaseType 
		{
			get => eca_RevisionType.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					eca_quotasheetversions revision;
					if (Enum.TryParse<eca_quotasheetversions>(value, out revision))
					{
						eca_RevisionType = revision;
					}
				}
			}
		}

		public int? Version
		{
			get => eca_Version;
			set => eca_Version = value;
		}

		public Entity Entity
		{
			get => this;
		}
        public int? Status 
        { 
            get => eca_Status; 
            set => eca_Status = value; 
        }

        #endregion

        #region Public Methods

        public void UpdateFromNewVersion( IQuotaSheetHeaderRecord newRecord )
		{
			// Apply the incopming data updates to the current record
			if (newRecord.Name != this.Name)
			{
				Name = newRecord.Name;
			}
			if (newRecord.AwardNumber != AwardNumber)
			{
				AwardNumber = newRecord.AwardNumber;
			}
			if (newRecord.SubProgramCode != SubProgramCode)
			{
				SubProgramCode = newRecord.SubProgramCode;
			}
			if (newRecord.FiscalYear != FiscalYear)
			{
				FiscalYear = newRecord.FiscalYear;
			}
			if (newRecord.Appropriation != null && newRecord.Appropriation.Id != Appropriation?.Id )
			{ 
				Appropriation = newRecord.Appropriation;
			}
			if (newRecord.Region != Region)
			{
				Region = newRecord.Region;
			}
			if (newRecord.SubRegion != SubRegion)
			{
				SubRegion = newRecord.SubRegion;
			}
			if (newRecord.GranteeType != GranteeType)
			{
				GranteeType = newRecord.GranteeType;
			}
			if (newRecord.Notes != Notes)
			{
				Notes = newRecord.Notes;
			}

			if (Version == null)
			{
				Version = 1;
			}
			else
			{
				Version++;
			}
		}

		#endregion

	}
}