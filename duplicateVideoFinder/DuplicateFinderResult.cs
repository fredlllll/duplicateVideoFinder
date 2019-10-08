using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    public class DuplicateFinderResult
    {
        public List<List<FileInfo>>[] dupeListsByGenerator;

        public DuplicateFinderResult(int generatorCount)
        {
            dupeListsByGenerator = new List<List<FileInfo>>[generatorCount];
            for(int i =0; i< dupeListsByGenerator.Length; i++)
            {
                dupeListsByGenerator[i] = new List<List<FileInfo>>();
            }
        }
    }
}
