// witnessprobe (C#), REVISION 2026-11-18: the NARROW shape-witness emission, written by hand as the converter would emit
// it, over the real golib (ж<T>, slice<T>, append, Ꮡ). stdout prints the Go reference's lines (../go/expected.txt), and
// equal output is the bar. stderr prints the CONTROL arms: the failure each rule exists to prevent.
// No proxy, no copy, no array pun: T is the box itself, and every constrained call goes through a witness.
using System;
using System.Linq;
using System.Reflection;
using go;
using static go.builtin;

namespace go;

// SHOULD 7: witness type parameters carry a marker. The Go-name printer below DERIVES what to drop from it; nothing
// tells it a count.
[AttributeUsage(AttributeTargets.GenericParameter)]
public sealed class GoShapeWitnessAttribute : Attribute { }

// ---- lsconv ----

public interface Script
{
    @string FileName();
    @string Text();
}

public interface ScriptΔshape<T>
{
    static abstract @string FileName(T self);
    static abstract @string Text(T self);
}

// The generic witness for a NOMINAL implementer (a value type, or an interface embedding Script). Used only by the
// CONTROL arm below: the closed selection never binds the proxy.
public readonly struct ScriptΔshapeOf<T> : ScriptΔshape<T> where T : Script
{
    public static @string FileName(T self) => self.FileName();
    public static @string Text(T self) => self.Text();
}

public partial struct SourceFile
{
    internal @string name;
    internal @string text;

    public SourceFile(@string name, @string text) { this.name = name; this.text = text; }
}

public static partial class main_package
{
    internal static @string FileName(this ж<SourceFile> f) => f.Value.name;
    internal static @string Text(this ж<SourceFile> f) => f.Value.text;
}

// MUST 2 + 3: THE canonical witness for (*SourceFile, Script), emitted eagerly in SourceFile's assembly, which sees
// Script. Every caller names this one; none emits its own.
public readonly struct SourceFileжScriptΔshape : ScriptΔshape<ж<SourceFile>>
{
    public static @string FileName(ж<SourceFile> self) => self.FileName();
    public static @string Text(ж<SourceFile> self) => self.Text();
}

// No methods, so no witness parameter: MappedSpan's identity is Go's MappedSpan[T] exactly.
public partial struct MappedSpan<T>
{
    public T Script;
    public nint Start;

    public MappedSpan(T Script, nint Start) { this.Script = Script; this.Start = Start; }
}

public static partial class main_package
{
    public static slice<MappedSpan<T>> FromLSPRange<T, [GoShapeWitness] TScript>(T script, nint start)
        where TScript : struct, ScriptΔshape<T>
    {
        return lspRangeToVirtual<T, TScript>(new slice<T>(new[] { script }), start);
    }

    internal static slice<MappedSpan<T>> lspRangeToVirtual<T, [GoShapeWitness] TScript>(slice<T> scripts, nint start)
        where TScript : struct, ScriptΔshape<T>
    {
        slice<MappedSpan<T>> @out = default!;
        foreach (var (i, s) in range(scripts)) {
            @out = append(@out, new MappedSpan<T>(s, start + i + len(TScript.Text(s))));
        }
        return @out;
    }

    public static slice<MappedSpan<ж<SourceFile>>> FromLSPRangeForSourceFile(slice<ж<SourceFile>> files, nint start)
    {
        return lspRangeToVirtual<ж<SourceFile>, SourceFileжScriptΔshape>(files, start);
    }

    internal static void renameFirst<T>(slice<T> scripts, T other) { scripts[0] = other; }

    // PROPERTY CHECK (SHOULD 5), not a selection arm: a plain []T parameter is class B's position, not narrow-selected.
    internal static @string renameFirstNamed<T, [GoShapeWitness] TScript>(slice<T> scripts, T other)
        where TScript : struct, ScriptΔshape<T>
    {
        scripts[0] = other;
        return TScript.FileName(other);
    }

    // MUST 1: dump forwards its T into FromLSPRange (selected), so the closure selects dump and it takes the witness.
    internal static object dump<T, [GoShapeWitness] TScript>(T x)
        where TScript : struct, ScriptΔshape<T>
    {
        return FromLSPRange<T, TScript>(x, 0);
    }
}

// CONTROL for MUST 1: the UNCLOSED selection. dump would stay on today's non-self route, so T is the proxy and the
// result is slice<MappedSpan<proxy>>: the Go assertion to []MappedSpan[*SourceFile] fails where Go succeeds.
public sealed class SourceFileжScriptᴾ(ж<SourceFile> box) : Script
{
    public ж<SourceFile> Box => box;
    public @string FileName() => box.FileName();
    public @string Text() => box.Text();
}

