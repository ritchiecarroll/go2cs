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
using runtime = go.runtime_package;
// </ImportedTypeAliases>

using go;
using static go.runtime.pprof_package;
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
[assembly: GoDynamicTypeLift("7374727563747b6d752073796e632e4d757465783b206d206d61705b737472696e675d2a72756e74696d652f7070726f662e50726f66696c657d", "profilesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2070726f66696c696e6720626f6f6c3b20646f6e65206368616e20626f6f6c7d", "cpuᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.text.tabwriter_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<keysByCount, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<runtimeProfile, countProfile>(Pointer = true)]
[assembly: GoImplement<stackProfile, countProfile>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Profile))]
[assembly: GoFieldAccess(typeof(go.@internal.profilerecord_package.BlockProfileRecord))]
[assembly: GoFieldAccess(typeof(go.@internal.profilerecord_package.MemProfileRecord))]
[assembly: GoFieldAccess(typeof(labelMap))]
[assembly: GoFieldAccess(typeof(profMapEntry))]
[assembly: GoFieldAccess(typeof(profileBuilder))]
[assembly: GoFieldAccess(typeof(runtime_package.MemStats))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("runtime/pprof/elf.go", "elf.cs", "AA0egprCgoKClKSCuIKmgpSktLaCkpSkgoKClLSCgoKUtoKClIKUkpSCpoKUgoKClIKCgoKCgpSClIKUpg==", "", "", "", "28:21")]
[assembly: GoPositionMap("runtime/pprof/label.go", "label.cs", "ABgugoKClAAJGNKClJSCpoKqooKmgoKCgqaCkoKClIK0grSCgtqClAACEgAIAoKUgoKCgpSUhoKCgpSmlKqigoKCpqqigoKC", "111-113:1")]
[assembly: GoPositionMap("runtime/pprof/map.go", "map.cs", "ABw4lIKCgpSClpKCgoKUgoK4goKClNiClIKCgpSCpoKUgpSClIKCgpSClA==")]
[assembly: GoPositionMap("runtime/pprof/pe.go", "pe.cs", "AAwawoKClA==")]
[assembly: GoPositionMap("runtime/pprof/pprof.go", "pprof.cs", "AMAB+ALs7Ozs7OwAIz6CgpQABxCCyoIAAhIACAKCgoKUgpS4gtiygoLYsoKUgoSmhtiSqOKCgoKUAAUoABYCgpSCpoKCgpSmgoKClKKo8oKCogACIgARAoKUgriCgoKUppTKgoIACx6ygoKCpoKCgqYACA7UgoKCgpSUgoKCgoKmgoKU6qSSgoKCgpSCgpSUgoKCgoKCgoKUppSUgoKCgpS4goKClIKCgqaCgoKCkoK4lAAIEoKCgpKSgpQACAqigoKCgoKCgoKmgpSCpqaClK7CqJKCqJLqoqaCgqaCAAIU8oK4goKCgsqCppiCgpSCgoKCgoLcAAcQgoKCpsqCgqaClILKgoKCgoKCgpSCgoKCgpSCgoKCgpSCgoKCgoKCppSCqJKC6MiCupLokoKUqgAKDoK4uIK41riCgoKCgpSUlJSCpo7igpS4goKCgoLKAAcQgoIACSYADRQACgSCgoKmgpSCgoIABRAACQSCgoKClJSCgpSCpqaUgqzSgpSClIKCoqaSgqiSguiS6JIACAiSgoKCgoKCgriYgqaCgpSCgoKUgoKCgoKCgpSCgriClKjWpsbW1tbWxg==", "318-320:1;494-505:1;548-552:2;666-668:1;769-771:1;997-999:1", "", "775=InUseObjects/1/1/1;782=InUseObjects/1/1/1,InUseBytes/1/1/1")]
[assembly: GoPositionMap("runtime/pprof/pprof_windows.go", "pprof_windows.cs", "AA4egoKCgoI=")]
[assembly: GoPositionMap("runtime/pprof/proto.go", "proto.cs", "ABQuoABl3AGigoKCgpSmgoKCgrqSgoKCqJKCgoKClIKokoKCgoKokoKCgqiSgoKCgoKC7oKUpsqCgqamgoKmppSCgoKUAA8swoIACBKCrsKUgpSCuIKCpgARJIKClIKUgpSCgoKClJSC7qaClAAICLKUgoKCgoKCpoKUkoKUgoKSgsqUpoKC3IKCgpQAAhgACwKmgpSCgoAACxiCgoLKgrjugpSCtoKCgpSCpoCCgtqAgsiAgoKUgraAgqQAOHSCgoKCgqyypoKCgpSCpoKUgqaCgoKCgpQACw4ACQKClJSCip6C7oKCgsSCgoKCgu6UgoKUgoKCpqaCgoKCgoKCpoLWgtQAFSymgoKCgqaCgoKCgpSCgpSCgpSClJSCgpSCgoKUpoKCgqbKAAoWgriCpoI=", "369-373:1;690-695:1", "", "254=Now/1/1/3;324=FuncPCABIInternal/1/1/4;648=runtime_FrameStartLine/1/1/4")]
[assembly: GoPositionMap("runtime/pprof/proto_windows.go", "proto_windows.cs", "ABGGAYKCgqaU")]
[assembly: GoPositionMap("runtime/pprof/protobuf.go", "protobuf.cs", "AA4cgoKClKaCgqaUgqaClIKClIKCgoKCgpSCuIKClKaCgqaCgpSmgpSCgpSCgoKCgoKUgriCgqaCgriCgpSmgoKUuIKClMqCgqaCgoKCgoKCgg==")]
[assembly: GoPositionMap("runtime/pprof/protomem.go", "protomem.cs", "ABogkoKCgoKCgoKCpoKCooKCuIKCgqaCpoKClKaCgoKClJKCuAACGAAJAoKmpqaClA==", "60-64:1")]
[assembly: GoPositionMap("runtime/pprof/runtime.go", "runtime.cs", "AAsgysampqbGyrKCAAIUAAkCgoKCog==")]
// </GoSourcePositionMaps>

