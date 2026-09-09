


using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the F3S Document Catalog SP Action table
    /// </summary>
    public interface IDocumentCatalogSPactionRecord : ITableRecord
    {

		#region Interrogation Properties
		#endregion

		#region Data fields

		string CatalogName { get; set; }
		string Action { get; set; }
		string NewCatalogName { get; set; }

        #endregion

    }

}
