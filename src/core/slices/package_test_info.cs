// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.slices_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.slices_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2073205b5d696e743b2077616e74205b5d696e747d", "compactTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73205b5d696e743b20666e2066756e6328696e742920626f6f6c3b2077616e74205b5d696e747d", "deleteFuncTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73205b5d696e743b206920696e743b20616464205b5d696e743b2077616e74205b5d696e747d", "insertTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73205b5d696e743b206920696e743b206a20696e743b2077616e74205b5d696e747d", "deleteTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73205b5d696e743b207620696e743b2077616e7420696e747d", "indexTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7331205b5d666c6f617436343b207332205b5d666c6f617436343b2077616e7420696e747d", "compareFloatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7331205b5d666c6f617436343b207332205b5d666c6f617436343b2077616e74457175616c20626f6f6c3b2077616e74457175616c4e614e20626f6f6c7d", "equalFloatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7331205b5d696e743b207332205b5d696e743b2077616e7420626f6f6c7d", "equalIntTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7331205b5d696e743b207332205b5d696e743b2077616e7420696e747d", "compareIntTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("slices/iter_test.go", "iter_test.cs", "AA0cgoKCgpSSgoKClIKClILKgoKCgpSSgoKClIKClILKgoKCgpSCgoKClIKUgsqCgoLKlIKCgoK4goKCgsgABxCCgoKCgoLcgoKCgsqCgoKCuIKCgoK4opKCgpSEgoKUgqgABhSCgpSCAAoIggAlWLKSgoKWgpaCuoKCggALDIIACRaQksqEkoKUlpaAgsii", "143-143:1;167-175:1;168-174:1.1;232-253:1;269-269:1")]
[assembly: go.GoPositionMap("slices/slices_test.go", "slices_test.cs", "ABsoABw0AA4mooKAgraCgILckqiSgqiSpqKCgIK2goCCpICCuIKCgpSCloKCgpaGguiihoKCoAAJCAA5cAA0cIKCgpSUgoCCtoKAgriCgIK2goCC2oKCgpS4goKClJSCgIK2goCCuIKAgraCgIK4goKAgqaCgoCCpoaAgqaGgIIACggAG0CCgoCC2oLaooaCuIKCgIK4goCCpICC+KKGgpLKooKAgtqigoCCuIKAgqSAgqSAggALCAAaQIKCgIKkgoKAgriEkoKCgqaCyoKCgoKSkpKSgpSCgoKCgoIADhCCgoIADiSQkgAKCgAgSoKCgoCCAAsKACBKgoKCgILasoKAgraCAAwGgoKChAAMHJCSyoKChISUlIK4goKChIiUlIK4goKCgpSCgoKUgIKkgIIACggAIEqCgoKAgtqCspKCgoKCAAsMgoaCgoKCpoKCgpSCgsqCgoKAgriCgoKAgsiC8oKChISAgqaUlIK4ggALAoKChIKClJaAgqaUlIK4grKSgoKCgtyChoKCgoKmgoKClIKCyoKEgoKClIKogoKClIKCqICQkqSAkJKSgpSokrKAkqKSgriCgoKClIKUgoKUgviCgoKAgqaCgoCCpoKCgIKmgoKAgqao0oKC9oIAECaCgoKC7oKCgoIACgqCgoKEAAgUgrCSysYACwKCgoKClpKEgIKmgpaUuIIACwKCgoSyhICCppSUgriCgoKCkpKSkpKClIKCgoKCggAIEoKClJKEgoCCAAoIggAPOrKSgoKCppKCgoLegoKCgoKCgoKCpoKCuIKCgoKCgoKCgqaCgriCyJKCgoCCpoSCgIIACQiCABEqsoKClIKSlIKCkoKUAA0KhAAdRIKCkoKClKKClKKSgIIAEgyUABY2gILcAAoegIIADAyCAAcSkJL6goKClA==", "85-85:1;124-126:1;138-138:1;255-260:1;285-290:1;294-299:1;334-336:2;341-343:3;390-392:1;425-427:1;505-510:1;563-563:1;662-666:1;690-690:1;716-718:1;796-803:1;811-817:1;818-827:2;873-878:1;895-902:1;900-900:1.1;910-916:1;914-914:1.1;917-926:2;924-924:2.1;953-953:1;956-956:2;966-969:3;967-967:3.1;1047-1053:1;1084-1084:1;1188-1190:1;1191-1193:2;1199-1201:3;1202-1204:4;1211-1217:5;1218-1224:6;1313-1315:1;1370-1375:1;1371-1373:1.1;1451-1451:1", "", "57=NaN/1/2/12,NaN/2/2/13;228=NaN/1/13/15,NaN/2/13/16,NaN/3/13/20,NaN/4/13/21,NaN/5/13/25,NaN/6/13/26,NaN/7/13/30,NaN/8/13/35,NaN/9/13/36,NaN/10/13/41,NaN/11/13/46,NaN/12/13/50,NaN/13/13/51")]
[assembly: go.GoPositionMap("slices/sort_test.go", "sort_test.cs", "ABEkkpKUgoKCgoK4goKCgoK4goKCgoK4goKCgoK4ooKClIKClIKUgoIACRaSqJKCvKKSgoKCgpSCgqaCppSmopKClJaClIKUgoKClIKogoKClIKogpSCgoKUggAIEoIACAaChAAKHrKigoKWgoKWgoKWgoK63oKCgpaCgoK4goKClIK6goKEgoKWgoLKgoKEkJKWkJKWoJKWoJIACgiCgoKClpaEACNespKCgoKogoKCAAwOgoIABRSyooKCgqiChoKCAAwOgoIABxiyooKCgu6CgoaCgg==", "33-33:1;175-175:1;193-213:2;264-264:1;267-267:2;271-271:3;275-275:4;279-279:5;344-358:1;375-392:1;384-386:1.1;411-418:1;424-426:1", "", "416=NaN/1/1/5,Inf/1/1/6")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("slices_test")]
public static partial class slices_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("Large")] internal partial struct BenchmarkCompact_Large_Large {}
    [GoLocalName("Large")] internal partial struct BenchmarkEqualFunc_Large_Large {}
    [GoLocalName("Large")] internal partial struct BenchmarkIndexFunc_Large_Large {}
    [GoLocalName("Large")] internal partial struct BenchmarkIndex_Large_Large {}
    internal partial struct BenchmarkReplace_cases {}
    internal partial struct TestBinarySearchFloats_tests {}
    internal partial struct TestBinarySearchInts_tests {}
    internal partial struct TestBinarySearch_tests {}
    internal partial struct TestChunkPanics_type {}
    internal partial struct TestChunk_cases {}
    internal partial struct TestConcat_cases {}
    internal partial struct TestConcat_too_large_cases {}
    [GoLocalName("void")] internal partial struct TestConcat_too_large_void {}
    internal partial struct TestDeletePanics_type {}
    [GoLocalName("S")] internal partial struct TestInference_S {}
    internal partial struct TestInsertPanics_type {}
    internal partial struct TestMinMax_tests {}
    internal partial struct TestRepeatPanics_type {}
    internal partial struct TestRepeat_type {}
    internal partial struct TestRepeat_typeᴛ1 {}
    internal partial struct TestReplacePanics_type {}
    internal partial struct TestReplace_type {}
    internal partial struct compactTestsᴛ1 {}
    internal partial struct compareFloatTestsᴛ1 {}
    internal partial struct compareIntTestsᴛ1 {}
    internal partial struct deleteFuncTestsᴛ1 {}
    internal partial struct deleteTestsᴛ1 {}
    internal partial struct equalFloatTestsᴛ1 {}
    internal partial struct equalIntTestsᴛ1 {}
    internal partial struct indexTestsᴛ1 {}
    internal partial struct insertTestsᴛ1 {}
    internal partial struct intPair {}
    internal partial struct intPairs {}
    public partial struct S {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(go.math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.slices_package));
    }
}
