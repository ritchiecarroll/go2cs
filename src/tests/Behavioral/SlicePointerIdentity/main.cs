namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal nint id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object selfˢ = (@string)"self       :"u8;
private static readonly object selfMidˢ = (@string)"self mid   :"u8;
private static readonly object distinctˢ = (@string)"distinct   :"u8;
private static readonly object resliceˢ = (@string)"reslice    :"u8;
private static readonly object resliceXˢ = (@string)"reslice x  :"u8;
private static readonly object reslice2ˢ = (@string)"reslice 2  :"u8;
private static readonly object reslice2ˢ2 = (@string)"reslice^2  :"u8;
private static readonly object appendSameˢ = (@string)"append same:"u8;
private static readonly object arrSelfˢ = (@string)"arr self   :"u8;
private static readonly object arrVsSlcˢ = (@string)"arr vs slc :"u8;
private static readonly object arrVsSlc2ˢ = (@string)"arr vs slc2:"u8;
private static readonly object structElemˢ = (@string)"struct elem:"u8;
private static readonly object writeThruˢ = (@string)"write thru :"u8;
private static readonly @string twoˢ = "two"u8;
private static readonly @string threeˢ = "three"u8;
private static readonly object mapLookupˢ = (@string)"map lookup :"u8;
private static readonly object mapLenˢ = (@string)"map len    :"u8;
private static readonly @string twoˢ2 = "TWO"u8;
private static readonly object mapLen2ˢ = (@string)"map len 2  :"u8;
private static readonly object mapResliceˢ = (@string)"map reslice:"u8;
private static readonly object mapMissˢ = (@string)"map miss   :"u8;
private static readonly object otherBackˢ = (@string)"other back :"u8;

internal static void Main() {
    var s = new slice<nint>(4, 8);
    foreach (var (i, _) in s) {
        s[i] = i * 10;
    }
    fmt.Println(selfˢ, Ꮡ(s, 0) == Ꮡ(s, 0));
    fmt.Println(selfMidˢ, Ꮡ(s, 2) == Ꮡ(s, 2));
    fmt.Println(distinctˢ, Ꮡ(s, 0) == Ꮡ(s, 1));
    var t = s[1..];
    fmt.Println(resliceˢ, Ꮡ(t, 0) == Ꮡ(s, 1));
    fmt.Println(resliceXˢ, Ꮡ(t, 0) == Ꮡ(s, 0));
    fmt.Println(reslice2ˢ, Ꮡ(t, 2) == Ꮡ(s, 3));
    var u = t[1..];
    fmt.Println(reslice2ˢ2, Ꮡ(u, 0) == Ꮡ(s, 2));
    var w = append(s.slice(0, 0, 1), (nint)(99));
    fmt.Println(appendSameˢ, Ꮡ(w, 0) == Ꮡ(s, 0));
    ref var a = ref heap(new array<nint>(4), out var Ꮡa);
    var v = a[..];
    fmt.Println(arrSelfˢ, Ꮡa.at<nint>(1) == Ꮡa.at<nint>(1));
    fmt.Println(arrVsSlcˢ, Ꮡ(v, 2) == Ꮡa.at<nint>(2));
    fmt.Println(arrVsSlc2ˢ, Ꮡ(v, 0) == Ꮡa.at<nint>(1));
    var ns = new slice<node>(3);
    fmt.Println(structElemˢ, Ꮡ(ns, 1) == Ꮡ(ns, 1), Ꮡ(ns, 1) == Ꮡ(ns, 2));
    var p = Ꮡ(s, 1);
    p.Value = 42;
    fmt.Println(writeThruˢ, s[1], t[0]);
    var m = new map<ж<nint>, @string>{};
    m[Ꮡ(s, 2)] = twoˢ;
    m[Ꮡ(s, 3)] = threeˢ;
    fmt.Println(mapLookupˢ, m[Ꮡ(s, 2)], m[Ꮡ(s, 3)]);
    fmt.Println(mapLenˢ, len(m));
    m[Ꮡ(s, 2)] = twoˢ2;
    fmt.Println(mapLen2ˢ, len(m));
    fmt.Println(mapResliceˢ, m[Ꮡ(t, 1)], m[Ꮡ(t, 2)]);
    var (_, miss) = m[Ꮡ(s, 0), ꟷ];
    fmt.Println(mapMissˢ, miss);
    var z = new slice<nint>(4);
    fmt.Println(otherBackˢ, Ꮡ(z, 0) == Ꮡ(s, 0));
}

} // end main_package
