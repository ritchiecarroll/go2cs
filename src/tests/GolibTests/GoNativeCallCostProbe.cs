// GoNativeCallCostProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// A MEASUREMENT, not a guard (S-a, docs/phase4/DESIGN-native-call-seam.md §5.2). OPT-IN: it runs only when
// GO2CS_NATIVE_SEAM_PROBE names a file to append READING lines to, and reads Inconclusive otherwise, so the suite
// never pays for it.
//
// What it measures is what GoNativeCall costs a real kernel transition. Each arm is one call through a
// `delegate* unmanaged` to the binding host's anchor:
//   A0  the anchor alone;
//   A1  the anchor inside Enter / try / finally Exit, on a goroutine (the seam's uncounted path);
//   A2  A1 on a goroutine counted in runningPanicDefers (the path MainExit arms; set through reflection, test-only);
//   A3  A1 with a ReenterManaged / ReturnToNative pair inside (one callback level);
//   C+  A1 with Thread.SpinWait(1) inside: the POSITIVE CONTROL. Its excess over A1 must match the spin's own cost,
//       measured alone as SPIN, or the instrument cannot read a few nanoseconds at all.
//
// The anchor must be a real kernel transition, never a user-mode read (the cross-host anchor-artifact trap):
// windows GetProcessTimes on the current process, linux syscall(SYS_getppid) through libc, darwin getppid. The
// design names SwitchToThread for windows; GetProcessTimes replaces it because a yield can switch threads, which
// adds load-dependent noise to the very arm the others are read against.
//
// Protocol per process: a warm-up round, then Rounds rounds, each running every arm in turn (interleaved, so
// drift hits every arm alike), each arm Iterations calls. The process's figure for an arm is its fastest round.
// The figure of record is the FLOOR over processes, per arm, as absolute ns/call with A0 named as the base, under
// both tiering configurations (default, and DOTNET_TieredCompilation=0).
[TestClass]
public unsafe class GoNativeCallCostProbe
{
    private const string OutputVariable = "GO2CS_NATIVE_SEAM_PROBE";
    private const int Iterations = 1_000_000;
    private const int Rounds = 5;

    private static delegate*<long> s_anchor;
    private static long s_sink;

