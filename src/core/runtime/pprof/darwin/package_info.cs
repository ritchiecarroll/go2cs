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
using static go.runtime.pprof_package;

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
[assembly: GoPositionMap("runtime/pprof/label.go", "label.cs", "ABkugoKClAALGNKClJSCpoLKooKmgoKCgqaCkoKClIK0grSCgtqClAACEgAIAoKUgoKCgpSUhoKCgpSmlKqigoKCpqqigoKC", "111-113:1")]
[assembly: GoPositionMap("runtime/pprof/map.go", "map.cs", "AB84lIKCgpSClpKCgoKUgoK4goKClNiClIKCgpSCpoKUgpSClIKCgpSClA==")]
[assembly: GoPositionMap("runtime/pprof/pe.go", "pe.cs", "AAwawoKClA==")]
[assembly: GoPositionMap("runtime/pprof/pprof.go", "pprof.cs", "AMEB+ALs7Ozs7OwAIz6CgpQABxCCyoIAAhIACAKCgoKUgpS4gtiygoLYsoKUgoSmhviSqOKCgoKUAAUoABYCgpSCpoKCgpSmgoKClKKo8oKCogACIgARAoKUgriCgoKUppQACAqCggANHrKCgoKmgoKCpgAIDtSCgoKClJSCgoKCgqaCgpTqpJKCgoKClIKClJSCgoKCgoKCgpSmlJSCgoKClLiCgoKUgoKCpoKCgoKSgriUAAoSgoKCkpKClAAKCqKCgoKCgoKCgqaClIKmpoKUrsKokoKokuqipoKCpoIAAhTygriCgoKCyoKmmIKClIKCgoKCgtwABxCCgoKmyoKCpoKUgsqCgoKCgoKClIKCgoKClIKCgoKClIKCgoKCgoKmlIKokoLoyIK6kuiSgpSqAAoOgri4grjWuIKCgoKClJSUlIKmjuKClLiCgoKCgsoACRCCggALJgANFAAKBIKCgqaClIKCggAFEAAJBIKCgoKUlIKClIKmppSCrNKClIKUgoKippKCqJKC6JLokgAICJKCgoKCgoKCuJiCpoKClIKCgpSCgoKCgoKClIKCuIKUqNamxtbW1tbG", "318-320:1;494-505:1;548-552:2;666-668:1;769-771:1;997-999:1", "", "788=InUseObjects/1/1/1;795=InUseObjects/1/1/1,InUseBytes/1/1/1")]
[assembly: GoPositionMap("runtime/pprof/pprof_rusage.go", "pprof_rusage.cs", "AA0ikoKktLS0toKCgg==")]
[assembly: GoPositionMap("runtime/pprof/proto.go", "proto.cs", "ABUuoABn3AGigoKCgpSmgoKCgrqSgoKCqJKCgoKClIKokoKCgoKokoKCgqiSgoKCgoKC7oKUxsqCgqamgoKmppSCgoKUAA8swoIACBKCzsKUgpSCuIKCpgARJIKClIKUgpSCgoKClJSC7qaClAAMCLKUgoKCgoKCpoKUkoKUgoKSgsqUpoKC3IKCgpQAAhgACwKmgpSCgoAACxiCgoLKgrjugpSCtoKCgpSCpoCCgtqAgsiAgoKUgraAgqQAPHSCgoKCgqyypoKCgpSCpoKUgqaCgoKCgpQADw4ACQKClJSCip6C7oKCgsSCgoKCgu6UgoKUgoKCpqaCgoKCgoKCpoL2gtQAFSymgoKCgqaCgoKCgpSCgpSCgpSClJSCgpSCgoKUpoKCgqbKAAoWgtiCpoI=", "369-373:1;690-695:1", "", "259=Now/1/1/3;331=FuncPCABIInternal/1/1/4;667=runtime_FrameStartLine/1/1/4")]
[assembly: GoPositionMap("runtime/pprof/protobuf.go", "protobuf.cs", "ABIcgoKClKaCgqaUgqaClIKClIKCgoKCgpSCuIKClKaCgqaCgpSmgpSCgpSCgoKCgoKUgriCgqaCgriCgpSmgoKUuIKClAAICoKCpoKCgoKCgoKC")]
[assembly: GoPositionMap("runtime/pprof/protomem.go", "protomem.cs", "ABogkoKCgoKCgoKCpoKCooKCuIKCgqaCpoKClKaCgoKClJKCuAACGAAJAoKmpqaClA==", "60-64:1")]
[assembly: GoPositionMap("runtime/pprof/runtime.go", "runtime.cs", "AAsgysampqbGyrKCAAIUAAkCgoKCog==")]
[assembly: GoPositionMap("runtime/pprof/vminfo_darwin.go", "vminfo_darwin.cs", "AAwagqqigpKCgpS4gpSUlLjKlLiCgsqClK72", "", "", "42=LEUint64/1/1/2,regionFilename/1/1/3;55=Getpid/1/1/1")]
// </GoSourcePositionMaps>

