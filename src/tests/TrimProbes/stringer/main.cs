namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;

partial class main_package {

partial struct K /*num:nuint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string kayˢ = "kay"u8;

public static @string String(this K k) {
    return kayˢ;
}

partial struct S {
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string essˢ = "ess"u8;

public static @string String(this S s) {
    return essˢ;
}

partial struct P {
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string peeˢ = "pee"u8;

public static @string String(this ref P p) {
    return peeˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object reflectKindˢ = (@string)"1 reflect.Kind:"u8;
private static readonly object timeDurationˢ = (@string)"2 time.Duration:"u8;
private static readonly object mainNamedIntˢ = (@string)"3 main named int:"u8;
private static readonly object mainStructˢ = (@string)"4 main struct:"u8;
private static readonly object mainPointerReceiverˢ = (@string)"5 main pointer receiver:"u8;
private static readonly object assertKindToFmtStringerˢ = (@string)"7 assert Kind to fmt.Stringer:"u8;
private static readonly object directCallˢ = (@string)"8 direct call:"u8;

internal static void Main() {
    fmt.Println(reflectKindˢ, reflect.Array);
    fmt.Println(timeDurationˢ, 1500 * time.Millisecond);
    fmt.Println(mainNamedIntˢ, ((K)3));
    fmt.Println(mainStructˢ, new S(1));
    fmt.Println(mainPointerReceiverˢ, Ꮡ(new P(1)));
    fmt.Printf("6 %%v of a Kind: %v, %%s: %s, %%d: %d\n"u8, reflect.ΔSlice, reflect.Map, reflect.ΔInt);
    any x = reflect.Struct;
    var (s, ok) = x._<fmt.Stringer>(ᐧ);
    fmt.Println(assertKindToFmtStringerˢ, ok, s != default!);
    fmt.Println(directCallˢ, reflect.Chan.String());
}

} // end main_package
