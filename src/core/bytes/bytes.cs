// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bytes implements functions for the manipulation of byte slices.
// It is analogous to the facilities of the [strings] package.
namespace go;

using bytealg = @internal.bytealg_package;
using bits = math.bits_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using go.unicode;
using math;

partial class bytes_package {

// Equal reports whether a and b
// are the same length and contain the same bytes.
// A nil argument is equivalent to an empty slice.
public static bool Equal(slice<byte> a, slice<byte> b) {
    // Neither cmd/compile nor gccgo allocates for these string conversions.
    return ((sstring)a) == ((sstring)b);
}

// Compare returns an integer comparing two byte slices lexicographically.
// The result will be 0 if a == b, -1 if a < b, and +1 if a > b.
// A nil argument is equivalent to an empty slice.
public static nint Compare(slice<byte> a, slice<byte> b) {
    return bytealg.Compare(a, b);
}

// explode splits s into a slice of UTF-8 sequences, one per Unicode code point (still slices of bytes),
// up to a maximum of n byte slices. Invalid UTF-8 sequences are chopped into individual bytes.
internal static slice<slice<byte>> explode(slice<byte> s, nint n) {
    if (n <= 0 || n > len(s)) {
        n = len(s);
    }
    var a = new slice<slice<byte>>(n);
    nint size = default!;
    nint na = 0;
    while (len(s) > 0) {
        if (na + 1 >= n) {
            a[na] = s;
            na++;
            break;
        }
        (_, size) = utf8.DecodeRune(s);
        a[na] = s.slice(0, size, size);
        s = s[(int)(size)..];
        na++;
    }
    return a[0..(int)(na)];
}

// Count counts the number of non-overlapping instances of sep in s.
// If sep is an empty slice, Count returns 1 + the number of UTF-8-encoded code points in s.
public static nint Count(slice<byte> s, slice<byte> sep) {
    // special case
    if (len(sep) == 0) {
        return utf8.RuneCount(s) + 1;
    }
    if (len(sep) == 1) {
        return bytealg.Count(s, sep[0]);
    }
    nint n = 0;
    while (ᐧ) {
        nint i = Index(s, sep);
        if (i == -1) {
            return n;
        }
        n++;
        s = s[(int)(i + len(sep))..];
    }
}

// Contains reports whether subslice is within b.
public static bool Contains(slice<byte> b, slice<byte> subslice) {
    return Index(b, subslice) != -1;
}

// ContainsAny reports whether any of the UTF-8-encoded code points in chars are within b.
public static bool ContainsAny(slice<byte> b, @string chars) {
    return IndexAny(b, chars) >= 0;
}

// ContainsRune reports whether the rune is contained in the UTF-8-encoded byte slice b.
public static bool ContainsRune(slice<byte> b, rune r) {
    return IndexRune(b, r) >= 0;
}

// ContainsFunc reports whether any of the UTF-8-encoded code points r within b satisfy f(r).
public static bool ContainsFunc(slice<byte> b, Func<rune, bool> f) {
    return IndexFunc(b, f) >= 0;
}

// IndexByte returns the index of the first instance of c in b, or -1 if c is not present in b.
public static nint IndexByte(slice<byte> b, byte c) {
    return bytealg.IndexByte(b, c);
}

internal static nint indexBytePortable(slice<byte> s, byte c) {
    foreach (var (i, b) in s) {
        if (b == c) {
            return i;
        }
    }
    return -1;
}

// LastIndex returns the index of the last instance of sep in s, or -1 if sep is not present in s.
public static nint LastIndex(slice<byte> s, slice<byte> sep) {
    nint n = len(sep);
    switch (ᐧ) {
    case {} when n is 0: {
        return len(s);
    }
    case {} when n is 1: {
        return bytealg.LastIndexByte(s, sep[0]);
    }
    case {} when n == len(s): {
        if (Equal(s, sep)) {
            return 0;
        }
        return -1;
    }
    case {} when n > len(s): {
        return -1;
    }}

    return bytealg.LastIndexRabinKarp(s, sep);
}

// LastIndexByte returns the index of the last instance of c in s, or -1 if c is not present in s.
public static nint LastIndexByte(slice<byte> s, byte c) {
    return bytealg.LastIndexByte(s, c);
}

// IndexRune interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index of the first occurrence in s of the given rune.
// It returns -1 if rune is not present in s.
// If r is [utf8.RuneError], it returns the first instance of any
// invalid UTF-8 byte sequence.
public static nint IndexRune(slice<byte> s, rune r) {
    const bool haveFastIndex = /* bytealg.MaxBruteForce > 0 */ true;
    switch (ᐧ) {
    case {} when 0 <= r && r < utf8.RuneSelf: {
        return IndexByte(s, (byte)r);
    }
    case {} when r == utf8.RuneError: {
        for (nint i = 0; i < len(s); ) {
            var (r1, n) = utf8.DecodeRune(s[(int)(i)..]);
            if (r1 == utf8.RuneError) {
                return i;
            }
            i += n;
        }
        return -1;
    }
    case {} when !utf8.ValidRune(r): {
        return -1;
    }
    default: {
        // Search for rune r using the last byte of its UTF-8 encoded form.
        // The distribution of the last byte is more uniform compared to the
        // first byte which has a 78% chance of being [240, 243, 244].
        array<byte> b = new(4); /* utf8.UTFMax */
        nint n = utf8.EncodeRune(b[..], r);
        nint last = n - 1;
        nint i = last;
        nint fails = 0;
        while (i < len(s)) {
            if (s[i] != b[last]) {
                nint o = IndexByte(s[(int)(i + 1)..], b[last]);
                if (o < 0) {
                    return -1;
                }
                i += o + 1;
            }
            // Step backwards comparing bytes.
            for (nint j = 1; j < n; j++) {
                if (s[i - j] != b[last - j]) {
                    goto next;
                }
            }
            return i - last;
next:
            fails++;
            i++;
            if ((haveFastIndex && fails > bytealg.Cutover(i)) && i < len(s) || (!haveFastIndex && fails >= 4 + (i >> (int)(4)) && i < len(s))) {
                goto fallback;
            }
        }
        return -1;
fallback:
        if (haveFastIndex){
            // Switch to bytealg.Index, if available, or a brute force search when
            // IndexByte returns too many false positives.
            {
                nint j = bytealg.Index(s[(int)(i - last)..], b[..(int)(n)]); if (j >= 0) {
                    return i + j - last;
                }
            }
        } else {
            // If bytealg.Index is not available a brute force search is
            // ~1.5-3x faster than Rabin-Karp since n is small.
            var c0 = b[last];
            var c1 = b[last - 1]; // There are at least 2 chars to match
loop:
            for (; i < len(s); i++) {
                if (s[i] == c0 && s[i - 1] == c1) {
                    for (nint k = 2; k < n; k++) {
                        if (s[i - k] != b[last - k]) {
                            goto continue_loop;
                        }
                    }
                    return i - last;
                }
continue_loop:;
            }
break_loop:;
        }
        return -1;
    }}

}

// IndexAny interprets s as a sequence of UTF-8-encoded Unicode code points.
// It returns the byte index of the first occurrence in s of any of the Unicode
// code points in chars. It returns -1 if chars is empty or if there is no code
// point in common.
public static nint IndexAny(slice<byte> s, @string chars) {
    if (chars == ""u8) {
        // Avoid scanning all of s.
        return -1;
    }
    if (len(s) == 1) {
        var r = (rune)s[0];
        if (r >= utf8.RuneSelf) {
            // search utf8.RuneError.
            foreach (var (_, rᴛ1) in chars) {
                r = rᴛ1;

                if (r == utf8.RuneError) {
                    return 0;
                }
            }
            return -1;
        }
        if (bytealg.IndexByteString(chars, s[0]) >= 0) {
            return 0;
        }
        return -1;
    }
    if (len(chars) == 1) {
        var r = (rune)chars[0];
        if (r >= utf8.RuneSelf) {
            r = utf8.RuneError;
        }
        return IndexRune(s, r);
    }
    if (len(s) > 8) {
        {
            var (@as, isASCII) = makeASCIISet(chars); if (isASCII) {
                foreach (var (i, c) in s) {
                    if (@as.contains(c)) {
                        return i;
                    }
                }
                return -1;
            }
        }
    }
    nint width = default!;
    for (nint i = 0; i < len(s); i += width) {
        var r = (rune)s[i];
        if (r < utf8.RuneSelf) {
            if (bytealg.IndexByteString(chars, s[i]) >= 0) {
                return i;
            }
            width = 1;
            continue;
        }
        (r, width) = utf8.DecodeRune(s[(int)(i)..]);
        if (r != utf8.RuneError) {
            // r is 2 to 4 bytes
            if (len(chars) == width) {
                if (chars == ((@string)r)) {
                    return i;
                }
                continue;
            }
            // Use bytealg.IndexString for performance if available.
            if (bytealg.MaxLen >= width) {
                if (bytealg.IndexString(chars, ((@string)r)) >= 0) {
                    return i;
                }
                continue;
            }
        }
        foreach (var (_, ch) in chars) {
            if (r == ch) {
                return i;
            }
        }
    }
    return -1;
}

// LastIndexAny interprets s as a sequence of UTF-8-encoded Unicode code
// points. It returns the byte index of the last occurrence in s of any of
// the Unicode code points in chars. It returns -1 if chars is empty or if
// there is no code point in common.
public static nint LastIndexAny(slice<byte> s, @string chars) {
    if (chars == ""u8) {
        // Avoid scanning all of s.
        return -1;
    }
    if (len(s) > 8) {
        {
            var (@as, isASCII) = makeASCIISet(chars); if (isASCII) {
                for (nint i = len(s) - 1; i >= 0; i--) {
                    if (@as.contains(s[i])) {
                        return i;
                    }
                }
                return -1;
            }
        }
    }
    if (len(s) == 1) {
        var r = (rune)s[0];
        if (r >= utf8.RuneSelf) {
            foreach (var (_, rᴛ1) in chars) {
                r = rᴛ1;

                if (r == utf8.RuneError) {
                    return 0;
                }
            }
            return -1;
        }
        if (bytealg.IndexByteString(chars, s[0]) >= 0) {
            return 0;
        }
        return -1;
    }
    if (len(chars) == 1) {
        var cr = (rune)chars[0];
        if (cr >= utf8.RuneSelf) {
            cr = utf8.RuneError;
        }
        for (nint i = len(s); i > 0; ) {
            var (r, size) = utf8.DecodeLastRune(s[..(int)(i)]);
            i -= size;
            if (r == cr) {
                return i;
            }
        }
        return -1;
    }
    for (nint i = len(s); i > 0; ) {
        var r = (rune)s[i - 1];
        if (r < utf8.RuneSelf) {
            if (bytealg.IndexByteString(chars, s[i - 1]) >= 0) {
                return i - 1;
            }
            i--;
            continue;
        }
        (r, var size) = utf8.DecodeLastRune(s[..(int)(i)]);
        i -= size;
        if (r != utf8.RuneError) {
            // r is 2 to 4 bytes
            if (len(chars) == size) {
                if (chars == ((@string)r)) {
                    return i;
                }
                continue;
            }
            // Use bytealg.IndexString for performance if available.
            if (bytealg.MaxLen >= size) {
                if (bytealg.IndexString(chars, ((@string)r)) >= 0) {
                    return i;
                }
                continue;
            }
        }
        foreach (var (_, ch) in chars) {
            if (r == ch) {
                return i;
            }
        }
    }
    return -1;
}

// Generic split: splits after each instance of sep,
// including sepSave bytes of sep in the subslices.
internal static slice<slice<byte>> genSplit(slice<byte> s, slice<byte> sep, nint sepSave, nint n) {
    if (n == 0) {
        return default!;
    }
    if (len(sep) == 0) {
        return explode(s, n);
    }
    if (n < 0) {
        n = Count(s, sep) + 1;
    }
    if (n > len(s) + 1) {
        n = len(s) + 1;
    }
    var a = new slice<slice<byte>>(n);
    n--;
    nint i = 0;
    while (i < n) {
        nint m = Index(s, sep);
        if (m < 0) {
            break;
        }
        a[i] = s.slice(-1, m + sepSave, m + sepSave);
        s = s[(int)(m + len(sep))..];
        i++;
    }
    a[i] = s;
    return a[..(int)(i + 1)];
}

// SplitN slices s into subslices separated by sep and returns a slice of
// the subslices between those separators.
// If sep is empty, SplitN splits after each UTF-8 sequence.
// The count determines the number of subslices to return:
//   - n > 0: at most n subslices; the last subslice will be the unsplit remainder;
//   - n == 0: the result is nil (zero subslices);
//   - n < 0: all subslices.
//
// To split around the first instance of a separator, see [Cut].
public static slice<slice<byte>> SplitN(slice<byte> s, slice<byte> sep, nint n) {
    return genSplit(s, sep, 0, n);
}

// SplitAfterN slices s into subslices after each instance of sep and
// returns a slice of those subslices.
// If sep is empty, SplitAfterN splits after each UTF-8 sequence.
// The count determines the number of subslices to return:
//   - n > 0: at most n subslices; the last subslice will be the unsplit remainder;
//   - n == 0: the result is nil (zero subslices);
//   - n < 0: all subslices.
public static slice<slice<byte>> SplitAfterN(slice<byte> s, slice<byte> sep, nint n) {
    return genSplit(s, sep, len(sep), n);
}

// Split slices s into all subslices separated by sep and returns a slice of
// the subslices between those separators.
// If sep is empty, Split splits after each UTF-8 sequence.
// It is equivalent to SplitN with a count of -1.
//
// To split around the first instance of a separator, see [Cut].
public static slice<slice<byte>> Split(slice<byte> s, slice<byte> sep) {
    return genSplit(s, sep, 0, -1);
}

// SplitAfter slices s into all subslices after each instance of sep and
// returns a slice of those subslices.
// If sep is empty, SplitAfter splits after each UTF-8 sequence.
// It is equivalent to SplitAfterN with a count of -1.
public static slice<slice<byte>> SplitAfter(slice<byte> s, slice<byte> sep) {
    return genSplit(s, sep, len(sep), -1);
}

internal static array<uint8> asciiSpace = new array<uint8>(256){[(rune)'\t'] = 1, [(rune)'\n'] = 1, [(rune)'\v'] = 1, [(rune)'\f'] = 1, [(rune)'\r'] = 1, [(rune)' '] = 1};

// Fields interprets s as a sequence of UTF-8-encoded code points.
// It splits the slice s around each instance of one or more consecutive white space
// characters, as defined by [unicode.IsSpace], returning a slice of subslices of s or an
// empty slice if s contains only white space.
public static slice<slice<byte>> Fields(slice<byte> s) {
    // First count the fields.
    // This is an exact count if s is ASCII, otherwise it is an approximation.
    nint n = 0;
    nint wasSpace = 1;
    // setBits is used to track which bits are set in the bytes of s.
    var setBits = (uint8)0;
    for (nint iΔ1 = 0; iΔ1 < len(s); iΔ1++) {
        var r = s[iΔ1];
        setBits |= (byte)(r);
        nint isSpace = (nint)asciiSpace[r];
        n += (nint)(wasSpace & ~isSpace);
        wasSpace = isSpace;
    }
    if (setBits >= utf8.RuneSelf) {
        // Some runes in the input slice are not ASCII.
        return FieldsFunc(s, Δunicode.IsSpace);
    }
    // ASCII fast path
    var a = new slice<slice<byte>>(n);
    nint na = 0;
    nint fieldStart = 0;
    nint i = 0;
    // Skip spaces in the front of the input.
    while (i < len(s) && asciiSpace[s[i]] != 0) {
        i++;
    }
    fieldStart = i;
    while (i < len(s)) {
        if (asciiSpace[s[i]] == 0) {
            i++;
            continue;
        }
        a[na] = s.slice(fieldStart, i, i);
        na++;
        i++;
        // Skip spaces in between fields.
        while (i < len(s) && asciiSpace[s[i]] != 0) {
            i++;
        }
        fieldStart = i;
    }
    if (fieldStart < len(s)) {
        // Last field might end at EOF.
        a[na] = s.slice(fieldStart, len(s), len(s));
    }
    return a;
}

// A span is used to record a slice of s of the form s[start:end].
// The start index is inclusive and the end index is exclusive.
[GoType("dyn")] internal partial struct FieldsFunc_span {
    internal nint start;
    internal nint end;
}

// FieldsFunc interprets s as a sequence of UTF-8-encoded code points.
// It splits the slice s at each run of code points c satisfying f(c) and
// returns a slice of subslices of s. If all code points in s satisfy f(c), or
// len(s) == 0, an empty slice is returned.
//
// FieldsFunc makes no guarantees about the order in which it calls f(c)
// and assumes that f always returns the same value for a given c.
public static slice<slice<byte>> FieldsFunc(slice<byte> s, Func<rune, bool> f) {
    var spans = new slice<FieldsFunc_span>(0, 32);
    // Find the field start and end indices.
    // Doing this in a separate pass (rather than slicing the string s
    // and collecting the result substrings right away) is significantly
    // more efficient, possibly due to cache effects.
    nint start = -1; // valid span start if >= 0
    for (nint i = 0; i < len(s); ) {
        nint size = 1;
        var r = (rune)s[i];
        if (r >= utf8.RuneSelf) {
            (r, size) = utf8.DecodeRune(s[(int)(i)..]);
        }
        if (f(r)){
            if (start >= 0) {
                spans = append(spans, new FieldsFunc_span(start, i));
                start = -1;
            }
        } else {
            if (start < 0) {
                start = i;
            }
        }
        i += size;
    }
    // Last field might end at EOF.
    if (start >= 0) {
        spans = append(spans, new FieldsFunc_span(start, len(s)));
    }
    // Create subslices from recorded field indices.
    var a = new slice<slice<byte>>(len(spans));
    foreach (var (i, span) in spans) {
        a[i] = s.slice(span.start, span.end, span.end);
    }
    return a;
}

// Join concatenates the elements of s to create a new byte slice. The separator
// sep is placed between elements in the resulting slice.
public static slice<byte> Join(slice<slice<byte>> s, slice<byte> sep) {
    if (len(s) == 0) {
        return new byte[]{}.slice();
    }
    if (len(s) == 1) {
        // Just return a copy.
        return appendꓸꓸꓸ(slice<byte>(default!), s[0]);
    }
    nint n = default!;
    if (len(sep) > 0) {
        if (len(sep) >= maxInt / (len(s) - 1)) {
            throw panic("bytes: Join output length overflow");
        }
        n += len(sep) * (len(s) - 1);
    }
    foreach (var (_, v) in s) {
        if (len(v) > maxInt - n) {
            throw panic("bytes: Join output length overflow");
        }
        n += len(v);
    }
    var b = bytealg.MakeNoZero(n).slice(-1, n, n);
    nint bp = copy(b, s[0]);
    foreach (var (_, v) in s[1..]) {
        bp += copy(b[(int)(bp)..], sep);
        bp += copy(b[(int)(bp)..], v);
    }
    return b;
}

// HasPrefix reports whether the byte slice s begins with prefix.
public static bool HasPrefix(slice<byte> s, slice<byte> prefix) {
    return len(s) >= len(prefix) && Equal(s[..(int)(len(prefix))], prefix);
}

// HasSuffix reports whether the byte slice s ends with suffix.
public static bool HasSuffix(slice<byte> s, slice<byte> suffix) {
    return len(s) >= len(suffix) && Equal(s[(int)(len(s) - len(suffix))..], suffix);
}

// Map returns a copy of the byte slice s with all its characters modified
// according to the mapping function. If mapping returns a negative value, the character is
// dropped from the byte slice with no replacement. The characters in s and the
// output are interpreted as UTF-8-encoded code points.
public static slice<byte> Map(Func<rune, rune> mapping, slice<byte> s) {
    // In the worst case, the slice can grow when mapped, making
    // things unpleasant. But it's so rare we barge in assuming it's
    // fine. It could also shrink but that falls out naturally.
    var b = new slice<byte>(0, len(s));
    for (nint i = 0; i < len(s); ) {
        nint wid = 1;
        var r = (rune)s[i];
        if (r >= utf8.RuneSelf) {
            (r, wid) = utf8.DecodeRune(s[(int)(i)..]);
        }
        r = mapping(r);
        if (r >= 0) {
            b = utf8.AppendRune(b, r);
        }
        i += wid;
    }
    return b;
}

// Despite being an exported symbol,
// Repeat is linknamed by widely used packages.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/num
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
// Note that this comment is not part of the doc comment.
//
//go:linkname Repeat

// Repeat returns a new byte slice consisting of count copies of b.
//
// It panics if count is negative or if the result of (len(b) * count)
// overflows.
public static slice<byte> Repeat(slice<byte> b, nint count) {
    if (count == 0) {
        return new byte[]{}.slice();
    }
    // Since we cannot return an error on overflow,
    // we should panic if the repeat will generate an overflow.
    // See golang.org/issue/16237.
    if (count < 0) {
        throw panic("bytes: negative Repeat count");
    }
    var (hi, lo) = bits.Mul((nuint)len(b), (nuint)count);
    if (hi > 0 || lo > (nuint)maxInt) {
        throw panic("bytes: Repeat output length overflow");
    }
    nint n = (nint)lo; // lo = len(b) * count
    if (len(b) == 0) {
        return new byte[]{}.slice();
    }
    // Past a certain chunk size it is counterproductive to use
    // larger chunks as the source of the write, as when the source
    // is too large we are basically just thrashing the CPU D-cache.
    // So if the result length is larger than an empirically-found
    // limit (8KB), we stop growing the source string once the limit
    // is reached and keep reusing the same source string - that
    // should therefore be always resident in the L1 cache - until we
    // have completed the construction of the result.
    // This yields significant speedups (up to +100%) in cases where
    // the result length is large (roughly, over L2 cache size).
    const nint chunkLimit = /* 8 * 1024 */ 8192;
    nint chunkMax = n;
    if (chunkMax > chunkLimit) {
        chunkMax = chunkLimit / len(b) * len(b);
        if (chunkMax == 0) {
            chunkMax = len(b);
        }
    }
    var nb = bytealg.MakeNoZero(n).slice(-1, n, n);
    nint bp = copy(nb, b);
    while (bp < n) {
        nint chunk = min(bp, chunkMax);
        bp += copy(nb[(int)(bp)..], nb[..(int)(chunk)]);
    }
    return nb;
}

// ToUpper returns a copy of the byte slice s with all Unicode letters mapped to
// their upper case.
public static slice<byte> ToUpper(slice<byte> s) {
    var (isASCII, hasLower) = (true, false);
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c >= utf8.RuneSelf) {
            isASCII = false;
            break;
        }
        hasLower = hasLower || ((rune)'a' <= c && c <= (rune)'z');
    }
    if (isASCII) {
        // optimize for ASCII-only byte slices.
        if (!hasLower) {
            // Just return a copy.
            return appendꓸꓸꓸ(slice<byte>(""u8), s);
        }
        var b = bytealg.MakeNoZero(len(s)).slice(-1, len(s), len(s));
        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if ((rune)'a' <= c && c <= (rune)'z') {
                c -= (byte)((rune)'a' - (rune)'A');
            }
            b[i] = c;
        }
        return b;
    }
    return Map(Δunicode.ToUpper, s);
}

