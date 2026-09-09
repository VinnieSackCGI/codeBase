
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_ObligationTypes : IObligationTypeRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_ObligationTypesState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => eca_ObligationTypeName;
			set => eca_ObligationTypeName = value;
		}

		public string Name
		{
			get => eca_ObligationTypeName;
			set => eca_ObligationTypeName = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion

	}
}
