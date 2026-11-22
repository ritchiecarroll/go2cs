namespace go;

using static PromotedValueEmbedLib_package;

partial class PromotedValueEmbedLib_package {

partial struct common {
    internal @string label;
}

} partial class PromotedValueEmbedLibᴛcommon {

internal static @string Name(this ref common c) {
    return c.label;
}

} partial class PromotedValueEmbedLib_package {

partial struct Widget {
    /*embed*/ internal common common;
}

public static ж<Widget> New(@string label) {
    return Ꮡ(new Widget(common: new common(label: label)));
}

partial struct Gadget {
    internal @string tag;
}

} partial class PromotedValueEmbedLibᴛGadget {

public static @string Name(this ref Gadget g) {
    return g.tag;
}

} partial class PromotedValueEmbedLib_package {

public static ж<Gadget> NewGadget(@string tag) {
    return Ꮡ(new Gadget(tag: tag));
}

partial struct Meta {
    internal @string text;
    internal bool ok;
}

} partial class PromotedValueEmbedLibᴛMeta {

public static @string Text(this Meta m) { return m.text; }

public static bool Ok(this Meta m) { return m.ok; }

public static (@string, bool) Both(this Meta m) { return (m.text, m.ok); }

public static (ж<Meta>, @string) Self(this ж<Meta> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    return (Ꮡm, m.text);
}

} partial class PromotedValueEmbedLib_package {

partial struct Badge {
    /*embed*/ public Meta Meta;
    internal nint kind;
}

public static Badge NewBadge(@string text) {
    return new Badge(Meta: new Meta(text: text, ok: true), kind: 1);
}

} // end PromotedValueEmbedLib_package
