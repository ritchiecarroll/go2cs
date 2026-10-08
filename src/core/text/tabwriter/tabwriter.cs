// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tabwriter implements a write filter (tabwriter.Writer) that
// translates tabbed columns in input into properly aligned text.
//
// The package is using the Elastic Tabstops algorithm described at
// http://nickgravgaard.com/elastictabstops/index.html.
//
// The text/tabwriter package is frozen and is not accepting new features.
namespace go.text;

using fmt = fmt_package;
using io = io_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class tabwriter_package {

// ----------------------------------------------------------------------------
// Filter implementation

// A cell represents a segment of text terminated by tabs or line breaks.
// The text itself is stored in a separate buffer; cell only describes the
// segment's size in bytes, its width in runes, and whether it's an htab
// ('\t') terminated cell.
partial struct cell {
    internal nint size; // cell size in bytes
    internal nint width; // cell width in runes
    internal bool htab; // true if the cell is terminated by an htab ('\t')
}

// A Writer is a filter that inserts padding around tab-delimited
// columns in its input to align them in the output.
//
// The Writer treats incoming bytes as UTF-8-encoded text consisting
// of cells terminated by horizontal ('\t') or vertical ('\v') tabs,
// and newline ('\n') or formfeed ('\f') characters; both newline and
// formfeed act as line breaks.
//
// Tab-terminated cells in contiguous lines constitute a column. The
// Writer inserts padding as needed to make all cells in a column have
// the same width, effectively aligning the columns. It assumes that
// all characters have the same width, except for tabs for which a
// tabwidth must be specified. Column cells must be tab-terminated, not
// tab-separated: non-tab terminated trailing text at the end of a line
// forms a cell but that cell is not part of an aligned column.
// For instance, in this example (where | stands for a horizontal tab):
//
//	aaaa|bbb|d
//	aa  |b  |dd
//	a   |
//	aa  |cccc|eee
//
// the b and c are in distinct columns (the b column is not contiguous
// all the way). The d and e are not in a column at all (there's no
// terminating tab, nor would the column be contiguous).
//
// The Writer assumes that all Unicode code points have the same width;
// this may not be true in some fonts or if the string contains combining
// characters.
//
// If [DiscardEmptyColumns] is set, empty columns that are terminated
// entirely by vertical (or "soft") tabs are discarded. Columns
// terminated by horizontal (or "hard") tabs are not affected by
// this flag.
//
// If a Writer is configured to filter HTML, HTML tags and entities
// are passed through. The widths of tags and entities are
// assumed to be zero (tags) and one (entities) for formatting purposes.
//
// A segment of text may be escaped by bracketing it with [Escape]
// characters. The tabwriter passes escaped text segments through
// unchanged. In particular, it does not interpret any tabs or line
// breaks within the segment. If the [StripEscape] flag is set, the
// Escape characters are stripped from the output; otherwise they
// are passed through as well. For the purpose of formatting, the
// width of the escaped text is always computed excluding the Escape
// characters.
//
// The formfeed character acts like a newline but it also terminates
// all columns in the current line (effectively calling [Writer.Flush]). Tab-
// terminated cells in the next line start new columns. Unless found
// inside an HTML tag or inside an escaped text segment, formfeed
// characters appear as newlines in the output.
//
// The Writer must buffer input internally, because proper spacing
// of one line may depend on the cells in future lines. Clients must
// call Flush when done calling [Writer.Write].
partial struct Writer {
    // configuration
    internal io.Writer output;
    internal nint minwidth;
    internal nint tabwidth;
    internal nint padding;
    internal array<byte> padbytes = new(8);
    internal nuint flags;
    // current state
    internal slice<byte> buf; // collected text excluding tabs or line breaks
    internal nint pos;     // buffer position up to which cell.width of incomplete cell has been computed
    internal cell cell;     // current incomplete cell; cell.width is up to buf[pos] excluding ignored sections
    internal byte endChar;     // terminating char of escaped sequence (Escape for escapes, '>', ';' for HTML tags/entities, or 0)
    internal slice<slice<cell>> lines; // list of lines; each line is a list of cells
    internal slice<nint> widths; // list of column widths in runes - re-used during formatting
}

// addLine adds a new line.
// flushed is a hint indicating whether the underlying writer was just flushed.
// If so, the previous line is not likely to be a good indicator of the new line's cells.
internal static void addLine(this ref Writer b, bool flushed) {
    // Grow slice instead of appending,
    // as that gives us an opportunity
    // to re-use an existing []cell.
    {
        nint n = len(b.lines) + 1; if (n <= cap(b.lines)){
            b.lines = b.lines.slice(0, n);
            b.lines[n - 1] = b.lines[n - 1][..0];
        } else {
            b.lines = builtin.append(b.lines, (slice<cell>)(default!));
        }
    }
    if (!flushed) {
        // The previous line is probably a good indicator
        // of how many cells the current line will have.
        // If the current line's capacity is smaller than that,
        // abandon it and make a new one.
        {
            nint n = len(b.lines); if (n >= 2) {
                {
                    nint prev = len(b.lines[n - 2]); if (prev > cap(b.lines[n - 1])) {
                        b.lines[n - 1] = new slice<cell>(0, prev);
                    }
                }
            }
        }
    }
}

// Reset the current state.
internal static void reset(this ref Writer b) {
    b.buf = b.buf[..0];
    b.pos = 0;
    b.cell = new cell(nil);
    b.endChar = 0;
    b.lines = b.lines[0..0];
    b.widths = b.widths[0..0];
    b.addLine(true);
}

// Internal representation (current state):
//
// - all text written is appended to buf; tabs and line breaks are stripped away
// - at any given time there is a (possibly empty) incomplete cell at the end
//   (the cell starts after a tab or line break)
// - cell.size is the number of bytes belonging to the cell so far
// - cell.width is text width in runes of that cell from the start of the cell to
//   position pos; html tags and entities are excluded from this width if html
//   filtering is enabled
// - the sizes and widths of processed text are kept in the lines list
//   which contains a list of cells for each line
// - the widths list is a temporary list with current widths used during
//   formatting; it is kept in Writer because it's re-used
//
//                    |<---------- size ---------->|
//                    |                            |
//                    |<- width ->|<- ignored ->|  |
//                    |           |             |  |
// [---processed---tab------------<tag>...</tag>...]
// ^                  ^                         ^
// |                  |                         |
// buf                start of incomplete cell  pos

// Formatting can be controlled with these flags.
public const nuint FilterHTML = /* 1 << iota */ 1;

public const nuint StripEscape = 2;

public const nuint AlignRight = 4;

public const nuint DiscardEmptyColumns = 8;

public const nuint TabIndent = 16;

public const nuint Debug = 32;

// A [Writer] must be initialized with a call to Init. The first parameter (output)
// specifies the filter output. The remaining parameters control the formatting:
//
//	minwidth	minimal cell width including any padding
//	tabwidth	width of tab characters (equivalent number of spaces)
//	padding		padding added to a cell before computing its width
//	padchar		ASCII char used for padding
//			if padchar == '\t', the Writer will assume that the
//			width of a '\t' in the formatted output is tabwidth,
//			and cells are left-aligned independent of align_left
//			(for correct-looking results, tabwidth must correspond
//			to the tab width in the viewer displaying the result)
//	flags		formatting control
public static ж<Writer> Init(this ж<Writer> Ꮡb, io.Writer output, nint minwidth, nint tabwidth, nint padding, byte padchar, nuint flags) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (minwidth < 0 || tabwidth < 0 || padding < 0) {
        throw panic("negative minwidth, tabwidth, or padding");
    }
    b.output = output;
    b.minwidth = minwidth;
    b.tabwidth = tabwidth;
    b.padding = padding;
    foreach (var (i, _) in b.padbytes) {
        b.padbytes[i] = padchar;
    }
    if (padchar == (rune)'\t') {
        // tab padding enforces left-alignment
        flags &= unchecked((nuint)~(nuint)(AlignRight));
    }
    b.flags = flags;
    b.reset();
    return Ꮡb;
}

