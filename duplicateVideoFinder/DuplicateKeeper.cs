using duplicateVideoFinder.Metrics;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace duplicateVideoFinder
{
    public static class DuplicateKeeper
    {
        /// <summary>
        /// Picks the single best file to keep from a duplicate group.
        /// Non-binary duplicates (e.g. same duration) are ranked by bitrate.
        /// Binary duplicates (identical hash) are ranked by folder ("sorted" wins)
        /// and then by how "title-like" the filename is.
        /// </summary>
        public static FileInfo GetFileToKeep(DupeFileCollection dupes)
        {
            var existing = dupes.Where(f => f.Exists).ToList();
            if (existing.Count <= 1)
            {
                return existing.FirstOrDefault();
            }
            return existing.OrderByDescending(f => Score(f, dupes.metric)).First();
        }

        static double Score(FileInfo f, AMetric metric)
        {
            double score = 0;

            if (metric is DurationMetric dm)
            {
                // higher bitrate wins
                if (dm.duration > 0)
                {
                    score += (f.Length * 8.0) / dm.duration;
                }
                else
                {
                    score += f.Length;
                }
                if (IsSorted(f))
                {
                    score += 1; // tiny tiebreak when bitrates are equal
                }
                score += FilenameQuality(f.Name) * 0.01;
            }
            else
            {
                // binary duplicates: identical content, so prefer location/title
                if (IsSorted(f))
                {
                    score += 1000;
                }
                score += FilenameQuality(f.Name) * 10;
                score += f.Length; // tiny tiebreak against exact ties
            }

            return score;
        }

        static bool IsSorted(FileInfo f)
        {
            return f.DirectoryName.IndexOf("UNSORTED", StringComparison.OrdinalIgnoreCase) < 0;
        }

        /// <summary>
        /// Rough heuristic: real video titles score higher than ID-like names
        /// (GUIDs, hex digests, pure numbers, camera-style names).
        /// </summary>
        static double FilenameQuality(string name)
        {
            string stem = Path.GetFileNameWithoutExtension(name);
            if (string.IsNullOrEmpty(stem))
            {
                return 0;
            }

            double score = 0;

            // more space/underscore-separated words usually means a descriptive title
            int wordCount = stem.Split(new[] { ' ', '_', '-', '.', '\'' }, StringSplitOptions.RemoveEmptyEntries).Length;
            score += Math.Min(wordCount, 10);

            // has real letters beyond a single char
            if (stem.Any(char.IsLetter))
            {
                score += 5;
            }

            if (Regex.IsMatch(stem, @"^[0-9a-f]{32}$", RegexOptions.IgnoreCase))
            {
                score -= 30; // md5-like digest
            }
            if (Regex.IsMatch(stem, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase))
            {
                score -= 30; // GUID
            }
            if (Regex.IsMatch(stem, @"^\d+$"))
            {
                score -= 20; // pure number
            }
            if (Regex.IsMatch(stem, @"^(IMG|DSC|VID|MOV|MVI|PXL)\d*", RegexOptions.IgnoreCase))
            {
                score -= 10; // camera-style name
            }

            return score;
        }
    }
}