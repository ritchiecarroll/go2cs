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

internal static void arm(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println(name, "PANIC", r);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    arm("map missing key:", () => {
        var m = new map<@string, counts>{};
        var (v, ok) = m["x", ꟷ];
        fmt.Println("map missing key:", len(v.vals), v.vals[3], ok);
    });
    arm("generic var:", () => {
        var z = zeroVar<counts>();
        fmt.Println("generic var:", len(z.vals), z.vals[3]);
    });
    arm("generic named result:", () => {
        var z = zeroNamed<counts>();
        fmt.Println("generic named result:", len(z.vals), z.vals[3]);
    });
    arm("generic new:", () => {
        var z = zeroNew<counts>();
        fmt.Println("generic new:", len(z.vals), z.vals[3]);
    });
    arm("generic captured var:", () => {
        var z = zeroCaptured<counts>();
        fmt.Println("generic captured var:", len(z.vals), z.vals[3]);
    });
    arm("generic addressed var:", () => {
        var z = zeroAddressed<counts>();
        fmt.Println("generic addressed var:", len(z.vals), z.vals[3]);
    });
    arm("closed channel:", () => {
        var ch = new channel<counts>(0);
        close(ch);
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println("closed channel:", len(v.vals), v.vals[3], ok);
    });
    arm("parked receive:", [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ1 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ1);
        }, GoStack.Bounded);
        var (v, ok) = ᐸꟷ(ch, ꟷ);
        fmt.Println("parked receive:", len(v.vals), v.vals[3], ok);
    });
    arm("parked bare receive:", [MethodImpl(MethodImplOptions.NoInlining)] () => {
        var ch = new channel<counts>(0);
        var chʗ2 = ch;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            close(chʗ2);
        }, GoStack.Bounded);
        var v = ᐸꟷ(ch);
        fmt.Println("parked bare receive:", len(v.vals), v.vals[3]);
    });
    arm("failed assertion:", () => {
        any x = (nint)7;
        var (v, ok) = x._<counts>(ᐧ);
        fmt.Println("failed assertion:", len(v.vals), v.vals[3], ok);
    });
}

} // end main_package
