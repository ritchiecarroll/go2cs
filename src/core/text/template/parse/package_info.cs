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
using strings = go.strings_package;
// </ImportedTypeAliases>

using go;
using static go.text.template.parse_package;
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
[assembly: GoPositionMap("text/template/parse/lex.go", "lex.cs", "ABosgpSkpKSUACpaADh2koKClIKCgpSokoKCqJKCgpSCzqKCgoKokqiSgqyyooKokoKUgqiSlKrCkoKCgqrCgoKClIKCgsySgpSClAAIEgAMGrKCgpSCgoKUgqKCgoKCppSUgqKUqJKokoKUgpSokqrCgoKCgpSCgoKUgoKCgoKosoKCgpSCgoKUooKClIKCtIKClKiygoKClIKCgqKUgqjogoKClJSkpIKkpIKUpKSkpKS2goKClrSCpIKkgqSCgpSkpM7SgoKCgoKUgriCgoKmqLKC6IKCgpSUgoKUpKSkAAYQoqrCgpSqwoKClJSCgoKCgqaClK7CgoKUlJSqwoKCtIKUtKTGruKClJSClJSmlJSClIKCgoKCpoKClIKClIKCppSCgpSosoKCtIKUtKTGqLKCgpSk1qiSqJKmgqaC")]
[assembly: GoPositionMap("text/template/parse/node.go", "node.cs", "AChUgqqiACROgqaCpoKmgoKCpoKCuKKClIKClKaCAAoWgqaCpoKmgqaCAAoWgqaCgoKmgoKCpoKmggANHIKmgqaCgoKmgoKCgpSUgpSmgoKUuIKmooKUgoKUgoKClKaCAA0cgqaCgoKmgoKCpoKmggALFoKmgqaCgoKmgoKClIKCgoKUuIKmooKUgoKUAAoYkqzSgqzSgqaCpoKmgqaCAAsYgqaCgoKmgoKClLiCpoIACRSCprimgqaCpoKmggAJFIKmuOaCpoKmgqaCAAwagqaCgoKmgoKCuIKmggANHIKokoKUgoKUpoKCgqaCgoKClJSCgriCpoIAChaCAAgGgoKUpoKmgqaCABgqgoKUgoKUgpSCgoKCgoK2gpSCgqaCgoKCgoK4goKClIKCgoKCgriCgoKCgpSCpoKUgpSCgpSCgriClKqigoKCgoKUgoLKgqaCpoKmgoKCAAsYgqaCpoKmgqaCAAoWguaCpoKmgqaCAAoWgqaC5oKmgqaCpoIADRyCgoIACAaCgpS0tLSkgoKCgoKCgoKUpoKmgpSkpKQACRKCpoIADRaCpsKCggAJFIKmwoKC7oKmggAKEIKmggAPGoKmgoKC5oKCgoKClKaCpoI=")]
[assembly: GoPositionMap("text/template/parse/parse.go", "parse.cs", "ADBmkoK6soKUAAcY4oKCgoKokoKUlKiSqqKCqrKCgqiSgpSCgqiygoKCpqiSgoKs0gAFEtKCgoKUgoKClIKUgoKosoKCqJKokoKClKiSgoKU6JKCgoKCgqaUqPKCgoKUgpS0AAgGsoKCgoKCggAIEJKCgoKuAAwCgoKCgoKCgoLosoKCgpSCupKU1taCgqYACAjmpOzSgoKCgoKCgoKCgoKUlKS0AAkQ0oKCgoKClIKCgoKUggACEAAJAoKCgpSUlILsAAgCgKSkgoKkpLTWggAGFAAJAqSkpKSkpKSkpJSClAACEPKClIKUAAIQ8oKUgpSs4oKUgoLKgoKUgoKStIKSgoKmtLa0tLaCtoKmgrTatIKmgqbaAAkCgoKClIKCgpQADhqCgoKCgoKClIKCyAAGEuIAAxLikgACEuIABxDyAAYQ8riClIIABhQACQKUgoKUgoKCgoKCgoKUgpQABhIACAKCgoKCgpSUpqKUgoKUtKQABhIACAKCgoKCgpSktPiklIKUAAIUAAkCgoKUgoKC7pS0tLS2AAYcAA8CgKSCgpSkpKSkpKSCgpSkgpSCgJKkgoKUpILYkoKClIKmqJKqwoKCgqaC", "807-807:1", "", "63=CopyList/1/1/3;272=hasFunction/1/2/2,hasFunction/2/2/3", "", "1040:60,1041:21")]
// </GoSourcePositionMaps>

namespace go.text.template;

