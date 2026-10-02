namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static nint counter;

internal static channel<bool> done = new channel<bool>(0);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object twoEvaluatedˢ = (@string)"  two() evaluated"u8;
private static readonly @string sevenˢ = "seven"u8;

internal static (nint, @string) two() {
    fmt.Println(twoEvaluatedˢ);
    return (7, sevenˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object threeEvaluatedˢ = (@string)"  three() evaluated"u8;
private static readonly @string threeˢ = "three"u8;

internal static (nint, @string, bool) three() {
    fmt.Println(threeEvaluatedˢ);
    return (3, threeˢ, true);
}

internal static (nint, nint) next() {
    counter++;
    return (counter, counter * 10);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneEvaluatedˢ = (@string)"  one() evaluated"u8;

internal static nint one() {
    fmt.Println(oneEvaluatedˢ);
    return 1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object showˢ = (@string)"  show:"u8;

internal static void show(nint n, @string s) {
    fmt.Println(showˢ, n, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object show3ˢ = (@string)"  show3:"u8;

internal static void show3(nint n, @string s, bool b) {
    fmt.Println(show3ˢ, n, s, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object showOneˢ = (@string)"  showOne:"u8;

internal static void showOne(nint n) {
    fmt.Println(showOneˢ, n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object reportˢ = (@string)"  report:"u8;
private static readonly object counterNowˢ = (@string)"counter now"u8;

internal static void report(nint a, nint b) {
    fmt.Println(reportˢ, a, b, counterNowˢ, counter);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goShowˢ = (@string)"  goShow:"u8;

internal static void goShow(nint n, @string s) {
    fmt.Println(goShowˢ, n, s);
    done.ᐸꟷ(true);
}

[GoType] partial struct sink {
    internal @string tag;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sinkˢ = (@string)"  sink"u8;
private static readonly object tookˢ = (@string)"took"u8;

[GoRecv] internal static void take(this ref sink s, nint n, @string msg) {
    fmt.Println(sinkˢ, s.tag, tookˢ, n, msg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferSpreadEnterˢ = (@string)"deferSpread: enter"u8;
private static readonly object deferSpreadBodyˢ = (@string)"deferSpread: body"u8;

internal static void deferSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferSpreadEnterˢ);
        defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println(deferSpreadBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferThreeEnterˢ = (@string)"deferThree: enter"u8;
private static readonly object deferThreeBodyˢ = (@string)"deferThree: body"u8;

internal static void deferThree() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferThreeEnterˢ);
        defer(ᴛ1 => show3(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), three(), ref ᒐ);
        fmt.Println(deferThreeBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferCaptureEnterˢ = (@string)"deferCapture: enter"u8;
private static readonly object deferCaptureCounterIsNowˢ = (@string)"deferCapture: counter is now"u8;

internal static void deferCapture() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferCaptureEnterˢ);
        defer(ᴛ1 => report(ᴛ1.Item1, ᴛ1.Item2), next(), ref ᒐ);
        next();
        next();
        fmt.Println(deferCaptureCounterIsNowˢ, counter);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferOrderEnterˢ = (@string)"deferOrder: enter"u8;
private static readonly object deferOrderBodyˢ = (@string)"deferOrder: body"u8;

internal static void deferOrder() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferOrderEnterˢ);
        defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        defer(ᴛ1 => show3(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), three(), ref ᒐ);
        fmt.Println(deferOrderBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferMethodSpreadEnterˢ = (@string)"deferMethodSpread: enter"u8;
private static readonly object deferMethodSpreadBodyˢ = (@string)"deferMethodSpread: body"u8;

internal static void deferMethodSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferMethodSpreadEnterˢ);
        var s = Ꮡ(new sink(tag: "A"u8));
        var sʗ1 = s;
        defer(ᴛ1 => sʗ1.take(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        s.Value.tag = "B"u8;
        fmt.Println(deferMethodSpreadBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferLoopSpreadEnterˢ = (@string)"deferLoopSpread: enter"u8;
private static readonly object deferLoopSpreadBodyˢ = (@string)"deferLoopSpread: body, counter"u8;

internal static void deferLoopSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferLoopSpreadEnterˢ);
        for (nint i = 0; i < 3; i++) {
            defer(ᴛ1 => report(ᴛ1.Item1, ᴛ1.Item2), next(), ref ᒐ);
        }
        fmt.Println(deferLoopSpreadBodyˢ, counter);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferControlPlainEnterˢ = (@string)"deferControlPlain: enter"u8;
private static readonly @string elevenˢ = "eleven"u8;
private static readonly object deferControlPlainBodyˢ = (@string)"deferControlPlain: body"u8;

internal static void deferControlPlain() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferControlPlainEnterˢ);
        defer(show, (nint)(11), elevenˢ, ref ᒐ);
        fmt.Println(deferControlPlainBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferControlSingleValueCall() {
    GoFrame ᒐ = default;
    try {
        fmt.Println((@string)"deferControlSingleValueCall: enter"u8);
        defer(showOne, one(), ref ᒐ);
        fmt.Println((@string)"deferControlSingleValueCall: body"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goSpreadEnterˢ = (@string)"goSpread: enter"u8;
private static readonly object goSpreadDoneˢ = (@string)"goSpread: done"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void goSpread() {
    fmt.Println(goSpreadEnterˢ);
    goǃ(ᴛ1 => goShow(ᴛ1.Item1, ᴛ1.Item2), two());
    ᐸꟷ(done);
    fmt.Println(goSpreadDoneˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goCaptureSpreadEnterˢ = (@string)"goCaptureSpread: enter"u8;
private static readonly object goCaptureSpreadDoneˢ = (@string)"goCaptureSpread: done"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void goCaptureSpread() {
    fmt.Println(goCaptureSpreadEnterˢ);
    goǃ(ᴛ1 => goShow(ᴛ1.Item1, ᴛ1.Item2), pair());
    ᐸꟷ(done);
    fmt.Println(goCaptureSpreadDoneˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string capturedˢ = "captured"u8;

internal static (nint, @string) pair() {
    counter++;
    return (counter, capturedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object finalCounterˢ = (@string)"final counter"u8;
private static readonly object regsˢ = (@string)"regs"u8;

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
    fmt.Println(finalCounterˢ, counter, regsˢ, regs);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object showAllˢ = (@string)"  showAll:"u8;

internal static void showAll(params ꓸꓸꓸany partsʗp) {
    var parts = partsʗp.slice();

    fmt.Println(showAllˢ, parts);
}

internal static array<nint> regs = new nint[]{1, 2, 3}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object setRegsNowˢ = (@string)"  setRegs: now"u8;
private static readonly object wasˢ = (@string)"was"u8;

internal static (nint, nint, nint) setRegs(nint a, nint b, nint c) {
    var old = regs.Clone();
    regs = new nint[]{a, b, c}.array();
    fmt.Println(setRegsNowˢ, regs, wasˢ, old);
    return (old[0], old[1], old[2]);
}

internal static void deferResultReturningSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println((@string)"deferResultReturningSpread: enter"u8);
        defer(ᴛ1 => setRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), setRegs(7, 8, 9), ref ᒐ);
        fmt.Println((@string)"deferResultReturningSpread: body, regs"u8, regs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferFuncLitSpreadEnterˢ = (@string)"deferFuncLitSpread: enter"u8;
private static readonly object litˢ = (@string)"  lit:"u8;
private static readonly object deferFuncLitSpreadBodyˢ = (@string)"deferFuncLitSpread: body"u8;

internal static void deferFuncLitSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferFuncLitSpreadEnterˢ);
        defer(ᴛ1 => ((Action<nint, @string>)((nint n, @string s) => {
            fmt.Println(litˢ, n, s);
        }))(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println(deferFuncLitSpreadBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferVariadicSpreadEnterˢ = (@string)"deferVariadicSpread: enter"u8;
private static readonly object deferVariadicSpreadBodyˢ = (@string)"deferVariadicSpread: body"u8;

internal static void deferVariadicSpread() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(deferVariadicSpreadEnterˢ);
        defer(ᴛ1 => showAll(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
        fmt.Println(deferVariadicSpreadBodyˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
