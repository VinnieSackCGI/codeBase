using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Plugins_CommonLibrary.Entities.Interfaces
{
    /// <summary>
    ///  Exposes the necessary fields by the Domain layer from the Team table
    /// </summary>
    public interface ITeamRecord : ITableRecord
    {

        #region Data fields

        string Name  { get; set; }
        string EmailAddress { get; set; }
        string Type { get; set; }
        string TypeMembership { get; set; }

        #endregion
    }

}
