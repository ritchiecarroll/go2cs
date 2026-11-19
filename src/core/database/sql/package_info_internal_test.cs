// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.database.sql_package;
using static go.database.sql_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("64617461626173652f73716c2e646561646c696e6572", "waitCondition_deadliner")]
[assembly: GoDynamicTypeLift("696e746572666163657b446561646c696e652829202874696d652e54696d652c20626f6f6c297d", "waitCondition_deadliner")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b20666e2066756e632829206572726f727d", "hookOpenErrᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b20666e2066756e63282a64617461626173652f73716c2e66616b65436f6e6e2c206572726f72297d", "hookPostCloseConnᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<Dummy, go.database.sql.driver_package.Driver>(Promoted = true)]
[assembly: GoImplement<Dummy, go.database.sql.driver_package.Driver>]
[assembly: GoImplement<anyTypeConverter, go.database.sql.driver_package.ValueConverter>]
[assembly: GoImplement<badConn, go.database.sql.driver_package.Conn>]
[assembly: GoImplement<badDriver, go.database.sql.driver_package.Driver>]
[assembly: GoImplement<concurrentDBExecTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentDBQueryTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentRandomTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentStmtExecTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentStmtQueryTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentTxExecTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentTxQueryTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentTxStmtExecTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<concurrentTxStmtQueryTest, concurrentTest>(Pointer = true)]
[assembly: GoImplement<ctxOnlyConn, go.database.sql.driver_package.Conn>(Pointer = true)]
[assembly: GoImplement<ctxOnlyConn, go.database.sql.driver_package.ExecerContext>(Pointer = true)]
[assembly: GoImplement<ctxOnlyConn, go.database.sql.driver_package.QueryerContext>(Pointer = true)]
[assembly: GoImplement<ctxOnlyDriver, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<dec, global::go.database.sql_package.decimalDecompose>]
[assembly: GoImplement<decFinite, global::go.database.sql_package.decimalDecompose>]
[assembly: GoImplement<fakeConn, go.database.sql.driver_package.Conn>(Pointer = true)]
[assembly: GoImplement<fakeConn, go.database.sql.driver_package.Validator>(Pointer = true)]
[assembly: GoImplement<fakeConn, memToucher>(Pointer = true)]
[assembly: GoImplement<fakeConnector, go.database.sql.driver_package.Connector>(Pointer = true)]
[assembly: GoImplement<fakeDriver, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<fakeDriverCtx, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<fakeDriverCtx, go.database.sql.driver_package.DriverContext>(Pointer = true)]
[assembly: GoImplement<fakeDriverString, go.database.sql.driver_package.ValueConverter>]
[assembly: GoImplement<fakeError, error>]
[assembly: GoImplement<fakeStmt, go.database.sql.driver_package.Stmt>(Pointer = true)]
[assembly: GoImplement<fakeStmt, memToucher>(Promoted = true)]
[assembly: GoImplement<fakeTx, go.database.sql.driver_package.Tx>(Pointer = true)]
[assembly: GoImplement<nvcConn, go.database.sql.driver_package.Conn>(Pointer = true)]
[assembly: GoImplement<nvcConn, go.database.sql.driver_package.NamedValueChecker>(Pointer = true)]
[assembly: GoImplement<nvcDriver, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<pingConn, go.database.sql.driver_package.Conn>]
[assembly: GoImplement<pingConn, go.database.sql.driver_package.Pinger>]
[assembly: GoImplement<pingDriver, go.database.sql.driver_package.Driver>(Pointer = true)]
[assembly: GoImplement<rowsCursor, go.database.sql.driver_package.Rows>(Pointer = true)]
[assembly: GoImplement<stubDriverStmt, go.database.sql.driver_package.Stmt>]
[assembly: GoImplement<unknownInputsConn, go.database.sql.driver_package.Conn>]
[assembly: GoImplement<unknownInputsConnector, go.database.sql.driver_package.Connector>(Pointer = true)]
[assembly: GoImplement<unknownInputsStmt, go.database.sql.driver_package.Stmt>]
[assembly: GoImplement<unknownInputsValueConverter, go.database.sql.driver_package.ValueConverter>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(TestMultiResultSetQuery_row1))]
[assembly: GoFieldAccess(typeof(TestMultiResultSetQuery_row2))]
[assembly: GoFieldAccess(typeof(TestQueryContext_row))]
[assembly: GoFieldAccess(typeof(TestQueryNamedArg_row))]
[assembly: GoFieldAccess(typeof(TestQuery_row))]
[assembly: GoFieldAccess(typeof(TestRowsCloseError_row))]
[assembly: GoFieldAccess(typeof(ctxOnlyConn))]
[assembly: GoFieldAccess(typeof(ctxOnlyDriver))]
[assembly: GoFieldAccess(typeof(fakeConn))]
[assembly: GoFieldAccess(typeof(fakeConnector))]
[assembly: GoFieldAccess(typeof(fakeDB))]
[assembly: GoFieldAccess(typeof(fakeDriver))]
[assembly: GoFieldAccess(typeof(fakeStmt))]
[assembly: GoFieldAccess(typeof(fakeTx))]
[assembly: GoFieldAccess(typeof(nvcDriver))]
[assembly: GoFieldAccess(typeof(pingDriver))]
[assembly: GoFieldAccess(typeof(row))]
[assembly: GoFieldAccess(typeof(rowsCursor))]
[assembly: GoFieldAccess(typeof(table))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("database/sql/convert_test.go", "convert_test.cs", "ABYmAEVclAB59AGCpoKmgqaCpoKmgqaCsoKCgpSigpSClIKUgpSClIKUgpSClIKUgpSClIKUgoKCgqaCgoKUgoKCuIKUgsqCgoKClIKUgoKUggAJFMqCgoKCgpSCpoKUggAODpIADyiCpoKCkoKUgoKCgoK4griygoIABxCCpLaCuJKUggAICpKElIKC/IIACwqigpQADgaCgoKCAC5kgoKCgoKUggANGKKCgqaClKSCgsaCgriClJQADBKigoKmgpSkxoK4gpSUAAsGggAHGrKSgoKCgpSUgpSC", "220-223:1;363-379:1;381-386:2;403-405:3;590-605:1", "", "95=Unix/1/7/19,UTC/1/6/19,Unix/2/7/20,FixedZone/1/1/20,In/1/1/20,Unix/3/7/21,UTC/2/6/21,Unix/4/7/23,UTC/3/6/23,Unix/5/7/24,UTC/4/6/24,Unix/6/7/24,UTC/5/6/24,Unix/7/7/53,UTC/6/6/53;379=Unix/1/1/18,UTC/1/1/18")]
[assembly: GoPositionMap("database/sql/fakedb_test.go", "fakedb_test.cs", "AEJ+goKCpoLmgoKUggAIDrSCABMigqaCAA0UggAmVIKmsoKCACBKlIIADA6CgoKCgoIACxSigoKiAAgQAAgCgoKCgoKmgoKUlJSCgoKUgpSCgoKClKbSgoKClIKCgpT20oKCoqTSgoKClIKUgqaC2JKClIKm4oKCgoKUgpQACAaCgoKCgqaClLiCgpSCgpSClILmooKUgpSCggALEKKCgqLMosaCgoKClMa0ggAKBtKCkoKUgoKCgpSCgoKmgoKUgpSClIIACAaCggAKBoLIxpSmyoKClKaUpsqCgpTGoszSgoKUlIKCgpSCgoKUkoKCgpSCgqaClKiygoKUgoKCgoKUgpSowoKClIKCgoKClJKCgoKUlIKUlLS0goKClLSClIKClIKCgpS0gpSUgoKmAAoMgqaigoKmgqaCkrKCgpSCgpSCpIK0goKUgraCgpSCgoK4goLmgriCgti0tNi0gpSClIKUlMaCgpSClKaCgpSClIKUgoKClIKUxvqCxrSigpSCpoKUgqaCgpSUgqam1oKkgqSClKTIlMaCgoKmzPKCgpSCgoKCpoKkgoKUgoKCgpSCgoKmgqaUlIK4gpQADQyCAAgGooKUgqaClIKmgoKmgoKCpoKClIKCgoKCpoKCgriCgoKUABQyppSCgoKCgpSmgoK4goKCgpSClJSCgpSCpoKmgoKUxoKCppSCgpSClKYACBKmgoKUAAoMgoKClIIACgyCgoKUggAePoKCpoKCgqamgoKCgoKmgqaCAAwKgoKmgpSCgoKUgqaCAAgOlIKClJSmgoKmgoKCgoKUAA8WgrSkgpSUAAkKgsaClKSkpKSkpLa2trakpKSUpoKUpKSkpKSkpKSkpKSkpJQ=", "416-431:1", "", "", "", "525:21,526:13")]
[assembly: GoPositionMap("database/sql/sql_test.go", "sql_test.cs", "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", "33-37:1;38-42:2;43-52:3;105-113:1;106-111:1.1;115-115:2;117-117:3;119-119:4;126-126:5;127-127:6;128-134:7;153-157:1;177-180:2;204-207:1;394-397:1;403-407:1;430-433:1;471-474:1;644-656:1;667-675:2;690-693:3;972-979:1;1414-1420:1;1428-1445:2;1429-1440:2.1;1441-1443:2.2;1516-1540:1;1596-1600:1;1729-1732:1;1739-1739:1;1752-1754:1;1755-1755:2;1764-1766:3;2053-2057:1;2091-2097:2;2166-2170:1;2229-2233:1;2240-2240:2;2247-2251:3;2254-2258:4;2345-2345:1;2346-2346:2;2410-2417:3;2437-2441:1;2469-2475:2;2504-2506:3;2532-2538:1;2678-2712:1;2689-2697:1.1;2756-2761:2;2772-2777:3;2858-2861:1;2862-2864:2;2865-2869:3;2927-2931:1;2932-2932:2;2943-3003:3;2957-2966:3.1;2967-2982:3.2;3078-3106:1;3083-3090:1.1;3109-3112:2;3117-3123:3;3128-3135:4;3138-3140:5;3151-3154:6;3167-3173:7;3186-3210:1;3190-3195:1.1;3213-3225:2;3214-3224:2.1;3230-3244:3;3231-3243:3.1;3574-3583:1;3607-3609:1;3662-3692:1;3663-3666:1.1;3717-3732:1;3718-3721:1.1;3754-3761:1;3762-3762:2;3831-3833:1;3865-3873:1;3948-3950:1;3960-3962:2;4017-4019:1;4021-4023:2;4024-4078:3;4045-4047:3.1;4395-4407:1;4409-4421:2;4426-4428:1;4544-4547:1;4559-4561:2;4685-4693:1;4840-4849:1;4859-4865:1;4886-4896:1;4897-4897:2;4899-4912:3;4913-4925:4;4926-4957:5;4958-4968:6", "", "804=Named/1/2/3,Named/2/2/4", "419:21,470:21,595:21,927:21,1203:21,1280:21,1353:21,1404:21,1433:21,1440:21,1478:21,1589:21,1641:21,1707:21,1714:21,1747:21,1777:21,1784:21,1815:21,1855:21,1919:21,1929:21,1940:21,2009:29,2049:21,2501:21,2614:21,2621:21,2628:21,2958:21,3413:21,3500:41,3662:21,3675:21,3740:21,3837:37,3854:37,3916:21,3953:21,4093:21,4111:21,4622:21,4743:33,4794:21,4812:33,4845:21,4903:21,4910:21,4917:21,4924:21,5349:21,5409:21,5446:21,5560:21,5569:21,5747:21,5789:27,5832:21,5872:21,5900:21,5934:21,6074:21,6258:21", "184:25,185:17,1877:29,1878:21,2311:21,2312:13,2916:21,2917:13,2927:21,2928:13,3064:21,3065:25,3705:21,3706:13,3798:21,3799:25,4735:33,4736:25,4806:33,4807:25,4862:21,4863:13,5164:21,5165:13,6064:21,6065:13")]
// </GoSourcePositionMaps>

