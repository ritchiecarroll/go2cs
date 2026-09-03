// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using io = io_package;
using Δsync = sync_package;
using ꓸꓸꓸstring = Span<@string>;

partial class strings_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸsync() {
    builtin.initPackage(typeof(sync_package));
}

// Replacer replaces a list of strings with replacements.
// It is safe for concurrent use by multiple goroutines.
[GoType] partial struct Replacer {
    internal Δsync.Once once; // guards buildOnce method
    internal replacer r;
    internal slice<@string> oldnew;
}

// replacer is the interface that a replacement algorithm needs to implement.
[GoType] partial interface replacer {
    @string Replace(@string s);
    (nint n, error err) WriteString(io.Writer w, @string s);
}

// NewReplacer returns a new [Replacer] from a list of old, new string
// pairs. Replacements are performed in the order they appear in the
// target string, without overlapping matches. The old string
// comparisons are done in argument order.
//
// NewReplacer panics if given an odd number of arguments.
public static ж<Replacer> NewReplacer(params ꓸꓸꓸstring oldnewʗp) {
    var oldnew = oldnewʗp.slice();

    if (len(oldnew) % 2 == 1) {
        throw panic("strings.NewReplacer: odd argument count");
    }
    return Ꮡ(new Replacer(oldnew: appendꓸꓸꓸ(slice<@string>(default!), oldnew)));
}

[GoRecv] internal static void buildOnce(this ref Replacer r) {
    r.r = r.build();
    r.oldnew = default!;
}

[GoRecv] internal static replacer build(this ref Replacer b) {
    var oldnew = b.oldnew;
    if (len(oldnew) == 2 && len(oldnew[0]) > 1) {
        return new singleStringReplacerжreplacer(makeSingleStringReplacer(oldnew[0], oldnew[1]));
    }
    var allNewBytes = true;
    for (nint i = 0; i < len(oldnew); i += 2) {
        if (len(oldnew[i]) != 1) {
            return new genericReplacerжreplacer(makeGenericReplacer(oldnew));
        }
        if (len(oldnew[i + 1]) != 1) {
            allNewBytes = false;
        }
    }
    if (allNewBytes) {
        ref var rΔ1 = ref heap<byteReplacer>(out var ᏑrΔ1);
        rΔ1 = new byteReplacer(new byte[256].array());
        foreach (var (i, _) in rΔ1) {
            rΔ1[i] = (byte)i;
        }
        // The first occurrence of old->new map takes precedence
        // over the others with the same old string.
        for (nint i = len(oldnew) - 2; i >= 0; i -= 2) {
            var o = oldnew[i][0];
            var n = oldnew[i + 1][0];
            rΔ1[o] = n;
        }
        return new byteReplacerжreplacer(ᏑrΔ1);
    }
    ref var r = ref heap<byteStringReplacer>(out var Ꮡr);
    r = new byteStringReplacer(toReplace: new slice<@string>(0, len(oldnew) / 2));
    // The first occurrence of old->new map takes precedence
    // over the others with the same old string.
    for (nint i = len(oldnew) - 2; i >= 0; i -= 2) {
        var o = oldnew[i][0];
        @string n = oldnew[i + 1];
        // To avoid counting repetitions multiple times.
        if (r.replacements[o] == default!) {
            // We need to use string([]byte{o}) instead of string(o),
            // to avoid utf8 encoding of o.
            // E. g. byte(150) produces string of length 2.
            r.toReplace = append(r.toReplace, ((@string)new byte[]{o}.slice()));
        }
        r.replacements[o] = slice<byte>(n);
    }
    return new byteStringReplacerжreplacer(Ꮡr);
}

// Replace returns a copy of s with all replacements performed.
public static @string Replace(this ж<Replacer> Ꮡr, @string s) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    return r.r.Replace(s);
}

// WriteString writes s to w with all replacements performed.
public static (nint n, error err) WriteString(this ж<Replacer> Ꮡr, io.Writer w, @string s) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.of(Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    return r.r.WriteString(w, s);
}

