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
using static go.@internal.trace.@internal.oldtrace_package;

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
[assembly: GoDynamicTypeLift("7374727563747b4e616d6520737472696e673b206d696e56657273696f6e20696e7465726e616c2f74726163652f76657273696f6e2e56657273696f6e3b20537461636b20626f6f6c3b2041726773205b5d737472696e673b205341726773205b5d737472696e677d", "EventDescriptionsᴛ1")]
[assembly: GoTypeAlias("STWReason", "ΔSTWReason")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Events, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Event, ж<Event>>(Indirect = true)]
[assembly: GoImplicitConv<Event, ж<Event>>]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/trace/internal/oldtrace/order.go", "order.cs", "AB5G9pSCgoKkgoKCpIKCggACEILigqiCgoKkgoKCpIKCgqSCgoKkgoKCpoLIktaCgpSCppSUtLSCyoKmsoKmooKCgoKCpqKCgoKUgriigoKCkpSCgIKkgpSClA==")]
[assembly: go.GoPositionMap("internal/trace/internal/oldtrace/parser.go", "parser.cs", "AGvqAYKClICClKSmgoKApoKClIKClIKCloKCgqaCgoK2AAIUAAkGgoKU2NKCABAmgoKCgoKEgIKmgoKWgpaUlIKCgpSClIK6gIKmAAcQACZOooKClIKCqLKCgqaCgqa4poKmgqaCpoKClIKCgqaUpoKCgoKCggAFHgAODKKWgoKUhKYAABD0goKUgoKChJSCgpSCgqaCpJSoggAKGIKUgoKCqLaCgoKmlMyWlKSkpISAgqTMggAKGIKCgpSkgoKUgoKUgpS2hKiUkoKCgoKUgpSCloKCgpaCgqaEgoSCgoKUgoKUqIKCggANHqgABRCCgoKClLiCypSCgurIxoKClAAMGoKClpSUgIKkgoKUgriCgpSClIKUgoKClIKUgpSCgpSWgqSAgqiEgpKUgpaCgpSWgoKW3IKkgoKCgoKClKaCgIL+goKWgKamgoKClIKCgoKUuIKmlJSCgpSmgoKUgsyCypKCgpSCgoKEhIKWgoKCgoKClIKUgpSCgpSogoKUgqimooKClIKUgpSCgpSq0oKClIKClJSCgpSClJSCpIK4tqSClIKClIKClIKCgoKCgoKChICCttyigoKCgoKClKaUgqqkAAYSgMKUgoKCyKaCygAPDOK8AAAcgoKCgpSCgoKUgpSClJaChJSCgpSEpIKClIKUhKSClISkkpSkgoKUpIKClKSCgpSEpIKClISogqKWpIKClISkgoKUgoSkgoKUgoSkgoKAgqSAgqSmgoKClIKUgoKCgoKWgpaCpIKCgIKkgoKEkpaCpIKCgIKkgoKChIKkgoKClIKUgoKUgpSCgqaCgoCCpISkgoKAgqSCgoSCpIKClIKEpoKCgIKkgoKChIKkgoCCpKaCpoKCkrSCkoKSloKUuLi6vKaWkoKClIKmooKClKaCgoKCgpSqooKClIKClJSkggB6dgA1eJKC3oKClIKmgoKClLaClKY=", "181-183:1;347-355:1;373-375:1;1034-1046:1")]
// </GoSourcePositionMaps>

namespace go.@internal.trace.@internal;

[GoPackage("oldtrace")]
public static partial class oldtrace_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct batchOffset {}
    internal partial struct gState {}
    internal partial struct gStatus {}
    [GoValueClone("ev")] internal partial struct orderEvent {}
    internal partial struct orderEventList {}
    internal partial struct parser {}
    [GoLocalName("gdesc")] internal partial struct postProcessTrace_gdesc {}
    [GoLocalName("pdesc")] internal partial struct postProcessTrace_pdesc {}
    internal partial struct proc {}
    internal partial struct rawEvent {}
    [GoValueClone("Args")] public partial struct Event {}
    public partial struct EventDescriptionsᴛ1 {}
    public partial struct Events {}
    public partial struct Frame {}
    public partial struct Timestamp {}
    public partial struct Trace {}
    public partial struct ΔSTWReason {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸevent() => builtin.initPackage(typeof(go.@internal.trace.event_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    // </ImportInitializers>
}
