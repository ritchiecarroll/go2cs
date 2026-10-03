namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static channel<bool> sink = new channel<bool>(1);

[GoType] partial struct runner {
    internal nint n;
}

[MethodImpl(MethodImplOptions.NoInlining)] [GoRecv] internal static nint fork(this ref runner r, nint c, Action<nint> dispatch) {
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var doneʗ2 = doneʗ1;
            defer(() => {
                doneʗ2.ᐸꟷ(true);
            }, ref ᒐ);
            dispatch(c);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    ᐸꟷ(done);
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferSeesTheReassignedˢ = (@string)"defer sees the reassigned channel:"u8;

internal static void later(channel<bool> done) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(deferSeesTheReassignedˢ, done == default!);
        }, ref ᒐ);
        done = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object dispatchˢ = (@string)"dispatch"u8;

internal static void Main() {
    var r = Ꮡ(new runner(nil));
    fmt.Println(r.fork(5, (nint v) => {
        fmt.Println(dispatchˢ, v);
    }));
    later(sink);
}

} // end main_package
