using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plugins_CommonLibrary.Services
{
	using System.Runtime.CompilerServices;
	using Entities.Adapters.Interfaces;

	public interface IWebAPIService
	{
		bool CreateRecord( string pluralEntityName, object data );
		bool DeleteRecord( string pluralEntityName, object data );
	}

	enum WebAPIRequestType
	{
		Create,
		Delete,
	}

	public class WebAPIService : IWebAPIService
	{
		private string organizationStringJSON;
		private string connectionStringJSON;
		private ServiceConfig config;
		private IEnvironmentVariablesAdapter envVarsAdapter;
		private ITracingService tracer;

		private static string _accessToken = null;

		public WebAPIService(IEnvironmentVariablesAdapter envVarsAdapter, ITracingService tracer )
		{
			if (envVarsAdapter == null)
			{
				throw new ArgumentNullException("Missing Environment Variables Adapter");
			}

			this.envVarsAdapter = envVarsAdapter;
			this.tracer = tracer;

			// Get the connection string from the Environment Variables table
			// refer to the WebApiSettings folder for the list of settings for each environment

			connectionStringJSON = envVarsAdapter.GetValue("eca_ECAServicePrincipal");

			organizationStringJSON = envVarsAdapter.GetValue("cart_EnvironmentNameGUID");

			config = new ServiceConfig(organizationStringJSON, connectionStringJSON);
		}

		public bool CreateRecord( string pluralEntityName, object data )
		{
			var accessToken = GetAccessToken();
			if (string.IsNullOrEmpty(accessToken))
			{
				tracer.Trace("Failed to obtain an Access Token from WebAPI service for Create.");
				return false;
			}

			var result = this.BuildEntity(accessToken, config.WebApiUrl, pluralEntityName, data, WebAPIRequestType.Create).GetAwaiter().GetResult();
			return (result == string.Empty);
		}

		public bool DeleteRecord( string pluralEntityName, object data )
		{
			var accessToken = GetAccessToken();
			if (string.IsNullOrEmpty(accessToken))
			{
				tracer.Trace("Failed to obtain an Access Token from WebAPI service for Delete.");
				return false;
			}

			var result = this.BuildEntity(accessToken, config.WebApiUrl, pluralEntityName, data, WebAPIRequestType.Delete).GetAwaiter().GetResult();
			return (result == string.Empty);
		}

		/// <summary>
		/// Check for the existance of an AccessToken and create one if none found
		/// </summary>
		/// <returns></returns>
		private string GetAccessToken()
		{
			if (string.IsNullOrEmpty(_accessToken))
			{
				_accessToken = this.GetTokenWithoutADAL().GetAwaiter().GetResult();
			}
			return _accessToken;
		}

		/// <summary>
		/// Gets the token for authentication
		/// </summary>
		/// <param name="loginUrl"></param>
		/// <param name="resource"></param>
		/// <param name="clientId"></param>
		/// <param name="clientSecret"></param>
		/// <returns>The token</returns>
		private async Task<string> GetTokenWithoutADAL()
		{

			HttpClient client = new HttpClient();
			var postData = $"client_id={config.ClientId}&client_secret={config.ClientSecret}&resource={config.Organization}&grant_type=client_credentials";

			var tokenUri = string.Concat(config.Authority, "/oauth2/token");
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenUri);
			request.Content = new StringContent(postData, Encoding.UTF8);
			request.Content.Headers.Remove("Content-Type");
			request.Content.Headers.TryAddWithoutValidation("Content-Type", $"application/x-www-form-urlencoded");

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			var responseMessage = await client.SendAsync(request);
			if (responseMessage.StatusCode == HttpStatusCode.OK)
			{
				var jsonResponseString = await responseMessage.Content.ReadAsStringAsync();

				var jsonContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponseString);

				return jsonContent["access_token"];
			}
			return null;
		}

		/// <summary>
		/// Returns a json string with the pdf result
		/// </summary>
		/// <param name="token"></param>
		/// <param name="webApiUrl"></param>
		/// <param name="entityTypeCode"></param>
		/// <param name="selectedRecord"></param>
		/// <param name="selectedTemplate"></param>
		/// <param name="tracingService"></param>
		/// <returns>The json string with the pdf result</returns>
		private async Task<string> BuildEntity( string token, string webApiUrl, string pluralEntityName, object data, WebAPIRequestType requestType )
		{
			var url = $"{webApiUrl}/{pluralEntityName}";

			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
			client.DefaultRequestHeaders.Add("OData-Version", "4.0");
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			HttpResponseMessage response;

			if (requestType == WebAPIRequestType.Create)
			{
				var jobject = JsonConvert.SerializeObject(data);
				var content = new StringContent(jobject, Encoding.UTF8, "application/json");
				response = await client.PostAsync(url, content);
			}
			else if (requestType == WebAPIRequestType.Delete)
			{
				url += $"({data.ToString()})";
				response = await client.DeleteAsync(url);
			}
			else
			{
				return "";
			}

			string jsonContent = await response.Content.ReadAsStringAsync();

			return jsonContent;
		}

	}

	/// <summary>
	/// Class to process to parse a connection either in Json format or ";" separated string 
	/// </summary>
	public class ServiceConfig
	{
		private readonly string organizationString = null;
		private readonly string connectionString;
		private string authority = null;
		private string organization = null;
		private string tenantId = null;
		private string clientId = null;
		private string clientSecret = null;
		private string webApiUrl = null;

		/// <summary>
		/// Constructor that parses a connection string
		/// </summary>
		/// <param name="connectionString">The connection string to instantiate the configuration</param>
		public ServiceConfig( string organizationString, string connectionString, bool jsonFormat = true )
		{
			this.organizationString = organizationString;
			this.connectionString = connectionString;

			Authority = string.Concat(baseAuthority, "state.gov");

			if (jsonFormat)
			{
				ParseJsonOrganizationString();
				ParseJsonConnectionString();
			}
			else
			{

				if (organizationString.StartsWith("https"))
				{
					Organization = organizationString;
				}
				else
				{
					Organization = string.Concat("https://", organizationString);
				}

				ParseConnectionString();
			}
		}

		private const string baseAuthority = "https://login.microsoftonline.com/";

		/// <summary>
		/// The authority to use to authorize user. 
		/// </summary>
		public string Authority
		{
			get => authority;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					authority = value;
				}
				else
				{
					throw new Exception("ServiceConfig.Authority value cannot be null.");
				}
			}
		}

		/// <summary>
		/// The Url to the CDS environment, i.e "https://yourorg.crm.dynamics.com"
		/// </summary>
		public string Organization
		{
			get => organization;
			set

			{
				if (!string.IsNullOrEmpty(value))
				{
					if ( value.StartsWith("https://"))
					{
						organization = value;
					}
					else
					{
						organization = string.Concat("https://", value);
					}
				}
				else
				{
					throw new Exception("ServiceConfig.Organization value cannot be null.");
				}
			}
		}

		/// <summary>
		/// The id of the application registered with Azure AD
		/// </summary>
		public string ClientId
		{
			get => clientId;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					clientId = value;
				}
				else
				{
					throw new Exception("ServiceConfig.ClientId value cannot be null.");
				}
			}
		}

		/// <summary>
		/// The secret of the application registered with Azure AD
		/// </summary>
		public string ClientSecret
		{
			get => clientSecret;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					clientSecret = value;
				}
				else
				{
					throw new Exception("ServiceConfig.ClientSecret value cannot be null.");
				}
			}
		}

		/// <summary>
		/// The secret of the application registered with Azure AD
		/// </summary>
		public string TenantId
		{
			get => tenantId;
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					tenantId = value;
				}
			}
		}

		/// <summary>
		/// The secret of the application registered with Azure AD
		/// </summary>
		public string WebApiUrl
		{
			get
			{
				if (string.IsNullOrEmpty(webApiUrl))
				{
					// Form it from the tenant info and Version
					return string.Concat(Organization, "/api/data/v", Version);
				}
				else
				{
					return webApiUrl;
				}
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					webApiUrl = value;
				}
			}
		}

		/// <summary>
		/// The version of the Web API to use
		/// Default is '9.1'
		/// </summary>
		public string Version { get; set; } = "9.1";

		/// <summary>
		/// The maximum number of attempts to retry a request blocked by service protection limits.
		/// Default is 3.
		/// </summary>
		public byte MaxRetries { get; set; } = 3;

		/// <summary>
		/// The amount of time to try completing a request before it will be cancelled.
		/// Default is 120 (2 minutes)
		/// </summary>
		public ushort TimeoutInSeconds { get; set; } = 120;

		private void ParseConnectionString()
		{
			// Parse the connection string and set the found parameters
			try
			{
				var keyValues = connectionString.Split(';');
				foreach (var keyValue in keyValues)
				{
					var key = keyValue.Split('=')[0];
					var value = keyValue.Split('=')[1];

					if (value.ToLower() == "null")
					{
						value = null;
					}

					// Assign all the keys found
					if (key == "Organization")
					{
						Organization = value;
					}
					else if (key == "TenantID")
					{
						Authority = string.Concat(baseAuthority, value); ;
					}
					else if (key == "ClientId")
					{
						ClientId = value;
					}
					else if (key == "ClientSecret")
					{
						ClientSecret = value;
					}
					else if (key == "Version")
					{
						Version = value;
					}
					else if (key == "MaxRetries")
					{
						if (byte.TryParse(value, out byte maxRetries))
						{
							MaxRetries = maxRetries;
						}
					}
					else if (key == "TimeoutInSeconds")
					{
						if (ushort.TryParse(value, out ushort timeoutInSeconds))
						{
							TimeoutInSeconds = timeoutInSeconds;
						}
					}
					//else if (key == "Password")
					//{
					//    if (!string.IsNullOrEmpty(value))
					//    {
					//        var ss = new SecureString();

					//        value.ToCharArray().ToList().ForEach(ss.AppendChar);
					//        ss.MakeReadOnly();

					//        Password = ss;
					//    }
					//}
				}
			}
			catch (Exception)
			{
				return;
			}
		}

		private void ParseJsonConnectionString()
		{
			// Parse the connection string and set the found parameters
			try
			{
				var jsonSettings = JObject.Parse(connectionString);
				TenantId = jsonSettings.TryGetValue("Tenant ID", out JToken jsonTenantId) ? jsonTenantId.ToString() : null;
				ClientId = jsonSettings.TryGetValue("Client ID", out JToken jsonClientID) ? jsonClientID.ToString() : null;
				ClientSecret = jsonSettings.TryGetValue("Client Secret ID", out JToken jsonClientSecret) ? jsonClientSecret.ToString() : null;
			}
			catch
			{
				return;
			}
		}

		private void ParseJsonOrganizationString()
		{
			// Parse the connection string and set the found parameters
			try
			{
				var jsonSettings = JObject.Parse(organizationString);
				Organization = jsonSettings.TryGetValue("Name", out JToken jsonOrgName) ? jsonOrgName.ToString() : null;
			}
			catch
			{
				return;
			}
		}


	}

}
