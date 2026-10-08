namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Reader {
    @string Read();
}

partial interface Writer {
    void Write(@string s);
}

partial interface ReadWriteCloser :
    Reader,
    Writer
{
    @string Close();
}

partial interface Conn {
    @string Read();
    void Write(@string s);
    @string Close();
}

partial struct conn {
    internal @string data;
}

internal static @string Read(this ref conn c) {
    return c.data;
}

internal static void Write(this ref conn c, @string s) {
    c.data = s;
}

internal static @string Close(this ref conn c) {
    return "closed:"u8 + c.data;
}

internal static @string readFrom(Reader r) {
    return r.Read();
}

internal static void writeTo(Writer w, @string s) {
    w.Write(s);
}

internal static Writer asWriter(Conn c) {
    return new ConnᴠWriter(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "hello"u8;
private static readonly @string viaWriterˢ = "via-writer"u8;

internal static void Main() {
    Conn c = new connжConn(Ꮡ(new conn(data: "init"u8)));
    ReadWriteCloser rwc = new ConnᴠReadWriteCloser(c);
    writeTo(new ConnᴠWriter(c), helloˢ);
    fmt.Println(readFrom(new ConnᴠReader(c)));
    var w = asWriter(c);
    w.Write(viaWriterˢ);
    fmt.Println(c.Read());
    fmt.Println(rwc.Close());
}

} // end main_package
