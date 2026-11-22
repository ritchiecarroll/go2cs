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
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Edge))]
[assembly: GoFieldAccess(typeof(Function))]
[assembly: GoFieldAccess(typeof(Label))]
[assembly: GoFieldAccess(typeof(Line))]
[assembly: GoFieldAccess(typeof(Location))]
[assembly: GoFieldAccess(typeof(Mapping))]
[assembly: GoFieldAccess(typeof(Node))]
[assembly: GoFieldAccess(typeof(NodeInfo))]
[assembly: GoFieldAccess(typeof(Profile))]
[assembly: GoFieldAccess(typeof(Sample))]
[assembly: GoFieldAccess(typeof(go.@internal.profile_package.ValueType))]
[assembly: GoFieldAccess(typeof(profileMerger))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/profile/encode.go", "encode.cs", "AA4agqzigpSCgqaCgoKClIKCgoIABhCCgpSCgoKCAAYQgoK4goKmgoKClKaClKaCgoKmgpSCgqaCgriCgpSClIKUgpSClIKCgoKCgpTGuIKCgriCgoK4goKCuIKCgriCgoK4goKUgpQACxiCgoIACxrSlIKCgoKmgoKCgoKmgoKCgoKCgoKUpqaCgqaCgoKikoKCgpSmgpSClIKClKaClICCtoCCgqSigoKmgoKCxoKmgoLGAAkQgqaCgoKUgtgABxCCgoLYgqaCgoLGAAsUgqaCgoKCgoKCgoKCxgAPHIKmgoKCgoLY3IKCgtiCpoKCxgAJEIKmgoKCgoLGAA0cgoKCgpSmgoKUgoKUgg==", "", "", "44=addString/1/2/2,addString/2/2/3;59=addString/1/1/2")]
[assembly: GoPositionMap("internal/profile/filter.go", "filter.cs", "AA8e4oKCgoKCgqaCytKCgoKClIK4goKClIK4")]
[assembly: GoPositionMap("internal/profile/graph.go", "graph.cs", "AECKAaKClKqigpSqoqrSgoKCgpSClKaCggANFpKokoKClIKmpgAICqQAFy6ygoK4gqamgqaCpoKCggAJDIKCgqamgqaCgoKCggAUKKKClMjCgoKCgpKCgpSClIKClAAIEoKClIKCgoKCgoKmgoKCpoKCpoKmlLimlIKCgpSClIKUlAAGEqKUpKQAChKCgpS4goKU3tKCgoKCgpSCgpSUxoKCgpSmsoKCpoKUxoKClMqCyMKCgpTGlIKClILaspSUgqaCgqSClIKUlMyygoKmgqiSgoKUAAgKgqaCgqaCgoKmgpSmgsaCgpQ=")]
[assembly: GoPositionMap("internal/profile/merge.go", "merge.cs", "AA4q8oKUgoKmAAcQlIKClMqmgoLKgqa4zNSCpoKCgriCgoK4goKClKaCxoKCgqYAFi6i7oKUgoKClIKCgoKCgoLKgoKClJSCgoLIwoKCpoKClJSCgpSUABAaooKmgoKmgu6CuIKCgpSCgoLIkriUgpSCgoKUlIIADBKigqaCuIKCgoKUAAsYpoKCgsqmtIKCgsqUtAAIDAALEIK4pqKClIKUgoKClO6CgoLIkgAOHqKCgriigoKCgoKUgoKUgoKCpoK4AA0cgszSgqaCpoKCpsqi", "", "", "207=Join/1/3/1,Join/2/3/2,Join/3/3/3;378=mapFunction/1/1/1")]
[assembly: GoPositionMap("internal/profile/profile.go", "profile.cs", "AHL4AaKCgqaCgoKUgoKUpoKCpoKUpoKUgoKmgoCCtoCCtsjSgoKCkoLe1IKClIKC3IKCgpSClJSCgoKUgpSUgoKClIKUgoKCpoKCgsqssoKCgoK4goKCgpSC3IKCgpSCgqaCyurEgoKUgoKUgqaCgoKUgoKCgpSCgpSCgoKCgpSUgoKClLiCgoKClIKUgoKCyoKmlLiCgoKClIKUgpSClAAHEK7igqamgqaUgoKUgoKUgoKmgoKCuIKs0oKmgqaCgriqooKCpqqigoKmxrKClMiygpSCgIKkgIK2AAsWtJKCuIKClIKCpqiSqJKClIKClKiSgpSCgoKCpoKUgoKCuA==", "", "", "", "182:21")]
[assembly: GoPositionMap("internal/profile/proto.go", "proto.cs", "ACpagoKCpoKCgpSmgoKmlIKmgpSCgpSCgoKCgoKUgriCgpSmgoKmgoKUpoKUgoKUgoKCgoKClIK4goKmgoK4goKUuIKClKaigoKCgoKCgqaCgqaCpoKmooKCgqbmgoKCgoKUgoIACgqCgoKUgoKCgpSCgsaClIK0goKClIKUgrSClIK0puaCgpSmooCCpIKChJKCgpSClIKmpqKClIKmopSClIKCgpSClJSUgoCCpIKmooKUgqailIKUgoKClIKUlJSCgIKkgqaCgpSCpqKCgpSCpqKClIKUlA==")]
[assembly: GoPositionMap("internal/profile/prune.go", "prune.cs", "AA4isoKUgoKCgpSClIKC3JSmlJTcuIKCgoKClIKUgoKUgoIABRCikpSCgpSCgqaU")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("profile")]
public static partial class profile_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(profile_package.Edge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛEdge;
[GoMethodHost(typeof(profile_package.EdgeMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛEdgeMap;
[GoMethodHost(typeof(profile_package.Function)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛFunction;
[GoMethodHost(typeof(profile_package.Graph)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛGraph;
[GoMethodHost(typeof(profile_package.Label)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛLabel;
[GoMethodHost(typeof(profile_package.Line)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛLine;
[GoMethodHost(typeof(profile_package.Location)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛLocation;
[GoMethodHost(typeof(profile_package.Mapping)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛMapping;
[GoMethodHost(typeof(profile_package.Node)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛNode;
[GoMethodHost(typeof(profile_package.NodeInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛNodeInfo;
[GoMethodHost(typeof(profile_package.NodeMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛNodeMap;
[GoMethodHost(typeof(profile_package.Nodes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛNodes;
[GoMethodHost(typeof(profile_package.Profile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛProfile;
[GoMethodHost(typeof(profile_package.Sample)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛSample;
[GoMethodHost(typeof(profile_package.ValueType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛValueType;
[GoMethodHost(typeof(profile_package.edgeList)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛedgeList;
[GoMethodHost(typeof(profile_package.locationMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛlocationMap;
[GoMethodHost(typeof(profile_package.profileMerger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class profileᴛprofileMerger;
// </MethodHolders>
