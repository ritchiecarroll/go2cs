namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, vᴛ1) in compareTests) {
        ref var tt = ref heap(new compareTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var p = Ꮡtt;
        fmt.Println((@string)(~p).a, (@string)(~p).b, (~p).i);
    }

    foreach (var (_, vᴛ2) in sizeTests) {
        ref var st = ref heap(new sizeTestsᴛ1(), out var Ꮡst);
        st = vᴛ2;

        var q = Ꮡst;
        fmt.Println((~q).name, (~q).want);
    }

    fmt.Println(new Bee(nil).probe());
    fmt.Println(new Why(nil).probe());
}

} // end main_package
