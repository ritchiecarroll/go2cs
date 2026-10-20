// GoroutineCreatorSiteTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the cost of a <c>go</c> statement against the depth of the stack it runs on (F2, 2026-10-11). Goroutine
/// creation recorded its creator by resolving a frame for EVERY frame on the stack, so a <c>go</c> deep in a recursion
/// paid for the whole recursion: go-cmp's cyclic Transformer test (65,536 levels, a goroutine launched from that depth)
/// took 386 s, Go 0.3 s. The creator of a <c>go</c> site never changes, so it is resolved once per site.
/// </summary>
[TestClass]
public class GoroutineCreatorSiteTests
{
    private const int Depth = 50_000;
    private const int Launches = 200;

    // RED before the per-site creator: every one of the 200 launches resolves the 50,000-frame stack. The bound is
    // generous on purpose -- a slow host must not fail it, only the per-launch stack walk.
    [TestMethod]
    public void AGoStatementDeepInTheStackCostsNoMoreThanAShallowOne()
    {
        TimeSpan elapsed = default;
        Thread runner = new(() => elapsed = DeepThenLaunch(Depth, Launches), 512 * 1024 * 1024);

        runner.Start();
        runner.Join();

        Assert.IsTrue(elapsed < TimeSpan.FromSeconds(3), $"{Launches} goroutines launched from {Depth:N0} frames deep took {elapsed.TotalSeconds:F2} s");
    }

    // The mechanism: one walk per `go` site, never one per `go`.
    [TestMethod]
    public void ASiteResolvesItsCreatorOnce()
    {
        long before = Interlocked.Read(ref Goroutine.CreatorWalks);
        using CountdownEvent done = new(100);

        for (int i = 0; i < 100; i++)
            builtin.goǃ(() => done.Signal());

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutines ran");
        Assert.AreEqual(1L, Interlocked.Read(ref Goroutine.CreatorWalks) - before, "100 launches from one site walk the stack once");
    }

    // The cached answer is the walk's answer: every launch from a site reports the method that holds the `go`.
    [TestMethod]
    public void ACachedSiteReportsTheSameCreator()
    {
        System.Reflection.MethodBase?[] creators = new System.Reflection.MethodBase?[3];
        using CountdownEvent done = new(creators.Length);

        for (int i = 0; i < creators.Length; i++)
        {
            int slot = i;
            Launch(() => { creators[slot] = Goroutine.Current?.Creator; done.Signal(); });
        }

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutines ran");

        foreach (System.Reflection.MethodBase? creator in creators)
            Assert.AreEqual(nameof(Launch), creator?.Name, "the creator is the method holding the go statement");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Launch(Action body) => builtin.goǃ(body);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TimeSpan DeepThenLaunch(int depth, int launches)
    {
        if (depth > 0)
            return DeepThenLaunch(depth - 1, launches) + TimeSpan.Zero;

        using CountdownEvent done = new(launches);
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < launches; i++)
            builtin.goǃ(() => done.Signal());

        done.Wait();

        return stopwatch.Elapsed;
    }
}
