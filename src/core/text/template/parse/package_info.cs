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
