namespace HappiestProgrammer.Core.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using HappiestProgrammer.Core.DataSources.GitHub;
    using HappiestProgrammer.Core.DataSources.StackOverflow;
    using HappiestProgrammer.Core.Models;
    using Newtonsoft.Json;
    using System.Linq;

    public class CommentDataLoader
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<Comment> ReadAllFromDisk(string path)
        {
            return Directory.GetFiles(path)
                .SelectMany(file => File.ReadLines(file)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(JsonConvert.DeserializeObject<Comment>));
        }

        public void WriteAllToDisk(string path, DateTime startTime, DateTime endTime)
        {
            var dataSources = new List<ICommentRetriever>
                {
                    new StackOverflowClient(),
                    new GitHubClient()
                };

            Parallel.ForEach(LanguageTags.All, new ParallelOptions { MaxDegreeOfParallelism = 4 }, lanugage => 
                Parallel.ForEach(dataSources, dataSource =>
                {
                    try
                    {
                        using (var streamWriter = new StreamWriter(this.GetPath(path, lanugage, dataSource.Source, startTime, endTime)))
                        {
                            var serializer = new JsonSerializer();

                            foreach (var comment in dataSource.GetComments(lanugage, startTime, endTime))
                            {
                                serializer.Serialize(streamWriter, comment);
                                streamWriter.WriteLine();
                                streamWriter.Flush();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Error writing file for {0} from {1}", lanugage, dataSource.Source), ex);
                    }
                }));
        }

        private string GetPath(string path, string lanugage, string dataSource, DateTime startTime, DateTime endTime)
        {
            Directory.CreateDirectory(path);
            return Path.Combine(path, string.Format("{0}_{1}_{2:yyyyMMdd}_{3:yyyyMMdd}.json", dataSource, lanugage, startTime, endTime));
        }
    }
}
