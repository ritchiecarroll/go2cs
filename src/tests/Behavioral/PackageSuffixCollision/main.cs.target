namespace go;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using ownc = PackageSuffixCollision.ownc_package;
using ownf = PackageSuffixCollision.ownf_package;
using ownt = PackageSuffixCollision.ownt_package;
using ownv = PackageSuffixCollision.ownv_package;
using shadow = PackageSuffixCollision.shadow_package;
using PackageSuffixCollision;

partial class main_package {

partial struct Δmain_package {
    internal nint Δfmt_package;
}

partial struct holder {
    internal nint n;
}

internal static nint Δfmt_package⁔(/*this*/ holder h) { return h.n + 1; }

internal static nint ΔFmt_package(this Δmain_package m) { return m.Δfmt_package * 3; }

internal static nint Δfmt_package = 1;

internal static @string local() {
    @string Δstrings_package = "local";
    return strings.ToUpper(Δstrings_package);
}

internal static @string param(nint Δstrconv_package) {
    return strconv.Itoa(Δstrconv_package + 1);
}

internal static void main() {
    fmt.Println("own type:", ownt.Get(21));
    fmt.Println("own var:", ownv.Get(), ownv.Get());
    fmt.Println("own const:", ownc.Get());
    fmt.Println("own func:", ownf.Get());
    fmt.Println("pkg var:", Δfmt_package);
    Δfmt_package += 10;
    fmt.Println("pkg var after:", Δfmt_package);
    fmt.Println("local:", local());
    fmt.Println("param:", param(41));
    var m = new Δmain_package(Δfmt_package: 7);
    fmt.Println("field/method:", m.Δfmt_package, new holder(n: 7).Δfmt_package(), m.ΔFmt_package());
    fmt.Println("alias shadow:", shadow.Get());
}

} // end main_package
