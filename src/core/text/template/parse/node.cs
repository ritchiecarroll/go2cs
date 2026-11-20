// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Parse nodes.
namespace go.text.template;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using static parse_package;

partial class parse_package {

internal static @string textFormat = "%s"; // Changed to "%q" in tests for better error messages.

// A Node is an element in the parse tree. The interface is trivial.
// The interface contains an unexported method so that only
// types local to this package can satisfy it.
partial interface Node :
    fmt.Stringer
{
    NodeType Type();
    // Copy does a deep copy of the Node and all its components.
    // To avoid type assertions, some XxxNodes also have specialized
    // CopyXxx methods that return *XxxNode.
    Node Copy();
    Pos Position(); // byte position of start of node in full original input string
    // tree returns the containing *Tree.
    // It is unexported so all implementations of Node are in this package.
    ж<Tree> tree();
    // writeTo writes the String output to the builder.
    void writeTo(ж<strings.Builder> _);
}

// NodeType identifies the type of a parse tree node.
partial struct NodeType /*num:nint*/;

// Pos represents a byte position in the original input text from which
// this template was parsed.
partial struct Pos /*num:nint*/;

} partial class parse_package {

internal static Pos Positionᴍ(Pos p) {
    return p;
}

} partial class parse_package {

// Type returns itself and provides an easy default implementation
// for embedding in a Node. Embedded in all non-trivial Nodes.
internal static NodeType Typeᴍ(NodeType t) {
    return t;
}

} partial class parse_package {

public static NodeType NodeText => /* iota */ 0; // Plain text.
public static NodeType NodeAction => 1;          // A non-control action such as a field evaluation.
public static NodeType NodeBool => 2;            // A boolean constant.
public static NodeType NodeChain => 3;           // A sequence of field accesses.
public static NodeType NodeCommand => 4;         // An element of a pipeline.
public static NodeType NodeDot => 5;             // The cursor, dot.
internal static NodeType nodeElse => 6;          // An else action. Not added to tree.
internal static NodeType nodeEnd => 7;           // An end action. Not added to tree.
public static NodeType NodeField => 8;           // A field or method name.
public static NodeType NodeIdentifier => 9;      // An identifier; always a function name.
public static NodeType NodeIf => 10;             // An if action.
public static NodeType NodeList => 11;           // A list of Nodes.
public static NodeType NodeNil => 12;            // An untyped nil constant.
public static NodeType NodeNumber => 13;         // A numerical constant.
public static NodeType NodePipe => 14;           // A pipeline of commands.
public static NodeType NodeRange => 15;          // A range action.
public static NodeType NodeString => 16;         // A string constant.
public static NodeType NodeTemplate => 17;       // A template invocation action.
public static NodeType NodeVariable => 18;       // A $ variable.
public static NodeType NodeWith => 19;           // A with action.
public static NodeType NodeComment => 20;        // A comment.
public static NodeType NodeBreak => 21;          // A break action.
public static NodeType NodeContinue => 22;       // A continue action.

// Nodes.

// ListNode holds a sequence of nodes.
partial struct ListNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<Node> Nodes; // The element nodes in lexical order.
}

} partial class parse_package {

internal static ж<ListNode> newListᴍ(ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new ListNode(tr: Ꮡt, NodeType: NodeList, Pos: pos));
}

} partial class parse_package {

internal static void appendᴍ(ref ListNode l, Node n) {
    l.Nodes = builtin.append(l.Nodes, n);
}

internal static ж<Tree> treeᴍ(ref ListNode l) {
    return l.tr;
}

internal static @string Stringᴍ(ref ListNode l) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    l.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref ListNode l, ж<strings.Builder> sb) {
    foreach (var (_, n) in range(l.Nodes)) {
        n.writeTo(sb);
    }
}

internal static ж<ListNode> CopyListᴍ(ж<ListNode> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return Ꮡl;
    }
    var n = l.tr.newList(l.Pos);
    foreach (var (_, elem) in range(l.Nodes)) {
        n.append(elem.Copy());
    }
    return n;
}