namespace go.runtime;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using strings = strings_package;
using @internal.runtime.pprof;
using static pprof_package;
using @unsafe = unsafe_package;
using io = io_package;
using runtime = runtime_package;
using @internal;
using text;
using compress;
// </GlyphForwarderUsings>

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
    [GoValueClone("Offset", "Pad_cgo_1")] internal partial struct machVMRegionBasicInfoData {}
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
[GoMethodHost(typeof(pprof_package.Profile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛProfile;
[GoMethodHost(typeof(pprof_package.keysByCount)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛkeysByCount;
[GoMethodHost(typeof(pprof_package.labelMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛlabelMap;
[GoMethodHost(typeof(pprof_package.pcDeck)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛpcDeck;
[GoMethodHost(typeof(pprof_package.profMap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛprofMap;
[GoMethodHost(typeof(pprof_package.profileBuilder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛprofileBuilder;
[GoMethodHost(typeof(pprof_package.protobuf)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛprotobuf;
[GoMethodHost(typeof(pprof_package.runtimeProfile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛruntimeProfile;
[GoMethodHost(typeof(pprof_package.stackProfile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pprofᴛstackProfile;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class pprofᴛlabelMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<labelMap> Ꮡl) => pprof_package.Stringᴍ(Ꮡl);
}
partial class pprofᴛprofMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<profMapEntry> lookup(this ref profMap m, slice<uint64> stk, @unsafe.Pointer tag) => pprof_package.lookupᴍ(ref m, stk, tag);
}
partial class pprofᴛProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref Profile p) => pprof_package.Nameᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Count(this ж<Profile> Ꮡp) => pprof_package.Countᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ж<Profile> Ꮡp, any value, nint skip) => pprof_package.Addᴍ(Ꮡp, value, skip);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Remove(this ж<Profile> Ꮡp, any value) => pprof_package.Removeᴍ(Ꮡp, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteTo(this ж<Profile> Ꮡp, io.Writer w, nint debug) => pprof_package.WriteToᴍ(Ꮡp, w, debug);
}
partial class pprofᴛstackProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this stackProfile x) => pprof_package.Lenᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uintptr> Stack(this stackProfile x, nint i) => pprof_package.Stackᴍ(x, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<labelMap> Label(this stackProfile x, nint i) => pprof_package.Labelᴍ(x, i);
}
partial class pprofᴛkeysByCount {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref keysByCount x) => pprof_package.Lenᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this ref keysByCount x, nint i, nint j) => pprof_package.Swapᴍ(ref x, i, j);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this ref keysByCount x, nint i, nint j) => pprof_package.Lessᴍ(ref x, i, j);
}
partial class pprofᴛruntimeProfile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref runtimeProfile p) => pprof_package.Lenᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uintptr> Stack(this ref runtimeProfile p, nint i) => pprof_package.Stackᴍ(ref p, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<labelMap> Label(this ref runtimeProfile p, nint i) => pprof_package.Labelᴍ(ref p, i);
}
partial class pprofᴛprofileBuilder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 stringIndex(this ref profileBuilder b, @string s) => pprof_package.stringIndexᴍ(ref b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flush(this ref profileBuilder b) => pprof_package.flushᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pbValueType(this ref profileBuilder b, nint tag, @string typ, @string unit) => pprof_package.pbValueTypeᴍ(ref b, tag, typ, unit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pbSample(this ref profileBuilder b, slice<int64> values, slice<uint64> locs, Action labels) => pprof_package.pbSampleᴍ(ref b, values, locs, labels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pbLabel(this ref profileBuilder b, nint tag, @string key, @string str, int64 num) => pprof_package.pbLabelᴍ(ref b, tag, key, str, num);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pbLine(this ref profileBuilder b, nint tag, uint64 funcID, int64 line) => pprof_package.pbLineᴍ(ref b, tag, funcID, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void pbMapping(this ref profileBuilder b, nint tag, uint64 id, uint64 @base, uint64 limit, uint64 offset, @string @file, @string buildID, bool hasFuncs) => pprof_package.pbMappingᴍ(ref b, tag, id, @base, limit, offset, @file, buildID, hasFuncs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error addCPUData(this ref profileBuilder b, slice<uint64> data, slice<@unsafe.Pointer> tags) => pprof_package.addCPUDataᴍ(ref b, data, tags);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error build(this ж<profileBuilder> Ꮡb) => pprof_package.buildᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uint64> /*newLocs*/ appendLocsForStack(this ж<profileBuilder> Ꮡb, slice<uint64> locs, slice<uintptr> stk) => pprof_package.appendLocsForStackᴍ(Ꮡb, locs, stk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 emitLocation(this ж<profileBuilder> Ꮡb) => pprof_package.emitLocationᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addMapping(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID) => pprof_package.addMappingᴍ(ref b, lo, hi, offset, @file, buildID);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addMappingEntry(this ref profileBuilder b, uint64 lo, uint64 hi, uint64 offset, @string @file, @string buildID, bool fake) => pprof_package.addMappingEntryᴍ(ref b, lo, hi, offset, @file, buildID, fake);
}
partial class pprofᴛpcDeck {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref pcDeck d) => pprof_package.resetᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool /*success*/ tryAdd(this ref pcDeck d, uintptr pc, slice<runtime.Frame> frames, symbolizeFlag symbolizeResult) => pprof_package.tryAddᴍ(ref d, pc, frames, symbolizeResult);
}
partial class pprofᴛprotobuf {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void varint(this ref protobuf b, uint64 x) => pprof_package.varintᴍ(ref b, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void length(this ref protobuf b, nint tag, nint len) => pprof_package.lengthᴍ(ref b, tag, len);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint64(this ref protobuf b, nint tag, uint64 x) => pprof_package.uint64ᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint64s(this ref protobuf b, nint tag, slice<uint64> x) => pprof_package.uint64sᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uint64Opt(this ref protobuf b, nint tag, uint64 x) => pprof_package.uint64Optᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void int64(this ref protobuf b, nint tag, int64 x) => pprof_package.int64ᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void int64Opt(this ref protobuf b, nint tag, int64 x) => pprof_package.int64Optᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void int64s(this ref protobuf b, nint tag, slice<int64> x) => pprof_package.int64sᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @string(this ref protobuf b, nint tag, @string x) => pprof_package.@stringᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void strings(this ref protobuf b, nint tag, slice<@string> x) => pprof_package.stringsᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stringOpt(this ref protobuf b, nint tag, @string x) => pprof_package.stringOptᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @bool(this ref protobuf b, nint tag, bool x) => pprof_package.@boolᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void boolOpt(this ref protobuf b, nint tag, bool x) => pprof_package.boolOptᴍ(ref b, tag, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static msgOffset startMessage(this ref protobuf b) => pprof_package.startMessageᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void endMessage(this ref protobuf b, nint tag, msgOffset start) => pprof_package.endMessageᴍ(ref b, tag, start);
}
// </GlyphForwarders>
