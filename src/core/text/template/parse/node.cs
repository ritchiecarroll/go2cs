// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Parse nodes.
namespace go.text.template;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class parse_package {

internal static @string textFormat = "%s"u8; // Changed to "%q" in tests for better error messages.

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

public static Pos Position(this Pos p) {
    return p;
}

// Type returns itself and provides an easy default implementation
// for embedding in a Node. Embedded in all non-trivial Nodes.
public static NodeType Type(this NodeType t) {
    return t;
}

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

internal static ж<ListNode> newList(this ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new ListNode(tr: Ꮡt, NodeType: NodeList, Pos: pos));
}

internal static void append(this ref ListNode l, Node n) {
    l.Nodes = builtin.append(l.Nodes, n);
}

internal static ж<Tree> tree(this ref ListNode l) {
    return l.tr;
}

public static @string String(this ref ListNode l) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    l.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref ListNode l, ж<strings.Builder> sb) {
    foreach (var (_, n) in l.Nodes) {
        n.writeTo(sb);
    }
}

public static ж<ListNode> CopyList(this ж<ListNode> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return Ꮡl;
    }
    var n = l.tr.newList(l.Pos);
    foreach (var (_, elem) in l.Nodes) {
        n.append(elem.Copy());
    }
    return n;
}

public static Node Copy(this ж<ListNode> Ꮡl) {
    return new ListNodeжNode(Ꮡl.CopyList());
}

// TextNode holds plain text.
partial struct TextNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<byte> Text; // The text; may span newlines.
}

internal static ж<TextNode> newText(this ж<Tree> Ꮡt, Pos pos, @string text) {
    return Ꮡ(new TextNode(tr: Ꮡt, NodeType: NodeText, Pos: pos, Text: slice<byte>(text)));
}

public static @string String(this ref TextNode t) {
    return fmt.Sprintf(textFormat, t.Text);
}

internal static void writeTo(this ref TextNode t, ж<strings.Builder> sb) {
    sb.WriteString(t.String());
}

internal static ж<Tree> tree(this ref TextNode t) {
    return t.tr;
}

public static Node Copy(this ref TextNode t) {
    return new TextNodeжNode(Ꮡ(new TextNode(tr: t.tr, NodeType: NodeText, Pos: t.Pos, Text: builtin.appendꓸꓸꓸ(slice<byte>([]), t.Text))));
}

// CommentNode holds a comment.
partial struct CommentNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public @string Text; // Comment text.
}

internal static ж<CommentNode> newComment(this ж<Tree> Ꮡt, Pos pos, @string text) {
    return Ꮡ(new CommentNode(tr: Ꮡt, NodeType: NodeComment, Pos: pos, Text: text));
}

public static @string String(this ref CommentNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref CommentNode c, ж<strings.Builder> sb) {
    sb.WriteString("{{"u8);
    sb.WriteString(c.Text);
    sb.WriteString("}}"u8);
}

internal static ж<Tree> tree(this ref CommentNode c) {
    return c.tr;
}

public static Node Copy(this ref CommentNode c) {
    return new CommentNodeжNode(Ꮡ(new CommentNode(tr: c.tr, NodeType: NodeComment, Pos: c.Pos, Text: c.Text)));
}

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

internal static ж<PipeNode> newPipeline(this ж<Tree> Ꮡt, Pos pos, nint line, slice<ж<VariableNode>> vars) {
    return Ꮡ(new PipeNode(tr: Ꮡt, NodeType: NodePipe, Pos: pos, Line: line, Decl: vars));
}

internal static void append(this ref PipeNode p, ж<CommandNode> command) {
    p.Cmds = builtin.append(p.Cmds, command);
}

