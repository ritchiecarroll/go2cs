namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

[GoType("[3]nint")] partial struct Grid;

internal static void show(@string label, any v) {
    fmt.Printf("%-16s %%T=%-18T String()=%s\n"u8, label, v, reflect.TypeOf(v).String());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string uint8ˢ = "[6]uint8"u8;
private static readonly @string uint8ˢ2 = "[][6]uint8"u8;
private static readonly @string intˢ = "[][3]int"u8;
private static readonly @string intˢ2 = "[2][3]int"u8;
private static readonly @string intˢ3 = "[][2][3]int"u8;
private static readonly @string map2IntIntˢ = "map[[2]int][]int"u8;
private static readonly @string gridˢ = "[]Grid"u8;
private static readonly @string byteˢ = "[]*[4]byte"u8;

internal static void Main() {
    show(uint8ˢ, new uint8[]{}.array(6));
    show(uint8ˢ2, new array<uint8>[]{new uint8[]{}.array(6)}.slice());
    show(intˢ, new array<nint>[]{new nint[]{}.array(3)}.slice());
    show(intˢ2, new array<nint>[]{}.array(2, () => new(3)));
    show(intˢ3, new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice());
    show(map2IntIntˢ, new map<array<nint>, slice<nint>>{[new nint[]{}.array(2)] = default!});
    show(gridˢ, new Grid[]{new nint[]{}.array(3)}.slice());
    show(byteˢ, new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice());
    fmt.Printf("slice-of-array Elem().String()=%s Len()=%d\n"u8,
        reflect.TypeOf(new array<uint8>[]{new uint8[]{}.array(6)}.slice()).Elem().String(),
        reflect.TypeOf(new array<uint8>[]{new uint8[]{}.array(6)}.slice()).Elem().Len());
}

} // end main_package
