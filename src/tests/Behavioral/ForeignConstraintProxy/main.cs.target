namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using b1 = ForeignConstraintProxy.b1_package;
using elem = ForeignConstraintProxy.elem_package;
using lib = ForeignConstraintProxy.lib_package;
using ForeignConstraintProxy;

partial class main_package {

partial struct mnode {
    internal nint k;
}

internal static bool Less(this ref mnode n, ж<mnode> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return n.k < o.k;
}

partial struct age /*num:nint*/;

internal static bool Less(this age a, age b) {
    return a < b;
}

internal static void Main() {
    fmt.Println(new lib.Ord2<age>(V: 3).Below(5), new lib.Ord<age>(V: 3).Below(2));
    fmt.Println(new lib.Ord2<lib.жLesser<mnode>>(V: Ꮡ(new mnode(1))).Below(Ꮡ(new mnode(0))), new lib.Ord<lib.жOrd_T<mnode>>(V: Ꮡ(new mnode(1))).Below(Ꮡ(new mnode(4))));

    fmt.Println(new lib.Ord2<lib.жLesser<elem.Node>>(V: Ꮡ(new elem.Node(K: 1))).Below(Ꮡ(new elem.Node(K: 0))), new lib.Ord<lib.жOrd_T<elem.Node>>(V: Ꮡ(new elem.Node(K: 1))).Below(Ꮡ(new elem.Node(K: 4))));

    var v = b1.Make(2);
    lib.Ord2<lib.жLesser<elem.Node>> w = v;
    any a = v;
    var (x, ok) = a._<lib.Ord2<lib.жLesser<elem.Node>>>(ᐧ);
    fmt.Println(w.Below(Ꮡ(new elem.Node(K: 3))), ok, x.Below(Ꮡ(new elem.Node(K: 1))));
    fmt.Printf("%T\n"u8, v);
    fmt.Println(AreEqual(reflect.TypeOf(v), reflect.TypeOf(new lib.Ord2<lib.жLesser<elem.Node>>())));

    var vi = b1.MakeInline(2);
    lib.Ord<lib.жOrd_T<elem.Node>> wi = vi;
    any ai = vi;
    var (xi, oki) = ai._<lib.Ord<lib.жOrd_T<elem.Node>>>(ᐧ);
    fmt.Println(wi.Below(Ꮡ(new elem.Node(K: 3))), oki, xi.Below(Ꮡ(new elem.Node(K: 1))));
    fmt.Printf("%T\n"u8, vi);
    fmt.Println(AreEqual(reflect.TypeOf(vi), reflect.TypeOf(new lib.Ord<lib.жOrd_T<elem.Node>>())));

    var z = reflect.New(reflect.TypeOf(v)).Elem();
    z.Field(0).Set(reflect.ValueOf(Ꮡ(new elem.Node(K: 9))));
    fmt.Println(z.Interface()._<lib.Ord2<lib.жLesser<elem.Node>>>().Below(Ꮡ(new elem.Node(K: 1))), z.Field(0).Type(), z.Field(0).Kind());
    var zi = reflect.New(reflect.TypeOf(vi)).Elem();
    zi.Field(0).Set(reflect.ValueOf(Ꮡ(new elem.Node(K: 0))));
    fmt.Println(zi.Interface()._<lib.Ord<lib.жOrd_T<elem.Node>>>().Below(Ꮡ(new elem.Node(K: 1))), zi.Field(0).Type(), zi.Field(0).Elem().Field(0).Int());
}

} // end main_package
