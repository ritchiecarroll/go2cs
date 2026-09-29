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
global using runtimeꓸError = go.runtime_package.ΔError;
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("text/template/parse/lex.go", "lex.cs", "ABksgpSkpKSkAChaADh2koKClIKCgpSokoKCqJKCgpSCzqKCgoKokqiSgqyygoKokoKUgqiSlKrCgoKCgqrCgoKClIKCgsySgpSClAAIEgAIGrKAgoKUgoKClIKCgoKCgqaklIKClKiSqJKSlJKUqJKqwoKCgoKUgoKClIKCgoKCqLKCgoKUgoKClIKCgpSCgpSCgpSosoKCgpSCgoKClIKo6IKCgpSUgKSkgqSkgpSkpKSkpKaSgoKWtIKkgqSCpIKClKSk7tKCgoKCgpSCuIKCgqaosoKA2IKCkpSUgoKUpKSkAAkQoqrCkpSqwpKClJSCgoKCgqaClK7CgoKUlKSqwoKCtICCpLSk1q7igpSAlIKUpKaUlIKUgqSkpoKClIKClIKCppSCgpSosoKCtICCpLSk1qiygoKUpNaokqiSpoKmgg==")]
[assembly: go.GoPositionMap("text/template/parse/node.go", "node.cs", "ACVUgqqiACROgqaCpoKmgoKCpoKCuKKClIKClKaCAAoWgqaCpoKmgqaCAAoWgqaCgoKmgoKCpoKmggANHIKmgqaCgoKmgoKCgpSUgpSmgoKUuIKmooKUgoKUgoKClKaCAA0cgqaCgoKmgoKCpoKmggAKFoKmgqaCgoKmgoKClICCgoKCpLiCpqKClIKClAAKGJKs0oKs0oKmgqaCpoKmggALGIKmgoKCpoKCgpS4gqaCAAkUgqa4poKmgqaCpoIACRSCprjWgqaCpoKmggAMGoKmgoKCpoKCgriCpoIADRyCqJKClIKClKaCgoKmgoCCgoKUpIKCuIKmggAKFoLmgoKUpoKmgqaCABcqgoKUgoKUgpSCgoKCgoKmgJKkgoK2goKCgoKCuIKCgpSCgoKCgoK4goKkgpSCpoKUgpSCgpSCgriClKqigoKCgoKUgoLKgqaCpoKmgoKCAAsYgqaCpoKmgqaCAAoWgtaCpoKmgqaCAAoWgqaC1oKmgqaCpoIADRyCgoLmgoKUpKSkpIKCgoKCgoKClKaCpoKUpKSkAAkSgqaCAAoWgqaA0oCigKKAAAoUgqaA0oCigKKA/oKmggAHEIKmggAMGoKmgoKC1oKCgoKClKaCpoI=")]
[assembly: go.GoPositionMap("text/template/parse/parse.go", "parse.cs", "ACpcsoKUAAcY4oKCgoKokoKUlKiSqqKCqrKCgqiSgpSCgqiygoKCpqiSgoKs0gAFEtKCgoKUgoKClIKUgoKosoKCqJKokoKClKiSgoKU2JKCgoKCgqaUqOKCgoCCpIKUtOaygoKCgoIABhCSgoKCrgAMAoKCgoKCgoKC6LKCgoKUgrqSlNbWgoKmAAgI5qTc0oKCgoKCgoKCgoKClJSApKQACRDSgoKCgoKUgoKCgpSCAAIQAAkCgoKClKSUgtwACAKApKSCgqSktNaCAAUUAAkCgKSkpKSkpKSkpLSClAACEPKAgqSClAACEPKAgqSClKzigpSCgILKgoKUgoKCtIKCgoKWpLbEtNaCgKaSpoKk6rSCpoKW6gAJAoKCgpSCgoKUAAMaAAsCgoKkgoKUgoLIAAYS4gADEuKSAAIS4gAGEPIABRDyuIKUggAFFAAJAoSCgoSCgoKCgoKCgpSChAAFEgAIAoKCgoKClJSmopSCgpSkpAAFEgAIAoKCgoKClICkpMjElIKUAAIUAAkCgoKUgoKC7pSkpKS2AAUcAA0CgKSCgpSkpKSkpKSCgpSkpIKClLSCqJKCgpSCpqiSqsKCgoKmgg==")]
// </GoSourcePositionMaps>

namespace go.text.template;

[GoPackage("parse")]
public static partial class parse_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
