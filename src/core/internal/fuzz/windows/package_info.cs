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
using static go.@internal.fuzz_package;

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
[assembly: GoPositionMap("internal/fuzz/encoding.go", "encoding.cs", "ABkwwoKUpoIACgS0AAwaAAoWxoKUxgAPHIKU6rTEtqiygpSCgpSCgpSCooKClIKClJQAEQaCgoKClIKClIKUlIKClIKClIKClIKClKaCgoKClKS0tKaCgpSCgoKUgoKClMqYgpSCpoK0gpSClJTEppS0gpTEuLSClLSCpKSmgpSCgpSCgpSClIKUpIKUpIKUpIKUgqSClLSClIKClKSClIKClKS6kv6CpIKkgqSCpLTKkqSCpIKkgqSCpLQ=")]
[assembly: GoPositionMap("internal/fuzz/fuzz.go", "fuzz.cs", "AE+qAQAOAoKUgpSClJSmgoKmgoKCyIKSlpKCsoKCgpS4uJSClIKUgoLKgqKClIKCgpSCAAgSgoKClIKCgoKCgqaCgrKCgpSCgpTugoKSpIKUgqaCgoKmgoKCpgAJCMiCgoLqgpSUgoKCgoKUgqaClMqCgoKmgoKCyoIABxKUgoKCAAYQgoKCgoLeAAgSpqaCgoKClIKCupSCgoKCAAsaggAGEKaCgoKCAAkUyLYAFiCCpoKmggALGOKCsoKClIKCgpSCgsqUgpQAG0KigqYAgQHGArSCgqYACBKClIKCgoLKgoK4goKmgoKmlJSCgoKClIKCgqbGgoKCpoKCgoKClJSClIKCgpSmggACGgAKAqaUgqaUpoKClMqClKaCpoKCgpSCgqaokoKqooK8ooIABhCCpsqqoqaUgoKUlIKUgoKCgpSCgriCgoKmqqKCAAIgAA0CrLKClIKCgoKUqqKoggACENKClIKCptyClAANFKKCgpTOwoKCgoKUgoLcgpSCgoKUgoKCgpSUgpSmgoKClIKUqqKClIKClIKCpq7igoKClIKClKaCpoKCgqamABMolILGooI=", "119-144:1;149-164:2;184-194:3", "", "175=New/1/1/2;303=New/1/1/2;322=diffCoverage/1/1/3,countBits/1/1/3;334=countBits/1/1/3;373=countBits/1/2/5,countBits/2/2/6;670=Now/1/2/2,Now/2/2/6;787=warmupRun/1/1/3", "124:21", "165:21,166:13")]
[assembly: GoPositionMap("internal/fuzz/mem.go", "mem.cs", "ADJ2tOr0goKUooKCyoKCuIIACAiSqqKCgqiSgqyygoKUggACEuKCgpQ=", "72-77:1", "", "", "", "76:21,77:13")]
[assembly: GoPositionMap("internal/fuzz/minimize.go", "minimize.cs", "AAkWgqai3KaCgoKUgoKmyoKClIKCgoKmgqa4goKCgpSCgoKmgoLcgoKCpoKClKbG", "", "", "", "23:27")]
[assembly: GoPositionMap("internal/fuzz/mutator.go", "mutator.cs", "ABMmgsaCpoKClKqipKSkugAKELiClLS0tLS0tLS0tLSCxrS0gpSClIKUgrSClIKUgpSCxMiCgoKCpoKUlJSCtoKUlJSCyoKCgoKmgpSUpoK2gpSUlILKgoKCpoKUgpSUgraClIKUlIK2goKUgpSUgraClIIACA4AFirSgoKClKaCgoKCAAkMgoIABBCCgpSC", "259-264:1", "", "", "", "313:21,314:13")]
[assembly: GoPositionMap("internal/fuzz/mutators_byteslice.go", "mutators_byteslice.cs", "AAoQsoKUgoKCgqrCgoKClIKCgpSqwoKUgoKClLiClKamppSCgqiygpSCgoKUgoKosoKUgoKosoKUuIKosoKUgoKClIKosoKUgoKClJSosoKUgoKUgoKCqLKClIKClIKCgqiygpSCgpSCgoKqwoKUgoKqwoKUgoKCqsKClIKCgqiygpQACBKCgpSCgoKClKiygpSCgoKClKiygpSCgoLKgoKUqLKClIKCgsqCgpSmgsqClIKCgoKCgg==")]
[assembly: GoPositionMap("internal/fuzz/pcg.go", "pcg.cs", "ADZYgoKCgpKClKaokoKCgpSCgoKCgoLGgoKmsoKmgoKokoKCqqKClAACENKCgoKCgoKCgqaokgAOFpI=")]
[assembly: GoPositionMap("internal/fuzz/queue.go", "queue.cs", "ABsugqaCgoKClIKCgpSCpoKClIKCpoKClIKCgoKmgoKUpoI=")]
[assembly: GoPositionMap("internal/fuzz/sys_windows.go", "sys_windows.cs", "ABUm4oKCygAHEIK4gu6CgqaCAA0UyLK4gpSCgqbKooKCgoKCgoIADAiygoKUooKmgoKCgoKUgoKUgoKClIKUpqaokqiS", "20-24:1", "", "36=Fd/1/1/1;86=UnmapViewOfFile/1/1/1,CloseHandle/1/1/2,Close/1/1/3", "", "29:21,30:13")]
[assembly: GoPositionMap("internal/fuzz/trace.go", "trace.cs", "AAowAAoCgoKUgoKClII=")]
[assembly: GoPositionMap("internal/fuzz/worker.go", "worker.cs", "AD1+goKClIKCAAsWkoKClIIAAhoADQSUgoK45oKClLiCgpQAChaUgKbIytyCgpSClJSmlNyUpsqClAAJFMiCytyCpoIABxIABRIACgKCgoK2yoKUgtwABxAACRSCpgAOFoIAAhYACgKClICCpICCgoKUlKakAAIaAA8CgpSCgpSCggAJFIKClJKCgoKUkqaCgoLcgoKCgpSCgqYABhgACwKClLaUpoKCggAIDoKSgqaCuKaCgsaCgoKCuIKmgoLIgoLIAAkgAAoCkoKUxoKUkoKC6ACEAa4CAAwCgoKCgoCCgpTIgpS0tLSmgIIAGDYAEAKCgoKUlIKQpIKCgqSCgpKClIKCpoKCgpSClKaygoKCgoKClJSClKaCgoKClIKClKaCpKSCgpSUgoKCgpSCgoKUggAKDNKCkJKCkJKCgpSCgoKC2oKCgoKCgoLugpS4AAUQAAkCgoKCgqiC3IKCgoKUgpTcwoKUtLSkgoKCgoKCgriUpoKUgpSUtMSkxqKCyqIAEiSCzAAIAoK4gIKC2oCCgqQADhS4AAsCgpSSgpSAkoKCgpSCgoKClIKCpoKCgoKCgqaCgpTKlLS0pJSmlIKCgsqCgoKmgoKCuIKCggAJCAALAoKUgoKUgoKCgpSClIKCgpSCgpSQkpSClISCgoKUgpSCgqaUgoLcprjo4oKCgoLaooKCgpQAEyCigpSYooKigqa0pA==", "386-389:1;426-429:1;491-499:1;492-494:1.1;715-715:1;724-727:2;741-743:3;744-759:4;805-805:1;807-807:2;855-858:1;881-912:2;996-996:1;1100-1100:1;1184-1187:1", "", "285=Sprintf/1/1/2;542=newMutator/1/1/11", "394:21,401:21,551:29", "792:21,793:13,805:21,806:13,902:21,903:13,907:21,908:13,1155:21,1156:13,1284:21,1285:13")]
// </GoSourcePositionMaps>

namespace go.@internal;

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
[GoMethodHost(typeof(fuzz_package.MalformedCorpusError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛMalformedCorpusError;
[GoMethodHost(typeof(fuzz_package.contextReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛcontextReader;
[GoMethodHost(typeof(fuzz_package.coordinator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛcoordinator;
[GoMethodHost(typeof(fuzz_package.crashError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛcrashError;
[GoMethodHost(typeof(fuzz_package.mutator)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛmutator;
[GoMethodHost(typeof(fuzz_package.noCopy)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛnoCopy;
[GoMethodHost(typeof(fuzz_package.pcgRand)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛpcgRand;
[GoMethodHost(typeof(fuzz_package.queue)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛqueue;
[GoMethodHost(typeof(fuzz_package.sharedMem)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛsharedMem;
[GoMethodHost(typeof(fuzz_package.worker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛworker;
[GoMethodHost(typeof(fuzz_package.workerClient)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛworkerClient;
[GoMethodHost(typeof(fuzz_package.workerServer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fuzzᴛworkerServer;
// </MethodHolders>
