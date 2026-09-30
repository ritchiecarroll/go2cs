// ThreadStateCensusTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the one hazard of reusing a thread for goroutine after goroutine
/// (<see cref="GoroutineThreadPool"/>, which <see cref="Coro"/> runs on): per-THREAD state is not
/// per-GOROUTINE state, so a pooled thread must carry nothing from its previous goroutine.
/// </summary>
/// <remarks>
/// <para>
/// Two arms. The CENSUS reads the source tree for every <c>[ThreadStatic]</c>, <c>ThreadLocal</c> and
/// <c>AsyncLocal</c> field and requires each to be listed below with its disposition and a PROOF -- a
/// line that must exist in that file, showing the reset (or naming why none is owed). A new
/// thread-static fails until it is classified, and an entry whose field or proof is gone fails too, so
/// the list stays exactly the tree.
/// </para>
/// <para>
/// The REUSE arm runs goroutine A on a pooled thread, dirties every golib-owned slot and a slot
/// registered the way packages above golib register theirs, then runs goroutine B on the SAME thread
/// and asserts B sees none of it -- and a goroutine identity of its own.
/// </para>
/// </remarks>
[TestClass]
public class ThreadStateCensusTests
{
    private enum Disposition
    {
        /// <summary>Reset by golib's own <c>GoroutineThreadState.ResetForReuse</c>.</summary>
        GolibReset,

        /// <summary>Reset by a <c>GoroutineThreadState.Register</c> call in the declaring file.</summary>
        Registered,

        /// <summary>Retired by <c>Goroutine.Run</c>'s own scope at every goroutine's end.</summary>
        RetiredByRun,

        /// <summary>An <c>AsyncLocal</c>: flows with the ExecutionContext, which every pooled body runs under afresh.</summary>
        FlowsWithContext,

        /// <summary>A per-THREAD resource, not goroutine state: kept across goroutines on purpose.</summary>
        KeptThreadResource
    }

