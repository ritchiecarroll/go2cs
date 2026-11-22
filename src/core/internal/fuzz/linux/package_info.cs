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
using binary = go.encoding.binary_package;
using context = go.context_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.fuzz_package;
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
[assembly: GoDynamicTypeLift("7374727563747b506172656e7420737472696e673b205061746820737472696e673b2044617461205b5d627974653b2056616c756573205b5d616e793b2047656e65726174696f6e20696e743b2049735365656420626f6f6c7d", "CorpusEntryᴛ1")]
[assembly: GoTypeAlias("CorpusEntry", "go.@internal.fuzz_package.CorpusEntryᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<MalformedCorpusError, error>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<contextReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<crashError, error>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<pcgRand, mutatorRand>(Pointer = true)]
[assembly: GoImplement<syscall_package.Signal, os_package.Signal>]
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
[assembly: GoFieldAccess(typeof(coordinator))]
[assembly: GoFieldAccess(typeof(fuzzResult))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BasicLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(mutator))]
[assembly: GoFieldAccess(typeof(pcgRand))]
[assembly: GoFieldAccess(typeof(sharedMem))]
[assembly: GoFieldAccess(typeof(sharedMemHeader))]
[assembly: GoFieldAccess(typeof(time_package.Ticker))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]
[assembly: GoFieldAccess(typeof(workerClient))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/fuzz/counters_supported.go", "counters_supported.cs", "AAoiwoKC")]
[assembly: GoPositionMap("internal/fuzz/coverage.go", "coverage.cs", "AAscooKuwoKCvKKClIKCgoKmgpSCgpSqooKClKqigoKmqqKCgqamgoKClLiSAAwWgoKCgoKCgg==")]
[assembly: GoPositionMap("internal/fuzz/encoding.go", "encoding.cs", "ABkwwoKUpoIACgS0AAwaAAoWxoKUxgAPHIKU6rTEtqiygpSCgpSCgpSCooKClIKClJQAEQaCgoKClIKClIKUlIKClIKClIKClIKClKaCgoKClKS0tKaCgpSCgoKUgoKClMqYgpSCpoK0gpSClJTEppS0gpTEuKSClLSClKSmgpSCgpSCgpSClIKUpIKUpIKUpIKUgqSClLSClIKClKSClIKClKS6ku6CpIKkgqSCpLTKkpSCpIKkgqSCpLQ=")]
[assembly: GoPositionMap("internal/fuzz/fuzz.go", "fuzz.cs", "AE2qAQAOAoKUgpSClJSmgoKmgoKCyIKSlpKCsoKCgpS4uJSClIKUgoLKgqKClIKCgpSCAAgSgoKClIKCgoKCgqaCgrKCgpSCgpTugoKSpIKUgqaCgoKmgoKCpgAJCMiCgoLqgpSUgoKCgoKUgqaClMqCgoKmgoKCyoIABxKUgoKCAAYQgoKCgoLeAAgSpqaCgoKClIKCupSCgoKCAAsaggAGEKaCgoKCAAkUyLYAFCCCpoKmggAHGOKCsoKClIKCgpSCgsqUgpQAGUKigqYAgQHGArSCgqYACBKClIKCgoLKgoK4goKmgoKmlJSCgoKClIKCgqamgoKCpoKCgoKClJSClIKCgpSmggACGgAKAqaUgqaUpoKClMqClKaCpoKCgpSCgqaokoKqooK8ooIABhCCpsqqoqaUgoKUlIKUgoKCgpSCgriCgoKmqqKCAAIgAA0CrLKClIKCgoKUqqKoggACENKClIKCptyClAAJFKKCgpSuwoKCgoKUgoLcgpSCgoKUgoKCgpSUgpSmgoKClIKUqqKClIKClIKCpq7igoKClIKClKaCpoKCgqamABMolIKmooI=", "119-144:1;149-164:2;184-194:3", "", "173=New/1/1/2;301=New/1/1/2;320=diffCoverage/1/1/3,countBits/1/1/3;332=countBits/1/1/3;371=countBits/1/2/5,countBits/2/2/6;660=Now/1/2/2,Now/2/2/6;775=warmupRun/1/1/3", "122:21", "163:57,164:13")]
[assembly: GoPositionMap("internal/fuzz/mem.go", "mem.cs", "ADF2tOr0goKUooKCyoKCuILokqqigoKokoKssoKClIIAAhLigoKU", "72-77:1", "", "", "", "75:57,76:13")]
[assembly: GoPositionMap("internal/fuzz/minimize.go", "minimize.cs", "AAkWgqai3KaCgoKUgoKmyoKClIKCgoKmgqa4goKCgpSCgoKmgoLcgoKCpoKClKbG", "", "", "", "23:27")]
[assembly: GoPositionMap("internal/fuzz/mutator.go", "mutator.cs", "ABImgqaCpoKClKqipKSkugAKELiClLS0tLS0tLS0tLSCxrS0gpSClIKUgrSClIKUgpSCxMiCgoKCpoKUlJSCtoKUlJSCyoKCgoKmgpSUpoK2gpSUlILKgoKCpoKUgpSUgraClIKUlIK2goKUgpSUgraClILuABQq0oKCgpSmgoKCgvyCggAEEIKClII=", "259-264:1", "", "", "", "306:62,307:13")]
[assembly: GoPositionMap("internal/fuzz/mutators_byteslice.go", "mutators_byteslice.cs", "AAoQsoKUgoKCgqrCgoKClIKCgpSqwoKUgoKClLiClKamppSCgqiygpSCgoKUgoKosoKUgoKosoKUuIKosoKUgoKClIKosoKUgoKClJSosoKUgoKUgoKCqLKClIKClIKCgqiygpSCgpSCgoKqwoKUgoKqwoKUgoKCqsKClIKCgqiygpQACBKCgpSCgoKClKiygpSCgoKClKiygpSCgoLKgoKUqLKClIKCgsqCgpSmgsqClIKCgoKCgg==")]
[assembly: GoPositionMap("internal/fuzz/pcg.go", "pcg.cs", "ADNYgoKCgpKClKaokoKCgpSCgoKCgoKmgoKmsoKmgoKokoKCqqKClAACENKCgoKCgoKCgqaokgAKFpI=")]
[assembly: GoPositionMap("internal/fuzz/queue.go", "queue.cs", "ABcugqaCgoKClIKCgpSCpoKClIKCpoKClIKCgoKmgoKUpoI=")]
[assembly: GoPositionMap("internal/fuzz/sys_posix.go", "sys_posix.cs", "ABMkgoKCgoKmqsiypoKUgoKmqqKCgoIACgiygoKCgoKUgoKUgoKClIKCqqKCgpSCrLKCgpSCAAIcAAsCAAcSpA==", "", "", "40=Munmap/1/1/1,Close/1/1/2")]
[assembly: GoPositionMap("internal/fuzz/trace.go", "trace.cs", "AAowAAoCgoKUgoKClII=")]
[assembly: GoPositionMap("internal/fuzz/worker.go", "worker.cs", "ADx+goKClIKCAAkWkoKClIIAAhoADQSUgoK45oKClLiCgpQAChaUgKbIytyCgpSClJSmlNyUpsqClAAJFMiCytyCpoIABxIABRIACgKCgoK2yoKUgtwABxAACRSCpgAOFoIAAhYACgKClICCpICCgoKUlKakAAIaAA8CgpSCgpSCggAJFIKClJKCgoKUkqaCgoLcgoKCgpSCgqYABhgACwKClLaUpoKCggAIDoKSgqaCuKaCgsaCgoKCuIKmgoLIgoLIAAcgAAoCkoKUxoKUkoKC6ACCAa4CAAwCgoKCgoCCgpTIgpS0tLSmgIIAFDYAEAKCgoKUlIKQpIKCgqSCgpKClIKCpoKCgpSClKaygoKCgoKClJSClKaCgoKClIKClKaCpKSCgpSUgoKCgpSCgoKUggAKDNKCkJKCkJKCgpSCgoKC2oKCgoKCgoLugpS4AAUQAAkCgoKCgqiC3IKCgoKUgpTcwoKUtLSkgoKCgoKCgriUpoKUgpSUtMSkpqKCqqIAECSCrAAIAoK4gIKC2oCCgqQADBSYAAsCgpSSgpSAkoKCgpSCgoKClIKCpoKCgoKCgqaCgpTKlLS0pJSmlIKCgsqCgoKmgoKCuIKCggAJCAALAoKUgoKUgoKCgpSClIKCgpSCgpSQkpSClISCgoKUgpSCgqaUgoLcprjo4oKCgoLaooKCgpQADyCigpSYooKigqa0pA==", "386-389:1;426-429:1;491-499:1;492-494:1.1;715-715:1;724-727:2;741-743:3;744-759:4;805-805:1;807-807:2;855-858:1;881-912:2;996-996:1;1100-1100:1;1184-1187:1", "", "282=Sprintf/1/1/2;537=newMutator/1/1/11", "391:21,398:21,546:29", "781:60,782:13,794:60,795:13,891:64,892:13,896:64,897:13,1132:64,1133:13,1261:60,1262:13")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static global::go.@internal.fuzz_package;
using binary = global::go.encoding.binary_package;
using context = global::go.context_package;
using time = global::go.time_package;
using ꓸꓸꓸCorpusEntry = Span<CorpusEntry>;
using ꓸꓸꓸany = Span<any>;

[GoPackage("fuzz")]
public static partial class fuzz_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface mutatorRand {}
    internal partial struct call {}
    internal partial struct contextReader {}
    internal partial struct coordinator {}
    internal partial struct corpus {}
    internal partial struct crashError {}
    internal partial struct fuzzInput {}
    internal partial struct fuzzMinimizeInput {}
    internal partial struct fuzzResponse {}
    internal partial struct fuzzResult {}
    internal partial struct minimizeResponse {}
    internal partial struct mutator {}
    internal partial struct noCopy {}
    internal partial struct pcgRand {}
    internal partial struct pingResponse {}
    internal partial struct queue {}
    internal partial struct sharedMem {}
    internal partial struct sharedMemHeader {}
    internal partial struct sharedMemSys {}
    internal partial struct worker {}
    internal partial struct workerClient {}
    internal partial struct workerComm {}
    internal partial struct workerServer {}
    public partial struct CoordinateFuzzingOpts {}
    public partial struct CorpusEntryᴛ1 {}
    public partial struct MalformedCorpusError {}
    public partial struct fuzzArgs {}
    public partial struct minimizeArgs {}
    public partial struct pingArgs {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(global::go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(global::go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(global::go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fuzz_package.MalformedCorpusError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛMalformedCorpusError;
[GoMethodHost(typeof(fuzz_package.contextReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛcontextReader;
[GoMethodHost(typeof(fuzz_package.coordinator)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛcoordinator;
[GoMethodHost(typeof(fuzz_package.crashError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛcrashError;
[GoMethodHost(typeof(fuzz_package.mutator)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛmutator;
[GoMethodHost(typeof(fuzz_package.noCopy)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛnoCopy;
[GoMethodHost(typeof(fuzz_package.pcgRand)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛpcgRand;
[GoMethodHost(typeof(fuzz_package.queue)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛqueue;
[GoMethodHost(typeof(fuzz_package.sharedMem)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛsharedMem;
[GoMethodHost(typeof(fuzz_package.worker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛworker;
[GoMethodHost(typeof(fuzz_package.workerClient)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛworkerClient;
[GoMethodHost(typeof(fuzz_package.workerServer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fuzzᴛworkerServer;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fuzzᴛMalformedCorpusError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref MalformedCorpusError e) => fuzz_package.Error⁔(ref e);
}
partial class fuzzᴛcontextReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<contextReader> Ꮡcr, slice<byte> b) => fuzz_package.Read⁔(Ꮡcr, b);
}
partial class fuzzᴛcoordinator {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool, error) addCorpusEntries(this ref coordinator c, bool addToCache, params ꓸꓸꓸCorpusEntry entriesʗp) => fuzz_package.addCorpusEntries⁔(ref c, addToCache, entriesʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void updateStats(this ref coordinator c, fuzzResult result) => fuzz_package.updateStats⁔(ref c, result);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logStats(this ref coordinator c) => fuzz_package.logStats⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fuzzInput, bool) peekInput(this ref coordinator c) => fuzz_package.peekInput⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sentInput(this ref coordinator c, fuzzInput input) => fuzz_package.sentInput⁔(ref c, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void refillInputQueue(this ref coordinator c) => fuzz_package.refillInputQueue⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void queueForMinimization(this ref coordinator c, fuzzResult result, slice<byte> keepCoverage) => fuzz_package.queueForMinimization⁔(ref c, result, keepCoverage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fuzzMinimizeInput, bool) peekMinimizeInput(this ref coordinator c) => fuzz_package.peekMinimizeInput⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sentMinimizeInput(this ref coordinator c, fuzzMinimizeInput input) => fuzz_package.sentMinimizeInput⁔(ref c, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool warmupRun(this ref coordinator c) => fuzz_package.warmupRun⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint updateCoverage(this ref coordinator c, slice<byte> newCoverage) => fuzz_package.updateCoverage⁔(ref c, newCoverage);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool canMinimize(this ref coordinator c) => fuzz_package.canMinimize⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration elapsed(this ref coordinator c) => fuzz_package.elapsed⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readCache(this ref coordinator c) => fuzz_package.readCache⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void debugLogf(this ref coordinator c, @string format, params ꓸꓸꓸany argsʗp) => fuzz_package.debugLogf⁔(ref c, format, argsʗp);
}
partial class fuzzᴛcrashError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref crashError e) => fuzz_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this ref crashError e) => fuzz_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string CrashPath(this ref crashError e) => fuzz_package.CrashPath⁔(ref e);
}
partial class fuzzᴛmutator {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint rand(this ref mutator m, nint n) => fuzz_package.rand⁔(ref m, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static binary.ByteOrder randByteOrder(this ref mutator m) => fuzz_package.randByteOrder⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint chooseLen(this ref mutator m, nint n) => fuzz_package.chooseLen⁔(ref m, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mutate(this ж<mutator> Ꮡm, slice<any> vals, nint maxBytes) => fuzz_package.mutate⁔(Ꮡm, vals, maxBytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 mutateInt(this ref mutator m, int64 v, int64 maxValue) => fuzz_package.mutateInt⁔(ref m, v, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 mutateUInt(this ref mutator m, uint64 v, uint64 maxValue) => fuzz_package.mutateUInt⁔(ref m, v, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static float64 mutateFloat(this ref mutator m, float64 v, float64 maxValue) => fuzz_package.mutateFloat⁔(ref m, v, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mutateBytes(this ж<mutator> Ꮡm, ж<slice<byte>> ᏑptrB) => fuzz_package.mutateBytes⁔(Ꮡm, ᏑptrB);
}
partial class fuzzᴛnoCopy {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Lock(this ref noCopy _) => fuzz_package.Lock⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Unlock(this ref noCopy _) => fuzz_package.Unlock⁔(ref _);
}
partial class fuzzᴛpcgRand {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void step(this ref pcgRand r) => fuzz_package.step⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void save(this ref pcgRand r, ж<uint64> ᏑrandState, ж<uint64> ᏑrandInc) => fuzz_package.save⁔(ref r, ᏑrandState, ᏑrandInc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void restore(this ref pcgRand r, uint64 randState, uint64 randInc) => fuzz_package.restore⁔(ref r, randState, randInc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32(this ref pcgRand r) => fuzz_package.uint32⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint intn(this ref pcgRand r, nint n) => fuzz_package.intn⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 uint32n(this ref pcgRand r, uint32 n) => fuzz_package.uint32n⁔(ref r, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool @bool(this ref pcgRand r) => fuzz_package.bool⁔(ref r);
}
partial class fuzzᴛqueue {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint cap(this ref queue q) => fuzz_package.cap⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this ref queue q) => fuzz_package.grow⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void enqueue(this ref queue q, any e) => fuzz_package.enqueue⁔(ref q, e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (any, bool) dequeue(this ref queue q) => fuzz_package.dequeue⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (any, bool) peek(this ref queue q) => fuzz_package.peek⁔(ref q);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clear(this ref queue q) => fuzz_package.clear⁔(ref q);
}
partial class fuzzᴛsharedMem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<sharedMemHeader> header(this ref sharedMem m) => fuzz_package.header⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> valueRef(this ref sharedMem m) => fuzz_package.valueRef⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> valueCopy(this ref sharedMem m) => fuzz_package.valueCopy⁔(ref m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setValue(this ref sharedMem m, slice<byte> b) => fuzz_package.setValue⁔(ref m, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setValueLen(this ref sharedMem m, nint n) => fuzz_package.setValueLen⁔(ref m, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref sharedMem m) => fuzz_package.Close⁔(ref m);
}
partial class fuzzᴛworker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error cleanup(this ref worker w) => fuzz_package.cleanup⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error coordinate(this ж<worker> Ꮡw, context.Context ctx) => fuzz_package.coordinate⁔(Ꮡw, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fuzzResult min, error err) minimize(this ж<worker> Ꮡw, context.Context ctx, fuzzMinimizeInput input) => fuzz_package.minimize⁔(Ꮡw, ctx, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isRunning(this ref worker w) => fuzz_package.isRunning⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error startAndPing(this ж<worker> Ꮡw, context.Context ctx) => fuzz_package.startAndPing⁔(Ꮡw, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ start(this ж<worker> Ꮡw) => fuzz_package.start⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error stop(this ж<worker> Ꮡw) => fuzz_package.stop⁔(Ꮡw);
}
partial class fuzzᴛworkerClient {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<workerClient> Ꮡwc) => fuzz_package.Close⁔(Ꮡwc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (CorpusEntry entryOut, minimizeResponse resp, error retErr) minimize(this ж<workerClient> Ꮡwc, context.Context ctx, CorpusEntry entryIn, minimizeArgs argsʗp) => fuzz_package.minimize⁔(Ꮡwc, ctx, entryIn, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (CorpusEntry entryOut, fuzzResponse resp, bool isInternalError, error err) fuzz(this ж<workerClient> Ꮡwc, context.Context ctx, CorpusEntry entryIn, fuzzArgs argsʗp) => fuzz_package.fuzz⁔(Ꮡwc, ctx, entryIn, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ping(this ж<workerClient> Ꮡwc, context.Context ctx) => fuzz_package.ping⁔(Ꮡwc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ callLocked(this ref workerClient wc, context.Context ctx, call c, any resp) => fuzz_package.callLocked⁔(ref wc, ctx, c, resp);
}
partial class fuzzᴛworkerServer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error serve(this ж<workerServer> Ꮡws, context.Context ctx) => fuzz_package.serve⁔(Ꮡws, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fuzzResponse /*resp*/ fuzz(this ж<workerServer> Ꮡws, context.Context ctx, fuzzArgs args) => fuzz_package.fuzz⁔(Ꮡws, ctx, args);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static minimizeResponse /*resp*/ minimize(this ж<workerServer> Ꮡws, context.Context ctx, minimizeArgs args) => fuzz_package.minimize⁔(Ꮡws, ctx, args);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool success, error retErr) minimizeInput(this ж<workerServer> Ꮡws, context.Context ctx, slice<any> vals, ж<sharedMem> Ꮡmem, minimizeArgs args) => fuzz_package.minimizeInput⁔(Ꮡws, ctx, vals, Ꮡmem, args);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static pingResponse ping(this ref workerServer ws, context.Context ctx, pingArgs args) => fuzz_package.ping⁔(ref ws, ctx, args);
}
// </MethodForwarders>
