// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

// tree is a self-balancing AVL tree; see
// Lewis & Denenberg, Data Structures and Their Algorithms.
//
// An AVL tree is a binary tree in which the difference between the
// heights of a node's two subtrees--the node's "balance factor"--is
// at most one. It is more strictly balanced than a red/black tree,
// and thus favors lookups at the expense of updates, which is the
// appropriate trade-off for FileSet.
//
// Insertion at a node may cause its ancestors' balance factors to
// temporarily reach ±2, requiring rebalancing of each such ancestor
// by a rotation.
//
// Each key is the pos-end range of a single File.
// All Files in the tree must have disjoint ranges.
//
// The implementation is simplified from Russ Cox's github.com/rsc/omap.
using fmt = fmt_package;
using iter = iter_package;

partial class token_package {

// A tree is a tree-based ordered map:
// each value is a *File, keyed by its Pos range.
// All map entries cover disjoint ranges.
//
// The zero value of tree is an empty map ready to use.
partial struct tree {
    internal ж<node> root;
}

partial struct node {
    // We use the notation (parent left right) in many comments.
    internal ж<node> parent;
    internal ж<node> left;
    internal ж<node> right;
    internal ж<ΔFile> @file;
    internal Δkey key;      // = file.key(), but improves locality (25% faster)
    internal int32 balance; // at most ±2
    internal int32 height;
}

// A key represents the Pos range of a File.
partial struct Δkey {
    internal nint start, end;
}

internal static Δkey key(this ref ΔFile f) {
    return new Δkey(f.@base, f.@base + f.size);
}

// compareKey reports whether x is before y (-1),
// after y (+1), or overlapping y (0).
// This is a total order so long as all
// files in the tree have disjoint ranges.
//
// All files are separated by at least one unit.
// This allows us to use strict < comparisons.
// Use key{p, p} to search for a zero-width position
// even at the start or end of a file.
internal static nint compareKey(Δkey x, Δkey y) {
    switch (ᐧ) {
    case {} when x.end < y.start: {
        return -1;
    }
    case {} when y.end < x.start: {
        return +1;
    }}
    return 0;
}

// check asserts that each node's height, subtree, and parent link is
// correct.
internal static void check(this ж<node> Ꮡn, ж<node> Ꮡparent) {
    ref var n = ref Ꮡn.DerefOrNull();
    ref var parent = ref Ꮡparent.DerefOrNull();

    const bool debugging = false;
    if (debugging) {
        if (Ꮡn == nil) {
            return;
        }
        if (n.parent != Ꮡparent) {
            throw panic("bad parent");
        }
        n.left.check(Ꮡn);
        n.right.check(Ꮡn);
        n.checkBalance();
    }
}

internal static void checkBalance(this ref node n) {
    var (lheight, rheight) = (n.left.safeHeight(), n.right.safeHeight());
    var balance = rheight - lheight;
    if (balance != n.balance) {
        throw panic("bad node.balance");
    }
    if (!(-2 <= balance && balance <= +2)) {
        throw panic(fmt.Sprintf("node.balance out of range: %d"u8, balance));
    }
    var h = 1 + max(lheight, rheight);
    if (h != n.height) {
        throw panic("bad node.height");
    }
}

// locate returns a pointer to the variable that holds the node
// identified by k, along with its parent, if any. If the key is not
// present, it returns a pointer to the node where the key should be
// inserted by a subsequent call to [tree.set].
internal static (ж<ж<node>> pos, ж<node> parent) locate(this ж<tree> Ꮡt, Δkey k) {
    ж<ж<node>> pos = default;
    ж<node> parent = default;
    ref var t = ref Ꮡt.DerefOrNull();

    pos = Ꮡt.of(tree.Ꮡroot);
    var x = t.root;
    while (x != nil) {
        nint sign = compareKey(k, x.key);
        if (sign < 0) {
            (pos, x, parent) = (x.of(node.Ꮡleft), x.left, x);
        } else if (sign > 0) {
            (pos, x, parent) = (x.of(node.Ꮡright), x.right, x);
        } else {
            break;
        }
    }
    return (pos, parent);
}

// all returns an iterator over the tree t.
// If t is modified during the iteration,
// some files may not be visited.
// No file will be visited multiple times.
internal static iter.Seq<ж<ΔFile>> all(this ж<tree> Ꮡt) {
    return (Func<ж<ΔFile>, bool> yield) => {
        if (Ꮡt == nil) {
            return;
        }
        var x = Ꮡt.Value.root;
        if (x != nil) {
            while (x.left != nil) {
                x = x.left;
            }
        }
        while (x != nil && yield(x.@file)) {
            if (x.height >= 0) {
                // still in tree
                x = x.next();
            } else {
                // deleted
                var (ᴛ1, ᴛ2) = Ꮡt.locate(x.key);
                x = Ꮡt.Value.nextAfter(ᴛ1, ᴛ2);
            }
        }
    };
}

// nextAfter returns the node in the key sequence following
// (pos, parent), a result pair from [tree.locate].
internal static ж<node> nextAfter(this ref tree t, ж<ж<node>> Ꮡpos, ж<node> parent) {
    ref var pos = ref Ꮡpos.DerefOrNull();

    switch (ᐧ) {
    case {} when pos != nil: {
        return (pos).next();
    }
    case {} when parent == nil: {
        return default;
    }
    case {} when Ꮡpos == parent.of(node.Ꮡleft): {
        return parent;
    }
    default: {
        return parent.next();
    }}
}

internal static ж<node> next(this ж<node> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.right == nil) {
        while (x.parent != nil && x.parent.right == Ꮡx) {
            Ꮡx = x.parent; x = ref Ꮡx.DerefOrNull();
        }
        return x.parent;
    }
    Ꮡx = x.right; x = ref Ꮡx.DerefOrNull();
    while (x.left != nil) {
        Ꮡx = x.left; x = ref Ꮡx.DerefOrNull();
    }
    return Ꮡx;
}

