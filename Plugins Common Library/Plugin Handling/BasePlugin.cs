using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel;

using Newtonsoft.Json;

using Microsoft.Xrm.Sdk;

using Plugins_CommonLibrary.Entities.Interfaces;
using Plugins_CommonLibrary.Exceptions;
using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Helpers;
using Plugins_CommonLibrary.Plugin_Handling.Interfaces;
using Microsoft.Xrm.Sdk.Workflow;
using System.Web.UI.WebControls;

namespace Plugins_CommonLibrary.Plugin_Handling
{
    public abstract class BasePlugin : IPluginHandler
	{
        protected IBaseRepository baseDbService;
        protected IExtendedPluginContext context;
        protected ITracingService tracer;
        protected DateTime dateTimeLocal;

        protected BasePlugin()
        {
            this.RegisteredEvents = new List<RegisteredEvent>();
            foreach (CrmPluginRegistrationAttribute attribute in this.GetType().GetCustomAttributes(true))
                RegisteredEvents.Add(new RegisteredEvent(attribute.Stage.GetValueOrDefault(), attribute.Message,
                                                    attribute.EntityLogicalName));
        }

		public List<RegisteredEvent> RegisteredEvents { get; }

		public bool ShouldDoVerboseTracing { get; private set; }

		protected void InitializeContext(IExtendedPluginContext executeContext, bool asUser = false)
        {
            context = executeContext;
            tracer = executeContext.TracingService;

            var now = DateTime.Now;
            dateTimeLocal = context.SystemOrganizationService.RetrieveLocalTimeFromUTCTime(now);

            // =================================================
            // Composition Root
            // -------------------------------------------------
            // Initialize the context required for the Plug-in
            //  .. Database service which created at the BusinessDbService
            baseDbService = GetDbService( (asUser ? context.UserOrganizationService : context.SystemOrganizationService) );

            baseDbService.LocalDateTime = dateTimeLocal;
            baseDbService.UserId = (asUser ? context.ExecutionContext.InitiatingUserId : context.ExecutionContext.UserId);

            tracer.Trace($"Initiating User Id = {context.ExecutionContext.InitiatingUserId}, User Id = {context.ExecutionContext.UserId}");
        }

		public void ResetUserContext(Guid userId )
		{
			context.ResetUserOrganizationService(userId);
			baseDbService = GetDbService(context.UserOrganizationService);
			baseDbService.UserId = userId;
			tracer.Trace($"Executing User Id reset to \"{baseDbService.UserId}\".");
		}

		///     Main entry point for he business logic that the plug-in is to execute.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		/// <remarks>
		///     For improved performance, Microsoft Dynamics CRM caches plug-in instances.
		///     The plug-in's Execute method should be written to be stateless as the constructor
		///     is not called for every invocation of the plug-in. Also, multiple system threads
		///     could execute the plug-in at the same time. All per invocation state information
		///     is stored in the context. This means that you should not use global variables in plug-ins.
		/// </remarks>
		public void Execute( IServiceProvider serviceProvider )
		{
			Guard.AgainstNull(serviceProvider, nameof(serviceProvider));

			// Construct the local plug-in context.
			IExtendedPluginContext context = this.CreatePluginContext(serviceProvider);

			this.TraceEntities(context);

			try
			{
#if !DEBUG
                // Verify plug-in is running for a registered event
                if (context.Event == null)
                {
                    context.Trace($"No Registered Event Found for Event: {context.MessageName}, Entity: {context.PrimaryEntityName}, and Stage: {context.Stage}!");
                    return;
                }
#endif
				//     context.Trace($"Executing Registered Event Found for Event: {context.MessageName}, Entity: {context.PrimaryEntityName}, and Stage: {context.Stage}");

				// Invoke the custom implementation
				Action<IExtendedPluginContext> execute = context.Event?.Execute == null ? this.ExecutePlugin : new Action<IExtendedPluginContext>(c => context.Event.Execute(c));

				var start = DateTime.UtcNow;
				this.ShouldDoVerboseTracing = true; // this.GetVerboseTracingSetting(context);

#if !DEBUG
				this.TraceEntities(context);
#endif

				execute(context);

				//var end = DateTime.UtcNow;
				//      context.Trace($"Plugin {context.PluginTypeName} ended execution at {end} with a total run time of {(end - start).TotalSeconds} seconds.");
			}
			catch (EntityModifiedOrCreatedByOtherResourceSinceLastRetrievalException e)
			{
				context.Trace($"Restarting plugin for entity type {context.GetTargetEntity().LogicalName} with id of {context.GetTargetEntity()?.Id} ");
				this.Execute(serviceProvider);
			}
			catch (DocumentNumberExistsException e)
			{
				context.Trace($"Document number exists : {e.DocumentNumber}");
				throw e;
			}
			catch (FaultException<OrganizationServiceFault> e)
			{
				context.Trace($"\nOrg Service Fault Exception:\n\n{e.ToString()}\n");

				throw;
			}
			catch (FaultException e)
			{
				context.Trace($"\nFault Exception:\n\n{e.ToString()}\n");

				throw;
			}
			catch (GeneralException e)
			{
				context.Trace($"EXConnect Exception:\n\n{e.ToString()}\n");

				throw new InvalidPluginExecutionException(e.Message);
			}
			catch (InvalidPluginExecutionException e)
			{
				throw;
			}
			catch (Exception e)
			{
				context.Trace($"\n{e.ToString()}\n");
				context.Trace($"\n{e?.StackTrace?.ToString()}\n");
				throw new InvalidPluginExecutionException("EXConnect was unable to process this request");
			}
			finally
			{
				context.Trace($"Exiting {context.PluginTypeName}.Execute()");
				this.TraceEntities(context);
			}
		}

		protected  IExtendedPluginContext CreatePluginContext(IServiceProvider serviceProvider)
        {
            return new PluginContext(serviceProvider, this);
        }

		private void TraceEntities( IExtendedPluginContext context )
		{
			if (this.ShouldDoVerboseTracing)
			{
				var attributes = new ExecutionAttributes
				{
					Stage = context?.Stage,
					Depth = (int)context?.Depth,
					IsolationMode = (int) context?.IsolationMode,
					Message = context?.MessageName,
					InitiatingUser = (Guid)(context?.InitiatingUserId),
					EntityLogicalName =(context?.PrimaryEntity != null 
										? context?.PrimaryEntity.LogicalName 
										: context?.PrimaryEntityName),
					EntityId = (context?.PrimaryEntity != null 
										? context?.PrimaryEntity.Id 
										: context?.PrimaryEntityId).GetValueOrDefault()
				};

				var traceEntity = new EntityTrace(context?.GetTargetEntity(), context?.GetTargetEntityReference(),
								context?.GetRelationship(), context?.GetRelatedEntities(),
								context?.PreEntityImages?.FirstOrDefault().Value,
								context?.PostEntityImages?.FirstOrDefault().Value, attributes);
				context.Trace("json:" + this.compressString(JsonConvert.SerializeObject(traceEntity)));
			}
		}

		internal string compressString( string text )
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			var memoryStream = new MemoryStream();
			using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
			{
				gZipStream.Write(buffer, 0, buffer.Length);
			}

			memoryStream.Position = 0;

			var compressedData = new byte[memoryStream.Length];
			memoryStream.Read(compressedData, 0, compressedData.Length);

			var gZipBuffer = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
			return Convert.ToBase64String(gZipBuffer);
		}

		protected abstract IBaseRepository GetDbService(IOrganizationService corgService);

        protected abstract void ExecutePlugin(IExtendedPluginContext executeContext);
    }
}
