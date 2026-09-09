
using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the F3S Web Role table
    /// </summary>
    public interface ISupportDocumentsRecord : ITableRecord
    {

		#region Interrogation Properties

		bool HasUpdatedName { get; }

		#endregion

		#region Data fields

		string CatalogName  { get; set; }

        #endregion

    }

}
