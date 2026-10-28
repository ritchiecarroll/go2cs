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

internal partial struct main_i /*dyn*/ {
    internal nint u;
}

internal static void Main() {
    fmt.Println("CanSet via embedded  :", reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println("CanSet via unexported:", reflect.ValueOf(new holds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Index(0).Field(0).CanSet());
    fmt.Println("CanSet via Slice     :", reflect.ValueOf(new embeds(new sElem(new sElemᴛ1[]{new()}.slice()))).Field(0).Slice(0, 1).Index(0).Field(0).CanSet());

    var (_, foundTwice) = reflect.TypeOf(new twice()).FieldByName("B");
    var (fOnce, foundOnce) = reflect.TypeOf(new once()).FieldByName("B");
    var (fD, foundD) = reflect.TypeOf(new twice()).FieldByName("D");
    fmt.Println("twice.B  found:", foundTwice);
    fmt.Println("once.B   found:", foundOnce, "index:", fOnce.Index);
    fmt.Println("twice.D  found:", foundD, "index:", fD.Index);
    fmt.Println("twice.B  value valid:", reflect.ValueOf(new twice()).FieldByName("B").IsValid());
    var (_, foundDeeperB) = reflect.TypeOf(new deeper()).FieldByName("B");
    var (fDeeperD, foundDeeperD) = reflect.TypeOf(new deeper()).FieldByName("D");
    fmt.Println("deeper.B found:", foundDeeperB, " value valid:", reflect.ValueOf(new deeper()).FieldByName("B").IsValid());
    fmt.Println("deeper.D found:", foundDeeperD, "index:", fDeeperD.Index, " value:", reflect.ValueOf(new deeper()).FieldByName("D").IsValid());

    var (_, mTwice) = reflect.TypeOf(new twice()).MethodByName("M");
    var (_, mDeeper) = reflect.TypeOf(new deeper()).MethodByName("M");
    var (_, mOnce) = reflect.TypeOf(new once()).MethodByName("M");
    fmt.Println("M promoted -- twice:", mTwice, " deeper:", mDeeper, " once:", mOnce);

    var lt = reflect.TypeOf(new local(new fieldlib.Outer()));
    var ot = reflect.TypeOf(new fieldlib.Outer());
    @string mine = reflect.TypeOf(new main_i()).Field(0).PkgPath;
    fmt.Println("local.Field(0) exported, PkgPath empty:", lt.Field(0).IsExported(), lt.Field(0).PkgPath == ""u8);
    fmt.Println("local.Field(1) exported:", lt.Field(1).IsExported());
    fmt.Println("local.Field(1) PkgPath == Outer.Field(1) PkgPath:", lt.Field(1).PkgPath == ot.Field(1).PkgPath);
    fmt.Println("local.Field(1) PkgPath is foreign (not this package, not empty):", lt.Field(1).PkgPath != mine, lt.Field(1).PkgPath != ""u8);
    fmt.Println("anon unexported PkgPath is this package:", mine == "main"u8);

    void anon(@string label, reflectꓸType t) {
        var f = t.Field(0);
        fmt.Printf("%-14s name=%-5s anonymous=%-5v type=%-6v pkgPath=%q\n"u8, label, f.Name, f.Anonymous, f.Type, f.PkgPath);
    }
    anon("struct{ int }", reflect.TypeOf(new embedsInt()));
    anon("struct{ *int }", reflect.TypeOf(new embedsIntPtr()));
    anon("struct{ n T }", reflect.TypeOf(new holdsNamed()));
    anon("struct{ myInt }", reflect.TypeOf(new embedsNamed()));
}

} // end main_package
