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
using static go.go.@internal.gccgoimporter_package;

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
[assembly: GoPositionMap("go/internal/gccgoimporter/ar.go", "ar.cs", "ADVq0oCCtoKAgrakpKSkupKCgoKAgqSUgqaCgqaCuIKCgriClIKCAAgOooKClIKClKiSgoKCpoKCgpSCgrissoKUAAgOgoKU")]
[assembly: GoPositionMap("go/internal/gccgoimporter/gccgoinstallation.go", "gccgoinstallation.cs", "ACc80oKCgoKmgoKmgoKClLaCgoLsgoKClJSosoKCgoKUlIKCgpSmlKqi")]
[assembly: GoPositionMap("go/internal/gccgoimporter/importer.go", "importer.cs", "AC1UooKClO6CgsoADyAACwKCgpSCkoK4goKCpoK2gqaCpraCgoKCgpSCpoKCgoKClIKmggASGIIACgqCpoKCgoKClIKCpoKSgoKUgpSCpoKCpoKClIKkpoKCgqaCgoKUgoK4lIKCgoIAFzK2/KKCgIKkgIKk", "83-87:1;146-247:1", "", "", "202:29,225:33", "101:21,102:13")]
[assembly: GoPositionMap("go/internal/gccgoimporter/parser.go", "parser.cs", "ADxwooKCgoKmooKAkoKCggALEILGgoKmpqKmgoKClIKmgoKUpoKCgriCgoKU2qKClIKmgoKUgqaCgpS0yIKmgqzigoKmlMaCxoK2rMSClIKCgpSu8oKCgpSCgpSokpSCpoLGgoKU+NKCgoKClIKUpLTEyIKClAAICNKUgpSUgoKCgpSCgoKClIKClIKCqLKCgriU6NKCgoKCgoIACAoACAaCgoK4lIKCgqaC1ramtoKCgqaCgoKmlIKCpoKCgpTIgpSClIK2gsiCtoKClIKmgoKClIKClIK2tqiygoKClIKCgpQADBC2AAgQgoKUlIKUAAMS0oKUgpSClIKUgpS0goKClJTEAAoQ0oKCgoKmgu6CgoKUgoKClIKCgrimgoKCuIKUgpSCgpSmgoKCgpS4gpSCgpSCgoKUgpSCgoKCgoKClIK4poKCgoKUpoKCgoKUqLKCgpSClIKmgpSClILospSClIKCgpSC6LKUgpSClIKCyIKCgsiC6LKUgpSClIKCgoKClJSC2LKClIKCgpSCgoKClKaUqLKUgoKUgoKCpoKmurKClIKUgqiylIKCgpSCpoKClMqm6LKUgpSClIKCgoKUgoKmlJSC6LKCgoKCgqaClJSospSmpKampsimpqaCACc8ggAFOOKCgujilIKUgoKClIKClJSCyIKCgraCpoKmgqwACAKCguoACAaCgoKClKaCgpSClJSCgoKClLQACwbigoKEnIKCgoKmgpTKgoKCgpSUlIKSpoK0ptKCgoKmgoKCgoKUgpSCgpSCtKaSgoKCgpSokoIAAxIACQKUpqSCgoK2goK2goKUtpSCgpTIgpSCgoK2xAACGgANApSmpLaCgoKCgpS2goKCtoKCtoKCgoKCtoKCgoK2goK2goKClLaCgraCgoKUtoKCgrbKsoKUgoKUlIKCgqaC", "66-66:1;1014-1016:1;1048-1050:1;1078-1082:1;1162-1164:1", "", "1088=Lookup/1/4/18,Type/1/4/18,Lookup/2/4/19,Type/2/4/19,Lookup/3/4/20,Type/3/4/20,Lookup/4/4/21,Type/4/4/21", "", "1198:29,1199:13,1244:29,1245:13,1282:64,1283:13,1378:35,1379:17")]
// </GoSourcePositionMaps>

