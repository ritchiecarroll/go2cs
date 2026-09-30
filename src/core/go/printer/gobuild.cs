// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using constraint = global::go.go.build.constraint_package;
using slices = slices_package;
using tabwriter = text.tabwriter_package;
using global::go.go.build;
using text;

partial class printer_package {

[GoRecv] internal static void fixGoBuildLines(this ref printer p) {
    if (len(p.goBuild) + len(p.plusBuild) == 0) {
        return;
    }
    // Find latest possible placement of //go:build and // +build comments.
    // That's just after the last blank line before we find a non-comment.
    // (We'll add another blank line after our comment block.)
    // When we start dropping // +build comments, we can skip over /* */ comments too.
    // Note that we are processing tabwriter input, so every comment
    // begins and ends with a tabwriter.Escape byte.
    // And some newlines have turned into \f bytes.
    nint insert = 0;
    for (nint pos = 0; ᐧ ; ) {
        // Skip leading space at beginning of line.
        var blank = true;
        while (pos < len(p.output) && (p.output[pos] == (rune)' ' || p.output[pos] == (rune)'\t')) {
            pos++;
        }
        // Skip over // comment if any.
        if (pos + 3 < len(p.output) && p.output[pos] == tabwriter.Escape && p.output[pos + 1] == (rune)'/' && p.output[pos + 2] == (rune)'/') {
            blank = false;
            while (pos < len(p.output) && !isNL(p.output[pos])) {
                pos++;
            }
        }
        // Skip over \n at end of line.
        if (pos >= len(p.output) || !isNL(p.output[pos])) {
            break;
        }
        pos++;
        if (blank) {
            insert = pos;
        }
    }
    // If there is a //go:build comment before the place we identified,
    // use that point instead. (Earlier in the file is always fine.)
    if (len(p.goBuild) > 0 && p.goBuild[0] < insert){
        insert = p.goBuild[0];
    } else 
    if (len(p.plusBuild) > 0 && p.plusBuild[0] < insert) {
        insert = p.plusBuild[0];
    }
    constraint.Expr x = default!;
    switch (len(p.goBuild)) {
    case 0: {
        foreach (var (_, pos) in p.plusBuild) {
            // Synthesize //go:build expression from // +build lines.
            var (y, err) = constraint.Parse(p.commentTextAt(pos));
            if (err != default!) {
                x = default!;
                break;
            }
            if (x == default!){
                x = y;
            } else {
                x = new constraint.AndExprжExpr(Ꮡ(new constraint.AndExpr(X: x, Y: y)));
            }
        }
        break;
    }
    case 1: {
        (x, _) = constraint.Parse(p.commentTextAt(p.goBuild[0]));
        break;
    }}

    // Parse //go:build expression.
    slice<byte> block = default!;
    if (x == default!){
        // Don't have a valid //go:build expression to treat as truth.
        // Bring all the lines together but leave them alone.
        // Note that these are already tabwriter-escaped.
        foreach (var (_, pos) in p.goBuild) {
            block = appendꓸꓸꓸ(block, p.lineAt(pos));
        }
        foreach (var (_, pos) in p.plusBuild) {
            block = appendꓸꓸꓸ(block, p.lineAt(pos));
        }
    } else {
        block = append(block, (byte)(tabwriter.Escape));
        block = append(block, ((@string)"//go:build "u8).ꓸꓸꓸ);
        block = append(block, x.String().ꓸꓸꓸ);
        block = append(block, (byte)(tabwriter.Escape), (byte)((rune)'\n'));
        if (len(p.plusBuild) > 0) {
            var (lines, err) = constraint.PlusBuildLines(x);
            if (err != default!) {
                lines = new @string[]{"// +build error: "u8 + err.Error()}.slice();
            }
            foreach (var (_, line) in lines) {
                block = append(block, (byte)(tabwriter.Escape));
                block = append(block, line.ꓸꓸꓸ);
                block = append(block, (byte)(tabwriter.Escape), (byte)((rune)'\n'));
            }
        }
    }
    block = append(block, (byte)((rune)'\n'));
    // Build sorted list of lines to delete from remainder of output.
    var toDelete = appendꓸꓸꓸ(p.goBuild, p.plusBuild);
    slices.Sort<slice<nint>, nint>(toDelete);
    // Collect output after insertion point, with lines deleted, into after.
    slice<byte> after = default!;
    nint start = insert;
    foreach (var (_, end) in toDelete) {
        if (end < start) {
            continue;
        }
        after = appendLines(after, p.output.slice(start, end));
        start = end + len(p.lineAt(end));
    }
    after = appendLines(after, p.output.slice(start));
    {
        nint n = len(after); if (n >= 2 && isNL(after[n - 1]) && isNL(after[n - 2])) {
            after = after.slice(0, n - 1);
        }
    }
    p.output = p.output.slice(0, insert);
    p.output = appendꓸꓸꓸ(p.output, block);
    p.output = appendꓸꓸꓸ(p.output, after);
}

// appendLines is like append(x, y...)
// but it avoids creating doubled blank lines,
// which would not be gofmt-standard output.
// It assumes that only whole blocks of lines are being appended,
// not line fragments.
internal static slice<byte> appendLines(slice<byte> x, slice<byte> y) {
    if (len(y) > 0 && isNL(y[0]) && (len(x) == 0 || len(x) >= 2 && isNL(x[len(x) - 1]) && isNL(x[len(x) - 2]))) {
        // y starts in blank line
        // x is empty or ends in blank line
        y = y[1..]; // delete y's leading blank line
    }
    return appendꓸꓸꓸ(x, y);
}

[GoRecv] internal static slice<byte> lineAt(this ref printer p, nint start) {
    nint pos = start;
    while (pos < len(p.output) && !isNL(p.output[pos])) {
        pos++;
    }
    if (pos < len(p.output)) {
        pos++;
    }
    return p.output.slice(start, pos);
}

[GoRecv] internal static @string commentTextAt(this ref printer p, nint start) {
    if (start < len(p.output) && p.output[start] == tabwriter.Escape) {
        start++;
    }
    nint pos = start;
    while (pos < len(p.output) && p.output[pos] != tabwriter.Escape && !isNL(p.output[pos])) {
        pos++;
    }
    return ((@string)(p.output.slice(start, pos)));
}

internal static bool isNL(byte b) {
    return b == (rune)'\n' || b == (rune)'\f';
}

} // end printer_package
