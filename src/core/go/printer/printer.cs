// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package printer implements printing of AST nodes.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using token = global::go.go.token_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;
using tabwriter = text.tabwriter_package;
using unicode = unicode_package;
using global::go.go;
using global::go.go.build;
using text;
using ꓸꓸꓸany = Span<any>;

partial class printer_package {

internal static UntypedInt maxNewlines => 2; // max. number of newlines between source text
internal const bool debug = false; // enable for debugging
internal static UntypedInt infinity => /* 1 << 30 */ 1073741824;

partial struct whiteSpace /*num:byte*/;

internal static whiteSpace ignore => /* whiteSpace(0) */ 0;
internal static whiteSpace blank => /* whiteSpace(' ') */ 32;
internal static whiteSpace vtab => /* whiteSpace('\v') */ 11;
internal static whiteSpace newline => /* whiteSpace('\n') */ 10;
internal static whiteSpace formfeed => /* whiteSpace('\f') */ 12;
internal static whiteSpace indent => /* whiteSpace('>') */ 62;
internal static whiteSpace unindent => /* whiteSpace('<') */ 60;

partial struct pmode /*num:nint*/;

internal static pmode noExtraBlank => /* 1 << iota */ 1;         // disables extra blank after /*-style comment
internal static pmode noExtraLinebreak => 2;     // disables extra line break after /*-style comment

partial struct commentInfo {
    internal nint cindex;              // index of the next comment
    internal ж<ast.CommentGroup> comment; // = printer.comments[cindex-1]; or nil
    internal nint commentOffset;              // = printer.posFor(printer.comments[cindex-1].List[0].Pos()).Offset; or infinity
    internal bool commentNewline;              // true if the comment group contains newlines
}

partial struct printer {
    // Configuration (does not change after initialization)
    public partial ref Config Config { get; }
    internal ж<token.FileSet> fset;
    // Current state
    internal slice<byte> output;  // raw printer result
    internal nint indent;         // current indentation
    internal nint level;         // level == 0: outside composite literal; level > 0: inside composite literal
    internal pmode mode;        // current printer mode
    internal bool endAlignment;         // if set, terminate alignment immediately
    internal bool impliedSemi;         // if set, a linebreak implies a semicolon
    internal token.Token lastTok;  // last token printed (token.ILLEGAL if it's whitespace)
    internal token.Token prevOpen;  // previous non-brace "open" token (, [, or token.ILLEGAL
    internal slice<whiteSpace> wsbuf; // delayed white space
    internal slice<nint> goBuild;  // start index of all //go:build comments in output
    internal slice<nint> plusBuild;  // start index of all // +build comments in output
    // Positions
    // The out position differs from the pos position when the result
    // formatting differs from the source formatting (in the amount of
    // white space). If there's a difference and SourcePos is set in
    // ConfigMode, //line directives are used in the output to restore
    // original source positions for a reader.
    internal tokenꓸPosition pos; // current position in AST (source) space
    internal tokenꓸPosition @out; // current position in output space
    internal tokenꓸPosition last; // value of pos after calling writeString
    internal ж<nint> linePtr;       // if set, record out.Line for the next token in *linePtr
    internal error sourcePosErr;          // if non-nil, the first error emitting a //line directive
    // The list of all source comments, in order of appearance.
    internal slice<ж<ast.CommentGroup>> comments; // may be nil
    internal bool useNodeComments;                // if not set, ignore lead and line comments of nodes
    // Information about p.comments[p.cindex]; set up by nextComment.
    internal partial ref commentInfo commentInfo { get; }
    // Cache of already computed node sizes.
    internal map<ast.Node, nint> nodeSizes;
    // Cache of most recently computed line position.
    internal tokenꓸPos cachedPos;
    internal nint cachedLine; // line corresponding to cachedPos
}

internal static void internalError(this ref printer p, params ꓸꓸꓸany msgʗp) {
    var msg = msgʗp.sslice();

    if (debug) {
        fmt.Print(p.pos.String() + ": ");
        fmt.Println(msg.ꓸꓸꓸ);
        throw panic("go/printer");
    }
}

// commentsHaveNewline reports whether a list of comments belonging to
// an *ast.CommentGroup contains newlines. Because the position information
// may only be partially correct, we also have to read the comment text.
internal static bool commentsHaveNewline(this ref printer p, slice<ж<ast.Comment>> list) {
    // len(list) > 0
    nint line = p.lineFor(list[0].Pos());
    foreach (var (i, c) in list) {
        if (i > 0 && p.lineFor(list[i].Pos()) != line) {
            // not all comments on the same line
            return true;
        }
        {
            @string t = c.Value.Text; if (len(t) >= 2 && (t[1] == (rune)'/' || strings.Contains(t, "\n"u8))) {
                return true;
            }
        }
    }
    _ = line;
    return false;
}

internal static void nextComment(this ref printer p) {
    while (p.cindex < len(p.comments)) {
        var c = p.comments[p.cindex];
        p.cindex++;
        {
            var list = c.Value.List; if (len(list) > 0) {
                p.comment = c;
                p.commentOffset = p.posFor(list[0].Pos()).Offset;
                p.commentNewline = p.commentsHaveNewline(list);
                return;
            }
        }
    }
    // we should not reach here (correct ASTs don't have empty
    // ast.CommentGroup nodes), but be conservative and try again
    // no more comments
    p.commentOffset = infinity;
}

// commentBefore reports whether the current comment group occurs
// before the next position in the source code and printing it does
// not introduce implicit semicolons.
internal static bool commentBefore(this ref printer p, tokenꓸPosition next) {
    return p.commentOffset < next.Offset && (!p.impliedSemi || !p.commentNewline);
}

// commentSizeBefore returns the estimated size of the
// comments on the same line before the next position.
internal static nint commentSizeBefore(this ж<printer> Ꮡp, tokenꓸPosition next) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        // save/restore current p.commentInfo (p.nextComment() modifies it)
        defer((commentInfo info) => {
            Ꮡp.Value.commentInfo = info;
        }, Ꮡp.Value.commentInfo, ref ᒐ);
        nint size = 0;
        while (p.commentBefore(next)) {
            foreach (var (_, c) in (~p.comment).List) {
                size += len((~c).Text);
            }
            p.nextComment();
        }
        return size;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// recordLine records the output line number for the next non-whitespace
// token in *linePtr. It is used to compute an accurate line number for a
// formatted construct, independent of pending (not yet emitted) whitespace
// or comments.
internal static void recordLine(this ref printer p, ж<nint> ᏑlinePtr) {
    ref var linePtr = ref ᏑlinePtr.DerefOrNull();

    p.linePtr = ᏑlinePtr;
}

// linesFrom returns the number of output lines between the current
// output line and the line argument, ignoring any pending (not yet
// emitted) whitespace or comments. It is used to compute an accurate
// size (in number of lines) for a formatted construct.
internal static nint linesFrom(this ref printer p, nint line) {
    return p.@out.Line - line;
}

internal static tokenꓸPosition posFor(this ref printer p, tokenꓸPos pos) {
    // not used frequently enough to cache entire token.Position
    return p.fset.PositionFor(pos, false);
}

/* absolute position */
internal static nint lineFor(this ref printer p, tokenꓸPos pos) {
    if (pos != p.cachedPos) {
        p.cachedPos = pos;
        p.cachedLine = p.fset.PositionFor(pos, false).Line;
    }
    /* absolute position */
    return p.cachedLine;
}

// writeLineDirective writes a //line directive if necessary.
internal static void writeLineDirective(this ref printer p, tokenꓸPosition pos) {
    if (pos.IsValid() && (p.@out.Line != pos.Line || p.@out.Filename != pos.Filename)) {
        if (strings.ContainsAny(pos.Filename, "\r\n"u8)) {
            if (p.sourcePosErr == default!) {
                p.sourcePosErr = fmt.Errorf("go/printer: source filename contains unexpected newline character: %q"u8, pos.Filename);
            }
            return;
        }
        p.output = append(p.output, (byte)(tabwriter.Escape)); // protect '\n' in //line from tabwriter interpretation
        p.output = append(p.output, fmt.Sprintf("//line %s:%d\n"u8, pos.Filename, pos.Line).ꓸꓸꓸ);
        p.output = append(p.output, (byte)(tabwriter.Escape));
        // p.out must match the //line directive
        p.@out.Filename = pos.Filename;
        p.@out.Line = pos.Line;
    }
}

// writeIndent writes indentation.
internal static void writeIndent(this ref printer p) {
    // use "hard" htabs - indentation columns
    // must not be discarded by the tabwriter
    nint n = p.Config.Indent + p.indent; // include base indentation
    for (nint i = 0; i < n; i++) {
        p.output = append(p.output, (byte)((rune)'\t'));
    }
    // update positions
    p.pos.Offset += n;
    p.pos.Column += n;
    p.@out.Column += n;
}

// writeByte writes ch n times to p.output and updates p.pos.
// Only used to write formatting (white space) characters.
internal static void writeByte(this ref printer p, byte ch, nint n) {
    if (p.endAlignment) {
        // Ignore any alignment control character;
        // and at the end of the line, break with
        // a formfeed to indicate termination of
        // existing columns.
        switch (ch) {
        case (rune)'\t' or (rune)'\v': {
            ch = (rune)' ';
            break;
        }
        case (rune)'\n' or (rune)'\f': {
            ch = (rune)'\f';
            p.endAlignment = false;
            break;
        }}

    }
    if (p.@out.Column == 1) {
        // no need to write line directives before white space
        p.writeIndent();
    }
    for (nint i = 0; i < n; i++) {
        p.output = append(p.output, ch);
    }
    // update positions
    p.pos.Offset += n;
    if (ch == (rune)'\n' || ch == (rune)'\f') {
        p.pos.Line += n;
        p.@out.Line += n;
        p.pos.Column = 1;
        p.@out.Column = 1;
        return;
    }
    p.pos.Column += n;
    p.@out.Column += n;
}

// writeString writes the string s to p.output and updates p.pos, p.out,
// and p.last. If isLit is set, s is escaped w/ tabwriter.Escape characters
// to protect s from being interpreted by the tabwriter.
//
// Note: writeString is only used to write Go tokens, literals, and
// comments, all of which must be written literally. Thus, it is correct
// to always set isLit = true. However, setting it explicitly only when
// needed (i.e., when we don't know that s contains no tabs or line breaks)
// avoids processing extra escape characters and reduces run time of the
// printer benchmark by up to 10%.
internal static void writeString(this ref printer p, tokenꓸPosition pos, @string s, bool isLit) {
    if (p.@out.Column == 1) {
        if ((Mode)(p.Config.Mode & SourcePos) != 0) {
            p.writeLineDirective(pos);
        }
        p.writeIndent();
    }
    if (pos.IsValid()) {
        // update p.pos (if pos is invalid, continue with existing p.pos)
        // Note: Must do this after handling line beginnings because
        // writeIndent updates p.pos if there's indentation, but p.pos
        // is the position of s.
        p.pos = pos;
    }
    if (isLit) {
        // Protect s such that is passes through the tabwriter
        // unchanged. Note that valid Go programs cannot contain
        // tabwriter.Escape bytes since they do not appear in legal
        // UTF-8 sequences.
        p.output = append(p.output, (byte)(tabwriter.Escape));
    }
    if (debug) {
        p.output = append(p.output, fmt.Sprintf("/*%s*/"u8, pos).ꓸꓸꓸ); // do not update p.pos!
    }
    p.output = append(p.output, s.ꓸꓸꓸ);
    // update positions
    nint nlines = 0;
    nint li = default!;      // index of last newline; valid if nlines > 0
    for (nint i = 0; i < len(s); i++) {
        // Raw string literals may contain any character except back quote (`).
        {
            var ch = s[i]; if (ch == (rune)'\n' || ch == (rune)'\f') {
                // account for line break
                nlines++;
                li = i;
                // A line break inside a literal will break whatever column
                // formatting is in place; ignore any further alignment through
                // the end of the line.
                p.endAlignment = true;
            }
        }
    }
    p.pos.Offset += len(s);
    if (nlines > 0){
        p.pos.Line += nlines;
        p.@out.Line += nlines;
        nint c = len(s) - li;
        p.pos.Column = c;
        p.@out.Column = c;
    } else {
        p.pos.Column += len(s);
        p.@out.Column += len(s);
    }
    if (isLit) {
        p.output = append(p.output, (byte)(tabwriter.Escape));
    }
    p.last = p.pos;
}

// writeCommentPrefix writes the whitespace before a comment.
// If there is any pending whitespace, it consumes as much of
// it as is likely to help position the comment nicely.
// pos is the comment position, next the position of the item
// after all pending comments, prev is the previous comment in
// a group of comments (or nil), and tok is the next token.
internal static void writeCommentPrefix(this ref printer p, tokenꓸPosition pos, tokenꓸPosition next, ж<ast.Comment> Ꮡprev, token.Token tok) {
    ref var prev = ref Ꮡprev.DerefOrNull();

    if (len(p.output) == 0) {
        // the comment is the first item to be printed - don't write any whitespace
        return;
    }
    if (pos.IsValid() && pos.Filename != p.last.Filename) {
        // comment in a different file - separate with newlines
        p.writeByte((rune)'\f', maxNewlines);
        return;
    }
    if (pos.Line == p.last.Line && (Ꮡprev == nil || prev.Text[1] != (rune)'/')){
        // comment on the same line as last item:
        // separate with at least one separator
        var hasSep = false;
        if (Ꮡprev == nil) {
            // first comment of a comment group
            nint j = 0;
            foreach (var (i, ch) in p.wsbuf) {
                var exprᴛ1 = ch;
                if (exprᴛ1 == blank) {
                    p.wsbuf[i] = ignore;
                    continue;
                }
                else if (exprᴛ1 == vtab) {
                    hasSep = true;
                    continue;
                }
                else if (exprᴛ1 == indent) {
                    continue;
                }

                // ignore any blanks before a comment
                // respect existing tabs - important
                // for proper formatting of commented structs
                // apply pending indentation
                j = i;
                break;
            }
            p.writeWhitespace(j);
        }
        // make sure there is at least one separator
        if (!hasSep) {
            var sep = (byte)(rune)'\t';
            if (pos.Line == next.Line) {
                // next item is on the same line as the comment
                // (which must be a /*-style comment): separate
                // with a blank instead of a tab
                sep = (rune)' ';
            }
            p.writeByte(sep, 1);
        }
    } else {
        // comment on a different line:
        // separate with at least one line break
        var droppedLinebreak = false;
        nint j = 0;
        foreach (var (i, ch) in p.wsbuf) {
            var exprᴛ2 = ch;
            if (exprᴛ2 == blank || exprᴛ2 == vtab) {
                p.wsbuf[i] = ignore;
                continue;
            }
            else if (exprᴛ2 == indent) {
                continue;
            }
            else if (exprᴛ2 == unindent) {
                if (i + 1 < len(p.wsbuf) && p.wsbuf[i + 1] == unindent) {
                    // ignore any horizontal whitespace before line breaks
                    // apply pending indentation
                    // if this is not the last unindent, apply it
                    // as it is (likely) belonging to the last
                    // construct (e.g., a multi-line expression list)
                    // and is not part of closing a block
                    continue;
                }
                if (tok != token.RBRACE && pos.Column == next.Column) {
                    // if the next token is not a closing }, apply the unindent
                    // if it appears that the comment is aligned with the
                    // token; otherwise assume the unindent is part of a
                    // closing block and stop (this scenario appears with
                    // comments before a case label where the comments
                    // apply to the next case instead of the current one)
                    continue;
                }
            }
            else if (exprᴛ2 == newline || exprᴛ2 == formfeed) {
                p.wsbuf[i] = ignore;
                droppedLinebreak = Ꮡprev == nil; // record only if first comment of a group
            }

            j = i;
            break;
        }
        p.writeWhitespace(j);
        // determine number of linebreaks before the comment
        nint n = 0;
        if (pos.IsValid() && p.last.IsValid()) {
            n = pos.Line - p.last.Line;
            if (n < 0) {
                // should never happen
                n = 0;
            }
        }
        // at the package scope level only (p.indent == 0),
        // add an extra newline if we dropped one before:
        // this preserves a blank line before documentation
        // comments at the package scope level (issue 2570)
        if (p.indent == 0 && droppedLinebreak) {
            n++;
        }
        // make sure there is at least one line break
        // if the previous comment was a line comment
        if (n == 0 && Ꮡprev != nil && prev.Text[1] == (rune)'/') {
            n = 1;
        }
        if (n > 0) {
            // use formfeeds to break columns before a comment;
            // this is analogous to using formfeeds to separate
            // individual lines of /*-style comments
            p.writeByte((rune)'\f', nlimit(n));
        }
    }
}

// Returns true if s contains only white space
// (only tabs and blanks can appear in the printer's context).
internal static bool isBlank(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] > (rune)' ') {
            return false;
        }
    }
    return true;
}

