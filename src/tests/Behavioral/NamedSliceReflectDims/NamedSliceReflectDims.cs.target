namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct UUID /*[16]byte*/;

partial struct UUIDs /*[]UUID*/;

partial struct Rows /*[]array<nint>*/;

partial struct Names /*[]@string*/;

internal static void Main() {
    UUIDs u = GoReflect.WithElemDims(new UUID[]{}.slice(), 16);
    Rows r = GoReflect.WithElemDims(new array<nint>[]{}.slice(), 3);
    fmt.Println("named empty:", reflect.TypeOf(u).Elem().Len(), reflect.TypeOf(r).Elem().Len());

    var rows = GoReflect.WithElemDims(new array<nint>[]{new nint[]{1, 2, 3}.array()}.slice(), 3);
    Rows window = GoReflect.WithElemDims(rows[..0], 3);
    fmt.Println("named window:", reflect.TypeOf(window).Elem().Len());
    Rows full = rows;

    fmt.Println("unnamed empty:", reflect.TypeOf(GoReflect.WithElemDims(new UUID[]{}.slice(), 16)).Elem().Len(), reflect.TypeOf(GoReflect.WithElemDims(new array<nint>[]{}.slice(), 3)).Elem().Len());
    fmt.Println("named non-empty:", reflect.TypeOf(full).Elem().Len());

    fmt.Println("named non-array:", reflect.TypeOf(new Names(new @string[]{}.slice())).Elem().Kind());
}

} // end main_package