// debugging support (keep code around)
internal static void dump(this ref Writer b) {
    nint pos = 0;
    foreach (var (i, line) in b.lines) {
        print((@string)"("u8, i, (@string)") "u8);
        foreach (var (_, c) in line) {
            print((@string)"["u8, ((@string)(b.buf.slice(pos, pos + c.size))), (@string)"]"u8);
            pos += c.size;
        }
        print((@string)"\n"u8);
    }
    print((@string)"\n"u8);
}

// local error wrapper so we can distinguish errors we want to return
// as errors from genuine panics (which we don't want to return as errors)
partial struct osError {
    internal error err;
}

internal static void write0(this ref Writer b, slice<byte> buf) {
    var (n, err) = b.output.Write(buf);
    if (n != len(buf) && err == default!) {
        err = io.ErrShortWrite;
    }
    if (err != default!) {
        throw panic(new osError(err));
    }
}

internal static void writeN(this ref Writer b, slice<byte> src, nint n) {
    while (n > len(src)) {
        b.write0(src);
        n -= len(src);
    }
    b.write0(src.slice(0, n));
}

internal static slice<byte> newline = new byte[]{(rune)'\n'}.slice();
internal static slice<byte> tabs = slice<byte>("\t\t\t\t\t\t\t\t"u8);