internal static void setRoot(this ref tree t, ж<node> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    t.root = Ꮡx;
    if (Ꮡx != nil) {
        x.parent = default;
    }
}

internal static void setLeft(this ж<node> Ꮡx, ж<node> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    x.left = Ꮡy;
    if (Ꮡy != nil) {
        y.parent = Ꮡx;
    }
}

internal static void setRight(this ж<node> Ꮡx, ж<node> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    x.right = Ꮡy;
    if (Ꮡy != nil) {
        y.parent = Ꮡx;
    }
}

internal static int32 safeHeight(this ж<node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return -1;
    }
    return n.height;
}

internal static void update(this ref node n) {
    var (lheight, rheight) = (n.left.safeHeight(), n.right.safeHeight());
    n.height = max(lheight, rheight) + 1;
    n.balance = rheight - lheight;
}

internal static void replaceChild(this ref tree t, ж<node> Ꮡparent, ж<node> old, ж<node> @new) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    switch (ᐧ) {
    case {} when Ꮡparent == nil: {
        if (t.root != old) {
            throw panic("corrupt tree");
        }
        t.setRoot(@new);
        break;
    }
    case {} when parent.left == old: {
        Ꮡparent.setLeft(@new);
        break;
    }
    case {} when parent.right == old: {
        Ꮡparent.setRight(@new);
        break;
    }
    default: {
        throw panic("corrupt tree");
        break;
    }}
}

// rebalanceUp visits each excessively unbalanced ancestor
// of x, restoring balance by rotating it.
//
// x is a node that has just been mutated, and so the height and
// balance of x and its ancestors may be stale, but the children of x
// must be in a valid state.
internal static void rebalanceUp(this ref tree t, ж<node> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    while (Ꮡx != nil) {
        var h = x.height;
        x.update();
        switch (x.balance) {
        case var ᴛ when ᴛ == -2: {
            if (x.left.balance == 1) {
                t.rotateLeft(x.left);
            }
            Ꮡx = t.rotateRight(Ꮡx); x = ref Ꮡx.DerefOrNull();
            break;
        }
        case var ᴛ when ᴛ == +2: {
            if (x.right.balance == -1) {
                t.rotateRight(x.right);
            }
            Ꮡx = t.rotateLeft(Ꮡx); x = ref Ꮡx.DerefOrNull();
            break;
        }}
        if (x.height == h) {
            // x's height has not changed, so the height
            // and balance of its ancestors have not changed;
            // no further rebalancing is required.
            return;
        }
        Ꮡx = x.parent; x = ref Ꮡx.DerefOrNull();
    }
}

// rotateRight rotates the subtree rooted at node y.
// turning (y (x a b) c) into (x a (y b c)).
internal static ж<node> rotateRight(this ref tree t, ж<node> Ꮡy) {
    ref var y = ref Ꮡy.DerefOrNull();

    // p -> (y (x a b) c)
    var p = y.parent;
    var x = y.left;
    var b = x.right;

    x.checkBalance();
    y.checkBalance();

    x.setRight(Ꮡy);
    Ꮡy.setLeft(b);
    t.replaceChild(p, Ꮡy, x);

    y.update();
    x.update();
    return x;
}

