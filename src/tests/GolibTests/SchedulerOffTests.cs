// SchedulerOffTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// The M:N scheduler is experimental and opt-in: inert unless the run sets <c>GO2CS_MN=1</c>. With it unset, a goroutine
/// is a thread, no carrier or monitor starts, and a goroutine that parks pays nothing to the scheduler: none of its
/// counters moves. A park in default mode used to count itself in <c>thread-blocks</c>, an interlocked add on one global
/// word from every blocking goroutine of every program (release review TC-8).
/// </summary>
[TestClass]
public class SchedulerOffTests
{
    private const int Goroutines = 8;

    [TestMethod]
    public void WithTheSchedulerOffNoCarrierStartsAndParksMoveNoCounter()
    {
        if (GoScheduler.Enabled)
            Assert.Inconclusive("GO2CS_MN=1 is set for this run: the scheduler-off row is NOT MEASURED");

        Dictionary<string, long> before = Counters();

        // Each goroutine parks in a channel receive (its outermost plain park) until the test sends to it.
        channel<int> work = new(0);
        channel<int> done = new(0);

        for (int i = 0; i < Goroutines; i++)
        {
            goǃ(() =>
            {
                int value = work.Receive();
                done.Send(value);
            });
        }

        Thread.Sleep(100);

        for (int i = 0; i < Goroutines; i++)
            work.Send(i);

        for (int i = 0; i < Goroutines; i++)
            done.Receive();

        Dictionary<string, long> after = Counters();

        Assert.AreEqual(0L, after["carriers-started"], "no carrier starts with the scheduler off");
        Assert.AreEqual(0L, after["peak-carriers"], "and none runs");

        foreach (string name in new[] { "thread-blocks", "handoffs", "parks", "steps", "injections", "retakes" })
            Assert.AreEqual(before[name], after[name], $"{name} moved by {after[name] - before[name]} across {Goroutines} parked goroutines");
    }

    // The scheduler's one-line reading, as numbers by name. Reading() is a report; it starts nothing.
    private static Dictionary<string, long> Counters()
    {
        Dictionary<string, long> counters = new(StringComparer.Ordinal);

        foreach (Match match in Regex.Matches(GoScheduler.Reading(), @"(?<name>[a-z-]+)=(?<value>-?\d+)"))
            counters[match.Groups["name"].Value] = long.Parse(match.Groups["value"].Value);

        foreach (string name in new[] { "carriers-started", "peak-carriers", "thread-blocks", "handoffs", "parks", "steps", "injections", "retakes" })
            Assert.IsTrue(counters.ContainsKey(name), $"the reading names {name}: {GoScheduler.Reading()}");

        return counters;
    }
}
