// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Standalone runner for the go2cs performance comparison suite. For each benchmark project under
// src/tests/Performance it builds three variants of the same program -- the original Go binary, the
// transpiled C# on the normal JIT runtime, and the transpiled C# as a Native AOT self-contained
// executable -- verifies all three produce identical output (checksums), then measures workload time
// (in-program, excludes startup), process wall time, and peak working set, reducing the samples to a
// markdown report. Phases: Transpile -> Build -> Verify -> Measure.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using PerformanceRunner;

return Runner.Main(args);

namespace PerformanceRunner
{
    internal enum Phase { Transpile, Build, Verify, Measure }

    internal enum Variant { Go, Jit, Aot }

    internal readonly record struct RunSample(double WallMs, double InnerMs, long PeakBytes);

    internal sealed class VariantResult
    {
        public bool BuildOk { get; set; }
        public bool VerifyOk { get; set; }
        public List<RunSample> Samples { get; } = new();
        public string? Message { get; set; }
    }

    internal sealed class ProjectResult
    {
        public required string Name { get; init; }
        public Dictionary<Variant, VariantResult> Variants { get; } = new()
        {
            [Variant.Go] = new VariantResult(),
            [Variant.Jit] = new VariantResult(),
            [Variant.Aot] = new VariantResult()
        };

        public List<string> Messages { get; } = new();
        public bool Failed { get; set; }
    }

    internal static class Runner
    {
        // Timeouts are SAFETY NETS against a hung child, not performance assumptions -- size them
        // for the slowest host this suite legitimately runs on, never from one machine's measured
        // time. A publish that legitimately outlives its timeout is killed and reported as a
        // failed COLUMN ("n/a"), which reads exactly like a real AOT defect: at 600s an i7-5820K
        // (2014 Haswell-E) timed out on PerfStartup -- the SMALLEST benchmark -- where an
        // i9-13900K published in seconds; the same publish then completed in 1,574s once given
        // room. ILC compiles the full converted-stdlib closure per benchmark since the trees
        // unified (2026-08-01), so a slow host legitimately spends tens of minutes per publish
        // (2026-08-10; the same shape as the -test-timeout lesson in CLAUDE.md, where both
        // sides' hidden 10-minute defaults faked a failing tail).
        private const int TranspileTimeoutMs = 60_000;
        private const int GoBuildTimeoutMs = 300_000;
        private const int BuildAllTimeoutMs = 600_000;
        private const int BuildOneTimeoutMs = 300_000;
        // 12 hours since the .NET 10 hop, and sized from MEASUREMENT rather than hope: the first
        // COMPLETED 10-ILC publish measured 11,862s (~3.3h) on the perf-canon laptop — so the 4h
        // interim value this moved to first held only ~20% headroom on the FASTEST measuring box,
        // and the slowest-legitimate-host doctrine above prices an i7-5820K-class publish at
        // roughly 8-10h. Both prior values died the same death this constant's 2026-08-10 comment
        // predicted: the 60-minute value killed PerfStartup mid-compile on the canon laptop, and
        // the i9 sweeper then reproduced the identical false red at exactly 3,600s (kill within
        // 5s of the constant, ILC verifiably healthy at ~1.3 cores throughout) — a watchdog that
        // quietly became a performance assumption the moment the toolchain under it changed.
        // Overridable via GO2CS_AOT_PUBLISH_TIMEOUT (seconds) — the behavioral runner's pattern —
        // so a fast lane opts DOWN for fail-fast, and a still-slower host opts up, without an
        // instrument edit. At 12h this only ever fires on a genuine hang, which is the only thing
        // a safety net is for.
        private static readonly int AotPublishTimeoutMs =
            int.TryParse(Environment.GetEnvironmentVariable("GO2CS_AOT_PUBLISH_TIMEOUT"), out int aotSeconds) && aotSeconds > 0
                ? aotSeconds * 1000
                : 43_200_000;
        private const int RunTimeoutMs = 120_000;

        private const string Config = "Release";
        // DERIVED from this runner's own bin tail, never spelled: the executable lives at
        // .../bin/<config>/<tfm>/, so the last segment of its base directory IS the TFM it was
        // built for -- the BehavioralTestBase pattern (CENSUS-tfm-inventory.md Class D), which is
        // what makes this the harness a TFM hop does not touch. A hardcoded "net9.0" here was a
        // FALSE-RED generator: after a hop the build succeeds, this probe misses the new folder,
        // and the runner reports hundreds of corpus failures on a green tree.
        private static readonly string NetVersion =
            AppContext.BaseDirectory.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries)[^1];

        // Executable suffix for a built .NET apphost or Go binary. Windows only; empty everywhere
        // else. Hard-coding ".exe" made every File.Exists probe in Verify/Measure fail off Windows,
        // which this runner reports as "exe missing" -- a per-variant message, not a failure, so a
        // run would complete and publish a results table measured from nothing (F4,
        // docs/PLAN-linux-operation.md).
        private static readonly string s_exeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";

        // Preferred report order; projects not listed are appended alphabetically. Related
        // benchmarks are grouped (compute, strings, containers, interfaces), matching the
        // benchmark-description table in README.md.
        private static readonly string[] s_reportOrder =
        {
            "PerfStartup", "PerfFib", "PerfSieve", "PerfMatMul",
            "PerfString", "PerfStringView", "PerfStringMatch",
            "PerfMap", "PerfSort", "PerfChannel",
            "PerfIfaceCall", "PerfIface", "PerfIfaceShell", "PerfIfaceShellChurn"
        };

        private static string s_srcRoot = null!;
        private static string s_perfDir = null!;
        private static string s_converterSrc = null!;
        private static string s_go2csExe = null!;

