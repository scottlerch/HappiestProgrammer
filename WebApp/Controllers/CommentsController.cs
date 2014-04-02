using HappiestProgrammer.Core.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.OutputCache;
using WebAPI.OutputCache.TimeAttributes;
using WebApp.Models;
using HappiestProgrammer.Core.Utilities;

namespace WebApp.Controllers
{
    public class CommentsController : ApiController
    {
        // GET api/values
        [CacheOutput(ClientTimeSpan = 86400, ServerTimeSpan = 86400)]
        public IEnumerable<Comment> Get(DateTime date, string language, bool positive, int days)
        {
            var table = this.GetSentimentsTable();

            var query = new TableQuery<CommentEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, date.ToString("yyyyMMdd")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, date.AddDays(-days).ToString("yyyyMMdd"))));

            foreach (var entity in table.ExecuteQuery(query)
                .Where(c => 
                    string.Compare(c.Language, language, ignoreCase: true) == 0 && 
                    ((positive && c.Score > 0) || (!positive && c.Score < 0)))
                .OrderBy(c => c.Score, descending: positive)
                .Take(5))
            {
                yield return new Comment
                {
                    Score = entity.Score,
                    Text = entity.Comment,
                    Date = entity.Date,
                };
            }
        }

        private CloudTable GetSentimentsTable()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var tableClient = storageAccount.CreateCloudTableClient();

            return tableClient.GetTableReference("commentscores");
        }
    }
}