internal static void writePadding(this ref Writer b, nint textw, nint cellw, bool useTabs) {
    if (b.padbytes[0] == (rune)'\t' || useTabs) {
        // padding is done with tabs
        if (b.tabwidth == 0) {
            return; // tabs have no width - can't do any padding
        }
        // make cellw the smallest multiple of b.tabwidth
        cellw = quo((cellw + b.tabwidth - 1), b.tabwidth) * b.tabwidth;
        nint n = cellw - textw; // amount of padding
        if (n < 0) {
            throw panic("internal error");
        }
        b.writeN(tabs, quo((n + b.tabwidth - 1), b.tabwidth));
        return;
    }
    // padding is done with non-tab characters
    b.writeN(b.padbytes[0..], cellw - textw);
}

internal static slice<byte> vbar = new byte[]{(rune)'|'}.slice();

internal static nint /*pos*/ writeLines(this ref Writer b, nint pos0, nint line0, nint line1) {
    nint pos = default!;

    pos = pos0;
    for (nint i = line0; i < line1; i++) {
        var line = b.lines[i];
        // if TabIndent is set, use tabs to pad leading empty cells
        var useTabs = (nuint)(b.flags & TabIndent) != 0;
        foreach (var (j, c) in line) {
            if (j > 0 && (nuint)(b.flags & Debug) != 0) {
                // indicate column break
                b.write0(vbar);
            }
            if (c.size == 0){
                // empty cell
                if (j < len(b.widths)) {
                    b.writePadding(c.width, b.widths[j], useTabs);
                }
            } else {
                // non-empty cell
                useTabs = false;
                if ((nuint)(b.flags & AlignRight) == 0){
                    // align left
                    b.write0(b.buf.slice(pos, pos + c.size));
                    pos += c.size;
                    if (j < len(b.widths)) {
                        b.writePadding(c.width, b.widths[j], false);
                    }
                } else {
                    // align right
                    if (j < len(b.widths)) {
                        b.writePadding(c.width, b.widths[j], false);
                    }
                    b.write0(b.buf.slice(pos, pos + c.size));
                    pos += c.size;
                }
            }
        }
        if (i + 1 == len(b.lines)){
            // last buffered line - we don't have a newline, so just write
            // any outstanding buffered data
            b.write0(b.buf.slice(pos, pos + b.cell.size));
            pos += b.cell.size;
        } else {
            // not the last line - write newline
            b.write0(newline);
        }
    }
    return pos;
}

