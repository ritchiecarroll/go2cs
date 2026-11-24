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
using constant = go.go.constant_package;
using io = go.io_package;
using types = go.go.types_package;
// </ImportedTypeAliases>

using go;
using static go.go.@internal.gccgoimporter_package;
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
[assembly: GoDynamicTypeLift("696e746572666163657b4e616d65282920737472696e677d", "GetImporter_type")]
[assembly: GoDynamicTypeLift("7374727563747b676f2f74797065732e547970657d", "reservedᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Reader, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReadSeeker>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<reservedᴛ1, go.go.types_package.Type>(Pointer = true)]
[assembly: GoImplement<reservedᴛ1, go.go.types_package.Type>(Promoted = true)]
[assembly: GoImplement<seekerReadAt, io_package.ReaderAt>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(go.@internal.xcoff_package.Archive))]
[assembly: GoFieldAccess(typeof(go.@internal.xcoff_package.Member))]
[assembly: GoFieldAccess(typeof(go.text.scanner_package.Scanner))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/internal/gccgoimporter/ar.go", "ar.cs", "ADRq0oCCtoKAgrakpKSkupKCgoKAgqSUgqaCgqaCuIKCgriClIKCAAgOooKClIKClKiSgoKCpoKCgpSCgrissoKU7oKClA==")]
[assembly: GoPositionMap("go/internal/gccgoimporter/gccgoinstallation.go", "gccgoinstallation.cs", "ACM80oKCgoKmgoKmgoKClLaCgoLsgoKClJSosoKCgoKUlIKCgpSmlKqi")]
[assembly: GoPositionMap("go/internal/gccgoimporter/importer.go", "importer.cs", "AC1UooKClO6CgsoADyAACwKCgpSCkoK4goKCpoK2gqaCpraCgoKCgpSCpoKCgoKClIKmggASGIIACgqCpoKCgoKClIKCpoKSgoKUgpSCpoKCpoKClIKkpoKCgqaCgoKUgoK4lIKCgoIAFzK2/KKCgIKkgIKk", "83-87:1;146-247:1", "", "", "202:29,225:33", "101:54,102:13")]
[assembly: GoPositionMap("go/internal/gccgoimporter/parser.go", "parser.cs", "ADlwooKCgoKmooKAkoKCggAHEIKmgoKmpqKmgoKClIKmgoKUpoKCgriCgoKU2qKClIKmgoKUgqaCgpS0yIKmgqzigoKmlMaCxoK2rMSClIKCgpSu8oKCgpSCgpSokpSCpoKmgoKU2NKCgoKClIKUpLTEyIKClAAICNKUgpSUgoKCgpSCgoKClIKClIKCqLKCgriU6NKCgoKCgoIACAoACAaCgoK4lIKCgqaC1ramtoKCgqaCgoKmlIKCpoKCgpTIgpSClIK2gsiCtoKClIKmgoKClIKClIK2tqiygoKClIKCgpQAChCWAAgQgoKUlIKUAAMS0oKUgpSClIKUgpS0goKClJTEAAoQ0oKCgoKmgu6CgoKUgoKClIKCgrimgoKCuIKUgpSCgpSmgoKCgpS4gpSCgpSCgoKUgpSCgoKCgoKClIK4poKCgoKUpoKCgoKUqLKCgpSClIKmgpSClILospSClIKCgpSC6LKUgpSClIKCyIKCgsiC6LKUgpSClIKCgoKClJSC2LKClIKCgpSCgoKClKaUqLKUgoKUgoKCpoKmurKClIKUgqiylIKCgpSCpoKClMqm6LKUgpSClIKCgoKUgoKmlJSC6LKCgoKCgqaClJSospSmpKampsimpqaCACM8ggADOOKCgujilIKUgoKClIKClJSCyIKCgraCpoKmgqwACAKCguoACAaCgoKClKaCgpSClJSCgoKClLT24oKChJyCgoKCpoKUyoKCgoKUlJSCkqaCtKbSgoKCpoKCgoKClIKUgoKUgrSmkoKCgoKUqJKCAAMSAAkClKakgoKCtoKCtoKClLaUgoKUyIKUgoKCtsQAAhoADQKUpqS2goKCgoKUtoKCgraCgraCgoKCgraCgoKCtoKCtoKCgpS2goK2goKClLaCgoK2yrKClIKClJSCgoKmgg==", "66-66:1;1014-1016:1;1048-1050:1;1078-1082:1;1162-1164:1", "", "1067=Lookup/1/4/18,Type/1/4/18,Lookup/2/4/19,Type/2/4/19,Lookup/3/4/20,Type/3/4/20,Lookup/4/4/21,Type/4/4/21", "", "1175:72,1176:13,1217:68,1218:13,1255:107,1256:13,1351:86,1352:17")]
// </GoSourcePositionMaps>

namespace go.go.@internal;

using static global::go.go.@internal.gccgoimporter_package;
using constant = global::go.go.constant_package;
using io = global::go.io_package;
using types = global::go.go.types_package;
using ꓸꓸꓸany = variadic<any>;
using ꓸꓸꓸstring = variadic<@string>;

[GoPackage("gccgoimporter")]
public static partial class gccgoimporter_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface GetImporter_type {}
    internal partial struct fixupRecord {}
    internal partial struct importError {}
    [GoLocalName("typeOffset")] internal partial struct parseTypes_typeOffset {}
    internal partial struct parser {}
    internal partial struct reservedᴛ1 {}
    internal partial struct seekerReadAt {}
    public partial struct GccgoInstallation {}
    public partial struct InitData {}
    public partial struct PackageInit {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸelf() => builtin.initPackage(typeof(debug.elf_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸconstant() => builtin.initPackage(typeof(global::go.go.constant_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸxcoff() => builtin.initPackage(typeof(global::go.@internal.xcoff_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸscanner() => builtin.initPackage(typeof(text.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gccgoimporter_package.GccgoInstallation)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gccgoimporterᴛGccgoInstallation;
[GoMethodHost(typeof(gccgoimporter_package.importError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gccgoimporterᴛimportError;
[GoMethodHost(typeof(gccgoimporter_package.parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gccgoimporterᴛparser;
[GoMethodHost(typeof(gccgoimporter_package.seekerReadAt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gccgoimporterᴛseekerReadAt;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class gccgoimporterᴛGccgoInstallation {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error /*err*/ InitFromDriver(this ref GccgoInstallation inst, @string gccgoPath, params ꓸꓸꓸstring argsʗp) => gccgoimporter_package.InitFromDriver⁔(ref inst, gccgoPath, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> /*paths*/ SearchPaths(this ref GccgoInstallation inst) => gccgoimporter_package.SearchPaths⁔(ref inst);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Importer GetImporter(this ref GccgoInstallation inst, slice<@string> incpaths, map<ж<types.Package>, InitData> initmap) => gccgoimporter_package.GetImporter⁔(ref inst, incpaths, initmap);
}
partial class gccgoimporterᴛimportError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this importError e) => gccgoimporter_package.Error⁔(e);
}
partial class gccgoimporterᴛparser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<parser> Ꮡp, @string filename, io.Reader src, map<@string, ж<types.Package>> imports) => gccgoimporter_package.init⁔(Ꮡp, filename, src, imports);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initScanner(this ж<parser> Ꮡp, @string filename, io.Reader src) => gccgoimporter_package.initScanner⁔(Ꮡp, filename, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref parser p, any err) => gccgoimporter_package.error⁔(ref p, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ref parser p, @string format, params ꓸꓸꓸany argsʗp) => gccgoimporter_package.errorf⁔(ref p, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string expect(this ref parser p, rune tok) => gccgoimporter_package.expect⁔(ref p, tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void expectEOL(this ref parser p) => gccgoimporter_package.expectEOL⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void expectKeyword(this ref parser p, @string keyword) => gccgoimporter_package.expectKeyword⁔(ref p, keyword);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string parseString(this ref parser p) => gccgoimporter_package.parseString⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string parseUnquotedString(this ref parser p) => gccgoimporter_package.parseUnquotedString⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ref parser p) => gccgoimporter_package.next⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string path, @string name) parseQualifiedName(this ref parser p) => gccgoimporter_package.parseQualifiedName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string path, @string name) parseUnquotedQualifiedName(this ref parser p) => gccgoimporter_package.parseUnquotedQualifiedName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string pkgpath, @string name) parseQualifiedNameStr(this ref parser p, @string unquotedName) => gccgoimporter_package.parseQualifiedNameStr⁔(ref p, unquotedName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Package> getPkg(this ref parser p, @string pkgpath, @string name) => gccgoimporter_package.getPkg⁔(ref p, pkgpath, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Package> pkg, @string name) parseExportedName(this ref parser p) => gccgoimporter_package.parseExportedName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string parseName(this ref parser p) => gccgoimporter_package.parseName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Var> field, @string tag) parseField(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseField⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Var> param, bool isVariadic) parseParam(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseParam⁔(Ꮡp, kind, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Var> parseVar(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseVar⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (constant.Value val, types.Type typ) parseConversion(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseConversion⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (constant.Value val, types.Type typ) parseConstValue(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseConstValue⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Const> parseConst(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseConst⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reserve(this ref parser p, nint n) => gccgoimporter_package.reserve⁔(ref p, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ref parser p, types.Type t, slice<any> nlist) => gccgoimporter_package.update⁔(ref p, t, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseNamedType(this ж<parser> Ꮡp, slice<any> nlist) => gccgoimporter_package.parseNamedType⁔(Ꮡp, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 parseInt64(this ref parser p) => gccgoimporter_package.parseInt64⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint parseInt(this ref parser p) => gccgoimporter_package.parseInt⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseArrayOrSliceType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseArrayOrSliceType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseMapType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseMapType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseChanType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseChanType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseStructType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseStructType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<types.Tuple>, bool) parseParamList(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseParamList⁔(Ꮡp, kind, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Tuple> parseResultList(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseResultList⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Signature> parseFunctionType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseFunctionType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Func> parseFunc(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseFunc⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseInterfaceType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseInterfaceType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parsePointerType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parsePointerType⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseTypeSpec(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, slice<any> nlist) => gccgoimporter_package.parseTypeSpec⁔(Ꮡp, Ꮡpkg, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static types.Type parseType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseType⁔(Ꮡp, Ꮡpkg, nʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (types.Type t, nint n1) parseTypeAfterAngle(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseTypeAfterAngle⁔(Ꮡp, Ꮡpkg, nʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (types.Type t, nint n1) parseTypeExtended(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseTypeExtended⁔(Ꮡp, Ꮡpkg, nʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skipInlineBody(this ж<parser> Ꮡp) => gccgoimporter_package.skipInlineBody⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseTypes(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg) => gccgoimporter_package.parseTypes⁔(Ꮡp, Ꮡpkg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseSavedType(this ж<parser> Ꮡp, ж<types.Package> Ꮡpkg, nint i, slice<any> nlist) => gccgoimporter_package.parseSavedType⁔(Ꮡp, Ꮡpkg, i, nlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static PackageInit parsePackageInit(this ref parser p) => gccgoimporter_package.parsePackageInit⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeCreatePackage(this ref parser p) => gccgoimporter_package.maybeCreatePackage⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseInitDataDirective(this ж<parser> Ꮡp) => gccgoimporter_package.parseInitDataDirective⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseDirective(this ж<parser> Ꮡp) => gccgoimporter_package.parseDirective⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<types.Package> parsePackage(this ж<parser> Ꮡp) => gccgoimporter_package.parsePackage⁔(Ꮡp);
}
partial class gccgoimporterᴛseekerReadAt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) ReadAt(this seekerReadAt sra, slice<byte> p, int64 off) => gccgoimporter_package.ReadAt⁔(sra, p, off);
}
// </MethodForwarders>
