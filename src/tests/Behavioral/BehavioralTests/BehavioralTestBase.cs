// BehavioralTestBase.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Comment out the following line to standard dotnet builds instead of publish profiles for C# projects.
// Using publish profiles can increase run-time startup performance, after first run initialization, but
// increases build times due to extra compile time processing, e.g. trimming analysis, etc.
//#define USE_PUBLISH_PROFILES

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Don't enable for timing tests:
//[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
// A second reason, from outside this assembly: uncommenting the line above is a DESIGN QUESTION for
// the four junction-GODEBUG statics in testing's PackageAncestry.cs, whose single
// capture-and-restore pairing assumes these hosts run one at a time. The lock those statics take
// closes corruption OF THE STATICS, not of the pairing: with two hosts overlapping, host B skips its
// apply because host A's winsymlink=0 is already in the environment, then A's restore retracts the
// setting while B's junctions are still staged, and B's later toolchain calls refuse their
// internal/... imports again -- attributed to nobody. So the statics themselves are safe, and what is
// owed there first is a reading of whether ONE process-wide capture is the right shape when several
// hosts stage at once, or whether it has to become per-host; the lock only makes that a design
// question rather than a corruption.

namespace BehavioralTests;

[TestClass]
public abstract class BehavioralTestBase
{
    // Move up from here: ".../go2cs/src/tests/Behavioral/BehavioralTests/bin/Debug/net9.0" to "src".
    //
    // Built from Path.Combine SEGMENTS rather than an embedded @"..\..\..\..\..\..\..\": .NET does
    // NOT normalize a backslash on Unix, so that literal is ONE directory name there and every path
    // derived from it points at nothing (F4, docs/PLAN-linux-operation.md).
    //
    // SIX, where the literal spelled seven -- and the discrepancy is the point. The old form was
    // concatenated, not combined ($@"{execPath}{RootPath}go2cs\"), and execPath carries no trailing
    // separator, so its last segment fused with the first "..": "net9.0" + ".." = "net9.0..".
    // Windows path normalization strips trailing dots from a segment, turning that back into
    // "net9.0" and EATING one level, so seven written levels resolved as six. Measured, not
    // reasoned: GetFullPath(execPath + @"..\..\..\..\..\..\..\" + @"go2cs\") is <repo>\src\go2cs.
    // Nothing off Windows performs that strip -- "net9.0.." would be a literal directory name that
    // does not exist -- so the accident had to be resolved into the real level count to port at all.
    private static readonly string RootPath = Path.Combine("..", "..", "..", "..", "..", ".."); // At "src" folder

    // Trailing separator retained: two derived test classes interpolate this directly as
    // $"{TestRootPath}{targetProject}", so the separator is load-bearing at those call sites.
    protected static readonly string TestRootPath = Path.Combine("..", "..", "..", "..") + Path.DirectorySeparatorChar;

    // Executable suffix for a built .NET apphost or Go binary. Windows only; empty everywhere else.
    // A hard-coded ".exe" is not a cosmetic wart here -- CompileCSProject probes File.Exists on it to
    // decide whether a build can be skipped, and OutputComparisonTests runs it.
    private static readonly string s_exeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";

    // Build-output path fragments, from the host's own separator. The literals @"\bin\" / @"\obj\"
    // these replaced do not ERROR off Windows -- they simply never match, so bin/ and obj/ enumerate
    // as Go package directories and get transpiled.
    private static readonly string s_binFragment = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
    private static readonly string s_objFragment = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";

    protected static string BinOutput { get; private set; } = null!;

    protected static string NetVersion { get; private set; } = null!;

    protected static string go2cs { get; private set; } = null!;

    // The converter's -go2cspath: the root it reads an imported package's package_info.cs from when it
    // mints the emitted <ImportedTypeAliases> block. NOT the MSBuild $(go2csPath) property of the same
    // name (that one is set in CompileCSProject). Its default is ~/go2cs, so leaving it ambient made the
    // transpiled output -- and with it the golden comparison's verdict -- depend on whatever GO2CSPATH the
    // shell carried (BOARD-next-validation-candidates.md, 2026-08-06). Pinned here to the src root derived
    // from this assembly's own location: the behavioral .csproj files bind $(go2csPath)core\<pkg> with
    // MSBuild $(go2csPath) -> $(SolutionDir) -> src\, so src\core is what they compile against and src\ is
    // the only root whose metadata describes those assemblies.
    protected static string Go2csRoot { get; private set; } = null!;

