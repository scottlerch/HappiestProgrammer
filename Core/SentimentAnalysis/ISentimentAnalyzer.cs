namespace HappiestProgrammer.Core.SentimentAnalysis
{
    using System.Collections.Generic;
    using System.Linq;

    public interface ISentimentAnalyzer
    {
        IDictionary<string, float> GetLanguageAnalysis(ILookup<string, Models.Comment> commentsByLanguage);

        Models.CommentScoreLists GetCommentSentimentLists(ILookup<string, Models.Comment> commentsByLanguage, int size);
    }
}
