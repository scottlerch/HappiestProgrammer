using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;

namespace HappiestProgrammer.CombinedWorkerRoles
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            Task.WaitAll(
                Task.Run(() =>
                {
                    var dataLoader = new HappiestProgrammer.Core.Services.DataLoader();
                    dataLoader.Run();
                }),
                Task.Run(() =>
                {
                    var sentimentCalculator = new HappiestProgrammer.Core.Services.SentimentCalculator();
                    sentimentCalculator.Run();
                }));
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