// Format the text between line0 and line1 (excluding line1); pos
// is the buffer position corresponding to the beginning of line0.
// Returns the buffer position corresponding to the beginning of
// line1 and an error, if any.
internal static nint /*pos*/ format(this ref Writer b, nint pos0, nint line0, nint line1) {
    nint pos = default!;

    pos = pos0;
    nint column = len(b.widths);
    for (nint @this = line0; @this < line1; @this++) {
        var line = b.lines[@this];
        if (column >= len(line) - 1) {
            continue;
        }
        // cell exists in this column => this line
        // has more cells than the previous line
        // (the last cell per line is ignored because cells are
        // tab-terminated; the last cell per line describes the
        // text before the newline/formfeed and does not belong
        // to a column)
        // print unprinted lines until beginning of block
        pos = b.writeLines(pos, line0, @this);
        line0 = @this;
        // column block begin
        nint width = b.minwidth; // minimal column width
        var discardable = true; // true if all cells in this column are empty and "soft"
        for (; @this < line1; @this++) {
            line = b.lines[@this];
            if (column >= len(line) - 1) {
                break;
            }
            // cell exists in this column
            var c = line[column];
            // update width
            {
                nint w = c.width + b.padding; if (w > width) {
                    width = w;
                }
            }
            // update discardable
            if (c.width > 0 || c.htab) {
                discardable = false;
            }
        }
        // column block end
        // discard empty columns if necessary
        if (discardable && (nuint)(b.flags & DiscardEmptyColumns) != 0) {
            width = 0;
        }
        // format and print all columns to the right of this column
        // (we know the widths of this column and all columns to the left)
        b.widths = builtin.append(b.widths, width); // push width
        pos = b.format(pos, line0, @this);
        b.widths = b.widths.slice(0, len(b.widths) - 1); // pop width
        line0 = @this;
    }
    // print unprinted lines until end
    return b.writeLines(pos, line0, line1);
}

// Append text to current cell.
internal static void append(this ref Writer b, slice<byte> text) {
    b.buf = builtin.appendꓸꓸꓸ(b.buf, text);
    b.cell.size += len(text);
}

// Update the cell width.
internal static void updateWidth(this ref Writer b) {
    b.cell.width += utf8.RuneCount(b.buf.slice(b.pos));
    b.pos = len(b.buf);
}

// To escape a text segment, bracket it with Escape characters.
// For instance, the tab in this string "Ignore this tab: \xff\t\xff"
// does not terminate a cell and constitutes a single character of
// width one for formatting purposes.
//
// The value 0xff was chosen because it cannot appear in a valid UTF-8 sequence.
public static UntypedInt Escape => /* '\xff' */ 255;

// Start escaped mode.
internal static void startEscape(this ref Writer b, byte ch) {
    var exprᴛ1 = ch;
    if (exprᴛ1 == Escape) {
        b.endChar = Escape;
    }
    else if (exprᴛ1 is (rune)'<') {
        b.endChar = (rune)'>';
    }
    else if (exprᴛ1 is (rune)'&') {
        b.endChar = (rune)';';
    }

}

// Terminate escaped mode. If the escaped text was an HTML tag, its width
// is assumed to be zero for formatting purposes; if it was an HTML entity,
// its width is assumed to be one. In all other cases, the width is the
// unicode width of the text.
internal static void endEscape(this ref Writer b) {
    var exprᴛ1 = b.endChar;
    if (exprᴛ1 == Escape) {
        b.updateWidth();
        if ((nuint)(b.flags & StripEscape) == 0) {
            b.cell.width -= 2; // don't count the Escape chars
        }
    }
    else if (exprᴛ1 is (rune)'>') {
    }
    else if (exprᴛ1 is (rune)';') {
        b.cell.width++; // entity, count as one rune
    }

    // tag of zero width
    b.pos = len(b.buf);
    b.endChar = 0;
}

// Terminate the current cell by adding it to the list of cells of the
// current line. Returns the number of cells in that line.
internal static nint terminateCell(this ref Writer b, bool htab) {
    b.cell.htab = htab;
    var line = Ꮡ(b.lines, len(b.lines) - 1);
    line.ValueSlot = builtin.append(line.ValueSlot, b.cell);
    b.cell = new cell(nil);
    return len(line.ValueSlot);
}