    private static readonly (string Key, Disposition Disposition, string Proof, string Reason)[] Census =
    [
        ("golib/AllocationCounter.cs|t_count", Disposition.GolibReset, "internal static void ResetThread() => t_count = 0;", "the per-thread allocation tally"),
        ("golib/builtin.cs|s_fallthrough", Disposition.GolibReset, "internal static void ResetFallthrough() => s_fallthrough.Value = false;", "a switch fallthrough flag, set and consumed within one statement"),
        ("golib/channel.cs|t_frames", Disposition.GolibReset, "internal static void ResetThread() => t_frames = null;", "a select's pending receive frames"),
        ("golib/GoThreadState.cs|t_current", Disposition.GolibReset, "internal static void ResetThread() { if (t_current is { } state) state.Reset(); }", "the thread's ONE panic-and-defer holder: the five panic slots (captured, handled, unclaimed, in-flight foreign, recoverable), the panic-sequence depth and the entries captureCallers pairs with GoFrame.Run frames (GoFuncRoot.ResetThread calls GoThreadState.ResetThread)"),
        ("golib/GoFrame.cs|t_lastActivation", Disposition.KeptThreadResource, "[ThreadStatic] private static long t_lastActivation;", "the last Run activation number on this thread: it only ever increases, so it is unique WITHIN the thread (not across threads; a panic's SiteOwnerThread carries the thread) and needs no reset"),
        ("golib/GoFrame.cs|t_splicesRefused", Disposition.GolibReset, "t_splicesRefused = false;", "set once a stopped range-over-func seq resumes on its coro thread; cleared with the sequences (GoFrame.ResetSequences)"),
        ("golib/PanicException.cs|t_lastAdopted", Disposition.GolibReset, "internal static void ResetThread() => t_lastAdopted = null;", "the panic this thread's catches adopted last, which a range-over-func early stop marks (GoFuncRoot.ResetThread calls PanicException.ResetThread)"),
        ("golib/GoFrame.cs|t_threadToken", Disposition.KeptThreadResource, "[ThreadStatic] private static object? t_threadToken;", "this thread's identity for a panic's SiteOwnerThread: a thread property, not goroutine state"),
        ("golib/GoexitException.cs|t_started", Disposition.GolibReset, "internal static void ResetThread() => t_started = false;", "whether a Goexit has been raised on this goroutine (GoFuncRoot.ResetThread calls GoexitException.ResetThread)"),
        ("golib/runtime/GoschedBackoff.cs|t_consecutiveInertYields", Disposition.GolibReset, "internal static void ResetThread() => t_consecutiveInertYields = 0;", "a Gosched backoff streak"),
        ("golib/runtime/Goroutine.cs|t_current", Disposition.RetiredByRun, "t_current = null;", "the goroutine identity; Goroutine.Run's scope retires it"),
        ("golib/runtime/Goroutine.cs|s_profileLabels", Disposition.FlowsWithContext, "AsyncLocal<object?> s_profileLabels", "pprof labels, inherited at goroutine creation"),
        ("runtime/stubs_impl.cs|t_getg", Disposition.Registered, "GoroutineThreadState.Register(static () => t_getg = null)", "getg()'s cache of the goroutine's g"),
        ("runtime/stubs_impl.cs|t_getgOwner", Disposition.Registered, "GoroutineThreadState.Register(static () => t_getgOwner = null)", "the golib identity getg()'s cache was minted for"),
        ("golib/GoMemProfile.cs|t_recording", Disposition.Registered, "GoroutineThreadState.Register(static () => t_recording = false)", "true only while this thread records a memory-profile sample (also cleared in its finally)"),
        ("golib/GoMemProfile.cs|t_nextSample", Disposition.KeptThreadResource, "private static long t_nextSample;", "the bytes left until this thread's next memory-profile sample: Go keeps it on the M (mcache.nextSample), a thread, so it carries across goroutines as in Go"),
        ("golib/GoMemProfile.cs|t_seeded", Disposition.KeptThreadResource, "private static bool t_seeded;", "whether this thread's sample countdown is seeded, as Go seeds it once per mcache"),
        ("golib/GoCheapRand.cs|t_state", Disposition.KeptThreadResource, "private static ulong t_state;", "Go's cheaprand state (where each map range starts): Go keeps it on the M (m.cheaprand), a thread, so it carries across goroutines as in Go"),
        ("runtime/cpusampler_impl.cs|t_cpuSamplerDraining", Disposition.Registered, "GoroutineThreadState.Register(static () => t_cpuSamplerDraining = false)", "true only inside a CPU sampler's Stop drain (also cleared in its finally)"),
        ("runtime/debug/stubs_impl.cs|t_panicOnFault", Disposition.Registered, "GoroutineThreadState.Register(static () => t_panicOnFault = false)", "debug.SetPanicOnFault, per goroutine in Go"),
        ("runtime/lock_managed_impl.cs|t_heldLocks", Disposition.Registered, "Array.Clear(held);", "the runtime locks this goroutine holds"),
        ("runtime/lock_managed_impl.cs|t_heldCount", Disposition.Registered, "t_heldCount = 0;", "the runtime locks this goroutine holds"),
        ("runtime/managed_impl.cs|t_chunk", Disposition.KeptThreadResource, "private static byte[]? t_chunk;", "the print sink's reused chunk buffer: scratch whose contents never outlive one Write call, so it carries no goroutine state"),
        ("runtime/lock_managed_impl.cs|t_runtimeLockProfilePending", Disposition.Registered, "t_runtimeLockProfilePending = false;", "whether this goroutine's M has a runtime-lock contention record for an unlock to carry (the mutex-profile early-out)"),
        ("sync/runtime_impl.cs|t_procId", Disposition.Registered, "t_procId = 0;", "sync.Pool's shard id for this goroutine"),
        ("sync/runtime_impl.cs|t_procIdAssigned", Disposition.Registered, "t_procIdAssigned = false;", "sync.Pool's shard id for this goroutine"),
        ("testing/TestExecution.cs|s_current", Disposition.FlowsWithContext, "AsyncLocal<TestExecution?> s_current", "the test a goroutine belongs to, inherited from its creator"),
        ("time/time_impl.cs|t_highResTimer", Disposition.KeptThreadResource, "private static highResTimer? t_highResTimer;", "a per-thread OS high-resolution timer handle: a thread resource, reused across goroutines on purpose"),
        ("time/time_impl.cs|t_highResTimerProbed", Disposition.KeptThreadResource, "private static bool t_highResTimerProbed;", "whether this thread probed for that timer"),
    ];

