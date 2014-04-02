using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;

namespace SentimentCalculator
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            var sentimentCalculator = new HappiestProgrammer.Core.Services.SentimentCalculator();
            sentimentCalculator.Run();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
