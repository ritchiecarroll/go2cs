namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using fieldlib = ReflectFieldMetadata.fieldlib_package;
using ReflectFieldMetadata;

partial class main_package {

[GoType("dyn")] partial struct sElemᴛ1 {
    public nint C;
}

[GoType("[]sElemᴛ1")] partial struct sElem;

[GoType] partial struct embeds {
    internal partial ref sElem sElem { get; }
}

[GoType] partial struct holds {
    internal sElem f;
}

[GoType] partial struct @base {
    public nint B;
}

internal static nint M(this @base b) {
    return b.B;
}

[GoType] partial struct viaX {
    internal partial ref @base @base { get; }
}

[GoType] partial struct viaY {
    internal partial ref @base @base { get; }
}

[GoType] partial struct twice {
    internal partial ref viaX viaX { get; }
    internal partial ref ж<viaY> viaY { get; }
    public nint D;
}

[GoType] partial struct once {
    internal partial ref viaX viaX { get; }
    public nint D;
}

[GoType] partial struct deeper {
    internal partial ref twice twice { get; }
}

[GoType("ReflectFieldMetadata.fieldlib_package.Outer")] partial struct local;

[GoType("num:nint")] partial struct myInt;

[GoType] partial struct embedsInt {
    [GoEmbedded] internal nint @int;
}

[GoType] partial struct embedsIntPtr {
    [GoEmbedded] internal ж<nint> @int;
}

[GoType] partial struct holdsNamed {
    internal myInt n;
}

[GoType] partial struct embedsNamed {
    internal partial ref myInt myInt { get; }
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
private static readonly @string structIntˢ = "struct{ int }"u8;
private static readonly @string structIntˢ2 = "struct{ *int }"u8;
private static readonly @string structNTˢ = "struct{ n T }"u8;
private static readonly @string structMyIntˢ = "struct{ myInt }"u8;

[GoType("dyn")] internal partial struct main_i {
    internal nint u;
}

internal static void Main() {
    fmt.Println(canSetViaEmbeddedˢ, reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println(canSetViaUnexportedˢ, reflect.ValueOf(new holds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println(canSetViaSliceˢ, reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Slice(0, 1).Index(0).Field(0).CanSet());
    var (_, foundTwice) = reflect.TypeOf(new twice(nil)).FieldByName("B"u8);
    var (fOnce, foundOnce) = reflect.TypeOf(new once(nil)).FieldByName("B"u8);
    var (fD, foundD) = reflect.TypeOf(new twice(nil)).FieldByName("D"u8);
    fmt.Println(twiceBFoundˢ, foundTwice);
    fmt.Println(onceBFoundˢ, foundOnce, indexˢ, fOnce.Index);
    fmt.Println(twiceDFoundˢ, foundD, indexˢ, fD.Index);
    fmt.Println(twiceBValueValidˢ, reflect.ValueOf(new twice(nil)).FieldByName("B"u8).IsValid());
    var (_, foundDeeperB) = reflect.TypeOf(new deeper(nil)).FieldByName("B"u8);
    var (fDeeperD, foundDeeperD) = reflect.TypeOf(new deeper(nil)).FieldByName("D"u8);
    fmt.Println(deeperBFoundˢ, foundDeeperB, valueValidˢ, reflect.ValueOf(new deeper(nil)).FieldByName("B"u8).IsValid());
    fmt.Println(deeperDFoundˢ, foundDeeperD, indexˢ, fDeeperD.Index, valueˢ, reflect.ValueOf(new deeper(nil)).FieldByName("D"u8).IsValid());
    var (_, mTwice) = reflect.TypeOf(new twice(nil)).MethodByName("M"u8);
    var (_, mDeeper) = reflect.TypeOf(new deeper(nil)).MethodByName("M"u8);
    var (_, mOnce) = reflect.TypeOf(new once(nil)).MethodByName("M"u8);
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
    anon(structIntˢ, reflect.TypeOf(new embedsInt(nil)));
    anon(structIntˢ2, reflect.TypeOf(new embedsIntPtr(nil)));
    anon(structNTˢ, reflect.TypeOf(new holdsNamed(nil)));
    anon(structMyIntˢ, reflect.TypeOf(new embedsNamed(nil)));
}

} // end main_package
