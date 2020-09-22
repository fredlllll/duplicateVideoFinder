using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.Text;

namespace duplicateVideoFinder
{
    public interface IProgressReceiver
    {
        void Update(IProgress progress);
    }
}
