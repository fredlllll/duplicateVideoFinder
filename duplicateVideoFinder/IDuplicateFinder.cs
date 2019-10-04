using duplicateVideoFinder.Progresses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace duplicateVideoFinder
{
    public interface IDuplicateFinder
    {
        public delegate void ProgressHandler(IProgress progress);
        event ProgressHandler OnProgress;

        DuplicateCollection FindDuplicates(DirectoryInfo directory);
    }
}
