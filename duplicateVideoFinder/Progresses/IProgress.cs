using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder.Progresses
{
    public interface IProgress
    {
        float Progress
        {
            get;
        }

        string Status
        {
            get;
        }
    }
}
