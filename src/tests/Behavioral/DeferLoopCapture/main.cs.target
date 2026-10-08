namespace go;

using fmt = fmt_package;
using sort = sort_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial interface closer {
    error Close();
}

partial struct conn {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closeˢ = (@string)"close"u8;

internal static error Close(this ref conn c) {
    fmt.Println(closeˢ, c.name);
    return default!;
}

partial struct asyncConn {
    internal @string name;
    internal channel<@string> @out;
}

internal static error Close(this ref asyncConn a) {
    a.@out.ᐸꟷ(a.name);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlReceiverBoundAtˢ = (@string)"-- control: receiver bound at defer-statement time --"u8;

internal static void receiverReassignedNoLoop() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(controlReceiverBoundAtˢ);
        var x = Ꮡ(new conn(name: "first"u8));
        var xʗ1 = x;
        defer(() => xʗ1.Close(), ref ᒐ);
        x = Ꮡ(new conn(name: "second"u8));
        _ = x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object red1DeferCICloseˢ = (@string)"-- red 1: defer c[i].Close() --"u8;
private static readonly object connˢ = (@string)"conn"u8;

internal static void deferIndexedReceiverInLoop() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(red1DeferCICloseˢ);
        ref var c = ref heap(new array<closer>(3), out var Ꮡc);
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            var i = iᴛ1;
            c[i] = new connжcloser(Ꮡ(new conn(name: fmt.Sprint(connˢ, i))));
            var cʗ1 = c;
            defer(() => cʗ1[i].Close(), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object red2GoCICloseˢ = (@string)"-- red 2: go c[i].Close() --"u8;
private static readonly object closedˢ = (@string)"closed"u8;

internal static partial void goIndexedReceiverInLoop() {
    fmt.Println(red2GoCICloseˢ);
    var @out = new channel<@string>(3);
    ref var c = ref heap(new array<closer>(3), out var Ꮡc);
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        c[i] = new asyncConnжcloser(Ꮡ(new asyncConn(name: fmt.Sprint((@string)"g"u8, i), @out: @out)));
        var cʗ1 = c;
        goǃ(() => cʗ1[i].Close());
    }
    var got = new slice<@string>(0, 3);
    for (nint j = 0; j < 3; j++) {
        var selᴛ1 = @out;
        var selᴛ2 = time.After((time.Duration)(5000000000L));
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var s): {
            got = append(got, s);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            got = append(got, "TIMEOUT"u8);
            break;
        }}
    }
    sort.Strings(got);
    foreach (var (_, s) in got) {
        fmt.Println(closedˢ, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlClosureCapture3ˢ = (@string)"-- control: closure capture (3-clause) --"u8;
private static readonly object closureˢ = (@string)"closure"u8;

internal static void closureCapture3Clause() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(controlClosureCapture3ˢ);
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            var i = iᴛ1;
            defer(() => {
                fmt.Println(closureˢ, i);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlClosureCaptureˢ = (@string)"-- control: closure capture (range) --"u8;
private static readonly object rangeˢ = (@string)"range"u8;

internal static void closureCaptureRange() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(controlClosureCaptureˢ);
        foreach (var (_, v) in new @string[]{"x"u8, "y"u8, "z"u8}.slice()) {
            defer(() => {
                fmt.Println(rangeˢ, v);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object controlPlainArgs3Clauseˢ = (@string)"-- control: plain args (3-clause) --"u8;
private static readonly object argˢ = (@string)"arg"u8;

internal static void plainArgs() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(controlPlainArgs3Clauseˢ);
        for (nint i = 0; i < 3; i++) {
            defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), argˢ, i, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    receiverReassignedNoLoop();
    goIndexedReceiverInLoop();
    deferIndexedReceiverInLoop();
    closureCapture3Clause();
    closureCaptureRange();
    plainArgs();
}

} // end main_package
