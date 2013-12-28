using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentCalculator
{
    public class LanguageSentiment : TableEntity
    {
        public LanguageSentiment(string language, DateTime date, double score)
        {
            this.PartitionKey = date.ToString("yyyyMMdd");
            this.RowKey = language.Replace("#", "sharp") + date.ToString("_yyyyMMdd");
            this.Date = date;
            this.Language = language;
            this.Score = score;
        }

        public LanguageSentiment() { }

        public DateTime Date { get; set; }

        public string Language { get; set; }

        public double Score { get; set; }
    }
}
