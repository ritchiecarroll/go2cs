namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct counts {
    internal array<nint> vals = new(4);
    internal nint n;
}

internal static T zeroVar<T>() {
    T z = GoZero<T>();
    return z;
}

internal static T /*z*/ zeroNamed<T>() {
    T z = GoZero<T>();

    return z;
}

internal static T zeroNew<T>() {
    return @new<T>().ValueSlot;
}

internal static T zeroCaptured<T>() {
    T z = GoZero<T>();
    var zʗ1 = z;
    T get() => zʗ1;
    return get();
}

internal static T zeroAddressed<T>() {
    ref var z = ref heap<T>(out var Ꮡz);
    var p = Ꮡz;
    return p.ValueSlot;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object panicˢ = (@string)"PANIC"u8;

internal static void arm(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(name, panicˢ, r);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string mapMissingKeyˢ = "map missing key:"u8;
private static readonly @string genericVarˢ = "generic var:"u8;
private static readonly @string genericNamedResultˢ = "generic named result:"u8;
private static readonly @string genericNewˢ = "generic new:"u8;
private static readonly @string genericCapturedVarˢ = "generic captured var:"u8;
private static readonly @string genericAddressedVarˢ = "generic addressed var:"u8;
private static readonly @string closedChannelˢ = "closed channel:"u8;
private static readonly @string parkedReceiveˢ = "parked receive:"u8;
private static readonly @string parkedBareReceiveˢ = "parked bare receive:"u8;
private static readonly @string failedAssertionˢ = "failed assertion:"u8;

internal static void Main() {
    arm(mapMissingKeyˢ, () => {
        var m = new map<@string, counts>{};
        var (v, ok) = m["x"u8, ꟷ];
        fmt.Println(mapMissingKeyˢ, len(v.vals), v.vals[3], ok);
    });
    arm(genericVarˢ, () => {
        var z = zeroVar<counts>();
        fmt.Println(genericVarˢ, len(z.vals), z.vals[3]);
    });
    arm(genericNamedResultˢ, () => {
        var z = zeroNamed<counts>();
        fmt.Println(genericNamedResultˢ, len(z.vals), z.vals[3]);
    });
    arm(genericNewˢ, () => {
        var z = zeroNew<counts>();
        fmt.Println(genericNewˢ, len(z.vals), z.vals[3]);
    });
    arm(genericCapturedVarˢ, () => {
        var z = zeroCaptured<counts>();
        fmt.Println(genericCapturedVarˢ, len(z.vals), z.vals[3]);
    });
    arm(genericAddressedVarˢ, () => {
        var z = zeroAddressed<counts>();
        fmt.Println(genericAddressedVarˢ, len(z.vals), z.vals[3]);
    });
    arm(closedChannelˢ, () => {
        var ch = new channel<counts>(0);
        close(ch);
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println(closedChannelˢ, len(v.vals), v.vals[3], ok);
    });
    arm(parkedReceiveˢ, [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ1 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ1);
        });
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println(parkedReceiveˢ, len(v.vals), v.vals[3], ok);
    });
    arm(parkedBareReceiveˢ, [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ2 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ2);
        });
        var v = ᐸꟷ(ch);
        fmt.Println(parkedBareReceiveˢ, len(v.vals), v.vals[3]);
    });
    arm(failedAssertionˢ, () => {
        any x = (nint)(7);
        var (v, ok) = x._<counts>(ᐧ);
        fmt.Println(failedAssertionˢ, len(v.vals), v.vals[3], ok);
    });
}

} // end main_package
