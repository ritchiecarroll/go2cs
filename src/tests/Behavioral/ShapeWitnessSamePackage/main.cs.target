namespace go;

using fmt = fmt_package;
using strings = strings_package;
using static main_package;

partial class main_package {

partial interface Script {
    @string FileName();
    @string Text();
}

partial struct SourceFile {
    internal @string name, text;
}

} partial class mainᴛSourceFile {

public static @string FileName(this ref SourceFile f) { return f.name; }
public static @string Text(this ref SourceFile f) { return f.text; }

} partial class main_package {

partial struct MappedSpan<T> {
    public T Script;
    public nint Start;
}

public static slice<MappedSpan<T>> FromLSPRange<T, TScript>(T script, nint start)
    where TScript : struct, ScriptΔshape<T>
{
    return lspRangeToVirtual<T, TScript>(slice<T>([script]), start);
}

internal static slice<MappedSpan<T>> lspRangeToVirtual<T, TScript>(slice<T> scripts, nint start)
    where TScript : struct, ScriptΔshape<T>
{
    slice<MappedSpan<T>> @out = default;
    foreach (var (i, s) in range(scripts)) {
        @out = append(@out, new MappedSpan<T>(s, start + i + len(TScript.Text(s))));
    }
    return @out;
}

public static slice<MappedSpan<ж<SourceFile>>> FromLSPRangeForSourceFile(slice<ж<SourceFile>> files, nint start) {
    return lspRangeToVirtual<ж<SourceFile>, SourceFileжScriptΔshape>(files, start);
}

internal static any dump<T, TScript>(T x)
    where TScript : struct, ScriptΔshape<T>
{ return FromLSPRange<T, TScript>(x, 0); }

partial interface Named {
    @string Name();
}

partial struct Export {
    internal @string name;
}

} partial class mainᴛExport {

public static @string Name(this ref Export e) { return e.name; }

} partial class main_package {

partial struct Index<T, TNamed>
    where TNamed : struct, NamedΔshape<T>
{
    internal slice<T> entries;
}

} partial class mainᴛIndex {

public static slice<T> Find<T, TNamed>(this ref Index<T, TNamed> idx, @string name)
    where TNamed : struct, NamedΔshape<T>
{
    slice<T> @out = default;
    foreach (var (_, e) in range(idx.entries)) {
        if (TNamed.Name(e) == name) {
            @out = append(@out, e);
        }
    }
    return @out;
}

public static void Add<T, TNamed>(this ref Index<T, TNamed> idx, T e)
    where TNamed : struct, NamedΔshape<T>
{ idx.entries = append(idx.entries, e); }

} partial class main_package {

partial struct registry {
    internal Index<ж<Export>, ExportжNamedΔshape> exports;
}

partial interface Value {
    @string kind();
}

partial struct u8 {
    internal byte v;
}

} partial class mainᴛu8 {

internal static @string kind(this u8 _) { return "u8"; }

} partial class main_package {

partial struct Ptr<T>
    where T : Value
{
    internal nint off;
}

} partial class mainᴛPtr {

internal static @string kind<T>(this Ptr<T> _)
    where T : Value
{
    T x = GoZero<T>();
    return "Ptr["u8 + x.kind() + "]"u8;
}

} partial class main_package {

partial struct Array<T, TValue>
    where TValue : struct, ValueΔshape<T>
{
    internal slice<T> vals;
}

} partial class mainᴛArray {

internal static @string kind<T, TValue>(this Array<T, TValue> _)
    where TValue : struct, ValueΔshape<T>
{
    T x = GoZero<T>();
    return "Array["u8 + TValue.kind(x) + "]"u8;
}

} partial class main_package {

public static Array<T, TValue> /*y*/ ConstArray<T, TValue>(slice<T> vals, @string name)
    where TValue : struct, ValueΔshape<T>
{
    Array<T, TValue> y = default;

    y.vals = vals;
    return y;
}

partial struct memFile {
    internal @string name;
}

} partial class mainᴛmemFile {

internal static @string FileName(this memFile m) { return m.name; }
internal static @string Text(this memFile m) { return "mem:"u8 + m.name; }

} partial class main_package {

partial struct boxed {
    internal nint n;
}

} partial class mainᴛboxed {

internal static @string kind(this ref boxed _) { return "boxed"; }

} partial class main_package {

internal static void main() {
    var a = Ꮡ(new SourceFile("a.ts", "let x"));
    var spans = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 10);
    fmt.Println("FromLSPRange:", len(spans), spans[0].Start, spans[0].Script == a);
    spans[0].Script.text = "changed through the result";
    fmt.Println("write through the result:", a.text);

    var files = slice<ж<SourceFile>>([a, Ꮡ(new SourceFile("b.ts", "yz"))]);
    var masked = FromLSPRangeForSourceFile(files, 0);
    fmt.Println("masked site:", len(masked), masked[1].Start, masked[1].Script == files[1], fmt.Sprintf("%T"u8, masked));

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
    fmt.Println("value implementer:", len(mems), mems[0].Start, mems[0].Script.Text());

    Script iface = new SourceFileжScript(a);
    var viaIface = FromLSPRange<Script, ScriptΔshapeOf<Script>>(iface, 0);
    fmt.Println("interface argument:", AreEqual(viaIface[0].Script, iface), viaIface[0].Start);

    var b = Ꮡ(new boxed(7));
    var boxes = ConstArray<ж<boxed>, boxedжValueΔshape>(slice<ж<boxed>>([b]), "b");
    boxes.vals[0].n = 8;
    fmt.Println("pointer carrier:", boxes.kind(), b.n, boxes.vals[0] == b);
}

} // end main_package
