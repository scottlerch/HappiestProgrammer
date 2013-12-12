namespace HappiestProgrammer.Core.SentimentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class SimpleWordScoreAnalysis : ISentimentAnalyzer
    {
        private readonly WordSentiments wordSentiments;

        public SimpleWordScoreAnalysis()
        {
            this.wordSentiments = new WordSentiments();
        }

        public IDictionary<string, float> GetLanguageAnalysis(ILookup<string, Models.Comment> commentsByLanguage)
        {
            var languageScore = new Dictionary<string, float>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var comments in commentsByLanguage)
            {
                languageScore.Add(
                    comments.Key,
                    comments.Average(
                        c => Regex.Split(c.Text, @"\W")
                            .Where(word => !string.IsNullOrWhiteSpace(word))
                            .DefaultIfEmpty()
                            .Average(word => word == null? 0F : this.wordSentiments.GetScore(word))));
            }

            return languageScore;
        }
    }
}
