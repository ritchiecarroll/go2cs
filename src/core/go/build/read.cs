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
/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname
using global::go.go;
using global::go.unicode;

partial class build_package {

partial struct importReader {
    internal ж<bufio.Reader> b;
    internal slice<byte> buf;
    internal byte peek;
    internal error err;
    internal bool eof;
    internal nint nerr;
    internal tokenꓸPosition pos;
}

internal static slice<byte> bom = slice<byte>([0xef, 0xbb, 0xbf]);

internal static ж<importReader> newImportReader(@string name, io.Reader r) {
    var b = bufio.NewReader(r);
    // Remove leading UTF-8 BOM.
    // Per https://golang.org/ref/spec#Source_code_representation:
    // a compiler may ignore a UTF-8-encoded byte order mark (U+FEFF)
    // if it is the first Unicode code point in the source text.
    {
        var (leadingBytes, err) = b.Peek(3); if (err == nil && bytes.Equal(leadingBytes, bom)) {
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
    return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c >= utf8.RuneSelf;
}

internal static error errSyntax = errors.New("syntax error"u8);
internal static error errNUL = errors.New("unexpected NUL in input"u8);

// syntaxError records a syntax error, but only if an I/O error has not already been recorded.
internal static void syntaxError(this ref importReader r) {
    if (r.err == nil) {
        r.err = errSyntax;
    }
}

// readByte reads the next byte from the input, saves it in buf, and returns it.
// If an error occurs, readByte records the error in r.err and returns 0.
internal static byte readByte(this ref importReader r) {
    var (c, err) = r.b.ReadByte();
    if (err == nil) {
        r.buf = append(r.buf, c);
        if (c == 0) {
            err = errNUL;
        }
    }
    if (err != nil) {
        if (AreEqual(err, io.EOF)) {
            r.eof = true;
        } else if (r.err == nil) {
            r.err = err;
        }
        c = 0;
    }
    return c;
}

// readRest reads the entire rest of the file into r.buf.
internal static void readRest(this ref importReader r) {
    for (;;) {
        if (len(r.buf) == cap(r.buf)) {
            // Grow the buffer
            r.buf = append(r.buf, (byte)0).slice(0, len(r.buf));
        }
        var (n, err) = r.b.Read(r.buf.slice(len(r.buf), cap(r.buf)));
        r.buf = r.buf.slice(0, len(r.buf) + n);
        if (err != nil) {
            if (AreEqual(err, io.EOF)) {
                r.eof = true;
            } else if (r.err == nil) {
                r.err = err;
            }
            break;
        }
    }
}

// peekByte returns the next byte from the input reader but does not advance beyond it.
// If skipSpace is set, peekByte skips leading spaces and comments.
internal static byte peekByte(this ref importReader r, bool skipSpace) {
    if (r.err != nil) {
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
    while (r.err == nil && !r.eof) {
        if (skipSpace) {
            // For the purposes of this reader, semicolons are never necessary to
            // understand the input and are treated as spaces.
            switch (c) {
            case (byte)' ' or (byte)'\f' or (byte)'\t' or (byte)'\r' or (byte)'\n' or (byte)';': {
                c = r.readByte();
                continue;
                break;
            }
            case (byte)'/': {
                c = r.readByte();
                if (c == '/') {
                    while (c != '\n' && r.err == nil && !r.eof) {
                        c = r.readByte();
                    }
                } else if (c == '*') {
                    byte c1 = default;
                    while ((c != '*' || c1 != '/') && r.err == nil) {
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
internal static byte nextByte(this ref importReader r, bool skipSpace) {
    var c = r.peekByte(skipSpace);
    r.peek = 0;
    return c;
}

// readKeyword reads the given keyword from the input.
// If the keyword is not present, readKeyword records a syntax error.
internal static void readKeyword(this ref importReader r, @string kw) {
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
internal static void readIdent(this ref importReader r) {
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
internal static void readString(this ref importReader r) {
    switch (r.nextByte(true)) {
    case (byte)'`': {
        while (r.err == nil) {
            if (r.nextByte(false) == '`') {
                break;
            }
            if (r.eof) {
                r.syntaxError();
            }
        }
        break;
    }
    case (byte)'"': {
        while (r.err == nil) {
            var c = r.nextByte(false);
            if (c == '"') {
                break;
            }
            if (r.eof || c == '\n') {
                r.syntaxError();
            }
            if (c == '\\') {
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
internal static void readImport(this ref importReader r) {
    var c = r.peekByte(true);
    if (c == '.') {
        r.peek = 0;
    } else if (isIdent(c)) {
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
    if (r.err == nil && !r.eof) {
        // Didn't reach EOF, so must have found a non-space byte. Remove it.
        r.buf = r.buf.slice(0, len(r.buf) - 1);
    }
    return (r.buf, r.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageˢ = "package"u8;
internal static readonly @string importˢ = "import"u8;
internal static readonly @string goEmbedˢ = "//go:embed"u8;

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
    while (r.peekByte(true) == 'i') {
        r.readKeyword(importˢ);
        if (r.peekByte(true) == '(') {
            r.nextByte(false);
            while (r.peekByte(true) != ')' && r.err == nil) {
                r.readImport();
            }
            r.nextByte(false);
        } else {
            r.readImport();
        }
    }

    info.header = r.buf;

    // If we stopped successfully before EOF, we read a byte that told us we were done.
    // Return all but that last byte, which would cause a syntax error if we let it through.
    if (r.err == nil && !r.eof) {
        info.header = r.buf.slice(0, len(r.buf) - 1);
    }

    // If we stopped for a syntax error, consume the whole file so that
    // we are sure we don't change the errors that go/parser returns.
    if (AreEqual(r.err, errSyntax)) {
        r.err = default;
        r.readRest();
        info.header = r.buf;
    }
    if (r.err != nil) {
        return r.err;
    }

    if (info.fset == nil) {
        return default;
    }

    // Parse file header & record imports.
    (info.parsed, info.parseErr) = parser.ParseFile(info.fset, info.name, info.header, (parser.Mode)(parser.ImportsOnly | parser.ParseComments));
    if (info.parseErr != nil) {
        return default;
    }

    var hasEmbed = false;
    foreach (var (_, decl) in info.parsed.Decls) {
        var (d, ok) = decl._<ж<ast.GenDecl>>(ᐧ);
        if (!ok) {
            continue;
        }
        foreach (var (_, dspec) in d.Specs) {
            var (spec, okΔ1) = dspec._<ж<ast.ImportSpec>>(ᐧ);
            if (!okΔ1) {
                continue;
            }
            @string quoted = spec.Path.Value.Value;
            var (path, err) = strconv.Unquote(quoted);
            if (err != nil) {
                return fmt.Errorf("parser returned invalid quoted string: <%s>"u8, quoted);
            }
            if (!isValidImport(path)) {
                // The parser used to return a parse error for invalid import paths, but
                // no longer does, so check for and create the error here instead.
                info.parseErr = new scanner_ΔErrorᴠerror(new scannerꓸError(Pos: info.fset.Position(spec.Pos()), Msg: "invalid import path: "u8 + path));
                info.imports = default;
                return default;
            }
            if (path == "embed"u8) {
                hasEmbed = true;
            }

            var doc = spec.Doc;
            if (doc == nil && len(d.Specs) == 1) {
                doc = d.Doc;
            }
            info.imports = append(info.imports, new fileImport(path, spec.Pos(), doc));
        }
    }

    // Extract directives.
    foreach (var (_, group) in info.parsed.Comments) {
        if (group.Pos() >= info.parsed.Package) {
            break;
        }
        foreach (var (_, c) in group.List) {
            if (strings.HasPrefix(c.Text, "//go:"u8)) {
                info.directives = append(info.directives, new Directive(c.Text, info.fset.Position(c.Slash)));
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
        r.readRest();
        var fset = token.NewFileSet();
        var @file = fset.AddFile(r.pos.Filename, -1, len(r.buf));
        scanner.Scanner sc = default;
        sc.Init(@file, r.buf, default, scanner.ScanComments);
        for (;;) {
            var (pos, tok, lit) = sc.Scan();
            if (tok == token.EOF) {
                break;
            }
            if (tok == token.COMMENT && strings.HasPrefix(lit, goEmbedˢ)) {
                // Ignore badly-formed lines - the compiler will report them when it finds them,
                // and we can pretend they are not there to help go list succeed with what it knows.
                var (embs, err) = parseGoEmbed(fset, pos, lit);
                if (err == nil) {
                    info.embeds = appendꓸꓸꓸ(info.embeds, embs);
                }
            }
        }
    }

    return default;
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

// parseGoEmbed parses a "//go:embed" to extract the glob patterns.
// It accepts unquoted space-separated patterns as well as double-quoted and back-quoted Go strings.
// This must match the behavior of cmd/compile/internal/noder.go.
internal static (slice<fileEmbed>, error) parseGoEmbed(ж<token.FileSet> fset, tokenꓸPos pos, @string comment) {
    var (dir, ok) = ast.ParseDirective(pos, comment);
    if (!ok || dir.Tool != "go"u8 || dir.Name != "embed"u8) {
        return (default, default);
    }
    var (args, err) = dir.ParseArgs();
    if (err != nil) {
        return (default, err);
    }
    slice<fileEmbed> list = default;
    foreach (var (_, arg) in args) {
        list = append(list, new fileEmbed(arg.Arg, fset.Position(arg.Pos)));
    }
    return (list, default);
}

} // end build_package
