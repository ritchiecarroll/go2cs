namespace go;

using fmt = fmt_package;
using latelib = NamedInterfaceLateAssert.latelib_package;
using NamedInterfaceLateAssert;

partial class main_package {

partial struct dirLike /*@string*/;

internal static @string Name(this dirLike d) {
    return ((@string)d);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dirˢ = "dir"u8;

internal static @string Kind(this dirLike d) {
    return dirˢ;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string partialˢ = "partial"u8;

internal static @string Kind(this partial p) {
    return partialˢ;
}

internal static void report(@string label, @string s, bool ok) {
    fmt.Println(label, ok, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string rootˢ = "root"u8;
private static readonly @string topDirLikeˢ = "Top-dirLike    "u8;
private static readonly @string midDirLikeˢ = "Mid-dirLike    "u8;
private static readonly @string baseDirLikeˢ = "Base-dirLike   "u8;
private static readonly @string quietDirLikeˢ = "quiet-dirLike  "u8;
private static readonly @string topPtrDirLikeˢ = "Top-ptrDirLike "u8;
private static readonly @string quietPtrDirˢ = "quiet-ptrDir   "u8;
private static readonly @string topPartialˢ = "Top-partial    "u8;
private static readonly @string midPartialˢ = "Mid-partial    "u8;
private static readonly @string quietPartialˢ = "quiet-partial  "u8;

internal static void Main() {
    ref var d = ref heap(new dirLike(), out var Ꮡd);
    d = rootˢ;
    var (s, ok) = latelib.TryTop(d);
    report(topDirLikeˢ, s, ok);
    (s, ok) = latelib.TryMid(d);
    report(midDirLikeˢ, s, ok);
    (s, ok) = latelib.TryBase(d);
    report(baseDirLikeˢ, s, ok);
    (s, ok) = latelib.TryQuiet(d);
    report(quietDirLikeˢ, s, ok);
    (s, ok) = latelib.TryTop(Ꮡd);
    report(topPtrDirLikeˢ, s, ok);
    (s, ok) = latelib.TryQuiet(Ꮡd);
    report(quietPtrDirˢ, s, ok);
    (s, ok) = latelib.TryTop(new partial(n: "p"u8));
    report(topPartialˢ, s, ok);
    (s, ok) = latelib.TryMid(new partial(n: "p"u8));
    report(midPartialˢ, s, ok);
    (s, ok) = latelib.TryQuiet(new partial(n: "p"u8));
    report(quietPartialˢ, s, ok);
}

} // end main_package
