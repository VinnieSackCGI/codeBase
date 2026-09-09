using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

using Plugins_CommonLibrary.Helpers;

namespace Plugins_CommonLibrary.Extensions
{
	public static class OrganizationServiceExtensions
	{
		public static List<T> RetrieveMultiple<T>(this IOrganizationService service, QueryExpression queryExpression, bool noLock = true) where T : Entity
		{
			Guard.AgainstNull(queryExpression, nameof(queryExpression));

			queryExpression.NoLock = noLock;

			return service.RetrieveMultiple(queryExpression)?.ToEntityEnumerable<T>()?.ToList();
		}

		public static IEnumerable<Entity> RetrieveMultiple(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, bool noLock = true)
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			return service.RetrieveMultiple(queryExpression)?.Entities?.ToList();
		}

		public static List<T> RetrieveMultiple<T>(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, bool noLock = true) where T : Entity
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			return service.RetrieveMultiple(queryExpression)?.ToEntityEnumerable<T>()?.ToList();
		}

		public static IEnumerable<Entity> RetrieveMultipleByAttributeOrder(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, string orderByAttribute, OrderType? orderType, bool noLock = true)
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			if (!string.IsNullOrWhiteSpace(orderByAttribute))
			{
				OrderType order = orderType ?? OrderType.Ascending;
				queryExpression.AddOrder(orderByAttribute, order);
			}

			return service.RetrieveMultiple(queryExpression)?.Entities?.ToList();
		}

		public static List<T> RetrieveMultipleByAttributeOrder<T>(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, string orderByAttribute, OrderType? orderType, bool noLock = true) where T : Entity
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			if (!string.IsNullOrWhiteSpace(orderByAttribute))
			{
				OrderType order = orderType ?? OrderType.Ascending;
				queryExpression.AddOrder(orderByAttribute, order);
			}

			return service.RetrieveMultiple(queryExpression)?.ToEntityEnumerable<T>()?.ToList();
		}

		public static Entity Retrieve(this IOrganizationService service, EntityReference reference, ColumnSet columnSet)
		{
			Guard.AgainstNull(reference, nameof(reference));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			return service.Retrieve(reference.LogicalName, reference.Id, columnSet);
		}

		public static T Retrieve<T>(this IOrganizationService service, EntityReference reference, ColumnSet columnSet = null) where T : Entity
		{
			Guard.AgainstNull(reference, nameof(reference));

			columnSet = columnSet ?? new ColumnSet(true);

			return service.Retrieve(reference.LogicalName, reference.Id, columnSet)?.ToEntity<T>();
		}

		public static T Retrieve<T>(this IOrganizationService service, string entityName, Guid id, ColumnSet columnSet) where T : Entity
		{
			Guard.AgainstNull(columnSet, nameof(columnSet));

			return service.Retrieve(entityName, id, columnSet)?.ToEntity<T>();
		}

		public static T GetFirst<T>(this IOrganizationService service, QueryExpression queryExpression) where T : Entity
		{
			Guard.AgainstNull(queryExpression, nameof(queryExpression));

			return service.RetrieveMultiple(queryExpression.First().NoLock()).Entities.First()?.ToEntity<T>();
		}

		public static T GetFirstOrDefault<T>(this IOrganizationService service, QueryExpression queryExpression) where T : Entity
		{
			Guard.AgainstNull(queryExpression, nameof(queryExpression));

			return service.RetrieveMultiple(queryExpression.First().NoLock()).Entities.FirstOrDefault()?.ToEntity<T>();
		}

		public static T GetFirstOrDefault<T>(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, bool noLock = true) where T : Entity
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			return service.RetrieveMultiple(queryExpression)?.ToEntityEnumerable<T>()?.FirstOrDefault();
		}

		public static T GetSingle<T>(this IOrganizationService service, QueryExpression queryExpression) where T : Entity
		{
			Guard.AgainstNull(queryExpression, nameof(queryExpression));

			return service.RetrieveMultiple(queryExpression.First().NoLock()).Entities.Single()?.ToEntity<T>();
		}

		public static T GetSingleOrDefault<T>(this IOrganizationService service, QueryExpression queryExpression) where T : Entity
		{
			Guard.AgainstNull(queryExpression, nameof(queryExpression));

			return service.RetrieveMultiple(queryExpression.First().NoLock()).Entities.SingleOrDefault()?.ToEntity<T>();
		}

		public static T GetSingle<T>(this IOrganizationService service, string entityName, FilterExpression filter, ColumnSet columnSet, bool noLock = true) where T : Entity
		{
			Guard.AgainstNull(filter, nameof(filter));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var queryExpression = new QueryExpression
			{
				EntityName = entityName,
				ColumnSet = columnSet,
				Criteria = filter,
				NoLock = noLock
			};

			return service.RetrieveMultiple(queryExpression).Entities.Single()?.ToEntity<T>();
		}

		public static IEnumerable<T> GetEntitiesFromEntityReferenceCollection<T>(this IOrganizationService service, EntityReferenceCollection entityReferenceCollection, ColumnSet columnSet) where T : Entity
		{
			// todo: rewrite to account for multiple entity types in collection

			Guard.AgainstNull(entityReferenceCollection, nameof(entityReferenceCollection));
			Guard.AgainstNull(columnSet, nameof(columnSet));

			var entities = new List<T>();
			foreach (EntityReference entityReference in entityReferenceCollection)
			{
				entities.Add(service.Retrieve<T>(entityReference.LogicalName, entityReference.Id, columnSet));
			}

			return entities;
		}

		public static void Associate(this IOrganizationService service, Entity entity, string relationshipLogicalName, params Entity[] entities)
		{
			var relationship = new Relationship(relationshipLogicalName);
			if (entity.LogicalName == entities.First().LogicalName)
			{
				relationship.PrimaryEntityRole = EntityRole.Referenced;
			}

			service.Associate(entity.LogicalName, entity.Id, relationship, new EntityReferenceCollection(entities.Select(e => e.ToEntityReference()).ToList()));
		}

		public static void Associate(this IOrganizationService service, EntityReference entity, string relationshipLogicalName, params Entity[] entities)
		{
			var relationship = new Relationship(relationshipLogicalName);
			if (entity.LogicalName == entities.First().LogicalName)
			{
				relationship.PrimaryEntityRole = EntityRole.Referenced;
			}

			service.Associate(entity.LogicalName, entity.Id, relationship, new EntityReferenceCollection(entities.Select(e => e.ToEntityReference()).ToList()));
		}

		public static void Associate(this IOrganizationService service, Entity entity, string relationshipLogicalName, params EntityReference[] entities)
		{
			var relationship = new Relationship(relationshipLogicalName);
			if (entity.LogicalName == entities.First().LogicalName)
			{
				relationship.PrimaryEntityRole = EntityRole.Referenced;
			}

			service.Associate(entity.LogicalName, entity.Id, relationship, new EntityReferenceCollection(entities.ToList()));
		}

		public static void Associate(this IOrganizationService service, EntityReference entity, string relationshipLogicalName, params EntityReference[] entities)
		{
			var relationship = new Relationship(relationshipLogicalName);
			if (entity.LogicalName == entities.First().LogicalName)
			{
				relationship.PrimaryEntityRole = EntityRole.Referenced;
			}

			service.Associate(entity.LogicalName, entity.Id, relationship, new EntityReferenceCollection(entities.ToList()));
		}

		public static void Assign(this IOrganizationService service, EntityReference assignee, EntityReference assignment)
		{
			var assignRequest = new AssignRequest
			{
				Assignee = assignee,
				Target = assignment
			};
			service.Execute(assignRequest);
		}

		public static void Assign(this IOrganizationService service, EntityReference assignee, Entity assignment)
		{
			var assignRequest = new AssignRequest
			{
				Assignee = assignee,
				Target = assignment.ToEntityReference()
			};
			service.Execute(assignRequest);
		}

		public static void Assign(this IOrganizationService service, Entity assignee, EntityReference assignment)
		{
			var assignRequest = new AssignRequest
			{
				Assignee = assignee.ToEntityReference(),
				Target = assignment
			};
			service.Execute(assignRequest);
		}

		public static void Assign(this IOrganizationService service, Entity assignee, Entity assignment)
		{
			var assignRequest = new AssignRequest
			{
				Assignee = assignee.ToEntityReference(),
				Target = assignment.ToEntityReference()
			};
			service.Execute(assignRequest);
		}

		public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService service, OrganizationRequestCollection requestCollection, bool returnResponses = true, bool continueOnError = true)
		{
			Guard.AgainstNull(service, nameof(service), "A valid Organization Service Proxy must be specified.");

			// Validate the request collection.
			Guard.AgainstNull(requestCollection, nameof(requestCollection), "The collection of requests to batch process cannot be null.");

			// Ensure the user is not attempting to pass in more than 1000 requests for the batch job, as this is the maximum number CRM allows within a single batch.
			if (requestCollection.Count > 1000)
			{
				throw new ArgumentOutOfRangeException(nameof(requestCollection), "The Entity Collection cannot contain more than 1000 items, as that is the maximum number of messages that can be processed by the CRM web services in a single batch.");
			}

			try
			{
				// Instantiate a new ExecuteMultipleRequest.
				var multipleRequest = new ExecuteMultipleRequest
				{
					Settings = new ExecuteMultipleSettings
					{
						ContinueOnError = continueOnError,
						ReturnResponses = returnResponses
					},
					Requests = requestCollection
				};

				return service.Execute(multipleRequest) as ExecuteMultipleResponse;
			}
			catch (Exception ex)
			{
				throw new Exception("An error occurred while executing an ExecuteMultipleRequest. See inner exception for details.", ex);
			}
		}

		/// <summary>
		/// Adds a CreateRequest to the OrganizationRequestCollection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="requests">The requests.</param>
		/// <param name="entity">The entity.</param>
		public static void AddCreate<T>(this OrganizationRequestCollection requests, T entity) where T : Entity
		{
			requests.Add(new CreateRequest { Target = entity });
		}
