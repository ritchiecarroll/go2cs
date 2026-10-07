namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType("num:uintptr")] partial struct Handle;

internal static partial uintptr addrOfNamed(ж<Handle> Ꮡh) {
    return (uintptr)Ꮡh;
}

internal static partial uintptr addrOfPtrPtr(ж<ж<uint16>> Ꮡpp) {
    return (uintptr)Ꮡpp;
}

internal static partial uintptr addrOfBasic(ж<uint16> Ꮡb) {
    return (uintptr)Ꮡb;
}

internal static partial (uintptr, Handle) liveAlias(ж<Handle> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    h = h + 7;
    return ((uintptr)Ꮡh, h);
}

[GoType] partial struct node {
    internal nint id;
    internal ж<node> parent;
}

internal static partial ж<node> newNode(nint id, ж<node> Ꮡparent) {
    return Ꮡ(new node(id: id, parent: Ꮡparent));
}

internal static partial nint depth(ref node n) {
    nint d = 0;
    for (var p = n.parent; p != nil; p = p.Value.parent) {
        d++;
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedNilˢ = (@string)"named nil  :"u8;
private static readonly object ptrptrNilˢ = (@string)"ptrptr nil :"u8;
private static readonly object basicNilˢ = (@string)"basic nil  :"u8;
private static readonly object namedNonnilNonzeroˢ = (@string)"named nonnil nonzero :"u8;
private static readonly object ptrptrNonnilNonzeroˢ = (@string)"ptrptr nonnil nonzero:"u8;
private static readonly object basicNonnilNonzeroˢ = (@string)"basic nonnil nonzero :"u8;
private static readonly object liveAliasNonzeroˢ = (@string)"live alias nonzero:"u8;
private static readonly object valueˢ = (@string)"value:"u8;
private static readonly object readbackˢ = (@string)"readback:"u8;
private static readonly object namedArgLabelˢ = (@string)"named-arg label:"u8;
private static readonly object depthsˢ = (@string)"depths         :"u8;
private static readonly object linksˢ = (@string)"links          :"u8;

internal static void Main() {
    fmt.Println(namedNilˢ, addrOfNamed(nil));
    fmt.Println(ptrptrNilˢ, addrOfPtrPtr(nil));
    fmt.Println(basicNilˢ, addrOfBasic(nil));
    ref var h = ref heap(new Handle(), out var Ꮡh);
    h = 3;
    fmt.Println(namedNonnilNonzeroˢ, addrOfNamed(Ꮡh) != 0);
    ref var u = ref heap(new uint16(), out var Ꮡu);
    u = 9;
    ref var pu = ref heap<ж<uint16>>(out var Ꮡpu);
    pu = Ꮡu;
    fmt.Println(ptrptrNonnilNonzeroˢ, addrOfPtrPtr(Ꮡpu) != 0);
    fmt.Println(basicNonnilNonzeroˢ, addrOfBasic(Ꮡu) != 0);
    var (addr, val) = liveAlias(Ꮡh);
    fmt.Println(liveAliasNonzeroˢ, addr != 0, valueˢ, val, readbackˢ, h);
    var root = newNode(1, nil);
    var child = newNode(2, root);
    var grand = newNode(3, child);
    fmt.Println(namedArgLabelˢ, (~root).id, (~child).id, (~grand).id);
    fmt.Println(depthsˢ, depth(ref (root).DerefOrNull()), depth(ref (child).DerefOrNull()), depth(ref (grand).DerefOrNull()));
    fmt.Println(linksˢ, (~root).parent == nil, (~child).parent == root, (~grand).parent == child);
}

} // end main_package
