namespace HappiestProgrammer.Core.DataSources.StackOverflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HappiestProgrammer.Core;
    using HappiestProgrammer.Core.Utilities;
    using StackExchange.StacMan;
    using Answers = StackExchange.StacMan.Answers;
    using Questions = StackExchange.StacMan.Questions;
    using System.Diagnostics;

    public class StackOverflowClient : ICommentRetriever
    {
        private readonly StacManClient client;

        public StackOverflowClient()
        {
            this.client =  new StacManClient(Settings.Default.StackOverflowKey);
        }

        public string Source
        {
            get { return "stackoverflow"; }
        }

        public IEnumerable<HappiestProgrammer.Core.Models.Comment> GetComments(string language, DateTime startTime, DateTime endTime)
        {
            var questionIds = GetQuestionIds(language, startTime, endTime).ToList();
            var answerIds = GetAnswerIds(questionIds);
            var comments = GetComments(questionIds.Concat(answerIds));

            return comments.Select(c => new HappiestProgrammer.Core.Models.Comment
                {
                    Text = c.Body, 
                    CommentId = c.CommentId.ToString(), 
                    DataSource = "stackoverflow.com", 
                    Language = language, 
                }).ToList();
        }

        private IEnumerable<StackExchange.StacMan.Comment> GetComments(IEnumerable<int> postIds)
        {
            // https://api.stackexchange.com/2.1/posts/15581721%3B15787953%3B15804800/comments?order=desc&sort=creation&site=stackoverflow&filter=!)rVscPO0U2Q85(gAaeyi

            foreach (var postIdsChunk in postIds.Partition(100))
            {
                bool hasMore = true;
                const int pageSize = 100;
                int page = 1;

                while (hasMore)
                {
                    var commentsResult =
                        this.client.Posts.GetComments(
                            site: "stackoverflow",
                            ids:postIdsChunk,
                            page: page,
                            pagesize: pageSize,
                            order: Order.Desc,
                            sort: StackExchange.StacMan.Comments.Sort.Creation,
                            filter: "!)rVscPO0U2Q85(gAaeyi").Result;

                    /*
                    {
                      "items": [
                        {
                          "comment_id": 22484251,
                          "post_id": 15581721,
                          "creation_date": 1365073861,
                          "body": "If you already <i>have</i> the location of the working area on the screen, then the problem is just of homography re-projection, do you have the bounds of the working area? If yes, I&#39;ll post my code and usage in the answers...",
                          "owner": {
                            "user_id": 1275880,
                            "display_name": "Shreyas Kapur"
                          }
                        },
                     */

                    if (commentsResult.Success)
                    {
                        if (commentsResult.Data == null || commentsResult.Data.Items == null)
                        {
                            Trace.TraceError("No data found");
                            yield break;
                        }

                        foreach (var item in commentsResult.Data.Items)
                        {
                            yield return item;
                        }

                        hasMore = commentsResult.Data.HasMore;
                        page++;
                    }
                    else
                    {
                        Trace.TraceError("Unable to load comments: {0}", commentsResult.Error);
                        yield break;
                    }
                }
            }
        }

        private IEnumerable<int> GetAnswerIds(IEnumerable<int> questionIds)
        {
            // https://api.stackexchange.com/2.1/questions/15524045/answers?order=desc&sort=creation&site=stackoverflow&filter=!--58Cp84lOTE

            foreach (var questionIdsChunk in questionIds.Partition(100))
            {
                var hasMore = true;
                const int pageSize = 100;
                var page = 1;

                while (hasMore)
                {
                    var answersResult =
                        this.client.Questions.GetAnswers(
                            site: "stackoverflow",
                            ids: questionIdsChunk,
                            page: page,
                            pagesize: pageSize,
                            order: Order.Desc,
                            sort: Answers.Sort.Creation,
                            filter: "!--58Cp84lOTE").Result;

                    if (answersResult.Success)
                    {
                        if (answersResult.Data == null || answersResult.Data.Items == null)
                        {
                            Trace.TraceError("No data found");
                            yield break;
                        }

                        foreach (var item in answersResult.Data.Items)
                        {
                            yield return item.AnswerId;
                        }

                        hasMore = answersResult.Data.HasMore;
                        page++;
                    }
                    else
                    {
                        Trace.TraceError("Unable to load answers: {0}", answersResult.Error);
                        yield break;
                    }
                }
            }
        }

        private IEnumerable<int> GetQuestionIds(string tag, DateTime startTime, DateTime endTime)
        {
            var hasMore = true;
            const int pageSize = 100;
            var page = 1;

            while (hasMore)
            {
                var searchResults =
                    this.client.Search.GetMatches(
                        site: "stackoverflow",
                        filter: "!)5E5HksQ0OCzhj7JCjx(FqMp-rnA",
                        page: page,
                        pagesize: pageSize,
                        fromdate: startTime,
                        todate: endTime,
                        sort: Questions.SearchSort.Creation,
                        order: Order.Desc,
                        tagged: tag,
                        nottagged: string.Join(";", LanguageTags.All.Except(new[] { tag }))).Result;

                if (searchResults.Success)
                {
                    if (searchResults.Data == null || searchResults.Data.Items == null)
                    {
                        Trace.TraceError("No data found");
                        yield break;
                    }

                    foreach (var item in searchResults.Data.Items)
                    {
                        yield return item.QuestionId;
                    }

                    hasMore = searchResults.Data.HasMore;
                    page++;
                }
                else
                {
                    Trace.TraceError("Unable to load answers", searchResults.Error);
                    yield break;
                }
            }
        }
    }
}
