using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder.Progresses
{
    public class BasicProgress : IProgress
    {
        float progress;
        string status;
        public float Progress => progress;

        public string Status => status;

        public BasicProgress(float progress, string status)
        {
            this.progress = progress;
            this.status = status;
        }

        public override string ToString()
        {
            return "[" + (progress * 100).ToString("0.00") + "%]: " + status;
        }
    }
}
