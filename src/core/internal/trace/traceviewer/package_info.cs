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
global using execꓸError = go.os.exec_package.ΔError;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using templateꓸError = go.html.template_package.ΔError;
global using templateꓸFuncMap = go.text.template_package.FuncMap;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using traceꓸEvent = go.@internal.trace_package.ΔEvent;
global using traceꓸLabel = go.@internal.trace_package.ΔLabel;
global using traceꓸLog = go.@internal.trace_package.ΔLog;
global using traceꓸMetric = go.@internal.trace_package.ΔMetric;
global using traceꓸRange = go.@internal.trace_package.ΔRange;
global using traceꓸRegion = go.@internal.trace_package.ΔRegion;
global using traceꓸStack = go.@internal.trace_package.ΔStack;
global using traceꓸStateTransition = go.@internal.trace_package.ΔStateTransition;
global using traceꓸTask = go.@internal.trace_package.ΔTask;
global using traceꓸTime = go.@internal.trace_package.ΔTime;
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
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.ΔHandler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.ReadSeeker>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/trace/traceviewer/emitter.go", "emitter.cs", "AB42soKCgoKEggALBIKCpoKUlKKClKKClIKUgpSmpqaCggAKCoIABSCE9Ka4gpKClJKClJSCgraClIKUgqbcgoKCzgADEIKogoKCgpSCgpaCgrqCgu6CgoKUgoKWgIIAFCiCgqYAAB4AEBCEloKCloKUgoKoqJKCgoKUggAJHKKEACNGgoK4goKUpoKmgqaCpoKClKaCpoKClAAXMIKClIKUkoKUggAKFgAUKoKClIKUkgAWLoKClKaCpoKClIIACBIAEiaCpoKCpoKmgoKCgpSCAAsYpoKCgpSCAAsYpoiygpSCpoKClJKClIIABxCokoIABhCmgqqiAAkGgoSClIKWhIKCgoSCgpSUlqaC7gAIEoLcAAcUoqiSgpSCgoSCgoKCgoKUAAcQggBaiAEAGTSCgg==", "36-40:1;41-65:2;47-50:2.1;51-54:2.2;66-68:3;69-73:4;97-99:1;100-129:2;106-109:2.1;110-113:2.2;121-123:2.3;124-126:2.4;130-132:3;133-206:4")]
[assembly: go.GoPositionMap("internal/trace/traceviewer/histogram.go", "histogram.cs", "ABculpKCgpSCgpSCgpSClKiSqLKCloSCgoKogoKUgpSmgpSCqIKC")]
[assembly: go.GoPositionMap("internal/trace/traceviewer/http.go", "http.cs", "ABEegpKAgoIALVYAxwGKA4KClAAKFoLWgoKAgoKkggCIAY4Cgg==", "16-21:1;279-286:1")]
[assembly: go.GoPositionMap("internal/trace/traceviewer/mmu.go", "mmu.cs", "AC9WgsqSlIKkgqS4AAoQgoKClAAQIqKCgoKClISSgoKUgqaokoKCgpaCgoKCuoKCgIKkppSCgoKUgqaAgraCgoKCgoKClIKUqIKCggDLAZADsoKCgpaCgoKClIaSgpaCgoIACBKEkqKCpg==", "49-59:1;101-109:1")]
[assembly: go.GoPositionMap("internal/trace/traceviewer/pprof.go", "pprof.cs", "ACkykqKChJKCgpSCgoKUgIKCpJaCgoKUkoKUgoKUgoCCgqSAgoKkgIKCpIKAgoKkgoKiAAkSggAHEIKCgoKCgoKCgtyClAAJFIKUlMrmgoKClIKAgqQ=", "26-81:1;30-34:1.1;52-55:1.2")]
// </GoSourcePositionMaps>

namespace go.@internal.trace;

[GoPackage("traceviewer")]
public static partial class traceviewer_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
