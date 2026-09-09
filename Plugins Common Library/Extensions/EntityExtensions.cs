using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using Plugins_CommonLibrary.Helpers;

namespace Plugins_CommonLibrary.Extensions
{
	public static class EntityExtensions
	{
		public static bool ContainsAndNotNull(this Entity entity, string name)
		{
			Guard.AgainstNull(name, nameof(name));

			return entity.Attributes.Contains(name) && entity.Attributes[name] != null;
		}

		public static void SyncRecordsFrom(this Entity destinationEntity, Entity sourceEntity, List<FieldMap> fieldsToSync, bool overwrite = true, bool includeNullsInOverwrite = false)
		{
			SyncRecords(destinationEntity, sourceEntity, fieldsToSync, overwrite, includeNullsInOverwrite);
		}

		public static void SyncRecordsTo(this Entity sourceEntity, Entity destinationEntity, List<FieldMap> fieldsToSync, bool overwrite = true, bool includeNullsInOverwrite = false)
		{
			SyncRecords(destinationEntity, sourceEntity, fieldsToSync, overwrite, includeNullsInOverwrite);
		}

		/// <summary>
		/// Copies values listed in fieldsToSync from a source record to the current record. 
		/// </summary>
		/// <param name="sourceEntity">Record that will be used to sync values from. If syncing two records of the same type, passing Target as the sourceEntity
		/// will copy only the changes in that record, while passing Pre/Post images will sync records completely. </param>
		/// <param name="destinationEntity">Destination entity will be used to set the returned record's key and entity name.</param>
		/// <param name="fieldsToSync">Maps source to destination fields</param>
		/// <param name="overwrite">Overwrite existing values in destination entity.</param>
		/// <param name="copyNullsOver">Copy nulls from source field to destination field.</param>
		public static void SyncRecords(Entity destinationEntity, Entity sourceEntity, List<FieldMap> fieldsToSync, bool overwrite = true, bool includeNullsInOverwrite = false)
		{
			foreach (var item in fieldsToSync)
			{
				var sourceValue = (Object)null;

				if (sourceEntity.ContainsAndNotNull(item.SourceField))
				{
					sourceValue = sourceEntity.Attributes[item.SourceField];
				}

				//if the source field has a value, or if the source field doesn't have a value, but we want to copy nulls over, then continue to copy it over 
				if (sourceValue != null || (includeNullsInOverwrite && sourceValue == null))
				{
					//if the destination record has a value, only set the value if it is being overwritten
					//if the destination contains the property but the value is null, set the value
					if ((destinationEntity.ContainsAndNotNull(item.TargetField) && overwrite) || destinationEntity.Contains(item.TargetField) && destinationEntity.Attributes[item.TargetField] == null)
					{
						destinationEntity.Attributes[item.TargetField] = sourceValue;
					}
					else if (!destinationEntity.Contains(item.TargetField)) //if the property is not apart of the collection, add the value.
					{
						destinationEntity.Attributes.Add(item.TargetField, sourceValue);
					}
				}
			}
		}

		public static void SetStateOrProvince(this Entity targetEntity, Entity preImageEntity, string stateAttr, string internationalAttr, string stateOrProvinceAttr, IOrganizationService organizationService)
		{
			if (!targetEntity.Contains(stateAttr)) return;
			if (targetEntity.GetBoolean(internationalAttr) == true) return;
			if (preImageEntity != null && !targetEntity.Contains(internationalAttr) && preImageEntity.GetBoolean(internationalAttr) == true) return;

			targetEntity.SetString(stateOrProvinceAttr, GetStateString(stateAttr, targetEntity, organizationService));
		}

		private static string GetStateString(string stateAttribute, Entity entity, IOrganizationService organizationService)
		{
			var stateString = "";
			if (entity.ContainsAndNotNull(stateAttribute))
			{
				stateString = organizationService.Retrieve(entity.GetEntityReference(stateAttribute), new ColumnSet("edms_abbreviation")).GetString("edms_abbreviation");
			}

			return stateString;
		}