public static @string String(this ref PipeNode p) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    p.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref PipeNode p, ж<strings.Builder> sb) {
    if (len(p.Decl) > 0) {
        foreach (var (i, v) in p.Decl) {
            if (i > 0) {
                sb.WriteString(", "u8);
            }
            v.writeTo(sb);
        }
        if (p.IsAssign) {
            sb.WriteString(" = "u8);
        } else {
            sb.WriteString(" := "u8);
        }
    }
    foreach (var (i, c) in p.Cmds) {
        if (i > 0) {
            sb.WriteString(" | "u8);
        }
        c.writeTo(sb);
    }
}

internal static ж<Tree> tree(this ref PipeNode p) {
    return p.tr;
}

public static ж<PipeNode> CopyPipe(this ж<PipeNode> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return Ꮡp;
    }
    var vars = new slice<ж<VariableNode>>(len(p.Decl));
    foreach (var (i, d) in p.Decl) {
        vars[i] = d.Copy()._<ж<VariableNode>>();
    }
    var n = p.tr.newPipeline(p.Pos, p.Line, vars);
    n.IsAssign = p.IsAssign;
    foreach (var (_, c) in p.Cmds) {
        n.append(c.Copy()._<ж<CommandNode>>());
    }
    return n;
}

public static Node Copy(this ж<PipeNode> Ꮡp) {
    return new PipeNodeжNode(Ꮡp.CopyPipe());
}

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

internal static ж<ActionNode> newAction(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe) {
    return Ꮡ(new ActionNode(tr: Ꮡt, NodeType: NodeAction, Pos: pos, Line: line, Pipe: pipe));
}

public static @string String(this ref ActionNode a) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    a.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref ActionNode a, ж<strings.Builder> sb) {
    sb.WriteString("{{"u8);
    a.Pipe.writeTo(sb);
    sb.WriteString("}}"u8);
}

internal static ж<Tree> tree(this ref ActionNode a) {
    return a.tr;
}

public static Node Copy(this ref ActionNode a) {
    return new ActionNodeжNode(a.tr.newAction(a.Pos, a.Line, a.Pipe.CopyPipe()));
}

// CommandNode holds a command (a pipeline inside an evaluating action).
partial struct CommandNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<Node> Args; // Arguments in lexical order: Identifier, field, or constant.
}

internal static ж<CommandNode> newCommand(this ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new CommandNode(tr: Ꮡt, NodeType: NodeCommand, Pos: pos));
}

internal static void append(this ref CommandNode c, Node arg) {
    c.Args = builtin.append(c.Args, arg);
}

public static @string String(this ref CommandNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref CommandNode c, ж<strings.Builder> sb) {
    foreach (var (i, arg) in c.Args) {
        if (i > 0) {
            sb.WriteByte((byte)' ');
        }
        {
            var (argΔ1, ok) = arg._<ж<PipeNode>>(ᐧ); if (ok) {
                sb.WriteByte((byte)'(');
                argΔ1.writeTo(sb);
                sb.WriteByte((byte)')');
                continue;
            }
        }
        arg.writeTo(sb);
    }
}

internal static ж<Tree> tree(this ref CommandNode c) {
    return c.tr;
}

public static Node Copy(this ж<CommandNode> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (Ꮡc == nil) {
        return new CommandNodeжNode(Ꮡc);
    }
    var n = c.tr.newCommand(c.Pos);
    foreach (var (_, cΔ1) in c.Args) {
        n.append(cΔ1.Copy());
    }
    return new CommandNodeжNode(n);
}

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

// SetPos sets the position. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
public static ж<IdentifierNode> SetPos(this ж<IdentifierNode> Ꮡi, Pos pos) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.Pos = pos;
    return Ꮡi;
}

// SetTree sets the parent tree for the node. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
public static ж<IdentifierNode> SetTree(this ж<IdentifierNode> Ꮡi, ж<Tree> t) {
    ref var i = ref Ꮡi.DerefOrNull();

    i.tr = t;
    return Ꮡi;
}

public static @string String(this ref IdentifierNode i) {
    return i.Ident;
}

internal static void writeTo(this ref IdentifierNode i, ж<strings.Builder> sb) {
    sb.WriteString(i.String());
}

