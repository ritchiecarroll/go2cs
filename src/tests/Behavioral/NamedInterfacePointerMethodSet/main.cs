namespace go;

using fmt = fmt_package;
using pmslib = NamedInterfacePointerMethodSet.pmslib_package;
using NamedInterfacePointerMethodSet;

partial class main_package {

partial struct PtrOnly {
    internal @string name;
}

public static @string Speak(this ref PtrOnly p) {
    return "ptr:"u8 + p.name;
}

partial struct ValOnly {
    internal @string name;
}

public static @string Speak(this ValOnly v) {
    return "val:"u8 + v.name;
}

partial struct Mixed {
    internal nint n;
}

public static @string Speak(this Mixed m) {
    return fmt.Sprintf("mixed:%d"u8, m.n);
}

public static void Bump(this ref Mixed m) {
    m.n++;
}

partial struct WrongSig {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string wrongˢ = "wrong"u8;

public static @string Speak(this WrongSig w, nint times) {
    return wrongˢ;
}

partial struct gbox<T> {
    internal T v;
}

internal static T Get<T>(this gbox<T> b) {
    return b.v;
}

internal static void report(@string label, @string s, bool ok) {
    fmt.Println(label, ok, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ptrOnlyValueˢ = "PtrOnly-value  "u8;
private static readonly @string ptrOnlyPointerˢ = "PtrOnly-pointer"u8;
private static readonly @string valOnlyValueˢ = "ValOnly-value  "u8;
private static readonly @string valOnlyPointerˢ = "ValOnly-pointer"u8;
private static readonly @string mixedValueˢ = "Mixed-value    "u8;
private static readonly @string mixedPointerˢ = "Mixed-pointer  "u8;
private static readonly @string wrongSigˢ = "WrongSig       "u8;
private static readonly @string intˢ = "int            "u8;
private static readonly @string gboxIntˢ = "gbox-int       "u8;
private static readonly @string gboxStringˢ = "gbox-string    "u8;

internal static void Main() {
    var (s, ok) = pmslib.TrySpeak(new PtrOnly(name: "a"u8));
    report(ptrOnlyValueˢ, s, ok);
    (s, ok) = pmslib.TrySpeak(Ꮡ(new PtrOnly(name: "a"u8)));
    report(ptrOnlyPointerˢ, s, ok);
    (s, ok) = pmslib.TrySpeak(new ValOnly(name: "b"u8));
    report(valOnlyValueˢ, s, ok);
    (s, ok) = pmslib.TrySpeak(Ꮡ(new ValOnly(name: "b"u8)));
    report(valOnlyPointerˢ, s, ok);
    (s, ok) = pmslib.TrySpeak(new Mixed(n: 41));
    report(mixedValueˢ, s, ok);
    var pm = Ꮡ(new Mixed(n: 41));
    pm.Bump();
    (s, ok) = pmslib.TrySpeak(pm.OrTypedNil());
    report(mixedPointerˢ, s, ok);
    (s, ok) = pmslib.TrySpeak(new WrongSig(nil));
    report(wrongSigˢ, s, ok);
    (s, ok) = pmslib.TrySpeak((nint)(42));
    report(intˢ, s, ok);
    (s, ok) = pmslib.TryGet(new gbox<nint>(v: 7));
    report(gboxIntˢ, s, ok);
    (s, ok) = pmslib.TryGet(new gbox<@string>(v: "g"u8));
    report(gboxStringˢ, s, ok);
}

} // end main_package