// commonPrefix returns the common prefix of a and b.
internal static @string commonPrefix(@string a, @string b) {
    nint i = 0;
    while (i < len(a) && i < len(b) && a[i] == b[i] && (a[i] <= (rune)' ' || a[i] == (rune)'*')) {
        i++;
    }
    return a.slice(0, i);
}

// trimRight returns s with trailing whitespace removed.
internal static @string trimRight(@string s) {
    return strings.TrimRightFunc(s, unicode.IsSpace);
}

// stripCommonPrefix removes a common prefix from /*-style comment lines (unless no
// comment line is indented, all but the first line have some form of space prefix).
// The prefix is computed using heuristics such that is likely that the comment
// contents are nicely laid out after re-printing each line using the printer's
// current indentation.
internal static void stripCommonPrefix(slice<@string> lines) {
    if (len(lines) <= 1) {
        return; // at most one line - nothing to do
    }
    // len(lines) > 1
    // The heuristic in this function tries to handle a few
    // common patterns of /*-style comments: Comments where
    // the opening /* and closing */ are aligned and the
    // rest of the comment text is aligned and indented with
    // blanks or tabs, cases with a vertical "line of stars"
    // on the left, and cases where the closing */ is on the
    // same line as the last comment text.
    // Compute maximum common white prefix of all but the first,
    // last, and blank lines, and replace blank lines with empty
    // lines (the first line starts with /* and has no prefix).
    // In cases where only the first and last lines are not blank,
    // such as two-line comments, or comments where all inner lines
    // are blank, consider the last line for the prefix computation
    // since otherwise the prefix would be empty.
    //
    // Note that the first and last line are never empty (they
    // contain the opening /* and closing */ respectively) and
    // thus they can be ignored by the blank line check.
    @string prefix = ""u8;
    var prefixSet = false;
    if (len(lines) > 2) {
        foreach (var (i, line) in lines.slice(1, len(lines) - 1)) {
            if (isBlank(line)){
                lines[1 + i] = ""u8; // range starts with lines[1]
            } else {
                if (!prefixSet) {
                    prefix = line;
                    prefixSet = true;
                }
                prefix = commonPrefix(prefix, line);
            }
        }
    }
    // If we don't have a prefix yet, consider the last line.
    if (!prefixSet) {
        @string line = lines[len(lines) - 1];
        prefix = commonPrefix(line, line);
    }
    /*
	 * Check for vertical "line of stars" and correct prefix accordingly.
	 */
    var lineOfStars = false;
    {
        var (p, _, ok) = strings.Cut(prefix, "*"u8); if (ok){
            // remove trailing blank from prefix so stars remain aligned
            prefix = strings.TrimSuffix(p, " "u8);
            lineOfStars = true;
        } else {
            // No line of stars present.
            // Determine the white space on the first line after the /*
            // and before the beginning of the comment text, assume two
            // blanks instead of the /* unless the first character after
            // the /* is a tab. If the first comment line is empty but
            // for the opening /*, assume up to 3 blanks or a tab. This
            // whitespace may be found as suffix in the common prefix.
            @string first = lines[0];
            if (isBlank(first[2..])){
                // no comment text on the first line:
                // reduce prefix by up to 3 blanks or a tab
                // if present - this keeps comment text indented
                // relative to the /* and */'s if it was indented
                // in the first place
                nint i = len(prefix);
                for (nint n = 0; n < 3 && i > 0 && prefix[i - 1] == (rune)' '; n++) {
                    i--;
                }
                if (i == len(prefix) && i > 0 && prefix[i - 1] == (rune)'\t') {
                    i--;
                }
                prefix = prefix.slice(0, i);
            } else {
                // comment text on the first line
                var suffix = new slice<byte>(len(first));
                nint n = 2; // start after opening /*
                while (n < len(first) && first[n] <= (rune)' ') {
                    suffix[n] = first[n];
                    n++;
                }
                if (n > 2 && suffix[2] == (rune)'\t'){
                    // assume the '\t' compensates for the /*
                    suffix = suffix.slice(2, n);
                } else {
                    // otherwise assume two blanks
                    (suffix[0], suffix[1]) = ((rune)' ', (rune)' ');
                    suffix = suffix.slice(0, n);
                }
                // Shorten the computed common prefix by the length of
                // suffix, if it is found as suffix of the prefix.
                prefix = strings.TrimSuffix(prefix, ((@string)suffix));
            }
        }
    }
    // Handle last line: If it only contains a closing */, align it
    // with the opening /*, otherwise align the text with the other
    // lines.
    @string last = lines[len(lines) - 1];
    @string closing = "*/"u8;
    var (before, _, _) = strings.Cut(last, closing); // closing always present
    if (isBlank(before)){
        // last line only contains closing */
        if (lineOfStars) {
            closing = " */"u8; // add blank to align final star
        }
        lines[len(lines) - 1] = prefix + closing;
    } else {
        // last line contains more comment text - assume
        // it is aligned like the other lines and include
        // in prefix computation
        prefix = commonPrefix(prefix, last);
    }
    // Remove the common prefix from all but the first and empty lines.
    foreach (var (i, line) in lines) {
        if (i > 0 && line != ""u8) {
            lines[i] = line.slice(len(prefix));
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string linePrefixᶜ = "//line "u8;

internal static void writeComment(this ж<printer> Ꮡp, ж<ast.Comment> Ꮡcomment) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();
        ref var comment = ref Ꮡcomment.DerefOrNull();

        @string text = comment.Text;
        var pos = p.posFor(comment.Pos());
        @string linePrefix = linePrefixᶜ;
        if (strings.HasPrefix(text, linePrefix) && (!pos.IsValid() || pos.Column == 1)) {
            // Possibly a //-style line directive.
            // Suspend indentation temporarily to keep line directive valid.
            defer((nint indent) => {
                Ꮡp.Value.indent = indent;
            }, Ꮡp.Value.indent, ref ᒐ);
            p.indent = 0;
        }
        // shortcut common case of //-style comments
        if (text[1] == (rune)'/') {
            if (constraint.IsGoBuild(text)){
                p.goBuild = append(p.goBuild, len(p.output));
            } else 
            if (constraint.IsPlusBuild(text)) {
                p.plusBuild = append(p.plusBuild, len(p.output));
            }
            p.writeString(pos, trimRight(text), true);
            return;
        }
        // for /*-style comments, print line by line and let the
        // write function take care of the proper indentation
        var lines = strings.Split(text, "\n"u8);
        // The comment started in the first column but is going
        // to be indented. For an idempotent result, add indentation
        // to all lines such that they look like they were indented
        // before - this will make sure the common prefix computation
        // is the same independent of how many times formatting is
        // applied (was issue 1835).
        if (pos.IsValid() && pos.Column == 1 && p.indent > 0) {
            foreach (var (i, line) in lines[1..]) {
                lines[1 + i] = "   "u8 + line;
            }
        }
        stripCommonPrefix(lines);
        // write comment lines, separated by formfeed,
        // without a line break after the last line
        foreach (var (i, line) in lines) {
            if (i > 0) {
                p.writeByte((rune)'\f', 1);
                pos = p.pos;
            }
            if (len(line) > 0) {
                p.writeString(pos, trimRight(line), true);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// writeCommentSuffix writes a line break after a comment if indicated
// and processes any leftover indentation information. If a line break
// is needed, the kind of break (newline vs formfeed) depends on the
// pending whitespace. The writeCommentSuffix result indicates if a
// newline was written or if a formfeed was dropped from the whitespace
// buffer.
internal static (bool wroteNewline, bool droppedFF) writeCommentSuffix(this ref printer p, bool needsLinebreak) {
    bool wroteNewline = default!;
    bool droppedFF = default!;

    foreach (var (i, ch) in p.wsbuf) {
        var exprᴛ1 = ch;
        if (exprᴛ1 == blank || exprᴛ1 == vtab) {
            p.wsbuf[i] = ignore;
        }
        else if (exprᴛ1 == indent || exprᴛ1 == unindent) {
        }
        else if (exprᴛ1 == newline || exprᴛ1 == formfeed) {
            if (needsLinebreak){
                // ignore trailing whitespace
                // don't lose indentation information
                // if we need a line break, keep exactly one
                // but remember if we dropped any formfeeds
                needsLinebreak = false;
                wroteNewline = true;
            } else {
                if (ch == formfeed) {
                    droppedFF = true;
                }
                p.wsbuf[i] = ignore;
            }
        }

    }
    p.writeWhitespace(len(p.wsbuf));
    // make sure we have a line break
    if (needsLinebreak) {
        p.writeByte((rune)'\n', 1);
        wroteNewline = true;
    }
    return (wroteNewline, droppedFF);
}

// containsLinebreak reports whether the whitespace buffer contains any line breaks.
internal static bool containsLinebreak(this ref printer p) {
    foreach (var (_, ch) in p.wsbuf) {
        if (ch == newline || ch == formfeed) {
            return true;
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object intersperseCommentsˢ = (@string)"intersperseComments called without pending comments"u8;

// intersperseComments consumes all comments that appear before the next token
// tok and prints it together with the buffered whitespace (i.e., the whitespace
// that needs to be written before the next token). A heuristic is used to mix
// the comments and whitespace. The intersperseComments result indicates if a
// newline was written or if a formfeed was dropped from the whitespace buffer.
internal static (bool wroteNewline, bool droppedFF) intersperseComments(this ж<printer> Ꮡp, tokenꓸPosition next, token.Token tok) {
    bool wroteNewline = default!;
    bool droppedFF = default!;

    ref var p = ref Ꮡp.DerefOrNull();
    ж<ast.Comment> last = default!;
    while (p.commentBefore(next)) {
        var list = p.comment.Value.List;
        var changed = false;
        if (p.lastTok != token.IMPORT && p.posFor(p.comment.Pos()).Column == 1 && p.posFor(p.comment.End() + 1) == next) {
            // do not rewrite cgo's import "C" comments
            // Unindented comment abutting next token position:
            // a top-level doc comment.
            list = formatDocComment(list);
            changed = true;
            if (len((~p.comment).List) > 0 && len(list) == 0) {
                // The doc comment was removed entirely.
                // Keep preceding whitespace.
                p.writeCommentPrefix(p.posFor(p.comment.Pos()), next, last, tok);
                // Change print state to continue at next.
                p.pos = next;
                p.last = next;
                // There can't be any more comments.
                p.nextComment();
                return p.writeCommentSuffix(false);
            }
        }
        foreach (var (_, c) in list) {
            p.writeCommentPrefix(p.posFor(c.Pos()), next, last, tok);
            Ꮡp.writeComment(c);
            last = c;
        }
        // In case list was rewritten, change print state to where
        // the original list would have ended.
        if (len((~p.comment).List) > 0 && changed) {
            last = (~p.comment).List[len((~p.comment).List) - 1];
            p.pos = p.posFor(last.End());
            p.last = p.pos;
        }
        p.nextComment();
    }
    if (last != nil) {
        // If the last comment is a /*-style comment and the next item
        // follows on the same line but is not a comma, and not a "closing"
        // token immediately following its corresponding "opening" token,
        // add an extra separator unless explicitly disabled. Use a blank
        // as separator unless we have pending linebreaks, they are not
        // disabled, and we are outside a composite literal, in which case
        // we want a linebreak (issue 15137).
        // TODO(gri) This has become overly complicated. We should be able
        // to track whether we're inside an expression or statement and
        // use that information to decide more directly.
        var needsLinebreak = false;
        if ((pmode)(p.mode & noExtraBlank) == 0 && (~last).Text[1] == (rune)'*' && p.lineFor(last.Pos()) == next.Line && tok != token.COMMA && (tok != token.RPAREN || p.prevOpen == token.LPAREN) && (tok != token.RBRACK || p.prevOpen == token.LBRACK)) {
            if (p.containsLinebreak() && (pmode)(p.mode & noExtraLinebreak) == 0 && p.level == 0){
                needsLinebreak = true;
            } else {
                p.writeByte((rune)' ', 1);
            }
        }
        // Ensure that there is a line break after a //-style comment,
        // before EOF, and before a closing '}' unless explicitly disabled.
        if ((~last).Text[1] == (rune)'/' || tok == token.EOF || tok == token.RBRACE && (pmode)(p.mode & noExtraLinebreak) == 0) {
            needsLinebreak = true;
        }
        return p.writeCommentSuffix(needsLinebreak);
    }
    // no comment was written - we should never reach here since
    // intersperseComments should not be called in that case
    p.internalError(intersperseCommentsˢ);
    return (wroteNewline, droppedFF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object negativeIndentationˢ = (@string)"negative indentation:"u8;

// writeWhitespace writes the first n whitespace entries.
internal static void writeWhitespace(this ref printer p, nint n) {
    // write entries
    for (nint i = 0; i < n; i++) {
        {
            var ch = p.wsbuf[i];
            var exprᴛ1 = ch;
            var matchᴛ1 = false;
            if (exprᴛ1 == ignore) { matchᴛ1 = true;
            }
            else if (exprᴛ1 == indent) { matchᴛ1 = true;
                p.indent++;
            }
            else if (exprᴛ1 == unindent) { matchᴛ1 = true;
                p.indent--;
                if (p.indent < 0) {
                    // ignore!
                    p.internalError(negativeIndentationˢ, p.indent);
                    p.indent = 0;
                }
            }
            else if (exprᴛ1 == newline || exprᴛ1 == formfeed) { matchᴛ1 = true;
                if (i + 1 < n && p.wsbuf[i + 1] == unindent) {
                    // A line break immediately followed by a "correcting"
                    // unindent is swapped with the unindent - this permits
                    // proper label positioning. If a comment is between
                    // the line break and the label, the unindent is not
                    // part of the comment whitespace prefix and the comment
                    // will be positioned correctly indented.
                    // Use a formfeed to terminate the current section.
                    // Otherwise, a long label name on the next line leading
                    // to a wide column may increase the indentation column
                    // of lines before the label; effectively leading to wrong
                    // indentation.
                    (p.wsbuf[i], p.wsbuf[i + 1]) = (unindent, formfeed);
                    i--; // do it again
                    continue;
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                p.writeByte((byte)ch, 1);
            }
        }

    }
    // shift remaining entries down
    nint l = copy(p.wsbuf, p.wsbuf.slice(n));
    p.wsbuf = p.wsbuf.slice(0, l);
}

// ----------------------------------------------------------------------------
// Printing interface

// nlimit limits n to maxNewlines.
internal static nint nlimit(nint n) {
    return min(n, (nint)(maxNewlines));
}

internal static bool /*b*/ mayCombine(token.Token prev, byte next) {
    bool b = default!;

    var exprᴛ1 = prev;
    if (exprᴛ1 == token.INT) {
        b = next == (rune)'.'; // 1.
    }
    else if (exprᴛ1 == token.ADD) {
        b = next == (rune)'+'; // ++
    }
    else if (exprᴛ1 == token.SUB) {
        b = next == (rune)'-'; // --
    }
    else if (exprᴛ1 == token.QUO) {
        b = next == (rune)'*'; // /*
    }
    else if (exprᴛ1 == token.LSS) {
        b = next == (rune)'-' || next == (rune)'<'; // <- or <<
    }
    else if (exprᴛ1 == token.AND) {
        b = next == (rune)'&' || next == (rune)'^'; // && or &^
    }

    return b;
}

internal static void setPos(this ref printer p, tokenꓸPos pos) {
    if (pos.IsValid()) {
        p.pos = p.posFor(pos); // accurate position of next item
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object whitespaceBufferNotEmptyˢ = (@string)"whitespace buffer not empty"u8;

// print prints a list of "items" (roughly corresponding to syntactic
// tokens, but also including whitespace and formatting information).
// It is the only print function that should be called directly from
// any of the AST printing functions in nodes.go.
//
// Whitespace is accumulated until a non-whitespace token appears. Any
// comments that need to appear before that token are printed first,
// taking into account the amount and structure of any pending white-
// space for best comment placement. Then, any leftover whitespace is
// printed, followed by the actual token.
internal static void print(this ж<printer> Ꮡp, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var p = ref Ꮡp.DerefOrNull();
    foreach (var (_, arg) in args) {
        // information about the current arg
        @string data = default!;
        bool isLit = default!;
        bool impliedSemi = default!; // value for p.impliedSemi after this arg
        // record previous opening token, if any
        var exprᴛ1 = p.lastTok;
        if (exprᴛ1 == token.ILLEGAL) {
        }
        else if (exprᴛ1 == token.LPAREN || exprᴛ1 == token.LBRACK) {
            p.prevOpen = p.lastTok;
        }
        else { /* default: */
            p.prevOpen = token.ILLEGAL;
        }

        // ignore (white space)
        // other tokens followed any opening token
        switch (arg.type()) {
        case pmode x: {
            p.mode ^= (pmode)(x);
            continue;
            break;
        }
        case whiteSpace x: {
            if (x == ignore) {
                // toggle printer mode
                // don't add ignore's to the buffer; they
                // may screw up "correcting" unindents (see
                // LabeledStmt)
                continue;
            }
            nint i = len(p.wsbuf);
            if (i == cap(p.wsbuf)) {
                // Whitespace sequences are very short so this should
                // never happen. Handle gracefully (but possibly with
                // bad comment placement) if it does happen.
                p.writeWhitespace(i);
                i = 0;
            }
            p.wsbuf = p.wsbuf.slice(0, i + 1);
            p.wsbuf[i] = x;
            if (x == newline || x == formfeed) {
                // newlines affect the current state (p.impliedSemi)
                // and not the state after printing arg (impliedSemi)
                // because comments can be interspersed before the arg
                // in this case
                p.impliedSemi = false;
            }
            p.lastTok = token.ILLEGAL;
            continue;
            break;
        }
        case ж<ast.Ident> x: {
            data = x.Value.Name;
            impliedSemi = true;
            p.lastTok = token.IDENT;
            break;
        }
        case ж<ast.BasicLit> x: {
            data = x.Value.Value;
            isLit = true;
            impliedSemi = true;
            p.lastTok = x.Value.Kind;
            break;
        }
        case token.Token x: {
            @string s = x.String();
            if (mayCombine(p.lastTok, s[0])) {
                // the previous and the current token must be
                // separated by a blank otherwise they combine
                // into a different incorrect token sequence
                // (except for token.INT followed by a '.' this
                // should never happen because it is taken care
                // of via binary expression formatting)
                if (len(p.wsbuf) != 0) {
                    p.internalError(whitespaceBufferNotEmptyˢ);
                }
                p.wsbuf = p.wsbuf[0..1];
                p.wsbuf[0] = (rune)' ';
            }
            data = s;
            var exprᴛ2 = x;
            if (exprᴛ2 == token.BREAK || exprᴛ2 == token.CONTINUE || exprᴛ2 == token.FALLTHROUGH || exprᴛ2 == token.RETURN || exprᴛ2 == token.INC || exprᴛ2 == token.DEC || exprᴛ2 == token.RPAREN || exprᴛ2 == token.RBRACK || exprᴛ2 == token.RBRACE) {
                impliedSemi = true;
            }

            p.lastTok = x;
            break;
        }
        case @string x: {
            data = x;
            isLit = true;
            impliedSemi = true;
            p.lastTok = token.STRING;
            break;
        }
        default: {
            var x = arg;
            fmt.Fprintf(new os.FileжWriter(os.Stderr), // some keywords followed by a newline imply a semicolon
 // incorrect AST - print error message
 "print: unsupported argument %v (%T)\n"u8, arg, arg);
            throw panic("go/printer type");
            break;
        }}
        // data != ""
        var next = p.pos; // estimated/accurate position of next item
        var (wroteNewline, droppedFF) = Ꮡp.flush(next, p.lastTok);
        // intersperse extra newlines if present in the source and
        // if they don't cause extra semicolons (don't do this in
        // flush as it will cause extra newlines at the end of a file)
        if (!p.impliedSemi) {
            nint n = nlimit(next.Line - p.pos.Line);
            // don't exceed maxNewlines if we already wrote one
            if (wroteNewline && n == maxNewlines) {
                n = maxNewlines - 1;
            }
            if (n > 0) {
                var ch = (byte)(rune)'\n';
                if (droppedFF) {
                    ch = (rune)'\f'; // use formfeed since we dropped one before
                }
                p.writeByte(ch, n);
                impliedSemi = false;
            }
        }
        // the next token starts now - record its line number if requested
        if (p.linePtr != nil) {
            p.linePtr.Value = p.@out.Line;
            p.linePtr = default!;
        }
        p.writeString(next, data, isLit);
        p.impliedSemi = impliedSemi;
    }
}

// flush prints any pending comments and whitespace occurring textually
// before the position of the next token tok. The flush result indicates
// if a newline was written or if a formfeed was dropped from the whitespace
// buffer.
internal static (bool wroteNewline, bool droppedFF) flush(this ж<printer> Ꮡp, tokenꓸPosition next, token.Token tok) {
    bool wroteNewline = default!;
    bool droppedFF = default!;

    ref var p = ref Ꮡp.DerefOrNull();
    if (p.commentBefore(next)){
        // if there are comments before the next item, intersperse them
        (wroteNewline, droppedFF) = Ꮡp.intersperseComments(next, tok);
    } else {
        // otherwise, write any leftover whitespace
        p.writeWhitespace(len(p.wsbuf));
    }
    return (wroteNewline, droppedFF);
}

// getDoc returns the ast.CommentGroup associated with n, if any.
internal static ж<ast.CommentGroup> getDoc(ast.Node n) {
    switch (n.type()) {
    case ж<ast.Field> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.ImportSpec> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.ValueSpec> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.TypeSpec> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.GenDecl> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.FuncDecl> nΔ1: {
        return (~nΔ1).Doc;
    }
    case ж<ast.File> nΔ1: {
        return (~nΔ1).Doc;
    }}
    return default!;
}

internal static ж<ast.CommentGroup> getLastComment(ast.Node n) {
    switch (n.type()) {
    case ж<ast.Field> nΔ1: {
        return (~nΔ1).Comment;
    }
    case ж<ast.ImportSpec> nΔ1: {
        return (~nΔ1).Comment;
    }
    case ж<ast.ValueSpec> nΔ1: {
        return (~nΔ1).Comment;
    }
    case ж<ast.TypeSpec> nΔ1: {
        return (~nΔ1).Comment;
    }
    case ж<ast.GenDecl> nΔ1: {
        if (len((~nΔ1).Specs) > 0) {
            return getLastComment((~nΔ1).Specs[len((~nΔ1).Specs) - 1]);
        }
        break;
    }
    case ж<ast.File> nΔ1: {
        if (len((~nΔ1).Comments) > 0) {
            return (~nΔ1).Comments[len((~nΔ1).Comments) - 1];
        }
        break;
    }}
    return default!;
}

internal static error printNode(this ж<printer> Ꮡp, any node) {
    ref var p = ref Ꮡp.DerefOrNull();

    // unpack *CommentedNode, if any
    slice<ж<ast.CommentGroup>> comments = default!;
    {
        var (cnode, ok) = node._<ж<CommentedNode>>(ᐧ); if (ok) {
            node = cnode.Value.Node;
            comments = cnode.Value.Comments;
        }
    }
    if (comments != default!){
        // commented node - restrict comment list to relevant range
        var (n, ok) = node._<ast.Node>(ᐧ);
        if (!ok) {
            goto unsupported;
        }
        tokenꓸPos beg = n.Pos();
        tokenꓸPos end = n.End();
        // if the node has associated documentation,
        // include that commentgroup in the range
        // (the comment list is sorted in the order
        // of the comment appearance in the source code)
        {
            var doc = getDoc(n); if (doc != nil) {
                beg = doc.Pos();
            }
        }
        {
            var com = getLastComment(n); if (com != nil) {
                {
                    tokenꓸPos e = com.End(); if (e > end) {
                        end = e;
                    }
                }
            }
        }
        // token.Pos values are global offsets, we can
        // compare them directly
        nint i = 0;
        while (i < len(comments) && comments[i].End() < beg) {
            i++;
        }
        nint j = i;
        while (j < len(comments) && comments[j].Pos() < end) {
            j++;
        }
        if (i < j) {
            p.comments = comments.slice(i, j);
        }
    } else 
    {
        var (n, ok) = node._<ж<ast.File>>(ᐧ); if (ok) {
            // use ast.File comments, if any
            p.comments = n.Value.Comments;
        }
    }
    // if there are no comments, use node comments
    p.useNodeComments = p.comments == default!;
    // get comments ready for use
    p.nextComment();
    Ꮡp.print(((pmode)0));
    // format node
    switch (node.type()) {
    case {} Δn when Δn._<ast.Expr>(out var n): {
        Ꮡp.expr(n);
        break;
    }
    case {} Δn when Δn._<ast.Stmt>(out var n): {
        {
            var (_, ok) = n._<ж<ast.LabeledStmt>>(ᐧ); if (ok) {
                // A labeled statement will un-indent to position the label.
                // Set p.indent to 1 so we don't get indent "underflow".
                p.indent = 1;
            }
        }
        Ꮡp.stmt(n, false);
        break;
    }
    case {} Δn when Δn._<ast.Decl>(out var n): {
        Ꮡp.decl(n);
        break;
    }
    case {} Δn when Δn._<ast.Spec>(out var n): {
        Ꮡp.spec(n, 1, false);
        break;
    }
    case slice<ast.Stmt> n: {
        foreach (var (_, s) in n) {
            // A labeled statement will un-indent to position the label.
            // Set p.indent to 1 so we don't get indent "underflow".
            {
                var (_, ok) = s._<ж<ast.LabeledStmt>>(ᐧ); if (ok) {
                    p.indent = 1;
                }
            }
        }
        Ꮡp.stmtList(n, 0, false);
        break;
    }
    case slice<ast.Decl> n: {
        Ꮡp.declList(n);
        break;
    }
    case ж<ast.File> n: {
        Ꮡp.@file(n);
        break;
    }
    default: {
        var n = node;
        goto unsupported;
        break;
    }}
    return p.sourcePosErr;
unsupported:
    return fmt.Errorf("go/printer: unsupported node type %T"u8, node);
}

// ----------------------------------------------------------------------------
// Trimmer

// A trimmer is an io.Writer filter for stripping tabwriter.Escape
// characters, trailing blanks and tabs, and for converting formfeed
// and vtab characters into newlines and htabs (in case no tabwriter
// is used). Text bracketed by tabwriter.Escape characters is passed
// through unchanged.
partial struct trimmer {
    internal io.Writer output;
    internal nint state;
    internal slice<byte> space;
}

// trimmer is implemented as a state machine.
// It can be in one of the following states:
internal static UntypedInt inSpace => iota; // inside space

internal static UntypedInt inEscape => 1; // inside text bracketed by tabwriter.Escapes

internal static UntypedInt inText => 2; // inside text

internal static void resetSpace(this ref trimmer p) {
    p.state = inSpace;
    p.space = p.space[0..0];
}

// Design note: It is tempting to eliminate extra blanks occurring in
//              whitespace in this function as it could simplify some
//              of the blanks logic in the node printing functions.
//              However, this would mess up any formatting done by
//              the tabwriter.
internal static slice<byte> aNewline = slice<byte>("\n"u8);

internal static (nint n, error err) Write(this ref trimmer p, slice<byte> data) {
    nint nΔ1 = default!;
    error err = default!;

    // invariants:
    // p.state == inSpace:
    //	p.space is unwritten
    // p.state == inEscape, inText:
    //	data[m:n] is unwritten
    nint m = 0;
    byte b = default!;
    foreach (var (iᴛ1, vᴛ1) in data) {
        nΔ1 = iᴛ1;
        b = vᴛ1;

        if (b == (rune)'\v') {
            b = (rune)'\t'; // convert to htab
        }
        var exprᴛ1 = p.state;
        if (exprᴛ1 == inSpace) {
            var exprᴛ2 = b;
            if (exprᴛ2 is (rune)'\t' or (rune)' ') {
                p.space = append(p.space, b);
            }
            else if (exprᴛ2 is (rune)'\n' or (rune)'\f') {
                p.resetSpace(); // discard trailing space
                (_, err) = p.output.Write(aNewline);
            }
            else if (exprᴛ2 == tabwriter.Escape) {
                (_, err) = p.output.Write(p.space);
                p.state = inEscape;
                m = nΔ1 + 1; // +1: skip tabwriter.Escape
            }
            else { /* default: */
                (_, err) = p.output.Write(p.space);
                p.state = inText;
                m = nΔ1;
            }

        }
        else if (exprᴛ1 == inEscape) {
            if (b == tabwriter.Escape) {
                (_, err) = p.output.Write(data.slice(m, nΔ1));
                p.resetSpace();
            }
        }
        else if (exprᴛ1 == inText) {
            var exprᴛ3 = b;
            if (exprᴛ3 is (rune)'\t' or (rune)' ') {
                (_, err) = p.output.Write(data.slice(m, nΔ1));
                p.resetSpace();
                p.space = append(p.space, b);
            }
            else if (exprᴛ3 is (rune)'\n' or (rune)'\f') {
                (_, err) = p.output.Write(data.slice(m, nΔ1));
                p.resetSpace();
                if (err == default!) {
                    (_, err) = p.output.Write(aNewline);
                }
            }
            else if (exprᴛ3 == tabwriter.Escape) {
                (_, err) = p.output.Write(data.slice(m, nΔ1));
                p.state = inEscape;
                m = nΔ1 + 1; // +1: skip tabwriter.Escape
            }

        }
        else { /* default: */
            throw panic("unreachable");
        }

        if (err != default!) {
            return (nΔ1, err);
        }
    }
    nΔ1 = len(data);
    var exprᴛ4 = p.state;
    if (exprᴛ4 == inEscape || exprᴛ4 == inText) {
        (_, err) = p.output.Write(data.slice(m, nΔ1));
        p.resetSpace();
    }

    return (nΔ1, err);
}

partial struct Mode /*num:nuint*/;

// ----------------------------------------------------------------------------
// Public interface
public static Mode RawFormat => /* 1 << iota */ 1;       // do not use a tabwriter; if set, UseSpaces is ignored
public static Mode TabIndent => 2;       // use tabs for indentation independent of UseSpaces
public static Mode UseSpaces => 4;       // use spaces instead of tabs for alignment
public static Mode SourcePos => 8;       // emit //line directives to preserve original source positions

// The mode below is not included in printer's public API because
// editing code text is deemed out of scope. Because this mode is
// unexported, it's also possible to modify or remove it based on
// the evolving needs of go/format and cmd/gofmt without breaking
// users. See discussion in CL 240683.
internal static Mode normalizeNumbers => /* 1 << 30 */ 1073741824;

// A Config node controls the output of Fprint.
partial struct Config {
    public Mode Mode; // default: 0
    public nint Tabwidth; // default: 8
    public nint Indent; // default: 0 (all code is indented at least by this much)
}

// Whitespace sequences are short.
// We start the printer with a 16K output buffer, which is currently
// larger than about 80% of Go files in the standard library.
internal static ж<sync.Pool> ᏑprinterPool = new StandardBox<sync.Pool>(new sync.Pool(
    New: () => Ꮡ(new printer(
            wsbuf: new slice<whiteSpace>(0, 16),
            output: new slice<byte>(0, (16 << (int)(10)))
        ))
));
internal static ref sync.Pool printerPool => ref ᏑprinterPool.Value;

internal static ж<printer> newPrinter(ref Config cfg, ж<token.FileSet> Ꮡfset, map<ast.Node, nint> nodeSizes) {
    var p = ᏑprinterPool.Get()._<ж<printer>>();
    p.Value = new printer(
        Config: cfg,
        fset: Ꮡfset,
        pos: new tokenꓸPosition(Line: 1, Column: 1),
        @out: new tokenꓸPosition(Line: 1, Column: 1),
        wsbuf: (~p).wsbuf[..0],
        nodeSizes: nodeSizes,
        cachedPos: -1,
        output: (~p).output[..0]
    );
    return p;
}

internal static void free(this ж<printer> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Hard limit on buffer size; see https://golang.org/issue/23199.
    if (cap(p.output) > (64 << (int)(10))) {
        return;
    }
    ᏑprinterPool.Put(Ꮡp.OrTypedNil());
}

// fprint implements Fprint and takes a nodesSizes map for setting up the printer state.
internal static error /*err*/ fprint(this ж<Config> Ꮡcfg, io.Writer output, ж<token.FileSet> Ꮡfset, any node, map<ast.Node, nint> nodeSizes) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var cfg = ref Ꮡcfg.DerefOrNull();

        // print node
        var p = newPrinter(ref (Ꮡcfg).DerefOrNull(), Ꮡfset, nodeSizes);
        var pʗ1 = p;
        defer(pʗ1.free, ref ᒐ);
        {
            err = p.printNode(node); if (err != default!) {
                goto ᒐdone;
            }
        }
        // print outstanding comments
        p.Value.impliedSemi = false; // EOF acts like a newline
        p.flush(new tokenꓸPosition(Offset: infinity, Line: infinity), token.EOF);
        // output is buffered in p.output now.
        // fix //go:build and // +build comments if needed.
        p.fixGoBuildLines();
        // redirect output through a trimmer to eliminate trailing whitespace
        // (Input to a tabwriter must be untrimmed since trailing tabs provide
        // formatting information. The tabwriter could provide trimming
        // functionality but no tabwriter is used when RawFormat is set.)
        output = new trimmerжWriter(Ꮡ(new trimmer(output: output)));
        // redirect output through a tabwriter if necessary
        if ((Mode)(cfg.Mode & RawFormat) == 0) {
            nint minwidth = cfg.Tabwidth;
            var padchar = (byte)(rune)'\t';
            if ((Mode)(cfg.Mode & UseSpaces) != 0) {
                padchar = (rune)' ';
            }
            nuint twmode = tabwriter.DiscardEmptyColumns;
            if ((Mode)(cfg.Mode & TabIndent) != 0) {
                minwidth = 0;
                twmode |= (nuint)(tabwriter.TabIndent);
            }
            output = new tabwriter_WriterжWriter(tabwriter.NewWriter(output, minwidth, cfg.Tabwidth, 1, padchar, twmode));
        }
        // write printer result via tabwriter/trimmer to output
        {
            (_, err) = output.Write((~p).output); if (err != default!) {
                goto ᒐdone;
            }
        }
        // flush tabwriter, if any
        {
            var (tw, _) = output._<ж<tabwriter.Writer>>(ᐧ); if (tw != nil) {
                err = tw.Flush();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// A CommentedNode bundles an AST node and corresponding comments.
// It may be provided as argument to any of the [Fprint] functions.
partial struct CommentedNode {
    public any Node; // *ast.File, or ast.Expr, ast.Decl, ast.Spec, or ast.Stmt
    public slice<ж<ast.CommentGroup>> Comments;
}

// Fprint "pretty-prints" an AST node to output for a given configuration cfg.
// Position information is interpreted relative to the file set fset.
// The node type must be *[ast.File], *[CommentedNode], [][ast.Decl], [][ast.Stmt],
// or assignment-compatible to [ast.Expr], [ast.Decl], [ast.Spec], or [ast.Stmt].
public static error Fprint(this ж<Config> Ꮡcfg, io.Writer output, ж<token.FileSet> Ꮡfset, any node) {
    return Ꮡcfg.fprint(output, Ꮡfset, node, new map<ast.Node, nint>());
}

// Fprint "pretty-prints" an AST node to output.
// It calls [Config.Fprint] with default settings.
// Note that gofmt uses tabs for indentation but spaces for alignment;
// use format.Node (package go/format) for output that matches gofmt.
public static error Fprint(io.Writer output, ж<token.FileSet> Ꮡfset, any node) {
    return (Ꮡ(new Config(Tabwidth: 8))).Fprint(output, Ꮡfset, node);
}

} // end printer_package