// ToLower returns a copy of the byte slice s with all Unicode letters mapped to
// their lower case.
public static slice<byte> ToLower(slice<byte> s) {
    var (isASCII, hasUpper) = (true, false);
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c >= utf8.RuneSelf) {
            isASCII = false;
            break;
        }
        hasUpper = hasUpper || ((rune)'A' <= c && c <= (rune)'Z');
    }
    if (isASCII) {
        // optimize for ASCII-only byte slices.
        if (!hasUpper) {
            return appendꓸꓸꓸ(slice<byte>(""u8), s);
        }
        var b = bytealg.MakeNoZero(len(s)).slice(-1, len(s), len(s));
        for (nint i = 0; i < len(s); i++) {
            var c = s[i];
            if ((rune)'A' <= c && c <= (rune)'Z') {
                c += (byte)((rune)'a' - (rune)'A');
            }
            b[i] = c;
        }
        return b;
    }
    return Map(Δunicode.ToLower, s);
}

// ToTitle treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their title case.
public static slice<byte> ToTitle(slice<byte> s) {
    return Map(Δunicode.ToTitle, s);
}

// ToUpperSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// upper case, giving priority to the special casing rules.
public static slice<byte> ToUpperSpecial(Δunicode.SpecialCase c, slice<byte> s) {
    var cʗ1 = c;
    return Map((rune p1) => cʗ1.ToUpper(p1), s);
}

// ToLowerSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// lower case, giving priority to the special casing rules.
public static slice<byte> ToLowerSpecial(Δunicode.SpecialCase c, slice<byte> s) {
    var cʗ1 = c;
    return Map((rune p1) => cʗ1.ToLower(p1), s);
}

// ToTitleSpecial treats s as UTF-8-encoded bytes and returns a copy with all the Unicode letters mapped to their
// title case, giving priority to the special casing rules.
public static slice<byte> ToTitleSpecial(Δunicode.SpecialCase c, slice<byte> s) {
    var cʗ1 = c;
    return Map((rune p1) => cʗ1.ToTitle(p1), s);
}

// ToValidUTF8 treats s as UTF-8-encoded bytes and returns a copy with each run of bytes
// representing invalid UTF-8 replaced with the bytes in replacement, which may be empty.
public static slice<byte> ToValidUTF8(slice<byte> s, slice<byte> replacement) {
    var b = new slice<byte>(0, len(s) + len(replacement));
    var invalid = false; // previous byte was from an invalid UTF-8 sequence
    for (nint i = 0; i < len(s); ) {
        var c = s[i];
        if (c < utf8.RuneSelf) {
            i++;
            invalid = false;
            b = append(b, c);
            continue;
        }
        var (_, wid) = utf8.DecodeRune(s[(int)(i)..]);
        if (wid == 1) {
            i++;
            if (!invalid) {
                invalid = true;
                b = appendꓸꓸꓸ(b, replacement);
            }
            continue;
        }
        invalid = false;
        b = appendꓸꓸꓸ(b, s[(int)(i)..(int)(i + wid)]);
        i += wid;
    }
    return b;
}

