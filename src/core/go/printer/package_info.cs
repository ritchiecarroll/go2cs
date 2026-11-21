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
using static go.go.printer_package;

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
[assembly: GoImplement<go.text.tabwriter_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<sizeCounter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<trimmer, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(CommentedNode))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ArrayType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.AssignStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BasicLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BinaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BlockStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.BranchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CallExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CaseClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ChanType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommClause))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Comment))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CommentGroup))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.CompositeLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.DeclStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.DeferStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ellipsis))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ExprStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Field))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FieldList))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.File))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ForStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.FuncLit))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GenDecl))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.GoStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.Ident))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IfStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ImportSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IncDecStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.IndexListExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.InterfaceType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.KeyValueExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.LabeledStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.MapType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ParenExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.RangeStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ReturnStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SelectorExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SendStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SliceExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StarExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.StructType))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.SwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeAssertExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSpec))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.TypeSwitchStmt))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.UnaryExpr))]
[assembly: GoFieldAccess(typeof(go.go.ast_package.ValueSpec))]
[assembly: GoFieldAccess(typeof(printer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("go/printer/comment.go", "comment.cs", "AA0exKKCgoKCAAkUlIKCgoKCgoKmgoKUgpSmpoKokpSCpoKCuKiSgoKCgoKClJTKgriS3AAKDPqCyoKClIKClIKCpqqigoKCgpSCuA==")]
[assembly: GoPositionMap("go/printer/gobuild.go", "gobuild.cs", "ABAaooIACRSClIKCpoKCgriClJSC3IKCgqaCpoKCgoKUgpTqtoK4gpSCpoKCgoKCgoKUgoKCuKaClpKCgoKUgpSCgqaCggAEENKUlMaCgoKUgpSmgoKUgoKUxoI=", "", "", "150=isNL/2/3/1,isNL/3/3/1")]
[assembly: GoPositionMap("go/printer/nodes.go", "nodes.cs", "ABhcABsCgoKCgoKClIKCpt7ygpSUgriClJSCyoIADBq0goKUgoKUAAkaAAoCgoKCgoKUgqamgoKClJSCpoKUlIKUAAcQgoLKgoKCuNyCpoKC7tyCgoKClIK4AAcQgoKClIKCgriCuIKUgoK4goKCgtyCgqaCuNyCgoKUpoKCpqaUgoKClJSUgqaCgqaUAA0YspKClIKCgoKCuIKUgriClKaUgoKm7oKmgsqCgoLKuIK4ggAEENKmpKSUqqL0pKSklMaigpSClJSCgpSClIKU2KKCgpSCgqbGgoKUgoKWkoKClIKmguaygoKCgpSUlIKCgoKCpoKCgoKClJSUgpSUgoKCpqaCgoLKgoKCgqaEgoKUgoKClIKCgpSCgoKmgpSCgpSCgpSCgpSmgoKUgqqCgoKCgpS4goKUlIKClIKCpoKUlIKClIKogoLM4pS0tpSmlIKCgraUppSCgoK2gsiktMamgoKClIKClJSClKaCgoKUpoKCgpQABE4AJwKCuIKCgqaUgoKClIKCgoKmgoKmgpSCgtiCggAIBrKUlLa2goKUtoKCgraClIKCgqaCyIKUgoKmgpSUyIKUtoKUgoK2ppSCgoLItoKCgoKClJSCyIKCgoKC2oKCgoKCyIKCgoKClpKCgoKCgoKCuIKmgoKClIKCpoKmgraCyoKUtKSClIKCpoKCgoKCgoKmlIKCgtqClIKCgrimgriCgoK2goLIgoKUgraCtoK2graCgoK2lLS0goKkgsYABhgACgKClILcgraAgoK2goKC2JSAguiCgpS0tKS0psaCgpSCqtKCgoKCgoKClJSCgqaCpoKCAAISAAgCgpSCgpSmppSCuIKCgpSClKaCusKCgoKCgsaClKSUpoKAuIKmpIKUppSCtqaCgpTGgoKClIKCyoKUgoKClIKCgoKCuIK+6IKCgpSCgoKCgqaUlJSUptailJS2AAoSgoKCgoKCgoKCppS2graCgoKCgraCgoK2goKUgoKCgraCtoK2goLcgqaClNqCgoLItoKCgoKCpOqCguqCgpSUgoK2goK2goKCgpSCgoK2goKUlIKCtoKClIKCgpTIgoK2goKCpoKClIKClIKCgsYABj4AGwKUkoKCyoKCgoKClIKmlIKmgqaUpsaigoKCgoKUgpSCgoKUgoKUAAkIAAoSgpSSggAKFoKUgoKCyoKClPzSlIKCgpSCgraClIKCgoKUgoKUtoKCgpSClJSClILGyLKCgpSUgoKCgqaCgoKClIKmgoKClIKmlIKElAAOFoKCgoKCuILOAAoKgqaCyoKCgpSUgpSokoKCpqjCgoKUlJSmgoKClIKUlAACEAAKAoK4gpSUgoKCgoKCgoKClJSUgoKCpoKUoqqygpSmsoKCuIKCgpSCgtailIK0tMTuwoKUtKTGooKCggAHEKaCgriUuLKCgoKCgg==", "1251-1264:1;1578-1584:1;1878-1880:1", "", "", "", "2112:31,2113:13")]
[assembly: GoPositionMap("go/printer/printer.go", "printer.cs", "AF3GAaKCgoK+xIKClJSCpoKmgoKCgoKCgoLcrLKq9IKmgoKClJTe4q7CppSmgoKClKiSgoKClKaCgpSCuraCgriCgqqiypS0gsiUpoK4goKCgoKClIIAAhoACgKCgpSmyqbKpoKUpoKClJSCuKaCgoKCgoKUgqaCpgACEgAIApSmlIKmpoKUgoKmgtiCxqSClKaCgriUzIKCgqaCxvyCAAcQgsaCpIKUpoKCgoIABxCCyoKmuO6igoKmqJKCgpSokgACENKCABYugoKCgoKUgoKUzIKC3IKUggAIEoLcgoKUgpSmgoKCgpSUpoK47oKCgpSClMq4goIACgrSgpSCpoCSuIKCgoKUgsoABxCCgri4goKClILGAAIQAAkCgqYACQyCgpSClMimgoKmqJKCgqYABRAACQKCgoKCuoKUppSClIKmgoKCuIKCgpSmAAoWgoqClMqGlMqC2KSC2KSCgoIACRLcgoKUpMqCzsKmopS0tLS0tKTGgoIABhwADQKEkoKm2Ma2poK2uJSCuIKUgoLKlIK2goK2goKCtoLugpSClJSWpMiCgoLGgsiCyoKUgpSCgoKUgsqCgqaCAAMQAAgClKaUyJKUpKSkpKSklKaClKSkpKSCxoK2xqSSgoKmlIKClILKgpSCgsqCgpSCgpSClJS4ppSmlNiClLS02IKCprS0xLaUggAaMoKCAAkStAAIDIKCsoKUlJS0grSCgrSCgtaCgsaUgoK0goKCxoKC1qSCppSUgrYAJE4AChiCggAJFMa0gqbI9IKSgqaCuNymgpSCgqaCgoKmuIK4gpYAEBzCzsI=", "154-156:1;637-637:1", "", "796=Pos/1/1/1,posFor/1/2/1,End/1/1/2,posFor/2/2/2;842=Pos/1/1/1,lineFor/1/1/1", "", "155:37,156:13,671:36,672:17")]
// </GoSourcePositionMaps>

namespace go.go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using global::go.go.build;
using text;
using static printer_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using global::go.go;
using global::go.unicode;
using io = io_package;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

[GoPackage("printer")]
public static partial class printer_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct commentInfo {}
    internal partial struct exprListMode {}
    internal partial struct paramMode {}
    internal partial struct pmode {}
    internal partial struct printer {}
    internal partial struct sizeCounter {}
    internal partial struct trimmer {}
    internal partial struct whiteSpace {}
    public partial struct CommentedNode {}
    public partial struct Config {}
    public partial struct Mode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdocꓸcomment() => builtin.initPackage(typeof(global::go.go.doc.comment_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtabwriter() => builtin.initPackage(typeof(text.tabwriter_package));
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
[GoMethodHost(typeof(printer_package.Config)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class printerᴛConfig;
[GoMethodHost(typeof(printer_package.printer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class printerᴛprinter;
[GoMethodHost(typeof(printer_package.sizeCounter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class printerᴛsizeCounter;
[GoMethodHost(typeof(printer_package.trimmer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class printerᴛtrimmer;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class printerᴛprinter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fixGoBuildLines(this ref printer p) => printer_package.fixGoBuildLinesᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> lineAt(this ref printer p, nint start) => printer_package.lineAtᴍ(ref p, start);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string commentTextAt(this ref printer p, nint start) => printer_package.commentTextAtᴍ(ref p, start);
}
partial class printerᴛprinter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*nbreaks*/ linebreak(this ж<printer> Ꮡp, nint line, nint min, whiteSpace ws, bool newSection) => printer_package.linebreakᴍ(Ꮡp, line, min, ws, newSection);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setComment(this ж<printer> Ꮡp, ж<ast.CommentGroup> Ꮡg) => printer_package.setCommentᴍ(Ꮡp, Ꮡg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void identList(this ж<printer> Ꮡp, slice<ж<ast.Ident>> list, bool indent) => printer_package.identListᴍ(Ꮡp, list, indent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void exprList(this ж<printer> Ꮡp, token.Pos prev0, slice<ast.Expr> list, nint depth, exprListMode mode, token.Pos next0, bool isIncomplete) => printer_package.exprListᴍ(Ꮡp, prev0, list, depth, mode, next0, isIncomplete);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parameters(this ж<printer> Ꮡp, ж<ast.FieldList> Ꮡfields, paramMode mode) => printer_package.parametersᴍ(Ꮡp, Ꮡfields, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void signature(this ж<printer> Ꮡp, ж<ast.FuncType> Ꮡsig) => printer_package.signatureᴍ(Ꮡp, Ꮡsig);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isOneLineFieldList(this ref printer p, slice<ж<ast.Field>> list) => printer_package.isOneLineFieldListᴍ(ref p, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setLineComment(this ж<printer> Ꮡp, @string text) => printer_package.setLineCommentᴍ(Ꮡp, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void fieldList(this ж<printer> Ꮡp, ж<ast.FieldList> Ꮡfields, bool isStruct, bool isIncomplete) => printer_package.fieldListᴍ(Ꮡp, Ꮡfields, isStruct, isIncomplete);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void binaryExpr(this ж<printer> Ꮡp, ж<ast.BinaryExpr> Ꮡx, nint prec1, nint cutoff, nint depth) => printer_package.binaryExprᴍ(Ꮡp, Ꮡx, prec1, cutoff, depth);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expr1(this ж<printer> Ꮡp, ast.Expr expr, nint prec1, nint depth) => printer_package.expr1ᴍ(Ꮡp, expr, prec1, depth);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool possibleSelectorExpr(this ж<printer> Ꮡp, ast.Expr expr, nint prec1, nint depth) => printer_package.possibleSelectorExprᴍ(Ꮡp, expr, prec1, depth);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool selectorExpr(this ж<printer> Ꮡp, ж<ast.SelectorExpr> Ꮡx, nint depth, bool isMethod) => printer_package.selectorExprᴍ(Ꮡp, Ꮡx, depth, isMethod);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expr0(this ж<printer> Ꮡp, ast.Expr x, nint depth) => printer_package.expr0ᴍ(Ꮡp, x, depth);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void expr(this ж<printer> Ꮡp, ast.Expr x) => printer_package.exprᴍ(Ꮡp, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stmtList(this ж<printer> Ꮡp, slice<ast.Stmt> list, nint nindent, bool nextIsRBrace) => printer_package.stmtListᴍ(Ꮡp, list, nindent, nextIsRBrace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void block(this ж<printer> Ꮡp, ж<ast.BlockStmt> Ꮡb, nint nindent) => printer_package.blockᴍ(Ꮡp, Ꮡb, nindent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void controlClause(this ж<printer> Ꮡp, bool isForStmt, ast.Stmt init, ast.Expr expr, ast.Stmt post) => printer_package.controlClauseᴍ(Ꮡp, isForStmt, init, expr, post);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool indentList(this ref printer p, slice<ast.Expr> list) => printer_package.indentListᴍ(ref p, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stmt(this ж<printer> Ꮡp, ast.Stmt stmt, bool nextIsRBrace) => printer_package.stmtᴍ(Ꮡp, stmt, nextIsRBrace);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void valueSpec(this ж<printer> Ꮡp, ж<ast.ValueSpec> Ꮡs, bool keepType) => printer_package.valueSpecᴍ(Ꮡp, Ꮡs, keepType);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void spec(this ж<printer> Ꮡp, ast.Spec spec, nint n, bool doIndent) => printer_package.specᴍ(Ꮡp, spec, n, doIndent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void genDecl(this ж<printer> Ꮡp, ж<ast.GenDecl> Ꮡd) => printer_package.genDeclᴍ(Ꮡp, Ꮡd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*size*/ nodeSize(this ref printer p, ast.Node n, nint maxSize) => printer_package.nodeSizeᴍ(ref p, n, maxSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint numLines(this ref printer p, ast.Node n) => printer_package.numLinesᴍ(ref p, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint bodySize(this ж<printer> Ꮡp, ж<ast.BlockStmt> Ꮡb, nint maxSize) => printer_package.bodySizeᴍ(Ꮡp, Ꮡb, maxSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcBody(this ж<printer> Ꮡp, nint headerSize, whiteSpace sep, ж<ast.BlockStmt> Ꮡb) => printer_package.funcBodyᴍ(Ꮡp, headerSize, sep, Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint distanceFrom(this ref printer p, token.Pos startPos, nint startOutCol) => printer_package.distanceFromᴍ(ref p, startPos, startOutCol);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void funcDecl(this ж<printer> Ꮡp, ж<ast.FuncDecl> Ꮡd) => printer_package.funcDeclᴍ(Ꮡp, Ꮡd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void decl(this ж<printer> Ꮡp, ast.Decl decl) => printer_package.declᴍ(Ꮡp, decl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void declList(this ж<printer> Ꮡp, slice<ast.Decl> list) => printer_package.declListᴍ(Ꮡp, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void @file(this ж<printer> Ꮡp, ж<ast.File> Ꮡsrc) => printer_package.@fileᴍ(Ꮡp, Ꮡsrc);
}
partial class printerᴛsizeCounter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref sizeCounter c, slice<byte> p) => printer_package.Writeᴍ(ref c, p);
}
partial class printerᴛprinter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void internalError(this ref printer p, params ꓸꓸꓸany msgʗp) => printer_package.internalErrorᴍ(ref p, msgʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool commentsHaveNewline(this ref printer p, slice<ж<ast.Comment>> list) => printer_package.commentsHaveNewlineᴍ(ref p, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void nextComment(this ref printer p) => printer_package.nextCommentᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool commentBefore(this ref printer p, token.Position next) => printer_package.commentBeforeᴍ(ref p, next);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint commentSizeBefore(this ж<printer> Ꮡp, token.Position next) => printer_package.commentSizeBeforeᴍ(Ꮡp, next);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recordLine(this ref printer p, ж<nint> ᏑlinePtr) => printer_package.recordLineᴍ(ref p, ᏑlinePtr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint linesFrom(this ref printer p, nint line) => printer_package.linesFromᴍ(ref p, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static token.Position posFor(this ref printer p, token.Pos pos) => printer_package.posForᴍ(ref p, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint lineFor(this ref printer p, token.Pos pos) => printer_package.lineForᴍ(ref p, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeLineDirective(this ref printer p, token.Position pos) => printer_package.writeLineDirectiveᴍ(ref p, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeIndent(this ref printer p) => printer_package.writeIndentᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeByte(this ref printer p, byte ch, nint n) => printer_package.writeByteᴍ(ref p, ch, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeString(this ref printer p, token.Position pos, @string s, bool isLit) => printer_package.writeStringᴍ(ref p, pos, s, isLit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeCommentPrefix(this ref printer p, token.Position pos, token.Position next, ж<ast.Comment> Ꮡprev, token.Token tok) => printer_package.writeCommentPrefixᴍ(ref p, pos, next, Ꮡprev, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeComment(this ж<printer> Ꮡp, ж<ast.Comment> Ꮡcomment) => printer_package.writeCommentᴍ(Ꮡp, Ꮡcomment);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool wroteNewline, bool droppedFF) writeCommentSuffix(this ref printer p, bool needsLinebreak) => printer_package.writeCommentSuffixᴍ(ref p, needsLinebreak);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool containsLinebreak(this ref printer p) => printer_package.containsLinebreakᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool wroteNewline, bool droppedFF) intersperseComments(this ж<printer> Ꮡp, token.Position next, token.Token tok) => printer_package.intersperseCommentsᴍ(Ꮡp, next, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeWhitespace(this ref printer p, nint n) => printer_package.writeWhitespaceᴍ(ref p, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setPos(this ref printer p, token.Pos pos) => printer_package.setPosᴍ(ref p, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void print(this ж<printer> Ꮡp, params ꓸꓸꓸany argsʗp) => printer_package.printᴍ(Ꮡp, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool wroteNewline, bool droppedFF) flush(this ж<printer> Ꮡp, token.Position next, token.Token tok) => printer_package.flushᴍ(Ꮡp, next, tok);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error printNode(this ж<printer> Ꮡp, any node) => printer_package.printNodeᴍ(Ꮡp, node);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<printer> Ꮡp) => printer_package.freeᴍ(Ꮡp);
}
partial class printerᴛtrimmer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void resetSpace(this ref trimmer p) => printer_package.resetSpaceᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref trimmer p, slice<byte> data) => printer_package.Writeᴍ(ref p, data);
}
partial class printerᴛConfig {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ fprint(this ж<Config> Ꮡcfg, io.Writer output, ж<token.FileSet> fset, any node, map<ast.Node, nint> nodeSizes) => printer_package.fprintᴍ(Ꮡcfg, output, fset, node, nodeSizes);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Fprint(this ж<Config> Ꮡcfg, io.Writer output, ж<token.FileSet> fset, any node) => printer_package.Fprintᴍ(Ꮡcfg, output, fset, node);
}
// </GlyphForwarders>