using static go.text.template.parse_package;
using strings = go.strings_package;
using ꓸꓸꓸany = Span<any>;

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
[GoMethodHost(typeof(parse_package.ActionNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛActionNode;
[GoMethodHost(typeof(parse_package.BoolNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛBoolNode;
[GoMethodHost(typeof(parse_package.BranchNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛBranchNode;
[GoMethodHost(typeof(parse_package.BreakNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛBreakNode;
[GoMethodHost(typeof(parse_package.ChainNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛChainNode;
[GoMethodHost(typeof(parse_package.CommandNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛCommandNode;
[GoMethodHost(typeof(parse_package.CommentNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛCommentNode;
[GoMethodHost(typeof(parse_package.ContinueNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛContinueNode;
[GoMethodHost(typeof(parse_package.DotNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛDotNode;
[GoMethodHost(typeof(parse_package.FieldNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛFieldNode;
[GoMethodHost(typeof(parse_package.IdentifierNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛIdentifierNode;
[GoMethodHost(typeof(parse_package.IfNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛIfNode;
[GoMethodHost(typeof(parse_package.ListNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛListNode;
[GoMethodHost(typeof(parse_package.NilNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛNilNode;
[GoMethodHost(typeof(parse_package.NodeType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛNodeType;
[GoMethodHost(typeof(parse_package.NumberNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛNumberNode;
[GoMethodHost(typeof(parse_package.PipeNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛPipeNode;
[GoMethodHost(typeof(parse_package.Pos)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛPos;
[GoMethodHost(typeof(parse_package.RangeNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛRangeNode;
[GoMethodHost(typeof(parse_package.StringNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛStringNode;
[GoMethodHost(typeof(parse_package.TemplateNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛTemplateNode;
[GoMethodHost(typeof(parse_package.TextNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛTextNode;
[GoMethodHost(typeof(parse_package.Tree)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛTree;
[GoMethodHost(typeof(parse_package.VariableNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛVariableNode;
[GoMethodHost(typeof(parse_package.WithNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛWithNode;
[GoMethodHost(typeof(parse_package.elseNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛelseNode;
[GoMethodHost(typeof(parse_package.endNode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛendNode;
[GoMethodHost(typeof(parse_package.item)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛitem;
[GoMethodHost(typeof(parse_package.lexer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class parseᴛlexer;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class parseᴛActionNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref ActionNode a) => parse_package.String⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref ActionNode a, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref a, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref ActionNode a) => parse_package.tree⁔(ref a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref ActionNode a) => parse_package.Copy⁔(ref a);
}
partial class parseᴛBoolNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref BoolNode b) => parse_package.String⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref BoolNode b, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref b, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref BoolNode b) => parse_package.tree⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref BoolNode b) => parse_package.Copy⁔(ref b);
}
partial class parseᴛBranchNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref BranchNode b) => parse_package.String⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref BranchNode b, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref b, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref BranchNode b) => parse_package.tree⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref BranchNode b) => parse_package.Copy⁔(ref b);
}
partial class parseᴛBreakNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref BreakNode b) => parse_package.Copy⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref BreakNode b) => parse_package.String⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref BreakNode b) => parse_package.tree⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref BreakNode b, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref b, Ꮡsb);
}
partial class parseᴛChainNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this ref ChainNode c, @string field) => parse_package.Add⁔(ref c, field);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref ChainNode c) => parse_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref ChainNode c, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref c, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref ChainNode c) => parse_package.tree⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref ChainNode c) => parse_package.Copy⁔(ref c);
}
partial class parseᴛCommandNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void append(this ref CommandNode c, Node arg) => parse_package.append⁔(ref c, arg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref CommandNode c) => parse_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref CommandNode c, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref c, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref CommandNode c) => parse_package.tree⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ж<CommandNode> Ꮡc) => parse_package.Copy⁔(Ꮡc);
}
partial class parseᴛCommentNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref CommentNode c) => parse_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref CommentNode c, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref c, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref CommentNode c) => parse_package.tree⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref CommentNode c) => parse_package.Copy⁔(ref c);
}
partial class parseᴛContinueNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref ContinueNode c) => parse_package.Copy⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref ContinueNode c) => parse_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref ContinueNode c) => parse_package.tree⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref ContinueNode c, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref c, Ꮡsb);
}
partial class parseᴛDotNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static NodeType Type(this ref DotNode d) => parse_package.Type⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref DotNode d) => parse_package.String⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref DotNode d, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref d, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref DotNode d) => parse_package.tree⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref DotNode d) => parse_package.Copy⁔(ref d);
}
partial class parseᴛFieldNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref FieldNode f) => parse_package.String⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref FieldNode f, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref f, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref FieldNode f) => parse_package.tree⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref FieldNode f) => parse_package.Copy⁔(ref f);
}
partial class parseᴛIdentifierNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<IdentifierNode> SetPos(this ж<IdentifierNode> Ꮡi, Pos pos) => parse_package.SetPos⁔(Ꮡi, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<IdentifierNode> SetTree(this ж<IdentifierNode> Ꮡi, ж<Tree> Ꮡt) => parse_package.SetTree⁔(Ꮡi, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref IdentifierNode i) => parse_package.String⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref IdentifierNode i, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref i, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref IdentifierNode i) => parse_package.tree⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref IdentifierNode i) => parse_package.Copy⁔(ref i);
}
partial class parseᴛIfNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref IfNode i) => parse_package.Copy⁔(ref i);
}
partial class parseᴛListNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void append(this ref ListNode l, Node n) => parse_package.append⁔(ref l, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref ListNode l) => parse_package.tree⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref ListNode l) => parse_package.String⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref ListNode l, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref l, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<ListNode> CopyList(this ж<ListNode> Ꮡl) => parse_package.CopyList⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ж<ListNode> Ꮡl) => parse_package.Copy⁔(Ꮡl);
}
partial class parseᴛNilNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static NodeType Type(this ref NilNode n) => parse_package.Type⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref NilNode n) => parse_package.String⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref NilNode n, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref n, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref NilNode n) => parse_package.tree⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref NilNode n) => parse_package.Copy⁔(ref n);
}
partial class parseᴛNodeType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static NodeType Type(this NodeType t) => parse_package.Type⁔(t);
}
partial class parseᴛNumberNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void simplifyComplex(this ref NumberNode n) => parse_package.simplifyComplex⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref NumberNode n) => parse_package.String⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref NumberNode n, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref n, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref NumberNode n) => parse_package.tree⁔(ref n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref NumberNode n) => parse_package.Copy⁔(ref n);
}
partial class parseᴛPipeNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void append(this ref PipeNode p, ж<CommandNode> Ꮡcommand) => parse_package.append⁔(ref p, Ꮡcommand);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref PipeNode p) => parse_package.String⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref PipeNode p, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref p, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref PipeNode p) => parse_package.tree⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<PipeNode> CopyPipe(this ж<PipeNode> Ꮡp) => parse_package.CopyPipe⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ж<PipeNode> Ꮡp) => parse_package.Copy⁔(Ꮡp);
}
partial class parseᴛPos {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Pos Position(this Pos p) => parse_package.Position⁔(p);
}
partial class parseᴛRangeNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref RangeNode r) => parse_package.Copy⁔(ref r);
}
partial class parseᴛStringNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref StringNode s) => parse_package.String⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref StringNode s, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref s, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref StringNode s) => parse_package.tree⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref StringNode s) => parse_package.Copy⁔(ref s);
}
partial class parseᴛTemplateNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref TemplateNode t) => parse_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref TemplateNode t, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref t, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref TemplateNode t) => parse_package.tree⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref TemplateNode t) => parse_package.Copy⁔(ref t);
}
partial class parseᴛTextNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref TextNode t) => parse_package.String⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref TextNode t, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref t, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref TextNode t) => parse_package.tree⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref TextNode t) => parse_package.Copy⁔(ref t);
}
partial class parseᴛTree {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ListNode> newList(this ж<Tree> Ꮡt, Pos pos) => parse_package.newList⁔(Ꮡt, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<TextNode> newText(this ж<Tree> Ꮡt, Pos pos, @string text) => parse_package.newText⁔(Ꮡt, pos, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<CommentNode> newComment(this ж<Tree> Ꮡt, Pos pos, @string text) => parse_package.newComment⁔(Ꮡt, pos, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<PipeNode> newPipeline(this ж<Tree> Ꮡt, Pos pos, nint line, slice<ж<VariableNode>> vars) => parse_package.newPipeline⁔(Ꮡt, pos, line, vars);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ActionNode> newAction(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> Ꮡpipe) => parse_package.newAction⁔(Ꮡt, pos, line, Ꮡpipe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<CommandNode> newCommand(this ж<Tree> Ꮡt, Pos pos) => parse_package.newCommand⁔(Ꮡt, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<VariableNode> newVariable(this ж<Tree> Ꮡt, Pos pos, @string ident) => parse_package.newVariable⁔(Ꮡt, pos, ident);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<DotNode> newDot(this ж<Tree> Ꮡt, Pos pos) => parse_package.newDot⁔(Ꮡt, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<NilNode> newNil(this ж<Tree> Ꮡt, Pos pos) => parse_package.newNil⁔(Ꮡt, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<FieldNode> newField(this ж<Tree> Ꮡt, Pos pos, @string ident) => parse_package.newField⁔(Ꮡt, pos, ident);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ChainNode> newChain(this ж<Tree> Ꮡt, Pos pos, Node node) => parse_package.newChain⁔(Ꮡt, pos, node);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<BoolNode> newBool(this ж<Tree> Ꮡt, Pos pos, bool @true) => parse_package.newBool⁔(Ꮡt, pos, @true);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<NumberNode>, error) newNumber(this ж<Tree> Ꮡt, Pos pos, @string text, itemType typ) => parse_package.newNumber⁔(Ꮡt, pos, text, typ);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<StringNode> newString(this ж<Tree> Ꮡt, Pos pos, @string orig, @string text) => parse_package.newString⁔(Ꮡt, pos, orig, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<endNode> newEnd(this ж<Tree> Ꮡt, Pos pos) => parse_package.newEnd⁔(Ꮡt, pos);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<elseNode> newElse(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newElse⁔(Ꮡt, pos, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<IfNode> newIf(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) => parse_package.newIf⁔(Ꮡt, pos, line, Ꮡpipe, Ꮡlist, ᏑelseList);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<BreakNode> newBreak(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newBreak⁔(Ꮡt, pos, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<ContinueNode> newContinue(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.newContinue⁔(Ꮡt, pos, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<RangeNode> newRange(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) => parse_package.newRange⁔(Ꮡt, pos, line, Ꮡpipe, Ꮡlist, ᏑelseList);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<WithNode> newWith(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) => parse_package.newWith⁔(Ꮡt, pos, line, Ꮡpipe, Ꮡlist, ᏑelseList);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<TemplateNode> newTemplate(this ж<Tree> Ꮡt, Pos pos, nint line, @string name, ж<PipeNode> Ꮡpipe) => parse_package.newTemplate⁔(Ꮡt, pos, line, name, Ꮡpipe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Tree> Copy(this ж<Tree> Ꮡt) => parse_package.Copy⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item next(this ref Tree t) => parse_package.next⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backup(this ref Tree t) => parse_package.backup⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backup2(this ref Tree t, item t1) => parse_package.backup2⁔(ref t, t1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backup3(this ref Tree t, item t2, item t1) => parse_package.backup3⁔(ref t, t2, t1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item peek(this ref Tree t) => parse_package.peek⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item /*token*/ nextNonSpace(this ref Tree t) => parse_package.nextNonSpace⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item peekNonSpace(this ref Tree t) => parse_package.peekNonSpace⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string location, @string context) ErrorContext(this ж<Tree> Ꮡt, Node n) => parse_package.ErrorContext⁔(Ꮡt, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ref Tree t, @string format, params ꓸꓸꓸany argsʗp) => parse_package.errorf⁔(ref t, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void error(this ref Tree t, error err) => parse_package.error⁔(ref t, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item expect(this ref Tree t, itemType expected, @string context) => parse_package.expect⁔(ref t, expected, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item expectOneOf(this ref Tree t, itemType expected1, itemType expected2, @string context) => parse_package.expectOneOf⁔(ref t, expected1, expected2, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void unexpected(this ref Tree t, item token, @string context) => parse_package.unexpected⁔(ref t, token, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void recover(this ж<Tree> Ꮡt, ж<error> Ꮡerrp) => parse_package.recover⁔(Ꮡt, Ꮡerrp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void startParse(this ref Tree t, slice<map<@string, any>> funcs, ж<lexer> Ꮡlex, map<@string, ж<Tree>> treeSet) => parse_package.startParse⁔(ref t, funcs, Ꮡlex, treeSet);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stopParse(this ref Tree t) => parse_package.stopParse⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Tree> tree, error err) Parse(this ж<Tree> Ꮡt, @string text, @string leftDelim, @string rightDelim, map<@string, ж<Tree>> treeSet, params Span<map<@string, any>> funcsʗp) => parse_package.Parse⁔(Ꮡt, text, leftDelim, rightDelim, treeSet, funcsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ж<Tree> Ꮡt) => parse_package.add⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parse(this ж<Tree> Ꮡt) => parse_package.parse⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseDefinition(this ж<Tree> Ꮡt) => parse_package.parseDefinition⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<ListNode> list, Node next) itemList(this ж<Tree> Ꮡt) => parse_package.itemList⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node textOrAction(this ж<Tree> Ꮡt) => parse_package.textOrAction⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void clearActionLine(this ref Tree t) => parse_package.clearActionLine⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node /*n*/ action(this ж<Tree> Ꮡt) => parse_package.action⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node breakControl(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.breakControl⁔(Ꮡt, pos, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node continueControl(this ж<Tree> Ꮡt, Pos pos, nint line) => parse_package.continueControl⁔(Ꮡt, pos, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<PipeNode> /*pipe*/ pipeline(this ж<Tree> Ꮡt, @string context, itemType end) => parse_package.pipeline⁔(Ꮡt, context, end);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkPipeline(this ref Tree t, ж<PipeNode> Ꮡpipe, @string context) => parse_package.checkPipeline⁔(ref t, Ꮡpipe, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) parseControl(this ж<Tree> Ꮡt, @string context) => parse_package.parseControl⁔(Ꮡt, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node ifControl(this ж<Tree> Ꮡt) => parse_package.ifControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node rangeControl(this ж<Tree> Ꮡt) => parse_package.rangeControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node withControl(this ж<Tree> Ꮡt) => parse_package.withControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node endControl(this ж<Tree> Ꮡt) => parse_package.endControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node elseControl(this ж<Tree> Ꮡt) => parse_package.elseControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node blockControl(this ж<Tree> Ꮡt) => parse_package.blockControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node templateControl(this ж<Tree> Ꮡt) => parse_package.templateControl⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string /*name*/ parseTemplateName(this ref Tree t, item token, @string context) => parse_package.parseTemplateName⁔(ref t, token, context);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<CommandNode> command(this ж<Tree> Ꮡt) => parse_package.command⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node operand(this ж<Tree> Ꮡt) => parse_package.operand⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node term(this ж<Tree> Ꮡt) => parse_package.term⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool hasFunction(this ref Tree t, @string name) => parse_package.hasFunction⁔(ref t, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void popVars(this ref Tree t, nint n) => parse_package.popVars⁔(ref t, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node useVar(this ж<Tree> Ꮡt, Pos pos, @string name) => parse_package.useVar⁔(Ꮡt, pos, name);
}
partial class parseᴛVariableNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref VariableNode v) => parse_package.String⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref VariableNode v, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref v, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref VariableNode v) => parse_package.tree⁔(ref v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref VariableNode v) => parse_package.Copy⁔(ref v);
}
partial class parseᴛWithNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Node Copy(this ref WithNode w) => parse_package.Copy⁔(ref w);
}
partial class parseᴛelseNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static NodeType Type(this ref elseNode e) => parse_package.Type⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref elseNode e) => parse_package.String⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref elseNode e, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref e, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref elseNode e) => parse_package.tree⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node Copy(this ref elseNode e) => parse_package.Copy⁔(ref e);
}
partial class parseᴛendNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref endNode e) => parse_package.String⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTo(this ref endNode e, ж<strings.Builder> Ꮡsb) => parse_package.writeTo⁔(ref e, Ꮡsb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Tree> tree(this ref endNode e) => parse_package.tree⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Node Copy(this ref endNode e) => parse_package.Copy⁔(ref e);
}
partial class parseᴛitem {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this item i) => parse_package.String⁔(i);
}
partial class parseᴛlexer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune next(this ref lexer l) => parse_package.next⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static rune peek(this ref lexer l) => parse_package.peek⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backup(this ref lexer l) => parse_package.backup⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item thisItem(this ref lexer l, itemType t) => parse_package.thisItem⁔(ref l, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static stateFn emit(this ref lexer l, itemType t) => parse_package.emit⁔(ref l, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static stateFn emitItem(this ref lexer l, item i) => parse_package.emitItem⁔(ref l, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ignore(this ref lexer l) => parse_package.ignore⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool accept(this ref lexer l, @string valid) => parse_package.accept⁔(ref l, valid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void acceptRun(this ref lexer l, @string valid) => parse_package.acceptRun⁔(ref l, valid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static stateFn errorf(this ref lexer l, @string format, params ꓸꓸꓸany argsʗp) => parse_package.errorf⁔(ref l, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static item nextItem(this ж<lexer> Ꮡl) => parse_package.nextItem⁔(Ꮡl);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (bool delim, bool trimSpaces) atRightDelim(this ref lexer l) => parse_package.atRightDelim⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool atTerminator(this ref lexer l) => parse_package.atTerminator⁔(ref l);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool scanNumber(this ref lexer l) => parse_package.scanNumber⁔(ref l);
}
// </MethodForwarders>