/*
		public static void AddUpdate<T>(this OrganizationRequestCollection requests, T entity, IRCADEServiceContext rcadeServiceContext) where T : Entity
		{
			if (!rcadeServiceContext.GetAttachedEntities().Contains(entity))
			{
				rcadeServiceContext.Attach(entity);
			}
			rcadeServiceContext.UpdateObject(entity);
			requests.Add(new UpdateRequest { Target = entity });
		}
*/
		public static void RunWorkflow(this IOrganizationService service, Guid workflow, Entity entity) => service.Execute(new ExecuteWorkflowRequest
		{
			WorkflowId = workflow,
			EntityId = entity.Id
		});

		/*
		/// <summary>
		/// Retrieves the first Active entity (with the given subset of columns only) 
		/// where the columnNameAndValue Pairs match
		/// </summary>
		/// <param name="service"></param>
		/// <param name="logicalName"></param>
		/// <param name="columnSet">Columns to retrieve</param>
		/// <param name="columnNameAndValuePairs">List of pairs that look like this:
		/// (string name of the column, value of the column) ie. "name","John Doe" goes to entity.name = "John Doe"
		/// </param>
		/// <returns></returns>
		public static Entity GetFirstOrDefault(this IOrganizationService service, string logicalName, ColumnSet columnSet,
				params object[] columnNameAndValuePairs)
		{
			var settings = new LateBoundQuerySettings(logicalName)
			{
				Columns = columnSet,
				First = true
			};
			return service.RetrieveMultiple(settings.CreateExpression(columnNameAndValuePairs)).Entities.FirstOrDefault();
		}
		*/

		/// <summary>
		/// Gets the local time from the UTC time.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="userId">The id of the user to lookup the timezone code user settings</param>
		/// <param name="utcTime">The given UTC time to find the user's local time for.  Defaults to DateTime.UtcNow</param>
		/// <param name="defaultTimeZoneCode">Default TimeZoneCode if the user has no TimeZoneCode defined.  Defaults to EDT.</param>
		public static DateTime GetUserLocalTime(this IOrganizationService service, Guid userId, DateTime utcTime)
		{
			var timeZoneCode = RetrieveUserSettingsTimeZoneCode(service);
			if (!timeZoneCode.HasValue)
			{
				return utcTime.AddHours(-4);
			}
			var request = new LocalTimeFromUtcTimeRequest
			{
				TimeZoneCode = timeZoneCode.Value,
				UtcTime = utcTime
			};

			var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

			return (response.LocalTime).AddHours(-4);
		}

		public static DateTime RetrieveLocalTimeFromUTCTime( this IOrganizationService service, DateTime utcTime )
		{
#if UNITTEST
            return DateTime.Now;
#else
			return RetrieveLocalTimeFromUTCTime(utcTime, RetrieveUserSettingsTimeZoneCode(service), service);
#endif
		}

		public static DateTime RetrieveUTCTimeFromLocalTime( this IOrganizationService service, DateTime localTime )
		{
#if UNITTEST
            return DateTime.Now;
#else
			return RetrieveUTCTimeFromLocalTime(localTime, RetrieveUserSettingsTimeZoneCode(service), service);
#endif
		}

		internal static int? RetrieveUserSettingsTimeZoneCode( IOrganizationService service )
		{
			try
			{

				var currentUserSettings = service.RetrieveMultiple(
					new QueryExpression("usersettings")
					{
						ColumnSet = new ColumnSet("timezonecode"),
						Criteria = new FilterExpression
						{
							Conditions =
							{
								new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
							}
						}
					}).Entities[0].ToEntity<Entity>();
				return (int?)currentUserSettings.Attributes["timezonecode"];
			}
			catch
			{
				return 35;
			}
		}

		internal static DateTime RetrieveLocalTimeFromUTCTime( DateTime utcTime, int? timeZoneCode, IOrganizationService service )
		{
			if (!timeZoneCode.HasValue)
				return DateTime.Now;
			var request = new LocalTimeFromUtcTimeRequest
			{
				TimeZoneCode = timeZoneCode.Value,
				UtcTime = utcTime.ToUniversalTime()
			};
			try
			{
				var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
				return response.LocalTime;
			}
			catch
			{
				return DateTime.Now;
			}
		}

		internal static DateTime RetrieveUTCTimeFromLocalTime( DateTime localTime, int? timeZoneCode, IOrganizationService service )
		{
			if (!timeZoneCode.HasValue)
				return DateTime.Now;
			var request = new UtcTimeFromLocalTimeRequest
			{
				TimeZoneCode = timeZoneCode.Value,
				LocalTime = localTime
			};
			try
			{
				var response = (UtcTimeFromLocalTimeResponse)service.Execute(request);
				return response.UtcTime;
			}
			catch
			{
				return DateTime.Now;
			}
		}
	}
}