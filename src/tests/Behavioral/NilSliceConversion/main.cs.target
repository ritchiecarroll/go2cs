namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var s = append(slice<@string>(default), (@string)"x", (@string)"y");
    fmt.Println(len(s), cap(s), s == nil);

    slice<byte> b = default;
    var c = appendꓸꓸꓸ(slice<byte>(default), b);
    fmt.Println(len(c), cap(c), c == nil);

    var n = slice<nint>(default);
    fmt.Println(len(n), cap(n), n == nil);

    slice<@string> zs = default;
    var t = zs[0..0];
    fmt.Println(len(t), cap(t), t == nil);
}

} // end main_package