internal static Node Copyᴍ(ж<ListNode> Ꮡl) {
    return new ListNodeжNode(Ꮡl.CopyList());
}

} partial class parse_package {

// TextNode holds plain text.
partial struct TextNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<byte> Text; // The text; may span newlines.
}

} partial class parse_package {

internal static ж<TextNode> newTextᴍ(ж<Tree> Ꮡt, Pos pos, @string text) {
    return Ꮡ(new TextNode(tr: Ꮡt, NodeType: NodeText, Pos: pos, Text: slice<byte>(text)));
}

} partial class parse_package {

internal static @string Stringᴍ(ref TextNode t) {
    return fmt.Sprintf(textFormat, t.Text);
}

internal static void writeToᴍ(ref TextNode t, ж<strings.Builder> sb) {
    sb.WriteString(t.String());
}

internal static ж<Tree> treeᴍ(ref TextNode t) {
    return t.tr;
}

internal static Node Copyᴍ(ref TextNode t) {
    return new TextNodeжNode(Ꮡ(new TextNode(tr: t.tr, NodeType: NodeText, Pos: t.Pos, Text: appendꓸꓸꓸ(slice<byte>([]), t.Text))));
}

} partial class parse_package {

// CommentNode holds a comment.
partial struct CommentNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public @string Text; // Comment text.
}

} partial class parse_package {

internal static ж<CommentNode> newCommentᴍ(ж<Tree> Ꮡt, Pos pos, @string text) {
    return Ꮡ(new CommentNode(tr: Ꮡt, NodeType: NodeComment, Pos: pos, Text: text));
}

} partial class parse_package {

internal static @string Stringᴍ(ref CommentNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref CommentNode c, ж<strings.Builder> sb) {
    sb.WriteString("{{");
    sb.WriteString(c.Text);
    sb.WriteString("}}");
}

internal static ж<Tree> treeᴍ(ref CommentNode c) {
    return c.tr;
}

internal static Node Copyᴍ(ref CommentNode c) {
    return new CommentNodeжNode(Ꮡ(new CommentNode(tr: c.tr, NodeType: NodeComment, Pos: c.Pos, Text: c.Text)));
}

} partial class parse_package {

// PipeNode holds a pipeline with optional declaration
partial struct PipeNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line;                   // The line number in the input. Deprecated: Kept for compatibility.
    public bool IsAssign;               // The variables are being assigned, not declared.
    public slice<ж<VariableNode>> Decl; // Variables in lexical order.
    public slice<ж<CommandNode>> Cmds;  // The commands in lexical order.
}

} partial class parse_package {

internal static ж<PipeNode> newPipelineᴍ(ж<Tree> Ꮡt, Pos pos, nint line, slice<ж<VariableNode>> vars) {
    return Ꮡ(new PipeNode(tr: Ꮡt, NodeType: NodePipe, Pos: pos, Line: line, Decl: vars));
}

} partial class parse_package {

internal static void appendᴍ(ref PipeNode p, ж<CommandNode> command) {
    p.Cmds = builtin.append(p.Cmds, command);
}

internal static @string Stringᴍ(ref PipeNode p) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    p.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref PipeNode p, ж<strings.Builder> sb) {
    if (len(p.Decl) > 0) {
        foreach (var (i, v) in range(p.Decl)) {
            if (i > 0) {
                sb.WriteString(", ");
            }
            v.writeTo(sb);
        }
        if (p.IsAssign) {
            sb.WriteString(" = ");
        } else {
            sb.WriteString(" := ");
        }
    }
    foreach (var (i, c) in range(p.Cmds)) {
        if (i > 0) {
            sb.WriteString(" | ");
        }
        c.writeTo(sb);
    }
}

internal static ж<Tree> treeᴍ(ref PipeNode p) {
    return p.tr;
}

internal static ж<PipeNode> CopyPipeᴍ(ж<PipeNode> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return Ꮡp;
    }
    var vars = new slice<ж<VariableNode>>(len(p.Decl));
    foreach (var (i, d) in range(p.Decl)) {
        vars[i] = d.Copy()._<ж<VariableNode>>();
    }
    var n = p.tr.newPipeline(p.Pos, p.Line, vars);
    n.IsAssign = p.IsAssign;
    foreach (var (_, c) in range(p.Cmds)) {
        n.append(c.Copy()._<ж<CommandNode>>());
    }
    return n;
}

internal static Node Copyᴍ(ж<PipeNode> Ꮡp) {
    return new PipeNodeжNode(Ꮡp.CopyPipe());
}

} partial class parse_package {

// ActionNode holds an action (something bounded by delimiters).
// Control actions have their own nodes; ActionNode represents simple
// ones such as field evaluations and parenthesized pipelines.
partial struct ActionNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line;        // The line number in the input. Deprecated: Kept for compatibility.
    public ж<PipeNode> Pipe; // The pipeline in the action.
}

} partial class parse_package {

internal static ж<ActionNode> newActionᴍ(ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe) {
    return Ꮡ(new ActionNode(tr: Ꮡt, NodeType: NodeAction, Pos: pos, Line: line, Pipe: pipe));
}

} partial class parse_package {

internal static @string Stringᴍ(ref ActionNode a) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    a.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref ActionNode a, ж<strings.Builder> sb) {
    sb.WriteString("{{");
    a.Pipe.writeTo(sb);
    sb.WriteString("}}");
}