namespace go.go.@internal;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using io = io_package;
using debug;
using global::go.@internal;
using static gccgoimporter_package;
using types = global::go.go.types_package;
using global::go.go;
using global::go.io;
using global::go.os;
using path;
using ꓸꓸꓸstring = Span<@string>;
using constant = global::go.go.constant_package;
using text;
using unicode;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(gccgoimporter_package.GccgoInstallation)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gccgoimporterᴛGccgoInstallation;
[GoMethodHost(typeof(gccgoimporter_package.importError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gccgoimporterᴛimportError;
[GoMethodHost(typeof(gccgoimporter_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gccgoimporterᴛparser;
[GoMethodHost(typeof(gccgoimporter_package.seekerReadAt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class gccgoimporterᴛseekerReadAt;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class gccgoimporterᴛseekerReadAt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) ReadAt(this seekerReadAt sra, slice<byte> p, int64 off) => gccgoimporter_package.ReadAtᴍ(sra, p, off);
}
partial class gccgoimporterᴛGccgoInstallation {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ InitFromDriver(this ref GccgoInstallation inst, @string gccgoPath, params ꓸꓸꓸstring argsʗp) => gccgoimporter_package.InitFromDriverᴍ(ref inst, gccgoPath, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> /*paths*/ SearchPaths(this ref GccgoInstallation inst) => gccgoimporter_package.SearchPathsᴍ(ref inst);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Func<map<@string, ж<types.Package>>, @string, @string, Func<@string, (io.ReadCloser, error)>, (ж<types.Package>, error)> GetImporter(this ref GccgoInstallation inst, slice<@string> incpaths, map<ж<types.Package>, InitData> initmap) => gccgoimporter_package.GetImporterᴍ(ref inst, incpaths, initmap);
}
partial class gccgoimporterᴛparser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<parser> Ꮡp, @string filename, io.Reader src, map<@string, ж<types.Package>> imports) => gccgoimporter_package.initᴍ(Ꮡp, filename, src, imports);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initScanner(this ж<parser> Ꮡp, @string filename, io.Reader src) => gccgoimporter_package.initScannerᴍ(Ꮡp, filename, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ref parser p, any err) => gccgoimporter_package.errorᴍ(ref p, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ref parser p, @string format, params ꓸꓸꓸany argsʗp) => gccgoimporter_package.errorfᴍ(ref p, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string expect(this ref parser p, rune tok) => gccgoimporter_package.expectᴍ(ref p, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expectEOL(this ref parser p) => gccgoimporter_package.expectEOLᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expectKeyword(this ref parser p, @string keyword) => gccgoimporter_package.expectKeywordᴍ(ref p, keyword);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string parseString(this ref parser p) => gccgoimporter_package.parseStringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string parseUnquotedString(this ref parser p) => gccgoimporter_package.parseUnquotedStringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ref parser p) => gccgoimporter_package.nextᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string path, @string name) parseQualifiedName(this ref parser p) => gccgoimporter_package.parseQualifiedNameᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string path, @string name) parseUnquotedQualifiedName(this ref parser p) => gccgoimporter_package.parseUnquotedQualifiedNameᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string pkgpath, @string name) parseQualifiedNameStr(this ref parser p, @string unquotedName) => gccgoimporter_package.parseQualifiedNameStrᴍ(ref p, unquotedName);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Package> getPkg(this ref parser p, @string pkgpath, @string name) => gccgoimporter_package.getPkgᴍ(ref p, pkgpath, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<types.Package> pkg, @string name) parseExportedName(this ref parser p) => gccgoimporter_package.parseExportedNameᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string parseName(this ref parser p) => gccgoimporter_package.parseNameᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<types.Var> field, @string tag) parseField(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseFieldᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<types.Var> param, bool isVariadic) parseParam(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> pkg) => gccgoimporter_package.parseParamᴍ(Ꮡp, kind, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Var> parseVar(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseVarᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (constant.Value val, types.Type typ) parseConversion(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseConversionᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (constant.Value val, types.Type typ) parseConstValue(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseConstValueᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Const> parseConst(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseConstᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reserve(this ref parser p, nint n) => gccgoimporter_package.reserveᴍ(ref p, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ref parser p, types.Type t, slice<any> nlist) => gccgoimporter_package.updateᴍ(ref p, t, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseNamedType(this ж<parser> Ꮡp, slice<any> nlist) => gccgoimporter_package.parseNamedTypeᴍ(Ꮡp, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 parseInt64(this ref parser p) => gccgoimporter_package.parseInt64ᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint parseInt(this ref parser p) => gccgoimporter_package.parseIntᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseArrayOrSliceType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseArrayOrSliceTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseMapType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseMapTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseChanType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseChanTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseStructType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseStructTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<types.Tuple>, bool) parseParamList(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> pkg) => gccgoimporter_package.parseParamListᴍ(Ꮡp, kind, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Tuple> parseResultList(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseResultListᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Signature> parseFunctionType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseFunctionTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Func> parseFunc(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseFuncᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseInterfaceType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseInterfaceTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parsePointerType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parsePointerTypeᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseTypeSpec(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) => gccgoimporter_package.parseTypeSpecᴍ(Ꮡp, pkg, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static types.Type parseType(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseTypeᴍ(Ꮡp, pkg, nʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (types.Type t, nint n1) parseTypeAfterAngle(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseTypeAfterAngleᴍ(Ꮡp, pkg, nʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (types.Type t, nint n1) parseTypeExtended(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) => gccgoimporter_package.parseTypeExtendedᴍ(Ꮡp, pkg, nʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void skipInlineBody(this ж<parser> Ꮡp) => gccgoimporter_package.skipInlineBodyᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseTypes(this ж<parser> Ꮡp, ж<types.Package> pkg) => gccgoimporter_package.parseTypesᴍ(Ꮡp, pkg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseSavedType(this ж<parser> Ꮡp, ж<types.Package> pkg, nint i, slice<any> nlist) => gccgoimporter_package.parseSavedTypeᴍ(Ꮡp, pkg, i, nlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static PackageInit parsePackageInit(this ref parser p) => gccgoimporter_package.parsePackageInitᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeCreatePackage(this ref parser p) => gccgoimporter_package.maybeCreatePackageᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseInitDataDirective(this ж<parser> Ꮡp) => gccgoimporter_package.parseInitDataDirectiveᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseDirective(this ж<parser> Ꮡp) => gccgoimporter_package.parseDirectiveᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<types.Package> parsePackage(this ж<parser> Ꮡp) => gccgoimporter_package.parsePackageᴍ(Ꮡp);
}
partial class gccgoimporterᴛimportError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this importError e) => gccgoimporter_package.Errorᴍ(e);
}
// </GlyphForwarders>
