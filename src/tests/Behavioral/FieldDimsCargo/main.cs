namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

[GoType("[3]nint")] partial struct Row;

[GoType] partial struct Target {
    [GoArrayDims(2), GoMapKeyDims(2)]
    public map<array<@string>, array<ж<float64>>> Marr;
    [GoArrayDims(3)]
    public ж<array<float64>> N;
    [GoArrayDims(3)]
    public ж<ж<ж<array<nint>>>> Deep;
    [GoArrayDims(5)]
    public map<@string, array<nint>> MapElem;
    [GoMapKeyDims(4)]
    public map<array<byte>, nint> MapKey;
    public array<byte> Plain = new(4);
    public array<array<nint>> Nested = new(2, () => new(3));
    public ж<Row> Named;
    [GoArrayDims(2)]
    public slice<array<nint>> SlcArr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fieldTypesˢ = (@string)"-- field types --"u8;
private static readonly object mapAccessorsˢ = (@string)"-- map accessors --"u8;
private static readonly object keyˢ = (@string)"key "u8;
private static readonly object elemˢ = (@string)"elem"u8;
private static readonly object decodeMapAllocationˢ = (@string)"-- decodeMap allocation --"u8;
private static readonly object newKeyLenˢ = (@string)"new key len "u8;
private static readonly object newElemLenˢ = (@string)"new elem len"u8;
private static readonly object pointerChainˢ = (@string)"-- pointer chain --"u8;
private static readonly object deepˢ = (@string)"deep      "u8;
private static readonly object deepElem3ˢ = (@string)"deep elem3"u8;
private static readonly object ptrElemˢ = (@string)"ptr elem  "u8;
private static readonly object decIndirectWalkˢ = (@string)"-- decIndirect walk --"u8;
private static readonly object landedOnˢ = (@string)"landed on"u8;
private static readonly object oneAccessorAtATimeˢ = (@string)"-- one accessor at a time --"u8;
private static readonly object elemOnlyKeyˢ = (@string)"elem-only key "u8;
private static readonly object elemOnlyElemˢ = (@string)"elem-only elem"u8;
private static readonly object keyOnlyKeyˢ = (@string)"key-only key  "u8;
private static readonly object keyOnlyElemˢ = (@string)"key-only elem "u8;
private static readonly object initializerRouteˢ = (@string)"-- initializer route (unchanged) --"u8;
private static readonly object plainˢ = (@string)"plain "u8;
private static readonly object nestedˢ = (@string)"nested"u8;
private static readonly object boundariesˢ = (@string)"-- boundaries --"u8;
private static readonly object namedPtrElemKindˢ = (@string)"named ptr elem kind"u8;
private static readonly object sliceElemKindˢ = (@string)"slice elem kind    "u8;

internal static void Main() {
    var t = reflect.TypeOf(new Target(nil));
    fmt.Println(fieldTypesˢ);
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        fmt.Printf("%-8s %s\n"u8, f.Name, f.Type);
    }
    var marr = t.Field(0).Type;
    fmt.Println(mapAccessorsˢ);
    fmt.Println(keyˢ, marr.Key(), marr.Key().Len());
    fmt.Println(elemˢ, marr.Elem(), marr.Elem().Len());
    fmt.Println(decodeMapAllocationˢ);
    fmt.Println(newKeyLenˢ, reflect.New(marr.Key()).Elem().Len());
    fmt.Println(newElemLenˢ, reflect.New(marr.Elem()).Elem().Len());
    var deep = t.Field(2).Type;
    fmt.Println(pointerChainˢ);
    fmt.Println(deepˢ, deep);
    fmt.Println(deepElem3ˢ, deep.Elem().Elem().Elem(), deep.Elem().Elem().Elem().Len());
    fmt.Println(ptrElemˢ, t.Field(1).Type.Elem(), t.Field(1).Type.Elem().Len());
    ref var target = ref heap(new Target(), out var Ꮡtarget);
    var walk = reflect.ValueOf(Ꮡtarget).Elem().Field(2);
    while (walk.Kind() == reflect.ΔPointer) {
        if (walk.IsNil()) {
            walk.Set(reflect.New(walk.Type().Elem()));
        }
        walk = walk.Elem();
    }
    fmt.Println(decIndirectWalkˢ);
    fmt.Println(landedOnˢ, walk.Type(), walk.Len(), walk.CanAddr());
    fmt.Println(oneAccessorAtATimeˢ);
    fmt.Println(elemOnlyKeyˢ, t.Field(3).Type.Key());
    fmt.Println(elemOnlyElemˢ, t.Field(3).Type.Elem(), t.Field(3).Type.Elem().Len());
    fmt.Println(keyOnlyKeyˢ, t.Field(4).Type.Key(), t.Field(4).Type.Key().Len());
    fmt.Println(keyOnlyElemˢ, t.Field(4).Type.Elem());
    fmt.Println(initializerRouteˢ);
    fmt.Println(plainˢ, t.Field(5).Type, t.Field(5).Type.Len());
    fmt.Println(nestedˢ, t.Field(6).Type, t.Field(6).Type.Len(), t.Field(6).Type.Elem().Len());
    fmt.Println(boundariesˢ);
    fmt.Println(namedPtrElemKindˢ, t.Field(7).Type.Elem().Kind());
    fmt.Println(sliceElemKindˢ, t.Field(8).Type.Elem().Kind());
}

} // end main_package
