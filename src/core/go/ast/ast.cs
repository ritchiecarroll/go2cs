// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ast declares the types used to represent syntax trees for Go
// packages.
namespace go.go;

using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;

partial class ast_package {

// ----------------------------------------------------------------------------
// Interfaces
//
// There are 3 main classes of nodes: Expressions and type nodes,
// statement nodes, and declaration nodes. The node names usually
// match the corresponding Go spec production names to which they
// correspond. The node fields correspond to the individual parts
// of the respective productions.
//
// All nodes contain position information marking the beginning of
// the corresponding source text segment; it is accessible via the
// Pos accessor method. Nodes may contain additional position info
// for language constructs where comments may be found between parts
// of the construct (typically any larger, parenthesized subpart).
// That position information is needed to properly position comments
// when printing the construct.

// All node types implement the Node interface.
[GoType] partial interface Node {
    tokenꓸPos Pos(); // position of first character belonging to the node
    tokenꓸPos End(); // position of first character immediately after the node
}

// All expression nodes implement the Expr interface.
[GoType] partial interface Expr :
    Node
{
    void exprNode();
}

// All statement nodes implement the Stmt interface.
[GoType] partial interface Stmt :
    Node
{
    void stmtNode();
}

// All declaration nodes implement the Decl interface.
[GoType] partial interface Decl :
    Node
{
    void declNode();
}

// ----------------------------------------------------------------------------
// Comments

// A Comment node represents a single //-style or /*-style comment.
//
// The Text field contains the comment text without carriage returns (\r) that
// may have been present in the source. Because a comment's end position is
// computed using len(Text), the position reported by [Comment.End] does not match the
// true source end position for comments containing carriage returns.
[GoType] partial struct Comment {
    public tokenꓸPos Slash; // position of "/" starting the comment
    public @string Text;   // comment text (excluding '\n' for //-style comments)
}

[GoRecv] public static tokenꓸPos Pos(this ref Comment c) {
    return c.Slash;
}

[GoRecv] public static tokenꓸPos End(this ref Comment c) {
    return ((tokenꓸPos)((nint)c.Slash + len(c.Text)));
}

// A CommentGroup represents a sequence of comments
// with no other tokens and no empty lines between.
[GoType] partial struct CommentGroup {
    public slice<ж<Comment>> List; // len(List) > 0
}

[GoRecv] public static tokenꓸPos Pos(this ref CommentGroup g) {
    return g.List[0].Pos();
}

[GoRecv] public static tokenꓸPos End(this ref CommentGroup g) {
    return g.List[len(g.List) - 1].End();
}

internal static bool isWhitespace(byte ch) {
    return ch == (rune)' ' || ch == (rune)'\t' || ch == (rune)'\n' || ch == (rune)'\r';
}

internal static @string stripTrailingWhitespace(@string s) {
    nint i = len(s);
    while (i > 0 && isWhitespace(s[i - 1])) {
        i--;
    }
    return s.slice(0, i);
}

// Text returns the text of the comment.
// Comment markers (//, /*, and */), the first space of a line comment, and
// leading and trailing empty lines are removed.
// Comment directives like "//line" and "//go:noinline" are also removed.
// Multiple empty lines are reduced to one, and trailing space on lines is trimmed.
// Unless the result is empty, it is newline-terminated.
public static @string Text(this ж<CommentGroup> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (Ꮡg == nil) {
        return ""u8;
    }
    var comments = new slice<@string>(len(g.List));
    foreach (var (i, c) in g.List) {
        comments[i] = c.Value.Text;
    }
    var lines = new slice<@string>(0, 10); // most comments are less than 10 lines
    foreach (var (_, vᴛ1) in comments) {
        var c = vᴛ1;

        // Remove comment markers.
        // The parser has given us exactly the comment text.
        switch (c[1]) {
        case (rune)'/': {
            c = c[2..];
            if (len(c) == 0) {
                //-style comment (no newline at the end)
                // empty line
                break;
            }
            if (c[0] == (rune)' ') {
                // strip first space - required for Example tests
                c = c[1..];
                break;
            }
            if (isDirective(c)) {
                // Ignore //go:noinline, //line, and so on.
                continue;
            }
            break;
        }
        case (rune)'*': {
            c = c.slice(2, len(c) - 2);
            break;
        }}

        /*-style comment */
        // Split on newlines.
        var cl = strings.Split(c, "\n"u8);
        // Walk lines, stripping trailing white space and adding to list.
        foreach (var (_, l) in cl) {
            lines = append(lines, stripTrailingWhitespace(l));
        }
    }
    // Remove leading blank lines; convert runs of
    // interior blank lines to a single blank line.
    nint n = 0;
    foreach (var (_, line) in lines) {
        if (line != ""u8 || n > 0 && lines[n - 1] != "") {
            lines[n] = line;
            n++;
        }
    }
    lines = lines.slice(0, n);
    // Add final "" entry to get trailing newline from Join.
    if (n > 0 && lines[n - 1] != "") {
        lines = append(lines, ""u8);
    }
    return strings.Join(lines, "\n"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lineˢ = "line "u8;
internal static readonly @string externˢ = "extern "u8;
internal static readonly @string exportˢ = "export "u8;

// isDirective reports whether c is a comment directive.
// This code is also in go/printer.
internal static bool isDirective(@string c) {
    // "//line " is a line directive.
    // "//extern " is for gccgo.
    // "//export " is for cgo.
    // (The // has been removed.)
    if (strings.HasPrefix(c, lineˢ) || strings.HasPrefix(c, externˢ) || strings.HasPrefix(c, exportˢ)) {
        return true;
    }
    // "//[a-z0-9]+:[a-z0-9]"
    // (The // has been removed.)
    nint colon = strings.Index(c, ":"u8);
    if (colon <= 0 || colon + 1 >= len(c)) {
        return false;
    }
    for (nint i = 0; i <= colon + 1; i++) {
        if (i == colon) {
            continue;
        }
        var b = c[i];
        if (!((rune)'a' <= b && b <= (rune)'z' || (rune)'0' <= b && b <= (rune)'9')) {
            return false;
        }
    }
    return true;
}

// ----------------------------------------------------------------------------
// Expressions and types

// A Field represents a Field declaration list in a struct type,
// a method list in an interface type, or a parameter/result declaration
// in a signature.
// [Field.Names] is nil for unnamed parameters (parameter lists which only contain types)
// and embedded struct fields. In the latter case, the field name is the type name.
[GoType] partial struct Field {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public slice<ж<Ident>> Names; // field/method/(type) parameter names; or nil
    public Expr Type;          // field/method/parameter type; or nil
    public ж<BasicLit> Tag;  // field tag; or nil
    public ж<CommentGroup> Comment; // line comments; or nil
}

[GoRecv] public static tokenꓸPos Pos(this ref Field f) {
    if (len(f.Names) > 0) {
        return f.Names[0].Pos();
    }
    if (f.Type != default!) {
        return f.Type.Pos();
    }
    return token.NoPos;
}

[GoRecv] public static tokenꓸPos End(this ref Field f) {
    if (f.Tag != nil) {
        return f.Tag.End();
    }
    if (f.Type != default!) {
        return f.Type.End();
    }
    if (len(f.Names) > 0) {
        return f.Names[len(f.Names) - 1].End();
    }
    return token.NoPos;
}

// A FieldList represents a list of Fields, enclosed by parentheses,
// curly braces, or square brackets.
[GoType] partial struct FieldList {
    public tokenꓸPos Opening; // position of opening parenthesis/brace/bracket, if any
    public slice<ж<Field>> List; // field list; or nil
    public tokenꓸPos Closing; // position of closing parenthesis/brace/bracket, if any
}

[GoRecv] public static tokenꓸPos Pos(this ref FieldList f) {
    if (f.Opening.IsValid()) {
        return f.Opening;
    }
    // the list should not be empty in this case;
    // be conservative and guard against bad ASTs
    if (len(f.List) > 0) {
        return f.List[0].Pos();
    }
    return token.NoPos;
}

[GoRecv] public static tokenꓸPos End(this ref FieldList f) {
    if (f.Closing.IsValid()) {
        return f.Closing + 1;
    }
    // the list should not be empty in this case;
    // be conservative and guard against bad ASTs
    {
        nint n = len(f.List); if (n > 0) {
            return f.List[n - 1].End();
        }
    }
    return token.NoPos;
}

// NumFields returns the number of parameters or struct fields represented by a [FieldList].
public static nint NumFields(this ж<FieldList> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    nint n = 0;
    if (Ꮡf != nil) {
        foreach (var (_, g) in f.List) {
            nint m = len((~g).Names);
            if (m == 0) {
                m = 1;
            }
            n += m;
        }
    }
    return n;
}

// An expression is represented by a tree consisting of one
// or more of the following concrete expression nodes.
[GoType] partial struct BadExpr {
    public tokenꓸPos From, To; // position range of bad expression
}


[GoType] partial struct Ident {
    public tokenꓸPos NamePos; // identifier position
    public @string Name;   // identifier name
    public ж<Object> Obj; // denoted object, or nil. Deprecated: see Object.
}


[GoType] partial struct Ellipsis {
    public tokenꓸPos ΔEllipsis; // position of "..."
    public Expr Elt;      // ellipsis element type (parameter lists only); or nil
}


[GoType] partial struct BasicLit {
    public tokenꓸPos ValuePos;   // literal position
    public token.Token Kind; // token.INT, token.FLOAT, token.IMAG, token.CHAR, or token.STRING
    public @string Value;     // literal string; e.g. 42, 0x7f, 3.14, 1e-9, 2.4i, 'a', '\x7f', "foo" or `\m\n\o`
}


[GoType] partial struct FuncLit {
    public ж<FuncType> Type; // function type
    public ж<BlockStmt> Body; // function body
}


[GoType] partial struct CompositeLit {
    public Expr Type;      // literal type; or nil
    public tokenꓸPos Lbrace; // position of "{"
    public slice<Expr> Elts; // list of composite elements; or nil
    public tokenꓸPos Rbrace; // position of "}"
    public bool Incomplete;      // true if (source) expressions are missing in the Elts list
}


[GoType] partial struct ParenExpr {
    public tokenꓸPos Lparen; // position of "("
    public Expr X;      // parenthesized expression
    public tokenꓸPos Rparen; // position of ")"
}


[GoType] partial struct SelectorExpr {
    public Expr X;   // expression
    public ж<Ident> Sel; // field selector
}


[GoType] partial struct IndexExpr {
    public Expr X;      // expression
    public tokenꓸPos Lbrack; // position of "["
    public Expr Index;      // index expression
    public tokenꓸPos Rbrack; // position of "]"
}


[GoType] partial struct IndexListExpr {
    public Expr X;      // expression
    public tokenꓸPos Lbrack; // position of "["
    public slice<Expr> Indices; // index expressions
    public tokenꓸPos Rbrack; // position of "]"
}


[GoType] partial struct SliceExpr {
    public Expr X;      // expression
    public tokenꓸPos Lbrack; // position of "["
    public Expr Low;      // begin of slice range; or nil
    public Expr High;      // end of slice range; or nil
    public Expr Max;      // maximum capacity of slice; or nil
    public bool Slice3;      // true if 3-index slice (2 colons present)
    public tokenꓸPos Rbrack; // position of "]"
}


[GoType] partial struct TypeAssertExpr {
    public Expr X;      // expression
    public tokenꓸPos Lparen; // position of "("
    public Expr Type;      // asserted type; nil means type switch X.(type)
    public tokenꓸPos Rparen; // position of ")"
}


[GoType] partial struct CallExpr {
    public Expr Fun;      // function expression
    public tokenꓸPos Lparen; // position of "("
    public slice<Expr> Args; // function arguments; or nil
    public tokenꓸPos Ellipsis; // position of "..." (token.NoPos if there is no "...")
    public tokenꓸPos Rparen; // position of ")"
}


[GoType] partial struct StarExpr {
    public tokenꓸPos Star; // position of "*"
    public Expr X;      // operand
}


[GoType] partial struct UnaryExpr {
    public tokenꓸPos OpPos;   // position of Op
    public token.Token Op; // operator
    public Expr X;        // operand
}


[GoType] partial struct BinaryExpr {
    public Expr X;        // left operand
    public tokenꓸPos OpPos;   // position of Op
    public token.Token Op; // operator
    public Expr Y;        // right operand
}


[GoType] partial struct KeyValueExpr {
    public Expr Key;
    public tokenꓸPos Colon; // position of ":"
    public Expr Value;
}

[GoType("num:nint")] partial struct ChanDir;

public static ChanDir SEND => /* 1 << iota */ 1;
public static ChanDir RECV => 2;

// A type is represented by a tree consisting of one
// or more of the following type-specific expression
// nodes.
[GoType] partial struct ArrayType {
    public tokenꓸPos Lbrack; // position of "["
    public Expr Len;      // Ellipsis node for [...]T array types, nil for slice types
    public Expr Elt;      // element type
}


[GoType] partial struct StructType {
    public tokenꓸPos Struct;  // position of "struct" keyword
    public ж<FieldList> Fields; // list of field declarations
    public bool Incomplete;       // true if (source) fields are missing in the Fields list
}

// Pointer types are represented via StarExpr nodes.

[GoType] partial struct FuncType {
    public tokenꓸPos Func;  // position of "func" keyword (token.NoPos if there is no "func")
    public ж<FieldList> TypeParams; // type parameters; or nil
    public ж<FieldList> Params; // (incoming) parameters; non-nil
    public ж<FieldList> Results; // (outgoing) results; or nil
}


[GoType] partial struct InterfaceType {
    public tokenꓸPos Interface;  // position of "interface" keyword
    public ж<FieldList> Methods; // list of embedded interfaces, methods, or types
    public bool Incomplete;       // true if (source) methods or types are missing in the Methods list
}


[GoType] partial struct MapType {
    public tokenꓸPos Map; // position of "map" keyword
    public Expr Key;
    public Expr Value;
}


[GoType] partial struct ChanType {
    public tokenꓸPos Begin; // position of "chan" keyword or "<-" (whichever comes first)
    public tokenꓸPos Arrow; // position of "<-" (token.NoPos if there is no "<-")
    public ChanDir Dir;   // channel direction
    public Expr Value;      // value type
}

// Pos and End implementations for expression/type nodes.
[GoRecv] public static tokenꓸPos Pos(this ref BadExpr x) {
    return x.From;
}

[GoRecv] public static tokenꓸPos Pos(this ref Ident x) {
    return x.NamePos;
}

[GoRecv] public static tokenꓸPos Pos(this ref Ellipsis x) {
    return x.ΔEllipsis;
}

[GoRecv] public static tokenꓸPos Pos(this ref BasicLit x) {
    return x.ValuePos;
}

[GoRecv] public static tokenꓸPos Pos(this ref FuncLit x) {
    return x.Type.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref CompositeLit x) {
    if (x.Type != default!) {
        return x.Type.Pos();
    }
    return x.Lbrace;
}

[GoRecv] public static tokenꓸPos Pos(this ref ParenExpr x) {
    return x.Lparen;
}

[GoRecv] public static tokenꓸPos Pos(this ref SelectorExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref IndexExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref IndexListExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref SliceExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref TypeAssertExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref CallExpr x) {
    return x.Fun.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref StarExpr x) {
    return x.Star;
}

[GoRecv] public static tokenꓸPos Pos(this ref UnaryExpr x) {
    return x.OpPos;
}

[GoRecv] public static tokenꓸPos Pos(this ref BinaryExpr x) {
    return x.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref KeyValueExpr x) {
    return x.Key.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref ArrayType x) {
    return x.Lbrack;
}

[GoRecv] public static tokenꓸPos Pos(this ref StructType x) {
    return x.Struct;
}

[GoRecv] public static tokenꓸPos Pos(this ref FuncType x) {
    if (x.Func.IsValid() || x.Params == nil) {
        // see issue 3870
        return x.Func;
    }
    return x.Params.Pos(); // interface method declarations have no "func" keyword
}

[GoRecv] public static tokenꓸPos Pos(this ref InterfaceType x) {
    return x.Interface;
}

[GoRecv] public static tokenꓸPos Pos(this ref MapType x) {
    return x.Map;
}

[GoRecv] public static tokenꓸPos Pos(this ref ChanType x) {
    return x.Begin;
}

[GoRecv] public static tokenꓸPos End(this ref BadExpr x) {
    return x.To;
}

[GoRecv] public static tokenꓸPos End(this ref Ident x) {
    return ((tokenꓸPos)((nint)x.NamePos + len(x.Name)));
}

[GoRecv] public static tokenꓸPos End(this ref Ellipsis x) {
    if (x.Elt != default!) {
        return x.Elt.End();
    }
    return x.ΔEllipsis + 3; // len("...")
}

[GoRecv] public static tokenꓸPos End(this ref BasicLit x) {
    return ((tokenꓸPos)((nint)x.ValuePos + len(x.Value)));
}

[GoRecv] public static tokenꓸPos End(this ref FuncLit x) {
    return x.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref CompositeLit x) {
    return x.Rbrace + 1;
}

[GoRecv] public static tokenꓸPos End(this ref ParenExpr x) {
    return x.Rparen + 1;
}

[GoRecv] public static tokenꓸPos End(this ref SelectorExpr x) {
    return x.Sel.End();
}

[GoRecv] public static tokenꓸPos End(this ref IndexExpr x) {
    return x.Rbrack + 1;
}

[GoRecv] public static tokenꓸPos End(this ref IndexListExpr x) {
    return x.Rbrack + 1;
}

[GoRecv] public static tokenꓸPos End(this ref SliceExpr x) {
    return x.Rbrack + 1;
}

[GoRecv] public static tokenꓸPos End(this ref TypeAssertExpr x) {
    return x.Rparen + 1;
}

[GoRecv] public static tokenꓸPos End(this ref CallExpr x) {
    return x.Rparen + 1;
}

[GoRecv] public static tokenꓸPos End(this ref StarExpr x) {
    return x.X.End();
}

[GoRecv] public static tokenꓸPos End(this ref UnaryExpr x) {
    return x.X.End();
}

[GoRecv] public static tokenꓸPos End(this ref BinaryExpr x) {
    return x.Y.End();
}

[GoRecv] public static tokenꓸPos End(this ref KeyValueExpr x) {
    return x.Value.End();
}

[GoRecv] public static tokenꓸPos End(this ref ArrayType x) {
    return x.Elt.End();
}

[GoRecv] public static tokenꓸPos End(this ref StructType x) {
    return x.Fields.End();
}

[GoRecv] public static tokenꓸPos End(this ref FuncType x) {
    if (x.Results != nil) {
        return x.Results.End();
    }
    return x.Params.End();
}

[GoRecv] public static tokenꓸPos End(this ref InterfaceType x) {
    return x.Methods.End();
}

[GoRecv] public static tokenꓸPos End(this ref MapType x) {
    return x.Value.End();
}

[GoRecv] public static tokenꓸPos End(this ref ChanType x) {
    return x.Value.End();
}

// exprNode() ensures that only expression/type nodes can be
// assigned to an Expr.
[GoRecv] internal static void exprNode(this ref BadExpr _) {
}

[GoRecv] internal static void exprNode(this ref Ident _) {
}

[GoRecv] internal static void exprNode(this ref Ellipsis _) {
}

[GoRecv] internal static void exprNode(this ref BasicLit _) {
}

[GoRecv] internal static void exprNode(this ref FuncLit _) {
}

[GoRecv] internal static void exprNode(this ref CompositeLit _) {
}

[GoRecv] internal static void exprNode(this ref ParenExpr _) {
}

[GoRecv] internal static void exprNode(this ref SelectorExpr _) {
}

[GoRecv] internal static void exprNode(this ref IndexExpr _) {
}

[GoRecv] internal static void exprNode(this ref IndexListExpr _) {
}

[GoRecv] internal static void exprNode(this ref SliceExpr _) {
}

[GoRecv] internal static void exprNode(this ref TypeAssertExpr _) {
}

[GoRecv] internal static void exprNode(this ref CallExpr _) {
}

[GoRecv] internal static void exprNode(this ref StarExpr _) {
}

[GoRecv] internal static void exprNode(this ref UnaryExpr _) {
}

[GoRecv] internal static void exprNode(this ref BinaryExpr _) {
}

[GoRecv] internal static void exprNode(this ref KeyValueExpr _) {
}

[GoRecv] internal static void exprNode(this ref ArrayType _) {
}

[GoRecv] internal static void exprNode(this ref StructType _) {
}

[GoRecv] internal static void exprNode(this ref FuncType _) {
}

[GoRecv] internal static void exprNode(this ref InterfaceType _) {
}

[GoRecv] internal static void exprNode(this ref MapType _) {
}

[GoRecv] internal static void exprNode(this ref ChanType _) {
}

// ----------------------------------------------------------------------------
// Convenience functions for Idents

// NewIdent creates a new [Ident] without position.
// Useful for ASTs generated by code other than the Go parser.
public static ж<Ident> NewIdent(@string name) {
    return Ꮡ(new Ident(token.NoPos, name, nil));
}

// IsExported reports whether name starts with an upper-case letter.
public static bool IsExported(@string name) {
    return token.IsExported(name);
}

// IsExported reports whether id starts with an upper-case letter.
[GoRecv] public static bool IsExported(this ref Ident id) {
    return token.IsExported(id.Name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

public static @string String(this ж<Ident> Ꮡid) {
    ref var id = ref Ꮡid.DerefOrNull();

    if (Ꮡid != nil) {
        return id.Name;
    }
    return nilˢ;
}

// ----------------------------------------------------------------------------
// Statements

// A statement is represented by a tree consisting of one
// or more of the following concrete statement nodes.
[GoType] partial struct BadStmt {
    public tokenꓸPos From, To; // position range of bad statement
}


[GoType] partial struct DeclStmt {
    public Decl Decl; // *GenDecl with CONST, TYPE, or VAR token
}


[GoType] partial struct EmptyStmt {
    public tokenꓸPos Semicolon; // position of following ";"
    public bool Implicit;      // if set, ";" was omitted in the source
}


[GoType] partial struct LabeledStmt {
    public ж<Ident> Label;
    public tokenꓸPos Colon; // position of ":"
    public Stmt Stmt;
}


[GoType] partial struct ExprStmt {
    public Expr X; // expression
}


[GoType] partial struct SendStmt {
    public Expr Chan;
    public tokenꓸPos Arrow; // position of "<-"
    public Expr Value;
}


[GoType] partial struct IncDecStmt {
    public Expr X;
    public tokenꓸPos TokPos;   // position of Tok
    public token.Token Tok; // INC or DEC
}


[GoType] partial struct AssignStmt {
    public slice<Expr> Lhs;
    public tokenꓸPos TokPos;   // position of Tok
    public token.Token Tok; // assignment token, DEFINE
    public slice<Expr> Rhs;
}


[GoType] partial struct GoStmt {
    public tokenꓸPos Go; // position of "go" keyword
    public ж<CallExpr> Call;
}


[GoType] partial struct DeferStmt {
    public tokenꓸPos Defer; // position of "defer" keyword
    public ж<CallExpr> Call;
}


[GoType] partial struct ReturnStmt {
    public tokenꓸPos Return; // position of "return" keyword
    public slice<Expr> Results; // result expressions; or nil
}


[GoType] partial struct BranchStmt {
    public tokenꓸPos TokPos;   // position of Tok
    public token.Token Tok; // keyword token (BREAK, CONTINUE, GOTO, FALLTHROUGH)
    public ж<Ident> Label;   // label name; or nil
}


[GoType] partial struct BlockStmt {
    public tokenꓸPos Lbrace; // position of "{"
    public slice<Stmt> List;
    public tokenꓸPos Rbrace; // position of "}", if any (may be absent due to syntax error)
}


[GoType] partial struct IfStmt {
    public tokenꓸPos If; // position of "if" keyword
    public Stmt Init;      // initialization statement; or nil
    public Expr Cond;      // condition
    public ж<BlockStmt> Body;
    public Stmt Else; // else branch; or nil
}


[GoType] partial struct CaseClause {
    public tokenꓸPos Case; // position of "case" or "default" keyword
    public slice<Expr> List; // list of expressions or types; nil means default case
    public tokenꓸPos Colon; // position of ":"
    public slice<Stmt> Body; // statement list; or nil
}


[GoType] partial struct SwitchStmt {
    public tokenꓸPos Switch;  // position of "switch" keyword
    public Stmt Init;       // initialization statement; or nil
    public Expr Tag;       // tag expression; or nil
    public ж<BlockStmt> Body; // CaseClauses only
}


[GoType] partial struct TypeSwitchStmt {
    public tokenꓸPos Switch;  // position of "switch" keyword
    public Stmt Init;       // initialization statement; or nil
    public Stmt Assign;       // x := y.(type) or y.(type)
    public ж<BlockStmt> Body; // CaseClauses only
}


[GoType] partial struct CommClause {
    public tokenꓸPos Case; // position of "case" or "default" keyword
    public Stmt Comm;      // send or receive statement; nil means default case
    public tokenꓸPos Colon; // position of ":"
    public slice<Stmt> Body; // statement list; or nil
}


[GoType] partial struct SelectStmt {
    public tokenꓸPos Select;  // position of "select" keyword
    public ж<BlockStmt> Body; // CommClauses only
}


[GoType] partial struct ForStmt {
    public tokenꓸPos For; // position of "for" keyword
    public Stmt Init;      // initialization statement; or nil
    public Expr Cond;      // condition; or nil
    public Stmt Post;      // post iteration statement; or nil
    public ж<BlockStmt> Body;
}


[GoType] partial struct RangeStmt {
    public tokenꓸPos For;   // position of "for" keyword
    public Expr Key, Value;        // Key, Value may be nil
    public tokenꓸPos TokPos;   // position of Tok; invalid if Key == nil
    public token.Token Tok; // ILLEGAL if Key == nil, ASSIGN, DEFINE
    public tokenꓸPos Range;   // position of "range" keyword
    public Expr X;        // value to range over
    public ж<BlockStmt> Body;
}

// Pos and End implementations for statement nodes.
[GoRecv] public static tokenꓸPos Pos(this ref BadStmt s) {
    return s.From;
}

[GoRecv] public static tokenꓸPos Pos(this ref DeclStmt s) {
    return s.Decl.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref EmptyStmt s) {
    return s.Semicolon;
}

[GoRecv] public static tokenꓸPos Pos(this ref LabeledStmt s) {
    return s.Label.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref ExprStmt s) {
    return s.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref SendStmt s) {
    return s.Chan.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref IncDecStmt s) {
    return s.X.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref AssignStmt s) {
    return s.Lhs[0].Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref GoStmt s) {
    return s.Go;
}

[GoRecv] public static tokenꓸPos Pos(this ref DeferStmt s) {
    return s.Defer;
}

[GoRecv] public static tokenꓸPos Pos(this ref ReturnStmt s) {
    return s.Return;
}

[GoRecv] public static tokenꓸPos Pos(this ref BranchStmt s) {
    return s.TokPos;
}

[GoRecv] public static tokenꓸPos Pos(this ref BlockStmt s) {
    return s.Lbrace;
}

[GoRecv] public static tokenꓸPos Pos(this ref IfStmt s) {
    return s.If;
}

[GoRecv] public static tokenꓸPos Pos(this ref CaseClause s) {
    return s.Case;
}

[GoRecv] public static tokenꓸPos Pos(this ref SwitchStmt s) {
    return s.Switch;
}

[GoRecv] public static tokenꓸPos Pos(this ref TypeSwitchStmt s) {
    return s.Switch;
}

[GoRecv] public static tokenꓸPos Pos(this ref CommClause s) {
    return s.Case;
}

[GoRecv] public static tokenꓸPos Pos(this ref SelectStmt s) {
    return s.Select;
}

[GoRecv] public static tokenꓸPos Pos(this ref ForStmt s) {
    return s.For;
}

[GoRecv] public static tokenꓸPos Pos(this ref RangeStmt s) {
    return s.For;
}

[GoRecv] public static tokenꓸPos End(this ref BadStmt s) {
    return s.To;
}

[GoRecv] public static tokenꓸPos End(this ref DeclStmt s) {
    return s.Decl.End();
}

[GoRecv] public static tokenꓸPos End(this ref EmptyStmt s) {
    if (s.Implicit) {
        return s.Semicolon;
    }
    return s.Semicolon + 1; /* len(";") */
}

[GoRecv] public static tokenꓸPos End(this ref LabeledStmt s) {
    return s.Stmt.End();
}

[GoRecv] public static tokenꓸPos End(this ref ExprStmt s) {
    return s.X.End();
}

[GoRecv] public static tokenꓸPos End(this ref SendStmt s) {
    return s.Value.End();
}

[GoRecv] public static tokenꓸPos End(this ref IncDecStmt s) {
    return s.TokPos + 2; /* len("++") */
}

[GoRecv] public static tokenꓸPos End(this ref AssignStmt s) {
    return s.Rhs[len(s.Rhs) - 1].End();
}

[GoRecv] public static tokenꓸPos End(this ref GoStmt s) {
    return s.Call.End();
}

[GoRecv] public static tokenꓸPos End(this ref DeferStmt s) {
    return s.Call.End();
}

[GoRecv] public static tokenꓸPos End(this ref ReturnStmt s) {
    {
        nint n = len(s.Results); if (n > 0) {
            return s.Results[n - 1].End();
        }
    }
    return s.Return + 6; // len("return")
}

[GoRecv] public static tokenꓸPos End(this ref BranchStmt s) {
    if (s.Label != nil) {
        return s.Label.End();
    }
    return ((tokenꓸPos)((nint)s.TokPos + len(s.Tok.String())));
}

[GoRecv] public static tokenꓸPos End(this ref BlockStmt s) {
    if (s.Rbrace.IsValid()) {
        return s.Rbrace + 1;
    }
    {
        nint n = len(s.List); if (n > 0) {
            return s.List[n - 1].End();
        }
    }
    return s.Lbrace + 1;
}

[GoRecv] public static tokenꓸPos End(this ref IfStmt s) {
    if (s.Else != default!) {
        return s.Else.End();
    }
    return s.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref CaseClause s) {
    {
        nint n = len(s.Body); if (n > 0) {
            return s.Body[n - 1].End();
        }
    }
    return s.Colon + 1;
}

[GoRecv] public static tokenꓸPos End(this ref SwitchStmt s) {
    return s.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref TypeSwitchStmt s) {
    return s.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref CommClause s) {
    {
        nint n = len(s.Body); if (n > 0) {
            return s.Body[n - 1].End();
        }
    }
    return s.Colon + 1;
}

[GoRecv] public static tokenꓸPos End(this ref SelectStmt s) {
    return s.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref ForStmt s) {
    return s.Body.End();
}

[GoRecv] public static tokenꓸPos End(this ref RangeStmt s) {
    return s.Body.End();
}

// stmtNode() ensures that only statement nodes can be
// assigned to a Stmt.
[GoRecv] internal static void stmtNode(this ref BadStmt _) {
}

[GoRecv] internal static void stmtNode(this ref DeclStmt _) {
}

[GoRecv] internal static void stmtNode(this ref EmptyStmt _) {
}

[GoRecv] internal static void stmtNode(this ref LabeledStmt _) {
}

[GoRecv] internal static void stmtNode(this ref ExprStmt _) {
}

[GoRecv] internal static void stmtNode(this ref SendStmt _) {
}

[GoRecv] internal static void stmtNode(this ref IncDecStmt _) {
}

[GoRecv] internal static void stmtNode(this ref AssignStmt _) {
}

[GoRecv] internal static void stmtNode(this ref GoStmt _) {
}

[GoRecv] internal static void stmtNode(this ref DeferStmt _) {
}

[GoRecv] internal static void stmtNode(this ref ReturnStmt _) {
}

[GoRecv] internal static void stmtNode(this ref BranchStmt _) {
}

[GoRecv] internal static void stmtNode(this ref BlockStmt _) {
}

[GoRecv] internal static void stmtNode(this ref IfStmt _) {
}

[GoRecv] internal static void stmtNode(this ref CaseClause _) {
}

[GoRecv] internal static void stmtNode(this ref SwitchStmt _) {
}

[GoRecv] internal static void stmtNode(this ref TypeSwitchStmt _) {
}

[GoRecv] internal static void stmtNode(this ref CommClause _) {
}

[GoRecv] internal static void stmtNode(this ref SelectStmt _) {
}

[GoRecv] internal static void stmtNode(this ref ForStmt _) {
}

[GoRecv] internal static void stmtNode(this ref RangeStmt _) {
}

// ----------------------------------------------------------------------------
// Declarations

// A Spec node represents a single (non-parenthesized) import,
// constant, type, or variable declaration.
[GoType] partial interface Spec :
    Node
{
    void specNode();
}


[GoType] partial struct ImportSpec {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public ж<Ident> Name;     // local package name (including "."); or nil
    public ж<BasicLit> Path;  // import path
    public ж<CommentGroup> Comment; // line comments; or nil
    public tokenꓸPos EndPos;     // end of spec (overrides Path.Pos if nonzero)
}


[GoType] partial struct ValueSpec {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public slice<ж<Ident>> Names; // value names (len(Names) > 0)
    public Expr Type;          // value type; or nil
    public slice<Expr> Values;   // initial values; or nil
    public ж<CommentGroup> Comment; // line comments; or nil
}


[GoType] partial struct TypeSpec {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public ж<Ident> Name;     // type name
    public ж<FieldList> TypeParams; // type parameters; or nil
    public tokenꓸPos Assign;     // position of '=', if any
    public Expr Type;          // *Ident, *ParenExpr, *SelectorExpr, *StarExpr, or any of the *XxxTypes
    public ж<CommentGroup> Comment; // line comments; or nil
}

// Pos and End implementations for spec nodes.
[GoRecv] public static tokenꓸPos Pos(this ref ImportSpec s) {
    if (s.Name != nil) {
        return s.Name.Pos();
    }
    return s.Path.Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref ValueSpec s) {
    return s.Names[0].Pos();
}

[GoRecv] public static tokenꓸPos Pos(this ref TypeSpec s) {
    return s.Name.Pos();
}

[GoRecv] public static tokenꓸPos End(this ref ImportSpec s) {
    if (s.EndPos != 0) {
        return s.EndPos;
    }
    return s.Path.End();
}

[GoRecv] public static tokenꓸPos End(this ref ValueSpec s) {
    {
        nint n = len(s.Values); if (n > 0) {
            return s.Values[n - 1].End();
        }
    }
    if (s.Type != default!) {
        return s.Type.End();
    }
    return s.Names[len(s.Names) - 1].End();
}

[GoRecv] public static tokenꓸPos End(this ref TypeSpec s) {
    return s.Type.End();
}

// specNode() ensures that only spec nodes can be
// assigned to a Spec.
[GoRecv] internal static void specNode(this ref ImportSpec _) {
}

[GoRecv] internal static void specNode(this ref ValueSpec _) {
}

[GoRecv] internal static void specNode(this ref TypeSpec _) {
}

// A declaration is represented by one of the following declaration nodes.
[GoType] partial struct BadDecl {
    public tokenꓸPos From, To; // position range of bad declaration
}


[GoType] partial struct GenDecl {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public tokenꓸPos TokPos;     // position of Tok
    public token.Token Tok;   // IMPORT, CONST, TYPE, or VAR
    public tokenꓸPos Lparen;     // position of '(', if any
    public slice<Spec> Specs;
    public tokenꓸPos Rparen; // position of ')', if any
}


[GoType] partial struct FuncDecl {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public ж<FieldList> Recv; // receiver (methods); or nil (functions)
    public ж<Ident> Name;     // function/method name
    public ж<FuncType> Type;  // function signature: type and value parameters, results, and position of "func" keyword
    public ж<BlockStmt> Body; // function body; or nil for external (non-Go) function
}

// Pos and End implementations for declaration nodes.
[GoRecv] public static tokenꓸPos Pos(this ref BadDecl d) {
    return d.From;
}

[GoRecv] public static tokenꓸPos Pos(this ref GenDecl d) {
    return d.TokPos;
}

[GoRecv] public static tokenꓸPos Pos(this ref FuncDecl d) {
    return d.Type.Pos();
}

[GoRecv] public static tokenꓸPos End(this ref BadDecl d) {
    return d.To;
}

[GoRecv] public static tokenꓸPos End(this ref GenDecl d) {
    if (d.Rparen.IsValid()) {
        return d.Rparen + 1;
    }
    return d.Specs[0].End();
}

[GoRecv] public static tokenꓸPos End(this ref FuncDecl d) {
    if (d.Body != nil) {
        return d.Body.End();
    }
    return d.Type.End();
}

// declNode() ensures that only declaration nodes can be
// assigned to a Decl.
[GoRecv] internal static void declNode(this ref BadDecl _) {
}

[GoRecv] internal static void declNode(this ref GenDecl _) {
}

[GoRecv] internal static void declNode(this ref FuncDecl _) {
}

// ----------------------------------------------------------------------------
// Files and packages

// A File node represents a Go source file.
//
// The Comments list contains all comments in the source file in order of
// appearance, including the comments that are pointed to from other nodes
// via Doc and Comment fields.
//
// For correct printing of source code containing comments (using packages
// go/format and go/printer), special care must be taken to update comments
// when a File's syntax tree is modified: For printing, comments are interspersed
// between tokens based on their position. If syntax tree nodes are
// removed or moved, relevant comments in their vicinity must also be removed
// (from the [File.Comments] list) or moved accordingly (by updating their
// positions). A [CommentMap] may be used to facilitate some of these operations.
//
// Whether and how a comment is associated with a node depends on the
// interpretation of the syntax tree by the manipulating program: except for Doc
// and [Comment] comments directly associated with nodes, the remaining comments
// are "free-floating" (see also issues [#18593], [#20744]).
//
// [#18593]: https://go.dev/issue/18593
// [#20744]: https://go.dev/issue/20744
[GoType] partial struct File {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public tokenꓸPos Package;     // position of "package" keyword
    public ж<Ident> Name;     // package name
    public slice<Decl> Decls;   // top-level declarations; or nil
    public tokenꓸPos FileStart, FileEnd;       // start and end of entire file
    public ж<Scope> Scope;       // package scope (this file only). Deprecated: see Object
    public slice<ж<ImportSpec>> Imports; // imports in this file
    public slice<ж<Ident>> Unresolved; // unresolved identifiers in this file. Deprecated: see Object
    public slice<ж<CommentGroup>> Comments; // list of all comments in the source file
    public @string GoVersion;         // minimum Go version required by //go:build or // +build directives
}

// Pos returns the position of the package declaration.
// It may be invalid, for example in an empty file.
//
// (Use FileStart for the start of the entire file. It is always valid.)
[GoRecv] public static tokenꓸPos Pos(this ref File f) {
    return f.Package;
}

// End returns the end of the last declaration in the file.
// It may be invalid, for example in an empty file.
//
// (Use FileEnd for the end of the entire file. It is always valid.)
[GoRecv] public static tokenꓸPos End(this ref File f) {
    {
        nint n = len(f.Decls); if (n > 0) {
            return f.Decls[n - 1].End();
        }
    }
    return f.Name.End();
}

// A Package node represents a set of source files
// collectively building a Go package.
//
// Deprecated: use the type checker [go/types] instead; see [Object].
[GoType] partial struct Package {
    public @string Name;            // package name
    public ж<Scope> Scope;          // package scope across all files
    public map<@string, ж<Object>> Imports; // map of package id -> package object
    public map<@string, ж<File>> Files; // Go source files by filename
}

[GoRecv] public static tokenꓸPos Pos(this ref Package p) {
    return token.NoPos;
}

[GoRecv] public static tokenꓸPos End(this ref Package p) {
    return token.NoPos;
}

// IsGenerated reports whether the file was generated by a program,
// not handwritten, by detecting the special comment described
// at https://go.dev/s/generatedcode.
//
// The syntax tree must have been parsed with the [parser.ParseComments] flag.
// Example:
//
//	f, err := parser.ParseFile(fset, filename, src, parser.ParseComments|parser.PackageClauseOnly)
//	if err != nil { ... }
//	gen := ast.IsGenerated(f)
public static bool IsGenerated(ж<File> Ꮡfile) {
    var (_, ok) = generator(ref (Ꮡfile).DerefOrNull());
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doNotEditˢ = " DO NOT EDIT."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ = "// Code generated "u8;

internal static (@string, bool) generator(ref File @file) {
    foreach (var (_, group) in @file.Comments) {
        foreach (var (_, comment) in (~group).List) {
            if (comment.Pos() > @file.Package) {
                break; // after package declaration
            }
            // opt: check Contains first to avoid unnecessary array allocation in Split.
            @string prefix = prefixᶜ;
            if (strings.Contains((~comment).Text, prefix)) {
                foreach (var (_, line) in strings.Split((~comment).Text, "\n"u8)) {
                    {
                        var (rest, ok) = strings.CutPrefix(line, prefix); if (ok) {
                            {
                                var (gen, okΔ1) = strings.CutSuffix(rest, doNotEditˢ); if (okΔ1) {
                                    return (gen, true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return ("", false);
}

// Unparen returns the expression with any enclosing parentheses removed.
public static Expr Unparen(Expr e) {
    while (ᐧ) {
        var (paren, ok) = e._<ж<ParenExpr>>(ᐧ);
        if (!ok) {
            return e;
        }
        e = paren.Value.X;
    }
}

} // end ast_package
