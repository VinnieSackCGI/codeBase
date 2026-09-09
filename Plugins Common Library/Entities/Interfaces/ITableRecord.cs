using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Plugins_CommonLibrary.Entities.Interfaces
{
    /// </summary>
    public interface ITableRecord
    {
        Guid Id { get; set; }

        Entity Entity { get; }

        DateTime? ModifiedOn { get; set; }
        DateTime? CreatedOn { get; set; }

        EntityReference ModifiedBy { get; set; }
        EntityReference CreatedBy { get; set; }
    }

    public interface IKeyTableRecord : ITableRecord
    {
        string Code { get; set; }
        string Name { get; set; }
    }

}
