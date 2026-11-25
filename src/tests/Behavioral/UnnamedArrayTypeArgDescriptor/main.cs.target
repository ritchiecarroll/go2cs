namespace go;

using fmt = fmt_package;
using unique = unique_package;
using wrap = UnnamedArrayTypeArgDescriptor.wrap_package;
using UnnamedArrayTypeArgDescriptor;

partial class main_package {

partial struct pair /*[2]@string*/;

internal static unique.Handle<T> wrapMake<T, Tᴀ>(T v)
    where Tᴀ : IGoArrayDims
{
    return unique.Make<T, Tᴀ>(v);
}

private sealed class ᴀ2 : IGoArrayDims { public static nint[] Dims { get; } = [2]; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string strˢ = "str"u8;

internal partial struct main_value /*dyn*/ {
    internal array<@string> a = new(2);
}

internal static void main() {
    var ab = unique.Make<array<@string>, ᴀ2>(array<@string>(["a", "b"]));
    var ac = unique.Make<array<@string>, ᴀ2>(array<@string>(["a", "c"]));
    var ab2 = unique.Make<array<@string>, ᴀ2>(array<@string>(["a", "b"]));
    var abc = unique.Make<array<@string>, ᴀ3>(array<@string>(["a", "b", "c"]));
    fmt.Println("pair ab == ab:", ab == ab2);
    fmt.Println("pair ab == ac:", ab == ac);
    fmt.Println("pair values:", ab.Value(), ac.Value(), ab2.Value());
    fmt.Println("triple value:", abc.Value(), len(abc.Value()));

    var p = Ꮡ(array<@string>(["x", "y"]));
    var q = Ꮡ(array<@string>(["x", "y"]));
    fmt.Println("pointer p == p:", unique.Make<ж<array<@string>>>(p) == unique.Make<ж<array<@string>>>(p));
    fmt.Println("pointer p == q:", unique.Make<ж<array<@string>>>(p) == unique.Make<ж<array<@string>>>(q));

    var s1 = unique.Make<main_value>(new main_value(array<@string>(["s", "t"])));
    var s2 = unique.Make<main_value>(new main_value(array<@string>(["s", "t"])));
    var s3 = unique.Make<main_value>(new main_value(array<@string>(["s", "u"])));
    fmt.Println("struct same:", s1 == s2, "different:", s1 == s3, "value:", s1.Value().a);

    var d1 = unique.Make<pair>(new pair(new @string[]{"d", "e"}.array()));
    var d2 = unique.Make<pair>(new pair(new @string[]{"d", "e"}.array()));
    var d3 = unique.Make<pair>(new pair(new @string[]{"d", "f"}.array()));
    fmt.Println("defined same:", d1 == d2, "different:", d1 == d3, "value:", d1.Value());

    var f = unique.Make<array<@string>, ᴀ2>;
    fmt.Println("method value same:", f(array<@string>(["m", "n"])) == f(array<@string>(["m", "n"])),
        "different:", f(array<@string>(["m", "n"])) == f(array<@string>(["m", "o"])));
    fmt.Println("method value meets direct:", f(array<@string>(["a", "b"])) == ab);

    var w1 = wrapMake<array<@string>, ᴀ2>(array<@string>(["w", "x"]));
    var w2 = wrapMake<array<@string>, ᴀ2>(array<@string>(["w", "x"]));
    var w3 = wrapMake<array<@string>, ᴀ2>(array<@string>(["w", "y"]));
    fmt.Println("local wrapper same:", w1 == w2, "different:", w1 == w3, "meets direct:", wrapMake<array<@string>, ᴀ2>(array<@string>(["a", "b"])) == ab);

    var e1 = wrap.Wrap<array<@string>, ᴀ2>(array<@string>(["e", "f"]));
    var e2 = wrap.Wrap<array<@string>, ᴀ2>(array<@string>(["e", "f"]));
    var e3 = wrap.Wrap<array<@string>, ᴀ3>(array<@string>(["e", "f", "g"]));
    fmt.Println("foreign wrapper same:", e1 == e2, "meets direct:", wrap.Wrap<array<@string>, ᴀ2>(array<@string>(["a", "b"])) == ab, "triple:", e3.Value());
    fmt.Println("foreign Same:", wrap.Same<array<@string>, ᴀ2>(array<@string>(["k", "l"]), array<@string>(["k", "l"])), wrap.Same<array<@string>, ᴀ2>(array<@string>(["k", "l"]), array<@string>(["k", "m"])));
    fmt.Println("foreign non-array:", wrap.Same(strˢ, strˢ), wrap.Same<nint>(1, 2));
}

private sealed class ᴀ3 : IGoArrayDims { public static nint[] Dims { get; } = [3]; }

} // end main_package
