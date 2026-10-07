// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Standalone runner for the go2cs behavioral suite. Mirrors the four MSTest phases
// (Transpile -> Compile -> TargetComparison -> OutputComparison) but as a plain console app that is
// NOT hosted in testhost.exe, removing the self-lock failure mode. It also collapses the per-project
// "dotnet build" churn (180 invocations) into a single parallel MSBuild call (Tier 2a).

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using BehavioralRunner;

return Runner.Main(args);

namespace BehavioralRunner
{
    internal enum Phase { Transpile, Compile, Target, Output }

    // Timeout is a verdict distinct from Fail, and the distinction is the whole point: Fail means a
    // tool ran to completion and reported the project broken, Timeout means the runner's own budget
    // expired first and nothing was learned either way. Collapsing them (as this enum did until
    // 2026-08-10) makes an under-sized budget indistinguishable from a corpus regression in the
    // summary -- a FALSE RED, the mirror of the false-green routes CLAUDE.md catalogs. It is
    // reported as NOT MEASURED, the same word check-no-regression.ps1 uses for the same situation,
    // and it still fails the run: an unmeasured project must never read as a pass.
    //
    // BestEffort is Timeout's sibling and joined it for the same reason: the converter EXITS ZERO on a
    // package it could not fully type-check, so a best-effort emission -- output the run did not really
    // regenerate -- was reported as a Transpile PASS and handed on to Compile, Target and Output, where
    // it reads as a downstream break billed to the wrong layer. Both are NOT MEASURED and both fail the
    // run; they are separate members because the REMEDIES are opposite (raise a budget vs. convert on a
    // host that can type-check the package), and a report that cannot tell them apart sends the reader
    // to the wrong one. See src/tests/BestEffortConversion.cs for the classification itself.
    internal enum Status { Pass, Fail, Skip, Timeout, BestEffort }

    internal sealed class ProjectResult
    {
        public required string Name { get; init; }
        public Dictionary<Phase, Status> Phases { get; } = new();
        public List<string> Messages { get; } = new();

        // Deliberately NOT collapsed into a single `Failed`. One existed and was left unreferenced
        // after Report started distinguishing the two, which is a trap rather than a convenience: the
        // next caller reaching for an obviously-named `Failed` to build a failure roster would sweep
        // every timeout back in with the real breaks and reinstate exactly the false red this
        // distinction removes. Callers state which one they mean.
        public bool HasFail => Phases.Values.Any(s => s == Status.Fail);
        public bool HasTimeout => Phases.Values.Any(s => s == Status.Timeout);
        public bool HasBestEffort => Phases.Values.Any(s => s == Status.BestEffort);

        // The two unmeasured statuses under one question, for the callers that only need "did this run
        // learn anything about the project". Spelled once here rather than as `HasTimeout ||
        // HasBestEffort` at each site: the next status of this kind must reach every such caller by
        // being added here, not by being remembered at four call sites.
        public bool HasUnmeasured => HasTimeout || HasBestEffort;
    }

    internal static class Runner
    {
        // Timeout budgets (ms). A build/transpile/run that exceeds one is treated as hung and killed
        // with its whole process tree -- the runner never blocks indefinitely the way the MSTest Exec
        // did. These were CONSTANTS until 2026-08-10, which made them the one input to the run that no
        // caller could influence: a wall-clock budget measured on one machine, applied unchanged to
        // every machine and every future corpus size. They now resolve flag > environment > default
        // (see Main), and an expired budget reports NOT MEASURED rather than impersonating a failure.
        //
        // Measured 2026-08-10 on an i7-5820K (6C/12T, ~3x slower than the desktop CLAUDE.md's timing
        // table is baselined on) at 555 enumerated packages: the one-shot parallel build exceeded
        // 300 s on a cold tree AND on a warm one. Warm state cannot save it -- the Transpile phase
        // rewrites every .cs immediately before Compile, so the batch is never an incremental no-op
        // and every project genuinely recompiles. (Corrected 2026-10-05: true when measured, no longer
        // since incremental .cs writes -- Transpile now rewrites only the .cs whose bytes change, so a
        // warm batch over an unchanged emission IS largely an incremental no-op.) The batch therefore timed out on every run and
        // dropped the whole corpus onto the per-project fallback, where each project must first build
        // the core dependency closure and so ALSO exceeded 180 s cold: ~15 minutes producing zero
        // assemblies and 555 Fail entries that were pure infrastructure. For scale, a full
        // `dotnet build src/go2cs.slnx` of the same tree took 1,432 s cold -- roughly 5x this batch
        // budget.
        //
        // The build defaults are sized from that slow-host measurement, per the safety-net doctrine
        // (a timeout is a net against a HUNG child, never a performance assumption): a default that
        // always expires on a legitimate host makes the runner unusable out of the box on exactly
        // the machine that runs it most, while a fast lane's only cost is how long a genuine hang
        // takes to be declared -- and a lane that wants the old fail-fast behavior opts DOWN
        // explicitly (--build-timeout 300 / GO2CS_BUILD_TIMEOUT=300). Transpile and Run keep their
        // original sizes: neither has ever expired on a healthy run on any measured host (528
        // output runs at 30 s on the i7-5820K), and Run is the phase where a real deadlock is the
        // likeliest cause, so its net stays tight.
        private const int DefaultBuildAllTimeoutMs = 2_400_000;  // one-shot parallel build of every C# target
        private const int DefaultBuildOneTimeoutMs = 300_000;    // per-project fallback build / go build
        private const int DefaultTranspileTimeoutMs = 60_000;
        private const int DefaultRunTimeoutMs = 30_000;

        private static int s_buildAllTimeoutMs = DefaultBuildAllTimeoutMs;
        private static int s_buildOneTimeoutMs = DefaultBuildOneTimeoutMs;
        private static int s_transpileTimeoutMs = DefaultTranspileTimeoutMs;
        private static int s_runTimeoutMs = DefaultRunTimeoutMs;

        // Consecutive per-project build timeouts that mean the budget itself is too small rather than
        // that some individual project hangs. Once the fallback has burned this many in a row there is
        // nothing left to learn by spending BuildOne on each remaining suspect -- on the cold i7-5820K
        // run that was ~15 minutes to discover an already-known fact -- so the rest are marked NOT
        // MEASURED without being attempted. Three in a row, because no healthy corpus has three
        // adjacent projects that each individually exceed the per-project budget while the batch also
        // failed. Nothing is marked Pass by this path, so it cannot manufacture a false green.
        private const int ConsecutiveTimeoutBailout = 3;

        private const string Config = "Release";
        // DERIVED from this runner's own bin tail, never spelled: the executable lives at
        // .../bin/<config>/<tfm>/, so the last segment of its base directory IS the TFM it was
        // built for -- the BehavioralTestBase pattern (CENSUS-tfm-inventory.md Class D), which is
        // what makes this the harness a TFM hop does not touch. A hardcoded "net9.0" here was a
        // FALSE-RED generator: after a hop the build succeeds, this probe misses the new folder,
        // and the runner reports hundreds of corpus failures on a green tree.
        private static readonly string NetVersion =
            AppContext.BaseDirectory.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries)[^1];

        // Executable suffix for a built .NET apphost or Go binary. Windows only; empty everywhere else.
        // Hard-coding ".exe" made every File.Exists probe below fail on Linux, which the Output phase
        // reported as "missing C# or Go exe" -- a SKIP, not a failure, so the run stayed green while
        // comparing nothing (F4, docs/PLAN-linux-operation.md). Both halves are closed now: the suffix
        // here, and the verdict in RunOutputComparison, where a missing binary is a named failure rather
        // than a silent skip -- so a future way of losing a binary cannot reproduce that vacuous green.
        private static readonly string s_exeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";

        // The bin/obj path fragments that used to live here moved to the shared
        // src/tests/BehavioralPackages.cs with the package walk that is their only consumer. They are
        // NOT re-declared here: a second copy would be free to drift from the walk it exists for, and
        // the reason they exist at all (a literal @"\bin\" never MATCHES off Windows, so build output
        // enumerates as behavioral packages instead of erroring) is a property of the walk.

        private static string s_repoRoot = null!;
        private static string s_srcRoot = null!;
        private static string s_behavioralDir = null!;
        private static string s_converterSrc = null!;
        private static string s_go2csExe = null!;

        // Go-build failures are tracked separately (a Go build failing is not a C# compile failure, but
        // it must still surface and fail the run, since it blocks output comparison).
        private static readonly List<string> s_goBuildFailures = new();

        // Projects whose Go oracle could not be rebuilt this run, split by cause so the Output phase can
        // report each honestly. Both must block the comparison: the Output phase gates only on the C#
        // Compile status, so without these a project whose `go build` failed would be compared against
        // whatever bin\Release\Go\<p>.exe an EARLIER run left behind -- today's C# against yesterday's
        // Go, scored as a Pass.
        private static readonly HashSet<string> s_goBuildBroken = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> s_goBuildTimedOut = new(StringComparer.OrdinalIgnoreCase);

        // Newest shared-dependency output (golib, the analyzer, core/* and any redirected package) as of
        // the pre-build. A target assembly older than this predates its own dependencies, so it cannot be
        // treated as evidence that the target still compiles -- see SuspectProjects.
        private static DateTime s_sharedDepStamp = DateTime.MinValue;

        // Set by --update-targets, and consulted by RunTranspile alone. A golden is the AUTHORITATIVE
        // record of what the converter emits, so the one thing that must never precede minting it is
        // an up-to-date SKIP. UpToDate answers on MTIMES, and nothing about "newer than its .go and
        // than go2cs.exe" says the .cs came from THIS converter: a `git checkout HEAD -- <file>.cs`,
        // a Copy-Item, an editor save, or any restore of the .cs alone leaves precisely that relation
        // over content from some other converter. The skip then reports Pass, the copy writes those
        // bytes into the golden, and afterwards .cs, .cs.target and UpToDate all agree by
        // construction, so no later run can see it (false-green route #2, turned on the goldens
        // themselves). Ordinary runs KEEP the skip: there it is a real optimisation whose worst case
        // is a stale COMPARISON, and only a stale RECORD is unrecoverable.
        private static bool s_forceTranspile;

