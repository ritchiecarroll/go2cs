// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using io = io_package;

partial class comment_package {

// An mdPrinter holds the state needed for printing a Doc as Markdown.
partial struct mdPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal @string headingPrefix;
    internal bytes.Buffer raw;
}

// Markdown returns a Markdown formatting of the Doc.
// See the [Printer] documentation for ways to customize the Markdown output.
public static slice<byte> Markdown(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var d = ref Ꮡd.DerefOrNull();

    var mp = Ꮡ(new mdPrinter(
        Printer: Ꮡp,
        headingPrefix: strings.Repeat("#"u8, p.headingLevel()) + " "u8
    ));
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    foreach (var (i, x) in d.Content) {
        if (i > 0) {
            @out.WriteByte((rune)'\n');
        }
        mp.block(Ꮡout, x);
    }
    return @out.Bytes();
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fourSpaceᶜ = "    "u8;

// block prints the block x to out.
internal static void block(this ж<mdPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var @out = ref Ꮡout.DerefOrNull();

    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "?%T"u8, xΔ1);
        break;
    }
    case ж<Paragraph> xΔ1: {
        Ꮡp.text(Ꮡout, (~xΔ1).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case ж<Heading> xΔ1: {
        @out.WriteString(p.headingPrefix);
        Ꮡp.text(Ꮡout, (~xΔ1).Text);
        {
            @string id = p.headingID(xΔ1); if (id != ""u8) {
                @out.WriteString(" {#"u8);
                @out.WriteString(id);
                @out.WriteString("}"u8);
            }
        }
        @out.WriteString("\n"u8);
        break;
    }
    case ж<Code> xΔ1: {
        @string md = xΔ1.Value.Text;
        while (md != ""u8) {
            @string line = default!;
            (line, md, _) = strings.Cut(md, "\n"u8);
            if (line != ""u8) {
                @out.WriteString("\t"u8);
                @out.WriteString(line);
            }
            @out.WriteString("\n"u8);
        }
        break;
    }
    case ж<List> xΔ1: {
        var loose = xΔ1.BlankBetween();
        foreach (var (i, item) in (~xΔ1).Items) {
            if (i > 0 && loose) {
                @out.WriteString("\n"u8);
            }
            {
                @string n = item.Value.Number; if (n != ""u8){
                    @out.WriteString(" "u8);
                    @out.WriteString(n);
                    @out.WriteString(". "u8);
                } else {
                    @out.WriteString("  - "u8); // SP SP - SP
                }
            }
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = fourSpaceᶜ;
                if (iΔ1 > 0) {
                    @out.WriteString("\n" + fourSpace);
                }
                Ꮡp.text(Ꮡout, (~blk._<ж<Paragraph>>()).Text);
                @out.WriteString("\n"u8);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
internal static void text(this ж<mdPrinter> Ꮡp, ж<bytes.Buffer> Ꮡout, slice<ΔText> x) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var @out = ref Ꮡout.DerefOrNull();

    p.raw.Reset();
    p.rawText(Ꮡp.of(mdPrinter.Ꮡraw), x);
    var line = bytes.TrimSpace(p.raw.Bytes());
    if (len(line) == 0) {
        return;
    }
    switch (line[0]) {
    case (rune)'+' or (rune)'-' or (rune)'*' or (rune)'#': {
        @out.WriteByte((rune)'\\');
        break;
    }
    case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9': {
        nint i = 1;
        while (i < len(line) && (rune)'0' <= line[i] && line[i] <= (rune)'9') {
            // Escape what would be the start of an unordered list or heading.
            i++;
        }
        if (i < len(line) && (line[i] == (rune)'.' || line[i] == (rune)')')) {
            // Escape what would be the start of an ordered list.
            @out.Write(line.slice(0, i));
            @out.WriteByte((rune)'\\');
            line = line.slice(i);
        }
        break;
    }}

    @out.Write(line);
}

// rawText prints the text sequence x to out,
// without worrying about escaping characters
// that have special meaning at the start of a Markdown line.
internal static void rawText(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, slice<ΔText> x) {
    ref var @out = ref Ꮡout.DerefOrNull();

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain tΔ1: {
            p.escape(Ꮡout, ((@string)tΔ1));
            break;
        }
        case Italic tΔ1: {
            @out.WriteString("*"u8);
            p.escape(Ꮡout, ((@string)tΔ1));
            @out.WriteString("*"u8);
            break;
        }
        case ж<Link> tΔ1: {
            @out.WriteString("["u8);
            p.rawText(Ꮡout, (~tΔ1).Text);
            @out.WriteString("]("u8);
            @out.WriteString((~tΔ1).URL);
            @out.WriteString(")"u8);
            break;
        }
        case ж<DocLink> tΔ1: {
            @string url = p.docLinkURL(tΔ1);
            if (url != ""u8) {
                @out.WriteString("["u8);
            }
            p.rawText(Ꮡout, (~tΔ1).Text);
            if (url != ""u8) {
                @out.WriteString("]("u8);
                url = strings.ReplaceAll(url, "("u8, "%28"u8);
                url = strings.ReplaceAll(url, ")"u8, "%29"u8);
                @out.WriteString(url);
                @out.WriteString(")"u8);
            }
            break;
        }}
    }
}

// escape prints s to out as plain text,
// escaping special characters to avoid being misinterpreted
// as Markdown markup sequences.
internal static void escape(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, @string s) {
    ref var @out = ref Ꮡout.DerefOrNull();

    nint start = 0;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'\n': {
            @out.WriteString(s.slice(start, i));
            @out.WriteByte((rune)' ');
            start = i + 1;
            continue;
            break;
        }
        case (rune)'`' or (rune)'_' or (rune)'*' or (rune)'[' or (rune)'<' or (rune)'\\': {
            @out.WriteString(s.slice(start, i));
            @out.WriteByte((rune)'\\');
            @out.WriteByte(s[i]);
            start = i + 1;
            break;
        }}

    }
    // Turn all \n into spaces, for a few reasons:
    //   - Avoid introducing paragraph breaks accidentally.
    //   - Avoid the need to reindent after the newline.
    //   - Avoid problems with Markdown renderers treating
    //     every mid-paragraph newline as a <br>.
    // Not all of these need to be escaped all the time,
    // but is valid and easy to do so.
    // We assume the Markdown is being passed to a
    // Markdown renderer, not edited by a person,
    // so it's fine to have escapes that are not strictly
    // necessary in some cases.
    @out.WriteString(s.slice(start));
}

} // end comment_package
