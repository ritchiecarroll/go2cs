namespace go.MethodExprDotImport;

partial class mep_package {

partial struct Reader {
    public @string Name;
}

internal static (@string, error) Read⁔(/*this*/ ref Reader r, byte delim) {
    return (r.Name, default);
}

internal static (@string, error) Peek⁔(/*this*/ ref Reader r, byte delim) {
    return ("peek:" + r.Name, default);
}

} // end mep_package
