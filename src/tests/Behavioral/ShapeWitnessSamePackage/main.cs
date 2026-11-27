namespace go;

using fmt = fmt_package;
using stdio = io_package;
using strings = strings_package;
using Δio = io_package;
using ꓸꓸꓸstring = variadic<@string>;

partial class main_package {

partial interface Script {
    @string FileName();
    @string Text();
}

partial struct SourceFile {
    internal @string name, text;
}

public static @string FileName(this ref SourceFile f) { return f.name; }
public static @string Text(this ref SourceFile f) { return f.text; }

partial struct MappedSpan<T> {
    public T Script;
    public nint Start;
}

public static slice<MappedSpan<T>> FromLSPRange<T, TΔScript>(T script, nint start)
    where TΔScript : struct, ScriptΔshape<T>
{
    return lspRangeToVirtual<T, TΔScript>(slice<T>([script]), start);
}

internal static slice<MappedSpan<T>> lspRangeToVirtual<T, TΔScript>(slice<T> scripts, nint start)
    where TΔScript : struct, ScriptΔshape<T>
{
    slice<MappedSpan<T>> @out = default;
    foreach (var (i, s) in range(scripts)) {
        @out = append(@out, new MappedSpan<T>(s, start + i + len(TΔScript.Text(s))));
    }
    return @out;
}

public static slice<MappedSpan<ж<SourceFile>>> FromLSPRangeForSourceFile(slice<ж<SourceFile>> files, nint start) {
    return lspRangeToVirtual<ж<SourceFile>, SourceFileжScriptΔshape>(files, start);
}

internal static any dump<T, TΔScript>(T x)
    where TΔScript : struct, ScriptΔshape<T>
{ return FromLSPRange<T, TΔScript>(x, 0); }

partial interface Named {
    @string Name();
}

partial struct Export {
    internal @string name;
}

public static @string Name(this ref Export e) { return e.name; }

partial struct Index<T, TΔNamed>
    where TΔNamed : struct, NamedΔshape<T>
{
    internal slice<T> entries;
}

internal static @string nameOf<T, TΔNamed>(T x)
    where TΔNamed : struct, NamedΔshape<T>
{ return TΔNamed.Name(x); }

public static slice<T> Find<T, TΔNamed>(this ref Index<T, TΔNamed> idx, @string name)
    where TΔNamed : struct, NamedΔshape<T>
{
    slice<T> @out = default;
    foreach (var (_, e) in range(idx.entries)) {
        if (nameOf<T, TΔNamed>(e) == name) {
            @out = append(@out, e);
        }
    }
    return @out;
}

public static void Add<T, TΔNamed>(this ref Index<T, TΔNamed> idx, T e)
    where TΔNamed : struct, NamedΔshape<T>
{ idx.entries = append(idx.entries, e); }

partial struct registry {
    internal Index<ж<Export>, ExportжNamedΔshape> exports;
}

partial interface Value {
    @string kind();
}

partial struct u8 {
    internal byte v;
}

internal static @string kind(this u8 _) { return "u8"; }

partial struct Ptr<T>
    where T : Value
{
    internal nint off;
}

internal static @string kind<T>(this Ptr<T> _)
    where T : Value
{
    T x = builtin.GoZero<T>();
    return "Ptr["u8 + x.kind() + "]"u8;
}

partial struct Array<T, TΔValue>
    where TΔValue : struct, ValueΔshape<T>
{
    internal slice<T> vals;
}

internal static @string kind<T, TΔValue>(this Array<T, TΔValue> _)
    where TΔValue : struct, ValueΔshape<T>
{
    T x = builtin.GoZero<T>();
    return "Array["u8 + TΔValue.kind(x) + "]"u8;
}

public static Array<T, TΔValue> /*y*/ ConstArray<T, TΔValue>(slice<T> vals, @string name)
    where TΔValue : struct, ValueΔshape<T>
{
    Array<T, TΔValue> y = default;

    y.vals = vals;
    return y;
}

partial struct memFile {
    internal @string name;
}

internal static @string FileName(this memFile m) { return m.name; }
internal static @string Text(this memFile m) { return "mem:"u8 + m.name; }

partial struct boxed {
    internal nint n;
}

internal static @string kind(this ref boxed _) { return "boxed"; }

internal static void main() {
    var a = Ꮡ(new SourceFile("a.ts", "let x"));
    var spans = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 10);
    fmt.Println("FromLSPRange:", len(spans), spans[0].Start, spans[0].Script == a);
    spans[0].Script.text = "changed through the result";
    fmt.Println("write through the result:", a.text);