internal static ж<Tree> tree(this ref IdentifierNode i) {
    return i.tr;
}

public static Node Copy(this ref IdentifierNode i) {
    return new IdentifierNodeжNode(NewIdentifier(i.Ident).SetTree(i.tr).SetPos(i.Pos));
}

// VariableNode holds a list of variable names, possibly with chained field
// accesses. The dollar sign is part of the (first) name.
partial struct VariableNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<@string> Ident; // Variable name and fields in lexical order.
}

internal static ж<VariableNode> newVariable(this ж<Tree> Ꮡt, Pos pos, @string ident) {
    return Ꮡ(new VariableNode(tr: Ꮡt, NodeType: NodeVariable, Pos: pos, Ident: strings.Split(ident, "."u8)));
}

public static @string String(this ref VariableNode v) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    v.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref VariableNode v, ж<strings.Builder> sb) {
    foreach (var (i, id) in v.Ident) {
        if (i > 0) {
            sb.WriteByte((byte)'.');
        }
        sb.WriteString(id);
    }
}

internal static ж<Tree> tree(this ref VariableNode v) {
    return v.tr;
}

public static Node Copy(this ref VariableNode v) {
    return new VariableNodeжNode(Ꮡ(new VariableNode(tr: v.tr, NodeType: NodeVariable, Pos: v.Pos, Ident: builtin.appendꓸꓸꓸ(slice<@string>([]), v.Ident))));
}

// DotNode holds the special identifier '.'.
partial struct DotNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

internal static ж<DotNode> newDot(this ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new DotNode(tr: Ꮡt, NodeType: NodeDot, Pos: pos));
}

public static NodeType Type(this ref DotNode d) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeDot;
}

public static @string String(this ref DotNode d) {
    return "."u8;
}

internal static void writeTo(this ref DotNode d, ж<strings.Builder> sb) {
    sb.WriteString(d.String());
}

internal static ж<Tree> tree(this ref DotNode d) {
    return d.tr;
}

public static Node Copy(this ref DotNode d) {
    return new DotNodeжNode(d.tr.newDot(d.Pos));
}

// NilNode holds the special identifier 'nil' representing an untyped nil constant.
partial struct NilNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

internal static ж<NilNode> newNil(this ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new NilNode(tr: Ꮡt, NodeType: NodeNil, Pos: pos));
}

public static NodeType Type(this ref NilNode n) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeNil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "nil"u8;

public static @string String(this ref NilNode n) {
    return nilˢ;
}

internal static void writeTo(this ref NilNode n, ж<strings.Builder> sb) {
    sb.WriteString(n.String());
}

internal static ж<Tree> tree(this ref NilNode n) {
    return n.tr;
}

public static Node Copy(this ref NilNode n) {
    return new NilNodeжNode(n.tr.newNil(n.Pos));
}

// FieldNode holds a field (identifier starting with '.').
// The names may be chained ('.x.y').
// The period is dropped from each ident.
partial struct FieldNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public slice<@string> Ident; // The identifiers in lexical order.
}

internal static ж<FieldNode> newField(this ж<Tree> Ꮡt, Pos pos, @string ident) {
    return Ꮡ(new FieldNode(tr: Ꮡt, NodeType: NodeField, Pos: pos, Ident: strings.Split(ident[1..], "."u8))); // [1:] to drop leading period
}

public static @string String(this ref FieldNode f) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    f.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref FieldNode f, ж<strings.Builder> sb) {
    foreach (var (_, id) in f.Ident) {
        sb.WriteByte((byte)'.');
        sb.WriteString(id);
    }
}

internal static ж<Tree> tree(this ref FieldNode f) {
    return f.tr;
}

public static Node Copy(this ref FieldNode f) {
    return new FieldNodeжNode(Ꮡ(new FieldNode(tr: f.tr, NodeType: NodeField, Pos: f.Pos, Ident: builtin.appendꓸꓸꓸ(slice<@string>([]), f.Ident))));
}

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

