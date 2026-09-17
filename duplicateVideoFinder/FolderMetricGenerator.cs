using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Metrics;
using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace duplicateVideoFinder
{
    public class FolderMetricGenerator
    {
        AMetricGenerator metricGen;
        DirectoryInfo targetDir;
        string[] extensionsToProcess;
        SearchOption searchOption;


        public FolderMetricGenerator(AMetricGenerator metricGen, DirectoryInfo targetDir, string[] extensionsToProcess, SearchOption searchOption)
        {
            this.metricGen = metricGen;
            this.targetDir = targetDir;
            this.extensionsToProcess = extensionsToProcess;
            this.searchOption = searchOption;
        }

        public MetricDict GenerateMetrics(IProgressReceiver progressReceiver)
        {
            var files = FileFinder.GetFiles(targetDir, extensionsToProcess, searchOption);
            return GenerateMetrics(files, progressReceiver);
        }

        public MetricDict GenerateMetrics(IEnumerable<FileInfo> files, IProgressReceiver progressReceiver)
        {
            var fileList = files as IReadOnlyList<FileInfo> ?? files.ToList();
            int fileCount = fileList.Count;

            MetricDict metrics = new MetricDict();

            int currentFile = 0;
            Parallel.ForEach(fileList, new Action<FileInfo>((f) =>
            {
                try
                {
                    var metric = metricGen.Generate(f);
                    if (metric != null)
                    {
                        metrics[f] = metric;
                    }
                }
                catch
                {
                    // a single unreadable/locked file must not abort the whole scan
                }
                System.Threading.Interlocked.Increment(ref currentFile);
                progressReceiver?.Update(new FractionalProgress(currentFile, fileCount, f.FullName));
            }));

            return metrics;
        }
    }
}