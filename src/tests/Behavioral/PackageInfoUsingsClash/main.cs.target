namespace go;

using fmt = fmt_package;
using clashlib = PackageInfoUsingsClash.clashlib_package;
using PackageInfoUsingsClash;

partial class main_package {

internal static void main() {
    ref var m = ref heap<clashlib.MethodImpl>(clashlib.New(2), out var Ꮡm);
    fmt.Println(m.Get());

    m.Bump(5);
    fmt.Println(m.Get());

    var p = Ꮡm;
    p.Bump(1);
    fmt.Println(p.Get(), m.Get());

    var get = ((Func<clashlib.MethodImpl, nint>)(global::go.PackageInfoUsingsClash.clashlibᴛMethodImpl.Get));
    fmt.Println(get(m));

    clashlib.StackTrace trace = default;
    trace.Push("main");
    trace.Push("caller");
    fmt.Println(trace.String(), len(trace));
    fmt.Println(trace.Describe().String());
    fmt.Println(trace);
}

} // end main_package
