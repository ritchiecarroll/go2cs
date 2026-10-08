namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct A /*@string*/;

partial struct B /*@string*/;

partial struct otherString /*@string*/;

partial struct pkgString /*@string*/;

partial struct T /*bool*/;

partial struct U /*bool*/;

public static nint Len(this A a) {
    return len(a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;
private static readonly @string xyzˢ = "xyz"u8;

internal partial struct main_myString /*@string*/;

internal partial struct main_N /*num:nint*/;

internal partial struct main_M /*num:nint*/;

internal static void Main() {
    A a = ((A)(@string)abcˢ);
    B b = ((B)(@string)a);
    fmt.Println(b, len(b), a.Len());
    otherString o = ((otherString)(@string)xyzˢ);
    pkgString p = ((pkgString)(@string)o);
    fmt.Println(p, ((otherString)(@string)p));
    main_myString m = ((main_myString)(@string)o);
    fmt.Println(m, ((otherString)(@string)m) == o);
    var t = ((T)true);
    var u = ((U)(bool)t);
    fmt.Println(u, !(bool)u);
    fmt.Println(((main_M)(nint)((main_N)7)));
}

} // end main_package
