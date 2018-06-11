using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace ODataExamples.API.Classes{
    public interface ITelemetryTracker{
        void TrackException(Exception ex);
        void TrackTrace(string message, SeverityLevel severity);
        void TrackEvent(string eventName, Dictionary<string, string> properties);
        Stopwatch StartTrackRequest(string requestName);
        void StopTrackRequest(string requestName, Stopwatch stopwatch);
    }

    public class TelemetryTracker : ITelemetryTracker{
        private readonly TelemetryClient _telemetry = new TelemetryClient();

        public void TrackException(Exception ex){
            _telemetry.TrackException(ex);
        }

        public void TrackTrace(string message, SeverityLevel severity){
            _telemetry.TrackTrace(message, severity);
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties){
            _telemetry.TrackEvent(eventName, properties);
        }

        public Stopwatch StartTrackRequest(string requestName){
            _telemetry.Context.Operation.Id = Guid.NewGuid().ToString();
            return Stopwatch.StartNew();
        }

        public void StopTrackRequest(string requestName, Stopwatch stopwatch){
            stopwatch.Stop();
            _telemetry.TrackRequest(requestName, DateTime.Now, stopwatch.Elapsed, "200", true);
        }
    }

    public class ValidationActionFilter : ActionFilterAttribute{
        public override void OnActionExecuting(HttpActionContext actionContext){
            var tracker = new TelemetryTracker();
            var modelState = actionContext.ModelState;
            if (modelState.IsValid) return;

            var ex = new Exception(modelState.ToString());
            tracker.TrackException(ex);
        }
    }
}