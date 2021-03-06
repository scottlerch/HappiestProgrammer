using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;

namespace HappiestProgrammer.DataLoader
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            var dataLoader = new HappiestProgrammer.Core.Services.DataLoader();
            dataLoader.Run();
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
