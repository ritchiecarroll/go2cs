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
using reflect = go.reflect_package;
// </ImportedTypeAliases>

using go;
using static go.fmt_package;
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
[assembly: GoPositionMap("fmt/errors.go", "errors.cs", "AA0uAA8GgriosoKUgoKCgoKUtIKCtIKUgoKClIKmpIIABxCCpoIABxCCpoI=")]
[assembly: GoPositionMap("fmt/format.go", "format.cs", "AC9sgoKCpqKCqJKClIKClIKCppSCpoKClKiSgoKUgpSCpoK6koKClIKUgqaCAAkKkoKUupLKgoKUgoLKpoKCgoKCgoKCpoKCgoKUgoKUgoKCpoKCgpSCgoKokoKCpqaUgpTugoKUgoKCgoKUgoKC7qaUgoKCgsaCgoLGgoKCxoKCgsakgoKCgriCpoKCgrSCgtiCgoK2goKCgqaCgoKCgoKCgsqCgoKokoKCgoKCuKiSgoKCgoKUgqaokoKokoKokoKUpoKmgoKUgqaClKaClKaCpoKUlIKClIKUpoKUppSUgrqSqJKssoKCgpSCgpS8xoKClIKqooKClIKClLzUgqaCgpS4griCgpSClIKCuIKClJSCzKKUgpSCpKSCtIKCgpS0gqaCuJSClJSCgpSmuIKCgoKUgqY=")]
[assembly: GoPositionMap("fmt/print.go", "print.cs", "AFqiAQAIAoKCgoKmgIKkgIKCpILcgqaCpoKmggAZPMqSgoKCgoKoAAoQgpSUgqaCgoKmlJSClKSkpKSUqqKCqqKCrgAIAoKCgoKqwqiygoKCgqrCgoKCggACEAAJAoKCgoKs0qrCgoKCgqzSgoKCggACFAALAoKCgoKs0qrCgoKCgqzSgoKCgqyygoKUqqKCqNKClIKClIKUpoKCgpSCgqaigoKCgpSCgrSCgrSkgqailLTMooKCgqiylIKUxrS0tLS0tLS0zMKUtLS0tM72lIKClIKCgrTIopSClMa0tLS0yKKUgoKCgpSCgoKUlJSCgoKUlMa0tLS0yKKClLSCppSCgoKCgpSUlIKU2LS0AAgI0riCgriUppSUgoKCgoKCgoKUtAAMBNKClJSCgoKmuICCgoKCyIKAgoKUguzclIKCgqaCgoLIAAoGooKUgpS0pMqUgqSCuJS0tLS0tLS0tLS0tLS0tLS0tNiCgoKm1qbe1IKCgqaClLSClJS02KSkpKSkpKSkgoKCgpSUlIKCgoKUpoKClIKUtoKUgoKCgpSmgoKCppSkgoKCgpSmtqaCgoKCyoKCpoK2goKCgpSCgoKUlJSCgoKUlNqCpIKCpqSkutKCgoKUpIKCgsaCgoIACAyCgoKmAAIS9IK4goKCgpSmrLKClIKCgpSCpoKCgqaCgoKmooKCgoKCgoKCgpSClJS4poKCgoKUtLS0tNiClIK2gpSClIKCgqb6poKClILKgoKClJSCgsqCgoKUgoKClIKClIKUlIKCgsqCpoKCpoKUlKSkpIK2gpSCgqSCAAgOgoKCgoKUgpSCgqa4ooKClIKUgrzCgoKUlA==")]
[assembly: GoPositionMap("fmt/scan.go", "scan.cs", "AD5+4qrCAAIUAAkCyrKCgoKUrvKq0q7yrgAIAoKCgqrigoKCrgAIAoKCggAiRLKmwoKCpoKCgoKUgpSmgoKUqsKCgoKUlKzSgoKUpoKCgoKmgqaCpuKCgpKUuIKUgoL6AAwagoKUgqKClIKmqJIADSDSgoKCgpSCgpSq4oKCgoKUgoKUgoKUgpSCgoKCgoKUpoKCgqaC5oKCpoKmysKCgpSUgoKCgoKCgoKoxIKCpoKUgoLssoKCgpSClIKClIKUgoIABBCygqaCgoKUgoKUlKaClIKCgqaqooKClIKClJSClKiSgoKUppSClKqiqJKCgqaC6JKCgoKmlKSkgpSkgpSUABgewoKCgpSCtIK0gqTokoKCgqaUqJKCgoKCgpSssoKUlJSCpIKkgqT8ooKUgoKCgoKCpoKCpoKCgpSCgoKUqqKClIKCgoKCgpSClIKCgpSCgoKUrLKUgqaUgpSCgoKCpqaUuJSUpqz0goKUgqaCgoKUgpSmgoKCpqjIpoKUgpSUgpSClJSUgoKU7sKClIKCgoKCAAgKwoKUgoKUtLSk6JKCgqaCgoKUlLaCgoKCuIKCpoKClKSkqJKClKSklOzigoKUgoKClIKCgpTokoKCgoKUlIKClAAOGpKCgoIACQqygpSAgoKCgpSUtpS0tLS0tLS0tLS0tLS02IKCgsaCgoLG2MSCgoKClKS0tLTGgoKUgoKCxoKCtLTs0oKCgoKUxuYACAKCgoKmgoKCgpSCgrgADxYACgKCAAcQgoKCgoKClJSClIKCgpSCpoKCpoKUgqaClIKmuJSCpoKClLiCgoKUlAAICgAJAoKUgoKCgqaUgqaUlpKCgqaClIKUgoKUgoKmgoKUlIKClIKU", "248-256:1", "", "", "1312:21,1451:21", "300:51,301:13")]
// </GoSourcePositionMaps>

