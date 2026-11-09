namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Tag {
    internal @string str;
    internal uint16 pExt;
}

public static readonly ж<Tag> ᏑUnd = new StaticBox<Tag>(default(Tag));
public static ref Tag Und => ref ((StaticBox<Tag>)ᏑUnd).ValueSlot;

internal static bool equalTags(this Tag t, Tag o) { return t.str == o.str; }

internal static nint gen(this Tag t, slice<byte> b) { return copy(b, "core"u8); }

public static void RemakeString(this ж<Tag> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt.equalTags(Und)) {
        return;
    }
    array<byte> buf = new(8);
    var b = buf.slice(0, Ꮡt.gen(buf[..]));
    t.pExt = (uint16)len(b);
    t.str = ((sstring)b) + "!"u8;
}

internal static (Tag, error) next(Tag t) {
    t.str += "+"u8;
    return (t, default);
}

internal static (Tag, error) minimize(this Tag t) {
    t.RemakeString();
    return (t, default);
}

internal static (Tag, error) minimizeDefine(this Tag t) {
    (t, var err) = next(t);
    if (err != nil) {
        return (t, err);
    }
    Ꮡt.RemakeString();
    return (t, default);
}

internal static (Tag, error) minimizeAssign(this Tag t) {
    error err = default;
    (t, err) = next(t);
    if (err != nil) {
        return (t, err);
    }
    t.RemakeString();
    return (t, default);
}

internal static Tag minimizeMethodValue(this Tag t) {
    var remake = Ꮡ(t).RemakeString;
    remake();
    return t;
}

partial struct wrapper {
    internal Tag tag;
    internal nint n;
}

internal static wrapper remakeField(this wrapper w) {
    Ꮡ(w).Ꮡtag.RemakeString();
    w.n++;
    return w;
}

partial struct handle {
    /*embed*/ public ж<Tag> Tag;
}

internal static void remake(this handle h) { h.RemakeString(); }

internal static void remakeMethodValue(this handle h) {
    var remake = Ꮡ(h).RemakeString;
    remake();
}

internal static void remakeHandleParam(handle hʗp) {
    ref var h = ref heap(hʗp, out var Ꮡh);

    Ꮡh.RemakeString();
}

internal static Tag remakeParam(Tag tʗp) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    Ꮡt.RemakeString();
    return t;
}

internal static Tag remakeLocal() {
    ref var t = ref heap<Tag>(new Tag(str: "local"), out var Ꮡt);
    Ꮡt.RemakeString();
    return t;
}

internal static void Main() {
    var orig = new Tag(str: "a");

    var (t, _) = orig.minimize();
    fmt.Println(t.str, t.pExt, orig.str, orig.pExt);

    (t, _) = orig.minimizeDefine();
    fmt.Println(t.str, t.pExt, orig.str, orig.pExt);

    (t, _) = orig.minimizeAssign();
    fmt.Println(t.str, t.pExt, orig.str, orig.pExt);

    t = orig.minimizeMethodValue();
    fmt.Println(t.str, t.pExt, orig.str, orig.pExt);

    var w = new wrapper(tag: orig);
    var w2 = w.remakeField();
    fmt.Println(w2.tag.str, w2.tag.pExt, w2.n, w.tag.str, w.n);

    t = remakeParam(orig);
    fmt.Println(t.str, t.pExt, orig.str);

    t = remakeLocal();
    fmt.Println(t.str, t.pExt);

    var (p1, p2, p3) = (Ꮡ(new Tag(str: "p1")), Ꮡ(new Tag(str: "p2")), Ꮡ(new Tag(str: "p3")));
    new handle(p1).remake();
    new handle(p2).remakeMethodValue();
    remakeHandleParam(new handle(p3));
    fmt.Println(p1.str, p1.pExt, p2.str, p2.pExt, p3.str, p3.pExt);

    var (p4, p5) = (Ꮡ(new Tag(str: "p4")), Ꮡ(new Tag(str: "p5")));
    ref var h4 = ref heap<handle>(out var Ꮡh4);
    h4 = new handle(p4);
    ref var h5 = ref heap<handle>(out var Ꮡh5);
    h5 = new handle(p5);
    Ꮡh4.RemakeString();
    var remake = Ꮡh5.RemakeString;
    remake();
    fmt.Println(p4.str, p4.pExt, p5.str, p5.pExt);

    Tag empty = default;
    (t, _) = empty.minimize();
    fmt.Printf("%q %d\n"u8, t.str, t.pExt);
}

} // end main_package
