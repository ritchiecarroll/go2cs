namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static void where(@string tag) {
    var (_, _, line, _) = runtime.Caller(1);
    fmt.Printf("%s: %d\n"u8, tag, line);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fallsOffTheEndˢ = "falls off the end"u8;

internal static void fallsOffEnd() {
    GoFrame ᒐ = default;
    try {
        defer(where, fallsOffTheEndˢ, ref ᒐ);
        nint x = 1;
        _ = x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deferInALoopFallsOffTheˢ = "defer in a loop, falls off the end"u8;

internal static void loopDefer() {
    GoFrame ᒐ = default;
    try {
        for (nint i = 0; i < 1; i++) {
            defer(where, deferInALoopFallsOffTheˢ, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string endsInReturnControlˢ = "ends in return (control)"u8;

internal static nint endsInReturn() {
    GoFrame ᒐ = default;
    try {
        defer(where, endsInReturnControlˢ, ref ᒐ);
        nint x = 7;
        return x;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string endsInAnAllReturnSwitchˢ = "ends in an all-return switch (control)"u8;

internal static nint endsInSwitch(nint k) {
    GoFrame ᒐ = default;
    try {
        defer(where, endsInAnAllReturnSwitchˢ, ref ᒐ);
        switch (k) {
        case 1: {
            return 10;
        }
        default: {
            return 20;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string funcLiteralFallsOffTheˢ = "func literal, falls off the end"u8;

internal static void Main() {
    fallsOffEnd();
    loopDefer();
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(where, funcLiteralFallsOffTheˢ, ref ᒐ);
            nint y = 2;
            _ = y;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    endsInReturn();
    endsInSwitch(5);
}

} // end main_package
