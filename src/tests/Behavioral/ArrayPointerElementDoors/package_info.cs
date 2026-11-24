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
using static go.main_package;

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
// </InterfaceImplementations>

// The struct types whose arrays this package holds inline (the go.arrays family), as the converter
// admitted them: a `GoInlineArrayType` record per named type, a `GoInlineArrayKeys` record with the
// count and a digest of every admitted type (anonymous ones included), a `GoInlineArrayAnonymous`
// record per admitted anonymous type, a `GoInlineArraySeed` record per -inline-arrays seed, and a
// `GoInlineArrays` record naming the rule when it is not the default. A -tests conversion applies the
// same rule and seeds rather than deciding again, and refuses a production set that admits differently.
// The section exists only while there is a record to hold.
// <InlineArrays>
[assembly: go.golib.GoInlineArrayKeys(1, "425230f2e775fe99")]
[assembly: go.golib.GoInlineArrayType("hdr")]
// </InlineArrays>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(hdr))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "AB9YgpSCpoKUpoKUgqamgoK6gqaCpqLGgpaCgoKWgoKWgoKWgoKWgoKCloKCloKCgpiCgoKCgpaCgoKCloKCloKCgpaCgoKWgoKCgoaEgpSCpoKCgpaCgpaClIKCpqaEgoKCgpaCgoKWgoKCgoKWgoKCgpaCgoKWgoKCgoKCloKCroKClJSCppSUgoKClKSCgqaUlIKmlJSCgqaUloKCgqaUmIKCpoKmlJSCgqamooKmgoKipKKCpoKCoqSigqaCoqSCgoKCgoI=", "335-337:1;345-347:1;355-357:1", "", "", "", "296:50,297:13,311:48,312:13,326:50,327:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoPlatformExclusive("linux")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct hdr {}
    [GoLocalName("sixteen")] internal partial struct nativeMemory_sixteen {}
    internal partial struct pair {}
    internal partial struct smallMem {}
    public partial struct RawMem {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    // </ImportInitializers>
}
