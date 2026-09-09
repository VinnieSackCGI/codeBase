using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_Neighborhood : INeighborhoodRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_NeighborhoodState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => eca_Name;
			set => eca_Name = value;
		}

		public string Name
		{
			get => eca_Name;
			set => eca_Name = value;
		}

		public IEnumerable<SystemUser> BudgetAnalystsList
		{
			get => eca_Neighborhood_BusinessAnalysts;
			set => eca_Neighborhood_BusinessAnalysts = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion
	}
}
