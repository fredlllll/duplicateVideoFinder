using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder
{
    public class AppSettings : Settings
    {
        public static readonly AppSettings Instance = new AppSettings();

        public readonly string[] extensionsToProcess;

        public AppSettings() : base()
        {
            //extensions to process
            JArray extensions = Data["extensionsToProcess"] as JArray;
            if (extensions != null && extensions.Count > 0)
            {
                extensionsToProcess = new string[extensions.Count];
                for (int i = 0; i < extensionsToProcess.Length; i++)
                {
                    extensionsToProcess[i] = extensions[i].Value<string>();
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "settings.json does not define a non-empty 'extensionsToProcess' array. Add the video extensions you want to scan.");
            }
        }
    }
}