        public static int Main(string[] args)
        {
            // The report uses non-ASCII glyphs (× ratios, · separators); default console codepage mangles them.
            Console.OutputEncoding = Encoding.UTF8;

            // ----- argument parsing -----
            string? filter = null;
            bool listOnly = false;
            bool noAot = false;
            bool updateReadme = false;
            int runs = 5;
            HashSet<Phase> phases = new() { Phase.Transpile, Phase.Build, Phase.Verify, Phase.Measure };

            // Release legs (ReleaseRun.cs): set by --leg, and then the in-tree phases do not run.
            List<ReleaseLeg> legs = new();
            string? workRoot = null, aotManifest = null, aotDrop = null;
            int reviewRuns = 11;
            bool tc0 = false, fd1 = false;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                switch (a)
                {
                    case "--filter" when i + 1 < args.Length:
                        filter = args[++i];
                        break;
                    case "--leg" when i + 1 < args.Length:
                        try
                        {
                            legs.Add(ReleaseLeg.Parse(args[++i]));
                        }
                        catch (FormatException ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                            return 2;
                        }
                        break;
                    case "--work" when i + 1 < args.Length:
                        workRoot = Path.GetFullPath(args[++i]);
                        break;
                    case "--aot-manifest" when i + 1 < args.Length:
                        aotManifest = Path.GetFullPath(args[++i]);
                        break;
                    case "--aot-drop" when i + 1 < args.Length:
                        aotDrop = Path.GetFullPath(args[++i]);
                        break;
                    case "--review-runs" when i + 1 < args.Length:
                        if (!int.TryParse(args[++i], out reviewRuns) || reviewRuns < 1)
                        {
                            Console.Error.WriteLine("--review-runs requires a positive integer");
                            return 2;
                        }
                        break;
                    case "--tc0":
                        tc0 = true;
                        break;
                    case "--fd1":
                        fd1 = true;
                        break;
                    case "--phase" when i + 1 < args.Length:
                        phases = ParsePhases(args[++i]);
                        break;
                    case "--runs" when i + 1 < args.Length:
                        if (!int.TryParse(args[++i], out runs) || runs < 1)
                        {
                            Console.Error.WriteLine("--runs requires a positive integer");
                            return 2;
                        }
                        break;
                    case "--no-aot":
                        noAot = true;
                        break;
                    case "--update-readme":
                        updateReadme = true;
                        break;
                    case "--list":
                        listOnly = true;
                        break;
                    case "--help" or "-h":
                        PrintUsage();
                        return 0;
                    default:
                        Console.Error.WriteLine($"Unknown argument: {a}");
                        PrintUsage();
                        return 2;
                }
            }

            // ----- resolve paths -----
            // Runner lives at src\tests\Performance\PerformanceRunner; performance dir is its parent.
            // Path.Combine with SEGMENTS, not an embedded @"..\..\..\..": .NET does not normalize a
            // backslash on Unix, so that string is ONE directory name there and GetFullPath yields a
            // path that exists nowhere -- discovery then finds zero benchmarks. Identical on Windows.
            s_perfDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            s_srcRoot = Path.GetFullPath(Path.Combine(s_perfDir, "..", ".."));
            s_converterSrc = Path.Combine(s_srcRoot, "go2cs");
            s_go2csExe = Path.Combine(s_converterSrc, "bin", $"go2cs{s_exeSuffix}");

