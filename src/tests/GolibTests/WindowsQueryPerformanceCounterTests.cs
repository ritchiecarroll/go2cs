// WindowsQueryPerformanceCounterTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using winint = go.@internal.syscall.windows_package;

namespace GolibTests;

/// <summary>
/// internal/syscall/windows.QueryPerformanceCounter and QueryPerformanceFrequency are bodyless in Go: runtime
/// pushes their bodies (<c>//go:linkname windows_QueryPerformanceCounter internal/syscall/windows.QueryPerformanceCounter</c>
/// in os_windows.go). Unwired, each was a throwing PartialStubGenerator stub, and the
/// crypto/internal/fips140test host died in one (crypto/internal/fips140deps/time's high-precision now, read by
/// crypto/internal/entropy/v1.0.0). These arms call the two through internal/syscall/windows exactly as that
/// caller does.
/// </summary>
/// <remarks>
/// The oracle is .NET's own read of the same two kernel32 counters: on Windows, Stopwatch.GetTimestamp is
/// QueryPerformanceCounter and Stopwatch.Frequency is QueryPerformanceFrequency (Stopwatch.IsHighResolution).
/// So the frequency must be EQUAL, and a counter read must fall between two Stopwatch reads taken around it.
/// That separates Go's counter from any other monotonic clock, which a plain "it increases" arm would not.
/// Windows only: the package is windows-exclusive, and this file is Compile Removed for other flavours.
/// </remarks>
[TestClass]
public class WindowsQueryPerformanceCounterTests
{
    private static bool OnWindows => System.OperatingSystem.IsWindows();

    [TestMethod]
    public void TheFrequencyIsTheKernelsPerformanceFrequency()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("QueryPerformanceFrequency is the windows flavor");
            return;
        }

        long frequency = winint.QueryPerformanceFrequency();

        Assert.IsTrue(frequency > 0, $"QueryPerformanceFrequency() = {frequency}, want > 0");
        Assert.AreEqual(Stopwatch.Frequency, frequency, "the frequency is not QueryPerformanceFrequency's (Stopwatch.Frequency reads that call)");
    }

    [TestMethod]
    public void TheCounterIsTheKernelsPerformanceCounter()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("QueryPerformanceCounter is the windows flavor");
            return;
        }

        long before = Stopwatch.GetTimestamp();
        long counter = winint.QueryPerformanceCounter();
        long after = Stopwatch.GetTimestamp();

        Assert.IsTrue(before <= counter && counter <= after,
            $"QueryPerformanceCounter() = {counter} is not between two reads of the same counter ({before}, {after})");
    }

    [TestMethod]
    public void TheCounterRises()
    {
        if (!OnWindows)
        {
            Assert.Inconclusive("QueryPerformanceCounter is the windows flavor");
            return;
        }

        long first = winint.QueryPerformanceCounter();
        Thread.Sleep(1);
        long second = winint.QueryPerformanceCounter();

        Assert.IsTrue(second > first, $"QueryPerformanceCounter() read {first}, then {second} a millisecond later");
    }
}
