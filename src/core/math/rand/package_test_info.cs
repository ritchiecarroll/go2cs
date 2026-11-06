// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.math.rand_package;
global using static global::go.math.rand_internal_test_package;

// <ImportedTypeAliases>
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static global::go.math.rand_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<go.math.rand_package.Rand, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(go.os.exec_package.Cmd))]
[assembly: GoFieldAccess(typeof(statsResults))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("default_test.cs", 184, 5, 5)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("math/rand/auto_test.go", "auto_test.cs", "AAsg2LKC7oKCgoKCgoI=")]
[assembly: GoPositionMap("math/rand/default_test.go", "default_test.cs", "ACAoyIKmgoCCgraUkoKCgoKCgpSCgpSCAAUUwoKCpoKCpoIAChamgoKigqKkgoKygqLogoKygqL6goKCooKipIKCsoKi1raSgqaCgoKU", "37-52:1;88-91:1;95-98:2;104-107:3;115-118:4;122-125:5;131-134:6")]
[assembly: GoPositionMap("math/rand/race_test.go", "race_test.cs", "AA4ewpiCgoKCooKCgoKCgoKCgoKCgoKClIKClLTE", "24-47:1")]
[assembly: GoPositionMap("math/rand/rand_test.go", "rand_test.cs", "AB420gAMEoKCgpSmmMKCgoKUgoKClKaCgpKCgpSCgqaCgoKCgriCgoKCgoKClJQAAxCygoKClKamgoKmpqaqkoK4goKCgoKUgoKCgoIABhaygoKClKamgpSCgqampqqSgriCgoKCggAFFPKCvIKClIKCgoKCgoKCgoKCgpSmwoK8goKUgoKCgoKCgoKCgoKClKyygoKUlIKCpqyygoKUlIKCpqaCgoCCpICCpICCyIKCgIKkgIKkgIIACgiCpNqU1pS4gqaCgoKCyoKCgoKClIKo/KaCgqamgqaCgsqCgoKCgpSCuIKCgoKClIKCgoKUgriCgoKCgpSCgoKClIK4lIKSgM7EgoKCgsqCgoKClPiSgoKClJKkgoK4ggANEoKUkJK4stqCgpSChJKCgqaCgoKClIK4goKCgoIAEw6mgoKCgoKCgsqCgoKCgoKCpoKCgoKCgs6ygriCgoLKooKCuKKCgriigoK4ooKCuKKCgriigoK4ooKCuKKCgriigoKCgpSQzMKCgoKC3KKCgoKCuKKCgoKCuKKCgoKCuIKCgoKCooKCtKQ=", "456-456:1;491-555:1;504-504:1.1;505-505:1.2;506-506:1.3;507-514:1.4;512-512:1.4.1;518-553:1.5;562-571:1;574-583:2;584-592:3;604-608:1;674-674:1;683-687:1;723-728:1")]
[assembly: GoPositionMap("math/rand/regress_test.go", "regress_test.cs", "ABYs1KKCgoKClIKCgoKUgoKCgoKUgoKCgoKClLaCgpSCgoKClIKUtra2goK2gqaCgoKUgpSCgoKUgoKUlJSCgpSCpqaCuA==")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("rand_test")]
public static partial class rand_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(go.testing.iotest_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.math.rand_package));
    }
}
