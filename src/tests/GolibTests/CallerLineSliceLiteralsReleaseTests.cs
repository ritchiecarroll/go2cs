// CallerLineSliceLiteralsReleaseTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// A call inside a Go SLICE LITERAL answers a line inside its own literal in an OPTIMIZED build (seat #54).
/// </summary>
/// <remarks>
/// <para>
/// CallerLineMultiLineDebugTests compares every multi-line shape with Go in a Debug build, whose native-to-IL map
/// is per call, so it cannot see a Release-only loss. This arm builds the behavioral project CallerLineSliceLiterals
/// Release, runs it with GO2CS_SLICE_LITERAL_LINES=1 tiered and with tiering off (the -tests act's configuration),
/// and checks what an optimized build holds:
/// </para>
/// <para>
/// (a) no element answers a line before its own literal's spec line (each literal's first element sits on that
/// line, so Go's first value IS it) nor after Go's own line for it; and (b) the first field of each struct element
/// in <c>structs</c> and <c>localStructs</c> answers that field's own line, exactly as Go does.
/// </para>
/// <para>
/// Both failed when slice literals lowered through an inline stack buffer: the optimizing JIT mapped the element
/// stores to no IL offset of their own, so a struct element answered its spec's line, and under tiering off a
/// local literal answered the PREVIOUS statement's line (runtime's TestLineNumber, sliceLit[0]). Shapes an
/// optimized build never answers per call (an element's later calls, nested slices, pointer elements) are the
/// disclosed D1b limit and are not asserted here. Inconclusive only where no pinned Go toolchain or no dotnet host
/// resolves, naming which.
/// </para>
/// </remarks>
[TestClass]
public class CallerLineSliceLiteralsReleaseTests
{
    private const string Switch = "GO2CS_SLICE_LITERAL_LINES";

    // The literals whose struct elements' first fields must answer Go's own line: value index 0 and 2.
    private static readonly string[] StructGroups = ["structs", "localStructs"];

