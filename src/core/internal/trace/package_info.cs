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
global using oldtraceꓸSTWReason = go.@internal.trace.@internal.oldtrace_package.ΔSTWReason;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using bufio = go.bufio_package;
using oldtrace = go.@internal.trace.@internal.oldtrace_package;
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
[assembly: GoTypeAlias("Event", "ΔEvent")]
[assembly: GoTypeAlias("Label", "ΔLabel")]
[assembly: GoTypeAlias("Log", "ΔLog")]
[assembly: GoTypeAlias("Metric", "ΔMetric")]
[assembly: GoTypeAlias("Range", "ΔRange")]
[assembly: GoTypeAlias("Region", "ΔRegion")]
[assembly: GoTypeAlias("Stack", "ΔStack")]
[assembly: GoTypeAlias("StateTransition", "ΔStateTransition")]
[assembly: GoTypeAlias("Task", "ΔTask")]
[assembly: GoTypeAlias("Time", "ΔTime")]
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
[assembly: GoImplicitConv<dataTable<stringID, @string>, ж<dataTable<stringID, @string>>>(Indirect = true)]
[assembly: GoImplicitConv<evTable, ж<evTable>>(Indirect = true)]
[assembly: GoImplicitConv<spilledBatch, ж<spilledBatch>>(Indirect = true)]
[assembly: GoImplicitConv<ΔTime, oldtrace.Timestamp>(Inverted = false, ValueType = "int64")]
[assembly: GoImplicitConv<ΔTime, timestamp>(Inverted = true, ValueType = "int64")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/trace/base.go", "base.cs", "ACJMopSkABQyooKUgpSAgqSCgoKCqqKClAALHvKClICCpIKs0pSmgoKCgpSCppSmgpSClIKCgoKClKrCgpSCgqSCgIK2qLKCgpSCpoKUgoKmrNKCgpTOkgAPMPYABxCCAAcQgoKClA==")]
[assembly: go.GoPositionMap("internal/trace/batch.go", "batch.cs", "ABw6gqaCpoKmggAICKqCgpSAgriCgoKClLqCgpSCgpSCgqiCgpSCmJKCgoKUgqg=", "", "", "105=Bytes/1/1/3")]
[assembly: go.GoPositionMap("internal/trace/batchcursor.go", "batchcursor.cs", "ABgw1oKCgqaCpoKmgoKmlpaCpqIAAhQACASCgoKUloKClJaCgpSWgoKClIKUppSWgqaUgqamlIKCpoKUgqaCgoKUpoKCgpSUgpSmgoKCgpSCgIKCtoKAgoK2")]
[assembly: go.GoPositionMap("internal/trace/event.go", "event.cs", "ACW0AZKClKYAFVSSAG6CApKSgpSCgoLcggAHEAA2frKosgACGAALAgACEPIAAhoADAKu4oKUgpSCgsqCgpT80oKUgpSCpIKkgqSkrNKClIKUAAoS0oCCpIKYgsSCpIKCgpSUpIKCgpS2pKzSgpSClAALHtKAgqSCgpSCpIKkpAAGFNKAgqSClAAFEtKClIKUAAYU0oKUgpSkpoLalKa0goKUgqSkpIKkpIKCpIKCqtSCpIKkgqSCgqSkrNKClIKCAAkUADVs7gAKFNKCgpSApIKkgqSCgoKCgoKUlLaCpIKkgqSCgpSCgqSCgqSCgoKCgsiCtICCgoKCgraqwoKUloKCgoLegoKCgqamgg==", "270-287:1", "", "191=mustGet/1/2/2,mustGet/2/2/3;391=mustGet/1/1/1;522=mustGet/1/1/2;540=mustGet/1/2/2,mustGet/2/2/3")]
[assembly: go.GoPositionMap("internal/trace/gc.go", "gc.cs", "ADFuAAgEAAAegoKCgoKCloaGipKCgpaUtIKClIKClIKmlJSCgoKUlILGuJa0uJyCpOaYAAQUAAoCmJKigoKUgoKUyrSCopKCkoKCgILYgoKSgpKCgoCC6IKClIKCgpSSlKa0ggAHEIKCypSClIKClIKCuKimgoKCgpTMgt6CgpSmgoKUlJSClKYABBCCqJIAH0aigoKU7pSCgoKCgoIABhCCppSCgoK4gpKCgoKCgoKClJamsoKCAA4egqaCpoKmgqaCgoIACxiCpoKClKaCpoKmgoKCABg82AACEPKClISmqNyCgpSmgrqCgpSWlKaWgpSUgoCCysiorsKSggACENKSgoIAAhoACgKCAA0egoKCAAIS4oKCgqaokoKWgoKCAA0clJSmooKCloKCooKAgqaCgpS6uoKCgrjegoKClIKCgpSCiuiCgoK4gpSCgpS0tLS6gpaWqsIADyKCgoKAgqSClIKClILeuoCClKSClLgADSKynuKUgriSgoKClKaUgoKClKqigoKm1oKmgqaC", "81-83:1;84-86:2;87-89:3")]
[assembly: go.GoPositionMap("internal/trace/generation.go", "generation.cs", "AClgAAkC7oKCgIKkmKKCooKUgriClJSUlJSUkoKUAAcQlICCyoIABxKCloCCuIKCpoaospSAgtaAgtaCgpS0goKUgpS0gpSAgtaAgqS0qsqCkoKCgoKUgoKClIKCgqaUrNKClIKCgpaClIKClIKogoKogoKUgqiCgpSCqIKCgIK2rNKClIKCgpaUgoKUgqiCgqiCgpSCqIKUgoKUgoKUgoKUgoKUhICCAAkUgIK2rLKClIKCgpaUgoKUgqiCgqiCgpSWgoKUloKClIKCqIKCqAAKFKiSgpSCloKCpqqigpSCgoKUuA==", "133-135:1;189-208:1")]
[assembly: go.GoPositionMap("internal/trace/mud.go", "mud.cs", "ADJ4woKWgqiClIKogoKCgpSClIKCgpSClIKCgoLMgIKClJSmAAUU0qiCgoKUgoKUlIIAAhDSgpQAAhTygqiCmIKClIKCkoKCgpKCgpKCgpSCpqiCgoKmypSUgpSUgpQ=", "170-172:1")]
[assembly: go.GoPositionMap("internal/trace/oldtrace.go", "oldtrace.cs", "AHW8AaKCgoKCgoKCgoKEhpKCgoKmqIKUgpKAgraCgoKUgpaClIKSgoKUgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoSUlpaCuoLugoKCqsKCgoSCuIKUgpaCgpaEgpKCuoKUgqYAARIACgKCgoSUgqaCuAAOHoK2goCCgpSCgsaAgoKUgoLGpKSCgoKkgqSkpIKClKSCgoKUtgANGgAOHKSCpIKkgqSCpIKkpIKkgqSCpIKkgqSCpIKogqKCyoKUlIKCygAMHAAIFIK4pKSCgqSIsqSkpKSkgoKClIKCpIaCooLutpS0tMSCpIz0poKAgriCgoCCtoKUgoKUAAwaqqKCgg==", "124-128:1;142-146:2;409-420:1", "", "555=addExtraString/1/1/4")]
[assembly: go.GoPositionMap("internal/trace/order.go", "order.cs", "ACNcAA8ClJaSgoSCgoKClpKCgriUgoKWgoKUgpSClAAQCgAzkgGikpKClIKAgriCkriCkoKUlIKClLaWgoIABxCUgoKCgoKCpoKmgqaigrqC3MqCgIKkgoKCgoKmAAsUgoKUgpSCgIKkgoKCgqaigoKCuKaCgIIACBCCloKogoKohIKUgpSCgqiCgqiCAAcSgoKmopKClIKUgoCCgpSSlIKUpISClrSSmrLKlIKegvaUuJK4muSCpsaCgIK2gIK2koCCpJKClIKCptiAgqSCgpSCuIKWkqaCpoLEgqaigoKCuKaCgIKkgoKCgoKmtIKCgriUgqaCpgALFICCpIKClIKUgoKCuJS6lojmpsaCgoKWgoKCpsaAgqSCgpSCpoKCgpQACxiCgpSCgqbYgIKkgoKUgpSWgoKUgpSCgqYADBaCgoKUgsqAgqSCgpSClIKCgoKm2ICCtpKAgqSUgoKCpgASIoCCtoKClIKmgoKWgoKClIKmgpaCgpSCpgAKEIKAgsiCuIK6goKClIKAgqSCprKCgLiCgqaCpICCpIKmsoCCpIKCgoKUgoKUgIKkgqaygIKkgoKCgpSCgpSAgqSCAAIUAAkCgoKClIKCgpSUlIKUgoCCpIKmooKCgoKClJSUgpSCgoCCpIKmooKUlIKUgpSCgoCCpIKmtICCpIKmtICCpIKmtICCpICCpIKmosqCgpSAgqSCpqKAgqSCgpSCprSAgqSCgpSCgpSAgqSCpqK4goKUgIKkgqaigIKkgoKUgoKUppSCprSAgqSCqJIACRi0gpKCqIKSgqiCkoKUAA4ekpSkpKQAFS6SgIKkAA0ikoKokpSUgIKkggAXOLKClIKqooKClJKClKiSgoKmrLKCgoKCgqaCpoKCggAIFJKokqiSpoKCgpSCgpSCgpSCggAQJpKClIKokoKCqIKCgoKWgoKqooKUgoKCggACEPIABxCC")]
[assembly: go.GoPositionMap("internal/trace/reader.go", "reader.cs", "ACJGkoKCgpSUgoKUyAAMGAAEEAAKAoKClJQAESiCgpSAgqSClKiAgriCgoKUgpTclpKCgpSWlrKCgoKClJSUqJSChICCuIKClJSmlKaCgoKCyoKUgIKS3IKCgoCCkoKCtoLKgoKU5oKCgoKCgpSU", "98-109:1;162-182:2", "", "48=convertOldFormat/1/1/1")]
[assembly: go.GoPositionMap("internal/trace/resources.go", "resources.cs", "ACRkogAIDLKUpKSkpKSkAAkkotyylKSkpKQADyaylKSkpKQACRaygoKUtLS0gqyygpSssoKUrLKClKyygpQAGz6C7oIABhzygpQAAhTygpQ=")]
[assembly: go.GoPositionMap("internal/trace/summary.go", "summary.cs", "ADqKAaKqwoKClAAkVILugriokoSCuIKUqLKCgoKClIKCgoKUpqKCgoKUgoKUrNKEgqiCqIKUgpSClIKUgpSClKzSgpSEzIKCgoKmACFOkgANHrKClISWqIKmkoLEmIKUlqKUloKCAAAYgAALAoKCpKaCpqKCuJKCuJKCgpSUgpaCAAMQwtyCqqaCora0goKClrS0goLcgoKigIKCggAIDoKSnqqCAAgCtoKUgoCCpJTWgoKUpIKAgqaUxoKUgoKUgriCguyEgpKkgoKCgJSCgoK4lIKCpIKCqoKCgoKilJqygoK2hoKC6IKCgoKUqqKCloKCgoKClJSClJSUAA4SuoqigpSClIKClIKCgpTugoKUgoKUgoCCtpTWuA==", "587-600:1", "", "112=UnknownTime/1/1/5;523=Time/1/1/4,snapshotStat/1/1/4;662=Goroutine/1/1/1")]
[assembly: go.GoPositionMap("internal/trace/value.go", "value.cs", "ABQ64qyygpSuwpSk")]
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
    [GoValueClone("extraArr")] internal partial struct oldTraceConverter {}
    internal partial struct ordering {}
    internal partial struct pState {}
    internal partial struct queue<T> {}
    internal partial struct rangeP {}
    internal partial struct rangeState {}
    internal partial struct rangeType {}
    internal partial struct schedCtx {}
    internal partial struct seqCounter {}
    internal partial struct spilledBatch {}
    internal partial struct stack {}
    internal partial struct stackID {}
    internal partial struct stringID {}
    internal partial struct taskState {}
    internal partial struct timedEventArgs {}
    internal partial struct timestamp {}
    internal partial struct totalUtil {}
    internal partial struct userRegion {}
    internal partial struct utilHeap {}
    public partial struct EventKind {}
    public partial struct ExperimentalBatch {}
    public partial struct ExperimentalData {}
    public partial struct ExperimentalEvent {}
    public partial struct Frame {}
    public partial struct GoID {}
    public partial struct GoState {}
    public partial struct GoroutineExecStats {}
    public partial struct GoroutineSummary {}
    public partial struct MMUCurve {}
    public partial struct MutatorUtil {}
    public partial struct ProcID {}
    public partial struct ProcState {}
    public partial struct RangeAttribute {}
    public partial struct Reader {}
    public partial struct ResourceID {}
    public partial struct ResourceKind {}
    public partial struct StackFrame {}
    public partial struct Summarizer {}
    public partial struct Summary {}
    public partial struct TaskID {}
    public partial struct ThreadID {}
    public partial struct UserRegionSummary {}
    public partial struct UserTaskSummary {}
    public partial struct UtilFlags {}
    public partial struct UtilWindow {}
    public partial struct Value {}
    public partial struct ValueKind {}
    [GoValueClone("@base")] public partial struct ΔEvent {}
    public partial struct ΔLabel {}
    public partial struct ΔLog {}
    public partial struct ΔMetric {}
    public partial struct ΔRange {}
    public partial struct ΔRegion {}
    public partial struct ΔStack {}
    public partial struct ΔStateTransition {}
    public partial struct ΔTask {}
    public partial struct ΔTime {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸevent() => builtin.initPackage(typeof(go.@internal.trace.event_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸeventꓸgo122() => builtin.initPackage(typeof(go.@internal.trace.@event.go122_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸinternalꓸoldtrace() => builtin.initPackage(typeof(go.@internal.trace.@internal.oldtrace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
