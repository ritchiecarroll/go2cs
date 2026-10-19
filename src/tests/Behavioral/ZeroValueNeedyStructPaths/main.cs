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
private static readonly object mapMissingKeyˢ = (@string)"map missing key:"u8;
private static readonly object genericVarˢ = (@string)"generic var:"u8;
private static readonly object genericNamedResultˢ = (@string)"generic named result:"u8;
private static readonly object genericNewˢ = (@string)"generic new:"u8;
private static readonly object genericCapturedVarˢ = (@string)"generic captured var:"u8;
private static readonly object genericAddressedVarˢ = (@string)"generic addressed var:"u8;
private static readonly object closedChannelˢ = (@string)"closed channel:"u8;
private static readonly object parkedReceiveˢ = (@string)"parked receive:"u8;
private static readonly object parkedBareReceiveˢ = (@string)"parked bare receive:"u8;
private static readonly object failedAssertionˢ = (@string)"failed assertion:"u8;

internal static void Main() {
    arm("map missing key:", () => {
        var m = new map<@string, counts>{};
        var (v, ok) = m["x", ꟷ];
        fmt.Println(mapMissingKeyˢ, len(v.vals), v.vals[3], ok);
    });
    arm("generic var:", () => {
        var z = zeroVar<counts>();
        fmt.Println(genericVarˢ, len(z.vals), z.vals[3]);
    });
    arm("generic named result:", () => {
        var z = zeroNamed<counts>();
        fmt.Println(genericNamedResultˢ, len(z.vals), z.vals[3]);
    });
    arm("generic new:", () => {
        var z = zeroNew<counts>();
        fmt.Println(genericNewˢ, len(z.vals), z.vals[3]);
    });
    arm("generic captured var:", () => {
        var z = zeroCaptured<counts>();
        fmt.Println(genericCapturedVarˢ, len(z.vals), z.vals[3]);
    });
    arm("generic addressed var:", () => {
        var z = zeroAddressed<counts>();
        fmt.Println(genericAddressedVarˢ, len(z.vals), z.vals[3]);
    });
    arm("closed channel:", () => {
        var ch = new channel<counts>(0);
        close(ch);
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println(closedChannelˢ, len(v.vals), v.vals[3], ok);
    });
    arm("parked receive:", [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ1 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ1);
        });
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println(parkedReceiveˢ, len(v.vals), v.vals[3], ok);
    });
    arm("parked bare receive:", [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ2 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ2);
        });
        var v = ᐸꟷ(ch);
        fmt.Println(parkedBareReceiveˢ, len(v.vals), v.vals[3]);
    });
    arm("failed assertion:", () => {
        any x = (nint)(7);
        var (v, ok) = x._<counts>(ᐧ);
        fmt.Println(failedAssertionˢ, len(v.vals), v.vals[3], ok);
    });
}

} // end main_package
