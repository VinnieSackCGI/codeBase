using System;
using System.Activities;

using Microsoft.Xrm.Sdk.Workflow;

using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Plugin_Handling;

using EXConnect_Plugins.Plugin_Handling;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins
{
    [CrmPluginRegistration("GetCurrentFiscalDateInfoCodeActivity",
    "Get Current Fiscal Date Info",
    "Gets the Current Fiscal Year, Month, and Day",
    "EXConnect_CodeActivites",
    IsolationModeEnum.Sandbox)]

    public class GetCurrentFiscalDateInfoCodeActivity : MainCodeActivity
    {
        [Output("Fiscal Year")]
        public OutArgument<int> FiscalYear { get; set; }

        [Output("Fiscal Month")]
        public OutArgument<int> FiscalMonth { get; set; }

        [Output("Fiscal Day")]
        public OutArgument<int> FiscalDay { get; set; }

#if DESKTOP
        public GetCurrentFiscalDateInfoCodeActivity(IRepository dbService = null, ITracingService tracer = null)
        {
            if (dbService != null)
            {
                this.dbService = dbService;
            }
            if (tracer != null)
            {
                this.tracer = tracer;
            }
        }
#endif

        protected override void Execute(CodeActivityContext context)
        {
            InitializeContext(context);

            tracer.Trace("Processing the Code Activity");

            var today = DateTime.Now;

            tracer.Trace($"Computing Fiscal information based on Now : {today.ToString("dd-MMM-yyyy HH:mm:ss")}");

            FiscalYear.Set(context, today.FiscalYear());
            FiscalMonth.Set(context, today.MonthOfFiscalYear());
            FiscalDay.Set(context, today.DayOfFiscalYear());

        }
    }
}
