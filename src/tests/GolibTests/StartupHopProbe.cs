// StartupHopProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// PROBE BRANCH ONLY (C46, never merged): the start-up cost of running a program's main on a second thread that reserves
// 1 GiB, on the runner's own RID, JIT and Native AOT. A tiny console program is built twice in a temp directory; each
// arm is a separate process, interleaved, and each process reports the in-process cost of the hop (Stopwatch around
// create + Start + Join of an empty body) while the parent times the whole process. The result is reported through
// Assert.Fail so the job log carries it whatever the logger verbosity.
[TestClass]
public class StartupHopProbe
{
    private const int Runs = 30;

    private const string Project = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net10.0</TargetFramework>
            <InvariantGlobalization>true</InvariantGlobalization>
            <ImplicitUsings>disable</ImplicitUsings>
          </PropertyGroup>
        </Project>
        """;

    private const string Program = """
        using System.Diagnostics;
        using System.Threading;

        static class P
        {
            static void Main(string[] args)
            {
                long t0 = Stopwatch.GetTimestamp();
                switch (args[0])
                {
                    case "inline": Body(); break;
                    case "default": { Thread t = new(Body) { IsBackground = true }; t.Start(); t.Join(); break; }
                    default: { Thread t = new(Body, 1024 * 1024 * 1024) { IsBackground = true }; t.Start(); t.Join(); break; }
                }
                long t1 = Stopwatch.GetTimestamp();
                System.Console.WriteLine(Stopwatch.GetElapsedTime(t0, t1).TotalMicroseconds.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
            }

            static void Body() { }
        }
        """;

    private static readonly string[] Arms = ["inline", "default", "gib"];

    [TestMethod]
    public void MeasureTheMainThreadHop()
    {
        string rid = RuntimeInformation.RuntimeIdentifier;
        string root = Path.Combine(Path.GetTempPath(), $"go2cs-c46-probe-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "hop.csproj"), Project);
        File.WriteAllText(Path.Combine(root, "Program.cs"), Program);

        StringBuilder report = new();
        report.AppendLine($"C46 PROBE RESULT (not a failure) -- rid {rid}, {Environment.ProcessorCount} cpus, {RuntimeInformation.OSDescription}, runtime {RuntimeInformation.FrameworkDescription}, {Runs} processes per arm, interleaved");

        string jitDir = Path.Combine(root, "jit");
        string aotDir = Path.Combine(root, "aot");
        string exe = OperatingSystem.IsWindows() ? "hop.exe" : "hop";

        Run("dotnet", ["build", root, "-c", "Release", "-nologo", "-o", jitDir], report, "jit build");
        Run("dotnet", ["publish", root, "-c", "Release", "-nologo", "-r", rid, "-p:PublishAot=true", "-o", aotDir], report, "aot publish");

        foreach ((string label, string dir) in new[] { ("JIT", jitDir), ("AOT", aotDir) })
        {
            string path = Path.Combine(dir, exe);

            if (!File.Exists(path))
            {
                report.AppendLine($"{label}: no executable at {path}");
                continue;
            }

            Dictionary<string, List<double>> hop = Arms.ToDictionary(a => a, _ => new List<double>());
            Dictionary<string, List<double>> wall = Arms.ToDictionary(a => a, _ => new List<double>());

            foreach (string arm in Arms)
                Child(path, arm); // warm the file cache once per arm, not recorded

            for (int i = 0; i < Runs; i++)
            {
                foreach (string arm in Arms)
                {
                    (double h, double w) = Child(path, arm);
                    hop[arm].Add(h);
                    wall[arm].Add(w);
                }
            }

            foreach (string arm in Arms)
                report.AppendLine($"{label} {arm,-7} hop us: {Stats(hop[arm])}   process ms: {Stats(wall[arm])}");
        }

        try { Directory.Delete(root, recursive: true); } catch (Exception) { }

        Assert.Fail(report.ToString());
    }

    private static (double Hop, double Wall) Child(string path, string arm)
    {
        ProcessStartInfo start = new(path, [arm]) { RedirectStandardOutput = true, UseShellExecute = false };
        long t0 = Stopwatch.GetTimestamp();
        using Process process = Process.Start(start)!;
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        double wall = Stopwatch.GetElapsedTime(t0).TotalMilliseconds;

        return (double.Parse(output.Trim(), System.Globalization.CultureInfo.InvariantCulture), wall);
    }

    private static string Stats(List<double> values)
    {
        double[] sorted = values.OrderBy(v => v).ToArray();
        return $"min {sorted[0]:F1} median {sorted[sorted.Length / 2]:F1} p90 {sorted[(int)(sorted.Length * 0.9)]:F1} max {sorted[^1]:F1}";
    }

    private static void Run(string file, string[] args, StringBuilder report, string step)
    {
        ProcessStartInfo start = new(file, args) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        using Process process = Process.Start(start)!;
        System.Threading.Tasks.Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        System.Threading.Tasks.Task<string> stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (process.ExitCode != 0)
            report.AppendLine($"{step} exit {process.ExitCode}:\n{stdout.Result}\n{stderr.Result}");
    }
}
