using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace duplicateVideoFinder
{
    public static class FileFinder
    {
        static string GetSearchRegex(string[] extensions)
        {
            if (extensions != null && extensions.Length > 0)
            {
                string[] parts = new string[extensions.Length];
                for (int i = 0; i < extensions.Length; i++)
                {
                    parts[i] = "(.*\\." + extensions[i] + "$)";
                }
                return string.Join('|', parts);
            }
            return ".*";
        }

        public static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo dir, string[] extensions = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex regex = new Regex(GetSearchRegex(extensions), RegexOptions.IgnoreCase);
            return dir.EnumerateFiles("*", searchOption).Where(file => regex.IsMatch(file.Name));
        }

        public static FileInfo[] GetFiles(DirectoryInfo dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFiles(dir, extensions, searchOption).ToArray();
        }
    }
}
