namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct item {
    internal nint n;
}

[GoType("ж<item>")] partial class itemPtr;

internal static void Main() {
    var pi = Ꮡ(new item(2));
    var ip = new itemPtr(pi);
    var other = new itemPtr(Ꮡ(new item(2)));
    fmt.Println(ip == new itemPtr(pi), ip != other, ip == nil, ip != new itemPtr(pi), ip == ip);
}

} // end main_package
