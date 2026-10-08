// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using slices = slices_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;

partial class comment_package {

// A Doc is a parsed Go doc comment.
partial struct Doc {
    // Content is the sequence of content blocks in the comment.
    public slice<Block> Content;
    // Links is the link definitions in the comment.
    public slice<ж<LinkDef>> Links;
}

// A LinkDef is a single link definition.
partial struct LinkDef {
    public @string Text; // the link text
    public @string URL; // the link URL
    public bool Used;   // whether the comment uses the definition
}

// A Block is block-level content in a doc comment,
// one of [*Code], [*Heading], [*List], or [*Paragraph].
partial interface Block {
    void block();
}

// A Heading is a doc comment heading.
partial struct Heading {
    public slice<ΔText> Text; // the heading text
}

internal static void block(this ref Heading _) {
}

// A List is a numbered or bullet list.
// Lists are always non-empty: len(Items) > 0.
// In a numbered list, every Items[i].Number is a non-empty string.
// In a bullet list, every Items[i].Number is an empty string.
partial struct List {
    // Items is the list items.
    public slice<ж<ListItem>> Items;
    // ForceBlankBefore indicates that the list must be
    // preceded by a blank line when reformatting the comment,
    // overriding the usual conditions. See the BlankBefore method.
    //
    // The comment parser sets ForceBlankBefore for any list
    // that is preceded by a blank line, to make sure
    // the blank line is preserved when printing.
    public bool ForceBlankBefore;
    // ForceBlankBetween indicates that list items must be
    // separated by blank lines when reformatting the comment,
    // overriding the usual conditions. See the BlankBetween method.
    //
    // The comment parser sets ForceBlankBetween for any list
    // that has a blank line between any two of its items, to make sure
    // the blank lines are preserved when printing.
    public bool ForceBlankBetween;
}

internal static void block(this ref List _) {
}

// BlankBefore reports whether a reformatting of the comment
// should include a blank line before the list.
// The default rule is the same as for [BlankBetween]:
// if the list item content contains any blank lines
// (meaning at least one item has multiple paragraphs)
// then the list itself must be preceded by a blank line.
// A preceding blank line can be forced by setting [List].ForceBlankBefore.
public static bool BlankBefore(this ref List l) {
    return l.ForceBlankBefore || l.BlankBetween();
}

// BlankBetween reports whether a reformatting of the comment
// should include a blank line between each pair of list items.
// The default rule is that if the list item content contains any blank lines
// (meaning at least one item has multiple paragraphs)
// then list items must themselves be separated by blank lines.
// Blank line separators can be forced by setting [List].ForceBlankBetween.
public static bool BlankBetween(this ref List l) {
    if (l.ForceBlankBetween) {
        return true;
    }
    foreach (var (_, item) in l.Items) {
        if (len((~item).Content) != 1) {
            // Unreachable for parsed comments today,
            // since the only way to get multiple item.Content
            // is multiple paragraphs, which must have been
            // separated by a blank line.
            return true;
        }
    }
    return false;
}

// A ListItem is a single item in a numbered or bullet list.
partial struct ListItem {
    // Number is a decimal string in a numbered list
    // or an empty string in a bullet list.
    public @string Number; // "1", "2", ...; "" for bullet list
    // Content is the list content.
    // Currently, restrictions in the parser and printer
    // require every element of Content to be a *Paragraph.
    public slice<Block> Content; // Content of this item.
}

// A Paragraph is a paragraph of text.
partial struct Paragraph {
    public slice<ΔText> Text;
}

internal static void block(this ref Paragraph _) {
}

// A Code is a preformatted code block.
partial struct Code {
    // Text is the preformatted text, ending with a newline character.
    // It may be multiple lines, each of which ends with a newline character.
    // It is never empty, nor does it start or end with a blank line.
    public @string Text;
}

internal static void block(this ref Code _) {
}

// A Text is text-level content in a doc comment,
// one of [Plain], [Italic], [*Link], or [*DocLink].
partial interface ΔText {
    void text();
}

partial struct Plain /*@string*/;

internal static void text(this Plain _) {
}

partial struct Italic /*@string*/;

internal static void text(this Italic _) {
}

// A Link is a link to a specific URL.
partial struct Link {
    public bool Auto;   // is this an automatic (implicit) link of a literal URL?
    public slice<ΔText> Text; // text of link
    public @string URL; // target URL of link
}

internal static void text(this ref Link _) {
}

// A DocLink is a link to documentation for a Go package or symbol.
partial struct DocLink {
    public slice<ΔText> Text; // text of link
    // ImportPath, Recv, and Name identify the Go package or symbol
    // that is the link target. The potential combinations of
    // non-empty fields are:
    //  - ImportPath: a link to another package
    //  - ImportPath, Name: a link to a const, func, type, or var in another package
    //  - ImportPath, Recv, Name: a link to a method in another package
    //  - Name: a link to a const, func, type, or var in this package
    //  - Recv, Name: a link to a method in this package
    public @string ImportPath; // import path
    public @string Recv; // receiver type, without any pointer star, for methods
    public @string Name; // const, func, type, var, or method name
}

internal static void text(this ref DocLink _) {
}

// A Parser is a doc comment parser.
// The fields in the struct can be filled in before calling [Parser.Parse]
// in order to customize the details of the parsing process.
partial struct Parser {
    // Words is a map of Go identifier words that
    // should be italicized and potentially linked.
    // If Words[w] is the empty string, then the word w
    // is only italicized. Otherwise it is linked, using
    // Words[w] as the link target.
    // Words corresponds to the [go/doc.ToHTML] words parameter.
    public map<@string, @string> Words;
    // LookupPackage resolves a package name to an import path.
    //
    // If LookupPackage(name) returns ok == true, then [name]
    // (or [name.Sym] or [name.Sym.Method])
    // is considered a documentation link to importPath's package docs.
    // It is valid to return "", true, in which case name is considered
    // to refer to the current package.
    //
    // If LookupPackage(name) returns ok == false,
    // then [name] (or [name.Sym] or [name.Sym.Method])
    // will not be considered a documentation link,
    // except in the case where name is the full (but single-element) import path
    // of a package in the standard library, such as in [math] or [io.Reader].
    // LookupPackage is still called for such names,
    // in order to permit references to imports of other packages
    // with the same package names.
    //
    // Setting LookupPackage to nil is equivalent to setting it to
    // a function that always returns "", false.
    public Func<@string, (@string importPath, bool ok)> LookupPackage;
    // LookupSym reports whether a symbol name or method name
    // exists in the current package.
    //
    // If LookupSym("", "Name") returns true, then [Name]
    // is considered a documentation link for a const, func, type, or var.
    //
    // Similarly, if LookupSym("Recv", "Name") returns true,
    // then [Recv.Name] is considered a documentation link for
    // type Recv's method Name.
    //
    // Setting LookupSym to nil is equivalent to setting it to a function
    // that always returns false.
    public Func<@string, @string, bool> LookupSym;
}

// parseDoc is parsing state for a single doc comment.
partial struct parseDoc {
    public partial ref ж<Parser> Parser { get; }
    public partial ref ж<Doc> Doc { get; }
    internal map<@string, ж<LinkDef>> links;
    internal slice<@string> lines;
    internal Func<@string, @string, bool> lookupSym;
}

// lookupPkg is called to look up the pkg in [pkg], [pkg.Name], and [pkg.Name.Recv].
// If pkg has a slash, it is assumed to be the full import path and is returned with ok = true.
//
// Otherwise, pkg is probably a simple package name like "rand" (not "crypto/rand" or "math/rand").
// d.LookupPackage provides a way for the caller to allow resolving such names with reference
// to the imports in the surrounding package.
//
// There is one collision between these two cases: single-element standard library names
// like "math" are full import paths but don't contain slashes. We let d.LookupPackage have
// the first chance to resolve it, in case there's a different package imported as math,
// and otherwise we refer to a built-in list of single-element standard library package names.
internal static (@string importPath, bool ok) lookupPkg(this ref parseDoc d, @string pkg) {
    if (strings.Contains(pkg, "/"u8)) {
        // assume a full import path
        if (validImportPath(pkg)) {
            return (pkg, true);
        }
        return ("", false);
    }
    if (d.LookupPackage != default!) {
        // Give LookupPackage a chance.
        {
            var (path, okΔ1) = d.LookupPackage(pkg); if (okΔ1) {
                return (path, true);
            }
        }
    }
    return DefaultLookupPackage(pkg);
}

internal static bool isStdPkg(@string path) {
    var (_, ok) = slices.BinarySearch(stdPkgs, path);
    return ok;
}

// DefaultLookupPackage is the default package lookup
// function, used when [Parser.LookupPackage] is nil.
// It recognizes names of the packages from the standard
// library with single-element import paths, such as math,
// which would otherwise be impossible to name.
//
// Note that the go/doc package provides a more sophisticated
// lookup based on the imports used in the current package.
public static (@string importPath, bool ok) DefaultLookupPackage(@string name) {
    if (isStdPkg(name)) {
        return (name, true);
    }
    return ("", false);
}

// Parse parses the doc comment text and returns the *[Doc] form.
// Comment markers (/* // and */) in the text must have already been removed.
public static ж<Doc> Parse(this ж<Parser> Ꮡp, @string text) {
    ref var p = ref Ꮡp.DerefOrNull();

    var lines = unindent(strings.Split(text, "\n"u8));
    var d = Ꮡ(new parseDoc(
        Parser: Ꮡp,
        Doc: @new<Doc>(),
        links: new map<@string, ж<LinkDef>>(),
        lines: lines,
        lookupSym: (@string recv, @string name) => false
    ));
    if (p.LookupSym != default!) {
        d.Value.lookupSym = p.LookupSym;
    }
    // First pass: break into block structure and collect known links.
    // The text is all recorded as Plain for now.
    span prev = default!;
    foreach (var (_, s) in parseSpans(lines)) {
        Block b = default!;
        var exprᴛ1 = s.kind;
        if (exprᴛ1 == spanList) {
            b = new ListжBlock(d.list(lines.slice(s.start, s.end), prev.end < s.start));
        }
        else if (exprᴛ1 == spanCode) {
            b = new CodeжBlock(d.code(lines.slice(s.start, s.end)));
        }
        else if (exprᴛ1 == spanOldHeading) {
            b = d.oldHeading(lines[s.start]);
        }
        else if (exprᴛ1 == spanHeading) {
            b = d.heading(lines[s.start]);
        }
        else if (exprᴛ1 == spanPara) {
            b = d.paragraph(lines.slice(s.start, s.end));
        }
        else { /* default: */
            throw panic("go/doc/comment: internal error: unknown span kind");
        }

        if (b != default!) {
            d.Value.Content = append((~d).Content, b);
        }
        prev = s;
    }
    // Second pass: interpret all the Plain text now that we know the links.
    foreach (var (_, b) in (~d).Content) {
        switch (b.type()) {
        case ж<Paragraph> bΔ1: {
            bΔ1.Value.Text = d.parseLinkedText(((@string)((~bΔ1).Text[0]._<Plain>())));
            break;
        }
        case ж<List> bΔ1: {
            foreach (var (_, i) in (~bΔ1).Items) {
                foreach (var (_, c) in (~i).Content) {
                    var pΔ1 = c._<ж<Paragraph>>();
                    pΔ1.Value.Text = d.parseLinkedText(((@string)((~pΔ1).Text[0]._<Plain>())));
                }
            }
            break;
        }}
    }
    return (~d).Doc;
}

// A span represents a single span of comment lines (lines[start:end])
// of an identified kind (code, heading, paragraph, and so on).
partial struct span {
    internal nint start;
    internal nint end;
    internal spanKind kind;
}

partial struct spanKind /*num:nint*/;

internal static spanKind _ᴛ1ʗ => /* iota */ 0;
internal static spanKind spanCode => 1;
internal static spanKind spanHeading => 2;
internal static spanKind spanList => 3;
internal static spanKind spanOldHeading => 4;
internal static spanKind spanPara => 5;

internal static slice<span> parseSpans(slice<@string> lines) {
    slice<span> spans = default!;
    // The loop may process a line twice: once as unindented
    // and again forced indented. So the maximum expected
    // number of iterations is 2*len(lines). The repeating logic
    // can be subtle, though, and to protect against introduction
    // of infinite loops in future changes, we watch to see that
    // we are not looping too much. A panic is better than a
    // quiet infinite loop.
    nint watchdog = 2 * len(lines);
    nint i = 0;
    nint forceIndent = 0;
Spans:
    while (ᐧ) {
        // Skip blank lines.
        while (i < len(lines) && lines[i] == "") {
            i++;
        }
        if (i >= len(lines)) {
            break;
        }
        {
            watchdog--; if (watchdog < 0) {
                throw panic("go/doc/comment: internal error: not making progress");
            }
        }
        spanKind kind = default!;
        nint start = i;
        nint end = i;
        if (i < forceIndent || indented(lines[i])){
            // Indented (or force indented).
            // Ends before next unindented. (Blank lines are OK.)
            // If this is an unindented list that we are heuristically treating as indented,
            // then accept unindented list item lines up to the first blank lines.
            // The heuristic is disabled at blank lines to contain its effect
            // to non-gofmt'ed sections of the comment.
            var unindentedListOK = isList(lines[i]) && i < forceIndent;
            i++;
            while (i < len(lines) && (lines[i] == "" || i < forceIndent || indented(lines[i]) || (unindentedListOK && isList(lines[i])))) {
                if (lines[i] == "") {
                    unindentedListOK = false;
                }
                i++;
            }
            // Drop trailing blank lines.
            end = i;
            while (end > start && lines[end - 1] == "") {
                end--;
            }
            // If indented lines are followed (without a blank line)
            // by an unindented line ending in a brace,
            // take that one line too. This fixes the common mistake
            // of pasting in something like
            //
            // func main() {
            //	fmt.Println("hello, world")
            // }
            //
            // and forgetting to indent it.
            // The heuristic will never trigger on a gofmt'ed comment,
            // because any gofmt'ed code block or list would be
            // followed by a blank line or end of comment.
            if (end < len(lines) && strings.HasPrefix(lines[end], "}"u8)) {
                end++;
            }
            if (isList(lines[start])){
                kind = spanList;
            } else {
                kind = spanCode;
            }
        } else {
            // Unindented. Ends at next blank or indented line.
            i++;
            while (i < len(lines) && lines[i] != "" && !indented(lines[i])) {
                i++;
            }
            end = i;
            // If unindented lines are followed (without a blank line)
            // by an indented line that would start a code block,
            // check whether the final unindented lines
            // should be left for the indented section.
            // This can happen for the common mistakes of
            // unindented code or unindented lists.
            // The heuristic will never trigger on a gofmt'ed comment,
            // because any gofmt'ed code block would have a blank line
            // preceding it after the unindented lines.
            if (i < len(lines) && lines[i] != "" && !isList(lines[i])) {
                switch (ᐧ) {
                case {} when isList(lines[i - 1]): {
                    forceIndent = end;
                    end--;
                    while (end > start && isList(lines[end - 1])) {
                        // If the final unindented line looks like a list item,
                        // this may be the first indented line wrap of
                        // a mistakenly unindented list.
                        // Leave all the unindented list items.
                        end--;
                    }
                    break;
                }
                case {} when strings.HasSuffix(lines[i - 1], "{"u8) || strings.HasSuffix(lines[i - 1], @"\"u8): {
                    forceIndent = end;
                    end--;
                    break;
                }}

                // If the final unindented line ended in { or \
                // it is probably the start of a misindented code block.
                // Give the user a single line fix.
                // Often that's enough; if not, the user can fix the others themselves.
                if (start == end && forceIndent > start) {
                    i = start;
                    goto continue_Spans;
                }
            }
            // Span is either paragraph or heading.
            if (end - start == 1 && isHeading(lines[start])){
                kind = spanHeading;
            } else 
            if (end - start == 1 && isOldHeading(lines[start], lines, start)){
                kind = spanOldHeading;
            } else {
                kind = spanPara;
            }
        }
        spans = append(spans, new span(start, end, kind));
        i = end;
continue_Spans:;
    }
break_Spans:;
    return spans;
}

// indented reports whether line is indented
// (starts with a leading space or tab).
internal static bool indented(@string line) {
    return line != ""u8 && (line[0] == (rune)' ' || line[0] == (rune)'\t');
}

// unindent removes any common space/tab prefix
// from each line in lines, returning a copy of lines in which
// those prefixes have been trimmed from each line.
// It also replaces any lines containing only spaces with blank lines (empty strings).
internal static slice<@string> unindent(slice<@string> lines) {
    // Trim leading and trailing blank lines.
    while (len(lines) > 0 && isBlank(lines[0])) {
        lines = lines[1..];
    }
    while (len(lines) > 0 && isBlank(lines[len(lines) - 1])) {
        lines = lines.slice(0, len(lines) - 1);
    }
    if (len(lines) == 0) {
        return default!;
    }
    // Compute and remove common indentation.
    @string prefix = leadingSpace(lines[0]);
    foreach (var (_, line) in lines[1..]) {
        if (!isBlank(line)) {
            prefix = commonPrefix(prefix, leadingSpace(line));
        }
    }
    var @out = new slice<@string>(len(lines));
    foreach (var (i, vᴛ1) in lines) {
        var line = vᴛ1;

        line = strings.TrimPrefix(line, prefix);
        if (strings.TrimSpace(line) == ""u8) {
            line = ""u8;
        }
        @out[i] = line;
    }
    while (len(@out) > 0 && @out[0] == "") {
        @out = @out[1..];
    }
    while (len(@out) > 0 && @out[len(@out) - 1] == "") {
        @out = @out.slice(0, len(@out) - 1);
    }
    return @out;
}

// isBlank reports whether s is a blank line.
internal static bool isBlank(@string s) {
    return len(s) == 0 || (len(s) == 1 && s[0] == (rune)'\n');
}

// commonPrefix returns the longest common prefix of a and b.
internal static @string commonPrefix(@string a, @string b) {
    nint i = 0;
    while (i < len(a) && i < len(b) && a[i] == b[i]) {
        i++;
    }
    return a.slice(0, i);
}

// leadingSpace returns the longest prefix of s consisting of spaces and tabs.
internal static @string leadingSpace(@string s) {
    nint i = 0;
    while (i < len(s) && (s[i] == (rune)' ' || s[i] == (rune)'\t')) {
        i++;
    }
    return s.slice(0, i);
}

// isOldHeading reports whether line is an old-style section heading.
// line is all[off].
internal static bool isOldHeading(@string line, slice<@string> all, nint off) {
    if (off <= 0 || all[off - 1] != "" || off + 2 >= len(all) || all[off + 1] != "" || leadingSpace(all[off + 2]) != ""u8) {
        return false;
    }
    line = strings.TrimSpace(line);
    // a heading must start with an uppercase letter
    var (r, _) = utf8.DecodeRuneInString(line);
    if (!unicode.IsLetter(r) || !unicode.IsUpper(r)) {
        return false;
    }
    // it must end in a letter or digit:
    (r, _) = utf8.DecodeLastRuneInString(line);
    if (!unicode.IsLetter(r) && !unicode.IsDigit(r)) {
        return false;
    }
    // exclude lines with illegal characters. we allow "(),"
    if (strings.ContainsAny(line, ";:!?+*/=[]{}_^°&§~%#@<\">\\"u8)) {
        return false;
    }
    // allow "'" for possessive "'s" only
    for (@string b = line; ᐧ ; ) {
        bool ok = default!;
        {
            (_, b, ok) = strings.Cut(b, "'"u8); if (!ok) {
                break;
            }
        }
        if (b != "s"u8 && !strings.HasPrefix(b, "s "u8)) {
            return false; // ' not followed by s and then end-of-word
        }
    }
    // allow "." when followed by non-space
    for (@string b = line; ᐧ ; ) {
        bool ok = default!;
        {
            (_, b, ok) = strings.Cut(b, "."u8); if (!ok) {
                break;
            }
        }
        if (b == ""u8 || strings.HasPrefix(b, " "u8)) {
            return false; // not followed by non-space
        }
    }
    return true;
}

// oldHeading returns the *Heading for the given old-style section heading line.
internal static Block oldHeading(this ref parseDoc d, @string line) {
    return new HeadingжBlock(Ꮡ(new Heading(Text: new ΔText[]{((Plain)strings.TrimSpace(line))}.slice())));
}

// isHeading reports whether line is a new-style section heading.
internal static bool isHeading(@string line) {
    return len(line) >= 2 && line[0] == (rune)'#' && (line[1] == (rune)' ' || line[1] == (rune)'\t') && strings.TrimSpace(line) != "#"u8;
}

// heading returns the *Heading for the given new-style section heading line.
internal static Block heading(this ref parseDoc d, @string line) {
    return new HeadingжBlock(Ꮡ(new Heading(Text: new ΔText[]{((Plain)strings.TrimSpace(line[1..]))}.slice())));
}

// code returns a code block built from the lines.
internal static ж<Code> code(this ref parseDoc d, slice<@string> lines) {
    var body = unindent(lines);
    body = append(body, ""u8); // to get final \n from Join
    return Ꮡ(new Code(Text: strings.Join(body, "\n"u8)));
}

// paragraph returns a paragraph block built from the lines.
// If the lines are link definitions, paragraph adds them to d and returns nil.
internal static Block paragraph(this ref parseDoc d, slice<@string> lines) {
    // Is this a block of known links? Handle.
    slice<ж<LinkDef>> defs = default!;
    foreach (var (_, line) in lines) {
        var (def, ok) = parseLink(line);
        if (!ok) {
            goto NoDefs;
        }
        defs = append(defs, def);
    }
    foreach (var (_, def) in defs) {
        d.Links = append(d.Links, def);
        if (d.links[(~def).Text] == nil) {
            d.links[(~def).Text] = def;
        }
    }
    return default!;
NoDefs:
    return new ParagraphжBlock(Ꮡ(new Paragraph(Text: new ΔText[]{((Plain)strings.Join(lines, "\n"u8))}.slice())));
}

// parseLink parses a single link definition line:
//
//	[text]: url
//
// It returns the link definition and whether the line was well formed.
internal static (ж<LinkDef>, bool) parseLink(@string line) {
    if (line == ""u8 || line[0] != (rune)'[') {
        return (default!, false);
    }
    nint i = strings.Index(line, "]:"u8);
    if (i < 0 || i + 3 >= len(line) || (line[i + 2] != (rune)' ' && line[i + 2] != (rune)'\t')) {
        return (default!, false);
    }
    ref var text = ref heap<@string>(out var Ꮡtext);
    text = line.slice(1, i);
    ref var url = ref heap<@string>(out var Ꮡurl);
    url = strings.TrimSpace(line.slice(i + 3));
    nint j = strings.Index(url, "://"u8);
    if (j < 0 || !isScheme(url.slice(0, j))) {
        return (default!, false);
    }
    // Line has right form and has valid scheme://.
    // That's good enough for us - we are not as picky
    // about the characters beyond the :// as we are
    // when extracting inline URLs from text.
    return (Ꮡ(new LinkDef(Text: text, URL: url)), true);
}

// list returns a list built from the indented lines,
// using forceBlankBefore as the value of the List's ForceBlankBefore field.
internal static ж<List> list(this ж<parseDoc> Ꮡd, slice<@string> lines, bool forceBlankBefore) {
    var (num, _, _) = listMarker(lines[0]);
    ж<List> list = Ꮡ(new List(ForceBlankBefore: forceBlankBefore));
    ref var item = ref heap<ж<ListItem>>(out var Ꮡitem);
    ref var text = ref heap<slice<@string>>(out var Ꮡtext);
    void flush() {
        if (Ꮡitem.ValueSlot != nil) {
            {
                var para = Ꮡd.Value.paragraph(Ꮡtext.ValueSlot); if (para != default!) {
                    Ꮡitem.ValueSlot.Value.Content = append((~Ꮡitem.ValueSlot).Content, para);
                }
            }
        }
        Ꮡtext.ValueSlot = default!;
    }
    foreach (var (_, vᴛ1) in lines) {
        var line = vᴛ1;

        {
            ref var n = ref heap<@string>(out var Ꮡn);
            (n, var after, var ok) = listMarker(line); if (ok && (n != ""u8) == (num != ""u8)) {
                // start new list item
                flush();
                item = Ꮡ(new ListItem(Number: n));
                list.Value.Items = append((~list).Items, item);
                line = after;
            }
        }
        line = strings.TrimSpace(line);
        if (line == ""u8) {
            list.Value.ForceBlankBetween = true;
            flush();
            continue;
        }
        text = append(text, strings.TrimSpace(line));
    }
    flush();
    return list;
}

// listMarker parses the line as beginning with a list marker.
// If it can do that, it returns the numeric marker ("" for a bullet list),
// the rest of the line, and ok == true.
// Otherwise, it returns "", "", false.
internal static (@string num, @string rest, bool ok) listMarker(@string line) {
    @string num = default!;
    @string rest = default!;

    line = strings.TrimSpace(line);
    if (line == ""u8) {
        return ("", "", false);
    }
    // Can we find a marker?
    {
        var (r, n) = utf8.DecodeRuneInString(line); if (r == (rune)'•' || r == (rune)'*' || r == (rune)'+' || r == (rune)'-'){
            (num, rest) = ("", line.slice(n));
        } else 
        if ((rune)'0' <= line[0] && line[0] <= (rune)'9'){
            nint nΔ1 = 1;
            while (nΔ1 < len(line) && (rune)'0' <= line[nΔ1] && line[nΔ1] <= (rune)'9') {
                nΔ1++;
            }
            if (nΔ1 >= len(line) || (line[nΔ1] != (rune)'.' && line[nΔ1] != (rune)')')) {
                return ("", "", false);
            }
            (num, rest) = (line.slice(0, nΔ1), line.slice(nΔ1 + 1));
        } else {
            return ("", "", false);
        }
    }
    if (!indented(rest) || strings.TrimSpace(rest) == ""u8) {
        return ("", "", false);
    }
    return (num, rest, true);
}

// isList reports whether the line is the first line of a list,
// meaning starts with a list marker after any indentation.
// (The caller is responsible for checking the line is indented, as appropriate.)
internal static bool isList(@string line) {
    var (_, _, ok) = listMarker(line);
    return ok;
}

// parseLinkedText parses text that is allowed to contain explicit links,
// such as [math.Sin] or [Go home page], into a slice of Text items.
//
// A “pkg” is only assumed to be a full import path if it starts with
// a domain name (a path element with a dot) or is one of the packages
// from the standard library (“[os]”, “[encoding/json]”, and so on).
// To avoid problems with maps, generics, and array types, doc links
// must be both preceded and followed by punctuation, spaces, tabs,
// or the start or end of a line. An example problem would be treating
// map[ast.Expr]TypeAndValue as containing a link.
internal static slice<ΔText> parseLinkedText(this ж<parseDoc> Ꮡd, @string text) {
    ref var d = ref Ꮡd.DerefOrNull();

    ref var @out = ref heap<slice<ΔText>>(out var Ꮡout);
    nint wrote = 0;
    void flush(nint i) {
        if (wrote < i) {
            Ꮡout.ValueSlot = Ꮡd.Value.parseText(Ꮡout.ValueSlot, text.slice(wrote, i), true);
            wrote = i;
        }
    }
    nint start = -1;
    slice<byte> buf = default!;
    for (nint i = 0; i < len(text); i++) {
        var c = text[i];
        if (c == (rune)'\n' || c == (rune)'\t') {
            c = (rune)' ';
        }
        switch (c) {
        case (rune)'[': {
            start = i;
            break;
        }
        case (rune)']': {
            if (start >= 0) {
                {
                    var (def, ok) = d.links[tmpstring(buf), ꟷ]; if (ok){
                        def.Value.Used = true;
                        flush(start);
                        @out = append(@out, (ΔText)(new LinkжΔText(Ꮡ(new Link(
                            Text: d.parseText(default!, text.slice(start + 1, i), false),
                            URL: (~def).URL
                        )))));
                        wrote = i + 1;
                    } else 
                    {
                        var (link, okΔ1) = d.docLink(text.slice(start + 1, i), text.slice(0, start), text.slice(i + 1)); if (okΔ1) {
                            flush(start);
                            link.Value.Text = d.parseText(default!, text.slice(start + 1, i), false);
                            @out = append(@out, (ΔText)(new DocLinkжΔText(link)));
                            wrote = i + 1;
                        }
                    }
                }
            }
            start = -1;
            buf = buf[..0];
            break;
        }}

        if (start >= 0 && i != start) {
            buf = append(buf, c);
        }
    }
    flush(len(text));
    return @out;
}

// docLink parses text, which was found inside [ ] brackets,
// as a doc link if possible, returning the DocLink and ok == true
// or else nil, false.
// The before and after strings are the text before the [ and after the ]
// on the same line. Doc links must be preceded and followed by
// punctuation, spaces, tabs, or the start or end of a line.
internal static (ж<DocLink> link, bool ok) docLink(this ref parseDoc d, @string text, @string before, @string after) {
    ж<DocLink> link = default!;
    bool ok = default!;

    if (before != ""u8) {
        var (r, _) = utf8.DecodeLastRuneInString(before);
        if (!unicode.IsPunct(r) && r != (rune)' ' && r != (rune)'\t' && r != (rune)'\n') {
            return (default!, false);
        }
    }
    if (after != ""u8) {
        var (r, _) = utf8.DecodeRuneInString(after);
        if (!unicode.IsPunct(r) && r != (rune)' ' && r != (rune)'\t' && r != (rune)'\n') {
            return (default!, false);
        }
    }
    text = strings.TrimPrefix(text, "*"u8);
    ref var pkg = ref heap<@string>(out var Ꮡpkg);
    ref var name = ref heap<@string>(out var Ꮡname);
    (pkg, name, ok) = splitDocName(text);
    ref var recv = ref heap(new @string(), out var Ꮡrecv);
    if (ok) {
        (pkg, recv, _) = splitDocName(pkg);
    }
    if (pkg != ""u8){
        {
            (pkg, ok) = d.lookupPkg(pkg); if (!ok) {
                return (default!, false);
            }
        }
    } else {
        {
            ok = d.lookupSym(recv, name); if (!ok) {
                return (default!, false);
            }
        }
    }
    link = Ꮡ(new DocLink(
        ImportPath: pkg,
        Recv: recv,
        Name: name
    ));
    return (link, true);
}

// If text is of the form before.Name, where Name is a capitalized Go identifier,
// then splitDocName returns before, name, true.
// Otherwise it returns text, "", false.
internal static (@string before, @string name, bool foundDot) splitDocName(@string text) {
    @string before = default!;
    @string name = default!;

    nint i = strings.LastIndex(text, "."u8);
    name = text.slice(i + 1);
    if (!isName(name)) {
        return (text, "", false);
    }
    if (i >= 0) {
        before = text.slice(0, i);
    }
    return (before, name, true);
}

// parseText parses s as text and returns the result of appending
// those parsed Text elements to out.
// parseText does not handle explicit links like [math.Sin] or [Go home page]:
// those are handled by parseLinkedText.
// If autoLink is true, then parseText recognizes URLs and words from d.Words
// and converts those to links as appropriate.
internal static slice<ΔText> parseText(this ref parseDoc d, slice<ΔText> @out, @string s, bool autoLink) {
    ref var w = ref heap(new strings.Builder(), out var Ꮡw);
    nint wrote = 0;
    void writeUntil(nint i) {
        Ꮡw.WriteString(s.slice(wrote, i));
        wrote = i;
    }
    var writeUntilʗ1 = writeUntil;
    void flush(nint i) {
        writeUntilʗ1(i);
        if (Ꮡw.Value.Len() > 0) {
            @out = append(@out, (ΔText)(((Plain)Ꮡw.Value.String())));
            Ꮡw.Value.Reset();
        }
    }
    for (nint i = 0; i < len(s); ) {
        @string t = s.slice(i);
        if (autoLink) {
            {
                ref var url = ref heap<@string>(out var Ꮡurl);
                (url, var ok) = autoURL(t); if (ok) {
                    flush(i);
                    // Note: The old comment parser would look up the URL in words
                    // and replace the target with words[URL] if it was non-empty.
                    // That would allow creating links that display as one URL but
                    // when clicked go to a different URL. Not sure what the point
                    // of that is, so we're not doing that lookup here.
                    @out = append(@out, (ΔText)(new LinkжΔText(Ꮡ(new Link(Auto: true, Text: new ΔText[]{((Plain)url)}.slice(), URL: url)))));
                    i += len(url);
                    wrote = i;
                    continue;
                }
            }
            {
                var (id, ok) = ident(t); if (ok) {
                    ref var url = ref heap<@string>(out var Ꮡurl);
                    (url, var italics) = d.Words[id, ꟷ];
                    if (!italics) {
                        i += len(id);
                        continue;
                    }
                    flush(i);
                    if (url == ""u8){
                        @out = append(@out, (ΔText)(((Italic)id)));
                    } else {
                        @out = append(@out, (ΔText)(new LinkжΔText(Ꮡ(new Link(Auto: true, Text: new ΔText[]{((Italic)id)}.slice(), URL: url)))));
                    }
                    i += len(id);
                    wrote = i;
                    continue;
                }
            }
        }
        switch (ᐧ) {
        case {} when strings.HasPrefix(t, "``"u8): {
            if (len(t) >= 3 && t[2] == (rune)'`') {
                // Do not convert `` inside ```, in case people are mistakenly writing Markdown.
                i += 3;
                while (i < len(t) && t[i] == (rune)'`') {
                    i++;
                }
                break;
            }
            writeUntil(i);
            Ꮡw.WriteRune((rune)'“');
            i += 2;
            wrote = i;
            break;
        }
        case {} when strings.HasPrefix(t, "''"u8): {
            writeUntil(i);
            Ꮡw.WriteRune((rune)'”');
            i += 2;
            wrote = i;
            break;
        }
        default: {
            i++;
            break;
        }}

    }
    flush(len(s));
    return @out;
}

// autoURL checks whether s begins with a URL that should be hyperlinked.
// If so, it returns the URL, which is a prefix of s, and ok == true.
// Otherwise it returns "", false.
// The caller should skip over the first len(url) bytes of s
// before further processing.
internal static (@string url, bool ok) autoURL(@string s) {
    // Find the ://. Fast path to pick off non-URL,
    // since we call this at every position in the string.
    // The shortest possible URL is ftp://x, 7 bytes.
    nint i = default!;
    switch (ᐧ) {
    case {} when len(s) is < 7: {
        return ("", false);
    }
    case {} when s[3] is (rune)':': {
        i = 3;
        break;
    }
    case {} when s[4] is (rune)':': {
        i = 4;
        break;
    }
    case {} when s[5] is (rune)':': {
        i = 5;
        break;
    }
    case {} when s[6] is (rune)':': {
        i = 6;
        break;
    }
    default: {
        return ("", false);
    }}

    if (i + 3 > len(s) || s.slice(i, i + 3) != "://") {
        return ("", false);
    }
    // Check valid scheme.
    if (!isScheme(s.slice(0, i))) {
        return ("", false);
    }
    // Scan host part. Must have at least one byte,
    // and must start and end in non-punctuation.
    i += 3;
    if (i >= len(s) || !isHost(s[i]) || isPunct(s[i])) {
        return ("", false);
    }
    i++;
    nint end = i;
    while (i < len(s) && isHost(s[i])) {
        if (!isPunct(s[i])) {
            end = i + 1;
        }
        i++;
    }
    i = end;
    // At this point we are definitely returning a URL (scheme://host).
    // We just have to find the longest path we can add to it.
    // Heuristics abound.
    // We allow parens, braces, and brackets,
    // but only if they match (#5043, #22285).
    // We allow .,:;?! in the path but not at the end,
    // to avoid end-of-sentence punctuation (#18139, #16565).
    var stk = new byte[]{}.slice();
    end = i;
Path:
    for (; i < len(s); i++) {
        if (isPunct(s[i])) {
            continue;
        }
        if (!isPath(s[i])) {
            break;
        }
        switch (s[i]) {
        case (rune)'(': {
            stk = append(stk, (byte)((rune)')'));
            break;
        }
        case (rune)'{': {
            stk = append(stk, (byte)((rune)'}'));
            break;
        }
        case (rune)'[': {
            stk = append(stk, (byte)((rune)']'));
            break;
        }
        case (rune)')' or (rune)'}' or (rune)']': {
            if (len(stk) == 0 || stk[len(stk) - 1] != s[i]) {
                goto break_Path;
            }
            stk = stk.slice(0, len(stk) - 1);
            break;
        }}

        if (len(stk) == 0) {
            end = i + 1;
        }
continue_Path:;
    }
break_Path:;
    return (s.slice(0, end), true);
}

// isScheme reports whether s is a recognized URL scheme.
// Note that if strings of new length (beyond 3-7)
// are added here, the fast path at the top of autoURL will need updating.
internal static bool isScheme(@string s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == "file"u8 || exprᴛ1 == "ftp"u8 || exprᴛ1 == "gopher"u8 || exprᴛ1 == "http"u8 || exprᴛ1 == "https"u8 || exprᴛ1 == "mailto"u8 || exprᴛ1 == "nntp"u8) {
        return true;
    }

