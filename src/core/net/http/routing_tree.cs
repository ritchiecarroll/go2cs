// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements a decision tree for fast matching of requests to
// patterns.
//
// The root of the tree branches on the host of the request.
// The next level branches on the method.
// The remaining levels branch on consecutive segments of the path.
//
// The "more specific wins" precedence rule can result in backtracking.
// For example, given the patterns
//     /a/b/z
//     /a/{x}/c
// we will first try to match the path "/a/b/c" with /a/b/z, and
// when that fails we will try against /a/{x}/c.
namespace go.net;

using strings = strings_package;

partial class http_package {

// A routingNode is a node in the decision tree.
// The same struct is used for leaf and interior nodes.
partial struct routingNode {
    // A leaf node holds a single pattern and the Handler it was registered
    // with.
    internal ж<pattern> pattern;
    internal ΔHandler handler;
    // An interior node maps parts of the incoming request to child nodes.
    // special children keys:
    //     "/"	trailing slash (resulting from {$})
    //	   ""   single wildcard
    internal mapping<@string, ж<routingNode>> children;
    internal ж<routingNode> multiChild; // child with multi wildcard
    internal ж<routingNode> emptyChild; // optimization: child with key ""
}

// addPattern adds a pattern and its associated Handler to the tree
// at root.
internal static void addPattern(this ж<routingNode> Ꮡroot, ж<pattern> Ꮡp, ΔHandler h) {
    ref var p = ref Ꮡp.DerefOrNull();

    // First level of tree is host.
    var n = Ꮡroot.addChild(p.host);
    // Second level of tree is method.
    n = n.addChild(p.method);
    // Remaining levels are path.
    n.addSegments(p.segments, Ꮡp, h);
}

// addSegments adds the given segments to the tree rooted at n.
// If there are no segments, then n is a leaf node that holds
// the given pattern and handler.
internal static void addSegments(this ж<routingNode> Ꮡn, slice<segment> segs, ж<pattern> Ꮡp, ΔHandler h) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (builtin.len(segs) == 0) {
        n.set(Ꮡp, h);
        return;
    }
    var seg = segs[0];
    if (seg.multi){
        if (builtin.len(segs) != 1) {
            throw panic("multi wildcard not last");
        }
        var c = Ꮡ(new routingNode(nil));
        n.multiChild = c;
        c.set(Ꮡp, h);
    } else 
    if (seg.wild){
        Ꮡn.addChild(""u8).addSegments(segs[1..], Ꮡp, h);
    } else {
        Ꮡn.addChild(seg.s).addSegments(segs[1..], Ꮡp, h);
    }
}

// set sets the pattern and handler for n, which
// must be a leaf node.
internal static void set(this ref routingNode n, ж<pattern> Ꮡp, ΔHandler h) {
    if (n.pattern != nil || n.handler != default!) {
        throw panic("non-nil leaf fields");
    }
    n.pattern = Ꮡp;
    n.handler = h;
}

// addChild adds a child node with the given key to n
// if one does not exist, and returns the child.
internal static ж<routingNode> addChild(this ж<routingNode> Ꮡn, @string key) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (key == ""u8) {
        if (n.emptyChild == nil) {
            n.emptyChild = Ꮡ(new routingNode(nil));
        }
        return n.emptyChild;
    }
    {
        var cΔ1 = Ꮡn.findChild(key); if (cΔ1 != nil) {
            return cΔ1;
        }
    }
    var c = Ꮡ(new routingNode(nil));
    n.children.add(key, c);
    return c;
}

// findChild returns the child of n with the given key, or nil
// if there is no child with that key.
internal static ж<routingNode> findChild(this ж<routingNode> Ꮡn, @string key) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (key == ""u8) {
        return n.emptyChild;
    }
    var (r, _) = Ꮡn.of(routingNode.Ꮡchildren).find(key);
    return r;
}

// match returns the leaf node under root that matches the arguments, and a list
// of values for pattern wildcards in the order that the wildcards appear.
// For example, if the request path is "/a/b/c" and the pattern is "/{x}/b/{y}",
// then the second return value will be []string{"a", "c"}.
internal static (ж<routingNode>, slice<@string>) match(this ж<routingNode> Ꮡroot, @string host, @string method, @string path) {
    ref var root = ref Ꮡroot.DerefOrNull();

    if (host != ""u8) {
        // There is a host. If there is a pattern that specifies that host and it
        // matches, we are done. If the pattern doesn't match, fall through to
        // try patterns with no host.
        {
            var (l, m) = Ꮡroot.findChild(host).matchMethodAndPath(method, path); if (l != nil) {
                return (l, m);
            }
        }
    }
    return root.emptyChild.matchMethodAndPath(method, path);
}

