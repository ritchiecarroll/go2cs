// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.sort_package;
global using static global::go.sort_internal_test_package;

// <ImportedTypeAliases>
global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;
global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.sort_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220696e747d", "intPairsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206e20696e743b20662066756e6328696e742920626f6f6c3b206920696e747d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20726573756c7420696e743b206920696e747d", "wrappertestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<ByName, sort_package.Interface>]
[assembly: GoImplement<ByWeight, sort_package.Interface>]
[assembly: GoImplement<adversaryTestingData, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<intPairs, sort_package.Interface>]
[assembly: GoImplement<multiSorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<myStructs, sort_package.Interface>]
[assembly: GoImplement<nonDeterministicTestingData, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<planetSorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<testingData, sort_package.Interface>(Pointer = true)]
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
[assembly: go.GoPositionMap("sort/example_keys_test.go", "example_keys_test.cs", "ABg2ksgACBSSqJKokqYADBCkhoaGqoKEgoSChII=", "66-68:1;69-71:2;72-74:3;75-77:4")]
[assembly: go.GoPositionMap("sort/example_multi_test.go", "example_multi_test.cs", "ABo4soKqwsySqJIAAhLihJKCgpa2/KYAEh7EhoaGgqiCloKEgoSChII=", "95-97:1;98-100:2;101-103:3;104-106:4")]
[assembly: go.GoPositionMap("sort/example_wrapper_test.go", "example_wrapper_test.cs", "AA0cgAAJEoCigAAIDIAACAyA5IIABxKCgoSCggACJgAPAoI=")]
[assembly: go.GoPositionMap("sort/search_test.go", "search_test.cs", "AA4cgroACAQAGDyCgoKCAAsKgoKCgpaWhAAjYLKSmIKCAAUQooKUpqaCgoKmgpKCgICSgpSCpoLMkvQACBqCgoLKgoKCgoKC5oKClIKUgoK4ooK+soKSgoLe2oKSgoSUlISCgpSClII=", "15-17:1;122-131:1;123-125:1.1;156-156:1;229-229:1;248-251:1", "", "29=f/1/12/13,f/2/12/14,f/3/12/15,f/4/12/16,f/5/12/17,f/6/12/18,f/7/12/19,f/8/12/20,f/9/12/21,f/10/12/22,f/11/12/23,f/12/12/24;180=SearchInts/1/1/5,SearchFloat64s/1/1/6,SearchStrings/1/1/7,Search/1/3/8,Search/2/3/9,Search/3/3/10")]
[assembly: go.GoPositionMap("sort/sort_slices_benchmark_test.go", "sort_slices_benchmark_test.cs", "ABIosoKCgpSmgoKClKaCgoKUpoKCgpSCyqKCgoKCuKKCgoKCuKKCgoKCuKKCgoKCvKKCgoKCgoKClJSmooKCgoK4ooKCgoK4ooKEgriigoSCAAwagKKAooCkgoKCgpSmgoKCgpaChIKCyqKCgoKCuKKCgoKCgg==", "175-175:1;194-194:1")]
[assembly: go.GoPositionMap("sort/sort_test.go", "sort_test.cs", "ABYokpKUgoKCgoKCuIKCgoKCgrqSgoSCloK4goKCgoKCuIKCgoKCuIKCgoKCuIKCgoKCuIKSlpKCuKKCgpSCgpSClIKCuKKCgoKCgoKCgpSCypSCgpSCgoKmooKCgoKogoKCgoIACBKCpIKClKSCgrjGgoCCuKiCtKSigoKClISCgoKCuKKCgoKUhIKCgpK4ooKCgpSEgoKCgriigoKCgpSCgriigoKCgpSCgriigoKCgpSCgriigoKCgpSCgriigoKClIKCgoKCuKKCgoKUgoKCgpK4ooKCgoKUgoK4ooKCgoKUgpK4ooKCgoKUgoIAGj6AooKCpIKClIKmgoKClKaigoKUgoKSgoKCgoKCgpSkpKSkgoKUgsqCgpSCtoK2gpSCtoKUgraCmKSiuJKCAAoYggAHEIKmgqaCAA4egKSCgpSEgpSCpoKogpKClqaCpoKSgoKUpoKCgoKUgoLKgoKCggAJEpCigKKAppKCupKSgoKCgpSClJSmopKClJaClIKUgoKClIKogoKClIKogpSCgoKUgrjEgoKClIKUguyClILogNKA5KKClIKCgoKCgoKCgpSUgoKCgoKUggAIDICigKKAooCigKKA", "53-53:1;97-99:1;100-100:2;194-198:1;236-236:1;334-334:1;360-360:1;523-523:1;527-527:1;531-531:1", "", "214=NewPCG/1/1/1,New/1/1/1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("sort_test")]
public static partial class sort_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestFind_tests {}
    internal partial struct adversaryTestingData {}
    internal partial struct au {}
    internal partial struct earthMass {}
    internal partial struct intPairs {}
    internal partial struct intPairsᴛ1 {}
    internal partial struct myStruct {}
    internal partial struct myStructs {}
    internal partial struct nonDeterministicTestingData {}
    internal partial struct planetSorter {}
    internal partial struct testingData {}
    internal partial struct testsᴛ1 {}
    internal partial struct wrappertestsᴛ1 {}
    public partial struct ByName {}
    public partial struct ByWeight {}
    public partial struct Change {}
    public partial struct Grams {}
    public partial struct Organ {}
    public partial struct Organs {}
    public partial struct Planet {}
    public partial struct multiSorter {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(go.math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.sort_package));
    }
}
