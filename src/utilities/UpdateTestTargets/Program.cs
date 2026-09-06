// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

// Move up from the working directory this is run from -- "go2cs/src/utilities/UpdateTestTargets/
// bin/Debug/net9.0" -- to "go2cs/src". Built from Path.Combine SEGMENTS rather than an embedded
// @"..\..\..\..\..\": .NET does not normalize backslashes on Unix, so that literal is a single
// directory NAME there and every path below resolves to somewhere that does not exist. Identical
// bytes on Windows, correct on both (F4, docs/PLAN-linux-operation.md).
string rootPath = Path.Combine("..", "..", "..", "..", "..");

// Behavioral tree, relative to that root.
string behavioralRoot = Path.Combine(rootPath, "tests", "Behavioral");

// The generated blocks below are written with explicit CRLF, matching every other emitted artifact
// in this repository (and the eol=crlf pin in .gitattributes). File.WriteAllLines would otherwise
// use Environment.NewLine and emit LF-terminated *Tests.cs on a non-Windows host while the injected
// test-method lines kept their own "\r\n" -- a mixed file, from the one utility the documented
// add-a-test flow requires.
const string NewLine = "\r\n";

// Scan all behavioral test folders
string[] behavioralTestDirs = Directory.GetDirectories(behavioralRoot);

List<string> targetTests = [];

foreach (string testDir in behavioralTestDirs)
{
    if (testDir.EndsWith("Tests"))
        continue;

    // Only real behavioral test projects (those containing Go source) are test targets. Utility folders
    // that live under tests\Behavioral but have no .go files -- e.g. BehavioralRunner -- are not tests
    // and must not get phantom Check<Name>() methods injected.
    if (Directory.GetFiles(testDir, "*.go").Length == 0)
        continue;

    // Get last subdirectory name which is the test project name
    string[] dirParts = testDir.Split(Path.DirectorySeparatorChar);
    targetTests.Add(dirParts[^1]);
}

// Directory.GetDirectories' enumeration order is host- and filesystem-dependent (Windows: a
// case-insensitive sort; Linux: raw directory-entry order, effectively unordered) -- sorting here
// makes the four generated <TestMethods> blocks below identical regardless of which host the
// utility runs on, so adding one test project no longer reorders every existing line as a side
// effect of running it on a different platform.
targetTests.Sort(StringComparer.Ordinal);

// F8 -- the platform-exclusive projects, i.e. those this host may not write a golden for. The two
// jobs below split on this set, and they split ASYMMETRICALLY on purpose:
//
//   * the four generated <TestMethods> blocks KEEP their entries, because BehavioralTestBase's
//     CheckTarget calls SkipIfPlatformExclusive first and reports Inconclusive by name at RUN time.
//     Dropping the entries here would make the generated classes host-dependent -- a Windows run and
//     a Linux run of this utility would emit different files, and whichever ran last would show the
//     other host's projects as deletions. The entries are a pure function of the project set; only
//     the goldens are a function of the HOST.
//
//   * the .cs.target goldens are SKIPPED, because the .cs beside a non-native project here is a
//     best-effort conversion of a package this host cannot type-check. Copying it over the golden
//     would bank that best-effort output as the record of what the converter emits -- and the
//     package's native host would then fail its Target phase against it. Where no golden exists yet
//     the copy mints an untracked one, which is the same drift wearing a different name.
List<string> platformExclusive = targetTests
    .Where(targetTest => PlatformExclusive.ShouldSkip(Path.GetFullPath(Path.Combine(behavioralRoot, targetTest)), out _))
    .ToList();

(string testClass, Func<string, bool>? filter)[] testClasses =
[
    ("TranspileTests", null),                       // Tests transpilation of Go code to C# code
    ("CompileTests", null),                         // Tests compilation of transpiled C# code
    ("TargetComparisonTests", null),                // Tests comparison of transpiled C# code to expected target
    ("OutputComparisonTests", MatchConsoleOutput)   // Tests comparison of console output to expected output
];

