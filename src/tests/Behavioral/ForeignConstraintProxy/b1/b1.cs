namespace go.ForeignConstraintProxy;

using elem = go.ForeignConstraintProxy.elem_package;
using lib = go.ForeignConstraintProxy.lib_package;
using go.ForeignConstraintProxy;

partial class b1_package {

public static lib.Ord2<lib.жLesser<elem.Node>> Make(nint k) {
    return new lib.Ord2<lib.жLesser<elem.Node>>(V: Ꮡ(new elem.Node(K: k)));
}

public static lib.Ord<lib.жOrd_T<elem.Node>> MakeInline(nint k) {
    return new lib.Ord<lib.жOrd_T<elem.Node>>(V: Ꮡ(new elem.Node(K: k)));
}

} // end b1_package
