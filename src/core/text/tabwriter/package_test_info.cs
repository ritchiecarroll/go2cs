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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.text.tabwriter_package;
using static go.text.tabwriter_test_package;

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
[assembly: GoDynamicTypeLift("7374727563747b746573746e616d6520737472696e673b206d696e776964746820696e743b20746162776964746820696e743b2070616464696e6720696e743b207061646368617220627974653b20666c6167732075696e743b2073726320737472696e673b20657870656374656420737472696e677d", "testsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Writer, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("text/tabwriter/tabwriter.go", "tabwriter.cs", "AGveAeiAgoKUpsqAgoCC/pKCgoKCgoIAJoQBAA8CgpSCgoKCgpSUlISEqJKCgoKCgpSUAAgSgoKClIK4goKClKiCloKUgqaCgoKUgqimlKKCgpaEgpSWlIK4gpKCgoK2gpSCuqaCpqau4oKCgoSCAAgUgpaCgoKCgqaUgIK2gsyCuoKCgqiokoKokoIAChiSlKSkAAQQwpSCgti0gqqigoKCgqbSgIKUlICCgqTErMLaAAgCgoLsxIKUlKiCpsoACgKWgoKUloKCgoKkgtyClN6CgoKSlKikgoKC3JSCgpSCgsyCgtqi")]
[assembly: go.GoPositionMap("text/tabwriter/tabwriter_test.go", "tabwriter_test.cs", "ABMmgKSApIKCgoKCgqaUpoCkgoKClIK4ooKCloKCuIKChIKWgoKCloKCgpSWgoKCgoKCpgAUBgDAAowIgoLsgqaigIKClLT21KKCgoKCgoKi1KKCgoKCgqLkgpSSgoKCgoKClIKUqIKCgpSClAAHEIKUgpKCgpSCgpTcgoKUlIKSgoKUgoKUABQmooKCpoI=", "662-686:1;663-673:1.1;675-685:1.2;695-706:1;717-728:1")]
// </GoSourcePositionMaps>

namespace go.text;

[GoPackage("tabwriter")]
public static partial class tabwriter_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct cell {}
    internal partial struct osError {}
    [GoValueClone("padbytes")] public partial struct Writer {}
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
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

[GoPackage("tabwriter_test")]
public static partial class tabwriter_test_package
{
}