// matchMethodAndPath matches the method and path.
// Its return values are the same as [routingNode.match].
// The receiver should be a child of the root.
internal static (ж<routingNode>, slice<@string>) matchMethodAndPath(this ж<routingNode> Ꮡn, @string method, @string path) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return (default!, default!);
    }
    {
        var (l, m) = Ꮡn.findChild(method).matchPath(path, default!); if (l != nil) {
            // Exact match of method name.
            return (l, m);
        }
    }
    if (method == "HEAD"u8) {
        // GET matches HEAD too.
        {
            var (l, m) = Ꮡn.findChild(getˢ).matchPath(path, default!); if (l != nil) {
                return (l, m);
            }
        }
    }
    // No exact match; try patterns with no method.
    return n.emptyChild.matchPath(path, default!);
}

// matchPath matches a path.
// Its return values are the same as [routingNode.match].
// matchPath calls itself recursively. The matches argument holds the wildcard matches
// found so far.
internal static (ж<routingNode>, slice<@string>) matchPath(this ж<routingNode> Ꮡn, @string path, slice<@string> matches) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return (default!, default!);
    }
    // If path is empty, then we are done.
    // If n is a leaf node, we found a match; return it.
    // If n is an interior node (which means it has a nil pattern),
    // then we failed to match.
    if (path == ""u8) {
        if (n.pattern == nil) {
            return (default!, default!);
        }
        return (Ꮡn, matches);
    }
    // Get the first segment of path.
    var (seg, rest) = firstSegment(path);
    // First try matching against patterns that have a literal for this position.
    // We know by construction that such patterns are more specific than those
    // with a wildcard at this position (they are either more specific, equivalent,
    // or overlap, and we ruled out the first two when the patterns were registered).
    {
        var (nΔ1, m) = Ꮡn.findChild(seg).matchPath(rest, matches); if (nΔ1 != nil) {
            return (nΔ1, m);
        }
    }
    // If matching a literal fails, try again with patterns that have a single
    // wildcard (represented by an empty string in the child mapping).
    // Again, by construction, patterns with a single wildcard must be more specific than
    // those with a multi wildcard.
    // We skip this step if the segment is a trailing slash, because single wildcards
    // don't match trailing slashes.
    if (seg != "/"u8) {
        {
            var (nΔ2, m) = n.emptyChild.matchPath(rest, append(matches, seg)); if (nΔ2 != nil) {
                return (nΔ2, m);
            }
        }
    }
    // Lastly, match the pattern (there can be at most one) that has a multi
    // wildcard in this position to the rest of the path.
    {
        var c = n.multiChild; if (c != nil) {
            // Don't record a match for a nameless wildcard (which arises from a
            // trailing slash in the pattern).
            if ((~c).pattern.lastSegment().s != ""u8) {
                matches = append(matches, pathUnescape(path[1..])); // remove initial slash
            }
            return (c, matches);
        }
    }
    return (default!, default!);
}

// firstSegment splits path into its first segment, and the rest.
// The path must begin with "/".
// If path consists of only a slash, firstSegment returns ("/", "").
// The segment is returned unescaped, if possible.
internal static (@string seg, @string rest) firstSegment(@string path) {
    if (path == "/"u8) {
        return ("/", "");
    }
    path = path[1..]; // drop initial slash
    nint i = strings.IndexByte(path, (rune)'/');
    if (i < 0) {
        i = builtin.len(path);
    }
    return (pathUnescape(path.slice(0, i)), path.slice(i));
}

// matchingMethods adds to methodSet all the methods that would result in a
// match if passed to routingNode.match with the given host and path.
internal static void matchingMethods(this ж<routingNode> Ꮡroot, @string host, @string path, map<@string, bool> methodSet) {
    ref var root = ref Ꮡroot.DerefOrNull();

    if (host != ""u8) {
        Ꮡroot.findChild(host).matchingMethodsPath(path, methodSet);
    }
    root.emptyChild.matchingMethodsPath(path, methodSet);
    if (methodSet[getˢ]) {
        methodSet[headˢ] = true;
    }
}

internal static void matchingMethodsPath(this ж<routingNode> Ꮡn, @string path, map<@string, bool> set) {
    if (Ꮡn == nil) {
        return;
    }
    var setʗ1 = set;
    Ꮡn.of(routingNode.Ꮡchildren).eachPair((@string method, ж<routingNode> c) => {
        {
            var (p, _) = c.matchPath(path, default!); if (p != nil) {
                setʗ1[method] = true;
            }
        }
        return true;
    });
}

// Don't look at the empty child. If there were an empty
// child, it would match on any method, but we only
// call this when we fail to match on a method.

} // end http_package
