namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Tag {
    internal nint id;
    public nint Value;
    internal Seal inner;
}

partial struct Seal {
    internal nint n;
}

partial struct Chest {
    internal Tag tag;
    internal slice<Tag> tags;
}

internal static Tag Tag⁔(/*this*/ ref Chest c) { return c.tag; }

internal static ж<nint> Seal⁔(/*this*/ ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }

internal static @string Forms⁔(/*this*/ ж<Chest> Ꮡc, any v) {
    ref var c = ref Ꮡc.DerefOrNull();

    var lit = new Tag(id: 1, Value: 2);
    var ptr = Ꮡ(new Tag(id: 3));

    Tag zero = default;

    var list = slice<Tag>([lit, ptr.Value, zero]);
    var table = new map<Tag, nint>{[lit] = 1};
    var made = new slice<Tag>(2);

    var fresh = @new<Tag>();
    fresh.Value.Value = 9;

    var valueAddr = ptr.of(Tag.ᏑValue);
    valueAddr.Value += 4;

    var idAddr = Ꮡc.Ꮡtag.Ꮡid;
    idAddr.Value = 7;

    var (_, isTag) = v._<Tag>(ᐧ);
    var (_, isPtr) = v._<ж<Tag>>(ᐧ);

    @string kind = "other";

    switch (v.type()) {
    case Tag: {
        kind = "tag";
        break;
    }
    case ж<Tag> _:
    case slice<Tag> _: {
        kind = "tags";
        break;
    }}

    nint count = len(list) + len(table) + len(made) + pick<Tag>(list, 0).id;

    return fmt.Sprint(lit.id, (~ptr).Value, zero.id, count, (~fresh).Value, c.tag.id, isTag, isPtr, kind, Ꮡc.Seal().Value);
}

internal static T pick<T>(slice<T> s, nint i) { return s[i]; }

partial struct SliceBuilder {
    internal nint n;
}

internal static nint Limit⁔(/*this*/ ref main_package.SliceBuilder s) {
    var next = new main_package.SliceBuilder(n: s.n + (nint)Limit);
    s = next;
    return s.n;
}

partial struct math {
    internal nint n;
}

internal static nint Limit⁔(/*this*/ ref main_package.math m) {
    var next = new main_package.math(n: m.n + (nint)Limit);
    m = next;
    return m.n;
}

internal static void forms() {
    var c = Ꮡ(new Chest(tags: [new(id: 2)]));
    fmt.Println("forms", c.Forms(new Tag()), c.Forms(Ꮡ(new Tag())), c.Forms((nint)1), c.Tag().id, len(c.tags));

    var sb = new SliceBuilder(n: 2);
    nint sbGot = sb.Limit();
    fmt.Println("golib-named type", sbGot, sb.n);

    var mt = new math(n: 1);
    nint mtGot = mt.Limit();
    fmt.Println("namespace-named type", mtGot, mt.n);
}

} // end main_package
