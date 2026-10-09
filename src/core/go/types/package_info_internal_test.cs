// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.types_package;
using static go.go.types_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("696e746572666163657b54797065506172616d732829202a676f2f74797065732e54797065506172616d4c6973747d", "TestInstanceInfo_typeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b2073726320737472696e673b2073696720737472696e677d", "builtinCallsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73726320737472696e673b206f626a20737472696e673b2077616e7420737472696e673b20616c69617320626f6f6c7d", "testObjectsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/types/context_test.go", "context_test.cs", "ABEcooIACRzSlIKUlIKUlIKClpaCgILKgILKgIK4gII=")]
[assembly: global::go.GoPositionMap("go/types/errors_test.go", "errors_test.cs", "ABAYgoKCgIKmgoKAgqaCgoCCAAgIggAGEoKC")]
[assembly: global::go.GoPositionMap("go/types/sizeof_test.go", "sizeof_test.cs", "ABEakoQAHUiCgoKClII=")]
[assembly: global::go.GoPositionMap("go/types/termlist_test.go", "termlist_test.cs", "AA0gkoKCgoKUpoKCuIIAChaAgtqCAAgSgoKCyoIACRSCgoIACAqCAA0ggoKCAAgKggAPJoKCgoIACAqCABEqgoKCggAJCoIAChyCgoKCAAkKggAMIIKCgoIACQqCABY0goKCggAJCoIAESqCgoKC")]
[assembly: global::go.GoPositionMap("go/types/token_test.go", "token_test.cs", "AA4eAA0chJKCuJSCgoKCgg==")]
[assembly: global::go.GoPositionMap("go/types/typeset_test.go", "typeset_test.cs", "ABEcgoLoggAXOIKCgoKYkoKCqIKClIKCqIKC")]
[assembly: global::go.GoPositionMap("go/types/typeterm_test.go", "typeterm_test.cs", "AA8egoK2AAkUgoKAgtqCgoKUpoKCgpSmggAPIIKCgoKAgraCgILaogAfQoKCgoKCgILaggARJIKCgoKAgraCgILaggAJFIKCgoKAgtqCABImgoKCgoCC2oIACxiCgoKCgIK2goCC")]
[assembly: global::go.GoPositionMap("go/types/util_test.go", "util_test.cs", "AA8igKSgooA=")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("types")]
public static partial class types_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuild() => builtin.initPackage(typeof(global::go.go.build_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸformat() => builtin.initPackage(typeof(global::go.go.format_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸimporter() => builtin.initPackage(typeof(global::go.go.importer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtypes() => builtin.initPackage(typeof(global::go.go.types_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbuildcfg() => builtin.initPackage(typeof(global::go.@internal.buildcfg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸdiff() => builtin.initPackage(typeof(global::go.@internal.diff_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(global::go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(global::go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtypesꓸerrors() => builtin.initPackage(typeof(global::go.@internal.types.errors_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
