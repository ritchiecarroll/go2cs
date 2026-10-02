namespace go;

using fmt = fmt_package;
using iter = iter_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static iter.Seq<nint> count(nint n) {
    return (Func<nint, bool> yield) => {
        for (nint i = 0; i < n; i++) {
            if (!yield(i)) {
                return;
            }
        }
    };
}

internal static iter.Seq2<nint, int64> squares(nint n) {
    return (Func<nint, int64, bool> yield) => {
        for (nint i = 0; i < n; i++) {
            if (!yield(i, (int64)i * (int64)i)) {
                return;
            }
        }
    };
}

internal static void @catch(@string label, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%s panicked: %v\n"u8, label, r);
                    return;
                }
            }
            fmt.Printf("%s did not panic\n"u8, label);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static bool goexits(Action f) {
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var clean = false;
            var doneʗ2 = doneʗ1;
            defer(() => {
                doneʗ2.ᐸꟷ(recover() == default! && !clean);
            }, ref ᒐ);
            f();
            clean = true;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return ᐸꟷ(done);
}

internal static iter.Seq<nint> panicSeq() {
    return (Func<nint, bool> yield) => {
        throw panic("boom");
    };
}

internal static iter.Seq<nint> panicCleanupSeq() {
    return (Func<nint, bool> yield) => {
        while (ᐧ) {
            if (!yield(55)) {
                throw panic("cleanup boom");
            }
        }
    };
}

internal static iter.Seq<nint> goexitSeq() {
    return (Func<nint, bool> yield) => {
        runtime.Goexit();
    };
}

internal static Func<(nint, bool)> nextSlot;

internal static iter.Seq<nint> doubleNext() {
    return (Func<nint, bool> _) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        fmt.Printf("double next panicked: %v\n"u8, r);
                    }
                }
            }, ref ᒐ);
            nextSlot();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pullˢ = (@string)"pull:"u8;
private static readonly object pullAfterExhaustionˢ = (@string)"pull after exhaustion:"u8;
private static readonly object stopAfterExhaustionIsANoˢ = (@string)"stop after exhaustion is a no-op"u8;
private static readonly object pull2ˢ = (@string)"pull2:"u8;
private static readonly object earlyPullˢ = (@string)"early pull:"u8;
private static readonly object pullAfterEarlyStopˢ = (@string)"pull after early stop:"u8;
private static readonly @string firstNextˢ = "first next"u8;
private static readonly object pullAfterPanicˢ = (@string)"pull after panic:"u8;
private static readonly @string stopAfterPanicˢ = "stop after panic"u8;
private static readonly object cleanupPullˢ = (@string)"cleanup pull:"u8;
private static readonly @string stopIntoCleanupˢ = "stop into cleanup"u8;
private static readonly object goexitCrossedˢ = (@string)"goexit crossed:"u8;
private static readonly object pullAfterGoexitˢ = (@string)"pull after goexit:"u8;
private static readonly object pullAfterImmediateStopˢ = (@string)"pull after immediate stop:"u8;

internal static void Main() {
    var (next, stop) = iter.Pull(count(3));
    while (ᐧ) {
        var (vΔ1, okΔ1) = next();
        fmt.Println(pullˢ, vΔ1, okΔ1);
        if (!okΔ1) {
            break;
        }
    }
    var (v, ok) = next();
    fmt.Println(pullAfterExhaustionˢ, v, ok);
    stop();
    stop();
    fmt.Println(stopAfterExhaustionIsANoˢ);
    var (next2, stop2) = iter.Pull2(squares(3));
    while (ᐧ) {
        var (k, v2, ok2) = next2();
        fmt.Println(pull2ˢ, k, v2, ok2);
        if (!ok2) {
            break;
        }
    }
    stop2();
    var (next3, stop3) = iter.Pull(count(100));
    var (a, aok) = next3();
    var (b, bok) = next3();
    fmt.Println(earlyPullˢ, a, aok, b, bok);
    stop3();
    var (c, cok) = next3();
    fmt.Println(pullAfterEarlyStopˢ, c, cok);
    var (nextP, stopP) = iter.Pull(panicSeq());
    var nextPʗ1 = nextP;
    @catch(firstNextˢ, () => {
        nextPʗ1();
    });
    var (pv, pok) = nextP();
    fmt.Println(pullAfterPanicˢ, pv, pok);
    var stopPʗ1 = stopP;
    @catch(stopAfterPanicˢ, () => {
        stopPʗ1();
    });
    var (nextC, stopC) = iter.Pull(panicCleanupSeq());
    var (cv, cvok) = nextC();
    fmt.Println(cleanupPullˢ, cv, cvok);
    var stopCʗ1 = stopC;
    @catch(stopIntoCleanupˢ, () => {
        stopCʗ1();
    });
    var (nextD, _) = iter.Pull(doubleNext());
    nextSlot = nextD;
    nextD();
    ref var goexitNext = ref heap<Func<(nint, bool)>>(out var ᏑgoexitNext);
    fmt.Println(goexitCrossedˢ, goexits(() => {
        var (n, _) = iter.Pull(goexitSeq());
        ᏑgoexitNext.ValueSlot = n;
        n();
    }));
    var (gv, gok) = goexitNext();
    fmt.Println(pullAfterGoexitˢ, gv, gok);
    var (nextI, stopI) = iter.Pull(panicSeq());
    stopI();
    var (iv, iok) = nextI();
    fmt.Println(pullAfterImmediateStopˢ, iv, iok);
}

} // end main_package
