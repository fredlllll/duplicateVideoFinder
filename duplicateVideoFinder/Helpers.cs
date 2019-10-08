using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace duplicateVideoFinder
{
    public static class Helpers
    {
        public static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo dir, string searchPatternRegex = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternRegex, RegexOptions.IgnoreCase);
            return dir.EnumerateFiles("*", searchOption).Where(file => reSearchPattern.IsMatch(file.Name));
        }
        public static FileInfo[] GetFiles(DirectoryInfo dir, string searchPatternRegex = ".*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return EnumerateFiles(dir, searchPatternRegex, searchOption).ToArray();
            /*Regex reSearchPattern = new Regex(searchPatternRegex, RegexOptions.IgnoreCase);
            var allFiles = dir.GetFiles("*", searchOption);
            List<FileInfo> files = new List<FileInfo>();
            foreach(var fi in allFiles)
            {
                if (reSearchPattern.IsMatch(fi.Name))
                {
                    files.Add(fi);
                }
            }
            return files.ToArray();*/
        }
    }
}