    return false;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ = GoBigConst.Parse("10633823862292363665388054147449749504");

// isHost reports whether c is a byte that can appear in a URL host,
// like www.example.com or user@[::1]:8080
internal static bool isHost(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c > 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<26-1)<<'A' |
	(1<<26-1)<<'a' |
	(1<<10-1)<<'0' |
	1<<'_' |
	1<<'@' |
	1<<'-' |
	1<<'.' |
	1<<'[' |
	1<<']' |
	1<<':' */
            maskᶜ;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (576284830442979328UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (576460746666278911UL)))) != 0;
}

// isPunct reports whether c is a punctuation byte that can appear
// inside a path but not at the end.
internal static bool isPunct(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c > 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    UntypedInt mask = /* 0 |
	1<<'.' |
	1<<',' |
	1<<':' |
	1<<';' |
	1<<'?' |
	1<<'!' */ 10088151134830067712;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (10088151134830067712UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (((uint64)mask).Rsh((uint64)(64)))))) != 0;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ1 = GoBigConst.Parse("148873535423923614449401688976238051328");

// isPath reports whether c is a (non-punctuation) path byte.
internal static bool isPath(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c > 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<26-1)<<'A' |
	(1<<26-1)<<'a' |
	(1<<10-1)<<'0' |
	1<<'$' |
	1<<'\'' |
	1<<'(' |
	1<<')' |
	1<<'*' |
	1<<'+' |
	1<<'&' |
	1<<'#' |
	1<<'=' |
	1<<'@' |
	1<<'~' |
	1<<'_' |
	1<<'/' |
	1<<'-' |
	1<<'[' |
	1<<']' |
	1<<'{' |
	1<<'}' |
	1<<'%' */
            maskᶜ1;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (2593985390075445248UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (8070450526610784255UL)))) != 0;
}

