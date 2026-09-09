using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Plugins_CommonLibrary.Extensions
{
	public static class EntityCollectionExtensions
	{
		public static IEnumerable<T> ToEntityEnumerable<T>(this EntityCollection col) where T : Entity
		{
			if (typeof(T) == typeof(Entity))
			{
				// T is Entity.  No need to cast, just convert.
				return (List<T>) (object) col?.Entities?.ToList();
			}

			return col?.Entities?.Select(e => e.ToEntity<T>());
		}
	}
}