// isSeparator reports whether the rune could mark a word boundary.
// TODO: update when package unicode captures more of the properties.
internal static bool isSeparator(rune r) {
    // ASCII alphanumerics and underscore are not separators
    if (r <= 0x7F) {
        switch (ᐧ) {
        case {} when (rune)'0' <= r && r <= (rune)'9': {
            return false;
        }
        case {} when (rune)'a' <= r && r <= (rune)'z': {
            return false;
        }
        case {} when (rune)'A' <= r && r <= (rune)'Z': {
            return false;
        }
        case {} when r is (rune)'_': {
            return false;
        }}

        return true;
    }
    // Letters and digits are not separators
    if (Δunicode.IsLetter(r) || Δunicode.IsDigit(r)) {
        return false;
    }
    // Otherwise, all we can do for now is treat spaces as separators.
    return Δunicode.IsSpace(r);
}

// Title treats s as UTF-8-encoded bytes and returns a copy with all Unicode letters that begin
// words mapped to their title case.
//
// Deprecated: The rule Title uses for word boundaries does not handle Unicode
// punctuation properly. Use golang.org/x/text/cases instead.
public static slice<byte> Title(slice<byte> s) {
    // Use a closure here to remember state.
    // Hackish but effective. Depends on Map scanning in order and calling
    // the closure once per rune.
    var prev = (rune)' ';
    return Map(
        (rune r) => {
            if (isSeparator(prev)) {
                prev = r;
                return Δunicode.ToTitle(r);
            }
            prev = r;
            return r;
        },
        s);
}

