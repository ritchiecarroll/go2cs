namespace go.UsingStaticNamespaceAlias;

partial class aliaslib_package {

public static readonly ж<@string> ᏑUnsafe = new StaticBox<@string>("unsafe-var");
public static ref @string Unsafe => ref ((StaticBox<@string>)ᏑUnsafe).ValueSlot;

public static nint Closure(nint n) {
    return n * 3;
}

public static @string Marshal(@string s) {
    return "<"u8 + s + ">"u8;
}

} // end aliaslib_package
