namespace HappiestProgrammer.Core
{
    using System.Collections.Generic;
    using System.IO;

    public static class LanguageTags
    {
        private static readonly string[] Tags = File.ReadAllLines("Resources\\language_tags.txt");

        public static IEnumerable<string> All { get { return Tags; }  }
    }
}
