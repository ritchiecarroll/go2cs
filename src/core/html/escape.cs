// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package html provides functions for escaping and unescaping HTML text.
namespace go;

using strings = strings_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class html_package {

// These replacements permit compatibility with old numeric entities that
// assumed Windows-1252 encoding.
// https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state
[GoLazy] partial class lazyᴛreplacementTable { internal static array<rune> replacementTable = new rune[]{
    '\u20AC', // First entry is what 0x80 should be replaced with.
    '\u0081',
    '\u201A',
    '\u0192',
    '\u201E',
    '\u2026',
    '\u2020',
    '\u2021',
    '\u02C6',
    '\u2030',
    '\u0160',
    '\u2039',
    '\u0152',
    '\u008D',
    '\u017D',
    '\u008F',
    '\u0090',
    '\u2018',
    '\u2019',
    '\u201C',
    '\u201D',
    '\u2022',
    '\u2013',
    '\u2014',
    '\u02DC',
    '\u2122',
    '\u0161',
    '\u203A',
    '\u0153',
    '\u009D',
    '\u017E',
    '\u0178' // Last entry is 0x9F.
    // 0x00->'\uFFFD' is handled programmatically.
    // 0x0D->'\u000D' is a no-op.
}.array(); }

// unescapeEntity reads an entity like "&lt;" from b[src:] and writes the
// corresponding "<" to b[dst:], returning the incremented dst and src cursors.
// Precondition: b[src] == '&' && dst <= src.
internal static (nint dst1, nint src1) unescapeEntity(slice<byte> b, nint dst, nint src, map<@string, rune> entity, map<@string, array<rune>> entity2) {
    nint dst1 = default;
    nint src1 = default;

    const bool attribute = false;

    // http://www.whatwg.org/specs/web-apps/current-work/multipage/tokenization.html#consume-a-character-reference

    // i starts at 1 because we already know that s[0] == '&'.
    nint i = 1;
    var s = b.slice(src);

    if (len(s) <= 1) {
        b[dst] = b[src];
        return (dst + 1, src + 1);
    }

    if (s[i] == '#') {
        if (len(s) <= 3) { // We need to have at least "&#.".
            b[dst] = b[src];
            return (dst + 1, src + 1);
        }
        i++;
        var c = s[i];
        var hex = false;
        if (c == 'x' || c == 'X') {
            hex = true;
            i++;
        }

        var x = (rune)'\x00';
        while (i < len(s)) {
            c = s[i];
            i++;
            if (hex) {
                if ('0' <= c && c <= '9') {
                    x = 16 * x + (rune)c - '0';
                    continue;
                } else if ('a' <= c && c <= 'f') {
                    x = 16 * x + (rune)c - 'a' + 10;
                    continue;
                } else if ('A' <= c && c <= 'F') {
                    x = 16 * x + (rune)c - 'A' + 10;
                    continue;
                }
            } else if ('0' <= c && c <= '9') {
                x = 10 * x + (rune)c - '0';
                continue;
            }
            if (c != ';') {
                i--;
            }
            break;
        }

        if (i <= 3) { // No characters matched.
            b[dst] = b[src];
            return (dst + 1, src + 1);
        }

        if (0x80 <= x && x <= 0x9F) {
            // Replace characters from Windows-1252 with UTF-8 equivalents.
            x = replacementTable[x - 0x80];
        } else if (x == 0 || (0xD800 <= x && x <= 0xDFFF) || x > 0x10FFFF) {
            // Replace invalid characters with the replacement character.
            x = '\uFFFD';
        }

        return (dst + utf8.EncodeRune(b.slice(dst), x), src + i);
    }

    // Consume the maximum number of characters possible, with the
    // consumed characters matching one of the named references.

    while (i < len(s)) {
        var c = s[i];
        i++;
        // Lower-cased characters are more common in entities, so we check for them first.
        if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9') {
            continue;
        }
        if (c != ';') {
            i--;
        }
        break;
    }

    var entityName = s.slice(1, i);
    if (len(entityName) == 0) {
        // No-op.
    } else if (attribute && entityName[len(entityName) - 1] != ';' && len(s) > i && s[i] == '=') {
        // No-op.
    } else {
        var x = entity[tmpstring(entityName)]; if (x != 0) {
            return (dst + utf8.EncodeRune(b.slice(dst), x), src + i);
        } else {
            var xΔ1 = entity2[tmpstring(entityName), () => new array<rune>(2)].Clone(); if (xΔ1[0] != 0) {
                nint dst1Δ1 = dst + utf8.EncodeRune(b.slice(dst), xΔ1[0]);
                return (dst1Δ1 + utf8.EncodeRune(b.slice(dst1Δ1), xΔ1[1]), src + i);
            } else if (!attribute) {
                nint maxLen = len(entityName) - 1;
                if (maxLen > longestEntityWithoutSemicolon) {
                    maxLen = longestEntityWithoutSemicolon;
                }
                for (nint j = maxLen; j > 1; j--) {
                    {
                        var xΔ2 = entity[tmpstring(entityName.slice(0, j))]; if (xΔ2 != 0) {
                            return (dst + utf8.EncodeRune(b.slice(dst), xΔ2), src + j + 1);
                        }
                    }
                }
            }
        }
    }

    (dst1, src1) = (dst + i, src + i);
    copy(b.slice(dst, dst1), b.slice(src, src1));
    return (dst1, src1);
}

internal static ж<strings.Replacer> htmlEscaper = strings.NewReplacer(
    @"&"u8, "&amp;",
    @"'", "&#39;", // "&#39;" is shorter than "&apos;" and apos was not in HTML until HTML5.
    @"<", "&lt;",
    @">", "&gt;",
    @"""", "&#34;" // "&#34;" is shorter than "&quot;".
);

// EscapeString escapes special characters like "<" to become "&lt;". It
// escapes only five such characters: <, >, &, ' and ".
// [UnescapeString](EscapeString(s)) == s always holds, but the converse isn't
// always true.
public static @string EscapeString(@string s) {
    return htmlEscaper.Replace(s);
}

// UnescapeString unescapes entities like "&lt;" to become "<". It unescapes a
// larger range of entities than [EscapeString] escapes. For example, "&aacute;"
// unescapes to "á", as does "&#225;" and "&#xE1;".
// UnescapeString([EscapeString](s)) == s always holds, but the converse isn't
// always true.
public static @string UnescapeString(@string s) {
    nint i = strings.IndexByte(s, (byte)'&');

    if (i < 0) {
        return s;
    }

    var b = slice<byte>(s);
    var (entity, entity2) = entityMaps();
    var (dst, src) = unescapeEntity(b, i, i, entity, entity2);
    while (len(s.slice(src)) > 0) {
        if (s[src] == '&') {
            i = 0;
        } else {
            i = strings.IndexByte(s.slice(src), (byte)'&');
        }
        if (i < 0) {
            dst += copy(b.slice(dst), s.slice(src));
            break;
        }

        if (i > 0) {
            copy(b.slice(dst), s.slice(src, src + i));
        }
        (dst, src) = unescapeEntity(b, dst + i, src + i, entity, entity2);
    }
    return (@string)(b.slice(0, dst));
}

} // end html_package
