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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "no panic"u8;

internal static @string /*msg*/ recovered(Action fn) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprint(r);
                }
            }
        }, ref ᒐ);
        fn();
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

internal static reflect.StructField field(@string name, reflectꓸType t) {
    return new reflect.StructField(Name: name, Type: t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object internedˢ = (@string)"interned:"u8;
private static readonly object usableAsAMapKeyˢ = (@string)"| usable as a map key:"u8;
private static readonly object byArrayLengthˢ = (@string)"by array length:"u8;
private static readonly object byTagˢ = (@string)"| by tag:"u8;
private static readonly object byFieldNameˢ = (@string)"| by field name:"u8;
private static readonly object byFieldCountˢ = (@string)"| by field count:"u8;
private static readonly object lengthsˢ = (@string)"lengths:"u8;
private static readonly object sizesˢ = (@string)"| sizes:"u8;
private static readonly @string arrˢ = "Arr"u8;
private static readonly @string nestedˢ = "Nested"u8;
private static readonly object dimsTypeˢ = (@string)"dims type:"u8;
private static readonly object sizeˢ = (@string)"| size:"u8;
private static readonly object elemLensˢ = (@string)"elem lens:"u8;
private static readonly object zeroˢ = (@string)"zero:"u8;
private static readonly object deepequalToFreshˢ = (@string)"| deepequal to fresh:"u8;
private static readonly object taggedTypeˢ = (@string)"tagged type:"u8;
private static readonly @string jsonˢ = "json"u8;
private static readonly @string xmlˢ = "xml"u8;
private static readonly @string tailˢ = "Tail"u8;
private static readonly object embeddedTypeˢ = (@string)"embedded type:"u8;
private static readonly object embeddedFlagsˢ = (@string)"embedded flags:"u8;
private static readonly object namesˢ = (@string)"| names:"u8;
private static readonly object embedTypeˢ = (@string)"| embed type:"u8;
private static readonly object numfieldˢ = (@string)"| numfield:"u8;
private static readonly object implementsStringerˢ = (@string)"implements Stringer:"u8;
private static readonly object implementsAnyˢ = (@string)"| implements any:"u8;
private static readonly object comparableˢ = (@string)"| comparable:"u8;
private static readonly object declaredStringerDoesˢ = (@string)"| declared stringer does:"u8;
private static readonly object nestedStructsˢ = (@string)"nested structs:"u8;
private static readonly object noNameˢ = (@string)"no name:"u8;
private static readonly object invalidNameˢ = (@string)"invalid name:"u8;
private static readonly object noTypeˢ = (@string)"no type:"u8;
private static readonly object duplicateˢ = (@string)"duplicate:"u8;
private static readonly object unexportedNoPkgpathˢ = (@string)"unexported no pkgpath:"u8;
private static readonly object anonymousWithPkgpathˢ = (@string)"anonymous with pkgpath:"u8;

internal partial struct main_i /*dyn*/ {
    public int64 Q;
}

internal static void Main() {
    var intT = reflect.TypeOf((int64)0);
    var byteT = reflect.TypeOf((uint8)0);
    var strT = reflect.TypeOf((@string)""u8);
    var shape = new reflect.StructField[]{field("A"u8, intT), field("B"u8, strT)}.slice();
    var first = reflect.StructOf(shape);
    var second = reflect.StructOf(new reflect.StructField[]{field("A"u8, intT), field("B"u8, strT)}.slice());
    fmt.Println(internedˢ, AreEqual(first, second), usableAsAMapKeyˢ,
        new map<reflectꓸType, nint>{[first] = 1}[second] == 1);
    fmt.Printf("type: %v | kind=%v | name=%q | pkgpath=%q | numfield=%d | size=%d | align=%d\n"u8,
        first, first.Kind(), first.Name(), first.PkgPath(), first.NumField(), first.Size(), first.Align());
    var one = reflect.StructOf(new reflect.StructField[]{field("F"u8, reflect.ArrayOf(1, intT))}.slice());
    var two = reflect.StructOf(new reflect.StructField[]{field("F"u8, reflect.ArrayOf(2, intT))}.slice());
    var tagged = reflect.StructOf(new reflect.StructField[]{new(Name: "A"u8, Type: intT, Tag: @"json:""a"""u8)}.slice());
    var renamed = reflect.StructOf(new reflect.StructField[]{field("Z"u8, intT), field("B"u8, strT)}.slice());
    fmt.Println(byArrayLengthˢ, !AreEqual(one, two), byTagˢ, !AreEqual(tagged, reflect.StructOf(shape[..1])),
        byFieldNameˢ, !AreEqual(renamed, first), byFieldCountˢ, !AreEqual(first, reflect.StructOf(shape[..1])));
    fmt.Println(lengthsˢ, one.Field(0).Type.Len(), two.Field(0).Type.Len(),
        sizesˢ, one.Size(), two.Size());
    var dims = reflect.StructOf(new reflect.StructField[]{
        field("N"u8, intT),
        field(arrˢ, reflect.ArrayOf(3, byteT)),
        field(nestedˢ, reflect.ArrayOf(2, reflect.ArrayOf(3, byteT)))
    }.slice());
    fmt.Println(dimsTypeˢ, dims, sizeˢ, dims.Size());
    for (nint i = 0; i < dims.NumField(); i++) {
        var f = dims.Field(i);
        fmt.Printf("  field %d: name=%s type=%v offset=%d index=%v anonymous=%v tag=%q pkgpath=%q\n"u8,
            i, f.Name, f.Type, f.Offset, f.Index, f.Anonymous, f.Tag, f.PkgPath);
    }
    fmt.Println(elemLensˢ, dims.Field(1).Type.Len(), dims.Field(2).Type.Len(),
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
    fmt.Println(zeroˢ, reflect.Zero(dims).Interface(), deepequalToFreshˢ,
        reflect.DeepEqual(reflect.Zero(dims).Interface(), reflect.New(dims).Elem().Interface()));
    var tags = reflect.StructOf(new reflect.StructField[]{
        new(Name: "A"u8, Type: intT, Tag: @"json:""a,omitempty"""u8),
        new(Name: "B"u8, Type: strT, Tag: @"xml:""b"""u8),
        new(Name: "C"u8, Type: byteT)
    }.slice());
    fmt.Println(taggedTypeˢ, tags);
    fmt.Printf("tags: %q %q %q | json key=%q xml key=%q\n"u8,
        tags.Field(0).Tag, tags.Field(1).Tag, tags.Field(2).Tag,
        tags.Field(0).Tag.Get(jsonˢ), tags.Field(1).Tag.Get(xmlˢ));
    var embedType = reflect.TypeOf(new main_i());
    var embedded = reflect.StructOf(new reflect.StructField[]{
        new(Name: "Celsius"u8, Type: reflect.TypeOf(((Celsius)0D)), Anonymous: true),
        field(tailˢ, intT)
    }.slice());
    fmt.Println(embeddedTypeˢ, embedded);
    fmt.Println(embeddedFlagsˢ, embedded.Field(0).Anonymous, embedded.Field(1).Anonymous,
        namesˢ, embedded.Field(0).Name, embedded.Field(1).Name,
        embedTypeˢ, embedded.Field(0).Type, numfieldˢ, embedType.NumField());
    var unexported = reflect.StructOf(new reflect.StructField[]{
        new(Name: "Shown"u8, Type: intT),
        new(Name: "hidden"u8, Type: intT, PkgPath: "main"u8)
    }.slice());
    fmt.Printf("unexported: %v | field pkgpaths=%q,%q | exported=%v,%v\n"u8, unexported,
        unexported.Field(0).PkgPath, unexported.Field(1).PkgPath,
        unexported.Field(0).IsExported(), unexported.Field(1).IsExported());
    var stringerT = reflect.TypeOf(((ж<fmt.Stringer>)nil)).Elem();
    fmt.Println(implementsStringerˢ, first.Implements(stringerT),
        implementsAnyˢ, first.Implements(reflect.TypeOf(((ж<any>)nil)).Elem()),
        comparableˢ, first.Comparable(),
        declaredStringerDoesˢ, reflect.TypeOf(new stringer(nil)).Implements(stringerT));
    var deep = byteT;
    for (nint i = 0; i < 5; i++) {
        deep = reflect.StructOf(new reflect.StructField[]{field("N"u8, deep)}.slice());
    }
    fmt.Println(nestedStructsˢ, deep, sizeˢ, deep.Size());
    var intTʗ1 = intT;
    fmt.Println(noNameˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field(""u8, intTʗ1)}.slice());
    }));
    var intTʗ2 = intT;
    fmt.Println(invalidNameˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field("1bad"u8, intTʗ2)}.slice());
    }));
    fmt.Println(noTypeˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "A"u8)}.slice());
    }));
    var intTʗ3 = intT;
    var strTʗ1 = strT;
    fmt.Println(duplicateˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{field("A"u8, intTʗ3), field("A"u8, strTʗ1)}.slice());
    }));
    var intTʗ4 = intT;
    fmt.Println(unexportedNoPkgpathˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "lower"u8, Type: intTʗ4)}.slice());
    }));
    fmt.Println(anonymousWithPkgpathˢ, recovered(() => {
        reflect.StructOf(new reflect.StructField[]{new(Name: "Celsius"u8, Type: reflect.TypeOf(((Celsius)0D)), Anonymous: true, PkgPath: "main"u8)}.slice());
    }));
}

partial struct Celsius /*num:float64*/;

} // end main_package
