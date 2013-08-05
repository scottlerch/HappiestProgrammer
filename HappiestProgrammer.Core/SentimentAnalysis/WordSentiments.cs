namespace HappiestProgrammer.Core.SentimentAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class WordSentiments
    {
        private readonly Dictionary<string, float> sentiments;

        public WordSentiments()
        {
            this.sentiments = new Dictionary<string, float>(StringComparer.InvariantCultureIgnoreCase);

            var positiveWords =
                File.ReadLines(@"Resources\positive-words.txt")
                    .Where(s => !s.StartsWith(";") && !string.IsNullOrWhiteSpace(s));

            foreach (var word in positiveWords)
            {
                this.AddSentiment(word, 1);
            }

            var negativeWords =
                File.ReadLines(@"Resources\negative-words.txt")
                    .Where(s => !s.StartsWith(";") && !string.IsNullOrWhiteSpace(s));

            foreach (var word in negativeWords)
            {
                this.AddSentiment(word, -1);
            }
        }

        public float GetScore(string word)
        {
            float score;

            if (this.sentiments.TryGetValue(word, out score))
            {
                return score;
            }

            return 0;
        }

        private void AddSentiment(string word, float score)
        {
            if (this.sentiments.ContainsKey(word))
            {
                this.sentiments[word] = (this.sentiments[word] + score) / 2F;
            }
            else
            {
                this.sentiments[word] = score;
            }
        }
    }
}
