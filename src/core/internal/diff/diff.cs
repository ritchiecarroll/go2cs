// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using io = io_package;

partial class diff_package {

// A pair is a pair of values tracked for both the x and y side of a diff.
// It is typically a pair of line indexes.
[GoType] partial struct pair {
    internal nint x, y;
}

// Diff returns an anchored diff of the two texts old and new
// in the “unified diff” format. If old and new are identical,
// Diff returns a nil slice (no output).
//
// Unix diff implementations typically look for a diff with
// the smallest number of lines inserted and removed,
// which can in the worst case take time quadratic in the
// number of lines in the texts. As a result, many implementations
// either can be made to run for a long time or cut off the search
// after a predetermined amount of work.
//
// In contrast, this implementation looks for a diff with the
// smallest number of “unique” lines inserted and removed,
// where unique means a line that appears just once in both old and new.
// We call this an “anchored diff” because the unique lines anchor
// the chosen matching regions. An anchored diff is usually clearer
// than a standard diff, because the algorithm does not try to
// reuse unrelated blank lines or closing braces.
// The algorithm also guarantees to run in O(n log n) time
// instead of the standard O(n²) time.
//
// Some systems call this approach a “patience diff,” named for
// the “patience sorting” algorithm, itself named for a solitaire card game.
// We avoid that name for two reasons. First, the name has been used
// for a few different variants of the algorithm, so it is imprecise.
// Second, the name is frequently interpreted as meaning that you have
// to wait longer (to be patient) for the diff, meaning that it is a slower algorithm,
// when in fact the algorithm is faster than the standard one.
public static slice<byte> Diff(@string oldName, slice<byte> old, @string newName, slice<byte> @new) {
    if (bytes.Equal(old, @new)) {
        return default!;
    }
    var x = lines(old);
    var y = lines(@new);
    // Print diff header.
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "diff %s %s\n"u8, oldName, newName);
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "--- %s\n"u8, oldName);
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "+++ %s\n"u8, newName);
    // Loop over matches to consider,
    // expanding each match to include surrounding lines,
    // and then printing diff chunks.
    // To avoid setup/teardown cases outside the loop,
    // tgs returns a leading {0,0} and trailing {len(x), len(y)} pair
    // in the sequence of matches.
    pair done = default!;           // printed up to x[:done.x] and y[:done.y]
    
    pair chunk = default!;         // start lines of current chunk
    
    pair count = default!;         // number of lines from each side in current chunk
    
    slice<@string> ctext = default!;             // lines for current chunk
    foreach (var (_, m) in tgs(x, y)) {
        if (m.x < done.x) {
            // Already handled scanning forward from earlier match.
            continue;
        }
        // Expand matching lines as far as possible,
        // establishing that x[start.x:end.x] == y[start.y:end.y].
        // Note that on the first (or last) iteration we may (or definitely do)
        // have an empty match: start.x==end.x and start.y==end.y.
        var start = m;
        while (start.x > done.x && start.y > done.y && x[start.x - 1] == y[start.y - 1]) {
            start.x--;
            start.y--;
        }
        var end = m;
        while (end.x < len(x) && end.y < len(y) && x[end.x] == y[end.y]) {
            end.x++;
            end.y++;
        }
        // Emit the mismatched lines before start into this chunk.
        // (No effect on first sentinel iteration, when start = {0,0}.)
        foreach (var (_, s) in x.slice(done.x, start.x)) {
            ctext = append(ctext, "-"u8 + s);
            count.x++;
        }
        foreach (var (_, s) in y.slice(done.y, start.y)) {
            ctext = append(ctext, "+"u8 + s);
            count.y++;
        }
        // If we're not at EOF and have too few common lines,
        // the chunk includes all the common lines and continues.
        UntypedInt C = 3; // number of context lines
        if ((end.x < len(x) || end.y < len(y)) && (end.x - start.x < C || (len(ctext) > 0 && end.x - start.x < (nint)(2 * C)))) {
            foreach (var (_, s) in x.slice(start.x, end.x)) {
                ctext = append(ctext, " "u8 + s);
                count.x++;
                count.y++;
            }
            done = end;
            continue;
        }
        // End chunk with common lines for context.
        if (len(ctext) > 0) {
            nint n = end.x - start.x;
            if (n > C) {
                n = C;
            }
            foreach (var (_, s) in x.slice(start.x, start.x + n)) {
                ctext = append(ctext, " "u8 + s);
                count.x++;
                count.y++;
            }
            done = new pair(start.x + n, start.y + n);
            // Format and emit chunk.
            // Convert line numbers to 1-indexed.
            // Special case: empty file shows up as 0,0 not 1,0.
            if (count.x > 0) {
                chunk.x++;
            }
            if (count.y > 0) {
                chunk.y++;
            }
            fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "@@ -%d,%d +%d,%d @@\n"u8, chunk.x, count.x, chunk.y, count.y);
            foreach (var (_, s) in ctext) {
                @out.WriteString(s);
            }
            count.x = 0;
            count.y = 0;
            ctext = ctext[..0];
        }
        // If we reached EOF, we're done.
        if (end.x >= len(x) && end.y >= len(y)) {
            break;
        }
        // Otherwise start a new chunk.
        chunk = new pair(end.x - (nint)C, end.y - (nint)C);
        foreach (var (_, s) in x.slice(chunk.x, end.x)) {
            ctext = append(ctext, " "u8 + s);
            count.x++;
            count.y++;
        }
        done = end;
    }
    return @out.Bytes();
}

