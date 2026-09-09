using System;

namespace Plugins_CommonLibrary.Plugin_Handling
{
    using Interfaces;

    public class RegisteredEvent
    {
        public RegisteredEvent(StageEnum stage, string messageName, string entityLogicalName, Action<IExtendedPluginContext> action = null)
        {
            this.Stage = stage;
            this.MessageName = messageName;
            this.EntityLogicalName = entityLogicalName;
            this.Execute = action; 
        }

        public StageEnum Stage { get; }
        public string MessageName { get; }
        public string EntityLogicalName { get; }
        public Action<IExtendedPluginContext> Execute { get; }
    }
}