namespace go.database;

[GoPackage("sql")]
public static partial class sql_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("deadliner")] partial interface waitCondition_deadliner {}
    [GoValueClone("@out")] partial struct TestDecimal_list {}
    [GoLocalName("execTest")] partial struct TestExec_execTest {}
    [GoLocalName("row1")] partial struct TestMultiResultSetQuery_row1 {}
    [GoLocalName("row2")] partial struct TestMultiResultSetQuery_row2 {}
    [GoLocalName("row")] partial struct TestQueryContext_row {}
    [GoLocalName("row")] partial struct TestQueryNamedArg_row {}
    [GoLocalName("row")] partial struct TestQuery_row {}
    [GoLocalName("row")] partial struct TestRowsCloseError_row {}
    [GoLocalName("Str")] partial struct TestTypedString_Str {}
    [GoLocalName("userDefinedBytes")] partial struct TestUserDefinedBytes_userDefinedBytes {}
    [GoValueClone("coefficient")] partial struct dec {}
    [GoValueClone("coefficient")] partial struct decFinite {}
    [GoLocalName("dbConn")] partial struct init_dbConn {}
    [GoValueClone("rows")] partial struct nullTestSpec {}
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
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(global::go.database.sql_package.DB)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛDB;
[GoMethodHost(typeof(sql_internal_test_package.Valuer_P)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛValuer_P;
[GoMethodHost(typeof(sql_internal_test_package.Valuer_V)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛValuer_V;
[GoMethodHost(typeof(sql_internal_test_package.alwaysErrScanner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛalwaysErrScanner;
[GoMethodHost(typeof(sql_internal_test_package.alwaysErrValuer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛalwaysErrValuer;
[GoMethodHost(typeof(sql_internal_test_package.anyTypeConverter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛanyTypeConverter;
[GoMethodHost(typeof(sql_internal_test_package.badConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛbadConn;
[GoMethodHost(typeof(sql_internal_test_package.badDriver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛbadDriver;
[GoMethodHost(typeof(sql_internal_test_package.concurrentDBExecTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentDBExecTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentDBQueryTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentDBQueryTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentRandomTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentRandomTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentStmtExecTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentStmtExecTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentStmtQueryTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentStmtQueryTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentTxExecTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentTxExecTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentTxQueryTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentTxQueryTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentTxStmtExecTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentTxStmtExecTest;
[GoMethodHost(typeof(sql_internal_test_package.concurrentTxStmtQueryTest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛconcurrentTxStmtQueryTest;
[GoMethodHost(typeof(sql_internal_test_package.ctxOnlyConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛctxOnlyConn;
[GoMethodHost(typeof(sql_internal_test_package.ctxOnlyDriver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛctxOnlyDriver;
[GoMethodHost(typeof(sql_internal_test_package.dec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛdec;
[GoMethodHost(typeof(sql_internal_test_package.decFinite)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛdecFinite;
[GoMethodHost(typeof(sql_internal_test_package.fakeConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeConn;
[GoMethodHost(typeof(sql_internal_test_package.fakeConnector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeConnector;
[GoMethodHost(typeof(sql_internal_test_package.fakeDB)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeDB;
[GoMethodHost(typeof(sql_internal_test_package.fakeDriver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeDriver;
[GoMethodHost(typeof(sql_internal_test_package.fakeDriverCtx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeDriverCtx;
[GoMethodHost(typeof(sql_internal_test_package.fakeDriverString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeDriverString;
[GoMethodHost(typeof(sql_internal_test_package.fakeError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeError;
[GoMethodHost(typeof(sql_internal_test_package.fakeStmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeStmt;
[GoMethodHost(typeof(sql_internal_test_package.fakeTx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛfakeTx;
[GoMethodHost(typeof(sql_internal_test_package.issue69728Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛissue69728Type;
[GoMethodHost(typeof(sql_internal_test_package.nvcConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛnvcConn;
[GoMethodHost(typeof(sql_internal_test_package.nvcDriver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛnvcDriver;
[GoMethodHost(typeof(sql_internal_test_package.pingConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛpingConn;
[GoMethodHost(typeof(sql_internal_test_package.pingDriver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛpingDriver;
[GoMethodHost(typeof(sql_internal_test_package.rowsCursor)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛrowsCursor;
[GoMethodHost(typeof(sql_internal_test_package.stubDriverStmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛstubDriverStmt;
[GoMethodHost(typeof(sql_internal_test_package.table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛtable;
[GoMethodHost(typeof(sql_internal_test_package.testScanner)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛtestScanner;
[GoMethodHost(typeof(sql_internal_test_package.unknownInputsConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛunknownInputsConn;
[GoMethodHost(typeof(sql_internal_test_package.unknownInputsConnector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛunknownInputsConnector;
[GoMethodHost(typeof(sql_internal_test_package.unknownInputsStmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛunknownInputsStmt;
[GoMethodHost(typeof(sql_internal_test_package.unknownInputsValueConverter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sql_internal_testᴛunknownInputsValueConverter;
// </MethodHolders>
