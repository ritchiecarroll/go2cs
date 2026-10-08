// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using io = io_package;

partial class comment_package {

// A Printer is a doc comment printer.
// The fields in the struct can be filled in before calling
// any of the printing methods
// in order to customize the details of the printing process.
partial struct Printer {
    // HeadingLevel is the nesting level used for
    // HTML and Markdown headings.
    // If HeadingLevel is zero, it defaults to level 3,
    // meaning to use <h3> and ###.
    public nint HeadingLevel;
    // HeadingID is a function that computes the heading ID
    // (anchor tag) to use for the heading h when generating
    // HTML and Markdown. If HeadingID returns an empty string,
    // then the heading ID is omitted.
    // If HeadingID is nil, h.DefaultID is used.
    public Func<ж<Heading>, @string> HeadingID;
    // DocLinkURL is a function that computes the URL for the given DocLink.
    // If DocLinkURL is nil, then link.DefaultURL(p.DocLinkBaseURL) is used.
    public Func<ж<DocLink>, @string> DocLinkURL;
    // DocLinkBaseURL is used when DocLinkURL is nil,
    // passed to [DocLink.DefaultURL] to construct a DocLink's URL.
    // See that method's documentation for details.
    public @string DocLinkBaseURL;
    // TextPrefix is a prefix to print at the start of every line
    // when generating text output using the Text method.
    public @string TextPrefix;
    // TextCodePrefix is the prefix to print at the start of each
    // preformatted (code block) line when generating text output,
    // instead of (not in addition to) TextPrefix.
    // If TextCodePrefix is the empty string, it defaults to TextPrefix+"\t".
    public @string TextCodePrefix;
    // TextWidth is the maximum width text line to generate,
    // measured in Unicode code points,
    // excluding TextPrefix and the newline character.
    // If TextWidth is zero, it defaults to 80 minus the number of code points in TextPrefix.
    // If TextWidth is negative, there is no limit.
    public nint TextWidth;
}

internal static nint headingLevel(this ref Printer p) {
    if (p.HeadingLevel <= 0) {
        return 3;
    }
    return p.HeadingLevel;
}

internal static @string headingID(this ref Printer p, ж<Heading> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    if (p.HeadingID == default!) {
        return h.DefaultID();
    }
    return p.HeadingID(Ꮡh);
}

internal static @string docLinkURL(this ref Printer p, ж<DocLink> Ꮡlink) {
    ref var link = ref Ꮡlink.DerefOrNull();

    if (p.DocLinkURL != default!) {
        return p.DocLinkURL(Ꮡlink);
    }
    return link.DefaultURL(p.DocLinkBaseURL);
}

// DefaultURL constructs and returns the documentation URL for l,
// using baseURL as a prefix for links to other packages.
//
// The possible forms returned by DefaultURL are:
//   - baseURL/ImportPath, for a link to another package
//   - baseURL/ImportPath#Name, for a link to a const, func, type, or var in another package
//   - baseURL/ImportPath#Recv.Name, for a link to a method in another package
//   - #Name, for a link to a const, func, type, or var in this package
//   - #Recv.Name, for a link to a method in this package
//
// If baseURL ends in a trailing slash, then DefaultURL inserts
// a slash between ImportPath and # in the anchored forms.
// For example, here are some baseURL values and URLs they can generate:
//
//	"/pkg/" → "/pkg/math/#Sqrt"
//	"/pkg"  → "/pkg/math#Sqrt"
//	"/"     → "/math/#Sqrt"
//	""      → "/math#Sqrt"
public static @string DefaultURL(this ref DocLink l, @string baseURL) {
    if (l.ImportPath != ""u8) {
        @string slash = ""u8;
        if (strings.HasSuffix(baseURL, "/"u8)){
            slash = "/"u8;
        } else {
            baseURL += "/"u8;
        }
        switch (ᐧ) {
        case {} when l.Name == ""u8: {
            return baseURL + l.ImportPath + slash;
        }
        case {} when l.Recv != ""u8: {
            return baseURL + l.ImportPath + slash + "#"u8 + l.Recv + "."u8 + l.Name;
        }
        default: {
            return baseURL + l.ImportPath + slash + "#"u8 + l.Name;
        }}

    }
    if (l.Recv != ""u8) {
        return "#"u8 + l.Recv + "."u8 + l.Name;
    }
    return "#"u8 + l.Name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hdrˢ = "hdr-"u8;

// DefaultID returns the default anchor ID for the heading h.
//
// The default anchor ID is constructed by converting every
// rune that is not alphanumeric ASCII to an underscore
// and then adding the prefix “hdr-”.
// For example, if the heading text is “Go Doc Comments”,
// the default ID is “hdr-Go_Doc_Comments”.
public static @string DefaultID(this ref Heading h) {
    // Note: The “hdr-” prefix is important to avoid DOM clobbering attacks.
    // See https://pkg.go.dev/github.com/google/safehtml#Identifier.
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    textPrinter p = new(nil);
    p.oneLongLine(Ꮡout, h.Text);
    @string s = strings.TrimSpace(@out.String());
    if (s == ""u8) {
        return ""u8;
    }
    @out.Reset();
    Ꮡout.WriteString(hdrˢ);
    foreach (var (_, r) in s) {
        if (r < 0x80 && isIdentASCII((byte)r)){
            Ꮡout.WriteByte((byte)r);
        } else {
            Ꮡout.WriteByte((rune)'_');
        }
    }
    return @out.String();
}

partial struct commentPrinter {
    public partial ref ж<Printer> Printer { get; }
}

// Comment returns the standard Go formatting of the [Doc],
// without any comment markers.
public static slice<byte> Comment(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    var cp = Ꮡ(new commentPrinter(Printer: Ꮡp));
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    foreach (var (i, x) in d.Content) {
        if (i > 0 && blankBefore(x)) {
            @out.WriteString("\n"u8);
        }
        cp.block(Ꮡout, x);
    }
    // Print one block containing all the link definitions that were used,
    // and then a second block containing all the unused ones.
    // This makes it easy to clean up the unused ones: gofmt and
    // delete the final block. And it's a nice visual signal without
    // affecting the way the comment formats for users.
    for (nint i = 0; i < 2; i++) {
        var used = i == 0;
        var first = true;
        foreach (var (_, def) in d.Links) {
            if ((~def).Used == used) {
                if (first) {
                    @out.WriteString("\n"u8);
                    first = false;
                }
                @out.WriteString("["u8);
                @out.WriteString((~def).Text);
                @out.WriteString("]: "u8);
                @out.WriteString((~def).URL);
                @out.WriteString("\n"u8);
            }
        }
    }
    return @out.Bytes();
}

// blankBefore reports whether the block x requires a blank line before it.
// All blocks do, except for Lists that return false from x.BlankBefore().
internal static bool blankBefore(Block x) {
    {
        var (xΔ1, ok) = x._<ж<List>>(ᐧ); if (ok) {
            return xΔ1.BlankBefore();
        }
    }
    return true;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fourSpaceᶜ1 = "    "u8;

// block prints the block x to out.
internal static void block(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.DerefOrNull();

    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "?%T"u8, xΔ1);
        break;
    }
    case ж<Paragraph> xΔ1: {
        p.text(Ꮡout, ""u8, (~xΔ1).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case ж<Heading> xΔ1: {
        @out.WriteString("# "u8);
        p.text(Ꮡout, ""u8, (~xΔ1).Text);
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
            @out.WriteString(" "u8);
            if ((~item).Number == ""u8){
                @out.WriteString(" - "u8);
            } else {
                @out.WriteString((~item).Number);
                @out.WriteString(". "u8);
            }
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = fourSpaceᶜ1;
                if (iΔ1 > 0) {
                    @out.WriteString("\n" + fourSpace);
                }
                p.text(Ꮡout, fourSpace, (~blk._<ж<Paragraph>>()).Text);
                @out.WriteString("\n"u8);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
internal static void text(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, slice<ΔText> x) {
    ref var @out = ref Ꮡout.DerefOrNull();

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain tΔ1: {
            p.indent(Ꮡout, indent, ((@string)tΔ1));
            break;
        }
        case Italic tΔ1: {
            p.indent(Ꮡout, indent, ((@string)tΔ1));
            break;
        }
        case ж<Link> tΔ1: {
            if ((~tΔ1).Auto){
                p.text(Ꮡout, indent, (~tΔ1).Text);
            } else {
                @out.WriteString("["u8);
                p.text(Ꮡout, indent, (~tΔ1).Text);
                @out.WriteString("]"u8);
            }
            break;
        }
        case ж<DocLink> tΔ1: {
            @out.WriteString("["u8);
            p.text(Ꮡout, indent, (~tΔ1).Text);
            @out.WriteString("]"u8);
            break;
        }}
    }
}

// indent prints s to out, indenting with the indent string
// after each newline in s.
internal static void indent(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, @string s) {
    ref var @out = ref Ꮡout.DerefOrNull();

    while (s != ""u8) {
        var (line, rest, ok) = strings.Cut(s, "\n"u8);
        @out.WriteString(line);
        if (ok) {
            @out.WriteString("\n"u8);
            @out.WriteString(indent);
        }
        s = rest;
    }
}

} // end comment_package
