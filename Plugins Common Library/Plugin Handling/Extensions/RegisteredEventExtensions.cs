using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

using Plugins_CommonLibrary.Plugin_Handling.Interfaces;

namespace Plugins_CommonLibrary.Plugin_Handling.Extensions
{
	/// <summary>
	///     Plugin execution event
	/// </summary>
	public static class RegisteredEventExtensions
	{
		/// <summary>
		/// Extension to the IPluginExecutionContext interface
		/// </summary>
		/// <param name="context"></param>
		/// <param name="events"></param>
		/// <returns></returns>
		public static RegisteredEvent GetEvent( this IPluginExecutionContext context, IEnumerable<RegisteredEvent> events )
		{
			return events.FirstOrDefault(e => (int)e.Stage == context.Stage &&
												(e.MessageName == context.MessageName) &&
												(string.IsNullOrWhiteSpace(e.EntityLogicalName) ||
												(e.EntityLogicalName == context.PrimaryEntityName)));
		}
	}
}
