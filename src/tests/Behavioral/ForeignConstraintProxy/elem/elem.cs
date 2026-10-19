namespace go.ForeignConstraintProxy;

partial class elem_package {

partial struct Node {
    public nint K;
}

public static bool Less(this ref Node n, ж<Node> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    return n.K < o.K;
}

} // end elem_package
