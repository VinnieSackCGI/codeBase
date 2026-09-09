using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_FundingType : IFundingTypeRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_FundingTypeState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => eca_FundingTypeName;
			set => eca_FundingTypeName = value;
		}

		public string Name
		{
			get => eca_FundingTypeName;
			set => eca_FundingTypeName = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion

	}
}
