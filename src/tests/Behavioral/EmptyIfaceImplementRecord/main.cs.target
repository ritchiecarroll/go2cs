global using EmptyInterface = object;

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {
// Descriptor carrier for `EmptyInterface` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("EmptyInterface")] public interface EmptyInterfaceᴅ { }


[GoType] partial struct name {
    internal @string s;
}

internal static @string String(this name n) {
    return n.s;
}

internal static nint args(params ꓸꓸꓸany xsʗp) {
    var xs = xsʗp.sslice();

    return len(xs);
}

[GoType("dyn")] internal partial struct main_xs {
    [GoDescriptorType(Self = typeof(EmptyInterfaceᴅ))]
    [GoEmbedded] public EmptyInterface EmptyInterface;
}

internal static void Main() {
    fmt.Stringer s = new name("ok"u8);
    fmt.Println(args(new main_xs()), s.String());
}

} // end main_package