// isName reports whether s is a capitalized Go identifier (like Name).
internal static bool isName(@string s) {
    var (t, ok) = ident(s);
    if (!ok || t != s) {
        return false;
    }
    var (r, _) = utf8.DecodeRuneInString(s);
    return unicode.IsUpper(r);
}

// ident checks whether s begins with a Go identifier.
// If so, it returns the identifier, which is a prefix of s, and ok == true.
// Otherwise it returns "", false.
// The caller should skip over the first len(id) bytes of s
// before further processing.
internal static (@string id, bool ok) ident(@string s) {
    // Scan [\pL_][\pL_0-9]*
    nint n = 0;
    while (n < len(s)) {
        {
            var c = s[n]; if (c < utf8.RuneSelf) {
                if (isIdentASCII(c) && (n > 0 || c < (rune)'0' || c > (rune)'9')) {
                    n++;
                    continue;
                }
                break;
            }
        }
        var (r, nr) = utf8.DecodeRuneInString(s.slice(n));
        if (unicode.IsLetter(r)) {
            n += nr;
            continue;
        }
        break;
    }
    return (s.slice(0, n), n > 0);
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ2 = GoBigConst.Parse("10633823849912963253799171395480977408");

// isIdentASCII reports whether c is an ASCII identifier byte.
internal static bool isIdentASCII(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c > 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<26-1)<<'A' |
	(1<<26-1)<<'a' |
	(1<<10-1)<<'0' |
	1<<'_' */
            maskᶜ2;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (287948901175001088UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (576460745995190270UL)))) != 0;
}

