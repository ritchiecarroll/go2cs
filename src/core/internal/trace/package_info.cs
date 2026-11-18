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
using tracev1 = go.@internal.trace.@internal.tracev1_package;
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
[assembly: GoPositionMap("internal/trace/base.go", "base.cs", "ACFCopSUABcwooKUgpSClIKCgoKqooKUAA8e8oKUgpSCqLKClIKCrNKUpoKCgoKUgqaUpoKUgpSCgoKCgpSqwoKUgoKUgoKmqLKCgpSCpoKUgoKmrNKCgpQACQ6SABYw9gAHEIIACxCCgoKU")]
[assembly: GoPositionMap("internal/trace/batch.go", "batch.cs", "AB86gqaCpoKmgqqCAAoIqoKClICCpICCyIKCgoKUyoKClIKClIKCuIKClIKokoKCgpSCuA==", "", "", "124=Bytes/1/1/3")]
[assembly: GoPositionMap("internal/trace/batchcursor.go", "batchcursor.cs", "ABku1oKCgqaCpoKmgoKmpqaCpqIABBQACASCgoKUpoKClKaCgpSmgoKClIKUppSmgqaUgqamlIKCpoKUgqaCgoKUpoKCgpSUgpSmgoKCgpSCgoKmgoKCpg==")]
[assembly: GoPositionMap("internal/trace/event.go", "event.cs", "AF28AZKClMYAKlSSAHL4AaSCgoIADRqSkoKUgoKC3IIABRKygoLGgoKC3AAkSpKClIKClAAlUgAIBMqkgpSCuKaClIKUpILGgILWgILWgILWgsaAgtaAgtaAgtaCtuaigpSktLS0lIKmooKUgpSCppbWpIKUlLS0tMaClJSCtIK0lLTGgpSUgrSCtJS0xoKClIKUlJS0lLSUpKailIKmgoK0grSCgoLGgoKCgsaCgpSCgoKUgpSUlIKCtIKCtIK0xoK0lIK0goK0goK0grTGlIK0grSCtMaUgrTG2JSmooKCgpSCgoKCgoKmgqaCgoKUlOaigpSClIKCtIKCxqSmooKClLS0pKaigoKCgqaigoKCpqKCgtyUAAsWsqiyAAIYAAsCAAIQ8gACGgAMAq7igpSClIKCyoKClAAKDNKClIKUgrSCtIK0pKzSgpSClAANEtKClIK4grSCtIKCgpTGgoKClMakrNKClIKUAAse0oKUgoKUgrSCtKQABhTSgpSClAAFEtKClIKUAAYU0oKUgpS0xoLKlMa0goKUgrS0tLS0goK0goLqtIK0tLSCgoK0pKrCgpSCgoKClIKC7gAiTNKClIKCAAwWADVs7u7uAAsS0oKClKSCgoKUtIK0goKCgoKClJTGgrSCtIK0gpSCgrSCgqSCgoKCxoKCgoKUlLSCgoLugoKClKrCgpSmgoKCgu6CgoKCpsaCAAsYgg==", "287-304:1;418-433:1", "", "294=mustGet/1/2/2,mustGet/2/2/3;936=append/1/2/2,append/2/2/3;1092=mustGet/1/1/1;1168=Uint64Value/1/2/3,Uint64Value/2/2/7;1225=mustGet/1/1/2;1243=mustGet/1/2/2,mustGet/2/2/3;1374=mul/1/1/1;1427=Experiments/1/1/2;1622=Format/1/1/3", "", "426:21,428:13")]
[assembly: GoPositionMap("internal/trace/gc.go", "gc.cs", "AD5uAAwEAAAegoKCgoKCpoaGmpKCgqaUtIKClIKClIKmlJSCgoKUlIK2uKa0uNyCpKa4AAwUgriSgoKClIKClNq0goKCgoKCgoLIgoKCgoKCgoLYgoKUgoKClIKUprSCAAcQgoLKlIKUgoKUgoK4uKaCgoKClNyC7oKClLaCgpSUlIKUpgAHEILIkgAhRqKCgpTulIKCgoKCggAHEIKmlIKCgriCgoKCgoKCgoKUpsaygoIAEh6CpoKmgqaCpoKCggAPGIKmgoKUpoKmgqaCgoIAHDzYAAIQ8oKUlKa43IKClKaCyoKClKaUpqaClJSCgsq4uM7CgoIAAhDSgoKCAAIaAAoCggAOHoKCggACEuKCgoKmuIKCpoKCggANHJSUpqKCgqaCgqKCgqaCgpTKyoKCgtjugoKClIKCgpSCiviCgoK4gpSCgpS0tKTKgqamqsIAECKCgoKClIKUgoKUgu7KgpSUgpS4ABEisp7ilIK4koKCgpSmlIKCgpSqooKCpgAIBoKmgqaC", "81-83:1;84-86:2;87-89:3")]
[assembly: GoPositionMap("internal/trace/generation.go", "generation.cs", "AClMooKUAAcQgoKCgpSUgpSUlIKUlJSClIKUgsqClIIACRSCpoCCyIIAECgACQKClO6UgoKClJaSgqKClIK4gpSUlJSUlJSClAAHEJSClILKgpSCAAkUgqaAgsiC6LKUgILWgILWgoKUtICC1oKUgILWpIKUpKrKgpKCgoKClIKCgpSCgoKmlKzSgpSCgoKmgpSCgpSCuIKCuIKClIK4goKUgriCgoKmrNKClIKCgqaUgoKUgriCgriCgpSCuIKUgoKUgoKUgoKUgoKUlIIACRSCpqyygpSCgoKmlIKClIK4goK4goKUpoKClKaCgpSCgriCgrgACRQACxiCgpSCgoKCuIKUgoKUgpSCpoKCprSClJSCgpSCgoKClIKCgpSCgsq0pqqigpS4ppSCgqY=", "271-290:1;600-602:1")]
[assembly: GoPositionMap("internal/trace/mud.go", "mud.cs", "ADd4woKmgriClIK4goKCgpSClIKCgpSClIKCgoLcgoKUlKYABBTSuIKCgpSCgpSUggACENKClAACFPKCuIKYgoKUgoKSgoKCgoKCgoKClIKmuIKCgqbKlJSClJSClA==", "170-172:1")]
[assembly: GoPositionMap("internal/trace/order.go", "order.cs", "ACZeAA8ClKaSgpSCgoKClpKCgriUgoKmgoKUgpSClKaC+gBKkgGigoKClIKCuIKCuIKCgpSUgoKUpqaCggAHEJSCgoKCgoKmgqaCpqKCyoLcyoKClIKCgoKCpgALFIKClIKUgoKUgoKCgqaigoKCuKaCggAHEIKmgriCgriUgpSClIKCuIKCuIIACBKCgqaigoKUgpSCgoKUgpSClJSUgqa0gsqCypSC7oKmlLiCuMqkgqbGgoKmgqaCgpSCgpSCgqbYgpSCgpSCuIKmgsaCxoKkgqaigoKCuKaCgpSCgoKCgqa0goKCuJSCpoKmAAsUgpSCgpSClIKCgriUyqa4xqa2goKCpoKCgqbGgpSCgpSCpoKCgpQACxiCgpSCgqbYgpSCgpSClKaCgpSClIKCpgAMFoKCgpSCyoKUgoKUgpSCgoKCptiCpoKClJSCgoKmABIigqaCgpSCpoKCpoKCgpSCpoKmgoKUgqYAChCCgriCuILKgoKClIKClIKmsoK4goKmgpSClIKmsoCCpIKCgoKUgoKUgIKkgqaygIKkgoKCgpSCgpSAgqSCAAIUAAkCgoKClIKCgpSUlIKUgoKUgqaigoKCgoKUlJSClIKCgpSCpqKClJSClIKUgoKClIKmtIKUgqa0gpSCprSAgqSAgqSCpqLKgoKUgpSCpqKClIKClIKmtICCpIKClIKClICCpIKmoriCgpSClIKmooKUgoKUgoKUppSCprSClIKokgALGLSCgoK4goKCuIKCgpQAFx6SlKSklAAXLpKClAAPIpKCqJKUlIKUggAbOLKClIKqooKClIKClKiSrLKCgoKCgqaCpoKCggAKFJLIkqiSxoKCgpSCgpSCgpSCggASJpKClIKokoKCuIKCgoKmgoKqooKUgoKCggAEEPIABxCCABMs")]
[assembly: GoPositionMap("internal/trace/reader.go", "reader.cs", "ADNskoKSgpSUgoKUyAAMGN4ACgSCuIKUgoKCpoKUgoKCgpQAEyiCgpSClIKUuIK4goKUgpSCuIKCgpSUgpSCuIKClJSmlKaCgoKCyoKUgIKC3IKCgoKCgoKmgtqCgpQACgqigpSCgpSCgpTcgoKokoKCgoKClKrClIKCgpSmprKCgoKClJSUlKbGgoKCgoKClJQ=", "134-145:1;172-192:2", "", "66=convertV1Trace/1/1/1", "", "144:21,145:13")]
[assembly: GoPositionMap("internal/trace/resources.go", "resources.cs", "ADFkogASDLKUpKSkpKSUABEkogAODLKUpKSklAAdJrKUpKSklAALFrKCgpS0tKSCzLKClKyygpSssoKUrLKClAAcPpIABhCSAAgc8oKUAAIU8oKU")]
[assembly: GoPositionMap("internal/trace/summary.go", "summary.cs", "AD2KAaKqwoKClAAoVILugriokoSCuIKUqLKCgoKClIKCgoKUpqKCgoKUgoKUzNKUgriCuIKUgpSClIKUgpSClKyygpSU3IKCgoKmACNOkgAPHpKClJSmyIKmgoKUuIKUyoKCgu6ClLS0/IKUtrSmgoKCAAsYgoKClMaCxoKC2IKC2IKCgpSUgqaCAAgQgtyCyraCgra0goKCprSkgoLsgoKCgoKCAAgOgoLu6oKCtoKUgoKUlMaCgpS0goKmlLaClIKClILIgoLclIKCtIKCgpSCgoK4lIKClIKC2oKCgoKClMqCgoLGpoKCyIKCgoKUqqKCpoKCgoKClJSClJSUABAS2oqigpSClIKClIKCgpTugoKUgoKUgoKmlOa4", "618-631:1", "", "119=UnknownTime/1/1/5;579=Time/1/1/4,snapshotStat/1/1/4;723=Goroutine/1/1/1")]
[assembly: GoPositionMap("internal/trace/tracev1.go", "tracev1.cs", "AJYBugGigoKCgoKCgoKClJaSgoKCpriClIKSgqaCgoKUgqaClIKSgoKUgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSUpqaCyoLugoKCqsKCgpSCuIKUgqaCgqaUgoKCyoKUgsYAAxIACgKCgpSUgqaCuAAOHoLGgoKClIKCxoKClIKCxrS0goKCtIK0tLSCgpS0goKClMYADRoADhy0grSCtIK0grSCtLSCtIK0grSCtIK0grSC2IKCgsqClJSCgsoADRwACRSCyLSkgoK0uIK0tLS0tIKCgpSCgrSmgoKC7saUtKS0grTctKaCgriCgoKmgpSCgpQADBrKooKC", "123-127:1;141-145:2;408-419:1", "", "635=addExtraString/1/1/4")]
[assembly: GoPositionMap("internal/trace/value.go", "value.cs", "ABtE4qyygpTqooKUlJTIkqiS")]
// </GoSourcePositionMaps>

namespace go.@internal;

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
