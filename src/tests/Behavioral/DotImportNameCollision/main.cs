namespace go;

using fmt = fmt_package;
using static DotImportNameCollision.a.keys_package;
using keys = DotImportNameCollision.b.keys_package;
using DotImportNameCollision.a;
using DotImportNameCollision.b;

partial class main_package {

partial struct holder {
    internal ж<global::go.DotImportNameCollision.a.keys_package.Key> k;
    internal global::go.DotImportNameCollision.a.keys_package.Count v;
}

internal static T first<T>(slice<T> xs) {
    return xs[0];
}

internal static void Main() {
    ж<global::go.DotImportNameCollision.a.keys_package.Key> k = NewKey("dot");
    global::go.DotImportNameCollision.a.keys_package.Count v = 3;
    var ks = slice<ж<global::go.DotImportNameCollision.a.keys_package.Key>>([k, NewKey("second")]);
    var m = new map<@string, global::go.DotImportNameCollision.a.keys_package.Count>{["x"] = v};
    var h = new holder(k: k, v: v);
    global::go.DotImportNameCollision.a.keys_package.Box<ж<global::go.DotImportNameCollision.a.keys_package.Key>> b = Wrap<ж<global::go.DotImportNameCollision.a.keys_package.Key>>(k);
    var f = first<ж<global::go.DotImportNameCollision.a.keys_package.Key>>(ks);

    var other = keys.NewKey(7);

    fmt.Println(k.Name(), v, len(ks), m["x"], h.k.Name(), h.v, b.V.Name(), f.Name(), other.ID());
}

} // end main_package
