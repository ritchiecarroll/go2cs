namespace go;

using fmt = fmt_package;
using static MethodExprDotImport.mep_package;
using mep = MethodExprDotImport.mep_package;

partial class main_package {

internal static void Main() {
    var readers = new Func<ж<mep.Reader>, byte, (@string, error)>[]{
        ((Func<ж<mep.Reader>, byte, (@string, error)>)(mep.Read)),
        ((Func<ж<mep.Reader>, byte, (@string, error)>)(mep.Peek))
    }.slice();

    var r = Ꮡ(new Reader(Name: "go2cs"));

    foreach (var (_, read) in readers) {
        var (s, _) = read(r, (rune)',');
        fmt.Println(s);
    }
}

} // end main_package
