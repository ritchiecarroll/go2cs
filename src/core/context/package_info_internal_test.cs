// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.context_package;
using static go.context_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "TestDeadlineExceededSupportsTimeout_type")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<myCtx, global::go.context_package.Context>(Pointer = true)]
[assembly: GoImplement<myCtx, global::go.context_package.Context>(Promoted = true)]
[assembly: GoImplement<myDoneCtx, global::go.context_package.Context>(Pointer = true)]
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
[assembly: go.GoPositionMap("context/context_test.go", "context_test.cs", "ACZMgoIACgbqgoKSgoKSkpTUtLS06oKCgoKCgoCCgraCpoSAgqSAgqSAgqaEgoKUloLWpICCtoKCgpaC1qSAgsSkooKSgoS0tNiCgoCCpoKCgpSWhIKCgpSo1qSAgrik1oCC1gALBIKCgIK4goKCgoKEgoKCgoKEgoKSgoIAFhaCgqSigoKCgoKmgoKCgpSWgoKEgoKEgpSCkoSCgpKEgpKEgpKEgpKEgoSCkpaCgoKSog==", "54-54:1;106-115:2;194-198:1;216-216:2;236-242:1;243-250:2")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("context")]
public static partial class context_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
