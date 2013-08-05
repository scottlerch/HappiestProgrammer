namespace HappiestProgrammer.Runner
{
    using System;
    using System.Linq;
    using HappiestProgrammer.Core.DataSources;
    using HappiestProgrammer.Core.SentimentAnalysis;

    internal class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Info("Starting application...");

            // LoadFromDataSource();
            CalculateSentimentFromDisk();

            log.Info("Complete!");

            Console.ReadKey();
        }

        private static void LoadFromDataSource()
        {
            var dataLoader = new CommentDataLoader();

            var startTime = new DateTime(2013, 7, 31, 0, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddDays(1);

            dataLoader.WriteAllToDisk("comments", startTime, endTime);
        }

        private static void CalculateSentimentFromDisk()
        {
            var dataLoader = new CommentDataLoader();

            var commentsByLanguage = dataLoader.ReadAllFromDisk("SampleData")
                .ToLookup(comment => comment.Language);

            var analyzer = new SimpleWordScoreAnalysis();

            var languageScores = analyzer.GetLanguageAnalysis(commentsByLanguage);

            var sortedScores = languageScores
                .Select(p => new { Language = p.Key, Score = p.Value })
                .OrderByDescending(p => p.Score);

            foreach (var languageScore in sortedScores)
            {
                log.InfoFormat("{0,-15}{1}", languageScore.Language, languageScore.Score);
            }
        }
    }
}
