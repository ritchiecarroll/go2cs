namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static nint counter;

internal static channel<bool> done = new channel<bool>(0);

internal static (nint, @string) two() {
    fmt.Println("  two() evaluated");
    return (7, "seven");
}

internal static (nint, @string, bool) three() {
    fmt.Println("  three() evaluated");
    return (3, "three", true);
}

internal static (nint, nint) next() {
    counter++;
    return (counter, counter * 10);
}

internal static nint one() {
    fmt.Println("  one() evaluated");
    return 1;
}

internal static void show(nint n, @string s) {
    fmt.Println("  show:", n, s);
}

internal static void show3(nint n, @string s, bool b) {
    fmt.Println("  show3:", n, s, b);
}

internal static void showOne(nint n) {
    fmt.Println("  showOne:", n);
}

internal static void report(nint a, nint b) {
    fmt.Println("  report:", a, b, "counter now", counter);
}

internal static void goShow(nint n, @string s) {
    fmt.Println("  goShow:", n, s);
    done.ᐸꟷ(true);
}

partial struct sink {
    internal @string tag;
}

internal static void take(this ref sink s, nint n, @string msg) {
    fmt.Println("  sink", s.tag, "took", n, msg);
}

internal static void deferSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferSpread: enter");
        defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println("deferSpread: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferThree() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferThree: enter");
        defer(ᴛ1 => show3(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), three(), ref ᒐ);
        fmt.Println("deferThree: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferCapture() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferCapture: enter");
        defer(ᴛ1 => report(ᴛ1.Item1, ᴛ1.Item2), next(), ref ᒐ);
        next();
        next();
        fmt.Println("deferCapture: counter is now", counter);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferOrder() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferOrder: enter");
        defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        defer(ᴛ1 => show3(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), three(), ref ᒐ);
        fmt.Println("deferOrder: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferMethodSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferMethodSpread: enter");
        var s = Ꮡ(new sink(tag: "A"));
        var sʗ1 = s;
        defer(ᴛ1 => sʗ1.take(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        s.Value.tag = "B";
        fmt.Println("deferMethodSpread: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferLoopSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferLoopSpread: enter");
        for (nint i = 0; i < 3; i++) {
            defer(ᴛ1 => report(ᴛ1.Item1, ᴛ1.Item2), next(), ref ᒐ);
        }
        fmt.Println("deferLoopSpread: body, counter", counter);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string elevenˢ = "eleven"u8;

internal static void deferControlPlain() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferControlPlain: enter");
        defer(show, (nint)11, elevenˢ, ref ᒐ);
        fmt.Println("deferControlPlain: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferControlSingleValueCall() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferControlSingleValueCall: enter");
        defer(showOne, one(), ref ᒐ);
        fmt.Println("deferControlSingleValueCall: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void goSpread() {
    fmt.Println("goSpread: enter");
    goǃ(ᴛ1 => goShow(ᴛ1.Item1, ᴛ1.Item2), two());
    ᐸꟷ(done);
    fmt.Println("goSpread: done");
}

internal static partial void goCaptureSpread() {
    fmt.Println("goCaptureSpread: enter");
    goǃ(ᴛ1 => goShow(ᴛ1.Item1, ᴛ1.Item2), pair());
    ᐸꟷ(done);
    fmt.Println("goCaptureSpread: done");
}

internal static (nint, @string) pair() {
    counter++;
    return (counter, "captured");
}

internal static void Main() {
    deferSpread();
    deferThree();
    deferCapture();
    deferOrder();
    deferMethodSpread();
    deferLoopSpread();
    deferControlPlain();
    deferControlSingleValueCall();
    goSpread();
    goCaptureSpread();
    deferFuncLitSpread();
    deferVariadicSpread();
    deferResultReturningSpread();
    fmt.Println("final counter", counter, "regs", regs);
}

internal static void showAll(params ꓸꓸꓸany partsʗp) {
    var parts = partsʗp.slice();

    fmt.Println("  showAll:", parts);
}

internal static array<nint> regs = new nint[]{1, 2, 3}.array();

internal static (nint, nint, nint) setRegs(nint a, nint b, nint c) {
    var old = regs.Clone();
    regs = new nint[]{a, b, c}.array();
    fmt.Println("  setRegs: now", regs, "was", old);
    return (old[0], old[1], old[2]);
}

internal static void deferResultReturningSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferResultReturningSpread: enter");
        defer(ᴛ1 => setRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), setRegs(7, 8, 9), ref ᒐ);
        fmt.Println("deferResultReturningSpread: body, regs", regs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferFuncLitSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferFuncLitSpread: enter");
        defer(ᴛ1 => ((Action<nint, @string>)((nint n, @string s) => {
            fmt.Println("  lit:", n, s);
        }))(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println("deferFuncLitSpread: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferVariadicSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("deferVariadicSpread: enter");
        defer(ᴛ1 => showAll(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println("deferVariadicSpread: body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