internal static ж<Tree> treeᴍ(ref ActionNode a) {
    return a.tr;
}

internal static Node Copyᴍ(ref ActionNode a) {
    return new ActionNodeжNode(a.tr.newAction(a.Pos, a.Line, a.Pipe.CopyPipe()));
}

} partial class parse_package {

// CommandNode holds a command (a pipeline inside an evaluating action).
partial struct CommandNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<Node> Args; // Arguments in lexical order: Identifier, field, or constant.
}

} partial class parse_package {

internal static ж<CommandNode> newCommandᴍ(ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new CommandNode(tr: Ꮡt, NodeType: NodeCommand, Pos: pos));
}

} partial class parse_package {

internal static void appendᴍ(ref CommandNode c, Node arg) {
    c.Args = builtin.append(c.Args, arg);
}

internal static @string Stringᴍ(ref CommandNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref CommandNode c, ж<strings.Builder> sb) {
    foreach (var (i, arg) in range(c.Args)) {
        if (i > 0) {
            sb.WriteByte((byte)' ');
        }
        var (argΔ1, ok) = arg._<ж<PipeNode>>(ᐧ); if (ok) {
            sb.WriteByte((byte)'(');
            argΔ1.writeTo(sb);
            sb.WriteByte((byte)')');
            continue;
        }
        arg.writeTo(sb);
    }
}

internal static ж<Tree> treeᴍ(ref CommandNode c) {
    return c.tr;
}

internal static Node Copyᴍ(ж<CommandNode> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (Ꮡc == nil) {
        return new CommandNodeжNode(Ꮡc);
    }
    var n = c.tr.newCommand(c.Pos);
    foreach (var (_, cΔ1) in range(c.Args)) {
        n.append(cΔ1.Copy());
    }
    return new CommandNodeжNode(n);
}

} partial class parse_package {

// IdentifierNode holds an identifier.
partial struct IdentifierNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public @string Ident; // The identifier's name.
}

// NewIdentifier returns a new [IdentifierNode] with the given identifier name.
public static ж<IdentifierNode> NewIdentifier(@string ident) {
    return Ꮡ(new IdentifierNode(NodeType: NodeIdentifier, Ident: ident));
}

} partial class parse_package {

// SetPos sets the position. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
internal static ж<IdentifierNode> SetPosᴍ(ж<IdentifierNode> Ꮡi, Pos pos) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.Pos = pos;
    return Ꮡi;
}

// SetTree sets the parent tree for the node. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
internal static ж<IdentifierNode> SetTreeᴍ(ж<IdentifierNode> Ꮡi, ж<Tree> t) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.tr = t;
    return Ꮡi;
}

internal static @string Stringᴍ(ref IdentifierNode i) {
    return i.Ident;
}

internal static void writeToᴍ(ref IdentifierNode i, ж<strings.Builder> sb) {
    sb.WriteString(i.String());
}

internal static ж<Tree> treeᴍ(ref IdentifierNode i) {
    return i.tr;
}

internal static Node Copyᴍ(ref IdentifierNode i) {
    return new IdentifierNodeжNode(NewIdentifier(i.Ident).SetTree(i.tr).SetPos(i.Pos));
}

} partial class parse_package {

// VariableNode holds a list of variable names, possibly with chained field
// accesses. The dollar sign is part of the (first) name.
partial struct VariableNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<@string> Ident; // Variable name and fields in lexical order.
}

} partial class parse_package {

internal static ж<VariableNode> newVariableᴍ(ж<Tree> Ꮡt, Pos pos, @string ident) {
    return Ꮡ(new VariableNode(tr: Ꮡt, NodeType: NodeVariable, Pos: pos, Ident: strings.Split(ident, ".")));
}

} partial class parse_package {

internal static @string Stringᴍ(ref VariableNode v) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    v.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref VariableNode v, ж<strings.Builder> sb) {
    foreach (var (i, id) in range(v.Ident)) {
        if (i > 0) {
            sb.WriteByte((byte)'.');
        }
        sb.WriteString(id);
    }
}

internal static ж<Tree> treeᴍ(ref VariableNode v) {
    return v.tr;
}

internal static Node Copyᴍ(ref VariableNode v) {
    return new VariableNodeжNode(Ꮡ(new VariableNode(tr: v.tr, NodeType: NodeVariable, Pos: v.Pos, Ident: appendꓸꓸꓸ(slice<@string>([]), v.Ident))));
}

} partial class parse_package {

// DotNode holds the special identifier '.'.
partial struct DotNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

} partial class parse_package {

internal static ж<DotNode> newDotᴍ(ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new DotNode(tr: Ꮡt, NodeType: NodeDot, Pos: pos));
}

} partial class parse_package {

internal static NodeType Typeᴍ(ref DotNode d) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeDot;
}

