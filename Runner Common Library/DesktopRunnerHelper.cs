
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Xrm.Sdk;
using System.IO.Compression;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Plugins_CommonLibrary.Plugin_Handling;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;

namespace PluginsRunner_CommonLibrary
{
	public static class Runner
	{
		/// <summary>
		/// Returns the connection string for the given username and environment (SIO, DEV, or UAT).
		/// The environment URL is returned  in the envURL parameter.
		/// </summary>
		/// <param name="statePrefix"></param>
		/// <param name="environment"></param>
		/// <param name="envURL"></param>
		/// <returns type="string"></returns>
		/// <exception cref="Exception"></exception>
		public static string GetConnectionString(string statePrefix, string environment, out string envURL)
		{
			// Deterinme the environment URL for the given environment 
			string userName = string.Empty;

			switch (environment.ToUpper())
			{
				case "DEV":
					// DOS EXConnect Dev environment
					envURL = "https://orgbe4948ae.crm9.dynamics.com/";
					userName = $"{statePrefix}@state.gov";
					break;
				case "UAT":
					// DOS EXConnect Test environment
					envURL = "https://org97a22398.crm9.dynamics.com";
					userName = $"{statePrefix}@state.gov";
					break;
				case "TRN":
					// DOS EXConnect Training environment
					envURL = "https://org47460d38.crm9.dynamics.com";
					userName = $"{statePrefix}@state.gov";
					break;
				case "PRD":
					// DOS EXConnect Production environment
					envURL = "https://org28316031.crm9.dynamics.com";
					userName = $"{statePrefix}@state.gov";
					break;
				default:
					throw new Exception("Specified ENV string not resolved.");
			}

			string conn = $@"
                        Url ={envURL};
                        AuthType =OAuth;
                        UserName ={userName};
                        AppId =51f81489-12ee-4a9e-aaae-a2591f45987d;
                        RedirectUri =app://58145B91-0C36-4500-8554-080854F2AC97;
                        LoginPrompt=Auto;
                        RequireNewInstance =True";
			
			return conn;
		}

		public static string GetFormattedJsonFromText( string unPrettyJson )
		{
			return JsonConvert.SerializeObject(unPrettyJson, Formatting.Indented);
		}

		public static void ExecutePlugin<T>( Type typeOfPluginToRun, JObject objectsFromJson,
										CrmServiceClient svc, ITracingService tracer) where T : Entity
		{
			// Define the entity class that the plugin triggers on
			var targetEntity = Runner.GetTargetEntity<T>(objectsFromJson);
			var targetReference = Runner.GetTargetEntityReference(objectsFromJson);

			var relationship = Runner.GetRelationship(objectsFromJson);
			var relatedEntities = Runner.GetRelatedEntities(objectsFromJson);

			var preImageEntity = Runner.GetPreImageEntity<T>(objectsFromJson);
			var postImageEntity = Runner.GetPostImageEntity<T>(objectsFromJson);

			var request = new WhoAmIRequest();

			var response = (WhoAmIResponse)svc.Execute(new WhoAmIRequest());
			dynamic plugin = Activator.CreateInstance(typeOfPluginToRun);

			var attrs = Runner.GetAttributesFromJson(objectsFromJson);

			var context = new RunnerPluginContext(response.UserId, targetEntity, targetReference, attrs,
													relationship, relatedEntities, preImageEntity, postImageEntity);
			IOrganizationServiceFactory organizationServiceFactory = new RunnerOrganizationServiceFactory(svc);
			IServiceProvider provider = new RunnerServiceProvider(context, organizationServiceFactory, tracer);

			plugin.Execute(provider);

		}

		public static T GetTargetEntity<T>( JObject objectsFromJson ) where T : Entity
		{
			// Determine if coming in we have an entity or an object
			return GetEntityFromJson<T>(objectsFromJson, "TargetEntity");
		}

		public static EntityReference GetTargetEntityReference( JObject objectsFromJson )
		{
			return GetEntityReferenceFromJson(objectsFromJson, "TargetReference");
		}

