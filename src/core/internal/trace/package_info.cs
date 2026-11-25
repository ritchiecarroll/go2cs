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
using iter = go.iter_package;
using time = go.time_package;
using tracev1 = go.@internal.trace.@internal.tracev1_package;
using tracev2 = go.@internal.trace.tracev2_package;
using version = go.@internal.trace.version_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.trace_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b696f2e5265616465723b20696f2e427974655265616465727d", "readBatch_r")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bandUtilHeap, go.container.heap_package.Interface>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, readBatch_r>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteReader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<utilHeap, go.container.heap_package.Interface>(Pointer = true)]
[assembly: GoImplement<utilHeap, sort_package.Interface>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Time, timestamp>(Inverted = true, ValueType = "int64")]
[assembly: GoImplicitConv<Time, tracev1.Timestamp>(Inverted = false, ValueType = "int64")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ClockSnapshot))]
[assembly: GoFieldAccess(typeof(GoroutineSummary))]
[assembly: GoFieldAccess(typeof(MutatorUtilizationV2_perP))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(UserRegionSummary))]
[assembly: GoFieldAccess(typeof(UserTaskSummary))]
[assembly: GoFieldAccess(typeof(accumulator))]
[assembly: GoFieldAccess(typeof(batch))]
[assembly: GoFieldAccess(typeof(batchCursor))]
[assembly: GoFieldAccess(typeof(cpuSample))]
[assembly: GoFieldAccess(typeof(evTable))]
[assembly: GoFieldAccess(typeof(gState))]
[assembly: GoFieldAccess(typeof(generation))]
[assembly: GoFieldAccess(typeof(go.@internal.trace.@internal.tracev1_package.Event))]
[assembly: GoFieldAccess(typeof(go.@internal.trace.tracev2_package.EventSpec))]
[assembly: GoFieldAccess(typeof(mState))]
[assembly: GoFieldAccess(typeof(mmuSeries))]
[assembly: GoFieldAccess(typeof(mud))]
[assembly: GoFieldAccess(typeof(pState))]
[assembly: GoFieldAccess(typeof(traceV1Converter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/trace/base.go", "base.cs", "AB5CopSkABMwooKUgpSClIKCgoKqooKUAAse8oKUgpSCqLKClIKCrNKUpoKCgoKUgqaUpoKUgpSCgoKCgpSqwoKUgoKUgoKmqLKCgpSCpoKUgoKmrNKCgpTekgASMPYABxCCAAcQgoKClA==")]
[assembly: GoPositionMap("internal/trace/batch.go", "batch.cs", "ABw6gqaCpoKmgqqCAAgIqoKClICCpICCyIKCgoKUyoKClIKClIKCuIKClIKokoKCgpSCuA==", "", "", "119=Bytes/1/1/3")]
[assembly: GoPositionMap("internal/trace/batchcursor.go", "batchcursor.cs", "ABYu1oKCgqaCpoKmgoKmpqaCpqIAAhQACASCgoKUpoKClKaCgpSmgoKClIKUppSmgqaUgqamlIKCpoKUgqaCgoKUpoKCgpSUgpSmgoKCgpSCgoKmgoKCpg==")]
[assembly: GoPositionMap("internal/trace/event.go", "event.cs", "AFq8AZKClKYAKFSSAHD4AaSCgoIACxqSkoKUgoKC3IIABRKygoKmgoKC3AAiSpKClIKClAAjUgAIBMqkgpSCuNaClIKUpILGgILWgILWgILWgsaAgtaAgtaAgtaCtuaigpSktLS0lIKmooKUgpSCppbWpIKUlLS0tMaClJSCtIK0lLTGgpSUgrSCtJS0xoKClIKUlJS0lLSUpKailIKmgoK0grSCgoLGgoKCgsaCgpSCgoKUgpSUlIKCtIKCtIK0xoK0lIK0goK0goK0grTGlIK0grSCtMaUgrTG2JSmooKCgpSCgoKCgoKmgqaCgoKUlOaigpSClIKCtIKCxqSmooKClLS0pKaigoKCgqaigoKCpqKCgvyUAAkWsqiyAAIYAAsCAAIQ8gACGgAMAq7igpSClIKCyoKClAAKDNKClIKUgrSCtIK0pKzSgpSClAANEtKClIK4grSCtIKCgpTGgoKClMakrNKClIKUAAse0oKUgoKUgrSCxKQABhTSgpSClAAFEtKClIKUAAYU0oKUgpS0xoLKlMbEgoKUgrS0tLS0goK0goLqtIK0tLSCgoK0pKrCgpSCgoKClIKC7gAeTNKClIKCAAoWADVs7u7uAAkS0oKClKSCgoKUtIK0goKCgoKClJTGgrSCtIK0gpSCgrSCgqSCgoKCxoKCgoKUlLSCgoL+goKClKrCgpSmgoKCgu6CgoKCpqaCAAsYgg==", "287-304:1;418-433:1", "", "283=mustGet/1/2/2,mustGet/2/2/3;922=append/1/2/2,append/2/2/3;1078=mustGet/1/1/1;1154=Uint64Value/1/2/3,Uint64Value/2/2/7;1212=mustGet/1/1/2;1230=mustGet/1/2/2,mustGet/2/2/3;1362=mul/1/1/1;1411=Experiments/1/1/2;1602=Format/1/1/3", "", "409:54,411:13")]
[assembly: GoPositionMap("internal/trace/gc.go", "gc.cs", "AD1uAAwEAAAegoKCgoKCpoaGmpKCgqaUtIKClIKClIK2lJSCgoKUlIK2uKa0uNyCpKa4AAwUgriSgoKClIKClNq0goKCgoKCgoLIgoKCgoKCgoLYgoKUgoKClIKUprSCAAcQgoLKlIKUgoKUgoK4uKaCgoKClPyC7oKCpLaCgpSUlIKUpgAHEIKokgAfRqKCgpTulIKCgoKCggAHEIKmlIKCgriCgoKCgoKCgoKUpqaygoIADh6CpoKmgqaCpoKCggALGIKmgoKUpoKmgqaCgoIAGDzYAAIQ8oKUlKa43IKClKaCyoKClKaUpqaClJSCgsq4uK7CgoIAAhDSgoKCAAIaAAoCggAOHoKCggACEuKCgoKmuIKCpoKCggANHJSUpqKCgqaCgqKCgqaCgpTKyoKCgrjugoKClIKCgpSCiviCgoK4gpSCgpS0tKTKgqamqsIAECKCgoKClIKUgoKUgu7KgpSUgpS4AA0isp7ilIK4koKCgpSmlIKCgpSqooKCpuaCpoKmgg==", "81-83:1;84-86:2;87-89:3")]
[assembly: GoPositionMap("internal/trace/generation.go", "generation.cs", "AClMooKUAAcQgoKCgpSUgpSUlIKUlJSClIKUgsqClIIACRSCpoCCyIIAECgACQKClO6UgoKClJaSgqKClIK4gpSUlJSUlJSClAAHEJSClILKgpSCAAkUgqaAgsiC6LKUgILWgILWgoKUtICC1oKUgILWpIKUpKrKgpKCgoKClIKCgpSCgoKmlKzSgpSCgoKmgpSCgpSCuIKCuIKClIK4goKUgriCgoKmrNKClIKCgqaUgoKUgriCgriCgpSCuIKUgoKUgoKUgoKUgoKUlIIACRSCpqyygpSCgoKmlIKClIK4goK4goKUpoKClKaCgpSCgriCgrgACRQACxiCgpSCgoKCuIKUgoKUgpSCpoKCprSClJSCgpSCgoKClIKCgpSCgsq0pqqigpS4ppSCgqY=", "271-290:1;600-602:1")]
[assembly: GoPositionMap("internal/trace/mud.go", "mud.cs", "ADR4woKmgriClIK4goKCgpSClIKCgpSClIKCgoLcgoKUlKYABBTSuIKCgpSCgpSUggACENKClAACFPKCuIKYgoKUgoKSgoKCgoKCgoKClIKmuIKCgqbKlJSClJSClA==", "170-172:1")]
[assembly: GoPositionMap("internal/trace/order.go", "order.cs", "ACNeAA8ClKaSgpSCgoKClpKCgriUgoKmgoKUgpSClKaC2gBIkgGigoKClIKCuIKCuIKCgpSUgoKUpqaCggAHEJSCgoKCgoKmgqaCpqKCyoLcyoKClIKCgoKCpgALFIKClIKUgoKUgoKCgqaigoKCuKaCggAHEIKmgriCgriUgpSClIKCuIKCuIIACBKCgqaigoKUgpSCgoKUgpSClJSUgqa0gsqCypSC7oKmlLiCuMqkgqbGgoKmgqaCgpSCgpSCgqbYgpSCgpSCuIKmgsaCxoKkgqaigoKCuKaCgpSCgoKCgqa0goKCuJSCpoKmAAsUgpSCgpSClIKCgriUyqa4xqa2goKCpoKCgqbGgpSCgpSCpoKCgpQACxiCgpSCgqbYgpSCgpSClKaCgpSClIKCpgAMFoKCgpSCyoKUgoKUgpSCgoKCptiCpoKClJSCgoKmABIigqaCgpSCpoKCpoKCgpSCtoKmgoKUgqYAChCCgriCuILKgoKClIKClIKmsoK4goKmgpSClIKmsoCCpIKCgoKUgoKUgIKkgqaygIKkgoKCgpSCgpSAgqSCAAIUAAkCgoKClIKCgpSUlIKUgoKUgqaigoKCgoKUlJSClIKCgpSCpqKClJSClIKUgoKClIKmtIKUgqa0gpSCprSAgqSAkqSCpqLKgoKUgpSCpqKClIKClIKmtICCpIKClIKClICCpIKmoriCgpSClIKmooKUgoKUgoKUppSCprSClIKokgAJGLSCgoK4goKCuIKCgpQAFR6SlKSklAAVLpKClAANIpKCqJKUlIKUggAXOLKClIKqooKClIKClKiSrLKCgoKCgqaCpoKCggAIFJKokqiSpoKCgpSCgpSCgpSCggAQJpKClIKokoKCuIKCgoKmgoKqooKUgoKCggACEPIABxCCABMs")]
[assembly: GoPositionMap("internal/trace/reader.go", "reader.cs", "ADJskoKSgpSUgoKUyAAMGL4ACgSCuIKUgoKCpoKUgoKCgpQAEyiCgpSClIKUuIK4goKUgpSCuIKCgpSUgpSCuIKClJSmpKaCgoKCyoK0gIKC3IKCgoKCgoKmgvqCgpQACgqigpSCgpSCgpTcgoKokoKCgoKClKrClIKCgpSmprKCgoKClJSUlKa2goKCgoKClJQ=", "134-145:1;172-192:2", "", "65=convertV1Trace/1/1/1", "", "141:59,142:13")]
[assembly: GoPositionMap("internal/trace/resources.go", "resources.cs", "AC5kogAMDLKUpKSkpKSUAA0kouyylKSkpJQAFyaylKSkpJQACRaygoKUtLSkgqyygpSssoKUrLKClKyygpQAGj6SAAYQkgAGHPKClAACFPKClA==")]
[assembly: GoPositionMap("internal/trace/summary.go", "summary.cs", "ADqKAaKqwoKClAAkVILugriokoSCuIKUqLKCgoKClIKCgoKUpqKCgoKUgoKUrNKUgriCuIKUgpSCpIKUgpSCpKyygpSU3IKCgoKmACFOkgANHpKClJSmyIKmgoKUuIKUyoKCgu6ClLS0/IKUtrSmgoKCAAsYkoKClMaixoKi2IKi2IKCoqS0gqaCAAgQgtyCyqaCgra0goKCprSkkpLsgoKCgoKCAAgOgoLu+pKC1oKUgoKklNaCgpTEgoKmlLaClIKClKLIkoLclIKCtJKCgpSCgoK4lIKClIKC2oKCgqKClMqCgoLGpoKiyIKCgoKUqqKCpoKCgoKClJSClJSUAA4S2oqigpSClIKClIKCgpTugoKUgoKUgoKmlOa4", "618-631:1", "", "112=UnknownTime/1/1/5;591=Time/1/1/4,snapshotStat/1/1/4;738=Goroutine/1/1/1")]
[assembly: GoPositionMap("internal/trace/tracev1.go", "tracev1.cs", "AJMBugGigoKCgoKCgoKClJaSgoKCpriClIKSgqaCgoKUgqaClIKSgoKUgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSUpqaCyoLugoKCqsKCgpSCuIKUgqaCgqaUgoKCyoKUgqYAARIACgKCgpSUgqaCuAAOHoLGgoKClIKCxoKClIKCxrS0koKCtIK0tLSCgpS0goKClMYADRoADhy0grSCtIK0grSCtLSCtIK0grSCtIK0grSC2IKCgsqClJSCgsoADRwACRSCyLSkgoK0uIK0tLS0tIKCgpSCgrSmgoKC/taUtKS0grTctKaCgriCgoKmgpSCgpQADBqqooKC", "123-127:1;141-145:2;408-419:1", "", "629=addExtraString/1/1/4")]
[assembly: GoPositionMap("internal/trace/value.go", "value.cs", "ABhE4qyygpTqooKUlJSokqiS")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.trace_package;
using iter = go.iter_package;
using time = go.time_package;
using tracev1 = go.@internal.trace.@internal.tracev1_package;
using tracev2 = go.@internal.trace.tracev2_package;
using version = go.@internal.trace.version_package;

[GoPackage("trace")]
public static partial class trace_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface readBatch_r {}
    [GoLocalName("perP")] internal partial struct MutatorUtilizationV2_perP {}
    [GoLocalName("procsCount")] internal partial struct MutatorUtilizationV2_procsCount {}
    [GoLocalName("unblockEdge")] internal partial struct RelatedGoroutinesV2_unblockEdge {}
    internal partial struct accumulator {}
    internal partial struct bandUtil {}
    internal partial struct bandUtilHeap {}
    [GoValueClone("args")] internal partial struct baseEvent {}
    internal partial struct batch {}
    [GoValueClone("ev")] internal partial struct batchCursor {}
    internal partial struct cpuSample {}
    internal partial struct dataTable<EI, E> {}
    internal partial struct edge {}
    internal partial struct evTable {}
    internal partial struct extraStringID {}
    internal partial struct frame {}
    internal partial struct frequency {}
    internal partial struct gState {}
    internal partial struct gcState {}
    internal partial struct generation {}
    internal partial struct goroutineSummary {}
    internal partial struct integrator {}
    internal partial struct mState {}
    internal partial struct mmuBand {}
    internal partial struct mmuSeries {}
    [GoValueClone("hist")] internal partial struct mud {}
    internal partial struct ordering {}
    internal partial struct pState {}
    internal partial struct queue<T> {}
    internal partial struct rangeP {}
    internal partial struct rangeState {}
    internal partial struct rangeType {}
    internal partial struct schedCtx {}
    internal partial struct schedReqs {}
    internal partial struct seqCounter {}
    internal partial struct spilledBatch {}
    internal partial struct stack {}
    internal partial struct stackID {}
    internal partial struct stringID {}
    internal partial struct sync {}
    internal partial struct taskState {}
    internal partial struct timedEventArgs {}
    internal partial struct timestamp {}
    internal partial struct totalUtil {}
    [GoValueClone("extraArr")] internal partial struct traceV1Converter {}
    internal partial struct userRegion {}
    internal partial struct utilHeap {}
    public partial interface EventDetails<ΔT> {}
    public partial struct ClockSnapshot {}
    [GoValueClone("@base")] public partial struct Event {}
    public partial struct EventConfig<T> {}
    public partial struct EventKind {}
    public partial struct ExperimentalBatch {}
    public partial struct ExperimentalEvent {}
    public partial struct GoID {}
    public partial struct GoState {}
    public partial struct GoroutineExecStats {}
    public partial struct GoroutineSummary {}
    public partial struct Label {}
    public partial struct Log {}
    public partial struct MMUCurve {}
    public partial struct Metric {}
    public partial struct MutatorUtil {}
    public partial struct ProcID {}
    public partial struct ProcState {}
    public partial struct Range {}
    public partial struct RangeAttribute {}
    public partial struct Reader {}
    public partial struct Region {}
    public partial struct ResourceID {}
    public partial struct ResourceKind {}
    public partial struct Stack {}
    public partial struct StackFrame {}
    public partial struct StackSample {}
    public partial struct StateTransition {}
    public partial struct Summarizer {}
    public partial struct Summary {}
    public partial struct Sync {}
    public partial struct Task {}
    public partial struct TaskID {}
    public partial struct ThreadID {}
    public partial struct Time {}
    public partial struct UserRegionSummary {}
    public partial struct UserTaskSummary {}
    public partial struct UtilFlags {}
    public partial struct UtilWindow {}
    public partial struct Value {}
    public partial struct ValueKind {}
    public partial struct constraint {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontainerꓸheap() => builtin.initPackage(typeof(container.heap_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸinternalꓸtracev1() => builtin.initPackage(typeof(go.@internal.trace.@internal.tracev1_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtracev2() => builtin.initPackage(typeof(go.@internal.trace.tracev2_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
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
[GoMethodHost(typeof(trace_package.Event)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛEvent;
[GoMethodHost(typeof(trace_package.EventKind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛEventKind;
[GoMethodHost(typeof(trace_package.ExperimentalEvent)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛExperimentalEvent;
[GoMethodHost(typeof(trace_package.GoState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛGoState;
[GoMethodHost(typeof(trace_package.GoroutineExecStats)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛGoroutineExecStats;
[GoMethodHost(typeof(trace_package.GoroutineSummary)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛGoroutineSummary;
[GoMethodHost(typeof(trace_package.MMUCurve)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛMMUCurve;
[GoMethodHost(typeof(trace_package.ProcState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛProcState;
[GoMethodHost(typeof(trace_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛReader;
[GoMethodHost(typeof(trace_package.ResourceID)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛResourceID;
[GoMethodHost(typeof(trace_package.ResourceKind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛResourceKind;
[GoMethodHost(typeof(trace_package.Stack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛStack;
[GoMethodHost(typeof(trace_package.StateTransition)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛStateTransition;
[GoMethodHost(typeof(trace_package.Summarizer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛSummarizer;
[GoMethodHost(typeof(trace_package.Time)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛTime;
[GoMethodHost(typeof(trace_package.UserTaskSummary)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛUserTaskSummary;
[GoMethodHost(typeof(trace_package.Value)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛValue;
[GoMethodHost(typeof(trace_package.accumulator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛaccumulator;
[GoMethodHost(typeof(trace_package.bandUtilHeap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛbandUtilHeap;
[GoMethodHost(typeof(trace_package.baseEvent)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛbaseEvent;
[GoMethodHost(typeof(trace_package.batch)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛbatch;
[GoMethodHost(typeof(trace_package.batchCursor)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛbatchCursor;
[GoMethodHost(typeof(trace_package.cpuSample)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛcpuSample;
[GoMethodHost(typeof(trace_package.dataTable<,>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛdataTable;
[GoMethodHost(typeof(trace_package.evTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛevTable;
[GoMethodHost(typeof(trace_package.frequency)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛfrequency;
[GoMethodHost(typeof(trace_package.gState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛgState;
[GoMethodHost(typeof(trace_package.gcState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛgcState;
[GoMethodHost(typeof(trace_package.integrator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛintegrator;
[GoMethodHost(typeof(trace_package.mmuSeries)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛmmuSeries;
[GoMethodHost(typeof(trace_package.mud)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛmud;
[GoMethodHost(typeof(trace_package.ordering)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛordering;
[GoMethodHost(typeof(trace_package.queue<>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛqueue;
[GoMethodHost(typeof(trace_package.rangeState)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛrangeState;
[GoMethodHost(typeof(trace_package.seqCounter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛseqCounter;
[GoMethodHost(typeof(trace_package.stack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛstack;
[GoMethodHost(typeof(trace_package.totalUtil)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛtotalUtil;
[GoMethodHost(typeof(trace_package.traceV1Converter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛtraceV1Converter;
[GoMethodHost(typeof(trace_package.utilHeap)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceᴛutilHeap;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class traceᴛEvent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static EventKind Kind(this Event e) => trace_package.Kind⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Time Time(this Event e) => trace_package.Time⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static GoID Goroutine(this Event e) => trace_package.Goroutine⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ProcID Proc(this Event e) => trace_package.Proc⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ThreadID Thread(this Event e) => trace_package.Thread⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Stack Stack(this Event e) => trace_package.Stack⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Metric Metric(this Event e) => trace_package.Metric⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Label Label(this Event e) => trace_package.Label⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Range Range(this Event e) => trace_package.Range⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<RangeAttribute> RangeAttributes(this Event e) => trace_package.RangeAttributes⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Task Task(this Event e) => trace_package.Task⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Region Region(this Event e) => trace_package.Region⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Log Log(this Event e) => trace_package.Log⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static StateTransition StateTransition(this Event e) => trace_package.StateTransition⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Sync Sync(this Event e) => trace_package.Sync⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ExperimentalEvent Experimental(this Event e) => trace_package.Experimental⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Event e) => trace_package.String⁔(e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error validateTableIDs(this Event e) => trace_package.validateTableIDs⁔(e);
}
partial class traceᴛEventKind {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this EventKind e) => trace_package.String⁔(e);
}
partial class traceᴛExperimentalEvent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Value ArgValue(this ExperimentalEvent e, nint i) => trace_package.ArgValue⁔(e, i);
}
partial class traceᴛGoState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Executing(this GoState s) => trace_package.Executing⁔(s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this GoState s) => trace_package.String⁔(s);
}
partial class traceᴛGoroutineExecStats {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static map<@string, time.Duration> NonOverlappingStats(this GoroutineExecStats s) => trace_package.NonOverlappingStats⁔(s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration UnknownTime(this GoroutineExecStats s) => trace_package.UnknownTime⁔(s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static GoroutineExecStats /*r*/ sub(this GoroutineExecStats s, GoroutineExecStats v) => trace_package.sub⁔(s, v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static GoroutineExecStats /*r*/ clone(this GoroutineExecStats s) => trace_package.clone⁔(s);
}
partial class traceᴛGoroutineSummary {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static GoroutineExecStats /*ret*/ snapshotStat(this ref GoroutineSummary g, Time lastTs) => trace_package.snapshotStat⁔(ref g, lastTs);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void finalize(this ref GoroutineSummary g, Time lastTs, ж<Event> Ꮡtrigger) => trace_package.finalize⁔(ref g, lastTs, Ꮡtrigger);
}
partial class traceᴛMMUCurve {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static float64 /*mmu*/ MMU(this ref MMUCurve c, time.Duration window) => trace_package.MMU⁔(ref c, window);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<UtilWindow> /*worst*/ Examples(this ref MMUCurve c, time.Duration window, nint n) => trace_package.Examples⁔(ref c, window, n);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<float64> MUD(this ref MMUCurve c, time.Duration window, slice<float64> quantiles) => trace_package.MUD⁔(ref c, window, quantiles);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mmu(this ref MMUCurve c, time.Duration window, ж<accumulator> Ꮡacc) => trace_package.mmu⁔(ref c, window, Ꮡacc);
}
partial class traceᴛProcState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Executing(this ProcState s) => trace_package.Executing⁔(s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ProcState s) => trace_package.String⁔(s);
}
partial class traceᴛReader {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Event e, error err) ReadEvent(this ж<Reader> Ꮡr) => trace_package.ReadEvent⁔(Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Event, error) nextGenWithSpill(this ref Reader r) => trace_package.nextGenWithSpill⁔(ref r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Event, error) installGen(this ref Reader r, ж<generation> Ꮡgen) => trace_package.installGen⁔(ref r, Ꮡgen);
}
partial class traceᴛResourceID {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static GoID Goroutine(this ResourceID r) => trace_package.Goroutine⁔(r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ProcID Proc(this ResourceID r) => trace_package.Proc⁔(r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ThreadID Thread(this ResourceID r) => trace_package.Thread⁔(r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ResourceID r) => trace_package.String⁔(r);
}
partial class traceᴛResourceKind {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ResourceKind r) => trace_package.String⁔(r);
}
partial class traceᴛStack {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static iter.Seq<StackFrame> Frames(this Stack s) => trace_package.Frames⁔(s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Stack s) => trace_package.String⁔(s);
}
partial class traceᴛStateTransition {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (GoState from, GoState to) Goroutine(this StateTransition d) => trace_package.Goroutine⁔(d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ProcState from, ProcState to) Proc(this StateTransition d) => trace_package.Proc⁔(d);
}
partial class traceᴛSummarizer {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Event(this ref Summarizer s, ж<Event> Ꮡev) => trace_package.Event⁔(ref s, Ꮡev);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<UserTaskSummary> getOrAddTask(this ref Summarizer s, TaskID id) => trace_package.getOrAddTask⁔(ref s, id);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Summary> ΔFinalize(this ref Summarizer s) => trace_package.ΔFinalize⁔(ref s);
}
partial class traceᴛTime {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration Sub(this Time t, Time t0) => trace_package.Sub⁔(t, t0);
}
partial class traceᴛUserTaskSummary {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Complete(this ref UserTaskSummary s) => trace_package.Complete⁔(ref s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<ж<UserTaskSummary>> Descendents(this ж<UserTaskSummary> Ꮡs) => trace_package.Descendents⁔(Ꮡs);
}
partial class traceᴛValue {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ValueKind Kind(this Value v) => trace_package.Kind⁔(v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Uint64(this Value v) => trace_package.Uint64⁔(v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Value v) => trace_package.String⁔(v);
}
partial class traceᴛaccumulator {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resetTime(this ref accumulator acc) => trace_package.resetTime⁔(ref acc);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool addMU(this ж<accumulator> Ꮡacc, int64 timeΔ1, float64 mu, time.Duration window) => trace_package.addMU⁔(Ꮡacc, timeΔ1, mu, window);
}
partial class traceᴛbandUtilHeap {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this bandUtilHeap h) => trace_package.Len⁔(h);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this bandUtilHeap h, nint i, nint j) => trace_package.Less⁔(h, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this bandUtilHeap h, nint i, nint j) => trace_package.Swap⁔(h, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref bandUtilHeap h, any x) => trace_package.Push⁔(ref h, x);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Pop(this ref bandUtilHeap h) => trace_package.Pop⁔(ref h);
}
partial class traceᴛbaseEvent {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<uint64> extra(this ref baseEvent e, version.Version v) => trace_package.extra⁔(ref e, v);
}
partial class traceᴛbatch {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isStringsBatch(this ref batch b) => trace_package.isStringsBatch⁔(ref b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isStacksBatch(this ref batch b) => trace_package.isStacksBatch⁔(ref b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isCPUSamplesBatch(this ref batch b) => trace_package.isCPUSamplesBatch⁔(ref b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isSyncBatch(this ref batch b, version.Version ver) => trace_package.isSyncBatch⁔(ref b, ver);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isEndOfGeneration(this ref batch b) => trace_package.isEndOfGeneration⁔(ref b);
}
partial class traceᴛbatchCursor {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool ok, error err) nextEvent(this ж<batchCursor> Ꮡb, slice<batch> batches, frequency freq) => trace_package.nextEvent⁔(Ꮡb, batches, freq);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint compare(this ref batchCursor b, ж<batchCursor> Ꮡa) => trace_package.compare⁔(ref b, Ꮡa);
}
partial class traceᴛcpuSample {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Event asEvent(this cpuSample s, ж<evTable> Ꮡtable) => trace_package.asEvent⁔(s, Ꮡtable);
}
partial class traceᴛdataTable {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error insert<EI, E>(this ref dataTable<EI, E> d, EI id, E data) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.insert⁔<EI, E>(ref d, id, data);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static EI append<EI, E>(this ref dataTable<EI, E> d, E data) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.append⁔<EI, E>(ref d, data);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void compactify<EI, E>(this ref dataTable<EI, E> d) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.compactify⁔<EI, E>(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (E, bool) get<EI, E>(this ref dataTable<EI, E> d, EI id) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.get⁔<EI, E>(ref d, id);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool forEach<EI, E>(this ref dataTable<EI, E> d, Func<EI, E, bool> yield) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.forEach⁔<EI, E>(ref d, yield);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static E mustGet<EI, E>(this ref dataTable<EI, E> d, EI id) where EI : /* ~uint64 */ IAdditionOperators<EI, EI, EI>, ISubtractionOperators<EI, EI, EI>, IMultiplyOperators<EI, EI, EI>, IDivisionOperators<EI, EI, EI>, IIncrementOperators<EI>, IDecrementOperators<EI>, IUnaryNegationOperators<EI, EI>, IModulusOperators<EI, EI, EI>, IBitwiseOperators<EI, EI, EI>, IShiftOperators<EI, int, EI>, IEqualityOperators<EI, EI, bool>, IComparisonOperators<EI, EI, bool>, new() => trace_package.mustGet⁔<EI, E>(ref d, id);
}
partial class traceᴛevTable {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static extraStringID addExtraString(this ref evTable t, @string s) => trace_package.addExtraString⁔(ref t, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string getExtraString(this ref evTable t, extraStringID id) => trace_package.getExtraString⁔(ref t, id);
}
partial class traceᴛfrequency {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Time mul(this frequency f, timestamp t) => trace_package.mul⁔(f, t);
}
partial class traceᴛgState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error beginRegion(this ref gState s, userRegion r) => trace_package.beginRegion⁔(ref s, r);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error endRegion(this ref gState s, userRegion r) => trace_package.endRegion⁔(ref s, r);
}
partial class traceᴛgcState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this gcState s) => trace_package.String⁔(s);
}
partial class traceᴛintegrator {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static totalUtil advance(this ref integrator @in, int64 time) => trace_package.advance⁔(ref @in, time);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 next(this ref integrator @in, int64 time) => trace_package.next⁔(ref @in, time);
}
partial class traceᴛmmuSeries {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64 start, int64 end) bandTime(this ref mmuSeries s, nint i) => trace_package.bandTime⁔(ref s, i);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<bandUtil> mkBandUtil(this ref mmuSeries c, nint series, time.Duration window) => trace_package.mkBandUtil⁔(ref c, series, window);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void bandMMU(this ref mmuSeries c, nint bandIdx, time.Duration window, ж<accumulator> Ꮡacc) => trace_package.bandMMU⁔(ref c, bandIdx, window, Ꮡacc);
}
partial class traceᴛmud {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ж<mud> Ꮡd, float64 l, float64 r, float64 area) => trace_package.add⁔(Ꮡd, l, r, area);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setTrackMass(this ref mud d, float64 mass) => trace_package.setTrackMass⁔(ref d, mass);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (float64, float64, bool) approxInvCumulativeSum(this ref mud d) => trace_package.approxInvCumulativeSum⁔(ref d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (float64, bool) invCumulativeSum(this ref mud d, float64 y) => trace_package.invCumulativeSum⁔(ref d, y);
}
partial class traceᴛordering {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, error) Advance(this ж<ordering> Ꮡo, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen) => trace_package.Advance⁔(Ꮡo, Ꮡev, Ꮡevt, m, gen);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string evName(this ref ordering o, tracev2.EventType typ) => trace_package.evName⁔(ref o, typ);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceProcStatus(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceProcStatus⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceProcStart(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceProcStart⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceProcStop(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceProcStop⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceProcSteal(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceProcSteal⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoStatus(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoStatus⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoCreate(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoCreate⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoStopExec(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoStopExec⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoStart(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoStart⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoUnblock(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoUnblock⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoSwitch(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoSwitch⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoSyscallBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoSyscallBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoSyscallEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoSyscallEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoSyscallEndBlocked(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoSyscallEndBlocked⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoCreateSyscall(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoCreateSyscall⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoDestroySyscall(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoDestroySyscall⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceUserTaskBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceUserTaskBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceUserTaskEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceUserTaskEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceUserRegionBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceUserRegionBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceUserRegionEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceUserRegionEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCActive⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceAnnotation(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceAnnotation⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceHeapMetric(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceHeapMetric⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCSweepBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCSweepBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCSweepActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCSweepActive⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGCSweepEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGCSweepEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoRangeBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoRangeBegin⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoRangeActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoRangeActive⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceGoRangeEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceGoRangeEnd⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (schedCtx, bool, error) advanceAllocFree(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) => trace_package.advanceAllocFree⁔(ref o, Ꮡev, Ꮡevt, m, gen, curCtx);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Event, bool) Next(this ref ordering o) => trace_package.Next⁔(ref o);
}
partial class traceᴛqueue {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void push<T>(this ref queue<T> q, T value) => trace_package.push⁔<T>(ref q, value);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void grow<T>(this ref queue<T> q) => trace_package.grow⁔<T>(ref q);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (T, bool) pop<T>(this ref queue<T> q) => trace_package.pop⁔<T>(ref q);
}
partial class traceᴛrangeState {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error beginRange(this ref rangeState s, rangeType typ) => trace_package.beginRange⁔(ref s, typ);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error activeRange(this ref rangeState s, rangeType typ, bool isInitialGen) => trace_package.activeRange⁔(ref s, typ, isInitialGen);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasRange(this ref rangeState s, rangeType typ) => trace_package.hasRange⁔(ref s, typ);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (stringID, error) endRange(this ref rangeState s, tracev2.EventType typ) => trace_package.endRange⁔(ref s, typ);
}
partial class traceᴛseqCounter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool succeeds(this seqCounter a, seqCounter b) => trace_package.succeeds⁔(a, b);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this seqCounter c) => trace_package.String⁔(c);
}
partial class traceᴛstack {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this stack s) => trace_package.String⁔(s);
}
partial class traceᴛtotalUtil {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static float64 mean(this totalUtil u, time.Duration dur) => trace_package.mean⁔(u, dur);
}
partial class traceᴛtraceV1Converter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error init(this ж<traceV1Converter> Ꮡit, tracev1.Trace pr) => trace_package.init⁔(Ꮡit, pr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Event, error) next(this ж<traceV1Converter> Ꮡit) => trace_package.next⁔(Ꮡit);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Event OUT, error ERR) convertEvent(this ж<traceV1Converter> Ꮡit, ж<tracev1.Event> Ꮡev) => trace_package.convertEvent⁔(Ꮡit, Ꮡev);
}
partial class traceᴛutilHeap {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this utilHeap h) => trace_package.Len⁔(h);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this utilHeap h, nint i, nint j) => trace_package.Less⁔(h, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this utilHeap h, nint i, nint j) => trace_package.Swap⁔(h, i, j);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Push(this ref utilHeap h, any x) => trace_package.Push⁔(ref h, x);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Pop(this ref utilHeap h) => trace_package.Pop⁔(ref h);
}
// </MethodForwarders>
