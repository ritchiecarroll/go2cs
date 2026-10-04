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
using static go.debug.gosym_package;

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
[assembly: GoImplement<DecodingError, error>(Pointer = true)]
[assembly: GoImplement<UnknownFileError, error>]
[assembly: GoImplement<UnknownLineError, error>(Pointer = true)]
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
[assembly: go.GoPositionMap("debug/gosym/pclntab.go", "pclntab.cs", "AEmiAQAMEoKCgoKUgoKUgoK0tLSCtJSmgpKs0oKUgq7igpSCgqYAAhYACAIAAh4ADgKCAAcYooKUqNKCgoIABhCEgpS6ppaCgoKUtLS0tLS0tLSkloKEhpiUgoKCgoKCgoKCgqSCgoKCgoKCgoKkgoKCgoKCgoKCgqTEpuSCgqiCgoKCgoKCgoKCAAcQlNiykoKUloKospKCgoKCgoKmgqiSgIKkgoKCqJKAgqSCgoKokqiSgpSosgAJFpKokoKClKiSqJKClAAIFLKCqJKotqaUpoCigKKAooCigKqygriCgpSCgqjCgoKUgpSUgoKCgqyylJKSgoKmAAIS4oKWkpKSkpKSgoKCgpTKgpSCgpSCpqaUqOKCgqiCgpSCgujigoKogoKUgoKCgoKUppKUgoCCpOjigoKogoKCnLKCgoKCgoKUlJSCgqbo4oKEgpSEgoKCpoKCgoKmoqrygoKogoK0", "209-212:1;251-253:2;254-256:3;303-305:1;337-339:1;570-574:1;587-591:1;619-623:1;686-688:1")]
[assembly: go.GoPositionMap("debug/gosym/symtab.go", "symtab.cs", "ACFGkK7SgoKUgpSU6qLMgqiCloKCloCCpKyypoKCuJSClMqCqJKCgIKCgsqmpAA5eoKCloKSlIKCmoLktIK0goKClIKClJSCgoKClIKCgoKUlIKCgoKmgoKUgriCgoKCgpSClIKUgoKmgoKUgsqCgpSCgpSCgpiSgoKCgqaUgoKCgtiClIKChIKCpoKUlKzSgoKClIKWgoKUgoKCgoKSgoKCgoKCgpaCgpKCgpSClLSClIKSgoKUgIKkxpS0tIKUgpaChIKUgoKUuoKCgpSklpKUhoKChrKCgIK2goaCkoKClIKU2oCSpIKYooKCgpS0tOqCgoKCgoKCgoK4koKClIKClLSCgrSCgsbIgpSClKqigoKCgpS0pMaq4oCCpIKClJSs8oKCloKCgpSWgoKUgoKCgqaqtIKClILYqqKCgoKmqJKCgpSC2AAMHAAJAgAAEISCgpKUuMiSlIK4+IKUpoKClpSCqIKCgoKCgpSmgoKSgsiClOaUAAQWsAAKFIIAChaCgoKUgg==", "338-341:1;355-401:2")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("gosym")]
public static partial class gosym_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("stackEnt")] internal partial struct lineFromAline_stackEnt {}
    internal partial struct sym {}
    internal partial struct version {}
    internal partial struct ΔfuncData {}
    internal partial struct ΔfuncTab {}
    public partial struct DecodingError {}
    public partial struct Func {}
    public partial struct LineTable {}
    public partial struct Obj {}
    public partial struct Sym {}
    public partial struct Table {}
    public partial struct UnknownFileError {}
    public partial struct UnknownLineError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}
