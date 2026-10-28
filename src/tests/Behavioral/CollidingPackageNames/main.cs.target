namespace go;

using fmt = fmt_package;
using dupmeta = collidea.dup_package;
using dup = collideb.dup_package;
using collidea;
using collideb;

partial class main_package {

internal static void Main() {
    fmt.Println(dupmeta.Greeting());
    fmt.Println(dup.Marker());

    dupmeta.Box<@string> b = default;
    b.V = "boxed-in-duprenamed";
    fmt.Println(b.Get());

    dupmeta.Widget w = default;
    fmt.Println(w.Marker());
}

} // end main_package
