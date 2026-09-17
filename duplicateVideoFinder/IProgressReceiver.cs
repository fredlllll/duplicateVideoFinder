using duplicateVideoFinder.Progresses;
using System;

namespace duplicateVideoFinder
{
    public interface IProgressReceiver
    {
        void Update(IProgress progress);
    }
}
