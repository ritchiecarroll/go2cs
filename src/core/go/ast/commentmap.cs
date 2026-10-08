// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using cmp = cmp_package;
using fmt = fmt_package;
using token = global::go.go.token_package;
using slices = slices_package;
using strings = strings_package;
using global::go.go;
using io = io_package;

partial class ast_package {

// sortComments sorts the list of comment groups in source order.
internal static void sortComments(slice<ж<CommentGroup>> list) {
    slices.SortFunc(list, (ж<CommentGroup> a, ж<CommentGroup> b) => cmp.Compare(a.Pos(), b.Pos()));
}

partial struct CommentMap /*map[Node, slice<ж<CommentGroup>>]*/;

internal static void addComment(this CommentMap cmap, Node n, ж<CommentGroup> Ꮡc) {
    var list = cmap[n];
    if (len(list) == 0){
        list = new ж<CommentGroup>[]{Ꮡc}.slice();
    } else {
        list = append(list, Ꮡc);
    }
    cmap[n] = list;
}

// nodeList returns the list of nodes of the AST n in source order.
internal static slice<Node> nodeList(Node n) {
    ref var list = ref heap<slice<Node>>(out var Ꮡlist);
    Inspect(n, (Node nΔ1) => {
        // don't collect comments
        switch (nΔ1.type()) {
        case null:
        case ж<CommentGroup> _:
        case ж<Comment> _: {
            return false;
        }}

        Ꮡlist.ValueSlot = append(Ꮡlist.ValueSlot, nΔ1);
        return true;
    });
    // Note: The current implementation assumes that Inspect traverses the
    //       AST in depth-first and thus _source_ order. If AST traversal
    //       does not follow source order, the sorting call below will be
    //       required.
    // slices.Sort(list, func(a, b Node) int {
    //       r := cmp.Compare(a.Pos(), b.Pos())
    //       if r != 0 {
    //               return r
    //       }
    //       return cmp.Compare(a.End(), b.End())
    // })
    return list;
}

// A commentListReader helps iterating through a list of comment groups.
partial struct commentListReader {
    internal ж<token.FileSet> fset;
    internal slice<ж<CommentGroup>> list;
    internal nint index;
    internal ж<CommentGroup> comment; // comment group at current index
    internal tokenꓸPosition pos, end; // source interval of comment group at current index
}

internal static bool eol(this ref commentListReader r) {
    return r.index >= len(r.list);
}

internal static void next(this ref commentListReader r) {
    if (!r.eol()) {
        r.comment = r.list[r.index];
        r.pos = r.fset.Position(r.comment.Pos());
        r.end = r.fset.Position(r.comment.End());
        r.index++;
    }
}

partial struct nodeStack /*[]Node*/;

// push pops all nodes that appear lexically before n
// and then pushes n on the stack.
internal static void push(this ref nodeStack s, Node n) {
    s.pop(n.Pos());
    s = append((s), n);
}

// pop pops all nodes that appear lexically before pos
// (i.e., whose lexical extent has ended before or at pos).
// It returns the last node popped.
internal static Node /*top*/ pop(this ref nodeStack s, tokenꓸPos pos) {
    Node top = default!;

    nint i = len(s);
    while (i > 0 && (s)[i - 1].End() <= pos) {
        top = (s)[i - 1];
        i--;
    }
    s = (s).slice(0, i);
    return top;
}

// NewCommentMap creates a new comment map by associating comment groups
// of the comments list with the nodes of the AST specified by node.
//
// A comment group g is associated with a node n if:
//
//   - g starts on the same line as n ends
//   - g starts on the line immediately following n, and there is
//     at least one empty line after g and before the next node
//   - g starts before n and is not associated to the node before n
//     via the previous rules
//
// NewCommentMap tries to associate a comment group to the "largest"
// node possible: For instance, if the comment is a line comment
// trailing an assignment, the comment is associated with the entire
// assignment rather than just the last operand in the assignment.
public static CommentMap NewCommentMap(ж<token.FileSet> Ꮡfset, Node node, slice<ж<CommentGroup>> comments) {
    if (len(comments) == 0) {
        return default!; // no comments to map
    }
    var cmap = new CommentMap(0);
    // set up comment reader r
    var tmp = new slice<ж<CommentGroup>>(len(comments));
    copy(tmp, comments); // don't change incoming comments
    sortComments(tmp);
    var r = new commentListReader(fset: Ꮡfset, list: tmp); // !r.eol() because len(comments) > 0
    r.next();
    // create node list in lexical order
    var nodes = nodeList(node);
    nodes = append(nodes, (Node)(default!)); // append sentinel
    // set up iteration variables
    Node p = default!;                       // previous node
    
    tokenꓸPosition pend = default!;                           // end of p
    
    Node pg = default!;                     // previous node group (enclosing nodes of "importance")
    
    tokenꓸPosition pgend = default!;                         // end of pg
    
    nodeStack stack = default!;                    // stack of node groups
    foreach (var (_, q) in nodes) {
        tokenꓸPosition qpos = default!;
        if (q != default!){
            qpos = Ꮡfset.Position(q.Pos()); // current node position
        } else {
            // set fake sentinel position to infinity so that
            // all comments get processed before the sentinel
            const nint infinity = /* 1 << 30 */ 1073741824;
            qpos.Offset = infinity;
            qpos.Line = infinity;
        }
        // process comments before current node
        while (r.end.Offset <= qpos.Offset) {
            // determine recent node group
            {
                var top = stack.pop(r.comment.Pos()); if (top != default!) {
                    pg = top;
                    pgend = Ꮡfset.Position(pg.End());
                }
            }
            // Try to associate a comment first with a node group
            // (i.e., a node of "importance" such as a declaration);
            // if that fails, try to associate it with the most recent
            // node.
            // TODO(gri) try to simplify the logic below
            Node assoc = default!;
            switch (ᐧ) {
            case {} when pg != default! && (pgend.Line == r.pos.Line || pgend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line): {
                assoc = pg;
                break;
            }
            case {} when p != default! && (pend.Line == r.pos.Line || pend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line || q == default!): {
                assoc = p;
                break;
            }
            default: {
                if (q == default!) {
                    // 1) comment starts on same line as previous node group ends, or
                    // 2) comment starts on the line immediately after the
                    //    previous node group and there is an empty line before
                    //    the current node
                    // => associate comment with previous node group
                    // same rules apply as above for p rather than pg,
                    // but also associate with p if we are at the end (q == nil)
                    // otherwise, associate comment with current node
                    // we can only reach here if there was no p
                    // which would imply that there were no nodes
                    throw panic("internal error: no comments should be associated with sentinel");
                }
                assoc = q;
                break;
            }}

            cmap.addComment(assoc, r.comment);
            if (r.eol()) {
                return cmap;
            }
            r.next();
        }
        // update previous node
        p = q;
        pend = Ꮡfset.Position(p.End());
        // update previous node group if we see an "important" node
        switch (q.type()) {
        case ж<File> _:
        case ж<Field> _:
        case {} ᴛ2 when ᴛ2._<Decl>(out var _):
        case {} ᴛ3 when ᴛ3._<Spec>(out var _):
        case {} ᴛ4 when ᴛ4._<Stmt>(out var _): {
            stack.push(q);
            break;
        }}

    }
    return cmap;
}

// Update replaces an old node in the comment map with the new node
// and returns the new node. Comments that were associated with the
// old node are associated with the new node.
public static Node Update(this CommentMap cmap, Node old, Node @new) {
    {
        var list = cmap[old]; if (len(list) > 0) {
            delete(cmap, old);
            cmap[@new] = appendꓸꓸꓸ(cmap[@new], list);
        }
    }
    return @new;
}

// Filter returns a new comment map consisting of only those
// entries of cmap for which a corresponding node exists in
// the AST specified by node.
public static CommentMap Filter(this CommentMap cmap, Node node) {
    var umap = new CommentMap(0);
    var cmapʗ1 = cmap;
    var umapʗ1 = umap;
    Inspect(node, (Node n) => {
        {
            var g = cmapʗ1[n]; if (len(g) > 0) {
                umapʗ1[n] = g;
            }
        }
        return true;
    });
    return umap;
}

// Comments returns the list of comment groups in the comment map.
// The result is sorted in source order.
public static slice<ж<CommentGroup>> Comments(this CommentMap cmap) {
    var list = new slice<ж<CommentGroup>>(0, len(cmap));
    foreach (var (_, e) in cmap) {
        list = appendꓸꓸꓸ(list, e);
    }
    sortComments(list);
    return list;
}

internal static @string summary(slice<ж<CommentGroup>> list) {
    UntypedInt maxLen = 40;
    bytes.Buffer buf = default!;
    // collect comments text
loop:
    foreach (var (_, group) in list) {
        // Note: CommentGroup.Text() does too much work for what we
        //       need and would only replace this innermost loop.
        //       Just do it explicitly.
        foreach (var (_, comment) in (~group).List) {
            if (buf.Len() >= maxLen) {
                goto break_loop;
            }
            buf.WriteString((~comment).Text);
        }
continue_loop:;
    }
break_loop:;
    // truncate if too long
    if (buf.Len() > maxLen) {
        buf.Truncate(maxLen - 3);
        buf.WriteString("..."u8);
    }
    // replace any invisibles with blanks
    var bytesΔ1 = buf.Bytes();
    foreach (var (i, b) in bytesΔ1) {
        switch (b) {
        case (rune)'\t' or (rune)'\n' or (rune)'\r': {
            bytesΔ1[i] = (rune)' ';
            break;
        }}

    }
    return ((@string)bytesΔ1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object commentMapˢ = (@string)"CommentMap {"u8;

public static @string String(this CommentMap cmap) {
    // print map entries in sorted order
    slice<Node> nodes = default!;
    foreach (var (node, _) in cmap) {
        nodes = append(nodes, node);
    }
    slices.SortFunc(nodes, (Node a, Node b) => {
        nint r = cmp.Compare(a.Pos(), b.Pos());
        if (r != 0) {
            return r;
        }
        return cmp.Compare(a.End(), b.End());
    });
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡbuf), commentMapˢ);
    foreach (var (_, node) in nodes) {
        var comment = cmap[node];
        // print name of identifiers; print node type for other nodes
        @string s = default!;
        {
            var (ident, ok) = node._<ж<Ident>>(ᐧ); if (ok){
                s = ident.Value.Name;
            } else {
                s = fmt.Sprintf("%T"u8, node);
            }
        }
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "\t%p  %20s:  %s\n"u8, node, s, summary(comment));
    }
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡbuf), (@string)"}"u8);
    return buf.String();
}

} // end ast_package