internal static void handlePanic(this ж<Writer> Ꮡb, ж<error> Ꮡerr, @string op) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();
        ref var err = ref Ꮡerr.DerefOrNull();

        {
            var e = recover(); if (e != default!) {
                if (op == "Flush"u8) {
                    // If Flush ran into a panic, we still need to reset.
                    b.reset();
                }
                {
                    var (nerr, ok) = e._<osError>(ᐧ); if (ok) {
                        err = nerr.err;
                        return;
                    }
                }
                throw panic(fmt.Sprintf("tabwriter: panic during %s (%v)"u8, op, e));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Flush should be called after the last call to [Writer.Write] to ensure
// that any data buffered in the [Writer] is written to output. Any
// incomplete escape sequence at the end is considered
// complete for formatting purposes.
public static error Flush(this ж<Writer> Ꮡb) {
    return Ꮡb.flush();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string flushˢ = "Flush"u8;

// flush is the internal version of Flush, with a named return value which we
// don't want to expose.
internal static error /*err*/ flush(this ж<Writer> Ꮡb) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(Ꮡb.handlePanic, Ꮡerr, flushˢ, ref ᒐ);
        b.flushNoDefers();
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;
}

// flushNoDefers is like flush, but without a deferred handlePanic call. This
// can be called from other methods which already have their own deferred
// handlePanic calls, such as Write, and avoid the extra defer work.
internal static void flushNoDefers(this ref Writer b) {
    // add current cell if not empty
    if (b.cell.size > 0) {
        if (b.endChar != 0) {
            // inside escape - terminate it even if incomplete
            b.endEscape();
        }
        b.terminateCell(false);
    }
    // format contents of buffer
    b.format(0, 0, len(b.lines));
    b.reset();
}

internal static slice<byte> hbar = slice<byte>("---\n"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string writeˢ = "Write"u8;

// Write writes buf to the writer b.
// The only errors returned are ones encountered
// while writing to the underlying output stream.
public static (nint n, error err) Write(this ж<Writer> Ꮡb, slice<byte> buf) {
    nint n = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(Ꮡb.handlePanic, Ꮡerr, writeˢ, ref ᒐ);
        // split text into cells
        n = 0;
        foreach (var (i, ch) in buf) {
            if (b.endChar == 0){
                // outside escape
                var exprᴛ1 = ch;
                if (exprᴛ1 is (rune)'\t' or (rune)'\v' or (rune)'\n' or (rune)'\f') {
                    b.append(buf.slice(n, i));
                    b.updateWidth();
                    n = i + 1; // ch consumed
                    nint ncells = b.terminateCell(ch == (rune)'\t');
                    if (ch == (rune)'\n' || ch == (rune)'\f') {
                        // end of cell
                        // terminate line
                        b.addLine(ch == (rune)'\f');
                        if (ch == (rune)'\f' || ncells == 1) {
                            // A '\f' always forces a flush. Otherwise, if the previous
                            // line has only one cell which does not have an impact on
                            // the formatting of the following lines (the last cell per
                            // line is ignored by format()), thus we can flush the
                            // Writer contents.
                            b.flushNoDefers();
                            if (ch == (rune)'\f' && (nuint)(b.flags & Debug) != 0) {
                                // indicate section break
                                b.write0(hbar);
                            }
                        }
                    }
                }
                else if (exprᴛ1 == Escape) {
                    b.append(buf.slice(n, i));
                    b.updateWidth();
                    n = i;
                    if ((nuint)(b.flags & StripEscape) != 0) {
                        // start of escaped sequence
                        n++; // strip Escape
                    }
                    b.startEscape(Escape);
                }
                else if (exprᴛ1 is (rune)'<' or (rune)'&') {
                    if ((nuint)(b.flags & FilterHTML) != 0) {
                        // possibly an html tag/entity
                        // begin of tag/entity
                        b.append(buf.slice(n, i));
                        b.updateWidth();
                        n = i;
                        b.startEscape(ch);
                    }
                }

            } else {
                // inside escape
                if (ch == b.endChar) {
                    // end of tag/entity
                    nint j = i + 1;
                    if (ch == Escape && (nuint)(b.flags & StripEscape) != 0) {
                        j = i; // strip Escape
                    }
                    b.append(buf.slice(n, j));
                    n = i + 1; // ch consumed
                    b.endEscape();
                }
            }
        }
        // append leftover text
        b.append(buf.slice(n));
        n = len(buf);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (n, Ꮡerr.ValueSlot);
}

// NewWriter allocates and initializes a new [Writer].
// The parameters are the same as for the Init function.
public static ж<Writer> NewWriter(io.Writer output, nint minwidth, nint tabwidth, nint padding, byte padchar, nuint flags) {
    return @new<Writer>().Init(output, minwidth, tabwidth, padding, padchar, flags);
}

} // end tabwriter_package