internal static ж<ChainNode> newChain(this ж<Tree> Ꮡt, Pos pos, Node node) {
    return Ꮡ(new ChainNode(tr: Ꮡt, NodeType: NodeChain, Pos: pos, Node: node));
}

// Add adds the named field (which should start with a period) to the end of the chain.
public static void Add(this ref ChainNode c, @string field) {
    if (len(field) == 0 || field[0] != '.') {
        throw panic("no dot in field");
    }
    field = field[1..]; // Remove leading dot.
    if (field == ""u8) {
        throw panic("empty field");
    }
    c.Field = builtin.append(c.Field, field);
}

public static @string String(this ref ChainNode c) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

internal static void writeTo(this ref ChainNode c, ж<strings.Builder> sb) {
    {
        var (_, ok) = c.Node._<ж<PipeNode>>(ᐧ); if (ok) {
            sb.WriteByte((byte)'(');
            c.Node.writeTo(sb);
            sb.WriteByte((byte)')');
        } else {
            c.Node.writeTo(sb);
        }
    }
    foreach (var (_, field) in c.Field) {
        sb.WriteByte((byte)'.');
        sb.WriteString(field);
    }
}

internal static ж<Tree> tree(this ref ChainNode c) {
    return c.tr;
}

public static Node Copy(this ref ChainNode c) {
    return new ChainNodeжNode(Ꮡ(new ChainNode(tr: c.tr, NodeType: NodeChain, Pos: c.Pos, Node: c.Node, Field: builtin.appendꓸꓸꓸ(slice<@string>([]), c.Field))));
}

// BoolNode holds a boolean constant.
partial struct BoolNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public bool True; // The value of the boolean constant.
}

