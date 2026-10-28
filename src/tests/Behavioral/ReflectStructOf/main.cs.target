namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct stringer {
    internal nint n;
}

internal static @string String(this stringer s) {
    return fmt.Sprint(s.n);
}

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = "no panic";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

internal static reflect.StructField field(@string name, reflectꓸType t) {
    return new reflect.StructField(Name: name, Type: t);
}

internal partial struct main_i /*dyn*/ {
    public int64 Q;
}

internal static void Main() {
    var intT = reflect.TypeOf((int64)0);
    var byteT = reflect.TypeOf((uint8)0);
    var strT = reflect.TypeOf("");

    var shape = new reflect.StructField[]{field("A", intT), field("B", strT)}.slice();
    var first = reflect.StructOf(shape);
    var second = reflect.StructOf(new reflect.StructField[]{field("A", intT), field("B", strT)}.slice());
    fmt.Println("interned:", AreEqual(first, second), "| usable as a map key:",
        new map<reflectꓸType, nint>{[first] = 1}[second] == 1);

    fmt.Printf("type: %v | kind=%v | name=%q | pkgpath=%q | numfield=%d | size=%d | align=%d\n"u8,
        first, first.Kind(), first.Name(), first.PkgPath(), first.NumField(), first.Size(), first.Align());

    var one = reflect.StructOf(new reflect.StructField[]{field("F", reflect.ArrayOf(1, intT))}.slice());
    var two = reflect.StructOf(new reflect.StructField[]{field("F", reflect.ArrayOf(2, intT))}.slice());
    var tagged = reflect.StructOf(new reflect.StructField[]{new(Name: "A", Type: intT, Tag: @"json:""a"""u8)}.slice());
    var renamed = reflect.StructOf(new reflect.StructField[]{field("Z", intT), field("B", strT)}.slice());
    fmt.Println("by array length:", !AreEqual(one, two), "| by tag:", !AreEqual(tagged, reflect.StructOf(shape[..1])),
        "| by field name:", !AreEqual(renamed, first), "| by field count:", !AreEqual(first, reflect.StructOf(shape[..1])));
    fmt.Println("lengths:", one.Field(0).Type.Len(), two.Field(0).Type.Len(),
        "| sizes:", one.Size(), two.Size());

    var dims = reflect.StructOf(new reflect.StructField[]{
        field("N", intT),
        field("Arr", reflect.ArrayOf(3, byteT)),
        field("Nested", reflect.ArrayOf(2, reflect.ArrayOf(3, byteT)))
    }.slice());
    fmt.Println("dims type:", dims, "| size:", dims.Size());
    for (nint i = 0; i < dims.NumField(); i++) {
        var f = dims.Field(i);
        fmt.Printf("  field %d: name=%s type=%v offset=%d index=%v anonymous=%v tag=%q pkgpath=%q\n"u8,
            i, f.Name, f.Type, f.Offset, f.Index, f.Anonymous, f.Tag, f.PkgPath);
    }
    fmt.Println("elem lens:", dims.Field(1).Type.Len(), dims.Field(2).Type.Len(),
        dims.Field(2).Type.Elem().Len());

    var v = reflect.New(dims).Elem();
    v.Field(0).SetInt(42);
    for (nint i = 0; i < 3; i++) {
        v.Field(1).Index(i).SetUint((uint64)(10 * (i + 1)));
    }
    v.Field(2).Index(1).Index(2).SetUint(9);
    fmt.Printf("value: %v | type=%v | field0=%d | arr2=%d | nested=%d\n"u8,
        v.Interface(), v.Type(), v.Field(0).Int(), v.Field(1).Index(2).Uint(),
        v.Field(2).Index(1).Index(2).Uint());
    fmt.Println("zero:", reflect.Zero(dims).Interface(), "| deepequal to fresh:",
        reflect.DeepEqual(reflect.Zero(dims).Interface(), reflect.New(dims).Elem().Interface()));

    var tags = reflect.StructOf(new reflect.StructField[]{
        new(Name: "A", Type: intT, Tag: @"json:""a,omitempty"""u8),
        new(Name: "B", Type: strT, Tag: @"xml:""b"""u8),
        new(Name: "C", Type: byteT)
    }.slice());
    fmt.Println("tagged type:", tags);
    fmt.Printf("tags: %q %q %q | json key=%q xml key=%q\n"u8,
        tags.Field(0).Tag, tags.Field(1).Tag, tags.Field(2).Tag,
        tags.Field(0).Tag.Get("json"), tags.Field(1).Tag.Get("xml"));

    var embedType = reflect.TypeOf(new main_i());
    var embedded = reflect.StructOf(new reflect.StructField[]{
        new(Name: "Celsius", Type: reflect.TypeOf((Celsius)0D), Anonymous: true),
        field("Tail", intT)
    }.slice());
    fmt.Println("embedded type:", embedded);
    fmt.Println("embedded flags:", embedded.Field(0).Anonymous, embedded.Field(1).Anonymous,
        "| names:", embedded.Field(0).Name, embedded.Field(1).Name,
        "| embed type:", embedded.Field(0).Type, "| numfield:", embedType.NumField());

    var unexported = reflect.StructOf(new reflect.StructField[]{
        new(Name: "Shown", Type: intT),
        new(Name: "hidden", Type: intT, PkgPath: "main")
    }.slice());
    fmt.Printf("unexported: %v | field pkgpaths=%q,%q | exported=%v,%v\n"u8, unexported,
        unexported.Field(0).PkgPath, unexported.Field(1).PkgPath,
        unexported.Field(0).IsExported(), unexported.Field(1).IsExported());

    var stringerT = reflect.TypeOf((ж<fmt.Stringer>)nil).Elem();
    fmt.Println("implements Stringer:", first.Implements(stringerT),
        "| implements any:", first.Implements(reflect.TypeOf((ж<any>)nil).Elem()),
        "| comparable:", first.Comparable(),
        "| declared stringer does:", reflect.TypeOf(new stringer()).Implements(stringerT));

    var deep = byteT;
    for (nint i = 0; i < 5; i++) {
        deep = reflect.StructOf(new reflect.StructField[]{field("N", deep)}.slice());
    }
    fmt.Println("nested structs:", deep, "| size:", deep.Size());

    var intTʗ1 = intT;
    fmt.Println("no name:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field("", intTʗ1)}.slice());
    }));
    var intTʗ2 = intT;
    fmt.Println("invalid name:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field("1bad", intTʗ2)}.slice());
    }));
    fmt.Println("no type:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "A")}.slice());
    }));
    var intTʗ3 = intT;
    var strTʗ1 = strT;
    fmt.Println("duplicate:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field("A", intTʗ3), field("A", strTʗ1)}.slice());
    }));
    var intTʗ4 = intT;
    fmt.Println("unexported no pkgpath:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "lower", Type: intTʗ4)}.slice());
    }));
    fmt.Println("anonymous with pkgpath:", recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "Celsius", Type: reflect.TypeOf((Celsius)0D), Anonymous: true, PkgPath: "main")}.slice());
    }));
}

partial struct Celsius /*num:float64*/;

} // end main_package
