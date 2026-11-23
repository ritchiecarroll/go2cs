namespace go;

using fmt = fmt_package;
using iter = iter_package;
using runtime = runtime_package;

partial class main_package {

internal static iter.Seq<nint> count(nint n) {
    return [GoFunc("count.func1")] (Func<nint, bool> yield) => {
        for (nint i = 0; i < n; i++) {
            if (!yield(i)) {
                return;
            }
        }
    };
}

internal static iter.Seq2<nint, int64> squares(nint n) {
    return [GoFunc("squares.func1")] (Func<nint, int64, bool> yield) => {
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
        defer([GoFunc("catch.func1")] () => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s panicked: %v\n"u8, label, r);
                return;
            }
            fmt.Printf("%s did not panic\n"u8, label);
        }, ref ᒐ);

        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial bool goexits(Action f) {
    var done = new channel<bool>(0);

    var doneʗ1 = done;
    goǃ([GoFunc("goexits.func1")] () => {
        GoFrame ᒐ = default;
        try {
            var clean = false;
            var doneʗ2 = doneʗ1;
            defer([GoFunc("goexits.func1.1")] () => {
                doneʗ2.ᐸꟷ(recover() == nil && !clean);
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
    return [GoFunc("panicSeq.func1")] (Func<nint, bool> yield) => {
        throw panic("boom");
    };
}

internal static iter.Seq<nint> panicCleanupSeq() {
    return [GoFunc("panicCleanupSeq.func1")] (Func<nint, bool> yield) => {
        for (;;) {
            if (!yield(55)) {
                throw panic("cleanup boom");
            }
        }
    };
}

internal static iter.Seq<nint> goexitSeq() {
    return [GoFunc("goexitSeq.func1")] (Func<nint, bool> yield) => {
        runtime.Goexit();
    };
}

internal static Func<(nint, bool)> nextSlot;

internal static iter.Seq<nint> doubleNext() {
    return [GoFunc("doubleNext.func1")] (Func<nint, bool> _) => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("doubleNext.func1.1")] () => {
                var r = recover(); if (r != nil) {
                    fmt.Printf("double next panicked: %v\n"u8, r);
                }
            }, ref ᒐ);

            nextSlot();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

internal static void main() {
    var (next, stop) = iter.Pull(count(3));

    for (;;) {
        var (vΔ1, okΔ1) = next();
        fmt.Println("pull:", vΔ1, okΔ1);

        if (!okΔ1) {
            break;
        }
    }

    var (v, ok) = next();
    fmt.Println("pull after exhaustion:", v, ok);

    stop();
    stop();
    fmt.Println("stop after exhaustion is a no-op");

    var (next2, stop2) = iter.Pull2(squares(3));

    for (;;) {
        var (k, v2, ok2) = next2();
        fmt.Println("pull2:", k, v2, ok2);

        if (!ok2) {
            break;
        }
    }

    stop2();

    var (next3, stop3) = iter.Pull(count(100));
    var (a, aok) = next3();
    var (b, bok) = next3();
    fmt.Println("early pull:", a, aok, b, bok);

    stop3();

    var (c, cok) = next3();
    fmt.Println("pull after early stop:", c, cok);

    var (nextP, stopP) = iter.Pull(panicSeq());
    var nextPʗ1 = nextP;
    @catch("first next", [GoFunc("main.func1")] () => {
        nextPʗ1();
    });

    var (pv, pok) = nextP();
    fmt.Println("pull after panic:", pv, pok);

    var stopPʗ1 = stopP;
    @catch("stop after panic", [GoFunc("main.func2")] () => {
        stopPʗ1();
    });

    var (nextC, stopC) = iter.Pull(panicCleanupSeq());
    var (cv, cvok) = nextC();
    fmt.Println("cleanup pull:", cv, cvok);

    var stopCʗ1 = stopC;
    @catch("stop into cleanup", [GoFunc("main.func3")] () => {
        stopCʗ1();
    });

    var (nextD, _) = iter.Pull(doubleNext());
    nextSlot = nextD;
    nextD();

    ref var goexitNext = ref heap<Func<(nint, bool)>>(out var ᏑgoexitNext);

    fmt.Println("goexit crossed:", goexits([GoFunc("main.func4")] () => {
        var (n, _) = iter.Pull(goexitSeq());
        ᏑgoexitNext.ValueSlot = n;
        n();
    }));

    var (gv, gok) = goexitNext();
    fmt.Println("pull after goexit:", gv, gok);

    var (nextI, stopI) = iter.Pull(panicSeq());
    stopI();

    var (iv, iok) = nextI();
    fmt.Println("pull after immediate stop:", iv, iok);
}

} // end main_package
