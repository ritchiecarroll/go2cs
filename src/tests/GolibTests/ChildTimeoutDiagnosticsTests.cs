// ChildTimeoutDiagnosticsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Guards <see cref="ChildTimeoutDiagnostics"/>: a child that does not finish is described before it is
/// killed, with the frame a thread is parked in and the output written before the kill.
/// </summary>
/// <remarks>
/// The child is `dotnet test` running <see cref="AHangingChildProbe"/> from this assembly, so the thread that
/// parks is in a testhost GRANDCHILD, as it would be under any `dotnet test` or `dotnet build` child. The
/// frame is found only if the helper walks the tree below the child it was handed.
/// </remarks>
[TestClass]
public class ChildTimeoutDiagnosticsTests
{
    private const string ProbeVariable = "G2CS_CHILD_HANG_PROBE_MARKER";

    [TestMethod]
    public void ATimedOutChildIsDescribedWithItsParkedFrameBeforeItIsKilled()
    {
        // Measured on Linux only. Windows has no ps for the tree walk; macOS dumps are written, but this file
        // does not claim ClrMD reads them. Elsewhere the helper still runs and says in its text what failed.
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("the parked-frame assertion is measured on Linux only");

        string marker = Path.Combine(Path.GetTempPath(), "g2cs-hang-probe-" + Guid.NewGuid().ToString("N"));
        string filter = $"FullyQualifiedName={typeof(ChildTimeoutDiagnosticsTests).FullName}.{nameof(AHangingChildProbe)}";
        string dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host && File.Exists(host) ? host : "dotnet";

        ProcessStartInfo start = new(dotnet, ["test", typeof(ChildTimeoutDiagnosticsTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ProbeVariable] = marker;

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        try
        {
            // The probe writes the marker immediately before it parks.
            Stopwatch waited = Stopwatch.StartNew();

            while (!File.Exists(marker) && !process.HasExited && waited.Elapsed < TimeSpan.FromSeconds(120))
                Thread.Sleep(200);

            Assert.IsTrue(File.Exists(marker), $"the probe never parked (exited: {process.HasExited})");
            Thread.Sleep(500);

            string text = ChildTimeoutDiagnostics.DescribeAndKill(process, stdout, stderr);
            Console.WriteLine(text);

            StringAssert.Contains(text, nameof(ParkedForTheTimeoutDiagnostics), "the parked frame is named\n" + text);
            StringAssert.Contains(text, "--- child stdout before the kill ---", text);
            Assert.IsTrue(process.HasExited, "the child is killed after it is described");
        }
        finally
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            File.Delete(marker);
        }
    }

    /// <summary>The child's test: inert unless the parent names a marker file; then it parks forever.</summary>
    [TestMethod]
    public void AHangingChildProbe()
    {
        string? marker = Environment.GetEnvironmentVariable(ProbeVariable);

        if (string.IsNullOrEmpty(marker))
            Assert.Inconclusive("runs only as ChildTimeoutDiagnosticsTests' child");
        else
            ParkedForTheTimeoutDiagnostics(marker);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ParkedForTheTimeoutDiagnostics(string marker)
    {
        using ManualResetEventSlim never = new(false);
        File.WriteAllText(marker, "parked");
        never.Wait();
    }
}
