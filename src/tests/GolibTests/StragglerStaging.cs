using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// Stages the goroutine an earlier test leaves behind -- live when a test takes its baseline, retiring
/// before the test compares -- deterministically, and reads the registry the way a straggler cannot
/// falsify.
/// </summary>
/// <remarks>
/// <para>
/// A test that starts a goroutine and returns once the goroutine's body SIGNALS leaves that goroutine
/// registered until its thread runs Unregister, after the body. The next test's baseline counts it, and
/// a count compared later reads it gone. <see cref="Stage"/> makes that happen on purpose: it parks a
/// goroutine, and the returned hook -- called right after a baseline read -- releases it and waits until
/// the registry no longer holds it, so the retirement lands inside the window every time.
/// </para>
/// <para>
/// The registry cannot tell a retiring goroutine from a long-lived parked one, so "wait until only the
/// known live set remains" has no end condition. What a straggler CAN'T do is appear: ids are minted
/// fresh and never reused, and in a serially run assembly nothing else starts a goroutine inside a
/// test's window. So <see cref="LiveIds"/> and <see cref="Minted"/> answer "did this minted anything"
/// exactly, and a goroutine a test is ABOUT is asserted present by its own id.
/// </para>
/// </remarks>
internal static class StragglerStaging
{
    private const int TimeoutMs = 30000;

    internal const int Iterations = 10;

    /// <summary>
    /// Parks a fresh goroutine and returns the hook that retires it: release, then wait until the
    /// registry no longer holds it.
    /// </summary>
    internal static Action Stage()
    {
        ManualResetEventSlim started = new(false);
        ManualResetEventSlim release = new(false);
        long id = 0;

        Goroutine.Start(() =>
        {
            id = Goroutine.Current!.Id;
            started.Set();
            release.Wait();
        });

        Assert.IsTrue(started.Wait(TimeoutMs), "the staged goroutine did not start");

        // No assertion INSIDE the hook: a caller may run it on a goroutine (NestedEnterKeepsOneIdentity),
        // where an escaping AssertFailedException would take the host down through golib's backstop. A
        // staged goroutine that never retired shows up as the caller's own count reading one high.
        return () =>
        {
            release.Set();
            SpinWait.SpinUntil(() => Goroutine.FromId(id) is null, TimeoutMs);
        };
    }

    /// <summary>The ids of every goroutine the registry holds right now.</summary>
    internal static HashSet<long> LiveIds()
    {
        HashSet<long> ids = [];

        foreach (Goroutine goroutine in Goroutine.Snapshot())
            ids.Add(goroutine.Id);

        return ids;
    }

    /// <summary>The goroutines live now whose ids were not live in <paramref name="before"/>: the ones minted since.</summary>
    internal static List<Goroutine> Minted(HashSet<long> before)
    {
        List<Goroutine> minted = [];

        foreach (Goroutine goroutine in Goroutine.Snapshot())
        {
            if (!before.Contains(goroutine.Id))
                minted.Add(goroutine);
        }

        return minted;
    }
}
