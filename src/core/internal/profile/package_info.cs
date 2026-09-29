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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.@internal.profile_package;

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
[assembly: GoImplement<Function, message>(Pointer = true)]
[assembly: GoImplement<Label, message>(Pointer = true)]
[assembly: GoImplement<Label, message>]
[assembly: GoImplement<Line, message>(Pointer = true)]
[assembly: GoImplement<Location, message>(Pointer = true)]
[assembly: GoImplement<Mapping, message>(Pointer = true)]
[assembly: GoImplement<Profile, message>(Pointer = true)]
[assembly: GoImplement<Sample, message>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<edgeList, sort_package.Interface>]
[assembly: GoImplement<go.@internal.profile_package.ValueType, message>(Pointer = true)]
[assembly: GoImplement<go.compress.gzip_package.Reader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Function, ж<Function>>(Indirect = true)]
[assembly: GoImplicitConv<Label, ж<Label>>(Indirect = true)]
[assembly: GoImplicitConv<Line, ж<Line>>(Indirect = true)]
[assembly: GoImplicitConv<Location, ж<Location>>(Indirect = true)]
[assembly: GoImplicitConv<Mapping, ж<Mapping>>(Indirect = true)]
[assembly: GoImplicitConv<Options, ж<Options>>(Indirect = true)]
[assembly: GoImplicitConv<Sample, ж<Sample>>(Indirect = true)]
[assembly: GoImplicitConv<go.@internal.profile_package.ValueType, ж<go.@internal.profile_package.ValueType>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/profile/encode.go", "encode.cs", "AAsagqyygoSCgpaCgoKClIKCgoIABhCCgpSCgoKCAAYQgoKogoKWgoKClKaClKaCgoKWgoSAgoKmgoK4goKUgpSClIKUgpSCgoKCgoCCpAASBqiCgoKogoKCqIKCgqiCgoKogoKCqIKClIKUAAYYgoKCAAYa0oSCgoKCloKCgoKCloKCgoKCgIKCgpS2loKCloKCgqKSgoKClKaClIKUgoKUloKEgIKmgIKCpKKCgpaCgoKmgqaCguYABRCCpoKCgpSCAAgIAAQQgoKCuIKmgoKC9gAGFIKmgoKCgoKCgoKCggAOBgANHIKmgoKCgoIACQjcgoKCuIKmgoLmAAUQgqaCgoKCggAJBgAIHIKCgoKUpoKClIKClII=")]
[assembly: go.GoPositionMap("internal/profile/filter.go", "filter.cs", "AAoe4oKCgoKCgqaCqtKCgoKClIK4goKClIK4")]
[assembly: go.GoPositionMap("internal/profile/graph.go", "graph.cs", "ADyKAaKClKqigpSqoqrSgIKCgoKUgpSmgoIACRaSqJKCgpSAgqaW+tQACC6ygoCCuICCpqaCpoKmgoKCzIKCgqamgqaCgoKCggAQKKKClKjCgoKCgpKCgpSClIKUgpSUAAcSgoCCpIKCgoKCgoKmgoKCpoKCloKmlKimlIKCgpSClIKUlAAGEqKUpKQACRKCgpS4goKUvtKCgoKCgpSCgpSUpoKCgpSmsoKAgqaAgqSmgoKUyoKowoKClKaUgoKUgrqShISCloKClIKUgpSUrLKCgpaCqJKCgpTKgqaCgpaCgoKWgoSmgqaCgpQ=")]
[assembly: go.GoPositionMap("internal/profile/merge.go", "merge.cs", "AAwq8oKUgoKWAAYQlIKChMqWgoK6gqaorNSAgqaCgoKogoKCqIKCgpSmgqaCgoKmABQuou6ClIKCgpSCgoKCgoKCyoKAgoKUpIKCgqiSgoKWgoKUhIKClIQADBqigpaAgoKmgu6CuIKAgoKkgoKCqJK4lIKUgoKClJSCAAgSooKWgIK4goCCgoKkAAsYloKCgqqmpIKCgrqUtAAJDAAHEIK4pqKClICCpIKAgoKk7oKCgqiSAAweooKAgriigoKCgoKUgoKUgoCCgraCqAAKHIKs0oKWgpaCgqaqog==")]
[assembly: go.GoPositionMap("internal/profile/profile.go", "profile.cs", "AHH4AaKCgpaCgoKUgoKUloKCloCCpKaSlIKCloKAgqaAgqao0oKCgpKC3tSCgpSCgsyCgoKUgpSUgoKClIKUlIKCgpSClIKAgoK2goCCgtqssoKCgoKogoKCgpSCzIKCgpSCgqaCutqkgoCCpIKClIKWgoKClIKCgoKUgoKUgoKCgoKUlIKCgpSogoKCgIKkgpSCgoCCyoK2lKiCgoKClIKUgpSClAAGEK7igIKmloKWhIKClIKClIKCloKCgriCrNKCloKWgoKoqqKCgqaqooKCpqaygpSosoKEgoCCpICCpgAEFrSSgqiCgpSCgIK2qJKokoKUgoKUqJKClIKCgoKmgpSCgoK4")]
[assembly: go.GoPositionMap("internal/profile/proto.go", "proto.cs", "AClYgoKCpoKCgpSmgoKmlIKmgpSCgpSCgoKCgoKUgriCgpSmgoKmgoKUpoKUgoKUgoKCgoKClIK4goKmgoK4goKUuIKClKaigoKCgoKCgqaCkqaCpoLWgoKCgoKUgoIACAqCgoKUgoKCgpSCgsaClIK0goKClIKUgrSClIK0ptaCgpSmooCCpIKChJKCgpSClICCtqaigIKkgqailIKCgoSAgqSUlIKAgqSCpqKAgqSCpqKClIKChICCpJSUgoCCpIKmgoCCpIKmooKAgqSCpqKAgqSClJQ=")]
[assembly: go.GoPositionMap("internal/profile/prune.go", "prune.cs", "AAsisoKEgoKCgIKUgpSCgtyUlpSUzLiCgoKCgpSClIKClIKCAAUQopKEgoCCpIKAgraU")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("profile")]
public static partial class profile_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface message {}
    [GoValueClone("tmp")] internal partial struct buffer {}
    internal partial struct edgeList {}
    internal partial struct functionKey {}
    internal partial struct locationKey {}
    internal partial struct mapInfo {}
    internal partial struct mappingKey {}
    internal partial struct nodePair {}
    internal partial struct profileMerger {}
    internal partial struct sampleKey {}
    public partial struct Edge {}
    public partial struct EdgeMap {}
    public partial struct Function {}
    public partial struct Graph {}
    public partial struct Label {}
    public partial struct Line {}
    public partial struct Location {}
    public partial struct Mapping {}
    public partial struct Node {}
    public partial struct NodeInfo {}
    public partial struct NodeMap {}
    public partial struct NodePtrSet {}
    public partial struct NodeSet {}
    public partial struct Nodes {}
    public partial struct Options {}
    public partial struct Profile {}
    public partial struct Sample {}
    public partial struct ValueType {}
    public partial struct locationMap {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
