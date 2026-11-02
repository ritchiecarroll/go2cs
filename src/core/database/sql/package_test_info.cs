// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.database.sql_package;
global using static global::go.database.sql_internal_test_package;

// <ImportedTypeAliases>
global using driverꓸRowsAffected = go.database.sql.driver_package.ΔRowsAffected;
global using driverꓸValue = object;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
// </ImportedTypeAliases>

using go;
using static global::go.database.sql_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("64617461626173652f73716c2e646561646c696e6572", "waitCondition_deadliner")]
[assembly: GoDynamicTypeLift("696e746572666163657b446561646c696e652829202874696d652e54696d652c20626f6f6c297d", "waitCondition_deadliner")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b20666e2066756e632829206572726f727d", "hookOpenErrᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b20666e2066756e63282a64617461626173652f73716c2e66616b65436f6e6e2c206572726f72297d", "hookPostCloseConnᴛ1")]
[assembly: GoTypeAlias("Conn", "ΔConn")]
[assembly: GoTypeAlias("Stmt", "ΔStmt")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<global::go.database.sql_package.dsnConnector, go.database.sql.driver_package.Connector>]
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.DB))]
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.Rows))]
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.Tx))]
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.driverConn))]
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.ΔConn))]
[assembly: GoFieldAccess(typeof(global::go.database.sql_package.ΔStmt))]
[assembly: GoFieldAccess(typeof(time_package.Ticker))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("database/sql/example_cli_test.go", "example_cli_test.cs", "ACUmooKClIKUgpSmgqaUlIKClIKkgpSigqaUoqjCgqSAgsQACAjCgqSCgoKUog==", "51-54:1", "", "", "60:21,68:21,95:21,120:21")]
[assembly: GoPositionMap("database/sql/example_test.go", "example_test.cs", "ABcqwoKCgpSSlIKCgKakyoKCuICCpKLkgoKCgoKUtLQACAiCgoKClIKClIIAHQiiggAAKoKClKSCmICCpJSClMqCmICCpJSAgsTkAAsUgqSCgIKkogAMBKIABhSCgpSkgoCC1vSiAAYUgoKUpIKClKSCgIK2gILE9IKCgpSCgoKClICCAAgIxoKClJKCgoKUgoKUgrTkgoKClIKCgoCCpJSAggALCIKCgpSCgoKAgqSUgoKAgqSUgIIACAi0goKUtoKCgpS0tMSktIKClLaCgoKUtLTEpMKCgoKUpIKCgoCCpKaAgqSi", "", "", "", "35:21,148:21,206:21,245:21,279:21,286:21,342:21,437:21,470:21,505:21")]
// </GoSourcePositionMaps>

namespace go.database;

[GoPackage("sql_test")]
public static partial class sql_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct ExampleDB_Prepare_projects {}
    internal partial struct ExampleTx_Prepare_projects {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsql() => builtin.initPackage(typeof(go.database.sql_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsqlꓸdriver() => builtin.initPackage(typeof(go.database.sql.driver_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.database.sql_package));
    }
}
