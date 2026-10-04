// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
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
[assembly: go.GoPositionMap("database/sql/convert_test.go", "convert_test.cs", "ABUmAEZclABr9AGCpoKmgqaCpoKmgqaisoKCgpSigpSClIKUgpSClIKUgpSClIKUgIKkgpSClIKCpKaAgoKClICCgoLYgpSCyoKCgoKUgpSCgpSCAAkUyoKCgoKClIKmgpSCABAOkgAPKIKWgoKSgIKkgoKCgoK4gqiygoIABhCClKaCqJKUggAICpKEhIKC3IL6ooKU9oKCgoIALmSCgoKCgpSCAAsYooKCpoKUpIKC1oKCqIKUhAAIEqKCgqaClKTWgqiClIQACQaCAAUaspKSgoKClJSClII=", "220-223:1;363-379:1;381-386:2;403-405:3;590-605:1")]
[assembly: go.GoPositionMap("database/sql/fakedb_test.go", "fakedb_test.cs", "AD9+goKCpoLWgoKUgu6UggAPIoKmggAJFIIAIlSCprKCggAeSpSCAAoOgoKCgoKCAAwUooKCogAFEAAIAoKCgoKAgraCgpSEhIKCgoSClIKCgoKUptKCgoKUgoKClNbSgoKipNKCgoKUgIKkgqaC2JKClIKm4oKCgoKUgIKk5oKCpIKmgpS4goKUgoKUgpSC1qKClIKUgoIAChCigoKizKKmgoKCgpSmlIL20oKSgpSCgoKClIKCgqaCgpSClIKUguaCggAKBoLYppSmyoKClKaUpsqCgpSmoqzSgoKUhIKCgpSCgoKUkoKCgpSCgqaClKiygoKUgoKCgoKUgpSowoKClIKCgoKClJKCgoKUhIKUlKSkgoKClKSClIKClIKCgpSkgqSUgoKm7IKmooKCloKWgpKygoKUgoKUgpSCpIKClIK2goKEgoKAgriCguaCqIKCuLSkqMSCpIKUgpSUpoKClIKUpoKClIKUgpSCgoKUgpSm2oKmlKKClIKWgpSCloKClISClqbGgpSCpICCpKSoxKaCgoKmrPKCgpSCgoKCloKUgoKUgoKCgpSCgIKCpoCCtpSkgqiClAAJDILmooKUgpaClIKWgoKWgoKCloKChIKCgoKCloKCgriCgoKUABQyloSCgoKCgpSWgoK4goKCgpSCgJSkgoKUgIK2gqaCgpS2goKWhIKChIKUlgAIEqaCgpTsgoKClILsgoKClIIAGj6CgqaCgoKmpoKCgoKCpoKmgvqCgpaClIKCgpSCpoIACA6AlIKCpJSmgoKmgoKCgoKUAAsWgrSkgpSU2oKmgpSkpKSkpKSmtra2tKSkpKaClKSkpKSkpKSkpKSkpKSk", "416-431:1")]
[assembly: go.GoPositionMap("database/sql/sql_test.go", "sql_test.cs", "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", "33-37:1;38-42:2;43-52:3;105-113:1;106-111:1.1;115-115:2;117-117:3;119-119:4;126-126:5;127-127:6;128-134:7;153-157:1;177-180:2;204-207:1;394-397:1;403-407:1;430-433:1;471-474:1;644-656:1;667-675:2;690-693:3;972-979:1;1416-1422:1;1430-1447:2;1431-1442:2.1;1443-1445:2.2;1518-1543:1;1600-1604:1;1733-1736:1;1743-1743:1;1756-1758:1;1759-1759:2;1768-1770:3;2057-2061:1;2095-2101:2;2170-2174:1;2233-2237:1;2244-2244:2;2251-2255:3;2258-2262:4;2349-2349:1;2350-2350:2;2414-2421:3;2441-2445:1;2473-2479:2;2508-2510:3;2536-2542:1;2682-2716:1;2693-2701:1.1;2760-2765:2;2776-2781:3;2863-2866:1;2867-2869:2;2870-2874:3;2932-2936:1;2937-2937:2;2950-3010:3;2964-2973:3.1;2974-2989:3.2;3085-3113:1;3090-3097:1.1;3116-3119:2;3124-3130:3;3135-3142:4;3145-3147:5;3158-3161:6;3174-3180:7;3193-3217:1;3197-3202:1.1;3220-3232:2;3221-3231:2.1;3237-3251:3;3238-3250:3.1;3581-3590:1;3614-3616:1;3669-3699:1;3670-3673:1.1;3724-3739:1;3725-3728:1.1;3761-3768:1;3769-3769:2;3839-3841:1;3873-3881:1;3956-3958:1;3968-3970:2;4025-4027:1;4029-4031:2;4032-4086:3;4053-4055:3.1;4408-4420:1;4422-4434:2;4439-4441:1;4557-4560:1;4572-4574:2;4698-4706:1;4853-4862:1;4872-4878:1;4899-4909:1;4910-4910:2;4912-4925:3;4926-4938:4;4939-4970:5;4971-4981:6")]
// </GoSourcePositionMaps>

namespace go.database;

[GoPackage("sql")]
public static partial class sql_internal_test_package
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
