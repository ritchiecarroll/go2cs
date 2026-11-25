// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using driverꓸValue = object;
using context = go.context_package;
using driver = go.database.sql.driver_package;
using reflect = go.reflect_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.database.sql_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b2076205b5d2a64617461626173652f73716c2e53746d747d", "Tx_stmts")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Conn, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<IsolationLevel, fmt_package.Stringer>]
[assembly: GoImplement<NullBool, Scanner>(Pointer = true)]
[assembly: GoImplement<NullByte, Scanner>(Pointer = true)]
[assembly: GoImplement<NullFloat64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt16, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt32, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullString, Scanner>(Pointer = true)]
[assembly: GoImplement<NullTime, Scanner>(Pointer = true)]
[assembly: GoImplement<Stmt, finalCloser>(Pointer = true)]
[assembly: GoImplement<Tx, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<driverConn, finalCloser>(Pointer = true)]
[assembly: GoImplement<driverConn, sync_package.Locker>(Pointer = true)]
[assembly: GoImplement<driverResult, Result>]
[assembly: GoImplement<driverResult, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<driverStmt, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<dsnConnector, go.database.sql.driver_package.Connector>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<IsolationLevel, driver.IsolationLevel>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ColumnType))]
[assembly: GoFieldAccess(typeof(Conn))]
[assembly: GoFieldAccess(typeof(DB))]
[assembly: GoFieldAccess(typeof(Null<>))]
[assembly: GoFieldAccess(typeof(NullBool))]
[assembly: GoFieldAccess(typeof(NullByte))]
[assembly: GoFieldAccess(typeof(NullFloat64))]
[assembly: GoFieldAccess(typeof(NullInt16))]
[assembly: GoFieldAccess(typeof(NullInt32))]
[assembly: GoFieldAccess(typeof(NullInt64))]
[assembly: GoFieldAccess(typeof(NullString))]
[assembly: GoFieldAccess(typeof(NullTime))]
[assembly: GoFieldAccess(typeof(Rows))]
[assembly: GoFieldAccess(typeof(Stmt))]
[assembly: GoFieldAccess(typeof(Tx))]
[assembly: GoFieldAccess(typeof(driverConn))]
[assembly: GoFieldAccess(typeof(driverStmt))]
[assembly: GoFieldAccess(typeof(go.database.sql.driver_package.NamedValue))]
[assembly: GoFieldAccess(typeof(strconv_package.NumError))]
[assembly: GoFieldAccess(typeof(time_package.Timer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("database/sql/convert.go", "convert.cs", "ABUslIKClKaCgpSCgpQAChaigsqCgtyCgoKUgpQAAhTygoKClIKUrOKCAAISAAgCypSCgoKCgu6CgpSCggACENKCooKCgpSClIIADBqCgpSCxMaCgpSCtIK0goKklLTcgqYAAh4ADAIACBAACASUlIKUgqSClIKkgpSSxpSClIKkgpSCpIKUgqSClILGlIKkgqSClIKkgpSixpTWlIKUgqSClIKkgpSC2JSClIKU7IK2goKmyJSUgpqCxoKAgoLWgoCCgtaCgpSkgqaCpoKClIKmgqaCgpTEpKaCggAHEJSCgpSCpIKUgoKCgpSCpIKUgoKCgpSCpIKUgoKCgpSCpIKUlIKkgsimgoKUpoKUpJSClKSkpKSUprKUpKSkpKSClKYAARoACwKGlA==", "340-340:1;345-352:2", "175=database/sql.ccChecker.CheckNamedValue;190=database/sql.ccChecker.CheckNamedValue")]
[assembly: GoPositionMap("database/sql/ctxutil.go", "ctxutil.cs", "AAwagoKUgoKmgtamgoKUgoKmpsSmgoKUgoKmpsSmgoKUgoKmpsSmgoKUgoKmpsQACAaigoKCgpSmpoLKgriCpoKCpoLW5oKCgoKUlA==")]
[assembly: GoPositionMap("database/sql/sql.go", "sql.cs", "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", "654-654:1;683-689:1;693-697:2;792-792:1;885-887:1;899-902:1;1462-1464:1;1598-1601:1;1638-1640:1;1641-1643:2;1671-1674:1;1697-1699:1;1708-1714:2;1724-1726:3;1741-1744:1;1781-1787:1;1807-1809:2;1873-1876:1;1902-1907:1;1949-1952:1;2004-2006:1;2083-2093:1;2312-2314:1;2344-2346:1;2438-2440:1;2459-2461:2;2648-2657:1;2758-2760:1;2792-2827:1;2813-2816:1.1;3040-3042:1;3102-3106:1;3455-3457:1", "", "1473=Invoke/1/2/2,Invoke/2/2/3;1638=Invoke/1/2/2,Invoke/2/2/3", "1970:21,4008:21", "1856:55,1857:13,1932:52,1933:13,2368:51,2369:13,3533:61,3534:13")]
// </GoSourcePositionMaps>

namespace go.database;

using static go.database.sql_package;
using context = go.context_package;
using driver = go.database.sql.driver_package;
using reflect = go.reflect_package;
using time = go.time_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("sql")]
public static partial class sql_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface @decimal {}
    internal partial interface decimalCompose {}
    internal partial interface decimalDecompose {}
    internal partial interface finalCloser {}
    internal partial interface stmtConnGrabber {}
    internal partial struct ccChecker {}
    internal partial struct connRequest {}
    internal partial struct connRequestAndIndex {}
    internal partial struct connRequestDelHandle {}
    internal partial struct connRequestSet {}
    internal partial struct connReuseStrategy {}
    internal partial struct connStmt {}
    internal partial struct depSet {}
    internal partial struct driverConn {}
    internal partial struct driverResult {}
    internal partial struct driverStmt {}
    internal partial struct dsnConnector {}
    public partial interface Result {}
    public partial interface Scanner {}
    public partial struct ColumnType {}
    public partial struct Conn {}
    public partial struct DB {}
    public partial struct DBStats {}
    public partial struct IsolationLevel {}
    public partial struct NamedArg {}
    public partial struct Null<T> {}
    public partial struct NullBool {}
    public partial struct NullByte {}
    public partial struct NullFloat64 {}
    public partial struct NullInt16 {}
    public partial struct NullInt32 {}
    public partial struct NullInt64 {}
    public partial struct NullString {}
    public partial struct NullTime {}
    public partial struct Out {}
    public partial struct RawBytes {}
    public partial struct Row {}
    public partial struct Rows {}
    public partial struct Stmt {}
    public partial struct Tx {}
    public partial struct TxOptions {}
    public partial struct Tx_stmts {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸdatabaseꓸsqlꓸdriver() => builtin.initPackage(typeof(go.database.sql.driver_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(sql_package.ColumnType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛColumnType;
[GoMethodHost(typeof(sql_package.Conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛConn;
[GoMethodHost(typeof(sql_package.DB)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛDB;
[GoMethodHost(typeof(sql_package.IsolationLevel)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛIsolationLevel;
[GoMethodHost(typeof(sql_package.Null<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNull;
[GoMethodHost(typeof(sql_package.NullBool)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullBool;
[GoMethodHost(typeof(sql_package.NullByte)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullByte;
[GoMethodHost(typeof(sql_package.NullFloat64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullFloat64;
[GoMethodHost(typeof(sql_package.NullInt16)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullInt16;
[GoMethodHost(typeof(sql_package.NullInt32)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullInt32;
[GoMethodHost(typeof(sql_package.NullInt64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullInt64;
[GoMethodHost(typeof(sql_package.NullString)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullString;
[GoMethodHost(typeof(sql_package.NullTime)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛNullTime;
[GoMethodHost(typeof(sql_package.Row)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛRow;
[GoMethodHost(typeof(sql_package.Rows)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛRows;
[GoMethodHost(typeof(sql_package.Stmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛStmt;
[GoMethodHost(typeof(sql_package.Tx)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛTx;
[GoMethodHost(typeof(sql_package.ccChecker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛccChecker;
[GoMethodHost(typeof(sql_package.connRequestSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛconnRequestSet;
[GoMethodHost(typeof(sql_package.driverConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛdriverConn;
[GoMethodHost(typeof(sql_package.driverResult)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛdriverResult;
[GoMethodHost(typeof(sql_package.driverStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛdriverStmt;
[GoMethodHost(typeof(sql_package.dsnConnector)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class sqlᴛdsnConnector;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class sqlᴛColumnType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Name(this ref ColumnType ci) => sql_package.Name⁔(ref ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 length, bool ok) Length(this ref ColumnType ci) => sql_package.Length⁔(ref ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 precision, int64 scale, bool ok) DecimalSize(this ref ColumnType ci) => sql_package.DecimalSize⁔(ref ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static reflect.Type ScanType(this ref ColumnType ci) => sql_package.ScanType⁔(ref ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool nullable, bool ok) Nullable(this ref ColumnType ci) => sql_package.Nullable⁔(ref ci);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string DatabaseTypeName(this ref ColumnType ci) => sql_package.DatabaseTypeName⁔(ref ci);
}
partial class sqlᴛConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverConn>, releaseConn, error) grabConn(this ж<Conn> Ꮡc, context.Context _) => sql_package.grabConn⁔(Ꮡc, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error PingContext(this ж<Conn> Ꮡc, context.Context ctx) => sql_package.PingContext⁔(Ꮡc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContext⁔(Ꮡc, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContext⁔(Ꮡc, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContext⁔(Ꮡc, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<Conn> Ꮡc, context.Context ctx, @string query) => sql_package.PrepareContext⁔(Ꮡc, ctx, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error /*err*/ Raw(this ж<Conn> Ꮡc, Func<any, error> f) => sql_package.Raw⁔(Ꮡc, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Tx>, error) BeginTx(this ж<Conn> Ꮡc, context.Context ctx, ж<TxOptions> Ꮡopts) => sql_package.BeginTx⁔(Ꮡc, ctx, Ꮡopts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closemuRUnlockCondReleaseConn(this ж<Conn> Ꮡc, error err) => sql_package.closemuRUnlockCondReleaseConn⁔(Ꮡc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context txCtx(this ref Conn c) => sql_package.txCtx⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ж<Conn> Ꮡc, error err) => sql_package.close⁔(Ꮡc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Conn> Ꮡc) => sql_package.Close⁔(Ꮡc);
}
partial class sqlᴛDB {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addDep(this ж<DB> Ꮡdb, finalCloser x, any dep) => sql_package.addDep⁔(Ꮡdb, x, dep);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addDepLocked(this ref DB db, finalCloser x, any dep) => sql_package.addDepLocked⁔(ref db, x, dep);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error removeDep(this ж<DB> Ꮡdb, finalCloser x, any dep) => sql_package.removeDep⁔(Ꮡdb, x, dep);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<error> removeDepLocked(this ref DB db, finalCloser x, any dep) => sql_package.removeDepLocked⁔(ref db, x, dep);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error pingDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release) => sql_package.pingDC⁔(ref db, ctx, Ꮡdc, release);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error PingContext(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.PingContext⁔(Ꮡdb, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Ping(this ж<DB> Ꮡdb) => sql_package.Ping⁔(Ꮡdb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<DB> Ꮡdb) => sql_package.Close⁔(Ꮡdb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint maxIdleConnsLocked(this ref DB db) => sql_package.maxIdleConnsLocked⁔(ref db);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration shortestIdleTimeLocked(this ref DB db) => sql_package.shortestIdleTimeLocked⁔(ref db);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxIdleConns(this ж<DB> Ꮡdb, nint n) => sql_package.SetMaxIdleConns⁔(Ꮡdb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetMaxOpenConns(this ж<DB> Ꮡdb, nint n) => sql_package.SetMaxOpenConns⁔(Ꮡdb, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetConnMaxLifetime(this ж<DB> Ꮡdb, time.Duration d) => sql_package.SetConnMaxLifetime⁔(Ꮡdb, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetConnMaxIdleTime(this ж<DB> Ꮡdb, time.Duration d) => sql_package.SetConnMaxIdleTime⁔(Ꮡdb, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startCleanerLocked(this ж<DB> Ꮡdb) => sql_package.startCleanerLocked⁔(Ꮡdb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void connectionCleaner(this ж<DB> Ꮡdb, time.Duration d) => sql_package.connectionCleaner⁔(Ꮡdb, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (time.Duration, slice<ж<driverConn>>) connectionCleanerRunLocked(this ref DB db, time.Duration d) => sql_package.connectionCleanerRunLocked⁔(ref db, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static DBStats Stats(this ж<DB> Ꮡdb) => sql_package.Stats⁔(Ꮡdb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void maybeOpenNewConnections(this ref DB db) => sql_package.maybeOpenNewConnections⁔(ref db);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void connectionOpener(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.connectionOpener⁔(Ꮡdb, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openNewConnection(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.openNewConnection⁔(Ꮡdb, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverConn>, error) conn(this ж<DB> Ꮡdb, context.Context ctx, connReuseStrategy strategy) => sql_package.conn⁔(Ꮡdb, ctx, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void noteUnusedDriverStatement(this ж<DB> Ꮡdb, ж<driverConn> Ꮡc, ж<driverStmt> Ꮡds) => sql_package.noteUnusedDriverStatement⁔(Ꮡdb, Ꮡc, Ꮡds);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void putConn(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err, bool resetSession) => sql_package.putConn⁔(Ꮡdb, Ꮡdc, err, resetSession);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool putConnDBLocked(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err) => sql_package.putConnDBLocked⁔(Ꮡdb, Ꮡdc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error retry(this ref DB db, Func<connReuseStrategy, error> fn) => sql_package.retry⁔(ref db, fn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<DB> Ꮡdb, context.Context ctx, @string query) => sql_package.PrepareContext⁔(Ꮡdb, ctx, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Stmt>, error) Prepare(this ж<DB> Ꮡdb, @string query) => sql_package.Prepare⁔(Ꮡdb, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Stmt>, error) prepare(this ж<DB> Ꮡdb, context.Context ctx, @string query, connReuseStrategy strategy) => sql_package.prepare⁔(Ꮡdb, ctx, query, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Stmt>, error) prepareDC(this ж<DB> Ꮡdb, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release, stmtConnGrabber cg, @string query) => sql_package.prepareDC⁔(Ꮡdb, ctx, Ꮡdc, release, cg, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) ExecContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContext⁔(Ꮡdb, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) Exec(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Exec⁔(Ꮡdb, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Result, error) exec(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) => sql_package.exec⁔(Ꮡdb, ctx, query, args, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Result res, error err) execDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release, @string query, slice<any> args) => sql_package.execDC⁔(ref db, ctx, Ꮡdc, release, query, args);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContext⁔(Ꮡdb, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Query⁔(Ꮡdb, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Rows>, error) query(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) => sql_package.query⁔(Ꮡdb, ctx, query, args, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Rows>, error) queryDC(this ref DB db, context.Context ctx, context.Context txctx, ж<driverConn> Ꮡdc, Action<error> releaseConn, @string query, slice<any> args) => sql_package.queryDC⁔(ref db, ctx, txctx, Ꮡdc, releaseConn, query, args);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContext⁔(Ꮡdb, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRow(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRow⁔(Ꮡdb, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Tx>, error) BeginTx(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> Ꮡopts) => sql_package.BeginTx⁔(Ꮡdb, ctx, Ꮡopts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Tx>, error) Begin(this ж<DB> Ꮡdb) => sql_package.Begin⁔(Ꮡdb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Tx> tx, error err) begin(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> Ꮡopts, connReuseStrategy strategy) => sql_package.begin⁔(Ꮡdb, ctx, Ꮡopts, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Tx> tx, error err) beginDC(this ж<DB> Ꮡdb, context.Context ctxʗp, ж<driverConn> Ꮡdc, Action<error> release, ж<TxOptions> Ꮡopts) => sql_package.beginDC⁔(Ꮡdb, ctxʗp, Ꮡdc, release, Ꮡopts);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static driver.Driver Driver(this ref DB db) => sql_package.Driver⁔(ref db);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Conn>, error) Conn(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.Conn⁔(Ꮡdb, ctx);
}
partial class sqlᴛIsolationLevel {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this IsolationLevel i) => sql_package.String⁔(i);
}
partial class sqlᴛNull {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan<T>(this ж<Null<T>> Ꮡn, any value) => sql_package.Scan⁔<T>(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value<T>(this Null<T> n) => sql_package.Value⁔<T>(n);
}
partial class sqlᴛNullBool {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullBool> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullBool n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullByte {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullByte> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullByte n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullFloat64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullFloat64> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullFloat64 n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullInt16 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullInt16> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt16 n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullInt32 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullInt32> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt32 n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullInt64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullInt64> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt64 n) => sql_package.Value⁔(n);
}
partial class sqlᴛNullString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullString> Ꮡns, any value) => sql_package.Scan⁔(Ꮡns, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullString ns) => sql_package.Value⁔(ns);
}
partial class sqlᴛNullTime {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<NullTime> Ꮡn, any value) => sql_package.Scan⁔(Ꮡn, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullTime n) => sql_package.Value⁔(n);
}
partial class sqlᴛRow {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<Row> Ꮡr, params ꓸꓸꓸany destʗp) => sql_package.Scan⁔(Ꮡr, destʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Err(this ref Row r) => sql_package.Err⁔(ref r);
}
partial class sqlᴛRows {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error lasterrOrErrLocked(this ref Rows rs, error err) => sql_package.lasterrOrErrLocked⁔(ref rs, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initContextClose(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx) => sql_package.initContextClose⁔(Ꮡrs, ctx, txctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void awaitDone(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx, context.Context closectx) => sql_package.awaitDone⁔(Ꮡrs, ctx, txctx, closectx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Next(this ж<Rows> Ꮡrs) => sql_package.Next⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool doClose, bool ok) nextLocked(this ж<Rows> Ꮡrs) => sql_package.nextLocked⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool NextResultSet(this ж<Rows> Ꮡrs) => sql_package.NextResultSet⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Err(this ж<Rows> Ꮡrs) => sql_package.Err⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> rawbuf(this ж<Rows> Ꮡrs) => sql_package.rawbuf⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static RawBytes setrawbuf(this ж<Rows> Ꮡrs, slice<byte> b) => sql_package.setrawbuf⁔(Ꮡrs, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) Columns(this ж<Rows> Ꮡrs) => sql_package.Columns⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<ж<ColumnType>>, error) ColumnTypes(this ж<Rows> Ꮡrs) => sql_package.ColumnTypes⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) => sql_package.Scan⁔(Ꮡrs, destʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error scanLocked(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) => sql_package.scanLocked⁔(Ꮡrs, destʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closemuRUnlockIfHeldByScan(this ж<Rows> Ꮡrs) => sql_package.closemuRUnlockIfHeldByScan⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Rows> Ꮡrs) => sql_package.Close⁔(Ꮡrs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ж<Rows> Ꮡrs, error errʗp) => sql_package.close⁔(Ꮡrs, errʗp);
}
partial class sqlᴛStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.ExecContext⁔(Ꮡs, ctx, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) Exec(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.Exec⁔(Ꮡs, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removeClosedStmtLocked(this ref Stmt s) => sql_package.removeClosedStmtLocked⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverConn> dc, Action<error> releaseConn, ж<driverStmt> ds, error err) connStmt(this ж<Stmt> Ꮡs, context.Context ctx, connReuseStrategy strategy) => sql_package.connStmt⁔(Ꮡs, ctx, strategy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverStmt>, error) prepareOnConnLocked(this ж<Stmt> Ꮡs, context.Context ctx, ж<driverConn> Ꮡdc) => sql_package.prepareOnConnLocked⁔(Ꮡs, ctx, Ꮡdc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.QueryContext⁔(Ꮡs, ctx, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.Query⁔(Ꮡs, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContext⁔(Ꮡs, ctx, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRow(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.QueryRow⁔(Ꮡs, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Stmt> Ꮡs) => sql_package.Close⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error finalClose(this ж<Stmt> Ꮡs) => sql_package.finalClose⁔(Ꮡs);
}
partial class sqlᴛTx {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void awaitDone(this ж<Tx> Ꮡtx) => sql_package.awaitDone⁔(Ꮡtx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isDone(this ж<Tx> Ꮡtx) => sql_package.isDone⁔(Ꮡtx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this ref Tx tx, error err) => sql_package.close⁔(ref tx, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverConn>, releaseConn, error) grabConn(this ж<Tx> Ꮡtx, context.Context ctx) => sql_package.grabConn⁔(Ꮡtx, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static context.Context txCtx(this ref Tx tx) => sql_package.txCtx⁔(ref tx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closemuRUnlockRelease(this ж<Tx> Ꮡtx, error _) => sql_package.closemuRUnlockRelease⁔(Ꮡtx, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closePrepared(this ж<Tx> Ꮡtx) => sql_package.closePrepared⁔(Ꮡtx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Commit(this ж<Tx> Ꮡtx) => sql_package.Commit⁔(Ꮡtx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error rollback(this ж<Tx> Ꮡtx, bool discardConn) => sql_package.rollback⁔(Ꮡtx, discardConn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Rollback(this ж<Tx> Ꮡtx) => sql_package.Rollback⁔(Ꮡtx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query) => sql_package.PrepareContext⁔(Ꮡtx, ctx, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Stmt>, error) Prepare(this ж<Tx> Ꮡtx, @string query) => sql_package.Prepare⁔(Ꮡtx, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Stmt> StmtContext(this ж<Tx> Ꮡtx, context.Context ctx, ж<Stmt> Ꮡstmt) => sql_package.StmtContext⁔(Ꮡtx, ctx, Ꮡstmt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Stmt> Stmt(this ж<Tx> Ꮡtx, ж<Stmt> Ꮡstmt) => sql_package.Stmt⁔(Ꮡtx, Ꮡstmt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContext⁔(Ꮡtx, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (Result, error) Exec(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Exec⁔(Ꮡtx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContext⁔(Ꮡtx, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Query⁔(Ꮡtx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContext⁔(Ꮡtx, ctx, query, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Row> QueryRow(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRow⁔(Ꮡtx, query, argsʗp);
}
partial class sqlᴛccChecker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error CheckNamedValue(this ccChecker c, ж<driver.NamedValue> Ꮡnv) => sql_package.CheckNamedValue⁔(c, Ꮡnv);
}
partial class sqlᴛconnRequestSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void CloseAndRemoveAll(this ref connRequestSet s) => sql_package.CloseAndRemoveAll⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this ref connRequestSet s) => sql_package.Len⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static connRequestDelHandle Add(this ref connRequestSet s, channel<connRequest> v) => sql_package.Add⁔(ref s, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Delete(this ref connRequestSet s, connRequestDelHandle h) => sql_package.Delete⁔(ref s, h);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deleteIndex(this ref connRequestSet s, nint idx) => sql_package.deleteIndex⁔(ref s, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (channel<connRequest> v, bool ok) TakeRandom(this ref connRequestSet s) => sql_package.TakeRandom⁔(ref s);
}
partial class sqlᴛdriverConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void releaseConn(this ж<driverConn> Ꮡdc, error err) => sql_package.releaseConn⁔(Ꮡdc, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void removeOpenStmt(this ж<driverConn> Ꮡdc, ж<driverStmt> Ꮡds) => sql_package.removeOpenStmt⁔(Ꮡdc, Ꮡds);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool expired(this ref driverConn dc, time.Duration timeout) => sql_package.expired⁔(ref dc, timeout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error resetSession(this ж<driverConn> Ꮡdc, context.Context ctx) => sql_package.resetSession⁔(Ꮡdc, ctx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool validateConnection(this ж<driverConn> Ꮡdc, bool needsReset) => sql_package.validateConnection⁔(Ꮡdc, needsReset);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<driverStmt>, error) prepareLocked(this ж<driverConn> Ꮡdc, context.Context ctx, stmtConnGrabber cg, @string query) => sql_package.prepareLocked⁔(Ꮡdc, ctx, cg, query);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Func<error> closeDBLocked(this ж<driverConn> Ꮡdc) => sql_package.closeDBLocked⁔(Ꮡdc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<driverConn> Ꮡdc) => sql_package.Close⁔(Ꮡdc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error finalClose(this ж<driverConn> Ꮡdc) => sql_package.finalClose⁔(Ꮡdc);
}
partial class sqlᴛdriverResult {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) LastInsertId(this driverResult dr) => sql_package.LastInsertId⁔(dr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) RowsAffected(this driverResult dr) => sql_package.RowsAffected⁔(dr);
}
partial class sqlᴛdriverStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<driverStmt> Ꮡds) => sql_package.Close⁔(Ꮡds);
}
partial class sqlᴛdsnConnector {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (driver.Conn, error) Connect(this dsnConnector t, context.Context _) => sql_package.Connect⁔(t, _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static driver.Driver Driver(this dsnConnector t) => sql_package.Driver⁔(t);
}
// </MethodForwarders>