// trieNode is a node in a lookup trie for prioritized key/value pairs. Keys
// and values may be empty. For example, the trie containing keys "ax", "ay",
// "bcbc", "x" and "xy" could have eight nodes:
//
//	n0  -
//	n1  a-
//	n2  .x+
//	n3  .y+
//	n4  b-
//	n5  .cbc+
//	n6  x+
//	n7  .y+
//
// n0 is the root node, and its children are n1, n4 and n6; n1's children are
// n2 and n3; n4's child is n5; n6's child is n7. Nodes n0, n1 and n4 (marked
// with a trailing "-") are partial keys, and nodes n2, n3, n5, n6 and n7
// (marked with a trailing "+") are complete keys.
[GoType] partial struct trieNode {
    // value is the value of the trie node's key/value pair. It is empty if
    // this node is not a complete key.
    internal @string value;
    // priority is the priority (higher is more important) of the trie node's
    // key/value pair; keys are not necessarily matched shortest- or longest-
    // first. Priority is positive if this node is a complete key, and zero
    // otherwise. In the example above, positive/zero priorities are marked
    // with a trailing "+" or "-".
    internal nint priority;
// A trie node may have zero, one or more child nodes:
//  * if the remaining fields are zero, there are no children.
//  * if prefix and next are non-zero, there is one child in next.
//  * if table is non-zero, it defines all the children.
//
// Prefixes are preferred over tables when there is one child, but the
// root node always uses a table for lookup efficiency.

    // prefix is the difference in keys between this trie node and the next.
    // In the example above, node n4 has prefix "cbc" and n4's next node is n5.
    // Node n5 has no children and so has zero prefix, next and table fields.
    internal @string prefix;
    internal ж<trieNode> next;
    // table is a lookup table indexed by the next byte in the key, after
    // remapping that byte through genericReplacer.mapping to create a dense
    // index. In the example above, the keys only use 'a', 'b', 'c', 'x' and
    // 'y', which remap to 0, 1, 2, 3 and 4. All other bytes remap to 5, and
    // genericReplacer.tableSize will be 5. Node n0's table will be
    // []*trieNode{ 0:n1, 1:n4, 3:n6 }, where the 0, 1 and 3 are the remapped
    // 'a', 'b' and 'x'.
    internal slice<ж<trieNode>> table;
}

[GoRecv] internal static void add(this ref trieNode t, @string key, @string val, nint priority, ж<genericReplacer> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (key == ""u8) {
        if (t.priority == 0) {
            t.value = val;
            t.priority = priority;
        }
        return;
    }
    if (t.prefix != ""u8){
        // Need to split the prefix among multiple nodes.
        nint n = default!;       // length of the longest common prefix
        for (; n < len(t.prefix) && n < len(key); n++) {
            if (t.prefix[n] != key[n]) {
                break;
            }
        }
        if (n == len(t.prefix)){
            t.next.add(key[(int)(n)..], val, priority, Ꮡr);
        } else 
        if (n == 0){
            // First byte differs, start a new lookup table here. Looking up
            // what is currently t.prefix[0] will lead to prefixNode, and
            // looking up key[0] will lead to keyNode.
            ж<trieNode> prefixNode = default!;
            if (len(t.prefix) == 1){
                prefixNode = t.next;
            } else {
                prefixNode = Ꮡ(new trieNode(
                    prefix: t.prefix[1..],
                    next: t.next
                ));
            }
            var keyNode = @new<trieNode>();
            t.table = new slice<ж<trieNode>>(r.tableSize);
            t.table[r.mapping[t.prefix[0]]] = prefixNode;
            t.table[r.mapping[key[0]]] = keyNode;
            t.prefix = ""u8;
            t.next = default!;
            keyNode.add(key[1..], val, priority, Ꮡr);
        } else {
            // Insert new node after the common section of the prefix.
            var next = Ꮡ(new trieNode(
                prefix: t.prefix[(int)(n)..],
                next: t.next
            ));
            t.prefix = t.prefix[..(int)(n)];
            t.next = next;
            next.add(key[(int)(n)..], val, priority, Ꮡr);
        }
    } else 
    if (t.table != default!){
        // Insert into existing table.
        var m = r.mapping[key[0]];
        if (t.table[m] == nil) {
            t.table[m] = @new<trieNode>();
        }
        t.table[m].add(key[1..], val, priority, Ꮡr);
    } else {
        t.prefix = key;
        t.next = @new<trieNode>();
        t.next.add(""u8, val, priority, Ꮡr);
    }
}

