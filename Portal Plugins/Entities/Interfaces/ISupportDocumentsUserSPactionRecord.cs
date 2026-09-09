
using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the F3S Support Documents User SP Action table
    /// </summary>
    public interface ISupportDocumentsUserSPactionRecord : ITableRecord
    {

        #region Interrogation Properties
        #endregion

        #region Data fields

        string EmailAddress { get; set; }
		string Action { get; set; }
		string PermissionLevel { get; set; }

        #endregion

    }

}