internal static @string Stringᴍ(ref DotNode d) {
    return ".";
}

internal static void writeToᴍ(ref DotNode d, ж<strings.Builder> sb) {
    sb.WriteString(d.String());
}

internal static ж<Tree> treeᴍ(ref DotNode d) {
    return d.tr;
}

internal static Node Copyᴍ(ref DotNode d) {
    return new DotNodeжNode(d.tr.newDot(d.Pos));
}

} partial class parse_package {

// NilNode holds the special identifier 'nil' representing an untyped nil constant.
partial struct NilNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

} partial class parse_package {

internal static ж<NilNode> newNilᴍ(ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new NilNode(tr: Ꮡt, NodeType: NodeNil, Pos: pos));
}

} partial class parse_package {

internal static NodeType Typeᴍ(ref NilNode n) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeNil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢᴍNilNode => ref nilˢᵇᴍNilNode;
private static readonly @string nilˢᵇᴍNilNode = "nil"u8;

internal static @string Stringᴍ(ref NilNode n) {
    return nilˢᴍNilNode;
}

internal static void writeToᴍ(ref NilNode n, ж<strings.Builder> sb) {
    sb.WriteString(n.String());
}

internal static ж<Tree> treeᴍ(ref NilNode n) {
    return n.tr;
}

internal static Node Copyᴍ(ref NilNode n) {
    return new NilNodeжNode(n.tr.newNil(n.Pos));
}

} partial class parse_package {

// FieldNode holds a field (identifier starting with '.').
// The names may be chained ('.x.y').
// The period is dropped from each ident.
partial struct FieldNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<@string> Ident; // The identifiers in lexical order.
}

} partial class parse_package {

internal static ж<FieldNode> newFieldᴍ(ж<Tree> Ꮡt, Pos pos, @string ident) {
    return Ꮡ(new FieldNode(tr: Ꮡt, NodeType: NodeField, Pos: pos, Ident: strings.Split(ident[1..], "."))); // [1:] to drop leading period
}

} partial class parse_package {

internal static @string Stringᴍ(ref FieldNode f) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    f.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref FieldNode f, ж<strings.Builder> sb) {
    foreach (var (_, id) in range(f.Ident)) {
        sb.WriteByte((byte)'.');
        sb.WriteString(id);
    }
}

internal static ж<Tree> treeᴍ(ref FieldNode f) {
    return f.tr;
}

internal static Node Copyᴍ(ref FieldNode f) {
    return new FieldNodeжNode(Ꮡ(new FieldNode(tr: f.tr, NodeType: NodeField, Pos: f.Pos, Ident: appendꓸꓸꓸ(slice<@string>([]), f.Ident))));
}

} partial class parse_package {

// ChainNode holds a term followed by a chain of field accesses (identifier starting with '.').
// The names may be chained ('.x.y').
// The periods are dropped from each ident.
partial struct ChainNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public Node Node;
    public slice<@string> Field; // The identifiers in lexical order.
}

} partial class parse_package {

internal static ж<ChainNode> newChainᴍ(ж<Tree> Ꮡt, Pos pos, Node node) {
    return Ꮡ(new ChainNode(tr: Ꮡt, NodeType: NodeChain, Pos: pos, Node: node));
}

} partial class parse_package {

// Add adds the named field (which should start with a period) to the end of the chain.
internal static void Addᴍ(ref ChainNode c, @string field) {
    if (len(field) == 0 || field[0] != '.') {
        throw panic("no dot in field");
    }
    field = field[1..]; // Remove leading dot.
    if (field == ""u8) {
        throw panic("empty field");
    }
    c.Field = append(c.Field, field);
}

internal static @string Stringᴍ(ref ChainNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeToᴍ(ref ChainNode c, ж<strings.Builder> sb) {
    var (_, ok) = c.Node._<ж<PipeNode>>(ᐧ); if (ok) {
        sb.WriteByte((byte)'(');
        c.Node.writeTo(sb);
        sb.WriteByte((byte)')');
    } else {
        c.Node.writeTo(sb);
    }
    foreach (var (_, field) in range(c.Field)) {
        sb.WriteByte((byte)'.');
        sb.WriteString(field);
    }
}

internal static ж<Tree> treeᴍ(ref ChainNode c) {
    return c.tr;
}

internal static Node Copyᴍ(ref ChainNode c) {
    return new ChainNodeжNode(Ꮡ(new ChainNode(tr: c.tr, NodeType: NodeChain, Pos: c.Pos, Node: c.Node, Field: appendꓸꓸꓸ(slice<@string>([]), c.Field))));
}

} partial class parse_package {

// BoolNode holds a boolean constant.
partial struct BoolNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public bool True; // The value of the boolean constant.
}

} partial class parse_package {

internal static ж<BoolNode> newBoolᴍ(ж<Tree> Ꮡt, Pos pos, bool @true) {
    return Ꮡ(new BoolNode(tr: Ꮡt, NodeType: NodeBool, Pos: pos, True: @true));
}

} partial class parse_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trueˢᴍBoolNode => ref trueˢᵇᴍBoolNode;
private static readonly @string trueˢᵇᴍBoolNode = "true"u8;
internal static ref readonly @string falseˢᴍBoolNode => ref falseˢᵇᴍBoolNode;
private static readonly @string falseˢᵇᴍBoolNode = "false"u8;

