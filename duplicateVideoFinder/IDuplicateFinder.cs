using duplicateVideoFinder.Progresses;
using System.IO;

namespace duplicateVideoFinder
{
    public interface IDuplicateFinder
    {
        public delegate void ProgressHandler(IProgress progress);
        event ProgressHandler OnProgress;

        DuplicateFinderResult FindDuplicates(DirectoryInfo dir, bool topDirOnly = false);
    }
}
