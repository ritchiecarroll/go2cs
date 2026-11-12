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
[assembly: GoPositionMap("go/internal/gccgoimporter/importer.go", "importer.cs", "AC1UooKClO6CgsoADyAACwKCgpSCkoK4goKCpoK2gqaCpraCgoKCgpSCpoKCgoKClIKmggASGIIACgqCpoKCgoKClIKCpoKSgoKUgpSCpoKCpoKClIKkpoKCgqaCgoKUgoK4pIKCgoIAFzK2/KKCgIKkgIKk", "83-87:1;146-247:1", "", "", "202:29,225:33", "101:21,102:13")]
[assembly: GoPositionMap("go/internal/gccgoimporter/parser.go", "parser.cs", "ADxwooKCgoKmooKAkoKCggALEILGgoKmpqKmgoKClIKmgoKUpoKCgriCgoKU2qKClIKmgoKUgqaCgpS0yIKmgqzigoKmlMaCxoK2rMSClIKCgpSu8oKCgpSCgpSokpSCpoLGgoKU+NKCgoKClIKUpLTEyIKClAAICNKUgpSUgoKCgpSCgoKClIKClIKCqLKCgriU6NKCgoKCgoIACAoACAaCgoK4lIKCgqaC1ramtoKCgqaCgoKmlIKCpoKCgpTIgpSClIK2gsiCtoKClIKmgoKClIKClIK2tqiygoKClIKCgpQADBC2AAgQgoKUlIKUAAMS0oKUgpSClIKUgpS0goKClJTEAAoQ0oKCgoKmgu6CgoKUgoKClIKCgrimgoKCuIKUgpSCgpSmgoKCgpS4gpSCgpSCgoKUgpSCgoKCgoKClIK4poKCgoKUpoKCgoKUqLKCgpSClIKmgpSClILospSClIKCgpSC6LKUgpSClIKCyIKCgsiC6LKUgpSClIKCgoKClJSC2LKClIKCgpSCgoKClKaUqLKUgoKUgoKCpoKmurKClIKUgqiylIKCgpSCpoKClMqm6LKUgpSClIKCgoKUgoKmlJSC6LKCgoKCgqaClJSospSmpKampsimpqaCACc8ggAFOOKCgujilIKUgoKClIKClJSCyIKCgraCpoKmgqwACAKCguoACAaCgoKClKaCgpSClJSCgoKClLQACwbigoKEnIKCgoKmgpTKgoKCgpSUlIKSpoK0ptKCgoKmgoKCgoKUgpSCgpSCtKaSgoKCgpSokoIAAxIACQKUpqSCgoK2goK2goKUtpSCgpTIgpSCgoK2xAACGgANApSmpLaCgoKCgpS2goKCtoKCtoKCgoKCtoKCgoK2goK2goKClLaCgraCgoKUtoKCgrbKsoKUgoKUlIKCgqaC", "66-66:1;1014-1016:1;1048-1050:1;1078-1082:1;1162-1164:1", "", "1088=Lookup/1/4/18,Type/1/4/18,Lookup/2/4/19,Type/2/4/19,Lookup/3/4/20,Type/3/4/20,Lookup/4/4/21,Type/4/4/21", "", "1198:29,1199:13,1244:29,1245:13,1282:64,1283:13,1378:35,1379:17")]
// </GoSourcePositionMaps>

namespace go.go.@internal;

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
