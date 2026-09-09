using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;


using Plugins_CommonLibrary.Interfaces;
using Plugins_CommonLibrary.Services;

using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	public partial class rcade_CARTTemplates : ITemplateRecord
	{

		private ITracingService tracer;
		private IOrganizationService orgService;

		public void Initialize( IOrganizationService orgService, ITracingService tracer )
		{
			if (orgService == null)
			{
				throw new NullReferenceException("Organization Service");
			}
			if (tracer == null)
			{
				throw new NullReferenceException("Tracing Service");
			}

			this.orgService = orgService;
			this.tracer = tracer;
		}

		#region Interrogation fields

		public bool IsActive
		{
			get => StateCode == rcade_CARTTemplatesState.Active;
		}

		#endregion

		#region Data fields

		public string Name
		{
			get => rcade_Name;
			set => rcade_Name = value;
		}

		public string TemplateType
		{
			get => this.eca_TemplateType.ToString();
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					rcade_CARTTemplates_eca_TemplateType type;
					if (Enum.TryParse<rcade_CARTTemplates_eca_TemplateType>(value, out type))
					{
						eca_TemplateType = type;
						return;
					}
				}
				eca_TemplateType = null;
			}
		}

		private IFileData fileTemplate;

		public IFileData TemplateContents
		{
			get
			{
				if (fileTemplate == null)
				{
					fileTemplate = FileContents.GetFileContents(orgService,
													"eca_filecontents", EntityLogicalName, Id);
				}
				return fileTemplate;
			}
			set
			{
				if (value == null)
				{
					eca_FileContents = null;
				}
				FileContents.LoadFileContents(orgService, tracer, value, 
										"eca_filecontents", EntityLogicalName, Id);
			}
		}

		public bool NeedsUpdate 
		{  
			get => eca_NeedsUpdate.GetValueOrDefault();
			set => eca_NeedsUpdate = value;
		}

		#endregion


		public Entity Entity
		{
			get => this;
		}

	}
}
