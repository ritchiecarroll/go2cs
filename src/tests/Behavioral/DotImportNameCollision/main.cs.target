namespace go;

using fmt = fmt_package;
using static DotImportNameCollision.a.keys_package;
using keys = DotImportNameCollision.b.keys_package;
using DotImportNameCollision.a;
using DotImportNameCollision.b;

partial class main_package {

partial struct holder {
    internal ж<keys.Key> k;
    internal keys.Count v;
}

internal static T first<T>(slice<T> xs) {
    return xs[0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dotˢ = "dot"u8;
private static readonly @string secondˢ = "second"u8;

internal static void Main() {
    ж<keys.Key> k = NewKey(dotˢ);
    keys.Count v = 3;
    var ks = new ж<keys.Key>[]{k, NewKey(secondˢ)}.slice();
    var m = new map<@string, keys.Count>{["x"u8] = v};
    var h = new holder(k: k, v: v);
    keys.Box<ж<keys.Key>> b = Wrap<ж<keys.Key>>(k);
    var f = first<ж<keys.Key>>(ks);

    var other = keys.NewKey(7);

    fmt.Println(k.Name(), v, len(ks), m["x"u8], h.k.Name(), h.v, b.V.Name(), f.Name(), other.ID());
}

} // end main_package
