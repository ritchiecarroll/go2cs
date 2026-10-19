namespace go;

using fmt = fmt_package;
using GenericTypeAliasLib = GenericTypeAliasLib_package;

partial class main_package {

internal static void Main() {
    var b = GenericTypeAliasLib.NewBox<nint>(41);
    b.Set(b.Get() + 1);
    fmt.Println(b.Get(), GenericTypeAliasLib.Label("bystander"));
}

} // end main_package
