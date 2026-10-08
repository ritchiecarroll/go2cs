namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Closer {
    @string Close();
}

partial interface Reader {
    @string Read();
}

partial interface ReadCloser :
    Reader,
    Closer
{
}

partial struct myFile {
    /*embed*/ public Closer Closer;
    internal @string data;
}

internal static @string Read(this myFile f) {
    return "read:"u8 + f.data;
}

internal static @string Close(this myFile f) {
    return "closed:"u8 + f.data;
}

internal static @string useCloser(Closer c) {
    return c.Close();
}

internal static @string useReadCloser(ReadCloser rc) {
    return rc.Read() + ","u8 + rc.Close();
}

internal static void Main() {
    var f = new myFile(data: "x"u8);
    fmt.Println(useCloser(f));
    fmt.Println(useReadCloser(f));
}

} // end main_package
