namespace HappiestProgrammer.Core.Utilities
{
    using System.Net;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string StripHtml(this string html)
        {
            html = Regex.Replace(html, @"<[^>]*(>|$)", "", RegexOptions.Multiline);
            html = Regex.Replace(html, @"[\s\r\n]+", " ");
            html = WebUtility.HtmlDecode(html);
            return html;
        }
    }
}
