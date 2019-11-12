using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    public class DuplicateFinderResult
    {
        public List<DupeCollection>[] dupeListsByGenerator;

        public DuplicateFinderResult(int generatorCount)
        {
            dupeListsByGenerator = new List<DupeCollection>[generatorCount];
            for(int i =0; i< dupeListsByGenerator.Length; i++)
            {
                dupeListsByGenerator[i] = new List<DupeCollection>();
            }
        }
    }
}
