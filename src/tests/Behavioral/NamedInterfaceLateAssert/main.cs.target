namespace go;

using fmt = fmt_package;
using latelib = NamedInterfaceLateAssert.latelib_package;
using NamedInterfaceLateAssert;

partial class main_package {

partial struct dirLike /*@string*/;

internal static @string Name(this dirLike d) {
    return ((@string)d);
}

internal static @string Kind(this dirLike d) {
    return "dir";
}

internal static @string Detail(this dirLike d, @string prefix) {
    return prefix + ":"u8 + ((@string)d);
}

internal static @string Whisper(this dirLike d) {
    return "shh "u8 + ((@string)d);
}

partial struct partial {
    internal @string n;
}

internal static @string Name(this partial p) {
    return p.n;
}

internal static @string Kind(this partial p) {
    return "partial";
}

internal static void report(@string label, @string s, bool ok) {
    fmt.Println(label, ok, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string rootˢ = "root"u8;

internal static void Main() {
    ref var d = ref heap(new dirLike(), out var Ꮡd);
    d = rootˢ;

    var (s, ok) = latelib.TryTop(d);
    report("Top-dirLike    ", s, ok);

    (s, ok) = latelib.TryMid(d);
    report("Mid-dirLike    ", s, ok);

    (s, ok) = latelib.TryBase(d);
    report("Base-dirLike   ", s, ok);

    (s, ok) = latelib.TryQuiet(d);
    report("quiet-dirLike  ", s, ok);

    (s, ok) = latelib.TryTop(Ꮡd);
    report("Top-ptrDirLike ", s, ok);

    (s, ok) = latelib.TryQuiet(Ꮡd);
    report("quiet-ptrDir   ", s, ok);

    (s, ok) = latelib.TryTop(new partial(n: "p"));
    report("Top-partial    ", s, ok);

    (s, ok) = latelib.TryMid(new partial(n: "p"));
    report("Mid-partial    ", s, ok);

    (s, ok) = latelib.TryQuiet(new partial(n: "p"));
    report("quiet-partial  ", s, ok);
}

} // end main_package
