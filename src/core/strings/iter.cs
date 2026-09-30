// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class strings_package {

// Lines returns an iterator over the newline-terminated lines in the string s.
// The lines yielded by the iterator include their terminating newlines.
// If s is empty, the iterator yields no lines at all.
// If s does not end in a newline, the final yielded line will not end in a newline.
// It returns a single-use iterator.
public static iter.Seq<@string> Lines(@string s) {
    return (Func<@string, bool> yield) => {
        while (len(s) > 0) {
            @string line = default!;
            {
                nint i = IndexByte(s, (rune)'\n'); if (i >= 0){
                    (line, s) = (s.slice(0, i + 1), s.slice(i + 1));
                } else {
                    (line, s) = (s, "");
                }
            }
            if (!yield(line)) {
                return;
            }
        }
        return;
    };
}

// explodeSeq returns an iterator over the runes in s.
internal static iter.Seq<@string> explodeSeq(@string s) {
    return (Func<@string, bool> yield) => {
        while (len(s) > 0) {
            var (_, size) = utf8.DecodeRuneInString(s);
            if (!yield(s.slice(0, size))) {
                return;
            }
            s = s.slice(size);
        }
    };
}

// splitSeq is SplitSeq or SplitAfterSeq, configured by how many
// bytes of sep to include in the results (none or all).
internal static iter.Seq<@string> splitSeq(@string s, @string sep, nint sepSave) {
    if (len(sep) == 0) {
        return explodeSeq(s);
    }
    return (Func<@string, bool> yield) => {
        while (ᐧ) {
            nint i = Index(s, sep);
            if (i < 0) {
                break;
            }
            @string frag = s.slice(0, i + sepSave);
            if (!yield(frag)) {
                return;
            }
            s = s.slice(i + len(sep));
        }
        yield(s);
    };
}

// SplitSeq returns an iterator over all substrings of s separated by sep.
// The iterator yields the same strings that would be returned by [Split](s, sep),
// but without constructing the slice.
// It returns a single-use iterator.
public static iter.Seq<@string> SplitSeq(@string s, @string sep) {
    return splitSeq(s, sep, 0);
}

// SplitAfterSeq returns an iterator over substrings of s split after each instance of sep.
// The iterator yields the same strings that would be returned by [SplitAfter](s, sep),
// but without constructing the slice.
// It returns a single-use iterator.
public static iter.Seq<@string> SplitAfterSeq(@string s, @string sep) {
    return splitSeq(s, sep, len(sep));
}

// FieldsSeq returns an iterator over substrings of s split around runs of
// whitespace characters, as defined by [unicode.IsSpace].
// The iterator yields the same strings that would be returned by [Fields](s),
// but without constructing the slice.
public static iter.Seq<@string> FieldsSeq(@string s) {
    return (Func<@string, bool> yield) => {
        nint start = -1;
        for (nint i = 0; i < len(s); ) {
            nint size = 1;
            var r = (rune)s[i];
            var isSpace = asciiSpace[s[i]] != 0;
            if (r >= utf8.RuneSelf) {
                (r, size) = utf8.DecodeRuneInString(s.slice(i));
                isSpace = Δunicode.IsSpace(r);
            }
            if (isSpace){
                if (start >= 0) {
                    if (!yield(s.slice(start, i))) {
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
            yield(s.slice(start));
        }
    };
}

// FieldsFuncSeq returns an iterator over substrings of s split around runs of
// Unicode code points satisfying f(c).
// The iterator yields the same strings that would be returned by [FieldsFunc](s),
// but without constructing the slice.
public static iter.Seq<@string> FieldsFuncSeq(@string s, Func<rune, bool> f) {
    return (Func<@string, bool> yield) => {
        nint start = -1;
        for (nint i = 0; i < len(s); ) {
            nint size = 1;
            var r = (rune)s[i];
            if (r >= utf8.RuneSelf) {
                (r, size) = utf8.DecodeRuneInString(s.slice(i));
            }
            if (f(r)){
                if (start >= 0) {
                    if (!yield(s.slice(start, i))) {
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
            yield(s.slice(start));
        }
    };
}

} // end strings_package
