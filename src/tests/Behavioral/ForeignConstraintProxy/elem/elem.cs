namespace go.ForeignConstraintProxy;

using static elem_package;

partial class elem_package {

partial struct Node {
    public nint K;
}

} partial class elemᴛNode {

public static bool Less(this ref Node n, ж<Node> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return n.K < o.K;
}

} // end elemᴛNode