namespace go;

using static go.fmt_package;
using reflect = go.reflect_package;

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
[GoMethodHost(typeof(fmt_package.buffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛbuffer;
[GoMethodHost(typeof(fmt_package.fmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛfmt;
[GoMethodHost(typeof(fmt_package.pp)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛpp;
[GoMethodHost(typeof(fmt_package.readRune)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛreadRune;
[GoMethodHost(typeof(fmt_package.ss)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛss;
[GoMethodHost(typeof(fmt_package.stringReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛstringReader;
[GoMethodHost(typeof(fmt_package.wrapError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛwrapError;
[GoMethodHost(typeof(fmt_package.wrapErrors)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fmtᴛwrapErrors;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fmtᴛbuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref buffer b, slice<byte> p) => fmt_package.write⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeString(this ref buffer b, sstring s) => fmt_package.writeString⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeByte(this ref buffer b, byte c) => fmt_package.writeByte⁔(ref b, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeRune(this ref buffer b, rune r) => fmt_package.writeRune⁔(ref b, r);
}
partial class fmtᴛfmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearflags(this ref fmt f) => fmt_package.clearflags⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref fmt f, ж<buffer> Ꮡbuf) => fmt_package.init⁔(ref f, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writePadding(this ref fmt f, nint n) => fmt_package.writePadding⁔(ref f, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pad(this ref fmt f, slice<byte> b) => fmt_package.pad⁔(ref f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void padString(this ref fmt f, sstring s) => fmt_package.padString⁔(ref f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtBoolean(this ref fmt f, bool v) => fmt_package.fmtBoolean⁔(ref f, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtUnicode(this ref fmt f, uint64 u) => fmt_package.fmtUnicode⁔(ref f, u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtInteger(this ref fmt f, uint64 u, nint @base, bool isSigned, rune verb, sstring digits) => fmt_package.fmtInteger⁔(ref f, u, @base, isSigned, verb, digits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string truncateString(this ref fmt f, @string s) => fmt_package.truncateString⁔(ref f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> truncate(this ref fmt f, slice<byte> b) => fmt_package.truncate⁔(ref f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtS(this ref fmt f, @string s) => fmt_package.fmtS⁔(ref f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtBs(this ref fmt f, slice<byte> b) => fmt_package.fmtBs⁔(ref f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtSbx(this ref fmt f, sstring s, slice<byte> b, sstring digits) => fmt_package.fmtSbx⁔(ref f, s, b, digits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtSx(this ref fmt f, sstring s, sstring digits) => fmt_package.fmtSx⁔(ref f, s, digits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtBx(this ref fmt f, slice<byte> b, sstring digits) => fmt_package.fmtBx⁔(ref f, b, digits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtQ(this ref fmt f, @string s) => fmt_package.fmtQ⁔(ref f, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtC(this ref fmt f, uint64 c) => fmt_package.fmtC⁔(ref f, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtQc(this ref fmt f, uint64 c) => fmt_package.fmtQc⁔(ref f, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtFloat(this ref fmt f, float64 v, nint size, rune verb, nint prec) => fmt_package.fmtFloat⁔(ref f, v, size, verb, prec);
}
partial class fmtᴛpp {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<pp> Ꮡp) => fmt_package.free⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint wid, bool ok) Width(this ref pp p) => fmt_package.Width⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint prec, bool ok) Precision(this ref pp p) => fmt_package.Precision⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Flag(this ref pp p, nint b) => fmt_package.Flag⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint ret, error err) Write(this ref pp p, slice<byte> b) => fmt_package.Write⁔(ref p, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint ret, error err) WriteString(this ref pp p, @string s) => fmt_package.WriteString⁔(ref p, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unknownType(this ref pp p, reflect.Value v) => fmt_package.unknownType⁔(ref p, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void badVerb(this ж<pp> Ꮡp, rune verb) => fmt_package.badVerb⁔(Ꮡp, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtBool(this ж<pp> Ꮡp, bool v, rune verb) => fmt_package.fmtBool⁔(Ꮡp, v, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmt0x64(this ref pp p, uint64 v, bool leading0x) => fmt_package.fmt0x64⁔(ref p, v, leading0x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtInteger(this ж<pp> Ꮡp, uint64 v, bool isSigned, rune verb) => fmt_package.fmtInteger⁔(Ꮡp, v, isSigned, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtFloat(this ж<pp> Ꮡp, float64 v, nint size, rune verb) => fmt_package.fmtFloat⁔(Ꮡp, v, size, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtComplex(this ж<pp> Ꮡp, complex128 v, nint size, rune verb) => fmt_package.fmtComplex⁔(Ꮡp, v, size, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtString(this ж<pp> Ꮡp, @string v, rune verb) => fmt_package.fmtString⁔(Ꮡp, v, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtBytes(this ж<pp> Ꮡp, slice<byte> v, rune verb, sstring typeString) => fmt_package.fmtBytes⁔(Ꮡp, v, verb, typeString);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fmtPointer(this ж<pp> Ꮡp, reflect.Value value, rune verb) => fmt_package.fmtPointer⁔(Ꮡp, value, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void catchPanic(this ж<pp> Ꮡp, any arg, rune verb, @string method) => fmt_package.catchPanic⁔(Ꮡp, arg, verb, method);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*handled*/ handleMethods(this ж<pp> Ꮡp, rune verb) => fmt_package.handleMethods⁔(Ꮡp, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void printArg(this ж<pp> Ꮡp, any arg, rune verb) => fmt_package.printArg⁔(Ꮡp, arg, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void printValue(this ж<pp> Ꮡp, reflect.Value value, rune verb, nint depth) => fmt_package.printValue⁔(Ꮡp, value, verb, depth);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint newArgNum, nint newi, bool found) argNumber(this ref pp p, nint argNum, sstring format, nint i, nint numArgs) => fmt_package.argNumber⁔(ref p, argNum, format, i, numArgs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void badArgNum(this ref pp p, rune verb) => fmt_package.badArgNum⁔(ref p, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void missingArg(this ref pp p, rune verb) => fmt_package.missingArg⁔(ref p, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void doPrintf(this ж<pp> Ꮡp, sstring format, slice<any> a) => fmt_package.doPrintf⁔(Ꮡp, format, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void doPrint(this ж<pp> Ꮡp, slice<any> a) => fmt_package.doPrint⁔(Ꮡp, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void doPrintln(this ж<pp> Ꮡp, slice<any> a) => fmt_package.doPrintln⁔(Ꮡp, a);
}
partial class fmtᴛreadRune {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte b, error err) readByte(this ref readRune r) => fmt_package.readByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune rr, nint size, error err) ReadRune(this ref readRune r) => fmt_package.ReadRune⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error UnreadRune(this ref readRune r) => fmt_package.UnreadRune⁔(ref r);
}
partial class fmtᴛss {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref ss s, slice<byte> buf) => fmt_package.Read⁔(ref s, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (rune r, nint size, error err) ReadRune(this ref ss s) => fmt_package.ReadRune⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint wid, bool ok) Width(this ref ss s) => fmt_package.Width⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune /*r*/ getRune(this ref ss s) => fmt_package.getRune⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune /*r*/ mustReadRune(this ref ss s) => fmt_package.mustReadRune⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error UnreadRune(this ref ss s) => fmt_package.UnreadRune⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref ss s, error err) => fmt_package.error⁔(ref s, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorString(this ref ss s, @string err) => fmt_package.errorString⁔(ref s, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> tok, error err) Token(this ж<ss> Ꮡs, bool skipSpace, Func<rune, bool> f) => fmt_package.Token⁔(Ꮡs, skipSpace, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<ss> Ꮡs, ssave old) => fmt_package.free⁔(Ꮡs, old);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SkipSpace(this ref ss s) => fmt_package.SkipSpace⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> token(this ref ss s, bool skipSpace, Func<rune, bool> f) => fmt_package.token⁔(ref s, skipSpace, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool consume(this ref ss s, sstring ok, bool accept) => fmt_package.consume⁔(ref s, ok, accept);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool peek(this ref ss s, sstring ok) => fmt_package.peek⁔(ref s, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void notEOF(this ref ss s) => fmt_package.notEOF⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool accept(this ref ss s, sstring ok) => fmt_package.accept⁔(ref s, ok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool okVerb(this ref ss s, rune verb, sstring okVerbs, sstring typ) => fmt_package.okVerb⁔(ref s, verb, okVerbs, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool scanBool(this ref ss s, rune verb) => fmt_package.scanBool⁔(ref s, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint @base, @string digits) getBase(this ref ss s, rune verb) => fmt_package.getBase⁔(ref s, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string scanNumber(this ref ss s, sstring digits, bool haveDigits) => fmt_package.scanNumber⁔(ref s, digits, haveDigits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 scanRune(this ref ss s, nint bitSize) => fmt_package.scanRune⁔(ref s, bitSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint @base, @string digits, bool zeroFound) scanBasePrefix(this ref ss s) => fmt_package.scanBasePrefix⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 scanInt(this ref ss s, rune verb, nint bitSize) => fmt_package.scanInt⁔(ref s, verb, bitSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 scanUint(this ref ss s, rune verb, nint bitSize) => fmt_package.scanUint⁔(ref s, verb, bitSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string floatToken(this ref ss s) => fmt_package.floatToken⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string real, @string imag) complexTokens(this ref ss s) => fmt_package.complexTokens⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 convertFloat(this ref ss s, @string str, nint n) => fmt_package.convertFloat⁔(ref s, str, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static complex128 scanComplex(this ref ss s, rune verb, nint n) => fmt_package.scanComplex⁔(ref s, verb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string /*str*/ convertString(this ref ss s, rune verb) => fmt_package.convertString⁔(ref s, verb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string quotedString(this ref ss s) => fmt_package.quotedString⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte b, bool ok) hexByte(this ref ss s) => fmt_package.hexByte⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string hexString(this ref ss s) => fmt_package.hexString⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanPercent(this ref ss s) => fmt_package.scanPercent⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scanOne(this ж<ss> Ꮡs, rune verb, any arg) => fmt_package.scanOne⁔(Ꮡs, verb, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint numProcessed, error err) doScan(this ж<ss> Ꮡs, slice<any> a) => fmt_package.doScan⁔(Ꮡs, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*i*/ advance(this ref ss s, sstring format) => fmt_package.advance⁔(ref s, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint numProcessed, error err) doScanf(this ж<ss> Ꮡs, sstring format, slice<any> a) => fmt_package.doScanf⁔(Ꮡs, format, a);
}
partial class fmtᴛstringReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ref stringReader r, slice<byte> b) => fmt_package.Read⁔(ref r, b);
}
partial class fmtᴛwrapError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref wrapError e) => fmt_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this ref wrapError e) => fmt_package.Unwrap⁔(ref e);
}
partial class fmtᴛwrapErrors {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref wrapErrors e) => fmt_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<error> Unwrap(this ref wrapErrors e) => fmt_package.Unwrap⁔(ref e);
}
// </MethodForwarders>
