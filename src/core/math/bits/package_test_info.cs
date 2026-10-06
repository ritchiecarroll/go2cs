// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.math.bits_package;
global using static global::go.math.bits_internal_test_package;

// <ImportedTypeAliases>
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.math.bits_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6e6c7a20696e743b206e747a20696e743b20706f7020696e747d", "entryᴛ1")]
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
[assembly: go.GoPositionMap("math/bits/bits_test.go", "bits_test.cs", "AA8cgoKAgsiCgoKCgoKCgoKUgqiCgoKClIKogoKCgpSClIKCgrqCgoKClIKUgoKCABEkooKClKaigoKUpqKCgpSmooKClKaigoKUpoKCgoKCgoKCgpSCqIKCgpSCqIKCgpSClIKCgrqCgoKUgpSCgoIABxCigoKUpqKCgpSmooKClKaigoKUpqKCgpSmgoKCgpaCgpaCgsqCgoKCqIKCgqiCgoKUgoKCuoKCgpSCgoLcooKClKaigoKUpqKCgpSmooKClKaigoKUpoKEgoKCgoKUgoKWgoKCgpSCgpaCgoKClIKClIKCgoKClIKCqIKCgoKUgoKUgoKCgoKUgoLcooKClKaigoKUpqKCgpSmooKClKaigoKU5pSCqAATLIK4goKCgoKWgoKCgpaCgoKClIKCgoKCqIKClIKCgoKCyqKCgpSmooKClKaigoKUpqKCgpSmooKClOaCAAoagriCgoKCgpaCgoKClIKCgoKCqIKClIKCgoKCyqKCgpSmooKClKaigoKUpqKCgpSmgoKCgoKCgpSCgoKogoKCqIKCgpSCgoK6goKClIKCggAZHKKCgoKmAAwagoKCpoKCggANCoKCgoKmAAwagoKCABEIooKCgqYADBqCgoKmgoKCuKaUgoKUpoKClKaCgpSmgoKUpoKClKaCooKAgraippCSkJKQpoKCuKaUgoKUpoKClKaCgpSmgoKUpoKClKaCooKAgraippCSkJKQpoKCABIIooKCgqaCgoKmvoKCgqaCgoIADgiCgoKCpoKCgqYABBCCgoIAEgiigoKCpoKCgqYABBCCgoKmgoKCAAkStIKAgpKAgsaCotS0goCCkoCCxoKi1LSCgIKSgILGgqLUtIKAgpKAgsaCotS0goCCkoCCxoKi1LSCgIKSgILGgqKkpoKCgoKClLiUgoKCgoKUuKaCgoKCgpQACAiCAAsggoKUgoLcopKClKaikoKUpqKSgpSmooKCgoKCgoKCgpSmopKClKaikoKUpqKSgpSmooKCgoKCgoKCgpSmopKClKaikoKUpqKSgpSmopKClKaikoKUpqKSgpQACxqCgpSCgoKClJaCgoKClJaCgoKClA==", "716-721:1;741-741:2;742-742:3;743-743:4;744-744:5;750-755:1;777-782:1;802-802:2;803-803:3;804-804:4;805-805:5;813-819:1;820-826:2;827-833:3;834-840:4;841-847:5;850-857:6;851-855:6.1;860-860:7;861-861:8;862-862:9;875-881:1;882-888:2;889-895:3;896-902:4;903-909:5;912-919:6;913-917:6.1;922-922:7;923-923:8;924-924:9;934-939:1;940-945:2;959-959:3;960-960:4;961-961:5;962-962:6;967-972:1;973-978:2;995-1000:1;1001-1006:2;1021-1021:3;1022-1022:4;1023-1023:5;1024-1024:6;1035-1041:1;1048-1054:1;1061-1067:1;1074-1080:1;1087-1093:1;1100-1106:1")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("bits_test")]
public static partial class bits_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestAddSubUint32_type {}
    internal partial struct TestAddSubUint64_type {}
    internal partial struct TestAddSubUint_type {}
    internal partial struct TestMulDiv32_type {}
    internal partial struct TestMulDiv64_type {}
    internal partial struct TestMulDiv_type {}
    internal partial struct TestRem64Overflow_Rem64Tests {}
    internal partial struct TestReverseBytes_type {}
    internal partial struct TestReverse_type {}
    internal partial struct entryᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.math.bits_package));
    }
}
