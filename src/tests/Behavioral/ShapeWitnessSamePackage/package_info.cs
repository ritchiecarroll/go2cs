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
using static go.main_package;

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
[assembly: GoImplement(typeof(Ptr<>), typeof(Value))]
[assembly: GoImplement(typeof(go.main_package.Array<,>), typeof(Value))]
[assembly: GoImplement<Export, Named>(Pointer = true)]
[assembly: GoImplement<SourceFile, Script>(Pointer = true)]
[assembly: GoImplement<go.main_package.Array<u8, ValueΔshapeOf<u8>>, Value>]
[assembly: GoImplement<memFile, Script>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<u8, Value>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Export))]
[assembly: GoFieldAccess(typeof(SourceFile))]
[assembly: GoFieldAccess(typeof(boxed))]
[assembly: GoFieldAccess(typeof(memSrc))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "ABMsggAGEKKmooKClKiCqJAACRD8kJSigoKCpqaQAA0U+KCAAAgIoICkwoIABhCC3JSCgoKCgpSCgsSClIKCgoKCgpSCgoKClIKClJSCtIKClIKCgpiCgpSCgpiCggAQLJSiuKCkmKKCpqKCgoKClA==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct boxed {}
    internal partial struct memFile {}
    internal partial struct memSrc {}
    internal partial struct registry {}
    internal partial struct u8 {}
    public partial interface Named {}
    public partial interface Script {}
    public partial interface Source {}
    public partial interface Value {}
    public partial struct Array<T, TΔValue> {}
    public partial struct Export {}
    public partial struct Index<T, TΔNamed> {}
    public partial struct MappedSpan<T> {}
    public partial struct Ptr<T> {}
    public partial struct SourceFile {}
    public partial struct Span<T> {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>

    // A generic declaration under a non-self method-set constraint takes a SHAPE WITNESS: its type
    // parameter stays the Go type itself, and a call through it goes through a witness struct, one per
    // (type, constraint) pair, declared once here so every use names the same C# type.

    // <ShapeWitnesses>
    internal readonly struct boxedжValueΔshape : ValueΔshape<ж<boxed>> { public static @string kind(ж<boxed> self) => self.kind(); }
    internal readonly struct memSrcжSourceΔshape : SourceΔshape<ж<memSrc>> { public static @string Join(ж<memSrc> self, @string sep, params global::go.variadic<@string> parts) => self.Join(sep, parts); public static io_package.Reader Open(ж<memSrc> self) => self.Open(); public static void Stamp(ж<memSrc> self, params global::go.variadic<@string> parts) => self.Stamp(parts); }
    public interface NamedΔshape<T> { static abstract @string Name(T self); }
    public interface ScriptΔshape<T> { static abstract @string FileName(T self); static abstract @string Text(T self); }
    public interface SourceΔshape<T> { static abstract @string Join(T self, @string sep, params global::go.variadic<@string> parts); static abstract io_package.Reader Open(T self); static abstract void Stamp(T self, params global::go.variadic<@string> parts); }
    public interface ValueΔshape<T> { static abstract @string kind(T self); }
    public readonly struct ExportжNamedΔshape : NamedΔshape<ж<Export>> { public static @string Name(ж<Export> self) => self.Name(); }
    public readonly struct NamedΔshapeOf<T> : NamedΔshape<T> where T : Named { public static @string Name(T self) => self.Name(); }
    public readonly struct ScriptΔshapeOf<T> : ScriptΔshape<T> where T : Script { public static @string FileName(T self) => self.FileName(); public static @string Text(T self) => self.Text(); }
    public readonly struct SourceFileжScriptΔshape : ScriptΔshape<ж<SourceFile>> { public static @string FileName(ж<SourceFile> self) => self.FileName(); public static @string Text(ж<SourceFile> self) => self.Text(); }
    public readonly struct SourceΔshapeOf<T> : SourceΔshape<T> where T : Source { public static @string Join(T self, @string sep, params global::go.variadic<@string> parts) => self.Join(sep, parts); public static io_package.Reader Open(T self) => self.Open(); public static void Stamp(T self, params global::go.variadic<@string> parts) => self.Stamp(parts); }
    public readonly struct ValueΔshapeOf<T> : ValueΔshape<T> where T : Value { public static @string kind(T self) => self.kind(); }
    // </ShapeWitnesses>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
// </MethodHolders>
