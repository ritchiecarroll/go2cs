// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Patterns for ServeMux routing.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using url = go.net.url_package;
using strings = strings_package;
using unicode = unicode_package;
using go.net;

partial class http_package {

// A pattern is something that can be matched against an HTTP request.
// It has an optional method, an optional host, and a path.
[GoType] partial struct pattern {
    internal @string str; // original string
    internal @string method;
    internal @string host;
    // The representation of a path differs from the surface syntax, which
    // simplifies most algorithms.
    //
    // Paths ending in '/' are represented with an anonymous "..." wildcard.
    // For example, the path "a/" is represented as a literal segment "a" followed
    // by a segment with multi==true.
    //
    // Paths ending in "{$}" are represented with the literal segment "/".
    // For example, the path "a/{$}" is represented as a literal segment "a" followed
    // by a literal segment "/".
    internal slice<segment> segments;
    internal @string loc; // source location of registering call, for helpful messages
}

[GoRecv] internal static @string String(this ref pattern p) {
    return p.str;
}

[GoRecv] internal static segment lastSegment(this ref pattern p) {
    return p.segments[builtin.len(p.segments) - 1];
}

// A segment is a pattern piece that matches one or more path segments, or
// a trailing slash.
//
// If wild is false, it matches a literal segment, or, if s == "/", a trailing slash.
// Examples:
//
//	"a" => segment{s: "a"}
//	"/{$}" => segment{s: "/"}
//
// If wild is true and multi is false, it matches a single path segment.
// Example:
//
//	"{x}" => segment{s: "x", wild: true}
//
// If both wild and multi are true, it matches all remaining path segments.
// Example:
//
//	"{rest...}" => segment{s: "rest", wild: true, multi: true}
[GoType] partial struct segment {
    internal @string s; // literal or wildcard name or "/" for "/{$}".
    internal bool wild;
    internal bool multi; // "..." wildcard
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyPatternˢ = "empty pattern"u8;
internal static readonly @string hostPathMissingˢ = "host/path missing /"u8;
internal static readonly @string hostContainsMissingˢ = "host contains '{' (missing initial '/'?)"u8;
internal static readonly @string nonConnectPatternWithˢ = "non-CONNECT pattern with unclean path can never match"u8;
internal static readonly @string badWildcardSegmentMustˢ = "bad wildcard segment (must start with '{')"u8;
internal static readonly @string badWildcardSegmentMustˢ2 = "bad wildcard segment (must end with '}')"u8;
internal static readonly @string notAtEndˢ = "{$} not at end"u8;
internal static readonly @string wildcardNotAtEndˢ = "{...} wildcard not at end"u8;
internal static readonly @string emptyWildcardˢ = "empty wildcard"u8;

// parsePattern parses a string into a Pattern.
// The string's syntax is
//
//	[METHOD] [HOST]/[PATH]
//
// where:
//   - METHOD is an HTTP method
//   - HOST is a hostname
//   - PATH consists of slash-separated segments, where each segment is either
//     a literal or a wildcard of the form "{name}", "{name...}", or "{$}".
//
// METHOD, HOST and PATH are all optional; that is, the string can be "/".
// If METHOD is present, it must be followed by at least one space or tab.
// Wildcard names must be valid Go identifiers.
// The "{$}" and "{name...}" wildcard must occur at the end of PATH.
// PATH may end with a '/'.
// Wildcard names in a path must be distinct.
internal static (ж<pattern>, error err) parsePattern(@string s) {
    ж<pattern> _ᴛ1 = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        if (builtin.len(s) == 0) {
            (_ᴛ1, err) = (default!, errors.New(emptyPatternˢ)); goto ᒐdone;
        }
        nint off = 0; // offset into string
        defer(() => {
            if (err != default!) {
                err = fmt.Errorf("at offset %d: %w"u8, off, err);
            }
        }, ref ᒐ);
        ref var method = ref heap<@string>(out var Ꮡmethod);
        method = s;
        @string rest = ""u8;
        var found = false;
        {
            nint iΔ1 = strings.IndexAny(s, " \t"u8); if (iΔ1 >= 0) {
                (method, rest, found) = (s.slice(0, iΔ1), strings.TrimLeft(s.slice(iΔ1 + 1), " \t"u8), true);
            }
        }
        if (!found) {
            rest = method;
            method = ""u8;
        }
        if (method != ""u8 && !validMethod(method)) {
            (_ᴛ1, err) = (default!, fmt.Errorf("invalid method %q"u8, method)); goto ᒐdone;
        }
        var p = Ꮡ(new pattern(str: s, method: method));
        if (found) {
            off = builtin.len(method) + 1;
        }
        nint i = strings.IndexByte(rest, (rune)'/');
        if (i < 0) {
            (_ᴛ1, err) = (default!, errors.New(hostPathMissingˢ)); goto ᒐdone;
        }
        p.Value.host = rest.slice(0, i);
        rest = rest.slice(i);
        {
            nint j = strings.IndexByte((~p).host, (rune)'{'); if (j >= 0) {
                off += j;
                (_ᴛ1, err) = (default!, errors.New(hostContainsMissingˢ)); goto ᒐdone;
            }
        }
        // At this point, rest is the path.
        off += i;
        // An unclean path with a method that is not CONNECT can never match,
        // because paths are cleaned before matching.
        if (method != ""u8 && method != "CONNECT"u8 && rest != cleanPath(rest)) {
            (_ᴛ1, err) = (default!, errors.New(nonConnectPatternWithˢ)); goto ᒐdone;
        }
        var seenNames = new map<@string, bool>{}; // remember wildcard names to catch dups
        while (builtin.len(rest) > 0) {
            // Invariant: rest[0] == '/'.
            rest = rest[1..];
            off = builtin.len(s) - builtin.len(rest);
            if (builtin.len(rest) == 0) {
                // Trailing slash.
                p.Value.segments = append((~p).segments, new segment(wild: true, multi: true));
                break;
            }
            nint iΔ2 = strings.IndexByte(rest, (rune)'/');
            if (iΔ2 < 0) {
                iΔ2 = builtin.len(rest);
            }
            @string seg = default!;
            (seg, rest) = (rest.slice(0, iΔ2), rest.slice(iΔ2));
            {
                nint iΔ3 = strings.IndexByte(seg, (rune)'{'); if (iΔ3 < 0){
                    // Literal.
                    seg = pathUnescape(seg);
                    p.Value.segments = append((~p).segments, new segment(s: seg));
                } else {
                    // Wildcard.
                    if (iΔ3 != 0) {
                        (_ᴛ1, err) = (default!, errors.New(badWildcardSegmentMustˢ)); goto ᒐdone;
                    }
                    if (seg[builtin.len(seg) - 1] != (rune)'}') {
                        (_ᴛ1, err) = (default!, errors.New(badWildcardSegmentMustˢ2)); goto ᒐdone;
                    }
                    @string name = seg.slice(1, builtin.len(seg) - 1);
                    if (name == "$"u8) {
                        if (builtin.len(rest) != 0) {
                            (_ᴛ1, err) = (default!, errors.New(notAtEndˢ)); goto ᒐdone;
                        }
                        p.Value.segments = append((~p).segments, new segment(s: "/"u8));
                        break;
                    }
                    (name, var multi) = strings.CutSuffix(name, "..."u8);
                    if (multi && builtin.len(rest) != 0) {
                        (_ᴛ1, err) = (default!, errors.New(wildcardNotAtEndˢ)); goto ᒐdone;
                    }
                    if (name == ""u8) {
                        (_ᴛ1, err) = (default!, errors.New(emptyWildcardˢ)); goto ᒐdone;
                    }
                    if (!isValidWildcardName(name)) {
                        (_ᴛ1, err) = (default!, fmt.Errorf("bad wildcard name %q"u8, name)); goto ᒐdone;
                    }
                    if (seenNames[name]) {
                        (_ᴛ1, err) = (default!, fmt.Errorf("duplicate wildcard name %q"u8, name)); goto ᒐdone;
                    }
                    seenNames[name] = true;
                    p.Value.segments = append((~p).segments, new segment(s: name, wild: true, multi: multi));
                }
            }
        }
        (_ᴛ1, err) = (p, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, err);
}

internal static bool isValidWildcardName(@string s) {
    if (s == ""u8) {
        return false;
    }
    // Valid Go identifier.
    foreach (var (i, c) in s) {
        if (!unicode.IsLetter(c) && c != (rune)'_' && (i == 0 || !unicode.IsDigit(c))) {
            return false;
        }
    }
    return true;
}

internal static @string pathUnescape(@string path) {
    var (u, err) = url.PathUnescape(path);
    if (err != default!) {
        // Invalidly escaped path; use the original
        return path;
    }
    return u;
}

[GoType("@string")] partial struct relationship;

internal static readonly relationship equivalent = "equivalent"u8;              // both match the same requests
internal static readonly relationship moreGeneral = "moreGeneral"u8;             // p1 matches everything p2 does & more
internal static readonly relationship moreSpecific = "moreSpecific"u8;            // p2 matches everything p1 does & more
internal static readonly relationship disjoint = "disjoint"u8;                // there is no request that both match
internal static readonly relationship overlaps = "overlaps"u8;                // there is a request that both match, but neither is more specific

// conflictsWith reports whether p1 conflicts with p2, that is, whether
// there is a request that both match but where neither is higher precedence
// than the other.
//
//	Precedence is defined by two rules:
//	1. Patterns with a host win over patterns without a host.
//	2. Patterns whose method and path is more specific win. One pattern is more
//	   specific than another if the second matches all the (method, path) pairs
//	   of the first and more.
//
// If rule 1 doesn't apply, then two patterns conflict if their relationship
// is either equivalence (they match the same set of requests) or overlap
// (they both match some requests, but neither is more specific than the other).
[GoRecv] internal static bool conflictsWith(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.DerefOrNull();

    if (p1.host != p2.host) {
        // Either one host is empty and the other isn't, in which case the
        // one with the host wins by rule 1, or neither host is empty
        // and they differ, so they won't match the same paths.
        return false;
    }
    relationship rel = p1.comparePathsAndMethods(Ꮡp2);
    return rel == equivalent || rel == overlaps;
}

[GoRecv] internal static relationship comparePathsAndMethods(this ref pattern p1, ж<pattern> Ꮡp2) {
    relationship mrel = p1.compareMethods(Ꮡp2);
    // Optimization: avoid a call to comparePaths.
    if (mrel == disjoint) {
        return disjoint;
    }
    relationship prel = p1.comparePaths(Ꮡp2);
    return combineRelationships(mrel, prel);
}

// compareMethods determines the relationship between the method
// part of patterns p1 and p2.
//
// A method can either be empty, "GET", or something else.
// The empty string matches any method, so it is the most general.
// "GET" matches both GET and HEAD.
// Anything else matches only itself.
[GoRecv] internal static relationship compareMethods(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.DerefOrNull();

    if (p1.method == p2.method) {
        return equivalent;
    }
    if (p1.method == ""u8) {
        // p1 matches any method, but p2 does not, so p1 is more general.
        return moreGeneral;
    }
    if (p2.method == ""u8) {
        return moreSpecific;
    }
    if (p1.method == "GET"u8 && p2.method == "HEAD"u8) {
        // p1 matches GET and HEAD; p2 matches only HEAD.
        return moreGeneral;
    }
    if (p2.method == "GET"u8 && p1.method == "HEAD"u8) {
        return moreSpecific;
    }
    return disjoint;
}

// comparePaths determines the relationship between the path
// part of two patterns.
[GoRecv] internal static relationship comparePaths(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.DerefOrNull();

    // Optimization: if a path pattern doesn't end in a multi ("...") wildcard, then it
    // can only match paths with the same number of segments.
    if (builtin.len(p1.segments) != builtin.len(p2.segments) && !p1.lastSegment().multi && !p2.lastSegment().multi) {
        return disjoint;
    }
    // Consider corresponding segments in the two path patterns.
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    relationship rel = equivalent;
    for ((segs1, segs2) = (p1.segments, p2.segments); builtin.len(segs1) > 0 && builtin.len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        rel = combineRelationships(rel, compareSegments(segs1[0], segs2[0]));
        if (rel == disjoint) {
            return rel;
        }
    }
    // We've reached the end of the corresponding segments of the patterns.
    // If they have the same number of segments, then we've already determined
    // their relationship.
    if (builtin.len(segs1) == 0 && builtin.len(segs2) == 0) {
        return rel;
    }
    // Otherwise, the only way they could fail to be disjoint is if the shorter
    // pattern ends in a multi. In that case, that multi is more general
    // than the remainder of the longer pattern, so combine those two relationships.
    if (builtin.len(segs1) < builtin.len(segs2) && p1.lastSegment().multi) {
        return combineRelationships(rel, moreGeneral);
    }
    if (builtin.len(segs2) < builtin.len(segs1) && p2.lastSegment().multi) {
        return combineRelationships(rel, moreSpecific);
    }
    return disjoint;
}

// compareSegments determines the relationship between two segments.
internal static relationship compareSegments(segment s1, segment s2) {
    if (s1.multi && s2.multi) {
        return equivalent;
    }
    if (s1.multi) {
        return moreGeneral;
    }
    if (s2.multi) {
        return moreSpecific;
    }
    if (s1.wild && s2.wild) {
        return equivalent;
    }
    if (s1.wild) {
        if (s2.s == "/"u8) {
            // A single wildcard doesn't match a trailing slash.
            return disjoint;
        }
        return moreGeneral;
    }
    if (s2.wild) {
        if (s1.s == "/"u8) {
            return disjoint;
        }
        return moreSpecific;
    }
    // Both literals.
    if (s1.s == s2.s) {
        return equivalent;
    }
    return disjoint;
}

// combineRelationships determines the overall relationship of two patterns
// given the relationships of a partition of the patterns into two parts.
//
// For example, if p1 is more general than p2 in one way but equivalent
// in the other, then it is more general overall.
//
// Or if p1 is more general in one way and more specific in the other, then
// they overlap.
internal static relationship combineRelationships(relationship r1, relationship r2) {
    var exprᴛ1 = r1;
    if (exprᴛ1 == equivalent) {
        return r2;
    }
    if (exprᴛ1 == disjoint) {
        return disjoint;
    }
    if (exprᴛ1 == overlaps) {
        if (r2 == disjoint) {
            return disjoint;
        }
        return overlaps;
    }
    if (exprᴛ1 == moreGeneral || exprᴛ1 == moreSpecific) {
        var exprᴛ2 = r2;
        if (exprᴛ2 == equivalent) {
            return r1;
        }
        if (exprᴛ2 == inverseRelationship(r1)) {
            return overlaps;
        }
        { /* default: */
            return r2;
        }

    }
    else { /* default: */
        throw panic(fmt.Sprintf("unknown relationship %q"u8, r1));
    }

}

// If p1 has relationship `r` to p2, then
// p2 has inverseRelationship(r) to p1.
internal static relationship inverseRelationship(relationship r) {
    var exprᴛ1 = r;
    if (exprᴛ1 == moreSpecific) {
        return moreGeneral;
    }
    if (exprᴛ1 == moreGeneral) {
        return moreSpecific;
    }
    { /* default: */
        return r;
    }

}

// isLitOrSingle reports whether the segment is a non-dollar literal or a single wildcard.
internal static bool isLitOrSingle(segment seg) {
    if (seg.wild) {
        return !seg.multi;
    }
    return seg.s != "/"u8;
}

// describeConflict returns an explanation of why two patterns conflict.
internal static @string describeConflict(ж<pattern> Ꮡp1, ж<pattern> Ꮡp2) {
    ref var p1 = ref Ꮡp1.DerefOrNull();

    relationship mrel = p1.compareMethods(Ꮡp2);
    relationship prel = p1.comparePaths(Ꮡp2);
    relationship rel = combineRelationships(mrel, prel);
    if (rel == equivalent) {
        return fmt.Sprintf("%s matches the same requests as %s"u8, Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil());
    }
    if (rel != overlaps) {
        throw panic("describeConflict called with non-conflicting patterns");
    }
    if (prel == overlaps) {
        return fmt.Sprintf("""
%[1]s and %[2]s both match some paths, like %[3]q.
But neither is more specific than the other.
%[1]s matches %[4]q, but %[2]s doesn't.
%[2]s matches %[5]q, but %[1]s doesn't.
"""u8,
            Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), commonPath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp1).DerefOrNull(), ref (Ꮡp2).DerefOrNull()), differencePath(ref (Ꮡp2).DerefOrNull(), ref (Ꮡp1).DerefOrNull()));
    }
    if (mrel == moreGeneral && prel == moreSpecific) {
        return fmt.Sprintf("%s matches more methods than %s, but has a more specific path pattern"u8, Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil());
    }
    if (mrel == moreSpecific && prel == moreGeneral) {
        return fmt.Sprintf("%s matches fewer methods than %s, but has a more general path pattern"u8, Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil());
    }
    return fmt.Sprintf("bug: unexpected way for two patterns %s and %s to conflict: methods %s, paths %s"u8, Ꮡp1.OrTypedNil(), Ꮡp2.OrTypedNil(), mrel, prel);
}