        public static int Main(string[] args)
        {
            // ----- argument parsing -----
            string? filter = null;
            int sliceIndex = 0, sliceCount = 0;   // 0/0 = no slicing
            bool updateTargets = false;
            bool listOnly = false;
            HashSet<Phase> phases = new() { Phase.Transpile, Phase.Compile, Phase.Target, Phase.Output };

            // Held as nullable so "not given on the command line" stays distinguishable from "given a
            // value that happens to equal the default" -- the environment fallback below must apply
            // only in the former case.
            int? buildAllFlagMs = null, buildOneFlagMs = null, transpileFlagMs = null, runFlagMs = null;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                switch (a)
                {
                    case "--filter" when i + 1 < args.Length:
                        filter = args[++i];
                        break;
                    case "--slice" when i + 1 < args.Length:
                        if (!TryParseSlice(args[++i], out sliceIndex, out sliceCount)) return 2;
                        break;
                    case "--phase" when i + 1 < args.Length:
                        if (ParsePhases(args[++i]) is not { } parsed) return 2;
                        phases = parsed;
                        break;
                    case "--build-timeout" when i + 1 < args.Length:
                        if (!TryParseSeconds(args[++i], a, out int buildAll)) return 2;
                        buildAllFlagMs = buildAll;
                        break;
                    case "--build-one-timeout" when i + 1 < args.Length:
                        if (!TryParseSeconds(args[++i], a, out int buildOne)) return 2;
                        buildOneFlagMs = buildOne;
                        break;
                    case "--transpile-timeout" when i + 1 < args.Length:
                        if (!TryParseSeconds(args[++i], a, out int transpile)) return 2;
                        transpileFlagMs = transpile;
                        break;
                    case "--run-timeout" when i + 1 < args.Length:
                        if (!TryParseSeconds(args[++i], a, out int run)) return 2;
                        runFlagMs = run;
                        break;
                    case "--update-targets":
                        updateTargets = true;
                        // A re-baseline transpiles unconditionally. Set here rather than read from
                        // `updateTargets` inside RunTranspile so the rule is visible at the flag it
                        // belongs to and RunTranspile keeps one reason to consult one field.
                        s_forceTranspile = true;
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

            // ----- resolve timeout budgets: flag > environment > default -----
            s_buildAllTimeoutMs = ResolveTimeout(buildAllFlagMs, "GO2CS_BUILD_TIMEOUT", DefaultBuildAllTimeoutMs);
            s_buildOneTimeoutMs = ResolveTimeout(buildOneFlagMs, "GO2CS_BUILD_ONE_TIMEOUT", DefaultBuildOneTimeoutMs);
            s_transpileTimeoutMs = ResolveTimeout(transpileFlagMs, "GO2CS_TRANSPILE_TIMEOUT", DefaultTranspileTimeoutMs);
            s_runTimeoutMs = ResolveTimeout(runFlagMs, "GO2CS_RUN_TIMEOUT", DefaultRunTimeoutMs);

            // ----- resolve paths -----
            // Runner lives at src\tests\Behavioral\BehavioralRunner; behavioral dir is its parent.
            // Path.Combine with SEGMENTS, not an embedded @"..\..\..\..": .NET does not normalize a
            // backslash on Unix, so that string is ONE directory name there and GetFullPath yields a
            // path that exists nowhere -- discovery then finds zero projects and every phase reports
            // vacuously. Segment form is identical on Windows and correct on both.
            s_behavioralDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            s_srcRoot = Path.GetFullPath(Path.Combine(s_behavioralDir, "..", ".."));
            s_repoRoot = Path.GetFullPath(Path.Combine(s_srcRoot, ".."));
            s_converterSrc = Path.Combine(s_srcRoot, "go2cs");
            s_go2csExe = Path.Combine(s_converterSrc, "bin", $"go2cs{s_exeSuffix}");

            // ----- discover projects -----
            // A behavioral test project is a folder with both a .csproj and Go source. This naturally
            // excludes the BehavioralTests (MSTest) runner and this BehavioralRunner utility (no .go),
            // and any future utility folder, without brittle name checks.
            List<string> projects = Directory.GetDirectories(s_behavioralDir)
                .Where(d => Directory.GetFiles(d, "*.csproj").Length > 0)
                .Where(d => Directory.GetFiles(d, "*.go").Length > 0)
                .Select(Path.GetFileName)
                .Where(n => filter is null || n!.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            // F8 -- PLATFORM-EXCLUSIVE packages are removed from the enumeration and reported BY NAME.
            // A package whose Go source only type-checks on some platforms (ScmRightsSeam's unix-only
            // syscall API on Windows; FindFirstFileData's Win32 surface on Linux) cannot be measured
            // here at all: the converter emits a best-effort conversion and every phase downstream then
            // reports a failure that is really a host mismatch. Skipping SILENTLY would be worse than
            // the failure, so the names and their platforms are printed and counted separately, and
            // they never enter the pass/fail denominators.
            //
            // TWO AXES since 2026-09-04: GOOS and GOARCH. StdLibInternalAbi is native to every GOOS
            // and exclusive to amd64 -- its Go source does not build on arm64 at all, Go's own
            // filename rule dropping abi_amd64.go/goarch_amd64.go/zgoarch_amd64.go -- which is the
            // arm64 half of the darwin census reading `goarch.cs(23,22): error CS0145`. ShouldSkip
            // answers for both axes; nothing here needs to know which one fired.
            List<string> platformExclusive = projects
                .Where(n => PlatformExclusive.ShouldSkip(Path.Combine(s_behavioralDir, n), out _))
                .ToList();

            if (platformExclusive.Count > 0)
            {
                projects = projects.Except(platformExclusive).ToList();

                Console.WriteLine($"SKIPPED (platform-exclusive, {platformExclusive.Count}): native to another platform or architecture, so this {PlatformExclusive.HostTarget} host cannot measure them:");

                foreach (string n in platformExclusive)
                {
                    PlatformExclusive.ShouldSkip(Path.Combine(s_behavioralDir, n), out string nativeTo);
                    Console.WriteLine($"    {n} [{nativeTo}]");
                }

                Console.WriteLine();
            }


            // ----- SLICE -----
            // Applied AFTER the platform-exclusive removal, deliberately: the slices then partition
            // the MEASURABLE set evenly, and the leg's count assertion (sum of slice sizes == the
            // measurable total) is about the packages that were actually run. Slicing first would
            // hand one slice all six windows-exclusives and make its denominator meaningless.
            //
            // Both numbers are printed on one machine-readable line because the leg needs BOTH: the
            // per-slice size to sum, and the total to check every slice agrees on it. A partition bug
            // that drops packages shows up as a sum that is short -- which is route #3's shape (an
            // enumeration silently missing packages) reached through a new door, so it is asserted
            // rather than trusted.
            // Captured BEFORE the slice narrows it: this is what DISCOVERY produced, and it is what
            // the corpus-floor guard below must judge. Testing the post-slice count would make the
            // guard fire on every legitimate slice (measured: --slice 1/200 tripped it at 4 of 655),
            // and exempting slices from the guard instead would open a hole -- a slice run against a
            // broken discovery would then pass. Narrowing is deliberate; discovery collapsing is not.
            int discoveredMeasurable = projects.Count;

            if (sliceCount > 0)
            {
                int measurable = discoveredMeasurable;
                projects = Slice(projects, sliceIndex, sliceCount);
                Console.WriteLine($"SLICE {sliceIndex}/{sliceCount}: {projects.Count} of {measurable} measurable projects");
                Console.WriteLine();
            }

            if (listOnly)
            {
                foreach (string p in projects)
                    Console.WriteLine(p);
                Console.WriteLine($"({projects.Count} projects)");
                return 0;
            }

            if (projects.Count == 0)
            {
                Console.Error.WriteLine("No behavioral projects matched.");
                return 2;
            }

            // Enumeration shape guard. An UNFILTERED run walks the whole corpus, which has been in the
            // 500s since 2026-08 and only ever grows; a collapse to a handful means path construction is
            // broken (a separator baked for the wrong host), not that tests were deleted. Without this,
            // that failure is silent -- the run reports "3 project(s)" and passes, which is the shape of
            // a false green rather than a fault. A floor rather than a pinned count, because CLAUDE.md's
            // standing instruction for this corpus is to measure, never to decrement.
            if (filter is null && discoveredMeasurable < 400)
            {
                Console.Error.WriteLine($"Behavioral discovery found only {discoveredMeasurable} projects under {s_behavioralDir}.");
                Console.Error.WriteLine("That is far below the corpus size; discovery is broken, so this run would prove nothing.");
                return 2;
            }

            Console.WriteLine($"go2cs behavioral runner: {projects.Count} project(s), phases [{string.Join(", ", phases)}]");

            // Always echo the budgets, overridden or not. The failure this whole mechanism exists for
            // was invisible precisely because the caps were invisible: a run that timed out printed the
            // number only in the "build-all TIMED OUT" line it emitted AFTER paying the cost. One line
            // up front makes every log self-describing about what it was allowed to spend.
            Console.WriteLine($"  timeouts: build-all {Describe(s_buildAllTimeoutMs, DefaultBuildAllTimeoutMs)}, " +
                              $"build-one {Describe(s_buildOneTimeoutMs, DefaultBuildOneTimeoutMs)}, " +
                              $"transpile {Describe(s_transpileTimeoutMs, DefaultTranspileTimeoutMs)}, " +
                              $"run {Describe(s_runTimeoutMs, DefaultRunTimeoutMs)}");

            Stopwatch total = Stopwatch.StartNew();

            Dictionary<string, ProjectResult> results = projects.ToDictionary(p => p, p => new ProjectResult { Name = p });

            // ----- Phase: Transpile -----
            if (phases.Contains(Phase.Transpile) || phases.Contains(Phase.Target) || phases.Contains(Phase.Compile) || phases.Contains(Phase.Output))
            {
                if (!EnsureConverterBuilt())
                    return 1;

                RunTranspile(projects, results);

                if (updateTargets)
                {
                    int rebaselined = UpdateTargets(projects, results, out List<string> refused);

                    Console.WriteLine($"Updated .cs.target goldens for {rebaselined} project(s).");

                    if (refused.Count == 0)
                        return 0;

                    // Never re-baseline from a transpile that did not complete OR that completed
                    // best-effort. A killed converter leaves either the PREVIOUS converter's .cs or a
                    // truncated partial write; a degraded one exits 0 having written a .cs for a
                    // package it could not fully type-check. Copying either over the golden writes the
                    // unmeasured state into the authoritative record -- worse than a false green,
                    // because UpToDate then sees a .cs newer than both its .go and go2cs.exe and skips
                    // re-transpiling it on every later run, hiding the drift permanently. Exit
                    // non-zero so a wrapper cannot read this as success.
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"REFUSED to re-baseline {refused.Count} project(s) whose transpile was not measured:");

                    foreach (string p in refused.Take(20))
                        Console.Error.WriteLine($"  {p}");

                    if (refused.Count > 20)
                        Console.Error.WriteLine($"  ... and {refused.Count - 20} more.");

                    // The remedies are DIFFERENT and naming only one sends the reader to the wrong
                    // place: a timeout wants a bigger budget, a best-effort conversion wants a host
                    // that can type-check the package (or an F8 marker saying this one cannot).
                    Console.Error.WriteLine("Their goldens are UNCHANGED. A timeout wants a larger --transpile-timeout;");
                    Console.Error.WriteLine("a best-effort conversion wants a host that can type-check the package.");
                    return 1;
                }
            }

            // ----- Phase: Target comparison (pure file compare; no build) -----
            if (phases.Contains(Phase.Target))
                RunTargetComparison(projects, results);

            // ----- Phase: Compile (one-shot C# build-all + per-project go build) -----
            bool needRun = phases.Contains(Phase.Output);

            if (phases.Contains(Phase.Compile) || needRun)
            {
                RunCompileCSharp(projects, results);

                if (needRun)
                    RunCompileGo(projects, results);
            }

            // ----- Phase: Output comparison -----
            if (phases.Contains(Phase.Output))
                RunOutputComparison(projects, results);

            total.Stop();

            // ----- summary -----
            return Report(results.Values, total.Elapsed);
        }

        // The staleness question is answered by the SHARED ConverterBuildInputs (src/tests), not by a
        // local *.go enumeration: an embedded template or a converter internal/ package changes what
        // go2cs.exe emits while touching no top-level .go file, and a predicate that cannot see them
        // reports "up to date" while every phase below validates the PREVIOUS emission -- false-green
        // route #5 in CLAUDE.md. The same call is made by BehavioralTestBase and PerformanceRunner.
        private static bool EnsureConverterBuilt()
        {
            if (!ConverterBuildInputs.IsConverterStale(s_converterSrc, s_go2csExe))
                return true;

            Console.WriteLine("Building go2cs.exe (converter sources changed)...");
            ProcResult r = Exec("go", $"build -o \"{s_go2csExe}\"", s_converterSrc, s_buildOneTimeoutMs);

            if (r.ExitCode != 0)
            {
                Console.Error.WriteLine($"go build of converter failed ({r.ExitCode}):\n{r.StdErr}");
                return false;
            }

            return true;
        }

        // The converter is invoked with an EXPLICIT -go2cspath (s_srcRoot, derived from this runner's own
        // location) rather than inheriting the ambient GO2CSPATH. That flag -- not the MSBuild
        // $(go2csPath) property of the same name -- is the root the converter reads an imported package's
        // package_info.cs from to mint the emitted <ImportedTypeAliases> block, and its default is
        // ~/go2cs, which on most boxes is either absent or a stale deploy. Inherited, it made the
        // transpiled output (and so the Target phase's verdict) depend on the shell that launched the run
        // (BOARD-next-validation-candidates.md, 2026-08-06). src\ is the canonical root here because the
        // behavioral .csproj files bind $(go2csPath)core\<pkg> with MSBuild $(go2csPath) -> $(SolutionDir)
        // -> src\, so src\core is exactly what these tests compile and link against.
        private static void RunTranspile(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Transpile] {projects.Count} project(s)... ");
            int failed = 0, timedOut = 0, bestEffort = 0;

            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_behavioralDir, p);

