using HappiestProgrammer.Core.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using WebAPI.OutputCache;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LanguagesController : ApiController
    {
        // GET api/values
        [CacheOutput(ClientTimeSpan = 86400, ServerTimeSpan = 86400)]
        public IEnumerable<Language> Get(DateTime date, int days)
        {
            var table = this.GetSentimentsTable();

            var query = new TableQuery<LanguageSentimentEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, date.ToString("yyyyMMdd")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, date.AddDays(-days).ToString("yyyyMMdd"))));

            var rank = 1;
            foreach (var entity in table.ExecuteQuery(query)
                .GroupBy(s => s.Language)
                .Select(s => new
                {
                    Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.Key),
                    Score = Math.Round(s.Average(e => e.Score), 2),
                })
                .OrderByDescending(s => s.Score))
            {
                yield return new Language 
                { 
                    Name = entity.Name, 
                    Score = entity.Score,
                    Rank = rank++ 
                };
            }
        }

        private CloudTable GetSentimentsTable()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var tableClient = storageAccount.CreateCloudTableClient();

            return tableClient.GetTableReference("languagesentiments");
        }
    }
}
