namespace HappiestProgrammer.Runner
{
    using HappiestProgrammer.Core.DataSources;
    using HappiestProgrammer.Core.DataSources.GitHub;
    using HappiestProgrammer.Core.DataSources.Mock;
    using HappiestProgrammer.Core.DataSources.StackOverflow;
    using HappiestProgrammer.Core.SentimentAnalysis;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private static IEnumerable<ICommentRetriever> DefaultCommentRetrievers = new ICommentRetriever[] { new StackOverflowClient(), new GitHubClient() };
        private static IEnumerable<ICommentRetriever> MockCommentRetrievers = new ICommentRetriever[] { new MockClient() };

        internal static void Main()
        {
            Trace.TraceInformation("Starting application...");

            //LoadFromMockDataSource();
            //CalculateSentimentFromDisk();

            LoadFromDataSourceIntoAzure();
            CalculateSentimentFromAzure();

            Trace.TraceInformation("Complete!");

            Console.ReadKey();
        }

        private static void LoadFromDataSourceIntoAzure()
        {
            var dataLoader = new CommentDataLoader(new ICommentRetriever[] { new GitHubClient() });

            var startTime = new DateTime(2013, 8, 31, 0, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddDays(1);

            var container = GetCommentsBlobContainer();

            var date = new DateTime(2013, 8, 31, 0, 0, 0, DateTimeKind.Utc);

            dataLoader.Write(date, fileName => new MemoryStream(), (fileName, stream) =>
                {
                    var blockBlob = container.GetBlockBlobReference(fileName);

                    stream.Position = 0;
                    blockBlob.UploadFromStream(stream);
                });
        }

        private static void CalculateSentimentFromAzure()
        {
            var dataLoader = new CommentDataLoader(MockCommentRetrievers);

            var container = GetCommentsBlobContainer();

            var commentsByLanguage = dataLoader.Read(() => GetBlobStreams(container))
                .ToLookup(comment => comment.Language);

            var analyzer = new SimpleWordScoreAnalysis();

            var languageScores = analyzer.GetLanguageAnalysis(commentsByLanguage);

            var sortedScores = languageScores
                .Select(p => new { Language = p.Key, Score = p.Value })
                .OrderByDescending(p => p.Score);

            foreach (var languageScore in sortedScores)
            {
                Trace.TraceInformation("{0,-15}{1}", languageScore.Language, languageScore.Score);
            }
        }

        private static CloudBlobContainer GetCommentsBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference("comments");
        }

        private static IEnumerable<Stream> GetBlobStreams(Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container)
        {
            foreach (var blob in container.ListBlobs(prefix: null, useFlatBlobListing: true).Cast<CloudBlockBlob>())
            {
                using (var ms = new MemoryStream())
                {
                    blob.DownloadToStream(ms);
                    ms.Position = 0;
                    yield return ms;
                }
            }
        }

        private static void LoadFromMockDataSource()
        {
            var dataLoader = new CommentDataLoader(MockCommentRetrievers);

            var date = new DateTime(2013, 8, 31, 0, 0, 0, DateTimeKind.Utc);

            dataLoader.Write(date, fileName =>
                {
                    string fileDirectory = Path.Combine("MockData", Path.GetDirectoryName(fileName));
                    Directory.CreateDirectory(fileDirectory);
                    return File.OpenWrite(Path.Combine(Environment.CurrentDirectory, fileDirectory, Path.GetFileName(fileName)));
                });
        }

        private static void CalculateSentimentFromDisk()
        {
            var dataLoader = new CommentDataLoader(MockCommentRetrievers);

            var commentsByLanguage = dataLoader.Read(() => GetFileStreams("MockData"))
                .ToLookup(comment => comment.Language);

            var analyzer = new SimpleWordScoreAnalysis();

            var languageScores = analyzer.GetLanguageAnalysis(commentsByLanguage);

            var sortedScores = languageScores
                .Select(p => new { Language = p.Key, Score = p.Value })
                .OrderByDescending(p => p.Score);

            foreach (var languageScore in sortedScores)
            {
                Trace.TraceInformation("{0,-15}{1}", languageScore.Language, languageScore.Score);
            }
        }

        private static IEnumerable<Stream> GetFileStreams(string path)
        {
            foreach (var filePath in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories))
            {
                yield return File.OpenRead(filePath);
            }
        }
    }
}
