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

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;
using static profile_package;
using strings = strings_package;
using strconv = go.strconv_package;
using go;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using io = io_package;
using time = time_package;
using compress;
using regexp = regexp_package;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class profileᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Profile p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void preEncode(this ref Profile p) => profile_package.preEncodeᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Profile p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error postDecode(this ж<Profile> Ꮡp) => profile_package.postDecodeᴍ(Ꮡp);
}
partial class profileᴛValueType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref ValueType p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref ValueType p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛSample {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Sample p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Sample p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛLabel {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this Label p) => profile_package.decoderᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this Label p, ж<buffer> b) => profile_package.encodeᴍ(p, b);
}
partial class profileᴛMapping {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Mapping p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Mapping p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛLocation {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Location p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Location p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛLine {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Line p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Line p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛFunction {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Function p) => profile_package.decoderᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void encode(this ref Function p, ж<buffer> b) => profile_package.encodeᴍ(ref p, b);
}
partial class profileᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (bool fm, bool im) FilterSamplesByTag(this ref Profile p, Func<@string, @string, int64, bool> focus, Func<@string, @string, int64, bool> ignore) => profile_package.FilterSamplesByTagᴍ(ref p, focus, ignore);
}
partial class profileᴛNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 FlatValue(this ref Node n) => profile_package.FlatValueᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 CumValue(this ref Node n) => profile_package.CumValueᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddToEdge(this ж<Node> Ꮡn, ж<Node> to, int64 v, bool residual, bool inline) => profile_package.AddToEdgeᴍ(Ꮡn, to, v, residual, inline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AddToEdgeDiv(this ж<Node> Ꮡn, ж<Node> Ꮡto, int64 dv, int64 v, bool residual, bool inline) => profile_package.AddToEdgeDivᴍ(Ꮡn, Ꮡto, dv, v, residual, inline);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addSample(this ref Node n, int64 dw, int64 w, bool flat) => profile_package.addSampleᴍ(ref n, dw, w, flat);
}
partial class profileᴛNodeInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string PrintableName(this ref NodeInfo i) => profile_package.PrintableNameᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> NameComponents(this ref NodeInfo i) => profile_package.NameComponentsᴍ(ref i);
}
partial class profileᴛNodeMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Node> FindOrInsertNode(this NodeMap nm, NodeInfo info, NodeSet kept) => profile_package.FindOrInsertNodeᴍ(nm, info, kept);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Nodes nodes(this NodeMap nm) => profile_package.nodesᴍ(nm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Node> findOrInsertLine(this NodeMap nm, ж<Location> Ꮡl, Line li, ж<Options> Ꮡo) => profile_package.findOrInsertLineᴍ(nm, Ꮡl, li, Ꮡo);
}
partial class profileᴛEdgeMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Edge> FindTo(this EdgeMap em, ж<Node> n) => profile_package.FindToᴍ(em, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ref EdgeMap em, ж<Edge> e) => profile_package.Addᴍ(ref em, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Delete(this ref EdgeMap em, ж<Edge> e) => profile_package.Deleteᴍ(ref em, e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<Edge>> Sort(this EdgeMap em) => profile_package.Sortᴍ(em);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Sum(this EdgeMap em) => profile_package.Sumᴍ(em);
}
partial class profileᴛEdge {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 WeightValue(this ref Edge e) => profile_package.WeightValueᴍ(ref e);
}
partial class profileᴛlocationMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref locationMap l, uint64 id, Nodes n) => profile_package.addᴍ(ref l, id, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Nodes get(this locationMap l, uint64 id) => profile_package.getᴍ(l, id);
}
partial class profileᴛNodes {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 flat, int64 cum) Sum(this Nodes ns) => profile_package.Sumᴍ(ns);
}
partial class profileᴛGraph {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Graph g) => profile_package.Stringᴍ(ref g);
}
partial class profileᴛedgeList {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this edgeList el) => profile_package.Lenᴍ(el);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this edgeList el, nint i, nint j) => profile_package.Lessᴍ(el, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this edgeList el, nint i, nint j) => profile_package.Swapᴍ(el, i, j);
}
partial class profileᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Normalize(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.Normalizeᴍ(ref p, Ꮡpb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error compatible(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.compatibleᴍ(ref p, Ꮡpb);
}
partial class profileᴛprofileMerger {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Sample> mapSample(this ref profileMerger pm, ж<Sample> Ꮡsrc) => profile_package.mapSampleᴍ(ref pm, Ꮡsrc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Location> mapLocation(this ref profileMerger pm, ж<Location> Ꮡsrc) => profile_package.mapLocationᴍ(ref pm, Ꮡsrc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static mapInfo mapMapping(this ref profileMerger pm, ж<Mapping> Ꮡsrc) => profile_package.mapMappingᴍ(ref pm, Ꮡsrc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Line mapLine(this ref profileMerger pm, Line src) => profile_package.mapLineᴍ(ref pm, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Function> mapFunction(this ref profileMerger pm, ж<Function> Ꮡsrc) => profile_package.mapFunctionᴍ(ref pm, Ꮡsrc);
}
partial class profileᴛSample {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static sampleKey key(this ref Sample sample) => profile_package.keyᴍ(ref sample);
}
partial class profileᴛLocation {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static locationKey key(this ref Location l) => profile_package.keyᴍ(ref l);
}
partial class profileᴛMapping {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static mappingKey key(this ref Mapping m) => profile_package.keyᴍ(ref m);
}
partial class profileᴛFunction {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static functionKey key(this ref Function f) => profile_package.keyᴍ(ref f);
}
partial class profileᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ж<Profile> Ꮡp, io.Writer w) => profile_package.Writeᴍ(Ꮡp, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error CheckValid(this ref Profile p) => profile_package.CheckValidᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Aggregate(this ref Profile p, bool inlineFrame, bool function, bool filename, bool linenumber, bool address) => profile_package.Aggregateᴍ(ref p, inlineFrame, function, filename, linenumber, address);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Profile p) => profile_package.Stringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Merge(this ref Profile p, ж<Profile> Ꮡpb, float64 r) => profile_package.Mergeᴍ(ref p, Ꮡpb, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Compatible(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.Compatibleᴍ(ref p, Ꮡpb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasFunctions(this ref Profile p) => profile_package.HasFunctionsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasFileLines(this ref Profile p) => profile_package.HasFileLinesᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Profile> Copy(this ж<Profile> Ꮡp) => profile_package.Copyᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Demangle(this ref Profile p, Func<slice<@string>, (map<@string, @string>, error)> d) => profile_package.Demangleᴍ(ref p, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Empty(this ref Profile p) => profile_package.Emptyᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Scale(this ref Profile p, float64 ratio) => profile_package.Scaleᴍ(ref p, ratio);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error ScaleN(this ref Profile p, slice<float64> ratios) => profile_package.ScaleNᴍ(ref p, ratios);
}
partial class profileᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Prune(this ref Profile p, ж<regexp.Regexp> dropRx, ж<regexp.Regexp> keepRx) => profile_package.Pruneᴍ(ref p, dropRx, keepRx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error RemoveUninteresting(this ref Profile p) => profile_package.RemoveUninterestingᴍ(ref p);
}
// </GlyphForwarders>
