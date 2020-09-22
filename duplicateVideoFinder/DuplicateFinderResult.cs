using System.Collections.Generic;
using System.IO;

namespace duplicateVideoFinder
{
    public class DuplicateFinderResult
    {
        public Dictionary<string, DupeCollection> dupesByGenerator = new Dictionary<string, DupeCollection>();
    }
}
