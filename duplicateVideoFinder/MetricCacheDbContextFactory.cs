using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace duplicateVideoFinder
{
    public class MetricCacheDbContextFactory : IDesignTimeDbContextFactory<MetricCacheDbContext>
    {
        public MetricCacheDbContext CreateDbContext(string[] args)
        {
            string path = Path.Combine(Path.GetTempPath(), ".dvf-design", "metrics.db");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return new MetricCacheDbContext(path);
        }
    }
}