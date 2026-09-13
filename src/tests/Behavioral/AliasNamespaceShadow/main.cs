namespace go;

using fmt = fmt_package;
using inner = AliasNamespaceShadow.inner_package;
using AliasNamespaceShadow;

partial class main_package {

internal static void Main() {
    fmt.Println(inner.SortThree());
    fmt.Println(inner.LocalTag());
}

} // end main_package
