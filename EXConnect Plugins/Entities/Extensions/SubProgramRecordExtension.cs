using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class eca_Subprogram : ISubProgramRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_SubprogramState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => eca_Subprogramname;
			set => eca_Subprogramname = value;
		}

		public string Name
		{
			get => eca_Subprogramname;
			set => eca_Subprogramname = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion

	}
}
