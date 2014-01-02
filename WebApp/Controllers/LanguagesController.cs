using HappiestProgrammer.Core.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LanguagesController : ApiController
    {
        // GET api/values
        public IEnumerable<Language> Get(DateTime date)
        {
            var table = this.GetSentimentsTable();

            var query = new TableQuery<LanguageSentimentEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, date.ToString("yyyyMMdd")));

            foreach (var entity in table.ExecuteQuery(query).OrderBy(s => s.Score))
            {
                yield return new Language { Name = entity.Language, Score = entity.Score };
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