    private static string CoreRoot([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", "core"));

    private static readonly Regex s_fieldName = new(@"\b(\w+)\s*(=|;)", RegexOptions.Compiled);
    private static readonly Regex s_localField = new(@"(ThreadLocal|AsyncLocal)<.+>\s+(\w+)\s*=", RegexOptions.Compiled);

    [TestMethod]
    public void EveryThreadStaticInTheTree_IsClassified()
    {
        string root = CoreRoot();

        if (!Directory.Exists(Path.Combine(root, "golib")))
            Assert.Inconclusive($"source not found at {root}; this guard reads the tree it was built from");

        SortedSet<string> found = [];
        Dictionary<string, string> text = [];

        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');

            if (relative.Contains("/bin/") || relative.Contains("/obj/"))
                continue;

            string source = File.ReadAllText(file);

            if (!source.Contains("ThreadStatic") && !source.Contains("ThreadLocal<") && !source.Contains("AsyncLocal<"))
                continue;

            text[relative] = source;
            string[] lines = source.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string code = stripComment(lines[i]);

                if (code.Contains("[ThreadStatic]"))
                {
                    // The field is on this line after the attribute, or on the next code line.
                    string rest = code[(code.IndexOf("[ThreadStatic]", StringComparison.Ordinal) + "[ThreadStatic]".Length)..];

                    for (int j = i + 1; rest.Trim().Length == 0 && j < lines.Length; j++)
                        rest = stripComment(lines[j]);

                    if (s_fieldName.Match(rest) is { Success: true } m)
                        found.Add($"{relative}|{m.Groups[1].Value}");
                }
                else if (s_localField.Match(code) is { Success: true } local)
                {
                    found.Add($"{relative}|{local.Groups[2].Value}");
                }
            }
        }

        string[] listed = Census.Select(static e => e.Key).ToArray();
        string[] unlisted = found.Except(listed).ToArray();
        string[] stale = listed.Except(found).ToArray();

        Assert.AreEqual(0, unlisted.Length,
            "a thread-static a pooled thread could carry into its next goroutine is NOT classified -- reset it (GolibReset or a GoroutineThreadState.Register call) or state why it is kept, then list it here:\n  " + string.Join("\n  ", unlisted));
        Assert.AreEqual(0, stale.Length, "a census entry names a field that no longer exists:\n  " + string.Join("\n  ", stale));

        foreach ((string key, Disposition _, string proof, string _) in Census)
        {
            string file = key[..key.IndexOf('|')];
            Assert.IsTrue(text[file].Contains(proof, StringComparison.Ordinal), $"{key}: its proof is gone from {file}: {proof}");
        }
    }

    private static string stripComment(string line)
    {
        string trimmed = line.TrimStart();

        if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("*", StringComparison.Ordinal))
            return "";

