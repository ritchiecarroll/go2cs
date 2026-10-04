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
global using driverꓸRowsAffected = go.database.sql.driver_package.ΔRowsAffected;
global using driverꓸValue = object;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using driver = go.database.sql.driver_package;
// </ImportedTypeAliases>

using go;
using static go.database.sql_package;

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
[assembly: GoTypeAlias("Conn", "ΔConn")]
[assembly: GoTypeAlias("Stmt", "ΔStmt")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<IsolationLevel, fmt_package.Stringer>]
[assembly: GoImplement<NullBool, Scanner>(Pointer = true)]
[assembly: GoImplement<NullByte, Scanner>(Pointer = true)]
[assembly: GoImplement<NullFloat64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt16, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt32, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullString, Scanner>(Pointer = true)]
[assembly: GoImplement<NullTime, Scanner>(Pointer = true)]
[assembly: GoImplement<Tx, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<driverConn, finalCloser>(Pointer = true)]
[assembly: GoImplement<driverConn, sync_package.Locker>(Pointer = true)]
[assembly: GoImplement<driverResult, Result>]
[assembly: GoImplement<driverResult, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<driverStmt, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<dsnConnector, go.database.sql.driver_package.Connector>]
[assembly: GoImplement<ΔConn, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<ΔStmt, finalCloser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<IsolationLevel, driver.IsolationLevel>(Inverted = true, ValueType = "nint")]
[assembly: GoImplicitConv<driverConn, ж<driverConn>>(Indirect = true)]
[assembly: GoImplicitConv<driverStmt, ж<driverStmt>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("database/sql/convert.go", "convert.cs", "ABUslIKClKaCgpSCgpQAChaigsqCgsyAgoKClIKUAAIU8oKCgpSClKziggACEgAIArqEgoKCgoLegoKUgoIAARDSgqKCgIKAgqSCpIIACxqCgpSCxMaCgpSCpIKkgoKklKTcgpYAAh4ADAIABhAACASUlIKUgqSClIKkgpSCxpSClIKkgpSCpIKUgqSClILGlIKkgqSClIKkgpSCxpTWlIKUgqSClIKkgpSCyJSSlIKU3ILGgoKmuISUgpqC1oKAgoLWgoCCgtaCgpSkgpaAgqaCgpSCloKWgoKUxKSWgoIABhCUgoKUgqSClIKCgoKUgqSClIKCgoKUgqSClIKCgoKUgqSClJSCpIK4poKAgqSmgpSklIKUpKSkpKSmspSkpKSkpIKkpgABGgALAoCGpA==", "340-340:1;345-352:2", "175=database/sql.ccChecker.CheckNamedValue;190=database/sql.ccChecker.CheckNamedValue")]
[assembly: go.GoPositionMap("database/sql/ctxutil.go", "ctxutil.cs", "AAwagoCCpIKCpoLWpoKClIKClqbEpoKClIKClqbEpoKAgqSCgpamxKaCgIKkgoKWpsTmooCCgoKClKamgrqCqIKWgoKmgtbWgoKCgpSU")]
[assembly: go.GoPositionMap("database/sql/sql.go", "sql.cs", "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", "654-654:1;683-689:1;693-697:2;792-792:1;885-887:1;899-902:1;1462-1464:1;1598-1601:1;1638-1640:1;1641-1643:2;1671-1674:1;1697-1699:1;1708-1714:2;1724-1726:3;1741-1744:1;1781-1787:1;1807-1809:2;1873-1876:1;1902-1907:1;1949-1952:1;2004-2006:1;2083-2093:1;2312-2314:1;2344-2346:1;2438-2440:1;2459-2461:2;2648-2657:1;2758-2760:1;2792-2827:1;2813-2816:1.1;3040-3042:1;3102-3106:1;3455-3457:1")]
// </GoSourcePositionMaps>

namespace go.database;

[GoPackage("sql")]
public static partial class sql_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    internal partial struct depSet {}
    internal partial struct driverConn {}
    internal partial struct driverResult {}
    internal partial struct driverStmt {}
    internal partial struct dsnConnector {}
    internal partial struct ΔconnStmt {}
    public partial interface Result {}
    public partial interface Scanner {}
    public partial struct ColumnType {}
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
    public partial struct Tx {}
    public partial struct TxOptions {}
    public partial struct Tx_stmts {}
    public partial struct ΔConn {}
    public partial struct ΔStmt {}
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
