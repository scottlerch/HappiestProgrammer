namespace HappiestProgrammer.Core.DataSources.Twitter
{
    using System;
    using System.Collections.Generic;

    public class TwitterClient : ICommentRetriever
    {
        public IEnumerable<Models.Comment> GetComments(string language, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public string Source
        {
            get { throw new NotImplementedException(); }
        }
    }
}
