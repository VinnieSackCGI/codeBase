
using Plugins_CommonLibrary.Entities.Interfaces;

namespace EXConnect_Plugins.Entities.Interfaces
{
    public interface ICARTuserRoleRecord : ITableRecord
    {
        #region Interrogation Properties
        #endregion

        #region Data fields

        string Name { get; set; }

        #endregion
    }
}
