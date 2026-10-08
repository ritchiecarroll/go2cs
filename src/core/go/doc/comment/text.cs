// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;
using io = io_package;

partial class comment_package {

// A textPrinter holds the state needed for printing a Doc as plain text.
partial struct textPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal strings.Builder @long;
    internal @string prefix;
    internal @string codePrefix;
    internal nint width;
}

// Text returns a textual formatting of the [Doc].
// See the [Printer] documentation for ways to customize the text output.
public static slice<byte> Text(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var d = ref Ꮡd.DerefOrNull();

    var tp = Ꮡ(new textPrinter(
        Printer: Ꮡp,
        prefix: p.TextPrefix,
        codePrefix: p.TextCodePrefix,
        width: p.TextWidth
    ));
    if ((~tp).codePrefix == ""u8) {
        tp.Value.codePrefix = p.TextPrefix + "\t"u8;
    }
    if ((~tp).width == 0) {
        tp.Value.width = 80 - utf8.RuneCountInString((~tp).prefix);
    }
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    foreach (var (i, x) in d.Content) {
        if (i > 0 && blankBefore(x)) {
            @out.WriteString((~tp).prefix);
            writeNL(Ꮡout);
        }
        tp.block(Ꮡout, x);
    }
    var anyUsed = false;
    foreach (var (_, def) in d.Links) {
        if ((~def).Used) {
            anyUsed = true;
            break;
        }
    }
    if (anyUsed) {
        writeNL(Ꮡout);
        foreach (var (_, def) in d.Links) {
            if ((~def).Used) {
                fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "[%s]: %s\n"u8, (~def).Text, (~def).URL);
            }
        }
    }
    return @out.Bytes();
}

// writeNL calls out.WriteByte('\n')
// but first trims trailing spaces on the previous line.
internal static void writeNL(ж<bytes.Buffer> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    // Trim trailing spaces.
    var data = @out.Bytes();
    nint n = 0;
    while (n < len(data) && (data[len(data) - n - 1] == (rune)' ' || data[len(data) - n - 1] == (rune)'\t')) {
        n++;
    }
    if (n > 0) {
        @out.Truncate(len(data) - n);
    }
    @out.WriteByte((rune)'\n');
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fourSpaceᶜ2 = "    "u8;

// block prints the block x to out.
internal static void block(this ж<textPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var @out = ref Ꮡout.DerefOrNull();

    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "?%T\n"u8, xΔ1);
        break;
    }
    case ж<Paragraph> xΔ1: {
        @out.WriteString(p.prefix);
        Ꮡp.text(Ꮡout, ""u8, (~xΔ1).Text);
        break;
    }
    case ж<Heading> xΔ1: {
        @out.WriteString(p.prefix);
        @out.WriteString("# "u8);
        Ꮡp.text(Ꮡout, ""u8, (~xΔ1).Text);
        break;
    }
    case ж<Code> xΔ1: {
        @string text = xΔ1.Value.Text;
        while (text != ""u8) {
            @string line = default!;
            (line, text, _) = strings.Cut(text, "\n"u8);
            if (line != ""u8) {
                @out.WriteString(p.codePrefix);
                @out.WriteString(line);
            }
            writeNL(Ꮡout);
        }
        break;
    }
    case ж<List> xΔ1: {
        var loose = xΔ1.BlankBetween();
        foreach (var (i, item) in (~xΔ1).Items) {
            if (i > 0 && loose) {
                @out.WriteString(p.prefix);
                writeNL(Ꮡout);
            }
            @out.WriteString(p.prefix);
            @out.WriteString(" "u8);
            if ((~item).Number == ""u8){
                @out.WriteString(" - "u8);
            } else {
                @out.WriteString((~item).Number);
                @out.WriteString(". "u8);
            }
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = fourSpaceᶜ2;
                if (iΔ1 > 0) {
                    writeNL(Ꮡout);
                    @out.WriteString(p.prefix);
                    @out.WriteString(fourSpace);
                }
                Ꮡp.text(Ꮡout, fourSpace, (~blk._<ж<Paragraph>>()).Text);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
internal static void text(this ж<textPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, @string indent, slice<ΔText> x) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var @out = ref Ꮡout.DerefOrNull();

    p.oneLongLine(Ꮡp.of(textPrinter.Ꮡlong), x);
    var words = strings.Fields(p.@long.String());
    p.@long.Reset();
    slice<nint> seq = default!;
    if (p.width < 0 || len(words) == 0){
        seq = new nint[]{0, len(words)}.slice(); // one long line
    } else {
        seq = wrap(words, p.width - utf8.RuneCountInString(indent));
    }
    for (nint i = 0; i + 1 < len(seq); i++) {
        if (i > 0) {
            @out.WriteString(p.prefix);
            @out.WriteString(indent);
        }
        foreach (var (j, w) in words.slice(seq[i], seq[i + 1])) {
            if (j > 0) {
                @out.WriteString(" "u8);
            }
            @out.WriteString(w);
        }
        writeNL(Ꮡout);
    }
}

// oneLongLine prints the text sequence x to out as one long line,
// without worrying about line wrapping.
// Explicit links have the [ ] dropped to improve readability.
internal static void oneLongLine(this ref textPrinter p, ж<strings.Builder> Ꮡout, slice<ΔText> x) {
    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain tΔ1: {
            Ꮡout.WriteString(((@string)tΔ1));
            break;
        }
        case Italic tΔ1: {
            Ꮡout.WriteString(((@string)tΔ1));
            break;
        }
        case ж<Link> tΔ1: {
            p.oneLongLine(Ꮡout, (~tΔ1).Text);
            break;
        }
        case ж<DocLink> tΔ1: {
            p.oneLongLine(Ꮡout, (~tΔ1).Text);
            break;
        }}
    }
}

