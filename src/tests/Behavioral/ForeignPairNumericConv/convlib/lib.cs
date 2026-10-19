namespace go.ForeignPairNumericConv;

partial class convlib_package {

partial struct Handle /*num:uintptr*/;

partial struct Token /*num:uintptr*/;

public static Token NewToken(uintptr v) {
    return ((Token)v);
}

public static @string String(this Handle h) {
    return "handle";
}

public static uintptr Raw(this Token t) {
    return (uintptr)t;
}

} // end convlib_package
