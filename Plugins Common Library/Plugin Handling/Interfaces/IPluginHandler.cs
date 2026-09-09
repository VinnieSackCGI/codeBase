
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

using Plugins_CommonLibrary.Plugin_Handling.Extensions;

namespace Plugins_CommonLibrary.Plugin_Handling.Interfaces
{
	public interface IPluginHandler : IPlugin
    {
        List<RegisteredEvent> RegisteredEvents { get; }

    }
}
