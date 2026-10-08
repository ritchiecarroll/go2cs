namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Sigset /*[2]uint32*/;

partial struct holder {
    internal /*[3]*/ ж<array<uint16>> versym;
}

internal static nint takeArg(ж<Sigset> Ꮡp) {
    return reflect.ValueOf(Ꮡp.OrTypedNil()).Type().Elem().Len();
}

internal static ж<array<byte>> ret() {
    return ж<array<byte>>.NilBoxOfDims(5L);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assignLocalLenˢ = (@string)"assign local  Len:"u8;
private static readonly object capˢ = (@string)"Cap:"u8;
private static readonly object assignFieldElemLenˢ = (@string)"assign field  elem len:"u8;
private static readonly object argumentElemLenˢ = (@string)"argument      elem len:"u8;
private static readonly object resultElemLenˢ = (@string)"result        elem len:"u8;
private static readonly object conversionLenˢ = (@string)"conversion    Len:"u8;

internal static void Main() {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    a = ж<array<nint>>.NilBoxOfDims(3L);
    fmt.Println(assignLocalLenˢ, reflect.ValueOf(a.OrTypedNil()).Len(), capˢ, reflect.ValueOf(a.OrTypedNil()).Cap());
    holder h = default!;
    h.versym = ж<array<uint16>>.NilBoxOfDims(3L);
    fmt.Println(assignFieldElemLenˢ, reflect.ValueOf(h.versym.OrTypedNil()).Type().Elem().Len());
    fmt.Println(argumentElemLenˢ, takeArg(ж<Sigset>.NilBoxOfDims(2L)));
    fmt.Println(resultElemLenˢ, reflect.ValueOf(ret().OrTypedNil()).Type().Elem().Len());
    fmt.Println(conversionLenˢ, reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(7L)).Len());
}

} // end main_package