// validImportPath reports whether path is a valid import path.
// It is a lightly edited copy of golang.org/x/mod/module.CheckImportPath.
internal static bool validImportPath(@string path) {
    if (!utf8.ValidString(path)) {
        return false;
    }
    if (path == ""u8) {
        return false;
    }
    if (path[0] == (rune)'-') {
        return false;
    }
    if (strings.Contains(path, "//"u8)) {
        return false;
    }
    if (path[len(path) - 1] == (rune)'/') {
        return false;
    }
    nint elemStart = 0;
    foreach (var (i, r) in path) {
        if (r == (rune)'/') {
            if (!validImportPathElem(path.slice(elemStart, i))) {
                return false;
            }
            elemStart = i + 1;
        }
    }
    return validImportPathElem(path.slice(elemStart));
}

internal static bool validImportPathElem(@string elem) {
    if (elem == ""u8 || elem[0] == (rune)'.' || elem[len(elem) - 1] == (rune)'.') {
        return false;
    }
    for (nint i = 0; i < len(elem); i++) {
        if (!importPathOK(elem[i])) {
            return false;
        }
    }
    return true;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ3 = GoBigConst.Parse("95704415580147579119642937602632318976");

internal static bool importPathOK(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c > 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<26-1)<<'A' |
	(1<<26-1)<<'a' |
	(1<<10-1)<<'0' |
	1<<'-' |
	1<<'.' |
	1<<'~' |
	1<<'_' |
	1<<'+' */
            maskᶜ3;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (288063250384289792UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (5188146764422578174UL)))) != 0;
}

} // end comment_package
