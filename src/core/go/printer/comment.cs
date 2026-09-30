// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using comment = global::go.go.doc.comment_package;
using strings = strings_package;
using global::go.go;
using global::go.go.doc;

partial class printer_package {

// formatDocComment reformats the doc comment list,
// returning the canonical formatting.
internal static slice<ж<ast.Comment>> formatDocComment(slice<ж<ast.Comment>> list) {
    // Extract comment text (removing comment markers).
    @string kind = default!;
    @string text = default!;
    slice<ж<ast.Comment>> directives = default!;
    if (len(list) == 1 && strings.HasPrefix((~list[0]).Text, "/*"u8)){
        kind = "/*"u8;
        text = list[0].Value.Text;
        if (!strings.Contains(text, "\n"u8) || allStars(text)) {
            // Single-line /* .. */ comment in doc comment position,
            // or multiline old-style comment like
            //	/*
            //	 * Comment
            //	 * text here.
            //	 */
            // Should not happen, since it will not work well as a
            // doc comment, but if it does, just ignore:
            // reformatting it will only make the situation worse.
            return list;
        }
        text = text.slice(2, len(text) - 2); // cut /* and */
    } else 
    if (strings.HasPrefix((~list[0]).Text, "//"u8)){
        kind = "//"u8;
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        foreach (var (_, c) in list) {
            var (after, found) = strings.CutPrefix((~c).Text, "//"u8);
            if (!found) {
                return list;
            }
            // Accumulate //go:build etc lines separately.
            if (isDirective(after)) {
                directives = append(directives, c);
                continue;
            }
            Ꮡb.WriteString(strings.TrimPrefix(after, " "u8));
            Ꮡb.WriteString("\n"u8);
        }
        text = b.String();
    } else {
        // Not sure what this is, so leave alone.
        return list;
    }
    if (text == ""u8) {
        return list;
    }
    // Parse comment and reformat as text.
    ref var p = ref heap(new comment.Parser(), out var Ꮡp);
    var d = Ꮡp.Parse(text);
    ref var pr = ref heap(new comment.Printer(), out var Ꮡpr);
    text = ((@string)Ꮡpr.Comment(d));
    // For /* */ comment, return one big comment with text inside.
    ref var slash = ref heap<tokenꓸPos>(out var Ꮡslash);
    slash = list[0].Value.Slash;
    if (kind == "/*"u8) {
        var c = Ꮡ(new ast.Comment(
            Slash: slash,
            Text: "/*\n"u8 + text + "*/"u8
        ));
        return new ж<ast.Comment>[]{c}.slice();
    }
    // For // comment, return sequence of // lines.
    slice<ж<ast.Comment>> @out = default!;
    while (text != ""u8) {
        ref var line = ref heap(new @string(), out var Ꮡline);
        (line, text, _) = strings.Cut(text, "\n"u8);
        if (line == ""u8){
            line = "//"u8;
        } else 
        if (strings.HasPrefix(line, "\t"u8)){
            line = "//"u8 + line;
        } else {
            line = "// "u8 + line;
        }
        @out = append(@out, Ꮡ(new ast.Comment(
            Slash: slash,
            Text: line
        )));
    }
    if (len(directives) > 0) {
        @out = append(@out, Ꮡ(new ast.Comment(
            Slash: slash,
            Text: "//"u8
        )));
        foreach (var (_, c) in directives) {
            @out = append(@out, Ꮡ(new ast.Comment(
                Slash: slash,
                Text: (~c).Text
            )));
        }
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lineˢ = "line "u8;
internal static readonly @string externˢ = "extern "u8;
internal static readonly @string exportˢ = "export "u8;

// isDirective reports whether c is a comment directive.
// See go.dev/issue/37974.
// This code is also in go/ast.
internal static bool isDirective(@string c) {
    // "//line " is a line directive.
    // "//extern " is for gccgo.
    // "//export " is for cgo.
    // (The // has been removed.)
    if (strings.HasPrefix(c, lineˢ) || strings.HasPrefix(c, externˢ) || strings.HasPrefix(c, exportˢ)) {
        return true;
    }
    // "//[a-z0-9]+:[a-z0-9]"
    // (The // has been removed.)
    nint colon = strings.Index(c, ":"u8);
    if (colon <= 0 || colon + 1 >= len(c)) {
        return false;
    }
    for (nint i = 0; i <= colon + 1; i++) {
        if (i == colon) {
            continue;
        }
        var b = c[i];
        if (!((rune)'a' <= b && b <= (rune)'z' || (rune)'0' <= b && b <= (rune)'9')) {
            return false;
        }
    }
    return true;
}

// allStars reports whether text is the interior of an
// old-style /* */ comment with a star at the start of each line.
internal static bool allStars(@string text) {
    for (nint i = 0; i < len(text); i++) {
        if (text[i] == (rune)'\n') {
            nint j = i + 1;
            while (j < len(text) && (text[j] == (rune)' ' || text[j] == (rune)'\t')) {
                j++;
            }
            if (j < len(text) && text[j] != (rune)'*') {
                return false;
            }
        }
    }
    return true;
}

} // end printer_package
