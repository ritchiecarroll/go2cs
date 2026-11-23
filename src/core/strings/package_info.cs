// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.strings_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<appendSliceWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<byteReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<byteStringReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<genericReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<singleStringReplacer, replacer>(Pointer = true)]
[assembly: GoImplement<stringWriter, io_package.StringWriter>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Replacer))]
[assembly: GoFieldAccess(typeof(genericReplacer))]
[assembly: GoFieldAccess(typeof(stringFinder))]
[assembly: GoFieldAccess(typeof(trieNode))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("strings/builder.go", "builder.cs", "ABlAAAgC3IKCupKogJqglpKCqqKCgqzSgoKUgrzCgoKqwoKCqsKCgoKqwoKC")]
[assembly: GoPositionMap("strings/clone.go", "clone.cs", "AAoqAAoC")]
[assembly: GoPositionMap("strings/compare.go", "compare.cs", "AAoe4g==")]
[assembly: GoPositionMap("strings/iter.go", "iter.cs", "AAwk0oKCgoKUlIIABRCigoKCgoKUlJSCgoKUgoKUlAADEMKuwq7CgoKCgoKCgoKUgoKClJSClJSCAAQSwoKCgoKCgoKUlIKUlII=", "19-31:1;37-60:1;84-109:1;117-136:1")]
[assembly: GoPositionMap("strings/reader.go", "reader.cs", "ABYyooKUrrCWwoKUgoKC6NSClIKUgoKUqJKCgpSCguiSgpSCgqjSgoKUgoKCAAgIkoKUgpSCggAICJKCgpS0tLSUgpSCqMKCgpSCgoKUgoKClKiAmJA=")]
[assembly: GoPositionMap("strings/replace.go", "replace.cs", "ABlAAAgCgpSmgoKmgoKCpoKCgpSCuIKCgriCgoKUpqaCgpS4lJaosoKosoIANG6igoKClKaEkoKCpoKCiLKClMqCgoKCgoKmuIKClJSCgpSUgoK49oKCgoKCgoKCpoKUgoKClIKCgoKCgpSmAA0cgpSCgoK4gqaCgoKUgriUgpTMkoKokoLugqaCgoKUpoKCgqbCgpKClIKCgoLKgoKCgoKClIKCgpSCgpSUgoKUAAoWgqaCgpKCgoKUgoKCgpSClIKmsoKSgoKClIKCgpSCgoKUlIKCAAcQgoKCgoKClKaClKaigoKCgoKUgoKCgqaCgoKCpoKCgoKmABcwgoKUgoKUgrqCgpSCuIKUgoKCgoKUgqamsoKCgoKClIKCgoKmgoKCgqaCgoKU")]
[assembly: GoPositionMap("strings/search.go", "search.cs", "AC5ggsq4gsqC3IKCgqamgoKUuKaigoKmqqKClIKCgpSClJQ=")]
[assembly: GoPositionMap("strings/strings.go", "strings.cs", "ABYusoKClIKCgoKUgpSqtIKUgpSCgoKClIK6kqiSqJKokqiSgpSkpIKUpKaCgoKClIKUgoKCgoKmqJKuwoKUpIKCpqTagoKCgoKCgoKUpoKCpoKCgoKEppSEkoKmgoKCgoKCgqbYvKKUlJSCgpSUgoKCgqamgoKmrLKUlIKCgpSClJSCgoKCpqaCgoKUgoKCgqaUgoKCgqaokqqigpSClIKmgpSCgoKCgoKUgoKUggACHgALAAABGAAKAgACHgALAAABGAAKAqae9oKUgoKCgoKCppSmgoKClIKUgoKCgpSCgpSClJSClAAJFgAKBgAFFIKCgoK4poLcgriCgqaqopSkpoKCgpSUgoKUpoKCgoKClKiSqJKs/rSigoKmgoKCgqamgoKCpoK4gqaClLiCpsoAETjClKTcgpSCgpSUgriUlKSkpKQABBwACgKCgoKCuIKCgoKClKiSgoKCgoKUpoKClJiCgoKCgoKUgqaClJSokoKCgoKClKaCgpSYgoKCgoKClIKmgpSUqpCYorqiuqK6opSCgqaCgoKCgsqCpoKCgoKCgoKUgoKCgoKUlIKCpqq0gpSkpKSUpoKmAAIQAAgIgpSCgpSCzqKCgpSqooKCgpSUqqKqoqqirLKCgqassoKCgoKmAAwewoKCgpSUqJKqooKUgpSSlK7CgpSClJKUpoKClKaigoKUlKaCgoKClJSuwoKUgpSSlKaCgpSmooKClJSmgoKSgpSClJSsxIKmlIKUlIKCgpS4uKqiqqIAAhLigriCgoKokoKCgoKCgoKmgoKCgoKCpoIAAhDSrMSCgoKCgriCuIKmgpSmlIKCgrSCuILKgriCppSClMqCgpSClLiokq7CrsKuwg==", "874-881:1", "773=unicode.SpecialCase.ToUpper;779=unicode.SpecialCase.ToLower;785=unicode.SpecialCase.ToTitle")]
// </GoSourcePositionMaps>

namespace go;

using static go.strings_package;
using io = go.io_package;

