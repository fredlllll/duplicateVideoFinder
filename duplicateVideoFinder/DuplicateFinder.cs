using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Metrics;
using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
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

        private class IntermediateResult : List<Dictionary<AMetric, List<FileInfo>>>
        {
            public IntermediateResult(int count)
            {
                var comparer = new EqualsComparer<AMetric>();
                for (int i = 0; i < count; i++)
                {
                    this.Add(new Dictionary<AMetric, List<FileInfo>>(comparer));
                }
            }
        }

        private readonly AMetricGenerator[] generators;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public DuplicateFinder(AMetricGenerator[] generators)
        {
            this.generators = generators;
        }

        public DuplicateFinderResult FindDuplicates(DirectoryInfo dir)
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));
            var files = dir.EnumerateFiles("*", SearchOption.AllDirectories);
            int fileCount = 0;

            //get the total count of files asynchronously so we can start collecting metrics using the enumerator
            var fileCountTask = new Task(() =>
            {
                var getFiles = dir.GetFiles("*", SearchOption.AllDirectories);
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
                currentFile++;//TODO: make this threadsafe
                OnProgress?.Invoke(new FractionalProgress(currentFile, fileCount, f.FullName));
            }));


            IntermediateResult duplicates = new IntermediateResult(generators.Length);
            foreach (var item in metrics)
            {
                for (int i = 0; i < generators.Length; i++)
                {
                    var metric = item[0];
                    var dupeDict = duplicates[i];
                    List<FileInfo> dupeFiles;
                    if (!dupeDict.TryGetValue(metric, out dupeFiles))
                    {
                        dupeFiles = new List<FileInfo>();
                        dupeDict[metric] = dupeFiles;
                    }
                    dupeFiles.Add(item.File);
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
