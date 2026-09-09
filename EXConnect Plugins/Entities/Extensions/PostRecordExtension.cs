
using System;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_Post : IPostRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == rcade_PostState.Active;
		}

		#endregion

		#region Data Properties

		public string Code
		{
			get => rcade_Post1;
			set => rcade_Post1 = value;
		}

		public string Name
		{
			get => rcade_postname;
			set => rcade_postname = value;
		}

		public string Region
		{
			get => rcade_Region;
			set => rcade_Region = value;
		}

		public Entity Entity
		{
			get => this;
		}

		#endregion

	}
}