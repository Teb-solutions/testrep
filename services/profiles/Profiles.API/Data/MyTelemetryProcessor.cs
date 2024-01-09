using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Data
{
    public class MyTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public MyTelemetryProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        public void Process(ITelemetry telemetry)
        {

            TraceTelemetry trace = telemetry as TraceTelemetry;

            if (trace != null && trace.Properties.Keys.Contains("CategoryName"))
            {
                //Here I just filter out 2 kinds of trace messages, you can adjust your code as per your need.
                if (trace.Properties["CategoryName"] == "Microsoft.AspNetCore.Hosting.Internal.WebHost" || trace.Context.Properties["CategoryName"] == "Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker")
                {
                    //return means abandon this trace message which has the specified CategoryName
                    return;
                }
            }

            if (trace == null)
            {
                this.Next.Process(telemetry);

            }

            if (trace != null)
            {
                this.Next.Process(trace);
            }
        }
    }
}