		public static void EnsureAttributes(this Entity targetEntity, IOrganizationService organizationService, string[] attributeNames)
		{
			Guard.AgainstNull(organizationService, nameof(organizationService));

			attributeNames = attributeNames ?? new string[0];
			attributeNames = attributeNames.Where(attribute => attribute != null).ToArray();
			if(attributeNames.Length.Equals(0))
			{
				return;
			}

			var missingAttributes = attributeNames.Where(attribute => !targetEntity.Attributes.ContainsKey(attribute)).ToArray();
			var entityWithMissingAttritbutes =  organizationService.Retrieve(targetEntity.ToEntityReference(), new ColumnSet(missingAttributes));

			foreach(var missingAttribute in missingAttributes)
			{
				if(!entityWithMissingAttritbutes.Contains(missingAttribute))
				{
					throw new ArgumentException($"Attribute '{missingAttribute}' could not be found for the entity.");
				}

				targetEntity.Attributes.Add(new KeyValuePair<string, object>(missingAttribute, entityWithMissingAttritbutes[missingAttribute]));
			}			
		}

		#region Get Methods

		public static T GetValue<T>(this Entity entity, string attributeLogicalName)
		{
			if (!entity.Contains(attributeLogicalName))
			{
				return default(T);
			}

			var result = entity.Attributes[attributeLogicalName];
			if (result is AliasedValue)
			{
				var aliasedValue = (AliasedValue)result;
				return (T)aliasedValue.Value;
			}

			return entity.GetAttributeValue<T>(attributeLogicalName);
		}

		public static EntityReference GetEntityReference(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<EntityReference>(attributeLogicalName);
		}

		public static EntityCollection GetEntityCollection(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<EntityCollection>(attributeLogicalName);
		}

		public static OptionSetValue GetOptionSetValue(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<OptionSetValue>(attributeLogicalName);
		}

		public static BooleanManagedProperty GetBooleanManagedProperty(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<BooleanManagedProperty>(attributeLogicalName);
		}

		public static Money GetMoney(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<Money>(attributeLogicalName);
		}

		public static string GetString(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<string>(attributeLogicalName);
		}

		public static Guid? GetGuid(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<Guid?>(attributeLogicalName);
		}

		public static DateTime? GetDateTime(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<DateTime?>(attributeLogicalName);
		}

		public static int? GetInteger(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<int?>(attributeLogicalName);
		}

		public static bool? GetBoolean(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<bool?>(attributeLogicalName);
		}

		public static long? GetLong(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<long?>(attributeLogicalName);
		}

		public static double? GetDouble(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<double?>(attributeLogicalName);
		}

		public static decimal? GetDecimal(this Entity entity, string attributeLogicalName)
		{
			return entity.GetValue<decimal?>(attributeLogicalName);
		}

		public static EntityReference GetOwner(this Entity entity)
		{
			return entity.GetValue<EntityReference>("ownerid");
		}

		public static byte[] GetImage(this Entity entity)
		{
			return entity.GetValue<byte[]>("entityimage");
		}

		#region With Default Option

		public static T GetValue<T>(this Entity entity, string attributeLogicalName, T defaultValue)
		{
			if (!entity.Contains(attributeLogicalName))
			{
				return default(T);
			}

			T returnValue;
			var result = entity.Attributes[attributeLogicalName];
			if (result is AliasedValue)
			{
				var aliasedValue = (AliasedValue)result;
				returnValue = (T)aliasedValue.Value;
				return returnValue != null ? returnValue : defaultValue;
			}

			var value = entity.GetAttributeValue<T>(attributeLogicalName);

			return value != null ? value : defaultValue;
		}

		public static EntityReference GetEntityReference(this Entity entity, string attributeLogicalName, EntityReference defaultValue)
		{
			return GetValue<EntityReference>(entity, attributeLogicalName, defaultValue);
		}

