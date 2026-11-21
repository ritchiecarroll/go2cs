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
// </ImportedTypeAliases>

using go;
using static go.strings_package;

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
[assembly: GoPositionMap("strings/builder.go", "builder.cs", "ABxAAAgC3IKCupKogJqglpKCqqKCgqzSgoKUgrzCgoKqwoKCqsKCgoKqwoKC")]
[assembly: GoPositionMap("strings/clone.go", "clone.cs", "AAoqAAoC")]
[assembly: GoPositionMap("strings/compare.go", "compare.cs", "AAoe4g==")]
[assembly: GoPositionMap("strings/iter.go", "iter.cs", "AAwk0oKCgoKUlIIABRCigoKCgoKUlJSCgoKUgoKUlAADEMKuwq7CgoKCgoKCgoKUgoKClJSClJSCAAQSwoKCgoKCgoKUlIKUlII=", "19-31:1;37-60:1;84-109:1;117-136:1")]
[assembly: GoPositionMap("strings/reader.go", "reader.cs", "ABkyooKUrrCWwoKUgoKC6NSClIKUgoKUqJKCgpSCguiSgpSCgqjSgoKUgoKCAAgIkoKUgpSCggAICJKCgpS0tLSUgpSCqMKCgpSCgoKUgoKClKiAuJA=")]
[assembly: GoPositionMap("strings/replace.go", "replace.cs", "ABpAAAgCgpTGgoKmgoKCpoKCgpSCuIKCgriCgoKUpqaCgpS4lJaosoKosoIAOG6igoKClKaEkoKCpoKCiLKClMqCgoKCgoKmuIKClJSCgpSUgoLY9oKCgoKCgoKCpoKUgoKClIKCgoKCgpSmAA8cgpSCgoK4gqaCgoKUgriUgpTskoKokoIACg6CxoKCgpTGgoKCpsKCkoKUgoKCgsqCgoKCgoKUgoKClIKClJSCgpQADBaCxoKCkoKCgpSCgoKClIKUgqaygpKCgoKUgoKClIKCgpSUgoIACxCCgoKCgoKUpoKUpqKCgoKCgpSCgoKCpoKCgoKmgoKCgqYAGzCCgpSCgpSCuoKClIK4gpSCgoKCgpSCpqaygoKCgoKUgoKCgqaCgoKCpoKCgpQ=")]
[assembly: GoPositionMap("strings/search.go", "search.cs", "ADBggsq4gsqC3IKCgqamgoKUuKaigoKmyqKClIKCgpSClJQ=")]
[assembly: GoPositionMap("strings/strings.go", "strings.cs", "ABcusoKClIKCgoKUgpSqtIKUgpSCgoKClIK6kqiSqJKokqiSgpSkpIKUpKaCgoKClIKUgoKCgoKmqJKuwoKUpIKCpqTagoKCgoKCgoKUpoKCpoKCgoKEppSEkoKmgoKCgoKCgqbYvKKUlJSCgpSUgoKCgqamgoKmrLKUlIKCgpSClJSCgoKCpqaCgoKUgoKCgqaUgoKCgqaokqqigpSClIKmgpSCgoKCgoKUgoKUggACHgALAAABGAAKAgACHgALAAABGAAKAqae9oKUgoKCgoKCppSmgoKClIKUgoKCgpSCgpSClJSClAAJFgAKBgAFFIKCgoK4poLcgriCgqaqopSkpoKCgpSUgoKUpoKCgoKClKiSqJKs/rSigoKmgoKCgqamgoKCpoK4gqaClLiCpsoAETjClKTcgpSCgpSUgriUlKSkpKQABBwACgKCgoKCuIKCgoKClKiSgoKCgoKUpoKClJiCgoKCgoKUgqaClJSokoKCgoKClKaCgpSYgoKCgoKClIKmgpSUqpCYorqiuqK6opSCgqaCgoKCgsqCpoKCgoKCgoKUgoKCgoKUlIKCpqq0gpSkpKSUpoKmAAIQAAgIgpSCgpSCzqKCgpSqooKCgpSUqqKqoqqirLKCgqassoKCgoKmAAwewoKCgpSUyJLKooKUgpSSlK7CgpSClJKUpoKClKaigoKUlKaCgoKClJSuwoKUgpSSlKaCgpSmooKClJSmgoKSgpSClJSsxIKmlIKUlIKCgpS4uKqiqqIAAhLigriCgoKokoKCgoKCgoKmgoKCgoKCpoIAAhDSrMSCgoKCgriCuIKmgpSmlIKCgrSCuILKgriCppSClMqCgpSClLiokq7CrsKuwg==", "874-881:1", "773=unicode.SpecialCase.ToUpper;779=unicode.SpecialCase.ToLower;785=unicode.SpecialCase.ToTitle")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using go.unicode;
using static strings_package;
using errors = errors_package;
using io = io_package;
using Δsync = sync_package;
using ꓸꓸꓸstring = Span<@string>;
using stringslite = @internal.stringslite_package;
using bits = math.bits_package;
using Δunicode = unicode_package;
using math;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(strings_package.Builder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛBuilder;
[GoMethodHost(typeof(strings_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛReader;
[GoMethodHost(typeof(strings_package.Replacer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛReplacer;
[GoMethodHost(typeof(strings_package.appendSliceWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛappendSliceWriter;
[GoMethodHost(typeof(strings_package.asciiSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛasciiSet;
[GoMethodHost(typeof(strings_package.byteReplacer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛbyteReplacer;
[GoMethodHost(typeof(strings_package.byteStringReplacer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛbyteStringReplacer;
[GoMethodHost(typeof(strings_package.genericReplacer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛgenericReplacer;
[GoMethodHost(typeof(strings_package.singleStringReplacer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛsingleStringReplacer;
[GoMethodHost(typeof(strings_package.stringFinder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛstringFinder;
[GoMethodHost(typeof(strings_package.stringWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛstringWriter;
[GoMethodHost(typeof(strings_package.trieNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class stringsᴛtrieNode;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class stringsᴛBuilder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyCheck(this ж<Builder> Ꮡb) => strings_package.copyCheckᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Builder b) => strings_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Builder b) => strings_package.Lenᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Cap(this ref Builder b) => strings_package.Capᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Builder b) => strings_package.Resetᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void grow(this ref Builder b, nint n) => strings_package.growᴍ(ref b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Grow(this ж<Builder> Ꮡb, nint n) => strings_package.Growᴍ(Ꮡb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Write(this ж<Builder> Ꮡb, slice<byte> p) => strings_package.Writeᴍ(Ꮡb, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteByte(this ж<Builder> Ꮡb, byte c) => strings_package.WriteByteᴍ(Ꮡb, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteRune(this ж<Builder> Ꮡb, rune r) => strings_package.WriteRuneᴍ(Ꮡb, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s) => strings_package.WriteStringᴍ(Ꮡb, s);
}
partial class stringsᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Reader r) => strings_package.Lenᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Size(this ref Reader r) => strings_package.Sizeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Read(this ref Reader r, slice<byte> b) => strings_package.Readᴍ(ref r, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) ReadAt(this ref Reader r, slice<byte> b, int64 off) => strings_package.ReadAtᴍ(ref r, b, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (byte, error) ReadByte(this ref Reader r) => strings_package.ReadByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnreadByte(this ref Reader r) => strings_package.UnreadByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (rune ch, nint size, error err) ReadRune(this ref Reader r) => strings_package.ReadRuneᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnreadRune(this ref Reader r) => strings_package.UnreadRuneᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) Seek(this ref Reader r, int64 offset, nint whence) => strings_package.Seekᴍ(ref r, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Reader r, io.Writer w) => strings_package.WriteToᴍ(ref r, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Reader r, @string s) => strings_package.Resetᴍ(ref r, s);
}
partial class stringsᴛReplacer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void buildOnce(this ref Replacer r) => strings_package.buildOnceᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static replacer build(this ref Replacer b) => strings_package.buildᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Replace(this ж<Replacer> Ꮡr, @string s) => strings_package.Replaceᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) WriteString(this ж<Replacer> Ꮡr, io.Writer w, @string s) => strings_package.WriteStringᴍ(Ꮡr, w, s);
}
partial class stringsᴛtrieNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref trieNode t, @string key, @string val, nint priority, ж<genericReplacer> Ꮡr) => strings_package.addᴍ(ref t, key, val, priority, Ꮡr);
}
partial class stringsᴛgenericReplacer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string val, nint keylen, bool found) lookup(this ж<genericReplacer> Ꮡr, @string s, bool ignoreRoot) => strings_package.lookupᴍ(Ꮡr, s, ignoreRoot);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Replace(this ж<genericReplacer> Ꮡr, @string s) => strings_package.Replaceᴍ(Ꮡr, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ж<genericReplacer> Ꮡr, io.Writer w, @string s) => strings_package.WriteStringᴍ(Ꮡr, w, s);
}
partial class stringsᴛappendSliceWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref appendSliceWriter w, slice<byte> p) => strings_package.Writeᴍ(ref w, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) WriteString(this ref appendSliceWriter w, @string s) => strings_package.WriteStringᴍ(ref w, s);
}
partial class stringsᴛstringWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) WriteString(this stringWriter w, @string s) => strings_package.WriteStringᴍ(w, s);
}
partial class stringsᴛsingleStringReplacer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Replace(this ref singleStringReplacer r, @string s) => strings_package.Replaceᴍ(ref r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref singleStringReplacer r, io.Writer w, @string s) => strings_package.WriteStringᴍ(ref r, w, s);
}
partial class stringsᴛbyteReplacer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Replace(this ref byteReplacer r, @string s) => strings_package.Replaceᴍ(ref r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref byteReplacer r, io.Writer w, @string s) => strings_package.WriteStringᴍ(ref r, w, s);
}
partial class stringsᴛbyteStringReplacer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Replace(this ref byteStringReplacer r, @string s) => strings_package.Replaceᴍ(ref r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) WriteString(this ref byteStringReplacer r, io.Writer w, @string s) => strings_package.WriteStringᴍ(ref r, w, s);
}
partial class stringsᴛstringFinder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint next(this ref stringFinder f, @string text) => strings_package.nextᴍ(ref f, text);
}
partial class stringsᴛasciiSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool contains(this ref asciiSet @as, byte c) => strings_package.containsᴍ(ref @as, c);
}
// </GlyphForwarders>
