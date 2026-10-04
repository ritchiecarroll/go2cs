global using Hook = object;

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸHook = Span<Hook>;

partial class main_package {
// Descriptor carrier for `Hook` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("Hook")] public interface Hookᴅ { }


internal static nint compose(params ꓸꓸꓸHook hsʗp) {
    var hs = hsʗp.sslice();

    return len(hs);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object twoˢ = (@string)"two"u8;

internal static void Main() {
    fmt.Println(compose((nint)(1), twoˢ, 3.0D));
}

} // end main_package
