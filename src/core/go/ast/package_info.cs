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
using reflect = go.reflect_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.ast_package;
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
[assembly: GoPositionMap("go/ast/ast.go", "ast.cs", "AE2SAYIABxCClJSCgoKUAAISAAkCgpSCgqaCxqaClJSUgpSU2MimgtyCgoKCpqaCpgAKCuqCyoKClIKClIKCpgASJoKClIKUpoKClIKUgpQAChaCgriClKaCgriClKiygoKCgoKUpgDSAbYDgIKCgoKCgoKUlIKCgoKCgoKCgoKCgoKCgpSUgoKUgoKCgpSUgriUlIKCgoKCgoKCgoKCgoKCgoKClJSCgpiigoKCgoKCgoKCgoKCgoKClIKCgoKCnsCWgJaA1KKClACoAdoCgIKCgoKCgoKCgoKCgoKCgoKCgoKClIKCgoKUlIKCgoKUgoKCgoKUlIKClKSCgpSClJSCgpSUgoKUlIKCgoKUlIKCmKKCgoKCgoKCgoKCgoKCgoKCgoKCACtekoKUlIKUgoKUpoKClIKUlJiiggAnWICCgpSCgoKUlIKClKqiggAmWLCcwoKUAA0cggABGAAKAoIACgaCgoKClpKCgoKC7qiSgoKClA==")]
[assembly: GoPositionMap("go/ast/commentmap.go", "commentmap.cs", "ABAikgAHFIKCgpSUqJKClLSUggAOHgALGIKmgoKCkpIABxSigqzSgoKClIIAAiQAEQKCpqaCgoKCpoKWAAYQgoKCmKKCuJSCgp7SAAYS3samlKSCgpS4gqbUyKyygoKUrLKCooKUlKqigoKUgqaCgqaCuIKClOqCgriCgpTI1qSSgpSCgoKUpoKCgoSSgpSUlII=", "18-20:1;41-49:1;242-247:1;304-310:1")]
[assembly: GoPositionMap("go/ast/directive.go", "directive.cs", "ACtwAA0GgqaC3IKClIKClIKmgqaCgoKUpoKmggAKIgAIApSCgoKUlLaCgqamgoKC2pQAChSCgqaCgoKmgoKClKaCgg==")]
[assembly: GoPositionMap("go/ast/filter.go", "filter.cs", "AAsgwgACFPIAAhgACQIABBCigoKCgqasspSkgsaUprKClIKCgoKUgpSCgoKUlIKClIKmgpSCpoKCgoK4goKClLSClIK2gpSmooKUgoKCpqaClKSkpIKUpIKCpIKUpIKCpJSmgpSCgoKClMaCgpSU3LamgoKCgoKmAAIS4qaClIKklAACGAAJAqaCgoKCgqaCAAIgAA0CpoKCgoKmABU2spSUgIK2gILIyp4ACgiCgoKCkoKCgoKClIKCgpSCppzCgoKCgoKCgpSClIKClLi4qJKCgoKCgoKCogAJFIKClKamlJS4ggAIEoKCgoKCprqSgoKCgoIABxCC3IKCupKCgoKCgso=")]
[assembly: GoPositionMap("go/ast/import.go", "import.cs", "AA0gwoKCpqaUuIKCgpSCpoKmgoKCgoKC3IKCgoLcgqaCgoKUpoKCgpSmgoKClKiSgpQADBq4griCgriCgoKCgoKClJSCgoKCppSClILKgoK4goKCgpSUgoKUgpSCAAcQgoKCgoKUgoKCgpTKgoKClKaCuKaCgoKUgoKCgoLc3Jis0oKCgg==", "186-200:1;242-244:2")]
[assembly: GoPositionMap("go/ast/print.go", "print.cs", "ABMsopSUAAIYAAkCpsQACBKCgsqCgpSChNqiAAwalLKClIKCgoKUgoKCgpSCgoK4lIKClAAIFLKCAAMaAAoCgoKmlLaCgoKCgoKCgpSUtriCgpSCyIKCgoKCgoKUlLaCgpSCgoKCgoKClJS2goKCgqaCgoKCgpSCgriCtoKmgraCgsg=", "54-58:1", "", "", "", "57:46,58:13")]
[assembly: GoPositionMap("go/ast/resolve.go", "resolve.cs", "ABQqgqaipsKClJSCgoKUuKKCgoKmAA80AAwCgqaCgpSktILIgsqmpoK4goKCgoKUgoKCgoLugoK4lIKUyoKCgsrKlIKCgoKCqIKmgg==")]
[assembly: GoPositionMap("go/ast/scope.go", "scope.cs", "ABg0koKssq7ygpSosoKCgoKCpoIAIkiSrLKClIKC2IKUpIKC2ILGgsaCxoKCAAgMAA4iAAkU")]
[assembly: GoPositionMap("go/ast/walk.go", "walk.cs", "ABEmooIAAxgACAKC3Oy2gpSCgpSClILIAAsOgsiCtoKUtraCtoK2graCgpSClILIgoLIgra2toK2gsiClLa2gpSClILItoK2AAgO/IK2toK2toK2tra2gsi2gpSCgoLIgraClIKUtoKUgraClLa2gpSClIKUtoKUgpSCyIKUgpSCgsiClIKClIKCyIKUgoKUgoIACA6ClLaClIKUgoKC2oKUguyC2LbKgoKUAAIYAAkCAAIS4oKCgpSUAAQiAAwCgoKClJSUlJSC", "387-396:1;389-395:1.1;413-424:1")]
// </GoSourcePositionMaps>