    var files = slice<ж<SourceFile>>([a, Ꮡ(new SourceFile("b.ts", "yz"))]);
    var masked = FromLSPRangeForSourceFile(files, 0);
    var evalᴛ1 = len(masked);
    var evalᴛ2 = masked[1].Script == files[1];
    var evalᴛ3 = fmt.Sprintf("%T"u8, masked);
    fmt.Println("masked site:", evalᴛ1, masked[1].Start, evalᴛ2, evalᴛ3);

    var (_, ok) = dump<ж<SourceFile>, SourceFileжScriptΔshape>(a)._<slice<MappedSpan<ж<SourceFile>>>>(ᐧ);
    fmt.Println("dump through any:", ok);

    registry r = default;
    r.exports.Add(Ꮡ(new Export("x")));
    r.exports.Add(Ꮡ(new Export("y")));
    var found = r.exports.Find("x");
    fmt.Println("index field:", len(found), found[0] == r.exports.entries[0]);
    found[0].name = "x2";
    fmt.Println("write through Find:", r.exports.entries[0].name);

    var p = new Ptr<u8>(3);
    fmt.Println("gen Ptr:", p.kind(), p.off);
    var arr = ConstArray<u8, ValueΔshapeOf<u8>>(slice<u8>([new(1), new(2)]), "k");
    arr.vals[0].v = 9;
    fmt.Println("gen ConstArray:", arr.kind(), len(arr.vals), arr.vals[0].v);

    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (_, s) in range(lspRangeToVirtual<ж<SourceFile>, SourceFileжScriptΔshape>(files, 1))) {
        Ꮡsb.WriteString(s.Script.FileName());
    }
    fmt.Println("range over the result:", sb.String());

    var mems = FromLSPRange<memFile, ScriptΔshapeOf<memFile>>(new memFile("m.ts"), 2);
    var evalᴛ4 = len(mems);
    var evalᴛ5 = mems[0].Script.Text();
    fmt.Println("value implementer:", evalᴛ4, mems[0].Start, evalᴛ5);

    Script iface = new SourceFileжScript(a);
    var viaIface = FromLSPRange<Script, ScriptΔshapeOf<Script>>(iface, 0);
    fmt.Println("interface argument:", AreEqual(viaIface[0].Script, iface), viaIface[0].Start);

    var b = Ꮡ(new boxed(7));
    var boxes = ConstArray<ж<boxed>, boxedжValueΔshape>(slice<ж<boxed>>([b]), "b");
    boxes.vals[0].n = 8;
    fmt.Println("pointer carrier:", boxes.kind(), b.n, boxes.vals[0] == b);

    Value asValue = new Array<u8, ValueΔshapeOf<u8>>(vals: [new(5)]);
    var @explicit = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 3);
    fmt.Println("written instantiations:", asValue.kind(), @explicit[0].Start, @explicit[0].Script == a);

    var (srcs, n) = Readers<ж<memSrc>, memSrcжSourceΔshape>(slice<ж<memSrc>>([Ꮡ(new memSrc("abc")), Ꮡ(new memSrc("de"))]));
    fmt.Println("foreign type in the signature:", len(srcs), n, srcs[1].data);
    fmt.Println("package Span:", new Span<nint>(1, 2).hi);

    var parts = slice<@string>(["x", "y"]);
    var stamped = StampThrough<ж<memSrc>, memSrcжSourceΔshape>(srcs[0], parts);
    fmt.Println("spread through the witness:", parts[0], parts[1], stamped[0] == srcs[0]);
}

partial interface Source {
    stdio.Reader Open();
    @string Join(@string sep, params ꓸꓸꓸstring partsʗp);
    void Stamp(params ꓸꓸꓸstring partsʗp);
}

partial struct Span<T> {
    internal T lo, hi;
}

partial struct memSrc {
    internal @string data;
}

internal static stdio.Reader Open(this ref memSrc m) { return new strings_ReaderжReader(strings.NewReader(m.data)); }

internal static @string Join(this ref memSrc m, @string sep, params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.slice();

    var evalᴛ1 = strings.Join(parts, sep);
    return m.data + ":"u8 + evalᴛ1;
}

internal static void Stamp(this ref memSrc m, params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.slice();

    stampFirst(parts, m.data);
}

internal static void stampFirst(slice<@string> dst, @string v) { dst[0] = v; }

public static slice<T> StampThrough<T, TΔSource>(T s, slice<@string> parts)
    where TΔSource : struct, SourceΔshape<T>
{
    TΔSource.Stamp(s, parts.ꓸꓸꓸ);
    return [s];
}

public static (slice<T>, nint) Readers<T, TΔSource>(slice<T> srcs)
    where TΔSource : struct, SourceΔshape<T>
{
    nint n = 0;
    foreach (var (_, s) in range(srcs)) {
        var (b, _) = stdio.ReadAll(TΔSource.Open(s));
        n += len(b);
        n += len(TΔSource.Join(s, ",", "x", "y"));
    }
    return (srcs, n);
}

} // end main_package
