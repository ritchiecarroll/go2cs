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
using static go.go.scanner_package;

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
[assembly: GoImplement<ErrorList, error>]
[assembly: GoImplement<ErrorList, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<ErrorList, sort_package.Interface>]
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
[assembly: GoFieldAccess(typeof(Error))]
[assembly: GoFieldAccess(typeof(Scanner))]
[assembly: GoFieldAccess(typeof(go.go.token_package.Position))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/scanner/errors.go", "errors.cs", "ABkwkqaUAAoQkqiAloCClIKCuIKUgpSClKyyqLKCgoKCgoKCpuiSlKSUqqKClMyygoKUgg==")]
[assembly: GoPositionMap("go/scanner/scanner.go", "scanner.cs", "ADZokgARHtKCgoKClJKUxoKCgqqClJSCtoKUgoKClLyigpQADTQAEQSClIKCgoKUgoKCgoKUgoK4goKUpqLsxIKCgpSmgoKClKaCgpS4goKCgoKClIKCgoK4lILugoLKgqaCpsa6xIKUgpSCgriUgqyykoKUgoKCgpSmpoKCyoKCgriCgrjGgoKCpoKmgqaCAAQQ0gAIEoKUlILugoKCyoKClJSCgpSCxoKUpJSmgoIAAxAACQKCgoKCgoKClIKmgoKClIKmAAoGgoKUgoKCpoKCgoKUgrSCtIK0gra4goKClIKmgriClLSkgoKClIKCgpSCuIKCpoKClIKCuAAOBoKUpKSUqJKCgqaCgoKCyoKCgpSCxrSClLaCpgAMDsKUgpKUgqS0grSCtIK0goKUgqaCgoKCgoKUgpSCgqaCgqYACAaUlIKCgoKUgoKUlIKClIKCgtyCpuaUlIKCgoKUgoKUgrjGgoKC3IKCpgAIBpSUgoKCgoKUgoKUgriCgoKmpoKCAAMU0oKClKaCgoKUgoKUpoKCgpSCgoKClJQAAkIAIgKCpoKCpqamgqSClIKUtoLGgrSClIKClOqCpIKCgrSCgrSCgoKCtMaCgoKCxrSCtLSCtLSCtLSCtIKCxoKCxrSUgqaClJSUlIKmxrS0goKUxrS0tIKClMa0xqaClKaCgsaCpg==", "53-55:1")]
// </GoSourcePositionMaps>

namespace go.go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using token = global::go.go.token_package;
using global::go.go;
using static scanner_package;
using global::go.go.@internal;
using global::go.unicode;
using path;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

[GoPackage("scanner")]
public static partial class scanner_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct Error {}
    public partial struct ErrorList {}
    public partial struct Mode {}
    public partial struct Scanner {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸinternalꓸscannerhooks() => builtin.initPackage(typeof(global::go.go.@internal.scannerhooks_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(scanner_package.Error)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class scannerᴛError;
[GoMethodHost(typeof(scanner_package.ErrorList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class scannerᴛErrorList;
[GoMethodHost(typeof(scanner_package.Scanner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class scannerᴛScanner;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class scannerᴛError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this Error e) => scanner_package.Errorᴍ(e);
}
partial class scannerᴛErrorList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ref ErrorList p, token.Position pos, @string msg) => scanner_package.Addᴍ(ref p, pos, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref ErrorList p) => scanner_package.Resetᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ErrorList p) => scanner_package.Lenᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Swap(this ErrorList p, nint i, nint j) => scanner_package.Swapᴍ(p, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Less(this ErrorList p, nint i, nint j) => scanner_package.Lessᴍ(p, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Sort(this ErrorList p) => scanner_package.Sortᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RemoveMultiples(this ж<ErrorList> Ꮡp) => scanner_package.RemoveMultiplesᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ErrorList p) => scanner_package.Errorᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Err(this ErrorList p) => scanner_package.Errᴍ(p);
}
partial class scannerᴛScanner {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ref Scanner s) => scanner_package.nextᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte peek(this ref Scanner s) => scanner_package.peekᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Init(this ref Scanner s, ж<token.File> Ꮡfile, slice<byte> src, Action<token.Position, @string> err, Mode mode) => scanner_package.Initᴍ(ref s, Ꮡfile, src, err, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ref Scanner s, nint offs, @string msg) => scanner_package.errorᴍ(ref s, offs, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ref Scanner s, nint offs, @string format, params ꓸꓸꓸany argsʗp) => scanner_package.errorfᴍ(ref s, offs, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, nint) scanComment(this ref Scanner s) => scanner_package.scanCommentᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void updateLineInfo(this ref Scanner s, nint next, nint offs, slice<byte> text) => scanner_package.updateLineInfoᴍ(ref s, next, offs, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string scanIdentifier(this ref Scanner s) => scanner_package.scanIdentifierᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*digsep*/ digits(this ref Scanner s, nint @base, ж<nint> Ꮡinvalid) => scanner_package.digitsᴍ(ref s, @base, Ꮡinvalid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (token.Token, @string) scanNumber(this ref Scanner s) => scanner_package.scanNumberᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool scanEscape(this ref Scanner s, rune quote) => scanner_package.scanEscapeᴍ(ref s, quote);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string scanRune(this ref Scanner s) => scanner_package.scanRuneᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string scanString(this ref Scanner s) => scanner_package.scanStringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, nint) scanRawString(this ref Scanner s) => scanner_package.scanRawStringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void skipWhitespace(this ref Scanner s) => scanner_package.skipWhitespaceᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Token switch2(this ref Scanner s, token.Token tok0, token.Token tok1) => scanner_package.switch2ᴍ(ref s, tok0, tok1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Token switch3(this ref Scanner s, token.Token tok0, token.Token tok1, rune ch2, token.Token tok2) => scanner_package.switch3ᴍ(ref s, tok0, tok1, ch2, tok2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Token switch4(this ref Scanner s, token.Token tok0, token.Token tok1, rune ch2, token.Token tok2, token.Token tok3) => scanner_package.switch4ᴍ(ref s, tok0, tok1, ch2, tok2, tok3);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (token.Pos pos, token.Token tok, @string lit) Scan(this ref Scanner s) => scanner_package.Scanᴍ(ref s);
}
// </GlyphForwarders>
