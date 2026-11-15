using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// R5 and R6 of the pin-extent index (docs/phase4/DESIGN-cgo-pin-extents.md, section 7), in a CHILD PROCESS that links golib
// and no cgo package. This suite cannot be that process: it compiles a cgo golden whose module initializer registers C
// libraries at load, which switches the index on. Nor can a second copy of golib in another load context be: golib
// assumes one copy per process, and its module initializer, run again, closes descriptors the console holds on linux and
// darwin. So the child is a separate program, built and run here the way CallerLineMultiLineDebugTests builds its own.
//
// It runs on every host, and fails rather than skips when it cannot build: the linux and darwin legs are the ones the
// load-context shape would have broken.
[TestClass]
public class PinExtentsChildTests
{
    private const int BudgetMs = 360_000;

    [TestMethod]
    public void AProcessThatLinksNoCgoPackageHasNoIndexUntilItRegistersAndThenBackfillsTheExactRegistry()
    {
        string? dotnet = DotnetHost();
        Assert.IsNotNull(dotnet, "no dotnet host resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): the child cannot be built here");

        string? src = SourceRoot();
        Assert.IsNotNull(src, "the source root (the folder holding go2cs.slnx) is not above the test binary");

        string project = Path.Combine(src!, "tests", "GolibTests", "testdata", "pinextents");
        ChildDeadline deadline = new(BudgetMs);
        Stopwatch built = Stopwatch.StartNew();

        (int buildCode, string buildOut) = Run(dotnet!, ["build", Path.Combine(project, "PinExtentsChild.csproj"), "-c", "Release", "-nologo", "-nodeReuse:false"], project, deadline.For(300_000));
        Assert.AreEqual(0, buildCode, $"the child's build failed:{Environment.NewLine}{Tail(buildOut)}");
        Console.WriteLine($"the child's build took {built.ElapsedMilliseconds} ms");

        (int code, string output) = Run(dotnet!, [Path.Combine(project, "bin", "Release", "net10.0", "PinExtentsChild.dll")], project, deadline.For(120_000));
        Assert.AreEqual(0, code, $"the child failed:{Environment.NewLine}{Tail(output)}");

        Dictionary<string, string> lines = Lines(output).Where(line => line.StartsWith('R')).ToDictionary(line => string.Join(' ', line.Split(' ').Take(2)), line => line);

        string Line(string key) => lines.TryGetValue(key, out string? line) ? line : throw new AssertFailedException($"the child printed no `{key}` line:{Environment.NewLine}{output}");

        // R6: no index before or after a pin loop, and no byte of one per pin: a pin costs exactly the buffer object.
        Assert.AreEqual("R6 index-before-pins uncreated", Line("R6 index-before-pins"));
        Assert.AreEqual("R6 index-after-pins uncreated", Line("R6 index-after-pins"));

        string[] bytes = Line("R6 bytes-per-pin").Split(' ');
        // "R6 bytes-per-pin <pin> bare-buffer <bare>"
        Assert.AreEqual(bytes[4], bytes[2], $"a pin allocated more than its buffer object: {Line("R6 bytes-per-pin")}");

        // R5: before registration nothing is refused by extent; registration creates the index and backfills the exact
        // registry's pins, so the value slot's interior is refused. The buffer made before registration and never handed
        // through the exit is row 11, disclosed: it crosses. A pin made after registration enters at construction.
        Assert.AreEqual("R5 before-register slot+4 crossed direct+8 crossed", Line("R5 before-register"));
        Assert.AreEqual("R5 index-after-register created", Line("R5 index-after-register"));
        Assert.AreEqual("R5 after-register slot refused slot+4 refused direct+8 crossed", Line("R5 after-register"), "row 11 (direct+8) is DISCLOSED as crossing");
        Assert.AreEqual("R5 pinned-after-register+8 refused", Line("R5 pinned-after-register+8"));
    }

    private static string[] Lines(string output) =>
        output.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);

    private static string Tail(string output) =>
        string.Join(Environment.NewLine, Lines(output).TakeLast(30));

    private static string? SourceRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "go2cs.slnx")))
                return dir.FullName;
        }

        return null;
    }

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

    private static (int Code, string Output) Run(string file, string[] args, string directory, int timeoutMs)
    {
        ProcessStartInfo start = new(file, args)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(timeoutMs))
        {
            string diagnosis = ChildTimeoutDiagnostics.DescribeAndKill(process, stdout, stderr);
            Assert.Fail($"{Path.GetFileName(file)} {string.Join(' ', args)} did not finish in {timeoutMs / 1000} s{Environment.NewLine}{diagnosis}");
        }

        process.WaitForExit();
        return (process.ExitCode, stdout.Result + stderr.Result);
    }
}
