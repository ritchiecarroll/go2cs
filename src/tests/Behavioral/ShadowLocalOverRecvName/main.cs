namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Inner {
    internal nint n;
}

[GoType] partial struct Thing {
    internal nint val;
    internal Inner inner;
}

[GoType] partial interface HasName {
    @string Name();
}

[GoType] partial struct Tag {
    internal @string tag;
}

public static @string Name(this Tag g) {
    return g.tag;
}

[GoType] partial struct Box {
    [GoEmbedded] public HasName HasName;
    internal nint id;
}

[GoRecv] internal static nint n(this ref Thing t) {
    return t.val;
}

public static nint Sum(this Thing t) {
    return t.val + t.inner.n;
}

[GoRecv] public static nint PtrShadowCall(this ref Thing t, ж<Thing> Ꮡother) {
    nint sum = t.Sum();
    {
        var tΔ1 = Ꮡother;
        sum += (~tΔ1).Sum();
    }
    return sum;
}

[GoRecv] public static nint PtrShadowChain(this ref Thing t, ж<Thing> Ꮡother) {
    t.inner.n = 1;
    {
        var tΔ1 = Ꮡother;
        var q = tΔ1.of(Thing.Ꮡinner).of(Inner.Ꮡn);
        q.Value = 77;
    }
    return t.inner.n;
}

public static nint ValShadowField(this ж<Thing> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.val = 2;
    nint got = 0;
    {
        ref var tΔ1 = ref heap<Thing>(out var ᏑtΔ1);
        tΔ1 = new Thing(val: 7);
        var q = ᏑtΔ1;
        var p = ᏑtΔ1.of(Thing.Ꮡval);
        p.Value = 99;
        got = q.n();
    }
    return got + t.val;
}

[GoRecv] public static nint SliceShadowIndex(this ref Thing t) {
    {
        var tΔ1 = new nint[]{10, 20, 30}.slice();
        var p = Ꮡ(tΔ1, 1);
        p.Value = 55;
        return tΔ1[1];
    }
}

[GoRecv] public static @string IfaceShadowEmbed(this ref Box b, ж<Box> Ꮡother) {
    @string name = b.HasName.Name();
    {
        var bΔ1 = Ꮡother;
        name += (~bΔ1).HasName.Name();
    }
    return name;
}

internal static void Main() {
    var recv = Ꮡ(new Thing(val: 5, inner: new Inner(n: 3)));
    var other = Ꮡ(new Thing(val: 4, inner: new Inner(n: 2)));
    fmt.Println(recv.PtrShadowCall(other));
    fmt.Println(recv.PtrShadowChain(other));
    fmt.Println((~other).inner.n);
    fmt.Println(recv.ValShadowField());
    fmt.Println(recv.SliceShadowIndex());
    var box = Ꮡ(new Box(HasName: new Tag(tag: "R"u8), id: 1));
    var obox = Ꮡ(new Box(HasName: new Tag(tag: "S"u8), id: 2));
    fmt.Println(box.IfaceShadowEmbed(obox));
}

} // end main_package
