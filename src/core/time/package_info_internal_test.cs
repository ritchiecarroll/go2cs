// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using Δtesting = go.testing_package;
using Δtime = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.time_package;
using static go.time_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4e616d6520737472696e673b20546573742066756e632874696d652e74657374696e6754297d", "InternalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6279746573205b5d627974653b2077616e7420737472696e677d", "invalidEncodingTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b636f756e7420696e743b206465736320737472696e673b20666e2066756e6328297d", "mallocTestᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b206d2074696d652e4475726174696f6e3b2077616e742074696d652e4475726174696f6e7d", "durationTruncateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e7420666c6f617436347d", "secDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e7420696e7436347d", "nsDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b642074696d652e4475726174696f6e3b2077616e742074696d652e4475726174696f6e7d", "durationAbsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6461746520737472696e673b206f6b20626f6f6c7d", "dayOutOfRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065787065637420737472696e677d", "parseDurationErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2077616e742074696d652e4475726174696f6e7d", "parseDurationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e2074696d652e54696d653b2077616e7420737472696e677d", "goStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d6f6e6f20696e7436343b2077616e7420737472696e677d", "monotonicStringTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20662066756e632874312074696d652e54696d652c2074322074696d652e54696d652920626f6f6c7d", "defaultLocTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73747220737472696e673b20642074696d652e4475726174696f6e7d", "durationTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73756d20666c6f617436343b206d61782074696d652e4475726174696f6e3b20636f756e7420696e7436343b205f205b355d696e7436347d", "BenchmarkParallelTimerLatency_type")]
[assembly: GoDynamicTypeLift("7374727563747b742074696d652e54696d653b20642074696d652e4475726174696f6e7d", "truncateRoundTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b742074696d652e54696d653b20752074696d652e54696d653b20642074696d652e4475726174696f6e7d", "subTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d652074696d652e54696d653b206a736f6e20737472696e677d", "jsonTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b74696d652074696d652e54696d653b2077616e7420737472696e677d", "notEncodableTimesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b76616c756520737472696e673b206f6b20626f6f6c7d", "monthOutOfRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b76616c756520737472696e673b2077616e7420696e747d", "longFractionalDigitsTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7965617220696e743b206d6f6e746820696e743b2064617920696e743b20686f757220696e743b206d696e20696e743b2073656320696e743b206e73656320696e743b207a202a74696d652e4c6f636174696f6e3b20756e697820696e7436347d", "dateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7965617220696e743b206d6f6e746820696e743b20646920696e747d", "daysInTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b796561727320696e743b206d6f6e74687320696e743b206461797320696e747d", "addDateTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7a6f6e654e616d6520737472696e673b2066696c654e616d6520737472696e673b20646174652066756e63282a74696d652e4c6f636174696f6e292074696d652e54696d653b2077616e744e616d6520737472696e673b2077616e744f666673657420696e747d", "slimTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<RuleKind, global::go.time_package.ruleKind>(Inverted = false, ValueType = "nint")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("time/abs_test.go", "abs_test.cs", "AB8uAAgYgoaCgoKCgoKmgIK2goKUgIKC2oKCgoKCgoKCgoCCtoCCgtqCgoKClIKCgoKCgoKCgoKmgIK4goKCpoCCuICCgqSCgoKCgsqChoKCgoKCgoCCtoKCyoKGgoKCgoKClIKCgoCC/oKCgoI=", "36-38:1;85-88:1;133-135:1;154-156:1")]
[assembly: go.GoPositionMap("time/export_test.go", "export_test.cs", "AAoWgoKmgoKmgqaCgriCkoKCkoKSkoKCgpaCABEogoLuuAA3bpSioqKi")]
[assembly: go.GoPositionMap("time/export_windows_test.go", "export_windows_test.cs", "AAoOgoKAtoKCgLaC", "9-9:1;14-14:1")]
[assembly: go.GoPositionMap("time/internal_test.go", "internal_test.cs", "AAoOlNbcgoKClIK2lIKCgriSlK4ACgiC7qK2koQ=", "31-33:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("time")]
public static partial class time_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsyscallꓸwindowsꓸregistry() => builtin.initPackage(typeof(@internal.syscall.windows.registry_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() => builtin.initPackage(typeof(go.testing.quick_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸtimeꓸtzdata() => builtin.initPackage(typeof(go.time.tzdata_package));
    // </ImportInitializers>
}
