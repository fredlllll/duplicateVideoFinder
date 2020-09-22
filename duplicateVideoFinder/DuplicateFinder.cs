using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Progresses;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace duplicateVideoFinder
{
    public class DuplicateFinder : IDuplicateFinder, IProgressReceiver
    {
        private readonly AMetricGenerator[] generators;
        private readonly DirectoryInfo dir;
        private readonly bool topDirOnly;

        public event IDuplicateFinder.ProgressHandler OnProgress;

        public DuplicateFinder(AMetricGenerator[] generators, DirectoryInfo dir, bool topDirOnly = false)
        {
            this.generators = generators;
            this.dir = dir;
            this.topDirOnly = topDirOnly;
        }

        public DuplicateFinderResult FindDuplicates()
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));

            SearchOption filesSearchOption = topDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

            int fileCount = 0;
            //get the total count of files asynchronously so we can start collecting metrics using the enumerator
            var fileCountTask = new Task(() =>
            {
                var getFiles = FileFinder.GetFiles(dir, AppSettings.Instance.extensionsToProcess, filesSearchOption);
                fileCount = getFiles.Length;
            });
            fileCountTask.Start();

            Dictionary<string, MetricDict> metricsPerGenerator = new Dictionary<string, MetricDict>();

            foreach (var gen in generators) //should i make this a parallel foreach too? most of it is IO heavy, so only gain would be for saving and loading metrics
            {
                MetricDict fileMetrics = MetricCache.LoadMetrics(dir, gen.ID);
                if (fileMetrics == null)
                {
                    var metricGen = new FolderMetricGenerator(gen, dir, AppSettings.Instance.extensionsToProcess, filesSearchOption);
                    fileMetrics = metricGen.GenerateMetrics(this);
                    MetricCache.SaveMetrics(fileMetrics, dir, gen.ID);
                }
                metricsPerGenerator[gen.ID] = fileMetrics;
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
