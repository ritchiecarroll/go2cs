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
global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;
global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.context_package;

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
[assembly: GoImplement<afterFuncCtx, canceler>(Pointer = true)]
[assembly: GoImplement<backgroundCtx, Context>]
[assembly: GoImplement<cancelCtx, Context>(Pointer = true)]
[assembly: GoImplement<cancelCtx, canceler>(Pointer = true)]
[assembly: GoImplement<deadlineExceededError, error>]
[assembly: GoImplement<emptyCtx, Context>]
[assembly: GoImplement<stopCtx, Context>(Promoted = true)]
[assembly: GoImplement<stopCtx, Context>]
[assembly: GoImplement<timerCtx, Context>(Pointer = true)]
[assembly: GoImplement<timerCtx, canceler>(Pointer = true)]
[assembly: GoImplement<todoCtx, Context>]
[assembly: GoImplement<valueCtx, Context>(Pointer = true)]
[assembly: GoImplement<withoutCancelCtx, Context>]
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
[assembly: go.GoPositionMap("context/context.go", "context.cs", "AKYBzgK4+ICigKKA/LKmgqaCpoIACQqCAAkKgq7CrsIABCDygpAABTQACwKCkLaCgpSCggACEgAIAoCCgpL+AAUoABECpoKSgoKUgpQADRyigoKUggATMOKCgpSCgpSCgpSokoCCgqSCgpSCgpQAChaUggARHqKClKbSgoKUgoKCgoKU1qKCgoKqwoSCgpamksiAlIKUlIKUlIKmgJSCgpS4gqaCgrQADBSCgIKkpoKs0oKUgpSCgoKUgoKCgpSUlJSChIIAAxDCgpTusqaCpoKmgqaCAAIYAAkCrNKClICUpKaCgoKCkKSCkoKSppAADxqCpoKqooKUlIKCgpQAAhoACgKssgACIAANAoKUgpSClAAYGrKUpKSUpoKqgoKUpoKClIKUtIKUtKaUtIKU1LQ=", "242-242:1;270-270:1;324-333:1;326-328:1.1;351-353:1;501-503:1;513-519:2;638-638:1;643-645:2;647-647:3")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("context")]
public static partial class context_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface afterFuncer {}
    internal partial interface canceler {}
    internal partial interface stringer {}
    internal partial struct afterFuncCtx {}
    internal partial struct backgroundCtx {}
    internal partial struct cancelCtx {}
    internal partial struct deadlineExceededError {}
    internal partial struct emptyCtx {}
    internal partial struct stopCtx {}
    internal partial struct timerCtx {}
    internal partial struct todoCtx {}
    internal partial struct valueCtx {}
    internal partial struct withoutCancelCtx {}
    public partial interface Context {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() => builtin.initPackage(typeof(@internal.reflectlite_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