internal static ж<BoolNode> newBool(this ж<Tree> Ꮡt, Pos pos, bool @true) {
    return Ꮡ(new BoolNode(tr: Ꮡt, NodeType: NodeBool, Pos: pos, True: @true));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

public static @string String(this ref BoolNode b) {
    if (b.True) {
        return trueˢ;
    }
    return falseˢ;
}

internal static void writeTo(this ref BoolNode b, ж<strings.Builder> sb) {
    sb.WriteString(b.String());
}

internal static ж<Tree> tree(this ref BoolNode b) {
    return b.tr;
}

public static Node Copy(this ref BoolNode b) {
    return new BoolNodeжNode(b.tr.newBool(b.Pos, b.True));
}

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eEpPˢ = ".eEpP"u8;

internal static (ж<NumberNode>, error) newNumber(this ж<Tree> Ꮡt, Pos pos, @string text, itemType typ) {
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
        {
            var (_, errΔ4) = fmt.Sscan(text, n.of(NumberNode.ᏑComplex128)); if (errΔ4 != nil) {
                return (default, errΔ4);
            }
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
            if (!strings.ContainsAny(text, eEpPˢ)) {
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

// simplifyComplex pulls out any other types that are represented by the complex number.
// These all require that the imaginary part be zero.
internal static void simplifyComplex(this ref NumberNode n) {
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

public static @string String(this ref NumberNode n) {
    return n.Text;
}

internal static void writeTo(this ref NumberNode n, ж<strings.Builder> sb) {
    sb.WriteString(n.String());
}

internal static ж<Tree> tree(this ref NumberNode n) {
    return n.tr;
}

public static Node Copy(this ref NumberNode n) {
    var nn = @new<NumberNode>();
    nn.Value = n; // Easy, fast, correct.
    return new NumberNodeжNode(nn);
}

// StringNode holds a string constant. The value has been "unquoted".
partial struct StringNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public @string Quoted; // The original text of the string, with quotes.
    public @string Text;   // The string, after quote processing.
}

internal static ж<StringNode> newString(this ж<Tree> Ꮡt, Pos pos, @string orig, @string text) {
    return Ꮡ(new StringNode(tr: Ꮡt, NodeType: NodeString, Pos: pos, Quoted: orig, Text: text));
}

public static @string String(this ref StringNode s) {
    return s.Quoted;
}

internal static void writeTo(this ref StringNode s, ж<strings.Builder> sb) {
    sb.WriteString(s.String());
}

internal static ж<Tree> tree(this ref StringNode s) {
    return s.tr;
}

public static Node Copy(this ref StringNode s) {
    return new StringNodeжNode(s.tr.newString(s.Pos, s.Quoted, s.Text));
}

// endNode represents an {{end}} action.
// It does not appear in the final parse tree.
partial struct endNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
}

internal static ж<endNode> newEnd(this ж<Tree> Ꮡt, Pos pos) {
    return Ꮡ(new endNode(tr: Ꮡt, NodeType: nodeEnd, Pos: pos));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string endˢ = "{{end}}"u8;

internal static @string String(this ref endNode e) {
    return endˢ;
}

internal static void writeTo(this ref endNode e, ж<strings.Builder> sb) {
    sb.WriteString(e.String());
}

internal static ж<Tree> tree(this ref endNode e) {
    return e.tr;
}

internal static Node Copy(this ref endNode e) {
    return new endNodeжNode(e.tr.newEnd(e.Pos));
}

// elseNode represents an {{else}} action. Does not appear in the final tree.
partial struct elseNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
}

internal static ж<elseNode> newElse(this ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new elseNode(tr: Ꮡt, NodeType: nodeElse, Pos: pos, Line: line));
}

internal static NodeType Type(this ref elseNode e) {
    return nodeElse;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string elseˢ = "{{else}}"u8;

internal static @string String(this ref elseNode e) {
    return elseˢ;
}

internal static void writeTo(this ref elseNode e, ж<strings.Builder> sb) {
    sb.WriteString(e.String());
}

internal static ж<Tree> tree(this ref elseNode e) {
    return e.tr;
}

internal static Node Copy(this ref elseNode e) {
    return new elseNodeжNode(e.tr.newElse(e.Pos, e.Line));
}

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

public static @string String(this ref BranchNode b) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    b.writeTo(Ꮡsb);
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeˢ = "range"u8;
internal static readonly @string withˢ = "with"u8;

internal static void writeTo(this ref BranchNode b, ж<strings.Builder> sb) {
    @string name = ""u8;
    switch (b.NodeType) {
    case var ᴛ when ᴛ == NodeIf: {
        name = "if"u8;
        break;
    }
    case var ᴛ when ᴛ == NodeRange: {
        name = rangeˢ;
        break;
    }
    case var ᴛ when ᴛ == NodeWith: {
        name = withˢ;
        break;
    }
    default: {
        throw panic("unknown branch type");
        break;
    }}
    sb.WriteString("{{"u8);
    sb.WriteString(name);
    sb.WriteByte((byte)' ');
    b.Pipe.writeTo(sb);
    sb.WriteString("}}"u8);
    b.List.writeTo(sb);
    if (b.ElseList != nil) {
        sb.WriteString(elseˢ);
        b.ElseList.writeTo(sb);
    }
    sb.WriteString(endˢ);
}

internal static ж<Tree> tree(this ref BranchNode b) {
    return b.tr;
}

public static Node Copy(this ref BranchNode b) {
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

// IfNode represents an {{if}} action and its commands.
partial struct IfNode {
    /*embed*/ public BranchNode BranchNode;
}

internal static ж<IfNode> newIf(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new IfNode(new BranchNode(tr: Ꮡt, NodeType: NodeIf, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

public static Node Copy(this ref IfNode i) {
    return new IfNodeжNode(i.tr.newIf(i.Pos, i.Line, i.Pipe.CopyPipe(), i.List.CopyList(), i.ElseList.CopyList()));
}

// BreakNode represents a {{break}} action.
partial struct BreakNode {
    internal ж<Tree> tr;
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    public nint Line;
}

internal static ж<BreakNode> newBreak(this ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new BreakNode(tr: Ꮡt, NodeType: NodeBreak, Pos: pos, Line: line));
}

public static Node Copy(this ref BreakNode b) {
    return new BreakNodeжNode(b.tr.newBreak(b.Pos, b.Line));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string breakˢ = "{{break}}"u8;

public static @string String(this ref BreakNode b) {
    return breakˢ;
}

internal static ж<Tree> tree(this ref BreakNode b) {
    return b.tr;
}

internal static void writeTo(this ref BreakNode b, ж<strings.Builder> sb) {
    sb.WriteString(breakˢ);
}

// ContinueNode represents a {{continue}} action.
partial struct ContinueNode {
    internal ж<Tree> tr;
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    public nint Line;
}

internal static ж<ContinueNode> newContinue(this ж<Tree> Ꮡt, Pos pos, nint line) {
    return Ꮡ(new ContinueNode(tr: Ꮡt, NodeType: NodeContinue, Pos: pos, Line: line));
}

public static Node Copy(this ref ContinueNode c) {
    return new ContinueNodeжNode(c.tr.newContinue(c.Pos, c.Line));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string continueˢ = "{{continue}}"u8;

public static @string String(this ref ContinueNode c) {
    return continueˢ;
}

internal static ж<Tree> tree(this ref ContinueNode c) {
    return c.tr;
}

internal static void writeTo(this ref ContinueNode c, ж<strings.Builder> sb) {
    sb.WriteString(continueˢ);
}

// RangeNode represents a {{range}} action and its commands.
partial struct RangeNode {
    /*embed*/ public BranchNode BranchNode;
}

internal static ж<RangeNode> newRange(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new RangeNode(new BranchNode(tr: Ꮡt, NodeType: NodeRange, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

public static Node Copy(this ref RangeNode r) {
    return new RangeNodeжNode(r.tr.newRange(r.Pos, r.Line, r.Pipe.CopyPipe(), r.List.CopyList(), r.ElseList.CopyList()));
}

// WithNode represents a {{with}} action and its commands.
partial struct WithNode {
    /*embed*/ public BranchNode BranchNode;
}

internal static ж<WithNode> newWith(this ж<Tree> Ꮡt, Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) {
    return Ꮡ(new WithNode(new BranchNode(tr: Ꮡt, NodeType: NodeWith, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

public static Node Copy(this ref WithNode w) {
    return new WithNodeжNode(w.tr.newWith(w.Pos, w.Line, w.Pipe.CopyPipe(), w.List.CopyList(), w.ElseList.CopyList()));
}

// TemplateNode represents a {{template}} action.
partial struct TemplateNode {
    /*embed*/ public NodeType NodeType;
    /*embed*/ public Pos Pos;
    internal ж<Tree> tr;
    public nint Line;        // The line number in the input. Deprecated: Kept for compatibility.
    public @string Name;     // The name of the template (unquoted).
    public ж<PipeNode> Pipe; // The command to evaluate as dot for the template.
}

internal static ж<TemplateNode> newTemplate(this ж<Tree> Ꮡt, Pos pos, nint line, @string name, ж<PipeNode> pipe) {
    return Ꮡ(new TemplateNode(tr: Ꮡt, NodeType: NodeTemplate, Pos: pos, Line: line, Name: name, Pipe: pipe));
}

public static @string String(this ref TemplateNode t) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    t.writeTo(Ꮡsb);
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string templateˢ = "{{template "u8;

internal static void writeTo(this ref TemplateNode t, ж<strings.Builder> sb) {
    sb.WriteString(templateˢ);
    sb.WriteString(strconv.Quote(t.Name));
    if (t.Pipe != nil) {
        sb.WriteByte((byte)' ');
        t.Pipe.writeTo(sb);
    }
    sb.WriteString("}}"u8);
}

internal static ж<Tree> tree(this ref TemplateNode t) {
    return t.tr;
}

public static Node Copy(this ref TemplateNode t) {
    return new TemplateNodeжNode(t.tr.newTemplate(t.Pos, t.Line, t.Name, t.Pipe.CopyPipe()));
}

} // end parse_package
