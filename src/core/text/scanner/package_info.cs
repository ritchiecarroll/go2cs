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
using static go.text.scanner_package;
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
[assembly: GoFieldAccess(typeof(Scanner))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("text/scanner/scanner.go", "scanner.cs", "ACRIgNSCgoKUgpQAJ1gAChiSgpQAPZYB0riCgqaCgoKCuKamgoKCgpQACA7ipJSmgoK4gtyCgoKCgoKClIKUlILcuIKUgpSCgoKC3IKCpqa0goK2AAISAAgCgoKCgpSs0pSCgqamooKCgoKUgoKUpqKmgoKUprSCgpSmgoIAARAACgKCgoKCgoKCgpSCpoKCgpSCpgAKBqKCgoKWkoKCgoKClIK0grSCtIK2goKCgsqCgoKUgqaCuIKUtKSCgoKUgoKClIKmgqaCgoK4AAwGgpSkpKS6koKCpoKCgoLKgoKClILGtIKUtoKmpoKUpJTmgoKClIKUpoKCprS0tLS0pOaigoKCgpSClJSUpoKCgoKClPiCgviUlIKClLiCgoKClIKCgoKmAAIQ8qaClISS2IK4gpSCyoK4gpSCgpTGgpTGlLSCgpS0goKUtIKCxoKCgoKClILGgoKUtOqUgq7igoKmgsaCxoKkqsKUppS4lMqCgg==")]
// </GoSourcePositionMaps>

namespace go.text;

using static go.text.scanner_package;
using io = go.io_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("scanner")]
public static partial class scanner_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Position {}
    [GoValueClone("srcBuf")] public partial struct Scanner {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
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
[GoMethodHost(typeof(scanner_package.Position)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class scannerᴛPosition;
[GoMethodHost(typeof(scanner_package.Scanner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class scannerᴛScanner;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class scannerᴛPosition {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsValid(this ref Position pos) => scanner_package.IsValid⁔(ref pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Position pos) => scanner_package.String⁔(pos);
}
partial class scannerᴛScanner {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scanner> Init(this ж<Scanner> Ꮡs, io.Reader src) => scanner_package.Init⁔(Ꮡs, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune next(this ж<Scanner> Ꮡs) => scanner_package.next⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static rune Next(this ж<Scanner> Ꮡs) => scanner_package.Next⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static rune Peek(this ж<Scanner> Ꮡs) => scanner_package.Peek⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ж<Scanner> Ꮡs, @string msg) => scanner_package.error⁔(Ꮡs, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ж<Scanner> Ꮡs, @string format, params ꓸꓸꓸany argsʗp) => scanner_package.errorf⁔(Ꮡs, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isIdentRune(this ref Scanner s, rune ch, nint i) => scanner_package.isIdentRune⁔(ref s, ch, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune scanIdentifier(this ж<Scanner> Ꮡs) => scanner_package.scanIdentifier⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune ch, nint digsep) digits(this ж<Scanner> Ꮡs, rune ch0, nint @base, ж<rune> Ꮡinvalid) => scanner_package.digits⁔(Ꮡs, ch0, @base, Ꮡinvalid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune, rune) scanNumber(this ж<Scanner> Ꮡs, rune ch, bool seenDot) => scanner_package.scanNumber⁔(Ꮡs, ch, seenDot);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune scanDigits(this ж<Scanner> Ꮡs, rune ch, nint @base, nint n) => scanner_package.scanDigits⁔(Ꮡs, ch, @base, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune scanEscape(this ж<Scanner> Ꮡs, rune quote) => scanner_package.scanEscape⁔(Ꮡs, quote);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*n*/ scanString(this ж<Scanner> Ꮡs, rune quote) => scanner_package.scanString⁔(Ꮡs, quote);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanRawString(this ж<Scanner> Ꮡs) => scanner_package.scanRawString⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanChar(this ж<Scanner> Ꮡs) => scanner_package.scanChar⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune scanComment(this ж<Scanner> Ꮡs, rune ch) => scanner_package.scanComment⁔(Ꮡs, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static rune Scan(this ж<Scanner> Ꮡs) => scanner_package.Scan⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Position /*pos*/ Pos(this ref Scanner s) => scanner_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string TokenText(this ж<Scanner> Ꮡs) => scanner_package.TokenText⁔(Ꮡs);
}
// </MethodForwarders>
