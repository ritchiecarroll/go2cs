namespace go;

using fmt = fmt_package;
using targetlib = PromotedTargetParam.targetlib_package;
using PromotedTargetParam;

partial class main_package {

partial struct Base {
    internal @string name;
}

public static @string Pair(this Base b, @string target) {
    return b.name + "->"u8 + target;
}

public static void Rename(this ref Base b, @string target) {
    b.name = target;
}

partial struct ByValue {
    public partial ref Base Base { get; }
}

partial struct ByPointer {
    public partial ref ж<Base> Base { get; }
}

partial struct ForeignByValue {
    public partial ref PromotedTargetParam.targetlib_package.Finder Finder { get; }
}

partial struct ForeignByPointer {
    public partial ref ж<PromotedTargetParam.targetlib_package.Finder> Finder { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string paramˢ = "param"u8;
private static readonly @string valueRenamedˢ = "value-renamed"u8;
private static readonly @string pointerRenamedˢ = "pointer-renamed"u8;
private static readonly @string foreignValueRenamedˢ = "foreign-value-renamed"u8;
private static readonly @string foreignPointerRenamedˢ = "foreign-pointer-renamed"u8;

internal static void Main() {
    var v = new ByValue(new Base("value-recv"u8));
    fmt.Println(v.Pair(paramˢ));
    v.Base.Rename(valueRenamedˢ);
    fmt.Println(v.Pair(paramˢ));
    var p = new ByPointer(Ꮡ(new Base("pointer-recv"u8)));
    fmt.Println(p.Pair(paramˢ));
    p.Rename(pointerRenamedˢ);
    fmt.Println(p.Pair(paramˢ));
    var fv = new ForeignByValue(new targetlib.Finder(Name: "foreign-value"u8));
    fmt.Println(fv.Finder.Match(paramˢ));
    fv.Finder.Retarget(foreignValueRenamedˢ);
    fmt.Println(fv.Finder.Match(paramˢ));
    var fp = new ForeignByPointer(Ꮡ(new targetlib.Finder(Name: "foreign-pointer"u8)));
    fmt.Println(fp.Finder.Value.Match(paramˢ));
    fp.Finder.Value.Retarget(foreignPointerRenamedˢ);
    fmt.Println(fp.Finder.Value.Match(paramˢ));
}

} // end main_package
