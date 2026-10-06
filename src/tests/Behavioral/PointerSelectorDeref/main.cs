namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct header {
    internal uintptr tag;
    internal nint n;
}

[GoType] partial struct view {
    internal uintptr tag;
    internal nint n;
}

internal static partial ж<view> viewOf(ж<header> Ꮡhp) {
    return Ꮡhp.Reinterpret<header, view>();
}

internal static partial nint readN(ж<view> Ꮡp) {
    return (~viewOf(Ꮡp.Reinterpret<view, header>())).n;
}

internal static void Main() {
    ref var h = ref heap<view>(out var Ꮡh);
    h = new view(tag: 7, n: 5);
    fmt.Println(readN(Ꮡh));
    ref var g = ref heap<header>(out var Ꮡg);
    g = new header(tag: 9, n: 3);
    nint v = ((Ꮡg.Reinterpret<header, view>())).Value.n;
    fmt.Println(v);
}

} // end main_package