// TrimLeftFunc treats s as UTF-8-encoded bytes and returns a subslice of s by slicing off
// all leading UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimLeftFunc(slice<byte> s, Func<rune, bool> f) {
    nint i = indexFunc(s, f, false);
    if (i == -1) {
        return default!;
    }
    return s[(int)(i)..];
}

// TrimRightFunc returns a subslice of s by slicing off all trailing
// UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimRightFunc(slice<byte> s, Func<rune, bool> f) {
    nint i = lastIndexFunc(s, f, false);
    if (i >= 0 && s[i] >= utf8.RuneSelf){
        var (_, wid) = utf8.DecodeRune(s[(int)(i)..]);
        i += wid;
    } else {
        i++;
    }
    return s[0..(int)(i)];
}

// TrimFunc returns a subslice of s by slicing off all leading and trailing
// UTF-8-encoded code points c that satisfy f(c).
public static slice<byte> TrimFunc(slice<byte> s, Func<rune, bool> f) {
    return TrimRightFunc(TrimLeftFunc(s, f), f);
}

// TrimPrefix returns s without the provided leading prefix string.
// If s doesn't start with prefix, s is returned unchanged.
public static slice<byte> TrimPrefix(slice<byte> s, slice<byte> prefix) {
    if (HasPrefix(s, prefix)) {
        return s[(int)(len(prefix))..];
    }
    return s;
}

// TrimSuffix returns s without the provided trailing suffix string.
// If s doesn't end with suffix, s is returned unchanged.
public static slice<byte> TrimSuffix(slice<byte> s, slice<byte> suffix) {
    if (HasSuffix(s, suffix)) {
        return s[..(int)(len(s) - len(suffix))];
    }
    return s;
}

