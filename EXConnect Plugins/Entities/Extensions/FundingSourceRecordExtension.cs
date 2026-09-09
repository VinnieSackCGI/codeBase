using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_FundingSource : IFundingSourceRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_FundingSourceState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => eca_FundingSource1;
			set => eca_FundingSource1 = value;
		}

		public string Name
		{
			get => eca_SourceName;
			set => eca_SourceName = value;
		}

		public EntityReference FundingType
		{
			get => eca_ParentFundingType;
			set => eca_ParentFundingType = value;
		}

		public EntityReference Appropriation
		{
			get
			{
				var relatedAppropriation = eca_FundingSource_Appropriation;
				if (relatedAppropriation != null)
				{
					return relatedAppropriation.First().ToEntityReference();
				}
				return null;
			} 
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion

	}
}
