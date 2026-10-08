// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using math = math_package;

partial class http_package {

// A routingIndex optimizes conflict detection by indexing patterns.
//
// The basic idea is to rule out patterns that cannot conflict with a given
// pattern because they have a different literal in a corresponding segment.
// See the comments in [routingIndex.possiblyConflictingPatterns] for more details.
partial struct routingIndex {
    // map from a particular segment position and value to all registered patterns
    // with that value in that position.
    // For example, the key {1, "b"} would hold the patterns "/a/b" and "/a/b/c"
    // but not "/a", "b/a", "/a/c" or "/a/{x}".
    internal map<routingIndexKey, slice<ж<pattern>>> segments;
    // All patterns that end in a multi wildcard (including trailing slash).
    // We do not try to be clever about indexing multi patterns, because there
    // are unlikely to be many of them.
    internal slice<ж<pattern>> multis;
}

partial struct routingIndexKey {
    internal nint pos;   // 0-based segment position
    internal @string s; // literal, or empty for wildcard
}

internal static void addPattern(this ref routingIndex idx, ж<pattern> Ꮡpat) {
    ref var pat = ref Ꮡpat.DerefOrNull();

    if (pat.lastSegment().multi){
        idx.multis = append(idx.multis, Ꮡpat);
    } else {
        if (idx.segments == default!) {
            idx.segments = new map<routingIndexKey, slice<ж<pattern>>>{};
        }
        foreach (var (pos, seg) in pat.segments) {
            var key = new routingIndexKey(pos: pos, s: ""u8);
            if (!seg.wild) {
                key.s = seg.s;
            }
            idx.segments[key] = append(idx.segments[key], Ꮡpat);
        }
    }
}

// possiblyConflictingPatterns calls f on all patterns that might conflict with
// pat. If f returns a non-nil error, possiblyConflictingPatterns returns immediately
// with that error.
//
// To be correct, possiblyConflictingPatterns must include all patterns that
// might conflict. But it may also include patterns that cannot conflict.
// For instance, an implementation that returns all registered patterns is correct.
// We use this fact throughout, simplifying the implementation by returning more
// patterns that we might need to.
internal static error /*err*/ possiblyConflictingPatterns(this ref routingIndex idx, ж<pattern> Ꮡpat, Func<ж<pattern>, error> f) {
    error err = default!;

    ref var pat = ref Ꮡpat.DerefOrNull();
    // Terminology:
    //   dollar pattern: one ending in "{$}"
    //   multi pattern: one ending in a trailing slash or "{x...}" wildcard
    //   ordinary pattern: neither of the above
    // apply f to all the pats, stopping on error.
    error apply(slice<ж<pattern>> pats) {
        if (err != default!) {
            return err;
        }
        foreach (var (_, p) in pats) {
            err = f(p);
            if (err != default!) {
                return err;
            }
        }
        return default!;
    }
    // Our simple indexing scheme doesn't try to prune multi patterns; assume
    // any of them can match the argument.
    {
        var errΔ1 = apply(idx.multis); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    if (pat.lastSegment().s == "/"u8) {
        // All paths that a dollar pattern matches end in a slash; no paths that
        // an ordinary pattern matches do. So only other dollar or multi
        // patterns can conflict with a dollar pattern. Furthermore, conflicting
        // dollar patterns must have the {$} in the same position.
        return apply(idx.segments[new routingIndexKey(s: "/"u8, pos: builtin.len(pat.segments) - 1)]);
    }
    // For ordinary and multi patterns, the only conflicts can be with a multi,
    // or a pattern that has the same literal or a wildcard at some literal
    // position.
    // We could intersect all the possible matches at each position, but we
    // do something simpler: we find the position with the fewest patterns.
    slice<ж<pattern>> lmin = default!;
    slice<ж<pattern>> wmin = default!;
    nint min = math.MaxInt;
    var hasLit = false;
    foreach (var (i, seg) in pat.segments) {
        if (seg.multi) {
            break;
        }
        if (!seg.wild) {
            hasLit = true;
            var lpats = idx.segments[new routingIndexKey(s: seg.s, pos: i)];
            var wpats = idx.segments[new routingIndexKey(s: ""u8, pos: i)];
            {
                nint sum = builtin.len(lpats) + builtin.len(wpats); if (sum < min) {
                    lmin = lpats;
                    wmin = wpats;
                    min = sum;
                }
            }
        }
    }
    if (hasLit) {
        apply(lmin);
        apply(wmin);
        return err;
    }
    // This pattern is all wildcards.
    // Check it against everything.
    foreach (var (_, pats) in idx.segments) {
        apply(pats);
    }
    return err;
}

} // end http_package
