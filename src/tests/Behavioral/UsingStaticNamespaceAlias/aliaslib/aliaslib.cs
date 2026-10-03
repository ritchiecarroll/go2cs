namespace go.UsingStaticNamespaceAlias;

partial class aliaslib_package {

public static @string Unsafe = "unsafe-var"u8;

public static nint Closure(nint n) {
    return n * 3;
}

public static @string Marshal(@string s) {
    return "<"u8 + s + ">"u8;
}

} // end aliaslib_package