// IndexFunc interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index in s of the first Unicode
// code point satisfying f(c), or -1 if none do.
public static nint IndexFunc(slice<byte> s, Func<rune, bool> f) {
    return indexFunc(s, f, true);
}

// LastIndexFunc interprets s as a sequence of UTF-8-encoded code points.
// It returns the byte index in s of the last Unicode
// code point satisfying f(c), or -1 if none do.
public static nint LastIndexFunc(slice<byte> s, Func<rune, bool> f) {
    return lastIndexFunc(s, f, true);
}

// indexFunc is the same as IndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
internal static nint indexFunc(slice<byte> s, Func<rune, bool> f, bool truth) {
    nint start = 0;
    while (start < len(s)) {
        nint wid = 1;
        var r = (rune)s[start];
        if (r >= utf8.RuneSelf) {
            (r, wid) = utf8.DecodeRune(s[(int)(start)..]);
        }
        if (f(r) == truth) {
            return start;
        }
        start += wid;
    }
    return -1;
}

// lastIndexFunc is the same as LastIndexFunc except that if
// truth==false, the sense of the predicate function is
// inverted.
internal static nint lastIndexFunc(slice<byte> s, Func<rune, bool> f, bool truth) {
    for (nint i = len(s); i > 0; ) {
        var r = (rune)s[i - 1];
        nint size = 1;
        if (r >= utf8.RuneSelf) {
            (r, size) = utf8.DecodeLastRune(s[0..(int)(i)]);
        }
        i -= size;
        if (f(r) == truth) {
            return i;
        }
    }
    return -1;
}

[GoType("[8]uint32")] partial struct asciiSet;

// makeASCIISet creates a set of ASCII characters and reports whether all
// characters in chars are ASCII.
internal static (asciiSet @as, bool ok) makeASCIISet(@string chars) {
    asciiSet @as = default!;

    for (nint i = 0; i < len(chars); i++) {
        var c = chars[i];
        if (c >= utf8.RuneSelf) {
            return (@as.Clone(), false);
        }
        @as[c / 32] |= (uint32)(((uint32)1 << (int)((c % 32))));
    }
    return (@as.Clone(), true);
}

// contains reports whether c is inside the set.
[GoRecv] internal static bool contains(this ref asciiSet @as, byte c) {
    return ((uint32)(@as.Value[c / 32] & (((uint32)1 << (int)((c % 32)))))) != 0;
}

// containsRune is a simplified version of strings.ContainsRune
// to avoid importing the strings package.
// We avoid bytes.ContainsRune to avoid allocating a temporary copy of s.
internal static bool containsRune(@string s, rune r) {
    foreach (var (_, c) in s) {
        if (c == r) {
            return true;
        }
    }
    return false;
}

// Trim returns a subslice of s by slicing off all leading and
// trailing UTF-8-encoded code points contained in cutset.
public static slice<byte> Trim(slice<byte> s, @string cutset) {
    if (len(s) == 0) {
        // This is what we've historically done.
        return default!;
    }
    if (cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimLeftByte(trimRightByte(s, cutset[0]), cutset[0]);
    }
    {
        ref var @as = ref heap<asciiSet>(out var Ꮡas);
        (@as, var ok) = makeASCIISet(cutset); if (ok) {
            return trimLeftASCII(trimRightASCII(s, Ꮡas), Ꮡas);
        }
    }
    return trimLeftUnicode(trimRightUnicode(s, cutset), cutset);
}

// TrimLeft returns a subslice of s by slicing off all leading
// UTF-8-encoded code points contained in cutset.
public static slice<byte> TrimLeft(slice<byte> s, @string cutset) {
    if (len(s) == 0) {
        // This is what we've historically done.
        return default!;
    }
    if (cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimLeftByte(s, cutset[0]);
    }
    {
        ref var @as = ref heap<asciiSet>(out var Ꮡas);
        (@as, var ok) = makeASCIISet(cutset); if (ok) {
            return trimLeftASCII(s, Ꮡas);
        }
    }
    return trimLeftUnicode(s, cutset);
}

internal static slice<byte> trimLeftByte(slice<byte> s, byte c) {
    while (len(s) > 0 && s[0] == c) {
        s = s[1..];
    }
    if (len(s) == 0) {
        // This is what we've historically done.
        return default!;
    }
    return s;
}

internal static slice<byte> trimLeftASCII(slice<byte> s, ж<asciiSet> Ꮡas) {
    ref var @as = ref Ꮡas.DerefOrNull();

    while (len(s) > 0) {
        if (!@as.contains(s[0])) {
            break;
        }
        s = s[1..];
    }
    if (len(s) == 0) {
        // This is what we've historically done.
        return default!;
    }
    return s;
}

internal static slice<byte> trimLeftUnicode(slice<byte> s, @string cutset) {
    while (len(s) > 0) {
        var r = (rune)s[0];
        nint n = 1;
        if (r >= utf8.RuneSelf) {
            (r, n) = utf8.DecodeRune(s);
        }
        if (!containsRune(cutset, r)) {
            break;
        }
        s = s[(int)(n)..];
    }
    if (len(s) == 0) {
        // This is what we've historically done.
        return default!;
    }
    return s;
}

// TrimRight returns a subslice of s by slicing off all trailing
// UTF-8-encoded code points that are contained in cutset.
public static slice<byte> TrimRight(slice<byte> s, @string cutset) {
    if (len(s) == 0 || cutset == ""u8) {
        return s;
    }
    if (len(cutset) == 1 && cutset[0] < utf8.RuneSelf) {
        return trimRightByte(s, cutset[0]);
    }
    {
        ref var @as = ref heap<asciiSet>(out var Ꮡas);
        (@as, var ok) = makeASCIISet(cutset); if (ok) {
            return trimRightASCII(s, Ꮡas);
        }
    }
    return trimRightUnicode(s, cutset);
}

