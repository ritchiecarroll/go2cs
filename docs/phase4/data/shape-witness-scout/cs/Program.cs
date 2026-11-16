// witnessprobe (C#): the NARROW shape-witness emission, written by hand as the converter would emit it, over the real
// golib (ж<T>, slice<T>, append, Ꮡ). It prints the Go reference's lines (../go/expected.txt); equal output is the bar.
// No proxy, no copy, no array pun: T is the box itself, and the constrained calls go through the witness.
using System;
using go;
using static go.builtin;

namespace go;

// ---- lsconv (the narrow witness form) ----

// The converted interface, unchanged (it still boxes any value assigned to Script).
public interface Script
{
    @string FileName();
    @string Text();
}

// The SHAPE WITNESS for Script: one static forwarder per method, taking the receiver as its first argument. It sits
// beside Script and is emitted only because a narrow declaration constrains by Script.
public interface ScriptΔshape<T>
{
    static abstract @string FileName(T self);
    static abstract @string Text(T self);
}

public partial struct SourceFile
{
    internal @string name;
    internal @string text;

    public SourceFile(@string name, @string text) { this.name = name; this.text = text; }
}

public static partial class main_package
{
    // Go: func (f *SourceFile) FileName() string / Text() string -- reached through the box.
    internal static @string FileName(this ж<SourceFile> f) => f.Value.name;
    internal static @string Text(this ж<SourceFile> f) => f.Value.text;
}

// The witness for *SourceFile against Script, generated per (element, interface) pair like today's adapters.
public readonly struct SourceFileжScriptΔshape : ScriptΔshape<ж<SourceFile>>
{
    public static @string FileName(ж<SourceFile> self) => self.FileName();
    public static @string Text(ж<SourceFile> self) => self.Text();
}

// MappedSpan has no methods, so it needs no clause and no witness: its identity is Go's MappedSpan[T] exactly.
public partial struct MappedSpan<T>
{
    public T Script;
    public nint Start;

    public MappedSpan(T Script, nint Start) { this.Script = Script; this.Start = Start; }
}

public static partial class main_package
{
    // Go: func FromLSPRange[T Script](script T, start int) []MappedSpan[T]
    public static slice<MappedSpan<T>> FromLSPRange<T, TScript>(T script, nint start)
        where TScript : struct, ScriptΔshape<T>
    {
        return lspRangeToVirtual<T, TScript>(new slice<T>(new[] { script }), start);
    }

    // Go: func lspRangeToVirtual[T Script](scripts []T, start int) []MappedSpan[T]
    internal static slice<MappedSpan<T>> lspRangeToVirtual<T, TScript>(slice<T> scripts, nint start)
        where TScript : struct, ScriptΔshape<T>
    {
        slice<MappedSpan<T>> @out = default!;
        foreach (var (i, s) in range(scripts)) {
            @out = append(@out, new MappedSpan<T>(s, start + i + len(TScript.Text(s))));
        }
        return @out;
    }

    // converters.go:136's shape, the masked site: a non-generic caller instantiating with the pointer.
    public static slice<MappedSpan<ж<SourceFile>>> FromLSPRangeForSourceFile(slice<ж<SourceFile>> files, nint start)
    {
        return lspRangeToVirtual<ж<SourceFile>, SourceFileжScriptΔshape>(files, start);
    }

    // A callee that writes its []T parameter (no method call, so no witness: the box binds, as class B's elided clause).
    internal static void renameFirst<T>(slice<T> scripts, T other) { scripts[0] = other; }

    // The same write from a callee that calls a method, so it takes the witness: the caller's slice, not a copy.
    internal static @string renameFirstNamed<T, TScript>(slice<T> scripts, T other)
        where TScript : struct, ScriptΔshape<T>
    {
        scripts[0] = other;
        return TScript.FileName(other);
    }
}

// ---- gc/internal/gen (the narrow witness form) ----

public interface Value { @string kind(); }

public interface ValueΔshape<T>
{
    static abstract @string kind(T self);
}

// A value type implements the converted interface directly, so ONE generic witness, emitted beside Value, serves every
// such T: it is canonical (the same type wherever it is named), which keeps Ptr<T, ...>'s identity single.
public readonly struct ValueΔshapeOf<T> : ValueΔshape<T> where T : Value
{
    public static @string kind(T self) => self.kind();
}

