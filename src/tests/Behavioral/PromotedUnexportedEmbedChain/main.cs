namespace go;

using fmt = fmt_package;
using lib = global::go.go.ppembedlib_package;
using global::go.go;
using ppembedlib = global::go.go.ppembedlib_package;

partial class main_package {

internal static void main() {
    var l = lib.NewLink("dest");
    l.of(lib.Link.ᏑbaseLink).of(lib.baseLink.ᏑBaseInline).of(lib.BaseInline.ᏑBaseNode).Add("a");
    l.of(lib.Link.ᏑbaseLink).of(lib.baseLink.ᏑBaseInline).of(lib.BaseInline.ᏑBaseNode).Add("b");
    fmt.Println("link:", l.Dest, l.of(lib.Link.ᏑbaseLink).of(lib.baseLink.ᏑBaseInline).of(lib.BaseInline.ᏑBaseNode).Count(), l.of(lib.Link.ᏑbaseLink).of(lib.baseLink.ᏑBaseInline).of(lib.BaseInline.ᏑBaseNode).Last());

    lib.Link v = new(nil);
    v.baseLink.BaseInline.BaseNode.Add("v1");
    fmt.Println("link value:", v.baseLink.BaseInline.BaseNode.Count(), v.baseLink.BaseInline.BaseNode.Last());

    var img = Ꮡ(new lib.Image());
    img.of(lib.Image.ᏑInline).of(lib.Inline.Ꮡinner).of(lib.inner.ᏑBaseNode).Add("i1");
    img.of(lib.Image.ᏑInline).of(lib.Inline.Ꮡinner).of(lib.inner.ᏑBaseNode).Add("i2");
    fmt.Println("image:", img.of(lib.Image.ᏑInline).of(lib.Inline.Ꮡinner).of(lib.inner.ᏑBaseNode).Count(), img.of(lib.Image.ᏑInline).of(lib.Inline.Ꮡinner).of(lib.inner.ᏑBaseNode).Last());

    lib.Image iv = new(nil);
    iv.Inline.inner.BaseNode.Add("iv");
    fmt.Println("image value:", iv.Inline.inner.BaseNode.Count(), iv.Inline.inner.BaseNode.Last());

    var e = Ꮡ(new lib.Emph());
    e.of(lib.Emph.ᏑemphBase).of(lib.emphBase.ᏑBaseNode).Add("e1");
    e.of(lib.Emph.ᏑemphBase).of(lib.emphBase.ᏑBaseNode).Add("e2");
    fmt.Println("emph:", e.of(lib.Emph.ᏑemphBase).of(lib.emphBase.ᏑBaseNode).Count(), e.of(lib.Emph.ᏑemphBase).of(lib.emphBase.ᏑBaseNode).Last());

    var p = Ꮡ(new lib.Para());
    p.of(lib.Para.ᏑBaseNode).Add("p1");
    fmt.Println("para:", p.of(lib.Para.ᏑBaseNode).Count(), p.of(lib.Para.ᏑBaseNode).Last());

    var t = Ꮡ(new lib.Tag());
    t.Bump();
    fmt.Println("tag:", t.Bump());
}

} // end main_package