// rotateLeft rotates the subtree rooted at node x.
// turning (x a (y b c)) into (y (x a b) c).
internal static ж<node> rotateLeft(this ref tree t, ж<node> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    // p -> (x a (y b c))
    var p = x.parent;
    var y = x.right;
    var b = y.left;

    x.checkBalance();
    y.checkBalance();

    y.setLeft(Ꮡx);
    Ꮡx.setRight(b);
    t.replaceChild(p, Ꮡx, y);

    x.update();
    y.update();
    return y;
}

// add inserts file into the tree, if not present.
// It panics if file overlaps with another.
internal static void add(this ж<tree> Ꮡt, ж<ΔFile> Ꮡfile) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var @file = ref Ꮡfile.DerefOrNull();

    var (pos, parent) = Ꮡt.locate(@file.key());
    if (pos.ValueSlot == nil) {
        t.set(Ꮡfile, pos, parent); // missing; insert
        return;
    }
    {
        var prev = (pos.ValueSlot).@file; if (prev != Ꮡfile) {
            throw panic(fmt.Sprintf("file %s (%d-%d) overlaps with file %s (%d-%d)"u8,
                prev.Name(), prev.Base(), prev.End(),
                @file.Name(), @file.Base(), @file.End()));
        }
    }
}

// set updates the existing node at (pos, parent) if present, or
// inserts a new node if not, so that it refers to file.
internal static void set(this ref tree t, ж<ΔFile> Ꮡfile, ж<ж<node>> Ꮡpos, ж<node> parent) {
    ref var @file = ref Ꮡfile.DerefOrNull();
    ref var pos = ref Ꮡpos.DerefOrNull();

    {
        var xΔ1 = pos; if (xΔ1 != nil) {
            // This code path isn't currently needed
            // because FileSet never updates an existing entry.
            // Remove this assertion if things change.
            if (true) { // defeat vet's unreachable pass
                throw panic("unreachable according to current FileSet requirements");
            }
            xΔ1.@file = Ꮡfile;
            return;
        }
    }
    var x = Ꮡ(new node(@file: Ꮡfile, key: @file.key(), parent: parent, height: -1));
    pos = x;
    t.rebalanceUp(x);
}

// delete deletes the node at pos.
internal static void delete(this ref tree t, ж<ж<node>> Ꮡpos) {
    ref var pos = ref Ꮡpos.DerefOrNull();

    t.root.check(nil);

    var x = pos;
    switch (ᐧ) {
    case {} when x == nil: {
        // This code path isn't currently needed because FileSet
        // only calls delete after a positive locate.
        // Remove this assertion if things change.
        if (true) { // defeat vet's unreachable pass
            throw panic("unreachable according to current FileSet requirements");
        }
        return;
    }
    case {} when x.left == nil: {
        {
            pos = x.right; if (pos != nil) {
                (pos).parent = x.parent;
            }
        }
        t.rebalanceUp(x.parent);
        break;
    }
    case {} when x.right == nil: {
        pos = x.left;
        x.left.parent = x.parent;
        t.rebalanceUp(x.parent);
        break;
    }
    default: {
        t.deleteSwap(Ꮡpos);
        break;
    }}

    x.balance = -100;
    x.parent = default;
    x.left = default;
    x.right = default;
    x.height = -1;
    t.root.check(nil);
}

// deleteSwap deletes a node that has two children by replacing
// it by its in-order successor, then triggers a rebalance.
internal static void deleteSwap(this ref tree t, ж<ж<node>> Ꮡpos) {
    ref var pos = ref Ꮡpos.DerefOrNull();

    var x = pos;
    var z = t.deleteMin(x.of(node.Ꮡright));

    pos = z;
    var unbalanced = z.parent; // lowest potentially unbalanced node
    if (unbalanced == x) {
        unbalanced = z; // (x a (z nil b)) -> (z a b)
    }
    z.parent = x.parent;
    z.height = x.height;
    z.balance = x.balance;
    z.setLeft(x.left);
    z.setRight(x.right);

    t.rebalanceUp(unbalanced);
}

// deleteMin updates the subtree rooted at *zpos to delete its minimum
// (leftmost) element, which may be *zpos itself. It returns the
// deleted node.
internal static ж<node> /*z*/ deleteMin(this ref tree t, ж<ж<node>> Ꮡzpos) {
    ж<node> z = default;
    ref var zpos = ref Ꮡzpos.DerefOrNull();

    while ((zpos).left != nil) {
        Ꮡzpos = (zpos).of(node.Ꮡleft); zpos = ref Ꮡzpos.DerefOrNull();
    }
    z = zpos;
    zpos = z.right;
    if (zpos != nil) {
        (zpos).parent = z.parent;
    }
    return z;
}

} // end token_package
