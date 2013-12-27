using HappiestProgrammer.Core.DataSources;
using HappiestProgrammer.Core.Models;
using HappiestProgrammer.Core.SentimentAnalysis;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SentimentCalculator
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            Trace.TraceInformation("SentimentCalculator entry point called");

            while (true)
            {
                Trace.TraceInformation("Starting sentiment calculations...");

                try
                {
                    var queue = GetLoadDataQueue();
                    CloudQueueMessage retrievedMessage;

                    while ((retrievedMessage = queue.GetMessage(visibilityTimeout: TimeSpan.FromHours(1))) == null)
                    {
                        Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                    }

                    Trace.TraceInformation("Got message from queue: {0}", retrievedMessage.Id);

                    dynamic data = JsonConvert.DeserializeObject<dynamic>(retrievedMessage.AsString);

                    DateTime date = (DateTime)data.Date;

                    var dataLoader = new CommentDataLoader();

                    var container = GetCommentsBlobContainer();

                    var comments = Enumerable.Empty<Comment>();

                    Trace.TraceInformation("Reading in comments for {0}...", date);

                    foreach (var source in new[] { "github", "stackoverflow" })
                    {
                        comments = comments.Concat(dataLoader.Read(() => GetBlobStreams(source + date.ToString("/yyyy/MM/dd/"), container)));
                    }

                    var commentsByLanguage = comments.ToLookup(comment => comment.Language);

                    var analyzer = new SimpleWordScoreAnalysis();

                    Trace.TraceInformation("Analyzing comments...");

                    var languageScores = analyzer.GetLanguageAnalysis(commentsByLanguage);

                    var sortedScores = languageScores
                        .Select(p => new { Language = p.Key, Score = p.Value })
                        .OrderByDescending(p => p.Score);

                    Trace.TraceInformation("Inserting comments into table...");

                    var table = GetSentimentsTable();
                    table.CreateIfNotExists();

                    var tableOperation = new TableBatchOperation();

                    foreach (var score in sortedScores)
                    {
                        tableOperation.InsertOrReplace(
                            new LanguageSentiment(score.Language, date, score.Score));
                    }

                    table.ExecuteBatch(tableOperation);

                    queue.DeleteMessage(retrievedMessage);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error loading data: {0}", ex);
                    Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                }
            }
        }

        private static CloudQueue GetLoadDataQueue()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference("calculatesentiment");

            return queue;
        }

        private static CloudBlobContainer GetCommentsBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference("comments");
        }

        private static CloudTable GetSentimentsTable()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var tableClient = storageAccount.CreateCloudTableClient();

            return tableClient.GetTableReference("languagesentiments");
        }

        private static IEnumerable<Stream> GetBlobStreams(string prefix, Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container)
        {
            foreach (var blob in container.ListBlobs(prefix, useFlatBlobListing: true).Cast<CloudBlockBlob>())
            {
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    ms.Position = 0;
                    yield return ms;
                }
            }
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
