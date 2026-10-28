namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct Handle /*num:uintptr*/;

internal static partial uintptr addrOfNamed(ж<Handle> h) {
    return (uintptr)h;
}

internal static partial uintptr addrOfPtrPtr(ж<ж<uint16>> pp) {
    return (uintptr)pp;
}

internal static partial uintptr addrOfBasic(ж<uint16> b) {
    return (uintptr)b;
}

internal static partial (uintptr, Handle) liveAlias(ж<Handle> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    h = h + 7;
    return ((uintptr)Ꮡh, h);
}

partial struct node {
    internal nint id;
    internal ж<node> parent;
}

internal static partial ж<node> newNode(nint id, ж<node> parent) {
    return Ꮡ(new node(id: id, parent: parent));
}

internal static partial nint depth(ref node n) {
    nint d = 0;
    for (var p = n.parent; p != nil; p = p.Value.parent) {
        d++;
    }
    return d;
}

internal static void Main() {
    fmt.Println("named nil  :", addrOfNamed(nil));
    fmt.Println("ptrptr nil :", addrOfPtrPtr(nil));
    fmt.Println("basic nil  :", addrOfBasic(nil));

    ref var h = ref heap(new Handle(), out var Ꮡh);
    h = 3;
    fmt.Println("named nonnil nonzero :", addrOfNamed(Ꮡh) != 0);

    ref var u = ref heap(new uint16(), out var Ꮡu);
    u = 9;
    ref var pu = ref heap<ж<uint16>>(out var Ꮡpu);
    pu = Ꮡu;
    fmt.Println("ptrptr nonnil nonzero:", addrOfPtrPtr(Ꮡpu) != 0);
    fmt.Println("basic nonnil nonzero :", addrOfBasic(Ꮡu) != 0);

    var (addr, val) = liveAlias(Ꮡh);
    fmt.Println("live alias nonzero:", addr != 0, "value:", val, "readback:", h);

    var root = newNode(1, nil);
    var child = newNode(2, root);
    var grand = newNode(3, child);
    fmt.Println("named-arg label:", (~root).id, (~child).id, (~grand).id);
    fmt.Println("depths         :", depth(ref (root).DerefOrNull()), depth(ref (child).DerefOrNull()), depth(ref (grand).DerefOrNull()));
    fmt.Println("links          :", (~root).parent == nil, (~child).parent == root, (~grand).parent == child);
}

} // end main_package
