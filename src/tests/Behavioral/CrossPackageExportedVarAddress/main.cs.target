namespace go;

using flag = flag_package;
using fmt = fmt_package;
using lib = global::go.go.xpkgvar_package;
using global::go.go;
using xpkgvar = global::go.go.xpkgvar_package;

partial class main_package {

internal static void Main() {
    var p = Ꮡ(lib.N);
    p.Value = 5;
    fmt.Println("&lib.N write:", lib.N, lib.ReadN());

    fmt.Println("&lib.N identity:", Ꮡ(lib.N) == Ꮡ(lib.N), p == Ꮡ(lib.N));

    var f = Ꮡ(lib.V).Inc;
    f();
    f();
    fmt.Println("method value:", lib.V.Value());

    lib.V.Inc();
    fmt.Println("direct call:", lib.V.Value());

    var fs = flag.NewFlagSet("guard", flag.ContinueOnError);
    fs.BoolVar(Ꮡ(lib.Flag), "x", false, "");
    fs.Parse(new @string[]{"-x"}.slice());
    fmt.Println("flag.BoolVar:", lib.Flag);

    var q = Ꮡ(lib.S).of(lib.Holder.ᏑN);
    q.Value = 3;
    fmt.Println("field pointer:", lib.S.N, lib.ReadS());

    Ꮡ(lib.S).of(lib.Holder.ᏑWG).Add(1);
    Ꮡ(lib.S).of(lib.Holder.ᏑWG).Done();
    Ꮡ(lib.S).of(lib.Holder.ᏑWG).Wait();
    fmt.Println("field-path pointer calls: ok");

    var e = Ꮡlib.Arr.at<nint>(1);
    e.Value = 7;
    fmt.Println("element pointer:", lib.Arr[1], lib.Arr);

    var t = Ꮡ(lib.Table);
    t.ValueSlot = append(t.ValueSlot, (nint)4);
    fmt.Println("lazy slice:", len(lib.Table), lib.Table);
}

} // end main_package
