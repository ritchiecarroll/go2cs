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
using io = go.io_package;
using regexp = go.regexp_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.profile_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoPositionMap("internal/profile/encode.go", "encode.cs", "AAsagqzigpSCgqaCgoKClIKCgoIABhCCgpSCgoKCAAYQgoK4goKmgoKClKaClKaCgoKmgpSCgqaCgriCgpSClIKUgpSClIKCgoKCgpSmuIKCgriCgoK4goKCuIKCgriCgoK4goKUgpQACxiCgoIACRrSlIKCgoKmgoKCgoKmgoKCgoKCgoKUpqaCgqaCgoKikoKCgpSmgpSClIKClKaClICCtoCCgqSigoKmgoKCpoKmgoKmAAcQgqaCgoKUgrgABxCCgoK4gqaCgoKmAAkUgqaCgoKCgoKCgoKCpgANHIKmgoKCgoK43IKCgriCpoKCpgAHEIKmgoKCgoKmAA0cgoKCgpSmgoKUgoKUgg==", "", "", "41=addString/1/2/2,addString/2/2/3;56=addString/1/1/2")]
[assembly: GoPositionMap("internal/profile/filter.go", "filter.cs", "AAse4oKCgoKCgqaCqtKCgoKClIK4goKClIK4")]
[assembly: GoPositionMap("internal/profile/graph.go", "graph.cs", "AD2KAaKClKqigpSqoqrSgoKCgpSClKaCggAJFpKokoKClIKmpgAICqQAEy6ygoK4gqamgqaCpoKCgtyCgoKmpoKmgoKCgoIAECiigpSowoKCgoKSgoKUgpSCgpQACBKCgpSCgoKCgoKCpoKCgqaCgqaCppS4ppSCgoKUgpSClJQABhKilKSkAAgSgoKUuIKClL7SgoKCgoKUgoKUlKaCgoKUprKCgqaClKaCgpTKgqjCgoKUppSCgpSCurKUlIKmgoKkgpSClJSssoKCpoKokoKClMqCpoKCpoKCgqaClKaCpoKClA==")]
[assembly: GoPositionMap("internal/profile/merge.go", "merge.cs", "AA0q8oKUgoKmAAcQlIKClMqmgoLKgqa4rNSCpoKCgriCgoK4goKClKaCpoKCgqYAFC6i7oKUgoKClIKCgoKCgoLKgoKClJSCgoKowoKCpoKClJSCgpSUAAwaooKmgoKmgu6CuIKCgpSCgoKokriUgpSCgoKUlIIACBKigqaCuIKCgoKUAAsYpoKCgqqmtIKCgsqUtAAIDAAHEIK4pqKClIKUgoKClO6CgoKokgAMHqKCgriigoKCgoKUgoKUgoKCpoK4AA0cgqzSgqaCpoKCpqqi", "", "", "198=Join/1/3/1,Join/2/3/2,Join/3/3/3;353=mapFunction/1/1/1")]
[assembly: GoPositionMap("internal/profile/profile.go", "profile.cs", "AHH4AaKCgqaCgoKUgoKUpoKCpoKUpoKUgoKmgoCCtoCCtqjSgoKCkoLe1IKClIKC3IKCgpSClJSCgoKUgpSUgoKClIKUgoKCpoKCgsqssoKCgoK4goKCgpSC3IKCgpSCgqaCyurEgoKUgoKUgqaCgoKUgoKCgpSCgpSCgoKCgpSUgoKClLiCgoKClIKUgoKCyoKmlLiCgoKClIKUgpSClAAHEK7igqamgqaUgoKUgoKUgoKmgoKCuIKs0oKmgqaCgriqooKCpqqigoKmprKClKiygpSCgIKkgIK2AAcWtJKCuIKClIKCpqiSqJKClIKClKiSgpSCgoKCpoKUgoKCuA==", "", "", "", "179:21")]
[assembly: GoPositionMap("internal/profile/proto.go", "proto.cs", "ACpagoKCpoKCgpSmgoKmlIKmgpSCgpSCgoKCgoKUgriCgpSmgoKmgoKUpoKUgoKUgoKCgoKClIK4goKmgoK4goKUuIKClKaigoKCgoKCgqaCgqaCpoKmooKCgqbmgoKCgoKUgoIACgqCgoKUgoKCgpSCgsaClIK0goKClIKUgrSClIK0puaCgpSmooCCpIKChJKCgpSClIKmpqKClIKmopSClIKCgpSClJSUgoCCpIKmooKUgqailIKUgoKClIKUlJSCgIKkgqaCgpSCpqKCgpSCpqKClIKUlA==")]
[assembly: GoPositionMap("internal/profile/prune.go", "prune.cs", "AAsisoKUgoKCgpSClIKC3JSmlJTcuIKCgoKClIKUgoKUgoIABRCikpSCgpSCgqaU")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.profile_package;
using io = go.io_package;
using regexp = go.regexp_package;

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
[GoMethodHost(typeof(profile_package.Edge)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛEdge;
[GoMethodHost(typeof(profile_package.EdgeMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛEdgeMap;
[GoMethodHost(typeof(profile_package.Function)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛFunction;
[GoMethodHost(typeof(profile_package.Graph)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛGraph;
[GoMethodHost(typeof(profile_package.Label)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛLabel;
[GoMethodHost(typeof(profile_package.Line)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛLine;
[GoMethodHost(typeof(profile_package.Location)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛLocation;
[GoMethodHost(typeof(profile_package.Mapping)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛMapping;
[GoMethodHost(typeof(profile_package.Node)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛNode;
[GoMethodHost(typeof(profile_package.NodeInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛNodeInfo;
[GoMethodHost(typeof(profile_package.NodeMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛNodeMap;
[GoMethodHost(typeof(profile_package.Nodes)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛNodes;
[GoMethodHost(typeof(profile_package.Profile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛProfile;
[GoMethodHost(typeof(profile_package.Sample)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛSample;
[GoMethodHost(typeof(profile_package.ValueType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛValueType;
[GoMethodHost(typeof(profile_package.edgeList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛedgeList;
[GoMethodHost(typeof(profile_package.locationMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛlocationMap;
[GoMethodHost(typeof(profile_package.profileMerger)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class profileᴛprofileMerger;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class profileᴛEdge {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 WeightValue(this ref Edge e) => profile_package.WeightValue⁔(ref e);
}
partial class profileᴛEdgeMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Edge> FindTo(this EdgeMap em, ж<Node> Ꮡn) => profile_package.FindTo⁔(em, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ref EdgeMap em, ж<Edge> Ꮡe) => profile_package.Add⁔(ref em, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Delete(this ref EdgeMap em, ж<Edge> Ꮡe) => profile_package.Delete⁔(ref em, Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<Edge>> Sort(this EdgeMap em) => profile_package.Sort⁔(em);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Sum(this EdgeMap em) => profile_package.Sum⁔(em);
}
partial class profileᴛFunction {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Function p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Function p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static functionKey key(this ref Function f) => profile_package.key⁔(ref f);
}
partial class profileᴛGraph {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Graph g) => profile_package.String⁔(ref g);
}
partial class profileᴛLabel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this Label p) => profile_package.decoder⁔(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this Label p, ж<buffer> Ꮡb) => profile_package.encode⁔(p, Ꮡb);
}
partial class profileᴛLine {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Line p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Line p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
}
partial class profileᴛLocation {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Location p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Location p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static locationKey key(this ref Location l) => profile_package.key⁔(ref l);
}
partial class profileᴛMapping {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Mapping p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Mapping p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static mappingKey key(this ref Mapping m) => profile_package.key⁔(ref m);
}
partial class profileᴛNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 FlatValue(this ref Node n) => profile_package.FlatValue⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 CumValue(this ref Node n) => profile_package.CumValue⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddToEdge(this ж<Node> Ꮡn, ж<Node> Ꮡto, int64 v, bool residual, bool inline) => profile_package.AddToEdge⁔(Ꮡn, Ꮡto, v, residual, inline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddToEdgeDiv(this ж<Node> Ꮡn, ж<Node> Ꮡto, int64 dv, int64 v, bool residual, bool inline) => profile_package.AddToEdgeDiv⁔(Ꮡn, Ꮡto, dv, v, residual, inline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addSample(this ref Node n, int64 dw, int64 w, bool flat) => profile_package.addSample⁔(ref n, dw, w, flat);
}
partial class profileᴛNodeInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PrintableName(this ref NodeInfo i) => profile_package.PrintableName⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> NameComponents(this ref NodeInfo i) => profile_package.NameComponents⁔(ref i);
}
partial class profileᴛNodeMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Node> FindOrInsertNode(this NodeMap nm, NodeInfo info, NodeSet kept) => profile_package.FindOrInsertNode⁔(nm, info, kept);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Nodes nodes(this NodeMap nm) => profile_package.nodes⁔(nm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Node> findOrInsertLine(this NodeMap nm, ж<Location> Ꮡl, Line li, ж<Options> Ꮡo) => profile_package.findOrInsertLine⁔(nm, Ꮡl, li, Ꮡo);
}
partial class profileᴛNodes {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 flat, int64 cum) Sum(this Nodes ns) => profile_package.Sum⁔(ns);
}
partial class profileᴛProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Profile p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void preEncode(this ref Profile p) => profile_package.preEncode⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Profile p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error postDecode(this ж<Profile> Ꮡp) => profile_package.postDecode⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool fm, bool im) FilterSamplesByTag(this ref Profile p, Func<@string, @string, int64, bool> focus, Func<@string, @string, int64, bool> ignore) => profile_package.FilterSamplesByTag⁔(ref p, focus, ignore);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Normalize(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.Normalize⁔(ref p, Ꮡpb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error compatible(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.compatible⁔(ref p, Ꮡpb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ж<Profile> Ꮡp, io.Writer w) => profile_package.Write⁔(Ꮡp, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error CheckValid(this ref Profile p) => profile_package.CheckValid⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Aggregate(this ref Profile p, bool inlineFrame, bool function, bool filename, bool linenumber, bool address) => profile_package.Aggregate⁔(ref p, inlineFrame, function, filename, linenumber, address);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Profile p) => profile_package.String⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Merge(this ref Profile p, ж<Profile> Ꮡpb, float64 r) => profile_package.Merge⁔(ref p, Ꮡpb, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Compatible(this ref Profile p, ж<Profile> Ꮡpb) => profile_package.Compatible⁔(ref p, Ꮡpb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasFunctions(this ref Profile p) => profile_package.HasFunctions⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool HasFileLines(this ref Profile p) => profile_package.HasFileLines⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Profile> Copy(this ж<Profile> Ꮡp) => profile_package.Copy⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Demangle(this ref Profile p, Func<slice<@string>, (map<@string, @string>, error)> d) => profile_package.Demangle⁔(ref p, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Empty(this ref Profile p) => profile_package.Empty⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Scale(this ref Profile p, float64 ratio) => profile_package.Scale⁔(ref p, ratio);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ScaleN(this ref Profile p, slice<float64> ratios) => profile_package.ScaleN⁔(ref p, ratios);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Prune(this ref Profile p, ж<regexp.Regexp> ᏑdropRx, ж<regexp.Regexp> ᏑkeepRx) => profile_package.Prune⁔(ref p, ᏑdropRx, ᏑkeepRx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RemoveUninteresting(this ref Profile p) => profile_package.RemoveUninteresting⁔(ref p);
}
partial class profileᴛSample {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Sample p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref Sample p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static sampleKey key(this ref Sample sample) => profile_package.key⁔(ref sample);
}
partial class profileᴛValueType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref ValueType p) => profile_package.decoder⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encode(this ref ValueType p, ж<buffer> Ꮡb) => profile_package.encode⁔(ref p, Ꮡb);
}
partial class profileᴛedgeList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this edgeList el) => profile_package.Len⁔(el);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this edgeList el, nint i, nint j) => profile_package.Less⁔(el, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this edgeList el, nint i, nint j) => profile_package.Swap⁔(el, i, j);
}
partial class profileᴛlocationMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref locationMap l, uint64 id, Nodes n) => profile_package.add⁔(ref l, id, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Nodes get(this locationMap l, uint64 id) => profile_package.get⁔(l, id);
}
partial class profileᴛprofileMerger {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Sample> mapSample(this ref profileMerger pm, ж<Sample> Ꮡsrc) => profile_package.mapSample⁔(ref pm, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Location> mapLocation(this ref profileMerger pm, ж<Location> Ꮡsrc) => profile_package.mapLocation⁔(ref pm, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static mapInfo mapMapping(this ref profileMerger pm, ж<Mapping> Ꮡsrc) => profile_package.mapMapping⁔(ref pm, Ꮡsrc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Line mapLine(this ref profileMerger pm, Line src) => profile_package.mapLine⁔(ref pm, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Function> mapFunction(this ref profileMerger pm, ж<Function> Ꮡsrc) => profile_package.mapFunction⁔(ref pm, Ꮡsrc);
}
// </MethodForwarders>