public static partial class main_package
{
    internal static object dumpUnclosed<T>(T x) where T : Script => FromLSPRange<T, ScriptΔshapeOf<T>>(x, 0);
}

// ---- autoimport (a pointer pair in a FIELD) ----

public interface Named { @string Name(); }

public interface NamedΔshape<T> { static abstract @string Name(T self); }

public partial struct Export
{
    internal @string name;

    public Export(@string name) { this.name = name; }
}

public static partial class main_package
{
    internal static @string Name(this ж<Export> e) => e.Value.name;
}

public readonly struct ExportжNamedΔshape : NamedΔshape<ж<Export>>
{
    public static @string Name(ж<Export> self) => self.Name();
}

// Index's own method calls Name(), so the TYPE carries the witness parameter.
public partial struct Index<T, [GoShapeWitness] TNamed>
    where TNamed : struct, NamedΔshape<T>
{
    internal slice<T> entries;

    public slice<T> Find(@string name)
    {
        slice<T> @out = default!;
        foreach (var (_, e) in range(entries)) {
            if (TNamed.Name(e) == name) {
                @out = append(@out, e);
            }
        }
        return @out;
    }
}

public partial struct registry
{
    internal Index<ж<Export>, ExportжNamedΔshape> exports;
}

// ---- gc/internal/gen ----

public interface Value { @string kind(); }

public interface ValueΔshape<T> { static abstract @string kind(T self); }

public readonly struct ValueΔshapeOf<T> : ValueΔshape<T> where T : Value
{
    public static @string kind(T self) => self.kind();
}

// wrap[T Value] embeds Value; its witness carries both.
public interface Δwrap<T> : Value { T wrap(nint x); }

public interface ΔwrapΔshape<W, T> { static abstract T wrap(W self, nint x); }

public readonly struct ΔwrapΔshapeOf<W, T> : ΔwrapΔshape<W, T> where W : Δwrap<T>
{
    public static T wrap(W self, nint x) => self.wrap(x);
}

public partial struct u8 : Value
{
    internal byte v;

    public u8(byte v) { this.v = v; }

    public @string kind() => "u8";
}

public partial struct Ptr<T, [GoShapeWitness] TValue> : Δwrap<Ptr<T, TValue>>
    where TValue : struct, ValueΔshape<T>
{
    internal nint off;

    public Ptr(nint off) { this.off = off; }

    public @string kind() { T x = default!; return "Ptr["u8 + TValue.kind(x) + "]"u8; }

    public Ptr<T, TValue> wrap(nint x) => new(x);
}

// CONTROL for MUST 2: a second witness for the SAME pair, as a caller-local emission would mint.
public readonly struct u8ValueΔshapeRogue : ValueΔshape<u8>
{
    public static @string kind(u8 self) => self.kind();
}

public partial struct Array<T, [GoShapeWitness] TValue> : Value
    where TValue : struct, ValueΔshape<T>
{
    internal slice<T> vals;

    public @string kind() { T x = default!; return "Array["u8 + TValue.kind(x) + "]"u8; }
}

public static partial class main_package
{
    public static Array<T, TValue> ConstArray<T, [GoShapeWitness] TValue>(slice<T> vals, @string name)
        where TValue : struct, ValueΔshape<T>
    {
        Array<T, TValue> y = default;
        y.vals = vals;
        return y;
    }

    // SHOULD 4: Go's Deref[W wrap[T], T Value](ptr Ptr[W]) T. Ptr[W] needs W's Value witness, and the w.wrap call needs W's
    // wrap witness: two witness parameters for W, four type parameters in all.
    public static T Deref<W, T, [GoShapeWitness] WValue, [GoShapeWitness] WWrap>(Ptr<W, WValue> ptr)
        where WValue : struct, ValueΔshape<W>
        where WWrap : struct, ΔwrapΔshape<W, T>
    {
        W w = default!;
        return WWrap.wrap(w, ptr.off + 1);
    }

    // The Go name of a C# type, DERIVED from the GoShapeWitness markers: every marked generic parameter is dropped, at
    // every nesting depth.
    static string GoName(Type t)
    {
        if (!t.IsGenericType)
            return "main." + t.Name;
        var def = t.GetGenericTypeDefinition();
        var args = t.GetGenericArguments();
        if (def == typeof(slice<>)) return "[]" + GoName(args[0]);
        if (def == typeof(ж<>)) return "*" + GoName(args[0]);
        var formals = def.GetGenericArguments();
        var kept = args.Where((_, i) => !formals[i].IsDefined(typeof(GoShapeWitnessAttribute), false)).Select(GoName);
        return "main." + def.Name[..def.Name.IndexOf('`')] + "[" + string.Join(",", kept) + "]";
    }