internal static slice<byte> trimRightByte(slice<byte> s, byte c) {
    while (len(s) > 0 && s[len(s) - 1] == c) {
        s = s[..(int)(len(s) - 1)];
    }
    return s;
}

internal static slice<byte> trimRightASCII(slice<byte> s, ж<asciiSet> Ꮡas) {
    ref var @as = ref Ꮡas.DerefOrNull();

    while (len(s) > 0) {
        if (!@as.contains(s[len(s) - 1])) {
            break;
        }
        s = s[..(int)(len(s) - 1)];
    }
    return s;
}

internal static slice<byte> trimRightUnicode(slice<byte> s, @string cutset) {
    while (len(s) > 0) {
        var r = (rune)s[len(s) - 1];
        nint n = 1;
        if (r >= utf8.RuneSelf) {
            (r, n) = utf8.DecodeLastRune(s);
        }
        if (!containsRune(cutset, r)) {
            break;
        }
        s = s[..(int)(len(s) - n)];
    }
    return s;
}

// TrimSpace returns a subslice of s by slicing off all leading and
// trailing white space, as defined by Unicode.
public static slice<byte> TrimSpace(slice<byte> s) {
    // Fast path for ASCII: look for the first ASCII non-space byte
    nint start = 0;
    for (; start < len(s); start++) {
        var c = s[start];
        if (c >= utf8.RuneSelf) {
            // If we run into a non-ASCII byte, fall back to the
            // slower unicode-aware method on the remaining bytes
            return TrimFunc(s[(int)(start)..], Δunicode.IsSpace);
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    }
    // Now look for the first ASCII non-space byte from the end
    nint stop = len(s);
    for (; stop > start; stop--) {
        var c = s[stop - 1];
        if (c >= utf8.RuneSelf) {
            return TrimFunc(s[(int)(start)..(int)(stop)], Δunicode.IsSpace);
        }
        if (asciiSpace[c] == 0) {
            break;
        }
    }
    // At this point s[start:stop] starts and ends with an ASCII
    // non-space bytes, so we're done. Non-ASCII cases have already
    // been handled above.
    if (start == stop) {
        // Special case to preserve previous TrimLeftFunc behavior,
        // returning nil instead of empty slice if all spaces.
        return default!;
    }
    return s[(int)(start)..(int)(stop)];
}

// Runes interprets s as a sequence of UTF-8-encoded code points.
// It returns a slice of runes (Unicode code points) equivalent to s.
public static slice<rune> Runes(slice<byte> s) {
    var t = new slice<rune>(utf8.RuneCount(s));
    nint i = 0;
    while (len(s) > 0) {
        var (r, l) = utf8.DecodeRune(s);
        t[i] = r;
        i++;
        s = s[(int)(l)..];
    }
    return t;
}

// Replace returns a copy of the slice s with the first n
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the slice
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune slice.
// If n < 0, there is no limit on the number of replacements.
public static slice<byte> Replace(slice<byte> s, slice<byte> old, slice<byte> @new, nint n) {
    nint m = 0;
    if (n != 0) {
        // Compute number of replacements.
        m = Count(s, old);
    }
    if (m == 0) {
        // Just return a copy.
        return appendꓸꓸꓸ(slice<byte>(default!), s);
    }
    if (n < 0 || m < n) {
        n = m;
    }
    // Apply replacements to buffer.
    var t = new slice<byte>(len(s) + n * (len(@new) - len(old)));
    nint w = 0;
    nint start = 0;
    for (nint i = 0; i < n; i++) {
        nint j = start;
        if (len(old) == 0){
            if (i > 0) {
                var (_, wid) = utf8.DecodeRune(s[(int)(start)..]);
                j += wid;
            }
        } else {
            j += Index(s[(int)(start)..], old);
        }
        w += copy(t[(int)(w)..], s[(int)(start)..(int)(j)]);
        w += copy(t[(int)(w)..], @new);
        start = j + len(old);
    }
    w += copy(t[(int)(w)..], s[(int)(start)..]);
    return t[0..(int)(w)];
}

// ReplaceAll returns a copy of the slice s with all
// non-overlapping instances of old replaced by new.
// If old is empty, it matches at the beginning of the slice
// and after each UTF-8 sequence, yielding up to k+1 replacements
// for a k-rune slice.
public static slice<byte> ReplaceAll(slice<byte> s, slice<byte> old, slice<byte> @new) {
    return Replace(s, old, @new, -1);
}

// EqualFold reports whether s and t, interpreted as UTF-8 strings,
// are equal under simple Unicode case-folding, which is a more general
// form of case-insensitivity.
public static bool EqualFold(slice<byte> s, slice<byte> t) {
    // ASCII fast path
    nint i = 0;
    for (; i < len(s) && i < len(t); i++) {
        var sr = s[i];
        var tr = t[i];
        if ((byte)(sr | tr) >= utf8.RuneSelf) {
            goto hasUnicode;
        }
        // Easy case.
        if (tr == sr) {
            continue;
        }
        // Make sr < tr to simplify what follows.
        if (tr < sr) {
            (tr, sr) = (sr, tr);
        }
        // ASCII only, sr/tr must be upper/lower case
        if ((rune)'A' <= sr && sr <= (rune)'Z' && tr == (byte)(sr + (rune)'a' - (rune)'A')) {
            continue;
        }
        return false;
    }
    // Check if we've exhausted both strings.
    return len(s) == len(t);
hasUnicode:
    s = s[(int)(i)..];
    t = t[(int)(i)..];
    while (len(s) != 0 && len(t) != 0) {
        // Extract first rune from each.
        rune sr = default!;
        rune tr = default!;
        if (s[0] < utf8.RuneSelf){
            (sr, s) = ((rune)s[0], s[1..]);
        } else {
            var (rΔ1, size) = utf8.DecodeRune(s);
            (sr, s) = (rΔ1, s[(int)(size)..]);
        }
        if (t[0] < utf8.RuneSelf){
            (tr, t) = ((rune)t[0], t[1..]);
        } else {
            var (rΔ2, size) = utf8.DecodeRune(t);
            (tr, t) = (rΔ2, t[(int)(size)..]);
        }
        // If they match, keep going; if not, return false.
        // Easy case.
        if (tr == sr) {
            continue;
        }
        // Make sr < tr to simplify what follows.
        if (tr < sr) {
            (tr, sr) = (sr, tr);
        }
        // Fast check for ASCII.
        if (tr < utf8.RuneSelf) {
            // ASCII only, sr/tr must be upper/lower case
            if ((rune)'A' <= sr && sr <= (rune)'Z' && tr == sr + (rune)'a' - (rune)'A') {
                continue;
            }
            return false;
        }
        // General case. SimpleFold(x) returns the next equivalent rune > x
        // or wraps around to smaller values.
        var r = Δunicode.SimpleFold(sr);
        while (r != sr && r < tr) {
            r = Δunicode.SimpleFold(r);
        }
        if (r == tr) {
            continue;
        }
        return false;
    }
    // One string is empty. Are both?
    return len(s) == len(t);
}

// Index returns the index of the first instance of sep in s, or -1 if sep is not present in s.
public static nint Index(slice<byte> s, slice<byte> sep) {
    nint n = len(sep);
    switch (ᐧ) {
    case {} when n is 0: {
        return 0;
    }
    case {} when n is 1: {
        return IndexByte(s, sep[0]);
    }
    case {} when n == len(s): {
        if (Equal(sep, s)) {
            return 0;
        }
        return -1;
    }
    case {} when n > len(s): {
        return -1;
    }
    case {} when n <= bytealg.MaxLen: {
        if (len(s) <= bytealg.MaxBruteForce) {
            // Use brute force when s and sep both are small
            return bytealg.Index(s, sep);
        }
        var c0Δ2 = sep[0];
        var c1Δ2 = sep[1];
        nint iΔ2 = 0;
        nint tΔ2 = len(s) - n + 1;
        nint failsΔ2 = 0;
        while (iΔ2 < tΔ2) {
            if (s[iΔ2] != c0Δ2) {
                // IndexByte is faster than bytealg.Index, so use it as long as
                // we're not getting lots of false positives.
                nint o = IndexByte(s[(int)(iΔ2 + 1)..(int)(tΔ2)], c0Δ2);
                if (o < 0) {
                    return -1;
                }
                iΔ2 += o + 1;
            }
            if (s[iΔ2 + 1] == c1Δ2 && Equal(s[(int)(iΔ2)..(int)(iΔ2 + n)], sep)) {
                return iΔ2;
            }
            failsΔ2++;
            iΔ2++;
            // Switch to bytealg.Index when IndexByte produces too many false positives.
            if (failsΔ2 > bytealg.Cutover(iΔ2)) {
                nint r = bytealg.Index(s[(int)(iΔ2)..], sep);
                if (r >= 0) {
                    return r + iΔ2;
                }
                return -1;
            }
        }
        return -1;
    }}

    var c0 = sep[0];
    var c1 = sep[1];
    nint i = 0;
    nint fails = 0;
    nint t = len(s) - n + 1;
    while (i < t) {
        if (s[i] != c0) {
            nint o = IndexByte(s[(int)(i + 1)..(int)(t)], c0);
            if (o < 0) {
                break;
            }
            i += o + 1;
        }
        if (s[i + 1] == c1 && Equal(s[(int)(i)..(int)(i + n)], sep)) {
            return i;
        }
        i++;
        fails++;
        if (fails >= 4 + (i >> (int)(4)) && i < t) {
            // Give up on IndexByte, it isn't skipping ahead
            // far enough to be better than Rabin-Karp.
            // Experiments (using IndexPeriodic) suggest
            // the cutover is about 16 byte skips.
            // TODO: if large prefixes of sep are matching
            // we should cutover at even larger average skips,
            // because Equal becomes that much more expensive.
            // This code does not take that effect into account.
            nint j = bytealg.IndexRabinKarp(s[(int)(i)..], sep);
            if (j < 0) {
                return -1;
            }
            return i + j;
        }
    }
    return -1;
}

// Cut slices s around the first instance of sep,
// returning the text before and after sep.
// The found result reports whether sep appears in s.
// If sep does not appear in s, cut returns s, nil, false.
//
// Cut returns slices of the original slice s, not copies.
public static (slice<byte> before, slice<byte> after, bool found) Cut(slice<byte> s, slice<byte> sep) {
    {
        nint i = Index(s, sep); if (i >= 0) {
            return (s[..(int)(i)], s[(int)(i + len(sep))..], true);
        }
    }
    return (s, default!, false);
}

// Clone returns a copy of b[:len(b)].
// The result may have additional unused capacity.
// Clone(nil) returns nil.
public static slice<byte> Clone(slice<byte> b) {
    if (b == default!) {
        return default!;
    }
    return appendꓸꓸꓸ(new byte[]{}.slice(), b);
}

// CutPrefix returns s without the provided leading prefix byte slice
// and reports whether it found the prefix.
// If s doesn't start with prefix, CutPrefix returns s, false.
// If prefix is the empty byte slice, CutPrefix returns s, true.
//
// CutPrefix returns slices of the original slice s, not copies.
public static (slice<byte> after, bool found) CutPrefix(slice<byte> s, slice<byte> prefix) {
    if (!HasPrefix(s, prefix)) {
        return (s, false);
    }
    return (s[(int)(len(prefix))..], true);
}

// CutSuffix returns s without the provided ending suffix byte slice
// and reports whether it found the suffix.
// If s doesn't end with suffix, CutSuffix returns s, false.
// If suffix is the empty byte slice, CutSuffix returns s, true.
//
// CutSuffix returns slices of the original slice s, not copies.
public static (slice<byte> before, bool found) CutSuffix(slice<byte> s, slice<byte> suffix) {
    if (!HasSuffix(s, suffix)) {
        return (s, false);
    }
    return (s[..(int)(len(s) - len(suffix))], true);
}

} // end bytes_package
