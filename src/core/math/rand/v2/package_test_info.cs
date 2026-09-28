// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.math.rand.rand_package;
global using static global::go.math.rand.rand_internal_test_package;

// <ImportedTypeAliases>
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.math.rand.rand_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.math.rand.rand_package.ChaCha8, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.text.tabwriter_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<statsResults, ж<statsResults>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("math/rand/v2/auto_test.go", "auto_test.cs", "AA0g2LKC3oKCgoKCgoI=")]
[assembly: global::go.GoPositionMap("math/rand/v2/chacha8_test.go", "chacha8_test.cs", "ABYigoKCgIK4goKAgtqCgoSCgIKkpIKAgqaChIKAgqSCgIKmgoSAgqSmgoKCgoKUgpSAgraCgpSCgoCCpKSUgILIooKCuKKCgoKCuIKCgoKClIKWgoKCgpSCloKAgqSAgtqCgoKCgpSCloKCgoKUgpaCgIKkuKKCgoKUpqKCgoKCgoKU")]
[assembly: global::go.GoPositionMap("math/rand/v2/example_test.go", "example_test.cs", "ABUmogAVLAAPCviWgpKSqIKWlrqCgqiCgpYABR4ADAKCAAMQxJbWgoKSlKaCgpSigpSCuIKCgg==", "56-58:1;119-121:1;129-132:1")]
[assembly: global::go.GoPositionMap("math/rand/v2/pcg_test.go", "pcg_test.cs", "AA0YooKCgpQACAaCgryCgoKWgoKCloKAgqSCloKCgriCggAVLoKAgg==")]
[assembly: global::go.GoPositionMap("math/rand/v2/race_test.go", "race_test.cs", "AAwewpiCgoKCooKCgoKCgoKCgoKCgoKm", "24-42:1")]
[assembly: global::go.GoPositionMap("math/rand/v2/rand_test.go", "rand_test.cs", "ACxEgoKSlM7CgoKClIKCgpSmgoKSgoKUgoKmgoKCgoK4goKCgoKCgpSUAAMQsoKCgpSmloKClpaWqpKCuIKCgoKClIKCgoKCAAYWsoKCgpSmlpKUgoKWlpaqkoK4goKCgoIABRTygqyCgoSCgoKCgoKCgoKCgoKUpsKCrIKChIKCgoKCgoKCgoKCgpSssoKClJSCgqassoKClJSCgqamgoKAgqSAgqSAgsiCgoCCpICCpICC+IKUqtTWlLiCloKCgoLKlIKSgM7EgoKCgsqCgoKClPiSgoKClJKkgoKoggAKEIKUkJKostqCgpSChJKCgqaCgoKClIKokoKCgoIAChaCpqKCgoKUpqKCgpSmgoKCgpS4ooKClKaCgoKClLiigoKClMqigpSmooKCgpSmooKCgpSmooKCgoKUpqKCgoKClKaigoKCgpSmooKCgoKUpqKCgoKClKaigoKCgpSmooKCgoKUpqKCgoKClKaigoKCgpSmooKCgoKUpqKCgoKClKaigoKCgpSmooKCgpSmooKCgpSmooKCgpSmooKCgpSmooKCgpSoooKCgpSmooKCgoKClJCSlKrCgoKCgtyCgoKCgqKCguimgoKCgg==", "365-365:1;400-463:1;413-413:1.1;414-414:1.2;415-422:1.3;420-420:1.3.1;426-461:1.4;493-499:1;511-517:1;739-739:1;750-754:1;763-768:1")]
[assembly: global::go.GoPositionMap("math/rand/v2/regress_test.go", "regress_test.cs", "ACM6ooKCgoKEgoKCgpSCgpSCgoKCgoKUgoKCgoKqgoKUgriCgpSClLaClIKCgoKUgpSmpqamAAJbAAJggpaCgoKUgpSCgoKUpJSUpJSCgpSClIKmtoL4woKWgpKWgoKUkpSigoKClIKCloKCgoKoloIABRoACgKCgoKClIKClIKClIKCgpSAgg==", "161-163:1;172-177:2")]
// </GoSourcePositionMaps>

namespace go.math.rand;

[GoPackage("rand_test")]
public static partial class rand_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestUniformFactorial_tests {}
    internal partial struct statsResults {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸhex() => builtin.initPackage(typeof(encoding.hex_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸformat() => builtin.initPackage(typeof(global::go.go.format_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(global::go.math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(global::go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtabwriter() => builtin.initPackage(typeof(text.tabwriter_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.math.rand.rand_package));
    }
}
