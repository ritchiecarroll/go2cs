// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using global::go.go;
using global::go.unicode;

partial class build_package {

[GoType] partial struct importReader {
    internal ж<bufio.Reader> b;
    internal slice<byte> buf;
    internal byte peek;
    internal error err;
    internal bool eof;
    internal nint nerr;
    internal tokenꓸPosition pos;
}

internal static slice<byte> bom = new byte[]{0xef, 0xbb, 0xbf}.slice();

internal static ж<importReader> newImportReader(@string name, io.Reader r) {
    var b = bufio.NewReader(r);
    // Remove leading UTF-8 BOM.
    // Per https://golang.org/ref/spec#Source_code_representation:
    // a compiler may ignore a UTF-8-encoded byte order mark (U+FEFF)
    // if it is the first Unicode code point in the source text.
    {
        var (leadingBytes, err) = b.Peek(3); if (err == default! && bytes.Equal(leadingBytes, bom)) {
            b.Discard(3);
        }
    }
    return Ꮡ(new importReader(
        b: b,
        pos: new tokenꓸPosition(
            Filename: name,
            Line: 1,
            Column: 1
        )
    ));
}

internal static bool isIdent(byte c) {
    return (rune)'A' <= c && c <= (rune)'Z' || (rune)'a' <= c && c <= (rune)'z' || (rune)'0' <= c && c <= (rune)'9' || c == (rune)'_' || c >= utf8.RuneSelf;
}

internal static error errSyntax = errors.New("syntax error"u8);
internal static error errNUL = errors.New("unexpected NUL in input"u8);

// syntaxError records a syntax error, but only if an I/O error has not already been recorded.
[GoRecv] internal static void syntaxError(this ref importReader r) {
    if (r.err == default!) {
        r.err = errSyntax;
    }
}

// readByte reads the next byte from the input, saves it in buf, and returns it.
// If an error occurs, readByte records the error in r.err and returns 0.
[GoRecv] internal static byte readByte(this ref importReader r) {
    var (c, err) = r.b.ReadByte();
    if (err == default!) {
        r.buf = append(r.buf, c);
        if (c == 0) {
            err = errNUL;
        }
    }
    if (err != default!) {
        if (AreEqual(err, io.EOF)){
            r.eof = true;
        } else 
        if (r.err == default!) {
            r.err = err;
        }
        c = 0;
    }
    return c;
}

// readByteNoBuf is like readByte but doesn't buffer the byte.
// It exhausts r.buf before reading from r.b.
[GoRecv] internal static byte readByteNoBuf(this ref importReader r) {
    byte c = default!;
    error err = default!;
    if (len(r.buf) > 0){
        c = r.buf[0];
        r.buf = r.buf[1..];
    } else {
        (c, err) = r.b.ReadByte();
        if (err == default! && c == 0) {
            err = errNUL;
        }
    }
    if (err != default!) {
        if (AreEqual(err, io.EOF)){
            r.eof = true;
        } else 
        if (r.err == default!) {
            r.err = err;
        }
        return 0;
    }
    r.pos.Offset++;
    if (c == (rune)'\n'){
        r.pos.Line++;
        r.pos.Column = 1;
    } else {
        r.pos.Column++;
    }
    return c;
}

// peekByte returns the next byte from the input reader but does not advance beyond it.
// If skipSpace is set, peekByte skips leading spaces and comments.
[GoRecv] internal static byte peekByte(this ref importReader r, bool skipSpace) {
    if (r.err != default!) {
        {
            r.nerr++; if (r.nerr > 10000) {
                throw panic("go/build: import reader looping");
            }
        }
        return 0;
    }
    // Use r.peek as first input byte.
    // Don't just return r.peek here: it might have been left by peekByte(false)
    // and this might be peekByte(true).
    var c = r.peek;
    if (c == 0) {
        c = r.readByte();
    }
    while (r.err == default! && !r.eof) {
        if (skipSpace) {
            // For the purposes of this reader, semicolons are never necessary to
            // understand the input and are treated as spaces.
            switch (c) {
            case (rune)' ' or (rune)'\f' or (rune)'\t' or (rune)'\r' or (rune)'\n' or (rune)';': {
                c = r.readByte();
                continue;
                break;
            }
            case (rune)'/': {
                c = r.readByte();
                if (c == (rune)'/'){
                    while (c != (rune)'\n' && r.err == default! && !r.eof) {
                        c = r.readByte();
                    }
                } else 
                if (c == (rune)'*'){
                    byte c1 = default!;
                    while ((c != (rune)'*' || c1 != (rune)'/') && r.err == default!) {
                        if (r.eof) {
                            r.syntaxError();
                        }
                        (c, c1) = (c1, r.readByte());
                    }
                } else {
                    r.syntaxError();
                }
                c = r.readByte();
                continue;
                break;
            }}

        }
        break;
    }
    r.peek = c;
    return r.peek;
}

// nextByte is like peekByte but advances beyond the returned byte.
[GoRecv] internal static byte nextByte(this ref importReader r, bool skipSpace) {
    var c = r.peekByte(skipSpace);
    r.peek = 0;
    return c;
}

internal static slice<byte> goEmbed = slice<byte>("go:embed"u8);

// findEmbed advances the input reader to the next //go:embed comment.
// It reports whether it found a comment.
// (Otherwise it found an error or EOF.)
[GoRecv] internal static bool findEmbed(this ref importReader r, bool first) {
    // The import block scan stopped after a non-space character,
    // so the reader is not at the start of a line on the first call.
    // After that, each //go:embed extraction leaves the reader
    // at the end of a line.
    var startLine = !first;
    byte c = default!;
    while (r.err == default! && !r.eof) {
        c = r.readByteNoBuf();
Reswitch:
        switch (c) {
        default: {
            startLine = false;
            break;
        }
        case (rune)'\n': {
            startLine = true;
            break;
        }
        case (rune)' ' or (rune)'\t': {
            break;
        }
        case (rune)'"': {
            startLine = false;
            while (r.err == default!) {
                // leave startLine alone
                if (r.eof) {
                    r.syntaxError();
                }
                c = r.readByteNoBuf();
                if (c == (rune)'\\') {
                    r.readByteNoBuf();
                    if (r.err != default!) {
                        r.syntaxError();
                        return false;
                    }
                    continue;
                }
                if (c == (rune)'"') {
                    c = r.readByteNoBuf();
                    goto Reswitch;
                }
            }
            goto Reswitch;
            break;
        }
        case (rune)'`': {
            startLine = false;
            while (r.err == default!) {
                if (r.eof) {
                    r.syntaxError();
                }
                c = r.readByteNoBuf();
                if (c == (rune)'`') {
                    c = r.readByteNoBuf();
                    goto Reswitch;
                }
            }
            break;
        }
        case (rune)'\'': {
            startLine = false;
            while (r.err == default!) {
                if (r.eof) {
                    r.syntaxError();
                }
                c = r.readByteNoBuf();
                if (c == (rune)'\\') {
                    r.readByteNoBuf();
                    if (r.err != default!) {
                        r.syntaxError();
                        return false;
                    }
                    continue;
                }
                if (c == (rune)'\'') {
                    c = r.readByteNoBuf();
                    goto Reswitch;
                }
            }
            break;
        }
        case (rune)'/': {
            c = r.readByteNoBuf();
            switch (c) {
            default: {
                startLine = false;
                goto Reswitch;
                break;
            }
            case (rune)'*': {
                byte c1 = default!;
                while ((c != (rune)'*' || c1 != (rune)'/') && r.err == default!) {
                    if (r.eof) {
                        r.syntaxError();
                    }
                    (c, c1) = (c1, r.readByteNoBuf());
                }
                startLine = false;
                break;
            }
            case (rune)'/': {
                if (startLine) {
                    // Try to read this as a //go:embed comment.
                    foreach (var (i, _) in goEmbed) {
                        c = r.readByteNoBuf();
                        if (c != goEmbed[i]) {
                            goto SkipSlashSlash;
                        }
                    }
                    c = r.readByteNoBuf();
                    if (c == (rune)' ' || c == (rune)'\t') {
                        // Found one!
                        return true;
                    }
                }
SkipSlashSlash:
                while (c != (rune)'\n' && r.err == default! && !r.eof) {
                    c = r.readByteNoBuf();
continue_SkipSlashSlash:;
                }
break_SkipSlashSlash:;
                startLine = true;
                break;
            }}

            break;
        }}

        break_Reswitch:;
    }
    return false;
}

// readKeyword reads the given keyword from the input.
// If the keyword is not present, readKeyword records a syntax error.
[GoRecv] internal static void readKeyword(this ref importReader r, @string kw) {
    r.peekByte(true);
    for (nint i = 0; i < len(kw); i++) {
        if (r.nextByte(false) != kw[i]) {
            r.syntaxError();
            return;
        }
    }
    if (isIdent(r.peekByte(false))) {
        r.syntaxError();
    }
}

// readIdent reads an identifier from the input.
// If an identifier is not present, readIdent records a syntax error.
[GoRecv] internal static void readIdent(this ref importReader r) {
    var c = r.peekByte(true);
    if (!isIdent(c)) {
        r.syntaxError();
        return;
    }
    while (isIdent(r.peekByte(false))) {
        r.peek = 0;
    }
}

// readString reads a quoted string literal from the input.
// If an identifier is not present, readString records a syntax error.
[GoRecv] internal static void readString(this ref importReader r) {
    switch (r.nextByte(true)) {
    case (rune)'`': {
        while (r.err == default!) {
            if (r.nextByte(false) == (rune)'`') {
                break;
            }
            if (r.eof) {
                r.syntaxError();
            }
        }
        break;
    }
    case (rune)'"': {
        while (r.err == default!) {
            var c = r.nextByte(false);
            if (c == (rune)'"') {
                break;
            }
            if (r.eof || c == (rune)'\n') {
                r.syntaxError();
            }
            if (c == (rune)'\\') {
                r.nextByte(false);
            }
        }
        break;
    }
    default: {
        r.syntaxError();
        break;
    }}

}

// readImport reads an import clause - optional identifier followed by quoted string -
// from the input.
[GoRecv] internal static void readImport(this ref importReader r) {
    var c = r.peekByte(true);
    if (c == (rune)'.'){
        r.peek = 0;
    } else 
    if (isIdent(c)) {
        r.readIdent();
    }
    r.readString();
}

// readComments is like io.ReadAll, except that it only reads the leading
// block of comments in the file.
//
// readComments should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bazelbuild/bazel-gazelle
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname readComments
internal static (slice<byte>, error) readComments(io.Reader f) {
    var r = newImportReader(""u8, f);
    r.peekByte(true);
    if ((~r).err == default! && !(~r).eof) {
        // Didn't reach EOF, so must have found a non-space byte. Remove it.
        r.Value.buf = (~r).buf.slice(0, len((~r).buf) - 1);
    }
    return ((~r).buf, (~r).err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageˢ = "package"u8;
internal static readonly @string importˢ = "import"u8;

// readGoInfo expects a Go file as input and reads the file up to and including the import section.
// It records what it learned in *info.
// If info.fset is non-nil, readGoInfo parses the file and sets info.parsed, info.parseErr,
// info.imports and info.embeds.
//
// It only returns an error if there are problems reading the file,
// not for syntax errors in the file itself.
internal static error readGoInfo(io.Reader f, ref fileInfo info) {
    var r = newImportReader(info.name, f);
    r.readKeyword(packageˢ);
    r.readIdent();
    while (r.peekByte(true) == (rune)'i') {
        r.readKeyword(importˢ);
        if (r.peekByte(true) == (rune)'('){
            r.nextByte(false);
            while (r.peekByte(true) != (rune)')' && (~r).err == default!) {
                r.readImport();
            }
            r.nextByte(false);
        } else {
            r.readImport();
        }
    }
    info.header = r.Value.buf;
    // If we stopped successfully before EOF, we read a byte that told us we were done.
    // Return all but that last byte, which would cause a syntax error if we let it through.
    if ((~r).err == default! && !(~r).eof) {
        info.header = (~r).buf.slice(0, len((~r).buf) - 1);
    }
    // If we stopped for a syntax error, consume the whole file so that
    // we are sure we don't change the errors that go/parser returns.
    if (AreEqual((~r).err, errSyntax)) {
        r.Value.err = default!;
        while ((~r).err == default! && !(~r).eof) {
            r.readByte();
        }
        info.header = r.Value.buf;
    }
    if ((~r).err != default!) {
        return (~r).err;
    }
    if (info.fset == nil) {
        return default!;
    }
    // Parse file header & record imports.
    (info.parsed, info.parseErr) = parser.ParseFile(info.fset, info.name, info.header, (parser.Mode)(parser.ImportsOnly | parser.ParseComments));
    if (info.parseErr != default!) {
        return default!;
    }
    var hasEmbed = false;
    foreach (var (_, decl) in (~info.parsed).Decls) {
        var (d, ok) = decl._<ж<ast.GenDecl>>(ᐧ);
        if (!ok) {
            continue;
        }
        foreach (var (_, dspec) in (~d).Specs) {
            var (spec, okΔ1) = dspec._<ж<ast.ImportSpec>>(ᐧ);
            if (!okΔ1) {
                continue;
            }
            @string quoted = spec.Value.Path.Value.Value;
            var (path, err) = strconv.Unquote(quoted);
            if (err != default!) {
                return fmt.Errorf("parser returned invalid quoted string: <%s>"u8, quoted);
            }
            if (!isValidImport(path)) {
                // The parser used to return a parse error for invalid import paths, but
                // no longer does, so check for and create the error here instead.
                info.parseErr = new scanner_ΔErrorᴠerror(new scannerꓸError(Pos: info.fset.Position(spec.Pos()), Msg: "invalid import path: "u8 + path));
                info.imports = default!;
                return default!;
            }
            if (path == "embed"u8) {
                hasEmbed = true;
            }
            var doc = spec.Value.Doc;
            if (doc == nil && len((~d).Specs) == 1) {
                doc = d.Value.Doc;
            }
            info.imports = append(info.imports, new fileImport(path, spec.Pos(), doc));
        }
    }
    // Extract directives.
    foreach (var (_, group) in (~info.parsed).Comments) {
        if (group.Pos() >= (~info.parsed).Package) {
            break;
        }
        foreach (var (_, c) in (~group).List) {
            if (strings.HasPrefix((~c).Text, "//go:"u8)) {
                info.directives = append(info.directives, new Directive((~c).Text, info.fset.Position((~c).Slash)));
            }
        }
    }
    // If the file imports "embed",
    // we have to look for //go:embed comments
    // in the remainder of the file.
    // The compiler will enforce the mapping of comments to
    // declared variables. We just need to know the patterns.
    // If there were //go:embed comments earlier in the file
    // (near the package statement or imports), the compiler
    // will reject them. They can be (and have already been) ignored.
    if (hasEmbed) {
        slice<byte> line = default!;
        for (var first = true; r.findEmbed(first); first = false) {
            line = line[..0];
            var pos = r.Value.pos;
            while (ᐧ) {
                var c = r.readByteNoBuf();
                if (c == (rune)'\n' || (~r).err != default! || (~r).eof) {
                    break;
                }
                line = append(line, c);
            }
            // Add args if line is well-formed.
            // Ignore badly-formed lines - the compiler will report them when it finds them,
            // and we can pretend they are not there to help go list succeed with what it knows.
            var (embs, err) = parseGoEmbed(((@string)line), pos);
            if (err == default!) {
                info.embeds = appendꓸꓸꓸ(info.embeds, embs);
            }
        }
    }
    return default!;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string illegalCharsᶜ = "!\"#$%&'()*,:;<=>?[\\]^{|}`�";

// isValidImport checks if the import is a valid import using the more strict
// checks allowed by the implementation restriction in https://go.dev/ref/spec#Import_declarations.
// It was ported from the function of the same name that was removed from the
// parser in CL 424855, when the parser stopped doing these checks.
internal static bool isValidImport(@string s) {
    @string illegalChars = illegalCharsᶜ;
    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return false;
        }
    }
    return s != ""u8;
}

// parseGoEmbed parses the text following "//go:embed" to extract the glob patterns.
// It accepts unquoted space-separated patterns as well as double-quoted and back-quoted Go strings.
// This is based on a similar function in cmd/compile/internal/gc/noder.go;
// this version calculates position information as well.
internal static (slice<fileEmbed>, error) parseGoEmbed(@string args, tokenꓸPosition pos) {
    void trimBytes(nint n) {
        pos.Offset += n;
        pos.Column += utf8.RuneCountInString(args.slice(0, n));
        args = args.slice(n);
    }
    var trimBytesʗ1 = trimBytes;
    void trimSpace() {
        @string trim = strings.TrimLeftFunc(args, unicode.IsSpace);
        trimBytesʗ1(len(args) - len(trim));
    }
    slice<fileEmbed> list = default!;
    for (trimSpace(); args != ""u8; trimSpace()) {
        @string path = default!;
        var pathPos = pos;
Switch:
        switch (args[0]) {
        default: {
            nint i = len(args);
            foreach (var (j, c) in args) {
                if (unicode.IsSpace(c)) {
                    i = j;
                    break;
                }
            }
            path = args.slice(0, i);
            trimBytes(i);
            break;
        }
        case (rune)'`': {
            bool ok = default!;
            (path, _, ok) = strings.Cut(args[1..], "`"u8);
            if (!ok) {
                return (default!, fmt.Errorf("invalid quoted string in //go:embed: %s"u8, args));
            }
            trimBytes(1 + len(path) + 1);
            break;
        }
        case (rune)'"': {
            nint i = 1;
            for (; i < len(args); i++) {
                if (args[i] == (rune)'\\') {
                    i++;
                    continue;
                }
                if (args[i] == (rune)'"') {
                    var (q, err) = strconv.Unquote(args.slice(0, i + 1));
                    if (err != default!) {
                        return (default!, fmt.Errorf("invalid quoted string in //go:embed: %s"u8, args.slice(0, i + 1)));
                    }
                    path = q;
                    trimBytes(i + 1);
                    goto break_Switch;
                }
            }
            if (i >= len(args)) {
                return (default!, fmt.Errorf("invalid quoted string in //go:embed: %s"u8, args));
            }
            break;
        }}

        break_Switch:;
        if (args != ""u8) {
            var (r, _) = utf8.DecodeRuneInString(args);
            if (!unicode.IsSpace(r)) {
                return (default!, fmt.Errorf("invalid quoted string in //go:embed: %s"u8, args));
            }
        }
        list = append(list, new fileEmbed(path, pathPos));
    }
    return (list, default!);
}

} // end build_package
