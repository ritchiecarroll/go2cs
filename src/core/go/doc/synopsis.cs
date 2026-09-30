// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using comment = global::go.go.doc.comment_package;
using strings = strings_package;
using unicode = unicode_package;
using global::go.go.doc;

partial class doc_package {

// firstSentence returns the first sentence in s.
// The sentence ends after the first period followed by space and
// not preceded by exactly one uppercase letter.
internal static @string firstSentence(@string s) {
    rune ppp = default!;
    rune pp = default!;
    rune p = default!;
    foreach (var (i, rᴛ1) in s) {
        var q = rᴛ1;

        if (q == (rune)'\n' || q == (rune)'\r' || q == (rune)'\t') {
            q = (rune)' ';
        }
        if (q == (rune)' ' && p == (rune)'.' && (!unicode.IsUpper(pp) || unicode.IsUpper(ppp))) {
            return s.slice(0, i);
        }
        if (p == (rune)'。' || p == (rune)'．') {
            return s.slice(0, i);
        }
        (ppp, pp, p) = (pp, p, q);
    }
    return s;
}

// Synopsis returns a cleaned version of the first sentence in text.
//
// Deprecated: New programs should use [Package.Synopsis] instead,
// which handles links in text properly.
public static @string Synopsis(@string text) {
    ref var p = ref heap(new Package(), out var Ꮡp);
    return Ꮡp.Synopsis(text);
}

// IllegalPrefixes is a list of lower-case prefixes that identify
// a comment as not being a doc comment.
// This helps to avoid misinterpreting the common mistake
// of a copyright notice immediately before a package statement
// as being a doc comment.
public static slice<@string> IllegalPrefixes = new @string[]{
    "copyright"u8,
    "all rights"u8,
    "author"u8
}.slice();

// Synopsis returns a cleaned version of the first sentence in text.
// That sentence ends after the first period followed by space and not
// preceded by exactly one uppercase letter, or at the first paragraph break.
// The result string has no \n, \r, or \t characters and uses only single
// spaces between words. If text starts with any of the [IllegalPrefixes],
// the result is the empty string.
public static @string Synopsis(this ж<Package> Ꮡp, @string text) {
    ref var p = ref Ꮡp.DerefOrNull();

    text = firstSentence(text);
    @string lower = strings.ToLower(text);
    foreach (var (_, prefix) in IllegalPrefixes) {
        if (strings.HasPrefix(lower, prefix)) {
            return ""u8;
        }
    }
    var pr = p.Printer();
    pr.Value.TextWidth = -1;
    var d = Ꮡp.Parser().Parse(text);
    if (len((~d).Content) == 0) {
        return ""u8;
    }
    {
        var (_, ok) = (~d).Content[0]._<ж<comment.Paragraph>>(ᐧ); if (!ok) {
            return ""u8;
        }
    }
    d.Value.Content = (~d).Content[..1]; // might be blank lines, code blocks, etc in “first sentence”
    return strings.TrimSpace(((@string)pr.Text(d)));
}

} // end doc_package