internal static (@string val, nint keylen, bool found) lookup(this ж<genericReplacer> Ꮡr, @string s, bool ignoreRoot) {
    @string val = default!;
    nint keylen = default!;
    bool found = default!;

    ref var r = ref Ꮡr.DerefOrNull();
    // Iterate down the trie to the end, and grab the value and keylen with
    // the highest priority.
    nint bestPriority = 0;
    var node = Ꮡr.of(genericReplacer.Ꮡroot);
    nint n = 0;
    while (node != nil) {
        if ((~node).priority > bestPriority && !(ignoreRoot && node == Ꮡr.of(genericReplacer.Ꮡroot))) {
            bestPriority = node.Value.priority;
            val = node.Value.value;
            keylen = n;
            found = true;
        }
        if (s == ""u8) {
            break;
        }
        if ((~node).table != default!){
            var index = r.mapping[s[0]];
            if ((nint)index == r.tableSize) {
                break;
            }
            node = (~node).table[index];
            s = s[1..];
            n++;
        } else 
        if ((~node).prefix != ""u8 && HasPrefix(s, (~node).prefix)){
            n += len((~node).prefix);
            s = s[(int)(len((~node).prefix))..];
            node = node.Value.next;
        } else {
            break;
        }
    }
    return (val, keylen, found);
}

// genericReplacer is the fully generic algorithm.
// It's used as a fallback when nothing faster can be used.
[GoType] partial struct genericReplacer {
    internal trieNode root;
    // tableSize is the size of a trie node's lookup table. It is the number
    // of unique key bytes.
    internal nint tableSize;
    // mapping maps from key bytes to a dense index for trieNode.table.
    internal array<byte> mapping = new(256);
}

internal static ж<genericReplacer> makeGenericReplacer(slice<@string> oldnew) {
    var r = @new<genericReplacer>();
    // Find each byte used, then assign them each an index.
    for (nint i = 0; i < len(oldnew); i += 2) {
        @string key = oldnew[i];
        for (nint j = 0; j < len(key); j++) {
            r.Value.mapping[key[j]] = 1;
        }
    }
    foreach (var (_, b) in (~r).mapping.ΔRangeSnapshot()) {
        r.Value.tableSize += (nint)b;
    }
    byte index = default!;
    foreach (var (i, b) in (~r).mapping.ΔRangeSnapshot()) {
        if (b == 0){
            r.Value.mapping[i] = (byte)(~r).tableSize;
        } else {
            r.Value.mapping[i] = index;
            index++;
        }
    }
    // Ensure root node uses a lookup table (for performance).
    r.Value.root.table = new slice<ж<trieNode>>((~r).tableSize);
    for (nint i = 0; i < len(oldnew); i += 2) {
        r.of(genericReplacer.Ꮡroot).add(oldnew[i], oldnew[i + 1], len(oldnew) - i, r);
    }
    return r;
}

[GoType("[]byte")] partial struct appendSliceWriter;

// Write writes to the buffer to satisfy [io.Writer].
[GoRecv] internal static (nint, error) Write(this ref appendSliceWriter w, slice<byte> p) {
    w = appendꓸꓸꓸ(w, p);
    return (len(p), default!);
}

// WriteString writes to the buffer without string->[]byte->string allocations.
[GoRecv] internal static (nint, error) WriteString(this ref appendSliceWriter w, @string s) {
    w = append(w, s.ꓸꓸꓸ);
    return (len(s), default!);
}

[GoType] partial struct stringWriter {
    internal io.Writer w;
}

internal static (nint, error) WriteString(this stringWriter w, @string s) {
    return w.w.Write(slice<byte>(s));
}

internal static io.StringWriter getStringWriter(io.Writer w) {
    var (sw, ok) = w._<io.StringWriter>(ᐧ);
    if (!ok) {
        sw = new stringWriter(w);
    }
    return sw;
}

