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
using format = go.@internal.trace.traceviewer.format_package;
using http = go.net.http_package;
using template = go.html.template_package;
using time = go.time_package;
using trace = go.@internal.trace_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.trace.traceviewer_package;

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
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<countingWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<embed_package.FS, go.io.fs_package.FS>]
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.Handler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ReadSeeker>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(go.@internal.profile_package.Profile))]
[assembly: GoFieldAccess(typeof(go.@internal.trace.traceviewer.format_package.Event))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(mmuCacheEntry))]
[assembly: GoFieldAccess(typeof(splitter))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/trace/traceviewer/emitter.go", "emitter.cs", "ACI2soKCgoKUggALBIKCpoKUlKKClKKClIKUgpSmpqaCggAKCoIACSCU9Ka4gpKClJKClJSCgqaClIKUgqbcgoKC7gAFEIK4goKCgpSCgqaCgsqCgu6CgoKUgoKmggATKIKCpgAAHgATEJSmgoKmgpSCgriokoKCgpSCAAkcopQAI0aCgriCgpSmgqaCpoKmgoKUpoKmgoKUABcwgoKUgpSCgpSCAAoWABQqgoKUgpSCABYugoKUpoKmgoKUggAIEgASJoKmgoKmgqaCgoKClIIACximgoKClIIACximiLKClIKmgoKUgoKUggAHEKiSggAGEKaCqqIADgaClIKUgqaUgoKClIKClJSmpoLuAAgSgtwABxSiqJKClIKClIKCgoKCgpQABxCCAF2IAQAZNIKC", "36-40:1;41-65:2;47-50:2.1;51-54:2.2;66-68:3;69-73:4;97-99:1;100-129:2;106-109:2.1;110-113:2.2;121-123:2.3;124-126:2.4;130-132:3;133-206:4", "", "211=Sprintf/1/1/1;229=Sprintf/1/1/1;396=viewerTime/1/2/3,viewerTime/2/2/4;432=viewerTime/1/1/4;443=viewerTime/1/1/4;472=viewerTime/1/1/5;511=viewerTime/1/1/6;520=viewerTime/1/1/6;559=viewerTime/1/1/3;580=viewerTime/1/1/3;616=viewerTime/1/1/3")]
[assembly: GoPositionMap("internal/trace/traceviewer/histogram.go", "histogram.cs", "ABculpKCgpSCgpSCgpSClKiSqLKCppSCgoK4goKUgpSmgpSCqIKC")]
[assembly: GoPositionMap("internal/trace/traceviewer/http.go", "http.cs", "ABEegpKCggAtVgDJAYoDgoKUAAoWguaCgoKClIIAiAGOAoI=", "16-21:1;279-286:1")]
[assembly: GoPositionMap("internal/trace/traceviewer/mmu.go", "mmu.cs", "ADFWgsqSpIKkgpS4AAsQgoKClAAQIqKCgoKClJSSgoKUgqaokoKCgqaCgoKCyoKCgpSmlIKCgpSCpoKmgoKCgoKCgpSClLiCgoIAzAGQA7KCgoKmgoKCgpSWkoKmgoKCAAgShJKigqY=", "49-59:1;101-109:1")]
[assembly: GoPositionMap("internal/trace/traceviewer/pprof.go", "pprof.cs", "ADQykqKClJKCgpSCgoKUgoKUpoKCgpSSgpSCgpSCgIKCpICCgqSCgpSCgoKUgoKiAAkSggAHEIKCgoKCgoKCgtyClAAJFIKUlMoACAaCgoKUgoKU", "26-81:1;30-34:1.1;52-55:1.2", "", "", "", "85:25,86:17")]
// </GoSourcePositionMaps>

namespace go.@internal.trace;

using static go.@internal.trace.traceviewer_package;
using format = go.@internal.trace.traceviewer.format_package;
using http = go.net.http_package;
using template = go.html.template_package;
using time = go.time_package;
using trace = go.@internal.trace_package;

[GoPackage("traceviewer")]
public static partial class traceviewer_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("eventSz")] internal partial struct SplittingTraceConsumer_eventSz {}
    internal partial struct countingWriter {}
    internal partial struct frameNode {}
    internal partial struct heapStats {}
    internal partial struct linkedUtilWindow {}
    internal partial struct mmu {}
    internal partial struct mmuCacheEntry {}
    internal partial struct task {}
    public partial struct ArrowEvent {}
    public partial struct AsyncSliceEvent {}
    [GoValueClone("gstates", "prevGstates", "threadStats", "prevThreadStats")] public partial struct Emitter {}
    public partial struct GState {}
    public partial struct InstantEvent {}
    public partial struct Mode {}
    public partial struct ProfileRecord {}
    public partial struct Range {}
    public partial struct SliceEvent {}
    public partial struct ThreadState {}
    public partial struct TimeHistogram {}
    public partial struct TraceConsumer {}
    public partial struct View {}
    public partial struct ViewType {}
    public partial struct splitter {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸembed() => builtin.initPackage(typeof(embed_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhtmlꓸtemplate() => builtin.initPackage(typeof(html.template_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸprofile() => builtin.initPackage(typeof(go.@internal.profile_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(go.@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(traceviewer_package.Emitter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛEmitter;
[GoMethodHost(typeof(traceviewer_package.Range)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛRange;
[GoMethodHost(typeof(traceviewer_package.TimeHistogram)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛTimeHistogram;
[GoMethodHost(typeof(traceviewer_package.View)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛView;
[GoMethodHost(typeof(traceviewer_package.countingWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛcountingWriter;
[GoMethodHost(typeof(traceviewer_package.mmu)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class traceviewerᴛmmu;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class traceviewerᴛEmitter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Gomaxprocs(this ref Emitter e, uint64 v) => traceviewer_package.Gomaxprocs⁔(ref e, v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Resource(this ref Emitter e, uint64 id, @string name) => traceviewer_package.Resource⁔(ref e, id, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetResourceType(this ref Emitter e, @string name) => traceviewer_package.SetResourceType⁔(ref e, name);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetResourceFilter(this ref Emitter e, Func<uint64, bool> filter) => traceviewer_package.SetResourceFilter⁔(ref e, filter);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Task(this ref Emitter e, uint64 id, @string name, nint sortIndex) => traceviewer_package.Task⁔(ref e, id, name, sortIndex);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Slice(this ref Emitter e, SliceEvent s) => traceviewer_package.Slice⁔(ref e, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void TaskSlice(this ref Emitter e, SliceEvent s) => traceviewer_package.TaskSlice⁔(ref e, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Δslice(this ref Emitter e, SliceEvent s, uint64 sectionID, @string cname) => traceviewer_package.Δslice⁔(ref e, s, sectionID, cname);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void AsyncSlice(this ref Emitter e, AsyncSliceEvent s) => traceviewer_package.AsyncSlice⁔(ref e, s);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Instant(this ref Emitter e, InstantEvent i) => traceviewer_package.Instant⁔(ref e, i);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Arrow(this ref Emitter e, ArrowEvent a) => traceviewer_package.Arrow⁔(ref e, a);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void TaskArrow(this ref Emitter e, ArrowEvent a) => traceviewer_package.TaskArrow⁔(ref e, a);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void arrow(this ref Emitter e, ArrowEvent a, uint64 sectionID) => traceviewer_package.arrow⁔(ref e, a, sectionID);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Event(this ref Emitter e, ж<format.Event> Ꮡev) => traceviewer_package.Event⁔(ref e, Ꮡev);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void HeapAlloc(this ref Emitter e, time.Duration ts, uint64 v) => traceviewer_package.HeapAlloc⁔(ref e, ts, v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Focus(this ref Emitter e, uint64 id) => traceviewer_package.Focus⁔(ref e, id);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void GoroutineTransition(this ref Emitter e, time.Duration ts, GState from, GState to) => traceviewer_package.GoroutineTransition⁔(ref e, ts, from, to);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void IncThreadStateCount(this ref Emitter e, time.Duration ts, ThreadState state, int64 delta) => traceviewer_package.IncThreadStateCount⁔(ref e, ts, state, delta);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void HeapGoal(this ref Emitter e, time.Duration ts, uint64 v) => traceviewer_package.HeapGoal⁔(ref e, ts, v);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void emitHeapCounters(this ref Emitter e, time.Duration ts) => traceviewer_package.emitHeapCounters⁔(ref e, ts);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Err(this ref Emitter e) => traceviewer_package.Err⁔(ref e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool tsWithinRange(this ref Emitter e, time.Duration ts) => traceviewer_package.tsWithinRange⁔(ref e, ts);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void OptionalEvent(this ref Emitter e, ж<format.Event> Ꮡev) => traceviewer_package.OptionalEvent⁔(ref e, Ꮡev);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Flush(this ref Emitter e) => traceviewer_package.Flush⁔(ref e);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void threadMeta(this ref Emitter e, uint64 sectionID, uint64 tid, @string name, nint priority) => traceviewer_package.threadMeta⁔(ref e, sectionID, tid, name, priority);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void processMeta(this ref Emitter e, uint64 sectionID, @string name, nint priority) => traceviewer_package.processMeta⁔(ref e, sectionID, name, priority);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Stack(this ref Emitter e, slice<trace.StackFrame> stk) => traceviewer_package.Stack⁔(ref e, stk);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint buildBranch(this ref Emitter e, frameNode parent, slice<trace.StackFrame> stk) => traceviewer_package.buildBranch⁔(ref e, parent, stk);
}
partial class traceviewerᴛRange {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string URL(this Range r, ViewType viewType) => traceviewer_package.URL⁔(r, viewType);
}
partial class traceviewerᴛTimeHistogram {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ref TimeHistogram h, time.Duration d) => traceviewer_package.Add⁔(ref h, d);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static time.Duration BucketMin(this ref TimeHistogram h, nint bucket) => traceviewer_package.BucketMin⁔(ref h, bucket);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static template.HTML ToHTML(this ж<TimeHistogram> Ꮡh, Func<time.Duration, time.Duration, @string> urlmaker) => traceviewer_package.ToHTML⁔(Ꮡh, urlmaker);
}
partial class traceviewerᴛView {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string URL(this View v, nint rangeIdx) => traceviewer_package.URL⁔(v, rangeIdx);
}
partial class traceviewerᴛcountingWriter {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref countingWriter cw, slice<byte> data) => traceviewer_package.Write⁔(ref cw, data);
}
partial class traceviewerᴛmmu {
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<slice<trace.MutatorUtil>>, ж<trace.MMUCurve>, error) get(this ж<mmu> Ꮡm, trace.UtilFlags flags) => traceviewer_package.get⁔(Ꮡm, flags);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HandlePlot(this ж<mmu> Ꮡm, http.ResponseWriter w, ж<http.Request> Ꮡr) => traceviewer_package.HandlePlot⁔(Ꮡm, w, Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void HandleDetails(this ж<mmu> Ꮡm, http.ResponseWriter w, ж<http.Request> Ꮡr) => traceviewer_package.HandleDetails⁔(Ꮡm, w, Ꮡr);
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static linkedUtilWindow newLinkedUtilWindow(this ref mmu m, trace.UtilWindow ui, time.Duration window) => traceviewer_package.newLinkedUtilWindow⁔(ref m, ui, window);
}
// </MethodForwarders>