                if (!s_forceTranspile && UpToDate(projPath))
                {
                    results[p].Phases[Phase.Transpile] = Status.Pass;
                    continue;
                }

                bool ok = true, hitTimeout = false, hitBestEffort = false;

                foreach (string pkgPath in GoPackageDirs(projPath))
                {
                    ProcResult r = Exec(s_go2csExe, $"-go2cspath \"{s_srcRoot}\" \"{pkgPath}\"", pkgPath, s_transpileTimeoutMs);

                    if (r.ExitCode == 0)
                    {
                        // EXIT ZERO IS NOT ENOUGH. The converter reports a package it could not fully
                        // type-check (or a source file whose emission it had to skip) on stderr and then
                        // exits 0 with a best-effort emission on disk. Asking the exit code alone made
                        // this phase print PASS over output the run never really regenerated -- and every
                        // phase below then measured that output as though it were this converter's, which
                        // is the false-green shape the rest of this runner is built to refuse.
                        //
                        // Not a `break`: the emission is written either way, and the remaining packages
                        // of a multi-package project must still be converted or the tree is left half
                        // regenerated (a nested sub-library's package_info.cs is an INPUT to its parent's
                        // transpile). The verdict is already decided; the work still has to finish.
                        if (BestEffortConversion.NotFullyRegenerated(r.StdErr, out string[] degraded))
                        {
                            hitBestEffort = true;

                            foreach (string line in degraded)
                                results[p].Messages.Add($"transpile NOT MEASURED in {Path.GetFileName(pkgPath)}: {Truncate(line)}");
                        }

                        continue;
                    }

                    hitTimeout = TimedOut(r);

                    results[p].Messages.Add(hitTimeout
                        ? $"transpile TIMED OUT after {Seconds(s_transpileTimeoutMs)} in {Path.GetFileName(pkgPath)} (raise --transpile-timeout)"
                        : $"transpile exit {r.ExitCode} in {Path.GetFileName(pkgPath)}: {Truncate(r.StdErr)}");

                    ok = false;
                    break;
                }

