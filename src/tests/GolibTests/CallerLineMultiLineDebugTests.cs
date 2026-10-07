// CallerLineMultiLineDebugTests.cs - Gbtc
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
/// A call inside a MULTI-LINE statement reports its OWN line (the record's per-call table,
/// GoPositionMapAttribute.Calls), measured where the JIT's native-to-IL map is per call: a Debug build.
/// </summary>
/// <remarks>
/// The behavioral project CallerLineMultiLine runs Release, where the optimizing JIT maps calls that
/// share an expression (array and struct elements, one call's arguments, a sum, a chain) to ONE IL
/// offset, so it prints only the two shapes an optimized build answers exactly. This arm builds the same
/// project Debug, runs it and Go's with GO2CS_CALLER_LINE_ALL_SHAPES=1, and compares all eleven shapes
/// line for line. The control: Go must print every shape (twelve lines), or the switch did not reach the
/// programs and an equal two-line output would prove nothing. Inconclusive only where no pinned Go
/// toolchain or no dotnet host resolves, naming which.
/// </remarks>
[TestClass]
public class CallerLineMultiLineDebugTests
{
    private const string AllShapes = "GO2CS_CALLER_LINE_ALL_SHAPES";
    private const int ShapeLines = 12;

    [TestMethod]
    public void EveryShapeEqualsGoInADebugBuild()
    {
        (string? go, string? skip) = ExecutionTracerParserTests.Oracle();

        if (go is null)
            Assert.Inconclusive(skip);

        string? dotnet = DotnetHost();

        if (dotnet is null)
            Assert.Inconclusive("no dotnet host resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): the Debug build cannot be made here");

        string? src = SourceRoot();
        Assert.IsNotNull(src, "the source root (the folder holding go2cs.slnx) is not above the test binary");

        string project = Path.Combine(src!, "tests", "Behavioral", "CallerLineMultiLine");
        string go2csPath = src!.Replace('\\', '/').TrimEnd('/') + "/";

        (int buildCode, string buildOut) = Run(dotnet!, ["build", Path.Combine(project, "CallerLineMultiLine.csproj"), "-c", "Debug", "-nologo", "-nodeReuse:false", "-p:go2csPath=" + go2csPath], project, 1_800_000);
        Assert.AreEqual(0, buildCode, $"the Debug build failed:{Environment.NewLine}{Tail(buildOut)}");

        (int csCode, string csOut) = Run(dotnet!, [Path.Combine(project, "bin", "Debug", "net10.0", "CallerLineMultiLine.dll")], project, 120_000);
        (int goCode, string goOut) = Run(go!, ["run", "."], project, 600_000);

        Assert.AreEqual(0, goCode, $"go run failed:{Environment.NewLine}{Tail(goOut)}");
        Assert.AreEqual(0, csCode, $"the converted program failed:{Environment.NewLine}{Tail(csOut)}");

        string[] goLines = Lines(goOut);
        string[] csLines = Lines(csOut);

        Assert.AreEqual(ShapeLines, goLines.Length, $"control: Go printed {goLines.Length} lines, not every shape; {AllShapes} did not reach it:{Environment.NewLine}{goOut}");

        List<string> differ = [];

        for (int i = 0; i < goLines.Length; i++)
        {
            string cs = i < csLines.Length ? csLines[i] : "(missing)";

            if (cs != goLines[i])
                differ.Add($"{goLines[i].Split(' ')[0]}: Go `{goLines[i]}`, C# `{cs}`");
        }

        if (csLines.Length != goLines.Length)
            differ.Add($"line count: Go {goLines.Length}, C# {csLines.Length}");

        Assert.AreEqual(0, differ.Count, $"in a Debug build these shapes do not report their calls' own lines:{Environment.NewLine}{string.Join(Environment.NewLine, differ)}");
    }

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

    // Runs one child with every shape switched on: (exit code, stdout and stderr together).
    private static (int Code, string Output) Run(string file, string[] args, string directory, int timeoutMs)
    {
        ProcessStartInfo start = new(file, args)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[AllShapes] = "1";
        start.Environment["GOTOOLCHAIN"] = "local";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";

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
