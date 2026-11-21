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
[assembly: GoPositionMap("database/sql/convert.go", "convert.cs", "ABYslIKClKaCgpSCgpQADBaigsqCgtyCgoKUgpQAAhTygoKClIKUzOKCAAISAAgCypSCgoKCgu6CgpSCggACENKCooKCgpSClIIADBqCgpSCxMaCgpSCtIK0goKklLTcgqYAAh4ADAIACBAACASUlIKUgqSClIKkgpSCxpSClIKkgpSCpIKUgqSClILGlIKkgqSClIKkgpSCxpTWlIKUgqSClIKkgpSC2JSClIKU7IK2goKmyJSUgpqCxoKAgoLWgoCCgtaCgpSkgqaCpoKClIKmgqaCgpTEpKaCggAHEJSCgpSCpIKUgoKCgpSCpIKUgoKCgpSCpIKUgoKCgpSCpIKUlIKkgsimgoKUpoKUpJSClKSkpKSUprKUpKSkpKSClKYAARoACwKGlA==", "340-340:1;345-352:2", "175=database/sql.ccChecker.CheckNamedValue;190=database/sql.ccChecker.CheckNamedValue")]
[assembly: GoPositionMap("database/sql/ctxutil.go", "ctxutil.cs", "AAwagoKUgoKmgtamgoKUgoKmpsSmgoKUgoKmpsSmgoKUgoKmpsSmgoKUgoKmpsQACAaigoKCgpSmpoLKgriCpoKCpoLW5oKCgoKUlA==")]
[assembly: GoPositionMap("database/sql/sql.go", "sql.cs", "ADZgpprSgoKClIKUoqSigpSiprKCggAaTAAQCgAmLJKUpKSkpKSkpKQAJ0qygoKUgqiSgpQADhiygoKUgqiSgpQADhiygoKUgqiSgpQADhiygoKUgoKokoKUAA4YsoKClIKCqJKClAAOGLKCgpSCqJKClAAOGLKCgpSCqJKClAAOGLKCgpSCqJKClAAZLKKCgpSCpoKClJSCgoKUpgAxZABOqAGiptKCgqKkgoKUqvKClIKUgpTa8oKUgpSClNrCgoKUpoLugpSC6OKCgoKUgtaigoKClIKmgoKCgqaimKKCgoKUlIKUgoKCpoKCgpSCABAe8oKCgpSCggAUIPKCgqKkgoKUgoKClK7igoKCpoKCgqaClKbGgrYAEyCCpoIABCYAEAKC7qQAAigAEQKCgoKCpoKCgpSmxqKCgIKStoKqwoKUgoKmgqYAAhDSAAISAAkCgoKClIKUgoKClIKCgoKCgoKmgoKCgqYACAqCgqakpLiCgpSClAACGgAMAoKCpqaClIKCgoKClIKCggADGAAKAoKCgpSCgoIAAxLygpSUggAICoKCAAIQAAoCgpSCpoIACAqCoqaygoK4opSClJSCAAkKlIKCgoKmgoKCpoKmggAICr6ygoKmgoKCgoKCgoKCgriCgqbKgoKCgoKUpoKCgoKmpqYAFSrilIKUAAwa3LKCgoKCpoKCgoKUurKCtKTcAAkIgoKCgoKUgpSCgoKClNyClIK0AAwUtrKCgoKmpoLEpoKmgoKCgoKCgpSmgoKmyqaCgoKUptiCgpS47qaC+KSUggAHEIKCgoKClIK4goKUyIKCgoKCgoKClILugoIACRIACQKCgoKCpoKCgoLGAAoQ0oKCpoKCgoKUpoKClIKUgpSClJTKgoKClIKUgpSCggADGgAMAoKUgpSCgpS4goKCgoKUlAALEIKClIK4AAIWAAoCgpSCgqYAAhYACAKm7oKClKzSgoKClIKUgpQACRSCgoKU2tKClJKCpgACEPKmooKClKbigpSCgoKUgoKCsoKClJSCgpS4goKUgpSCkurSgpSSgqYAAhDypqKCgqau4oKCgpSCgoKCsoKClJSCgoK4yoK4goKClIKCpoKCgoKCytyCAAISAAgCggACGAALAgACGgAMAoKUgoKmAAIQ0qaigoKUqNKCgoKCgoKUgoLKggAIEoKokuoAAxIACQKClIKCpoKmuAAjTMKClIKUgqiygoKUqtKCgpSq0oKClAACEgAIAoIAAhYACgKCgpQAAhAACAKCpoKClIKCosqClJSChAAFGgAMAoKClKqigoK4gqbSgsqClIKCggAFENIAM3DmAAcQgqaC6rqygoIACg6ipvqCgoKUgpSmgq7CqOKCgoK0puimgpTEgu6CgpSCgpSClIIACA7CgqaC7oKClIKClIKUgpSCqJIAAhoADAKCgqaCgpSCgoIAAhgACQIABiYAFQKSgpSkgpSCgoLugpKUgqamgoKCuJSCgpKUgpSUpgAJFIKUgoKCAAUmABACqtKCgpQAAhDyqMKCgqau4gACEgAIAoIAAhgACwIAFi6CACxoAAgCgpSCkoKCpoKCpgAFEPLG0oKUgoKmgoKU/sKCgpSCgqaCgoKUgoKmguwACQKClIKCgoLKgoKCgpSmgpSCgqaCgoKCpqaClIKCpqrCgoKUgoKCgqoACAKClIKUkoKCpoKm7riSgpSCgpSCpoKmAAUQ8sbSgoKCgpQABxIACAKCgpQAAiIAEAKo4oKUgpSCgoKUgoKUlIKmppTW0oKCgoKClJQAMmiigpQACg6igpSClIKCAAIQ0oKClMSCtIL6AAISAAsIlIKmkoKUgpSClKbigsqClIKmgpSClIKCyoKUlAAGFgAPCJSCgoKmgpSCpoKCgoLKgpSCgoKU2gALCoKCuIKCAAUSAAgClJSqwpSUgoLGgrgACAKCgoKUgpSClNoACAKCgoKUgpSClAAXKJIAAhDSqqKssqqiAAIS4saClIKCppSAgpSkgIKkgIKkgIKkgIK2AAR+AD8CpqaCgoKClJTmsoKUgqaClIKmgoKCpqrCgoLYgoKCpsq8+JSm4oKUgpSUgqaClIKUgqaClJSCABQeAAoCggAPIIKCpoKClJSCgqbewgAZLrKCgtaygoL2goKosoKCogAbOKKCgpSogAALFrIACBKCgq7CgoKUgqaUpoKCgqaCrLKClIKCgg==", "654-654:1;683-689:1;693-697:2;792-792:1;885-887:1;899-902:1;1462-1464:1;1598-1601:1;1638-1640:1;1641-1643:2;1671-1674:1;1697-1699:1;1708-1714:2;1724-1726:3;1741-1744:1;1781-1787:1;1807-1809:2;1873-1876:1;1902-1907:1;1949-1952:1;2004-2006:1;2083-2093:1;2312-2314:1;2344-2346:1;2438-2440:1;2459-2461:2;2648-2657:1;2758-2760:1;2792-2827:1;2813-2816:1.1;3040-3042:1;3102-3106:1;3455-3457:1", "", "1540=Invoke/1/2/2,Invoke/2/2/3;1709=Invoke/1/2/2,Invoke/2/2/3", "2052:21,4151:21", "1938:21,1939:13,2014:21,2015:13,2458:21,2459:13,3658:21,3659:13")]
// </GoSourcePositionMaps>