    // The host's own RID rather than a pinned "win-x64" -- "win-x64" on this lane, "linux-x64" or
    // "osx-arm64" elsewhere. Only consumed under USE_PUBLISH_PROFILES (off by default).
    protected static string PublishProfile { get; private set; } = RuntimeInformation.RuntimeIdentifier;

    protected static string TargetConfig { get; private set; } = "Release";

    // Default timeout for build/transpile child processes (ms). Matches the ~3 min suite cap from
    // CLAUDE.md; a child that exceeds this is treated as hung and killed (with its whole process tree)
    // rather than blocking the suite forever via an unbounded WaitForExit.
    protected const int DefaultExecTimeoutMs = 180_000;

    // Tighter timeout for *running* a transpiled program. A deadlocked converted program (channel/
    // goroutine hang, a process blocked on stdin) is the worst offender: it must fail one test fast,
    // not wedge the run and leave an orphaned testhost holding a lock on BehavioralTests.dll.
    protected const int RunExecTimeoutMs = 30_000;

    protected static string GetCSExecPath(string projPath, string targetProject)
    {
    #if USE_PUBLISH_PROFILES
        return Path.Combine(projPath, "bin", TargetConfig, NetVersion, "publish", PublishProfile);
    #else
        return Path.Combine(projPath, "bin", TargetConfig, NetVersion);
    #endif
    }

    protected static string GetGoExePath(string projPath, string targetProject)
    {
        return Path.Combine(projPath, "bin", TargetConfig, "Go");
    }

    // The built C#/Go binary for a project, under the paths above. Centralized so the exe suffix is
    // decided once rather than at each of the four sites that used to spell ".exe".
    protected static string GetCSExeFile(string projPath, string targetProject) =>
        Path.Combine(GetCSExecPath(projPath, targetProject), $"{targetProject}{s_exeSuffix}");

    protected static string GetGoExeFile(string projPath, string targetProject) =>
        Path.Combine(GetGoExePath(projPath, targetProject), $"{targetProject}{s_exeSuffix}");

    private static readonly ConcurrentDictionary<string, object> s_projectLocks = new(StringComparer.OrdinalIgnoreCase);

    // Projects whose transpile this process found BEST-EFFORT, with the converter lines that said so.
    // Written once by TranspileProject and read by every later call for the same project -- see the
    // memo note there for why the up-to-date check alone would otherwise let the three phases below
    // Transpile pass on output the converter itself calls degraded.
    private static readonly ConcurrentDictionary<string, string[]> s_degradedProjects = new(StringComparer.OrdinalIgnoreCase);

    // How many times THIS process ran the converter over each project. Read by TranspileMemoTests, which holds the
    // harness to one transpile per project per process however many test classes ask for it.
    private static readonly ConcurrentDictionary<string, int> s_converterRuns = new(StringComparer.OrdinalIgnoreCase);

    internal static int ConverterRuns(string targetProject) => s_converterRuns.TryGetValue(targetProject, out int runs) ? runs : 0;

    // Projects this process has transpiled successfully. Every later UNFORCED call for one of them returns at once: the
    // converter and the .go are frozen for the life of the process (Init builds the converter once), so a second run
    // can only rewrite what the first wrote. See the memo check in TranspileProject.
    private static readonly ConcurrentDictionary<string, bool> s_transpiledProjects = new(StringComparer.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected static void Init(TestContext context)
    {
        string execPath = Directory.GetCurrentDirectory();
        string go2csSrc = Path.GetFullPath(Path.Combine(execPath, RootPath, "go2cs"));
        string go2csBin = Path.Combine(go2csSrc, "bin");

        // Resolved before the up-to-date early return below, so every path through Init sets it. The
        // trailing separator must go: a Windows command line ends the quoted argument on the closing
        // quote, and a backslash immediately before it escapes that quote instead.
        Go2csRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Combine(execPath, RootPath)));

        int projectNameIndex = execPath.IndexOf(nameof(BehavioralTests), StringComparison.OrdinalIgnoreCase);

