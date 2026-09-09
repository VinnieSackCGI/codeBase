using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Interfaces;
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
	public interface ITemplateRecord : ITableRecord 
	{
		#region Interrogation fields
		bool IsActive { get; }

		IFileData TemplateContents { get; set; }

		#endregion

		#region Data fields

		string TemplateType { get; set; }

		bool NeedsUpdate { get; set; }

		#endregion

		#region Methods

		void Initialize( IOrganizationService orgService, ITracingService tracer );

		#endregion

	}

}
