namespace go;

using fmt = fmt_package;
using lib = global::go.go.ppembedlib_package;
using global::go.go;
using ppembedlib = global::go.go.ppembedlib_package;

partial class main_package {

internal static void main() {
    var l = lib.NewLink("dest");
    l.Add("a");
    l.Add("b");
    fmt.Println("link:", l.Dest, l.Count(), l.Last());

    lib.Link v = new(nil);
    v.Add("v1");
    fmt.Println("link value:", v.Count(), v.Last());

    var img = Ꮡ(new lib.Image());
    img.Add("i1");
    img.Add("i2");
    fmt.Println("image:", img.Count(), img.Last());

    lib.Image iv = new(nil);
    iv.Add("iv");
    fmt.Println("image value:", iv.Count(), iv.Last());

    var e = Ꮡ(new lib.Emph());
    e.Add("e1");
    e.Add("e2");
    fmt.Println("emph:", e.Count(), e.Last());

    var p = Ꮡ(new lib.Para());
    p.of(lib.Para.ᏑBaseNode).Add("p1");
    fmt.Println("para:", p.of(lib.Para.ᏑBaseNode).Count(), p.of(lib.Para.ᏑBaseNode).Last());

    var t = Ꮡ(new lib.Tag());
    t.Bump();
    fmt.Println("tag:", t.Bump());
}

} // end main_package
