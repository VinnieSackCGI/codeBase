using System;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Plugin_Handling;

namespace Plugins_CommonLibrary.Extensions
{
    public static class PluginExecutionContextExtensions
	{
		public static bool HasSharedVariableWithValue(this IPluginExecutionContext context, string expectedKey, string expectedValue)
		{
			if (context.SharedVariables.ContainsKey(expectedKey) && context.SharedVariables[expectedKey].ToString() == expectedValue)
			{
				return true;
			}

			if (context.ParentContext != null)
			{			
				return context.ParentContext.HasSharedVariableWithValue(expectedKey, expectedValue);
			}

			return context.SharedVariables.ContainsKey(expectedKey) && context.SharedVariables[expectedKey].ToString() == expectedValue;
		}

		public static void SetSharedVariable(this IPluginExecutionContext context, string key, object value)
		{
			context.SharedVariables[key] = value;

			if (context.ParentContext != null)
			{
				context.ParentContext.SetSharedVariable(key, value);
			}
		}

		public static void RemoveSharedVariableWithValue(this IPluginExecutionContext context, string expectedKey, string expectedValue)
		{
			if (context.SharedVariables.ContainsKey(expectedKey) && context.SharedVariables[expectedKey].ToString() == expectedValue)
			{
				context.SharedVariables.Remove(expectedKey);
			}

			if (context.ParentContext != null)
			{
				context.ParentContext.RemoveSharedVariableWithValue(expectedKey, expectedValue);
			}
		}

		public static IPluginExecutionContext GetRootPluginContext(this IPluginExecutionContext context)
		{
			if (context.ParentContext != null)
			{
				return context.ParentContext.GetRootPluginContext();
			}

			return context;
		}

		/// <summary>
		/// Returns whether the root plugin context's message is Delete
		/// </summary>
		/// <param name="context">The current execution context.</param>
		/// <returns>True if root plugin context's message is Delete. False otherwise.</returns>
		public static bool RootPluginIsDelete(this IPluginExecutionContext context)
        {
			return context.GetRootPluginContext().MessageName == MessageNameEnum.Delete.ToString();
        } 

		public static bool ContextChainContainsMessage(this IPluginExecutionContext context, string messageName, Guid entityId)
		{
			if (context.PrimaryEntityId == entityId && context.MessageName == messageName)
			{
				return true;
			}

			if (context.ParentContext != null)
			{
				return ContextChainContainsMessage(context.ParentContext, messageName, entityId);
			}

			return false;
		}
	}
}