internal static @string Stringᴍ(ref BoolNode b) {
    if (b.True) {
        return trueˢᴍBoolNode;
    }
    return falseˢᴍBoolNode;
}

internal static void writeToᴍ(ref BoolNode b, ж<strings.Builder> sb) {
    sb.WriteString(b.String());
}

internal static ж<Tree> treeᴍ(ref BoolNode b) {
    return b.tr;
}

internal static Node Copyᴍ(ref BoolNode b) {
    return new BoolNodeжNode(b.tr.newBool(b.Pos, b.True));
}

} partial class parse_package {

// NumberNode holds a number: signed or unsigned integer, float, or complex.
// The value is parsed and stored under all the types that can represent the value.
// This simulates in a small amount of code the behavior of Go's ideal constants.
partial struct NumberNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public bool IsInt;            // Number has an integral value.
    public bool IsUint;           // Number has an unsigned integral value.
    public bool IsFloat;          // Number has a floating-point value.
    public bool IsComplex;        // Number is complex.
    public int64 Int64;           // The signed integer value.
    public uint64 Uint64;         // The unsigned integer value.
    public float64 Float64;       // The floating-point value.
    public complex128 Complex128; // The complex value.
    public @string Text;          // The original textual representation from the input.
}

} partial class parse_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string eEpPˢᴍTree => ref eEpPˢᵇᴍTree;
private static readonly @string eEpPˢᵇᴍTree = ".eEpP"u8;

