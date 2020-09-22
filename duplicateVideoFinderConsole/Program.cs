using duplicateVideoFinder;
using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Progresses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinderConsole
{
    class Program
    {
        private static void FindDupesAndOutputToJSON(DirectoryInfo di)
        {
            var finder = new DuplicateFinder(new AMetricGenerator[] { new HashMetricGenerator() }, di); //TODO: generators depend on input

            finder.OnProgress += Finder_OnProgress;

            var dupes = finder.FindDuplicates();

            JArray result = new JArray();

            foreach (var kv in dupes.dupesByGenerator)
            {
                foreach (var dupe in kv.Value)
                {
                    JArray fileList = new JArray();
                    foreach (var fi in dupe)
                    {
                        fileList.Add(fi.FullName);
                    }
                    result.Add(fileList);
                }
            }

            Console.WriteLine(result.ToString());
        }

        private class ConsoleProcessReceiver : IProgressReceiver
        {
            public void Update(IProgress progress)
            {
                Finder_OnProgress(progress);
            }
        }

        private static object consoleLock = new object();
        private static void Finder_OnProgress(IProgress progress)
        {
            lock (consoleLock)
            {
                int top = Console.CursorTop;
                int left = Console.CursorLeft;

                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("".PadLeft(Console.BufferWidth, ' '));
                Console.CursorTop = 0;
                Console.CursorLeft = 0;
                Console.Write("Progress: " + progress.ToString());

                Console.CursorTop = top;
                Console.CursorLeft = left;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("please provide a folder");
                return;
            }

            Console.Clear();
            Console.WriteLine();

            DirectoryInfo di = new DirectoryInfo(args[0]);

            //FindDupesAndOutputToJSON(di);

            var hashMetricGen = new HashMetricGenerator();
            var folderMetricGen = new FolderMetricGenerator(hashMetricGen, di, AppSettings.Instance.extensionsToProcess, SearchOption.AllDirectories);

            Console.WriteLine("Generating metrics");
            var metrics = folderMetricGen.GenerateMetrics(new ConsoleProcessReceiver());

            Console.WriteLine("Finding Dupes");
            var dupes = PotentialDuplicateFinder.FindDupes(metrics);

            Console.WriteLine("Autosorting Dupes");
            int autosorted = 0;
            foreach (var dupeFileCollection in dupes)
            {
                Dictionary<string, int> filenameCount = new Dictionary<string, int>();

                foreach (var file in dupeFileCollection)
                {
                    var filename = file.Name;
                    if (filenameCount.TryGetValue(filename, out int count))
                    {
                        filenameCount[filename] = count + 1;
                    }
                    else
                    {
                        filenameCount[filename] = 1;
                    }
                }

                foreach (var kv in filenameCount)
                {
                    if (kv.Value > 1) //we found a file with the same name
                    {
                        autosorted++;
                        foreach (var file in dupeFileCollection)
                        {
                            if (file.FullName.Contains("UNSORTED"))
                            {
                                file.Delete(); //delete file in UNSORTED
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Autosorted " + autosorted + " files");
            Console.WriteLine("Press any key to end");
            Console.ReadKey();
        }
    }
}
