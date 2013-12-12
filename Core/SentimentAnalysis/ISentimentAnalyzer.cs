namespace HappiestProgrammer.Core.SentimentAnalysis
{
    using System.Collections.Generic;
    using System.Linq;

    public interface ISentimentAnalyzer
    {
        IDictionary<string, float> GetLanguageAnalysis(ILookup<string, Models.Comment> commentsByLanguage);
    }
}