internal static (ж<NumberNode>, error) newNumberᴍ(ж<Tree> Ꮡt, Pos pos, @string text, itemType typ) {
    var n = Ꮡ(new NumberNode(tr: Ꮡt, NodeType: NodeNumber, Pos: pos, Text: text));
    switch (typ) {
    case var ᴛ when ᴛ == itemCharConstant: {
        var (rune, _, tail, errΔ3) = strconv.UnquoteChar(text[1..], text[0]);
        if (errΔ3 != nil) {
            return (default, errΔ3);
        }
        if (tail != "'"u8) {
            return (default, fmt.Errorf("malformed character constant: %s"u8, text));
        }
        n.Int64 = (int64)rune;
        n.IsInt = true;
        n.Uint64 = (uint64)rune;
        n.IsUint = true;
        n.Float64 = (float64)rune; // odd but those are the rules.
        n.IsFloat = true;
        return (n, default);
    }
    case var ᴛ when ᴛ == itemComplex: {
        // fmt.Sscan can parse the pair, so let it do the work.
        var (_, errΔ4) = fmt.Sscan(text, n.ᏑComplex128); if (errΔ4 != nil) {
            return (default, errΔ4);
        }
        n.IsComplex = true;
        n.simplifyComplex();
        return (n, default);
    }}
    // Imaginary constants can only be complex unless they are zero.
    if (len(text) > 0 && text[len(text) - 1] == 'i') {
        var (f, errΔ5) = strconv.ParseFloat(text.slice(0, len(text) - 1), 64);
        if (errΔ5 == nil) {
            n.IsComplex = true;
            n.Complex128 = complex(0D, f);
            n.simplifyComplex();
            return (n, default);
        }
    }
    // Do integer test first so we get 0x123 etc.
    var (u, err) = strconv.ParseUint(text, 0, 64); // will fail for -0; fixed below.
    if (err == nil) {
        n.IsUint = true;
        n.Uint64 = u;
    }
    (var i, err) = strconv.ParseInt(text, 0, 64);
    if (err == nil) {
        n.IsInt = true;
        n.Int64 = i;
        if (i == 0) {
            n.IsUint = true; // in case of -0.
            n.Uint64 = u;
        }
    }
    // If an integer extraction succeeded, promote the float.
    if (n.IsInt) {
        n.IsFloat = true;
        n.Float64 = (float64)n.Int64;
    } else if (n.IsUint) {
        n.IsFloat = true;
        n.Float64 = (float64)n.Uint64;
    } else {
        var (f, errΔ6) = strconv.ParseFloat(text, 64);
        if (errΔ6 == nil) {
            // If we parsed it as a float but it looks like an integer,
            // it's a huge number too large to fit in an int. Reject it.
            if (!strings.ContainsAny(text, eEpPˢᴍTree)) {
                return (default, fmt.Errorf("integer overflow: %q"u8, text));
            }
            n.IsFloat = true;
            n.Float64 = f;
            // If a floating-point extraction succeeded, extract the int if needed.
            if (!n.IsInt && (float64)(int64)f == f) {
                n.IsInt = true;
                n.Int64 = (int64)f;
            }
            if (!n.IsUint && (float64)(uint64)f == f) {
                n.IsUint = true;
                n.Uint64 = (uint64)f;
            }
        }
    }
    if (!n.IsInt && !n.IsUint && !n.IsFloat) {
        return (default, fmt.Errorf("illegal number syntax: %q"u8, text));
    }
    return (n, default);
}

} partial class parse_package {

// simplifyComplex pulls out any other types that are represented by the complex number.
// These all require that the imaginary part be zero.
internal static void simplifyComplexᴍ(ref NumberNode n) {
    n.IsFloat = imag(n.Complex128) == 0D;
    if (n.IsFloat) {
        n.Float64 = real(n.Complex128);
        n.IsInt = (float64)(int64)n.Float64 == n.Float64;
        if (n.IsInt) {
            n.Int64 = (int64)n.Float64;
        }
        n.IsUint = (float64)(uint64)n.Float64 == n.Float64;
        if (n.IsUint) {
            n.Uint64 = (uint64)n.Float64;
        }
    }
}

internal static @string Stringᴍ(ref NumberNode n) {
    return n.Text;
}

internal static void writeToᴍ(ref NumberNode n, ж<strings.Builder> sb) {
    sb.WriteString(n.String());
}

internal static ж<Tree> treeᴍ(ref NumberNode n) {
    return n.tr;
}

internal static Node Copyᴍ(ref NumberNode n) {
    var nn = @new<NumberNode>();
    nn.Value = n; // Easy, fast, correct.
    return new NumberNodeжNode(nn);
}

} partial class parse_package {

// StringNode holds a string constant. The value has been "unquoted".
partial struct StringNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public @string Quoted; // The original text of the string, with quotes.
    public @string Text;   // The string, after quote processing.
}

} partial class parse_package {

internal static ж<StringNode> newStringᴍ(ж<Tree> Ꮡt, Pos pos, @string orig, @string text) {
    return Ꮡ(new StringNode(tr: Ꮡt, NodeType: NodeString, Pos: pos, Quoted: orig, Text: text));
}

} partial class parse_package {

internal static @string Stringᴍ(ref StringNode s) {
    return s.Quoted;
}

internal static void writeToᴍ(ref StringNode s, ж<strings.Builder> sb) {
    sb.WriteString(s.String());
}

internal static ж<Tree> treeᴍ(ref StringNode s) {
    return s.tr;
}

internal static Node Copyᴍ(ref StringNode s) {
    return new StringNodeжNode(s.tr.newString(s.Pos, s.Quoted, s.Text));
}

} partial class parse_package {

// endNode represents an {{end}} action.
// It does not appear in the final parse tree.
partial struct endNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

} partial class parse_package {

internal static ж<endNode> newEndᴍ(ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new endNode(tr: Ꮡt, NodeType: nodeEnd, Pos: pos));
}

} partial class parse_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string endˢᴍendNode => ref endˢᵇᴍendNode;
private static readonly @string endˢᵇᴍendNode = "{{end}}"u8;

internal static @string Stringᴍ(ref endNode e) {
    return endˢᴍendNode;
}

internal static void writeToᴍ(ref endNode e, ж<strings.Builder> sb) {
    sb.WriteString(e.String());
}

internal static ж<Tree> treeᴍ(ref endNode e) {
    return e.tr;
}