            // ----- discover projects -----
            // A benchmark project is a folder with Go source; the generated .csproj appears after the
            // first transpile. This naturally excludes this runner utility (no .go).
            List<string> projects = Directory.GetDirectories(s_perfDir)
                .Where(d => Directory.GetFiles(d, "*.go").Length > 0)
                .Select(Path.GetFileName)
                .Where(n => filter is null || n!.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(ReportIndex)
                .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            if (listOnly)
            {
                foreach (string p in projects)
                    Console.WriteLine(p);
                Console.WriteLine($"({projects.Count} projects)");
                return 0;
            }

            if (projects.Count == 0)
            {
                Console.Error.WriteLine("No benchmark projects matched.");
                return 2;
            }

            if (legs.Count > 0)
            {
                if (workRoot is null)
                {
                    Console.Error.WriteLine("--leg needs --work <dir>: a release leg converts into a scratch tree, never into this suite's projects");
                    return 2;
                }

                if (legs.Select(l => l.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != legs.Count)
                {
                    Console.Error.WriteLine("two --leg values share a name");
                    return 2;
                }

                if ((aotManifest is null) != (aotDrop is null))
                {
                    Console.Error.WriteLine("--aot-manifest and --aot-drop go together: a shipped binary is admitted only by its manifest");
                    return 2;
                }

                return ReleaseRun.Run(s_perfDir, projects, new ReleaseRun.Options(legs, workRoot, aotManifest, aotDrop, runs, reviewRuns, tc0, fd1));
            }

            Console.WriteLine($"go2cs performance runner: {projects.Count} project(s), phases [{string.Join(", ", phases)}], {runs} measured run(s){(noAot ? ", AOT disabled" : "")}");
            Stopwatch total = Stopwatch.StartNew();

            Dictionary<string, ProjectResult> results = projects.ToDictionary(p => p, p => new ProjectResult { Name = p });

            // ----- Phase: Transpile -----
            if (phases.Contains(Phase.Transpile))
            {
                if (!EnsureConverterBuilt())
                    return 1;

                RunTranspile(projects, results);
            }

            // ----- Phase: Build (Go binary + C# JIT + C# Native AOT) -----
            if (phases.Contains(Phase.Build))
            {
                RunBuildGo(projects, results);
                RunBuildJit(projects, results);

                if (!noAot)
                    RunBuildAot(projects, results);
            }
            else
            {
                // Assume prior build outputs exist; verified per exe below.
                foreach (ProjectResult r in results.Values)
                foreach (VariantResult v in r.Variants.Values)
                    v.BuildOk = true;
            }

            // ----- Measurement configuration guard (before anything is compared or timed) -----
            if (phases.Contains(Phase.Verify) || phases.Contains(Phase.Measure))
                CheckJitConfiguration(projects, results);

            // ----- Phase: Verify (identical output across variants) -----
            if (phases.Contains(Phase.Verify))
                RunVerify(projects, results, noAot);

            // ----- Phase: Measure -----
            if (phases.Contains(Phase.Measure))
            {
                RunMeasure(projects, results, runs, noAot);

                string markdown = BuildMarkdown(projects, results, runs, noAot);
                Console.WriteLine();
                Console.WriteLine(markdown);

                // Never publish a table produced by a failing run -- a rejected measurement
                // configuration or a mismatched checksum must not reach README.md.
                if (updateReadme && results.Values.Any(r => r.Failed))
                    Console.Error.WriteLine("README not updated: the run has failing project(s).");
                else if (updateReadme && !UpdateReadme(markdown))
                    return 1;
            }

            total.Stop();

            // ----- summary -----
            List<ProjectResult> failures = results.Values.Where(r => r.Failed).ToList();

            if (failures.Count > 0)
            {
                Console.WriteLine($"---- {failures.Count} failing project(s) ----");

                foreach (ProjectResult r in failures)
                {
                    Console.WriteLine($"  {r.Name}");
                    foreach (string m in r.Messages)
                        Console.WriteLine($"      {m}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{(failures.Count == 0 ? "PASS" : "FAIL")}  ({projects.Count} projects, {total.Elapsed.TotalSeconds:N1}s)");
            return failures.Count == 0 ? 0 : 1;
        }

        private static int ReportIndex(string? name)
        {
            int idx = Array.IndexOf(s_reportOrder, name);
            return idx < 0 ? int.MaxValue : idx;
        }

        // ---- Phase: Transpile ----

        // As in BehavioralRunner: the staleness question is answered by the SHARED
        // ConverterBuildInputs (src/tests) rather than by a local top-level *.go enumeration, which
        // could not see an embedded template or a converter internal/ package changing and so let
        // every phase below measure the PREVIOUS emission -- false-green route #5 in CLAUDE.md.
        private static bool EnsureConverterBuilt()
        {
            if (!ConverterBuildInputs.IsConverterStale(s_converterSrc, s_go2csExe))
                return true;

            Console.WriteLine("Building go2cs.exe (converter sources changed)...");
            ProcResult r = Exec("go", $"build -o \"{s_go2csExe}\"", s_converterSrc, GoBuildTimeoutMs);

            if (r.ExitCode != 0)
            {
                Console.Error.WriteLine($"go build of converter failed ({r.ExitCode}):\n{r.StdErr}");
                return false;
            }

            return true;
        }

        // As in BehavioralRunner: the converter gets an EXPLICIT -go2cspath (s_srcRoot, derived from this
        // runner's own location) instead of inheriting the ambient GO2CSPATH. That flag -- not the MSBuild
        // $(go2csPath) property of the same name -- is the root the converter reads an imported package's
        // package_info.cs from when it mints the emitted <ImportedTypeAliases> block, and its default
        // (~/go2cs) is absent or stale on most boxes, so an inherited value makes the transpiled C# vary
        // with the launching shell (BOARD-next-validation-candidates.md, 2026-08-06). The benchmark
        // projects compile against src\core through MSBuild $(go2csPath) -> $(SolutionDir), so src\ is the
        // root whose metadata describes what they link.
        private static void RunTranspile(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Transpile] {projects.Count} project(s)... ");
            int failed = 0;

            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_perfDir, p);

                if (UpToDate(projPath))
                    continue;

                ProcResult r = Exec(s_go2csExe, $"-go2cspath \"{s_srcRoot}\" \"{projPath}\"", projPath, TranspileTimeoutMs);

                if (r.ExitCode != 0)
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"transpile exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        // A project is up to date when every .cs is newer than BOTH its matching .go source and the
        // converter binary that produced it. Omitting the converter would leave every benchmark "up to
        // date" after a converter-only change (the .go files don't move), so Verify/Measure would run
        // the PREVIOUS converter's C# -- a false green.
        private static bool UpToDate(string projPath)
        {
            DateTime exe = File.GetLastWriteTimeUtc(s_go2csExe);

            foreach (string go in Directory.GetFiles(projPath, "*.go"))
            {
                string cs = Path.ChangeExtension(go, ".cs");

                if (!File.Exists(cs))
                    return false;

                DateTime csTime = File.GetLastWriteTimeUtc(cs);

                if (csTime <= File.GetLastWriteTimeUtc(go) || csTime <= exe)
                    return false;
            }

            return true;
        }

        // ---- Phase: Build ----

        private static void RunBuildGo(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Build]    Go binaries... ");
            int failed = 0;

            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_perfDir, p);
                string goExe = GetExePath(p, Variant.Go);
                Directory.CreateDirectory(Path.GetDirectoryName(goExe)!);

                ProcResult r = Exec("go", $"build -o \"{goExe}\" .", projPath, GoBuildTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Go].BuildOk = true;
                }
                else
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"go build exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunBuildJit(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = Go2csPathArg();

            // Pre-build the shared dependencies (golib, the go2cs-gen analyzer, and the core/* packages
            // the targets reference) sequentially first, so the parallel target fan-out never races on
            // their obj/bin outputs (same MSB3026/27 mitigation as the BehavioralRunner).
            PreBuildSharedDeps(projects, go2csPathArg);

            Console.Write($"[Build]    C# JIT (one-shot parallel build of {projects.Count})... ");

            string traversal = WriteTraversalProject(projects);