namespace go.go;

using static global::go.go.ast_package;
using reflect = global::go.reflect_package;
using token = global::go.go.token_package;
using ꓸꓸꓸany = variadic<any>;

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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(ast_package.ArrayType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛArrayType;
[GoMethodHost(typeof(ast_package.AssignStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛAssignStmt;
[GoMethodHost(typeof(ast_package.BadDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBadDecl;
[GoMethodHost(typeof(ast_package.BadExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBadExpr;
[GoMethodHost(typeof(ast_package.BadStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBadStmt;
[GoMethodHost(typeof(ast_package.BasicLit)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBasicLit;
[GoMethodHost(typeof(ast_package.BinaryExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBinaryExpr;
[GoMethodHost(typeof(ast_package.BlockStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBlockStmt;
[GoMethodHost(typeof(ast_package.BranchStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛBranchStmt;
[GoMethodHost(typeof(ast_package.CallExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCallExpr;
[GoMethodHost(typeof(ast_package.CaseClause)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCaseClause;
[GoMethodHost(typeof(ast_package.ChanType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛChanType;
[GoMethodHost(typeof(ast_package.CommClause)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCommClause;
[GoMethodHost(typeof(ast_package.Comment)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛComment;
[GoMethodHost(typeof(ast_package.CommentGroup)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCommentGroup;
[GoMethodHost(typeof(ast_package.CommentMap)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCommentMap;
[GoMethodHost(typeof(ast_package.CompositeLit)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛCompositeLit;
[GoMethodHost(typeof(ast_package.DeclStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛDeclStmt;
[GoMethodHost(typeof(ast_package.DeferStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛDeferStmt;
[GoMethodHost(typeof(ast_package.Directive)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛDirective;
[GoMethodHost(typeof(ast_package.Ellipsis)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛEllipsis;
[GoMethodHost(typeof(ast_package.EmptyStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛEmptyStmt;
[GoMethodHost(typeof(ast_package.ExprStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛExprStmt;
[GoMethodHost(typeof(ast_package.Field)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛField;
[GoMethodHost(typeof(ast_package.FieldList)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛFieldList;
[GoMethodHost(typeof(ast_package.File)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛFile;
[GoMethodHost(typeof(ast_package.ForStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛForStmt;
[GoMethodHost(typeof(ast_package.FuncDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛFuncDecl;
[GoMethodHost(typeof(ast_package.FuncLit)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛFuncLit;
[GoMethodHost(typeof(ast_package.FuncType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛFuncType;
[GoMethodHost(typeof(ast_package.GenDecl)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛGenDecl;
[GoMethodHost(typeof(ast_package.GoStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛGoStmt;
[GoMethodHost(typeof(ast_package.Ident)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛIdent;
[GoMethodHost(typeof(ast_package.IfStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛIfStmt;
[GoMethodHost(typeof(ast_package.ImportSpec)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛImportSpec;
[GoMethodHost(typeof(ast_package.IncDecStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛIncDecStmt;
[GoMethodHost(typeof(ast_package.IndexExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛIndexExpr;
[GoMethodHost(typeof(ast_package.IndexListExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛIndexListExpr;
[GoMethodHost(typeof(ast_package.InterfaceType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛInterfaceType;
[GoMethodHost(typeof(ast_package.KeyValueExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛKeyValueExpr;
[GoMethodHost(typeof(ast_package.LabeledStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛLabeledStmt;
[GoMethodHost(typeof(ast_package.MapType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛMapType;
[GoMethodHost(typeof(ast_package.ObjKind)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛObjKind;
[GoMethodHost(typeof(ast_package.Object)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛObject;
[GoMethodHost(typeof(ast_package.Package)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛPackage;
[GoMethodHost(typeof(ast_package.ParenExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛParenExpr;
[GoMethodHost(typeof(ast_package.RangeStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛRangeStmt;
[GoMethodHost(typeof(ast_package.ReturnStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛReturnStmt;
[GoMethodHost(typeof(ast_package.Scope)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛScope;
[GoMethodHost(typeof(ast_package.SelectStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛSelectStmt;
[GoMethodHost(typeof(ast_package.SelectorExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛSelectorExpr;
[GoMethodHost(typeof(ast_package.SendStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛSendStmt;
[GoMethodHost(typeof(ast_package.SliceExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛSliceExpr;
[GoMethodHost(typeof(ast_package.StarExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛStarExpr;
[GoMethodHost(typeof(ast_package.StructType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛStructType;
[GoMethodHost(typeof(ast_package.SwitchStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛSwitchStmt;
[GoMethodHost(typeof(ast_package.TypeAssertExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛTypeAssertExpr;
[GoMethodHost(typeof(ast_package.TypeSpec)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛTypeSpec;
[GoMethodHost(typeof(ast_package.TypeSwitchStmt)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛTypeSwitchStmt;
[GoMethodHost(typeof(ast_package.UnaryExpr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛUnaryExpr;
[GoMethodHost(typeof(ast_package.ValueSpec)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛValueSpec;
[GoMethodHost(typeof(ast_package.commentListReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛcommentListReader;
[GoMethodHost(typeof(ast_package.directiveScanner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛdirectiveScanner;
[GoMethodHost(typeof(ast_package.inspector)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛinspector;
[GoMethodHost(typeof(ast_package.nodeStack)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛnodeStack;
[GoMethodHost(typeof(ast_package.pkgBuilder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛpkgBuilder;
[GoMethodHost(typeof(ast_package.printer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class astᴛprinter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class astᴛArrayType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ArrayType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ArrayType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref ArrayType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛAssignStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref AssignStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref AssignStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref AssignStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛBadDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BadDecl d) => ast_package.Pos⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BadDecl d) => ast_package.End⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declNode(this ref BadDecl _) => ast_package.declNode⁔(ref _);
}
partial class astᴛBadExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BadExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BadExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref BadExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛBadStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BadStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BadStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref BadStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛBasicLit {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BasicLit x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BasicLit x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref BasicLit _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛBinaryExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BinaryExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BinaryExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref BinaryExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛBlockStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BlockStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BlockStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref BlockStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛBranchStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref BranchStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref BranchStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref BranchStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛCallExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref CallExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref CallExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref CallExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛCaseClause {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref CaseClause s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref CaseClause s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref CaseClause _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛChanType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ChanType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ChanType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref ChanType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛCommClause {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref CommClause s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref CommClause s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref CommClause _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛComment {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Comment c) => ast_package.Pos⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Comment c) => ast_package.End⁔(ref c);
}
partial class astᴛCommentGroup {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref CommentGroup g) => ast_package.Pos⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref CommentGroup g) => ast_package.End⁔(ref g);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ж<CommentGroup> Ꮡg) => ast_package.Text⁔(Ꮡg);
}
partial class astᴛCommentMap {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addComment(this CommentMap cmap, Node n, ж<CommentGroup> Ꮡc) => ast_package.addComment⁔(cmap, n, Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Update(this CommentMap cmap, Node old, Node @new) => ast_package.Update⁔(cmap, old, @new);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static CommentMap Filter(this CommentMap cmap, Node node) => ast_package.Filter⁔(cmap, node);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<ж<CommentGroup>> Comments(this CommentMap cmap) => ast_package.Comments⁔(cmap);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this CommentMap cmap) => ast_package.String⁔(cmap);
}
partial class astᴛCompositeLit {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref CompositeLit x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref CompositeLit x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref CompositeLit _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛDeclStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref DeclStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref DeclStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref DeclStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛDeferStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref DeferStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref DeferStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref DeferStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛDirective {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Directive d) => ast_package.Pos⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Directive d) => ast_package.End⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<DirectiveArg>, error) ParseArgs(this ref Directive d) => ast_package.ParseArgs⁔(ref d);
}
partial class astᴛEllipsis {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Ellipsis x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Ellipsis x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref Ellipsis _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛEmptyStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref EmptyStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref EmptyStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref EmptyStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛExprStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ExprStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ExprStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref ExprStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛField {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Field f) => ast_package.Pos⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Field f) => ast_package.End⁔(ref f);
}
partial class astᴛFieldList {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref FieldList f) => ast_package.Pos⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref FieldList f) => ast_package.End⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumFields(this ж<FieldList> Ꮡf) => ast_package.NumFields⁔(Ꮡf);
}
partial class astᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref File f) => ast_package.Pos⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref File f) => ast_package.End⁔(ref f);
}
partial class astᴛForStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ForStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ForStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref ForStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛFuncDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref FuncDecl d) => ast_package.Pos⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref FuncDecl d) => ast_package.End⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declNode(this ref FuncDecl _) => ast_package.declNode⁔(ref _);
}
partial class astᴛFuncLit {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref FuncLit x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref FuncLit x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref FuncLit _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛFuncType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref FuncType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref FuncType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref FuncType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛGenDecl {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref GenDecl d) => ast_package.Pos⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref GenDecl d) => ast_package.End⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declNode(this ref GenDecl _) => ast_package.declNode⁔(ref _);
}
partial class astᴛGoStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref GoStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref GoStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref GoStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛIdent {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Ident x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Ident x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref Ident _) => ast_package.exprNode⁔(ref _);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsExported(this ref Ident id) => ast_package.IsExported⁔(ref id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Ident> Ꮡid) => ast_package.String⁔(Ꮡid);
}
partial class astᴛIfStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref IfStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref IfStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref IfStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛImportSpec {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ImportSpec s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ImportSpec s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void specNode(this ref ImportSpec _) => ast_package.specNode⁔(ref _);
}
partial class astᴛIncDecStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref IncDecStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref IncDecStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref IncDecStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛIndexExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref IndexExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref IndexExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref IndexExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛIndexListExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref IndexListExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref IndexListExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref IndexListExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛInterfaceType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref InterfaceType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref InterfaceType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref InterfaceType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛKeyValueExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref KeyValueExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref KeyValueExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref KeyValueExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛLabeledStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref LabeledStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref LabeledStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref LabeledStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛMapType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref MapType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref MapType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref MapType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛObjKind {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ObjKind kind) => ast_package.String⁔(kind);
}
partial class astᴛObject {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Object obj) => ast_package.Pos⁔(ref obj);
}
partial class astᴛPackage {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref Package p) => ast_package.Pos⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref Package p) => ast_package.End⁔(ref p);
}
partial class astᴛParenExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ParenExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ParenExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref ParenExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛRangeStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref RangeStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref RangeStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref RangeStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛReturnStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ReturnStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ReturnStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref ReturnStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛScope {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Object> Lookup(this ref Scope s, @string name) => ast_package.Lookup⁔(ref s, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Object> /*alt*/ Insert(this ref Scope s, ж<Object> Ꮡobj) => ast_package.Insert⁔(ref s, Ꮡobj);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Scope> Ꮡs) => ast_package.String⁔(Ꮡs);
}
partial class astᴛSelectStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref SelectStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref SelectStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref SelectStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛSelectorExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref SelectorExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref SelectorExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref SelectorExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛSendStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref SendStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref SendStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref SendStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛSliceExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref SliceExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref SliceExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref SliceExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛStarExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref StarExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref StarExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref StarExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛStructType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref StructType x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref StructType x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref StructType _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛSwitchStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref SwitchStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref SwitchStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref SwitchStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛTypeAssertExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref TypeAssertExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref TypeAssertExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref TypeAssertExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛTypeSpec {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref TypeSpec s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref TypeSpec s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void specNode(this ref TypeSpec _) => ast_package.specNode⁔(ref _);
}
partial class astᴛTypeSwitchStmt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref TypeSwitchStmt s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref TypeSwitchStmt s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stmtNode(this ref TypeSwitchStmt _) => ast_package.stmtNode⁔(ref _);
}
partial class astᴛUnaryExpr {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref UnaryExpr x) => ast_package.Pos⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref UnaryExpr x) => ast_package.End⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void exprNode(this ref UnaryExpr _) => ast_package.exprNode⁔(ref _);
}
partial class astᴛValueSpec {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos Pos(this ref ValueSpec s) => ast_package.Pos⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static token.Pos End(this ref ValueSpec s) => ast_package.End⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void specNode(this ref ValueSpec _) => ast_package.specNode⁔(ref _);
}
partial class astᴛcommentListReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool eol(this ref commentListReader r) => ast_package.eol⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ref commentListReader r) => ast_package.next⁔(ref r);
}
partial class astᴛdirectiveScanner {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skip(this ref directiveScanner s, nint n) => ast_package.skip⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string take(this ref directiveScanner s, nint n) => ast_package.take⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string takeNonSpace(this ref directiveScanner s) => ast_package.takeNonSpace⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void skipSpace(this ref directiveScanner s) => ast_package.skipSpace⁔(ref s);
}
partial class astᴛinspector {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Visitor Visit(this inspector f, Node node) => ast_package.Visit⁔(f, node);
}
partial class astᴛnodeStack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void push(this ref nodeStack s, Node n) => ast_package.push⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node /*top*/ pop(this ref nodeStack s, token.Pos pos) => ast_package.pop⁔(ref s, pos);
}
partial class astᴛpkgBuilder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref pkgBuilder p, token.Pos pos, @string msg) => ast_package.error⁔(ref p, pos, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ref pkgBuilder p, token.Pos pos, @string format, params ꓸꓸꓸany argsʗp) => ast_package.errorf⁔(ref p, pos, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declare(this ref pkgBuilder p, ж<Scope> Ꮡscope, ж<Scope> ᏑaltScope, ж<Object> Ꮡobj) => ast_package.declare⁔(ref p, Ꮡscope, ᏑaltScope, Ꮡobj);
}
partial class astᴛprinter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref printer p, slice<byte> data) => ast_package.Write⁔(ref p, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void printf(this ж<printer> Ꮡp, @string format, params ꓸꓸꓸany argsʗp) => ast_package.printf⁔(Ꮡp, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void print(this ж<printer> Ꮡp, reflect.Value x) => ast_package.print⁔(Ꮡp, x);
}
// </MethodForwarders>