internal static Node Copyᴍ(ref endNode e) {
    return new endNodeжNode(e.tr.newEnd(e.Pos));
}

} partial class parse_package {

// elseNode represents an {{else}} action. Does not appear in the final tree.
partial struct elseNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
}

} partial class parse_package {

internal static ж<elseNode> newElseᴍ(ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new elseNode(tr: Ꮡt, NodeType: nodeElse, Pos: pos, Line: line));
}

} partial class parse_package {

internal static NodeType Typeᴍ(ref elseNode e) {
    return nodeElse;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string elseˢᴍelseNode => ref elseˢᵇᴍelseNode;
private static readonly @string elseˢᵇᴍelseNode = "{{else}}"u8;

internal static @string Stringᴍ(ref elseNode e) {
    return elseˢᴍelseNode;
}

internal static void writeToᴍ(ref elseNode e, ж<strings.Builder> sb) {
    sb.WriteString(e.String());
}

internal static ж<Tree> treeᴍ(ref elseNode e) {
    return e.tr;
}

internal static Node Copyᴍ(ref elseNode e) {
    return new elseNodeжNode(e.tr.newElse(e.Pos, e.Line));
}

} partial class parse_package {

// BranchNode is the common representation of if, range, and with.
partial struct BranchNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line;            // The line number in the input. Deprecated: Kept for compatibility.
    public ж<PipeNode> Pipe;     // The pipeline to be evaluated.
    public ж<ListNode> List;     // What to execute if the value is non-empty.
    public ж<ListNode> ElseList; // What to execute if the value is empty (nil if absent).
}

} partial class parse_package {

internal static @string Stringᴍ(ref BranchNode b) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    b.writeTo(Ꮡsb);
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeˢᴍBranchNode => ref rangeˢᵇᴍBranchNode;
private static readonly @string rangeˢᵇᴍBranchNode = "range"u8;
internal static ref readonly @string withˢᴍBranchNode => ref withˢᵇᴍBranchNode;
private static readonly @string withˢᵇᴍBranchNode = "with"u8;
internal static ref readonly @string elseˢᴍBranchNode => ref elseˢᵇᴍBranchNode;
private static readonly @string elseˢᵇᴍBranchNode = "{{else}}"u8;
internal static ref readonly @string endˢᴍBranchNode => ref endˢᵇᴍBranchNode;
private static readonly @string endˢᵇᴍBranchNode = "{{end}}"u8;

internal static void writeToᴍ(ref BranchNode b, ж<strings.Builder> sb) {
    @string name = ""u8;
    switch (b.NodeType) {
    case var ᴛ when ᴛ == NodeIf: {
        name = "if";
        break;
    }
    case var ᴛ when ᴛ == NodeRange: {
        name = rangeˢᴍBranchNode;
        break;
    }
    case var ᴛ when ᴛ == NodeWith: {
        name = withˢᴍBranchNode;
        break;
    }
    default: {
        throw panic("unknown branch type");
        break;
    }}
    sb.WriteString("{{");
    sb.WriteString(name);
    sb.WriteByte((byte)' ');
    b.Pipe.writeTo(sb);
    sb.WriteString("}}");
    b.List.writeTo(sb);
    if (b.ElseList != nil) {
        sb.WriteString(elseˢᴍBranchNode);
        b.ElseList.writeTo(sb);
    }
    sb.WriteString(endˢᴍBranchNode);
}

internal static ж<Tree> treeᴍ(ref BranchNode b) {
    return b.tr;
}

internal static Node Copyᴍ(ref BranchNode b) {
    switch (b.NodeType) {
    case var ᴛ when ᴛ == NodeIf: {
        return new IfNodeжNode(b.tr.newIf(b.Pos, b.Line, b.Pipe, b.List, b.ElseList));
    }
    case var ᴛ when ᴛ == NodeRange: {
        return new RangeNodeжNode(b.tr.newRange(b.Pos, b.Line, b.Pipe, b.List, b.ElseList));
    }
    case var ᴛ when ᴛ == NodeWith: {
        return new WithNodeжNode(b.tr.newWith(b.Pos, b.Line, b.Pipe, b.List, b.ElseList));
    }
    default: {
        throw panic("unknown branch type");
        break;
    }}
}

} partial class parse_package {

// IfNode represents an {{if}} action and its commands.
partial struct IfNode {
    /*embed*/ public BranchNode BranchNode;
}

} partial class parse_package {

internal static ж<IfNode> newIfᴍ(ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new IfNode(new BranchNode(tr: Ꮡt, NodeType: NodeIf, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

} partial class parse_package {

internal static Node Copyᴍ(ref IfNode i) {
    return new IfNodeжNode(i.tr.newIf(i.Pos, i.Line, i.Pipe.CopyPipe(), i.List.CopyList(), i.ElseList.CopyList()));
}

} partial class parse_package {

// BreakNode represents a {{break}} action.
partial struct BreakNode {
    internal ж<Tree> tr;
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    public nint Line;
}

} partial class parse_package {

internal static ж<BreakNode> newBreakᴍ(ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new BreakNode(tr: Ꮡt, NodeType: NodeBreak, Pos: pos, Line: line));
}

} partial class parse_package {

internal static Node Copyᴍ(ref BreakNode b) { return new BreakNodeжNode(b.tr.newBreak(b.Pos, b.Line)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string breakˢᴍBreakNode => ref breakˢᵇᴍBreakNode;
private static readonly @string breakˢᵇᴍBreakNode = "{{break}}"u8;

internal static @string Stringᴍ(ref BreakNode b) { return breakˢᴍBreakNode; }
internal static ж<Tree> treeᴍ(ref BreakNode b) { return b.tr; }
internal static void writeToᴍ(ref BreakNode b, ж<strings.Builder> sb) { sb.WriteString(breakˢᴍBreakNode); }

} partial class parse_package {

// ContinueNode represents a {{continue}} action.
partial struct ContinueNode {
    internal ж<Tree> tr;
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    public nint Line;
}

} partial class parse_package {

internal static ж<ContinueNode> newContinueᴍ(ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new ContinueNode(tr: Ꮡt, NodeType: NodeContinue, Pos: pos, Line: line));
}

} partial class parse_package {

internal static Node Copyᴍ(ref ContinueNode c) { return new ContinueNodeжNode(c.tr.newContinue(c.Pos, c.Line)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string continueˢᴍContinueNode => ref continueˢᵇᴍContinueNode;
private static readonly @string continueˢᵇᴍContinueNode = "{{continue}}"u8;

internal static @string Stringᴍ(ref ContinueNode c) { return continueˢᴍContinueNode; }
internal static ж<Tree> treeᴍ(ref ContinueNode c) { return c.tr; }
internal static void writeToᴍ(ref ContinueNode c, ж<strings.Builder> sb) { sb.WriteString(continueˢᴍContinueNode); }

} partial class parse_package {

// RangeNode represents a {{range}} action and its commands.
partial struct RangeNode {
    /*embed*/ public BranchNode BranchNode;
}

} partial class parse_package {

internal static ж<RangeNode> newRangeᴍ(ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new RangeNode(new BranchNode(tr: Ꮡt, NodeType: NodeRange, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

} partial class parse_package {

internal static Node Copyᴍ(ref RangeNode r) {
    return new RangeNodeжNode(r.tr.newRange(r.Pos, r.Line, r.Pipe.CopyPipe(), r.List.CopyList(), r.ElseList.CopyList()));
}

} partial class parse_package {

// WithNode represents a {{with}} action and its commands.
partial struct WithNode {
    /*embed*/ public BranchNode BranchNode;
}

} partial class parse_package {

internal static ж<WithNode> newWithᴍ(ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new WithNode(new BranchNode(tr: Ꮡt, NodeType: NodeWith, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

} partial class parse_package {

internal static Node Copyᴍ(ref WithNode w) {
    return new WithNodeжNode(w.tr.newWith(w.Pos, w.Line, w.Pipe.CopyPipe(), w.List.CopyList(), w.ElseList.CopyList()));
}

} partial class parse_package {

// TemplateNode represents a {{template}} action.
partial struct TemplateNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line;        // The line number in the input. Deprecated: Kept for compatibility.
    public @string Name;     // The name of the template (unquoted).
    public ж<PipeNode> Pipe; // The command to evaluate as dot for the template.
}

} partial class parse_package {

internal static ж<TemplateNode> newTemplateᴍ(ж<Tree> Ꮡt, Pos pos, nint line, @string name, ж<PipeNode> pipe) {
    return Ꮡ(new TemplateNode(tr: Ꮡt, NodeType: NodeTemplate, Pos: pos, Line: line, Name: name, Pipe: pipe));
}

} partial class parse_package {

internal static @string Stringᴍ(ref TemplateNode t) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    t.writeTo(Ꮡsb);
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string templateˢᴍTemplateNode => ref templateˢᵇᴍTemplateNode;
private static readonly @string templateˢᵇᴍTemplateNode = "{{template "u8;

internal static void writeToᴍ(ref TemplateNode t, ж<strings.Builder> sb) {
    sb.WriteString(templateˢᴍTemplateNode);
    sb.WriteString(strconv.Quote(t.Name));
    if (t.Pipe != nil) {
        sb.WriteByte((byte)' ');
        t.Pipe.writeTo(sb);
    }
    sb.WriteString("}}");
}

internal static ж<Tree> treeᴍ(ref TemplateNode t) {
    return t.tr;
}

internal static Node Copyᴍ(ref TemplateNode t) {
    return new TemplateNodeжNode(t.tr.newTemplate(t.Pos, t.Line, t.Name, t.Pipe.CopyPipe()));
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
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
// </GlyphForwarders>
