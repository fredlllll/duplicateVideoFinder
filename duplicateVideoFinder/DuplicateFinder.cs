using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Metrics;
using duplicateVideoFinder.Progresses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace duplicateVideoFinder
{
    public class DuplicateFinder : IDuplicateFinder
    {
        private class EqualsComparer<T> : EqualityComparer<T>
        {
            public override bool Equals([AllowNull] T x, [AllowNull] T y)
            {
                if (object.Equals(x, y))
                {
                    return true;
                }
                if ((x == null && y != null) || (x != null && y == null))
                {
                    return false;
                }
                return x.Equals(y);
            }

            public override int GetHashCode([DisallowNull] T obj)
            {
                return obj.GetHashCode();
            }
        }

        private class IntermediateResult : List<Dictionary<AMetric, DupeFileCollection>>
        {
            public IntermediateResult(int count)
            {
                var comparer = new EqualsComparer<AMetric>();
                for (int i = 0; i < count; i++)
                {
                    this.Add(new Dictionary<AMetric, DupeFileCollection>(comparer));
                }
            }
        }

        private readonly AMetricGenerator[] generators;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public DuplicateFinder(AMetricGenerator[] generators)
        {
            this.generators = generators;
        }

        /// <summary>
        /// creates a regex to test against filenames
        /// </summary>
        /// <returns></returns>
        private string GetSearchRegex()
        {
            JArray extensions = Settings.appSettings.Data["extensionsToProcess"] as JArray;
            if (extensions != null && extensions.Count > 0)
            {
                List<string> parts = new List<string>();
                foreach (var token in extensions)
                {
                    var ext = token.Value<string>();
                    parts.Add("(.*\\." + ext + "$)");
                }
                return string.Join('|', parts);
            }
            return ".*";
        }

        public DuplicateFinderResult FindDuplicates(DirectoryInfo dir)
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));

            string searchRegex = GetSearchRegex();

            var files = Helpers.EnumerateFiles(dir, searchRegex, SearchOption.AllDirectories);

            int fileCount = 0;
            //get the total count of files asynchronously so we can start collecting metrics using the enumerator
            var fileCountTask = new Task(() =>
            {
                var getFiles = Helpers.GetFiles(dir, searchRegex, SearchOption.AllDirectories);
                fileCount = getFiles.Length;
            });
            fileCountTask.Start();

            List<MetricDict> metricsPerGenerator = new List<MetricDict>();

            int currentFile = 0;
            foreach (var gen in generators)
            {
                MetricDict fileMetrics = new MetricDict();
                Parallel.ForEach(files, new Action<FileInfo>((f) =>
                {
                    fileMetrics[f] = gen.Generate(f);
                    currentFile++;
                    OnProgress?.Invoke(new FractionalProgress(currentFile, fileCount * generators.Length, f.FullName));
                }));
                metricsPerGenerator.Add(fileMetrics);
            }

            DuplicateFinderResult dfr = new DuplicateFinderResult();

            for (int i = 0; i < generators.Length; i++)
            {
                Dictionary<AMetric, DupeFileCollection> filesByMetrics = new Dictionary<AMetric, DupeFileCollection>();
                foreach (var kv in metricsPerGenerator[i]) //running over metrics of each generator
                {
                    if(kv.Value == null)
                    {
                        continue;
                    }
                    DupeFileCollection dfc;
                    if (!filesByMetrics.TryGetValue(kv.Value, out dfc))
                    {
                        dfc = new DupeFileCollection(kv.Value);
                        filesByMetrics[kv.Value] = dfc;
                    }
                    dfc.Add(kv.Key);
                }

                DupeCollection dc = new DupeCollection();

                foreach (var kv in filesByMetrics)
                {
                    if (kv.Value.Count > 1)
                    {
                        dc.Add(kv.Value);
                    }
                }
                dfr.dupesByGenerator.Add(dc);
            }

            OnProgress?.Invoke(new BasicProgress(1, "Done"));

            return dfr;
        }
    }
}
