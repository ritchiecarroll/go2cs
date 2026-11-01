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
global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
// </ImportedTypeAliases>

using go;
using static go.errors_package;
using static go.errors_test_package;

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
[assembly: GoDynamicTypeLift("6572726f72735f746573742e74696d656f7574", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b417328616e792920626f6f6c7d", "as_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b4973286572726f722920626f6f6c7d", "is_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c3b206572726f727d", "TestAsType_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestAs_timeout")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829205b5d6572726f727d", "is_typeᴛ1")]
[assembly: GoDynamicTypeLift("696e746572666163657b556e777261702829206572726f727d", "Unwrap_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<errorString, error>(Pointer = true, Production = true)]
[assembly: GoImplement<joinError, error>(Pointer = true, Production = true)]
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
[assembly: GoFieldAccess(typeof(go.io.fs_package.PathError))]
[assembly: GoFieldAccess(typeof(joinError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("errors/errors.go", "errors.cs", "AD+AAaIABxCCAA8g")]
[assembly: GoPositionMap("errors/errors_test.go", "errors_test.cs", "ABQYlIKUgriCgriCgoI=")]
[assembly: GoPositionMap("errors/example_test.go", "example_test.cs", "ABUqgqaC3IKAggAJCoKCggAICoKmlILssoKCgpSCgoKCAA0YopKCggAMCoKCgoKCgpSClAANEoKAgoKUAAwYgqSC6qKCggAJDoKAgoKClAAIEIKAgoCClAAMFoKkgoKClMr6ooKClIKCggAIFqKCgpSCggAMEoKCgoI=", "", "", "27=Date/1/1/1;235=New/1/1/3")]
[assembly: GoPositionMap("errors/join.go", "join.cs", "AAooAAsCgoKCpoKUpoKCpu6mgqaCgoKmpoI=")]
[assembly: GoPositionMap("errors/join_test.go", "join_test.cs", "AAwagoCCpICCpICCAA0IgoKCggANIIKClIIACQqCgoIAChqCgg==")]
[assembly: GoPositionMap("errors/wrap.go", "wrap.cs", "AA8i4oaClAACKgASAoKmggAKBoKCgpSAgqSUgoLGgoKmtAAENAAVAoKUgpSCgoKUgoKU5oKCgoKUgIKklIKCxoKClIKmtMoAAR4ADQKCgpSCpoKCgIKkgIKClIK2lIKCxoKClICCtrQ=")]
[assembly: GoPositionMap("errors/wrap_test.go", "wrap_test.cs", "ABYggoKClJTIAB9IspKAggALFpSAooCigpS0tMSUABEGgoKCgoKClABbwAGylIKCooKClIKUgIIACgyCgtyCgsKClIKClKIACgiCgoKKlLq6urq6urq6uri6urq6urq6AAoYgoKCgoKC+qKClIKC+qKCgoKC+qKCgoCCAAoKgoKUAAYUgoCCAAkOgP6AooAACAiAooAACgyCpoKC", "23-25:1;64-68:2;204-215:1;229-238:1;230-232:1.1;351-356:1", "", "35=New/1/1/27;141=New/1/4/46,New/2/4/66,New/3/4/71,New/4/4/81;338=New/1/1/1;354=New/1/1/1;358=New/1/1/1;366=New/1/1/1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("errors")]
public static partial class errors_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface Unwrap_type {}
    internal partial interface as_type {}
    internal partial interface is_type {}
    internal partial interface is_typeᴛ1 {}
    internal partial struct errorString {}
    internal partial struct joinError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() => builtin.initPackage(typeof(@internal.reflectlite_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

[GoPackage("errors_test")]
public static partial class errors_test_package
{
}
