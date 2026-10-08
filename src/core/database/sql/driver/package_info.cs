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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.database.sql.driver_package;

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
[assembly: GoTypeAlias("RowsAffected", "ΔRowsAffected")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Value", "object")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<NotNull, ValueConverter>(Pointer = true)]
[assembly: GoImplement<NotNull, ValueConverter>]
[assembly: GoImplement<Null, ValueConverter>(Pointer = true)]
[assembly: GoImplement<Null, ValueConverter>]
[assembly: GoImplement<boolType, ValueConverter>]
[assembly: GoImplement<defaultConverter, ValueConverter>]
[assembly: GoImplement<int32Type, ValueConverter>]
[assembly: GoImplement<noRows, Result>]
[assembly: GoImplement<stringType, ValueConverter>]
[assembly: GoImplement<ΔRowsAffected, Result>(Pointer = true)]
[assembly: GoImplement<ΔRowsAffected, Result>]
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
[assembly: go.GoPositionMap("database/sql/driver/driver.go", "driver.cs", "AIIBqgIADRwA1AG+AwCSAaACgqaCAAoUxILWgg==")]
[assembly: go.GoPositionMap("database/sql/driver/types.go", "types.cs", "AD5+xICkgpSkgoKUpIKClJaClIKClKSCgpSmAAkSlIKClIKClKSCgpSkgoKUpAALFoKkpAAIEoKClAAIEoKClKiSgpTkpKSqogAVKpQAARoACwKAhqSmgoKWlIKClIKUqKaClpKUtqSkgoKUpKSkgoKUpKQ=")]
// </GoSourcePositionMaps>

namespace go.database.sql;

[GoPackage("driver")]
public static partial class driver_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface decimalDecompose {}
    public partial interface ColumnConverter {}
    public partial interface Conn {}
    public partial interface ConnBeginTx {}
    public partial interface ConnPrepareContext {}
    public partial interface Connector {}
    public partial interface Driver {}
    public partial interface DriverContext {}
    public partial interface Execer {}
    public partial interface ExecerContext {}
    public partial interface NamedValueChecker {}
    public partial interface Pinger {}
    public partial interface Queryer {}
    public partial interface QueryerContext {}
    public partial interface Result {}
    public partial interface Rows {}
    public partial interface RowsColumnTypeDatabaseTypeName {}
    public partial interface RowsColumnTypeLength {}
    public partial interface RowsColumnTypeNullable {}
    public partial interface RowsColumnTypePrecisionScale {}
    public partial interface RowsColumnTypeScanType {}
    public partial interface RowsNextResultSet {}
    public partial interface SessionResetter {}
    public partial interface Stmt {}
    public partial interface StmtExecContext {}
    public partial interface StmtQueryContext {}
    public partial interface Tx {}
    public partial interface Validator {}
    public partial interface ValueConverter {}
    public partial interface Valuer {}
    [GoLocalName("Value")] public partial interface Valueᴅ {}
    public partial struct IsolationLevel {}
    [GoMemberRecord("Value", GoMemberFact.Descriptor, typeof(Valueᴅ))] public partial struct NamedValue {}
    public partial struct NotNull {}
    public partial struct Null {}
    public partial struct TxOptions {}
    public partial struct boolType {}
    public partial struct defaultConverter {}
    public partial struct int32Type {}
    public partial struct noRows {}
    public partial struct stringType {}
    public partial struct ΔRowsAffected {}
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
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}
