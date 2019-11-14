using duplicateVideoFinder.MetricGenerators;
using duplicateVideoFinder.Metrics;
using duplicateVideoFinder.Progresses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
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

        string GetMetricsFileName(DirectoryInfo directory, string id)
        {
            using var md5 = MD5.Create();
            var dirNameHashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(directory.FullName));
            string dirNameHash = BitConverter.ToString(dirNameHashBytes).Replace("-", string.Empty);
            return dirNameHash + "_" + id + "_metrics.json";
        }

        void SaveMetrics(MetricDict dict, DirectoryInfo directory, string id)
        {
            string filename = GetMetricsFileName(directory, id);

            JObject obj = new JObject();
            JArray data = new JArray();
            Dictionary<Type, int> typeDict = new Dictionary<Type, int>();
            obj["directory"] = directory.FullName;
            obj["data"] = data;
            foreach (var kv in dict)
            {
                JObject entry = new JObject();

                int typeId;
                if (!typeDict.TryGetValue(kv.Value.GetType(), out typeId))
                {
                    typeId = typeDict.Count;
                    typeDict[kv.Value.GetType()] = typeId;
                }
                entry["type"] = typeId;
                entry["metric"] = JObject.FromObject(kv.Value);
                entry["file"] = kv.Key.FullName;
                data.Add(entry);
            }
            JObject typeDictJson = new JObject();
            foreach (var kv in typeDict)
            {
                typeDictJson[kv.Value.ToString()] = kv.Key.AssemblyQualifiedName;
            }
            obj["types"] = typeDictJson;

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonTextWriter jtw = new JsonTextWriter(sw))
            {
                obj.WriteTo(jtw);
            }
        }

        MetricDict LoadMetrics(DirectoryInfo directory, string id)
        {
            string filename = GetMetricsFileName(directory, id);
            if (File.Exists(filename))
            {
                MetricDict metricDict = new MetricDict();

                JObject metrics;
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonTextReader jtr = new JsonTextReader(sr))
                {
                    metrics = JObject.Load(jtr);
                }

                //recover type dict but with id as index
                Dictionary<int, Type> typeDict = new Dictionary<int, Type>();
                JObject typeDictJson = metrics["types"] as JObject;
                foreach (var kv in typeDictJson)
                {
                    typeDict[int.Parse(kv.Key)] = Type.GetType(kv.Value.Value<string>());
                }

                JArray data = metrics["data"] as JArray;
                foreach (JObject entry in data)
                {
                    Type t = typeDict[entry["type"].Value<int>()];
                    AMetric metric = entry["metric"].ToObject(t) as AMetric;
                    FileInfo file = new FileInfo(entry["file"].Value<string>());
                    metricDict[file] = metric;
                }
                return metricDict;
            }
            return null;
        }

        public DuplicateFinderResult FindDuplicates(DirectoryInfo dir, bool topDirOnly = false)
        {
            OnProgress?.Invoke(new BasicProgress(0, "Starting up..."));

            string searchRegex = GetSearchRegex();

            SearchOption filesSearchOption = SearchOption.AllDirectories;
            if (topDirOnly)
            {
                filesSearchOption = SearchOption.TopDirectoryOnly;
            }
            var files = Helpers.EnumerateFiles(dir, searchRegex, filesSearchOption);

            int fileCount = 0;
            //get the total count of files asynchronously so we can start collecting metrics using the enumerator
            var fileCountTask = new Task(() =>
            {
                var getFiles = Helpers.GetFiles(dir, searchRegex, filesSearchOption);
                fileCount = getFiles.Length;
            });
            fileCountTask.Start();

            List<MetricDict> metricsPerGenerator = new List<MetricDict>();

            int currentFile = 0;
            foreach (var gen in generators) //should i make this a parallel foreach too? most of it is IO heavy, so only gain would be for saving and loading metrics
            {
                MetricDict fileMetrics = LoadMetrics(dir, gen.ID);
                if (fileMetrics == null)
                {
                    fileMetrics = new MetricDict();
                    Parallel.ForEach(files, new Action<FileInfo>((f) =>
                    {
                        fileMetrics[f] = gen.Generate(f);
                        currentFile++;
                        OnProgress?.Invoke(new FractionalProgress(currentFile, fileCount * generators.Length, f.FullName));
                    }));

                    SaveMetrics(fileMetrics, dir, gen.ID);
                }
                metricsPerGenerator.Add(fileMetrics);
            }

            DuplicateFinderResult dfr = new DuplicateFinderResult();

            for (int i = 0; i < generators.Length; i++)
            {
                Dictionary<AMetric, DupeFileCollection> filesByMetrics = new Dictionary<AMetric, DupeFileCollection>();
                foreach (var kv in metricsPerGenerator[i]) //running over metrics of each generator
                {
                    if (kv.Value == null)
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
