using HappiestProgrammer.Core.DataSources;
using HappiestProgrammer.Core.DataSources.GitHub;
using HappiestProgrammer.Core.DataSources.StackOverflow;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
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

            CommentDataLoader dataLoader = null;

            try
            {
                dataLoader = new CommentDataLoader(new ICommentRetriever[] { new StackOverflowClient(), new GitHubClient() });
            }
            catch (Exception ex)
            {
                Trace.TraceError("Fatal error, unable to create data loader: {0}", ex);
                return;
            }

            while (true)
            {
                Trace.TraceInformation("Starting data load...");

                try
                {
                    var queue = GetLoadDataQueue();
                    CloudQueueMessage retrievedMessage;

                    while ((retrievedMessage = queue.GetMessage(visibilityTimeout: TimeSpan.FromHours(6))) == null)
                    {
                        Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                    }

                    Trace.TraceInformation("Got message from queue: {0}", retrievedMessage.Id);

                    DateTime endTime;

                    try
                    {
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(retrievedMessage.AsString);

                        endTime = ((DateTime)data.Date).Date.AddDays(1);

                        Trace.TraceInformation("Parsed Date from JSON");
                    }
                    catch
                    {
                        endTime = retrievedMessage.InsertionTime.Value.UtcDateTime.Date;

                        Trace.TraceInformation("Using insertion timestamp");
                    }

                    var startTime = endTime.AddDays(-1);

                    Trace.TraceInformation("Loading data from {0} to {1}", startTime, endTime);

                    var container = GetCommentsBlobContainer();

                    dataLoader.Write(startTime, fileName => new MemoryStream(), (fileName, stream) =>
                    {
                        Trace.TraceInformation("Writing Blob: {0}", fileName);

                        var blockBlob = container.GetBlockBlobReference(fileName);

                        stream.Position = 0;
                        blockBlob.UploadFromStream(stream);
                    });

                    var calculateSentimentQueue = GetCalculateSentimentQueue();

                    calculateSentimentQueue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(new { Date = startTime })));

                    queue.DeleteMessage(retrievedMessage);
                }
                catch(Exception ex)
                {
                    Trace.TraceError("Error loading data: {0}", ex);
                    Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                }
            }
        }

        private static CloudBlobContainer GetCommentsBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference("comments");
        }

        private static CloudQueue GetLoadDataQueue()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference("loaddata");

            return queue;
        }

        private static CloudQueue GetCalculateSentimentQueue()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference("calculatesentiment");

            return queue;
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
