// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package xml implements a simple XML 1.0 parser that
// understands XML name spaces.

// A Token is an interface holding one of the token types:
// [StartElement], [EndElement], [CharData], [Comment], [ProcInst], or [Directive].
global using ΔToken = object;

namespace go.encoding;

// References:
//    Annotated XML spec: https://www.xml.com/axml/testaxml.htm
//    XML name spaces: https://www.w3.org/TR/REC-xml-names/
using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class xml_package {

// A SyntaxError represents a syntax error in the XML input stream.
partial struct SyntaxError {
    public @string Msg;
    public nint Line;
}

public static @string Error(this ref SyntaxError e) {
    return "XML syntax error on line "u8 + strconv.Itoa(e.Line) + ": "u8 + e.Msg;
}

// A Name represents an XML name (Local) annotated
// with a name space identifier (Space).
// In tokens returned by [Decoder.Token], the Space identifier
// is given as a canonical URL, not the short prefix used
// in the document being parsed.
partial struct Name {
    public @string Space, Local;
}

// An Attr represents an attribute in an XML element (Name=Value).
partial struct Attr {
    public Name Name;
    public @string Value;
}

// A StartElement represents an XML start element.
partial struct StartElement {
    public Name Name;
    public slice<Attr> Attr;
}

// Copy creates a new copy of StartElement.
public static StartElement Copy(this StartElement e) {
    var attrs = new slice<Attr>(len(e.Attr));
    copy(attrs, e.Attr);
    e.Attr = attrs;
    return e;
}

// End returns the corresponding XML end element.
public static EndElement End(this StartElement e) {
    return new EndElement(e.Name);
}

// An EndElement represents an XML end element.
partial struct EndElement {
    public Name Name;
}

// A CharData represents XML character data (raw text),
// in which XML escape sequences have been replaced by
// the characters they represent.
partial struct CharData /*[]byte*/;

// Copy creates a new copy of CharData.
public static CharData Copy(this CharData c) {
    return (CharData)bytes.Clone(c);
}

// A Comment represents an XML comment of the form <!--comment-->.
// The bytes do not include the <!-- and --> comment markers.
partial struct Comment /*[]byte*/;

// Copy creates a new copy of Comment.
public static Comment Copy(this Comment c) {
    return (Comment)bytes.Clone(c);
}

// A ProcInst represents an XML processing instruction of the form <?target inst?>
partial struct ProcInst {
    public @string Target;
    public slice<byte> Inst;
}

// Copy creates a new copy of ProcInst.
public static ProcInst Copy(this ProcInst p) {
    p.Inst = bytes.Clone(p.Inst);
    return p;
}

// A Directive represents an XML directive of the form <!text>.
// The bytes do not include the <! and > markers.
partial struct Directive /*[]byte*/;

// Copy creates a new copy of Directive.
public static Directive Copy(this Directive d) {
    return (Directive)bytes.Clone(d);
}

// CopyToken returns a copy of a Token.
public static ΔToken CopyToken(ΔToken t) {
    switch (t.type()) {
    case CharData v: {
        return v.Copy();
    }
    case Comment v: {
        return v.Copy();
    }
    case Directive v: {
        return v.Copy();
    }
    case ProcInst v: {
        return v.Copy();
    }
    case StartElement v: {
        return v.Copy();
    }}
    return t;
}

// A TokenReader is anything that can decode a stream of XML tokens, including a
// [Decoder].
//
// When Token encounters an error or end-of-file condition after successfully
// reading a token, it returns the token. It may return the (non-nil) error from
// the same call or return the error (and a nil token) from a subsequent call.
// An instance of this general case is that a TokenReader returning a non-nil
// token at the end of the token stream may return either io.EOF or a nil error.
// The next Read should return nil, [io.EOF].
//
// Implementations of Token are discouraged from returning a nil token with a
// nil error. Callers should treat a return of nil, nil as indicating that
// nothing happened; in particular it does not indicate EOF.
partial interface TokenReader {
    (ΔToken, error) Token();
}

// A Decoder represents an XML parser reading a particular input stream.
// The parser assumes that its input is encoded in UTF-8.
partial struct Decoder {
    // Strict defaults to true, enforcing the requirements
    // of the XML specification.
    // If set to false, the parser allows input containing common
    // mistakes:
    //	* If an element is missing an end tag, the parser invents
    //	  end tags as necessary to keep the return values from Token
    //	  properly balanced.
    //	* In attribute values and character data, unknown or malformed
    //	  character entities (sequences beginning with &) are left alone.
    //
    // Setting:
    //
    //	d.Strict = false
    //	d.AutoClose = xml.HTMLAutoClose
    //	d.Entity = xml.HTMLEntity
    //
    // creates a parser that can handle typical HTML.
    //
    // Strict mode does not enforce the requirements of the XML name spaces TR.
    // In particular it does not reject name space tags using undefined prefixes.
    // Such tags are recorded with the unknown prefix as the name space URL.
    public bool Strict;
    // When Strict == false, AutoClose indicates a set of elements to
    // consider closed immediately after they are opened, regardless
    // of whether an end element is present.
    public slice<@string> AutoClose;
    // Entity can be used to map non-standard entity names to string replacements.
    // The parser behaves as if these standard mappings are present in the map,
    // regardless of the actual map content:
    //
    //	"lt": "<",
    //	"gt": ">",
    //	"amp": "&",
    //	"apos": "'",
    //	"quot": `"`,
    public map<@string, @string> Entity;
    // CharsetReader, if non-nil, defines a function to generate
    // charset-conversion readers, converting from the provided
    // non-UTF-8 charset into UTF-8. If CharsetReader is nil or
    // returns an error, parsing stops with an error. One of the
    // CharsetReader's result values must be non-nil.
    public Func<@string, io.Reader, (io.Reader, error)> CharsetReader;
    // DefaultSpace sets the default name space used for unadorned tags,
    // as if the entire XML stream were wrapped in an element containing
    // the attribute xmlns="DefaultSpace".
    public @string DefaultSpace;
    internal io.ByteReader r;
    internal TokenReader t;
    internal bytes.Buffer buf;
    internal ж<bytes.Buffer> saved;
    internal ж<stack> stk;
    internal nint stkDepth;
    internal ж<stack> free;
    internal bool needClose;
    internal Name toClose;
    internal ΔToken nextToken;
    internal nint nextByte;
    internal map<@string, @string> ns;
    internal error err;
    internal nint line;
    internal int64 linestart;
    internal int64 offset;
    internal bool inUnmarshalXML;
}

// NewDecoder creates a new XML parser reading from r.
// If r does not implement [io.ByteReader], NewDecoder will
// do its own buffering.
public static ж<Decoder> NewDecoder(io.Reader r) {
    var d = Ꮡ(new Decoder(
        ns: new map<@string, @string>(),
        nextByte: -1,
        line: 1,
        Strict: true
    ));
    d.switchToReader(r);
    return d;
}

// NewTokenDecoder creates a new XML parser using an underlying token stream.
public static ж<Decoder> NewTokenDecoder(TokenReader t) {
    // Is it already a Decoder?
    var (dΔ1, ok) = t._<ж<Decoder>>(ᐧ); if (ok) {
        return dΔ1;
    }
    var d = Ꮡ(new Decoder(
        ns: new map<@string, @string>(),
        t: t,
        nextByte: -1,
        line: 1,
        Strict: true
    ));
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedEofˢ => ref unexpectedEofˢᵇ;
private static readonly @string unexpectedEofˢᵇ = "unexpected EOF"u8;

// Token returns the next XML token in the input stream.
// At the end of the input stream, Token returns nil, [io.EOF].
//
// Slices of bytes in the returned token data refer to the
// parser's internal buffer and remain valid only until the next
// call to Token. To acquire a copy of the bytes, call [CopyToken]
// or the token's Copy method.
//
// Token expands self-closing elements such as <br>
// into separate start and end elements returned by successive calls.
//
// Token guarantees that the [StartElement] and [EndElement]
// tokens it returns are properly nested and matched:
// if Token encounters an unexpected end element
// or EOF before all expected end elements,
// it will return an error.
//
// If [Decoder.CharsetReader] is called and returns an error,
// the error is wrapped and returned.
//
// Token implements XML name spaces as described by
// https://www.w3.org/TR/REC-xml-names/. Each of the
// [Name] structures contained in the Token has the Space
// set to the URL identifying its name space when known.
// If Token encounters an unrecognized name space prefix,
// it uses the prefix as the Space rather than report an error.
public static (ΔToken, error) Token(this ref Decoder d) {
    ΔToken t = default;
    error err = default;
    if (d.stk != nil && d.stk.kind == stkEOF) {
        return (default, io.EOF);
    }
    if (d.nextToken != nil) {
        t = d.nextToken;
        d.nextToken = default;
    } else {
        (t, err) = d.rawToken(); if (t == nil && err != nil) {
            if (AreEqual(err, io.EOF) && d.stk != nil && d.stk.kind != stkEOF) {
                err = d.syntaxError(unexpectedEofˢ);
            }
            return (default, err);
        }
        // We still have a token to process, so clear any
        // errors (e.g. EOF) and proceed.
        err = default;
    }
    if (!d.Strict) {
        var (t1, ok) = d.autoClose(t); if (ok) {
            d.nextToken = t;
            t = t1;
        }
    }
    switch (t.type()) {
    case StartElement t1ᴛ1: {
        ref var t1 = ref heap(t1ᴛ1, out var Ꮡt1);
        // In XML name spaces, the translations listed in the
        // attributes apply to the element name and
        // to the other attribute names, so process
        // the translations first.
        foreach (var (_, a) in t1.Attr) {
            if (a.Name.Space == xmlnsPrefix) {
                var (v, ok) = d.ns[a.Name.Local, ꟷ];
                d.pushNs(a.Name.Local, v, ok);
                d.ns[a.Name.Local] = a.Value;
            }
            if (a.Name.Space == ""u8 && a.Name.Local == xmlnsPrefix) {
                // Default space for untagged names
                var (v, ok) = d.ns[""u8, ꟷ];
                d.pushNs(""u8, v, ok);
                d.ns[""u8] = a.Value;
            }
        }

        d.pushElement(t1.Name);
        d.translate(Ꮡt1.ᏑName, true);
        foreach (var i in range(t1.Attr)) {
            d.translate(Ꮡ(t1.Attr, i).ᏑName, false);
        }
        t = t1;
        break;
    }
    case EndElement t1ᴛ2: {
        ref var t1 = ref heap(t1ᴛ2, out var Ꮡt1);
        if (!d.popElement(Ꮡt1)) {
            return (default, d.err);
        }
        t = t1;
        break;
    }}
    return (t, err);
}

internal static ref readonly @string xmlURL => ref xmlURLᵇ;
private static readonly @string xmlURLᵇ = "http://www.w3.org/XML/1998/namespace"u8;
internal static ref readonly @string xmlnsPrefix => ref xmlnsPrefixᵇ;
private static readonly @string xmlnsPrefixᵇ = "xmlns"u8;
internal static ref readonly @string xmlPrefix => ref xmlPrefixᵇ;
private static readonly @string xmlPrefixᵇ = "xml"u8;

// Apply name space translation to name n.
// The default name space (for Space=="")
// applies only to element names, not to attribute names.
internal static void translate(this ref Decoder d, ж<Name> Ꮡn, bool isElementName) {
    ref var n = ref Ꮡn.DerefOrNull();

    switch (ᐧ) {
    case {} when n.Space == xmlnsPrefix: {
        return;
    }
    case {} when n.Space == ""u8 && !isElementName: {
        return;
    }
    case {} when n.Space == xmlPrefix: {
        n.Space = xmlURL;
        break;
    }
    case {} when n.Space == ""u8 && n.Local == xmlnsPrefix: {
        return;
    }}
    var (v, ok) = d.ns[n.Space, ꟷ]; if (ok) {
        n.Space = v;
    } else if (n.Space == ""u8) {
        n.Space = d.DefaultSpace;
    }
}

internal static void switchToReader(this ref Decoder d, io.Reader r) {
    // Get efficient byte at a time reader.
    // Assume that if reader has its own
    // ReadByte, it's efficient enough.
    // Otherwise, use bufio.
    var (rb, ok) = r._<io.ByteReader>(ᐧ); if (ok) {
        d.r = rb;
    } else {
        d.r = new bufio_ReaderжByteReader(bufio.NewReader(r));
    }
}

// Parsing state - stack holds old name space translations
// and the current set of open elements. The translations to pop when
// ending a given tag are *below* it on the stack, which is
// more work but forced on us by XML.
partial struct stack {
    internal ж<stack> next;
    internal nint kind;
    internal Name name;
    internal bool ok;
}

internal static UntypedInt stkStart => iota;
internal static UntypedInt stkNs => 1;
internal static UntypedInt stkEOF => 2;

internal static ж<stack> push(this ref Decoder d, nint kind) {
    var s = d.free;
    if (s != nil) {
        d.free = s.next;
    } else {
        s = @new<stack>();
    }
    s.next = d.stk;
    s.kind = kind;
    if (kind == stkStart) {
        d.stkDepth++;
    }
    d.stk = s;
    return s;
}

internal static ж<stack> pop(this ref Decoder d) {
    var s = d.stk;
    if (s != nil) {
        if (s.kind == stkStart) {
            d.stkDepth--;
        }
        d.stk = s.next;
        s.next = d.free;
        d.free = s;
    }
    return s;
}

// Record that after the current element is finished
// (that element is already pushed on the stack)
// Token should return EOF until popEOF is called.
internal static void pushEOF(this ref Decoder d) {
    // Walk down stack to find Start.
    // It might not be the top, because there might be stkNs
    // entries above it.
    var start = d.stk;
    while (start.kind != stkStart) {
        start = start.next;
    }
    // The stkNs entries below a start are associated with that
    // element too; skip over them.
    while (start.next != nil && start.next.kind == stkNs) {
        start = start.next;
    }
    var s = d.free;
    if (s != nil) {
        d.free = s.next;
    } else {
        s = @new<stack>();
    }
    s.kind = stkEOF;
    s.next = start.next;
    start.next = s;
}

// Undo a pushEOF.
// The element must have been finished, so the EOF should be at the top of the stack.
internal static bool popEOF(this ref Decoder d) {
    if (d.stk == nil || d.stk.kind != stkEOF) {
        return false;
    }
    d.pop();
    return true;
}

// Record that we are starting an element with the given name.
internal static void pushElement(this ref Decoder d, Name name) {
    var s = d.push(stkStart);
    s.name = name;
}

// Record that we are changing the value of ns[local].
// The old value is url, ok.
internal static void pushNs(this ref Decoder d, @string local, @string url, bool ok) {
    var s = d.push(stkNs);
    s.name.Local = local;
    s.name.Space = url;
    s.ok = ok;
}

// Creates a SyntaxError with the current line number.
internal static error syntaxError(this ref Decoder d, @string msg) {
    return new SyntaxErrorжerror(Ꮡ(new SyntaxError(Msg: msg, Line: d.line)));
}

// Record that we are ending an element with the given name.
// The name must match the record at the top of the stack,
// which must be a pushElement record.
// After popping the element, apply any undo records from
// the stack to restore the name translations that existed
// before we saw this element.
internal static bool popElement(this ref Decoder d, ж<EndElement> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var s = d.pop();
    var name = t.Name;
    switch (ᐧ) {
    case {} when s == nil || s.kind != stkStart: {
        d.err = d.syntaxError("unexpected end element </"u8 + name.Local + ">"u8);
        return false;
    }
    case {} when s.name.Local != name.Local: {
        if (!d.Strict) {
            d.needClose = true;
            d.toClose = t.Name;
            t.Name = s.name;
            return true;
        }
        d.err = d.syntaxError("element <"u8 + s.name.Local + "> closed by </"u8 + name.Local + ">"u8);
        return false;
    }
    case {} when s.name.Space != name.Space: {
        @string ns = name.Space;
        if (name.Space == ""u8) {
            ns = @"""""";
        }
        d.err = d.syntaxError("element <"u8 + s.name.Local + "> in space "u8 + s.name.Space + " closed by </"u8 + name.Local + "> in space "u8 + ns);
        return false;
    }}

    d.translate(Ꮡt.ᏑName, true);

    // Pop stack until a Start or EOF is on the top, undoing the
    // translations that were associated with the element we just closed.
    while (d.stk != nil && d.stk.kind != stkStart && d.stk.kind != stkEOF) {
        var sΔ1 = d.pop();
        if (sΔ1.ok) {
            d.ns[sΔ1.name.Local] = sΔ1.name.Space;
        } else {
            delete(d.ns, sΔ1.name.Local);
        }
    }

    return true;
}

// If the top element on the stack is autoclosing and
// t is not the end tag, invent the end tag.
internal static (ΔToken, bool) autoClose(this ref Decoder d, ΔToken t) {
    if (d.stk == nil || d.stk.kind != stkStart) {
        return (default, false);
    }
    foreach (var (_, s) in d.AutoClose) {
        if (strings.EqualFold(s, d.stk.name.Local)) {
            // This one should be auto closed if t doesn't close it.
            var (et, ok) = t._<EndElement>(ᐧ);
            if (!ok || !strings.EqualFold(et.Name.Local, d.stk.name.Local)) {
                return (new EndElement(d.stk.name), true);
            }
            break;
        }
    }
    return (default, false);
}

internal static error errRawToken = errors.New("xml: cannot use RawToken from UnmarshalXML method");

// RawToken is like [Decoder.Token] but does not verify that
// start and end elements match and does not translate
// name space prefixes to their corresponding URLs.
public static (ΔToken, error) RawToken(this ref Decoder d) {
    if (d.inUnmarshalXML) {
        return (default, errRawToken);
    }
    return d.rawToken();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string expectedElementNameAfterˢ => ref expectedElementNameAfterˢᵇ;
private static readonly @string expectedElementNameAfterˢᵇ = "expected element name after </"u8;
internal static ref readonly @string expectedTargetNameAfterˢ => ref expectedTargetNameAfterˢᵇ;
private static readonly @string expectedTargetNameAfterˢᵇ = "expected target name after <?"u8;
internal static ref readonly @string versionˢ => ref versionˢᵇ;
private static readonly @string versionˢᵇ = "version"u8;
internal static ref readonly @string encodingˢ => ref encodingˢᵇ;
private static readonly @string encodingˢᵇ = "encoding"u8;
internal static ref readonly @string utf8ˢ => ref utf8ˢᵇ;
private static readonly @string utf8ˢᵇ = "utf-8"u8;
internal static ref readonly @string invalidSequenceNotPartOfˢ => ref invalidSequenceNotPartOfˢᵇ;
private static readonly @string invalidSequenceNotPartOfˢᵇ = "invalid sequence <!- not part of <!--"u8;
internal static ref readonly @string invalidSequenceNotˢ => ref invalidSequenceNotˢᵇ;
private static readonly @string invalidSequenceNotˢᵇ = @"invalid sequence ""--"" not allowed in comments"u8;
internal static ref readonly @string invalidSequenceˢ => ref invalidSequenceˢᵇ;
private static readonly @string invalidSequenceˢᵇ = "invalid <![ sequence"u8;
internal static ref readonly @string expectedElementNameAfterˢ2 => ref expectedElementNameAfterˢ2ᵇ;
private static readonly @string expectedElementNameAfterˢ2ᵇ = "expected element name after <"u8;
internal static ref readonly @string expectedInElementˢ => ref expectedInElementˢᵇ;
private static readonly @string expectedInElementˢᵇ = "expected /> in element"u8;
internal static ref readonly @string expectedAttributeNameInˢ => ref expectedAttributeNameInˢᵇ;
private static readonly @string expectedAttributeNameInˢᵇ = "expected attribute name in element"u8;
internal static ref readonly @string attributeNameWithoutInˢ => ref attributeNameWithoutInˢᵇ;
private static readonly @string attributeNameWithoutInˢᵇ = "attribute name without = in element"u8;

internal static (ΔToken, error) rawToken(this ref Decoder d) {
    if (d.t != nil) {
        return d.t.Token();
    }
    if (d.err != nil) {
        return (default, d.err);
    }
    if (d.needClose) {
        // The last element we read was self-closing and
        // we returned just the StartElement half.
        // Return the EndElement half now.
        d.needClose = false;
        return (new EndElement(d.toClose), default);
    }

    var (b, ok) = d.getc();
    if (!ok) {
        return (default, d.err);
    }

    if (b != '<') {
        // Text section.
        d.ungetc(b);
        var data = d.text(-1, false);
        if (data == nil) {
            return (default, d.err);
        }
        return ((CharData)data, default);
    }

    (b, ok) = d.mustgetc(); if (!ok) {
        return (default, d.err);
    }
    switch (b) {
    case (byte)'/': {
        // </: End element
        Name nameΔ2 = default;
        (nameΔ2, ok) = d.nsname(); if (!ok) {
            if (d.err == nil) {
                d.err = d.syntaxError(expectedElementNameAfterˢ);
            }
            return (default, d.err);
        }
        d.space();
        (b, ok) = d.mustgetc(); if (!ok) {
            return (default, d.err);
        }
        if (b != '>') {
            d.err = d.syntaxError("invalid characters between </"u8 + nameΔ2.Local + " and >"u8);
            return (default, d.err);
        }
        return (new EndElement(nameΔ2), default);
    }
    case (byte)'?': {
        // <?: Processing instruction.
        @string targetΔ1 = default;
        (targetΔ1, ok) = d.name(); if (!ok) {
            if (d.err == nil) {
                d.err = d.syntaxError(expectedTargetNameAfterˢ);
            }
            return (default, d.err);
        }
        d.space();
        d.buf.Reset();
        byte b0 = default;
        for (;;) {
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default, d.err);
            }
            d.buf.WriteByte(b);
            if (b0 == '?' && b == '>') {
                break;
            }
            b0 = b;
        }
        var data = d.buf.Bytes();
        data = data.slice(0, len(data) - 2); // chop ?>

        if (targetΔ1 == "xml"u8) {
            @string content = (@string)data;
            @string ver = procInst(versionˢ, content);
            if (ver != ""u8 && ver != "1.0"u8) {
                d.err = fmt.Errorf("xml: unsupported version %q; only version 1.0 is supported"u8, ver);
                return (default, d.err);
            }
            @string enc = procInst(encodingˢ, content);
            if (enc != ""u8 && enc != "utf-8"u8 && enc != "UTF-8"u8 && !strings.EqualFold(enc, utf8ˢ)) {
                if (d.CharsetReader == nil) {
                    d.err = fmt.Errorf("xml: encoding %q declared but Decoder.CharsetReader is nil"u8, enc);
                    return (default, d.err);
                }
                var (newr, err) = d.CharsetReader(enc, d.r._<io.Reader>());
                if (err != nil) {
                    d.err = fmt.Errorf("xml: opening charset %q: %w"u8, enc, err);
                    return (default, d.err);
                }
                if (newr == nil) {
                    throw panic("CharsetReader returned a nil Reader for charset " + enc);
                }
                d.switchToReader(newr);
            }
        }
        return (new ProcInst(targetΔ1, data), default);
    }
    case (byte)'!': {
        // <!: Maybe comment, maybe CDATA.
        (b, ok) = d.mustgetc(); if (!ok) {
            return (default, d.err);
        }
        switch (b) {
        case (byte)'-': { // <!-
            // Probably <!-- for a comment.
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default, d.err);
            }
            if (b != '-') {
                d.err = d.syntaxError(invalidSequenceNotPartOfˢ);
                return (default, d.err);
            }
            // Look for terminator.
            d.buf.Reset();
            byte b0 = default;
            byte b1 = default;
            for (;;) {
                (b, ok) = d.mustgetc(); if (!ok) {
                    return (default, d.err);
                }
                d.buf.WriteByte(b);
                if (b0 == '-' && b1 == '-') {
                    if (b != '>') {
                        d.err = d.syntaxError(
                            invalidSequenceNotˢ);
                        return (default, d.err);
                    }
                    break;
                }
                (b0, b1) = (b1, b);
            }
            var data = d.buf.Bytes();
            data = data.slice(0, len(data) - 3); // chop -->
            return ((Comment)data, default);
        }
        case (byte)'[': { // <![
            // Probably <![CDATA[.
            for (nint i = 0; i < 6; i++) {
                (b, ok) = d.mustgetc(); if (!ok) {
                    return (default, d.err);
                }
                if (b != LiteralByteAt("CDATA["u8, i)) {
                    d.err = d.syntaxError(invalidSequenceˢ);
                    return (default, d.err);
                }
            }
            // Have <![CDATA[.  Read text until ]]>.
            var data = d.text(-1, true);
            if (data == nil) {
                return (default, d.err);
            }
            return ((CharData)data, default);
        }}

        // Probably a directive: <!DOCTYPE ...>, <!ENTITY ...>, etc.
        // We don't care, but accumulate for caller. Quoted angle
        // brackets do not count for nesting.
        d.buf.Reset();
        d.buf.WriteByte(b);
        var inquote = (uint8)0;
        nint depth = 0;
        for (;;) {
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default, d.err);
            }
            if (inquote == 0 && b == '>' && depth == 0) {
                break;
            }
HandleB:
            d.buf.WriteByte(b);
            switch (ᐧ) {
            case {} when b == inquote: {
                inquote = 0;
                break;
            }
            case {} when inquote is not 0: {
                // in quotes, no special action
                break;
            }
            case {} when b == (byte)'\'' || b == (byte)'"': {
                inquote = b;
                break;
            }
            case {} when b == (byte)'>' && inquote == 0: {
                depth--;
                break;
            }
            case {} when b == (byte)'<' && inquote == 0: {
                // Look for <!-- to begin comment.
                @string s = "!--";
                for (nint i = 0; i < len(s); i++) {
                    (b, ok) = d.mustgetc(); if (!ok) {
                        return (default, d.err);
                    }
                    if (b != s[i]) {
                        for (nint j = 0; j < i; j++) {
                            d.buf.WriteByte(s[j]);
                        }
                        depth++;
                        goto HandleB;
                    }
                }

                // Remove < that was written above.
                d.buf.Truncate(d.buf.Len() - 1);

                // Look for terminator.
                byte b0 = default;
                byte b1 = default;
                for (;;) {
                    (b, ok) = d.mustgetc(); if (!ok) {
                        return (default, d.err);
                    }
                    if (b0 == '-' && b1 == '-' && b == '>') {
                        break;
                    }
                    (b0, b1) = (b1, b);
                }

                // Replace the comment with a space in the returned Directive
                // body, so that markup parts that were separated by the comment
                // (like a "<" and a "!") don't get joined when re-encoding the
                // Directive, taking new semantic meaning.
                d.buf.WriteByte((byte)' ');
                break;
            }}
        }
        return ((Directive)d.buf.Bytes(), default);
    }}

    // Must be an open element like <a href="foo">
    d.ungetc(b);

    Name name = default;
    bool empty = default;
    slice<Attr> attr = default;
    (name, ok) = d.nsname(); if (!ok) {
        if (d.err == nil) {
            d.err = d.syntaxError(expectedElementNameAfterˢ2);
        }
        return (default, d.err);
    }

    attr = slice<Attr>([]);
    for (;;) {
        d.space();
        (b, ok) = d.mustgetc(); if (!ok) {
            return (default, d.err);
        }
        if (b == '/') {
            empty = true;
            (b, ok) = d.mustgetc(); if (!ok) {
                return (default, d.err);
            }
            if (b != '>') {
                d.err = d.syntaxError(expectedInElementˢ);
                return (default, d.err);
            }
            break;
        }
        if (b == '>') {
            break;
        }
        d.ungetc(b);

        var a = new Attr();
        (a.Name, ok) = d.nsname(); if (!ok) {
            if (d.err == nil) {
                d.err = d.syntaxError(expectedAttributeNameInˢ);
            }
            return (default, d.err);
        }
        d.space();
        (b, ok) = d.mustgetc(); if (!ok) {
            return (default, d.err);
        }
        if (b != '=') {
            if (d.Strict) {
                d.err = d.syntaxError(attributeNameWithoutInˢ);
                return (default, d.err);
            }
            d.ungetc(b);
            a.Value = a.Name.Local;
        } else {
            d.space();
            var data = d.attrval();
            if (data == nil) {
                return (default, d.err);
            }
            a.Value = (@string)data;
        }
        attr = append(attr, a);
    }
    if (empty) {
        d.needClose = true;
        d.toClose = name;
    }
    return (new StartElement(name, attr), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unquotedOrMissingˢ => ref unquotedOrMissingˢᵇ;
private static readonly @string unquotedOrMissingˢᵇ = "unquoted or missing attribute value in element"u8;

internal static slice<byte> attrval(this ref Decoder d) {
    var (b, ok) = d.mustgetc();
    if (!ok) {
        return default;
    }
    // Handle quoted attribute values
    if (b == '"' || b == '\'') {
        return d.text((nint)b, false);
    }
    // Handle unquoted attribute values for strict parsers
    if (d.Strict) {
        d.err = d.syntaxError(unquotedOrMissingˢ);
        return default;
    }
    // Handle unquoted attribute values for unstrict parsers
    d.ungetc(b);
    d.buf.Reset();
    for (;;) {
        (b, ok) = d.mustgetc();
        if (!ok) {
            return default;
        }
        // https://www.w3.org/TR/REC-html40/intro/sgmltut.html#h-3.2.2
        if ('a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || '0' <= b && b <= '9' || b == '_' || b == ':' || b == '-') {
            d.buf.WriteByte(b);
        } else {
            d.ungetc(b);
            break;
        }
    }
    return d.buf.Bytes();
}

// Skip spaces if any
internal static void space(this ref Decoder d) {
    for (;;) {
        var (b, ok) = d.getc();
        if (!ok) {
            return;
        }
        switch (b) {
        case (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t': {
            break;
        }
        default: {
            d.ungetc(b);
            return;
        }}
    }
}

// Read a single byte.
// If there is no byte to read, return ok==false
// and leave the error in d.err.
// Maintain line number.
internal static (byte b, bool ok) getc(this ref Decoder d) {
    byte b = default;

    if (d.err != nil) {
        return (0, false);
    }
    if (d.nextByte >= 0) {
        b = (byte)d.nextByte;
        d.nextByte = -1;
    } else {
        (b, d.err) = d.r.ReadByte();
        if (d.err != nil) {
            return (0, false);
        }
        if (d.saved != nil) {
            d.saved.WriteByte(b);
        }
    }
    if (b == '\n') {
        d.line++;
        d.linestart = d.offset + 1;
    }
    d.offset++;
    return (b, true);
}

// InputOffset returns the input stream byte offset of the current decoder position.
// The offset gives the location of the end of the most recently returned token
// and the beginning of the next token.
public static int64 InputOffset(this ref Decoder d) {
    return d.offset;
}

// InputPos returns the line of the current decoder position and the 1 based
// input position of the line. The position gives the location of the end of the
// most recently returned token.
public static (nint line, nint column) InputPos(this ref Decoder d) {
    return (d.line, (nint)(d.offset - d.linestart) + 1);
}

// Return saved offset.
// If we did ungetc (nextByte >= 0), have to back up one.
internal static nint savedOffset(this ref Decoder d) {
    nint n = d.saved.Len();
    if (d.nextByte >= 0) {
        n--;
    }
    return n;
}

// Must read a single byte.
// If there is no byte to read,
// set d.err to SyntaxError("unexpected EOF")
// and return ok==false
internal static (byte b, bool ok) mustgetc(this ref Decoder d) {
    byte b = default;
    bool ok = default;

    (b, ok) = d.getc(); if (!ok) {
        if (AreEqual(d.err, io.EOF)) {
            d.err = d.syntaxError(unexpectedEofˢ);
        }
    }
    return (b, ok);
}

// Unread a single byte.
internal static void ungetc(this ref Decoder d, byte b) {
    if (b == '\n') {
        d.line--;
    }
    d.nextByte = (nint)b;
    d.offset--;
}

internal static map<@string, rune> entity = new map<@string, rune>{
    ["lt"] = '<',
    ["gt"] = '>',
    ["amp"] = '&',
    ["apos"] = '\'',
    ["quot"] = '"'
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedEofInCdataˢ => ref unexpectedEofInCdataˢᵇ;
private static readonly @string unexpectedEofInCdataˢᵇ = "unexpected EOF in CDATA section"u8;
internal static ref readonly @string unescapedNotInCdataˢ => ref unescapedNotInCdataˢᵇ;
private static readonly @string unescapedNotInCdataˢᵇ = "unescaped ]]> not in CDATA section"u8;
internal static ref readonly @string unescapedInsideQuotedˢ => ref unescapedInsideQuotedˢᵇ;
private static readonly @string unescapedInsideQuotedˢᵇ = "unescaped < inside quoted string"u8;
internal static ref readonly @string invalidUtf8ˢ => ref invalidUtf8ˢᵇ;
private static readonly @string invalidUtf8ˢᵇ = "invalid UTF-8"u8;

// Read plain text section (XML calls it character data).
// If quote >= 0, we are in a quoted string and need to find the matching quote.
// If cdata == true, we are in a <![CDATA[ section and need to find ]]>.
// On failure return nil and leave the error in d.err.
internal static slice<byte> text(this ref Decoder d, nint quote, bool cdata) {
    byte b0 = default;
    byte b1 = default;
    nint trunc = default;
    d.buf.Reset();
Input:
    for (;;) {
        var (b, ok) = d.getc();
        if (!ok) {
            if (cdata) {
                if (AreEqual(d.err, io.EOF)) {
                    d.err = d.syntaxError(unexpectedEofInCdataˢ);
                }
                return default;
            }
            goto break_Input;
        }

        // <![CDATA[ section ends with ]]>.
        // It is an error for ]]> to appear in ordinary text,
        // but it is allowed in quoted strings.
        if (quote < 0 && b0 == ']' && b1 == ']' && b == '>') {
            if (cdata) {
                trunc = 2;
                goto break_Input;
            }
            d.err = d.syntaxError(unescapedNotInCdataˢ);
            return default;
        }

        // Stop reading text if we see a <.
        if (b == '<' && !cdata) {
            if (quote >= 0) {
                d.err = d.syntaxError(unescapedInsideQuotedˢ);
                return default;
            }
            d.ungetc((byte)'<');
            goto break_Input;
        }
        if (quote >= 0 && b == (byte)quote) {
            goto break_Input;
        }
        if (b == '&' && !cdata) {
            // Read escaped character expression up to semicolon.
            // XML in all its glory allows a document to define and use
            // its own character names with <!ENTITY ...> directives.
            // Parsers are required to recognize lt, gt, amp, apos, and quot
            // even if they have not been declared.
            nint before = d.buf.Len();
            d.buf.WriteByte((byte)'&');
            bool okΔ1 = default;
            @string text = default;
            bool haveText = default;
            (b, okΔ1) = d.mustgetc(); if (!okΔ1) {
                return default;
            }
            if (b == '#') {
                d.buf.WriteByte(b);
                (b, okΔ1) = d.mustgetc(); if (!okΔ1) {
                    return default;
                }
                nint @base = 10;
                if (b == 'x') {
                    @base = 16;
                    d.buf.WriteByte(b);
                    (b, okΔ1) = d.mustgetc(); if (!okΔ1) {
                        return default;
                    }
                }
                nint start = d.buf.Len();
                while ('0' <= b && b <= '9' || @base == 16 && 'a' <= b && b <= 'f' || @base == 16 && 'A' <= b && b <= 'F') {
                    d.buf.WriteByte(b);
                    (b, okΔ1) = d.mustgetc(); if (!okΔ1) {
                        return default;
                    }
                }
                if (b != ';') {
                    d.ungetc(b);
                } else {
                    @string s = (@string)(d.buf.Bytes().slice(start));
                    d.buf.WriteByte((byte)';');
                    var (n, err) = strconv.ParseUint(s, @base, 64);
                    if (err == nil && n <= unicode.MaxRune) {
                        text = (@string)(rune)n;
                        haveText = true;
                    }
                }
            } else {
                d.ungetc(b);
                if (!d.readName()) {
                    if (d.err != nil) {
                        return default;
                    }
                }
                (b, okΔ1) = d.mustgetc(); if (!okΔ1) {
                    return default;
                }
                if (b != ';') {
                    d.ungetc(b);
                } else {
                    var name = d.buf.Bytes().slice(before + 1);
                    d.buf.WriteByte((byte)';');
                    if (isName(name)) {
                        @string s = (@string)name;
                        var (r, okΔ2) = entity[s, ꟷ]; if (okΔ2) {
                            text = (@string)r;
                            haveText = true;
                        } else if (d.Entity != nil) {
                            (text, haveText) = d.Entity[s, ꟷ];
                        }
                    }
                }
            }

            if (haveText) {
                d.buf.Truncate(before);
                d.buf.WriteString(text);
                (b0, b1) = (0, 0);
                goto continue_Input;
            }
            if (!d.Strict) {
                (b0, b1) = (0, 0);
                goto continue_Input;
            }
            @string ent = (@string)(d.buf.Bytes().slice(before));
            if (ent[len(ent) - 1] != ';') {
                ent += " (no semicolon)"u8;
            }
            d.err = d.syntaxError("invalid character entity "u8 + ent);
            return default;
        }

        // We must rewrite unescaped \r and \r\n into \n.
        if (b == '\r') {
            d.buf.WriteByte((byte)'\n');
        } else if (b1 == '\r' && b == '\n') {
            // Skip \r\n--we already wrote \n.
        } else {
            d.buf.WriteByte(b);
        }

        (b0, b1) = (b1, b);
continue_Input:;
    }
break_Input:;
    var data = d.buf.Bytes();
    data = data.slice(0, len(data) - trunc);

    // Inspect each rune for being a disallowed character.
    var buf = data;
    while (len(buf) > 0) {
        var (r, size) = utf8.DecodeRune(buf);
        if (r == utf8.RuneError && size == 1) {
            d.err = d.syntaxError(invalidUtf8ˢ);
            return default;
        }
        buf = buf.slice(size);
        if (!isInCharacterRange(r)) {
            d.err = d.syntaxError(fmt.Sprintf("illegal character code %U"u8, r));
            return default;
        }
    }

    return data;
}

// Decide whether the given rune is in the XML Character Range, per
// the Char production of https://www.xml.com/axml/testaxml.htm,
// Section 2.2 Characters.
internal static bool /*inrange*/ isInCharacterRange(rune r) {
    return r == 0x09 || r == 0x0A || r == 0x0D || r >= 0x20 && r <= 0xD7FF || r >= 0xE000 && r <= 0xFFFD || r >= 0x10000 && r <= 0x10FFFF;
}

// Get name space name: name with a : stuck in the middle.
// The part before the : is the name space identifier.
internal static (Name name, bool ok) nsname(this ref Decoder d) {
    Name name = default;
    bool ok = default;

    (var s, ok) = d.name();
    if (!ok) {
        return (name, ok);
    }
    if (strings.Count(s, ":") > 1) {
        return (name, false);
    } else if (strings.Cut(s, ":") is var (space, local, okΔ1) && (!okΔ1 || space == ""u8 || local == ""u8)) {
        name.Local = s;
    } else {
        name.Space = space;
        name.Local = local;
    }
    return (name, true);
}

// Get name: /first(first|second)*/
// Do not set d.err if the name is missing (unless unexpected EOF is received):
// let the caller provide better context.
internal static (@string s, bool ok) name(this ref Decoder d) {
    d.buf.Reset();
    if (!d.readName()) {
        return ("", false);
    }

    // Now we check the characters.
    var b = d.buf.Bytes();
    if (!isName(b)) {
        d.err = d.syntaxError("invalid XML name: "u8 + ((sstring)b));
        return ("", false);
    }
    return ((@string)b, true);
}

// Read a name and append its bytes to d.buf.
// The name is delimited by any single-byte character not valid in names.
// All multi-byte characters are accepted; the caller must check their validity.
internal static bool /*ok*/ readName(this ref Decoder d) {
    bool ok = default;

    byte b = default;
    (b, ok) = d.mustgetc(); if (!ok) {
        return ok;
    }
    if (b < utf8.RuneSelf && !isNameByte(b)) {
        d.ungetc(b);
        return false;
    }
    d.buf.WriteByte(b);

    for (;;) {
        (b, ok) = d.mustgetc(); if (!ok) {
            return ok;
        }
        if (b < utf8.RuneSelf && !isNameByte(b)) {
            d.ungetc(b);
            break;
        }
        d.buf.WriteByte(b);
    }
    return true;
}

internal static bool isNameByte(byte c) {
    return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c == ':' || c == '.' || c == '-';
}

internal static bool isName(slice<byte> s) {
    if (len(s) == 0) {
        return false;
    }
    var (c, n) = utf8.DecodeRune(s);
    if (c == utf8.RuneError && n == 1) {
        return false;
    }
    if (!unicode.Is(first, c)) {
        return false;
    }
    while (n < len(s)) {
        s = s.slice(n);
        (c, n) = utf8.DecodeRune(s);
        if (c == utf8.RuneError && n == 1) {
            return false;
        }
        if (!unicode.Is(first, c) && !unicode.Is(second, c)) {
            return false;
        }
    }
    return true;
}

internal static bool isNameString(@string s) {
    if (len(s) == 0) {
        return false;
    }
    var (c, n) = utf8.DecodeRuneInString(s);
    if (c == utf8.RuneError && n == 1) {
        return false;
    }
    if (!unicode.Is(first, c)) {
        return false;
    }
    while (n < len(s)) {
        s = s.slice(n);
        (c, n) = utf8.DecodeRuneInString(s);
        if (c == utf8.RuneError && n == 1) {
            return false;
        }
        if (!unicode.Is(first, c) && !unicode.Is(second, c)) {
            return false;
        }
    }
    return true;
}

// These tables were generated by cut and paste from Appendix B of
// the XML spec at https://www.xml.com/axml/testaxml.htm
// and then reformatting. First corresponds to (Letter | '_' | ':')
// and second corresponds to NameChar.
[GoLazy] partial class lazyᴛfirst { internal static ж<unicode.RangeTable> first = Ꮡ(new unicode.RangeTable(
    R16: slice<unicode.Range16>([
        new(0x003A, 0x003A, 1),
        new(0x0041, 0x005A, 1),
        new(0x005F, 0x005F, 1),
        new(0x0061, 0x007A, 1),
        new(0x00C0, 0x00D6, 1),
        new(0x00D8, 0x00F6, 1),
        new(0x00F8, 0x00FF, 1),
        new(0x0100, 0x0131, 1),
        new(0x0134, 0x013E, 1),
        new(0x0141, 0x0148, 1),
        new(0x014A, 0x017E, 1),
        new(0x0180, 0x01C3, 1),
        new(0x01CD, 0x01F0, 1),
        new(0x01F4, 0x01F5, 1),
        new(0x01FA, 0x0217, 1),
        new(0x0250, 0x02A8, 1),
        new(0x02BB, 0x02C1, 1),
        new(0x0386, 0x0386, 1),
        new(0x0388, 0x038A, 1),
        new(0x038C, 0x038C, 1),
        new(0x038E, 0x03A1, 1),
        new(0x03A3, 0x03CE, 1),
        new(0x03D0, 0x03D6, 1),
        new(0x03DA, 0x03E0, 2),
        new(0x03E2, 0x03F3, 1),
        new(0x0401, 0x040C, 1),
        new(0x040E, 0x044F, 1),
        new(0x0451, 0x045C, 1),
        new(0x045E, 0x0481, 1),
        new(0x0490, 0x04C4, 1),
        new(0x04C7, 0x04C8, 1),
        new(0x04CB, 0x04CC, 1),
        new(0x04D0, 0x04EB, 1),
        new(0x04EE, 0x04F5, 1),
        new(0x04F8, 0x04F9, 1),
        new(0x0531, 0x0556, 1),
        new(0x0559, 0x0559, 1),
        new(0x0561, 0x0586, 1),
        new(0x05D0, 0x05EA, 1),
        new(0x05F0, 0x05F2, 1),
        new(0x0621, 0x063A, 1),
        new(0x0641, 0x064A, 1),
        new(0x0671, 0x06B7, 1),
        new(0x06BA, 0x06BE, 1),
        new(0x06C0, 0x06CE, 1),
        new(0x06D0, 0x06D3, 1),
        new(0x06D5, 0x06D5, 1),
        new(0x06E5, 0x06E6, 1),
        new(0x0905, 0x0939, 1),
        new(0x093D, 0x093D, 1),
        new(0x0958, 0x0961, 1),
        new(0x0985, 0x098C, 1),
        new(0x098F, 0x0990, 1),
        new(0x0993, 0x09A8, 1),
        new(0x09AA, 0x09B0, 1),
        new(0x09B2, 0x09B2, 1),
        new(0x09B6, 0x09B9, 1),
        new(0x09DC, 0x09DD, 1),
        new(0x09DF, 0x09E1, 1),
        new(0x09F0, 0x09F1, 1),
        new(0x0A05, 0x0A0A, 1),
        new(0x0A0F, 0x0A10, 1),
        new(0x0A13, 0x0A28, 1),
        new(0x0A2A, 0x0A30, 1),
        new(0x0A32, 0x0A33, 1),
        new(0x0A35, 0x0A36, 1),
        new(0x0A38, 0x0A39, 1),
        new(0x0A59, 0x0A5C, 1),
        new(0x0A5E, 0x0A5E, 1),
        new(0x0A72, 0x0A74, 1),
        new(0x0A85, 0x0A8B, 1),
        new(0x0A8D, 0x0A8D, 1),
        new(0x0A8F, 0x0A91, 1),
        new(0x0A93, 0x0AA8, 1),
        new(0x0AAA, 0x0AB0, 1),
        new(0x0AB2, 0x0AB3, 1),
        new(0x0AB5, 0x0AB9, 1),
        new(0x0ABD, 0x0AE0, 0x23),
        new(0x0B05, 0x0B0C, 1),
        new(0x0B0F, 0x0B10, 1),
        new(0x0B13, 0x0B28, 1),
        new(0x0B2A, 0x0B30, 1),
        new(0x0B32, 0x0B33, 1),
        new(0x0B36, 0x0B39, 1),
        new(0x0B3D, 0x0B3D, 1),
        new(0x0B5C, 0x0B5D, 1),
        new(0x0B5F, 0x0B61, 1),
        new(0x0B85, 0x0B8A, 1),
        new(0x0B8E, 0x0B90, 1),
        new(0x0B92, 0x0B95, 1),
        new(0x0B99, 0x0B9A, 1),
        new(0x0B9C, 0x0B9C, 1),
        new(0x0B9E, 0x0B9F, 1),
        new(0x0BA3, 0x0BA4, 1),
        new(0x0BA8, 0x0BAA, 1),
        new(0x0BAE, 0x0BB5, 1),
        new(0x0BB7, 0x0BB9, 1),
        new(0x0C05, 0x0C0C, 1),
        new(0x0C0E, 0x0C10, 1),
        new(0x0C12, 0x0C28, 1),
        new(0x0C2A, 0x0C33, 1),
        new(0x0C35, 0x0C39, 1),
        new(0x0C60, 0x0C61, 1),
        new(0x0C85, 0x0C8C, 1),
        new(0x0C8E, 0x0C90, 1),
        new(0x0C92, 0x0CA8, 1),
        new(0x0CAA, 0x0CB3, 1),
        new(0x0CB5, 0x0CB9, 1),
        new(0x0CDE, 0x0CDE, 1),
        new(0x0CE0, 0x0CE1, 1),
        new(0x0D05, 0x0D0C, 1),
        new(0x0D0E, 0x0D10, 1),
        new(0x0D12, 0x0D28, 1),
        new(0x0D2A, 0x0D39, 1),
        new(0x0D60, 0x0D61, 1),
        new(0x0E01, 0x0E2E, 1),
        new(0x0E30, 0x0E30, 1),
        new(0x0E32, 0x0E33, 1),
        new(0x0E40, 0x0E45, 1),
        new(0x0E81, 0x0E82, 1),
        new(0x0E84, 0x0E84, 1),
        new(0x0E87, 0x0E88, 1),
        new(0x0E8A, 0x0E8D, 3),
        new(0x0E94, 0x0E97, 1),
        new(0x0E99, 0x0E9F, 1),
        new(0x0EA1, 0x0EA3, 1),
        new(0x0EA5, 0x0EA7, 2),
        new(0x0EAA, 0x0EAB, 1),
        new(0x0EAD, 0x0EAE, 1),
        new(0x0EB0, 0x0EB0, 1),
        new(0x0EB2, 0x0EB3, 1),
        new(0x0EBD, 0x0EBD, 1),
        new(0x0EC0, 0x0EC4, 1),
        new(0x0F40, 0x0F47, 1),
        new(0x0F49, 0x0F69, 1),
        new(0x10A0, 0x10C5, 1),
        new(0x10D0, 0x10F6, 1),
        new(0x1100, 0x1100, 1),
        new(0x1102, 0x1103, 1),
        new(0x1105, 0x1107, 1),
        new(0x1109, 0x1109, 1),
        new(0x110B, 0x110C, 1),
        new(0x110E, 0x1112, 1),
        new(0x113C, 0x1140, 2),
        new(0x114C, 0x1150, 2),
        new(0x1154, 0x1155, 1),
        new(0x1159, 0x1159, 1),
        new(0x115F, 0x1161, 1),
        new(0x1163, 0x1169, 2),
        new(0x116D, 0x116E, 1),
        new(0x1172, 0x1173, 1),
        new(0x1175, 0x119E, 0x119E - 0x1175),
        new(0x11A8, 0x11AB, 0x11AB - 0x11A8),
        new(0x11AE, 0x11AF, 1),
        new(0x11B7, 0x11B8, 1),
        new(0x11BA, 0x11BA, 1),
        new(0x11BC, 0x11C2, 1),
        new(0x11EB, 0x11F0, 0x11F0 - 0x11EB),
        new(0x11F9, 0x11F9, 1),
        new(0x1E00, 0x1E9B, 1),
        new(0x1EA0, 0x1EF9, 1),
        new(0x1F00, 0x1F15, 1),
        new(0x1F18, 0x1F1D, 1),
        new(0x1F20, 0x1F45, 1),
        new(0x1F48, 0x1F4D, 1),
        new(0x1F50, 0x1F57, 1),
        new(0x1F59, 0x1F5B, 0x1F5B - 0x1F59),
        new(0x1F5D, 0x1F5D, 1),
        new(0x1F5F, 0x1F7D, 1),
        new(0x1F80, 0x1FB4, 1),
        new(0x1FB6, 0x1FBC, 1),
        new(0x1FBE, 0x1FBE, 1),
        new(0x1FC2, 0x1FC4, 1),
        new(0x1FC6, 0x1FCC, 1),
        new(0x1FD0, 0x1FD3, 1),
        new(0x1FD6, 0x1FDB, 1),
        new(0x1FE0, 0x1FEC, 1),
        new(0x1FF2, 0x1FF4, 1),
        new(0x1FF6, 0x1FFC, 1),
        new(0x2126, 0x2126, 1),
        new(0x212A, 0x212B, 1),
        new(0x212E, 0x212E, 1),
        new(0x2180, 0x2182, 1),
        new(0x3007, 0x3007, 1),
        new(0x3021, 0x3029, 1),
        new(0x3041, 0x3094, 1),
        new(0x30A1, 0x30FA, 1),
        new(0x3105, 0x312C, 1),
        new(0x4E00, 0x9FA5, 1),
        new(0xAC00, 0xD7A3, 1)
    ])
)); }

[GoLazy] partial class lazyᴛsecond { internal static ж<unicode.RangeTable> second = Ꮡ(new unicode.RangeTable(
    R16: slice<unicode.Range16>([
        new(0x002D, 0x002E, 1),
        new(0x0030, 0x0039, 1),
        new(0x00B7, 0x00B7, 1),
        new(0x02D0, 0x02D1, 1),
        new(0x0300, 0x0345, 1),
        new(0x0360, 0x0361, 1),
        new(0x0387, 0x0387, 1),
        new(0x0483, 0x0486, 1),
        new(0x0591, 0x05A1, 1),
        new(0x05A3, 0x05B9, 1),
        new(0x05BB, 0x05BD, 1),
        new(0x05BF, 0x05BF, 1),
        new(0x05C1, 0x05C2, 1),
        new(0x05C4, 0x0640, 0x0640 - 0x05C4),
        new(0x064B, 0x0652, 1),
        new(0x0660, 0x0669, 1),
        new(0x0670, 0x0670, 1),
        new(0x06D6, 0x06DC, 1),
        new(0x06DD, 0x06DF, 1),
        new(0x06E0, 0x06E4, 1),
        new(0x06E7, 0x06E8, 1),
        new(0x06EA, 0x06ED, 1),
        new(0x06F0, 0x06F9, 1),
        new(0x0901, 0x0903, 1),
        new(0x093C, 0x093C, 1),
        new(0x093E, 0x094C, 1),
        new(0x094D, 0x094D, 1),
        new(0x0951, 0x0954, 1),
        new(0x0962, 0x0963, 1),
        new(0x0966, 0x096F, 1),
        new(0x0981, 0x0983, 1),
        new(0x09BC, 0x09BC, 1),
        new(0x09BE, 0x09BF, 1),
        new(0x09C0, 0x09C4, 1),
        new(0x09C7, 0x09C8, 1),
        new(0x09CB, 0x09CD, 1),
        new(0x09D7, 0x09D7, 1),
        new(0x09E2, 0x09E3, 1),
        new(0x09E6, 0x09EF, 1),
        new(0x0A02, 0x0A3C, 0x3A),
        new(0x0A3E, 0x0A3F, 1),
        new(0x0A40, 0x0A42, 1),
        new(0x0A47, 0x0A48, 1),
        new(0x0A4B, 0x0A4D, 1),
        new(0x0A66, 0x0A6F, 1),
        new(0x0A70, 0x0A71, 1),
        new(0x0A81, 0x0A83, 1),
        new(0x0ABC, 0x0ABC, 1),
        new(0x0ABE, 0x0AC5, 1),
        new(0x0AC7, 0x0AC9, 1),
        new(0x0ACB, 0x0ACD, 1),
        new(0x0AE6, 0x0AEF, 1),
        new(0x0B01, 0x0B03, 1),
        new(0x0B3C, 0x0B3C, 1),
        new(0x0B3E, 0x0B43, 1),
        new(0x0B47, 0x0B48, 1),
        new(0x0B4B, 0x0B4D, 1),
        new(0x0B56, 0x0B57, 1),
        new(0x0B66, 0x0B6F, 1),
        new(0x0B82, 0x0B83, 1),
        new(0x0BBE, 0x0BC2, 1),
        new(0x0BC6, 0x0BC8, 1),
        new(0x0BCA, 0x0BCD, 1),
        new(0x0BD7, 0x0BD7, 1),
        new(0x0BE7, 0x0BEF, 1),
        new(0x0C01, 0x0C03, 1),
        new(0x0C3E, 0x0C44, 1),
        new(0x0C46, 0x0C48, 1),
        new(0x0C4A, 0x0C4D, 1),
        new(0x0C55, 0x0C56, 1),
        new(0x0C66, 0x0C6F, 1),
        new(0x0C82, 0x0C83, 1),
        new(0x0CBE, 0x0CC4, 1),
        new(0x0CC6, 0x0CC8, 1),
        new(0x0CCA, 0x0CCD, 1),
        new(0x0CD5, 0x0CD6, 1),
        new(0x0CE6, 0x0CEF, 1),
        new(0x0D02, 0x0D03, 1),
        new(0x0D3E, 0x0D43, 1),
        new(0x0D46, 0x0D48, 1),
        new(0x0D4A, 0x0D4D, 1),
        new(0x0D57, 0x0D57, 1),
        new(0x0D66, 0x0D6F, 1),
        new(0x0E31, 0x0E31, 1),
        new(0x0E34, 0x0E3A, 1),
        new(0x0E46, 0x0E46, 1),
        new(0x0E47, 0x0E4E, 1),
        new(0x0E50, 0x0E59, 1),
        new(0x0EB1, 0x0EB1, 1),
        new(0x0EB4, 0x0EB9, 1),
        new(0x0EBB, 0x0EBC, 1),
        new(0x0EC6, 0x0EC6, 1),
        new(0x0EC8, 0x0ECD, 1),
        new(0x0ED0, 0x0ED9, 1),
        new(0x0F18, 0x0F19, 1),
        new(0x0F20, 0x0F29, 1),
        new(0x0F35, 0x0F39, 2),
        new(0x0F3E, 0x0F3F, 1),
        new(0x0F71, 0x0F84, 1),
        new(0x0F86, 0x0F8B, 1),
        new(0x0F90, 0x0F95, 1),
        new(0x0F97, 0x0F97, 1),
        new(0x0F99, 0x0FAD, 1),
        new(0x0FB1, 0x0FB7, 1),
        new(0x0FB9, 0x0FB9, 1),
        new(0x20D0, 0x20DC, 1),
        new(0x20E1, 0x3005, 0x3005 - 0x20E1),
        new(0x302A, 0x302F, 1),
        new(0x3031, 0x3035, 1),
        new(0x3099, 0x309A, 1),
        new(0x309D, 0x309E, 1),
        new(0x30FC, 0x30FE, 1)
    ])
)); }

// HTMLEntity is an entity map containing translations for the
// standard HTML entity characters.
//
// See the [Decoder.Strict] and [Decoder.Entity] fields' documentation.
[GoLazy] partial class lazyᴛHTMLEntity { public static readonly ж<map<@string, @string>> ᏑHTMLEntity = new StaticBox<map<@string, @string>>(htmlEntity); }
public static ref map<@string, @string> HTMLEntity => ref ((StaticBox<map<@string, @string>>)ᏑHTMLEntity).ValueSlot;

[GoLazy] partial class lazyᴛhtmlEntity { internal static map<@string, @string> htmlEntity = new map<@string, @string>{
    /*
		hget http://www.w3.org/TR/html4/sgml/entities.html |
		ssam '
			,y /\&gt;/ x/\&lt;(.|\n)+/ s/\n/ /g
			,x v/^\&lt;!ENTITY/d
			,s/\&lt;!ENTITY ([^ ]+) .*U\+([0-9A-F][0-9A-F][0-9A-F][0-9A-F]) .+/	"\1": "\\u\2",/g
		'
	*/
    ["nbsp"] = "\u00A0",
    ["iexcl"] = "\u00A1",
    ["cent"] = "\u00A2",
    ["pound"] = "\u00A3",
    ["curren"] = "\u00A4",
    ["yen"] = "\u00A5",
    ["brvbar"] = "\u00A6",
    ["sect"] = "\u00A7",
    ["uml"] = "\u00A8",
    ["copy"] = "\u00A9",
    ["ordf"] = "\u00AA",
    ["laquo"] = "\u00AB",
    ["not"] = "\u00AC",
    ["shy"] = "\u00AD",
    ["reg"] = "\u00AE",
    ["macr"] = "\u00AF",
    ["deg"] = "\u00B0",
    ["plusmn"] = "\u00B1",
    ["sup2"] = "\u00B2",
    ["sup3"] = "\u00B3",
    ["acute"] = "\u00B4",
    ["micro"] = "\u00B5",
    ["para"] = "\u00B6",
    ["middot"] = "\u00B7",
    ["cedil"] = "\u00B8",
    ["sup1"] = "\u00B9",
    ["ordm"] = "\u00BA",
    ["raquo"] = "\u00BB",
    ["frac14"] = "\u00BC",
    ["frac12"] = "\u00BD",
    ["frac34"] = "\u00BE",
    ["iquest"] = "\u00BF",
    ["Agrave"] = "\u00C0",
    ["Aacute"] = "\u00C1",
    ["Acirc"] = "\u00C2",
    ["Atilde"] = "\u00C3",
    ["Auml"] = "\u00C4",
    ["Aring"] = "\u00C5",
    ["AElig"] = "\u00C6",
    ["Ccedil"] = "\u00C7",
    ["Egrave"] = "\u00C8",
    ["Eacute"] = "\u00C9",
    ["Ecirc"] = "\u00CA",
    ["Euml"] = "\u00CB",
    ["Igrave"] = "\u00CC",
    ["Iacute"] = "\u00CD",
    ["Icirc"] = "\u00CE",
    ["Iuml"] = "\u00CF",
    ["ETH"] = "\u00D0",
    ["Ntilde"] = "\u00D1",
    ["Ograve"] = "\u00D2",
    ["Oacute"] = "\u00D3",
    ["Ocirc"] = "\u00D4",
    ["Otilde"] = "\u00D5",
    ["Ouml"] = "\u00D6",
    ["times"] = "\u00D7",
    ["Oslash"] = "\u00D8",
    ["Ugrave"] = "\u00D9",
    ["Uacute"] = "\u00DA",
    ["Ucirc"] = "\u00DB",
    ["Uuml"] = "\u00DC",
    ["Yacute"] = "\u00DD",
    ["THORN"] = "\u00DE",
    ["szlig"] = "\u00DF",
    ["agrave"] = "\u00E0",
    ["aacute"] = "\u00E1",
    ["acirc"] = "\u00E2",
    ["atilde"] = "\u00E3",
    ["auml"] = "\u00E4",
    ["aring"] = "\u00E5",
    ["aelig"] = "\u00E6",
    ["ccedil"] = "\u00E7",
    ["egrave"] = "\u00E8",
    ["eacute"] = "\u00E9",
    ["ecirc"] = "\u00EA",
    ["euml"] = "\u00EB",
    ["igrave"] = "\u00EC",
    ["iacute"] = "\u00ED",
    ["icirc"] = "\u00EE",
    ["iuml"] = "\u00EF",
    ["eth"] = "\u00F0",
    ["ntilde"] = "\u00F1",
    ["ograve"] = "\u00F2",
    ["oacute"] = "\u00F3",
    ["ocirc"] = "\u00F4",
    ["otilde"] = "\u00F5",
    ["ouml"] = "\u00F6",
    ["divide"] = "\u00F7",
    ["oslash"] = "\u00F8",
    ["ugrave"] = "\u00F9",
    ["uacute"] = "\u00FA",
    ["ucirc"] = "\u00FB",
    ["uuml"] = "\u00FC",
    ["yacute"] = "\u00FD",
    ["thorn"] = "\u00FE",
    ["yuml"] = "\u00FF",
    ["fnof"] = "\u0192",
    ["Alpha"] = "\u0391",
    ["Beta"] = "\u0392",
    ["Gamma"] = "\u0393",
    ["Delta"] = "\u0394",
    ["Epsilon"] = "\u0395",
    ["Zeta"] = "\u0396",
    ["Eta"] = "\u0397",
    ["Theta"] = "\u0398",
    ["Iota"] = "\u0399",
    ["Kappa"] = "\u039A",
    ["Lambda"] = "\u039B",
    ["Mu"] = "\u039C",
    ["Nu"] = "\u039D",
    ["Xi"] = "\u039E",
    ["Omicron"] = "\u039F",
    ["Pi"] = "\u03A0",
    ["Rho"] = "\u03A1",
    ["Sigma"] = "\u03A3",
    ["Tau"] = "\u03A4",
    ["Upsilon"] = "\u03A5",
    ["Phi"] = "\u03A6",
    ["Chi"] = "\u03A7",
    ["Psi"] = "\u03A8",
    ["Omega"] = "\u03A9",
    ["alpha"] = "\u03B1",
    ["beta"] = "\u03B2",
    ["gamma"] = "\u03B3",
    ["delta"] = "\u03B4",
    ["epsilon"] = "\u03B5",
    ["zeta"] = "\u03B6",
    ["eta"] = "\u03B7",
    ["theta"] = "\u03B8",
    ["iota"] = "\u03B9",
    ["kappa"] = "\u03BA",
    ["lambda"] = "\u03BB",
    ["mu"] = "\u03BC",
    ["nu"] = "\u03BD",
    ["xi"] = "\u03BE",
    ["omicron"] = "\u03BF",
    ["pi"] = "\u03C0",
    ["rho"] = "\u03C1",
    ["sigmaf"] = "\u03C2",
    ["sigma"] = "\u03C3",
    ["tau"] = "\u03C4",
    ["upsilon"] = "\u03C5",
    ["phi"] = "\u03C6",
    ["chi"] = "\u03C7",
    ["psi"] = "\u03C8",
    ["omega"] = "\u03C9",
    ["thetasym"] = "\u03D1",
    ["upsih"] = "\u03D2",
    ["piv"] = "\u03D6",
    ["bull"] = "\u2022",
    ["hellip"] = "\u2026",
    ["prime"] = "\u2032",
    ["Prime"] = "\u2033",
    ["oline"] = "\u203E",
    ["frasl"] = "\u2044",
    ["weierp"] = "\u2118",
    ["image"] = "\u2111",
    ["real"] = "\u211C",
    ["trade"] = "\u2122",
    ["alefsym"] = "\u2135",
    ["larr"] = "\u2190",
    ["uarr"] = "\u2191",
    ["rarr"] = "\u2192",
    ["darr"] = "\u2193",
    ["harr"] = "\u2194",
    ["crarr"] = "\u21B5",
    ["lArr"] = "\u21D0",
    ["uArr"] = "\u21D1",
    ["rArr"] = "\u21D2",
    ["dArr"] = "\u21D3",
    ["hArr"] = "\u21D4",
    ["forall"] = "\u2200",
    ["part"] = "\u2202",
    ["exist"] = "\u2203",
    ["empty"] = "\u2205",
    ["nabla"] = "\u2207",
    ["isin"] = "\u2208",
    ["notin"] = "\u2209",
    ["ni"] = "\u220B",
    ["prod"] = "\u220F",
    ["sum"] = "\u2211",
    ["minus"] = "\u2212",
    ["lowast"] = "\u2217",
    ["radic"] = "\u221A",
    ["prop"] = "\u221D",
    ["infin"] = "\u221E",
    ["ang"] = "\u2220",
    ["and"] = "\u2227",
    ["or"] = "\u2228",
    ["cap"] = "\u2229",
    ["cup"] = "\u222A",
    ["int"] = "\u222B",
    ["there4"] = "\u2234",
    ["sim"] = "\u223C",
    ["cong"] = "\u2245",
    ["asymp"] = "\u2248",
    ["ne"] = "\u2260",
    ["equiv"] = "\u2261",
    ["le"] = "\u2264",
    ["ge"] = "\u2265",
    ["sub"] = "\u2282",
    ["sup"] = "\u2283",
    ["nsub"] = "\u2284",
    ["sube"] = "\u2286",
    ["supe"] = "\u2287",
    ["oplus"] = "\u2295",
    ["otimes"] = "\u2297",
    ["perp"] = "\u22A5",
    ["sdot"] = "\u22C5",
    ["lceil"] = "\u2308",
    ["rceil"] = "\u2309",
    ["lfloor"] = "\u230A",
    ["rfloor"] = "\u230B",
    ["lang"] = "\u2329",
    ["rang"] = "\u232A",
    ["loz"] = "\u25CA",
    ["spades"] = "\u2660",
    ["clubs"] = "\u2663",
    ["hearts"] = "\u2665",
    ["diams"] = "\u2666",
    ["quot"] = "\u0022",
    ["amp"] = "\u0026",
    ["lt"] = "\u003C",
    ["gt"] = "\u003E",
    ["OElig"] = "\u0152",
    ["oelig"] = "\u0153",
    ["Scaron"] = "\u0160",
    ["scaron"] = "\u0161",
    ["Yuml"] = "\u0178",
    ["circ"] = "\u02C6",
    ["tilde"] = "\u02DC",
    ["ensp"] = "\u2002",
    ["emsp"] = "\u2003",
    ["thinsp"] = "\u2009",
    ["zwnj"] = "\u200C",
    ["zwj"] = "\u200D",
    ["lrm"] = "\u200E",
    ["rlm"] = "\u200F",
    ["ndash"] = "\u2013",
    ["mdash"] = "\u2014",
    ["lsquo"] = "\u2018",
    ["rsquo"] = "\u2019",
    ["sbquo"] = "\u201A",
    ["ldquo"] = "\u201C",
    ["rdquo"] = "\u201D",
    ["bdquo"] = "\u201E",
    ["dagger"] = "\u2020",
    ["Dagger"] = "\u2021",
    ["permil"] = "\u2030",
    ["lsaquo"] = "\u2039",
    ["rsaquo"] = "\u203A",
    ["euro"] = "\u20AC"
}; }

// HTMLAutoClose is the set of HTML elements that
// should be considered to close automatically.
//
// See the [Decoder.Strict] and [Decoder.Entity] fields' documentation.
public static readonly ж<slice<@string>> ᏑHTMLAutoClose = new StaticBox<slice<@string>>(default(slice<@string>));
public static ref slice<@string> HTMLAutoClose => ref ((StaticBox<slice<@string>>)ᏑHTMLAutoClose).ValueSlot;
internal static void initᴛHTMLAutoClose() { HTMLAutoClose = htmlAutoClose; }

internal static slice<@string> htmlAutoClose = slice<@string>([
    /*
		hget http://www.w3.org/TR/html4/loose.dtd |
		9 sed -n 's/<!ELEMENT ([^ ]*) +- O EMPTY.+/	"\1",/p' | tr A-Z a-z
	*/
    "basefont",
    "br",
    "area",
    "link",
    "img",
    "param",
    "hr",
    "input",
    "col",
    "frame",
    "isindex",
    "base",
    "meta"
]);

internal static slice<byte> escQuot = slice<byte>("&#34;"u8); // shorter than "&quot;"
internal static slice<byte> escApos = slice<byte>("&#39;"u8); // shorter than "&apos;"
internal static slice<byte> escAmp = slice<byte>("&amp;"u8);
internal static slice<byte> escLT = slice<byte>("&lt;"u8);
internal static slice<byte> escGT = slice<byte>("&gt;"u8);
internal static slice<byte> escTab = slice<byte>("&#x9;"u8);
internal static slice<byte> escNL = slice<byte>("&#xA;"u8);
internal static slice<byte> escCR = slice<byte>("&#xD;"u8);
internal static slice<byte> escFFFD = slice<byte>("\uFFFD"u8); // Unicode replacement character

// EscapeText writes to w the properly escaped XML equivalent
// of the plain text data s.
public static error EscapeText(io.Writer w, slice<byte> s) {
    return escapeText(w, s, true);
}

// escapeText writes to w the properly escaped XML equivalent
// of the plain text data s. If escapeNewline is true, newline
// characters will be escaped.
internal static error escapeText(io.Writer w, slice<byte> s, bool escapeNewline) {
    slice<byte> esc = default;
    nint last = 0;
    for (nint i = 0; i < len(s); ) {
        var (r, width) = utf8.DecodeRune(s.slice(i));
        i += width;
        switch (r) {
        case '"': {
            esc = escQuot;
            break;
        }
        case '\'': {
            esc = escApos;
            break;
        }
        case '&': {
            esc = escAmp;
            break;
        }
        case '<': {
            esc = escLT;
            break;
        }
        case '>': {
            esc = escGT;
            break;
        }
        case '\t': {
            esc = escTab;
            break;
        }
        case '\n': {
            if (!escapeNewline) {
                continue;
            }
            esc = escNL;
            break;
        }
        case '\r': {
            esc = escCR;
            break;
        }
        default: {
            if (!isInCharacterRange(r) || (r == 0xFFFD && width == 1)) {
                esc = escFFFD;
                break;
            }
            continue;
            break;
        }}
        var (_, errΔ1) = w.Write(s.slice(last, i - width)); if (errΔ1 != nil) {
            return errΔ1;
        }
        var (_, errΔ2) = w.Write(esc); if (errΔ2 != nil) {
            return errΔ2;
        }
        last = i;
    }
    var (_, err) = w.Write(s.slice(last));
    return err;
}

// EscapeString writes to p the properly escaped XML equivalent
// of the plain text data s.
internal static void EscapeString(this ref printer p, @string s) {
    slice<byte> esc = default;
    nint last = 0;
    for (nint i = 0; i < len(s); ) {
        var (r, width) = utf8.DecodeRuneInString(s.slice(i));
        i += width;
        switch (r) {
        case '"': {
            esc = escQuot;
            break;
        }
        case '\'': {
            esc = escApos;
            break;
        }
        case '&': {
            esc = escAmp;
            break;
        }
        case '<': {
            esc = escLT;
            break;
        }
        case '>': {
            esc = escGT;
            break;
        }
        case '\t': {
            esc = escTab;
            break;
        }
        case '\n': {
            esc = escNL;
            break;
        }
        case '\r': {
            esc = escCR;
            break;
        }
        default: {
            if (!isInCharacterRange(r) || (r == 0xFFFD && width == 1)) {
                esc = escFFFD;
                break;
            }
            continue;
            break;
        }}
        p.WriteString(s.slice(last, i - width));
        p.Write(esc);
        last = i;
    }
    p.WriteString(s.slice(last));
}

// Escape is like [EscapeText] but omits the error return value.
// It is provided for backwards compatibility with Go 1.0.
// Code targeting Go 1.1 or later should use [EscapeText].
public static void Escape(io.Writer w, slice<byte> s) {
    EscapeText(w, s);
}

internal static slice<byte> cdataStart = slice<byte>("<![CDATA["u8);
internal static slice<byte> cdataEnd = slice<byte>("]]>"u8);
internal static slice<byte> cdataEscape = slice<byte>("]]]]><![CDATA[>"u8);

// emitCDATA writes to w the CDATA-wrapped plain text data s.
// It escapes CDATA directives nested in s.
internal static error emitCDATA(io.Writer w, slice<byte> s) {
    if (len(s) == 0) {
        return default;
    }
    var (_, errΔ1) = w.Write(cdataStart); if (errΔ1 != nil) {
        return errΔ1;
    }

    for (;;) {
        var (before, after, ok) = bytes.Cut(s, cdataEnd);
        if (!ok) {
            break;
        }
        // Found a nested CDATA directive end.
        var (_, errΔ2) = w.Write(before); if (errΔ2 != nil) {
            return errΔ2;
        }
        var (_, errΔ3) = w.Write(cdataEscape); if (errΔ3 != nil) {
            return errΔ3;
        }
        s = after;
    }

    var (_, errΔ4) = w.Write(s); if (errΔ4 != nil) {
        return errΔ4;
    }

    var (_, err) = w.Write(cdataEnd);
    return err;
}

// procInst parses the `param="..."` or `param='...'`
// value out of the provided string, returning "" if not found.
internal static @string procInst(@string param, @string s) {
    // TODO: this parsing is somewhat lame and not exact.
    // It works for all actual cases, though.
    param = param + "="u8;
    nint lenp = len(param);
    nint i = 0;
    byte sep = default;
    while (i < len(s)) {
        @string sub = s.slice(i);
        nint k = strings.Index(sub, param);
        if (k < 0 || lenp + k >= len(sub)) {
            return ""u8;
        }
        i += lenp + k + 1;
        var c = sub[lenp + k]; if (c == '\'' || c == '"') {
            sep = c;
            break;
        }
    }
    if (sep == 0) {
        return ""u8;
    }
    nint j = strings.IndexByte(s.slice(i), sep);
    if (j < 0) {
        return ""u8;
    }
    return s.slice(i, i + j);
}

} // end xml_package
