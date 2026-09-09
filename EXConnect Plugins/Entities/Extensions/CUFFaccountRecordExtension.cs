using System;

using Microsoft.Xrm.Sdk;

using EXConnect_Plugins.Entities.Interfaces;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_CUFFAccount : ICUFFaccountRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == rcade_CUFFAccountState.Active;
		}

		public bool IsSpendingEligible
		{
			get => eca_EligibleforSpending.GetValueOrDefault(false);
		}

		public bool IsAOAeligible
		{
			get => eca_EligibleforAOA.GetValueOrDefault(false);
		}

		#endregion

		#region Data Properties

		public string Name
		{
			get => rcade_CUFFAccountName;
			set => rcade_CUFFAccountName = value;
		}

		public string Description
		{
			get => rcade_description;
			set => rcade_description = value;
		}

		public EntityReference Neighborhood
		{
			get => eca_AccountNeighborhood;
			set => eca_AccountNeighborhood = value;
		}

		public EntityReference Division
		{
			get => eca_AccountDivision;
			set => eca_AccountDivision = value;
		}

		public EntityReference Program
		{
			get => eca_AccountProgram;
			set => eca_AccountProgram = value;
		}

		public EntityReference SubProgram
		{
			get => eca_AccountSubProgram;
			set => eca_AccountSubProgram = value;
		}

		public EntityReference FunctionCode
		{
			get => eca_FunctionCode;
			set => eca_FunctionCode = value;
		}

		public EntityReference ParentAccount
		{
			get => cr15a_ParentCUFFAccount;
			set => cr15a_ParentCUFFAccount = value;
		}

		public Entity Entity
		{
			get
			{
				return this;
			}
		}

		#endregion
	}
}

