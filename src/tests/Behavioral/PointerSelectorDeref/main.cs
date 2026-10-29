namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct header {
    internal uintptr tag;
    internal nint n;
}

partial struct view {
    internal uintptr tag;
    internal nint n;
}

internal static partial ж<view> viewOf(ж<header> hp) {
    return hp.Reinterpret<header, view>();
}

internal static partial nint readN(ж<view> p) {
    return viewOf(p.Reinterpret<view, header>()).n;
}

internal static void Main() {
    ref var h = ref heap<view>(new view(tag: 7, n: 5), out var Ꮡh);
    fmt.Println(readN(Ꮡh));

    ref var g = ref heap<header>(new header(tag: 9, n: 3), out var Ꮡg);
    nint v = ((Ꮡg.Reinterpret<header, view>())).Value.n;
    fmt.Println(v);
}

} // end main_package
