using HappiestProgrammer.Core.DataSources;
using HappiestProgrammer.Core.DataSources.GitHub;
using HappiestProgrammer.Core.DataSources.Mock;
using HappiestProgrammer.Core.DataSources.StackOverflow;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace HappiestProgrammer.DataLoader
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            Trace.TraceInformation("DataLoader entry point called", "Information");

            while (true)
            {
                Trace.TraceInformation("Working", "Information");

                try
                {
                    var dataLoader = new CommentDataLoader(new ICommentRetriever[] { new MockClient() });

                    var startTime = new DateTime(2013, 8, 31, 0, 0, 0, DateTimeKind.Utc);
                    var endTime = startTime.AddDays(1);

                    var container = GetCommentsBlobContainer();

                    var date = new DateTime(2013, 8, 31, 0, 0, 0, DateTimeKind.Utc);

                    dataLoader.Write(date, fileName => new MemoryStream(), (fileName, stream) =>
                    {
                        Trace.TraceInformation("Writing Blob: {0}", fileName);

                        var blockBlob = container.GetBlockBlobReference(fileName);

                        stream.Position = 0;
                        blockBlob.UploadFromStream(stream);
                    });
                }
                catch(Exception ex)
                {
                    Trace.TraceError("Error loading data: {0}", ex);
                }

                Thread.Sleep(60000 * 5);
            }
        }

        private static CloudBlobContainer GetCommentsBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference("comments");
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
