namespace HappiestProgrammer.Core.DataSources
{
    using HappiestProgrammer.Core.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class CommentDataLoader
    {
        private readonly IEnumerable<ICommentRetriever> dataSources;

        public CommentDataLoader()
        {
            this.dataSources = Enumerable.Empty<ICommentRetriever>();
        }

        public CommentDataLoader(IEnumerable<ICommentRetriever> dataSources)
        {
            this.dataSources = dataSources;
        }

        public IEnumerable<Comment> Read(Func<IEnumerable<Stream>> getReadStreams)
        {
            return getReadStreams()
                    .SelectMany(stream => this.GetLines(stream)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(JsonConvert.DeserializeObject<Comment>));
        }

        private IEnumerable<string> GetLines(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                yield return reader.ReadLine();
            }
        }

        public void Write(DateTime date, Func<string,Stream> getWriteStream, Action<string, Stream> streamWriteComplete = null)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("date must be Utc");
            }

            if (date.Date != date)
            {
                throw new ArgumentException("date must be a whole date with no time component");
            }

            var endTime = date.AddDays(1);

            Parallel.ForEach(LanguageTags.All, new ParallelOptions { MaxDegreeOfParallelism = 1 }, lanugage => 
                Parallel.ForEach(dataSources, dataSource =>
                {
                    try
                    {
                        var fileName = this.GetFileName(lanugage, dataSource.Source, date);

                        using (var streamWriter = new StreamWriter(getWriteStream(fileName)))
                        {
                            var serializer = new JsonSerializer();

                            foreach (var comment in dataSource.GetComments(lanugage, date, endTime))
                            {
                                serializer.Serialize(streamWriter, comment);
                                streamWriter.WriteLine();
                                streamWriter.Flush();
                            }

                            if (streamWriteComplete != null)
                            {
                                streamWriteComplete(fileName, streamWriter.BaseStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error writing file for {0} from {1}: {2}", lanugage, dataSource.Source, ex);
                    }
                }));
        }

        private string GetFileName(string lanugage, string dataSource, DateTime date)
        {
            return string.Format("{0}/{1:yyyy/MM/dd}/comments_{1:yyyyMMdd}_{2}.json", dataSource, date, lanugage);
        }
    }
}