foreach ((string testClass, Func<string, bool>? filter) in testClasses)
{
    string testFile = Path.GetFullPath(Path.Combine(behavioralRoot, "BehavioralTests", $"{testClass}.cs"));
    string[] testFileLines = File.ReadAllLines(testFile);
    int startLineIndex = -1;
    int endLineIndex = -1;

    for (int i = 0; i < testFileLines.Length; i++)
    {
        if (testFileLines[i].Contains("// <TestMethods>"))
        {
            startLineIndex = i + 1;
            continue;
        }
        
        if (testFileLines[i].Contains("// </TestMethods>"))
        {
            endLineIndex = i;
            break;
        }
    }

    if (startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex)
    {
        // Add all lines up to the start of the test methods
        List<string> lines = [ ..testFileLines[..startLineIndex] ];

        // Set up a filter predicate to include only specific test targets
        Func<string, bool> includeTestTarget = filter ?? (_ => true);

        // Add new test methods for each target test
        lines.AddRange(targetTests.Where(includeTestTarget).Select(targetTest =>
            $"{NewLine}    [TestMethod]{NewLine}    public void Check{targetTest}() => CheckTarget(\"{targetTest}\");"));

        lines.Add("");

        // Add all lines after the end of the test methods
        lines.AddRange(testFileLines[endLineIndex..]);

        File.WriteAllText(testFile, string.Join(NewLine, lines) + NewLine);
    }
    else
    {
        throw new InvalidOperationException($"Could not find '<TestMethods>...</TestMethods>' section in \"{testFile}\"");
    }
}

