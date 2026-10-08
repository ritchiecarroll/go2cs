namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct UUID /*[16]byte*/;

partial struct UUIDs /*[]UUID*/;

partial struct Rows /*[]array<nint>*/;

partial struct Names /*[]@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedEmptyˢ = (@string)"named empty:"u8;
private static readonly object namedWindowˢ = (@string)"named window:"u8;
private static readonly object unnamedEmptyˢ = (@string)"unnamed empty:"u8;
private static readonly object namedNonEmptyˢ = (@string)"named non-empty:"u8;
private static readonly object namedNonArrayˢ = (@string)"named non-array:"u8;

internal static void Main() {
    UUIDs u = GoReflect.WithElemDims(new UUID[]{}.slice(), 16);
    Rows r = GoReflect.WithElemDims(new array<nint>[]{}.slice(), 3);
    fmt.Println(namedEmptyˢ, reflect.TypeOf(u).Elem().Len(), reflect.TypeOf(r).Elem().Len());
    var rows = GoReflect.WithElemDims(new array<nint>[]{new nint[]{1, 2, 3}.array()}.slice(), 3);
    Rows window = GoReflect.WithElemDims(rows[..0], 3);
    fmt.Println(namedWindowˢ, reflect.TypeOf(window).Elem().Len());
    Rows full = rows;
    fmt.Println(unnamedEmptyˢ, reflect.TypeOf(GoReflect.WithElemDims(new UUID[]{}.slice(), 16)).Elem().Len(), reflect.TypeOf(GoReflect.WithElemDims(new array<nint>[]{}.slice(), 3)).Elem().Len());
    fmt.Println(namedNonEmptyˢ, reflect.TypeOf(full).Elem().Len());
    fmt.Println(namedNonArrayˢ, reflect.TypeOf(new Names(new @string[]{}.slice())).Elem().Kind());
}

} // end main_package
