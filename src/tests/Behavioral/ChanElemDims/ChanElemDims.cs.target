namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static void Main() {
    var c = new channel<array<nint>>(0, ChanCargo.Of(null, new nint[] { 3 }));

    fmt.Printf("value row [red by boundary until increment C: not measurable from a channel value] %%T=%T String()=%s Elem().Len()=%d\n"u8,
        c, reflect.TypeOf(c).String(), reflect.TypeOf(c).Elem().Len());

    var ct = reflect.ChanOf(reflect.BothDir, reflect.ArrayOf(3, reflect.TypeOf((nint)0)));
    @string name = ct.String();
    nint n = ct.Elem().Len();
    fmt.Printf("constructed row: ChanOf(BothDir, ArrayOf(3,int)) String()=%s Elem().Len()=%d\n"u8, name, n);
}

} // end main_package
