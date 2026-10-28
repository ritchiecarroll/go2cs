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

internal static void Main() {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    a = ж<array<nint>>.NilBoxOfDims(3L);
    fmt.Println("assign local  Len:", reflect.ValueOf(a.OrTypedNil()).Len(), "Cap:", reflect.ValueOf(a.OrTypedNil()).Cap());

    holder h = default;
    h.versym = ж<array<uint16>>.NilBoxOfDims(3L);
    fmt.Println("assign field  elem len:", reflect.ValueOf(h.versym.OrTypedNil()).Type().Elem().Len());

    fmt.Println("argument      elem len:", takeArg(ж<Sigset>.NilBoxOfDims(2L)));

    fmt.Println("result        elem len:", reflect.ValueOf(ret().OrTypedNil()).Type().Elem().Len());

    fmt.Println("conversion    Len:", reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(7L)).Len());
}

} // end main_package
