namespace HappiestProgrammer.Core.DataSources
{
    using System;
    using System.Collections.Generic;
    using HappiestProgrammer.Core.Models;

    public interface ICommentRetriever
    {
        IEnumerable<Comment> GetComments(string language, DateTime startTime, DateTime endTime);
        string Source { get; }
    }
}
