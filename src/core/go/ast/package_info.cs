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
global using reflectꓸValue = go.reflect_package.ΔValue;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.ast_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<ArrayType, Expr>(Pointer = true)]
[assembly: GoImplement<ArrayType, Node>(Pointer = true)]
[assembly: GoImplement<AssignStmt, Node>(Pointer = true)]
[assembly: GoImplement<AssignStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<BadDecl, Decl>(Pointer = true)]
[assembly: GoImplement<BadDecl, Node>(Pointer = true)]
[assembly: GoImplement<BadExpr, Expr>(Pointer = true)]
[assembly: GoImplement<BadExpr, Node>(Pointer = true)]
[assembly: GoImplement<BadStmt, Node>(Pointer = true)]
[assembly: GoImplement<BadStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<BasicLit, Expr>(Pointer = true)]
[assembly: GoImplement<BasicLit, Node>(Pointer = true)]
[assembly: GoImplement<BinaryExpr, Expr>(Pointer = true)]
[assembly: GoImplement<BinaryExpr, Node>(Pointer = true)]
[assembly: GoImplement<BlockStmt, Node>(Pointer = true)]
[assembly: GoImplement<BlockStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<BranchStmt, Node>(Pointer = true)]
[assembly: GoImplement<BranchStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<CallExpr, Expr>(Pointer = true)]
[assembly: GoImplement<CallExpr, Node>(Pointer = true)]
[assembly: GoImplement<CaseClause, Node>(Pointer = true)]
[assembly: GoImplement<CaseClause, Stmt>(Pointer = true)]
[assembly: GoImplement<ChanType, Expr>(Pointer = true)]
[assembly: GoImplement<ChanType, Node>(Pointer = true)]
[assembly: GoImplement<CommClause, Node>(Pointer = true)]
[assembly: GoImplement<CommClause, Stmt>(Pointer = true)]
[assembly: GoImplement<Comment, Node>(Pointer = true)]
[assembly: GoImplement<CommentGroup, Node>(Pointer = true)]
[assembly: GoImplement<CompositeLit, Expr>(Pointer = true)]
[assembly: GoImplement<CompositeLit, Node>(Pointer = true)]
[assembly: GoImplement<DeclStmt, Node>(Pointer = true)]
[assembly: GoImplement<DeclStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<DeferStmt, Node>(Pointer = true)]
[assembly: GoImplement<DeferStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<Directive, Node>(Pointer = true)]
[assembly: GoImplement<Ellipsis, Expr>(Pointer = true)]
[assembly: GoImplement<Ellipsis, Node>(Pointer = true)]
[assembly: GoImplement<EmptyStmt, Node>(Pointer = true)]
[assembly: GoImplement<EmptyStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<ExprStmt, Node>(Pointer = true)]
[assembly: GoImplement<ExprStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<Field, Node>(Pointer = true)]
[assembly: GoImplement<FieldList, Node>(Pointer = true)]
[assembly: GoImplement<File, Node>(Pointer = true)]
[assembly: GoImplement<ForStmt, Node>(Pointer = true)]
[assembly: GoImplement<ForStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<FuncDecl, Decl>(Pointer = true)]
[assembly: GoImplement<FuncDecl, Node>(Pointer = true)]
[assembly: GoImplement<FuncLit, Expr>(Pointer = true)]
[assembly: GoImplement<FuncLit, Node>(Pointer = true)]
[assembly: GoImplement<FuncType, Expr>(Pointer = true)]
[assembly: GoImplement<FuncType, Node>(Pointer = true)]
[assembly: GoImplement<GenDecl, Decl>(Pointer = true)]
[assembly: GoImplement<GenDecl, Node>(Pointer = true)]
[assembly: GoImplement<GoStmt, Node>(Pointer = true)]
[assembly: GoImplement<GoStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<Ident, Expr>(Pointer = true)]
[assembly: GoImplement<Ident, Node>(Pointer = true)]
[assembly: GoImplement<IfStmt, Node>(Pointer = true)]
[assembly: GoImplement<IfStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<ImportSpec, Node>(Pointer = true)]
[assembly: GoImplement<ImportSpec, Spec>(Pointer = true)]
[assembly: GoImplement<IncDecStmt, Node>(Pointer = true)]
[assembly: GoImplement<IncDecStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<IndexExpr, Expr>(Pointer = true)]
[assembly: GoImplement<IndexExpr, Node>(Pointer = true)]
[assembly: GoImplement<IndexListExpr, Expr>(Pointer = true)]
[assembly: GoImplement<IndexListExpr, Node>(Pointer = true)]
[assembly: GoImplement<InterfaceType, Expr>(Pointer = true)]
[assembly: GoImplement<InterfaceType, Node>(Pointer = true)]
[assembly: GoImplement<KeyValueExpr, Expr>(Pointer = true)]
[assembly: GoImplement<KeyValueExpr, Node>(Pointer = true)]
[assembly: GoImplement<LabeledStmt, Node>(Pointer = true)]
[assembly: GoImplement<LabeledStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<MapType, Expr>(Pointer = true)]
[assembly: GoImplement<MapType, Node>(Pointer = true)]
[assembly: GoImplement<Package, Node>(Pointer = true)]
[assembly: GoImplement<ParenExpr, Expr>(Pointer = true)]
[assembly: GoImplement<ParenExpr, Node>(Pointer = true)]
[assembly: GoImplement<RangeStmt, Node>(Pointer = true)]
[assembly: GoImplement<RangeStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<ReturnStmt, Node>(Pointer = true)]
[assembly: GoImplement<ReturnStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<SelectStmt, Node>(Pointer = true)]
[assembly: GoImplement<SelectStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<SelectorExpr, Expr>(Pointer = true)]
[assembly: GoImplement<SelectorExpr, Node>(Pointer = true)]
[assembly: GoImplement<SendStmt, Node>(Pointer = true)]
[assembly: GoImplement<SendStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<SliceExpr, Expr>(Pointer = true)]
[assembly: GoImplement<SliceExpr, Node>(Pointer = true)]
[assembly: GoImplement<StarExpr, Expr>(Pointer = true)]
[assembly: GoImplement<StarExpr, Node>(Pointer = true)]
[assembly: GoImplement<StructType, Expr>(Pointer = true)]
[assembly: GoImplement<StructType, Node>(Pointer = true)]
[assembly: GoImplement<SwitchStmt, Node>(Pointer = true)]
[assembly: GoImplement<SwitchStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<TypeAssertExpr, Expr>(Pointer = true)]
[assembly: GoImplement<TypeAssertExpr, Node>(Pointer = true)]
[assembly: GoImplement<TypeSpec, Node>(Pointer = true)]
[assembly: GoImplement<TypeSpec, Spec>(Pointer = true)]
[assembly: GoImplement<TypeSwitchStmt, Node>(Pointer = true)]
[assembly: GoImplement<TypeSwitchStmt, Stmt>(Pointer = true)]
[assembly: GoImplement<UnaryExpr, Expr>(Pointer = true)]
[assembly: GoImplement<UnaryExpr, Node>(Pointer = true)]
[assembly: GoImplement<ValueSpec, Node>(Pointer = true)]
[assembly: GoImplement<ValueSpec, Spec>(Pointer = true)]
[assembly: GoImplement<inspector, Visitor>]
[assembly: GoImplement<printer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(ArrayType))]
[assembly: GoFieldAccess(typeof(AssignStmt))]
[assembly: GoFieldAccess(typeof(BinaryExpr))]
[assembly: GoFieldAccess(typeof(BlockStmt))]
[assembly: GoFieldAccess(typeof(BranchStmt))]
[assembly: GoFieldAccess(typeof(CallExpr))]
[assembly: GoFieldAccess(typeof(CaseClause))]
[assembly: GoFieldAccess(typeof(CommClause))]
[assembly: GoFieldAccess(typeof(Comment))]
[assembly: GoFieldAccess(typeof(CommentGroup))]
[assembly: GoFieldAccess(typeof(CompositeLit))]
[assembly: GoFieldAccess(typeof(DeclStmt))]
[assembly: GoFieldAccess(typeof(DeferStmt))]
[assembly: GoFieldAccess(typeof(Ellipsis))]
[assembly: GoFieldAccess(typeof(ExprStmt))]
[assembly: GoFieldAccess(typeof(Field))]
[assembly: GoFieldAccess(typeof(FieldList))]
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(ForStmt))]
[assembly: GoFieldAccess(typeof(FuncDecl))]
[assembly: GoFieldAccess(typeof(FuncLit))]
[assembly: GoFieldAccess(typeof(FuncType))]
[assembly: GoFieldAccess(typeof(GenDecl))]
[assembly: GoFieldAccess(typeof(GoStmt))]
[assembly: GoFieldAccess(typeof(Ident))]
[assembly: GoFieldAccess(typeof(IfStmt))]
[assembly: GoFieldAccess(typeof(ImportSpec))]
[assembly: GoFieldAccess(typeof(IncDecStmt))]
[assembly: GoFieldAccess(typeof(IndexExpr))]
[assembly: GoFieldAccess(typeof(IndexListExpr))]
[assembly: GoFieldAccess(typeof(InterfaceType))]
[assembly: GoFieldAccess(typeof(KeyValueExpr))]
[assembly: GoFieldAccess(typeof(LabeledStmt))]
[assembly: GoFieldAccess(typeof(MapType))]
[assembly: GoFieldAccess(typeof(Package))]
[assembly: GoFieldAccess(typeof(ParenExpr))]
[assembly: GoFieldAccess(typeof(RangeStmt))]
[assembly: GoFieldAccess(typeof(ReturnStmt))]
[assembly: GoFieldAccess(typeof(Scope))]
[assembly: GoFieldAccess(typeof(SelectStmt))]
[assembly: GoFieldAccess(typeof(SelectorExpr))]
[assembly: GoFieldAccess(typeof(SendStmt))]
[assembly: GoFieldAccess(typeof(SliceExpr))]
[assembly: GoFieldAccess(typeof(StarExpr))]
[assembly: GoFieldAccess(typeof(StructType))]
[assembly: GoFieldAccess(typeof(SwitchStmt))]
[assembly: GoFieldAccess(typeof(TypeAssertExpr))]
[assembly: GoFieldAccess(typeof(TypeSpec))]
[assembly: GoFieldAccess(typeof(TypeSwitchStmt))]
[assembly: GoFieldAccess(typeof(UnaryExpr))]
[assembly: GoFieldAccess(typeof(ValueSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Object))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/ast/ast.go", "ast.cs", "AE2SAYIABxCClJSCgoKUAAISAAkCgpSCgqaCxqaClJSUgpSU2MimgtyCgoKCpqaCpgAKCuqCyoKClIKClIKCpgASJoKClIKUpoKClIKUgpQAChaCgriClKaCgriClKiygoKCgoKUpgDSAbYDgIKCgoKCgoKUlIKCgoKCgoKCgoKCgoKCgpSUgoKUgoKCgpSUgriUlIKCgoKCgoKCgoKCgoKCgoKClJSCgpiigoKCgoKCgoKCgoKCgoKClIKCgoKCnsCWgJaA1KKClACoAdoCgIKCgoKCgoKCgoKCgoKCgoKCgoKClIKCgoKUlIKCgoKUgoKCgoKUlIKClJSCgpSClJSCgpSUgoKUlIKCgoKUlIKCmKKCgoKCgoKCgoKCgoKCgoKCgoKCACtekoKUlIKUgoKUpoKClIKUlJiiggAnWICCgpSCgoKUlIKClKqiggAmWLCcwoKUAA0cggABGAAKAoIACgaCgoKClpKCgoKC7qiSgoKClA==")]
[assembly: GoPositionMap("go/ast/commentmap.go", "commentmap.cs", "ABAikgAHFIKCgpSUqJKClLSUggAOHgALGIKmgoKCgoIABxSigqzSgoKClIIAAiQAEQKCpqaCgoKCpoKWAAYQgoKCmKKCuJSCgp7SAAYS3samlKSCgpS4gqbUyKyygoKUrLKCooKUlKqigoKUgqaCgqaCuIKClOqCgriCgpTI1qSSgpSCgoKUpoKCgoSSgpSUlII=", "18-20:1;41-49:1;242-247:1;304-310:1")]
[assembly: GoPositionMap("go/ast/directive.go", "directive.cs", "ACtwAA0GgqaC3IKClIKClIKmgqaCgoKUpoKmggAKIgAIApSCgoKUlLaCgqamgoKC2pQAChSCgqaCgoKmgoKClKaCgg==")]
[assembly: GoPositionMap("go/ast/filter.go", "filter.cs", "AAsgwgACFPIAAhgACQIABBCigoKCgqasspSkgsaUprKClIKCgoKUgpSCgoKUlIKClIKmgpSCpoKCgoK4goKClLSClIK2gpSmooKUgoKCpqaClKSkpIKUpIKCpIKUpIKCpJSmgpSCgoKClMaCgpSU3LamgoKCgoKmAAIS4qaClIKklAACGAAJAqaCgoKCgqaCAAIgAA0CpoKCgoKmABU2spSUgIK2gILIyp4ACgiCgoKCkoKCgoKClIKCgpSCppzCgoKCgoKCgpSClIKClLi4qJKCgoKCgoKCogAJFIKClKamlJS4ggAIEoKCgoKCprqSgoKCgoIABxCC3IKCupKCgoKCgso=")]
[assembly: GoPositionMap("go/ast/import.go", "import.cs", "AA0gwoKCpqaUuIKCgpSCpoKmgoKCgoKC3IKCgoLcgqaCgoKUpoKCgpSmgoKClKiSgpQADBq4griCgriCgoKCgoKClJSCgoKCppSClILKgoK4goKCgpSUgoKUgpSCAAcQgoKCgoKUgoKCgpTKgoKClKaCuKaCgoKUgoKCgoLc3Jis0oKCgg==", "186-200:1;242-244:2")]
[assembly: GoPositionMap("go/ast/print.go", "print.cs", "ABMsopSUAAIYAAkCpsQACBKCgsqCgpSChNqiAAwalLKClIKCgoKUgoKCgpSCgoK4lIKClAAIFLKCAAMaAAoCgoKmlLaCgoKCgoKCgpSUtriCgpSCyIKCgoKCgoKUlLaCgpSCgoKCgoKClJS2goKCgqaCgoKCgpSCgriCtoKmgraCgsg=", "54-58:1", "", "", "", "57:21,58:13")]
[assembly: GoPositionMap("go/ast/resolve.go", "resolve.cs", "ABQqgqaipsKClJSCgoKUuKKCgoKmAA80AAwCgqaCgpSktILIgsqmpoK4goKCgoKUgoKCgoLugoK4lIKUyoKCgsrKlIKCgoKCqIKmgg==")]
[assembly: GoPositionMap("go/ast/scope.go", "scope.cs", "ABg0koKssq7ygpSosoKCgoKCpoIAIkiSrLKClIKC2IKUpIKC2ILGgsaCxoKCAAgMAA4iAAkU")]
[assembly: GoPositionMap("go/ast/walk.go", "walk.cs", "ABEmooIAAxgACAKC3Oy2gpSCgpSClILIAAsOgsiCtoKUtraCtoK2graCgpSClILIgoLIgra2toK2gsiClLa2gpSClILItoK2AAgO/IK2toK2toK2tra2gsi2gpSCgoLIgraClIKUtoKUgraClLa2gpSClIKUtoKUgpSCyIKUgpSCgsiClIKClIKCyIKUgoKUgoIACA6ClLaClIKUgoKC2oKUguyC2LbKgoKUAAIYAAkCAAIS4oKCgpSUAAQiAAwCgoKClJSUlJSC", "387-396:1;389-395:1.1;413-424:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("ast")]
public static partial class ast_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct cgPos {}
    internal partial struct commentListReader {}
    internal partial struct directiveScanner {}
    internal partial struct localError {}
    internal partial struct nodeStack {}
    internal partial struct pkgBuilder {}
    internal partial struct posSpan {}
    internal partial struct printer {}
    public partial interface Decl {}
    public partial interface Expr {}
    public partial interface Node {}
    public partial interface Spec {}
    public partial interface Stmt {}
    public partial interface Visitor {}
    public partial struct ArrayType {}
    public partial struct AssignStmt {}
    public partial struct BadDecl {}
    public partial struct BadExpr {}
    public partial struct BadStmt {}
    public partial struct BasicLit {}
    public partial struct BinaryExpr {}
    public partial struct BlockStmt {}
    public partial struct BranchStmt {}
    public partial struct CallExpr {}
    public partial struct CaseClause {}
    public partial struct ChanDir {}
    public partial struct ChanType {}
    public partial struct CommClause {}
    public partial struct Comment {}
    public partial struct CommentGroup {}
    public partial struct CommentMap {}
    public partial struct CompositeLit {}
    public partial struct DeclStmt {}
    public partial struct DeferStmt {}
    public partial struct Directive {}
    public partial struct DirectiveArg {}
    public partial struct Ellipsis {}
    public partial struct EmptyStmt {}
    public partial struct ExprStmt {}
    public partial struct Field {}
    public partial struct FieldList {}
    public partial struct File {}
    public partial struct ForStmt {}
    public partial struct FuncDecl {}
    public partial struct FuncLit {}
    public partial struct FuncType {}
    public partial struct GenDecl {}
    public partial struct GoStmt {}
    public partial struct Ident {}
    public partial struct IfStmt {}
    public partial struct ImportSpec {}
    public partial struct IncDecStmt {}
    public partial struct IndexExpr {}
    public partial struct IndexListExpr {}
    public partial struct InterfaceType {}
    public partial struct KeyValueExpr {}
    public partial struct LabeledStmt {}
    public partial struct MapType {}
    public partial struct MergeMode {}
    public partial struct ObjKind {}
    public partial struct Object {}
    public partial struct Package {}
    public partial struct ParenExpr {}
    public partial struct RangeStmt {}
    public partial struct ReturnStmt {}
    public partial struct Scope {}
    public partial struct SelectStmt {}
    public partial struct SelectorExpr {}
    public partial struct SendStmt {}
    public partial struct SliceExpr {}
    public partial struct StarExpr {}
    public partial struct StructType {}
    public partial struct SwitchStmt {}
    public partial struct TypeAssertExpr {}
    public partial struct TypeSpec {}
    public partial struct TypeSwitchStmt {}
    public partial struct UnaryExpr {}
    public partial struct ValueSpec {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(global::go.unicode.utf8_package));
    // </ImportInitializers>
}