// lines returns the lines in the file x, including newlines.
// If the file does not end in a newline, one is supplied
// along with a warning about the missing newline.
internal static slice<@string> lines(slice<byte> x) {
    var l = strings.SplitAfter(((@string)x), "\n"u8);
    if (l[len(l) - 1] == ""){
        l = l.slice(0, len(l) - 1);
    } else {
        // Treat last line as having a message about the missing newline attached,
        // using the same text as BSD/GNU diff (including the leading backslash).
        l[len(l) - 1] += "\n\\ No newline at end of file\n"u8;
    }
    return l;
}

// tgs returns the pairs of indexes of the longest common subsequence
// of unique lines in x and y, where a unique line is one that appears
// once in x and once in y.
//
// The longest common subsequence algorithm is as described in
// Thomas G. Szymanski, “A Special Case of the Maximal Common
// Subsequence Problem,” Princeton TR #170 (January 1975),
// available at https://research.swtch.com/tgs170.pdf.
internal static slice<pair> tgs(slice<@string> x, slice<@string> y) {
    // Count the number of times each string appears in a and b.
    // We only care about 0, 1, many, counted as 0, -1, -2
    // for the x side and 0, -4, -8 for the y side.
    // Using negative numbers now lets us distinguish positive line numbers later.
    var m = new map<@string, nint>();
    foreach (var (_, s) in x) {
        {
            nint c = m[s]; if (c > -2) {
                m[s] = c - 1;
            }
        }
    }
    foreach (var (_, s) in y) {
        {
            nint c = m[s]; if (c > -8) {
                m[s] = c - 4;
            }
        }
    }
    // Now unique strings can be identified by m[s] = -1+-4.
    //
    // Gather the indexes of those strings in x and y, building:
    //	xi[i] = increasing indexes of unique strings in x.
    //	yi[i] = increasing indexes of unique strings in y.
    //	inv[i] = index j such that x[xi[i]] = y[yi[j]].
    slice<nint> xi = default!;
    slice<nint> yi = default!;
    slice<nint> inv = default!;
    foreach (var (i, s) in y) {
        if (m[s] == -1 + -4) {
            m[s] = len(yi);
            yi = append(yi, i);
        }
    }
    foreach (var (i, s) in x) {
        {
            var (j, ok) = m[s, ꟷ]; if (ok && j >= 0) {
                xi = append(xi, i);
                inv = append(inv, j);
            }
        }
    }
    // Apply Algorithm A from Szymanski's paper.
    // In those terms, A = J = inv and B = [0, n).
    // We add sentinel pairs {0,0}, and {len(x),len(y)}
    // to the returned sequence, to help the processing loop.
    var J = inv;
    nint n = len(xi);
    var T = new slice<nint>(n);
    var L = new slice<nint>(n);
    foreach (var (i, _) in T) {
        T[i] = n + 1;
    }
    for (nint iᴛ1 = 0; iᴛ1 < n; iᴛ1++) {
        var i = iᴛ1;
        var Jʗ1 = J;
        var Tʗ1 = T;
        nint kΔ1 = sort.Search(n, (nint kΔ2) => Tʗ1[kΔ2] >= Jʗ1[i]);
        T[kΔ1] = J[i];
        L[i] = kΔ1 + 1;
    }
    nint k = 0;
    foreach (var (_, v) in L) {
        if (k < v) {
            k = v;
        }
    }
    var seq = new slice<pair>(2 + k);
    seq[1 + k] = new pair(len(x), len(y)); // sentinel at end
    nint lastj = n;
    for (nint i = n - 1; i >= 0; i--) {
        if (L[i] == k && J[i] < lastj) {
            seq[k] = new pair(xi[i], yi[J[i]]);
            k--;
        }
    }
    seq[0] = new pair(0, 0); // sentinel at start
    return seq;
}

} // end diff_package