public partial struct u8 : Value
{
    internal byte v;

    public u8(byte v) { this.v = v; }

    public @string kind() => "u8";
}

// Ptr's own methods call T's methods (`var x T; x.kind()`), so the TYPE carries the witness as a second parameter.
public partial struct Ptr<T, TValue> : Value
    where TValue : struct, ValueΔshape<T>
{
    internal nint off;

    public Ptr(nint off) { this.off = off; }

    public @string kind() { T x = default!; return "Ptr["u8 + TValue.kind(x) + "]"u8; }

    public Ptr<T, TValue> wrap(nint x) => new(x);
}

public partial struct Array<T, TValue> : Value
    where TValue : struct, ValueΔshape<T>
{
    internal slice<T> vals;

    public @string kind() { T x = default!; return "Array["u8 + TValue.kind(x) + "]"u8; }
}

public static partial class main_package
{
    // Go: func ConstArray[T Value](vals []T, name string) (y Array[T])
    public static Array<T, TValue> ConstArray<T, TValue>(slice<T> vals, @string name)
        where TValue : struct, ValueΔshape<T>
    {
        Array<T, TValue> y = default;
        y.vals = vals;
        return y;
    }

    static string GoName(Type t, int dropTrailing = 0)
    {
        if (!t.IsGenericType)
            return "main." + t.Name;
        var def = t.GetGenericTypeDefinition();
        var args = t.GetGenericArguments();
        if (def == typeof(slice<>)) return "[]" + GoName(args[0]);
        if (def == typeof(ж<>)) return "*" + GoName(args[0]);
        var shown = args[..^dropTrailing];
        return "main." + def.Name[..def.Name.IndexOf('`')] + "[" + string.Join(",", Array.ConvertAll(shown, a => GoName(a))) + "]";
    }

    static string CsName(Type t) =>
        !t.IsGenericType ? t.Name : t.Name[..t.Name.IndexOf('`')] + "<" + string.Join(", ", Array.ConvertAll(t.GetGenericArguments(), CsName)) + ">";

    public static void Main()
    {
        var a = Ꮡ(new SourceFile("a.ts"u8, "let x"u8));
        var spans = FromLSPRange<ж<SourceFile>, SourceFileжScriptΔshape>(a, 10);
        Console.WriteLine($"FromLSPRange: {len(spans)} {spans[0].Start} {(spans[0].Script == a ? "true" : "false")}");
        spans[0].Script.Value.text = "changed through the result"u8;
        Console.WriteLine($"write through the result: {a.Value.text}");

        var files = new slice<ж<SourceFile>>(new[] { a, Ꮡ(new SourceFile("b.ts"u8, "yz"u8)) });
        var masked = FromLSPRangeForSourceFile(files, 0);
        Console.WriteLine($"masked site: {len(masked)} {masked[1].Start} {(masked[1].Script == files[1] ? "true" : "false")} {GoName(masked.GetType())}");

        var other = Ꮡ(new SourceFile("c.ts"u8, ""u8));
        renameFirst(files, other);
        Console.WriteLine($"callee wrote the caller's slice: {(files[0] == other ? "true" : "false")}");
        var third = Ꮡ(new SourceFile("d.ts"u8, ""u8));
        Console.WriteLine($"witness callee wrote the caller's slice: {renameFirstNamed<ж<SourceFile>, SourceFileжScriptΔshape>(files, third)} {(files[0] == third ? "true" : "false")}");

        var p = new Ptr<u8, ValueΔshapeOf<u8>>(3);
        Console.WriteLine($"gen Ptr: {p.kind()} {GoName(p.GetType(), dropTrailing: 1)}");
        var arr = ConstArray<u8, ValueΔshapeOf<u8>>(new slice<u8>(new[] { new u8(1), new u8(2) }), "k"u8);
        arr.vals[0].v = 9;
        Console.WriteLine($"gen ConstArray: {arr.kind()} {len(arr.vals)} {GoName(arr.GetType(), dropTrailing: 1)}");

        // Not part of the Go comparison: the C# identities, so the witness parameter in Ptr/Array is visible.
        Console.Error.WriteLine($"C# identities: {CsName(masked.GetType())} | {CsName(p.GetType())} | {CsName(arr.GetType())}");
    }
}