		public static Relationship GetRelationship( JObject objectsFromJson )
		{
			return GetRelationshipFromJson(objectsFromJson);
		}
		public static EntityReferenceCollection GetRelatedEntities( JObject objectsFromJson )
		{
			return GetRelatedEntitiesFromJson(objectsFromJson);
		}

		public static T GetPreImageEntity<T>( JObject objectsFromJson ) where T : Entity
		{
			return GetEntityFromJson<T>(objectsFromJson, "PreImageEntity");
		}
		public static T GetPostImageEntity<T>( JObject objectsFromJson ) where T : Entity
		{
			return GetEntityFromJson<T>(objectsFromJson, "PostImageEntity");
		}

		public class DynamicsAttributeListValue
		{
			public string Key { get; set; }
			public dynamic Value { get; set; }
		}

		public static T GetEntityFromJson<T>( JObject objectsFromJson, string jsonTokenIdentifier ) where T : Entity
		{
			var objectsFromBlock = objectsFromJson[jsonTokenIdentifier];
			if (objectsFromBlock.HasValues)
			{
				var logicalName = objectsFromBlock["LogicalName"].ToObject<string>();
				var id = objectsFromBlock["Id"].ToObject<Guid>();
				var attributes = objectsFromBlock["Attributes"];

				if (attributes == null)
				{
					return null;
				}

				List<DynamicsAttributeListValue> attributesParsed = attributes.ToObject<List<DynamicsAttributeListValue>>();
				AttributeCollection attributeCollection = new AttributeCollection();
				PropertyInfo[] properties = typeof(T).GetProperties();

				foreach (var attribute in attributesParsed)
				{
					var property = properties.FirstOrDefault(x => string.Compare(x.Name, attribute.Key, true) == 0);
					if (property != null)
					{
						Type typeOfProperty = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

						dynamic value = null;
						if (typeOfProperty == typeof(Guid))
						{
							value = tryCastAsGuid(attribute.Value);
						}
						else if (typeOfProperty == typeof(EntityReference))
						{
							value = tryCastAsEntityReference(attribute.Value);
						}
						else if (typeOfProperty.IsEnum)
						{
							value = tryCastAsOptionSetValue(attribute.Value, typeOfProperty);
						}
						else if (typeOfProperty == typeof(Int32) && value == null)
						{
							typeOfProperty = typeof(Int32);
							if (attribute.Value != null)
							{
								value = Convert.ChangeType(attribute.Value, typeOfProperty);
							}
						}
						else if (typeOfProperty == typeof(Money))
						{
							bool parsedMoneyValue = decimal.TryParse(attribute?.Value?.Value.ToString(), out decimal moneyValue);
							if (parsedMoneyValue)
							{
								value = new Money(moneyValue);
							}
							else
							{
								value = (Money)null;
							}
						}
						else if (attribute.Value == null && typeOfProperty == typeof(DateTime))
						{
							value = (DateTime?)null;
						}
						else
						{
							value = Convert.ChangeType(attribute.Value, typeOfProperty);
						}

						attributeCollection.Add(new KeyValuePair<string, object>(attribute.Key, value));
					}
				}

				Entity entitiy = new Entity() { LogicalName = logicalName, Id = id, Attributes = attributeCollection };
				return entitiy.ToEntity<T>();
			}

			return null;
		}
		public static EntityReference GetEntityReferenceFromJson( JObject objectsFromJson, string jsonTokenIdentifier )
		{
			var objectsFromBlock = objectsFromJson[jsonTokenIdentifier];
			if (objectsFromBlock != null && objectsFromBlock.HasValues)
			{
				var logicalName = objectsFromBlock["LogicalName"].ToObject<string>();
				var id = objectsFromBlock["Id"].ToObject<Guid>();
				return new EntityReference(logicalName, id);
			}
			return null;
		}

		public static Relationship GetRelationshipFromJson( JObject objectsFromJson )
		{
			var relationship = objectsFromJson["Relationship"];
			if (relationship.HasValues)
			{
				var schemnaName = relationship["SchemaName"].ToObject<string>();
				var roleValue = relationship["PrimaryEntityRole"]?.ToObject<int?>();
				return new Relationship
				{
					SchemaName = schemnaName,
					PrimaryEntityRole = (EntityRole?)roleValue
				};
			}
			return null;
		}

