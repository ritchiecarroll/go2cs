// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strconv = strconv_package;
using io = io_package;
using static comment_package;

partial class comment_package {

// An htmlPrinter holds the state needed for printing a [Doc] as HTML.
partial struct htmlPrinter {
    /*embed*/ public ж<Printer> Printer;
    internal bool tight;
}

} partial class commentᴛPrinter {

// HTML returns an HTML formatting of the [Doc].
// See the [Printer] documentation for ways to customize the HTML output.
public static slice<byte> HTML(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    var hp = Ꮡ(new htmlPrinter(Printer: Ꮡp));
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    foreach (var (_, x) in range(d.Content)) {
        hp.block(Ꮡout, x);
    }
    return @out.Bytes();
}

} partial class commentᴛhtmlPrinter {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string preˢ => ref preˢᵇ;
private static readonly @string preˢᵇ = "<pre>"u8;
internal static ref readonly @string preˢ2 => ref preˢ2ᵇ;
private static readonly @string preˢ2ᵇ = "</pre>\n"u8;
internal static ref readonly @string valueˢ => ref valueˢᵇ;
private static readonly @string valueˢᵇ = @" value="""u8;

// block prints the block x to out.
internal static void block(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.DerefOrNull();

    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "?%T"u8, xΔ1);
        break;
    }
    case ж<Paragraph> xΔ1: {
        if (!p.tight) {
            @out.WriteString("<p>");
        }
        p.text(Ꮡout, xΔ1.Text);
        @out.WriteString("\n");
        break;
    }
    case ж<Heading> xΔ1: {
        @out.WriteString("<h");
        @string h = strconv.Itoa(p.headingLevel());
        @out.WriteString(h);
        @string id = p.headingID(xΔ1); if (id != ""u8) {
            @out.WriteString(@" id=""");
            p.escape(Ꮡout, id);
            @out.WriteString(@"""");
        }
        @out.WriteString(">");
        p.text(Ꮡout, xΔ1.Text);
        @out.WriteString("</h");
        @out.WriteString(h);
        @out.WriteString(">\n");
        break;
    }
    case ж<Code> xΔ1: {
        @out.WriteString(preˢ);
        p.escape(Ꮡout, xΔ1.Text);
        @out.WriteString(preˢ2);
        break;
    }
    case ж<List> xΔ1: {
        @string kind = "ol>\n";
        if (xΔ1.Items[0].Number == ""u8) {
            kind = "ul>\n";
        }
        @out.WriteString("<");
        @out.WriteString(kind);
        @string next = "1";
        foreach (var (_, item) in range(xΔ1.Items)) {
            @out.WriteString("<li");
            @string n = item.Number; if (n != ""u8) {
                if (n != next) {
                    @out.WriteString(valueˢ);
                    @out.WriteString(n);
                    @out.WriteString(@"""");
                    next = n;
                }
                next = inc(next);
            }
            @out.WriteString(">");
            p.tight = !xΔ1.BlankBetween();
            foreach (var (_, blk) in range(item.Content)) {
                p.block(Ꮡout, blk);
            }
            p.tight = false;
        }
        @out.WriteString("</");
        @out.WriteString(kind);
        break;
    }}
}

} partial class comment_package {

// inc increments the decimal string s.
// For example, inc("1199") == "1200".
internal static @string inc(@string s) {
    var b = slice<byte>(s);
    for (nint i = len(b) - 1; i >= 0; i--) {
        if (b[i] < '9') {
            b[i]++;
            return (@string)b;
        }
        b[i] = (byte)'0';
    }
    return "1"u8 + ((sstring)b);
}

} partial class commentᴛhtmlPrinter {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aHrefˢ => ref aHrefˢᵇ;
private static readonly @string aHrefˢᵇ = @"<a href="""u8;

// text prints the text sequence x to out.
internal static void text(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, slice<Text> x) {
    ref var @out = ref Ꮡout.DerefOrNull();

    foreach (var (_, t) in range(x)) {
        switch (t.type()) {
        case Plain tΔ1: {
            p.escape(Ꮡout, (@string)tΔ1);
            break;
        }
        case Italic tΔ1: {
            @out.WriteString("<i>");
            p.escape(Ꮡout, (@string)tΔ1);
            @out.WriteString("</i>");
            break;
        }
        case ж<Link> tΔ1: {
            @out.WriteString(aHrefˢ);
            p.escape(Ꮡout, tΔ1.URL);
            @out.WriteString(@""">");
            p.text(Ꮡout, tΔ1.Text);
            @out.WriteString("</a>");
            break;
        }
        case ж<DocLink> tΔ1: {
            @string url = p.docLinkURL(tΔ1);
            if (url != ""u8) {
                @out.WriteString(aHrefˢ);
                p.escape(Ꮡout, url);
                @out.WriteString(@""">");
            }
            p.text(Ꮡout, tΔ1.Text);
            if (url != ""u8) {
                @out.WriteString("</a>");
            }
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ampˢ => ref ampˢᵇ;
private static readonly @string ampˢᵇ = "&amp;"u8;
internal static ref readonly @string quotˢ => ref quotˢᵇ;
private static readonly @string quotˢᵇ = "&quot;"u8;
internal static ref readonly @string aposˢ => ref aposˢᵇ;
private static readonly @string aposˢᵇ = "&apos;"u8;

// escape prints s to out as plain text,
// escaping < & " ' and > to avoid being misinterpreted
// in larger HTML constructs.
internal static void escape(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, @string s) {
    ref var @out = ref Ꮡout.DerefOrNull();

    nint start = 0;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (byte)'<': {
            @out.WriteString(s.slice(start, i));
            @out.WriteString("&lt;");
            start = i + 1;
            break;
        }
        case (byte)'&': {
            @out.WriteString(s.slice(start, i));
            @out.WriteString(ampˢ);
            start = i + 1;
            break;
        }
        case (byte)'"': {
            @out.WriteString(s.slice(start, i));
            @out.WriteString(quotˢ);
            start = i + 1;
            break;
        }
        case (byte)'\'': {
            @out.WriteString(s.slice(start, i));
            @out.WriteString(aposˢ);
            start = i + 1;
            break;
        }
        case (byte)'>': {
            @out.WriteString(s.slice(start, i));
            @out.WriteString("&gt;");
            start = i + 1;
            break;
        }}
    }
    @out.WriteString(s.slice(start));
}

} // end commentᴛhtmlPrinter
