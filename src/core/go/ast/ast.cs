// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ast declares the types used to represent syntax trees for Go
// packages.
//
// Syntax trees may be constructed directly, but they are typically
// produced from Go source code by the parser; see the ParseFile
// function in package [go/parser].
namespace go.go;

using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using iter = iter_package;
using static ast_package;

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
partial interface Node {
    token.Pos Pos(); // position of first character belonging to the node
    token.Pos End(); // position of first character immediately after the node
}

// All expression nodes implement the Expr interface.
partial interface Expr :
    Node
{
    void exprNode();
}

// All statement nodes implement the Stmt interface.
partial interface Stmt :
    Node
{
    void stmtNode();
}

// All declaration nodes implement the Decl interface.
partial interface Decl :
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
partial struct Comment {
    public token.Pos Slash; // position of "/" starting the comment
    public @string Text;    // comment text (excluding '\n' for //-style comments)
}

} partial class astᴛComment {

public static token.Pos Pos(this ref Comment c) { return c.Slash; }
public static token.Pos End(this ref Comment c) { return (token.Pos)((nint)c.Slash + len(c.Text)); }

} partial class ast_package {

// A CommentGroup represents a sequence of comments
// with no other tokens and no empty lines between.
partial struct CommentGroup {
    public slice<ж<Comment>> List; // len(List) > 0
}

} partial class astᴛCommentGroup {

public static token.Pos Pos(this ref CommentGroup g) { return g.List[0].Pos(); }
public static token.Pos End(this ref CommentGroup g) { return g.List[len(g.List) - 1].End(); }

} partial class ast_package {

internal static bool isWhitespace(byte ch) { return ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r'; }

internal static @string stripTrailingWhitespace(@string s) {
    nint i = len(s);
    while (i > 0 && isWhitespace(s[i - 1])) {
        i--;
    }
    return s.slice(0, i);
}

} partial class astᴛCommentGroup {

// Text returns the text of the comment.
// Comment markers (//, /*, and */), the first space of a line comment, and
// leading and trailing empty lines are removed.
// Comment directives like "//line" and "//go:noinline" are also removed.
// Multiple empty lines are reduced to one, and trailing space on lines is trimmed.
// Unless the result is empty, it is newline-terminated.
public static @string Text(this ж<CommentGroup> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();
    var linesᴷ = new stackAppend<@string>();

    if (Ꮡg == nil) {
        return ""u8;
    }
    var comments = new slice<@string>(len(g.List));
    foreach (var (i, c) in range(g.List)) {
        comments[i] = c.Text;
    }

    var lines = new slice<@string>(0, 10); // most comments are less than 10 lines
    foreach (var (_, vᴛ1) in range(comments)) {
        var c = vᴛ1;

        // Remove comment markers.
        // The parser has given us exactly the comment text.
        switch (c[1]) {
        case (byte)'/': {
            //-style comment (no newline at the end)
            c = c[2..];
            if (len(c) == 0) {
                // empty line
                break;
            }
            if (c[0] == ' ') {
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
        case (byte)'*': {
            /*-style comment */
            c = c.slice(2, len(c) - 2);
            break;
        }}

        // Split on newlines.
        var cl = strings.SplitSeq(c, "\n");

        // Walk lines, stripping trailing white space and adding to list.
        foreach (var l in range<@string>(cl.Invoke)) {
            lines = append(lines, stripTrailingWhitespace(l));
        }
    }

    // Remove leading blank lines; convert runs of
    // interior blank lines to a single blank line.
    nint n = 0;
    foreach (var (_, line) in range(lines)) {
        if (line != ""u8 || n > 0 && lines[n - 1] != "") {
            lines[n] = line;
            n++;
        }
    }
    lines = lines.slice(0, n);

    // Add final "" entry to get trailing newline from Join.
    if (n > 0 && lines[n - 1] != "") {
        lines = append(linesᴷ, lines, ""u8);
    }

    return strings.Join(lines, "\n");
}

} partial class ast_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lineˢ => ref lineˢᵇ;
private static readonly @string lineˢᵇ = "line "u8;
internal static ref readonly @string externˢ => ref externˢᵇ;
private static readonly @string externˢᵇ = "extern "u8;
internal static ref readonly @string exportˢ => ref exportˢᵇ;
private static readonly @string exportˢᵇ = "export "u8;

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
    nint colon = strings.Index(c, ":");
    if (colon <= 0 || colon + 1 >= len(c)) {
        return false;
    }
    for (nint i = 0; i <= colon + 1; i++) {
        if (i == colon) {
            continue;
        }
        var b = c[i];
        if (!('a' <= b && b <= 'z' || '0' <= b && b <= '9')) {
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
partial struct Field {
    public ж<CommentGroup> Doc;     // associated documentation; or nil
    public slice<ж<Ident>> Names;   // field/method/(type) parameter names; or nil
    public Expr Type;               // field/method/parameter type; or nil
    public ж<BasicLit> Tag;         // field tag; or nil
    public ж<CommentGroup> Comment; // line comments; or nil
}

} partial class astᴛField {

public static token.Pos Pos(this ref Field f) {
    if (len(f.Names) > 0) {
        return f.Names[0].Pos();
    }
    if (f.Type != nil) {
        return f.Type.Pos();
    }
    return token.NoPos;
}

public static token.Pos End(this ref Field f) {
    if (f.Tag != nil) {
        return f.Tag.End();
    }
    if (f.Type != nil) {
        return f.Type.End();
    }
    if (len(f.Names) > 0) {
        return f.Names[len(f.Names) - 1].End();
    }
    return token.NoPos;
}

} partial class ast_package {

// A FieldList represents a list of Fields, enclosed by parentheses,
// curly braces, or square brackets.
partial struct FieldList {
    public token.Pos Opening;    // position of opening parenthesis/brace/bracket, if any
    public slice<ж<Field>> List; // field list; or nil
    public token.Pos Closing;    // position of closing parenthesis/brace/bracket, if any
}

} partial class astᴛFieldList {

public static token.Pos Pos(this ref FieldList f) {
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

public static token.Pos End(this ref FieldList f) {
    if (f.Closing.IsValid()) {
        return f.Closing + 1;
    }
    // the list should not be empty in this case;
    // be conservative and guard against bad ASTs
    nint n = len(f.List); if (n > 0) {
        return f.List[n - 1].End();
    }
    return token.NoPos;
}

// NumFields returns the number of parameters or struct fields represented by a [FieldList].
public static nint NumFields(this ж<FieldList> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    nint n = 0;
    if (Ꮡf != nil) {
        foreach (var (_, g) in range(f.List)) {
            nint m = len(g.Names);
            if (m == 0) {
                m = 1;
            }
            n += m;
        }
    }
    return n;
}

} partial class ast_package {

// An expression is represented by a tree consisting of one
// or more of the following concrete expression nodes.
// A BadExpr node is a placeholder for an expression containing
// syntax errors for which a correct expression node cannot be
// created.
//
partial struct BadExpr {
    public token.Pos From, To; // position range of bad expression
}

// An Ident node represents an identifier.
partial struct Ident {
    public token.Pos NamePos; // identifier position
    public @string Name;      // identifier name
    public ж<Object> Obj;     // denoted object, or nil. Deprecated: see Object.
}

// An Ellipsis node stands for the "..." type in a
// parameter list or the "..." length in an array type.
//
partial struct Ellipsis {
    public token.Pos ΔEllipsis; // position of "..."
    public Expr Elt;            // ellipsis element type (parameter lists only); or nil
}

// A BasicLit node represents a literal of basic type.
//
// Note that for the CHAR and STRING kinds, the literal is stored
// with its quotes. For example, for a double-quoted STRING, the
// first and the last rune in the Value field will be ". The
// [strconv.Unquote] and [strconv.UnquoteChar] functions can be
// used to unquote STRING and CHAR values, respectively.
//
// For raw string literals (Kind == token.STRING && Value[0] == '`'),
// the Value field contains the string text without carriage returns (\r) that
// may have been present in the source.
partial struct BasicLit {
    public token.Pos ValuePos; // literal position
    public token.Pos ValueEnd; // position immediately after the literal
    public token.Token Kind;   // token.INT, token.FLOAT, token.IMAG, token.CHAR, or token.STRING
    public @string Value;      // literal string; e.g. 42, 0x7f, 3.14, 1e-9, 2.4i, 'a', '\x7f', "foo" or `\m\n\o`
}

// A FuncLit node represents a function literal.
partial struct FuncLit {
    public ж<FuncType> Type;  // function type
    public ж<BlockStmt> Body; // function body
}

// A CompositeLit node represents a composite literal.
partial struct CompositeLit {
    public Expr Type;        // literal type; or nil
    public token.Pos Lbrace; // position of "{"
    public slice<Expr> Elts; // list of composite elements; or nil
    public token.Pos Rbrace; // position of "}"
    public bool Incomplete;  // true if (source) expressions are missing in the Elts list
}

// A ParenExpr node represents a parenthesized expression.
partial struct ParenExpr {
    public token.Pos Lparen; // position of "("
    public Expr X;           // parenthesized expression
    public token.Pos Rparen; // position of ")"
}

// A SelectorExpr node represents an expression followed by a selector.
partial struct SelectorExpr {
    public Expr X;       // expression
    public ж<Ident> Sel; // field selector
}

// An IndexExpr node represents an expression followed by an index.
partial struct IndexExpr {
    public Expr X;           // expression
    public token.Pos Lbrack; // position of "["
    public Expr Index;       // index expression
    public token.Pos Rbrack; // position of "]"
}

// An IndexListExpr node represents an expression followed by multiple
// indices.
partial struct IndexListExpr {
    public Expr X;              // expression
    public token.Pos Lbrack;    // position of "["
    public slice<Expr> Indices; // index expressions
    public token.Pos Rbrack;    // position of "]"
}

// A SliceExpr node represents an expression followed by slice indices.
partial struct SliceExpr {
    public Expr X;           // expression
    public token.Pos Lbrack; // position of "["
    public Expr Low;         // begin of slice range; or nil
    public Expr High;        // end of slice range; or nil
    public Expr Max;         // maximum capacity of slice; or nil
    public bool Slice3;      // true if 3-index slice (2 colons present)
    public token.Pos Rbrack; // position of "]"
}

// A TypeAssertExpr node represents an expression followed by a
// type assertion.
//
partial struct TypeAssertExpr {
    public Expr X;           // expression
    public token.Pos Lparen; // position of "("
    public Expr Type;        // asserted type; nil means type switch X.(type)
    public token.Pos Rparen; // position of ")"
}

// A CallExpr node represents an expression followed by an argument list.
partial struct CallExpr {
    public Expr Fun;           // function expression
    public token.Pos Lparen;   // position of "("
    public slice<Expr> Args;   // function arguments; or nil
    public token.Pos Ellipsis; // position of "..." (token.NoPos if there is no "...")
    public token.Pos Rparen;   // position of ")"
}

// A StarExpr node represents an expression of the form "*" Expression.
// Semantically it could be a unary "*" expression, or a pointer type.
//
partial struct StarExpr {
    public token.Pos Star; // position of "*"
    public Expr X;         // operand
}

// A UnaryExpr node represents a unary expression.
// Unary "*" expressions are represented via StarExpr nodes.
//
partial struct UnaryExpr {
    public token.Pos OpPos; // position of Op
    public token.Token Op;  // operator
    public Expr X;          // operand
}

// A BinaryExpr node represents a binary expression.
partial struct BinaryExpr {
    public Expr X;          // left operand
    public token.Pos OpPos; // position of Op
    public token.Token Op;  // operator
    public Expr Y;          // right operand
}

// A KeyValueExpr node represents (key : value) pairs
// in composite literals.
//
partial struct KeyValueExpr {
    public Expr Key;
    public token.Pos Colon; // position of ":"
    public Expr Value;
}

// The direction of a channel type is indicated by a bit
// mask including one or both of the following constants.
partial struct ChanDir /*num:nint*/;

public static ChanDir SEND => /* 1 << iota */ 1;
public static ChanDir RECV => 2;

// A type is represented by a tree consisting of one
// or more of the following type-specific expression
// nodes.
// An ArrayType node represents an array or slice type.
partial struct ArrayType {
    public token.Pos Lbrack; // position of "["
    public Expr Len;         // Ellipsis node for [...]T array types, nil for slice types
    public Expr Elt;         // element type
}

// A StructType node represents a struct type.
partial struct StructType {
    public token.Pos Struct;    // position of "struct" keyword
    public ж<FieldList> Fields; // list of field declarations
    public bool Incomplete;     // true if (source) fields are missing in the Fields list
}

// Pointer types are represented via StarExpr nodes.

// A FuncType node represents a function type.
partial struct FuncType {
    public token.Pos Func;          // position of "func" keyword (token.NoPos if there is no "func")
    public ж<FieldList> TypeParams; // type parameters; or nil
    public ж<FieldList> Params;     // (incoming) parameters; non-nil
    public ж<FieldList> Results;    // (outgoing) results; or nil
}

// An InterfaceType node represents an interface type.
partial struct InterfaceType {
    public token.Pos Interface;  // position of "interface" keyword
    public ж<FieldList> Methods; // list of embedded interfaces, methods, or types
    public bool Incomplete;      // true if (source) methods or types are missing in the Methods list
}

// A MapType node represents a map type.
partial struct MapType {
    public token.Pos Map; // position of "map" keyword
    public Expr Key;
    public Expr Value;
}

// A ChanType node represents a channel type.
partial struct ChanType {
    public token.Pos Begin; // position of "chan" keyword or "<-" (whichever comes first)
    public token.Pos Arrow; // position of "<-" (token.NoPos if there is no "<-")
    public ChanDir Dir;     // channel direction
    public Expr Value;      // value type
}

} partial class astᴛBadExpr {

// Pos and End implementations for expression/type nodes.
public static token.Pos Pos(this ref BadExpr x) { return x.From; }

} partial class astᴛIdent {

public static token.Pos Pos(this ref Ident x) { return x.NamePos; }

} partial class astᴛEllipsis {

public static token.Pos Pos(this ref Ellipsis x) { return x.ΔEllipsis; }

} partial class astᴛBasicLit {

public static token.Pos Pos(this ref BasicLit x) { return x.ValuePos; }

} partial class astᴛFuncLit {

public static token.Pos Pos(this ref FuncLit x) { return x.Type.Pos(); }

} partial class astᴛCompositeLit {

public static token.Pos Pos(this ref CompositeLit x) {
    if (x.Type != nil) {
        return x.Type.Pos();
    }
    return x.Lbrace;
}

} partial class astᴛParenExpr {

public static token.Pos Pos(this ref ParenExpr x) { return x.Lparen; }

} partial class astᴛSelectorExpr {

public static token.Pos Pos(this ref SelectorExpr x) { return x.X.Pos(); }

} partial class astᴛIndexExpr {

public static token.Pos Pos(this ref IndexExpr x) { return x.X.Pos(); }

} partial class astᴛIndexListExpr {

public static token.Pos Pos(this ref IndexListExpr x) { return x.X.Pos(); }

} partial class astᴛSliceExpr {

public static token.Pos Pos(this ref SliceExpr x) { return x.X.Pos(); }

} partial class astᴛTypeAssertExpr {

public static token.Pos Pos(this ref TypeAssertExpr x) { return x.X.Pos(); }

} partial class astᴛCallExpr {

public static token.Pos Pos(this ref CallExpr x) { return x.Fun.Pos(); }

} partial class astᴛStarExpr {

public static token.Pos Pos(this ref StarExpr x) { return x.Star; }

} partial class astᴛUnaryExpr {

public static token.Pos Pos(this ref UnaryExpr x) { return x.OpPos; }

} partial class astᴛBinaryExpr {

public static token.Pos Pos(this ref BinaryExpr x) { return x.X.Pos(); }

} partial class astᴛKeyValueExpr {

public static token.Pos Pos(this ref KeyValueExpr x) { return x.Key.Pos(); }

} partial class astᴛArrayType {

public static token.Pos Pos(this ref ArrayType x) { return x.Lbrack; }

} partial class astᴛStructType {

public static token.Pos Pos(this ref StructType x) { return x.Struct; }

} partial class astᴛFuncType {

public static token.Pos Pos(this ref FuncType x) {
    if (x.Func.IsValid() || x.Params == nil) { // see issue 3870
        return x.Func;
    }
    return x.Params.Pos(); // interface method declarations have no "func" keyword
}

} partial class astᴛInterfaceType {

public static token.Pos Pos(this ref InterfaceType x) { return x.Interface; }

} partial class astᴛMapType {

public static token.Pos Pos(this ref MapType x) { return x.Map; }

} partial class astᴛChanType {

public static token.Pos Pos(this ref ChanType x) { return x.Begin; }

} partial class astᴛBadExpr {

public static token.Pos End(this ref BadExpr x) { return x.To; }

} partial class astᴛIdent {

public static token.Pos End(this ref Ident x) { return (token.Pos)((nint)x.NamePos + len(x.Name)); }

} partial class astᴛEllipsis {

public static token.Pos End(this ref Ellipsis x) {
    if (x.Elt != nil) {
        return x.Elt.End();
    }
    return x.ΔEllipsis + 3; // len("...")
}

} partial class astᴛBasicLit {

public static token.Pos End(this ref BasicLit x) {
    if (!x.ValueEnd.IsValid()) {
        // Not from parser; use a heuristic.
        // (Incorrect for `...` containing \r\n;
        // see https://go.dev/issue/76031.)
        return (token.Pos)((nint)x.ValuePos + len(x.Value));
    }
    return x.ValueEnd;
}

} partial class astᴛFuncLit {

public static token.Pos End(this ref FuncLit x) { return x.Body.End(); }

} partial class astᴛCompositeLit {

public static token.Pos End(this ref CompositeLit x) { return x.Rbrace + 1; }

} partial class astᴛParenExpr {

public static token.Pos End(this ref ParenExpr x) { return x.Rparen + 1; }

} partial class astᴛSelectorExpr {

public static token.Pos End(this ref SelectorExpr x) { return x.Sel.End(); }

} partial class astᴛIndexExpr {

public static token.Pos End(this ref IndexExpr x) { return x.Rbrack + 1; }

} partial class astᴛIndexListExpr {

public static token.Pos End(this ref IndexListExpr x) { return x.Rbrack + 1; }

} partial class astᴛSliceExpr {

public static token.Pos End(this ref SliceExpr x) { return x.Rbrack + 1; }

} partial class astᴛTypeAssertExpr {

public static token.Pos End(this ref TypeAssertExpr x) { return x.Rparen + 1; }

} partial class astᴛCallExpr {

public static token.Pos End(this ref CallExpr x) { return x.Rparen + 1; }

} partial class astᴛStarExpr {

public static token.Pos End(this ref StarExpr x) { return x.X.End(); }

} partial class astᴛUnaryExpr {

public static token.Pos End(this ref UnaryExpr x) { return x.X.End(); }

} partial class astᴛBinaryExpr {

public static token.Pos End(this ref BinaryExpr x) { return x.Y.End(); }

} partial class astᴛKeyValueExpr {

public static token.Pos End(this ref KeyValueExpr x) { return x.Value.End(); }

} partial class astᴛArrayType {

public static token.Pos End(this ref ArrayType x) { return x.Elt.End(); }

} partial class astᴛStructType {

public static token.Pos End(this ref StructType x) { return x.Fields.End(); }

} partial class astᴛFuncType {

public static token.Pos End(this ref FuncType x) {
    if (x.Results != nil) {
        return x.Results.End();
    }
    return x.Params.End();
}

} partial class astᴛInterfaceType {

public static token.Pos End(this ref InterfaceType x) { return x.Methods.End(); }

} partial class astᴛMapType {

public static token.Pos End(this ref MapType x) { return x.Value.End(); }

} partial class astᴛChanType {

public static token.Pos End(this ref ChanType x) { return x.Value.End(); }

} partial class astᴛBadExpr {

// exprNode() ensures that only expression/type nodes can be
// assigned to an Expr.
internal static void exprNode(this ref BadExpr _) { }

} partial class astᴛIdent {

internal static void exprNode(this ref Ident _) { }

} partial class astᴛEllipsis {

internal static void exprNode(this ref Ellipsis _) { }

} partial class astᴛBasicLit {

internal static void exprNode(this ref BasicLit _) { }

} partial class astᴛFuncLit {

internal static void exprNode(this ref FuncLit _) { }

} partial class astᴛCompositeLit {

internal static void exprNode(this ref CompositeLit _) { }

} partial class astᴛParenExpr {

internal static void exprNode(this ref ParenExpr _) { }

} partial class astᴛSelectorExpr {

internal static void exprNode(this ref SelectorExpr _) { }

} partial class astᴛIndexExpr {

internal static void exprNode(this ref IndexExpr _) { }

} partial class astᴛIndexListExpr {

internal static void exprNode(this ref IndexListExpr _) { }

} partial class astᴛSliceExpr {

internal static void exprNode(this ref SliceExpr _) { }

} partial class astᴛTypeAssertExpr {

internal static void exprNode(this ref TypeAssertExpr _) { }

} partial class astᴛCallExpr {

internal static void exprNode(this ref CallExpr _) { }

} partial class astᴛStarExpr {

internal static void exprNode(this ref StarExpr _) { }

} partial class astᴛUnaryExpr {

internal static void exprNode(this ref UnaryExpr _) { }

} partial class astᴛBinaryExpr {

internal static void exprNode(this ref BinaryExpr _) { }

} partial class astᴛKeyValueExpr {

internal static void exprNode(this ref KeyValueExpr _) { }

} partial class astᴛArrayType {

internal static void exprNode(this ref ArrayType _) { }

} partial class astᴛStructType {

internal static void exprNode(this ref StructType _) { }

} partial class astᴛFuncType {

internal static void exprNode(this ref FuncType _) { }

} partial class astᴛInterfaceType {

internal static void exprNode(this ref InterfaceType _) { }

} partial class astᴛMapType {

internal static void exprNode(this ref MapType _) { }

} partial class astᴛChanType {

internal static void exprNode(this ref ChanType _) { }

} partial class ast_package {

// ----------------------------------------------------------------------------
// Convenience functions for Idents

// NewIdent creates a new [Ident] without position.
// Useful for ASTs generated by code other than the Go parser.
public static ж<Ident> NewIdent(@string name) { return Ꮡ(new Ident(token.NoPos, name, nil)); }

// IsExported reports whether name starts with an upper-case letter.
public static bool IsExported(@string name) { return token.IsExported(name); }

} partial class astᴛIdent {

// IsExported reports whether id starts with an upper-case letter.
public static bool IsExported(this ref Ident id) { return token.IsExported(id.Name); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;

public static @string String(this ж<Ident> Ꮡid) {
    ref var id = ref Ꮡid.DerefOrNull();

    if (Ꮡid != nil) {
        return id.Name;
    }
    return nilˢ;
}

} partial class ast_package {

// ----------------------------------------------------------------------------
// Statements

// A statement is represented by a tree consisting of one
// or more of the following concrete statement nodes.
// A BadStmt node is a placeholder for statements containing
// syntax errors for which no correct statement nodes can be
// created.
//
partial struct BadStmt {
    public token.Pos From, To; // position range of bad statement
}

// A DeclStmt node represents a declaration in a statement list.
partial struct DeclStmt {
    public Decl Decl; // *GenDecl with CONST, TYPE, or VAR token
}

// An EmptyStmt node represents an empty statement.
// The "position" of the empty statement is the position
// of the immediately following (explicit or implicit) semicolon.
//
partial struct EmptyStmt {
    public token.Pos Semicolon; // position of following ";"
    public bool Implicit;       // if set, ";" was omitted in the source
}

// A LabeledStmt node represents a labeled statement.
partial struct LabeledStmt {
    public ж<Ident> Label;
    public token.Pos Colon; // position of ":"
    public Stmt Stmt;
}

// An ExprStmt node represents a (stand-alone) expression
// in a statement list.
//
partial struct ExprStmt {
    public Expr X; // expression
}

// A SendStmt node represents a send statement.
partial struct SendStmt {
    public Expr Chan;
    public token.Pos Arrow; // position of "<-"
    public Expr Value;
}

// An IncDecStmt node represents an increment or decrement statement.
partial struct IncDecStmt {
    public Expr X;
    public token.Pos TokPos; // position of Tok
    public token.Token Tok;  // INC or DEC
}

// An AssignStmt node represents an assignment or
// a short variable declaration.
//
partial struct AssignStmt {
    public slice<Expr> Lhs;
    public token.Pos TokPos; // position of Tok
    public token.Token Tok;  // assignment token, DEFINE
    public slice<Expr> Rhs;
}

// A GoStmt node represents a go statement.
partial struct GoStmt {
    public token.Pos Go; // position of "go" keyword
    public ж<CallExpr> Call;
}

// A DeferStmt node represents a defer statement.
partial struct DeferStmt {
    public token.Pos Defer; // position of "defer" keyword
    public ж<CallExpr> Call;
}

// A ReturnStmt node represents a return statement.
partial struct ReturnStmt {
    public token.Pos Return;    // position of "return" keyword
    public slice<Expr> Results; // result expressions; or nil
}

// A BranchStmt node represents a break, continue, goto,
// or fallthrough statement.
//
partial struct BranchStmt {
    public token.Pos TokPos; // position of Tok
    public token.Token Tok;  // keyword token (BREAK, CONTINUE, GOTO, FALLTHROUGH)
    public ж<Ident> Label;   // label name; or nil
}

// A BlockStmt node represents a braced statement list.
partial struct BlockStmt {
    public token.Pos Lbrace; // position of "{"
    public slice<Stmt> List;
    public token.Pos Rbrace; // position of "}", if any (may be absent due to syntax error)
}

// An IfStmt node represents an if statement.
partial struct IfStmt {
    public token.Pos If; // position of "if" keyword
    public Stmt Init;    // initialization statement; or nil
    public Expr Cond;    // condition
    public ж<BlockStmt> Body;
    public Stmt Else; // else branch; or nil
}

// A CaseClause represents a case of an expression or type switch statement.
partial struct CaseClause {
    public token.Pos Case;   // position of "case" or "default" keyword
    public slice<Expr> List; // list of expressions or types; nil means default case
    public token.Pos Colon;  // position of ":"
    public slice<Stmt> Body; // statement list; or nil
}

// A SwitchStmt node represents an expression switch statement.
partial struct SwitchStmt {
    public token.Pos Switch;  // position of "switch" keyword
    public Stmt Init;         // initialization statement; or nil
    public Expr Tag;          // tag expression; or nil
    public ж<BlockStmt> Body; // CaseClauses only
}

// A TypeSwitchStmt node represents a type switch statement.
partial struct TypeSwitchStmt {
    public token.Pos Switch;  // position of "switch" keyword
    public Stmt Init;         // initialization statement; or nil
    public Stmt Assign;       // x := y.(type) or y.(type)
    public ж<BlockStmt> Body; // CaseClauses only
}

// A CommClause node represents a case of a select statement.
partial struct CommClause {
    public token.Pos Case;   // position of "case" or "default" keyword
    public Stmt Comm;        // send or receive statement; nil means default case
    public token.Pos Colon;  // position of ":"
    public slice<Stmt> Body; // statement list; or nil
}

// A SelectStmt node represents a select statement.
partial struct SelectStmt {
    public token.Pos Select;  // position of "select" keyword
    public ж<BlockStmt> Body; // CommClauses only
}

// A ForStmt represents a for statement.
partial struct ForStmt {
    public token.Pos For; // position of "for" keyword
    public Stmt Init;     // initialization statement; or nil
    public Expr Cond;     // condition; or nil
    public Stmt Post;     // post iteration statement; or nil
    public ж<BlockStmt> Body;
}

// A RangeStmt represents a for statement with a range clause.
partial struct RangeStmt {
    public token.Pos For;    // position of "for" keyword
    public Expr Key, Value;  // Key, Value may be nil
    public token.Pos TokPos; // position of Tok; invalid if Key == nil
    public token.Token Tok;  // ILLEGAL if Key == nil, ASSIGN, DEFINE
    public token.Pos Range;  // position of "range" keyword
    public Expr X;           // value to range over
    public ж<BlockStmt> Body;
}

} partial class astᴛBadStmt {

// Pos and End implementations for statement nodes.
public static token.Pos Pos(this ref BadStmt s) { return s.From; }

} partial class astᴛDeclStmt {

public static token.Pos Pos(this ref DeclStmt s) { return s.Decl.Pos(); }

} partial class astᴛEmptyStmt {

public static token.Pos Pos(this ref EmptyStmt s) { return s.Semicolon; }

} partial class astᴛLabeledStmt {

public static token.Pos Pos(this ref LabeledStmt s) { return s.Label.Pos(); }

} partial class astᴛExprStmt {

public static token.Pos Pos(this ref ExprStmt s) { return s.X.Pos(); }

} partial class astᴛSendStmt {

public static token.Pos Pos(this ref SendStmt s) { return s.Chan.Pos(); }

} partial class astᴛIncDecStmt {

public static token.Pos Pos(this ref IncDecStmt s) { return s.X.Pos(); }

} partial class astᴛAssignStmt {

public static token.Pos Pos(this ref AssignStmt s) { return s.Lhs[0].Pos(); }

} partial class astᴛGoStmt {

public static token.Pos Pos(this ref GoStmt s) { return s.Go; }

} partial class astᴛDeferStmt {

public static token.Pos Pos(this ref DeferStmt s) { return s.Defer; }

} partial class astᴛReturnStmt {

public static token.Pos Pos(this ref ReturnStmt s) { return s.Return; }

} partial class astᴛBranchStmt {

public static token.Pos Pos(this ref BranchStmt s) { return s.TokPos; }

} partial class astᴛBlockStmt {

public static token.Pos Pos(this ref BlockStmt s) { return s.Lbrace; }

} partial class astᴛIfStmt {

public static token.Pos Pos(this ref IfStmt s) { return s.If; }

} partial class astᴛCaseClause {

public static token.Pos Pos(this ref CaseClause s) { return s.Case; }

} partial class astᴛSwitchStmt {

public static token.Pos Pos(this ref SwitchStmt s) { return s.Switch; }

} partial class astᴛTypeSwitchStmt {

public static token.Pos Pos(this ref TypeSwitchStmt s) { return s.Switch; }

} partial class astᴛCommClause {

public static token.Pos Pos(this ref CommClause s) { return s.Case; }

} partial class astᴛSelectStmt {

public static token.Pos Pos(this ref SelectStmt s) { return s.Select; }

} partial class astᴛForStmt {

public static token.Pos Pos(this ref ForStmt s) { return s.For; }

} partial class astᴛRangeStmt {

public static token.Pos Pos(this ref RangeStmt s) { return s.For; }

} partial class astᴛBadStmt {

public static token.Pos End(this ref BadStmt s) { return s.To; }

} partial class astᴛDeclStmt {

public static token.Pos End(this ref DeclStmt s) { return s.Decl.End(); }

} partial class astᴛEmptyStmt {

public static token.Pos End(this ref EmptyStmt s) {
    if (s.Implicit) {
        return s.Semicolon;
    }
    return s.Semicolon + 1; /* len(";") */
}

} partial class astᴛLabeledStmt {

public static token.Pos End(this ref LabeledStmt s) { return s.Stmt.End(); }

} partial class astᴛExprStmt {

public static token.Pos End(this ref ExprStmt s) { return s.X.End(); }

} partial class astᴛSendStmt {

public static token.Pos End(this ref SendStmt s) { return s.Value.End(); }

} partial class astᴛIncDecStmt {

public static token.Pos End(this ref IncDecStmt s) {
    return s.TokPos + 2; /* len("++") */
}

} partial class astᴛAssignStmt {

public static token.Pos End(this ref AssignStmt s) { return s.Rhs[len(s.Rhs) - 1].End(); }

} partial class astᴛGoStmt {

public static token.Pos End(this ref GoStmt s) { return s.Call.End(); }

} partial class astᴛDeferStmt {

public static token.Pos End(this ref DeferStmt s) { return s.Call.End(); }

} partial class astᴛReturnStmt {

public static token.Pos End(this ref ReturnStmt s) {
    nint n = len(s.Results); if (n > 0) {
        return s.Results[n - 1].End();
    }
    return s.Return + 6; // len("return")
}

} partial class astᴛBranchStmt {

public static token.Pos End(this ref BranchStmt s) {
    if (s.Label != nil) {
        return s.Label.End();
    }
    return (token.Pos)((nint)s.TokPos + len(s.Tok.String()));
}

} partial class astᴛBlockStmt {

public static token.Pos End(this ref BlockStmt s) {
    if (s.Rbrace.IsValid()) {
        return s.Rbrace + 1;
    }
    nint n = len(s.List); if (n > 0) {
        return s.List[n - 1].End();
    }
    return s.Lbrace + 1;
}

} partial class astᴛIfStmt {

public static token.Pos End(this ref IfStmt s) {
    if (s.Else != nil) {
        return s.Else.End();
    }
    return s.Body.End();
}

} partial class astᴛCaseClause {

public static token.Pos End(this ref CaseClause s) {
    nint n = len(s.Body); if (n > 0) {
        return s.Body[n - 1].End();
    }
    return s.Colon + 1;
}

} partial class astᴛSwitchStmt {

public static token.Pos End(this ref SwitchStmt s) { return s.Body.End(); }

} partial class astᴛTypeSwitchStmt {

public static token.Pos End(this ref TypeSwitchStmt s) { return s.Body.End(); }

} partial class astᴛCommClause {

public static token.Pos End(this ref CommClause s) {
    nint n = len(s.Body); if (n > 0) {
        return s.Body[n - 1].End();
    }
    return s.Colon + 1;
}

} partial class astᴛSelectStmt {

public static token.Pos End(this ref SelectStmt s) { return s.Body.End(); }

} partial class astᴛForStmt {

public static token.Pos End(this ref ForStmt s) { return s.Body.End(); }

} partial class astᴛRangeStmt {

public static token.Pos End(this ref RangeStmt s) { return s.Body.End(); }

} partial class astᴛBadStmt {

// stmtNode() ensures that only statement nodes can be
// assigned to a Stmt.
internal static void stmtNode(this ref BadStmt _) { }

} partial class astᴛDeclStmt {

internal static void stmtNode(this ref DeclStmt _) { }

} partial class astᴛEmptyStmt {

internal static void stmtNode(this ref EmptyStmt _) { }

} partial class astᴛLabeledStmt {

internal static void stmtNode(this ref LabeledStmt _) { }

} partial class astᴛExprStmt {

internal static void stmtNode(this ref ExprStmt _) { }

} partial class astᴛSendStmt {

internal static void stmtNode(this ref SendStmt _) { }

} partial class astᴛIncDecStmt {

internal static void stmtNode(this ref IncDecStmt _) { }

} partial class astᴛAssignStmt {

internal static void stmtNode(this ref AssignStmt _) { }

} partial class astᴛGoStmt {

internal static void stmtNode(this ref GoStmt _) { }

} partial class astᴛDeferStmt {

internal static void stmtNode(this ref DeferStmt _) { }

} partial class astᴛReturnStmt {

internal static void stmtNode(this ref ReturnStmt _) { }

} partial class astᴛBranchStmt {

internal static void stmtNode(this ref BranchStmt _) { }

} partial class astᴛBlockStmt {

internal static void stmtNode(this ref BlockStmt _) { }

} partial class astᴛIfStmt {

internal static void stmtNode(this ref IfStmt _) { }

} partial class astᴛCaseClause {

internal static void stmtNode(this ref CaseClause _) { }

} partial class astᴛSwitchStmt {

internal static void stmtNode(this ref SwitchStmt _) { }

} partial class astᴛTypeSwitchStmt {

internal static void stmtNode(this ref TypeSwitchStmt _) { }

} partial class astᴛCommClause {

internal static void stmtNode(this ref CommClause _) { }

} partial class astᴛSelectStmt {

internal static void stmtNode(this ref SelectStmt _) { }

} partial class astᴛForStmt {

internal static void stmtNode(this ref ForStmt _) { }

} partial class astᴛRangeStmt {

internal static void stmtNode(this ref RangeStmt _) { }

} partial class ast_package {

// ----------------------------------------------------------------------------
// Declarations

// A Spec node represents a single (non-parenthesized) import,
// constant, type, or variable declaration.
// The Spec type stands for any of *ImportSpec, *ValueSpec, and *TypeSpec.
partial interface Spec :
    Node
{
    void specNode();
}

// An ImportSpec node represents a single package import.
partial struct ImportSpec {
    public ж<CommentGroup> Doc;     // associated documentation; or nil
    public ж<Ident> Name;           // local package name (including "."); or nil
    public ж<BasicLit> Path;        // import path
    public ж<CommentGroup> Comment; // line comments; or nil
    public token.Pos EndPos;        // end of spec (overrides Path.Pos if nonzero)
}

// A ValueSpec node represents a constant or variable declaration
// (ConstSpec or VarSpec production).
//
partial struct ValueSpec {
    public ж<CommentGroup> Doc;     // associated documentation; or nil
    public slice<ж<Ident>> Names;   // value names (len(Names) > 0)
    public Expr Type;               // value type; or nil
    public slice<Expr> Values;      // initial values; or nil
    public ж<CommentGroup> Comment; // line comments; or nil
}

// A TypeSpec node represents a type declaration (TypeSpec production).
partial struct TypeSpec {
    public ж<CommentGroup> Doc;     // associated documentation; or nil
    public ж<Ident> Name;           // type name
    public ж<FieldList> TypeParams; // type parameters; or nil
    public token.Pos Assign;        // position of '=', if any
    public Expr Type;               // *Ident, *ParenExpr, *SelectorExpr, *StarExpr, or any of the *XxxTypes
    public ж<CommentGroup> Comment; // line comments; or nil
}

} partial class astᴛImportSpec {

// Pos and End implementations for spec nodes.
public static token.Pos Pos(this ref ImportSpec s) {
    if (s.Name != nil) {
        return s.Name.Pos();
    }
    return s.Path.Pos();
}

} partial class astᴛValueSpec {

public static token.Pos Pos(this ref ValueSpec s) { return s.Names[0].Pos(); }

} partial class astᴛTypeSpec {

public static token.Pos Pos(this ref TypeSpec s) { return s.Name.Pos(); }

} partial class astᴛImportSpec {

public static token.Pos End(this ref ImportSpec s) {
    if (s.EndPos != 0) {
        return s.EndPos;
    }
    return s.Path.End();
}

} partial class astᴛValueSpec {

public static token.Pos End(this ref ValueSpec s) {
    nint n = len(s.Values); if (n > 0) {
        return s.Values[n - 1].End();
    }
    if (s.Type != nil) {
        return s.Type.End();
    }
    return s.Names[len(s.Names) - 1].End();
}

} partial class astᴛTypeSpec {

public static token.Pos End(this ref TypeSpec s) { return s.Type.End(); }

} partial class astᴛImportSpec {

// specNode() ensures that only spec nodes can be
// assigned to a Spec.
internal static void specNode(this ref ImportSpec _) { }

} partial class astᴛValueSpec {

internal static void specNode(this ref ValueSpec _) { }

} partial class astᴛTypeSpec {

internal static void specNode(this ref TypeSpec _) { }

} partial class ast_package {

// A declaration is represented by one of the following declaration nodes.
// A BadDecl node is a placeholder for a declaration containing
// syntax errors for which a correct declaration node cannot be
// created.
//
partial struct BadDecl {
    public token.Pos From, To; // position range of bad declaration
}

// A GenDecl node (generic declaration node) represents an import,
// constant, type or variable declaration. A valid Lparen position
// (Lparen.IsValid()) indicates a parenthesized declaration.
//
// Relationship between Tok value and Specs element type:
//
//	token.IMPORT  *ImportSpec
//	token.CONST   *ValueSpec
//	token.TYPE    *TypeSpec
//	token.VAR     *ValueSpec
//
partial struct GenDecl {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public token.Pos TokPos;    // position of Tok
    public token.Token Tok;     // IMPORT, CONST, TYPE, or VAR
    public token.Pos Lparen;    // position of '(', if any
    public slice<Spec> Specs;
    public token.Pos Rparen; // position of ')', if any
}

// A FuncDecl node represents a function declaration.
partial struct FuncDecl {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public ж<FieldList> Recv;   // receiver (methods); or nil (functions)
    public ж<Ident> Name;       // function/method name
    public ж<FuncType> Type;    // function signature: type and value parameters, results, and position of "func" keyword
    public ж<BlockStmt> Body;   // function body; or nil for external (non-Go) function
}

} partial class astᴛBadDecl {

// Pos and End implementations for declaration nodes.
public static token.Pos Pos(this ref BadDecl d) { return d.From; }

} partial class astᴛGenDecl {

public static token.Pos Pos(this ref GenDecl d) { return d.TokPos; }

} partial class astᴛFuncDecl {

public static token.Pos Pos(this ref FuncDecl d) { return d.Type.Pos(); }

} partial class astᴛBadDecl {

public static token.Pos End(this ref BadDecl d) { return d.To; }

} partial class astᴛGenDecl {

public static token.Pos End(this ref GenDecl d) {
    if (d.Rparen.IsValid()) {
        return d.Rparen + 1;
    }
    return d.Specs[0].End();
}

} partial class astᴛFuncDecl {

public static token.Pos End(this ref FuncDecl d) {
    if (d.Body != nil) {
        return d.Body.End();
    }
    return d.Type.End();
}

} partial class astᴛBadDecl {

// declNode() ensures that only declaration nodes can be
// assigned to a Decl.
internal static void declNode(this ref BadDecl _) { }

} partial class astᴛGenDecl {

internal static void declNode(this ref GenDecl _) { }

} partial class astᴛFuncDecl {

internal static void declNode(this ref FuncDecl _) { }

} partial class ast_package {

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
partial struct File {
    public ж<CommentGroup> Doc; // associated documentation; or nil
    public token.Pos Package;   // position of "package" keyword
    public ж<Ident> Name;       // package name
    public slice<Decl> Decls;   // top-level declarations; or nil
    public token.Pos FileStart, FileEnd;    // start and end of entire file
    public ж<Scope> Scope;                  // package scope (this file only). Deprecated: see Object
    public slice<ж<ImportSpec>> Imports;    // imports in this file
    public slice<ж<Ident>> Unresolved;      // unresolved identifiers in this file. Deprecated: see Object
    public slice<ж<CommentGroup>> Comments; // comments in the file, in lexical order
    public @string GoVersion;               // minimum Go version required by //go:build or // +build directives
}

} partial class astᴛFile {

// Pos returns the position of the package declaration.
// It may be invalid, for example in an empty file.
//
// (Use FileStart for the start of the entire file. It is always valid.)
public static token.Pos Pos(this ref File f) { return f.Package; }

// End returns the end of the last declaration in the file.
// It may be invalid, for example in an empty file.
//
// (Use FileEnd for the end of the entire file. It is always valid.)
public static token.Pos End(this ref File f) {
    nint n = len(f.Decls); if (n > 0) {
        return f.Decls[n - 1].End();
    }
    return f.Name.End();
}

} partial class ast_package {

// A Package node represents a set of source files
// collectively building a Go package.
//
// Deprecated: use the type checker [go/types] instead; see [Object].
partial struct Package {
    public @string Name;                    // package name
    public ж<Scope> Scope;                  // package scope across all files
    public map<@string, ж<Object>> Imports; // map of package id -> package object
    public map<@string, ж<File>> Files;     // Go source files by filename
}

} partial class astᴛPackage {

public static token.Pos Pos(this ref Package p) { return token.NoPos; }
public static token.Pos End(this ref Package p) { return token.NoPos; }

} partial class ast_package {

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
public static bool IsGenerated(ж<File> @file) {
    var (_, ok) = generator(ref (@file).DerefOrNull());
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doNotEditˢ => ref doNotEditˢᵇ;
private static readonly @string doNotEditˢᵇ = " DO NOT EDIT."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ => ref prefixᶜᵇ;
private static readonly @string prefixᶜᵇ = "// Code generated "u8;

internal static (@string, bool) generator(ref File @file) {
    foreach (var (_, group) in range(@file.Comments)) {
        foreach (var (_, comment) in range(group.List)) {
            if (comment.Pos() > @file.Package) {
                break; // after package declaration
            }
            // opt: check Contains first to avoid unnecessary array allocation in Split.
            @string prefix = prefixᶜ;
            if (strings.Contains(comment.Text, prefix)) {
                foreach (var line in range<@string>(strings.SplitSeq(comment.Text, "\n").Invoke)) {
                    var (rest, ok) = strings.CutPrefix(line, prefix); if (ok) {
                        var (gen, okΔ1) = strings.CutSuffix(rest, doNotEditˢ); if (okΔ1) {
                            return (gen, true);
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
    for (;;) {
        var (paren, ok) = e._<ж<ParenExpr>>(ᐧ);
        if (!ok) {
            return e;
        }
        e = paren.X;
    }
}

} // end ast_package