    [TestMethod]
    public void SliceLiteralCallsKeepTheirOwnLiteralsLineInARelease()
    {
        (string? go, string? skip) = ExecutionTracerParserTests.Oracle();

        if (go is null)
            Assert.Inconclusive(skip);

        string? dotnet = DotnetHost();

        if (dotnet is null)
            Assert.Inconclusive("no dotnet host resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): the Release build cannot be made here");

        string? src = SourceRoot();
        Assert.IsNotNull(src, "the source root (the folder holding go2cs.slnx) is not above the test binary");

        string project = Path.Combine(src!, "tests", "Behavioral", "CallerLineSliceLiterals");
        string go2csPath = src!.Replace('\\', '/').TrimEnd('/') + "/";

        // Built for this host's GOOS, so it runs here (see CallerLineMultiLineDebugTests).
        (int buildCode, string buildOut) = Run(dotnet!, ["build", Path.Combine(project, "CallerLineSliceLiterals.csproj"), "-c", "Release", "-nologo", "-nodeReuse:false", "-p:go2csPath=" + go2csPath, "-p:GoTargetOS=" + HostGoos()], project, 1_800_000, tiered: true);
        Assert.AreEqual(0, buildCode, $"the Release build failed:{Environment.NewLine}{Tail(buildOut)}");

        (int goCode, string goOut) = Run(go!, ["run", "."], project, 600_000, tiered: true);
        Assert.AreEqual(0, goCode, $"go run failed:{Environment.NewLine}{Tail(goOut)}");

        Dictionary<string, int[]> goLines = Parse(goOut);

        // The control: Go prints every literal, or the switch did not reach it and an empty comparison proves nothing.
        Assert.AreEqual(7, goLines.Count, $"control: Go printed {goLines.Count} literals, not seven; {Switch} did not reach it:{Environment.NewLine}{goOut}");

        string program = Path.Combine(project, "bin", "Release", "net10.0", "CallerLineSliceLiterals.dll");
        List<string> failures = [];

        foreach (bool tiered in new[] { true, false })
        {
            string config = tiered ? "tiered" : "tiering off";
            (int csCode, string csOut) = Run(dotnet!, [program], project, 120_000, tiered);
            Assert.AreEqual(0, csCode, $"the converted program failed ({config}):{Environment.NewLine}{Tail(csOut)}");

            Dictionary<string, int[]> csLines = Parse(csOut);

            foreach ((string label, int[] want) in goLines)
            {
                if (!csLines.TryGetValue(label, out int[]? got) || got.Length != want.Length)
                {
                    failures.Add($"{config} {label}: Go {Show(want)}, C# {(got is null ? "(missing)" : Show(got))}");
                    continue;
                }

                int spec = want[0];

                for (int i = 0; i < want.Length; i++)
                {
                    if (got[i] < spec || got[i] > want[i])
                        failures.Add($"{config} {label}[{i}] answered {got[i]}, outside its literal's lines {spec}..{want[i]} (Go {Show(want)}, C# {Show(got)})");
                }

                if (StructGroups.Contains(label))
                {
                    foreach (int i in new[] { 0, 2 })
                    {
                        if (got[i] != want[i])
                            failures.Add($"{config} {label}[{i}], a struct element's first field, answered {got[i]}, not its own line {want[i]} (Go {Show(want)}, C# {Show(got)})");
                    }
                }
            }
        }

        Assert.AreEqual(0, failures.Count, $"in a Release build these slice-literal calls leave their own literal's lines:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    // "label [l1 l2 ...]" lines, keyed by label; the lengths line and anything else is ignored.
    private static Dictionary<string, int[]> Parse(string output)
    {
        Dictionary<string, int[]> result = [];

        foreach (string line in Lines(output))
        {
            int open = line.IndexOf(" [", StringComparison.Ordinal);

            if (open <= 0 || !line.EndsWith(']'))
                continue;

            string[] values = line[(open + 2)..^1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (values.All(v => int.TryParse(v, out _)))
                result[line[..open]] = values.Select(int.Parse).ToArray();
        }

        return result;
    }

    private static string Show(int[] values) => "[" + string.Join(' ', values) + "]";

    // This host's GOOS in Go's spelling: the flavour a program built here must be to run here.
    private static string HostGoos() =>
        OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "darwin" : "linux";

    private static string[] Lines(string output) =>
        output.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);

    private static string Tail(string output) =>
        string.Join(Environment.NewLine, Lines(output).TakeLast(30));

    // The folder holding go2cs.slnx, above the test binary.
    private static string? SourceRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "go2cs.slnx")))
                return dir.FullName;
        }

        return null;
    }

    // The dotnet host: the one the SDK names for its children, else DOTNET_ROOT's, else the first on PATH.
    private static string? DotnetHost()
    {
        string exe = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";

        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host && File.Exists(host))
            return host;

        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { Length: > 0 } root && File.Exists(Path.Combine(root, exe)))
            return Path.Combine(root, exe);

        foreach (string dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            if (dir.Length > 0 && File.Exists(Path.Combine(dir, exe)))
                return Path.Combine(dir, exe);
        }

        return null;
    }

    // Runs one child with the line switch on: (exit code, stdout and stderr together). Tiering is set explicitly
    // either way, so an ambient DOTNET_TieredCompilation never chooses the configuration for the arm.
    private static (int Code, string Output) Run(string file, string[] args, string directory, int timeoutMs, bool tiered)
    {
        ProcessStartInfo start = new(file, args)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[Switch] = "1";
        start.Environment["GOTOOLCHAIN"] = "local";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment["DOTNET_TieredCompilation"] = tiered ? "1" : "0";

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(timeoutMs))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"{Path.GetFileName(file)} {string.Join(' ', args)} did not finish in {timeoutMs / 1000} s");
        }

        return (process.ExitCode, stdout.Result + stderr.Result);
    }
}