// writeMatchingPath writes to b a path that matches the segments.
internal static void writeMatchingPath(ж<strings.Builder> Ꮡb, slice<segment> segs) {
    foreach (var (_, s) in segs) {
        writeSegment(Ꮡb, s);
    }
}

internal static void writeSegment(ж<strings.Builder> Ꮡb, segment s) {
    Ꮡb.WriteByte((rune)'/');
    if (!s.multi && s.s != "/"u8) {
        Ꮡb.WriteString(s.s);
    }
}

// commonPath returns a path that both p1 and p2 match.
// It assumes there is such a path.
internal static @string commonPath(ref pattern p1, ref pattern p2) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    for ((segs1, segs2) = (p1.segments, p2.segments); builtin.len(segs1) > 0 && builtin.len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        {
            var s1 = segs1[0]; if (s1.wild){
                writeSegment(Ꮡb, segs2[0]);
            } else {
                writeSegment(Ꮡb, s1);
            }
        }
    }
    if (builtin.len(segs1) > 0){
        writeMatchingPath(Ꮡb, segs1);
    } else 
    if (builtin.len(segs2) > 0) {
        writeMatchingPath(Ꮡb, segs2);
    }
    return b.String();
}

// differencePath returns a path that p1 matches and p2 doesn't.
// It assumes there is such a path.
internal static @string differencePath(ref pattern p1, ref pattern p2) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    for ((segs1, segs2) = (p1.segments, p2.segments); builtin.len(segs1) > 0 && builtin.len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        var s1 = segs1[0];
        var s2 = segs2[0];
        if (s1.multi && s2.multi) {
            // From here the patterns match the same paths, so we must have found a difference earlier.
            Ꮡb.WriteByte((rune)'/');
            return b.String();
        }
        if (s1.multi && !s2.multi) {
            // s1 ends in a "..." wildcard but s2 does not.
            // A trailing slash will distinguish them, unless s2 ends in "{$}",
            // in which case any segment will do; prefer the wildcard name if
            // it has one.
            Ꮡb.WriteByte((rune)'/');
            if (s2.s == "/"u8) {
                if (s1.s != ""u8){
                    Ꮡb.WriteString(s1.s);
                } else {
                    Ꮡb.WriteString("x"u8);
                }
            }
            return b.String();
        }
        if (!s1.multi && s2.multi){
            writeSegment(Ꮡb, s1);
        } else 
        if (s1.wild && s2.wild){
            // Both patterns will match whatever we put here; use
            // the first wildcard name.
            writeSegment(Ꮡb, s1);
        } else 
        if (s1.wild && !s2.wild){
            // s1 is a wildcard, s2 is a literal.
            // Any segment other than s2.s will work.
            // Prefer the wildcard name, but if it's the same as the literal,
            // tweak the literal.
            if (s1.s != s2.s){
                writeSegment(Ꮡb, s1);
            } else {
                Ꮡb.WriteByte((rune)'/');
                Ꮡb.WriteString(s2.s + "x"u8);
            }
        } else 
        if (!s1.wild && s2.wild){
            writeSegment(Ꮡb, s1);
        } else {
            // Both are literals. A precondition of this function is that the
            // patterns overlap, so they must be the same literal. Use it.
            if (s1.s != s2.s) {
                throw panic(fmt.Sprintf("literals differ: %q and %q"u8, s1.s, s2.s));
            }
            writeSegment(Ꮡb, s1);
        }
    }
    if (builtin.len(segs1) > 0){
        // p1 is longer than p2, and p2 does not end in a multi.
        // Anything that matches the rest of p1 will do.
        writeMatchingPath(Ꮡb, segs1);
    } else 
    if (builtin.len(segs2) > 0) {
        writeMatchingPath(Ꮡb, segs2);
    }
    return b.String();
}

} // end http_package
