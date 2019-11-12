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

        private class IntermediateResult : List<Dictionary<AMetric, DupeCollection>>
        {
            public IntermediateResult(int count)
            {
                var comparer = new EqualsComparer<AMetric>();
                for (int i = 0; i < count; i++)
                {
                    this.Add(new Dictionary<AMetric, DupeCollection>(comparer));
                }
            }
        }

        private readonly AMetricGenerator[] generators;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public DuplicateFinder(AMetricGenerator[] generators)
        {
            this.generators = generators;
        }

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

            ConcurrentBag<FileMetricsCollection> metrics = new ConcurrentBag<FileMetricsCollection>();

            int currentFile = 0;
            Parallel.ForEach(files, new Action<FileInfo>((f) =>
            {
                FileMetricsCollection fmc = new FileMetricsCollection(f);
                foreach (var gen in generators)
                {
                    fmc.Add(gen.Generate(f));
                }
                metrics.Add(fmc);
                currentFile++;//TODO: make this threadsafe, but its only for progress report so low priority
                OnProgress?.Invoke(new FractionalProgress(currentFile, fileCount, f.FullName));
            }));


            IntermediateResult duplicates = new IntermediateResult(generators.Length);
            foreach (var item in metrics)
            {
                for (int i = 0; i < generators.Length; i++)
                {
                    var metric = item[0];
                    if (metric != null)
                    {
                        var dupeDict = duplicates[i];
                        DupeCollection dupeFiles;
                        if (!dupeDict.TryGetValue(metric, out dupeFiles))
                        {
                            dupeFiles = new DupeCollection();
                            dupeDict[metric] = dupeFiles;
                        }
                        dupeFiles.Add(item.File);
                    }
                }
            }

            DuplicateFinderResult dfr = new DuplicateFinderResult(generators.Length);

            for (int i = 0; i < generators.Length; i++)
            {
                var dupes = duplicates[i];
                var dupesList = dfr.dupeListsByGenerator[i];
                foreach (var kv in dupes)
                {
                    dupesList.Add(kv.Value);
                }
            }

            OnProgress?.Invoke(new BasicProgress(1, "Done"));
            return dfr;
        }
    }
}
