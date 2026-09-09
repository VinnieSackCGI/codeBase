
using Plugins_CommonLibrary.Entities.Interfaces;

namespace Portal_Plugins.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the Contact table
    /// </summary>
    public interface IContactRecord : ITableRecord
    {

        #region Interrogation Properties

        bool IsActive { get; }
        bool HasUpdatedEmail { get; }
        bool HasUpdatedName { get; }

        bool IsEmailConfirmed { get; }

        #endregion

        #region Data fields

        string Name
#if UNITTEST
        { get; set; }
#else
        { get; }
#endif
        string EmailAddress 
#if UNITTEST
        { get; set; }
#else
        { get; }
#endif
    string State
#if UNITTEST
        { get; set; }
#else
        { get; }
#endif

    #endregion
}

}
