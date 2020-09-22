using duplicateVideoFinder.Metrics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace duplicateVideoFinder
{
    public static class MetricCache
    {
        private static string GetMetricsFileName(DirectoryInfo directory, string id)
        {
            using var md5 = MD5.Create();
            var dirNameHashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(directory.FullName));
            string dirNameHash = BitConverter.ToString(dirNameHashBytes).Replace("-", string.Empty);
            return dirNameHash + "_" + id + "_metrics.json";
        }

        public static void ClearMetrics(DirectoryInfo directory, string genId)
        {
            string filename = GetMetricsFileName(directory, genId);
            File.Delete(filename);
        }

        public static void SaveMetrics(MetricDict dict, DirectoryInfo directory, string genId)
        {
            string filename = GetMetricsFileName(directory, genId);

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

        public static MetricDict LoadMetrics(DirectoryInfo directory, string genId)
        {
            string filename = GetMetricsFileName(directory, genId);
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
    }
}