                // Precedence, loudest first: a converter that FAILED outright is a more specific fact
                // than the degradation it may have printed on the way down, and an expired budget says
                // nothing at all about the emission. Only a run that completed AND said it was degraded
                // lands on BestEffort.
                if (!ok)
                {
                    if (hitTimeout)
                    {
                        results[p].Phases[Phase.Transpile] = Status.Timeout;
                        timedOut++;
                    }
                    else
                    {
                        results[p].Phases[Phase.Transpile] = Status.Fail;
                        failed++;
                    }
                }
                else if (hitBestEffort)
                {
                    results[p].Phases[Phase.Transpile] = Status.BestEffort;
                    bestEffort++;
                }
                else
                {
                    results[p].Phases[Phase.Transpile] = Status.Pass;
                }
            }

            Console.WriteLine(failed == 0 && timedOut == 0 && bestEffort == 0
                ? "ok"
                : $"{failed} failed{(timedOut > 0 ? $", {timedOut} timed out" : "")}{(bestEffort > 0 ? $", {bestEffort} NOT MEASURED (best-effort conversion)" : "")}");
        }

        // A project is up to date when every .cs is newer than BOTH its matching .go source and the
        // converter binary that produced it. The converter must be part of this comparison: converter
        // work is the normal case where the .go files DON'T change, so a .go-only check leaves every
        // project "up to date", skips transpilation entirely, and lets the Target/Output phases validate
        // the PREVIOUS converter's output against goldens that same converter generated -- a false green
        // that guards nothing. (check-no-regression.ps1 re-transpiles unconditionally and is immune,
        // and so is --update-targets since 2026-09-04: see s_forceTranspile. A stale COMPARISON is
        // recoverable; a stale RECORD is not, which is why only one of the two keeps the skip.)
        //
        // The two walks below FORWARD to the shared definition in src/tests/BehavioralPackages.cs,
        // linked into this project. Both encode invariants that three separate instruments have to
        // agree on -- production-sources-only, and DEEPEST-FIRST -- and route #3 is what disagreeing
        // about the second one cost. Kept as named forwarders rather than rewritten call sites so
        // this file's eight uses read exactly as they did.
        private static string[] ProductionGoFiles(string projPath) =>
            BehavioralPackages.ProductionGoFiles(projPath);

        /// <summary>
        /// Parses <c>--slice i/n</c>: the i-th of n contiguous, disjoint pieces of the enumeration,
        /// 1-based.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This exists because <c>--filter</c> CANNOT express a partition: it is a case-insensitive
        /// SUBSTRING match, so `--filter S` selects 455 of 664 projects rather than the 79 whose names
        /// begin with S, and no set of filter arguments covers the enumeration exactly or disjointly.
        /// A behavioral leg that must run every package on a disk too small to hold them all needs a
        /// real partition, and this is it.
        /// </para>
        /// <para>
        /// SLICING THE TOP-LEVEL LIST IS SAFE FOR THE DEEPEST-FIRST INVARIANT, which is the one thing
        /// worth checking before adding it (FALSE-GREEN route #3 exists because that invariant was
        /// once broken silently). Deepest-first ordering is applied by <see cref="GoPackageDirs"/>
        /// WITHIN a project -- a project and its nested sub-libraries are transpiled together, deepest
        /// first, in one step -- so a project is self-contained and carries its sub-libraries with it
        /// into whichever slice it lands in. No slice boundary can separate a sub-library from its
        /// consumer.
        /// </para>
        /// </remarks>
        private static bool TryParseSlice(string value, out int index, out int count)
        {
            index = count = 0;
            string[] parts = value.Split('/');

            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out index) || !int.TryParse(parts[1], out count) ||
                count < 1 || index < 1 || index > count)
            {
                Console.Error.WriteLine($"--slice expects i/n with 1 <= i <= n (got '{value}')");
                index = count = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// The i-th of n contiguous pieces of <paramref name="all"/>, sized so the pieces differ by
        /// at most one and their lengths SUM to the whole -- the property the leg's count assertion
        /// checks, and the one a partition bug would break silently.
        /// </summary>
        private static List<string> Slice(List<string> all, int index, int count)
        {
            int baseSize = all.Count / count;
            int remainder = all.Count % count;
            // The first `remainder` pieces take one extra, so every element lands in exactly one.
            int start = (index - 1) * baseSize + Math.Min(index - 1, remainder);
            int size = baseSize + (index <= remainder ? 1 : 0);
            return all.GetRange(start, size);
        }

        private static string[] GoPackageDirs(string projPath) =>
            BehavioralPackages.GoPackageDirs(projPath);

        private static bool UpToDate(string projPath)
        {
            DateTime exe = File.GetLastWriteTimeUtc(s_go2csExe);

            // Nested sub-library packages count: if one of them is out of date the project must be
            // re-transpiled, or the stale sibling records feed straight back into the parent's output.
            foreach (string pkgPath in GoPackageDirs(projPath))
            {
                foreach (string go in ProductionGoFiles(pkgPath))
                {
                    string cs = Path.ChangeExtension(go, ".cs");

                    if (!File.Exists(cs))
                        return false;

                    DateTime csTime = File.GetLastWriteTimeUtc(cs);

                    if (csTime <= File.GetLastWriteTimeUtc(go) || csTime <= exe)
                        return false;
                }
            }

            return true;
        }

        private static void RunTargetComparison(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Target]   byte-comparing goldens... ");
            int failed = 0;

            foreach (string p in projects)
            {
                // A timed-out transpile is skipped for the same reason a failed one is: there is no
                // trustworthy .cs to compare, and comparing the PREVIOUS run's leftover output against
                // its own golden would report a pass that proves nothing. A BEST-EFFORT transpile is the
                // sharpest case of all: it leaves a .cs on disk that is DIFFERENT from the emission the
                // golden records, so comparing it does not merely prove nothing -- on a package whose
                // degraded emission happens to match, it manufactures a byte-identical pass over output
                // this host cannot produce correctly.
                if (results[p].Phases.TryGetValue(Phase.Transpile, out Status t) && t is Status.Fail or Status.Timeout or Status.BestEffort)
                {
                    results[p].Phases[Phase.Target] = Status.Skip;
                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);
                bool ok = true;

                foreach (string go in ProductionGoFiles(projPath))
                {
                    string cs = Path.ChangeExtension(go, ".cs");
                    string target = cs + ".target";

                    if (!File.Exists(cs) || !File.Exists(target) || !FilesEqual(cs, target))
                    {
                        ok = false;
                        results[p].Messages.Add($"target mismatch: {Path.GetFileName(cs)}");
                    }
                }

                results[p].Phases[Phase.Target] = ok ? Status.Pass : Status.Fail;
                if (!ok) failed++;
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunCompileCSharp(IReadOnlyList<string> allProjects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = s_srcRoot.Replace('\\', '/').TrimEnd('/') + "/";

            // A project whose transpile did not complete has no trustworthy .cs to compile: what is on
            // disk is the PREVIOUS converter's output or a partial write from a killed process, so
            // building it would report Pass for code THIS run never produced. RunTargetComparison
            // already refuses to compare such a project; compile must refuse for the same reason, or a
            // summary can read "Transpile timeout 1 / Compile pass 555" -- a phase vouching for a
            // project the run did not measure. Absent status (transpile phase not run at all) is
            // treated as buildable, preserving the behavior when phases are selected individually.
            List<string> projects = allProjects
                .Where(p => !results[p].Phases.TryGetValue(Phase.Transpile, out Status t) || t == Status.Pass)
                .ToList();

            foreach (string p in allProjects.Except(projects, StringComparer.OrdinalIgnoreCase))
                results[p].Phases[Phase.Compile] = Status.Skip;

            if (projects.Count == 0)
            {
                Console.WriteLine("[Compile]  C# skipped: no project has a completed transpile.");
                return;
            }

            // Pre-build the shared dependencies (golib, the go2cs-gen analyzer, and the core/* packages
            // the targets reference) SEQUENTIALLY first. The one-shot parallel build of 180 targets that
            // each pull these shared projects otherwise races on their obj/bin output (intermittent
            // MSB3026/MSB3027 "file in use" under heavy parallelism). Building them to completion up front
            // means they are up to date during the fan-out, so no node writes to them and the race is gone.
            PreBuildSharedDeps(projects, go2csPathArg);

            // Generate a traversal project that builds every target csproj in a single parallel MSBuild
            // invocation -- replacing 180 sequential "dotnet build" calls (Tier 2a). go2csPath is pinned
            // to the src root so each target's golib/analyzer refs resolve to live source (matching the
            // MSTest harness, which sets the go2csPath env var); Configuration=Release matches TargetConfig.
            string traversal = WriteTraversalProject(projects);

            string commonArgs = $"-nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}";

            // RESTORE FIRST -- and in its own process. The traversal drives each target through the
            // MSBuild *task* (Targets="Build"), which -- unlike the "dotnet build <csproj>" the
            // per-project path uses -- does NOT imply a restore. Any project in the graph that has no
            // obj\project.assets.json therefore fails instantly with NETSDK1004 ("Assets file not
            // found"), which fails the whole batch and drops the ENTIRE suite onto the per-project
            // attribution path below -- a ~20 minute tax to discover that nothing was actually broken
            // (the fallback restores as it goes, so it reports 0 failures). That is not a cold-clone
            // curiosity: it fires on any fresh worktree, after clean-bin, and for any dependency
            // subtree the pre-build does not cover. Restore runs as a SEPARATE invocation because
            // restore rewrites a project's imports, so it must not share an evaluation with Build.
            Console.Write($"[Compile]  C# (restoring {projects.Count})... ");

            ProcResult restore = Exec("dotnet",
                $"build \"{traversal}\" -t:RestoreAll {commonArgs}",
                s_behavioralDir, s_buildAllTimeoutMs);

            Console.WriteLine(restore.ExitCode == 0 ? "ok" : $"restore reported errors (exit {restore.ExitCode})");

            if (restore.ExitCode != 0)
                Console.Error.WriteLine($"  restore output: {Truncate(restore.StdOut + restore.StdErr, 1000)}");

            Console.Write($"[Compile]  C# (one-shot parallel build of {projects.Count})... ");

            ProcResult all = Exec("dotnet",
                $"build \"{traversal}\" -t:BuildAll {commonArgs}",
                s_behavioralDir, s_buildAllTimeoutMs);

            if (all.ExitCode == 0)
            {
                foreach (string p in projects)
                    results[p].Phases[Phase.Compile] = Status.Pass;

                Console.WriteLine("ok");
                return;
            }

            // Build-all failed. Narrow the per-project attribution to the projects that could actually
            // be responsible, so one broken project costs one rebuild instead of 438. A project is a
            // suspect when MSBuild named it in an error line, OR when it has no up-to-date output
            // assembly (which also covers targets the failed batch never got around to scheduling).
            // Anything with a fresh assembly demonstrably compiled in THIS batch and is passed without
            // a rebuild. When the suspect set cannot be determined -- an empty set, or a timeout, where
            // the batch was killed mid-flight and the assembly evidence is meaningless -- every project
            // is attributed, preserving the original conservative behavior.
            bool timedOut = TimedOut(all);
            string buildOutput = all.StdOut + all.StdErr;

            List<string> suspects = timedOut
                ? projects.ToList()
                : SuspectProjects(projects, buildOutput);

            if (suspects.Count == 0)
                suspects = projects.ToList();

            Console.WriteLine(timedOut
                ? $"build-all TIMED OUT after {Seconds(s_buildAllTimeoutMs)}; attributing all {suspects.Count} per project..."
                : $"build-all reported errors; attributing {suspects.Count} suspect project(s) per project...");

            if (timedOut)
            {
                Console.Error.WriteLine($"  NOTE: the batch budget, not any project, expired. If this run is on a slower machine or a");
                Console.Error.WriteLine($"        grown corpus, raise it (--build-timeout <sec> / GO2CS_BUILD_TIMEOUT) rather than reading");
                Console.Error.WriteLine($"        the per-project attribution below as a corpus regression.");
            }

            // Always surface why the batch failed -- silently discarding it (as this path used to) makes
            // an infrastructure failure indistinguishable from a real compile break.
            Console.Error.WriteLine($"  build-all output: {Truncate(buildOutput, 1000)}");

            foreach (string p in projects.Except(suspects, StringComparer.OrdinalIgnoreCase))
                results[p].Phases[Phase.Compile] = Status.Pass;

            int failed = 0, timeouts = 0, consecutiveTimeouts = 0;
            bool budgetExhausted = false;

            foreach (string p in suspects)
            {
                // Circuit breaker. Once the per-project budget has proven too small several times in a
                // row, every remaining suspect is marked NOT MEASURED rather than attempted: spending
                // BuildOne on each of 555 projects to re-learn the same fact is the ~15-minute,
                // zero-assembly path that made this whole failure mode expensive as well as misleading.
                if (budgetExhausted)
                {
                    results[p].Phases[Phase.Compile] = Status.Timeout;
                    results[p].Messages.Add($"compile NOT ATTEMPTED: per-project budget ({Seconds(s_buildOneTimeoutMs)}) already exceeded {ConsecutiveTimeoutBailout}x consecutively");
                    timeouts++;
                    continue;
                }

                string csproj = Path.Combine(s_behavioralDir, p, $"{p}.csproj");

                ProcResult r = Exec("dotnet",
                    $"build \"{csproj}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_behavioralDir, s_buildOneTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Phases[Phase.Compile] = Status.Pass;
                    consecutiveTimeouts = 0;
                }
                else if (TimedOut(r))
                {
                    // NOT a compile break: the compiler never got to report on this project. On a cold
                    // tree each per-project build must first build the core dependency closure, which
                    // alone can exceed the budget.
                    results[p].Phases[Phase.Compile] = Status.Timeout;
                    results[p].Messages.Add($"compile TIMED OUT after {Seconds(s_buildOneTimeoutMs)} (raise --build-one-timeout)");
                    timeouts++;

                    if (++consecutiveTimeouts >= ConsecutiveTimeoutBailout)
                    {
                        budgetExhausted = true;
                        Console.WriteLine();
                        Console.Error.WriteLine($"  {ConsecutiveTimeoutBailout} consecutive per-project build timeouts: the budget is too small for this machine,");
                        Console.Error.WriteLine($"  not the corpus broken. Skipping the remaining suspects as NOT MEASURED.");
                    }
                }
                else
                {
                    results[p].Phases[Phase.Compile] = Status.Fail;
                    results[p].Messages.Add($"compile exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    failed++;
                    consecutiveTimeouts = 0;
                }
            }

            Console.WriteLine($"[Compile]  C# per-project: {failed} failed{(timeouts > 0 ? $", {timeouts} NOT MEASURED (timed out)" : "")}");
        }

        // Projects that could be responsible for a failed batch build: those MSBuild named in an error
        // line, plus those with no up-to-date output assembly. The assembly check is what makes it safe
        // to pass the remainder without rebuilding -- an assembly newer than every .cs in the project AND
        // newer than every shared dependency was necessarily produced from exactly the inputs in play now,
        // so a stale artifact from an earlier converter (or an earlier golib) can never be mistaken for a
        // fresh success. Note this degrades to attributing everything in the case that matters most: a
        // golib or generator change makes every assembly stale and every project a suspect. (A converter
        // change rewrote every .cs too until incremental .cs writes, 2026-10; now it rewrites only the
        // .cs whose emission moved, and an unchanged .cs keeps its time, so the shared-dependency half
        // of this check is what catches a runtime change.) The narrowing only ever pays off when the
        // inputs really did not move.
        private static List<string> SuspectProjects(IReadOnlyList<string> projects, string buildOutput)
        {
            HashSet<string> suspects = new(StringComparer.OrdinalIgnoreCase);

            // MSBuild appends the owning project to each diagnostic: "... error CS1002: ; expected
            // [C:\path\Foo.csproj]". Errors from a referenced package (e.g. a converted stdlib
            // dependency) name that project instead, which matches nothing here -- leaving the suspect
            // set empty and falling back to attributing everything.
            foreach (string line in buildOutput.Split('\n'))
            {
                int open = line.LastIndexOf('[');
                int close = line.LastIndexOf(".csproj]", StringComparison.OrdinalIgnoreCase);

                if (open < 0 || close < open)
                    continue;

                string name = Path.GetFileNameWithoutExtension(line[(open + 1)..(close + ".csproj".Length)]);

                foreach (string p in projects)
                {
                    if (string.Equals(p, name, StringComparison.OrdinalIgnoreCase))
                        suspects.Add(p);
                }
            }

            foreach (string p in projects)
            {
                if (suspects.Contains(p))
                    continue;

                string projPath = Path.Combine(s_behavioralDir, p);
                string assembly = Path.Combine(projPath, "bin", Config, NetVersion, $"{p}.dll");

                if (!File.Exists(assembly))
                {
                    suspects.Add(p);
                    continue;
                }

                DateTime built = File.GetLastWriteTimeUtc(assembly);

                // Stale against its own sources, or against the shared dependencies it links -- either
                // way the assembly proves nothing about whether this project still compiles today.
                if (built < s_sharedDepStamp || Directory.GetFiles(projPath, "*.cs").Any(cs => File.GetLastWriteTimeUtc(cs) > built))
                    suspects.Add(p);
            }

            return projects.Where(suspects.Contains).ToList();
        }

        private static void RunCompileGo(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Compile]  Go (per project)... ");
            int failed = 0, consecutiveTimeouts = 0;
            bool budgetExhausted = false;

            foreach (string p in projects)
            {
                // Only output-compared projects are Go-built (matching the MSTest harness, which emits a
                // Go-build step solely for [GoTestMatchingConsoleOutput] projects). Library-style projects
                // with no "package main" (e.g. Constraints) have nothing to "go build -o".
                if (!MatchConsoleOutput(p))
                    continue;

                // Building the Go oracle for a project whose C# side did not compile is pure waste: the
                // Output phase requires Phase.Compile == Pass and will skip it regardless. This matters
                // most in the case that motivated the whole change -- when the C# batch times out,
                // RunCompileCSharp now returns in milliseconds, and without this the run would go on to
                // spend a full budget per project building oracles for comparisons that cannot happen.
                if (!results[p].Phases.TryGetValue(Phase.Compile, out Status c) || c != Status.Pass)
                    continue;

                // Same circuit breaker as the C# fallback, and needed for the same reason: `go build` is
                // sequential here, so on a slow cold machine a systemically undersized budget would
                // otherwise be paid once per project. At the 600 s this change's own documentation
                // recommends for such a machine, that is days across the corpus -- with every result
                // discarded downstream anyway.
                if (budgetExhausted)
                {
                    string skipped = $"go build NOT ATTEMPTED: per-project budget ({Seconds(s_buildOneTimeoutMs)}) already exceeded {ConsecutiveTimeoutBailout}x consecutively";

                    s_goBuildTimedOut.Add(p);
                    results[p].Messages.Add(skipped);
                    s_goBuildFailures.Add($"{p}: {skipped}");
                    failed++;
                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);
                string goExeDir = Path.Combine(projPath, "bin", Config, "Go");
                Directory.CreateDirectory(goExeDir);

                if (!File.Exists(Path.Combine(projPath, "go.mod")))
                    Exec("go", $"mod init go2cs/{p}", projPath, s_buildOneTimeoutMs);

                ProcResult r = Exec("go", $"build -o \"{goExeDir}\"", projPath, s_buildOneTimeoutMs);

                if (r.ExitCode == 0)
                {
                    consecutiveTimeouts = 0;
                }
                else
                {
                    // A Go build that ran out of budget is labelled as such rather than left to read as
                    // a toolchain error, since the two want opposite responses (raise the budget vs.
                    // fix the Go source).
                    bool expired = TimedOut(r);

                    string detail = expired
                        ? $"go build TIMED OUT after {Seconds(s_buildOneTimeoutMs)} (raise --build-one-timeout)"
                        : $"go build exit {r.ExitCode}: {Truncate(r.StdErr)}";

                    (expired ? s_goBuildTimedOut : s_goBuildBroken).Add(p);
                    results[p].Messages.Add(detail);
                    s_goBuildFailures.Add($"{p}: {Truncate(detail, 200)}");
                    failed++;

                    if (!expired)
                    {
                        consecutiveTimeouts = 0;
                    }
                    else if (++consecutiveTimeouts >= ConsecutiveTimeoutBailout)
                    {
                        budgetExhausted = true;
                        Console.WriteLine();
                        Console.Error.WriteLine($"  {ConsecutiveTimeoutBailout} consecutive `go build` timeouts: the budget is too small for this machine.");
                        Console.Error.WriteLine($"  Skipping the remaining Go builds; their Output comparisons are NOT MEASURED.");
                    }
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunOutputComparison(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Output]   running C# vs Go, comparing exit code + stdout... ");
            int failed = 0, compared = 0, timeouts = 0;

            foreach (string p in projects)
            {
                if (!MatchConsoleOutput(p))
                {
                    results[p].Phases[Phase.Output] = Status.Skip;
                    continue;
                }

                // Require an explicit compile PASS rather than excluding known-bad statuses. A failed
                // compile means there is no C# exe to run; a timed-out or skipped one means there may
                // still be an exe on disk from an earlier run, and comparing that against today's Go
                // source scores a stale binary. Allow-listing the one good status is what makes a new
                // Status automatically safe here instead of silently falling through to a comparison.
                if (!results[p].Phases.TryGetValue(Phase.Compile, out Status c) || c != Status.Pass)
                {
                    results[p].Phases[Phase.Output] = Status.Skip;
                    continue;
                }

                // Same reasoning for the other side of the comparison: the Go binary is the ORACLE, so
                // a run whose `go build` did not succeed has nothing to measure against. Without this
                // the stale bin\Release\Go\<p>.exe from a previous run passes File.Exists below and the
                // comparison is scored as though both sides were current.
                if (s_goBuildTimedOut.Contains(p))
                {
                    results[p].Phases[Phase.Output] = Status.Timeout;
                    results[p].Messages.Add("no current Go oracle: go build timed out");
                    timeouts++;
                    continue;
                }

                if (s_goBuildBroken.Contains(p))
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add("no current Go oracle: go build failed");
                    failed++;
                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);
                string csExe = Path.Combine(projPath, "bin", Config, NetVersion, $"{p}{s_exeSuffix}");
                string goExe = Path.Combine(projPath, "bin", Config, "Go", $"{p}{s_exeSuffix}");
                string workDir = Path.GetDirectoryName(projPath)!;

                // Both binaries must exist by now: the project declared itself output-compared and its C#
                // compile PASSED, so an absent binary is a broken invariant, never a legitimate state.
                // Reporting it as a SKIP made that invisible -- Report counts a Skip as neither a failure
                // nor a reason to exit non-zero -- so the run stayed GREEN having compared nothing. That
                // is exactly the vacuous green the hard-coded ".exe" produced across the WHOLE corpus on
                // Linux, where every probe here missed (F4, docs/PLAN-linux-operation.md). The suffix
                // CAUSE is fixed above (s_exeSuffix); this closes the SYMPTOM, so the next way to lose a
                // binary -- a renamed AssemblyName, a moved output path, a new host -- cannot pass
                // silently the way that one did. The verdict follows the same document's 2026-08-08
                // coordinator ruling for this shape in check-no-regression.ps1: a gate that stopped
                // measuring fails BY NAME with exit 1, and being loud on a host where a project cannot
                // run is the honest outcome rather than a reason to stay quiet. Fail rather than a
                // not-measured bucket of its own because nothing here expired -- both builds ran to a
                // verdict and the artifact is still missing. The one legitimate no-binary case, a
                // library-style project with no `package main`, never reaches this line: MatchConsoleOutput
                // skipped it at the top of the loop. Neither does a failed or timed-out `go build` --
                // the oracle guards above intercept those by name first -- so what this catches is
                // exactly the C#-side losses (a renamed AssemblyName, a moved output path, a missing
                // apphost) and any exotic artifact loss after a build that reported success. The
                // message deliberately does NOT say "NOT MEASURED": on this runner that phrase heads
                // the Status.Timeout bucket, whose report says "these are NOT failures" -- and this one
                // is.
                bool haveCs = File.Exists(csExe), haveGo = File.Exists(goExe);

                if (!haveCs || !haveGo)
                {
                    string missing = !haveCs && !haveGo ? $"neither binary exists ({csExe}; {goExe})"
                        : !haveCs ? $"no C# binary at {csExe}"
                        : $"no Go binary at {goExe}";

                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add($"nothing to compare -- {missing} (its compile passed, so the artifact should exist)");
                    failed++;
                    continue;
                }

                ProcResult cs = Exec(csExe, null, workDir, s_runTimeoutMs);
                ProcResult go = Exec(goExe, null, workDir, s_runTimeoutMs);

                // Check for an expired budget BEFORE comparing, because Exec's timeout path returns
                // exit code -1 -- which the comparison below would otherwise report as
                // "exit code mismatch: C# -1 vs Go 0", i.e. as a genuine behavioral divergence. That is
                // the same false red as the compile phase, and a more insidious one: it names a real
                // test and a plausible symptom. A transpiled program is slower than its Go original by
                // a wide and variable margin (see the maphash case in CLAUDE.md, ~15 min vs 7.6 s), so
                // on a slower machine the C# side is exactly what runs out of budget first.
                if (TimedOut(cs) || TimedOut(go))
                {
                    string side = TimedOut(cs) && TimedOut(go) ? "both" : TimedOut(cs) ? "C#" : "Go";

                    results[p].Phases[Phase.Output] = Status.Timeout;
                    results[p].Messages.Add($"run TIMED OUT after {Seconds(s_runTimeoutMs)} ({side} side); raise --run-timeout");
                    timeouts++;
                    continue;
                }

                // Counted only once both sides have actually run to completion, so the reported figure
                // is comparisons PERFORMED. Incrementing before the timeout check above made a run where
                // every attempt expired report "20 compared, 0 failed, 20 timed out" -- asserting twenty
                // comparisons that never happened.
                compared++;

                // The Go binary is the oracle: exit codes must MATCH rather than both be zero, so a
                // program that legitimately crashes (e.g. an unrecovered panic exits 2, like Go) is
                // validated differentially instead of being rejected outright. stderr is compared by
                // FIRST LINE only: Go's panic report appends a machine-specific goroutine stack
                // trace, so a full comparison can never match; the first line carries the
                // deterministic report and is empty for clean runs.
                if (cs.ExitCode != go.ExitCode)
                {
                    // Name the CAUSE, not just the symptom. An exit-code mismatch is overwhelmingly a
                    // crash on the C# side, and golib's unrecovered-panic handler has already written
                    // the reason to stderr before exiting 2 (builtin.cs) -- the panic value for a Go
                    // panic, the full exception chain for a managed fault. Reporting the bare code
                    // discarded that text while holding it in hand, which is how the first darwin
                    // behavioral-smoke run (2026-08-25) reported twenty identical
                    // "exit code mismatch: C# 2 vs Go 0" lines and named none of the twenty causes --
                    // a whole CI leg whose log could not distinguish a corpus-flavor error from a
                    // missing syscall from a startup fault. The mismatch branch is exactly where the
                    // evidence matters MOST and was the one branch not printing it: the two branches
                    // below already quote their diff. Both sides are quoted because either can be the
                    // crashing one (a C#-side success against a Go-side failure is a real shape, e.g.
                    // an oracle that cannot run on this host), and an empty stderr is itself a finding
                    // -- it says the process died without reporting, which points at the host rather
                    // than at converted code. StdErrSummary rather than FirstLine: the first line of a
                    // WRAPPED managed failure names only the wrapper, which is the same evidence loss
                    // one layer in (see that helper).
                    string csErr = StdErrSummary(cs.StdErr), goErr = StdErrSummary(go.StdErr);

                    // A wider budget than Truncate's 300-char default ON PURPOSE: this summary
                    // now carries the innermost cause AND its first frames, and the frames are the
                    // half that names the caller. Truncating them away would leave the report
                    // saying exactly what it said before this was worth changing.
                    string detail = csErr.Length == 0 && goErr.Length == 0
                        ? " (neither side wrote to stderr)"
                        : $" -- C# stderr: \"{Truncate(csErr, MismatchStdErrBudget)}\"; Go stderr: \"{Truncate(goErr, MismatchStdErrBudget)}\"";

                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add($"exit code mismatch: C# {cs.ExitCode} vs Go {go.ExitCode}{detail}");
                    failed++;
                }
                else if (!string.Equals(cs.StdOut, go.StdOut, StringComparison.Ordinal))
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add("stdout mismatch C# vs Go");
                    failed++;
                }
                else if (!string.Equals(FirstLine(cs.StdErr), FirstLine(go.StdErr), StringComparison.Ordinal))
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add($"stderr first-line mismatch: C# \"{FirstLine(cs.StdErr)}\" vs Go \"{FirstLine(go.StdErr)}\"");
                    failed++;
                }
                else
                {
                    results[p].Phases[Phase.Output] = Status.Pass;
                }
            }

            Console.WriteLine($"{compared} compared, {failed} failed{(timeouts > 0 ? $", {timeouts} NOT MEASURED (timed out)" : "")}");
        }

        // Copies each project's freshly transpiled .cs over its .cs.target golden. Only projects whose
        // transpile PASSED are touched; the rest are reported through `refused` so the caller can fail
        // the run rather than silently baking unmeasured output into the goldens. `results` used to be
        // an unread parameter here, which is precisely how that hole stayed open.
        //
        // The transpile this reads was FORCED (s_forceTranspile is set by --update-targets), so
        // "Pass" here means the converter ran this second on this project -- not that its .cs happened
        // to be newer than its .go. Both refusals below therefore describe THIS run: a non-Pass status
        // is a transpile that failed or was killed, and a best-effort marker is a transpile that
        // exited 0 having degraded, which is the one an exit code cannot see.
        private static int UpdateTargets(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, out List<string> refused)
        {
            refused = new List<string>();
            int updated = 0;

            foreach (string p in projects)
            {
                // ONE question, asked of ONE representation. Transpile is Pass only when the converter
                // ran to completion on every package of this project AND did not say the emission was
                // degraded -- RunTranspile's BestEffort status carries that second half, so there is no
                // second best-effort test here to drift away from it. The status also NAMES the cause,
                // which matters because the three remedies differ: a bigger budget, a fixed converter,
                // or a host that can type-check the package.
                if (!results[p].Phases.TryGetValue(Phase.Transpile, out Status t) || t != Status.Pass)
                {
                    refused.Add($"{p} -- transpile {t switch
                    {
                        Status.Timeout    => "TIMED OUT",
                        Status.BestEffort => "was NOT MEASURED (best-effort conversion; the converter could not fully type-check it)",
                        _                 => "did not complete"
                    }}");

                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);

                foreach (string go in ProductionGoFiles(projPath))
                {
                    string cs = Path.ChangeExtension(go, ".cs");

                    if (File.Exists(cs))
                        File.Copy(cs, cs + ".target", overwrite: true);
                }

                updated++;
            }

            return updated;
        }

        // Builds the deduped union of ProjectReferences across the target csprojs (golib, the analyzer,
        // core/* packages) one at a time, so they are up to date before the parallel target fan-out.
        private static void PreBuildSharedDeps(IReadOnlyList<string> projects, string go2csPathArg)
        {
            HashSet<string> deps = new(StringComparer.OrdinalIgnoreCase);

            foreach (string p in projects)
            {
                string csprojDir = Path.Combine(s_behavioralDir, p);

                // Read the csproj AND the project-local Directory.Build.props/.targets, honouring both
                // Include and Remove, in the same order MSBuild imports them: props, then the project
                // body, then targets -- so a Remove in targets cancels an Include from the csproj,
                // exactly as it does in a real build. A test can legitimately redirect a
                // converter-generated reference from a Directory.Build.targets, because the csproj
                // itself is template output the converter rewrites on every re-transpile. Reading the
                // csproj alone therefore both chased a phantom path (MSB1009 "Project file does not
                // exist") and stayed blind to the real closure the test pulls in -- leaving that closure
                // unbuilt during the parallel fan-out, precisely the race this pre-build prevents.
                foreach (string file in new[] { "Directory.Build.props", $"{p}.csproj", "Directory.Build.targets" })
                {
                    string path = Path.Combine(csprojDir, file);

                    if (!File.Exists(path))
                        continue;

                    foreach (string line in File.ReadLines(path))
                    {
                        string? include = AttributeValue(line, "ProjectReference Include=\"");
                        string? remove = AttributeValue(line, "ProjectReference Remove=\"");

                        // Resolve relative to the csproj's OWN directory (not the runner CWD) so a relative
                        // cross-project ProjectReference (e.g. a cross-package test's `..\lib\lib.csproj`)
                        // resolves correctly instead of producing a phantom path + MSB1009 warning.
                        if (include is not null)
                            deps.Add(Path.GetFullPath(Expand(include), csprojDir));

                        if (remove is not null)
                            deps.Remove(Path.GetFullPath(Expand(remove), csprojDir));
                    }
                }
            }

            // A reference that still does not exist is a genuine wiring defect, not something to spend a
            // build call discovering -- report it plainly instead of via an opaque MSB1009.
            foreach (string missing in deps.Where(d => !File.Exists(d)).ToList())
            {
                Console.Error.WriteLine($"\n  WARNING: unresolved ProjectReference (skipped): {missing}");
                deps.Remove(missing);
            }

            Console.Write($"[Compile]  pre-building {deps.Count} shared dependencies... ");

            foreach (string dep in deps)
            {
                ProcResult r = Exec("dotnet",
                    $"build \"{dep}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_behavioralDir, s_buildOneTimeoutMs);

                if (TimedOut(r))
                {
                    // This one deserves its own wording: the pre-build is what makes the shared closure
                    // up to date before the fan-out, so a dep that ran out of budget here guarantees the
                    // whole batch that follows will fail too -- and it is the FIRST thing a cold tree on
                    // a slow machine hits, since a single core package can take a minute of its own.
                    Console.Error.WriteLine($"\n  WARNING: shared dep build TIMED OUT after {Seconds(s_buildOneTimeoutMs)} ({Path.GetFileName(dep)});");
                    Console.Error.WriteLine($"           the batch build below will almost certainly fail as a consequence. Raise --build-one-timeout.");
                }
                else if (r.ExitCode != 0)
                {
                    Console.Error.WriteLine($"\n  WARNING: shared dep build failed ({Path.GetFileName(dep)}): {Truncate(r.StdOut + r.StdErr)}");
                }

                // Record how fresh the dependency outputs are, for the staleness test in SuspectProjects.
                string depBin = Path.Combine(Path.GetDirectoryName(dep)!, "bin");

                if (!Directory.Exists(depBin))
                    continue;

                foreach (string dll in Directory.EnumerateFiles(depBin, "*.dll", SearchOption.AllDirectories))
                {
                    DateTime stamp = File.GetLastWriteTimeUtc(dll);

                    if (stamp > s_sharedDepStamp)
                        s_sharedDepStamp = stamp;
                }
            }

            Console.WriteLine("ok");
        }

        // Value of a double-quoted XML attribute on a line, or null when the attribute is absent.
        private static string? AttributeValue(string line, string attribute)
        {
            int idx = line.IndexOf(attribute, StringComparison.OrdinalIgnoreCase);

            if (idx < 0)
                return null;

            int start = idx + attribute.Length;
            int end = line.IndexOf('"', start);

            return end < 0 ? null : line[start..end];
        }

        // Expands the one MSBuild property the behavioral csprojs use in a ProjectReference path.
        private static string Expand(string path) =>
            path.Replace("$(go2csPath)", s_srcRoot + Path.DirectorySeparatorChar);

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
                string csproj = Path.Combine(s_behavioralDir, p, $"{p}.csproj");
                sb.AppendLine($"    <ProjectToBuild Include=\"{csproj}\" />");
            }

            sb.AppendLine("  </ItemGroup>");
            // Global props (Configuration, go2csPath) passed on the command line propagate to each project.
            // ("Target" is a reserved MSBuild item name, hence ProjectToBuild.) RestoreAll and BuildAll are
            // separate targets because the runner drives them in separate processes -- see RunCompileCSharp.
            sb.AppendLine("  <Target Name=\"RestoreAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Restore\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("  <Target Name=\"BuildAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Build\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("</Project>");

            File.WriteAllText(projFile, sb.ToString());
            return projFile;
        }

        private static bool MatchConsoleOutput(string project)
        {
            string packageInfo = Path.Combine(s_behavioralDir, project, "package_info.cs");

            return File.Exists(packageInfo) &&
                   File.ReadLines(packageInfo).Any(l => l.Trim() == "[GoTestMatchingConsoleOutput]");
        }

        private static bool FilesEqual(string a, string b)
        {
            // Compare with line endings normalized (CRLF -> LF). The converter emits CRLF for C# line
            // endings but preserves the Go source's LF inside multi-line string literals, so a golden has
            // mixed CRLF/LF bytes; with core.autocrlf=true git rewrites those in-string LFs to CRLF on
            // checkout. Normalizing endings makes the comparison immune to that round-trip (a pure
            // line-ending diff can only come from autocrlf, never from the deterministic converter), so no
            // real regression signal is lost. NOTE: this relaxes only the golden TEXT comparison -- a
            // project whose COMPILED program embeds and observes a multi-line string literal at runtime
            // (e.g. Solitaire's board) still needs `-text` in .gitattributes so the on-disk .cs keeps LF
            // newlines, else autocrlf corrupts the runtime value.
            return NormalizeLineEndings(File.ReadAllBytes(a))
                .AsSpan()
                .SequenceEqual(NormalizeLineEndings(File.ReadAllBytes(b)));
        }

        // Returns the bytes with every CR (0x0D) removed, collapsing CRLF to LF. The transpiled C# never
        // contains a bare CR, so stripping all CRs is a safe, allocation-light line-ending normalization.
        private static byte[] NormalizeLineEndings(byte[] bytes)
        {
            int count = 0;

            foreach (byte b in bytes)
            {
                if (b != (byte)'\r')
                    bytes[count++] = b;
            }

            Array.Resize(ref bytes, count);
            return bytes;
        }

        private static int Report(IEnumerable<ProjectResult> results, TimeSpan elapsed)
        {
            List<ProjectResult> all = results.ToList();
            List<ProjectResult> failures = all.Where(r => r.HasFail).ToList();

            // Reported separately from the failures, and only when a project has NO real failure --
            // a project that genuinely broke somewhere belongs under "failing", where the break is the
            // actionable fact, not under "not measured".
            List<ProjectResult> notMeasured = all.Where(r => !r.HasFail && r.HasUnmeasured).ToList();

            int Count(Phase ph, Status st) => all.Count(r => r.Phases.TryGetValue(ph, out Status s) && s == st);

            Console.WriteLine();
            Console.WriteLine("================ summary ================");
            foreach (Phase ph in Enum.GetValues<Phase>())
            {
                int pass = Count(ph, Status.Pass), fail = Count(ph, Status.Fail);
                int skip = Count(ph, Status.Skip), timeout = Count(ph, Status.Timeout);
                int bestEffort = Count(ph, Status.BestEffort);

                if (pass + fail + skip + timeout + bestEffort == 0) continue;

                // The best-effort column is appended only when it is non-zero, unlike the other four.
                // Only Transpile can ever produce it, so a permanent fifth column would print a
                // structural zero on three of the four rows -- noise that reads like a measurement.
                string degraded = bestEffort > 0 ? $"   best-effort {bestEffort,4}" : "";

                Console.WriteLine($"  {ph,-9}  pass {pass,4}   fail {fail,4}   skip {skip,4}   timeout {timeout,4}{degraded}");
            }

            if (failures.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"---- {failures.Count} failing project(s) ----");
                foreach (ProjectResult r in failures.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
                {
                    string phs = string.Join(",", r.Phases.Where(kv => kv.Value == Status.Fail).Select(kv => kv.Key));
                    Console.WriteLine($"  {r.Name} [{phs}]");
                    foreach (string m in r.Messages)
                        Console.WriteLine($"      {m}");
                }
            }

            if (notMeasured.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"---- {notMeasured.Count} project(s) NOT MEASURED ----");
                Console.WriteLine("  These are NOT failures: no tool reported them broken. This run proves nothing about them");
                Console.WriteLine("  either way, so it is reported as FAIL rather than silently passed. Two causes, opposite");
                Console.WriteLine("  remedies -- each project below is tagged with the one that applies:");
                Console.WriteLine("   * timeout      -- a budget expired and the runner stopped waiting. Raise it and re-run:");
                Console.WriteLine("                     --build-timeout / --build-one-timeout / --transpile-timeout / --run-timeout,");
                Console.WriteLine("                     or GO2CS_BUILD_TIMEOUT / GO2CS_BUILD_ONE_TIMEOUT / GO2CS_RUN_TIMEOUT.");
                Console.WriteLine("   * best-effort  -- the converter could not fully type-check the package (or had to skip a");
                Console.WriteLine("                     source file) and emitted degraded output, exiting 0. A bigger budget");
                Console.WriteLine("                     cannot help: convert on a host that can type-check it, or -- when the");
                Console.WriteLine("                     package is native to another platform -- mark it [GoPlatformExclusive]");
                Console.WriteLine("                     so it is skipped BY NAME before transpile instead (F8).");

                // Cap the roster: the pathological case is the whole corpus timing out, and 555 identical
                // lines bury the diagnosis above them rather than adding to it.
                foreach (ProjectResult r in notMeasured.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).Take(20))
                {
                    // The phase is tagged with WHICH of the two it was. Printing a bare phase name would
                    // put the reader back where the merged heading left them: knowing the project was not
                    // measured and not which of the two opposite remedies to reach for.
                    string phs = string.Join(",", r.Phases
                        .Where(kv => kv.Value is Status.Timeout or Status.BestEffort)
                        .Select(kv => $"{kv.Key}:{(kv.Value == Status.Timeout ? "timeout" : "best-effort")}"));

                    Console.WriteLine($"  {r.Name} [{phs}]");

                    // The converter's own words, for the best-effort rows only: the roster is capped and
                    // the failure roster above already prints its messages, but a best-effort project has
                    // no other account anywhere of WHAT did not type-check.
                    if (r.HasBestEffort)
                    {
                        foreach (string m in r.Messages.Where(m => m.Contains("NOT MEASURED", StringComparison.Ordinal)))
                            Console.WriteLine($"      {m}");
                    }
                }

                if (notMeasured.Count > 20)
                    Console.WriteLine($"  ... and {notMeasured.Count - 20} more.");
            }

            if (s_goBuildFailures.Count > 0)
            {
                // "problem(s)", not "failure(s)": this roster now carries expired budgets alongside real
                // toolchain errors, and heading a timeout as a failure is the same conflation the rest of
                // this report exists to undo. The per-entry text says which each one is.
                string expired = s_goBuildTimedOut.Count > 0 ? $" ({s_goBuildTimedOut.Count} timed out)" : "";

                Console.WriteLine();
                Console.WriteLine($"---- {s_goBuildFailures.Count} Go build problem(s){expired} ----");
                foreach (string m in s_goBuildFailures)
                    Console.WriteLine($"  {m}");
            }

            bool ok = failures.Count == 0 && notMeasured.Count == 0 && s_goBuildFailures.Count == 0;

            // The headline distinguishes the two, because they call for opposite responses: a real
            // failure is investigated, an unmeasured run is re-run with a bigger budget. The arms key
            // off `failures` and `notMeasured` ONLY -- an earlier version also required
            // s_goBuildFailures to be empty for the "purely unmeasured" arm, which meant a run whose
            // only problem was expired `go build` budgets fell through to the mixed arm and announced
            // the self-contradictory "FAIL (0 failing, N not measured)".
            string verdict = ok ? "PASS"
                : failures.Count == 0 && notMeasured.Count > 0 ? $"FAIL (NOT MEASURED: {notMeasured.Count})"
                : notMeasured.Count > 0 ? $"FAIL ({failures.Count} failing, {notMeasured.Count} not measured)"
                : "FAIL";

            Console.WriteLine();
            Console.WriteLine($"{verdict}  ({all.Count} projects, {elapsed.TotalSeconds:N1}s)");
            return ok ? 0 : 1;
        }

        // Returns null on any unrecognized token or an empty result, which the caller turns into a
        // usage error. Warning and continuing (as this did) meant a single typo -- `--phase compil` --
        // produced an EMPTY phase set: every phase guard in Main is then false, no project acquires any
        // status, and Report finds no failures and prints `PASS (555 projects, 0.1s)` with exit 0. That
        // is a vacuous run reported as a green one, the same class of hazard the enumeration floor in
        // Main exists to catch, and it is far likelier to be a mistyped flag than a deliberate no-op.
        private static HashSet<Phase>? ParsePhases(string csv)
        {
            HashSet<Phase> set = new();

            foreach (string token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (token.ToLowerInvariant())
                {
                    case "transpile": set.Add(Phase.Transpile); break;
                    case "compile": set.Add(Phase.Compile); break;
                    case "target": set.Add(Phase.Target); break;
                    case "output": set.Add(Phase.Output); break;
                    case "all": set.UnionWith(Enum.GetValues<Phase>()); break;
                    default:
                        Console.Error.WriteLine($"Unknown phase: {token}");
                        return null;
                }
            }

            if (set.Count == 0)
            {
                Console.Error.WriteLine($"--phase '{csv}' selects no phases; a run that measures nothing would report a vacuous pass.");
                return null;
            }

            return set;
        }

        private static void PrintUsage()
        {
            Console.WriteLine($"""
                BehavioralRunner -- standalone go2cs behavioral test runner (no testhost).

                Usage:
                  BehavioralRunner [--filter <substr>] [--phase <list>] [--update-targets] [--list]
                                   [--build-timeout <sec>] [--build-one-timeout <sec>]
                                   [--transpile-timeout <sec>] [--run-timeout <sec>]

                Options:
                  --filter <substr>     Only projects whose name contains <substr> (case-insensitive).
                  --slice <i>/<n>       Run only the i-th of n contiguous, disjoint pieces of the
                                        enumeration (1-based), applied after platform-exclusive
                                        skipping. Unlike --filter (a SUBSTRING match, which cannot
                                        partition), slices are disjoint and their sizes sum to the
                                        measurable total. Prints "SLICE i/n: k of m measurable
                                        projects" for the caller to sum and assert.
                  --phase <list>        Comma list of: transpile,compile,target,output,all (default all).
                  --update-targets      Re-transpile UNCONDITIONALLY (no up-to-date skip), then copy
                                        each .cs to its .cs.target golden, and stop. A project whose
                                        transpile failed, timed out, or exited 0 having converted
                                        best-effort is REFUSED by name and its golden left alone; the
                                        run then exits non-zero.
                  --list                List matched projects and exit.
                  -h, --help            Show this help.

                Timeout budgets (SECONDS; flag > environment variable > default):
                  --build-timeout       One-shot parallel build of every C# target.
                                        Env GO2CS_BUILD_TIMEOUT, default {DefaultBuildAllTimeoutMs / 1000}.
                  --build-one-timeout   Per-project fallback build, shared-dependency pre-build, go build.
                                        Env GO2CS_BUILD_ONE_TIMEOUT, default {DefaultBuildOneTimeoutMs / 1000}.
                  --transpile-timeout   One converter invocation.
                                        Env GO2CS_TRANSPILE_TIMEOUT, default {DefaultTranspileTimeoutMs / 1000}.
                  --run-timeout         One transpiled-program or Go-binary run in the Output phase.
                                        Env GO2CS_RUN_TIMEOUT, default {DefaultRunTimeoutMs / 1000}.

                  The defaults are sized for the fast desktop this suite is baselined on, so that lane
                  keeps failing fast. A slower machine (or a larger corpus) needs them raised: exceeding
                  a budget is reported as NOT MEASURED, never as a compile or behavioral failure, but it
                  still fails the run -- an unmeasured project must not read as a pass.

                Exit code 0 = all matched projects pass; 1 = at least one failure or unmeasured project;
                2 = usage error.
                """);
        }

        // ---- timeout budget resolution ----

        // Flag > environment > default. A malformed FLAG is a usage error (TryParseSeconds returns
        // false and Main exits 2) because it was typed deliberately; a malformed ENVIRONMENT value only
        // warns and falls back, because it may have been inherited from a shell the caller did not set
        // up. Either way the resolved value is echoed in the run header, so a misread cannot stay
        // invisible -- which is the failure mode this whole mechanism exists to end.
        private static int ResolveTimeout(int? flagMs, string environmentVariable, int defaultMs)
        {
            if (flagMs is not null)
                return flagMs.Value;

            string? setting = Environment.GetEnvironmentVariable(environmentVariable);

            if (string.IsNullOrWhiteSpace(setting))
                return defaultMs;

            if (TryParseSeconds(setting, environmentVariable, out int fromEnvironment))
                return fromEnvironment;

            Console.Error.WriteLine($"  WARNING: ignoring {environmentVariable}; using the default {Seconds(defaultMs)}.");
            return defaultMs;
        }

        // Parses a whole number of SECONDS into milliseconds. The unit is the trap here: every budget
        // is stored in ms and was written as ms when these were constants, so a caller reaching for
        // "300000" out of habit would silently ask for 300 ms -- a budget nothing can meet, producing a
        // total-timeout run that looks like catastrophic breakage. Anything above a day is therefore
        // rejected as a suspected millisecond value rather than honored.
        private static bool TryParseSeconds(string text, string option, out int milliseconds)
        {
            milliseconds = 0;

            if (!int.TryParse(text, out int seconds))
            {
                Console.Error.WriteLine($"{option}: '{text}' is not a whole number of seconds.");
                return false;
            }

            if (seconds <= 0)
            {
                Console.Error.WriteLine($"{option}: must be greater than zero (got {seconds}).");
                return false;
            }

            if (seconds > 86_400)
            {
                Console.Error.WriteLine($"{option}: {seconds} exceeds 24 hours -- this option is in SECONDS, not milliseconds.");
                return false;
            }

            milliseconds = seconds * 1000;
            return true;
        }

        // True only for a ProcResult produced by Exec's timeout path, never by a child exiting on its
        // own -- see the note on ProcResult for why this is a field and not a heuristic.
        private static bool TimedOut(in ProcResult result) => result.TimedOut;

        private static string Seconds(int milliseconds) => $"{milliseconds / 1000}s";

        // Renders a budget, marking it when it is not the built-in default, so the header line shows at
        // a glance whether a run was given a custom budget or inherited one from the environment.
        private static string Describe(int milliseconds, int defaultMs) =>
            milliseconds == defaultMs ? Seconds(milliseconds) : $"{Seconds(milliseconds)} (overridden)";

        // ---- process execution with timeout + whole-tree kill ----

        // TimedOut is carried as a FIELD rather than inferred from the exit code and a stderr prefix.
        // Sniffing for "exit -1 and stderr starting with TIMEOUT" very nearly works -- Exec is the only
        // producer and it writes that marker itself -- but the non-timeout return passes the child's
        // stderr through verbatim, so a child that exits -1 (0xFFFFFFFF, which .NET surfaces as -1 on
        // Windows) while its first stderr line happens to begin with "TIMEOUT" would be indistinguishable
        // from a real expiry. No program in this corpus does that, but the whole point of this verdict is
        // that infrastructure and corpus signals must never be confusable, and a flag costs nothing.
        private readonly record struct ProcResult(int ExitCode, string StdOut, string StdErr, bool TimedOut = false);

        private static ProcResult Exec(string application, string? arguments, string workingDir, int timeoutMs)
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
                return new ProcResult(-1, outBuf.ToString(), $"TIMEOUT after {timeoutMs} ms; killed process tree.\n{errBuf}", TimedOut: true);
            }

            process.WaitForExit();
            return new ProcResult(process.ExitCode, outBuf.ToString(), errBuf.ToString());
        }

        private static string FirstLine(string text)
        {
            int index = text.IndexOf('\n');

            return (index < 0 ? text : text[..index]).TrimEnd('\r');
        }

        /// <summary>
        /// A crashed process's stderr reduced to its first line PLUS the inner-exception chain that
        /// line hides, for reporting a run that died rather than diverged.
        /// </summary>
        /// <remarks>
        /// FirstLine alone is the right reduction for COMPARING stderr (the rest is a machine-specific
        /// traceback), but the wrong one for REPORTING a managed crash, because .NET's outermost line
        /// is frequently just a wrapper: `System.TypeInitializationException: The type initializer for
        /// '&lt;Module&gt;' threw an exception.` names no cause at all, and a module initializer is
        /// exactly where a converted program fails first. golib's own crash handler learned this and
        /// writes `ex.ToString()` for precisely that reason (builtin.cs) — this is the reading half of
        /// the same lesson: taking line one threw the chain away again at the last step. The darwin
        /// smoke of 2026-08-25 is the worked example — twenty programs reporting the wrapper, with
        /// `NotImplementedException: syscall: external (assembly or cgo) function is not implemented`
        /// sitting one `---&gt;` line below it.
        ///
        /// `--->` is the framework's own nesting marker in ToString() output, so keying on it needs no
        /// exception types here and works for any depth.
        ///
        /// Reported as the outermost line plus the INNERMOST cause, not the whole chain. Taking the
        /// first few levels instead was tried and is wrong: managed startup failures nest wrappers of
        /// the SAME type, so the darwin smoke's real chain reads `'&lt;Module&gt;'` → `'&lt;Module&gt;'`
        /// → `'go.os_package'` → the actual fault, and quoting from the top spent the whole line
        /// budget on three TypeInitializationExceptions and truncated the one exception that names
        /// what broke. The outermost line says where the program died and the innermost says why;
        /// everything between is plumbing. The intervening depth is reported as a count so a deep
        /// chain is still visible as deep.
        /// </remarks>
        private static string StdErrSummary(string text)
        {
            string first = FirstLine(text);

            if (first.Length == 0)
            {
                return first;
            }

            List<string> inner = [];
            List<string> frames = [];
            bool collecting = false;

            foreach (string line in text.Replace("\r", "").Split('\n'))
            {
                string trimmed = line.TrimStart();

                if (trimmed.StartsWith("---> ", StringComparison.Ordinal))
                {
                    inner.Add(trimmed);

                    // A new innermost cause supersedes whatever frames were collected for the
                    // previous one: only the LAST `--->` is reported, so only its frames are wanted.
                    frames.Clear();
                    collecting = true;
                    continue;
                }

                if (!collecting)
                {
                    continue;
                }

                if (trimmed.StartsWith("at ", StringComparison.Ordinal))
                {
                    if (frames.Count < MaxReportedFrames)
                    {
                        frames.Add(trimmed);
                    }

                    continue;
                }

                // Anything that is neither a frame nor a nesting marker ends this exception's
                // stack -- `--- End of inner exception stack trace ---` above all.
                collecting = false;
            }

            string cause = inner.Count switch
            {
                0 => first,
                1 => $"{first} {inner[^1]}",
                _ => $"{first} [+{inner.Count - 1} nested] {inner[^1]}"
            };

            return frames.Count == 0 ? cause : $"{cause} || {string.Join(" | ", frames)}";
        }

        /// <summary>
        /// How many frames of the innermost exception's stack to carry in a mismatch report.
        /// </summary>
        /// <remarks>
        /// Four is enough to name the failing leaf and the converted Go function that called it,
        /// which is the question a stub-throw failure actually poses -- WHICH caller reached the
        /// unimplemented entry point first. The darwin run-layer probe of 2026-09-02 is the worked
        /// example of needing it: the report read `NotImplementedException: rawSyscall: external
        /// (assembly or cgo) function is not implemented` and could name neither the Go function
        /// that called `rawSyscall` nor the package initializer above it, so the design record had
        /// to leave "the first failing call" unpinned. The frames were in the text the whole time --
        /// golib's crash handler writes `ex.ToString()`, which carries them -- and this helper threw
        /// them away one line before they were read, which is the same evidence loss the `---&gt;`
        /// reading was minted to fix, one layer further in.
        ///
        /// Bounded rather than unbounded because a mismatch report is one line in a summary: an
        /// unbounded stack would push the cause itself past the caller's truncation, re-creating
        /// the problem in the other direction.
        /// </remarks>
        private const int MaxReportedFrames = 4;

        /// <summary>
        /// Character budget for each side's stderr in an exit-code-mismatch report.
        /// </summary>
        private const int MismatchStdErrBudget = 900;

        private static string Truncate(string s, int max = 300)
        {
            s = s.Replace("\r", "").Replace("\n", " ").Trim();
            return s.Length <= max ? s : s[..max] + " ...";
        }
    }
}
