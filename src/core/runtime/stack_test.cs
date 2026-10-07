// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using reflect = reflect_package;
using Δregexp = regexp_package;
using static runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.sync;
using static global::go.runtime_internal_test_package;
using Δio = io_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// TestStackMem measures per-thread stack segment cache behavior.
// The test consumed up to 500MB in the past.
public static partial void TestStackMem(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        UntypedInt BatchSize = 32;
        const nint BatchCount = 256;
        UntypedInt ArraySize = 1024;
        UntypedInt RecursionDepth = 128;
        if (testing.Short()) {
            return;
        }
        defer(GOMAXPROCS, GOMAXPROCS(BatchSize), ref ᒐ);
        var s0 = @new<Δruntime.MemStats>();
        ReadMemStats(s0);
        for (nint b = 0; b < BatchCount; b++) {
            var c = new channel<bool>(BatchSize);
            for (nint i = 0; i < BatchSize; i++) {
                var cʗ1 = c;
                goǃ(() => {
                    ref var f = ref heap<Action<nint, array<byte>>>(out var Ꮡf);
                    Ꮡf.ValueSlot = (nint k, [GoArrayDims(1024)] array<byte> a) => {
                        a = a.Clone();
                        if (k == 0) {
                            time.Sleep(time.Millisecond);
                            return;
                        }
                        Ꮡf.ValueSlot(k - 1, a);
                    };
                    Ꮡf.ValueSlot(RecursionDepth, new byte[]{}.array(1024));
                    cʗ1.ᐸꟷ(true);
                });
            }
            for (nint i = 0; i < BatchSize; i++) {
                ᐸꟷ(c);
            }
            // The goroutines have signaled via c that they are ready to exit.
            // Give them a chance to exit by sleeping. If we don't wait, we
            // might not reuse them on the next batch.
            time.Sleep(10 * time.Millisecond);
        }
        var s1 = @new<Δruntime.MemStats>();
        ReadMemStats(s1);
        var consumed = (int64)((~s1).StackSys - (~s0).StackSys);
        Ꮡt.Logf("Consumed %vMB for stack mem"u8, (consumed >> (int)(20)));
        var estimate = (int64)(8 * BatchSize * ArraySize * RecursionDepth); // 8 is to reduce flakiness.
        if (consumed > estimate) {
            Ꮡt.Fatalf("Stack mem: want %v, got %v"u8, estimate, consumed);
        }
        // Due to broken stack memory accounting (https://golang.org/issue/7468),
        // StackInuse can decrease during function execution, so we cast the values to int64.
        var inuse = (int64)(~s1).StackInuse - (int64)(~s0).StackInuse;
        Ꮡt.Logf("Inuse %vMB for stack mem"u8, (inuse >> (int)(20)));
        if (inuse > ((int64)4 << (int)(20))) {
            Ꮡt.Fatalf("Stack inuse: want %v, got %v"u8, (nint)((4 << (int)(20))), inuse);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object firstGrowStackTookˢ = (@string)"first growStack took"u8;

// Test stack growing in different contexts.
public static partial void TestStackGrowth(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (flagQuick.Value) {
            Ꮡt.Skip(quickˢ);
        }
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        // in a normal goroutine
        time.Duration growDuration = default!;                // For debugging failures
        Ꮡwg.Add(1);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var start = ref heap<time.Time>(out var Ꮡstart);
                start = time.Now();
                growStack(nil);
                growDuration = time.Since(start);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();
        Ꮡt.Log(firstGrowStackTookˢ, growDuration);
        // in locked goroutine
        Ꮡwg.Add(1);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                LockOSThread();
                growStack(nil);
                UnlockOSThread();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();
        // in finalizer
        ref var finalizerStart = ref heap(new time.Time(), out var ᏑfinalizerStart);
        ref var started = ref heap(new atomic.Bool(), out var Ꮡstarted);
        ref var progress = ref heap(new atomic.Uint32(), out var Ꮡprogress);
        Ꮡwg.Add(1);
        var s = @new<@string>(); // Must be of a type that avoids the tiny allocator, or else the finalizer might not run.
        SetFinalizer(s.OrTypedNil(), (ж<@string> ss) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ᏑfinalizerStart.Value = time.Now();
                Ꮡstarted.Store(true);
                growStack(Ꮡprogress);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        ref var setFinalizerTime = ref heap<time.Time>(out var ᏑsetFinalizerTime);
        setFinalizerTime = time.Now();
        s = default!;
        {
            var (d, ok) = t.Deadline(); if (ok) {
                // Pad the timeout by an arbitrary 5% to give the AfterFunc time to run.
                var timeout = time.Until(d) * 19 / 20;
                var setFinalizerTimeʗ1 = setFinalizerTime;
                var timer = time.AfterFunc(timeout, () => {
                    // Panic — instead of calling t.Error and returning from the test — so
                    // that we get a useful goroutine dump if the test times out, especially
                    // if GOTRACEBACK=system or GOTRACEBACK=crash is set.
                    if (!Ꮡstarted.Load()){
                        throw panic("finalizer did not start");
                    } else {
                        throw panic(fmt.Sprintf("finalizer started %s ago (%s after registration) and ran %d iterations, but did not return"u8, time.Since(ᏑfinalizerStart.Value), ᏑfinalizerStart.Value.Sub(setFinalizerTimeʗ1), Ꮡprogress.Load()));
                    }
                });
                var timerʗ1 = timer;
                defer(() => timerʗ1.Stop(), ref ᒐ);
            }
        }
        GC();
        Ꮡwg.Wait();
        Ꮡt.Logf("finalizer started after %s and ran %d iterations in %v"u8, finalizerStart.Sub(setFinalizerTime), Ꮡprogress.Load(), time.Since(finalizerStart));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// ... and in init
//func init() {
//	growStack()
//}
internal static void growStack(ж<atomic.Uint32> Ꮡprogress) {
    nint n = (1 << (int)(10));
    if (testing.Short()) {
        n = (1 << (int)(8));
    }
    for (nint i = 0; i < n; i++) {
        ref var x = ref heap<nint>(out var Ꮡx);
        x = 0;
        growStackIter(Ꮡx, i);
        if (x != i + 1) {
            throw panic("stack is corrupted");
        }
        if (Ꮡprogress != nil) {
            Ꮡprogress.Store((uint32)i);
        }
    }
    GC();
}

// This function is not an anonymous func, so that the compiler can do escape
// analysis and place x on stack (and subsequently stack growth update the pointer).
internal static void growStackIter(ж<nint> Ꮡp, nint n) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (n == 0) {
        p = n + 1;
        GC();
        return;
    }
    p = n + 1;
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 0;
    growStackIter(Ꮡx, n - 1);
    if (x != n) {
        throw panic("stack is corrupted");
    }
}

public static partial void TestStackGrowthCallback(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    // test stack growth at chan op
    Ꮡwg.Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var c = new channel<nint>(1);
            var cʗ1 = c;
            growStackWithCallback(() => {
                cʗ1.ᐸꟷ(1);
                ᐸꟷ(cʗ1);
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // test stack growth at map op
    Ꮡwg.Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var m = new map<nint, nint>();
            var mʗ1 = m;
            growStackWithCallback(() => {
                (_, _) = mʗ1[1, ꟷ];
                mʗ1[1] = 1;
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // test stack growth at goroutine creation
    Ꮡwg.Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            growStackWithCallback([MethodImpl(MethodImplOptions.NoInlining)] () => {
                var done = new channel<bool>(0);
                var doneʗ1 = done;
                goǃ(() => {
                    doneʗ1.ᐸꟷ(true);
                });
                ᐸꟷ(done);
            });
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
}

internal static void growStackWithCallback(Action cb) {
    ref var f = ref heap<Action<nint>>(out var Ꮡf);
    f = (nint n) => {
        if (n == 0) {
            cb();
            return;
        }
        Ꮡf.ValueSlot(n - 1);
    };
    for (nint i = 0; i < (1 << (int)(10)); i++) {
        f(i);
    }
}

// TestDeferPtrs tests the adjustment of Defer's argument pointers (p aka &y)
// during a stack copy.
internal static void set(ж<nint> Ꮡp, nint x) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = x;
}

public static void TestDeferPtrs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var y = ref heap(new nint(), out var Ꮡy);
        defer(() => {
            if (Ꮡy.Value != 42) {
                Ꮡt.Errorf("defer's stack references were not adjusted appropriately"u8);
            }
        }, ref ᒐ);
        defer(set, Ꮡy, (nint)(42), ref ᒐ);
        growStack(nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("[4096]byte")] /* [4 * 1024]byte */
partial struct bigBuf;

// TestDeferPtrsGoexit is like TestDeferPtrs but exercises the possibility that the
// stack grows as part of starting the deferred function. It calls Goexit at various
// stack depths, forcing the deferred function (with >4kB of args) to be run at
// the bottom of the stack. The goal is to find a stack depth less than 4kB from
// the end of the stack. Each trial runs in a different goroutine so that an earlier
// stack growth does not invalidate a later attempt.
public static partial void TestDeferPtrsGoexit(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 100; i++) {
        var c = new channel<nint>(1);
        goǃ(testDeferPtrsGoexit, c, i);
        {
            nint n = ᐸꟷ(c); if (n != 42) {
                Ꮡt.Fatalf("defer's stack references were not adjusted appropriately (i=%d n=%d)"u8, i, n);
            }
        }
    }
}

internal static void testDeferPtrsGoexit(channel<nint> c, nint i) {
    GoFrame ᒐ = default;
    try {
        ref var y = ref heap(new nint(), out var Ꮡy);
        var cʗ1 = c;
        defer(() => {
            cʗ1.ᐸꟷ(Ꮡy.Value);
        }, ref ᒐ);
        defer(setBig, Ꮡy, (nint)(42), new bigBuf(new byte[4096].array()), ref ᒐ);
        useStackAndCall(i, Goexit);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void setBig(ж<nint> Ꮡp, nint x, bigBuf b) {
    b = b.Clone();

    ref var p = ref Ꮡp.DerefOrNull();
    p = x;
}

// TestDeferPtrsPanic is like TestDeferPtrsGoexit, but it's using panic instead
// of Goexit to run the Defers. Those two are different execution paths
// in the runtime.
public static partial void TestDeferPtrsPanic(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 100; i++) {
        var c = new channel<nint>(1);
        goǃ(testDeferPtrsGoexit, c, i);
        {
            nint n = ᐸꟷ(c); if (n != 42) {
                Ꮡt.Fatalf("defer's stack references were not adjusted appropriately (i=%d n=%d)"u8, i, n);
            }
        }
    }
}

internal static void testDeferPtrsPanic(channel<nint> c, nint i) {
    GoFrame ᒐ = default;
    try {
        ref var y = ref heap(new nint(), out var Ꮡy);
        var cʗ1 = c;
        defer(() => {
            if (recover() == default!) {
                cʗ1.ᐸꟷ(-1);
                return;
            }
            cʗ1.ᐸꟷ(Ꮡy.Value);
        }, ref ᒐ);
        defer(setBig, Ꮡy, (nint)(42), new bigBuf(new byte[4096].array()), ref ᒐ);
        useStackAndCall(i, () => {
            throw panic((nint)(1));
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

//go:noinline
internal static partial void testDeferLeafSigpanic1() {
    // Cause a sigpanic to be injected in this frame.
    //
    // This function has to be declared before
    // TestDeferLeafSigpanic so the runtime will crash if we think
    // this function's continuation PC is in
    // TestDeferLeafSigpanic.
    (((ж<nint>)nil)).Value = 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedPanicFromNilˢ = (@string)"expected panic from nil pointer"u8;

// TestDeferLeafSigpanic tests defer matching around leaf functions
// that sigpanic. This is tricky because on LR machines the outer
// function and the inner function have the same SP, but it's critical
// that we match up the defer correctly to get the right liveness map.
// See issue #25499.
public static void TestDeferLeafSigpanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Push a defer that will walk the stack.
        defer(() => {
            {
                var err = recover(); if (err == default!) {
                    Ꮡt.Fatal(expectedPanicFromNilˢ);
                }
            }
            GC();
        }, ref ᒐ);
        // Call a leaf function. We must set up the exact call stack:
        //
        //  deferring function -> leaf function -> sigpanic
        //
        // On LR machines, the leaf function will have the same SP as
        // the SP pushed for the defer frame.
        testDeferLeafSigpanic1();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestPanicUseStack checks that a chain of Panic structs on the stack are
// updated correctly if the stack grows during the deferred execution that
// happens as a result of the panic.
public static void TestPanicUseStack(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var pc = new slice<uintptr>(10000);
        var pcʗ1 = pc;
        defer(() => {
            recover();
            Callers(0, pcʗ1); // force stack walk
            var pcʗ2 = pcʗ1;
            useStackAndCall(100, () => {
                GoFrame ᒐ = default;
                try {
                    var pcʗ3 = pcʗ2;
                    defer(() => {
                        recover();
                        Callers(0, pcʗ3); // force stack walk
                        var pcʗ4 = pcʗ3;
                        useStackAndCall(200, () => {
                            GoFrame ᒐ = default;
                            try {
                                var pcʗ5 = pcʗ4;
                                defer(() => {
                                    recover();
                                    Callers(0, pcʗ5); // force stack walk
                                }, ref ᒐ);
                                throw panic((nint)(3));
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        });
                    }, ref ᒐ);
                    throw panic((nint)(2));
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }, ref ᒐ);
        throw panic((nint)(1));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPanicFar(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var xtree = ref heap<ж<xtreeNode>>(out var Ꮡxtree);
        var pc = new slice<uintptr>(10000);
        var pcʗ1 = pc;
        defer(() => {
            // At this point we created a large stack and unwound
            // it via recovery. Force a stack walk, which will
            // check the stack's consistency.
            Callers(0, pcʗ1);
        }, ref ᒐ);
        defer(() => {
            recover();
        }, ref ᒐ);
        useStackAndCall(100, () => {
            // Kick off the GC and make it do something nontrivial.
            // (This used to force stack barriers to stick around.)
            Ꮡxtree.ValueSlot = makeTree(18);
            // Give the GC time to start scanning stacks.
            time.Sleep(time.Millisecond);
            throw panic((nint)(1));
        });
        _ = xtree;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct xtreeNode {
    internal ж<xtreeNode> l, r;
}

internal static ж<xtreeNode> makeTree(nint d) {
    if (d == 0) {
        return @new<xtreeNode>();
    }
    return Ꮡ(new xtreeNode(makeTree(d - 1), makeTree(d - 1)));
}

// use about n KB of stack and call f
internal static void useStackAndCall(nint n, Action f) {
    if (n == 0) {
        f();
        return;
    }
    array<byte> b = new(1024);                     // makes frame about 1KB
    useStackAndCall(n - 1 + (nint)b[99], f);
}

internal static void useStack(nint n) {
    useStackAndCall(n, () => {
    });
}

internal static void growing(channel<nint> c, channel<EmptyStruct> done) {
    foreach (var n in c) {
        useStack(n);
        done.ᐸꟷ(new EmptyStruct());
    }
    done.ᐸꟷ(new EmptyStruct());
}

public static partial void TestStackCache(ж<testing.T> Ꮡt) {
    // Allocate a bunch of goroutines and grow their stacks.
    // Repeat a few times to test the stack cache.
    const nint R = 4;
    
    UntypedInt G = 200;
    
    const nint S = 5;
    for (nint i = 0; i < R; i++) {
        ref var reqchans = ref heap(new array<channel<nint>>(200), out var Ꮡreqchans);
        var done = new channel<EmptyStruct>(0);
        for (nint j = 0; j < G; j++) {
            reqchans[j] = new channel<nint>(0);
            goǃ(growing, reqchans[j], done);
        }
        for (nint s = 0; s < S; s++) {
            for (nint j = 0; j < G; j++) {
                reqchans[j].ᐸꟷ(((nint)1).Lsh((nuint)s));
            }
            for (nint j = 0; j < G; j++) {
                ᐸꟷ(done);
            }
        }
        for (nint j = 0; j < G; j++) {
            close(reqchans[j]);
        }
        for (nint j = 0; j < G; j++) {
            ᐸꟷ(done);
        }
    }
}

public static partial void TestStackOutput(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(1024);
    @string stk = ((@string)(b.slice(0, Stack(b, false))));
    if (!strings.HasPrefix(stk, goroutineˢ)) {
        Ꮡt.Errorf("Stack (len %d):\n%s"u8, len(stk), stk);
        Ꮡt.Errorf("Stack output should begin with \"goroutine \""u8);
    }
}

public static partial void TestStackAllOutput(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(1024);
    @string stk = ((@string)(b.slice(0, Stack(b, true))));
    if (!strings.HasPrefix(stk, goroutineˢ)) {
        Ꮡt.Errorf("Stack (len %d):\n%s"u8, len(stk), stk);
        Ꮡt.Errorf("Stack output should begin with \"goroutine \""u8);
    }
}

public static void TestStackPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that stack copying copies panics correctly. This is difficult
        // to test because it is very unlikely that the stack will be copied
        // in the middle of gopanic. But it can happen.
        // To make this test effective, edit panic.go:gopanic and uncomment
        // the GC() call just before freedefer(d).
        defer(() => {
            {
                var x = recover(); if (x == default!) {
                    Ꮡt.Errorf("recover failed"u8);
                }
            }
        }, ref ᒐ);
        useStack(32);
        throw panic("test panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void BenchmarkStackCopyPtr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = new channel<bool>(0);
    for (nint i = 0; i < b.N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1);
            iΔ1 = 1000000;
            countp(ᏑiΔ1);
            cʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(c);
    }
}

internal static void countp(ж<nint> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (n == 0) {
        return;
    }
    n--;
    countp(Ꮡn);
}

public static partial void BenchmarkStackCopy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = new channel<bool>(0);
    for (nint i = 0; i < b.N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            count(1000000);
            cʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(c);
    }
}

internal static nint count(nint n) {
    if (n == 0) {
        return 0;
    }
    return 1 + count(n - 1);
}

public static partial void BenchmarkStackCopyNoCache(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = new channel<bool>(0);
    for (nint i = 0; i < b.N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            count1(1000000);
            cʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(c);
    }
}

internal static nint count1(nint n) {
    if (n <= 0) {
        return 0;
    }
    return 1 + count2(n - 1);
}

internal static nint count2(nint n) {
    return 1 + count3(n - 1);
}

internal static nint count3(nint n) {
    return 1 + count4(n - 1);
}

internal static nint count4(nint n) {
    return 1 + count5(n - 1);
}

internal static nint count5(nint n) {
    return 1 + count6(n - 1);
}

internal static nint count6(nint n) {
    return 1 + count7(n - 1);
}

internal static nint count7(nint n) {
    return 1 + count8(n - 1);
}

internal static nint count8(nint n) {
    return 1 + count9(n - 1);
}

internal static nint count9(nint n) {
    return 1 + count10(n - 1);
}

internal static nint count10(nint n) {
    return 1 + count11(n - 1);
}

internal static nint count11(nint n) {
    return 1 + count12(n - 1);
}

internal static nint count12(nint n) {
    return 1 + count13(n - 1);
}

internal static nint count13(nint n) {
    return 1 + count14(n - 1);
}

internal static nint count14(nint n) {
    return 1 + count15(n - 1);
}

internal static nint count15(nint n) {
    return 1 + count16(n - 1);
}

internal static nint count16(nint n) {
    return 1 + count17(n - 1);
}

internal static nint count17(nint n) {
    return 1 + count18(n - 1);
}

internal static nint count18(nint n) {
    return 1 + count19(n - 1);
}

internal static nint count19(nint n) {
    return 1 + count20(n - 1);
}

internal static nint count20(nint n) {
    return 1 + count21(n - 1);
}

internal static nint count21(nint n) {
    return 1 + count22(n - 1);
}

internal static nint count22(nint n) {
    return 1 + count23(n - 1);
}

internal static nint count23(nint n) {
    return 1 + count1(n - 1);
}

[GoType] public partial struct stkobjT {
    internal ж<stkobjT> p;
    internal int64 x;
    internal array<nint> y = new(20); // consume some stack
}

// Sum creates a linked list of stkobjTs.
public static void Sum(int64 n, ж<stkobjT> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (n == 0) {
        return;
    }
    ref var s = ref heap<stkobjT>(out var Ꮡs);
    s = new stkobjT(p: Ꮡp, x: n);
    Sum(n - 1, Ꮡs);
    p.x += s.x;
}

public static partial void BenchmarkStackCopyWithStkobj(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = new channel<bool>(0);
    for (nint i = 0; i < b.N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            ref var s = ref heap(new stkobjT(), out var Ꮡs);
            Sum(100000, Ꮡs);
            cʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(c);
    }
}

public static partial void BenchmarkIssue18138(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Channel with N "can run a goroutine" tokens
    const nint N = 10;
    var c = new channel<slice<byte>>(N);
    for (nint i = 0; i < N; i++) {
        c.ᐸꟷ(new slice<byte>(1));
    }
    for (nint i = 0; i < b.N; i++) {
        ᐸꟷ(c); // get token
        var cʗ1 = c;
        goǃ(() => {
            useStackPtrs(1000, false); // uses ~1MB max
            var m = new slice<byte>(8192); // make GC trigger occasionally
            cʗ1.ᐸꟷ(m); // return token
        });
    }
}

internal static void useStackPtrs(nint nʗp, bool b) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    if (b) {
        // This code contributes to the stack frame size, and hence to the
        // stack copying cost. But since b is always false, it costs no
        // execution time (not even the zeroing of a).
        array<ж<nint>> a = new(128);                   // 1KB of pointers
        a[n] = Ꮡn;
        n = a[0].Value;
    }
    if (n == 0) {
        return;
    }
    useStackPtrs(n - 1, b);
}

[GoType] partial struct structWithMethod {
}

internal static @string caller(this structWithMethod s) {
    var (_, @file, line, ok) = Caller(1);
    if (!ok) {
        throw panic("Caller failed");
    }
    return fmt.Sprintf("%s:%d"u8, @file, line);
}

internal static slice<uintptr> callers(this structWithMethod s) {
    var pc = new slice<uintptr>(16);
    return pc.slice(0, Callers(0, pc));
}

internal static partial @string stack(this structWithMethod s) {
    var buf = new slice<byte>((4 << (int)(10)));
    return ((@string)(buf.slice(0, Stack(buf, false))));
}

internal static void nop(this structWithMethod s) {
}

internal static void inlinablePanic(this structWithMethod s) {
    throw panic("panic");
}

public static void TestStackWrapperCaller(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var d = ref heap(new structWithMethod(), out var Ꮡd);
    // Force the compiler to construct a wrapper method.
    var wrapper = ((Func<ж<structWithMethod>, @string>)([GoWrapper("(*structWithMethod).caller")] (p0) => caller(panicwrapRecv(p0, "value method runtime_test.structWithMethod.caller called using nil *structWithMethod pointer").Value)));
    // Check that the wrapper doesn't affect the stack trace.
    {
        @string dc = d.caller();
        @string ic = wrapper(Ꮡd); if (dc != ic) {
            Ꮡt.Fatalf("direct caller %q != indirect caller %q"u8, dc, ic);
        }
    }
}

public static void TestStackWrapperCallers(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var d = ref heap(new structWithMethod(), out var Ꮡd);
    var wrapper = ((Func<ж<structWithMethod>, slice<uintptr>>)([GoWrapper("(*structWithMethod).callers")] (p0) => callers(panicwrapRecv(p0, "value method runtime_test.structWithMethod.callers called using nil *structWithMethod pointer").Value)));
    // Check that <autogenerated> doesn't appear in the stack trace.
    var pcs = wrapper(Ꮡd);
    var frames = CallersFrames(pcs);
    while (ᐧ) {
        var (fr, more) = frames.Next();
        if (fr.File == "<autogenerated>"u8) {
            Ꮡt.Fatalf("<autogenerated> appears in stack trace: %+v"u8, fr);
        }
        if (!more) {
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string autogeneratedˢ = "<autogenerated>"u8;

public static void TestStackWrapperStack(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var d = ref heap(new structWithMethod(), out var Ꮡd);
    var wrapper = ((Func<ж<structWithMethod>, @string>)([GoWrapper("(*structWithMethod).stack")] (p0) => stack(panicwrapRecv(p0, "value method runtime_test.structWithMethod.stack called using nil *structWithMethod pointer").Value)));
    // Check that <autogenerated> doesn't appear in the stack trace.
    @string stk = wrapper(Ꮡd);
    if (strings.Contains(stk, autogeneratedˢ)) {
        Ꮡt.Fatalf("<autogenerated> appears in stack trace:\n%s"u8, stk);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inlinablePanicˢ = "inlinablePanic(...)"u8;

public static void TestStackWrapperStackInlinePanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that inline unwinding correctly tracks the callee by creating a
        // stack of the form wrapper -> inlined function -> panic. If we mess up
        // callee tracking, it will look like the wrapper called panic and we'll see
        // the wrapper in the stack trace.
        ref var d = ref heap(new structWithMethod(), out var Ꮡd);
        var wrapper = ((Action<ж<structWithMethod>>)([GoWrapper("(*structWithMethod).inlinablePanic")] (p0) => inlinablePanic(panicwrapRecv(p0, "value method runtime_test.structWithMethod.inlinablePanic called using nil *structWithMethod pointer").Value)));
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var err = recover();
            if (err == default!) {
                Ꮡt.Fatalf("expected panic"u8);
            }
            var buf = new slice<byte>((4 << (int)(10)));
            @string stk = ((@string)(buf.slice(0, Stack(buf, false))));
            if (strings.Contains(stk, autogeneratedˢ)) {
                Ꮡt.Fatalf("<autogenerated> appears in stack trace:\n%s"u8, stk);
            }
            // Self-check: make sure inlinablePanic got inlined.
            if (!testenv.OptimizationOff()) {
                if (!strings.Contains(stk, inlinablePanicˢ)) {
                    Ꮡt.Fatalf("inlinablePanic not inlined"u8);
                }
            }
        }, ref ᒐ);
        wrapper(Ꮡd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial interface I {
    void M();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sigpanicˢ = "sigpanic"u8;
internal static readonly @string runtimeTestIMˢ = "runtime_test.I.M"u8;
internal static readonly @string panicwrapˢ = "panicwrap"u8;
internal static readonly @string runtimeTestˢ = "runtime_test.(*structWithMethod).nop"u8;

public static void TestStackWrapperStackPanic(ж<testing.T> Ꮡt) {
    Ꮡt.Run(sigpanicˢ, (ж<testing.T> tΔ1) => {
        // nil calls to interface methods cause a sigpanic.
        testStackWrapperPanic(tΔ1, () => {
            ((Action<I>)([GoWrapper("I.M")] (p0) => wrapperRecv(p0).M()))(default!);
        }, runtimeTestIMˢ);
    });
    Ꮡt.Run(panicwrapˢ, (ж<testing.T> tΔ2) => {
        // Nil calls to value method wrappers call panicwrap.
        var wrapper = ((Action<ж<structWithMethod>>)([GoWrapper("(*structWithMethod).nop")] (p0) => nop(panicwrapRecv(p0, "value method runtime_test.structWithMethod.nop called using nil *structWithMethod pointer").Value)));
        var wrapperʗ1 = wrapper;
        testStackWrapperPanic(tΔ2, () => {
            wrapperʗ1(nil);
        }, runtimeTestˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string callersFramesˢ = "CallersFrames"u8;
internal static readonly @string stackˢ2 = "Stack"u8;

internal static void testStackWrapperPanic(ж<testing.T> Ꮡt, Action cb, @string expect) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that the stack trace from a panicking wrapper includes
    // the wrapper, even though elide these when they don't panic.
    Ꮡt.Run(callersFramesˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (err == default!) {
                    tΔ1.Fatalf("expected panic"u8);
                }
                var pcs = new slice<uintptr>(10);
                nint n = Callers(0, pcs);
                var frames = CallersFrames(pcs.slice(0, n));
                while (ᐧ) {
                    ref var frame = ref heap<Δruntime.Frame>(out var Ꮡframe);
                    (frame, var more) = frames.Next();
                    tΔ1.Log(frame.Function);
                    if (frame.Function == expect) {
                        return;
                    }
                    if (!more) {
                        break;
                    }
                }
                tΔ1.Fatalf("panicking wrapper %s missing from stack trace"u8, expect);
            }, ref ᒐ);
            cb();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡt.Run(stackˢ2, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                var err = recover();
                if (err == default!) {
                    tΔ2.Fatalf("expected panic"u8);
                }
                var buf = new slice<byte>((4 << (int)(10)));
                @string stk = ((@string)(buf.slice(0, Stack(buf, false))));
                if (!strings.Contains(stk, "\n"u8 + expect)) {
                    tΔ2.Fatalf("panicking wrapper %s missing from stack trace:\n%s"u8, expect, stk);
                }
            }, ref ᒐ);
            cb();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static void TestCallersFromWrapper(ж<testing.T> Ꮡt) {
    // Test that invoking CallersFrames on a stack where the first
    // PC is an autogenerated wrapper keeps the wrapper in the
    // trace. Normally we elide these, assuming that the wrapper
    // calls the thing you actually wanted to see, but in this
    // case we need to keep it.
    var pc = reflect.ValueOf(((Action<I>)([GoWrapper("I.M")] (p0) => wrapperRecv(p0).M()))).Pointer();
    var frames = CallersFrames(new uintptr[]{pc}.slice());
    var (frame, more) = frames.Next();
    if (frame.Function != "runtime_test.I.M"u8) {
        Ꮡt.Fatalf("want function %s, got %s"u8, runtimeTestIMˢ, frame.Function);
    }
    if (more) {
        Ꮡt.Fatalf("want 1 frame, got > 1"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object systemstackTailCallNotˢ = (@string)"systemstack tail call not implemented on ppc64x"u8;

public static void TestTracebackSystemstack(ж<testing.T> Ꮡt) {
    if (GOARCH == "ppc64"u8 || GOARCH == "ppc64le"u8) {
        Ꮡt.Skip(systemstackTailCallNotˢ);
    }
    // Test that profiles correctly jump over systemstack,
    // including nested systemstack calls.
    var pcs = new slice<uintptr>(20);
    pcs = pcs.slice(0, runtime_internal_test_package.TracebackSystemstack(pcs, 5));
    // Check that runtime.TracebackSystemstack appears five times
    // and that we see TestTracebackSystemstack.
    nint countIn = 0;
    nint countOut = 0;
    var frames = CallersFrames(pcs);
    ref var tb = ref heap(new strings.Builder(), out var Ꮡtb);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        fmt.Fprintf(new runtime_test_package.strings_BuilderжWriter(Ꮡtb), "\n%s+0x%x %s:%d"u8, frame.Function, frame.PC - frame.Entry, frame.File, frame.Line);
        var exprᴛ1 = frame.Function;
        if (exprᴛ1 == "runtime.TracebackSystemstack"u8) {
            countIn++;
        }
        else if (exprᴛ1 == "runtime_test.TestTracebackSystemstack"u8) {
            countOut++;
        }

        if (!more) {
            break;
        }
    }
    if (countIn != 5 || countOut != 1) {
        Ꮡt.Fatalf("expected 5 calls to TracebackSystemstack and 1 call to TestTracebackSystemstack, got:%s"u8, tb.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tracebackAncestorsˢ = "TracebackAncestors"u8;
internal static readonly @string originatingFromGoroutineˢ = "originating from goroutine"u8;
internal static readonly @string mainRecurseThenCallGoˢ = "main.recurseThenCallGo(...)"u8;
internal static readonly @string mainRecurseThenCallGo0xˢ = "main.recurseThenCallGo(0x"u8;

public static void TestTracebackAncestors(ж<testing.T> Ꮡt) {
    var goroutineRegex = Δregexp.MustCompile(goroutine09ˢ);
    foreach (var (_, tracebackDepth) in new nint[]{0, 1, 5, 50}.slice()) {
        @string output = runTestProg(Ꮡt, testprogˢ, tracebackAncestorsˢ, fmt.Sprintf("GODEBUG=tracebackancestors=%d"u8, tracebackDepth));
        nint numGoroutines = 3;
        nint numFrames = 2;
        nint ancestorsExpected = numGoroutines;
        if (numGoroutines > tracebackDepth) {
            ancestorsExpected = tracebackDepth;
        }
        var matches = goroutineRegex.FindAllStringSubmatch(output, -1);
        if (len(matches) != 2) {
            Ꮡt.Fatalf("want 2 goroutines, got:\n%s"u8, output);
        }
        // Check functions in the traceback.
        var fns = new @string[]{"main.recurseThenCallGo"u8, "main.main"u8, "main.printStack"u8, "main.TracebackAncestors"u8}.slice();
        foreach (var (_, fn) in fns) {
            if (!strings.Contains(output, "\n"u8 + fn + "("u8)) {
                Ꮡt.Fatalf("expected %q function in traceback:\n%s"u8, fn, output);
            }
        }
        {
            @string want = originatingFromGoroutineˢ;
            nint count = ancestorsExpected; if (strings.Count(output, want) != count) {
                Ꮡt.Errorf("output does not contain %d instances of %q:\n%s"u8, count, want, output);
            }
        }
        {
            @string want = mainRecurseThenCallGoˢ;
            nint count = ancestorsExpected * (numFrames + 1); if (strings.Count(output, want) != count) {
                Ꮡt.Errorf("output does not contain %d instances of %q:\n%s"u8, count, want, output);
            }
        }
        {
            @string want = mainRecurseThenCallGo0xˢ;
            nint count = 1; if (strings.Count(output, want) != count) {
                Ꮡt.Errorf("output does not contain %d instances of %q:\n%s"u8, count, want, output);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string deferLivenessˢ = "DeferLiveness"u8;

// Test that defer closure is correctly scanned when the stack is scanned.
public static void TestDeferLiveness(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, deferLivenessˢ, godebugClobberfree1ˢ);
    if (output != ""u8) {
        Ꮡt.Errorf("output:\n%s\n\nwant no output"u8, output);
    }
}

public static partial void TestDeferHeapAndStack(ж<testing.T> Ꮡt) {
    nint P = 4; // processors
    nint N = 10000; //iterations
    nint D = 200; // stack depth
    if (testing.Short()) {
        P /= 2;
        N /= 10;
        D /= 10;
    }
    var c = new channel<bool>(0);
    for (nint p = 0; p < P; p++) {
        var cʗ1 = c;
        goǃ(() => {
            for (nint i = 0; i < N; i++) {
                if (deferHeapAndStack(D) != 2 * D) {
                    throw panic("bad result");
                }
            }
            cʗ1.ᐸꟷ(true);
        });
    }
    for (nint p = 0; p < P; p++) {
        ᐸꟷ(c);
    }
}

// deferHeapAndStack(n) computes 2*n
internal static nint /*r*/ deferHeapAndStack(nint n) {
    nint r = default!;
    GoFrame ᒐ = default;
    try {
        if (n == 0) {
            r = 0; goto ᒐdone;
        }
        if (n % 2 == 0){
            // heap-allocated defers
            for (nint i = 0; i < 2; i++) {
                defer(() => {
                    r++;
                }, ref ᒐ);
            }
        } else {
            // stack-allocated defers
            defer(() => {
                r++;
            }, ref ᒐ);
            defer(() => {
                r++;
            }, ref ᒐ);
        }
        r = deferHeapAndStack(n - 1);
        escapeMe(Ꮡ(new array<byte>(1024))); // force some GCs
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

// Pass a value to escapeMe to force it to escape.
internal static Action<any> escapeMe = (any x) => {
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string framePointerAdjustˢ = "FramePointerAdjust"u8;

public static void TestFramePointerAdjust(ж<testing.T> Ꮡt) {
    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
    }
    else { /* default: */
        Ꮡt.Skipf("frame pointer is not supported on %s"u8, GOARCH);
    }

    @string output = runTestProg(Ꮡt, testprogˢ, framePointerAdjustˢ);
    if (output != ""u8) {
        Ꮡt.Errorf("output:\n%s\n\nwant no output"u8, output);
    }
}

// TestSystemstackFramePointerAdjust is a regression test for issue 59692 that
// ensures that the frame pointer of systemstack is correctly adjusted. See CL
// 489015 for more details.
public static void TestSystemstackFramePointerAdjust(ж<testing.T> Ꮡt) {
    growAndShrinkStack(512, new byte[]{}.array(1024));
}

// growAndShrinkStack grows the stack of the current goroutine in order to
// shrink it again and verify that all frame pointers on the new stack have
// been correctly adjusted. stackBallast is used to ensure we're not depending
// on the current heuristics of stack shrinking too much.
internal static void growAndShrinkStack(nint n, [GoArrayDims(1024)] array<byte> stackBallast) {
    stackBallast = stackBallast.Clone();

    if (n <= 0) {
        return;
    }
    growAndShrinkStack(n - 1, stackBallast);
    runtime_internal_test_package.ShrinkStackAndVerifyFramePointers();
}

} // end runtime_test_package
