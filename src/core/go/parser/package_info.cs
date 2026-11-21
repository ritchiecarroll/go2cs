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
[assembly: GoPositionMap("go/parser/parser.go", "parser.cs", "AE6eAbKCgJKUgoKC7MKCgoKCgoKCpoLGooKCqLKCAAkMooKCgpSqogAICPqCgpS0tMiCgoKCgqaCpoK4lLqSqPaClIKCyoKUrgAIAoKCgoKCuIKUAAIiABACgoKClIKClKaCpsqCgqamAA8YwoKmyoKCgpSCuKKkooKmlMa0tqaigoKUgqrSgpSUgqrCgoKUqMKktoKklIKmgpSkgoIACAiigpSCgoKUgpTGgoLcwoIABxCCgpSCgoIADBYAECLuAAoYwoKCgoKUlObSgqaCgoKWAAkOAAkCgqaCgoKW1qKCgoKCAAgM4oKmlIKCgoKmAAkGwoKmgoKmAAkI4oKmgqaUgoKmAAsK4oKmgpSCgoKClJS4gpSCggAJBsKCpoKCgoKCgoKCgoKClJSUlJSCuIKClJSUygANBsKCppSCgpSClIKCuIKCgriCgoK42IKClIKClIK4lLaCgpSCgqamgsiCgoK2goKCppSCAAkGwoKmgoKCuJSUABAUwoKmgpQACQbCgqaClAASEAAIBoKmgoKClKamgoKUlKbIyIK4graCgsiUgoL82oLKgsiClgAbBvKCuJSCgoKCAAgSgoKUgoKCgpSUlIKCgoKClIKmgpSmgriUgoKCgqaGooKSgpSCgoKmlISSgoKCgoKCgoKUgqaCpo7igoKClKaClIKmpsqCgoKCgoKClNzclIKClKqigoKCgoKUgoKClJSUgpQACwbCgqaCgoKUlIKCpgAJBsKCpoKCgoKUgqaCgoKmAAsGwoKmlIKCgqaClAANBsKCpoKCgoKCpoKCgoKCyoKCpoKCggAHEIKCgoKCgoKUlJSC6oKCgsbIgpQABxAACQbCgpSClIKCgoKCgoKUAAsGwoKUgoKCgoKCpoKCgoKCpgAJBsKCpoKUlIKClIKClIK0goK0goKClAAJDpQAEBTCgqaCgoKClAAJBsKCpoKCgoKCgoKCpoKClJQACQbCgqaCgoKCgoKUlJSUgoIABxDWwpSUgoKUpIKkpKSkpKSkgoKCgrgACQzygqaClgAJBsKCpoKClAAJBsKCpoKClAAJDOKCpoKUpoKClAANCuKCppSCpoKClIKCpoKCgoKCgqamlIKCuIKCggAJBsKCppQACQbCgqaCgpSUlJQACwbCgqaCpoKCgu6UgoKCgqaUgqaCgoKCguqCgoKC2oKUlIKCpoKClIKCpqaUuAALBsKCpoKCgoKCgoKClIKUlIKUAAkGwoKmgqaU1sKCpoKCgoKmAAsG0oKmgoKClJYACQbClIKmgoKCgpSCggANBsKCpoKasoCSgoKUgpS0tILcgpSC1rTYlLSC6IIACwyUgqa0AA8KwpSCppSygoK4ggAPIKa4goKUlIKClIKmuLiCgoKm1oKCgpTuAAgCgqaCmrKAkoKCkoKUgoIACgrSgqbWooKCgoIAFiIACAKCppSusoKCgoKCgoKUlKaCuKaCgriCAAcQgriCgoK4goK41oKCgoKUgpSUlObCgqaCgoKCpgALBsKCpoKCgoKmAAkGwoKmgoKCgpSUAAkGwoKmgoKClJQACwaCgpSClIKClIIAFQzygoKCuIKUlIKClKaCiIKCgoKUlIKmgqaCgoKClMqCpoIACAbClIKmlIKUgoKClLSCtIK2pgAJBsKCpoKCgoKUpoKU9oKCAAgGgqa2graCpMgACAbCgqaUkoKCgoKUgoKCggAMGqamgoKCgpSCgpSCpgALBsKCpoKCgoKClIKmgoKCppSClJSCgoKmgqa4poKUAAkGwoKmgoKCgpSCgpQADQbCgqaUooKCgoKClIKCgoKUpoKCgoKClIKCpqaClIKEoti0tIK4ggAMGgAREtKUgqaU7LiCxrS0tLSCtLS0tOqCxsaCgoKmABMQ4oKmgpS0graCgoKCgoKCgoKUgpSm3JTWwoKmgoKCpoKCgoLYgpSCgsaklO4ACQbSgqaCgoKUgpSi5MKCpoKUpoKCAAAgAA8CuIKCggAIEri4uLiUgpSmlAAHKgASApSklICU1oCUgoL4gIK4AAkQqqL0pKSklMbCgqaCgpKCgoKCgpSClKYAFBTCgqaClIKCppSCgoKmgqaClIKUgrSClIKCxrYACxgACwbCgqaClLa2tqaCgoKmAA0M4oLKgriCpoKClLiCpoKUgqaUgpSClJTKAAkUgoKUgqb4kpS0/g==", "81-81:1;1044-1049:1;1731-1731:1;1887-1887:1", "", "3280=Pos/1/1/6;3293=makeExpr/1/1/3", "1798:21,2284:21,2431:21,2929:21,3317:21", "2328:21,2329:13,2539:21,2540:13")]
[assembly: GoPositionMap("go/parser/resolver.go", "resolver.cs", "ACAq0oIABxCCpoKCpoKUgoKCgoKCgqaCABQqoqaigpS25oKCgpSClOaCgoKUpoKCppSCgoKCgriC5rKCgpSmgqaClIKClIKCgpQACwzYgoKCgpSCgoKClIKUyoIACA4ACQzigriClIKCgpSmgpTugoK4goK4goKCgsqCgvjCgqaotoKCgrbagoK2goK2gpSCuIKUlJTagoLIgraCgpTagoLIgoK2goKClIKCgsiCgoK2goKClMqCgpSUgsiCgoKUgoKmgsiCgoKU2oLIgoKClIKUgpS2goKCgoKUgpSCygAIEoKUpsiUgoKCgpSCgpTGgriCgoKClPyClLiC3IKCgoKUgoLYptbEgoKCpqKClIKCyqKClIK4+IKUgoKmgoKUgrSCxKSCgrimgoK4goLKooKUgqzSgqbigpSCgqI=")]
// </GoSourcePositionMaps>

