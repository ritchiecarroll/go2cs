namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct Color /*num:nint*/;

public static Color Red => 1;
internal static readonly @string greeting = "hi"u8;
internal const int64 big = /* 1 << 40 */ 1099511627776;

internal static channel<@string> results = new channel<@string>(1);

internal static @string kind(any v) {
    switch (v.type()) {
    case @string x: {
        return "string "u8 + x;
    }
    case nint x: {
        return fmt.Sprint("int ", x);
    }
    case int32 x: {
        return "rune "u8 + ((@string)x);
    }
    case int64 x: {
        return fmt.Sprint("int64 ", x);
    }
    case float64 x: {
        return fmt.Sprint("float64 ", x);
    }
    case bool x: {
        return fmt.Sprint("bool ", x);
    }
    case Color x: {
        return fmt.Sprint("Color ", (nint)x);
    }
    default: {
        var x = v;
        return fmt.Sprintf("other %T"u8, v);
    }}
}

internal static void report(any v) {
    results.ᐸꟷ(kind(v));
}

internal static void show(any v) {
    fmt.Println("deferred", kind(v));
}

internal static void worker(nint n) {
    results.ᐸꟷ(fmt.Sprint("worker ", n));
}

internal static void pair(@string label, nint n) {
    results.ᐸꟷ(fmt.Sprint(label, " ", n));
}

internal static void showPair(@string label, nint n) {
    fmt.Println("deferred", label, n);
}

internal static nint seq;

internal static nint next() {
    seq++;
    return seq;
}

internal static void trio(nint a, @string s, nint b) {
    fmt.Println("deferred trio", a, s, b);
}

partial struct box {
    internal @string name;
}

internal static void say(this ref box b, @string msg) {
    results.ᐸꟷ(b.name + " says "u8 + msg);
}

internal static void @catch(@string tag) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var r = recover(calledByDefer); if (r != nil) {
                fmt.Println(tag, "recovered:", r);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object firstˢ = (@string)"First"u8;
private static readonly @string squareˢ = "square:"u8;
private static readonly @string helloˢ = "hello"u8;
private static readonly object viaFuncValueˢ = (@string)"via func value"u8;

internal static partial void spawned() {
    goǃ(report, firstˢ);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, (nint)7);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, (rune)'r');
    fmt.Println(ᐸꟷ(results));

    goǃ(report, 2.5D);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, (bool)true);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, Red);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, big);
    fmt.Println(ᐸꟷ(results));

    goǃ(report, (@string)greeting);
    fmt.Println(ᐸꟷ(results));

    goǃ(worker, (nint)5);
    fmt.Println(ᐸꟷ(results));

    nint n = 3;
    goǃ(pair, squareˢ, n * n);
    n = 100;
    fmt.Println(ᐸꟷ(results));

    var b = Ꮡ(new box(name: "b"));
    var bʗ1 = b;
    goǃ(bʗ1.say, helloˢ, GoStack.Bounded);
    fmt.Println(ᐸꟷ(results));

    var f = report;
    var fʗ1 = f;
    goǃ(fʗ1, viaFuncValueˢ);
    fmt.Println(ᐸꟷ(results));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string xWasˢ = "x was"u8;
private static readonly object doneˢ = (@string)"done"u8;
private static readonly @string midˢ = "mid"u8;

internal static void deferred() {
    GoFrame ᒐ = default;
    try {
        nint x = 1;
        defer(showPair, xWasˢ, x, ref ᒐ);
        defer(show, doneˢ, ref ᒐ);
        defer(show, Red, ref ᒐ);
        defer(show, (rune)'d', ref ᒐ);
        defer(trio, next(), midˢ, next(), ref ᒐ);
        x = 2;
        seq = 100;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string caughtˢ = "caught"u8;

internal static void panicky() {
    GoFrame ᒐ = default;
    try {
        defer(@catch, caughtˢ, ref ᒐ, recovers: true);
        throw panic("boom");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    spawned();
    deferred();
    panicky();
    fmt.Println("end");
}

} // end main_package