internal static @string Replace(this ж<genericReplacer> Ꮡr, @string s) {
    ref var buf = ref heap<appendSliceWriter>(out var Ꮡbuf);
    buf = new appendSliceWriter(0, len(s));
    Ꮡr.WriteString(new appendSliceWriterжWriter(Ꮡbuf), s);
    return ((@string)(slice<byte>)buf);
}

internal static (nint n, error err) WriteString(this ж<genericReplacer> Ꮡr, io.Writer w, @string s) {
    nint n = default!;
    error err = default!;

    ref var r = ref Ꮡr.DerefOrNull();
    var sw = getStringWriter(w);
    nint last = default!;
    nint wn = default!;
    bool prevMatchEmpty = default!;
    for (nint i = 0; i <= len(s); ) {
        // Fast path: s[i] is not a prefix of any pattern.
        if (i != len(s) && r.root.priority == 0) {
            nint index = (nint)r.mapping[s[i]];
            if (index == r.tableSize || r.root.table[index] == nil) {
                i++;
                continue;
            }
        }
        // Ignore the empty match iff the previous loop found the empty match.
        var (val, keylen, match) = Ꮡr.lookup(s[(int)(i)..], prevMatchEmpty);
        prevMatchEmpty = match && keylen == 0;
        if (match) {
            (wn, err) = sw.WriteString(s[(int)(last)..(int)(i)]);
            n += wn;
            if (err != default!) {
                return (n, err);
            }
            (wn, err) = sw.WriteString(val);
            n += wn;
            if (err != default!) {
                return (n, err);
            }
            i += keylen;
            last = i;
            continue;
        }
        i++;
    }
    if (last != len(s)) {
        (wn, err) = sw.WriteString(s[(int)(last)..]);
        n += wn;
    }
    return (n, err);
}

// singleStringReplacer is the implementation that's used when there is only
// one string to replace (and that string has more than one byte).
[GoType] partial struct singleStringReplacer {
    internal ж<stringFinder> finder;
    // value is the new string that replaces that pattern when it's found.
    internal @string value;
}

internal static ж<singleStringReplacer> makeSingleStringReplacer(@string pattern, @string value) {
    return Ꮡ(new singleStringReplacer(finder: makeStringFinder(pattern), value: value));
}

[GoRecv] internal static @string Replace(this ref singleStringReplacer r, @string s) {
    ref var buf = ref heap(new Builder(), out var Ꮡbuf);
    nint i = 0;
    var matched = false;
    while (ᐧ) {
        nint match = r.finder.next(s[(int)(i)..]);
        if (match == -1) {
            break;
        }
        matched = true;
        Ꮡbuf.Grow(match + len(r.value));
        Ꮡbuf.WriteString(s[(int)(i)..(int)(i + match)]);
        Ꮡbuf.WriteString(r.value);
        i += match + len((~r.finder).pattern);
    }
    if (!matched) {
        return s;
    }
    Ꮡbuf.WriteString(s[(int)(i)..]);
    return buf.String();
}

[GoRecv] internal static (nint n, error err) WriteString(this ref singleStringReplacer r, io.Writer w, @string s) {
    nint n = default!;
    error err = default!;

    var sw = getStringWriter(w);
    nint i = default!;
    nint wn = default!;
    while (ᐧ) {
        nint match = r.finder.next(s[(int)(i)..]);
        if (match == -1) {
            break;
        }
        (wn, err) = sw.WriteString(s[(int)(i)..(int)(i + match)]);
        n += wn;
        if (err != default!) {
            return (n, err);
        }
        (wn, err) = sw.WriteString(r.value);
        n += wn;
        if (err != default!) {
            return (n, err);
        }
        i += match + len((~r.finder).pattern);
    }
    (wn, err) = sw.WriteString(s[(int)(i)..]);
    n += wn;
    return (n, err);
}

[GoType("[256]byte")] partial struct byteReplacer;

[GoRecv] internal static @string Replace(this ref byteReplacer r, @string s) {
    slice<byte> buf = default!;           // lazily allocated
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (r.Value[b] != b) {
            if (buf == default!) {
                buf = slice<byte>(s);
            }
            buf[i] = r.Value[b];
        }
    }
    if (buf == default!) {
        return s;
    }
    return ((@string)buf);
}

