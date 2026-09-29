// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class bytes_package {

// Lines returns an iterator over the newline-terminated lines in the byte slice s.
// The lines yielded by the iterator include their terminating newlines.
// If s is empty, the iterator yields no lines at all.
// If s does not end in a newline, the final yielded line will not end in a newline.
// It returns a single-use iterator.
public static iter.Seq<slice<byte>> Lines(slice<byte> s) {
    return (Func<slice<byte>, bool> yield) => {
        while (len(s) > 0) {
            slice<byte> line = default!;
            {
                nint i = IndexByte(s, (rune)'\n'); if (i >= 0){
                    (line, s) = (s.slice(0, i + 1), s.slice(i + 1));
                } else {
                    (line, s) = (s, default!);
                }
            }
            if (!yield(line.slice(0, len(line), len(line)))) {
                return;
            }
        }
        return;
    };
}

// explodeSeq returns an iterator over the runes in s.
internal static iter.Seq<slice<byte>> explodeSeq(slice<byte> s) {
    return (Func<slice<byte>, bool> yield) => {
        while (len(s) > 0) {
            var (_, size) = utf8.DecodeRune(s);
            if (!yield(s.slice(0, size, size))) {
                return;
            }
            s = s.slice(size);
        }
    };
}

// splitSeq is SplitSeq or SplitAfterSeq, configured by how many
// bytes of sep to include in the results (none or all).
internal static iter.Seq<slice<byte>> splitSeq(slice<byte> s, slice<byte> sep, nint sepSave) {
    if (len(sep) == 0) {
        return explodeSeq(s);
    }
    var sepʗ1 = sep;
    return (Func<slice<byte>, bool> yield) => {
        while (ᐧ) {
            nint i = Index(s, sepʗ1);
            if (i < 0) {
                break;
            }
            var frag = s.slice(0, i + sepSave);
            if (!yield(frag.slice(0, len(frag), len(frag)))) {
                return;
            }
            s = s.slice(i + len(sepʗ1));
        }
        yield(s.slice(0, len(s), len(s)));
    };
}

// SplitSeq returns an iterator over all subslices of s separated by sep.
// The iterator yields the same subslices that would be returned by [Split](s, sep),
// but without constructing a new slice containing the subslices.
// It returns a single-use iterator.
public static iter.Seq<slice<byte>> SplitSeq(slice<byte> s, slice<byte> sep) {
    return splitSeq(s, sep, 0);
}

// SplitAfterSeq returns an iterator over subslices of s split after each instance of sep.
// The iterator yields the same subslices that would be returned by [SplitAfter](s, sep),
// but without constructing a new slice containing the subslices.
// It returns a single-use iterator.
public static iter.Seq<slice<byte>> SplitAfterSeq(slice<byte> s, slice<byte> sep) {
    return splitSeq(s, sep, len(sep));
}

// FieldsSeq returns an iterator over subslices of s split around runs of
// whitespace characters, as defined by [unicode.IsSpace].
// The iterator yields the same subslices that would be returned by [Fields](s),
// but without constructing a new slice containing the subslices.
public static iter.Seq<slice<byte>> FieldsSeq(slice<byte> s) {
    var sʗ1 = s;
    return (Func<slice<byte>, bool> yield) => {
        nint start = -1;
        for (nint i = 0; i < len(sʗ1); ) {
            nint size = 1;
            var r = (rune)sʗ1[i];
            var isSpace = asciiSpace[sʗ1[i]] != 0;
            if (r >= utf8.RuneSelf) {
                (r, size) = utf8.DecodeRune(sʗ1.slice(i));
                isSpace = Δunicode.IsSpace(r);
            }
            if (isSpace){
                if (start >= 0) {
                    if (!yield(sʗ1.slice(start, i, i))) {
                        return;
                    }
                    start = -1;
                }
            } else 
            if (start < 0) {
                start = i;
            }
            i += size;
        }
        if (start >= 0) {
            yield(sʗ1.slice(start, len(sʗ1), len(sʗ1)));
        }
    };
}

// FieldsFuncSeq returns an iterator over subslices of s split around runs of
// Unicode code points satisfying f(c).
// The iterator yields the same subslices that would be returned by [FieldsFunc](s),
// but without constructing a new slice containing the subslices.
public static iter.Seq<slice<byte>> FieldsFuncSeq(slice<byte> s, Func<rune, bool> f) {
    var sʗ1 = s;
    return (Func<slice<byte>, bool> yield) => {
        nint start = -1;
        for (nint i = 0; i < len(sʗ1); ) {
            nint size = 1;
            var r = (rune)sʗ1[i];
            if (r >= utf8.RuneSelf) {
                (r, size) = utf8.DecodeRune(sʗ1.slice(i));
            }
            if (f(r)){
                if (start >= 0) {
                    if (!yield(sʗ1.slice(start, i, i))) {
                        return;
                    }
                    start = -1;
                }
            } else 
            if (start < 0) {
                start = i;
            }
            i += size;
        }
        if (start >= 0) {
            yield(sʗ1.slice(start, len(sʗ1), len(sʗ1)));
        }
    };
}

} // end bytes_package
