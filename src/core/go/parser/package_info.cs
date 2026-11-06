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
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.parser_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b506f73282920676f2f746f6b656e2e506f737d", "resolveFile_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<resolver, go.go.ast_package.Visitor>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.AssignStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BinaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BlockStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BranchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CaseClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ChanType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CompositeLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ellipsis))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ExprStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ForStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IfStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexListExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.KeyValueExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.LabeledStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Object))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Package))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.RangeStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Scope))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StarExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StructType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeAssertExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: GoFieldAccess(typeof(go.go.scanner_package.ΔError))]
[assembly: GoFieldAccess(typeof(parser))]
[assembly: GoFieldAccess(typeof(resolver))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/parser/interface.go", "interface.cs", "ABkwsoKUpLaCxpSUABBWAB4CgriCgqaUgpKUkoKCgsq4AAcQgpSCuIKEAAUoABQCgoKmgoKClIKCgpSCpoKCgoKCuJSCgrgAAhoAEAKCuIKCpoKClJKCgoKmgriCgriClIQABRTy", "98-127:1;215-227:1", "", "129=NewScope/1/1/2", "", "112:21,113:13,241:21,242:13")]
[assembly: GoPositionMap("go/parser/parser.go", "parser.cs", "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", "81-81:1;1044-1049:1;1731-1731:1;1887-1887:1", "", "3245=Pos/1/1/6;3258=makeExpr/1/1/3", "1775:21,2261:21,2408:21,2898:21,3282:21", "2305:21,2306:13,2516:21,2517:13")]
[assembly: GoPositionMap("go/parser/resolver.go", "resolver.cs", "AB8q0oIABxCCpoKCpoKUgoKCgoKCgqaCABIqoqaigpS25oKCgpSClOaCgoKUpoKCppSCgoKCgriC5rKCgpSmgqaClIKClIKCgpQACwzYgoKCgpSCgoKClIKUyoLu/OKCuIKUgoKClKaClO6CgriCgriCgoKCyoKC+MKCpqi2goKCttqCgraCgraClIK4gpSUlNqCgsiCtoKClNqCgsiCgraCgoKUgoKCyIKCgraCgoKUyoKClJSCyIKCgpSCgqaCyIKCgpTagsiCgoKUgpSClLaCgoKCgpSClILKAAgSgpSmyJSCgoKClIKClMaCuIKCgoKU/IKUuILcgoKCgpSCgtim1sSCgoKmooKUgoLKooKUgrj4gpSCgqaCgpSCtILEpIKCuKaCgriCgsqigpSCrNKCpuKClIKCog==")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("parser")]
public static partial class parser_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface resolveFile_type {}
    internal partial struct bailout {}
    internal partial struct field {}
    internal partial struct parseIfHeader_semi {}
    internal partial struct parser {}
    internal partial struct resolver {}
    public partial struct Mode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸinternalꓸscannerhooks() => builtin.initPackage(typeof(global::go.go.@internal.scannerhooks_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}