namespace go.runtime;

using static go.runtime.pprof_package;
using io = go.io_package;
using runtime = go.runtime_package;

[GoPackage("pprof")]
public static partial class pprof_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface countProfile {}
    internal partial struct cpuᴛ1 {}
    [GoLocalName("newFunc")] internal partial struct emitLocation_newFunc {}
    internal partial struct keysByCount {}
    internal partial struct labelContextKey {}
    internal partial struct labelMap {}
    internal partial struct locInfo {}
    internal partial struct memMap {}
    internal partial struct msgOffset {}
    internal partial struct pcDeck {}
    internal partial struct profMap {}
    internal partial struct profMapEntry {}
    [GoValueClone("pb")] internal partial struct profileBuilder {}
    internal partial struct profilesᴛ1 {}
    [GoValueClone("tmp")] internal partial struct protobuf {}
    internal partial struct runtimeProfile {}
    internal partial struct stackProfile {}
    internal partial struct symbolizeFlag {}
    public partial struct LabelSet {}
    public partial struct Profile {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindows() => builtin.initPackage(typeof(@internal.syscall.windows_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtabwriter() => builtin.initPackage(typeof(text.tabwriter_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(pprof_package.Profile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛProfile;
[GoMethodHost(typeof(pprof_package.keysByCount)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛkeysByCount;
[GoMethodHost(typeof(pprof_package.labelMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛlabelMap;
[GoMethodHost(typeof(pprof_package.pcDeck)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛpcDeck;
[GoMethodHost(typeof(pprof_package.profMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛprofMap;
[GoMethodHost(typeof(pprof_package.profileBuilder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛprofileBuilder;
[GoMethodHost(typeof(pprof_package.protobuf)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛprotobuf;
[GoMethodHost(typeof(pprof_package.runtimeProfile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛruntimeProfile;
[GoMethodHost(typeof(pprof_package.stackProfile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pprofᴛstackProfile;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class pprofᴛProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref Profile p) => pprof_package.Name⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Count(this ж<Profile> Ꮡp) => pprof_package.Count⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ж<Profile> Ꮡp, any value, nint skip) => pprof_package.Add⁔(Ꮡp, value, skip);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Remove(this ж<Profile> Ꮡp, any value) => pprof_package.Remove⁔(Ꮡp, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteTo(this ж<Profile> Ꮡp, io.Writer w, nint debug) => pprof_package.WriteTo⁔(Ꮡp, w, debug);
}
partial class pprofᴛkeysByCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref keysByCount x) => pprof_package.Len⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this ref keysByCount x, nint i, nint j) => pprof_package.Swap⁔(ref x, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this ref keysByCount x, nint i, nint j) => pprof_package.Less⁔(ref x, i, j);
}
partial class pprofᴛlabelMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<labelMap> Ꮡl) => pprof_package.String⁔(Ꮡl);
}
partial class pprofᴛpcDeck {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref pcDeck d) => pprof_package.reset⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*success*/ tryAdd(this ref pcDeck d, uintptr pc, slice<runtime.Frame> frames, symbolizeFlag symbolizeResult) => pprof_package.tryAdd⁔(ref d, pc, frames, symbolizeResult);
}
partial class pprofᴛprofMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<profMapEntry> lookup(this ref profMap m, slice<uint64> stk, unsafe_package.Pointer tag) => pprof_package.lookup⁔(ref m, stk, tag);
}
partial class pprofᴛprofileBuilder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 stringIndex(this ref profileBuilder b, @string s) => pprof_package.stringIndex⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this ref profileBuilder b) => pprof_package.flush⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pbValueType(this ref profileBuilder b, nint tag, @string typ, @string unit) => pprof_package.pbValueType⁔(ref b, tag, typ, unit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pbSample(this ref profileBuilder b, slice<int64> values, slice<uint64> locs, Action labels) => pprof_package.pbSample⁔(ref b, values, locs, labels);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pbLabel(this ref profileBuilder b, nint tag, @string key, @string str, int64 num) => pprof_package.pbLabel⁔(ref b, tag, key, str, num);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pbLine(this ref profileBuilder b, nint tag, uint64 funcID, int64 line) => pprof_package.pbLine⁔(ref b, tag, funcID, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void pbMapping(this ref profileBuilder b, nint tag, uint64 id, uint64 @base, uint64 limit, uint64 offset, @string @file, @string buildID, bool hasFuncs) => pprof_package.pbMapping⁔(ref b, tag, id, @base, limit, offset, @file, buildID, hasFuncs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error addCPUData(this ref profileBuilder b, slice<uint64> data, slice<unsafe_package.Pointer> tags) => pprof_package.addCPUData⁔(ref b, data, tags);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error build(this ж<profileBuilder> Ꮡb) => pprof_package.build⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uint64> /*newLocs*/ appendLocsForStack(this ж<profileBuilder> Ꮡb, slice<uint64> locs, slice<uintptr> stk) => pprof_package.appendLocsForStack⁔(Ꮡb, locs, stk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 emitLocation(this ж<profileBuilder> Ꮡb) => pprof_package.emitLocation⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addMapping(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID) => pprof_package.addMapping⁔(ref b, lo, hi, offset, @file, buildID);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addMappingEntry(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID, bool fake) => pprof_package.addMappingEntry⁔(ref b, lo, hi, offset, @file, buildID, fake);
}
partial class pprofᴛprotobuf {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void varint(this ref protobuf b, uint64 x) => pprof_package.varint⁔(ref b, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void length(this ref protobuf b, nint tag, nint len) => pprof_package.length⁔(ref b, tag, len);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint64(this ref protobuf b, nint tag, uint64 x) => pprof_package.uint64⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint64s(this ref protobuf b, nint tag, slice<uint64> x) => pprof_package.uint64s⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uint64Opt(this ref protobuf b, nint tag, uint64 x) => pprof_package.uint64Opt⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int64(this ref protobuf b, nint tag, int64 x) => pprof_package.int64⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int64Opt(this ref protobuf b, nint tag, int64 x) => pprof_package.int64Opt⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void int64s(this ref protobuf b, nint tag, slice<int64> x) => pprof_package.int64s⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @string(this ref protobuf b, nint tag, @string x) => pprof_package.string⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void strings(this ref protobuf b, nint tag, slice<@string> x) => pprof_package.strings⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stringOpt(this ref protobuf b, nint tag, @string x) => pprof_package.stringOpt⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void @bool(this ref protobuf b, nint tag, bool x) => pprof_package.bool⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void boolOpt(this ref protobuf b, nint tag, bool x) => pprof_package.boolOpt⁔(ref b, tag, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static msgOffset startMessage(this ref protobuf b) => pprof_package.startMessage⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void endMessage(this ref protobuf b, nint tag, msgOffset start) => pprof_package.endMessage⁔(ref b, tag, start);
}
partial class pprofᴛruntimeProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref runtimeProfile p) => pprof_package.Len⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uintptr> Stack(this ref runtimeProfile p, nint i) => pprof_package.Stack⁔(ref p, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<labelMap> Label(this ref runtimeProfile p, nint i) => pprof_package.Label⁔(ref p, i);
}
partial class pprofᴛstackProfile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this stackProfile x) => pprof_package.Len⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<uintptr> Stack(this stackProfile x, nint i) => pprof_package.Stack⁔(x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<labelMap> Label(this stackProfile x, nint i) => pprof_package.Label⁔(x, i);
}
// </MethodForwarders>
