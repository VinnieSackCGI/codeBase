using System;
using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Extensions;

namespace Plugins_CommonLibrary.Helpers
{
	public static class Guard
    {
        public static void AgainstNull(object argument)
        {

        }

        public static void AgainstNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void AgainstNull(object argument, string argumentName, string message)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, message);
            }
        }

        public static void AgainstEmptyOrWhitespace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException($"Parameter '{argumentName}' cannot be empty or whitespace", argumentName);
            }
        }
                   
        public static string AgainstOverflow(string argument, string argumentName, int limit, bool throwException)
		{
			Guard.AgainstNull(argument, nameof(argument));
			Guard.AgainstNull(argumentName, nameof(argumentName));
			Guard.AgainstEmptyOrWhitespace(argumentName, nameof(argumentName));

			if(argument.Length > limit)
			{
				if(throwException)
				{
					throw new ArgumentException($"Parameter '{argumentName}' cannot be longer than {limit} characters.", argumentName);
				}

				return argument.Substring(0, limit);
			}

			return argument;
		}

		public static void EntityContainsRequiresAttribute(Entity entity, string field)
		{
			Guard.AgainstNull(entity, nameof(entity));
			Guard.AgainstNull(field, nameof(field));
			Guard.AgainstEmptyOrWhitespace(field, nameof(field));

			if (!entity.Contains(field))
			{
				throw new ArgumentException($"Entity does not have required field '{field}'.");
			}

			if (!entity.ContainsAndNotNull(field))
			{
				throw new ArgumentException($"Required field '{field}' on Entity is null");
			}
		}

		public static void AgainstNullOrEmptyEntityReference(EntityReference reference, string objectName = null)
		{
			Guard.AgainstNull(reference, string.IsNullOrWhiteSpace(objectName) ? nameof(reference) : objectName);
			Guard.AgainstNull(reference.LogicalName, "logicalName");
			Guard.AgainstEmptyOrWhitespace(reference.LogicalName, "logicalName");

			if (reference.Id.Equals(default(Guid)))
			{
				throw new ArgumentException("The default GUID is not a valid Id", "id");
			}
		}

		public static void EntityReferenceIsRightType(EntityReference reference, string logicalName)
		{
			Guard.AgainstNullOrEmptyEntityReference(reference);

			Guard.AgainstNull(logicalName, nameof(logicalName));
			Guard.AgainstEmptyOrWhitespace(logicalName, nameof(logicalName));

			if (!reference.LogicalName.Equals(logicalName))
			{
				throw new ArgumentException($"EntityReference references type '{reference.LogicalName}' when expected type was '{logicalName}'");
			}
		}
	}
}
