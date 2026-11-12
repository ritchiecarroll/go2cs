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
using abi = go.@internal.abi_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.reflectlite_package;

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
[assembly: GoDynamicTypeLift("7374727563747b6220626f6f6c3b207820616e797d", "dummyᴛ1")]
[assembly: GoTypeAlias("Kind", "go.@internal.abi_package.Kind")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<rtype, go.@internal.reflectlite_package.Type>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<flag, abi.Kind>(Inverted = false, ValueType = "uint8")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.EmptyInterface))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.@internal.abi_package.StructType))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/reflectlite/type.go", "type.cs", "AIsBrAKCpoKmgqaC6qKCgoKCggAICoKClIKmgoKUgoIACgaCgpSCgoKClKaCggACGgALDgAIBIKmgqaCAAQWlIKCgpSmgoKClAAEHIKClIKCgoKUtKSUxoKmgoKClO6CgoKUpoKCgpSmgoKClKaCgoKUpoKCgpSmgoKClKaCgoKUAAQU8qqiAAYagoKUgoKmggAGvgEACQSCyoK4prKCpoKmAATYAdKClA==")]
[assembly: GoPositionMap("internal/reflectlite/value.go", "value.cs", "AE+kAYKmgoKUxtyqooKUgpTIkoKClJSCpoKCgoKU2MbsggAONIKClAAIIKKClIK+soKmgpSCAAUS0gAMvAHSqqLMxgAGMpKClAAELJKCgqYABBQACxwACQq4goKmgpS4lIKClJS4AAgU8qaayrKC")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("reflectlite")]
public static partial class reflectlite_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dummyᴛ1 {}
    internal partial struct flag {}
    internal partial struct name {}
    internal partial struct rtype {}
    public partial interface Type {}
    public partial struct Value {}
    public partial struct ValueError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(reflectlite_package.Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectliteᴛValue;
[GoMethodHost(typeof(reflectlite_package.ValueError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectliteᴛValueError;
[GoMethodHost(typeof(reflectlite_package.flag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectliteᴛflag;
[GoMethodHost(typeof(reflectlite_package.name)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectliteᴛname;
[GoMethodHost(typeof(reflectlite_package.rtype)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class reflectliteᴛrtype;
// </MethodHolders>
