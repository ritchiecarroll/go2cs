// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.@internal.trace_package;
using static go.@internal.trace_internal_test_package;

// <ExportedTypeAliases>
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
[assembly: go.GoPositionMap("internal/trace/batchcursor_test.go", "batchcursor_test.cs", "AA4egpaCgoKCgqiCgoKCpoKCqIKCgoKogoKCgqiCgoKCqIKCgqaCgoK6gpaCgoKCgpSCuIKmgoKCgoKUlIKmgoSCgpSCpoI=")]
[assembly: go.GoPositionMap("internal/trace/event_test.go", "event_test.cs", "AAoStJSSlJKUkpSSlJKUkpSS6KKCgIK2og==", "13-15:1;16-18:2;19-21:3;22-24:4;25-27:5;28-30:6;31-33:7;37-41:1")]
[assembly: go.GoPositionMap("internal/trace/export_test.go", "export_test.cs", "AAoO")]
[assembly: go.GoPositionMap("internal/trace/mud_test.go", "mud_test.cs", "AA0apoKCgoKCgpSCgpaCgpSCqIKCgoKClILcpoKCgoKCgoKCgoKChIKClIKmgpSClIIABhSygpSCgg==")]
[assembly: go.GoPositionMap("internal/trace/order_test.go", "order_test.cs", "ABESgoKCgpSCgIKktoCCpICCtoKCgoI=", "11-28:1")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("trace")]
public static partial class trace_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtrace() => builtin.initPackage(typeof(go.@internal.trace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸraw() => builtin.initPackage(typeof(go.@internal.trace.raw_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸtesttrace() => builtin.initPackage(typeof(go.@internal.trace.testtrace_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtraceꓸversion() => builtin.initPackage(typeof(go.@internal.trace.version_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
