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
global using constantꓸKind = go.go.constant_package.ΔKind;
global using elfꓸData = go.debug.elf_package.ΔData;
global using elfꓸSection = go.debug.elf_package.ΔSection;
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
global using typesꓸError = go.go.types_package.ΔError;
global using typesꓸInfo = go.go.types_package.ΔInfo;
global using typesꓸScope = go.go.types_package.ΔScope;
global using typesꓸSignature = go.go.types_package.ΔSignature;
global using typesꓸTerm = go.go.types_package.ΔTerm;
global using typesꓸType = go.go.types_package.ΔType;
global using xcoffꓸSection = go.@internal.xcoff_package.ΔSection;
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
[assembly: GoImplement<reservedᴛ1, go.go.types_package.ΔType>(Pointer = true)]
[assembly: GoImplement<reservedᴛ1, go.go.types_package.ΔType>(Promoted = true)]
[assembly: GoImplement<seekerReadAt, io_package.ReaderAt>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/ar.go", "ar.cs", "AD5q0oCCpoKAgqaUpKSkypKCgoKAgqSEgpaCgpaCuIKCgqiClIKAggAIDqKCgpSCgpSokoKCgpaCgoKUgoKorLKAgqTugoCCpA==")]
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/gccgoinstallation.go", "gccgoinstallation.cs", "ACI80oKCgoKWgoKWgoKClLaCgoLsgoKClISosoKCgoKUhIKCgpSWhKqi")]
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/importer.go", "importer.cs", "AC1UooKChO6CgroACSAACwKCgpSCkoKogoKCloKWgqaCpraCgoKCgpSCloKCgoKClIKWggALGIIACgqCloKCgoKAgqSCgqaCkoKClIKUgIK2goKWgoKUgqSWgoKCloKCgpSCgqiUgoKCggADMgAVBvyigoCCpICCpA==", "83-87:1;146-247:1")]
[assembly: global::go.GoPositionMap("go/internal/gccgoimporter/parser.go", "parser.cs", "ADlwooKCgoKmooKAkoKCggAHEIKmgoCCtqaipoKCgpSCpoKClKaCgoK4goKClNqigpSCpoKClIKmgoKUpMiCpoKs4oKClpS2graitqzEgpSCgoKUrvKCgoKUgoKUqJKUgqaCpoKAgqTY0oKCgoKUgIKUpLTE2IKClOjSlIKUlIKCgoKUgoKCgpSCgpSCqLKCgriU2NKCgoKCgoIACAoACAaCgoKolIKCgqaCtqampoKCgqaCgoKWlIKCloKCgpS4goSClIKmgqiCppKClIKmgoKClIKClIKmpqiygoKClIKCgpQACxCWAAgQgoKUlIKUAAMS0oKUgpSClIKUgpS7gpSClLSCgoKUlMQACRDSgoKCgpaC3oKCgpSCgoKUgoKCqKaCgoKogoSClIKClJaCgoKClKiClIKClIKCgpSClIKCgoKCgoKEgqimgoKCgpSmgoKSgpSosoKChIKEgpaChIKEgtiyhIKEgoKChILYsoSChIKUgoK4koKCyILYsoSChIKEgoKCgoKUhILYsoKEgoKClIKCgoKUpoSospSCgpSCpoKmyrKChIKEgqiylIKCgpSCloKChMqW2LKEgoSChIKCgoKUgoKmlISC2LKCgoKCgpaChISospSmlKampsimpqaCAB08ggADOOKCgtjihIKUgoKClIKClJSCuIKCgqaCpoKWgqwACAKCgtoACAaCgpKClJaCgoSClISCgoKClLT20oKChIyCgoKCloKUqoKCgoKUlISCgpaCtKbSgoKCloKCgoKClIKUgoKUgrSmkoKCgoKUqJKCAAMSAAkClJaUgoKCpoKCpoKClKaEkoKUqJKUgoKCpsQAAhoADQKUlpSmgoKCgoKUpoKCgqaCgqaCgoKCgqaCgoKCpoKCpoKCgpSmgoKmgoKClKaCgoKmyrKClIKClJSCgoCCtoI=", "66-66:1;1011-1013:1;1045-1047:1;1075-1079:1;1159-1161:1", "", "1014=Lookup/1/4/18,Type/1/4/18,Lookup/2/4/19,Type/2/4/19,Lookup/3/4/20,Type/3/4/20,Lookup/4/4/21,Type/4/4/21")]
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