        // Split on BOTH separators. The @"\" this replaced does not error off Windows -- it simply
        // never splits, so NetVersion silently becomes the whole "/bin/Debug/net9.0" tail and every
        // build-output path built from it points nowhere.
        BinOutput = execPath[(projectNameIndex + nameof(BehavioralTests).Length)..];
        NetVersion = BinOutput.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries)[^1];

        if (!Directory.Exists(go2csBin))
            Directory.CreateDirectory(go2csBin);

        go2cs = Path.Combine(go2csBin, $"go2cs{s_exeSuffix}");

        // If the exe is newer than every one of the converter's build inputs, the build can be
        // skipped. That set is the SHARED ConverterBuildInputs (src/tests), not a top-level *.go
        // enumeration: an embedded template (csproj scaffolding, the package_info skeleton,
        // profiles/*) or a converter internal/ package changes what go2cs.exe emits while touching no
        // top-level .go file, so the narrower predicate reported "up to date" and every phase below
        // then validated the PREVIOUS emission green -- false-green route #5 in CLAUDE.md. The same
        // call is made by BehavioralRunner and PerformanceRunner.
        if (!ConverterBuildInputs.IsConverterStale(go2csSrc, go2cs))
            return;

        int exitCode = Exec(context, "go", $"build -o \"{go2cs}\"", go2csSrc);

        if (exitCode != 0)
            throw new InvalidOperationException($"\"go build\" failed with exit code {exitCode:N0}");

        if (!File.Exists(go2cs))
            throw new InvalidOperationException($"Failed to find \"{Path.GetFileName(go2cs)}\" build for testing, check path: {go2cs}");
    }

    public TestContext? TestContext { get; set; }

    protected int Exec(string application, string? arguments, string? workingDir = null, DataReceivedEventHandler? outputHandler = null, int timeoutMs = DefaultExecTimeoutMs, DataReceivedEventHandler? errorHandler = null)
    {
        return Exec(TestContext, application, arguments, workingDir, outputHandler, timeoutMs, errorHandler);
    }

    protected static int Exec(TestContext? context, string application, string? arguments, string? workingDir = null, DataReceivedEventHandler? outputHandler = null, int timeoutMs = DefaultExecTimeoutMs, DataReceivedEventHandler? errorHandler = null)
    {
        ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = application,
            Arguments = arguments ?? "",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Disable MSBuild node reuse for every child we spawn. Persistent MSBuild worker nodes left
        // alive by in-test "dotnet build" calls are the root cause of the testhost/MSB3027 lock
        // contention: they keep handles on target bin+obj (and on BehavioralTests.dll) across runs.
        // Tearing them down on each child's exit removes the lingering lock-holders. Harmless for
        // the go/go2cs/transpiled-exe children, which ignore these variables.
        startInfo.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";

        if (!string.IsNullOrWhiteSpace(workingDir))
            startInfo.WorkingDirectory = workingDir;

        using Process process = new();
        
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        if (outputHandler is null)
        {
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    context?.WriteLine(e.Data);
            };
        }
        else
        {
            process.OutputDataReceived += outputHandler;
        }

        // stderr routes to the dedicated errorHandler when one is provided (so callers can compare
        // the streams separately); otherwise it falls back to the merged/legacy behavior.
        if (errorHandler is not null)
        {
            process.ErrorDataReceived += errorHandler;
        }
        else if (outputHandler is null)
        {
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    context?.WriteLine($"[ErrOut]: {e.Data}");
            };
        }
        else
        {
            process.ErrorDataReceived += outputHandler;
        }

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!process.WaitForExit(timeoutMs))
        {
            // Hung child: kill it and its entire descendant tree so nothing it spawned (MSBuild
            // nodes, a deadlocked transpiled program) survives to lock files or keep testhost alive.
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process may have exited in the race between the timeout and the kill; ignore.
            }

            // Allow the killed tree and async output handlers a brief window to drain.
            process.WaitForExit(5000);

            throw new TimeoutException($"Process \"{application} {arguments}\" in \"{workingDir ?? Directory.GetCurrentDirectory()}\" exceeded its {timeoutMs:N0} ms timeout and was terminated along with its child process tree.");
        }

        // Per Process docs: after a timed WaitForExit(int) returns true, call the parameterless
        // overload so the asynchronous output/error handlers are guaranteed to finish before ExitCode.
        process.WaitForExit();

        return process.ExitCode;
    }

    protected void TranspileProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string csproj = Path.GetFullPath(Path.Combine(targetPath, $"{targetProject}.csproj"));
        string projPath = Path.GetFullPath($"{targetPath}");

        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            // A project already found degraded IN THIS PROCESS stays degraded for every later caller.
            // Four test classes call this method for the same project, and the up-to-date check below
            // is satisfied by the very .cs a best-effort run just wrote -- so without this memo the
            // Transpile test would report NOT MEASURED and the Compile, Target and Output tests would
            // then skip straight past it and PASS on that same degraded output, which is the hole this
            // whole change closes, reopened one test class over.
            //
            // ⚠ RESIDUAL, stated rather than papered over: the memo is per-process, so a WARM tree whose
            // .cs were left behind by an earlier degraded run still satisfies the up-to-date check on
            // the first call and reads as a pass. BehavioralRunner's UpToDate has the identical
            // property. Both are bounded by the converter-mtime half of that check (any converter
            // rebuild invalidates the whole corpus), and check-no-regression.ps1 -- which re-transpiles
            // UNCONDITIONALLY and classifies the same two stderr classes -- is the instrument that
            // cannot be fooled by a warm tree. Closing it here would mean persisting a degraded verdict
            // on disk between runs, which is machinery for a case the unconditional gate already covers.
            if (s_degradedProjects.TryGetValue(targetProject, out string[] previouslyDegraded))
                AssertNotMeasured(targetProject, previouslyDegraded);

            // Once per project per process. The up-to-date check below cannot say this on its own: the converter
            // leaves an unchanged source untouched, so after a converter rebuild an unchanged emission stays OLDER than
            // the converter, and every test class after Transpile failed that check and re-ran the converter over the
            // same output -- three extra passes of the corpus per full run. A forced call still transpiles.
            if (!forceBuild && s_transpiledProjects.ContainsKey(targetProject))
                return;

            if (!forceBuild && File.Exists(csproj))
            {
                // If all .cs files are newer than associated .go files AND newer than the converter that
                // produced them, skip build. The converter must be part of this test: converter work is
                // the normal case where the .go files DON'T change, so a .go-only check would leave every
                // project "up to date" and let the Target/Output phases validate the PREVIOUS converter's
                // output against goldens that same converter generated -- a false green.
                // PRODUCTION sources only: a production transpile excludes `_test.go`, so an in-package
                // test file has no matching .cs and would pin this check permanently out of date.
                DateTime go2csTime = File.GetLastWriteTimeUtc(go2cs);
                FileInfo[] goFiles = GoPackageDirs(projPath)
                    .SelectMany(ProductionGoFiles)
                    .Select(fileName => new FileInfo(fileName)).ToArray();
                bool allUpToDate = true;

                foreach (FileInfo goFile in goFiles)
                {
                    FileInfo csFile = new(Path.ChangeExtension(goFile.FullName, ".cs"));

                    if (csFile.Exists && csFile.LastWriteTimeUtc > goFile.LastWriteTimeUtc && csFile.LastWriteTimeUtc > go2csTime)
                        continue;

                    allUpToDate = false;
                    break;
                }

                if (allUpToDate)
                    return;
            }

            int exitCode;

            // Collected across every package of the project, then judged once below. The converter
            // EXITS ZERO on a package it could not fully type-check (or a source file whose emission it
            // had to skip): it says so on stderr and writes a degraded .cs. Asserting the exit code
            // alone made this method report a clean transpile over output the run never really
            // regenerated, and the Compile, Target and Output tests that follow then measured that
            // output as though it were this converter's -- BehavioralRunner had the identical hole, and
            // both are closed through the same linked predicate so the two harnesses cannot drift apart
            // on what a measured transpile is. Nothing is collected on the FAILING path: a non-zero exit is
            // asserted immediately, because a converter that failed outright is a louder and more
            // specific fact than the degradation it may have printed on the way down.
            List<string> degraded = new();

            s_converterRuns.AddOrUpdate(targetProject, 1, (_, runs) => runs + 1);

            foreach (string pkgPath in GoPackageDirs(projPath))
            {
                StringBuilder stdErr = new();

                // The dedicated errorHandler suppresses the base Exec's stdout echo for this call, so
                // the converter's own stdout is re-attached explicitly: losing it would make a
                // transpile the least readable step in the log precisely when it goes wrong.
                exitCode = Exec(go2cs, $"-go2cspath \"{Go2csRoot}\" \"{pkgPath}\"", null,
                    (_, e) => { if (!string.IsNullOrEmpty(e.Data)) TestContext?.WriteLine(e.Data); },
                    DefaultExecTimeoutMs,
                    (_, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data))
                            return;

                        stdErr.AppendLine(e.Data);
                        TestContext?.WriteLine($"[ErrOut]: {e.Data}");
                    });

                Assert.AreEqual(0, exitCode, $"go2cs transpile for \"{targetProject}\" package \"{Path.GetFileName(pkgPath)}\" failed with exit code {exitCode:N0}");

                foreach (string line in BestEffortConversion.NotFullyRegeneratedLines(stdErr.ToString()))
                    degraded.Add($"{Path.GetFileName(pkgPath)}: {line}");
            }

            s_transpiledProjects[targetProject] = true;

            if (degraded.Count > 0)
            {
                s_degradedProjects[targetProject] = degraded.ToArray();
                AssertNotMeasured(targetProject, degraded.ToArray());
            }
        }
    }

    /// <summary>
    /// Reports a best-effort conversion as NOT MEASURED. Never returns.
    /// </summary>
    /// <remarks>
    /// Inconclusive for the same reason <see cref="SkipIfPlatformExclusive"/> is, and reached from the
    /// other direction: an UNMARKED package this host cannot fully type-check. A Pass would be a
    /// vacuous green over output the converter itself calls degraded; a Fail would be indistinguishable
    /// by name from a real conversion regression, which is how one hides among expected lines. NOT
    /// MEASURED is the honest verdict, and it is the word BehavioralRunner and check-no-regression.ps1
    /// both already use for it.
    /// </remarks>
    private static void AssertNotMeasured(string targetProject, string[] degraded)
    {
        Assert.Inconclusive(
            $"NOT MEASURED (best-effort conversion): go2cs could not fully regenerate \"{targetProject}\" on " +
            $"this {PlatformExclusive.HostGoos} host, so every phase below this one would measure output it did " +
            $"not produce. A package native to another platform belongs behind [GoPlatformExclusive] (F8); " +
            $"otherwise this is a real conversion defect. Converter said:{Environment.NewLine}  " +
            string.Join($"{Environment.NewLine}  ", degraded));
    }

    // A production transpile converts the package's PRODUCTION sources only -- go/packages excludes
    // `_test.go` -- so an in-package test file has no `.cs`, by design (it is still a real input: the
    // converter scans the sibling test half for declarator names and for globals whose address it takes).
    private static string[] ProductionGoFiles(string pkgPath) =>
        Directory.GetFiles(pkgPath, "*.go")
            .Where(fileName => !fileName.EndsWith("_test.go", StringComparison.OrdinalIgnoreCase))
            .ToArray();

    // Every Go package directory a project owns, DEEPEST-FIRST. Most projects are a single package, but
    // 22 carry nested sub-libraries (IoLike\FsLike, VersionedImport\vlib, …) that the converter must be
    // invoked on separately -- and BEFORE their parent, because a sub-library's generated
    // package_info.cs is an input to the parent's transpile (the parent reads its sibling's
    // [assembly: GoImplement] records when deciding whether to mint a local value adapter). Walking
    // top-level only left those sub-libraries permanently un-regenerated, which both froze them at an
    // old converter and made the parent's golden unable to fail on a regression in that area.
    /// <summary>
    /// F8 -- a behavioral package whose Go source only type-checks on some platforms, or on some
    /// architectures, is INCONCLUSIVE on any other host, with what it is native to named.
    /// </summary>
    /// <remarks>
    /// Inconclusive rather than Pass or Fail, and the distinction is the whole point. A Pass would be
    /// a vacuous green over a package the converter could not type-check; a Fail would be indis-
    /// tinguishable by name from a real conversion regression, which is how one could hide among
    /// expected lines. Inconclusive says exactly what happened and does not pretend to a verdict.
    ///
    /// Both axes since 2026-09-04: ShouldSkip answers for GOOS and GOARCH alike, so a package that is
    /// native to every GOOS but exclusive to one GOARCH (StdLibInternalAbi, whose Go source does not
    /// build on arm64) is covered by the same call with no second predicate here.
    /// </remarks>
    protected static void SkipIfPlatformExclusive(string targetProject)
    {
        string projPath = Path.GetFullPath($"{TestRootPath}{targetProject}");

        if (PlatformExclusive.ShouldSkip(projPath, out string nativeTo))
        {
            Assert.Inconclusive(
                $"SKIPPED (platform-exclusive): {targetProject} is native to [{nativeTo}]; " +
                $"this host measures as {PlatformExclusive.HostTarget}, where it cannot be measured.");
        }
    }

    private static string[] GoPackageDirs(string projPath) =>
        new[] { projPath }
            .Concat(Directory.GetDirectories(projPath, "*", SearchOption.AllDirectories)
                .Where(dirName => !dirName.Contains(s_binFragment, StringComparison.OrdinalIgnoreCase) &&
                                  !dirName.Contains(s_objFragment, StringComparison.OrdinalIgnoreCase))
                .Where(dirName => ProductionGoFiles(dirName).Length > 0))
            .OrderByDescending(dirName => dirName.Count(c => c == Path.DirectorySeparatorChar))
            .ThenBy(dirName => dirName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    protected void CompileCSProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string csExePath = GetCSExecPath(projPath, targetProject);
        string projExe = GetCSExeFile(projPath, targetProject);
        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            // Set 'go2csPath' environment variable. MSBuild wants the trailing separator: every
            // generated csproj concatenates it directly ($(go2csPath)core\<pkg>\...).
            Environment.SetEnvironmentVariable("go2csPath", Path.GetFullPath(Path.Combine(TestRootPath, "..", "..")) + Path.DirectorySeparatorChar);

            if (!forceBuild && File.Exists(projExe))
            {
                FileInfo projExeInfo = new(projExe);

                // If exe is newer than all .cs source files, can skip build
                if (Directory.GetFiles(projPath, "*.cs").Select(fileName => new FileInfo(fileName)).All(info => projExeInfo.LastWriteTimeUtc > info.LastWriteTimeUtc))
                    return;
            }

            int exitCode;

            // Compile C# project
        #if USE_PUBLISH_PROFILES
            Assert.AreEqual(0, exitCode = Exec("dotnet", $"publish \"{Path.Combine(projPath, $"{targetProject}.csproj")}\" -f {NetVersion} -r {PublishProfile} -c {TargetConfig} --sc true -o \"{csExePath}\""), $"dotnet publish for \"{targetProject}\" failed with exit code {exitCode:N0}");
        #else
            Assert.AreEqual(0, exitCode = Exec("dotnet", $"build --configuration {TargetConfig} \"{Path.Combine(projPath, $"{targetProject}.csproj")}\""), $"dotnet build for \"{targetProject}\" failed with exit code {exitCode:N0}");
        #endif

            // If matching console output, run once after compile to ensure any first run initialization
            // steps are completed. The exit code is intentionally not asserted: a program may
            // legitimately exit nonzero (e.g. an unrecovered panic exits 2, like Go) — the output
            // comparison phase validates the exit code differentially against the Go binary.
            if (MatchConsoleOutput(targetProject))
                Exec(projExe, null, csExePath, timeoutMs: RunExecTimeoutMs);
        }
    }

    protected void CompileGoProject(string targetProject)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string goExePath = GetGoExePath(projPath, targetProject);

        if (!Directory.Exists(goExePath))
            Directory.CreateDirectory(goExePath);
        
        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            int exitCode;

            // Make sure Go module is initialized
            if (!File.Exists(Path.Combine(projPath, "go.mod")))
                Assert.AreEqual(0, exitCode = Exec("go", $"mod init go2cs/{targetProject}", projPath), $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");

            // Compile Go project
            Assert.AreEqual(0, exitCode = Exec("go", $"build -o \"{goExePath}\"", projPath), $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    private static bool MatchConsoleOutput(string targetProject)
    {
        // Access "package_info.cs" file for the target test project
        string packageInfoFile = Path.GetFullPath(Path.Combine($"{TestRootPath}{targetProject}", "package_info.cs"));

        if (!File.Exists(packageInfoFile))
            return false;

        string[] packageInfoLines = File.ReadAllLines(packageInfoFile);

        // Check for "GoTestMatchingConsoleOutput" attribute -- for now, just check for its presence
        // by looking for the attribute name in the file on its own line. Future implementations could
        // load assembly and verify attribute presence via reflection -- this is a simpler approach:
        return packageInfoLines.Any(line => line.Trim().Equals("[GoTestMatchingConsoleOutput]"));
    }
}