        int comment = line.IndexOf("//", StringComparison.Ordinal);
        return comment >= 0 ? line[..comment] : line;
    }

    // ---- the reuse arm ----

    [ThreadStatic]
    private static int t_registeredProbe;

    private static readonly bool s_probeRegistered = GoroutineThreadState.Register(static () => t_registeredProbe = 0);

    [TestMethod]
    public void PooledThread_CarriesNothingFromItsPreviousGoroutine()
    {
        Assert.IsTrue(s_probeRegistered);

        int threadA = 0, threadB = 0;
        long goroutineA = 0, goroutineB = 0;
        Dictionary<string, object?> seenByB = [];

        // A failure on a worker would reach golib's process-killing backstop, so each step captures its
        // own and the test rethrows it here.
        Exception? failure = null;
        int idleWhenPlanted = -1;
        using ManualResetEventSlim planted = new(false);

        // What a FINISHED goroutine leaves on its thread is planted after its goroutine scope has closed
        // and before the worker resets -- the pool's own seam. Planting it inside the live goroutine
        // would hand the panic slots to golib's goroutine-exit machinery instead (measured: that
        // version took the whole test host down when run after the rest of the suite).
        GoroutineThreadPool.AfterBodyForTest = () =>
        {
            if (Environment.CurrentManagedThreadId != Volatile.Read(ref threadA) || planted.IsSet)
                return;

            try
            {
                Dirty();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            idleWhenPlanted = GoroutineThreadPool.IdleCount;
            planted.Set();
        };

        try
        {
            OnGoroutine(() => Coro.Start(() =>
            {
                Volatile.Write(ref threadA, Environment.CurrentManagedThreadId);
                goroutineA = Goroutine.Current!.Id;
            }).Switch());

            Assert.IsTrue(planted.Wait(10_000), "the worker never reached the post-body seam");

            // The worker resets, then idles: wait for it to be back in the idle set before B asks for one.
            for (int spin = 0; spin < 400 && GoroutineThreadPool.IdleCount <= idleWhenPlanted; spin++)
                Thread.Sleep(5);
        }
        finally
        {
            GoroutineThreadPool.AfterBodyForTest = null;
        }

        if (failure is not null)
            throw new AssertFailedException("planting goroutine A's leftovers failed", failure);

        OnGoroutine(() => Coro.Start(() =>
        {
            try
            {
                threadB = Environment.CurrentManagedThreadId;
                goroutineB = Goroutine.Current!.Id;

                foreach ((string name, Func<object?> read, object? _) in Slots())
                    seenByB[name] = read();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        }).Switch());

        if (failure is not null)
            throw new AssertFailedException("goroutine B failed", failure);

        if (threadA != threadB)
            Assert.Inconclusive($"the pool did not reuse A's thread ({threadA} then {threadB}); nothing to assert");

        Assert.AreNotEqual(goroutineA, goroutineB, "B runs as a goroutine of its own, not A's identity");

        foreach ((string name, Func<object?> _, object? clean) in Slots())
            Assert.AreEqual(clean, seenByB[name], $"{name}: goroutine B saw goroutine A's value on the reused thread");
    }

    // Drives a coro from a FRESH goroutine, never from the test thread: MSTest's thread can hold the MAIN
    // goroutine's identity lent to it (MainGoroutineIdentityTests), whose runtime g is published per
    // thread, last writer wins -- so once the runtime has loaded, a coro exit's Ready of that resumer
    // checks another thread's g and panics on a worker (measured in the full suite; a pre-existing
    // main-identity hazard, routed separately, not this seat's).
    private static void OnGoroutine(Action body)
    {
        Exception? error = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(30_000), "the driving goroutine never finished");

        if (error is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }

    // Each golib-owned slot (read through reflection, since the fields are private) and the registered
    // probe: its name, how to read it on the current thread, and its clean value.
    private static IEnumerable<(string Name, Func<object?> Read, object? Clean)> Slots()
    {
        // The tally is not a zero: with counting enabled by an earlier test, B's own goroutine start
        // counts its objects on this thread before B can read it. Clean means "not A's planted value".
        yield return ("AllocationCounter.t_count", () => (long)Field(typeof(AllocationCounter), "t_count").GetValue(null)! == PlantedCount, false);
        yield return ("GoschedBackoff.t_consecutiveInertYields", () => Field(typeof(Coro).Assembly.GetType("go.golib.GoschedBackoff")!, "t_consecutiveInertYields").GetValue(null), 0);
        yield return ("SelectPending.t_frames", () => Field(typeof(Coro).Assembly.GetType("go.SelectPending")!, "t_frames").GetValue(null), null);
        yield return ("builtin.s_fallthrough", () => ((ThreadLocal<bool>)Field(typeof(builtin), "s_fallthrough").GetValue(null)!).Value, false);

        // The panic-and-defer holder (GoThreadState): the five panic slots, the sequence depth and entries.
        yield return ("GoThreadState.CapturedPanic", () => GoThreadState.Current.CapturedPanic, null);
        yield return ("GoThreadState.HandledPanic", () => GoThreadState.Current.HandledPanic, null);
        yield return ("GoThreadState.UnclaimedPanic", () => GoThreadState.Current.UnclaimedPanic, null);
        yield return ("GoThreadState.InFlightForeign", () => GoThreadState.Current.InFlightForeign, null);
        yield return ("GoThreadState.RecoverablePanic", () => GoThreadState.Current.RecoverablePanic, null);
        yield return ("GoThreadState.SequenceDepth", () => GoThreadState.Current.SequenceDepth, 0);
        yield return ("GoThreadState.Sequences (no live entry)", () => SequencesClear(), true);
        yield return ("GoexitException.t_started", () => Field(typeof(GoexitException), "t_started").GetValue(null), false);
        yield return ("GoFrame.t_splicesRefused", () => Field(typeof(GoFrame), "t_splicesRefused").GetValue(null), false);
        yield return ("PanicException.t_lastAdopted", () => Field(typeof(PanicException), "t_lastAdopted").GetValue(null), null);
        yield return ("registered probe", () => t_registeredProbe, 0);
    }

    private const long PlantedCount = 1_234_567_890L;

    private static void Dirty()
    {
        Field(typeof(AllocationCounter), "t_count").SetValue(null, PlantedCount);
        Field(typeof(Coro).Assembly.GetType("go.golib.GoschedBackoff")!, "t_consecutiveInertYields").SetValue(null, 99);
        Field(typeof(Coro).Assembly.GetType("go.SelectPending")!, "t_frames").SetValue(null, Activator.CreateInstance(Field(typeof(Coro).Assembly.GetType("go.SelectPending")!, "t_frames").FieldType));
        ((ThreadLocal<bool>)Field(typeof(builtin), "s_fallthrough").GetValue(null)!).Value = true;

        PanicException stale = new("goroutine A's panic");
        GoThreadState state = GoThreadState.Current;
        state.CapturedPanic = stale;
        state.HandledPanic = stale;
        state.UnclaimedPanic = stale;
        state.RecoverablePanic = stale;
        state.InFlightForeign = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(new InvalidOperationException("A"));

        // A stale panic-sequence entry and depth, as a goroutine that ended mid-sequence would leave them,
        // and a Goexit mark: runtime.Callers would pair them with the next goroutine's Run frames.
        GoFrame.Sequence[] sequences = new GoFrame.Sequence[16];
        sequences[0].Panic = stale;
        sequences[0].Activation = 1;
        state.Sequences = sequences;
        state.SequenceDepth = 1;
        Field(typeof(GoexitException), "t_started").SetValue(null, true);
        Field(typeof(GoFrame), "t_splicesRefused").SetValue(null, true);
        Field(typeof(PanicException), "t_lastAdopted").SetValue(null, stale);

        t_registeredProbe = 7;
    }

    // True when this thread holds no live panic-sequence entry (the array is absent, or every entry is clear).
    private static bool SequencesClear()
    {
        if (GoThreadState.Current.Sequences is not { } sequences)
            return true;

        foreach (GoFrame.Sequence entry in sequences)
        {
            if (entry.Panic is not null || entry.Activation != 0)
                return false;
        }

        return true;
    }

    private static FieldInfo Field(Type type, string name) =>
        type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ?? throw new MissingFieldException(type.FullName, name);
}