    // Go names every generic function instantiation `pkg.F[...]`; the marker keeps witness names out of it.
    static string GoFuncName(MethodInfo m) => "main." + m.Name + (m.IsGenericMethod ? "[...]" : "");

    static string B(bool b) => b ? "true" : "false";

    static string CsName(Type t) =>
        !t.IsGenericType ? t.Name : t.Name[..t.Name.IndexOf('`')] + "<" + string.Join(", ", t.GetGenericArguments().Select(CsName)) + ">";

    public static void Main()
    {
        var a = Ꮡ(new SourceFile("a.ts"u8, "let x"u8));
        var spans = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 10);
        Console.WriteLine($"FromLSPRange: {len(spans)} {spans[0].Start} {B(spans[0].Script == a)}");
        spans[0].Script.Value.text = "changed through the result"u8;
        Console.WriteLine($"write through the result: {a.Value.text}");

        var files = new slice<ж<SourceFile>>(new[] { a, Ꮡ(new SourceFile("b.ts"u8, "yz"u8)) });
        var masked = FromLSPRangeForSourceFile(files, 0);
        Console.WriteLine($"masked site: {len(masked)} {masked[1].Start} {B(masked[1].Script == files[1])} {GoName(masked.GetType())}");

        var other = Ꮡ(new SourceFile("c.ts"u8, ""u8));
        renameFirst(files, other);
        Console.WriteLine($"callee wrote the caller's slice: {B(files[0] == other)}");
        var third = Ꮡ(new SourceFile("d.ts"u8, ""u8));
        Console.WriteLine($"witness callee wrote the caller's slice: {renameFirstNamed<ж<SourceFile>, SourceFileжScriptΔshape>(files, third)} {B(files[0] == third)}");

        Console.WriteLine($"dump through any (closed selection): {B(dump<ж<SourceFile>, SourceFileжScriptΔshape>(a) is slice<MappedSpan<ж<SourceFile>>>)}");

        registry r = default;
        r.exports.entries = append(r.exports.entries, Ꮡ(new Export("x"u8)), Ꮡ(new Export("y"u8)));
        Console.WriteLine($"index field: {len(r.exports.Find("x"u8))} {B(r.exports.Find("x"u8)[0] == r.exports.entries[0])} {GoName(r.exports.GetType())}");

        var p = new Ptr<u8, ValueΔshapeOf<u8>>(3);
        Console.WriteLine($"gen Ptr: {p.kind()} {GoName(p.GetType())}");
        var arr = ConstArray<u8, ValueΔshapeOf<u8>>(new slice<u8>(new[] { new u8(1), new u8(2) }), "k"u8);
        arr.vals[0].v = 9;
        Console.WriteLine($"gen ConstArray: {arr.kind()} {len(arr.vals)} {GoName(arr.GetType())}");
        var v = Deref<Ptr<u8, ValueΔshapeOf<u8>>, Ptr<u8, ValueΔshapeOf<u8>>,
                      ValueΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>>,
                      ΔwrapΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>, Ptr<u8, ValueΔshapeOf<u8>>>>(
            new Ptr<Ptr<u8, ValueΔshapeOf<u8>>, ValueΔshapeOf<Ptr<u8, ValueΔshapeOf<u8>>>>(4));
        Console.WriteLine($"gen Deref: {v.off} {v.kind()} {GoName(v.GetType())}");
        Console.WriteLine($"nested type name: {GoName(new slice<Ptr<u8, ValueΔshapeOf<u8>>>(new[] { p }).GetType())}");
        Func<ж<SourceFile>, nint, slice<MappedSpan<ж<SourceFile>>>> f = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>;
        Console.WriteLine($"generic func name: {GoFuncName(f.Method)}");

        // CONTROL arms (stderr): what each rule prevents.
        Console.Error.WriteLine($"CONTROL MUST 1 (unclosed dump, T = proxy): assertion to []MappedSpan[*SourceFile] = {B(dumpUnclosed(new SourceFileжScriptᴾ(a)) is slice<MappedSpan<ж<SourceFile>>>)} (Go: true)");
        object rogue = ConstArray<u8, u8ValueΔshapeRogue>(new slice<u8>(new[] { new u8(1) }), "k"u8);
        Console.Error.WriteLine($"CONTROL MUST 2 (caller-local witness): same Go type Array[u8], C# identity equal = {B(rogue is Array<u8, ValueΔshapeOf<u8>>)} (Go: true)");
        Console.Error.WriteLine($"C# identities: {CsName(r.exports.GetType())} | {CsName(v.GetType())} | {CsName(rogue.GetType())}");
    }
}
