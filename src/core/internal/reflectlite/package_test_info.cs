// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.reflectlite_package;
global using static global::go.@internal.reflectlite_internal_test_package;

// <ImportedTypeAliases>
global using Kind = go.@internal.abi_package.ΔKind;
global using abiꓸArrayType = go.@internal.abi_package.ΔArrayType;
global using abiꓸChanDir = go.@internal.abi_package.ΔChanDir;
global using abiꓸFuncType = go.@internal.abi_package.ΔFuncType;
global using abiꓸInterfaceType = go.@internal.abi_package.ΔInterfaceType;
global using abiꓸKind = go.@internal.abi_package.ΔKind;
global using abiꓸName = go.@internal.abi_package.ΔName;
global using abiꓸStructType = go.@internal.abi_package.ΔStructType;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;
global using reflectliteꓸType = go.@internal.reflectlite_package.ΔType;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.reflectlite_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b4628297d", "Δtypeᴛ30")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743820227265666c6563743a5c226869205c5c78303074686572655c5c745c5c6e5c5c5c225c5c5c5c5c22227d", "typeᴛ27_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e743820227265666c6563743a5c2268692074686572655c22227d", "typeᴛ26_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e7433327d", "typeᴛ21_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e7433327d", "typeᴛ22_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e7433327d", "typeᴛ23_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e7433327d", "typeᴛ24_x")]
[assembly: GoDynamicTypeLift("7374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e74383b206620696e7433327d", "typeᴛ25_x")]
[assembly: GoDynamicTypeLift("7374727563747b63206368616e202a696e7433323b206420666c6f617433327d", "typeᴛ18_x")]
[assembly: GoDynamicTypeLift("7374727563747b632066756e63286368616e202a696e7465726e616c2f7265666c6563746c6974655f746573742e696e74656765722c202a696e7438297d", "typeᴛ20_x")]
[assembly: GoDynamicTypeLift("7374727563747b662066756e632861726773202e2e2e696e74297d", "typeᴛ28_x")]
[assembly: GoDynamicTypeLift("7374727563747b696e7433323b20696e7436347d", "typeᴛ29_x")]
[assembly: GoDynamicTypeLift("7374727563747b6f726967205b5d696e743b206578747261205b5d696e747d", "appendTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78202a2a696e74387d", "Δtypeᴛ12")]
[assembly: GoDynamicTypeLift("7374727563747b78202a2a696e7465726e616c2f7265666c6563746c6974655f746573742e696e74656765727d", "Δtypeᴛ13")]
[assembly: GoDynamicTypeLift("7374727563747b78205b33325d696e7433327d", "Δtypeᴛ14")]
[assembly: GoDynamicTypeLift("7374727563747b78205b5d696e74387d", "Δtypeᴛ15")]
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207420616e793b206220626f6f6c7d", "implementsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b78206368616e3c2d20737472696e677d", "Δtypeᴛ17")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617433327d", "Δtypeᴛ10")]
[assembly: GoDynamicTypeLift("7374727563747b7820666c6f617436347d", "Δtypeᴛ11")]
[assembly: GoDynamicTypeLift("7374727563747b782066756e63286120696e74382c206220696e743332297d", "Δtypeᴛ19")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7431367d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7433327d", "Δtypeᴛ3")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e7436347d", "Δtypeᴛ4")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e74387d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e747d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b78206d61705b737472696e675d696e7433327d", "Δtypeᴛ16")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e743820227265666c6563743a5c226869205c5c78303074686572655c5c745c5c6e5c5c5c225c5c5c5c5c22227d7d", "Δtypeᴛ27")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e743820227265666c6563743a5c2268692074686572655c22227d7d", "Δtypeᴛ26")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e7433327d7d", "Δtypeᴛ21")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e7433327d7d", "Δtypeᴛ22")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e7433327d7d", "Δtypeᴛ23")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e7433327d7d", "Δtypeᴛ24")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b6120696e74383b206220696e74383b206320696e74383b206420696e74383b206520696e74383b206620696e7433327d7d", "Δtypeᴛ25")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b63206368616e202a696e7433323b206420666c6f617433327d7d", "Δtypeᴛ18")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b632066756e63286368616e202a696e7465726e616c2f7265666c6563746c6974655f746573742e696e74656765722c202a696e7438297d7d", "Δtypeᴛ20")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b662066756e632861726773202e2e2e696e74297d7d", "Δtypeᴛ28")]
[assembly: GoDynamicTypeLift("7374727563747b78207374727563747b696e7433323b20696e7436347d7d", "Δtypeᴛ29")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7431367d", "Δtypeᴛ7")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7433327d", "Δtypeᴛ8")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e7436347d", "Δtypeᴛ9")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e74387d", "Δtypeᴛ6")]
[assembly: GoDynamicTypeLift("7374727563747b782075696e747d", "Δtypeᴛ5")]
[assembly: GoTypeAlias("Loopy", "object")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Tint2", "go.@internal.reflectlite_test_package.Tint")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<mapError, error>(Pointer = true)]
[assembly: GoImplement<mapError, error>]
[assembly: GoImplement<visitor, go.go.ast_package.Visitor>]
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
[assembly: global::go.GoPositionMap("internal/reflectlite/all_test.go", "all_test.cs", "ABkmgqaCABAggoKCAMIBCAA3+gEAJWCCgoK4goKCuIKC6IKCgpSkpKSkpKSkpKSkpKSkpKSkgoIAMAqCACWAAbKSgoKCgpSUgIIAChIADBqCgoKCyoKSkoKCgriCkoKCggAICIKChIiCggAKBqKGgoKUgoSCgIL4gpKCgpTmzoKClIKCpuaCgoKGggAHEIKCgoKCABQogAAUFoKChIL2ADiEAZSAgqSCgoKUgoLKgoKCuIKCggArCKYACBKCgoKYloKChIaCgoSGgoKEhoKChIaCgoSGgoKssoKU1oKCgIIACBKSqKSokoKoutKCgoKClgAPFKIAHkSCgIIACQqCgpSClIKCgpSCuIKCgoSCgoKmgoKEgoKCABwKgoCSgoCUAAAykpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCmgpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpCSkJKQkpC2ooKCpqL+goKCgoKCAAkKhIaClIKCABQcAA4igoKCgIIACRTSgoaSkqIAFiqCgoKCgpaCgoKC", "350-363:1;441-441:1;798-801:1;808-817:1;818-827:2;822-822:2.1;831-831:1;833-833:2;861-861:3;862-862:4;863-863:5;864-864:6;865-865:7;866-866:8;867-867:9;868-868:10;869-869:11;870-870:12;871-871:13;872-872:14;873-873:15;874-874:16;875-875:17;876-876:18;877-877:19;878-878:20;879-879:21;880-880:22;881-881:23;882-882:24;883-883:25;884-884:26;885-885:27;889-889:28;890-890:29;891-891:30;892-892:31;893-893:32;894-894:33;895-895:34;896-896:35;897-897:36;898-898:37;899-899:38;900-900:39;901-901:40;902-902:41;903-903:42;904-904:43;905-905:44;906-906:45;907-907:46;908-908:47;909-909:48;910-910:49;911-911:50;912-912:51;913-913:52;917-921:1;1003-1005:1")]
[assembly: global::go.GoPositionMap("internal/reflectlite/reflect_mirror_test.go", "reflect_mirror_test.cs", "ABoqAA4egoKEpIKCgqamgpSCgIKCgoKClOqmgoSGgoKWhIIADAiUgoKApqaChL6SgrKCoqSEgpaCgoKUgoCC", "73-75:1;110-113:1")]
[assembly: global::go.GoPositionMap("internal/reflectlite/set_test.go", "set_test.cs", "ABIghqKCgoKCAAkIABAogKKAogANFICkkpSigoKCgILqAA4kooKCgoCC")]
[assembly: global::go.GoPositionMap("internal/reflectlite/tostring_test.go", "tostring_test.cs", "ABAmovaCgoKUgpSkpKSCpKSClLaCgoKUlIKkgoKCgoKUlIKkgoKCgqSCpIKCgoKCgpSUgqSkpA==")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("reflectlite_test")]
public static partial class reflectlite_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface notASTExpr {}
    internal partial struct TestBigUnnamedStruct_b {}
    [GoLocalName("Embed")] internal partial struct TestCanSetField_Embed {}
    [GoLocalName("S1")] internal partial struct TestCanSetField_S1 {}
    [GoLocalName("S2")] internal partial struct TestCanSetField_S2 {}
    [GoLocalName("S3")] internal partial struct TestCanSetField_S3 {}
    [GoLocalName("S4")] internal partial struct TestCanSetField_S4 {}
    [GoLocalName("embed")] internal partial struct TestCanSetField_embed {}
    [GoLocalName("testCase")] internal partial struct TestCanSetField_testCase {}
    internal partial struct TestCanSetField_tests {}
    internal partial struct TestImportPath_tests {}
    internal partial struct TestInterfaceValue_inter {}
    [GoLocalName("T")] internal partial struct TestInvalid_T {}
    internal partial struct TestIsNil_doNil {}
    internal partial struct TestIsNil_doNilᴛ1 {}
    internal partial struct TestIsNil_doNilᴛ2 {}
    internal partial struct TestIsNil_doNilᴛ3 {}
    internal partial struct TestIsNil_doNilᴛ4 {}
    internal partial struct TestIsNil_doNilᴛ5 {}
    internal partial struct TestIsNil_doNilᴛ6 {}
    internal partial struct TestIsNil_fi {}
    internal partial struct TestIsNil_mi {}
    internal partial struct TestIsNil_si {}
    internal partial struct TestMirrorWithReflect_type {}
    [GoLocalName("T")] internal partial struct TestSetPanic_T {}
    [GoLocalName("T2")] internal partial struct TestSetPanic_T2 {}
    [GoLocalName("t0")] internal partial struct TestSetPanic_t0 {}
    [GoLocalName("t1")] internal partial struct TestSetPanic_t1 {}
    internal partial struct TestUnaddressableField_localBuffer {}
    internal partial struct appendTestsᴛ1 {}
    internal partial struct big {}
    internal partial struct implementsTestsᴛ1 {}
    internal partial struct integer {}
    internal partial struct mapError {}
    internal partial struct nameTest {}
    internal partial struct notAnExpr {}
    internal partial struct pair {}
    internal partial struct self {}
    internal partial struct typeᴛ18_x {}
    internal partial struct typeᴛ20_x {}
    internal partial struct typeᴛ21_x {}
    internal partial struct typeᴛ22_x {}
    internal partial struct typeᴛ23_x {}
    internal partial struct typeᴛ24_x {}
    internal partial struct typeᴛ25_x {}
    internal partial struct typeᴛ26_x {}
    internal partial struct typeᴛ27_x {}
    internal partial struct typeᴛ28_x {}
    internal partial struct typeᴛ29_x {}
    internal partial struct visitor {}
    public partial class IntPtr {}
    public partial class IntPtr1 {}
    public partial class Loop {}
    public partial interface Δtypeᴛ30 {}
    public partial struct A {}
    public partial struct B<T> {}
    public partial struct Basic {}
    public partial struct Ch {}
    public partial struct D1 {}
    public partial struct D2 {}
    public partial struct DeepEqualTest {}
    public partial struct NotBasic {}
    public partial struct Point {}
    public partial struct S {}
    public partial struct T {}
    public partial struct Talias1 {}
    public partial struct Talias2 {}
    public partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678 {}
    public partial struct Tint {}
    public partial struct Δtype {}
    public partial struct Δtypeᴛ1 {}
    public partial struct Δtypeᴛ10 {}
    public partial struct Δtypeᴛ11 {}
    public partial struct Δtypeᴛ12 {}
    public partial struct Δtypeᴛ13 {}
    [GoValueClone("x")] public partial struct Δtypeᴛ14 {}
    public partial struct Δtypeᴛ15 {}
    public partial struct Δtypeᴛ16 {}
    public partial struct Δtypeᴛ17 {}
    public partial struct Δtypeᴛ18 {}
    public partial struct Δtypeᴛ19 {}
    public partial struct Δtypeᴛ2 {}
    public partial struct Δtypeᴛ20 {}
    public partial struct Δtypeᴛ21 {}
    public partial struct Δtypeᴛ22 {}
    public partial struct Δtypeᴛ23 {}
    public partial struct Δtypeᴛ24 {}
    public partial struct Δtypeᴛ25 {}
    public partial struct Δtypeᴛ26 {}
    public partial struct Δtypeᴛ27 {}
    public partial struct Δtypeᴛ28 {}
    public partial struct Δtypeᴛ29 {}
    public partial struct Δtypeᴛ3 {}
    public partial struct Δtypeᴛ4 {}
    public partial struct Δtypeᴛ5 {}
    public partial struct Δtypeᴛ6 {}
    public partial struct Δtypeᴛ7 {}
    public partial struct Δtypeᴛ8 {}
    public partial struct Δtypeᴛ9 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(global::go.@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() => builtin.initPackage(typeof(global::go.@internal.reflectlite_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(global::go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.reflectlite_package));
    }
}
