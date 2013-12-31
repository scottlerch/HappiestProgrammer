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
                        c => GetScore(c.Text)));
            }

            return languageScore;
        }

        private float GetScore(string text)
        {
            return Regex.Split(text, @"\W")
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .DefaultIfEmpty()
                .Average(word => word == null ? 0F : this.wordSentiments.GetScore(word));
        }

        public Models.CommentScoreLists GetCommentSentimentLists(ILookup<string, Models.Comment> commentsByLanguage, int size = 10)
        {
            var scores = commentsByLanguage
                .SelectMany(g => g)
                .Select(c => new Models.CommentScore { Comment = c, Score = GetScore(c.Text) })
                .OrderBy(v => v.Score)
                .ToList();

            return new Models.CommentScoreLists
            {
                Top = scores.Take(size),
                Bottom = GetLast(scores, size),
            };
        }

        private IEnumerable<T> GetLast<T>(IList<T> items, int size)
        {
            var minIndex = Math.Max(items.Count - size, 0);
            for (int i = items.Count - 1; i >= minIndex; i--)
            {
                yield return items[i];
            }
        }
    }
}
