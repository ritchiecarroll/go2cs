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
using static go.text.template.parse_package;

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
[assembly: GoImplement<ActionNode, Node>(Pointer = true)]
[assembly: GoImplement<BoolNode, Node>(Pointer = true)]
[assembly: GoImplement<BranchNode, Node>(Pointer = true)]
[assembly: GoImplement<BreakNode, Node>(Pointer = true)]
[assembly: GoImplement<ChainNode, Node>(Pointer = true)]
[assembly: GoImplement<CommandNode, Node>(Pointer = true)]
[assembly: GoImplement<CommentNode, Node>(Pointer = true)]
[assembly: GoImplement<ContinueNode, Node>(Pointer = true)]
[assembly: GoImplement<DotNode, Node>(Pointer = true)]
[assembly: GoImplement<FieldNode, Node>(Pointer = true)]
[assembly: GoImplement<IdentifierNode, Node>(Pointer = true)]
[assembly: GoImplement<IfNode, Node>(Pointer = true)]
[assembly: GoImplement<ListNode, Node>(Pointer = true)]
[assembly: GoImplement<NilNode, Node>(Pointer = true)]
[assembly: GoImplement<NumberNode, Node>(Pointer = true)]
[assembly: GoImplement<PipeNode, Node>(Pointer = true)]
[assembly: GoImplement<RangeNode, Node>(Pointer = true)]
[assembly: GoImplement<StringNode, Node>(Pointer = true)]
[assembly: GoImplement<TemplateNode, Node>(Pointer = true)]
[assembly: GoImplement<TextNode, Node>(Pointer = true)]
[assembly: GoImplement<VariableNode, Node>(Pointer = true)]
[assembly: GoImplement<WithNode, Node>(Pointer = true)]
[assembly: GoImplement<elseNode, Node>(Pointer = true)]
[assembly: GoImplement<endNode, Node>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(CommandNode))]
[assembly: GoFieldAccess(typeof(ListNode))]
[assembly: GoFieldAccess(typeof(NumberNode))]
[assembly: GoFieldAccess(typeof(PipeNode))]
[assembly: GoFieldAccess(typeof(TextNode))]
[assembly: GoFieldAccess(typeof(Tree))]
[assembly: GoFieldAccess(typeof(VariableNode))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("text/template/parse/lex.go", "lex.cs", "AB0sgpSkpKSUACxaADp2koKClIKCgpSokoKCqJKCgpSCzqKCgoKokqiSgqyygoKokoKUgqiSlKrCgoKCgqrCgoKClIKCguySgpSClAAIEgAMGrKCgpSCgoKUgoKCgoKCppSUgoKUqJLIkoKUgpTIkqrCgoKCgpSCgoKUgoKCgoKosoKCgpSCgoKUgoKClIKClIKClKiygoKClIKCgoKUgqjogoKClJSkpIKkpIKUpKSkpKS2goKClrSCpIKkgqSCgpSkpM7SgoKCgoKUgriCgoKmqLKC6IKCgpSUgoKUpKSkAAYQoqrCgpSqwoKClJSCgoKCgqaClM7CgoKUlJTKwoKCtIKUtKTGruKClJSClJTGlJSClIKCgoKCpoKClIKClIKCppSCgpTIsoKCtIKUtKTGqLKCgpSk1qiSqJKmgqaC")]
[assembly: GoPositionMap("text/template/parse/node.go", "node.cs", "ACtUgsqiAChOgsaCpoKmgoKCpoKCuKKClIKClKaCAA4WgsaCpoKmgqaCAA4WgsaCgoKmgoKCpoKmggARHILGgqaCgoKmgoKCgpSUgpSmgoKUuIKmooKUgoKUgoKClKaCABEcgsaCgoKmgoKCpoKmggAOFoLGgqaCgoKmgoKClIKCgoKUuIKmooKUgoKUAAwYkszSgqzSgqaCpoKmgqaCAA8YgsaCgoKmgoKClLiCpoIADRSCxrimgqaCpoKmggANFILGuOaCpoKmgqaCABAagsaCgoKmgoKCuIKmggARHILIkoKUgoKUpoKCgqaCgoKClJSCgriCpoIADhaCAAoGgoKUpoKmgqaCABwqgoKUgoKUgpSCgoKCgoK2gpSCgqaCgoKCgoK4goKClIKCgoKCgriCgoKCgpSCpoKUgpSCgpSCgriClMqigoKCgoKUgoLKgqaCpoKmgoKCAA8YgsaCpoKmgqaCAA4WggAIBoKmgqaCpoIADhaCxoLmgqaCpoKmggARHIKCggAMBoKClLS0tKSCgoKCgoKCgpSmgqaClKSkpAANEoLGggAOFoLGwoKCAA0UgsbCgoIACg6CxoIACxCCxoIAEBqCxoKCguaCgoKCgpSmgqaC")]
[assembly: GoPositionMap("text/template/parse/parse.go", "parse.cs", "ADFmkoLasoKUAAkY4oKCgoLIkoKUlKiSqqKCqrKCgqiSgpSCgqiygoKCpqiSgoLM0gAHEtKCgoKUgoKClIKUgoKosoKCqJKokoKClKiSgoKU6JKCgoKCgqaUqPKCgoKUgpS0AAgGsoKCgoKCggAGEJKCgoKuAAwCgoKCgoKCgoLosoKCgpSC2pKU1taCgqYACAjmpAAIDNKCgoKCgoKCgoKCgpSUpLQACRDSgoKCgoKUgoKCgpSCAAIQAAkCgoKClJSUguwACAKApKSCgqSktNaCAAYUAAkCpKSkpKSkpKSklIKUAAYQ8oKUgpQABhDygpSClKzigpSCgsqCgpSCgoK0goKCgqa0trS0toK2gqaCtNq0gqaCptoACQKCgoKUgoKClAAOGoKCgoKCgoKUgoLIAAYS4gAHEuKSAAYS4gAHEPIABhDyuIKUggAGFAAJApSCgpSCgoKCgoKCgpSClAAGEgAIAoKCgoKClJSmopSCgpS0pAAGEgAIAoKCgoKClKS0+KSUgpQAAhQACQKCgpSCgoLulLS0tLYABhwADwKApIKClKSkpKSkpIKClKSClIKAkqSCgpSkgtiSgoKUgqaokqrCgoKCpoI=", "807-807:1", "", "66=CopyList/1/1/3;283=hasFunction/1/2/2,hasFunction/2/2/3", "", "1067:29,1068:21")]
// </GoSourcePositionMaps>

