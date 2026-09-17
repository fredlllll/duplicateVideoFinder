using duplicateVideoFinder.Metrics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace duplicateVideoFinder
{
    public class MetricCacheEntry
    {
        public int Id { get; set; }
        public string DirectoryPath { get; set; }
        public string GeneratorId { get; set; }
        public string FilePath { get; set; }
        public long FileLength { get; set; }
        public long LastWriteUtcTicks { get; set; }
        public string MetricType { get; set; }
        public string MetricJson { get; set; }
    }

    public class MetricCacheDbContext : DbContext
    {
        public DbSet<MetricCacheEntry> Entries { get; set; }

        private readonly string dbPath;

        public MetricCacheDbContext(string dbPath)
        {
            this.dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + dbPath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetricCacheEntry>()
                .HasIndex(e => new { e.DirectoryPath, e.GeneratorId, e.FilePath })
                .IsUnique();
        }
    }

    public class MetricCacheLoadResult
    {
        public MetricDict Reusable { get; } = new MetricDict();
        public List<FileInfo> FilesToCompute { get; } = new List<FileInfo>();
    }

    public static class MetricCache
    {
        private const string CacheFolderName = ".dvf";
        private const string DbFileName = "metrics.db";

        private static string GetDbDirectory(DirectoryInfo directory)
        {
            return Path.Combine(directory.FullName, CacheFolderName);
        }

        private static string GetDbPath(DirectoryInfo directory)
        {
            return Path.Combine(GetDbDirectory(directory), DbFileName);
        }

        private static Type ResolveMetricType(string typeName)
        {
            if (typeof(HashMetric).FullName == typeName)
            {
                return typeof(HashMetric);
            }
            if (typeof(DurationMetric).FullName == typeName)
            {
                return typeof(DurationMetric);
            }
            return null;
        }

        // Old cache files (directory-fingerprint schema) have Entries rows without
        // per-file FileLength/LastWriteUtcTicks. Detect and drop them so the fresh
        // schema can be created; a full rescan is the safe fallback.
        private static bool EnsureSchemaUsable(string dbPath)
        {
            try
            {
                using var db = new MetricCacheDbContext(dbPath);
                string[] columns;
                string checkSql = "PRAGMA table_info('Entries')";
                using (var cmd = db.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = checkSql;
                    db.Database.OpenConnection();
                    using (var reader = cmd.ExecuteReader())
                    {
                        var cols = new List<string>();
                        while (reader.Read())
                        {
                            cols.Add(reader.GetString(1));
                        }
                        columns = cols.ToArray();
                    }
                }
                bool usable = columns.Contains("FileLength") && columns.Contains("LastWriteUtcTicks");
                if (!usable)
                {
                    File.Delete(dbPath);
                }
                return usable;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Splits the current files into ones whose cached metric is still valid
        /// (same length + last-write time) and ones that need recomputing.
        /// Only the delta must be recomputed, so a single added video does not
        /// invalidate the whole cache.
        /// </summary>
        public static MetricCacheLoadResult LoadMetrics(DirectoryInfo directory, string genId, IEnumerable<FileInfo> currentFiles)
        {
            var result = new MetricCacheLoadResult();
            string dbPath = GetDbPath(directory);
            if (!File.Exists(dbPath))
            {
                result.FilesToCompute.AddRange(currentFiles);
                return result;
            }
            if (!EnsureSchemaUsable(dbPath))
            {
                result.FilesToCompute.AddRange(currentFiles);
                return result;
            }

            try
            {
                using var db = new MetricCacheDbContext(dbPath);
                db.Database.EnsureCreated();
                var rows = db.Entries.AsNoTracking()
                    .Where(e => e.DirectoryPath == directory.FullName && e.GeneratorId == genId)
                    .ToList();
                var byPath = rows.ToDictionary(r => r.FilePath, StringComparer.OrdinalIgnoreCase);

                foreach (var f in currentFiles)
                {
                    if (byPath.TryGetValue(f.FullName, out var row)
                        && row.FileLength == f.Length
                        && row.LastWriteUtcTicks == f.LastWriteTimeUtc.Ticks)
                    {
                        var t = ResolveMetricType(row.MetricType);
                        if (t != null)
                        {
                            try
                            {
                                var metric = JObject.Parse(row.MetricJson).ToObject(t) as AMetric;
                                if (metric != null)
                                {
                                    result.Reusable[f] = metric;
                                    continue;
                                }
                            }
                            catch
                            {
                                // fall through to recompute
                            }
                        }
                    }
                    result.FilesToCompute.Add(f);
                }
            }
            catch
            {
                // corrupt/old cache: recompute everything
                result.Reusable.Clear();
                result.FilesToCompute.Clear();
                result.FilesToCompute.AddRange(currentFiles);
            }
            return result;
        }

        /// <summary>
        /// Prunes rows for files that no longer exist and upserts only the
        /// freshly computed ones. Unchanged files keep their existing rows.
        /// </summary>
        public static void SaveMetrics(
            DirectoryInfo directory,
            string genId,
            IReadOnlyCollection<FileInfo> currentFiles,
            MetricDict computed)
        {
            try
            {
                string dbDir = GetDbDirectory(directory);
                Directory.CreateDirectory(dbDir);
                using var db = new MetricCacheDbContext(GetDbPath(directory));
                db.Database.EnsureCreated();

                var rows = db.Entries
                    .Where(e => e.DirectoryPath == directory.FullName && e.GeneratorId == genId)
                    .ToList();
                var byPath = rows.ToDictionary(r => r.FilePath, StringComparer.OrdinalIgnoreCase);

                var currentPaths = currentFiles.Select(f => f.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // remove rows for files that are no longer present
                foreach (var row in rows)
                {
                    if (!currentPaths.Contains(row.FilePath))
                    {
                        db.Entries.Remove(row);
                    }
                }

                // upsert the rows for files computed in this run
                foreach (var kv in computed)
                {
                    var f = kv.Key;
                    if (byPath.TryGetValue(f.FullName, out var existing))
                    {
                        existing.FileLength = f.Length;
                        existing.LastWriteUtcTicks = f.LastWriteTimeUtc.Ticks;
                        existing.MetricType = kv.Value.GetType().FullName;
                        existing.MetricJson = JObject.FromObject(kv.Value).ToString();
                    }
                    else
                    {
                        db.Entries.Add(new MetricCacheEntry
                        {
                            DirectoryPath = directory.FullName,
                            GeneratorId = genId,
                            FilePath = f.FullName,
                            FileLength = f.Length,
                            LastWriteUtcTicks = f.LastWriteTimeUtc.Ticks,
                            MetricType = kv.Value.GetType().FullName,
                            MetricJson = JObject.FromObject(kv.Value).ToString()
                        });
                    }
                }

                db.SaveChanges();
            }
            catch
            {
                // cache is best-effort; a failing save shouldn't break the app
            }
        }

        public static void DeleteCache(DirectoryInfo directory, string genId)
        {
            try
            {
                string dbPath = GetDbPath(directory);
                if (!File.Exists(dbPath))
                {
                    return;
                }
                using var db = new MetricCacheDbContext(dbPath);
                db.Database.EnsureCreated();
                var rows = db.Entries
                    .Where(e => e.DirectoryPath == directory.FullName && e.GeneratorId == genId)
                    .ToList();
                db.Entries.RemoveRange(rows);
                db.SaveChanges();
            }
            catch
            {
                // cache is best-effort; a failing cleanup shouldn't break the app
            }
        }
    }
}