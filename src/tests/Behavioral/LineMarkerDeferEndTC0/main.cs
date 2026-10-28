namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void where(@string tag) {
    var (_, _, line, _) = runtime.Caller(1);
    fmt.Printf("%s: %d\n"u8, tag, line);
}

internal static void fallsOffEnd() {
    GoFrame ᒐ = default;
    try {
        defer(() => where("falls off the end"), ref ᒐ);
        nint x = 1;
        _ = x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void loopDefer() {
    GoFrame ᒐ = default;
    try {
        for (nint i = 0; i < 1; i++) {
            defer(() => where("defer in a loop, falls off the end"), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint endsInReturn() {
    GoFrame ᒐ = default;
    try {
        defer(() => where("ends in return (control)"), ref ᒐ);
        nint x = 7;
        return x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static nint endsInSwitch(nint k) {
    GoFrame ᒐ = default;
    try {
        defer(() => where("ends in an all-return switch (control)"), ref ᒐ);
        switch (k) {
        case 1: {
            return 10;
        }
        default: {
            return 20;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    fallsOffEnd();
    loopDefer();
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => where("func literal, falls off the end"), ref ᒐ);
            nint y = 2;
            _ = y;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    endsInReturn();
    endsInSwitch(5);
}

} // end main_package
