// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

partial struct comment {
    internal nint line, col;   // comment position
    internal @string text; // comment text, excluding "//", "/*", or "*/"
}

// commentMap collects all comments in the given src with comment text
// that matches the supplied regular expression rx and returns them as
// []comment lists in a map indexed by line number. The comment text is
// the comment with any comment markers ("//", "/*", or "*/") stripped.
// The position for each comment is the position of the token immediately
// preceding the comment, with all comments that are on the same line
// collected in a slice, in source order. If there is no preceding token
// (the matching comment appears at the beginning of the file), then the
// recorded position is unknown (line, col = 0, 0).
// If there are no matching comments, the result is nil.
internal static map<nint, slice<comment>> /*res*/ commentMap(slice<byte> src, ж<regexp.Regexp> Ꮡrx) {
    map<nint, slice<comment>> res = default!;

    var fset = token.NewFileSet();
    var @file = fset.AddFile(""u8, -1, len(src));
    scanner.Scanner s = default!;
    s.Init(@file, src, default!, scanner.ScanComments);
    tokenꓸPos prev = default!;                // position of last non-comment, non-semicolon token
    while (ᐧ) {
        var (pos, tok, lit) = s.Scan();
        var exprᴛ1 = tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == token.EOF) { matchᴛ1 = true;
            return res;
        }
        if (exprᴛ1 == token.COMMENT) { matchᴛ1 = true;
            if (lit[1] == (rune)'*') {
                lit = lit.slice(0, len(lit) - 2); // strip trailing */
            }
            lit = lit[2..]; // strip leading // or /*
            if (Ꮡrx.MatchString(lit)) {
                var p = fset.Position(prev);
                var err = new comment(p.Line, p.Column, lit);
                if (res == default!) {
                    res = new map<nint, slice<comment>>();
                }
                res[p.Line] = append(res[p.Line], err);
            }
        }
        else if (exprᴛ1 == token.SEMICOLON) { matchᴛ1 = true;
            if (lit == "\n"u8) {
                // ignore automatically inserted semicolon
                continue;
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            prev = pos;
        }

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ2 = "^ ERROR "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ14 = """
/* ERROR "0:0" */ /* ERROR "0:0" */ // ERROR "0:0"
// ERROR "0:0"
x /* ERROR "3:1" */                // ignore automatically inserted semicolon here
/* ERROR "3:1" */                  // position of x on previous line
   x /* ERROR "5:4" */ ;           // do not ignore this semicolon
/* ERROR "5:24" */                 // position of ; on previous line
	package /* ERROR "7:2" */  // indented with tab
        import  /* ERROR "8:9" */  // indented with blanks

"""u8;

public static void TestCommentMap(ж<testing.T> Ꮡt) {
    @string src = srcᶜ14;
    var m = commentMap(slice<byte>(src), regexp.MustCompile(errorˢ2));
    nint found = 0; // number of errors found
    foreach (var (line, errlist) in m) {
        foreach (var (_, err) in errlist) {
            if (err.line != line) {
                Ꮡt.Errorf("%v: got map line %d; want %d"u8, err, err.line, line);
                continue;
            }
            // err.line == line
            @string got = strings.TrimSpace(err.text[(int)(len(" ERROR "))..]);
            @string wantΔ1 = fmt.Sprintf(@"""%d:%d"""u8, line, err.col);
            if (got != wantΔ1) {
                Ꮡt.Errorf("%v: got msg %q; want %q"u8, err, got, wantΔ1);
                continue;
            }
            found++;
        }
    }
    nint want = strings.Count(src, errorˢ);
    if (found != want) {
        Ꮡt.Errorf("commentMap got %d errors; want %d"u8, found, want);
    }
}

} // end types_test_package