[GoRecv] internal static (nint n, error err) WriteString(this ref byteReplacer r, io.Writer w, @string s) {
    nint n = default!;

    var sw = getStringWriter(w);
    nint last = 0;
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (r.Value[b] == b) {
            continue;
        }
        if (last != i) {
            var (wn, errΔ1) = sw.WriteString(s[(int)(last)..(int)(i)]);
            n += wn;
            if (errΔ1 != default!) {
                return (n, errΔ1);
            }
        }
        last = i + 1;
        var (nw, errΔ2) = w.Write(r.Value[(int)(b)..(int)((nint)b + 1)]);
        n += nw;
        if (errΔ2 != default!) {
            return (n, errΔ2);
        }
    }
    if (last != len(s)) {
        var (nw, errΔ3) = sw.WriteString(s[(int)(last)..]);
        n += nw;
        if (errΔ3 != default!) {
            return (n, errΔ3);
        }
    }
    return (n, default!);
}

// byteStringReplacer is the implementation that's used when all the
// "old" values are single ASCII bytes but the "new" values vary in size.
[GoType] partial struct byteStringReplacer {
    // replacements contains replacement byte slices indexed by old byte.
    // A nil []byte means that the old byte should not be replaced.
    internal array<slice<byte>> replacements = new(256);
    // toReplace keeps a list of bytes to replace. Depending on length of toReplace
    // and length of target string it may be faster to use Count, or a plain loop.
    // We store single byte as a string, because Count takes a string.
    internal slice<@string> toReplace;
}

// countCutOff controls the ratio of a string length to a number of replacements
// at which (*byteStringReplacer).Replace switches algorithms.
// For strings with higher ration of length to replacements than that value,
// we call Count, for each replacement from toReplace.
// For strings, with a lower ratio we use simple loop, because of Count overhead.
// countCutOff is an empirically determined overhead multiplier.
// TODO(tocarip) revisit once we have register-based abi/mid-stack inlining.
internal static UntypedInt countCutOff => 8;

[GoRecv] internal static @string Replace(this ref byteStringReplacer r, @string s) {
    nint newSize = len(s);
    var anyChanges = false;
    // Is it faster to use Count?
    if (len(r.toReplace) * (nint)countCutOff <= len(s)){
        foreach (var (_, x) in r.toReplace) {
            {
                nint c = Count(s, x); if (c != 0) {
                    // The -1 is because we are replacing 1 byte with len(replacements[b]) bytes.
                    newSize += c * (len(r.replacements[x[0]]) - 1);
                    anyChanges = true;
                }
            }
        }
    } else {
        for (nint i = 0; i < len(s); i++) {
            var b = s[i];
            if (r.replacements[b] != default!) {
                // See above for explanation of -1
                newSize += len(r.replacements[b]) - 1;
                anyChanges = true;
            }
        }
    }
    if (!anyChanges) {
        return s;
    }
    var buf = new slice<byte>(newSize);
    nint j = 0;
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (r.replacements[b] != default!){
            j += copy(buf[(int)(j)..], r.replacements[b]);
        } else {
            buf[j] = b;
            j++;
        }
    }
    return ((@string)buf);
}

[GoRecv] internal static (nint n, error err) WriteString(this ref byteStringReplacer r, io.Writer w, @string s) {
    nint n = default!;
    error err = default!;

    var sw = getStringWriter(w);
    nint last = 0;
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        if (r.replacements[b] == default!) {
            continue;
        }
        if (last != i) {
            var (nwΔ1, errΔ1) = sw.WriteString(s[(int)(last)..(int)(i)]);
            n += nwΔ1;
            if (errΔ1 != default!) {
                return (n, errΔ1);
            }
        }
        last = i + 1;
        var (nw, errΔ2) = w.Write(r.replacements[b]);
        n += nw;
        if (errΔ2 != default!) {
            return (n, errΔ2);
        }
    }
    if (last != len(s)) {
        nint nw = default!;
        (nw, err) = sw.WriteString(s[(int)(last)..]);
        n += nw;
    }
    return (n, err);
}

} // end strings_package
