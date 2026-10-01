// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO(austin): All of these tests are skipped if the debuglog build
// tag isn't provided. That means we basically never test debuglog.
// There are two potential ways around this:
//
// 1. Make these tests re-build the runtime test with the debuglog
// build tag and re-invoke themselves.
//
// 2. Always build the whole debuglog infrastructure and depend on
// linker dead-code elimination to drop it. This is easy for dlog()
// since there won't be any calls to it. For printDebugLog, we can
// make panic call a wrapper that is call printDebugLog if the
// debuglog build tag is set, or otherwise do nothing. Then tests
// could call printDebugLog directly. This is the right answer in
// principle, but currently our linker reads in all symbols
// regardless, so this would slow down and bloat all links. If the
// linker gets more efficient about this, we should revisit this
// approach.
namespace go;

using fmt = fmt_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using System.Runtime.CompilerServices;
using static global::go.runtime_internal_test_package;
using Δio = io_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object debugLogTestsDisabledToˢ = (@string)"debug log tests disabled to avoid collisions with real debug logs"u8;

internal static void skipDebugLog(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.DlogEnabled) {
        Ꮡt.Skip(debugLogTestsDisabledToˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mBeginLogDNˢ = @"(?m)^>> begin log \d+ <<\n"u8;

internal static @string dlogCanonicalize(@string x) {
    var begin = Δregexp.MustCompile(mBeginLogDNˢ);
    x = begin.ReplaceAllString(x, ""u8);
    var prefix = Δregexp.MustCompile(@"(?m)^\[[^]]+\]"u8);
    x = prefix.ReplaceAllString(x, "[]"u8);
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testingˢ = "testing"u8;
internal static readonly @string testingˢ2 = "[] testing\n"u8;

public static void TestDebugLog(ж<testing.T> Ꮡt) {
    skipDebugLog(Ꮡt);
    runtime_internal_test_package.ResetDebugLog();
    runtime_internal_test_package.Dlog().S(testingˢ).End();
    @string got = dlogCanonicalize(runtime_internal_test_package.DumpDebugLog());
    {
        @string want = testingˢ2; if (got != want) {
            Ꮡt.Fatalf("want %q, got %q"u8, want, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string constStringˢ = "const string"u8;
internal static readonly @string trueFalse4232767ˢ = "[] true false -42 32767 18446744073709551615 0xfff 0x0 aaaa const string\n"u8;

public static void TestDebugLogTypes(ж<testing.T> Ꮡt) {
    skipDebugLog(Ꮡt);
    runtime_internal_test_package.ResetDebugLog();
    @string varString = strings.Repeat("a"u8, 4);
    runtime_internal_test_package.Dlog().B(true).B(false).I(-42).I16(0x7fff).U64(~(uint64)0).Hex(0xfff).P(default!).S(varString).S(constStringˢ).End();
    @string got = dlogCanonicalize(runtime_internal_test_package.DumpDebugLog());
    {
        @string want = trueFalse4232767ˢ; if (got != want) {
            Ꮡt.Fatalf("want %q, got %q"u8, want, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fRuntimeTestˢ = @"\[\] 0x[0-9a-f]+ \[runtime_test\.TestDebugLogSym\+0x[0-9a-f]+ .*/debuglog_test\.go:[0-9]+\]\n"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDebugLogSym(ж<testing.T> Ꮡt) {
    skipDebugLog(Ꮡt);
    runtime_internal_test_package.ResetDebugLog();
    var (pc, _, _, _) = Δruntime.Caller(0);
    runtime_internal_test_package.Dlog().PC(pc).End();
    @string got = dlogCanonicalize(runtime_internal_test_package.DumpDebugLog());
    var want = Δregexp.MustCompile(fRuntimeTestˢ);
    if (!want.MatchString(got)) {
        Ꮡt.Fatalf("want matching %s, got %q"u8, want.OrTypedNil(), got);
    }
}

public static void TestDebugLogInterleaving(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipDebugLog(Ꮡt);
    runtime_internal_test_package.ResetDebugLog();
    nint n1 = runtime_internal_test_package.CountDebugLog();
    Ꮡt.Logf("number of log shards at start: %d"u8, n1);
    const nint limit = 1000;
    const nint concurrency = 10;
    // Start several goroutines writing to the log simultaneously.
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    nint i = 0;
    var chans = new slice<channel<bool>>(concurrency);
    foreach (var gid in range(concurrency)) {
        chans[gid] = new channel<bool>(0);
        Ꮡwg.Add(1);
        var chansʗ1 = chans;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ж<Dlogger> log = default!;
                while (ᐧ) {
                    ᐸꟷ(chansʗ1[gid]);
                    if (log != nil) {
                        log.End();
                    }
                    var next = chansʗ1[(gid + 1) % len(chansʗ1)];
                    if (i >= limit) {
                        close(next);
                        break;
                    }
                    // Log an entry, but *don't* release the log shard until its our
                    // turn again. This should result in at least n=concurrency log
                    // shards.
                    log = runtime_internal_test_package.Dlog().I(i);
                    i++;
                    // Wake up the next logger goroutine.
                    next.ᐸꟷ(true);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    // Start the chain reaction.
    chans[0].ᐸꟷ(true);
    // Wait for them to finish and get the log.
    Ꮡwg.Wait();
    @string gotFull = runtime_internal_test_package.DumpDebugLog();
    @string got = dlogCanonicalize(gotFull);
    nint n2 = runtime_internal_test_package.CountDebugLog();
    Ꮡt.Logf("number of log shards at end: %d"u8, n2);
    if (n2 < concurrency) {
        Ꮡt.Errorf("created %d log shards, expected >= %d"u8, n2, (nint)(concurrency));
    }
    // Construct the desired output.
    ref var want = ref heap(new strings.Builder(), out var Ꮡwant);
    for (nint iΔ1 = 0; iΔ1 < limit; iΔ1++) {
        fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡwant), "[] %d\n"u8, iΔ1);
    }
    if (got != want.String()) {
        // Since the timestamps are useful in understand
        // failures of this test, we print the uncanonicalized
        // output.
        Ꮡt.Fatalf("want %q, got (uncanonicalized) %q"u8, want.String(), gotFull);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginLogDLostFirstDKbNˢ = @"^>> begin log \d+; lost first \d+KB <<\n"u8;

public static void TestDebugLogWraparound(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        skipDebugLog(Ꮡt);
        // Make sure we don't switch logs so it's easier to fill one up.
        Δruntime.LockOSThread();
        defer(Δruntime.UnlockOSThread, ref ᒐ);
        runtime_internal_test_package.ResetDebugLog();
        @string longString = strings.Repeat("a"u8, 128);
        ref var want = ref heap(new strings.Builder(), out var Ꮡwant);
        for ((nint i, nint j) = (0, 0); j < (nint)(2 * runtime_internal_test_package.DebugLogBytes); (i, j) = (i + 1, j + len(longString))) {
            runtime_internal_test_package.Dlog().I(i).S(longString).End();
            fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡwant), "[] %d %s\n"u8, i, longString);
        }
        @string log = runtime_internal_test_package.DumpDebugLog();
        // Check for "lost" message.
        var lost = Δregexp.MustCompile(beginLogDLostFirstDKbNˢ);
        if (!lost.MatchString(log)) {
            Ꮡt.Fatalf("want matching %s, got %q"u8, lost.OrTypedNil(), log);
        }
        var idx = lost.FindStringIndex(log);
        // Strip lost message.
        log = dlogCanonicalize(log.slice(idx[1]));
        // Check log.
        if (!strings.HasSuffix(want.String(), log)) {
            Ꮡt.Fatalf("wrong suffix:\n%s"u8, log);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDebugLogLongString(ж<testing.T> Ꮡt) {
    skipDebugLog(Ꮡt);
    runtime_internal_test_package.ResetDebugLog();
    @string longString = strings.Repeat("a"u8, runtime_internal_test_package.DebugLogStringLimit + 1);
    runtime_internal_test_package.Dlog().S(longString).End();
    @string got = dlogCanonicalize(runtime_internal_test_package.DumpDebugLog());
    @string want = "[] "u8 + strings.Repeat("a"u8, runtime_internal_test_package.DebugLogStringLimit) + " ..(1 more bytes)..\n"u8;
    if (got != want) {
        Ꮡt.Fatalf("want %q, got %q"u8, want, got);
    }
}

} // end runtime_test_package
