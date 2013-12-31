using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentimentCalculator
{
    public class CommentEntity : TableEntity
    {
        public CommentEntity(string language, DateTime date, double score, string comment)
        {
            this.PartitionKey = date.ToString("yyyyMMdd");
            this.RowKey = Guid.NewGuid().ToString();
            this.Date = date;
            this.Language = language;
            this.Score = score;
            this.Comment = comment;
        }

        public CommentEntity() { }

        public DateTime Date { get; set; }

        public string Language { get; set; }

        public double Score { get; set; }

        public string Comment { get; set; }
    }
}
