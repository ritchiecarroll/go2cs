namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Grid /*[3]nint*/;

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
private static readonly object ifaceUnexportedˢ = (@string)"iface unexported:"u8;
private static readonly object ifaceMixedˢ = (@string)"iface mixed     :"u8;
private static readonly object ifaceEmptyˢ = (@string)"iface empty     :"u8;

internal partial interface main_unexportedOnly /*dyn*/ {
    @string a(nint _);
}

internal partial interface main_mixedExportedness /*dyn*/ {
    void zeta();
    void Alpha(nint x);
}

internal static void Main() {
    show(uint8ˢ, new uint8[]{}.array(6));
    show(uint8ˢ2, GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6));
    show(intˢ, GoReflect.WithElemDims(new array<nint>[]{new nint[]{}.array(3)}.slice(), 3));
    show(intˢ2, new array<nint>[]{}.array(2, () => new(3)));
    show(intˢ3, GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice(), 2, 3));
    show(map2IntIntˢ, new map<array<nint>, slice<nint>>{[new nint[]{}.array(2)] = default!});
    show(gridˢ, GoReflect.WithElemDims(new Grid[]{new nint[]{}.array(3)}.slice(), 3));
    show(byteˢ, new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice());
    ref var unexportedOnly = ref heap<main_unexportedOnly>(out var ᏑunexportedOnly);
    ref var mixedExportedness = ref heap<main_mixedExportedness>(out var ᏑmixedExportedness);
    fmt.Println(ifaceUnexportedˢ, reflect.TypeOf(ᏑunexportedOnly).Elem().String());
    fmt.Println(ifaceMixedˢ, reflect.TypeOf(ᏑmixedExportedness).Elem().String());
    fmt.Println(ifaceEmptyˢ, reflect.TypeOf(@new<any>()).Elem().String());
    fmt.Printf("slice-of-array Elem().String()=%s Len()=%d\n"u8,
        reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)).Elem().String(),
        reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)).Elem().Len());
}

} // end main_package
