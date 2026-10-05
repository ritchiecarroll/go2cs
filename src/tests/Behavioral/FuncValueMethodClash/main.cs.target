namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;

partial class main_package {

[GoType] partial struct T {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string methodˢ = "method"u8;

public static @string Run(this T _) {
    return methodˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string funcˢ = "func"u8;

public static @string Run() {
    return funcˢ;
}

internal static @string use(Func<@string> f) {
    return f();
}

internal static @string show(any v) {
    var t = reflect.TypeOf(v);
    return fmt.Sprintf("%s in=%d out=%d"u8, t.Kind(), t.NumIn(), t.NumOut());
}

internal static any retAny() {
    return Run;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sameˢ = (@string)"same := "u8;
private static readonly object varˢ = (@string)"| var = "u8;
private static readonly object sameAnyˢ = (@string)"same any:"u8;
private static readonly object importedˢ = (@string)"imported := "u8;
private static readonly object importedAnyˢ = (@string)"imported any:"u8;
private static readonly object slotsˢ = (@string)"slots:"u8;
private static readonly object controlˢ = (@string)"control:"u8;

internal static void Main() {
    var a = Run;
    Func<@string> b = Run;
    fmt.Println(sameˢ, a(), varˢ, b());
    fmt.Println(sameAnyˢ, show(Run));
    _ = (Func<@string>)(Run);
    var c = time.After;
    Func<time.Duration, /*<-*/channel<time.Time>> d = time.After;
    fmt.Println(importedˢ, show((c).OrTypedNilFunc()), varˢ, show((d).OrTypedNilFunc()));
    fmt.Println(importedAnyˢ, show(time.After));
    _ = (Func<time.Duration, /*<-*/channel<time.Time>>)(time.After);
    any f = Run;
    any h = default!;
    h = time.After;
    var g = new any[]{time.After, Run}.slice();
    fmt.Println(slotsˢ, show(f), (@string)"|"u8, show(h), (@string)"|"u8, show(g[0]), (@string)"|"u8, show(g[1]), (@string)"|"u8, show(retAny()));
    Func<@string> e = Run;
    fmt.Println(controlˢ, e(), use(Run), new T(nil).Run());
}

} // end main_package
