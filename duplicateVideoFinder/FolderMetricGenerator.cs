using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Metrics;
using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            int fileCount = 0;
            //get the total count of files asynchronously so we can start collecting metrics using the enumerator
            var fileCountTask = new Task(() =>
            {
                var getFiles = FileFinder.GetFiles(targetDir, extensionsToProcess, searchOption);
                fileCount = getFiles.Length;
            });
            fileCountTask.Start();

            MetricDict metrics = new MetricDict();

            var files = FileFinder.EnumerateFiles(targetDir, extensionsToProcess, searchOption);
            int currentFile = 0; //TODO: make atomic increments to mitigate race conditions
            Parallel.ForEach(files, new Action<FileInfo>((f) =>
            {
                var metric = metricGen.Generate(f);
                if (metric != null)
                {
                    metrics[f] = metric;
                }
                currentFile++;
                progressReceiver?.Update(new FractionalProgress(currentFile, fileCount, f.FullName));
            }));

            return metrics;
        }
    }
}
