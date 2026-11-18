namespace go;

using fmt = fmt_package;
using static main_package;

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

} partial class mainᴛChest {

public static Tag Tag(this ref Chest c) { return c.tag; }

public static ж<nint> Seal(this ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }

public static @string Forms(this ж<Chest> Ꮡc, any v) {
    ref var c = ref Ꮡc.DerefOrNull();

    var lit = new Tag(id: 1, Value: 2);
    var ptr = Ꮡ(new Tag(id: 3));

    Tag zero = default;

    var list = slice<Tag>([lit, ptr.Value, zero]);
    var table = new map<Tag, nint>{[lit] = 1};
    var made = new slice<Tag>(2);

    var fresh = @new<Tag>();
    fresh.Value.Value = 9;

    var valueAddr = ptr.of(main_package.Tag.ᏑValue);
    valueAddr.Value += 4;

    var idAddr = Ꮡc.Ꮡtag.Ꮡid;
    idAddr.Value = 7;

    var (_, isTag) = v._<Tag>(ᐧ);
    var (_, isPtr) = v._<ж<Tag>>(ᐧ);

    @string kind = "other";

    switch (v.type()) {
    case Tag _: {
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

} partial class main_package {

internal static T pick<T>(slice<T> s, nint i) { return s[i]; }

internal static void forms() {
    var c = Ꮡ(new Chest(tags: [new(id: 2)]));
    fmt.Println("forms", c.Forms(new Tag()), c.Forms(Ꮡ(new Tag())), c.Forms((nint)1), c.Tag().id, len(c.tags));
}

} // end main_package