// A score is the score (also called weight) for a given line.
// add and cmp add and compare scores.
internal partial struct wrap_score /*dyn*/ {
    internal int64 hi;
    internal int64 lo;
}

// wrap wraps words into lines of at most max runes,
// minimizing the sum of the squares of the leftover lengths
// at the end of each line (except the last, of course),
// with a preference for ending lines at punctuation (.,:;).
//
// The returned slice gives the indexes of the first words
// on each line in the wrapped text with a final entry of len(words).
// Thus the lines are words[seq[0]:seq[1]], words[seq[1]:seq[2]],
// ..., words[seq[len(seq)-2]:seq[len(seq)-1]].
//
// The implementation runs in O(n log n) time, where n = len(words),
// using the algorithm described in D. S. Hirschberg and L. L. Larmore,
// “[The least weight subsequence problem],” FOCS 1985, pp. 137-143.
//
// [The least weight subsequence problem]: https://doi.org/10.1109/SFCS.1985.60
internal static slice<nint> /*seq*/ wrap(slice<@string> words, nint max) {
    slice<nint> seq = default!;

    // The algorithm requires that our scoring function be concave,
    // meaning that for all i₀ ≤ i₁ < j₀ ≤ j₁,
    // weight(i₀, j₀) + weight(i₁, j₁) ≤ weight(i₀, j₁) + weight(i₁, j₀).
    //
    // Our weights are two-element pairs [hi, lo]
    // ordered by elementwise comparison.
    // The hi entry counts the weight for lines that are longer than max,
    // and the lo entry counts the weight for lines that are not.
    // This forces the algorithm to first minimize the number of lines
    // that are longer than max, which correspond to lines with
    // single very long words. Having done that, it can move on to
    // minimizing the lo score, which is more interesting.
    //
    // The lo score is the sum for each line of the square of the
    // number of spaces remaining at the end of the line and a
    // penalty of 64 given out for not ending the line in a
    // punctuation character (.,:;).
    // The penalty is somewhat arbitrarily chosen by trying
    // different amounts and judging how nice the wrapped text looks.
    // Roughly speaking, using 64 means that we are willing to
    // end a line with eight blank spaces in order to end at a
    // punctuation character, even if the next word would fit in
    // those spaces.
    //
    // We care about ending in punctuation characters because
    // it makes the text easier to skim if not too many sentences
    // or phrases begin with a single word on the previous line.
    wrap_score add(wrap_score s, wrap_score t) => new wrap_score(s.hi + t.hi, s.lo + t.lo);
    nint cmp(wrap_score s, wrap_score t) {
        switch (ᐧ) {
        case {} when s.hi < t.hi: {
            return -1;
        }
        case {} when s.hi > t.hi: {
            return +1;
        }
        case {} when s.lo < t.lo: {
            return -1;
        }
        case {} when s.lo > t.lo: {
            return +1;
        }}

        return 0;
    }
    // total[j] is the total number of runes
    // (including separating spaces) in words[:j].
    var total = new slice<nint>(len(words) + 1);
    total[0] = 0;
    foreach (var (i, s) in words) {
        total[1 + i] = total[i] + utf8.RuneCountInString(s) + 1;
    }
    // weight returns weight(i, j).
    var totalʗ1 = total;
    var wordsʗ1 = words;
    wrap_score weight(nint i, nint j) {
        // On the last line, there is zero weight for being too short.
        nint nΔ1 = totalʗ1[j] - 1 - totalʗ1[i];
        if (j == len(wordsʗ1) && nΔ1 <= max) {
            return new wrap_score(0, 0);
        }
        // Otherwise the weight is the penalty plus the square of the number of
        // characters remaining on the line or by which the line goes over.
        // In the latter case, that value goes in the hi part of the score.
        // (See note above.)
        var p = wrapPenalty(wordsʗ1[j - 1]);
        var v = (int64)(max - nΔ1) * (int64)(max - nΔ1);
        if (nΔ1 > max) {
            return new wrap_score(v, p);
        }
        return new wrap_score(0, v + p);
    }
    // The rest of this function is “The Basic Algorithm” from
    // Hirschberg and Larmore's conference paper,
    // using the same names as in the paper.
    ref var f = ref heap<slice<wrap_score>>(out var Ꮡf);
    f = new wrap_score[]{new(0, 0)}.slice();
    var addʗ1 = add;
    var weightʗ1 = weight;
    wrap_score g(nint i, nint j) => addʗ1(Ꮡf.ValueSlot[i], weightʗ1(i, j));
    var cmpʗ1 = cmp;
    var gʗ1 = g;
    var wordsʗ2 = words;
    bool bridge(nint a, nint b, nint c) {
        var cmpʗ2 = cmpʗ1;
        var gʗ2 = gʗ1;
        nint k = c + sort.Search(len(wordsʗ2) + 1 - c, (nint kΔ1) => {
            kΔ1 += c;
            return cmpʗ2(gʗ2(a, kΔ1), gʗ2(b, kΔ1)) > 0;
        });
        if (k > len(wordsʗ2)) {
            return true;
        }
        return cmpʗ1(gʗ1(c, k), gʗ1(b, k)) <= 0;
    }
    // d is a one-ended deque implemented as a slice.
    var d = new slice<nint>(1, len(words));
    d[0] = 0;
    var bestleft = new slice<nint>(1, len(words));
    bestleft[0] = -1;
    for (nint m = 1; m < len(words); m++) {
        f = append(f, g(d[0], m));
        bestleft = append(bestleft, d[0]);
        while (len(d) > 1 && cmp(g(d[1], m + 1), g(d[0], m + 1)) <= 0) {
            d = d[1..]; // “Retire”
        }
        while (len(d) > 1 && bridge(d[len(d) - 2], d[len(d) - 1], m)) {
            d = d.slice(0, len(d) - 1); // “Fire”
        }
        if (cmp(g(m, len(words)), g(d[len(d) - 1], len(words))) < 0) {
            d = append(d, m); // “Hire”
            // The next few lines are not in the paper but are necessary
            // to handle two-word inputs correctly. It appears to be
            // just a bug in the paper's pseudocode.
            if (len(d) == 2 && cmp(g(d[1], m + 1), g(d[0], m + 1)) <= 0) {
                d = d[1..];
            }
        }
    }
    bestleft = append(bestleft, d[0]);
    // Recover least weight sequence from bestleft.
    nint n = 1;
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n++;
    }
    seq = new slice<nint>(n);
    for (nint m = len(words); m > 0; m = bestleft[m]) {
        n--;
        seq[n] = m;
    }
    return seq;
}

// wrapPenalty is the penalty for inserting a line break after word s.
internal static int64 wrapPenalty(@string s) {
    switch (s[len(s) - 1]) {
    case (rune)'.' or (rune)',' or (rune)':' or (rune)';': {
        return 0;
    }}

    return 64;
}

} // end comment_package