		public static EntityCollection GetEntityCollection(this Entity entity, string attributeLogicalName, EntityCollection defaultValue)
		{
			return GetValue<EntityCollection>(entity, attributeLogicalName, defaultValue);
		}

		public static OptionSetValue GetOptionSetValue(this Entity entity, string attributeLogicalName, OptionSetValue defaultValue)
		{
			return GetValue<OptionSetValue>(entity, attributeLogicalName, defaultValue);
		}

		public static BooleanManagedProperty GetBooleanManagedProperty(this Entity entity, string attributeLogicalName, BooleanManagedProperty defaultValue)
		{
			return GetValue<BooleanManagedProperty>(entity, attributeLogicalName, defaultValue);
		}

		public static Money GetMoney(this Entity entity, string attributeLogicalName, Money defaultValue)
		{
			return GetValue<Money>(entity, attributeLogicalName, defaultValue);
		}

		public static string GetString(this Entity entity, string attributeLogicalName, string defaultValue)
		{
			return GetValue<string>(entity, attributeLogicalName, defaultValue);
		}

		public static Guid? GetGuid(this Entity entity, string attributeLogicalName, Guid defaultValue)
		{
			return GetValue<Guid?>(entity, attributeLogicalName, defaultValue);
		}

		public static DateTime? GetDateTime(this Entity entity, string attributeLogicalName, DateTime? defaultValue)
		{
			return GetValue<DateTime?>(entity, attributeLogicalName, defaultValue);
		}

		public static int? GetInteger(this Entity entity, string attributeLogicalName, int? defaultValue)
		{
			return GetValue<int?>(entity, attributeLogicalName, defaultValue);
		}

		public static bool? GetBoolean(this Entity entity, string attributeLogicalName, bool? defaultValue)
		{
			return GetValue<bool?>(entity, attributeLogicalName, defaultValue);
		}

		public static long? GetLong(this Entity entity, string attributeLogicalName, long? defaultValue)
		{
			return GetValue<long?>(entity, attributeLogicalName, defaultValue);
		}

		public static double? GetDouble(this Entity entity, string attributeLogicalName, double? defaultValue)
		{
			return GetValue<double?>(entity, attributeLogicalName, defaultValue);
		}

		public static decimal? GetDecimal(this Entity entity, string attributeLogicalName, decimal? defaultValue)
		{
			return GetValue<decimal?>(entity, attributeLogicalName, defaultValue);
		}

		#endregion

		#endregion

		#region Set Methods
		public static void SetValue(this Entity entity, string attributeLogicalName, object value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetEntityReference(this Entity entity, string attributeLogicalName, EntityReference value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetEntityCollection(this Entity entity, string attributeLogicalName, EntityCollection value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetOptionSetValue(this Entity entity, string attributeLogicalName, OptionSetValue value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetBooleanManagedProperty(this Entity entity, string attributeLogicalName, BooleanManagedProperty value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetMoney(this Entity entity, string attributeLogicalName, Money value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetString(this Entity entity, string attributeLogicalName, string value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetGuid(this Entity entity, string attributeLogicalName, Guid? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetDateTime(this Entity entity, string attributeLogicalName, DateTime? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetInteger(this Entity entity, string attributeLogicalName, int? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetBoolean(this Entity entity, string attributeLogicalName, bool? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetLong(this Entity entity, string attributeLogicalName, long? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetDouble(this Entity entity, string attributeLogicalName, double? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetDecimal(this Entity entity, string attributeLogicalName, decimal? value)
		{
			entity[attributeLogicalName] = value;
		}

		public static void SetOwner(this Entity entity, EntityReference value)
		{
			entity["ownerid"] = value;
		}

		public static void SetImage(this Entity entity, byte[] value)
		{
			entity["entityimage"] = value;
		}

		public static void SetImageToContactImage(this Entity entity, EntityReference contact, IOrganizationService orgService)
		{
			entity["entityimage"] = contact != null ? orgService.Retrieve(contact.LogicalName, contact.Id, new ColumnSet("entityimage")).GetImage() : null;
		}

		#endregion
	}
}