namespace HappiestProgrammer.Core.DataSources.GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using HappiestProgrammer.Core.Models;
    using Newtonsoft.Json;
    using RestSharp;
    using System.Configuration;

    public class GitHubClient : ICommentRetriever
    {
        private readonly RestClient client;

        public GitHubClient()
        {
            this.client = new RestClient("https://api.github.com");
            this.client.AddDefaultHeader("Authorization", "token " + ConfigurationManager.AppSettings["GitHubToken"]);
        }

        public string Source
        {
            get { return "github"; }
        }

        public IEnumerable<Models.Comment> GetComments(string language, DateTime startTime, DateTime endTime)
        {
            string nextSearchRequestLink = null;

            do
            {
                var searchRequest = new RestRequest("search/repositories", Method.GET);

                if (nextSearchRequestLink == null)
                {
                    searchRequest.AddParameter("q", string.Format("language:{0}+pushed:{1:yyyy-MM-dd}..{2:yyyy-MM-dd}", this.ConvertLanguage(language), startTime, endTime));
                    searchRequest.AddParameter("per_page", 100);
                }
                else
                {
                    this.AddParamtersFromUrl(searchRequest, nextSearchRequestLink);
                }

                searchRequest.AddHeader("Accept", "application/vnd.github.preview");

                var searchResponse = client.Execute(searchRequest);

                this.CheckRateLimit(searchResponse);

                nextSearchRequestLink = this.GetNextLink(searchResponse);

                var repos = JsonConvert.DeserializeObject<dynamic>(searchResponse.Content);

                int searchtTotalCount = repos.total_count;

                foreach (var item in repos.items)
                {
                    string nextCommitsRequestLink = null;

                    do
                    {
                        string fullName = item.full_name;

                        var commitsRequest = new RestRequest("repos/" + fullName + "/commits", Method.GET);

                        if (nextCommitsRequestLink == null)
                        {
                            commitsRequest.AddParameter("since", string.Format("{0:yyyy-MM-dd}", startTime));
                            commitsRequest.AddParameter("until", string.Format("{0:yyyy-MM-dd}", endTime));
                            commitsRequest.AddParameter("per_page", 100);
                        }
                        else
                        {
                            this.AddParamtersFromUrl(commitsRequest, nextCommitsRequestLink);
                        }

                        var commitsResponse = client.Execute(commitsRequest);
                        this.CheckRateLimit(commitsResponse);
                        nextCommitsRequestLink = this.GetNextLink(commitsResponse);

                        var commits = JsonConvert.DeserializeObject<dynamic>(commitsResponse.Content);

                        foreach (var commitItem in commits)
                        {
                            yield return
                                new Comment
                                {
                                    CommentId = fullName + "/" + commitItem.sha,
                                    DataSource = "github.com",
                                    Language = language,
                                    Text = commitItem.commit.message,
                                };
                        }
                    }
                    while (nextCommitsRequestLink != null);
                }
            }
            while (nextSearchRequestLink != null);
        }

        private string ConvertLanguage(string language)
        {
            return language
                .Replace("#", "sharp")
                .Replace("vb.net", "visual-basic")
                .ToLower();
        }

        private string GetNextLink(IRestResponse response)
        {
            var links = response.Headers.FirstOrDefault(h => h.Name == "Link");

            if (links != null)
            {
                var nextLink = links.Value.ToString().Split(',').FirstOrDefault(s => s.Contains("rel=\"next\""));

                if (nextLink != null)
                {
                    return nextLink.Split(';')[0].Trim('<', '>');
                }
            }

            return null;
        }

        private void AddParamtersFromUrl(IRestRequest request, string url)
        {
            foreach (var parts in url.Split('?')[1].Split('&').Select(paramter => paramter.Split('=')))
            {
                request.AddParameter(parts[0], parts[1].Replace("%3A", ":").Replace("%2B", "+"));
            }
        }

        private void CheckRateLimit(IRestResponse response)
        {
            var rateLimitRemaining = response.Headers.FirstOrDefault(h => h.Name == "X-RateLimit-Remaining");
            
            if (rateLimitRemaining != null && rateLimitRemaining.Value.ToString() == "0")
            {
                var rateLimiteReset = response.Headers.FirstOrDefault(h => h.Name == "X-RateLimit-Reset");

                var rateResetTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(
                    double.Parse(rateLimiteReset.Value.ToString()));

                Thread.Sleep(rateResetTime - DateTime.UtcNow);
            }
        }
    }
}
