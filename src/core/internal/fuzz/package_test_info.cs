// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.fuzz_package;

// <ImportedTypeAliases>
global using CorpusEntry = go.@internal.fuzz_package.CorpusEntryᴛ1;
global using execꓸError = go.os.exec_package.ΔError;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using syscallꓸHandle = go.syscall_package.ΔHandle;
global using syscallꓸSignal = go.syscall_package.ΔSignal;
global using syscallꓸSockaddr = go.syscall_package.ΔSockaddr;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.fuzz_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<mockRand, global::go.@internal.fuzz_package.mutatorRand>(Pointer = true)]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("internal/fuzz/encoding_test.go", "encoding_test.cs", "ACEcggDgAfgClKioAAoUspKCgoKUlIKUgoKWgoKUgoKCAAUSsoKCloKCkoKCAAUSsoKCloKCgpKCgtyCgoKCgoKUgoLKgoKCgoKClIKCyqKCgoKCgoKChIKEgoSCgpSClIKCgsqigoKCgoKCgoKChIKChIKClIKUgoLKooKChIKChIKClIKUgoI=", "202-215:1;224-249:2;264-269:1;285-290:1;334-352:1;367-382:1;390-405:1")]
[assembly: global::go.GoPositionMap("internal/fuzz/minimize_test.go", "minimize_test.cs", "AB4qggADFIKCgoKmgpQABxCCgpSClAAHEIKClIKUAAcQgoKUAAgQgoKCgqaClAAHEIKClAAHEIKCgpTegpKSgrqCgoKClIKUgIKkggAJErKWgoKCgoKUgpSAgg==", "31-43:1;49-58:2;64-73:3;79-85:4;91-103:5;109-115:6;121-128:7;136-158:8;139-141:8.1;166-168:1")]
[assembly: global::go.GoPositionMap("internal/fuzz/mutator_test.go", "mutator_test.cs", "AA4eooKAkoKEAAcQkoKElIKC2KSigoCSgoQABxCSgoSUgoLYpKKCgJKChAAPIoKigoLYpIKCgoKCgoKClII=", "17-17:1;29-39:2;45-45:1;57-67:2;73-73:1;95-100:2")]
[assembly: global::go.GoPositionMap("internal/fuzz/mutators_byteslice_test.go", "mutators_byteslice_test.cs", "ABImgoKCpoKCgqaCgoKmgoKCpoKmggAKBoIAceoBkoKClIKCggAKDIIAEzCykpKSgoKC", "169-179:1;209-219:1;211-217:1.1")]
[assembly: global::go.GoPositionMap("internal/fuzz/queue_test.go", "queue_test.cs", "AAoSpJKAgqSAgriCgoKAgqSAgpKC3IKCgoKAgpKClKSCgIK2goKAgg==")]
[assembly: global::go.GoPositionMap("internal/fuzz/worker_test.go", "worker_test.cs", "ABcslIKCgoKU1sKClIKAkoS6goKUkoCCuIKChISCgoKChLSowoKUgoKCgIIACA7CgpSCgoKCgriCgpSClIKUAAMQwoK4gpSCgoKCgoKUkoCCtoCCpJKAgramooKSgoCCxNSigpaokoKUgoCCtoSCgoKCgoKCgoKigoKCgpSUgoLY", "38-38:1;42-42:2;50-54:3;137-141:1;145-149:2;156-156:1;175-179:1;191-204:2;193-199:2.1", "", "142=TypeOf/1/1/1")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("fuzz")]
public static partial class fuzz_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("testcase")] partial struct TestMinimizeInput_testcase {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(global::go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(global::go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.fuzz_package));
    }
}