    [TestMethod]
    public void TheSeamsCostOverARealKernelTransitionIsMeasured()
    {
        string? output = Environment.GetEnvironmentVariable(OutputVariable);

        if (string.IsNullOrEmpty(output))
        {
            Assert.Inconclusive($"opt-in: set {OutputVariable} to a file the READING lines are appended to");
            return;
        }

        string anchorName = BindAnchor();
        Exception? failure = null;
        StringBuilder lines = new();

        Thread thread = new(() =>
        {
            try
            {
                using (Goroutine.Enter())
                    Measure(anchorName, lines);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.Start();
        thread.Join();

        if (failure is not null)
            throw new AssertFailedException($"the probe failed on its thread: {failure}", failure);

        File.AppendAllText(output, lines.ToString());
    }

    private static void Measure(string anchorName, StringBuilder lines)
    {
        (string Name, Func<long> Run)[] arms =
        [
            ("A0", A0), ("A1", A1), ("A2", A2), ("A3", A3), ("C+", ControlPlus), ("SPIN", SpinAlone)
        ];

        double[] best = new double[arms.Length];
        Array.Fill(best, double.MaxValue);

        foreach ((_, Func<long> run) in arms)
            run(); // warm-up round, discarded

        for (int round = 0; round < Rounds; round++)
        {
            for (int arm = 0; arm < arms.Length; arm++)
            {
                double ns = arms[arm].Run() * 1e9 / Stopwatch.Frequency / Iterations;
                best[arm] = Math.Min(best[arm], ns);
            }
        }

        string tiering = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default";
        string stamp = $"pid={Environment.ProcessId} anchor={anchorName} tiering={tiering} os={RuntimeInformation.OSDescription.Replace(' ', '_')} arch={RuntimeInformation.ProcessArchitecture}";

        for (int arm = 0; arm < arms.Length; arm++)
            lines.AppendLine(string.Create(CultureInfo.InvariantCulture, $"READING {stamp} arm={arms[arm].Name} ns_per_call={best[arm]:F2}"));
    }

    private static string BindAnchor()
    {
        if (OperatingSystem.IsWindows())
        {
            s_anchor = &WindowsGetProcessTimes;
            return "GetProcessTimes";
        }

        nint libc = NativeLibrary.Load(OperatingSystem.IsMacOS() ? "libSystem.dylib" : "libc.so.6");

        if (OperatingSystem.IsMacOS())
        {
            s_getppid = (delegate* unmanaged<int>)NativeLibrary.GetExport(libc, "getppid");
            s_anchor = &DarwinGetppid;
            return "getppid";
        }

        s_syscall = (delegate* unmanaged<nint, nint>)NativeLibrary.GetExport(libc, "syscall");
        s_anchor = &LinuxSyscallGetppid;
        return "syscall(SYS_getppid)";
    }

    // The anchors are thin managed shims over the raw function pointer, so every arm pays the same shim.
    private static delegate* unmanaged<nint, long*, long*, long*, long*, int> s_getProcessTimes;
    private static delegate* unmanaged<int> s_getppid;
    private static delegate* unmanaged<nint, nint> s_syscall;

    private static long WindowsGetProcessTimes()
    {
        if (s_getProcessTimes is null)
            s_getProcessTimes = (delegate* unmanaged<nint, long*, long*, long*, long*, int>)NativeLibrary.GetExport(NativeLibrary.Load("kernel32.dll"), "GetProcessTimes");

        long creation, exit, kernel, user;
        return s_getProcessTimes(-1, &creation, &exit, &kernel, &user) + user;
    }

    private static long DarwinGetppid() => s_getppid();

    private static long LinuxSyscallGetppid() =>
        s_syscall(RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? 173 : 110);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long A0()
    {
        long sum = 0;
        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < Iterations; i++)
            sum += s_anchor();

        long elapsed = Stopwatch.GetTimestamp() - start;
        s_sink += sum;
        return elapsed;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long A1()
    {
        long sum = 0;
        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < Iterations; i++)
        {
            GoNativeCall.Enter();

            try
            {
                sum += s_anchor();
            }
            finally
            {
                GoNativeCall.Exit();
            }
        }

        long elapsed = Stopwatch.GetTimestamp() - start;
        s_sink += sum;
        return elapsed;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long A2()
    {
        SetCounted(true);

        try
        {
            return A1();
        }
        finally
        {
            SetCounted(false);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long A3()
    {
        long sum = 0;
        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < Iterations; i++)
        {
            GoNativeCall.Enter();

            try
            {
                sum += s_anchor();
                GoNativeCall.ReenterManaged();
                GoNativeCall.ReturnToNative();
            }
            finally
            {
                GoNativeCall.Exit();
            }
        }

        long elapsed = Stopwatch.GetTimestamp() - start;
        s_sink += sum;
        return elapsed;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long ControlPlus()
    {
        long sum = 0;
        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < Iterations; i++)
        {
            GoNativeCall.Enter();

            try
            {
                Thread.SpinWait(1);
                sum += s_anchor();
            }
            finally
            {
                GoNativeCall.Exit();
            }
        }

        long elapsed = Stopwatch.GetTimestamp() - start;
        s_sink += sum;
        return elapsed;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long SpinAlone()
    {
        long start = Stopwatch.GetTimestamp();

        for (int i = 0; i < Iterations; i++)
            Thread.SpinWait(1);

        return Stopwatch.GetTimestamp() - start;
    }

    // Test-only: MainExit's counted state, which no public surface sets.
    private static void SetCounted(bool counted)
    {
        MethodInfo set = typeof(Goroutine).GetMethod("SetPanicDefersCounted", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Goroutine.SetPanicDefersCounted not found: the probe's A2 arm needs re-pointing");

        set.Invoke(Goroutine.Current, [counted]);
    }
}
