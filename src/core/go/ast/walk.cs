// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using iter = iter_package;

partial class ast_package {

// A Visitor's Visit method is invoked for each node encountered by [Walk].
// If the result visitor w is not nil, [Walk] visits each of the children
// of node with the visitor w, followed by a call of w.Visit(nil).
partial interface Visitor {
    Visitor /*w*/ Visit(Node node);
}

internal static void walkList<N>(Visitor v, slice<N> list)
    where N : Node
{
    foreach (var (_, node) in list) {
        Walk(v, node);
    }
}

// TODO(gri): Investigate if providing a closure to Walk leads to
// simpler use (and may help eliminate Inspect in turn).

// Walk traverses an AST in depth-first order: It starts by calling
// v.Visit(node); node must not be nil. If the visitor w returned by
// v.Visit(node) is not nil, Walk is invoked recursively with visitor
// w for each of the non-nil children of node, followed by a call of
// w.Visit(nil).
public static void Walk(Visitor v, Node node) {
    {
        v = v.Visit(node); if (v == default!) {
            return;
        }
    }
    // walk children
    // (the order of the cases matches the order
    // of the corresponding node types in ast.go)
    switch (node.type()) {
    case ж<Comment> n: {
        break;
    }
    case ж<CommentGroup> n: {
        walkList(v, // Comments and fields
 // nothing to do
 widen<ж<Comment>, Node>((~n).List, elemᴛ1 => new CommentжNode(elemᴛ1)));
        break;
    }
    case ж<Field> n: {
        if ((~n).Doc != nil) {
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        walkList(v, widen<ж<Ident>, Node>((~n).Names, elemᴛ1 => new IdentжNode(elemᴛ1)));
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        if ((~n).Tag != nil) {
            Walk(v, new BasicLitжNode((~n).Tag));
        }
        if ((~n).Comment != nil) {
            Walk(v, new CommentGroupжNode((~n).Comment));
        }
        break;
    }
    case ж<FieldList> n: {
        walkList(v, widen<ж<Field>, Node>((~n).List, elemᴛ1 => new FieldжNode(elemᴛ1)));
        break;
    }
    case ж<BadExpr> _:
    case ж<Ident> _:
    case ж<BasicLit> _: {
        var n = node;
        break;
    }
    case ж<Ellipsis> n: {
        if ((~n).Elt != default!) {
            // Expressions
            // nothing to do
            Walk(v, (~n).Elt);
        }
        break;
    }
    case ж<FuncLit> n: {
        Walk(v, new FuncTypeжNode((~n).Type));
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<CompositeLit> n: {
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        walkList(v, (~n).Elts);
        break;
    }
    case ж<ParenExpr> n: {
        Walk(v, (~n).X);
        break;
    }
    case ж<SelectorExpr> n: {
        Walk(v, (~n).X);
        Walk(v, new IdentжNode((~n).Sel));
        break;
    }
    case ж<IndexExpr> n: {
        Walk(v, (~n).X);
        Walk(v, (~n).Index);
        break;
    }
    case ж<IndexListExpr> n: {
        Walk(v, (~n).X);
        walkList(v, (~n).Indices);
        break;
    }
    case ж<SliceExpr> n: {
        Walk(v, (~n).X);
        if ((~n).Low != default!) {
            Walk(v, (~n).Low);
        }
        if ((~n).High != default!) {
            Walk(v, (~n).High);
        }
        if ((~n).Max != default!) {
            Walk(v, (~n).Max);
        }
        break;
    }
    case ж<TypeAssertExpr> n: {
        Walk(v, (~n).X);
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        break;
    }
    case ж<CallExpr> n: {
        Walk(v, (~n).Fun);
        walkList(v, (~n).Args);
        break;
    }
    case ж<StarExpr> n: {
        Walk(v, (~n).X);
        break;
    }
    case ж<UnaryExpr> n: {
        Walk(v, (~n).X);
        break;
    }
    case ж<BinaryExpr> n: {
        Walk(v, (~n).X);
        Walk(v, (~n).Y);
        break;
    }
    case ж<KeyValueExpr> n: {
        Walk(v, (~n).Key);
        Walk(v, (~n).Value);
        break;
    }
    case ж<ArrayType> n: {
        if ((~n).Len != default!) {
            // Types
            Walk(v, (~n).Len);
        }
        Walk(v, (~n).Elt);
        break;
    }
    case ж<StructType> n: {
        Walk(v, new FieldListжNode((~n).Fields));
        break;
    }
    case ж<FuncType> n: {
        if ((~n).TypeParams != nil) {
            Walk(v, new FieldListжNode((~n).TypeParams));
        }
        if ((~n).Params != nil) {
            Walk(v, new FieldListжNode((~n).Params));
        }
        if ((~n).Results != nil) {
            Walk(v, new FieldListжNode((~n).Results));
        }
        break;
    }
    case ж<InterfaceType> n: {
        Walk(v, new FieldListжNode((~n).Methods));
        break;
    }
    case ж<MapType> n: {
        Walk(v, (~n).Key);
        Walk(v, (~n).Value);
        break;
    }
    case ж<ChanType> n: {
        Walk(v, (~n).Value);
        break;
    }
    case ж<BadStmt> n: {
        break;
    }
    case ж<DeclStmt> n: {
        Walk(v, // Statements
 // nothing to do
 (~n).Decl);
        break;
    }
    case ж<EmptyStmt> n: {
        break;
    }
    case ж<LabeledStmt> n: {
        Walk(v, // nothing to do
 new IdentжNode((~n).Label));
        Walk(v, (~n).Stmt);
        break;
    }
    case ж<ExprStmt> n: {
        Walk(v, (~n).X);
        break;
    }
    case ж<SendStmt> n: {
        Walk(v, (~n).Chan);
        Walk(v, (~n).Value);
        break;
    }
    case ж<IncDecStmt> n: {
        Walk(v, (~n).X);
        break;
    }
    case ж<AssignStmt> n: {
        walkList(v, (~n).Lhs);
        walkList(v, (~n).Rhs);
        break;
    }
    case ж<GoStmt> n: {
        Walk(v, new CallExprжNode((~n).Call));
        break;
    }
    case ж<DeferStmt> n: {
        Walk(v, new CallExprжNode((~n).Call));
        break;
    }
    case ж<ReturnStmt> n: {
        walkList(v, (~n).Results);
        break;
    }
    case ж<BranchStmt> n: {
        if ((~n).Label != nil) {
            Walk(v, new IdentжNode((~n).Label));
        }
        break;
    }
    case ж<BlockStmt> n: {
        walkList(v, (~n).List);
        break;
    }
    case ж<IfStmt> n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        Walk(v, (~n).Cond);
        Walk(v, new BlockStmtжNode((~n).Body));
        if ((~n).Else != default!) {
            Walk(v, (~n).Else);
        }
        break;
    }
    case ж<CaseClause> n: {
        walkList(v, (~n).List);
        walkList(v, (~n).Body);
        break;
    }
    case ж<SwitchStmt> n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        if ((~n).Tag != default!) {
            Walk(v, (~n).Tag);
        }
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<TypeSwitchStmt> n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        Walk(v, (~n).Assign);
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<CommClause> n: {
        if ((~n).Comm != default!) {
            Walk(v, (~n).Comm);
        }
        walkList(v, (~n).Body);
        break;
    }
    case ж<SelectStmt> n: {
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<ForStmt> n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        if ((~n).Cond != default!) {
            Walk(v, (~n).Cond);
        }
        if ((~n).Post != default!) {
            Walk(v, (~n).Post);
        }
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<RangeStmt> n: {
        if ((~n).Key != default!) {
            Walk(v, (~n).Key);
        }
        if ((~n).Value != default!) {
            Walk(v, (~n).Value);
        }
        Walk(v, (~n).X);
        Walk(v, new BlockStmtжNode((~n).Body));
        break;
    }
    case ж<ImportSpec> n: {
        if ((~n).Doc != nil) {
            // Declarations
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        if ((~n).Name != nil) {
            Walk(v, new IdentжNode((~n).Name));
        }
        Walk(v, new BasicLitжNode((~n).Path));
        if ((~n).Comment != nil) {
            Walk(v, new CommentGroupжNode((~n).Comment));
        }
        break;
    }
    case ж<ValueSpec> n: {
        if ((~n).Doc != nil) {
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        walkList(v, widen<ж<Ident>, Node>((~n).Names, elemᴛ1 => new IdentжNode(elemᴛ1)));
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        walkList(v, (~n).Values);
        if ((~n).Comment != nil) {
            Walk(v, new CommentGroupжNode((~n).Comment));
        }
        break;
    }
    case ж<TypeSpec> n: {
        if ((~n).Doc != nil) {
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        Walk(v, new IdentжNode((~n).Name));
        if ((~n).TypeParams != nil) {
            Walk(v, new FieldListжNode((~n).TypeParams));
        }
        Walk(v, (~n).Type);
        if ((~n).Comment != nil) {
            Walk(v, new CommentGroupжNode((~n).Comment));
        }
        break;
    }
    case ж<BadDecl> n: {
        break;
    }
    case ж<GenDecl> n: {
        if ((~n).Doc != nil) {
            // nothing to do
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        walkList(v, (~n).Specs);
        break;
    }
    case ж<FuncDecl> n: {
        if ((~n).Doc != nil) {
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        if ((~n).Recv != nil) {
            Walk(v, new FieldListжNode((~n).Recv));
        }
        Walk(v, new IdentжNode((~n).Name));
        Walk(v, new FuncTypeжNode((~n).Type));
        if ((~n).Body != nil) {
            Walk(v, new BlockStmtжNode((~n).Body));
        }
        break;
    }
    case ж<File> n: {
        if ((~n).Doc != nil) {
            // Files and packages
            Walk(v, new CommentGroupжNode((~n).Doc));
        }
        Walk(v, new IdentжNode((~n).Name));
        walkList(v, (~n).Decls);
        break;
    }
    case ж<Package> n: {
        foreach (var (_, f) in (~n).Files) {
            // don't walk n.Comments - they have been
            // visited already through the individual
            // nodes
            Walk(v, new FileжNode(f));
        }
        break;
    }
    default: {
        var n = node;
        throw panic(fmt.Sprintf("ast.Walk: unexpected node type %T"u8, n));
        break;
    }}
    v.Visit(default!);
}

internal delegate bool inspector(Node _);

internal static Visitor Visit(this inspector f, Node node) {
    if (f(node)) {
        return new inspectorᴠVisitor(f);
    }
    return default!;
}

// Inspect traverses an AST in depth-first order: It starts by calling
// f(node); node must not be nil. If f returns true, Inspect invokes f
// recursively for each of the non-nil children of node, followed by a
// call of f(nil).
public static void Inspect(Node node, Func<Node, bool> f) {
    Walk(new inspectorᴠVisitor(NilSafeDelegateConversion<inspector, Func<Node, bool>>(f)), node);
}

// Preorder returns an iterator over all the nodes of the syntax tree
// beneath (and including) the specified root, in depth-first
// preorder.
//
// For greater control over the traversal of each subtree, use [Inspect].
public static iter.Seq<Node> Preorder(Node root) {
    return (Func<Node, bool> yield) => {
        var ok = true;
        Inspect(root, (Node n) => {
            if (n != default!) {
                // yield must not be called once ok is false.
                ok = ok && yield(n);
            }
            return ok;
        });
    };
}

} // end ast_package
