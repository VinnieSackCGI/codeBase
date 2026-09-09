using System;
using System.Activities.Debugger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_Appropriation : IAppropriationRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == rcade_AppropriationState.Active;
		}

		public bool IsFundsControlled
		{
			get => eca_FundsControl.GetValueOrDefault(false);
		}

		#endregion

		#region Data Properties

		public string Allotment
		{
			get => rcade_allotment;
			set => rcade_allotment = value;
		}

		public string Code
		{
			get => rcade_AppropriationName;
			set => rcade_AppropriationName = value;
		}

		public string Name
		{
			get => rcade_Description;
			set => rcade_Description = value;
		}

		public int? CRDays
		{
			get => rcade_CRDays;
			set => rcade_CRDays = value;
		}

		public decimal? CRFactor
		{
			get => rcade_CRFactor;
			set => rcade_CRFactor = value;
		}

		public bool? AllowNegativeAccountBalances
		{
			get => rcade_AllowNegativeAccountBalances;
			set => rcade_AllowNegativeAccountBalances = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion
	}
}