namespace go.database;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bytes = bytes_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using time = time_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.database.sql;
using go.unicode;
using static sql_package;
using context = context_package;
using io = io_package;
using maps = maps_package;
using rand = math.rand.rand_package;
using runtime = runtime_package;
using slices = slices_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using go.sync;
using iter = iter_package;
using math.rand;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(sql_package.ColumnType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛColumnType;
[GoMethodHost(typeof(sql_package.Conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛConn;
[GoMethodHost(typeof(sql_package.DB)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛDB;
[GoMethodHost(typeof(sql_package.IsolationLevel)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛIsolationLevel;
[GoMethodHost(typeof(sql_package.Null<>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNull;
[GoMethodHost(typeof(sql_package.NullBool)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullBool;
[GoMethodHost(typeof(sql_package.NullByte)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullByte;
[GoMethodHost(typeof(sql_package.NullFloat64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullFloat64;
[GoMethodHost(typeof(sql_package.NullInt16)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullInt16;
[GoMethodHost(typeof(sql_package.NullInt32)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullInt32;
[GoMethodHost(typeof(sql_package.NullInt64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullInt64;
[GoMethodHost(typeof(sql_package.NullString)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullString;
[GoMethodHost(typeof(sql_package.NullTime)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛNullTime;
[GoMethodHost(typeof(sql_package.Row)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛRow;
[GoMethodHost(typeof(sql_package.Rows)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛRows;
[GoMethodHost(typeof(sql_package.Stmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛStmt;
[GoMethodHost(typeof(sql_package.Tx)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛTx;
[GoMethodHost(typeof(sql_package.ccChecker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛccChecker;
[GoMethodHost(typeof(sql_package.connRequestSet)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛconnRequestSet;
[GoMethodHost(typeof(sql_package.driverConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛdriverConn;
[GoMethodHost(typeof(sql_package.driverResult)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛdriverResult;
[GoMethodHost(typeof(sql_package.driverStmt)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛdriverStmt;
[GoMethodHost(typeof(sql_package.dsnConnector)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class sqlᴛdsnConnector;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class sqlᴛccChecker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error CheckNamedValue(this ccChecker c, ж<driver.NamedValue> Ꮡnv) => sql_package.CheckNamedValueᴍ(c, Ꮡnv);
}
partial class sqlᴛIsolationLevel {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this IsolationLevel i) => sql_package.Stringᴍ(i);
}
partial class sqlᴛNullString {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullString> Ꮡns, any value) => sql_package.Scanᴍ(Ꮡns, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullString ns) => sql_package.Valueᴍ(ns);
}
partial class sqlᴛNullInt64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullInt64> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt64 n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullInt32 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullInt32> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt32 n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullInt16 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullInt16> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullInt16 n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullByte {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullByte> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullByte n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullFloat64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullFloat64> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullFloat64 n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullBool {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullBool> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullBool n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNullTime {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<NullTime> Ꮡn, any value) => sql_package.Scanᴍ(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value(this NullTime n) => sql_package.Valueᴍ(n);
}
partial class sqlᴛNull {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan<T>(this ж<Null<T>> Ꮡn, any value) => sql_package.Scanᴍ<T>(Ꮡn, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (driverꓸValue, error) Value<T>(this Null<T> n) => sql_package.Valueᴍ<T>(n);
}
partial class sqlᴛdriverConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void releaseConn(this ж<driverConn> Ꮡdc, error err) => sql_package.releaseConnᴍ(Ꮡdc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void removeOpenStmt(this ж<driverConn> Ꮡdc, ж<driverStmt> ds) => sql_package.removeOpenStmtᴍ(Ꮡdc, ds);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool expired(this ref driverConn dc, time.Duration timeout) => sql_package.expiredᴍ(ref dc, timeout);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error resetSession(this ж<driverConn> Ꮡdc, context.Context ctx) => sql_package.resetSessionᴍ(Ꮡdc, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool validateConnection(this ж<driverConn> Ꮡdc, bool needsReset) => sql_package.validateConnectionᴍ(Ꮡdc, needsReset);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverStmt>, error) prepareLocked(this ж<driverConn> Ꮡdc, context.Context ctx, stmtConnGrabber cg, @string query) => sql_package.prepareLockedᴍ(Ꮡdc, ctx, cg, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<error> closeDBLocked(this ж<driverConn> Ꮡdc) => sql_package.closeDBLockedᴍ(Ꮡdc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<driverConn> Ꮡdc) => sql_package.Closeᴍ(Ꮡdc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error finalClose(this ж<driverConn> Ꮡdc) => sql_package.finalCloseᴍ(Ꮡdc);
}
partial class sqlᴛdriverStmt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<driverStmt> Ꮡds) => sql_package.Closeᴍ(Ꮡds);
}
partial class sqlᴛDB {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addDep(this ж<DB> Ꮡdb, finalCloser x, any dep) => sql_package.addDepᴍ(Ꮡdb, x, dep);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void addDepLocked(this ref DB db, finalCloser x, any dep) => sql_package.addDepLockedᴍ(ref db, x, dep);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error removeDep(this ж<DB> Ꮡdb, finalCloser x, any dep) => sql_package.removeDepᴍ(Ꮡdb, x, dep);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Func<error> removeDepLocked(this ref DB db, finalCloser x, any dep) => sql_package.removeDepLockedᴍ(ref db, x, dep);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error pingDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release) => sql_package.pingDCᴍ(ref db, ctx, Ꮡdc, release);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error PingContext(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.PingContextᴍ(Ꮡdb, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Ping(this ж<DB> Ꮡdb) => sql_package.Pingᴍ(Ꮡdb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<DB> Ꮡdb) => sql_package.Closeᴍ(Ꮡdb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint maxIdleConnsLocked(this ref DB db) => sql_package.maxIdleConnsLockedᴍ(ref db);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration shortestIdleTimeLocked(this ref DB db) => sql_package.shortestIdleTimeLockedᴍ(ref db);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxIdleConns(this ж<DB> Ꮡdb, nint n) => sql_package.SetMaxIdleConnsᴍ(Ꮡdb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetMaxOpenConns(this ж<DB> Ꮡdb, nint n) => sql_package.SetMaxOpenConnsᴍ(Ꮡdb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetConnMaxLifetime(this ж<DB> Ꮡdb, time.Duration d) => sql_package.SetConnMaxLifetimeᴍ(Ꮡdb, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetConnMaxIdleTime(this ж<DB> Ꮡdb, time.Duration d) => sql_package.SetConnMaxIdleTimeᴍ(Ꮡdb, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startCleanerLocked(this ж<DB> Ꮡdb) => sql_package.startCleanerLockedᴍ(Ꮡdb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void connectionCleaner(this ж<DB> Ꮡdb, time.Duration d) => sql_package.connectionCleanerᴍ(Ꮡdb, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (time.Duration, slice<ж<driverConn>>) connectionCleanerRunLocked(this ref DB db, time.Duration d) => sql_package.connectionCleanerRunLockedᴍ(ref db, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static DBStats Stats(this ж<DB> Ꮡdb) => sql_package.Statsᴍ(Ꮡdb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void maybeOpenNewConnections(this ref DB db) => sql_package.maybeOpenNewConnectionsᴍ(ref db);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void connectionOpener(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.connectionOpenerᴍ(Ꮡdb, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openNewConnection(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.openNewConnectionᴍ(Ꮡdb, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverConn>, error) conn(this ж<DB> Ꮡdb, context.Context ctx, connReuseStrategy strategy) => sql_package.connᴍ(Ꮡdb, ctx, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void noteUnusedDriverStatement(this ж<DB> Ꮡdb, ж<driverConn> Ꮡc, ж<driverStmt> ds) => sql_package.noteUnusedDriverStatementᴍ(Ꮡdb, Ꮡc, ds);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void putConn(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err, bool resetSession) => sql_package.putConnᴍ(Ꮡdb, Ꮡdc, err, resetSession);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool putConnDBLocked(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err) => sql_package.putConnDBLockedᴍ(Ꮡdb, Ꮡdc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error retry(this ref DB db, Func<connReuseStrategy, error> fn) => sql_package.retryᴍ(ref db, fn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<DB> Ꮡdb, context.Context ctx, @string query) => sql_package.PrepareContextᴍ(Ꮡdb, ctx, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Stmt>, error) Prepare(this ж<DB> Ꮡdb, @string query) => sql_package.Prepareᴍ(Ꮡdb, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Stmt>, error) prepare(this ж<DB> Ꮡdb, context.Context ctx, @string query, connReuseStrategy strategy) => sql_package.prepareᴍ(Ꮡdb, ctx, query, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Stmt>, error) prepareDC(this ж<DB> Ꮡdb, context.Context ctx, ж<driverConn> dc, Action<error> release, stmtConnGrabber cg, @string query) => sql_package.prepareDCᴍ(Ꮡdb, ctx, dc, release, cg, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) ExecContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContextᴍ(Ꮡdb, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) Exec(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Execᴍ(Ꮡdb, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Result, error) exec(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) => sql_package.execᴍ(Ꮡdb, ctx, query, args, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Result res, error err) execDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release, @string query, slice<any> args) => sql_package.execDCᴍ(ref db, ctx, Ꮡdc, release, query, args);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContextᴍ(Ꮡdb, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Queryᴍ(Ꮡdb, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Rows>, error) query(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) => sql_package.queryᴍ(Ꮡdb, ctx, query, args, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Rows>, error) queryDC(this ref DB db, context.Context ctx, context.Context txctx, ж<driverConn> Ꮡdc, Action<error> releaseConn, @string query, slice<any> args) => sql_package.queryDCᴍ(ref db, ctx, txctx, Ꮡdc, releaseConn, query, args);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContextᴍ(Ꮡdb, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRow(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowᴍ(Ꮡdb, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Tx>, error) BeginTx(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> opts) => sql_package.BeginTxᴍ(Ꮡdb, ctx, opts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Tx>, error) Begin(this ж<DB> Ꮡdb) => sql_package.Beginᴍ(Ꮡdb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Tx> tx, error err) begin(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> opts, connReuseStrategy strategy) => sql_package.beginᴍ(Ꮡdb, ctx, opts, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Tx> tx, error err) beginDC(this ж<DB> Ꮡdb, context.Context ctxʗp, ж<driverConn> dc, Action<error> release, ж<TxOptions> opts) => sql_package.beginDCᴍ(Ꮡdb, ctxʗp, dc, release, opts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static driver.Driver Driver(this ref DB db) => sql_package.Driverᴍ(ref db);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Conn>, error) Conn(this ж<DB> Ꮡdb, context.Context ctx) => sql_package.Connᴍ(Ꮡdb, ctx);
}
partial class sqlᴛdsnConnector {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (driver.Conn, error) Connect(this dsnConnector t, context.Context _) => sql_package.Connectᴍ(t, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static driver.Driver Driver(this dsnConnector t) => sql_package.Driverᴍ(t);
}
partial class sqlᴛConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<Conn> Ꮡc, context.Context _) => sql_package.grabConnᴍ(Ꮡc, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error PingContext(this ж<Conn> Ꮡc, context.Context ctx) => sql_package.PingContextᴍ(Ꮡc, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContextᴍ(Ꮡc, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContextᴍ(Ꮡc, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Conn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContextᴍ(Ꮡc, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<Conn> Ꮡc, context.Context ctx, @string query) => sql_package.PrepareContextᴍ(Ꮡc, ctx, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error /*err*/ Raw(this ж<Conn> Ꮡc, Func<any, error> f) => sql_package.Rawᴍ(Ꮡc, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Tx>, error) BeginTx(this ж<Conn> Ꮡc, context.Context ctx, ж<TxOptions> opts) => sql_package.BeginTxᴍ(Ꮡc, ctx, opts);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closemuRUnlockCondReleaseConn(this ж<Conn> Ꮡc, error err) => sql_package.closemuRUnlockCondReleaseConnᴍ(Ꮡc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static context.Context txCtx(this ref Conn c) => sql_package.txCtxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ж<Conn> Ꮡc, error err) => sql_package.closeᴍ(Ꮡc, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<Conn> Ꮡc) => sql_package.Closeᴍ(Ꮡc);
}
partial class sqlᴛTx {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void awaitDone(this ж<Tx> Ꮡtx) => sql_package.awaitDoneᴍ(Ꮡtx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isDone(this ж<Tx> Ꮡtx) => sql_package.isDoneᴍ(Ꮡtx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ref Tx tx, error err) => sql_package.closeᴍ(ref tx, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<Tx> Ꮡtx, context.Context ctx) => sql_package.grabConnᴍ(Ꮡtx, ctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static context.Context txCtx(this ref Tx tx) => sql_package.txCtxᴍ(ref tx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closemuRUnlockRelease(this ж<Tx> Ꮡtx, error _) => sql_package.closemuRUnlockReleaseᴍ(Ꮡtx, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closePrepared(this ж<Tx> Ꮡtx) => sql_package.closePreparedᴍ(Ꮡtx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Commit(this ж<Tx> Ꮡtx) => sql_package.Commitᴍ(Ꮡtx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error rollback(this ж<Tx> Ꮡtx, bool discardConn) => sql_package.rollbackᴍ(Ꮡtx, discardConn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Rollback(this ж<Tx> Ꮡtx) => sql_package.Rollbackᴍ(Ꮡtx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Stmt>, error) PrepareContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query) => sql_package.PrepareContextᴍ(Ꮡtx, ctx, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Stmt>, error) Prepare(this ж<Tx> Ꮡtx, @string query) => sql_package.Prepareᴍ(Ꮡtx, query);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Stmt> StmtContext(this ж<Tx> Ꮡtx, context.Context ctx, ж<Stmt> Ꮡstmt) => sql_package.StmtContextᴍ(Ꮡtx, ctx, Ꮡstmt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Stmt> Stmt(this ж<Tx> Ꮡtx, ж<Stmt> stmt) => sql_package.Stmtᴍ(Ꮡtx, stmt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.ExecContextᴍ(Ꮡtx, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) Exec(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Execᴍ(Ꮡtx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryContextᴍ(Ꮡtx, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.Queryᴍ(Ꮡtx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContextᴍ(Ꮡtx, ctx, query, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRow(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowᴍ(Ꮡtx, query, argsʗp);
}
partial class sqlᴛStmt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) ExecContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.ExecContextᴍ(Ꮡs, ctx, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (Result, error) Exec(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.Execᴍ(Ꮡs, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void removeClosedStmtLocked(this ref Stmt s) => sql_package.removeClosedStmtLockedᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverConn> dc, Action<error> releaseConn, ж<driverStmt> ds, error err) connStmt(this ж<Stmt> Ꮡs, context.Context ctx, connReuseStrategy strategy) => sql_package.connStmtᴍ(Ꮡs, ctx, strategy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<driverStmt>, error) prepareOnConnLocked(this ж<Stmt> Ꮡs, context.Context ctx, ж<driverConn> dc) => sql_package.prepareOnConnLockedᴍ(Ꮡs, ctx, dc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) QueryContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.QueryContextᴍ(Ꮡs, ctx, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rows>, error) Query(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.Queryᴍ(Ꮡs, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRowContext(this ж<Stmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowContextᴍ(Ꮡs, ctx, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Row> QueryRow(this ж<Stmt> Ꮡs, params ꓸꓸꓸany argsʗp) => sql_package.QueryRowᴍ(Ꮡs, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<Stmt> Ꮡs) => sql_package.Closeᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error finalClose(this ж<Stmt> Ꮡs) => sql_package.finalCloseᴍ(Ꮡs);
}
partial class sqlᴛRows {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error lasterrOrErrLocked(this ref Rows rs, error err) => sql_package.lasterrOrErrLockedᴍ(ref rs, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void initContextClose(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx) => sql_package.initContextCloseᴍ(Ꮡrs, ctx, txctx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void awaitDone(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx, context.Context closectx) => sql_package.awaitDoneᴍ(Ꮡrs, ctx, txctx, closectx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Next(this ж<Rows> Ꮡrs) => sql_package.Nextᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool doClose, bool ok) nextLocked(this ж<Rows> Ꮡrs) => sql_package.nextLockedᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool NextResultSet(this ж<Rows> Ꮡrs) => sql_package.NextResultSetᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Err(this ж<Rows> Ꮡrs) => sql_package.Errᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> rawbuf(this ж<Rows> Ꮡrs) => sql_package.rawbufᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static RawBytes setrawbuf(this ж<Rows> Ꮡrs, slice<byte> b) => sql_package.setrawbufᴍ(Ꮡrs, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) Columns(this ж<Rows> Ꮡrs) => sql_package.Columnsᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<ж<ColumnType>>, error) ColumnTypes(this ж<Rows> Ꮡrs) => sql_package.ColumnTypesᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) => sql_package.Scanᴍ(Ꮡrs, destʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error scanLocked(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) => sql_package.scanLockedᴍ(Ꮡrs, destʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closemuRUnlockIfHeldByScan(this ж<Rows> Ꮡrs) => sql_package.closemuRUnlockIfHeldByScanᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<Rows> Ꮡrs) => sql_package.Closeᴍ(Ꮡrs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this ж<Rows> Ꮡrs, error errʗp) => sql_package.closeᴍ(Ꮡrs, errʗp);
}
partial class sqlᴛColumnType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this ref ColumnType ci) => sql_package.Nameᴍ(ref ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 length, bool ok) Length(this ref ColumnType ci) => sql_package.Lengthᴍ(ref ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 precision, int64 scale, bool ok) DecimalSize(this ref ColumnType ci) => sql_package.DecimalSizeᴍ(ref ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static reflect.Type ScanType(this ref ColumnType ci) => sql_package.ScanTypeᴍ(ref ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (bool nullable, bool ok) Nullable(this ref ColumnType ci) => sql_package.Nullableᴍ(ref ci);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string DatabaseTypeName(this ref ColumnType ci) => sql_package.DatabaseTypeNameᴍ(ref ci);
}
partial class sqlᴛRow {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<Row> Ꮡr, params ꓸꓸꓸany destʗp) => sql_package.Scanᴍ(Ꮡr, destʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Err(this ref Row r) => sql_package.Errᴍ(ref r);
}
partial class sqlᴛdriverResult {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) LastInsertId(this driverResult dr) => sql_package.LastInsertIdᴍ(dr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) RowsAffected(this driverResult dr) => sql_package.RowsAffectedᴍ(dr);
}
partial class sqlᴛconnRequestSet {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void CloseAndRemoveAll(this ref connRequestSet s) => sql_package.CloseAndRemoveAllᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this ref connRequestSet s) => sql_package.Lenᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static connRequestDelHandle Add(this ref connRequestSet s, channel<connRequest> v) => sql_package.Addᴍ(ref s, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Delete(this ref connRequestSet s, connRequestDelHandle h) => sql_package.Deleteᴍ(ref s, h);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void deleteIndex(this ref connRequestSet s, nint idx) => sql_package.deleteIndexᴍ(ref s, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (channel<connRequest> v, bool ok) TakeRandom(this ref connRequestSet s) => sql_package.TakeRandomᴍ(ref s);
}
// </GlyphForwarders>