            ProcResult all = Exec("dotnet",
                $"build \"{traversal}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                s_perfDir, BuildAllTimeoutMs);

            if (all.ExitCode == 0)
            {
                foreach (string p in projects)
                    results[p].Variants[Variant.Jit].BuildOk = true;

                Console.WriteLine("ok");
                return;
            }

            // Build-all failed: fall back to per-project builds to attribute the failure(s).
            Console.WriteLine("build-all reported errors; attributing per project...");

            int failed = 0;

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");

                ProcResult r = Exec("dotnet",
                    $"build \"{csproj}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_perfDir, BuildOneTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Jit].BuildOk = true;
                }
                else
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"C# build exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine($"[Build]    C# JIT per-project: {failed} failed");
        }

        private static void RunBuildAot(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = Go2csPathArg();

            // The ILC native link step probes for MSVC link.exe via vswhere.exe and assumes it is on
            // PATH; prepend the VS Installer directory so the probe resolves cleanly.
            string vsInstaller = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer");

            // MSBUILDDISABLENODEREUSE: the PATH prepend below reaches only nodes THIS publish
            // spawns. Without it, dotnet publish hands the ILC targets to any idle MSBuild worker
            // node machine-wide -- including nodes another session started with an environment
            // lacking the VS Installer dir -- and the linker probe's vswhere then fails INSIDE the
            // node, its cmd error text captured into the linker path and executed as a garbage
            // command (MSB3073 exit 123; observed 2026-08-10 with two sibling sessions building).
            Dictionary<string, string> env = new() { ["MSBUILDDISABLENODEREUSE"] = "1" };

            if (Directory.Exists(vsInstaller))
                env["PATH"] = vsInstaller + ";" + Environment.GetEnvironmentVariable("PATH");

            Console.WriteLine($"[Build]    C# Native AOT ({projects.Count} publish(es), sequential -- slow)...");

            // A publish costs hours under the 10-ILC (~3.3h each on the perf-canon host, measured
            // 2026-08-24), so a completed publish must be reusable across runner invocations. The
            // skip predicate has three legs, each closing a distinct staleness route: the output
            // exists; it is newer than every publish input (all .cs including Generated, the
            // csproj, and go2cs.exe -- same family as UpToDate above); and a stamp written at
            // publish success matches the CURRENT toolchain, because a toolchain hop moves no
            // source mtime at all -- reusing a 9-ILC binary for a 10-column is exactly the stale-
            // publish trap this predicate exists to retire. No stamp (every pre-stamp publish, and
            // any foreign binary placed without one) means publish, never skip. An unreadable SDK
            // version disables skipping for the whole run rather than matching stamps vacuously.
            ProcResult sdkProbe = Exec("dotnet", "--version", s_perfDir, 60_000, env);
            string sdkVersion = sdkProbe.ExitCode == 0 ? sdkProbe.StdOut.Trim() : "";
            string currentStamp = sdkVersion.Length > 0 ? $"sdk={sdkVersion};config={Config};rid=win-x64;mode=PerfAot" : "";

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");
                string outDir = Path.GetDirectoryName(GetExePath(p, Variant.Aot))!;
                string stampPath = Path.Combine(outDir, "publish.stamp");

                if (currentStamp.Length > 0 && AotPublishUpToDate(p, stampPath, currentStamp, out string upToDateWhy))
                {
                    results[p].Variants[Variant.Aot].BuildOk = true;
                    Console.WriteLine($"           {p}... SKIPPED (publish up to date: {upToDateWhy})");
                    continue;
                }

                Console.Write($"           {p}... ");
                Stopwatch sw = Stopwatch.StartNew();

                string publishArgs =
                    $"publish \"{csproj}\" -nologo -clp:ErrorsOnly -c {Config} -p:PerfAot=true -p:go2csPath={go2csPathArg} -o \"{outDir}\"";

                ProcResult r = Exec("dotnet", publishArgs, s_perfDir, AotPublishTimeoutMs, env);

                // A publish killed mid-ILC (a watchdog, a stopped run, a machine crash) leaves a
                // TRUNCATED native obj behind, and the next incremental publish sees unchanged
                // inputs, skips ILC, and hands the linker the poisoned file -- a fast LNK1106
                // ("cannot seek to ...") that reads like a toolchain defect and recurs forever
                // until the intermediate is deleted (observed 2026-08-10 after a killed run).
                // Self-heal: on any failure, drop this project's native intermediates and retry
                // once from clean. A retry that also fails is a real failure, reported with both
                // messages.
                if (r.ExitCode != 0)
                {
                    string nativeObj = Path.Combine(s_perfDir, p, "obj", "aot");
                    string firstError = Truncate(r.StdOut + r.StdErr);

                    if (Directory.Exists(nativeObj))
                        Directory.Delete(nativeObj, recursive: true);

                    Console.Write($"retrying from clean intermediates (first attempt: exit {r.ExitCode})... ");
                    r = Exec("dotnet", publishArgs, s_perfDir, AotPublishTimeoutMs, env);

                    if (r.ExitCode != 0)
                        results[p].Messages.Add($"AOT publish first attempt exit: {firstError}");
                }

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Aot].BuildOk = true;

                    if (currentStamp.Length > 0)
                        File.WriteAllText(stampPath, currentStamp);

