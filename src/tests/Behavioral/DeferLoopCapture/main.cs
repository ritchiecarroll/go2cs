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

internal static error Close(this ref conn c) {
    fmt.Println("close", c.name);
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

internal static void receiverReassignedNoLoop() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- control: receiver bound at defer-statement time --");
        var x = Ꮡ(new conn(name: "first"));
        var xʗ1 = x;
        defer(() => xʗ1.Close(), ref ᒐ);
        x = Ꮡ(new conn(name: "second"));
        _ = x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferIndexedReceiverInLoop() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- red 1: defer c[i].Close() --");
        ref var c = ref heap(new array<closer>(3), out var Ꮡc);
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            var i = iᴛ1;
            c[i] = new connжcloser(Ꮡ(new conn(name: fmt.Sprint("conn", i))));
            var cʗ1 = c;
            defer(() => cʗ1[i].Close(), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void goIndexedReceiverInLoop() {
    var gotᴷ = new stackAppend<@string>();

    fmt.Println("-- red 2: go c[i].Close() --");
    var @out = new channel<@string>(3);
    ref var c = ref heap(new array<closer>(3), out var Ꮡc);
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        c[i] = new asyncConnжcloser(Ꮡ(new asyncConn(name: fmt.Sprint("g", i), @out: @out)));
        var cʗ1 = c;
        goǃ(() => cʗ1[i].Close());
    }
    var got = new slice<@string>(0, 3);
    for (nint j = 0; j < 3; j++) {
        var selᴛ1 = @out;
        var selᴛ2 = time.After((time.Duration)(5000000000L));
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var s): {
            got = append(gotᴷ, got, s);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            got = append(got, "TIMEOUT"u8);
            break;
        }}
    }
    sort.Strings(got);
    foreach (var (_, s) in got) {
        fmt.Println("closed", s);
    }
}

internal static void closureCapture3Clause() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- control: closure capture (3-clause) --");
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            var i = iᴛ1;
            defer(() => {
                fmt.Println("closure", i);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void closureCaptureRange() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- control: closure capture (range) --");
        foreach (var (_, v) in new @string[]{"x", "y", "z"}.slice()) {
            defer(() => {
                fmt.Println("range", v);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object argˢ = (@string)"arg"u8;

internal static void plainArgs() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- control: plain args (3-clause) --");
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
