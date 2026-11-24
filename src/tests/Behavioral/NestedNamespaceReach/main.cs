namespace go;

using fmt = fmt_package;
using check = NestedNamespaceReach.b.x.check_package;
using lib = NestedNamespaceReach.lib_package;
using user = NestedNamespaceReach.user_package;
using NestedNamespaceReach;
using NestedNamespaceReach.b.x;

partial class main_package {

internal static void main() {
    ref var c = ref heap(new lib.Counter(), out var Ꮡc);
    Ꮡc.Inc();
    fmt.Println(Ꮡc.Inc(), lib.Tag());

    fmt.Println(check.Run());

    fmt.Println(user.Value());
}

} // end main_package
