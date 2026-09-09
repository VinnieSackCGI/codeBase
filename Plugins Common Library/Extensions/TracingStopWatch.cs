using Microsoft.Xrm.Sdk;
using System.Diagnostics;

namespace Plugins_CommonLibrary.Extensions
{
    public class TracingStopWatch : Stopwatch
    {
        private ITracingService tracingService;
        private int checkpointCounter;
        public TracingStopWatch(ITracingService tracingService)
        {
            this.tracingService = tracingService;
            this.checkpointCounter = 0;
        }



        public void TraceCheckPoint(string message = null)
        {
            this.Stop();
            if (message == null)
            {
                this.tracingService.Trace($"Checkpoint {checkpointCounter}: time is {this.ElapsedMilliseconds}");
            }
            else
            {
                this.tracingService.Trace($"Checkpoint {checkpointCounter}: time is {this.ElapsedMilliseconds}. message: {message}");
            }
            this.checkpointCounter++;
            this.Start();
        }



        public new void Reset()
        {
            this.checkpointCounter = 0;
            base.Reset();
        }
    }
}
