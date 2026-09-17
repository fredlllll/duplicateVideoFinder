using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Progresses;
using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    public class DuplicateFinder : IDuplicateFinder, IProgressReceiver
    {
        private readonly AMetricGenerator[] generators;
        private readonly DirectoryInfo dir;
        private readonly bool topDirOnly;
        private readonly bool deleteCache;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public DuplicateFinder(AMetricGenerator[] generators, DirectoryInfo dir, bool topDirOnly = false, bool deleteCache = false)
        {
            this.generators = generators;
            this.dir = dir;
            this.topDirOnly = topDirOnly;
            this.deleteCache = deleteCache;
        }

        public DuplicateFinderResult FindDuplicates()
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));

            SearchOption filesSearchOption = topDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

            var currentFiles = FileFinder.GetFiles(dir, AppSettings.Instance.extensionsToProcess, filesSearchOption);

            Dictionary<string, MetricDict> metricsPerGenerator = new Dictionary<string, MetricDict>();

            foreach (var gen in generators)
            {
                if (deleteCache)
                {
                    MetricCache.DeleteCache(dir, gen.ID);
                }

                var loadResult = MetricCache.LoadMetrics(dir, gen.ID, currentFiles);

                if (loadResult.FilesToCompute.Count > 0)
                {
                    var metricGen = new FolderMetricGenerator(gen, dir, AppSettings.Instance.extensionsToProcess, filesSearchOption);
                    var computed = metricGen.GenerateMetrics(loadResult.FilesToCompute, this);
                    foreach (var kvp in computed)
                    {
                        loadResult.Reusable[kvp.Key] = kvp.Value;
                    }
                    MetricCache.SaveMetrics(dir, gen.ID, currentFiles, computed);
                }

                metricsPerGenerator[gen.ID] = loadResult.Reusable;
            }

            DuplicateFinderResult dfr = new DuplicateFinderResult();

            foreach (var gen in generators)
            {
                DupeCollection dc = PotentialDuplicateFinder.FindDupes(metricsPerGenerator[gen.ID]);
                dfr.dupesByGenerator[gen.ID] = dc;
            }

            OnProgress?.Invoke(new BasicProgress(1, "Done"));

            return dfr;
        }

        public void Update(IProgress progress)
        {
            OnProgress?.Invoke(progress); //TODO: this will now run 0-1 for each gen, dunno how to handle that yet
        }
    }
}