		public static EntityReferenceCollection GetRelatedEntitiesFromJson( JObject objectsFromJson )
		{
			var relatedEntities = objectsFromJson["RelatedEntities"];
			if (relatedEntities.HasValues)
			{
				var entityRefCollection = new EntityReferenceCollection();
				foreach (var entity in relatedEntities)
				{
					var id = entity["Id"].ToObject<Guid>();
					var logicalName = entity["LogicalName"].ToString();
					var name = entity["Name"].ToString();
					var entityRef = new EntityReference(logicalName, id);
					entityRef.Name = name;
					entityRefCollection.Add(entityRef);
				}
				return entityRefCollection;
			}
			return null;
		}

		public static ExecutionAttributes GetAttributesFromJson( JObject objectsFromJson )
		{
			var objectsFromBlock = objectsFromJson["Attributes"];
			if (objectsFromBlock.HasValues)
			{
				var logicalName = objectsFromBlock["EntityLogicalName"].ToObject<string>();
				var userid = objectsFromBlock["InitiatingUser"].ToObject<Guid>();
				var depth = objectsFromBlock["Depth"].ToObject<int>();
				var isolationMode = objectsFromBlock["IsolationMode"].ToObject<int>(); 
				var message = objectsFromBlock["Message"].ToObject<string>(); 
				var stage = objectsFromBlock["Stage"].ToObject<int>();
				return new ExecutionAttributes
				{
					EntityLogicalName = logicalName,
					InitiatingUser = userid,
					Depth = depth,
					IsolationMode = isolationMode,
					Message = message,
					Stage = stage
				};
			}

			return null;
		}


		public static EntityReference tryCastAsEntityReference( dynamic value )
		{
			try
			{
				EntityReference reference = new EntityReference() { Id = value.Id, LogicalName = value.LogicalName.ToString(), Name = value.Name.ToString() };

				if (reference != null && reference.Id != null && reference.LogicalName != null)
				{
					return reference;
				}
			}
			catch (Exception e)
			{
				return null;
			}

			return null;
		}

		public static Guid tryCastAsGuid( dynamic value )
		{
			if (value is JObject)
			{
				var id = value.TryGetValue("Id", out JToken jsonId) ? jsonId.ToString() : null;
				return new Guid(id);
			}
			return new Guid(value);
		}

		public static OptionSetValue tryCastAsOptionSetValue( dynamic value, Type type )
		{
			try
			{
				int.TryParse(value.Value.Value.ToString(), out int optionSetValue);

				return new OptionSetValue(optionSetValue);

			}
			catch (Exception e)
			{
				return null;
			}
		}

		public static string DecompressString( string compressedText )
		{
			byte[] gZipBuffer = Convert.FromBase64String(compressedText);
			using (var memoryStream = new MemoryStream())
			{
				int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
				memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

				var buffer = new byte[dataLength];

				memoryStream.Position = 0;
				using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					gZipStream.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}

		public static string GetJsonTextFromCompressedFile()
		{
			string jsonFileText;
			using (StreamReader reader = new StreamReader(@"../../../EXConnect Debug/EXConnect TraceData.txt"))
			{
				jsonFileText = DecompressString(reader.ReadToEnd());
			}

			return jsonFileText;
		}

	}


	public class TracerService : ITracingService
    {
        StreamWriter logWriter;

        public TracerService()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            string filePath = assemblyPath.Substring(0, assemblyPath.LastIndexOf(@"\") + 1);
            filePath += "PluginTest" + DateTime.Now.Ticks.ToString() + ".log";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            logWriter = File.CreateText(filePath);
        }

        public void Trace(string message, params object[] parameters)
        {
            // Flush and Close the file if form-feed character ("\v") is received
            if (message == "\f")
            {
                logWriter.Flush();
                logWriter.Close();
                return;
            }

            // Write both to the console and to the log file
            try
            {
                Console.WriteLine(message, parameters);
                logWriter.WriteLine(message, parameters);
            }
            catch (FormatException ex)
            {
                Console.Write(message);
                logWriter.Write(message);
            }
        }
    }

}
