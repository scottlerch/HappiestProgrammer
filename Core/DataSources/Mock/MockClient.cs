using System;
using System.Collections.Generic;

namespace HappiestProgrammer.Core.DataSources.Mock
{
    public class MockClient : ICommentRetriever
    {
        public IEnumerable<Models.Comment> GetComments(string language, DateTime startTime, DateTime endTime)
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new Models.Comment 
                { 
                    CommentId = Guid.NewGuid().ToString(), 
                    DataSource = Source, 
                    Language = language, 
                    Text = Guid.NewGuid().ToString() 
                };
            }
        }

        public string Source
        {
            get { return "mock"; }
        }
    }
}
