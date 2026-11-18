namespace go.MethodExprDotImport;

using static mep_package;

partial class mep_package {

partial struct Reader {
    public @string Name;
}

} partial class mepᴛReader {

public static (@string, error) Read(this ref Reader r, byte delim) {
    return (r.Name, default);
}

public static (@string, error) Peek(this ref Reader r, byte delim) {
    return ("peek:" + r.Name, default);
}

} // end mepᴛReader
