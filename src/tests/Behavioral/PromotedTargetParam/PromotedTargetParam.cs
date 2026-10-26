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
    /*embed*/ public Base Base;
}

partial struct ByPointer {
    /*embed*/ public ж<Base> Base;
}

partial struct ForeignByValue {
    /*embed*/ public PromotedTargetParam.targetlib_package.Finder Finder;
}

partial struct ForeignByPointer {
    /*embed*/ public ж<PromotedTargetParam.targetlib_package.Finder> Finder;
}

internal static void Main() {
    var v = new ByValue(new Base("value-recv"));
    fmt.Println(v.Pair("param"));
    v.Base.Rename("value-renamed");
    fmt.Println(v.Pair("param"));

    var p = new ByPointer(Ꮡ(new Base("pointer-recv")));
    fmt.Println(p.Pair("param"));
    p.Rename("pointer-renamed");
    fmt.Println(p.Pair("param"));

    var fv = new ForeignByValue(new targetlib.Finder(Name: "foreign-value"));
    fmt.Println(fv.Finder.Match("param"));
    fv.Finder.Retarget("foreign-value-renamed");
    fmt.Println(fv.Finder.Match("param"));

    var fp = new ForeignByPointer(Ꮡ(new targetlib.Finder(Name: "foreign-pointer")));
    fmt.Println(fp.Finder.Value.Match("param"));
    fp.Finder.Value.Retarget("foreign-pointer-renamed");
    fmt.Println(fp.Finder.Value.Match("param"));
}

} // end main_package
