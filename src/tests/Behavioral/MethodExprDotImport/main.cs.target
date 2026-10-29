namespace go;

using fmt = fmt_package;
using static MethodExprDotImport.mep_package;
using mep = MethodExprDotImport.mep_package;

partial class main_package {

internal static void Main() {
    var readers = slice<Func<ж<mep.Reader>, byte, (@string, error)>>([
        ((Func<ж<mep.Reader>, byte, (@string, error)>)(mep.Read)),
        ((Func<ж<mep.Reader>, byte, (@string, error)>)(mep.Peek))
    ]);

    var r = Ꮡ(new Reader(Name: "go2cs"));

    foreach (var (_, read) in readers) {
        var (s, _) = read(r, (byte)',');
        fmt.Println(s);
    }
}

} // end main_package