                    Console.WriteLine($"ok ({sw.Elapsed.TotalSeconds:N0}s)");
                }
                else
                {
                    // An AOT failure degrades that column to n/a rather than failing the whole run;
                    // it is still reported.
                    results[p].Variants[Variant.Aot].Message = "publish failed";
                    results[p].Messages.Add($"AOT publish exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    Console.WriteLine("FAILED (column reported as n/a)");
                }
            }
        }

        private static bool AotPublishUpToDate(string p, string stampPath, string currentStamp, out string why)
        {
            why = "";
            string exe = GetExePath(p, Variant.Aot);

            if (!File.Exists(exe) || !File.Exists(stampPath))
                return false;

            if (!string.Equals(File.ReadAllText(stampPath).Trim(), currentStamp, StringComparison.Ordinal))
                return false;

            DateTime exeTime = File.GetLastWriteTimeUtc(exe);

            if (exeTime <= File.GetLastWriteTimeUtc(s_go2csExe))
                return false;

            string projPath = Path.Combine(s_perfDir, p);

            if (exeTime <= File.GetLastWriteTimeUtc(Path.Combine(projPath, $"{p}.csproj")))
                return false;

            foreach (string cs in Directory.GetFiles(projPath, "*.cs", SearchOption.AllDirectories))
            {
                // bin holds the publish outputs themselves (and obj the intermediates); neither is
                // an input, and the output can never be newer than a file the publish itself wrote.
                // Generated holds the source generators' EmitCompilerGeneratedFiles copies -- also
                // OUTPUT, refreshed by any compile of the project, so counting it as an input made
                // the predicate self-invalidating: the run's own JIT build phase re-emitted them
                // and every publish older than that build spuriously republished (measured
                // 2026-08-25, PerfIfaceCall -- a farm-adopted row re-paid 3.3h for a .g.cs whose
                // real inputs had not moved). The generators' true upstream is the analyzer
                // assembly, checked explicitly below.
                string rel = Path.GetRelativePath(projPath, cs);

                if (rel.StartsWith("bin", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("obj", StringComparison.OrdinalIgnoreCase) || rel.StartsWith("Generated", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (exeTime <= File.GetLastWriteTimeUtc(cs))
                    return false;
            }

            // Deliberately NOT an input: the go2cs-gen analyzer assembly. Its mtime is rewritten
            // by ordinary dependency builds of UNCHANGED sources (measured 2026-08-25: a
            // benchmark's publish re-touched bin/Release/.../go2cs-gen.dll while nothing in the
            // analyzer moved), so as a signal it is the same self-poisoning class as Generated.
            // The accepted gap: an analyzer-SOURCE change can alter emitted C# with no
            // project-local input moving -- such a change owes a manual bin purge (or a stamp
            // scheme bump) before the next measured run, the same discipline a converter change
            // already owes the corpus.
            why = $"output newer than all inputs, stamp matches [{currentStamp}]";
            return true;
        }

        // ---- Measurement configuration guard ----

        // The published JIT row must come from a framework-dependent, JIT-hosted binary with dynamic
        // code enabled -- what README.md says it is. Building one is not enough to have one: the Native
        // AOT publish's BUILD step copies through $(OutDir), which the converter csproj template pinned
        // to the JIT tree, so the publish overwrote the JIT binary in place with a self-contained,
        // dynamic-code-disabled one. Every JIT figure published before 2026-07-26 was timed on THAT
        // binary -- 3.3x slow on PerfIfaceShell (2,514.9 ms vs 754.3 ms) -- and MSBuild's incremental
        // check then saw the outputs up to date, so a later JIT build did not repair it and the error
        // survived every re-measure. The template now defers to $(BaseOutputPath); this guard is what
        // makes a recurrence impossible to publish. Per the project's false-green discipline a
        // misconfigured binary FAILS the run rather than quietly shipping a number.
        //
        // JIT-only by construction: the Native AOT variant is legitimately self-contained with dynamic
        // code disabled, and that asymmetry is exactly what the guard is detecting.
        private static void CheckJitConfiguration(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write("[Config]   JIT runtimeconfig... ");
            int rejected = 0;

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                VariantResult vr = result.Variants[Variant.Jit];

                if (!vr.BuildOk)
                    continue;

                string exe = GetExePath(p, Variant.Jit);
                string configPath = Path.Combine(Path.GetDirectoryName(exe)!, $"{p}.runtimeconfig.json");
                string? problem = InspectJitRuntimeConfig(configPath);

                if (problem is null)
                    continue;

                // Clearing BuildOk keeps Verify and Measure away from it, so the column reports n/a
                // rather than a number nobody can trust.
                vr.BuildOk = false;
                vr.Message = "misconfigured";
                result.Failed = true;
                result.Messages.Add($"JIT measurement configuration rejected: {problem} [{configPath}]");
                rejected++;
            }

            Console.WriteLine(rejected == 0 ? "ok" : $"{rejected} REJECTED");
        }

        // The decision is BinaryShape.JitProblem (ReleaseLegs.cs), shared with the release legs and guarded by
        // PerformanceRunnerTests; this reads the file beside the binary.
        internal static string? InspectJitRuntimeConfig(string path) =>
            BinaryShape.JitProblem(File.Exists(path) ? File.ReadAllText(path) : null);

        // ---- Phase: Verify ----

        private static void RunVerify(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, bool noAot)
        {
            Console.Write($"[Verify]   comparing Go vs C# output... ");
            int failed = 0;

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                string projPath = Path.Combine(s_perfDir, p);
                string? goOutput = null;

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    if (v == Variant.Aot && noAot)
                        continue;

                    VariantResult vr = result.Variants[v];

                    if (!vr.BuildOk)
                        continue;

                    string exe = GetExePath(p, v);

                    if (!File.Exists(exe))
                    {
                        vr.BuildOk = false;
                        vr.Message = "exe missing";
                        continue;
                    }

                    ProcResult r = Exec(exe, null, projPath, RunTimeoutMs);

                    if (r.ExitCode != 0)
                    {
                        vr.Message = $"exit {r.ExitCode}";
                        result.Failed = true;
                        result.Messages.Add($"{v} run exit {r.ExitCode}: {Truncate(r.StdErr)}");
                        failed++;
                        continue;
                    }

                    // Timing lines differ between runs by construction; compare everything else.
                    string filtered = FilterTimingLines(r.StdOut);

                    if (v == Variant.Go)
                    {
                        goOutput = filtered;
                        vr.VerifyOk = true;
                    }
                    else if (goOutput is null)
                    {
                        vr.Message = "no Go output to compare";
                    }
                    else if (string.Equals(filtered, goOutput, StringComparison.Ordinal))
                    {
                        vr.VerifyOk = true;
                    }
                    else
                    {
                        vr.Message = "output mismatch vs Go";
                        result.Failed = true;
                        result.Messages.Add($"{v} output mismatch vs Go: [{Truncate(filtered, 120)}] vs [{Truncate(goOutput, 120)}]");
                        failed++;
                    }
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        internal static string FilterTimingLines(string output)
        {
            return string.Join('\n', output
                .Replace("\r", "")
                .Split('\n')
                .Where(l => !l.StartsWith("elapsed_ns:", StringComparison.Ordinal)));
        }

        // ---- Phase: Measure ----

        private static void RunMeasure(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, int runs, bool noAot)
        {
            Console.WriteLine($"[Measure]  1 warmup + {runs} run(s) per variant...");

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                string projPath = Path.Combine(s_perfDir, p);
                Console.Write($"           {p}... ");

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    if (v == Variant.Aot && noAot)
                        continue;

                    VariantResult vr = result.Variants[v];

                    if (!vr.BuildOk || !vr.VerifyOk)
                        continue;

                    string exe = GetExePath(p, v);

                    // Warmup run (OS file cache, AV scan of the exe) -- discarded.
                    RunMeasured(exe, projPath);

                    for (int i = 0; i < runs; i++)
                    {
                        (int exitCode, string stdOut, double wallMs, long peakBytes) = RunMeasured(exe, projPath);

                        if (exitCode != 0)
                        {
                            vr.Message = $"measure run exit {exitCode}";
                            result.Failed = true;
                            result.Messages.Add($"{v} measure run exit {exitCode}");
                            break;
                        }

                        vr.Samples.Add(new RunSample(wallMs, ParseInnerMs(stdOut), peakBytes));
                    }
                }

                Console.WriteLine("done");
            }
        }

        internal static double ParseInnerMs(string stdOut)
        {
            foreach (string line in stdOut.Split('\n'))
            {
                if (!line.StartsWith("elapsed_ns:", StringComparison.Ordinal))
                    continue;

                if (long.TryParse(line["elapsed_ns:".Length..].Trim(), out long ns))
                    return ns / 1_000_000.0;
            }

            return 0.0;
        }

        internal static (int exitCode, string stdOut, double wallMs, long peakBytes) RunMeasured(string exe, string workDir,
            Dictionary<string, string>? environment = null)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exe,
                Arguments = "",
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (environment is not null)
            {
                foreach ((string key, string value) in environment)
                    startInfo.EnvironmentVariables[key] = value;
            }

            StringBuilder outBuf = new();

            using Process process = new();
            process.StartInfo = startInfo;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) outBuf.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, _) => { };

            Stopwatch sw = Stopwatch.StartNew();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Peak working set is only queryable while the process is alive; poll on a tight loop.
            // The counter is monotonic, so the last successful sample is the peak (to within ~1 ms).
            // Sample immediately so even a near-instant process (Startup) yields at least one reading.
            long peak = 0;

            try
            {
                peak = process.PeakWorkingSet64;
            }
            catch
            {
                // Process exited before the first sample.
            }

            while (!process.WaitForExit(1))
            {
                try
                {
                    process.Refresh();
                    long ws = process.PeakWorkingSet64;

                    if (ws > peak)
                        peak = ws;
                }
                catch
                {
                    // Process exited between the wait and the query.
                }

                if (sw.ElapsedMilliseconds > RunTimeoutMs)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch { /* may have exited in the race */ }

                    process.WaitForExit(5000);
                    return (-1, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, peak);
                }
            }

            sw.Stop();
            process.WaitForExit();  // flush async output handlers

            return (process.ExitCode, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, peak);
        }

        // ---- Report ----

        internal static double Median(IEnumerable<double> values)
        {
            List<double> sorted = values.OrderBy(v => v).ToList();

            if (sorted.Count == 0)
                return 0.0;

            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;
        }

        private static string DisplayName(string project)
        {
            return project.StartsWith("Perf", StringComparison.Ordinal) ? project[4..] : project;
        }

        // The Startup benchmark has an empty workload; its meaningful number is process wall time.
        // Every other benchmark reports the in-program workload time (excludes startup + fmt setup).
        private static double TimeMetric(string project, VariantResult vr)
        {
            return project == "PerfStartup" ? Median(vr.Samples.Select(s => s.WallMs)) : Median(vr.Samples.Select(s => s.InnerMs));
        }

        private static string BuildMarkdown(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, int runs, bool noAot)
        {
            StringBuilder sb = new();
            CultureInfo ci = CultureInfo.InvariantCulture;

            string goVersion = "go";
            ProcResult goVer = Exec("go", "version", s_perfDir, 15_000);

            if (goVer.ExitCode == 0)
            {
                string[] parts = goVer.StdOut.Trim().Split(' ');
                if (parts.Length >= 3)
                    goVersion = parts[2];
            }

            string sdkVersion = "?";
            ProcResult sdkVer = Exec("dotnet", "--version", s_perfDir, 30_000);

            if (sdkVer.ExitCode == 0)
                sdkVersion = sdkVer.StdOut.Trim();

            string cpu = GetCpuName();
            string os = RuntimeInformation.OSDescription.Trim();

            sb.AppendLine($"**Environment:** {cpu} · {os} · {goVersion} · .NET SDK {sdkVersion} · {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine($"C# builds: JIT = framework-dependent `Release`; Native AOT = `-p:PublishAot=true` self-contained, partial trim. Median of {runs} runs (1 discarded warmup). Workload time is measured in-program and excludes process startup; the Startup row is pure process wall time. Ratios are relative to Go.");
            sb.AppendLine();
            sb.AppendLine("**Execution time** (milliseconds -- lower is better):");
            sb.AppendLine();
            AppendTable(sb, projects, results, noAot, ci,
                (p, vr) => TimeMetric(p, vr),
                (value, goValue) => FormatTimeCell(value, goValue, ci));

            sb.AppendLine();
            sb.AppendLine("**Peak memory** (working set, MB -- lower is better):");
            sb.AppendLine();
            AppendTable(sb, projects, results, noAot, ci,
                (_, vr) => Median(vr.Samples.Select(s => (double)s.PeakBytes)) / (1024.0 * 1024.0),
                (value, _) => value.ToString("N1", ci));

            return sb.ToString();
        }

        private static string FormatTimeCell(double value, double goValue, CultureInfo ci)
        {
            string cell = value.ToString("N1", ci);

            if (goValue > 0.0)
                cell += $" ({(value / goValue).ToString("N2", ci)}×)";

            return cell;
        }

        private static void AppendTable(StringBuilder sb, IReadOnlyList<string> projects,
            Dictionary<string, ProjectResult> results, bool noAot, CultureInfo ci,
            Func<string, VariantResult, double> metric, Func<double, double, string> format)
        {
            sb.AppendLine("| Benchmark | Go | C# (JIT) | C# (Native AOT) |");
            sb.AppendLine("|---|---:|---:|---:|");

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                VariantResult go = result.Variants[Variant.Go];
                double goValue = go.Samples.Count > 0 ? metric(p, go) : 0.0;

                sb.Append($"| {DisplayName(p)} ");

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    VariantResult vr = result.Variants[v];

                    if ((v == Variant.Aot && noAot) || vr.Samples.Count == 0)
                    {
                        sb.Append("| n/a ");
                        continue;
                    }

                    double value = metric(p, vr);
                    sb.Append($"| {(v == Variant.Go ? value.ToString("N1", ci) : format(value, goValue))} ");
                }

                sb.AppendLine("|");
            }
        }

        internal static string CpuName() => GetCpuName();

        private static string GetCpuName()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using Microsoft.Win32.RegistryKey? key =
                        Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");

                    if (key?.GetValue("ProcessorNameString") is string name)
                        return name.Trim();
                }
            }
            catch
            {
                // fall through to the environment variable
            }

            // PROCESSOR_IDENTIFIER is a Windows environment variable, so off Windows the environment
            // line of the published table read "unknown CPU" -- which is the one field of a
            // performance report that must never be a shrug, since the numbers mean nothing without
            // the part that produced them. Linux publishes the same fact in /proc/cpuinfo.
            try
            {
                if (OperatingSystem.IsLinux() && File.Exists("/proc/cpuinfo"))
                {
                    foreach (string line in File.ReadLines("/proc/cpuinfo"))
                    {
                        if (!line.StartsWith("model name", StringComparison.OrdinalIgnoreCase))
                            continue;

                        int colon = line.IndexOf(':');

                        if (colon >= 0 && colon + 1 < line.Length)
                            return line[(colon + 1)..].Trim();
                    }
                }
            }
            catch
            {
                // fall through to the environment variable
            }

            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown CPU";
        }

        private static bool UpdateReadme(string markdown)
        {
            const string BeginMarker = "<!-- PERF-RESULTS:BEGIN -->";
            const string EndMarker = "<!-- PERF-RESULTS:END -->";

            string readme = Path.Combine(s_perfDir, "README.md");

            if (!File.Exists(readme))
            {
                Console.Error.WriteLine($"README not found: {readme}");
                return false;
            }

            string text = File.ReadAllText(readme);
            int begin = text.IndexOf(BeginMarker, StringComparison.Ordinal);
            int end = text.IndexOf(EndMarker, StringComparison.Ordinal);

            if (begin < 0 || end < 0 || end < begin)
            {
                Console.Error.WriteLine($"README markers not found ({BeginMarker} / {EndMarker}); not updated.");
                return false;
            }

            string updated = text[..(begin + BeginMarker.Length)] + "\n\n" + markdown.TrimEnd() + "\n\n" + text[end..];
            File.WriteAllText(readme, updated);
            Console.WriteLine($"Updated results block in {readme}");
            return true;
        }

        // ---- Build helpers ----

        private static string Go2csPathArg() => s_srcRoot.Replace('\\', '/').TrimEnd('/') + "/";

        private static string GetExePath(string project, Variant variant)
        {
            string projPath = Path.Combine(s_perfDir, project);

            return variant switch
            {
                Variant.Go => Path.Combine(projPath, "bin", Config, "Go", $"{project}{s_exeSuffix}"),
                Variant.Jit => Path.Combine(projPath, "bin", Config, NetVersion, $"{project}{s_exeSuffix}"),
                Variant.Aot => Path.Combine(projPath, "bin", Config, "aot", $"{project}{s_exeSuffix}"),
                _ => throw new ArgumentOutOfRangeException(nameof(variant))
            };
        }

        // Builds the deduped union of ProjectReferences across the target csprojs (golib, the analyzer,
        // core/* packages) one at a time, so they are up to date before the parallel target fan-out.
        private static void PreBuildSharedDeps(IReadOnlyList<string> projects, string go2csPathArg)
        {
            HashSet<string> deps = new(StringComparer.OrdinalIgnoreCase);

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");

                if (!File.Exists(csproj))
                    continue;

                string csprojDir = Path.GetDirectoryName(csproj)!;

                foreach (string line in File.ReadLines(csproj))
                {
                    int idx = line.IndexOf("ProjectReference Include=\"", StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) continue;

                    int start = idx + "ProjectReference Include=\"".Length;
                    int end = line.IndexOf('"', start);
                    if (end < 0) continue;

                    string raw = line[start..end].Replace("$(go2csPath)", s_srcRoot + Path.DirectorySeparatorChar);
                    deps.Add(Path.GetFullPath(raw, csprojDir));
                }
            }

            Console.Write($"[Build]    pre-building {deps.Count} shared dependencies... ");

            foreach (string dep in deps)
            {
                ProcResult r = Exec("dotnet",
                    $"build \"{dep}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_perfDir, BuildOneTimeoutMs);

                if (r.ExitCode != 0)
                    Console.Error.WriteLine($"\n  WARNING: shared dep build failed ({Path.GetFileName(dep)}): {Truncate(r.StdOut + r.StdErr)}");
            }

            Console.WriteLine("ok");
        }

        // Writes a traversal MSBuild project that builds all target csprojs in parallel in one call.
        private static string WriteTraversalProject(IReadOnlyList<string> projects)
        {
            string objDir = Path.Combine(AppContext.BaseDirectory, "traversal");
            Directory.CreateDirectory(objDir);
            string projFile = Path.Combine(objDir, "_AllTargets.proj");

            StringBuilder sb = new();
            sb.AppendLine("<Project DefaultTargets=\"BuildAll\">");
            sb.AppendLine("  <ItemGroup>");

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");
                sb.AppendLine($"    <ProjectToBuild Include=\"{csproj}\" />");
            }

            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine("  <Target Name=\"BuildAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Build\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("</Project>");

            File.WriteAllText(projFile, sb.ToString());
            return projFile;
        }

        private static HashSet<Phase> ParsePhases(string csv)
        {
            HashSet<Phase> set = new();

            foreach (string token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (token.ToLowerInvariant())
                {
                    case "transpile": set.Add(Phase.Transpile); break;
                    case "build": set.Add(Phase.Build); break;
                    case "verify": set.Add(Phase.Verify); break;
                    case "measure": set.Add(Phase.Measure); break;
                    case "all": set.UnionWith(Enum.GetValues<Phase>()); break;
                    default: Console.Error.WriteLine($"Unknown phase: {token}"); break;
                }
            }

            return set;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("""
                PerformanceRunner -- go2cs Go vs transpiled C# performance comparison.

                Builds each benchmark three ways (Go binary, C# JIT, C# Native AOT self-contained),
                verifies identical program output, then measures workload time, process wall time,
                and peak working set, reporting a markdown summary.

                Usage:
                  PerformanceRunner [--filter <substr>] [--phase <list>] [--runs <n>] [--no-aot]
                                    [--update-readme] [--list]

                Options:
                  --filter <substr>     Only projects whose name contains <substr> (case-insensitive).
                  --phase <list>        Comma list of: transpile,build,verify,measure,all (default all).
                  --runs <n>            Measured runs per variant (default 5; +1 discarded warmup).
                  --no-aot              Skip the Native AOT column (much faster builds).
                  --update-readme       Rewrite the results block in ../README.md (between the
                                        PERF-RESULTS markers) with this run's tables.
                  --list                List matched projects and exit.
                  -h, --help            Show this help.

                Release legs (docs/PLAN-perf-rebaseline.md; the in-tree phases do not run):
                  PerformanceRunner --leg <spec> [--leg <spec>] --work <dir> [--aot-manifest <file> --aot-drop <dir>]
                                    [--runs <n>] [--review-runs <n>] [--tc0] [--fd1] [--filter <substr>]

                  --leg <spec>          name=<leg>;go2cs=<converter exe>;version=<go.* version>;go=<go exe>.
                                        Each program is converted by that converter with -recurse=nuget.
                  --work <dir>          Scratch root for the legs' conversions, builds and report.md.
                  --aot-manifest <file> Tab-separated: leg program sha256 sourceHash size ilc sdk.
                  --aot-drop <dir>      Shipped binaries at <dir>/<leg>/<program>.exe; one is timed only
                                        if its manifest line matches it and this box's converted source.
                  --review-runs <n>     Runs for the PERF-REVIEW rows (default 11).
                  --tc0                 Add the diagnostic TC0 arm (the JIT binary, DOTNET_TieredCompilation=0).
                  --fd1                 Add the diagnostic Fd1 arm (framework-dependent, untrimmed ReadyToRun).

                Exit code 0 = pass, 1 = failure, 2 = usage error.
                """);
        }

        // ---- process execution with timeout + whole-tree kill ----

        internal readonly record struct ProcResult(int ExitCode, string StdOut, string StdErr);

        internal static ProcResult Exec(string application, string? arguments, string workingDir, int timeoutMs,
            Dictionary<string, string>? environment = null)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = application,
                Arguments = arguments ?? "",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Disable MSBuild node reuse so in-runner builds never leave worker nodes holding locks.
            startInfo.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";
            startInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
            startInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";

            if (environment is not null)
            {
                foreach ((string key, string value) in environment)
                    startInfo.EnvironmentVariables[key] = value;
            }

            StringBuilder outBuf = new(), errBuf = new();

            using Process process = new();
            process.StartInfo = startInfo;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) outBuf.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) errBuf.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(entireProcessTree: true); }
                catch { /* may have exited in the race */ }

                process.WaitForExit(5000);
                return new ProcResult(-1, outBuf.ToString(), $"TIMEOUT after {timeoutMs} ms; killed process tree.\n{errBuf}");
            }

            process.WaitForExit();
            return new ProcResult(process.ExitCode, outBuf.ToString(), errBuf.ToString());
        }

        internal static string Truncate(string s, int max = 300)
        {
            s = s.Replace("\r", "").Replace("\n", " ").Trim();
            return s.Length <= max ? s : s[..max] + " ...";
        }
    }
}
