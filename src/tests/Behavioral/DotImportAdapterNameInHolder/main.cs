namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using strings = strings_package;
using static DotImportAdapterNameInHolder.a.sinks_package;
using static DotImportAdapterNameInHolder.b.sinks_package;

partial class main_package {

partial struct sink {
    internal slice<@string> lines;
}

internal static (nint, error) Write(this ref sink s, slice<byte> p) {
    s.lines = append(s.lines, (@string)p);
    return (len(p), default);
}

partial struct report {
    internal @string name;
}

internal static @string plain⁔(/*this*/ ref report r) { return r.name + ":method"u8; }

internal static @string buffer⁔(/*this*/ ref report r) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Δio.Writer w = new bytes_BufferжWriter(Ꮡbuf);
    fmt.Fprintf(w, "%s:buffer"u8, r.name);
    return Ꮡbuf.String();
}

internal static nint count⁔(/*this*/ ref report r) {
    var s = Ꮡ(new main_package.sink());
    Δio.Writer w = new main_package.sinkжWriter(s);
    fmt.Fprint(w, r.name, "|", "sink");
    return len(s.lines);
}

internal static @string builder⁔(/*this*/ ref report r) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    Δio.Writer w = new strings_BuilderжWriter(Ꮡsb);
    fmt.Fprintf(w, "%s:builder"u8, r.name);
    return sb.String();
}

internal static @string plain() {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Δio.Writer w = new bytes_BufferжWriter(Ꮡbuf);
    fmt.Fprint(w, "plain");
    return Ꮡbuf.String();
}

internal static void main() {
    var r = Ꮡ(new report(name: "r"));
    fmt.Println(r.buffer(), r.count(), r.builder(), r.plain(), plain());

    var (w, buf) = NewWriter();
    fmt.Fprint(w, "a");
    var (cw, counted) = Counted();
    fmt.Fprint(cw, "abc");
    fmt.Println(buf.String(), counted());

    ref var other = ref heap(new bytes.Buffer(), out var Ꮡother);
    var ww = Wrap(Ꮡother);
    fmt.Fprint(ww, "b");
    var (dw, doubled) = Doubled();
    fmt.Fprint(dw, "abc");
    fmt.Println(Ꮡother.String(), doubled());
}

} // end main_package
