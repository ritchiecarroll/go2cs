namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Grid /*[3]nint*/;

internal static void show(@string label, any v) {
    fmt.Printf("%-16s %%T=%-18T String()=%s\n"u8, label, v, reflect.TypeOf(v).String());
}

internal partial interface main_unexportedOnly /*dyn*/ {
    @string a(nint _);
}

internal partial interface main_mixedExportedness /*dyn*/ {
    void zeta();
    void Alpha(nint x);
}

internal static void Main() {
    show("[6]uint8", new uint8[]{}.array(6));
    show("[][6]uint8", GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6));
    show("[][3]int", GoReflect.WithElemDims(new array<nint>[]{new nint[]{}.array(3)}.slice(), 3));
    show("[2][3]int", new array<nint>[]{}.array(2, () => new(3)));
    show("[][2][3]int", GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice(), 2, 3));
    show("map[[2]int][]int", new map<array<nint>, slice<nint>>{[new nint[]{}.array(2)] = default});
    show("[]Grid", GoReflect.WithElemDims(new Grid[]{new nint[]{}.array(3)}.slice(), 3));
    show("[]*[4]byte", new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice());

    ref var unexportedOnly = ref heap<main_unexportedOnly>(out var ᏑunexportedOnly);
    ref var mixedExportedness = ref heap<main_mixedExportedness>(out var ᏑmixedExportedness);
    fmt.Println("iface unexported:", reflect.TypeOf(ᏑunexportedOnly).Elem().String());
    fmt.Println("iface mixed     :", reflect.TypeOf(ᏑmixedExportedness).Elem().String());
    fmt.Println("iface empty     :", reflect.TypeOf(@new<any>()).Elem().String());

    fmt.Printf("slice-of-array Elem().String()=%s Len()=%d\n"u8,
        reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)).Elem().String(),
        reflect.TypeOf(GoReflect.WithElemDims(new array<uint8>[]{new uint8[]{}.array(6)}.slice(), 6)).Elem().Len());
}

} // end main_package
