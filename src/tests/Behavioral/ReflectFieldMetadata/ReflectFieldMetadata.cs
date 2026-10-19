namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using fieldlib = ReflectFieldMetadata.fieldlib_package;
using ReflectFieldMetadata;

partial class main_package {

partial struct sElemᴛ1 /*dyn*/ {
    public nint C;
}

partial struct sElem /*[]sElemᴛ1*/;

partial struct embeds {
    /*embed*/ internal sElem sElem;
}

partial struct holds {
    internal sElem f;
}

partial struct @base {
    public nint B;
}

internal static nint M(this @base b) {
    return b.B;
}

partial struct viaX {
    /*embed*/ internal @base @base;
}

partial struct viaY {
    /*embed*/ internal @base @base;
}

partial struct twice {
    /*embed*/ internal viaX viaX;
    /*embed*/ internal ж<viaY> viaY;
    public nint D;
}

partial struct once {
    /*embed*/ internal viaX viaX;
    public nint D;
}

partial struct deeper {
    /*embed*/ internal twice twice;
}

partial struct local /*global::go.ReflectFieldMetadata.fieldlib_package.Outer*/;

partial struct myInt /*num:nint*/;

partial struct embedsInt {
    /*embed*/ internal nint @int;
}

partial struct embedsIntPtr {
    /*embed*/ internal ж<nint> @int;
}

partial struct holdsNamed {
    internal myInt n;
}

partial struct embedsNamed {
    /*embed*/ internal myInt myInt;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object canSetViaEmbeddedˢ = (@string)"CanSet via embedded  :"u8;
private static readonly object canSetViaUnexportedˢ = (@string)"CanSet via unexported:"u8;
private static readonly object canSetViaSliceˢ = (@string)"CanSet via Slice     :"u8;
private static readonly object twiceBFoundˢ = (@string)"twice.B  found:"u8;
private static readonly object onceBFoundˢ = (@string)"once.B   found:"u8;
private static readonly object indexˢ = (@string)"index:"u8;
private static readonly object twiceDFoundˢ = (@string)"twice.D  found:"u8;
private static readonly object twiceBValueValidˢ = (@string)"twice.B  value valid:"u8;
private static readonly object deeperBFoundˢ = (@string)"deeper.B found:"u8;
private static readonly object valueValidˢ = (@string)" value valid:"u8;
private static readonly object deeperDFoundˢ = (@string)"deeper.D found:"u8;
private static readonly object valueˢ = (@string)" value:"u8;
private static readonly object mPromotedTwiceˢ = (@string)"M promoted -- twice:"u8;
private static readonly object deeperˢ = (@string)" deeper:"u8;
private static readonly object onceˢ = (@string)" once:"u8;
private static readonly object localField0Exportedˢ = (@string)"local.Field(0) exported, PkgPath empty:"u8;
private static readonly object localField1Exportedˢ = (@string)"local.Field(1) exported:"u8;
private static readonly object localField1PkgPathOuterˢ = (@string)"local.Field(1) PkgPath == Outer.Field(1) PkgPath:"u8;
private static readonly object localField1PkgPathIsˢ = (@string)"local.Field(1) PkgPath is foreign (not this package, not empty):"u8;
private static readonly object anonUnexportedPkgPathIsˢ = (@string)"anon unexported PkgPath is this package:"u8;

internal partial struct main_i /*dyn*/ {
    internal nint u;
}

internal static void Main() {
    fmt.Println(canSetViaEmbeddedˢ, reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println(canSetViaUnexportedˢ, reflect.ValueOf(new holds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println(canSetViaSliceˢ, reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Slice(0, 1).Index(0).Field(0).CanSet());

    var (_, foundTwice) = reflect.TypeOf(new twice(nil)).FieldByName("B");
    var (fOnce, foundOnce) = reflect.TypeOf(new once(nil)).FieldByName("B");
    var (fD, foundD) = reflect.TypeOf(new twice(nil)).FieldByName("D");
    fmt.Println(twiceBFoundˢ, foundTwice);
    fmt.Println(onceBFoundˢ, foundOnce, indexˢ, fOnce.Index);
    fmt.Println(twiceDFoundˢ, foundD, indexˢ, fD.Index);
    fmt.Println(twiceBValueValidˢ, reflect.ValueOf(new twice(nil)).FieldByName("B").IsValid());
    var (_, foundDeeperB) = reflect.TypeOf(new deeper(nil)).FieldByName("B");
    var (fDeeperD, foundDeeperD) = reflect.TypeOf(new deeper(nil)).FieldByName("D");
    fmt.Println(deeperBFoundˢ, foundDeeperB, valueValidˢ, reflect.ValueOf(new deeper(nil)).FieldByName("B").IsValid());
    fmt.Println(deeperDFoundˢ, foundDeeperD, indexˢ, fDeeperD.Index, valueˢ, reflect.ValueOf(new deeper(nil)).FieldByName("D").IsValid());

    var (_, mTwice) = reflect.TypeOf(new twice(nil)).MethodByName("M");
    var (_, mDeeper) = reflect.TypeOf(new deeper(nil)).MethodByName("M");
    var (_, mOnce) = reflect.TypeOf(new once(nil)).MethodByName("M");
    fmt.Println(mPromotedTwiceˢ, mTwice, deeperˢ, mDeeper, onceˢ, mOnce);

    var lt = reflect.TypeOf(new local(new fieldlib.Outer(nil)));
    var ot = reflect.TypeOf(new fieldlib.Outer(nil));
    @string mine = reflect.TypeOf(new main_i()).Field(0).PkgPath;
    fmt.Println(localField0Exportedˢ, lt.Field(0).IsExported(), lt.Field(0).PkgPath == ""u8);
    fmt.Println(localField1Exportedˢ, lt.Field(1).IsExported());
    fmt.Println(localField1PkgPathOuterˢ, lt.Field(1).PkgPath == ot.Field(1).PkgPath);
    fmt.Println(localField1PkgPathIsˢ, lt.Field(1).PkgPath != mine, lt.Field(1).PkgPath != ""u8);
    fmt.Println(anonUnexportedPkgPathIsˢ, mine == "main"u8);

    void anon(@string label, reflectꓸType t) {
        var f = t.Field(0);
        fmt.Printf("%-14s name=%-5s anonymous=%-5v type=%-6v pkgPath=%q\n"u8, label, f.Name, f.Anonymous, f.Type, f.PkgPath);
    }
    anon("struct{ int }", reflect.TypeOf(new embedsInt(nil)));
    anon("struct{ *int }", reflect.TypeOf(new embedsIntPtr(nil)));
    anon("struct{ n T }", reflect.TypeOf(new holdsNamed(nil)));
    anon("struct{ myInt }", reflect.TypeOf(new embedsNamed(nil)));
}

} // end main_package
