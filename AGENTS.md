# AGENTS.md

## Project Overview

C# .NET duplicate video finder with three projects (all modernized to .NET 10):
- `duplicateVideoFinder/` - Core library (net10.0)
- `duplicateVideoFinderConsole/` - CLI tool (net10.0)
- `duplicateVideoFinderWindowsGUI/` - WinForms GUI (net10.0-windows)

## Build & Run

```bash
dotnet build
dotnet run --project duplicateVideoFinderConsole <folder_path>
```

## Architecture

Metric-based duplicate detection:
- `AMetricGenerator` implementations produce metrics from video files
- `MetricDict` (ConcurrentDictionary) maps FileInfo → AMetric
- `PotentialDuplicateFinder.FindDupes()` groups files with equal metrics
- `MetricCache` persists computed metrics to a SQLite database (EF Core 10)

Available generators:
- `HashMetricGenerator` - MD5 hash of file samples (beginning/middle/end for large files; `ReadExactly` so short reads can't yield bad hashes)
- `DurationMetricGenerator` - FFmpeg-based video duration (requires Xabe.FFmpeg); reads ffprobe `duration` (seconds), not `duration_ts`; returns null on unreadable files instead of throwing

## Metric Cache (SQLite / EF Core)

- SQLite DB lives at `{scannedFolder}/.dvf/metrics.db` (one file per scanned folder)
- EF Core 10 via `MetricCacheDbContext` (table: `Entries`), schema managed with **EF Core migrations** (in `Migrations/`); `db.Database.Migrate()` is auto-applied on every cache open via `MetricCache.EnsureMigrated`
- `Pooling=False` is in the SQLite connection string so a disposed context never locks the file (Pooling would keep it open and break the drop-and-recreate path on Windows)
- Pre-migrations DBs (created by the old `EnsureCreated` build: `Entries` without `__EFMigrationsHistory`) can't be upgraded in place — they are dropped and recreated by `Migrate()`; a full rescan is the safe fallback
- **Per-file incremental invalidation**: every cache row stores that file's own `FileLength` + `LastWriteUtcTicks` fingerprint; on rescan only new/changed files are recomputed, all other files reuse their cached metric
- `MetricCache.LoadMetrics(dir, genId, currentFiles)` returns a `MetricCacheLoadResult` with `Reusable` (valid cached metrics) and `FilesToCompute` (the delta)
- `MetricCache.SaveMetrics(dir, genId, currentFiles, computed)` upserts only the freshly computed rows and prunes rows for files that no longer exist
- `MetricCache.DeleteCache(dir, genId)` removes only the given generator's rows
- A failed cache read/save is silently swallowed — the app just rescans; never blocks on cache errors
- Metric types are resolved by full name (`HashMetric`/`DurationMetric`); unknown types are skipped on load
- Both the GUI and the console go through `DuplicateFinder` (console no longer bypasses the cache), so repeated runs reuse cached metrics

## Automatic Keep Selection

`DuplicateKeeper.GetFileToKeep(DupeFileCollection)` picks the single best file to keep:
- Non-binary duplicates (e.g. `DurationMetric`): highest bitrate (file size / duration) wins
- Binary duplicates (`HashMetric`): files outside `UNSORTED` folders win, then title-like filenames over ID-like ones (GUIDs, md5 digests, pure numbers, camera `IMG_/DSC_/VID_` names)

Wired into both UIs:
- GUI `FrmSelectFilesToKeep` walks **all** generators' duplicate groups in order (not just the first); highlights the recommended file (light-blue row, tooltip); "Select Best" checks only that one; "Delete Unselected and Next" deletes the rest off the UI thread
- Console autosort deletes duplicate files that live in `UNSORTED` folders unless they are the keeper
- Console fails gracefully when stdout is redirected (no `Console.Clear()`/cursor hacks in CI)

## Configuration

- `settings.json` defines the `extensionsToProcess` video extensions; loaded via `AppSettings.Instance` singleton
- Settings are looked up in the **executable directory** first, then the working directory
- Missing settings.json OR an empty `extensionsToProcess` array now **throws** (fail loudly) instead of silently scanning all files

## Key Files

- `FolderMetricGenerator.cs` - Orchestrates parallel metric computation (atomic progress counter; overload accepting a specific file list; per-file try/catch so one bad file can't abort a scan)
- `PotentialDuplicateFinder.cs` - Groups files by metric equality
- `MetricCache.cs` - EF Core SQLite persistence + per-file incremental invalidation
- `HashMetricGenerator.cs` - Samples 256 bytes from file start, middle, and end
- `DuplicateKeeper.cs` - Automatic "which file to keep" scoring
- `FrmSelectFilesToKeep.cs` - GUI review flow across all generators (batch-token protects against stale thumbnail races)

## Gotchas

- `DurationMetricGenerator` depends on FFmpeg being available. GUI downloads it at startup **in the background**
  (duration checkbox stays disabled until ready — only enabled on success); console needs FFmpeg in PATH
- `DurationMetric` stores ffprobe `duration` in **seconds** (bitrate = bytes*8/seconds)
- `DuplicateKeeper.FilenameQuality` is a rough heuristic; real titles vs IDs can be ambiguous
- Console's autosort only deletes duplicates under paths containing `UNSORTED` (case-insensitive, matching the keeper);
  it never deletes the keeper file; a locked file is counted as failed, not fatal
- Metric metrics are stored as JSON text columns; a metric class rename makes old cache rows unreadable (they're skipped)
- No test suite exists