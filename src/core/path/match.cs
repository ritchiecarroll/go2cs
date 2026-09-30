// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using utf8 = unicode.utf8_package;
using @internal;
using unicode;

partial class path_package {

// ErrBadPattern indicates a pattern was malformed.
public static error ErrBadPattern = errors.New("syntax error in pattern"u8);

// Match reports whether name matches the shell pattern.
// The pattern syntax is:
//
//	pattern:
//		{ term }
//	term:
//		'*'         matches any sequence of non-/ characters
//		'?'         matches any single non-/ character
//		'[' [ '^' ] { character-range } ']'
//		            character class (must be non-empty)
//		c           matches character c (c != '*', '?', '\\', '[')
//		'\\' c      matches character c
//
//	character-range:
//		c           matches character c (c != '\\', '-', ']')
//		'\\' c      matches character c
//		lo '-' hi   matches character c for lo <= c <= hi
//
// Match requires pattern to match all of name, not just a substring.
// The only possible returned error is [ErrBadPattern], when pattern
// is malformed.
public static (bool matched, error err) Match(@string pattern, @string name) {
Pattern:
    while (len(pattern) > 0) {
        bool star = default!;
        @string chunk = default!;
        (star, chunk, pattern) = scanChunk(pattern);
        if (star && chunk == ""u8) {
            // Trailing * matches rest of string unless it has a /.
            return (bytealg.IndexByteString(name, (rune)'/') < 0, default!);
        }
        // Look for match at current position.
        var (t, ok, errΔ1) = matchChunk(chunk, name);
        // if we're the last chunk, make sure we've exhausted the name
        // otherwise we'll give a false result even if we could still match
        // using the star
        if (ok && (len(t) == 0 || len(pattern) > 0)) {
            name = t;
            continue;
        }
        if (errΔ1 != default!) {
            return (false, errΔ1);
        }
        if (star) {
            // Look for match skipping i+1 bytes.
            // Cannot skip /.
            for (nint i = 0; i < len(name) && name[i] != (rune)'/'; i++) {
                var (tΔ1, okΔ1, errΔ2) = matchChunk(chunk, name.slice(i + 1));
                if (okΔ1) {
                    // if we're the last chunk, make sure we exhausted the name
                    if (len(pattern) == 0 && len(tΔ1) > 0) {
                        continue;
                    }
                    name = tΔ1;
                    goto continue_Pattern;
                }
                if (errΔ2 != default!) {
                    return (false, errΔ2);
                }
            }
        }
        // Before returning false with no error,
        // check that the remainder of the pattern is syntactically valid.
        while (len(pattern) > 0) {
            (_, chunk, pattern) = scanChunk(pattern);
            {
                var (_, _, errΔ3) = matchChunk(chunk, ""u8); if (errΔ3 != default!) {
                    return (false, errΔ3);
                }
            }
        }
        return (false, default!);
continue_Pattern:;
    }
break_Pattern:;
    return (len(name) == 0, default!);
}

// scanChunk gets the next segment of pattern, which is a non-star string
// possibly preceded by a star.
internal static (bool star, @string chunk, @string rest) scanChunk(@string pattern) {
    bool star = default!;

    while (len(pattern) > 0 && pattern[0] == (rune)'*') {
        pattern = pattern[1..];
        star = true;
    }
    var inrange = false;
    nint i = default!;
Scan:
    for (i = 0; i < len(pattern); i++) {
        switch (pattern[i]) {
        case (rune)'\\': {
            if (i + 1 < len(pattern)) {
                // error check handled in matchChunk: bad pattern.
                i++;
            }
            break;
        }
        case (rune)'[': {
            inrange = true;
            break;
        }
        case (rune)']': {
            inrange = false;
            break;
        }
        case (rune)'*': {
            if (!inrange) {
                goto break_Scan;
            }
            break;
        }}

continue_Scan:;
    }
break_Scan:;
    return (star, pattern.slice(0, i), pattern.slice(i));
}

// matchChunk checks whether chunk matches the beginning of s.
// If so, it returns the remainder of s (after the match).
// Chunk is all single-character operators: literals, char classes, and ?.
internal static (@string rest, bool ok, error err) matchChunk(@string chunk, @string s) {
    error err = default!;

    // failed records whether the match has failed.
    // After the match fails, the loop continues on processing chunk,
    // checking that the pattern is well-formed but no longer reading s.
    var failed = false;
    while (len(chunk) > 0) {
        if (!failed && len(s) == 0) {
            failed = true;
        }
        var exprᴛ1 = chunk[0];
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)'[') { matchᴛ1 = true;
            // character class
            rune r = default!;
            if (!failed) {
                nint n = default!;
                (r, n) = utf8.DecodeRuneInString(s);
                s = s.slice(n);
            }
            chunk = chunk[1..];
            var negated = false;
            if (len(chunk) > 0 && chunk[0] == (rune)'^') {
                // possibly negated
                negated = true;
                chunk = chunk[1..];
            }
            var match = false;
            nint nrange = 0;
            while (ᐧ) {
                // parse all ranges
                if (len(chunk) > 0 && chunk[0] == (rune)']' && nrange > 0) {
                    chunk = chunk[1..];
                    break;
                }
                rune loΔ1 = default!;
                rune hiΔ1 = default!;
                {
                    (loΔ1, chunk, err) = getEsc(chunk); if (err != default!) {
                        return ("", false, err);
                    }
                }
                hiΔ1 = loΔ1;
                if (chunk[0] == (rune)'-') {
                    {
                        (hiΔ1, chunk, err) = getEsc(chunk[1..]); if (err != default!) {
                            return ("", false, err);
                        }
                    }
                }
                if (loΔ1 <= r && r <= hiΔ1) {
                    match = true;
                }
                nrange++;
            }
            if (match == negated) {
                failed = true;
            }
        }
        else if (exprᴛ1 is (rune)'?') { matchᴛ1 = true;
            if (!failed) {
                if (s[0] == (rune)'/') {
                    failed = true;
                }
                var (_, n) = utf8.DecodeRuneInString(s);
                s = s.slice(n);
            }
            chunk = chunk[1..];
        }
        else if (exprᴛ1 is (rune)'\\') { matchᴛ1 = true;
            chunk = chunk[1..];
            if (len(chunk) == 0) {
                return ("", false, ErrBadPattern);
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            if (!failed) {
                if (chunk[0] != s[0]) {
                    failed = true;
                }
                s = s[1..];
            }
            chunk = chunk[1..];
        }

    }
    if (failed) {
        return ("", false, default!);
    }
    return (s, true, default!);
}

// getEsc gets a possibly-escaped character from chunk, for a character class.
internal static (rune r, @string nchunk, error err) getEsc(@string chunk) {
    rune r = default!;
    @string nchunk = default!;
    error err = default!;

    if (len(chunk) == 0 || chunk[0] == (rune)'-' || chunk[0] == (rune)']') {
        err = ErrBadPattern;
        return (r, nchunk, err);
    }
    if (chunk[0] == (rune)'\\') {
        chunk = chunk[1..];
        if (len(chunk) == 0) {
            err = ErrBadPattern;
            return (r, nchunk, err);
        }
    }
    (r, var n) = utf8.DecodeRuneInString(chunk);
    if (r == utf8.RuneError && n == 1) {
        err = ErrBadPattern;
    }
    nchunk = chunk.slice(n);
    if (len(nchunk) == 0) {
        err = ErrBadPattern;
    }
    return (r, nchunk, err);
}

} // end path_package