[GoPackage("strings")]
public static partial class strings_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface replacer {}
    [GoLocalName("span")] internal partial struct FieldsFunc_span {}
    internal partial struct appendSliceWriter {}
    internal partial struct asciiSet {}
    internal partial struct byteReplacer {}
    [GoValueClone("replacements")] internal partial struct byteStringReplacer {}
    [GoValueClone("mapping")] internal partial struct genericReplacer {}
    internal partial struct singleStringReplacer {}
    [GoValueClone("badCharSkip")] internal partial struct stringFinder {}
    internal partial struct stringWriter {}
    internal partial struct trieNode {}
    public partial struct Builder {}
    public partial struct Reader {}
    public partial struct Replacer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(strings_package.Builder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛBuilder;
[GoMethodHost(typeof(strings_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛReader;
[GoMethodHost(typeof(strings_package.Replacer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛReplacer;
[GoMethodHost(typeof(strings_package.appendSliceWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛappendSliceWriter;
[GoMethodHost(typeof(strings_package.asciiSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛasciiSet;
[GoMethodHost(typeof(strings_package.byteReplacer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛbyteReplacer;
[GoMethodHost(typeof(strings_package.byteStringReplacer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛbyteStringReplacer;
[GoMethodHost(typeof(strings_package.genericReplacer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛgenericReplacer;
[GoMethodHost(typeof(strings_package.singleStringReplacer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛsingleStringReplacer;
[GoMethodHost(typeof(strings_package.stringFinder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛstringFinder;
[GoMethodHost(typeof(strings_package.stringWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛstringWriter;
[GoMethodHost(typeof(strings_package.trieNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛtrieNode;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class stringsᴛBuilder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyCheck(this ж<Builder> Ꮡb) => strings_package.copyCheck⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Builder b) => strings_package.String⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Builder b) => strings_package.Len⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cap(this ref Builder b) => strings_package.Cap⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Builder b) => strings_package.Reset⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this ref Builder b, nint n) => strings_package.grow⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Grow(this ж<Builder> Ꮡb, nint n) => strings_package.Grow⁔(Ꮡb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ж<Builder> Ꮡb, slice<byte> p) => strings_package.Write⁔(Ꮡb, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteByte(this ж<Builder> Ꮡb, byte c) => strings_package.WriteByte⁔(Ꮡb, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteRune(this ж<Builder> Ꮡb, rune r) => strings_package.WriteRune⁔(Ꮡb, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s) => strings_package.WriteString⁔(Ꮡb, s);
}
partial class stringsᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Reader r) => strings_package.Len⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref Reader r) => strings_package.Size⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Reader r, slice<byte> b) => strings_package.Read⁔(ref r, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) ReadAt(this ref Reader r, slice<byte> b, int64 off) => strings_package.ReadAt⁔(ref r, b, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (byte, error) ReadByte(this ref Reader r) => strings_package.ReadByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadByte(this ref Reader r) => strings_package.UnreadByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (rune ch, nint size, error err) ReadRune(this ref Reader r) => strings_package.ReadRune⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadRune(this ref Reader r) => strings_package.UnreadRune⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ref Reader r, int64 offset, nint whence) => strings_package.Seek⁔(ref r, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Reader r, io.Writer w) => strings_package.WriteTo⁔(ref r, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Reader r, @string s) => strings_package.Reset⁔(ref r, s);
}
partial class stringsᴛReplacer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void buildOnce(this ref Replacer r) => strings_package.buildOnce⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static replacer build(this ref Replacer b) => strings_package.build⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Replace(this ж<Replacer> Ꮡr, @string s) => strings_package.Replace⁔(Ꮡr, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) WriteString(this ж<Replacer> Ꮡr, io.Writer w, @string s) => strings_package.WriteString⁔(Ꮡr, w, s);
}
partial class stringsᴛappendSliceWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref appendSliceWriter w, slice<byte> p) => strings_package.Write⁔(ref w, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) WriteString(this ref appendSliceWriter w, @string s) => strings_package.WriteString⁔(ref w, s);
}
partial class stringsᴛasciiSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool contains(this ref asciiSet @as, byte c) => strings_package.contains⁔(ref @as, c);
}
partial class stringsᴛbyteReplacer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Replace(this ref byteReplacer r, @string s) => strings_package.Replace⁔(ref r, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref byteReplacer r, io.Writer w, @string s) => strings_package.WriteString⁔(ref r, w, s);
}
partial class stringsᴛbyteStringReplacer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Replace(this ref byteStringReplacer r, @string s) => strings_package.Replace⁔(ref r, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref byteStringReplacer r, io.Writer w, @string s) => strings_package.WriteString⁔(ref r, w, s);
}
partial class stringsᴛgenericReplacer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string val, nint keylen, bool found) lookup(this ж<genericReplacer> Ꮡr, @string s, bool ignoreRoot) => strings_package.lookup⁔(Ꮡr, s, ignoreRoot);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Replace(this ж<genericReplacer> Ꮡr, @string s) => strings_package.Replace⁔(Ꮡr, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ж<genericReplacer> Ꮡr, io.Writer w, @string s) => strings_package.WriteString⁔(Ꮡr, w, s);
}
partial class stringsᴛsingleStringReplacer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Replace(this ref singleStringReplacer r, @string s) => strings_package.Replace⁔(ref r, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref singleStringReplacer r, io.Writer w, @string s) => strings_package.WriteString⁔(ref r, w, s);
}
partial class stringsᴛstringFinder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint next(this ref stringFinder f, @string text) => strings_package.next⁔(ref f, text);
}
partial class stringsᴛstringWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) WriteString(this stringWriter w, @string s) => strings_package.WriteString⁔(w, s);
}
partial class stringsᴛtrieNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref trieNode t, @string key, @string val, nint priority, ж<genericReplacer> Ꮡr) => strings_package.add⁔(ref t, key, val, priority, Ꮡr);
}
// </MethodForwarders>