namespace go.text.template;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using fmt = fmt_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using ꓸꓸꓸany = Span<any>;
using static parse_package;
using strconv = strconv_package;
using bytes = bytes_package;
using runtime = runtime_package;
// </GlyphForwarderUsings>

[GoPackage("parse")]
public static partial class parse_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct elseNode {}
    internal partial struct endNode {}
    internal partial struct item {}
    internal partial struct itemType {}
    internal partial struct lexOptions {}
    internal partial struct lexer {}
    public partial interface Node {}
    public partial struct ActionNode {}
    public partial struct BoolNode {}
    public partial struct BranchNode {}
    public partial struct BreakNode {}
    public partial struct ChainNode {}
    public partial struct CommandNode {}
    public partial struct CommentNode {}
    public partial struct ContinueNode {}
    public partial struct DotNode {}
    public partial struct FieldNode {}
    public partial struct IdentifierNode {}
    public partial struct IfNode {}
    public partial struct ListNode {}
    public partial struct Mode {}
    public partial struct NilNode {}
    public partial struct NodeType {}
    public partial struct NumberNode {}
    public partial struct PipeNode {}
    public partial struct Pos {}
    public partial struct RangeNode {}
    public partial struct StringNode {}
    public partial struct TemplateNode {}
    public partial struct TextNode {}
    [GoValueClone("token")] public partial struct Tree {}
    public partial struct VariableNode {}
    public partial struct WithNode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
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
[GoMethodHost(typeof(parse_package.ActionNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛActionNode;
[GoMethodHost(typeof(parse_package.BoolNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛBoolNode;
[GoMethodHost(typeof(parse_package.BranchNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛBranchNode;
[GoMethodHost(typeof(parse_package.BreakNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛBreakNode;
[GoMethodHost(typeof(parse_package.ChainNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛChainNode;
[GoMethodHost(typeof(parse_package.CommandNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛCommandNode;
[GoMethodHost(typeof(parse_package.CommentNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛCommentNode;
[GoMethodHost(typeof(parse_package.ContinueNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛContinueNode;
[GoMethodHost(typeof(parse_package.DotNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛDotNode;
[GoMethodHost(typeof(parse_package.FieldNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛFieldNode;
[GoMethodHost(typeof(parse_package.IdentifierNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛIdentifierNode;
[GoMethodHost(typeof(parse_package.IfNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛIfNode;
[GoMethodHost(typeof(parse_package.ListNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛListNode;
[GoMethodHost(typeof(parse_package.NilNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛNilNode;
[GoMethodHost(typeof(parse_package.NodeType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛNodeType;
[GoMethodHost(typeof(parse_package.NumberNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛNumberNode;
[GoMethodHost(typeof(parse_package.PipeNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛPipeNode;
[GoMethodHost(typeof(parse_package.Pos)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛPos;
[GoMethodHost(typeof(parse_package.RangeNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛRangeNode;
[GoMethodHost(typeof(parse_package.StringNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛStringNode;
[GoMethodHost(typeof(parse_package.TemplateNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛTemplateNode;
[GoMethodHost(typeof(parse_package.TextNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛTextNode;
[GoMethodHost(typeof(parse_package.Tree)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛTree;
[GoMethodHost(typeof(parse_package.VariableNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛVariableNode;
[GoMethodHost(typeof(parse_package.WithNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛWithNode;
[GoMethodHost(typeof(parse_package.elseNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛelseNode;
[GoMethodHost(typeof(parse_package.endNode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛendNode;
[GoMethodHost(typeof(parse_package.item)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛitem;
[GoMethodHost(typeof(parse_package.lexer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class parseᴛlexer;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class parseᴛitem {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this item i) => parse_package.Stringᴍ(i);
}
partial class parseᴛlexer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune next(this ref lexer l) => parse_package.nextᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static rune peek(this ref lexer l) => parse_package.peekᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void backup(this ref lexer l) => parse_package.backupᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item thisItem(this ref lexer l, itemType t) => parse_package.thisItemᴍ(ref l, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static stateFn emit(this ref lexer l, itemType t) => parse_package.emitᴍ(ref l, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static stateFn emitItem(this ref lexer l, item i) => parse_package.emitItemᴍ(ref l, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void ignore(this ref lexer l) => parse_package.ignoreᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool accept(this ref lexer l, @string valid) => parse_package.acceptᴍ(ref l, valid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void acceptRun(this ref lexer l, @string valid) => parse_package.acceptRunᴍ(ref l, valid);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static stateFn errorf(this ref lexer l, @string format, params ꓸꓸꓸany argsʗp) => parse_package.errorfᴍ(ref l, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item nextItem(this ж<lexer> Ꮡl) => parse_package.nextItemᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool delim, bool trimSpaces) atRightDelim(this ref lexer l) => parse_package.atRightDelimᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool atTerminator(this ref lexer l) => parse_package.atTerminatorᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool scanNumber(this ref lexer l) => parse_package.scanNumberᴍ(ref l);
}
partial class parseᴛPos {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Pos Position(this Pos p) => parse_package.Positionᴍ(p);
}
partial class parseᴛNodeType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static NodeType Type(this NodeType t) => parse_package.Typeᴍ(t);
}
partial class parseᴛTree {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ListNode> newList(this ж<Tree> Ꮡt, Pos pos) => parse_package.newListᴍ(Ꮡt, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<TextNode> newText(this ж<Tree> Ꮡt, Pos pos, @string text) => parse_package.newTextᴍ(Ꮡt, pos, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<CommentNode> newComment(this ж<Tree> Ꮡt, Pos pos, @string text) => parse_package.newCommentᴍ(Ꮡt, pos, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<PipeNode> newPipeline(this ж<Tree> Ꮡt, Pos pos, nint line, slice<ж<VariableNode>> vars) => parse_package.newPipelineᴍ(Ꮡt, pos, line, vars);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ActionNode> newAction(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe) => parse_package.newActionᴍ(Ꮡt, pos, line, pipe);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<CommandNode> newCommand(this ж<Tree> Ꮡt, Pos pos) => parse_package.newCommandᴍ(Ꮡt, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<VariableNode> newVariable(this ж<Tree> Ꮡt, Pos pos, @string ident) => parse_package.newVariableᴍ(Ꮡt, pos, ident);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<DotNode> newDot(this ж<Tree> Ꮡt, Pos pos) => parse_package.newDotᴍ(Ꮡt, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<NilNode> newNil(this ж<Tree> Ꮡt, Pos pos) => parse_package.newNilᴍ(Ꮡt, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<FieldNode> newField(this ж<Tree> Ꮡt, Pos pos, @string ident) => parse_package.newFieldᴍ(Ꮡt, pos, ident);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ChainNode> newChain(this ж<Tree> Ꮡt, Pos pos, Node node) => parse_package.newChainᴍ(Ꮡt, pos, node);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<BoolNode> newBool(this ж<Tree> Ꮡt, Pos pos, bool @true) => parse_package.newBoolᴍ(Ꮡt, pos, @true);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<NumberNode>, error) newNumber(this ж<Tree> Ꮡt, Pos pos, @string text, itemType typ) => parse_package.newNumberᴍ(Ꮡt, pos, text, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<StringNode> newString(this ж<Tree> Ꮡt, Pos pos, @string orig, @string text) => parse_package.newStringᴍ(Ꮡt, pos, orig, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<endNode> newEnd(this ж<Tree> Ꮡt, Pos pos) => parse_package.newEndᴍ(Ꮡt, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<elseNode> newElse(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newElseᴍ(Ꮡt, pos, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<IfNode> newIf(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) => parse_package.newIfᴍ(Ꮡt, pos, line, pipe, list, elseList);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<BreakNode> newBreak(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newBreakᴍ(Ꮡt, pos, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<ContinueNode> newContinue(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newContinueᴍ(Ꮡt, pos, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<RangeNode> newRange(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) => parse_package.newRangeᴍ(Ꮡt, pos, line, pipe, list, elseList);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<WithNode> newWith(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) => parse_package.newWithᴍ(Ꮡt, pos, line, pipe, list, elseList);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<TemplateNode> newTemplate(this ж<Tree> Ꮡt, Pos pos, nint line, @string name, ж<PipeNode> pipe) => parse_package.newTemplateᴍ(Ꮡt, pos, line, name, pipe);
}
partial class parseᴛListNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void append(this ref ListNode l, Node n) => parse_package.appendᴍ(ref l, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref ListNode l) => parse_package.treeᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref ListNode l) => parse_package.Stringᴍ(ref l);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref ListNode l, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref l, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<ListNode> CopyList(this ж<ListNode> Ꮡl) => parse_package.CopyListᴍ(Ꮡl);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ж<ListNode> Ꮡl) => parse_package.Copyᴍ(Ꮡl);
}
partial class parseᴛTextNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref TextNode t) => parse_package.Stringᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref TextNode t, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref t, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref TextNode t) => parse_package.treeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref TextNode t) => parse_package.Copyᴍ(ref t);
}
partial class parseᴛCommentNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref CommentNode c) => parse_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref CommentNode c, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref c, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref CommentNode c) => parse_package.treeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref CommentNode c) => parse_package.Copyᴍ(ref c);
}
partial class parseᴛPipeNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void append(this ref PipeNode p, ж<CommandNode> command) => parse_package.appendᴍ(ref p, command);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref PipeNode p) => parse_package.Stringᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref PipeNode p, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref p, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref PipeNode p) => parse_package.treeᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<PipeNode> CopyPipe(this ж<PipeNode> Ꮡp) => parse_package.CopyPipeᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ж<PipeNode> Ꮡp) => parse_package.Copyᴍ(Ꮡp);
}
partial class parseᴛActionNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref ActionNode a) => parse_package.Stringᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref ActionNode a, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref a, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref ActionNode a) => parse_package.treeᴍ(ref a);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref ActionNode a) => parse_package.Copyᴍ(ref a);
}
partial class parseᴛCommandNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void append(this ref CommandNode c, Node arg) => parse_package.appendᴍ(ref c, arg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref CommandNode c) => parse_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref CommandNode c, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref c, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref CommandNode c) => parse_package.treeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ж<CommandNode> Ꮡc) => parse_package.Copyᴍ(Ꮡc);
}
partial class parseᴛIdentifierNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<IdentifierNode> SetPos(this ж<IdentifierNode> Ꮡi, Pos pos) => parse_package.SetPosᴍ(Ꮡi, pos);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<IdentifierNode> SetTree(this ж<IdentifierNode> Ꮡi, ж<Tree> t) => parse_package.SetTreeᴍ(Ꮡi, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref IdentifierNode i) => parse_package.Stringᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref IdentifierNode i, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref i, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref IdentifierNode i) => parse_package.treeᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref IdentifierNode i) => parse_package.Copyᴍ(ref i);
}
partial class parseᴛVariableNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref VariableNode v) => parse_package.Stringᴍ(ref v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref VariableNode v, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref v, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref VariableNode v) => parse_package.treeᴍ(ref v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref VariableNode v) => parse_package.Copyᴍ(ref v);
}
partial class parseᴛDotNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static NodeType Type(this ref DotNode d) => parse_package.Typeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref DotNode d) => parse_package.Stringᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref DotNode d, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref d, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref DotNode d) => parse_package.treeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref DotNode d) => parse_package.Copyᴍ(ref d);
}
partial class parseᴛNilNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static NodeType Type(this ref NilNode n) => parse_package.Typeᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref NilNode n) => parse_package.Stringᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref NilNode n, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref n, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref NilNode n) => parse_package.treeᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref NilNode n) => parse_package.Copyᴍ(ref n);
}
partial class parseᴛFieldNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref FieldNode f) => parse_package.Stringᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref FieldNode f, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref f, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref FieldNode f) => parse_package.treeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref FieldNode f) => parse_package.Copyᴍ(ref f);
}
partial class parseᴛChainNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Add(this ref ChainNode c, @string field) => parse_package.Addᴍ(ref c, field);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref ChainNode c) => parse_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref ChainNode c, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref c, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref ChainNode c) => parse_package.treeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref ChainNode c) => parse_package.Copyᴍ(ref c);
}
partial class parseᴛBoolNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref BoolNode b) => parse_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref BoolNode b, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref b, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref BoolNode b) => parse_package.treeᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref BoolNode b) => parse_package.Copyᴍ(ref b);
}
partial class parseᴛNumberNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void simplifyComplex(this ref NumberNode n) => parse_package.simplifyComplexᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref NumberNode n) => parse_package.Stringᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref NumberNode n, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref n, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref NumberNode n) => parse_package.treeᴍ(ref n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref NumberNode n) => parse_package.Copyᴍ(ref n);
}
partial class parseᴛStringNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref StringNode s) => parse_package.Stringᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref StringNode s, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref s, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref StringNode s) => parse_package.treeᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref StringNode s) => parse_package.Copyᴍ(ref s);
}
partial class parseᴛendNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref endNode e) => parse_package.Stringᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref endNode e, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref e, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref endNode e) => parse_package.treeᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node Copy(this ref endNode e) => parse_package.Copyᴍ(ref e);
}
partial class parseᴛelseNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static NodeType Type(this ref elseNode e) => parse_package.Typeᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref elseNode e) => parse_package.Stringᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref elseNode e, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref e, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref elseNode e) => parse_package.treeᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node Copy(this ref elseNode e) => parse_package.Copyᴍ(ref e);
}
partial class parseᴛBranchNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref BranchNode b) => parse_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref BranchNode b, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref b, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref BranchNode b) => parse_package.treeᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref BranchNode b) => parse_package.Copyᴍ(ref b);
}
partial class parseᴛIfNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref IfNode i) => parse_package.Copyᴍ(ref i);
}
partial class parseᴛBreakNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref BreakNode b) => parse_package.Copyᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref BreakNode b) => parse_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref BreakNode b) => parse_package.treeᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref BreakNode b, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref b, sb);
}
partial class parseᴛContinueNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref ContinueNode c) => parse_package.Copyᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref ContinueNode c) => parse_package.Stringᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref ContinueNode c) => parse_package.treeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref ContinueNode c, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref c, sb);
}
partial class parseᴛRangeNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref RangeNode r) => parse_package.Copyᴍ(ref r);
}
partial class parseᴛWithNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref WithNode w) => parse_package.Copyᴍ(ref w);
}
partial class parseᴛTemplateNode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref TemplateNode t) => parse_package.Stringᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeTo(this ref TemplateNode t, ж<strings.Builder> sb) => parse_package.writeToᴍ(ref t, sb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Tree> tree(this ref TemplateNode t) => parse_package.treeᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Node Copy(this ref TemplateNode t) => parse_package.Copyᴍ(ref t);
}
partial class parseᴛTree {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Tree> Copy(this ж<Tree> Ꮡt) => parse_package.Copyᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item next(this ref Tree t) => parse_package.nextᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void backup(this ref Tree t) => parse_package.backupᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void backup2(this ref Tree t, item t1) => parse_package.backup2ᴍ(ref t, t1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void backup3(this ref Tree t, item t2, item t1) => parse_package.backup3ᴍ(ref t, t2, t1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item peek(this ref Tree t) => parse_package.peekᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item /*token*/ nextNonSpace(this ref Tree t) => parse_package.nextNonSpaceᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item peekNonSpace(this ref Tree t) => parse_package.peekNonSpaceᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string location, @string context) ErrorContext(this ж<Tree> Ꮡt, Node n) => parse_package.ErrorContextᴍ(Ꮡt, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ref Tree t, @string format, params ꓸꓸꓸany argsʗp) => parse_package.errorfᴍ(ref t, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void error(this ref Tree t, error err) => parse_package.errorᴍ(ref t, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item expect(this ref Tree t, itemType expected, @string context) => parse_package.expectᴍ(ref t, expected, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static item expectOneOf(this ref Tree t, itemType expected1, itemType expected2, @string context) => parse_package.expectOneOfᴍ(ref t, expected1, expected2, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unexpected(this ref Tree t, item token, @string context) => parse_package.unexpectedᴍ(ref t, token, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recover(this ж<Tree> Ꮡt, ж<error> Ꮡerrp) => parse_package.recoverᴍ(Ꮡt, Ꮡerrp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void startParse(this ref Tree t, slice<map<@string, any>> funcs, ж<lexer> Ꮡlex, map<@string, ж<Tree>> treeSet) => parse_package.startParseᴍ(ref t, funcs, Ꮡlex, treeSet);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stopParse(this ref Tree t) => parse_package.stopParseᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Tree> tree, error err) Parse(this ж<Tree> Ꮡt, @string text, @string leftDelim, @string rightDelim, map<@string, ж<Tree>> treeSet, params Span<map<@string, any>> funcsʗp) => parse_package.Parseᴍ(Ꮡt, text, leftDelim, rightDelim, treeSet, funcsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ж<Tree> Ꮡt) => parse_package.addᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parse(this ж<Tree> Ꮡt) => parse_package.parseᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseDefinition(this ж<Tree> Ꮡt) => parse_package.parseDefinitionᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<ListNode> list, Node next) itemList(this ж<Tree> Ꮡt) => parse_package.itemListᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node textOrAction(this ж<Tree> Ꮡt) => parse_package.textOrActionᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clearActionLine(this ref Tree t) => parse_package.clearActionLineᴍ(ref t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node /*n*/ action(this ж<Tree> Ꮡt) => parse_package.actionᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node breakControl(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.breakControlᴍ(Ꮡt, pos, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node continueControl(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.continueControlᴍ(Ꮡt, pos, line);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<PipeNode> /*pipe*/ pipeline(this ж<Tree> Ꮡt, @string context, itemType end) => parse_package.pipelineᴍ(Ꮡt, context, end);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkPipeline(this ref Tree t, ж<PipeNode> Ꮡpipe, @string context) => parse_package.checkPipelineᴍ(ref t, Ꮡpipe, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) parseControl(this ж<Tree> Ꮡt, @string context) => parse_package.parseControlᴍ(Ꮡt, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node ifControl(this ж<Tree> Ꮡt) => parse_package.ifControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node rangeControl(this ж<Tree> Ꮡt) => parse_package.rangeControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node withControl(this ж<Tree> Ꮡt) => parse_package.withControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node endControl(this ж<Tree> Ꮡt) => parse_package.endControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node elseControl(this ж<Tree> Ꮡt) => parse_package.elseControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node blockControl(this ж<Tree> Ꮡt) => parse_package.blockControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node templateControl(this ж<Tree> Ꮡt) => parse_package.templateControlᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string /*name*/ parseTemplateName(this ref Tree t, item token, @string context) => parse_package.parseTemplateNameᴍ(ref t, token, context);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<CommandNode> command(this ж<Tree> Ꮡt) => parse_package.commandᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node operand(this ж<Tree> Ꮡt) => parse_package.operandᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node term(this ж<Tree> Ꮡt) => parse_package.termᴍ(Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool hasFunction(this ref Tree t, @string name) => parse_package.hasFunctionᴍ(ref t, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void popVars(this ref Tree t, nint n) => parse_package.popVarsᴍ(ref t, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Node useVar(this ж<Tree> Ꮡt, Pos pos, @string name) => parse_package.useVarᴍ(Ꮡt, pos, name);
}
// </GlyphForwarders>
