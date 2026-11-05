// TimingLinesTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceRunner;

namespace PerformanceRunnerTests;

/// <summary>
/// A benchmark may time parts of its workload on their own and print each as a named part,
/// <c>elapsed_ns.&lt;name&gt;: &lt;n&gt;</c>, beside the row's <c>elapsed_ns: &lt;n&gt;</c> (PerfDeferPanic does). A part
/// differs run to run as the total does, so Verify must drop it, or the three binaries never compare equal; and
/// Measure must read each part, or the row reports only the sum. The total is still the row's time.
/// </summary>
[TestClass]
public class TimingLinesTests
{
    private const string Output =
        "checksum: 24000000 7500000\r\n" +
        "elapsed_ns.defer_literal: 30000000\r\n" +
        "elapsed_ns.defer_method: 32000000\r\n" +
        "elapsed_ns.panic_recover: 55000000\r\n" +
        "elapsed_ns: 117000000\r\n";

    [TestMethod]
    public void VerifyComparesNoTimingLine()
    {
        Assert.AreEqual("checksum: 24000000 7500000\n", TimingLines.Filter(Output));
    }

    [TestMethod]
    public void TheTotalIsStillTheRowsTime()
    {
        Assert.AreEqual(117.0, TimingLines.TotalMs(Output), 1e-9);
    }

    [TestMethod]
    public void EachPartIsReadInTheOrderItWasPrinted()
    {
        IReadOnlyList<(string Name, double Ms)> parts = TimingLines.Parts(Output);

        CollectionAssert.AreEqual(new[] { "defer_literal", "defer_method", "panic_recover" }, parts.Select(p => p.Name).ToArray());
        CollectionAssert.AreEqual(new[] { 30.0, 32.0, 55.0 }, parts.Select(p => p.Ms).ToArray());
    }

    [TestMethod]
    public void ARowWithoutPartsHasNone()
    {
        Assert.AreEqual(0, TimingLines.Parts("checksum: 1\nelapsed_ns: 5\n").Count);
        Assert.AreEqual("checksum: 1\n", TimingLines.Filter("checksum: 1\nelapsed_ns: 5\n"));
    }

    [TestMethod]
    public void ALineThatOnlyLooksLikeAPartIsOutput()
    {
        // No name, or no colon: the program printed it as output, so Verify compares it and Measure skips it.
        const string text = "elapsed_ns.: 5\nelapsed_ns.note\nelapsed_ns: 5\n";

        Assert.AreEqual("elapsed_ns.: 5\nelapsed_ns.note\n", TimingLines.Filter(text));
        Assert.AreEqual(0, TimingLines.Parts(text).Count);
    }
}
