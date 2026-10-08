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
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.sync_package;

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
[assembly: GoImplement<Mutex, Locker>(Pointer = true)]
[assembly: GoImplement<RWMutex, Locker>(Pointer = true)]
[assembly: GoImplement<rlocker, Locker>(Pointer = true)]
// </InterfaceImplementations>

// An exported pointer-receiver method that carries a `ref`-receiver primary beside its
// pointer-box twin is recorded here as a `GoRefPrimary` attribute, so a package in another
// assembly can bind the primary at a ref-addressable call site instead of allocating a box.
// Both names are the Go spellings. The section exists only while there is a record to hold.
// <RefVerdicts>
[assembly: GoRefPrimary("Mutex", "Lock")]
[assembly: GoRefPrimary("Mutex", "TryLock")]
[assembly: GoRefPrimary("Mutex", "Unlock")]
// </RefVerdicts>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("sync/cond.go", "cond.cs", "ACxgkgACJAARAoKCgoIAAhTygq7CggAQOLI=")]
[assembly: go.GoPositionMap("sync/hashtriemap.go", "hashtriemap.cs", "ACxisqiSqJKssqqiqJKqoqyyAAIQ0gACHAALAg==")]
[assembly: go.GoPositionMap("sync/runtime.go", "runtime.cs", "AAkc5gACFAAJAgABEgAIBqampqaigoKmkg==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("sync")]
public static partial class sync_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct copyChecker {}
    internal partial struct noCopy {}
    internal partial struct notifyList {}
    public partial struct Cond {}
    public partial struct Map {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsync() => builtin.initPackage(typeof(@internal.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}