namespace go.go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using scannerhooks = global::go.go.@internal.scannerhooks_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using global::go.go.@internal;
using global::go.go.build;
using ꓸꓸꓸany = Span<any>;
using static parser_package;
using ꓸꓸꓸжastꓸIdent = Span<ж<global::go.go.ast_package.Ident>>;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(parser_package.parser)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parserᴛparser;
[GoMethodHost(typeof(parser_package.resolver)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parserᴛresolver;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class parserᴛparser {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<parser> Ꮡp, ж<token.File> Ꮡfile, slice<byte> src, Mode mode) => parser_package.initᴍ(Ꮡp, Ꮡfile, src, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void printTrace(this ref parser p, params ꓸꓸꓸany aʗp) => parser_package.printTraceᴍ(ref p, aʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next0(this ж<parser> Ꮡp) => parser_package.next0ᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint lineFor(this ref parser p, token.Pos pos) => parser_package.lineForᴍ(ref p, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ast.Comment> comment, nint endline) consumeComment(this ж<parser> Ꮡp) => parser_package.consumeCommentᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ast.CommentGroup> comments, nint endline) consumeCommentGroup(this ж<parser> Ꮡp, nint n) => parser_package.consumeCommentGroupᴍ(Ꮡp, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void next(this ж<parser> Ꮡp) => parser_package.nextᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ж<parser> Ꮡp, token.Pos pos, @string msg) => parser_package.errorᴍ(Ꮡp, pos, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorExpected(this ж<parser> Ꮡp, token.Pos pos, @string msg) => parser_package.errorExpectedᴍ(Ꮡp, pos, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos expect(this ж<parser> Ꮡp, token.Token tok) => parser_package.expectᴍ(Ꮡp, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos /*pos*/ expect2(this ж<parser> Ꮡp, token.Token tok) => parser_package.expect2ᴍ(Ꮡp, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Pos expectClosing(this ж<parser> Ꮡp, token.Token tok, @string context) => parser_package.expectClosingᴍ(Ꮡp, tok, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.CommentGroup> /*comment*/ expectSemi(this ж<parser> Ꮡp) => parser_package.expectSemiᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool atComma(this ж<parser> Ꮡp, @string context, token.Token follow) => parser_package.atCommaᴍ(Ꮡp, context, follow);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void advance(this ж<parser> Ꮡp, map<token.Token, bool> to) => parser_package.advanceᴍ(Ꮡp, to);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.Ident> parseIdent(this ж<parser> Ꮡp) => parser_package.parseIdentᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<ast.Ident>> /*list*/ parseIdentList(this ж<parser> Ꮡp) => parser_package.parseIdentListᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ast.Expr> /*list*/ parseExprList(this ж<parser> Ꮡp) => parser_package.parseExprListᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ast.Expr> parseList(this ж<parser> Ꮡp, bool inRhs) => parser_package.parseListᴍ(Ꮡp, inRhs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseType(this ж<parser> Ꮡp) => parser_package.parseTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseQualifiedIdent(this ж<parser> Ꮡp, ж<ast.Ident> ident) => parser_package.parseQualifiedIdentᴍ(Ꮡp, ident);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseTypeName(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡident) => parser_package.parseTypeNameᴍ(Ꮡp, Ꮡident);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.ArrayType> parseArrayType(this ж<parser> Ꮡp, token.Pos lbrack, ast.Expr len) => parser_package.parseArrayTypeᴍ(Ꮡp, lbrack, len);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ast.Ident>, ast.Expr) parseArrayFieldOrTypeInstance(this ж<parser> Ꮡp, ж<ast.Ident> x) => parser_package.parseArrayFieldOrTypeInstanceᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.Field> parseFieldDecl(this ж<parser> Ꮡp) => parser_package.parseFieldDeclᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.StructType> parseStructType(this ж<parser> Ꮡp) => parser_package.parseStructTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.StarExpr> parsePointerType(this ж<parser> Ꮡp) => parser_package.parsePointerTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.Ellipsis> parseDotsType(this ж<parser> Ꮡp) => parser_package.parseDotsTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static field /*f*/ parseParamDecl(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname, bool typeSetsOK) => parser_package.parseParamDeclᴍ(Ꮡp, Ꮡname, typeSetsOK);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ж<ast.Field>> /*params*/ parseParameterList(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname0, ast.Expr typ0, token.Token closing, bool dddok) => parser_package.parseParameterListᴍ(Ꮡp, Ꮡname0, typ0, closing, dddok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.FieldList> parseTypeParameters(this ж<parser> Ꮡp) => parser_package.parseTypeParametersᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.FieldList> parseParameters(this ж<parser> Ꮡp, bool result) => parser_package.parseParametersᴍ(Ꮡp, result);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.FuncType> parseFuncType(this ж<parser> Ꮡp) => parser_package.parseFuncTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.Field> parseMethodSpec(this ж<parser> Ꮡp) => parser_package.parseMethodSpecᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr embeddedElem(this ж<parser> Ꮡp, ast.Expr x) => parser_package.embeddedElemᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr embeddedTerm(this ж<parser> Ꮡp) => parser_package.embeddedTermᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.InterfaceType> parseInterfaceType(this ж<parser> Ꮡp) => parser_package.parseInterfaceTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.MapType> parseMapType(this ж<parser> Ꮡp) => parser_package.parseMapTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.ChanType> parseChanType(this ж<parser> Ꮡp) => parser_package.parseChanTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseTypeInstance(this ж<parser> Ꮡp, ast.Expr typ) => parser_package.parseTypeInstanceᴍ(Ꮡp, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr tryIdentOrType(this ж<parser> Ꮡp) => parser_package.tryIdentOrTypeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ast.Stmt> /*list*/ parseStmtList(this ж<parser> Ꮡp) => parser_package.parseStmtListᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.BlockStmt> parseBody(this ж<parser> Ꮡp) => parser_package.parseBodyᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.BlockStmt> parseBlockStmt(this ж<parser> Ꮡp) => parser_package.parseBlockStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseFuncTypeOrLit(this ж<parser> Ꮡp) => parser_package.parseFuncTypeOrLitᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseOperand(this ж<parser> Ꮡp) => parser_package.parseOperandᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseSelector(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseSelectorᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseTypeAssertion(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseTypeAssertionᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseIndexOrSliceOrInstance(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parseIndexOrSliceOrInstanceᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.CallExpr> parseCallOrConversion(this ж<parser> Ꮡp, ast.Expr fun) => parser_package.parseCallOrConversionᴍ(Ꮡp, fun);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseValue(this ж<parser> Ꮡp) => parser_package.parseValueᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseElement(this ж<parser> Ꮡp) => parser_package.parseElementᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<ast.Expr> /*list*/ parseElementList(this ж<parser> Ꮡp) => parser_package.parseElementListᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseLiteralValue(this ж<parser> Ꮡp, ast.Expr typ) => parser_package.parseLiteralValueᴍ(Ꮡp, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parsePrimaryExpr(this ж<parser> Ꮡp, ast.Expr x) => parser_package.parsePrimaryExprᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseUnaryExpr(this ж<parser> Ꮡp) => parser_package.parseUnaryExprᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (token.Token, nint) tokPrec(this ref parser p) => parser_package.tokPrecᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseBinaryExpr(this ж<parser> Ꮡp, ast.Expr x, nint prec1) => parser_package.parseBinaryExprᴍ(Ꮡp, x, prec1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseExpr(this ж<parser> Ꮡp) => parser_package.parseExprᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr parseRhs(this ж<parser> Ꮡp) => parser_package.parseRhsᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ast.Stmt, bool) parseSimpleStmt(this ж<parser> Ꮡp, nint mode) => parser_package.parseSimpleStmtᴍ(Ꮡp, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.CallExpr> parseCallExpr(this ж<parser> Ꮡp, @string callType) => parser_package.parseCallExprᴍ(Ꮡp, callType);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Stmt parseGoStmt(this ж<parser> Ꮡp) => parser_package.parseGoStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Stmt parseDeferStmt(this ж<parser> Ꮡp) => parser_package.parseDeferStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.ReturnStmt> parseReturnStmt(this ж<parser> Ꮡp) => parser_package.parseReturnStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.BranchStmt> parseBranchStmt(this ж<parser> Ꮡp, token.Token tok) => parser_package.parseBranchStmtᴍ(Ꮡp, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Expr makeExpr(this ж<parser> Ꮡp, ast.Stmt s, @string want) => parser_package.makeExprᴍ(Ꮡp, s, want);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ast.Stmt init, ast.Expr cond) parseIfHeader(this ж<parser> Ꮡp) => parser_package.parseIfHeaderᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.IfStmt> parseIfStmt(this ж<parser> Ꮡp) => parser_package.parseIfStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.CaseClause> parseCaseClause(this ж<parser> Ꮡp) => parser_package.parseCaseClauseᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isTypeSwitchGuard(this ж<parser> Ꮡp, ast.Stmt s) => parser_package.isTypeSwitchGuardᴍ(Ꮡp, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Stmt parseSwitchStmt(this ж<parser> Ꮡp) => parser_package.parseSwitchStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.CommClause> parseCommClause(this ж<parser> Ꮡp) => parser_package.parseCommClauseᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.SelectStmt> parseSelectStmt(this ж<parser> Ꮡp) => parser_package.parseSelectStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Stmt parseForStmt(this ж<parser> Ꮡp) => parser_package.parseForStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Stmt /*s*/ parseStmt(this ж<parser> Ꮡp) => parser_package.parseStmtᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Spec parseImportSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> doc, token.Token _Δp2, nint _Δp3) => parser_package.parseImportSpecᴍ(Ꮡp, doc, _Δp2, _Δp3);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Spec parseValueSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> doc, token.Token keyword, nint iota) => parser_package.parseValueSpecᴍ(Ꮡp, doc, keyword, iota);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseGenericType(this ж<parser> Ꮡp, ж<ast.TypeSpec> Ꮡspec, token.Pos openPos, ж<ast.Ident> name0, ast.Expr typ0) => parser_package.parseGenericTypeᴍ(Ꮡp, Ꮡspec, openPos, name0, typ0);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Spec parseTypeSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> doc, token.Token _Δp2, nint _Δp3) => parser_package.parseTypeSpecᴍ(Ꮡp, doc, _Δp2, _Δp3);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.GenDecl> parseGenDecl(this ж<parser> Ꮡp, token.Token keyword, Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f) => parser_package.parseGenDeclᴍ(Ꮡp, keyword, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.FuncDecl> parseFuncDecl(this ж<parser> Ꮡp) => parser_package.parseFuncDeclᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Decl parseDecl(this ж<parser> Ꮡp, map<token.Token, bool> sync) => parser_package.parseDeclᴍ(Ꮡp, sync);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ast.File> parseFile(this ж<parser> Ꮡp) => parser_package.parseFileᴍ(Ꮡp);
}
partial class parserᴛresolver {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void trace(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) => parser_package.traceᴍ(ref r, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string sprintf(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) => parser_package.sprintfᴍ(ref r, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openScope(this ref resolver r, token.Pos pos) => parser_package.openScopeᴍ(ref r, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeScope(this ref resolver r) => parser_package.closeScopeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void openLabelScope(this ref resolver r) => parser_package.openLabelScopeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeLabelScope(this ref resolver r) => parser_package.closeLabelScopeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declare(this ref resolver r, any decl, any data, ж<ast.Scope> Ꮡscope, ast.ObjKind kind, params ꓸꓸꓸжastꓸIdent identsʗp) => parser_package.declareᴍ(ref r, decl, data, Ꮡscope, kind, identsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void shortVarDecl(this ref resolver r, ж<ast.AssignStmt> Ꮡdecl) => parser_package.shortVarDeclᴍ(ref r, Ꮡdecl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resolve(this ref resolver r, ж<ast.Ident> Ꮡident, bool collectUnresolved) => parser_package.resolveᴍ(ref r, Ꮡident, collectUnresolved);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkExprs(this ж<resolver> Ꮡr, slice<ast.Expr> list) => parser_package.walkExprsᴍ(Ꮡr, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkLHS(this ж<resolver> Ꮡr, slice<ast.Expr> list) => parser_package.walkLHSᴍ(Ꮡr, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkStmts(this ж<resolver> Ꮡr, slice<ast.Stmt> list) => parser_package.walkStmtsᴍ(Ꮡr, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ast.Visitor Visit(this ж<resolver> Ꮡr, ast.Node node) => parser_package.Visitᴍ(Ꮡr, node);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkFuncType(this ж<resolver> Ꮡr, ж<ast.FuncType> Ꮡtyp) => parser_package.walkFuncTypeᴍ(Ꮡr, Ꮡtyp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resolveList(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡlist) => parser_package.resolveListᴍ(Ꮡr, Ꮡlist);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declareList(this ref resolver r, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) => parser_package.declareListᴍ(ref r, Ꮡlist, kind);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkRecv(this ж<resolver> Ꮡr, ж<ast.FieldList> Ꮡrecv) => parser_package.walkRecvᴍ(Ꮡr, Ꮡrecv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkFieldList(this ж<resolver> Ꮡr, ж<ast.FieldList> list, ast.ObjKind kind) => parser_package.walkFieldListᴍ(Ꮡr, list, kind);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkTParams(this ж<resolver> Ꮡr, ж<ast.FieldList> list) => parser_package.walkTParamsᴍ(Ꮡr, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void walkBody(this ж<resolver> Ꮡr, ж<ast.BlockStmt> Ꮡbody) => parser_package.walkBodyᴍ(Ꮡr, Ꮡbody);
}
// </GlyphForwarders>