if (args.Contains("--createTargetFiles"))
{
    // --only <Name>[,<Name>...] narrows the transpile-and-copy of THIS invocation. It exists for one
    // reason and it is not convenience: the golden job below now transpiles every project it is about
    // to re-baseline, and the whole behavioral corpus is a ~25-minute run -- which would make the
    // REFUSAL branch a control nobody ever exercises, and CLAUDE.md rules that an unexercisable branch
    // is a false-green seed ("a control that needs a 25-minute CNR to run is a control nobody runs --
    // give it a check-only switch"). Default behaviour is unchanged: with no --only, every project is
    // transpiled and re-baselined exactly as before. The four <TestMethods> blocks are NOT narrowed by
    // it -- they are a pure function of the project set, generated above, before this branch.
    HashSet<string>? only = null;
    int onlyIndex = Array.IndexOf(args, "--only");

    if (onlyIndex >= 0)
    {
        if (onlyIndex + 1 >= args.Length)
        {
            Console.Error.WriteLine("--only expects a comma-separated list of behavioral project names.");
            return 2;
        }

        only = new HashSet<string>(
            args[onlyIndex + 1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase);

        string[] unknown = only.Where(name => !targetTests.Contains(name, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (unknown.Length > 0)
        {
            // A silently-empty --only would report "Updated 0 goldens" and exit 0, which is this
            // utility's own version of a gate that measures nothing and passes over the hole.
            Console.Error.WriteLine($"--only names {unknown.Length} project(s) that do not exist: {string.Join(", ", unknown)}");
            return 2;
        }
    }

    // Report the skip LOUDLY and BY NAME, in the runners' wording. A silent drop from the golden
    // job would trade a visible wrong golden for an invisible missing one, and the next reader would
    // have no way to tell "this host declined" from "this project has no goldens".
    if (platformExclusive.Count > 0)
    {
        Console.WriteLine($"SKIPPED (platform-exclusive, {platformExclusive.Count}): native to another platform, so this {PlatformExclusive.HostGoos} host cannot mint their goldens:");

        foreach (string targetTest in platformExclusive)
        {
            PlatformExclusive.ShouldSkip(Path.GetFullPath(Path.Combine(behavioralRoot, targetTest)), out string platforms);
            Console.WriteLine($"    {targetTest} [{platforms}] -- .cs.target left as it stands; [TestMethod] entries still generated");
        }

        Console.WriteLine();
    }

    // The set this invocation is about to re-baseline.
    //
    // MEASURED (negative control, 2026-09-02): this Except IS the guard and the banner above is NOT.
    // Neutering only this line back to `targetTests` still printed the full SKIPPED block and
    // re-minted ScmRightsSeam's golden -- so a reader trusting the banner would have believed the
    // skip held. Do not "simplify" the report and the exclusion into one place that only prints.
    string[] rebaseline = targetTests
        .Except(platformExclusive)
        .Where(targetTest => only is null || only.Contains(targetTest))
        .ToArray();

    // ------------------------------------------------------------------------------------------
    // RE-TRANSPILE FIRST, UNCONDITIONALLY. A .cs.target is the AUTHORITATIVE record of what the
    // converter emits, and until 2026-09-04 this utility minted one by copying whatever .cs happened
    // to be on disk -- it ran the converter not at all. The prerequisite ("re-transpile first, e.g.
    // via check-no-regression.ps1 or a runner pass, or the copy silently re-baselines stale output")
    // lived in a sentence in CLAUDE.md, which is the weakest place a gate can live: satisfiable by
    // memory, and unverifiable afterwards, because once the copy lands the .cs and its golden agree
    // by construction and no later run can tell the difference.
    //
    // There is deliberately NO up-to-date predicate here, and that is the point rather than an
    // omission. The runner's UpToDate answers on MTIMES: it calls a project up to date when every
    // .cs is strictly newer than its .go AND than go2cs.exe. Nothing about that pair says the .cs
    // came from THIS converter -- a `git checkout HEAD -- <file>.cs`, a Copy-Item, an editor save,
    // or any restore of the .cs alone leaves exactly that mtime relation over content from some
    // other converter. (MEASURED 2026-09-04: a WHOLE-TREE checkout is the one shape that does not
    // reliably trip it, because it writes .cs and .go within the same instant and the comparison is
    // strict -- which is a fact about write ORDER, not a property anyone should rely on.) Any mtime
    // rule is therefore exactly wrong for minting a RECORD. check-no-regression.ps1 has been immune
    // to this whole family for the same reason: it re-transpiles unconditionally.
    // ------------------------------------------------------------------------------------------
    string srcRoot = Path.GetFullPath(rootPath);
    string converterSrc = Path.Combine(srcRoot, "go2cs");
    string exeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
    string go2csExe = Path.Combine(converterSrc, "bin", $"go2cs{exeSuffix}");

    // Budgets in SECONDS, sharing the runner's environment-variable names so one host tuning covers
    // both golden paths. Safety nets against a hung child, never performance assumptions.
    int transpileTimeoutMs = SecondsFromEnv("GO2CS_TRANSPILE_TIMEOUT", 60);
    int buildTimeoutMs = SecondsFromEnv("GO2CS_BUILD_ONE_TIMEOUT", 300);

    // ------------------------------------------------------------------------------------------
    // TOOLCHAIN PIN -- checked BEFORE the staleness predicate, and that ordering is the whole point.
    // IsConverterStale compares the binary's embedded release against the live `go env GOVERSION`
    // (false-green route #4), so on a host whose bare `go` is NOT the pinned release it reports
    // STALE and this utility rebuilds go2cs.exe with the WRONG toolchain -- then re-baselines every
    // golden from that binary's emission. The run exits 0, prints no warning and refuses nothing;
    // the wrong goldens simply become the new definition of correct, and every later comparison is
    // measured against them. That is the one defect this instrument must never commit, because it
    // rewrites the RECORD rather than a result. (Measured 2026-09-06 on a container whose bare `go`
    // was go1.24.7 against a corpus pinned to 1.23.12: eight rows' goldens affected, no signal.)
    //
    // The pin is DERIVED from version.props -- the property of record -- not spelled here, so a
    // corpus hop moves it in one place. Printing the release is deliberately NOT enough: this repo
    // has already paid for an instrument that printed its pin and carried on.
    string versionProps = Path.Combine(srcRoot, "version.props");
    string? pinnedGo = null;

    if (File.Exists(versionProps))
    {
        Match match = Regex.Match(File.ReadAllText(versionProps), @"<GoStdLibVersion>\s*([^<\s]+)\s*</GoStdLibVersion>");

        if (match.Success)
        {
            pinnedGo = "go" + match.Groups[1].Value;
        }
    }

    if (pinnedGo is null)
    {
        Console.Error.WriteLine($"Cannot determine the pinned Go release: no <GoStdLibVersion> in \"{versionProps}\".");
        Console.Error.WriteLine("No goldens were written: an instrument that cannot know its pin must not mint a record.");
        return 1;
    }

    (int versionExit, string versionOut, _, _) = Run("go", "env GOVERSION", converterSrc, buildTimeoutMs);
    string liveGo = versionOut.Trim();

    if (versionExit != 0 || liveGo.Length == 0)
    {
        Console.Error.WriteLine("Cannot determine the live Go release (`go env GOVERSION` did not answer).");
        Console.Error.WriteLine("No goldens were written: an unverifiable toolchain must not mint a record.");
        return 1;
    }

    if (!string.Equals(liveGo, pinnedGo, StringComparison.Ordinal))
    {
        Console.Error.WriteLine($"TOOLCHAIN MISMATCH -- live `go` is {liveGo}, the corpus pins {pinnedGo}.");
        Console.Error.WriteLine("No goldens were written: a golden minted by the wrong toolchain becomes the new");
        Console.Error.WriteLine("definition of correct, and every later comparison is measured against it.");
        Console.Error.WriteLine($"Put the pinned toolchain first on PATH (its GOROOT's bin) and re-run.");
        return 1;
    }

    // "With the CURRENT go2cs.exe" is not the same question as "with the go2cs.exe on disk". The
    // staleness answer comes from the SHARED ConverterBuildInputs -- the same predicate the three
    // harnesses use -- because an embedded template or a converter internal/ package changes what
    // go2cs.exe EMITS while touching no top-level .go file, and a re-derived *.go walk here would
    // reopen false-green route #5 in the one instrument that writes the record.
    if (ConverterBuildInputs.IsConverterStale(converterSrc, go2csExe))
    {
        Console.WriteLine("Building go2cs.exe (converter build inputs changed)...");
        (int buildExit, _, string buildErr, bool buildTimedOut) = Run("go", $"build -o \"{go2csExe}\"", converterSrc, buildTimeoutMs);

        if (buildExit != 0)
        {
            Console.Error.WriteLine($"go build of the converter {(buildTimedOut ? "TIMED OUT" : $"failed ({buildExit})")}:");
            Console.Error.WriteLine(buildErr);
            Console.Error.WriteLine("No goldens were written: a golden minted by a stale converter is the defect this refuses.");
            return 1;
        }
    }

    Console.WriteLine($"Re-transpiling {rebaseline.Length} project(s) before re-baselining (unconditional -- no up-to-date skip)...");

    // project -> why it may not be re-baselined. Populated by the transpile pass; consulted by the
    // copy pass, which never touches a project named here.
    Dictionary<string, string> refused = new(StringComparer.Ordinal);
    Stopwatch elapsed = Stopwatch.StartNew();
    int done = 0;

    foreach (string targetTest in rebaseline)
    {
        // A whole-corpus run is CNR-length. Progress is not decoration there: a silent instrument is
        // indistinguishable from a hung one, and "the only tell was the implausible speed" is how
        // false-green route #6 was found.
        if (++done % 50 == 0)
            Console.WriteLine($"    {done}/{rebaseline.Length} transpiled ({elapsed.Elapsed:hh\\:mm\\:ss})");

        string projPath = Path.GetFullPath(Path.Combine(behavioralRoot, targetTest));

        // DEEPEST-FIRST, from the shared walk: a nested sub-library's generated package_info.cs is an
        // INPUT to its parent's transpile, so converting the parent first feeds it stale sibling
        // records -- and a golden minted from that cannot fail on a regression in exactly that area
        // (false-green route #3). Not re-derived here; three copies of this rule is how it broke.
        foreach (string pkgPath in BehavioralPackages.GoPackageDirs(projPath))
        {
            (int exit, _, string stdErr, bool timedOut) = Run(go2csExe, $"-go2cspath \"{srcRoot}\" \"{pkgPath}\"", pkgPath, transpileTimeoutMs);

            if (exit != 0)
            {
                refused[targetTest] = timedOut
                    ? $"transpile TIMED OUT after {transpileTimeoutMs / 1000}s in {Path.GetFileName(pkgPath)} (raise GO2CS_TRANSPILE_TIMEOUT)"
                    : $"transpile exit {exit} in {Path.GetFileName(pkgPath)}: {FirstLine(stdErr)}";
                break;
            }

            // The refusal an exit code CANNOT make. go2cs exits 0 on a package it could not fully
            // type-check, having written a degraded emission -- so without this the golden job's only
            // check would pass on precisely the run that must not mint a record. The classification
            // is the SHARED one the two transpiling harnesses use; this utility does not re-derive it.
            //
            // No `break` on this arm, unlike the exit-code arm above: the emission is written either
            // way, and the remaining packages of a multi-package project must still be converted or
            // the tree is left half regenerated (a nested sub-library's package_info.cs is an INPUT to
            // its parent's transpile). The verdict is already decided; the work still has to finish.
            if (BestEffortConversion.NotFullyRegenerated(stdErr, out string[] degraded))
            {
                // TRUNCATED on purpose: the converter's warning carries every unresolved symbol on
                // ONE line (22 of them in the measured control), and a refusal roster that scrolls
                // its own heading off the screen is a refusal nobody reads.
                refused[targetTest] = $"best-effort conversion in {Path.GetFileName(pkgPath)}: {Clip(degraded[0], 200)}";
            }
        }
    }

    // For each Go file converted to C#, create a target file for regression testing comparisons.
    int rebaselined = 0;

    foreach (string targetTest in rebaseline)
    {
        if (refused.ContainsKey(targetTest))
            continue;

        string projPath = Path.GetFullPath(Path.Combine(behavioralRoot, targetTest));

        // Iterate over each PRODUCTION .go file in project path. `_test.go` is excluded from a
        // production transpile by go/packages, so an in-package test file legitimately has no .cs and
        // needs no golden -- warning about it would be noise, not signal.
        foreach (string goSrcFile in BehavioralPackages.ProductionGoFiles(projPath))
        {
            string transpiledFile = Path.Combine(projPath, $"{Path.GetFileNameWithoutExtension(goSrcFile)}.cs");
            string targetFile = $"{transpiledFile}.target";

            if (!File.Exists(transpiledFile))
                Console.Error.WriteLine($"WARNING: Transpiled file \"{transpiledFile}\" does not exist -- skipping target file creation...");
            else
                File.Copy(transpiledFile, targetFile, true);
        }

        rebaselined++;
    }

    Console.WriteLine($"Updated .cs.target goldens for {rebaselined} project(s).");

    if (refused.Count > 0)
    {
        // Their goldens were never opened: the copy pass above skips them entirely, so this is a
        // refusal and not a report. Exit non-zero so no wrapper can read it as success -- the same
        // shape, and the same wording, as BehavioralRunner's own --update-targets refusal.
        Console.Error.WriteLine();
        Console.Error.WriteLine($"REFUSED to re-baseline {refused.Count} project(s) whose transpile was not measured:");

        foreach (KeyValuePair<string, string> entry in refused.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            Console.Error.WriteLine($"  {entry.Key} -- {entry.Value}");

        // The remedies are DIFFERENT and naming only one sends the reader to the wrong place: a
        // timeout wants a bigger budget, a best-effort conversion wants a host that can type-check
        // the package (or an F8 marker saying this one cannot).
        Console.Error.WriteLine("Their goldens are UNCHANGED. A timeout wants a larger GO2CS_TRANSPILE_TIMEOUT;");
        Console.Error.WriteLine("a best-effort conversion wants a host that can type-check the package.");
        return 1;
    }
}

return 0;

// Seconds from an environment variable, or the default when it is absent, empty or unparseable. A
// malformed value falls back LOUDLY rather than silently choosing 0, which would kill every child
// instantly and read as a corpus-wide transpile failure.
static int SecondsFromEnv(string name, int defaultSeconds)
{
    string? raw = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(raw))
        return defaultSeconds * 1000;

    if (int.TryParse(raw.Trim(), out int seconds) && seconds > 0)
        return seconds * 1000;

    Console.Error.WriteLine($"WARNING: {name}=\"{raw}\" is not a positive integer -- using the {defaultSeconds}s default.");
    return defaultSeconds * 1000;
}

static string FirstLine(string text)
{
    string trimmed = text.Trim();
    int newline = trimmed.IndexOf('\n');
    return Clip((newline < 0 ? trimmed : trimmed[..newline]).TrimEnd('\r'), 200);
}

static string Clip(string text, int max) => text.Length <= max ? text : text[..max] + " ...";

// A child process with a hard budget, killed with its whole TREE when it expires. Deliberately a
// local helper rather than a share of BehavioralRunner's Exec: process plumbing is not a domain
// invariant, and hoisting a 40-line Exec out of the runner would be a large diff across a file
// another lane is editing. What IS shared is everything that encodes a RULE -- the package walk,
// the staleness predicate, the best-effort markers.
static (int ExitCode, string StdOut, string StdErr, bool TimedOut) Run(string application, string arguments, string workingDir, int timeoutMs)
{
    ProcessStartInfo startInfo = new()
    {
        FileName = application,
        Arguments = arguments,
        WorkingDirectory = workingDir,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

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
        catch { /* already gone */ }

        return (-1, outBuf.ToString(), $"TIMEOUT after {timeoutMs} ms; killed process tree.\n{errBuf}", true);
    }

    // Parameterless overload after the timed one, so the asynchronous output handlers are guaranteed
    // to have drained before ExitCode and the buffers are read.
    process.WaitForExit();

    return (process.ExitCode, outBuf.ToString(), errBuf.ToString(), false);
}

static bool MatchConsoleOutput(string targetTest)
{
    // Access "package_info.cs" file for the target test project. Recomputed from Path.Combine
    // segments rather than captured, because a local function cannot close over a top-level local.
    string packageInfoFile = Path.GetFullPath(
        Path.Combine("..", "..", "..", "..", "..", "tests", "Behavioral", targetTest, "package_info.cs"));

    if (!File.Exists(packageInfoFile))
        return false;

    string[] packageInfoLines = File.ReadAllLines(packageInfoFile);

    // Check for "GoTestMatchingConsoleOutput" attribute -- for now, just check for its presence
    // by looking for the attribute name in the file on its own line. Future implementations could
    // load assembly and verify attribute presence via reflection - this is a simpler approach.
    return packageInfoLines.Any(line => line.Trim().Equals("[GoTestMatchingConsoleOutput]"));
}
