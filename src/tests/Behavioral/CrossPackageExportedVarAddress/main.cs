namespace go;

using flag = flag_package;
using fmt = fmt_package;
using lib = global::go.go.xpkgvar_package;
using global::go.go;
using xpkgvar = global::go.go.xpkgvar_package;

partial class main_package {

internal static void Main() {
    var p = lib.ᏑN;
    p.Value = 5;
    fmt.Println("&lib.N write:", lib.N, lib.ReadN());

    fmt.Println("&lib.N identity:", lib.ᏑN == lib.ᏑN, p == lib.ᏑN);

    ref var y = ref heap<nint>(out var Ꮡy);
    y = 9;
    p = Ꮡy;
    p.Value = 10;
    fmt.Println("re-point:", lib.N, y);

    var f = lib.ᏑV.Inc;
    f();
    f();
    fmt.Println("method value:", lib.V.Value());

    lib.V.Inc();
    fmt.Println("direct call:", lib.V.Value());

    var fs = flag.NewFlagSet("guard", flag.ContinueOnError);
    fs.BoolVar(lib.ᏑFlag, "x", false, "");
    fs.Parse(slice<@string>(["-x"]));
    fmt.Println("flag.BoolVar:", lib.Flag);

    var q = lib.ᏑS.of(lib.Holder.ᏑN);
    q.Value = 3;
    fmt.Println("field pointer:", lib.S.N, lib.ReadS());

    lib.ᏑS.of(lib.Holder.ᏑWG).Add(1);
    lib.ᏑS.of(lib.Holder.ᏑWG).Done();
    lib.ᏑS.of(lib.Holder.ᏑWG).Wait();
    fmt.Println("field-path pointer calls: ok");

    var e = lib.ᏑArr.at<nint>(1);
    e.Value = 7;
    fmt.Println("element pointer:", lib.Arr[1], lib.Arr);

    var t = lib.ᏑTable;
    t.ValueSlot = append(t.ValueSlot, (nint)4);
    fmt.Println("lazy slice:", len(lib.Table), lib.Table);
}

} // end main_package
