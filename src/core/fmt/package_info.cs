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
using static go.fmt_package;

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
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<pp, State>(Pointer = true)]
[assembly: GoImplement<readRune, io_package.RuneScanner>(Pointer = true)]
[assembly: GoImplement<ss, ScanState>(Pointer = true)]
[assembly: GoImplement<stringReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<wrapError, error>(Pointer = true)]
[assembly: GoImplement<wrapErrors, error>(Pointer = true)]
// </InterfaceImplementations>

// An exported function recorded here is an sstring twin: a @string member and a prioritized
// sstring member, so it has no single method group. A func value names its canonical delegate
// `<Name>ᶠ` instead. Go spellings. The section exists only while there is a record to hold.
// <SStringTwins>
[assembly: GoSStringTwin("Appendf")]
[assembly: GoSStringTwin("Errorf")]
[assembly: GoSStringTwin("Fprintf")]
[assembly: GoSStringTwin("Fscanf")]
[assembly: GoSStringTwin("Printf")]
[assembly: GoSStringTwin("Sprintf")]
[assembly: GoSStringTwin("Sscanf")]
// </SStringTwins>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(pp))]
[assembly: GoFieldAccess(typeof(ss))]
[assembly: GoFieldAccess(typeof(strconv_package.NumError))]
[assembly: GoFieldAccess(typeof(wrapError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("fmt/errors.go", "errors.cs", "AA4uAA8GgriosoKUgoKCgoKUtIKCtIKUgoKClIKmpIIACRCCpoIACxCCpoI=")]
[assembly: GoPositionMap("fmt/format.go", "format.cs", "ADJsgoKCpqKCqJKClIKClIKCppSCpoKClKiSgoKUgpSCpoK6koKClIKUgqaCAAkKkoKUupLKgoKUgoLKpoKCgoKCgoKCpoKCgoKUgoKUgoKCpoKCgpSCgoKokoKCpqaUgpTugoKUgoKCgoKUgoKC7qaUgoKCgsaCgoLGgoKCxoKCgsakgoKCgriCpoKCgrSCgtiCgoK2goKCgqaCgoKCgoKCgsqCgoKokoKCgoKCuKiSgoKCgoKUgqaokoKokoKokoKUpoKmgoKUgqaClKaClKaCpoKUlIKClIKUpoKUppSUgrqSqJKssoKCgpSCgpS8xoKClIKqooKClIKClLzUgqaCgpS4griCgpSClIKCuIKClJSCzKKUgpSCpKSCtIKCgpS0gqaCuJSClJSCgpSmuIKCgoKUgqY=")]
[assembly: GoPositionMap("fmt/print.go", "print.cs", "AFuiAQAIAoKCgoKmgIKkgIKCpIL8gqaCpoKmggAbPMqSgoKCgoLIAAoQgpSUgqaCgoKmlJSClKSkpKSUqqKCqqKCzgAIAoKCgoKqwqiygoKCgqrCgoKCggACEAAJAoKCgoKs0qrCgoKCgqzSgoKCggACFAALAoKCgoKs0qrCgoKCgqzSgoKCgqyygoKUqqKCqNKClIKClIKUxoKCgpSCgqaigoKCgpSCgrSCgrSkgqailLTMooKCgqiylIKUxrS0tLS0tLS0zMKUtLS0tM72lIKClIKCgrTIopSClMa0tLS0yKKUgoKCgpSCgoKUlJSCgoKUlMa0tLS0yKKClLSCppSCgoKCgpSUlIKU2LS0AAgI0riCgriUppSUgoKCgoKCgoKUtAAMBNKClJSCgoKmuICCgoKCyIKAgoKUguzclIKCgqaCgoLIAAoGooKUgpS0pMqUgqSCuJS0tLS0tLS0tLS0tLS0tLS0tNiCgoKm1qbe1IKCgqaClLSClJS02KSkpKSkpKSkgoKCgpSUlIKCgoKUpoKClIKUtoKUgoKCgpSmgoKCppSkgoKCgpSmtqaCgoKCyoKCpoK2goKCgpSCgoKUlJSCgoKUlNqCpIKCpqSk2tKCgoKUpIKCgsaCgoIACAyCgoKmAAIS9IK4goKCgpSmzLKClIKCgpSCpoKCgqaCgoKmooKCgoKCgoKCgpSClJS4poKCgoKUtLS0tNiClIK2gpSClIKCgqb6poKClILKgoKClJSCgsqCgoKUgoKClIKClIKUlIKCgsqCpoKCpoKUlKSkpIK2gpSCgqSCAAgOgoKCgoKUgpSCgqa4ooKClIKUgrzCgoKUlA==")]
[assembly: GoPositionMap("fmt/scan.go", "scan.cs", "AD9+4qrCAAIUAAkC6rKCgoKUzvKq0q7yrgAIAoKCgqrigoKCrgAIAoKCggAkRLKmwoKCpoKCgoKUgpSmgoKUqsKCgoKUlKzSgoKUpoKCgoKmgqaCpuKCgpKUuIKUgoIACQoADBqCgpSCooKUgqaokgAPINKCgoKClIKClKrigoKCgpSCgpSCgpSClIKCgoKCgpSmgoKCpoLmgoKmgsbKwoKClJSCgoKCgoKCgsjEgoKmgpSCguyygoKClIKUgoKUgpSCggAEELKCpoKCgpSCgpSUxoKUgoKCpsqigoKUgoKUlIKUqJKCgpSmlIKUqqKokoKCpoLokoKCgqaUpKSClKSClJQAHB7CgoKClIK0grSCpOiSgoKCppSokoKCgoKClKyygpSUlIKkgqSCpPyigpSCgoKCgoKmgoKmgoKClIKCgpSqooKUgoKCgoKClIKUgoKClIKCgpSsspSCppSClIKCgoKmppS4lJSmrPSCgpSCpoKCgpSClMaCgoKmyMimgpSClJSClIKUlJSCgpTuwoKUgoKCgoIACArCgpSCgpS0tKTokoKCpoKCgpSUtoKCgoK4goKmgoKUpKTIkoKUpKSUAAgM4oKClIKCgpSCgoKU6JKCgoKClJSCgpQAEhqSgoKCAAkKsoKUgIKCgoKUlLaUtLS0tLS0tLS0tLS0tNiCgoLGgoKCxtjEgoKCgpSktLS0xoKClIKCgsaCgrS0AAgM0oKCgoKUxgAIBgAIAoKCgqaCgoKClIKCuAAPFgAKAoIABxCCgoKCgoKUlIKUgoKClIKmgoKmgpSCpoKUgqa4lIKmgoKUuIKCgpSUAAgKAAkCgpSCgoKCppSCppSWkoKCpoKUgpSCgpSCgqaCgpSUgoKUgpQ=", "248-256:1", "", "", "1351:21,1490:21", "307:21,308:13")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using errors = errors_package;
using stringslite = @internal.stringslite_package;
using slices = slices_package;
using @internal;
using ꓸꓸꓸany = Span<any>;
using static fmt_package;
using strconv = strconv_package;
using utf8 = unicode.utf8_package;
using unicode;
using fmtsort = @internal.fmtsort_package;
using Δio = io_package;
using os = os_package;
using reflect = reflect_package;
using Δsync = sync_package;
using @unsafe = unsafe_package;
using Δmath = math_package;
// </GlyphForwarderUsings>

[GoPackage("fmt")]
public static partial class fmt_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct buffer {}
    [GoValueClone("intbuf")] internal partial struct fmt {}
    internal partial struct fmtFlags {}
    [GoValueClone("fmt")] internal partial struct pp {}
    [GoValueClone("buf", "pendBuf")] internal partial struct readRune {}
    internal partial struct scanError {}
    internal partial struct ss {}
    internal partial struct ssave {}
    internal partial struct stringReader {}
    internal partial struct wrapError {}
    internal partial struct wrapErrors {}
    public partial interface Formatter {}
    public partial interface GoStringer {}
    public partial interface ScanState {}
    public partial interface Scanner {}
    public partial interface State {}
    public partial interface Stringer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfmtsort() => builtin.initPackage(typeof(@internal.fmtsort_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fmt_package.buffer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛbuffer;
[GoMethodHost(typeof(fmt_package.fmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛfmt;
[GoMethodHost(typeof(fmt_package.pp)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛpp;
[GoMethodHost(typeof(fmt_package.readRune)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛreadRune;
[GoMethodHost(typeof(fmt_package.ss)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛss;
[GoMethodHost(typeof(fmt_package.stringReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛstringReader;
[GoMethodHost(typeof(fmt_package.wrapError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛwrapError;
[GoMethodHost(typeof(fmt_package.wrapErrors)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fmtᴛwrapErrors;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class fmtᴛwrapError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref wrapError e) => fmt_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Unwrap(this ref wrapError e) => fmt_package.Unwrapᴍ(ref e);
}
partial class fmtᴛwrapErrors {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref wrapErrors e) => fmt_package.Errorᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<error> Unwrap(this ref wrapErrors e) => fmt_package.Unwrapᴍ(ref e);
}
partial class fmtᴛfmt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearflags(this ref fmt f) => fmt_package.clearflagsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref fmt f, ж<buffer> Ꮡbuf) => fmt_package.initᴍ(ref f, Ꮡbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writePadding(this ref fmt f, nint n) => fmt_package.writePaddingᴍ(ref f, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pad(this ref fmt f, slice<byte> b) => fmt_package.padᴍ(ref f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void padString(this ref fmt f, sstring s) => fmt_package.padStringᴍ(ref f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtBoolean(this ref fmt f, bool v) => fmt_package.fmtBooleanᴍ(ref f, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtUnicode(this ref fmt f, uint64 u) => fmt_package.fmtUnicodeᴍ(ref f, u);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtInteger(this ref fmt f, uint64 u, nint @base, bool isSigned, rune verb, sstring digits) => fmt_package.fmtIntegerᴍ(ref f, u, @base, isSigned, verb, digits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string truncateString(this ref fmt f, @string s) => fmt_package.truncateStringᴍ(ref f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> truncate(this ref fmt f, slice<byte> b) => fmt_package.truncateᴍ(ref f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtS(this ref fmt f, @string s) => fmt_package.fmtSᴍ(ref f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtBs(this ref fmt f, slice<byte> b) => fmt_package.fmtBsᴍ(ref f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtSbx(this ref fmt f, sstring s, slice<byte> b, sstring digits) => fmt_package.fmtSbxᴍ(ref f, s, b, digits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtSx(this ref fmt f, sstring s, sstring digits) => fmt_package.fmtSxᴍ(ref f, s, digits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtBx(this ref fmt f, slice<byte> b, sstring digits) => fmt_package.fmtBxᴍ(ref f, b, digits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtQ(this ref fmt f, @string s) => fmt_package.fmtQᴍ(ref f, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtC(this ref fmt f, uint64 c) => fmt_package.fmtCᴍ(ref f, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtQc(this ref fmt f, uint64 c) => fmt_package.fmtQcᴍ(ref f, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtFloat(this ref fmt f, float64 v, nint size, rune verb, nint prec) => fmt_package.fmtFloatᴍ(ref f, v, size, verb, prec);
}
partial class fmtᴛbuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write(this ref buffer b, slice<byte> p) => fmt_package.writeᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeString(this ref buffer b, sstring s) => fmt_package.writeStringᴍ(ref b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeByte(this ref buffer b, byte c) => fmt_package.writeByteᴍ(ref b, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeRune(this ref buffer b, rune r) => fmt_package.writeRuneᴍ(ref b, r);
}
partial class fmtᴛpp {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<pp> Ꮡp) => fmt_package.freeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint wid, bool ok) Width(this ref pp p) => fmt_package.Widthᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint prec, bool ok) Precision(this ref pp p) => fmt_package.Precisionᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Flag(this ref pp p, nint b) => fmt_package.Flagᴍ(ref p, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint ret, error err) Write(this ref pp p, slice<byte> b) => fmt_package.Writeᴍ(ref p, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint ret, error err) WriteString(this ref pp p, @string s) => fmt_package.WriteStringᴍ(ref p, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unknownType(this ref pp p, reflect.Value v) => fmt_package.unknownTypeᴍ(ref p, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void badVerb(this ж<pp> Ꮡp, rune verb) => fmt_package.badVerbᴍ(Ꮡp, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtBool(this ж<pp> Ꮡp, bool v, rune verb) => fmt_package.fmtBoolᴍ(Ꮡp, v, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmt0x64(this ref pp p, uint64 v, bool leading0x) => fmt_package.fmt0x64ᴍ(ref p, v, leading0x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtInteger(this ж<pp> Ꮡp, uint64 v, bool isSigned, rune verb) => fmt_package.fmtIntegerᴍ(Ꮡp, v, isSigned, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtFloat(this ж<pp> Ꮡp, float64 v, nint size, rune verb) => fmt_package.fmtFloatᴍ(Ꮡp, v, size, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtComplex(this ж<pp> Ꮡp, complex128 v, nint size, rune verb) => fmt_package.fmtComplexᴍ(Ꮡp, v, size, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtString(this ж<pp> Ꮡp, @string v, rune verb) => fmt_package.fmtStringᴍ(Ꮡp, v, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtBytes(this ж<pp> Ꮡp, slice<byte> v, rune verb, sstring typeString) => fmt_package.fmtBytesᴍ(Ꮡp, v, verb, typeString);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fmtPointer(this ж<pp> Ꮡp, reflect.Value value, rune verb) => fmt_package.fmtPointerᴍ(Ꮡp, value, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void catchPanic(this ж<pp> Ꮡp, any arg, rune verb, @string method) => fmt_package.catchPanicᴍ(Ꮡp, arg, verb, method);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*handled*/ handleMethods(this ж<pp> Ꮡp, rune verb) => fmt_package.handleMethodsᴍ(Ꮡp, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void printArg(this ж<pp> Ꮡp, any arg, rune verb) => fmt_package.printArgᴍ(Ꮡp, arg, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void printValue(this ж<pp> Ꮡp, reflect.Value value, rune verb, nint depth) => fmt_package.printValueᴍ(Ꮡp, value, verb, depth);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint newArgNum, nint newi, bool found) argNumber(this ref pp p, nint argNum, sstring format, nint i, nint numArgs) => fmt_package.argNumberᴍ(ref p, argNum, format, i, numArgs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void badArgNum(this ref pp p, rune verb) => fmt_package.badArgNumᴍ(ref p, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void missingArg(this ref pp p, rune verb) => fmt_package.missingArgᴍ(ref p, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void doPrintf(this ж<pp> Ꮡp, sstring format, slice<any> a) => fmt_package.doPrintfᴍ(Ꮡp, format, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void doPrint(this ж<pp> Ꮡp, slice<any> a) => fmt_package.doPrintᴍ(Ꮡp, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void doPrintln(this ж<pp> Ꮡp, slice<any> a) => fmt_package.doPrintlnᴍ(Ꮡp, a);
}
partial class fmtᴛstringReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref stringReader r, slice<byte> b) => fmt_package.Readᴍ(ref r, b);
}
partial class fmtᴛss {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref ss s, slice<byte> buf) => fmt_package.Readᴍ(ref s, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune r, nint size, error err) ReadRune(this ref ss s) => fmt_package.ReadRuneᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint wid, bool ok) Width(this ref ss s) => fmt_package.Widthᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune /*r*/ getRune(this ref ss s) => fmt_package.getRuneᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune /*r*/ mustReadRune(this ref ss s) => fmt_package.mustReadRuneᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error UnreadRune(this ref ss s) => fmt_package.UnreadRuneᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ref ss s, error err) => fmt_package.errorᴍ(ref s, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorString(this ref ss s, @string err) => fmt_package.errorStringᴍ(ref s, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte> tok, error err) Token(this ж<ss> Ꮡs, bool skipSpace, Func<rune, bool> f) => fmt_package.Tokenᴍ(Ꮡs, skipSpace, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<ss> Ꮡs, ssave old) => fmt_package.freeᴍ(Ꮡs, old);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SkipSpace(this ref ss s) => fmt_package.SkipSpaceᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> token(this ref ss s, bool skipSpace, Func<rune, bool> f) => fmt_package.tokenᴍ(ref s, skipSpace, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool consume(this ref ss s, sstring ok, bool accept) => fmt_package.consumeᴍ(ref s, ok, accept);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool peek(this ref ss s, sstring ok) => fmt_package.peekᴍ(ref s, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void notEOF(this ref ss s) => fmt_package.notEOFᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool accept(this ref ss s, sstring ok) => fmt_package.acceptᴍ(ref s, ok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool okVerb(this ref ss s, rune verb, sstring okVerbs, sstring typ) => fmt_package.okVerbᴍ(ref s, verb, okVerbs, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool scanBool(this ref ss s, rune verb) => fmt_package.scanBoolᴍ(ref s, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint @base, @string digits) getBase(this ref ss s, rune verb) => fmt_package.getBaseᴍ(ref s, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string scanNumber(this ref ss s, sstring digits, bool haveDigits) => fmt_package.scanNumberᴍ(ref s, digits, haveDigits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 scanRune(this ref ss s, nint bitSize) => fmt_package.scanRuneᴍ(ref s, bitSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint @base, @string digits, bool zeroFound) scanBasePrefix(this ref ss s) => fmt_package.scanBasePrefixᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 scanInt(this ref ss s, rune verb, nint bitSize) => fmt_package.scanIntᴍ(ref s, verb, bitSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 scanUint(this ref ss s, rune verb, nint bitSize) => fmt_package.scanUintᴍ(ref s, verb, bitSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string floatToken(this ref ss s) => fmt_package.floatTokenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string real, @string imag) complexTokens(this ref ss s) => fmt_package.complexTokensᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static float64 convertFloat(this ref ss s, @string str, nint n) => fmt_package.convertFloatᴍ(ref s, str, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static complex128 scanComplex(this ref ss s, rune verb, nint n) => fmt_package.scanComplexᴍ(ref s, verb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string /*str*/ convertString(this ref ss s, rune verb) => fmt_package.convertStringᴍ(ref s, verb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string quotedString(this ref ss s) => fmt_package.quotedStringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte b, bool ok) hexByte(this ref ss s) => fmt_package.hexByteᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string hexString(this ref ss s) => fmt_package.hexStringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scanPercent(this ref ss s) => fmt_package.scanPercentᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scanOne(this ж<ss> Ꮡs, rune verb, any arg) => fmt_package.scanOneᴍ(Ꮡs, verb, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint numProcessed, error err) doScan(this ж<ss> Ꮡs, slice<any> a) => fmt_package.doScanᴍ(Ꮡs, a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*i*/ advance(this ref ss s, sstring format) => fmt_package.advanceᴍ(ref s, format);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint numProcessed, error err) doScanf(this ж<ss> Ꮡs, sstring format, slice<any> a) => fmt_package.doScanfᴍ(Ꮡs, format, a);
}
partial class fmtᴛreadRune {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte b, error err) readByte(this ref readRune r) => fmt_package.readByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (rune rr, nint size, error err) ReadRune(this ref readRune r) => fmt_package.ReadRuneᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error UnreadRune(this ref readRune r) => fmt_package.UnreadRuneᴍ(ref r);
}
// </GlyphForwarders>
