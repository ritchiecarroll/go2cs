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
global using execꓸError = go.os.exec_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using scannerꓸError = go.go.scanner_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.build_package;

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
[assembly: GoImplement<MultiplePackageError, error>(Pointer = true)]
[assembly: GoImplement<NoGoError, error>(Pointer = true)]
[assembly: GoImplement<go.go.scanner_package.ΔError, error>]
[assembly: GoImplement<os_package.File, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("go/build/build.go", "build.cs", "AHHuAbKAgqSokoCCpKiSgIKkqJKAgqSCqtKAgriAgtyChICCpICCpNiSgoKClIKCgpSopICCgoKUgoKUpKiSgIKmgoKUrLKCgpSCqJKCgsqUAAwalJTcsoKCgoKmgoKCpvzmkoKCkoKUgIKCppSkABwygoSCgoCCpIKChAAHEoKWhIKClJSktpKClLamgoKClABRhgKyqqIACRSCAAoWlKaCgoKUpgAWIgARAqaCloKCgoKClJSkprSSlIKktoSCgoKClILKhoKCgIKCgoKCtoKCgoC4goCCgraCgIKC3IKCguyCloCCkoKmhq6CooKClIKCgoKCgoKCgoKUlIKClJSUgpSCgszKgoKUgoKCgoKCgoKCppS4goKCpoKCgoKCgoKUzIKCgoKCgoKCgqaYkoKCgpSClJSCgoKUgpSWgoKCgoKmloKClAAIFIKUqJaClIKWlJaCgpaCgqKClIKCqIKSgoKCgoKCgoKCgpSClKiChIKCgpSClJKSgpSouqKkgIKkppSCuoKCgoKCqIKCgoKWgoKSuO6CloKCgoKCkoKCkoLMgoKCgoKUgoKAgtyCkoKUgoKCgoKmxoKCgrSCgoK0goKCtIKCgqaCgqaCqIKUhIKChIKCuoKClIKWgpSClKaClKSkpKSkpKSkpKaCgpSCgoKCgoKmpgAHGAAPCITMgt6ClOqCgoKSgpiigoLegILKgILKgpiCgpSmgqamgoCCgoKClLaCgpSogoSClpKChIKClAAFEICCpoKClIKCpsyCgoKCpoKClIKCpq7igoKCpqa0goKoqIKWgpS0goKUlILGloKCloKogoKCmJKClIK0goKClIKUgoKCgpSCxpSs4paCgoKCgpSWgoKWAAISAAgCggAcUAAPAoSWgoKUhJSWgpaClJaCgpaCgoKmgpSCgqiCgpSCloKWpoKCgpSCqJKokqiEhJaCgpSCgtwAASYAFwiCgrqUgoKUtoKCgoKAgpSkgoKUgoKUgIKC/AACGAAMAoKCgoSCgoKAgpSkggAJEoKUkpaCgpSUgpaCgoKAgoKCpJSClIKCgqborOKCorqCgqiAgriCgqiCgpaSgoKCgoKmgqiCgpSigIKklpa2lKSkpKSkpLba2ISCgpSCgpSCggACHAALAoKCgoKUkoKClIIADyCCgpSCgIK2AAYkABECgoKCgoKCgpS0grSCgsaCgrSCgoKUtIKUgpSCkoKUAAIQ0oKUlIKClKaCAAYiAA4CgqiClIKUgpSClIKUgpSCqIKCpoKCpoKCqAACJAAPAgAHEoKClISCgIKkgoKUlJSClLiYogAFEtI=", "600-608:1;623-625:2;688-714:3;866-874:4;1942-1942:1")]
[assembly: global::go.GoPositionMap("go/build/gc.go", "gc.cs", "AAwekg==")]
[assembly: global::go.GoPositionMap("go/build/read.go", "read.cs", "ACJElILKgIKkAAkUgqiCmJKCvKKCgoKCpoKCkoKUlKqigoKCgpSCgqiCgpKClJSCgoKUlKqigoCCpMyCgpSCppSCtoKCgqSCgoKClKaUgsaUgqiSgoKmmvqCgoKCgpS27IKSgpSCgoKCgpSUgoKmtoKCgpSCgoLagoKClIKCgoKClJSCgtqClIK2goKClJS2lIKCgqaClKaCgrQACAiqooKCgoKmgryigoKClIK8opSCgpSC2IKCgpSClILY3KKCgpKClAACHgAMAoKClJQABhTyhIKCgoKCgoKUlKiogrqCgoKUlIKWgqiCgpaCgoKClIKCgpSCgoKUpoKClIKWgoKUuoKClIKCAAsagoKCgoKCgoKUyoKCut7CgoKCpq7CgoKClJKCloKCgoKClIKCgoKmgraCgoKUtoKCgoKUgoKClIKCpoLYgoKCppQ=", "557-561:1;562-565:2")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("build")]
public static partial class build_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct Import_tried {}
    internal partial struct fileEmbed {}
    internal partial struct fileImport {}
    internal partial struct fileInfo {}
    internal partial struct importReader {}
    public partial struct Context {}
    public partial struct Directive {}
    public partial struct ImportMode {}
    public partial struct MultiplePackageError {}
    public partial struct NoGoError {}
    public partial struct Package {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdoc() => builtin.initPackage(typeof(global::go.go.doc_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbuildcfg() => builtin.initPackage(typeof(@internal.buildcfg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgoroot() => builtin.initPackage(typeof(@internal.goroot_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸplatform() => builtin.initPackage(typeof(@internal.platform_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyslist() => builtin.initPackage(typeof(@internal.syslist_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
