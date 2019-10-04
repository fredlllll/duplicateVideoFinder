using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder.Progresses
{
    public class FractionalProgress : IProgress
    {
        int finished;
        int total;
        string status;

        public float Progress => total > 0 ? ((float)finished) / total : 0;

        public string Status => status;

        public FractionalProgress(int finished, int total, string status)
        {
            this.finished = finished;
            this.total = total;
            this.status = status;
        }

        public override string ToString()
        {
            return "(" + finished + "/" + (total > 0 ? total.ToString() : "?") + ")[" + (Progress * 100).ToString("0.00") + "%]: " + status;
        }
    }
}
