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
using ast = go.go.ast_package;
using token = go.go.token_package;
// </ImportedTypeAliases>

using go;
using static go.go.parser_package;
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
[assembly: GoFieldAccess(typeof(go.go.scanner_package.Error))]
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

using static global::go.go.parser_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸжastꓸIdent = Span<ж<global::go.go.ast_package.Ident>>;

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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(parser_package.parser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parserᴛparser;
[GoMethodHost(typeof(parser_package.resolver)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parserᴛresolver;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class parserᴛparser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<parser> Ꮡp, ж<token.File> Ꮡfile, slice<byte> src, Mode mode) => parser_package.init⁔(Ꮡp, Ꮡfile, src, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void printTrace(this ref parser p, params ꓸꓸꓸany aʗp) => parser_package.printTrace⁔(ref p, aʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next0(this ж<parser> Ꮡp) => parser_package.next0⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint lineFor(this ref parser p, token.Pos pos) => parser_package.lineFor⁔(ref p, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ast.Comment> comment, nint endline) consumeComment(this ж<parser> Ꮡp) => parser_package.consumeComment⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ast.CommentGroup> comments, nint endline) consumeCommentGroup(this ж<parser> Ꮡp, nint n) => parser_package.consumeCommentGroup⁔(Ꮡp, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void next(this ж<parser> Ꮡp) => parser_package.next⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ж<parser> Ꮡp, token.Pos pos, @string msg) => parser_package.error⁔(Ꮡp, pos, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorExpected(this ж<parser> Ꮡp, token.Pos pos, @string msg) => parser_package.errorExpected⁔(Ꮡp, pos, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos expect(this ж<parser> Ꮡp, token.Token tok) => parser_package.expect⁔(Ꮡp, tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos /*pos*/ expect2(this ж<parser> Ꮡp, token.Token tok) => parser_package.expect2⁔(Ꮡp, tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static token.Pos expectClosing(this ж<parser> Ꮡp, token.Token tok, @string context) => parser_package.expectClosing⁔(Ꮡp, tok, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.CommentGroup> /*comment*/ expectSemi(this ж<parser> Ꮡp) => parser_package.expectSemi⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool atComma(this ж<parser> Ꮡp, @string context, token.Token follow) => parser_package.atComma⁔(Ꮡp, context, follow);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void advance(this ж<parser> Ꮡp, map<token.Token, bool> to) => parser_package.advance⁔(Ꮡp, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.Ident> parseIdent(this ж<parser> Ꮡp) => parser_package.parseIdent⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<ast.Ident>> /*list*/ parseIdentList(this ж<parser> Ꮡp) => parser_package.parseIdentList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ast.Expr> /*list*/ parseExprList(this ж<parser> Ꮡp) => parser_package.parseExprList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ast.Expr> parseList(this ж<parser> Ꮡp, bool inRhs) => parser_package.parseList⁔(Ꮡp, inRhs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseType(this ж<parser> Ꮡp) => parser_package.parseType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseQualifiedIdent(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡident) => parser_package.parseQualifiedIdent⁔(Ꮡp, Ꮡident);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseTypeName(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡident) => parser_package.parseTypeName⁔(Ꮡp, Ꮡident);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.ArrayType> parseArrayType(this ж<parser> Ꮡp, token.Pos lbrack, ast.Expr len) => parser_package.parseArrayType⁔(Ꮡp, lbrack, len);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ast.Ident>, ast.Expr) parseArrayFieldOrTypeInstance(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡx) => parser_package.parseArrayFieldOrTypeInstance⁔(Ꮡp, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.Field> parseFieldDecl(this ж<parser> Ꮡp) => parser_package.parseFieldDecl⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.StructType> parseStructType(this ж<parser> Ꮡp) => parser_package.parseStructType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.StarExpr> parsePointerType(this ж<parser> Ꮡp) => parser_package.parsePointerType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.Ellipsis> parseDotsType(this ж<parser> Ꮡp) => parser_package.parseDotsType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static field /*f*/ parseParamDecl(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname, bool typeSetsOK) => parser_package.parseParamDecl⁔(Ꮡp, Ꮡname, typeSetsOK);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ж<ast.Field>> /*params*/ parseParameterList(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname0, ast.Expr typ0, token.Token closing, bool dddok) => parser_package.parseParameterList⁔(Ꮡp, Ꮡname0, typ0, closing, dddok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.FieldList> parseTypeParameters(this ж<parser> Ꮡp) => parser_package.parseTypeParameters⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.FieldList> parseParameters(this ж<parser> Ꮡp, bool result) => parser_package.parseParameters⁔(Ꮡp, result);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.FuncType> parseFuncType(this ж<parser> Ꮡp) => parser_package.parseFuncType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.Field> parseMethodSpec(this ж<parser> Ꮡp) => parser_package.parseMethodSpec⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr embeddedElem(this ж<parser> Ꮡp, ast.Expr x) => parser_package.embeddedElem⁔(Ꮡp, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr embeddedTerm(this ж<parser> Ꮡp) => parser_package.embeddedTerm⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.InterfaceType> parseInterfaceType(this ж<parser> Ꮡp) => parser_package.parseInterfaceType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.MapType> parseMapType(this ж<parser> Ꮡp) => parser_package.parseMapType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.ChanType> parseChanType(this ж<parser> Ꮡp) => parser_package.parseChanType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseTypeInstance(this ж<parser> Ꮡp, ast.Expr typ) => parser_package.parseTypeInstance⁔(Ꮡp, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr tryIdentOrType(this ж<parser> Ꮡp) => parser_package.tryIdentOrType⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ast.Stmt> /*list*/ parseStmtList(this ж<parser> Ꮡp) => parser_package.parseStmtList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.BlockStmt> parseBody(this ж<parser> Ꮡp) => parser_package.parseBody⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.BlockStmt> parseBlockStmt(this ж<parser> Ꮡp) => parser_package.parseBlockStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseFuncTypeOrLit(this ж<parser> Ꮡp) => parser_package.parseFuncTypeOrLit⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseOperand(this ж<parser> Ꮡp) => parser_package.parseOperand⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseSelector(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseSelector⁔(Ꮡp, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseTypeAssertion(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseTypeAssertion⁔(Ꮡp, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseIndexOrSliceOrInstance(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseIndexOrSliceOrInstance⁔(Ꮡp, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.CallExpr> parseCallOrConversion(this ж<parser> Ꮡp, ast.Expr fun) => parser_package.parseCallOrConversion⁔(Ꮡp, fun);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseValue(this ж<parser> Ꮡp) => parser_package.parseValue⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseElement(this ж<parser> Ꮡp) => parser_package.parseElement⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<ast.Expr> /*list*/ parseElementList(this ж<parser> Ꮡp) => parser_package.parseElementList⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseLiteralValue(this ж<parser> Ꮡp, ast.Expr typ) => parser_package.parseLiteralValue⁔(Ꮡp, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parsePrimaryExpr(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parsePrimaryExpr⁔(Ꮡp, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseUnaryExpr(this ж<parser> Ꮡp) => parser_package.parseUnaryExpr⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (token.Token, nint) tokPrec(this ref parser p) => parser_package.tokPrec⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseBinaryExpr(this ж<parser> Ꮡp, ast.Expr x, nint prec1) => parser_package.parseBinaryExpr⁔(Ꮡp, x, prec1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseExpr(this ж<parser> Ꮡp) => parser_package.parseExpr⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr parseRhs(this ж<parser> Ꮡp) => parser_package.parseRhs⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ast.Stmt, bool) parseSimpleStmt(this ж<parser> Ꮡp, nint mode) => parser_package.parseSimpleStmt⁔(Ꮡp, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.CallExpr> parseCallExpr(this ж<parser> Ꮡp, @string callType) => parser_package.parseCallExpr⁔(Ꮡp, callType);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Stmt parseGoStmt(this ж<parser> Ꮡp) => parser_package.parseGoStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Stmt parseDeferStmt(this ж<parser> Ꮡp) => parser_package.parseDeferStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.ReturnStmt> parseReturnStmt(this ж<parser> Ꮡp) => parser_package.parseReturnStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.BranchStmt> parseBranchStmt(this ж<parser> Ꮡp, token.Token tok) => parser_package.parseBranchStmt⁔(Ꮡp, tok);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Expr makeExpr(this ж<parser> Ꮡp, ast.Stmt s, @string want) => parser_package.makeExpr⁔(Ꮡp, s, want);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ast.Stmt init, ast.Expr cond) parseIfHeader(this ж<parser> Ꮡp) => parser_package.parseIfHeader⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.IfStmt> parseIfStmt(this ж<parser> Ꮡp) => parser_package.parseIfStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.CaseClause> parseCaseClause(this ж<parser> Ꮡp) => parser_package.parseCaseClause⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isTypeSwitchGuard(this ж<parser> Ꮡp, ast.Stmt s) => parser_package.isTypeSwitchGuard⁔(Ꮡp, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Stmt parseSwitchStmt(this ж<parser> Ꮡp) => parser_package.parseSwitchStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.CommClause> parseCommClause(this ж<parser> Ꮡp) => parser_package.parseCommClause⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.SelectStmt> parseSelectStmt(this ж<parser> Ꮡp) => parser_package.parseSelectStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Stmt parseForStmt(this ж<parser> Ꮡp) => parser_package.parseForStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Stmt /*s*/ parseStmt(this ж<parser> Ꮡp) => parser_package.parseStmt⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Spec parseImportSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token _Δp2, nint _Δp3) => parser_package.parseImportSpec⁔(Ꮡp, Ꮡdoc, _Δp2, _Δp3);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Spec parseValueSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token keyword, nint iota) => parser_package.parseValueSpec⁔(Ꮡp, Ꮡdoc, keyword, iota);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseGenericType(this ж<parser> Ꮡp, ж<ast.TypeSpec> Ꮡspec, token.Pos openPos, ж<ast.Ident> Ꮡname0, ast.Expr typ0) => parser_package.parseGenericType⁔(Ꮡp, Ꮡspec, openPos, Ꮡname0, typ0);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Spec parseTypeSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token _Δp2, nint _Δp3) => parser_package.parseTypeSpec⁔(Ꮡp, Ꮡdoc, _Δp2, _Δp3);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.GenDecl> parseGenDecl(this ж<parser> Ꮡp, token.Token keyword, parseSpecFunction f) => parser_package.parseGenDecl⁔(Ꮡp, keyword, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.FuncDecl> parseFuncDecl(this ж<parser> Ꮡp) => parser_package.parseFuncDecl⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Decl parseDecl(this ж<parser> Ꮡp, map<token.Token, bool> sync) => parser_package.parseDecl⁔(Ꮡp, sync);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ast.File> parseFile(this ж<parser> Ꮡp) => parser_package.parseFile⁔(Ꮡp);
}
partial class parserᴛresolver {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void trace(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) => parser_package.trace⁔(ref r, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string sprintf(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) => parser_package.sprintf⁔(ref r, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openScope(this ref resolver r, token.Pos pos) => parser_package.openScope⁔(ref r, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeScope(this ref resolver r) => parser_package.closeScope⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void openLabelScope(this ref resolver r) => parser_package.openLabelScope⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeLabelScope(this ref resolver r) => parser_package.closeLabelScope⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declare(this ref resolver r, any decl, any data, ж<ast.Scope> Ꮡscope, ast.ObjKind kind, params ꓸꓸꓸжastꓸIdent identsʗp) => parser_package.declare⁔(ref r, decl, data, Ꮡscope, kind, identsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shortVarDecl(this ref resolver r, ж<ast.AssignStmt> Ꮡdecl) => parser_package.shortVarDecl⁔(ref r, Ꮡdecl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolve(this ref resolver r, ж<ast.Ident> Ꮡident, bool collectUnresolved) => parser_package.resolve⁔(ref r, Ꮡident, collectUnresolved);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkExprs(this ж<resolver> Ꮡr, slice<ast.Expr> list) => parser_package.walkExprs⁔(Ꮡr, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkLHS(this ж<resolver> Ꮡr, slice<ast.Expr> list) => parser_package.walkLHS⁔(Ꮡr, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkStmts(this ж<resolver> Ꮡr, slice<ast.Stmt> list) => parser_package.walkStmts⁔(Ꮡr, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ast.Visitor Visit(this ж<resolver> Ꮡr, ast.Node node) => parser_package.Visit⁔(Ꮡr, node);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkFuncType(this ж<resolver> Ꮡr, ж<ast.FuncType> Ꮡtyp) => parser_package.walkFuncType⁔(Ꮡr, Ꮡtyp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void resolveList(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist) => parser_package.resolveList⁔(Ꮡr, Ꮡlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void declareList(this ref resolver r, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) => parser_package.declareList⁔(ref r, Ꮡlist, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkRecv(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡrecv) => parser_package.walkRecv⁔(Ꮡr, Ꮡrecv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkFieldList(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) => parser_package.walkFieldList⁔(Ꮡr, Ꮡlist, kind);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkTParams(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist) => parser_package.walkTParams⁔(Ꮡr, Ꮡlist);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void walkBody(this ж<resolver> Ꮡr, ж<ast.BlockStmt> Ꮡbody) => parser_package.walkBody⁔(Ꮡr, Ꮡbody);
}
// </MethodForwarders>
