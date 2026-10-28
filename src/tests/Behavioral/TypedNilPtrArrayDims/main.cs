namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct pkgNamed3 /*[3]byte*/;

internal partial struct main_named3 /*[3]byte*/;

internal static void Main() {
    var t0 = reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(0L));
    var t3 = reflect.TypeOf(ж<array<byte>>.NilBoxOfDims(3L));
    fmt.Println("distinct types:", !AreEqual(t0, t3));
    fmt.Println("t0:", t0, "t3:", t3);

    fmt.Println("t3 elem len:", t3.Elem().Len());
    fmt.Println("value Len of nil *[3]int:", reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(3L)).Len());
    fmt.Println("value Cap of nil *[3]int:", reflect.ValueOf(ж<array<nint>>.NilBoxOfDims(3L)).Cap());

    any a = ж<array<byte>>.NilBoxOfDims(3L);
    any b = ж<array<byte>>.NilBoxOfDims(3L);
    fmt.Println("same type equal:", AreEqual(a, b));
    fmt.Println("same type, same reflect.Type:", AreEqual(reflect.TypeOf(a), reflect.TypeOf(b)));

    fmt.Println("boxed nil is non-nil interface:", a != nil);
    fmt.Printf("dynamic type: %T\n"u8, a);

    fmt.Println("package-level named array elem len:", reflect.TypeOf(ж<pkgNamed3>.NilBoxOfDims(3L)).Elem().Len());
    fmt.Println("local named array elem len:", reflect.TypeOf(ж<main_named3>.NilBoxOfDims(3L)).Elem().Len());
}

} // end main_package
