// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname

partial class textproto_package {

// TODO: This should be a distinguishable error (ErrMessageTooLarge)
// to allow mime/multipart to detect it.
internal static error errMessageTooLarge = errors.New("message too large"u8);

// A Reader implements convenience methods for reading requests
// or responses from a text protocol network connection.
partial struct Reader {
    public ж<bufio.Reader> R;
    internal ж<dotReader> dot;
    internal slice<byte> buf; // a re-usable buffer for readContinuedLineSlice
}

// NewReader returns a new [Reader] reading from r.
//
// To avoid denial of service attacks, the provided [bufio.Reader]
// should be reading from an [io.LimitReader] or similar Reader to bound
// the size of responses.
public static ж<Reader> NewReader(ж<bufio.Reader> Ꮡr) {
    return Ꮡ(new Reader(R: Ꮡr));
}

// ReadLine reads a single line from r,
// eliding the final \n or \r\n from the returned string.
public static (@string, error) ReadLine(this ref Reader r) {
    var (line, err) = r.readLineSlice(-1);
    return (((@string)line), err);
}

// ReadLineBytes is like [Reader.ReadLine] but returns a []byte instead of a string.
public static (slice<byte>, error) ReadLineBytes(this ref Reader r) {
    var (line, err) = r.readLineSlice(-1);
    if (line != default!) {
        line = bytes.Clone(line);
    }
    return (line, err);
}

// readLineSlice reads a single line from r,
// up to lim bytes long (or unlimited if lim is less than 0),
// eliding the final \r or \r\n from the returned string.
internal static (slice<byte>, error) readLineSlice(this ref Reader r, int64 lim) {
    r.closeDot();
    slice<byte> line = default!;
    while (ᐧ) {
        var (l, more, err) = r.R.ReadLine();
        if (err != default!) {
            return (default!, err);
        }
        if (lim >= 0 && (int64)len(line) + (int64)len(l) > lim) {
            return (default!, errMessageTooLarge);
        }
        // Avoid the copy if the first call produced a full line.
        if (line == default! && !more) {
            return (l, default!);
        }
        line = appendꓸꓸꓸ(line, l);
        if (!more) {
            break;
        }
    }
    return (line, default!);
}

// ReadContinuedLine reads a possibly continued line from r,
// eliding the final trailing ASCII white space.
// Lines after the first are considered continuations if they
// begin with a space or tab character. In the returned data,
// continuation lines are separated from the previous line
// only by a single space: the newline and leading white space
// are removed.
//
// For example, consider this input:
//
//	Line 1
//	  continued...
//	Line 2
//
// The first call to ReadContinuedLine will return "Line 1 continued..."
// and the second will return "Line 2".
//
// Empty lines are never continued.
public static (@string, error) ReadContinuedLine(this ref Reader r) {
    var (line, err) = r.readContinuedLineSlice(-1, noValidation);
    return (((@string)line), err);
}

// trim returns s with leading and trailing spaces and tabs removed.
// It does not assume Unicode or UTF-8.
internal static slice<byte> trim(slice<byte> s) {
    nint i = 0;
    while (i < len(s) && (s[i] == (rune)' ' || s[i] == (rune)'\t')) {
        i++;
    }
    nint n = len(s);
    while (n > i && (s[n - 1] == (rune)' ' || s[n - 1] == (rune)'\t')) {
        n--;
    }
    return s.slice(i, n);
}

// ReadContinuedLineBytes is like [Reader.ReadContinuedLine] but
// returns a []byte instead of a string.
public static (slice<byte>, error) ReadContinuedLineBytes(this ref Reader r) {
    var (line, err) = r.readContinuedLineSlice(-1, noValidation);
    if (line != default!) {
        line = bytes.Clone(line);
    }
    return (line, err);
}

// readContinuedLineSlice reads continued lines from the reader buffer,
// returning a byte slice with all lines. The validateFirstLine function
// is run on the first read line, and if it returns an error then this
// error is returned from readContinuedLineSlice.
// It reads up to lim bytes of data (or unlimited if lim is less than 0).
internal static (slice<byte>, error) readContinuedLineSlice(this ref Reader r, int64 lim, Func<slice<byte>, error> validateFirstLine) {
    if (validateFirstLine == default!) {
        return (default!, fmt.Errorf("missing validateFirstLine func"u8));
    }
    // Read the first line.
    var (line, err) = r.readLineSlice(lim);
    if (err != default!) {
        return (default!, err);
    }
    if (len(line) == 0) {
        // blank line - no continuation
        return (line, default!);
    }
    {
        var errΔ1 = validateFirstLine(line); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    // Optimistically assume that we have started to buffer the next line
    // and it starts with an ASCII letter (the next header key), or a blank
    // line, so we can avoid copying that buffered data around in memory
    // and skipping over non-existent whitespace.
    if (r.R.Buffered() > 1) {
        var (peek, _) = r.R.Peek(2);
        if (len(peek) > 0 && (isASCIILetter(peek[0]) || peek[0] == (rune)'\n') || len(peek) == 2 && peek[0] == (rune)'\r' && peek[1] == (rune)'\n') {
            return (trim(line), default!);
        }
    }
    // ReadByte or the next readLineSlice will flush the read buffer;
    // copy the slice into buf.
    r.buf = appendꓸꓸꓸ(r.buf[..0], trim(line));
    if (lim < 0) {
        lim = math.MaxInt64;
    }
    lim -= (int64)len(r.buf);
    // Read continuation lines.
    while (r.skipSpace() > 0) {
        r.buf = append(r.buf, (byte)((rune)' '));
        if ((int64)len(r.buf) >= lim) {
            return (default!, errMessageTooLarge);
        }
        var (lineΔ1, errΔ2) = r.readLineSlice(lim - (int64)len(r.buf));
        if (errΔ2 != default!) {
            break;
        }
        r.buf = appendꓸꓸꓸ(r.buf, trim(lineΔ1));
    }
    return (r.buf, default!);
}

// skipSpace skips R over all spaces and returns the number of bytes skipped.
internal static nint skipSpace(this ref Reader r) {
    nint n = 0;
    while (ᐧ) {
        var (c, err) = r.R.ReadByte();
        if (err != default!) {
            // Bufio will keep err until next read.
            break;
        }
        if (c != (rune)' ' && c != (rune)'\t') {
            r.R.UnreadByte();
            break;
        }
        n++;
    }
    return n;
}

internal static (nint code, bool continued, @string message, error err) readCodeLine(this ref Reader r, nint expectCode) {
    nint code = default!;
    bool continued = default!;
    @string message = default!;
    error err = default!;

    (var line, err) = r.ReadLine();
    if (err != default!) {
        return (code, continued, message, err);
    }
    return parseCodeLine(line, expectCode);
}

internal static (nint code, bool continued, @string message, error err) parseCodeLine(@string line, nint expectCode) {
    nint code = default!;
    bool continued = default!;
    @string message = default!;
    error err = default!;

    if (len(line) < 4 || line[3] != (rune)' ' && line[3] != (rune)'-') {
        err = ((ProtocolError)("short response: "u8 + line));
        return (code, continued, message, err);
    }
    continued = line[3] == (rune)'-';
    (code, err) = strconv.Atoi(line[0..3]);
    if (err != default! || code < 100) {
        err = ((ProtocolError)("invalid response code: "u8 + line));
        return (code, continued, message, err);
    }
    message = line[4..];
    if (1 <= expectCode && expectCode < 10 && code / 100 != expectCode || 10 <= expectCode && expectCode < 100 && code / 10 != expectCode || 100 <= expectCode && expectCode < 1000 && code != expectCode) {
        err = new ΔErrorжerror(Ꮡ(new ΔError(code, message)));
    }
    return (code, continued, message, err);
}

// ReadCodeLine reads a response code line of the form
//
//	code message
//
// where code is a three-digit status code and the message
// extends to the rest of the line. An example of such a line is:
//
//	220 plan9.bell-labs.com ESMTP
//
// If the prefix of the status does not match the digits in expectCode,
// ReadCodeLine returns with err set to &Error{code, message}.
// For example, if expectCode is 31, an error will be returned if
// the status is not in the range [310,319].
//
// If the response is multi-line, ReadCodeLine returns an error.
//
// An expectCode <= 0 disables the check of the status code.
public static (nint code, @string message, error err) ReadCodeLine(this ref Reader r, nint expectCode) {
    nint code = default!;
    @string message = default!;
    error err = default!;

    (code, var continued, message, err) = r.readCodeLine(expectCode);
    if (err == default! && continued) {
        err = ((ProtocolError)("unexpected multi-line response: "u8 + message));
    }
    return (code, message, err);
}

// ReadResponse reads a multi-line response of the form:
//
//	code-message line 1
//	code-message line 2
//	...
//	code message line n
//
// where code is a three-digit status code. The first line starts with the
// code and a hyphen. The response is terminated by a line that starts
// with the same code followed by a space. Each line in message is
// separated by a newline (\n).
//
// See page 36 of RFC 959 (https://www.ietf.org/rfc/rfc959.txt) for
// details of another form of response accepted:
//
//	code-message line 1
//	message line 2
//	...
//	code message line n
//
// If the prefix of the status does not match the digits in expectCode,
// ReadResponse returns with err set to &Error{code, message}.
// For example, if expectCode is 31, an error will be returned if
// the status is not in the range [310,319].
//
// An expectCode <= 0 disables the check of the status code.
public static (nint code, @string message, error err) ReadResponse(this ref Reader r, nint expectCode) {
    ref var code = ref heap(new nint(), out var Ꮡcode);
    @string message = default!;
    error err = default!;

    (code, var continued, var first, err) = r.readCodeLine(expectCode);
    var multi = continued;
    ref var messageBuilder = ref heap(new strings.Builder(), out var ᏑmessageBuilder);
    ᏑmessageBuilder.WriteString(first);
    while (continued) {
        var (line, errΔ1) = r.ReadLine();
        if (errΔ1 != default!) {
            return (0, "", errΔ1);
        }
        nint code2 = default!;
        @string moreMessage = default!;
        (code2, continued, moreMessage, errΔ1) = parseCodeLine(line, 0);
        if (errΔ1 != default! || code2 != code) {
            ᏑmessageBuilder.WriteByte((rune)'\n');
            ᏑmessageBuilder.WriteString(strings.TrimRight(line, "\r\n"u8));
            continued = true;
            continue;
        }
        ᏑmessageBuilder.WriteByte((rune)'\n');
        ᏑmessageBuilder.WriteString(moreMessage);
    }
    message = messageBuilder.String();
    if (err != default! && multi && message != ""u8) {
        // replace one line error message with all lines (full message)
        err = new ΔErrorжerror(Ꮡ(new ΔError(code, message)));
    }
    return (code, message, err);
}

// DotReader returns a new [Reader] that satisfies Reads using the
// decoded text of a dot-encoded block read from r.
// The returned Reader is only valid until the next call
// to a method on r.
//
// Dot encoding is a common framing used for data blocks
// in text protocols such as SMTP.  The data consists of a sequence
// of lines, each of which ends in "\r\n".  The sequence itself
// ends at a line containing just a dot: ".\r\n".  Lines beginning
// with a dot are escaped with an additional dot to avoid
// looking like the end of the sequence.
//
// The decoded form returned by the Reader's Read method
// rewrites the "\r\n" line endings into the simpler "\n",
// removes leading dot escapes if present, and stops with error [io.EOF]
// after consuming (and discarding) the end-of-sequence line.
public static io.Reader DotReader(this ж<Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.closeDot();
    r.dot = Ꮡ(new dotReader(r: Ꮡr));
    return new dotReaderжReader(r.dot);
}

partial struct dotReader {
    internal ж<Reader> r;
    internal nint state;
}

// Read satisfies reads by decoding dot-encoded data read from d.r.
internal static (nint n, error err) Read(this ж<dotReader> Ꮡd, slice<byte> b) {
    nint n = default!;
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    // Run data through a simple state machine to
    // elide leading dots, rewrite trailing \r\n into \n,
    // and detect ending .\r\n line.
    const nint stateBeginLine = iota; // beginning of line; initial state; must be zero
    
    const nint stateDot = 1; // read . at beginning of line
    
    const nint stateDotCR = 2; // read .\r at beginning of line
    
    const nint stateCR = 3;  // read \r (possibly at end of line)
    
    const nint stateData = 4; // reading data in middle of line
    
    const nint stateEOF = 5; // reached .\r\n end marker line
    var br = d.r.Value.R;
    while (n < len(b) && d.state != stateEOF) {
        byte c = default!;
        (c, err) = br.ReadByte();
        if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            break;
        }
        var exprᴛ1 = d.state;
        if (exprᴛ1 == stateBeginLine) {
            if (c == (rune)'.') {
                d.state = stateDot;
                continue;
            }
            if (c == (rune)'\r') {
                d.state = stateCR;
                continue;
            }
            d.state = stateData;
        }
        else if (exprᴛ1 == stateDot) {
            if (c == (rune)'\r') {
                d.state = stateDotCR;
                continue;
            }
            if (c == (rune)'\n') {
                d.state = stateEOF;
                continue;
            }
            d.state = stateData;
        }
        else if (exprᴛ1 == stateDotCR) {
            if (c == (rune)'\n') {
                d.state = stateEOF;
                continue;
            }
            br.UnreadByte();
            c = (rune)'\r';
            d.state = stateData;
        }
        else if (exprᴛ1 == stateCR) {
            do {
                if (c == (rune)'\n') {
                    // Not part of .\r\n.
                    // Consume leading dot and emit saved \r.
                    d.state = stateBeginLine;
                    break;
                }
                br.UnreadByte();
                c = (rune)'\r';
                d.state = stateData;
            } while (false);
        }
        else if (exprᴛ1 == stateData) {
            if (c == (rune)'\r') {
                // Not part of \r\n. Emit saved \r
                d.state = stateCR;
                continue;
            }
            if (c == (rune)'\n') {
                d.state = stateBeginLine;
            }
        }

        b[n] = c;
        n++;
    }
    if (err == default! && d.state == stateEOF) {
        err = io.EOF;
    }
    if (err != default! && (~d.r).dot == Ꮡd) {
        d.r.Value.dot = default!;
    }
    return (n, err);
}

// closeDot drains the current DotReader if any,
// making sure that it reads until the ending dot line.
internal static void closeDot(this ref Reader r) {
    if (r.dot == nil) {
        return;
    }
    var buf = new slice<byte>(128);
    while (r.dot != nil) {
        // When Read reaches EOF or an error,
        // it will set r.dot == nil.
        r.dot.Read(buf);
    }
}

// ReadDotBytes reads a dot-encoding and returns the decoded data.
//
// See the documentation for the [Reader.DotReader] method for details about dot-encoding.
public static (slice<byte>, error) ReadDotBytes(this ж<Reader> Ꮡr) {
    return io.ReadAll(Ꮡr.DotReader());
}

// ReadDotLines reads a dot-encoding and returns a slice
// containing the decoded lines, with the final \r\n or \n elided from each.
//
// See the documentation for the [Reader.DotReader] method for details about dot-encoding.
public static (slice<@string>, error) ReadDotLines(this ref Reader r) {
    // We could use ReadDotBytes and then Split it,
    // but reading a line at a time avoids needing a
    // large contiguous block of memory and is simpler.
    slice<@string> v = default!;
    error err = default!;
    while (ᐧ) {
        @string line = default!;
        (line, err) = r.ReadLine();
        if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            break;
        }
        // Dot by itself marks end; otherwise cut one dot.
        if (len(line) > 0 && line[0] == (rune)'.') {
            if (len(line) == 1) {
                break;
            }
            line = line[1..];
        }
        v = append(v, line);
    }
    return (v, err);
}

internal static slice<byte> colon = slice<byte>(":"u8);

// ReadMIMEHeader reads a MIME-style header from r.
// The header is a sequence of possibly continued Key: Value lines
// ending in a blank line.
// The returned map m maps [CanonicalMIMEHeaderKey](key) to a
// sequence of values in the same order encountered in the input.
//
// For example, consider this input:
//
//	My-Key: Value 1
//	Long-Key: Even
//	       Longer Value
//	My-Key: Value 2
//
// Given that input, ReadMIMEHeader returns the map:
//
//	map[string][]string{
//		"My-Key": {"Value 1", "Value 2"},
//		"Long-Key": {"Even Longer Value"},
//	}
public static (MIMEHeader, error) ReadMIMEHeader(this ж<Reader> Ꮡr) {
    return readMIMEHeader(Ꮡr, math.MaxInt64, math.MaxInt64);
}

// readMIMEHeader is accessed from mime/multipart.
//go:linkname readMIMEHeader

// readMIMEHeader is a version of ReadMIMEHeader which takes a limit on the header size.
// It is called by the mime/multipart package.
public static (MIMEHeader, error) readMIMEHeader(ж<Reader> Ꮡr, int64 maxMemory, int64 maxHeaders) {
    ref var r = ref Ꮡr.DerefOrNull();

    // Avoid lots of small slice allocations later by allocating one
    // large one ahead of time which we'll cut up into smaller
    // slices. If this isn't big enough later, we allocate small ones.
    slice<@string> strs = default!;
    nint hint = r.upcomingHeaderKeys();
    if (hint > 0) {
        if (hint > 1000) {
            hint = 1000; // set a cap to avoid overallocation
        }
        strs = new slice<@string>(hint);
    }
    var m = new MIMEHeader(hint);
    // Account for 400 bytes of overhead for the MIMEHeader, plus 200 bytes per entry.
    // Benchmarking map creation as of go1.20, a one-entry MIMEHeader is 416 bytes and large
    // MIMEHeaders average about 200 bytes per entry.
    maxMemory -= 400;
    const int64 mapEntryOverhead = 200;
    // The first line cannot start with a leading space.
    {
        var (buf, err) = r.R.Peek(1); if (err == default! && (buf[0] == (rune)' ' || buf[0] == (rune)'\t')) {
            const int64 errorLimit = 80; // arbitrary limit on how much of the line we'll quote
            var (line, errΔ1) = r.readLineSlice(errorLimit);
            if (errΔ1 != default!) {
                return (m, errΔ1);
            }
            return (m, ((ProtocolError)("malformed MIME header initial line: "u8 + ((sstring)line))));
        }
    }
    while (ᐧ) {
        var (kv, err) = r.readContinuedLineSlice(maxMemory, mustHaveFieldNameColon);
        if (len(kv) == 0) {
            return (m, err);
        }
        // Key ends at first colon.
        var (k, v, ok) = bytes.Cut(kv, colon);
        if (!ok) {
            return (m, ((ProtocolError)("malformed MIME header line: "u8 + ((sstring)kv))));
        }
        (var key, ok) = canonicalMIMEHeaderKey(k);
        if (!ok) {
            return (m, ((ProtocolError)("malformed MIME header line: "u8 + ((sstring)kv))));
        }
        foreach (var (_, c) in v) {
            if (!validHeaderValueByte(c)) {
                return (m, ((ProtocolError)("malformed MIME header line: "u8 + ((sstring)kv))));
            }
        }
        maxHeaders--;
        if (maxHeaders < 0) {
            return (default!, errMessageTooLarge);
        }
        // Skip initial spaces in value.
        @string value = ((@string)bytes.TrimLeft(v, " \t"u8));
        var vv = m[key];
        if (vv == default!) {
            maxMemory -= (int64)len(key);
            maxMemory -= mapEntryOverhead;
        }
        maxMemory -= (int64)len(value);
        if (maxMemory < 0) {
            return (m, errMessageTooLarge);
        }
        if (vv == default! && len(strs) > 0){
            // More than likely this will be a single-element key.
            // Most headers aren't multi-valued.
            // Set the capacity on strs[0] to 1, so any future append
            // won't extend the slice into the other strings.
            (vv, strs) = (strs.slice(0, 1, 1), strs[1..]);
            vv[0] = value;
            m[key] = vv;
        } else {
            m[key] = append(vv, value);
        }
        if (err != default!) {
            return (m, err);
        }
    }
}

// noValidation is a no-op validation func for readContinuedLineSlice
// that permits any lines.
internal static error noValidation(slice<byte> _) {
    return default!;
}

// mustHaveFieldNameColon ensures that, per RFC 7230, the
// field-name is on a single line, so the first line must
// contain a colon.
internal static error mustHaveFieldNameColon(slice<byte> line) {
    if (bytes.IndexByte(line, (rune)':') < 0) {
        return ((ProtocolError)fmt.Sprintf("malformed MIME header: missing colon: %q"u8, line));
    }
    return default!;
}

internal static slice<byte> nl = slice<byte>("\n"u8);

// upcomingHeaderKeys returns an approximation of the number of keys
// that will be in this header. If it gets confused, it returns 0.
internal static nint /*n*/ upcomingHeaderKeys(this ref Reader r) {
    nint n = default!;

    // Try to determine the 'hint' size.
    r.R.Peek(1); // force a buffer load if empty
    nint s = r.R.Buffered();
    if (s == 0) {
        return n;
    }
    var (peek, _) = r.R.Peek(s);
    while (len(peek) > 0 && n < 1000) {
        slice<byte> line = default!;
        (line, peek, _) = bytes.Cut(peek, nl);
        if (len(line) == 0 || (len(line) == 1 && line[0] == (rune)'\r')) {
            // Blank line separating headers from the body.
            break;
        }
        if (line[0] == (rune)' ' || line[0] == (rune)'\t') {
            // Folded continuation of the previous line.
            continue;
        }
        n++;
    }
    return n;
}

// CanonicalMIMEHeaderKey returns the canonical format of the
// MIME header key s. The canonicalization converts the first
// letter and any letter following a hyphen to upper case;
// the rest are converted to lowercase. For example, the
// canonical key for "accept-encoding" is "Accept-Encoding".
// MIME header keys are assumed to be ASCII only.
// If s contains a space or invalid header field bytes, it is
// returned without modifications.
public static @string CanonicalMIMEHeaderKey(@string s) {
    // Quick check for canonical encoding.
    var upper = true;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (!validHeaderFieldByte(c)) {
            return s;
        }
        if (upper && (rune)'a' <= c && c <= (rune)'z') {
            (s, _) = canonicalMIMEHeaderKey(slice<byte>(s));
            return s;
        }
        if (!upper && (rune)'A' <= c && c <= (rune)'Z') {
            (s, _) = canonicalMIMEHeaderKey(slice<byte>(s));
            return s;
        }
        upper = c == (rune)'-';
    }
    return s;
}

internal static UntypedInt toLower => /* 'a' - 'A' */ 32;

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ = GoBigConst.Parse("116972063611741436228934278030836105216");

// validHeaderFieldByte reports whether c is a valid byte in a header
// field name. RFC 7230 says:
//
//	header-field   = field-name ":" OWS field-value OWS
//	field-name     = token
//	tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "." /
//	        "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA
//	token = 1*tchar
internal static bool validHeaderFieldByte(byte c) {
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c >= 128, then 1<<c and 1<<(c-64) will both be zero,
    // and this function will return false.
    GoBigConst mask = /* 0 |
	(1<<(10)-1)<<'0' |
	(1<<(26)-1)<<'a' |
	(1<<(26)-1)<<'A' |
	1<<'!' |
	1<<'#' |
	1<<'$' |
	1<<'%' |
	1<<'&' |
	1<<'\'' |
	1<<'*' |
	1<<'+' |
	1<<'-' |
	1<<'.' |
	1<<'^' |
	1<<'_' |
	1<<'`' |
	1<<'|' |
	1<<'~' */
            maskᶜ;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & (288068722172624896UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & (6341068274398134270UL)))) != 0;
}

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maskᶜ1 = GoBigConst.Parse("170141183460469231731687303711589138944");

// validHeaderValueByte reports whether c is a valid byte in a header
// field value. RFC 7230 says:
//
//	field-content  = field-vchar [ 1*( SP / HTAB ) field-vchar ]
//	field-vchar    = VCHAR / obs-text
//	obs-text       = %x80-FF
//
// RFC 5234 says:
//
//	HTAB           =  %x09
//	SP             =  %x20
//	VCHAR          =  %x21-7E
internal static bool validHeaderValueByte(byte c) {
// VCHAR: %x21-7E
// SP: %x20
    // mask is a 128-bit bitmap with 1s for allowed bytes,
    // so that the byte c can be tested with a shift and an and.
    // If c >= 128, then 1<<c and 1<<(c-64) will both be zero.
    // Since this is the obs-text range, we invert the mask to
    // create a bitmap with 1s for disallowed bytes.
    GoBigConst mask = /* 0 |
	(1<<(0x7f-0x21)-1)<<0x21 |
	1<<0x20 |
	1<<0x09 */ // HTAB: %x09
            maskᶜ1;
    return ((uint64)((uint64)((((uint64)1).Lsh((uint64)(c))) & ~(18446744069414584832UL)) | (uint64)((((uint64)1).Lsh((uint64)((byte)(c - 64)))) & ~(9223372036854775807UL)))) == 0;
}

// canonicalMIMEHeaderKey is like CanonicalMIMEHeaderKey but is
// allowed to mutate the provided byte slice before returning the
// string.
//
// For invalid inputs (if a contains spaces or non-token bytes), a
// is unchanged and a string copy is returned.
//
// ok is true if the header key contains only valid characters and spaces.
// ReadMIMEHeader accepts header keys containing spaces, but does not
// canonicalize them.
internal static (@string, bool ok) canonicalMIMEHeaderKey(slice<byte> a) {
    if (len(a) == 0) {
        return ("", false);
    }
    // See if a looks like a header key. If not, return it unchanged.
    var noCanon = false;
    foreach (var (_, c) in a) {
        if (validHeaderFieldByte(c)) {
            continue;
        }
        // Don't canonicalize.
        if (c == (rune)' ') {
            // We accept invalid headers with a space before the
            // colon, but must not canonicalize them.
            // See https://go.dev/issue/34540.
            noCanon = true;
            continue;
        }
        return (((@string)a), false);
    }
    if (noCanon) {
        return (((@string)a), true);
    }
    var upper = true;
    foreach (var (i, vᴛ1) in a) {
        var c = vᴛ1;

        // Canonicalize: first letter upper case
        // and upper case after each dash.
        // (Host, User-Agent, If-Modified-Since).
        // MIME headers are ASCII only, so no Unicode issues.
        if (upper && (rune)'a' <= c && c <= (rune)'z'){
            c -= toLower;
        } else 
        if (!upper && (rune)'A' <= c && c <= (rune)'Z') {
            c += toLower;
        }
        a[i] = c;
        upper = c == (rune)'-'; // for next time
    }
    ᏑcommonHeaderOnce.Do(initCommonHeader);
    // The compiler recognizes m[string(byteSlice)] as a special
    // case, so a copy of a's bytes into a new string does not
    // happen in this map lookup:
    {
        @string v = commonHeader[tmpstring(a)]; if (v != ""u8) {
            return (v, true);
        }
    }
    return (((@string)a), true);
}

// commonHeader interns common header strings.
internal static map<@string, @string> commonHeader;

internal static ж<sync.Once> ᏑcommonHeaderOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once commonHeaderOnce => ref ᏑcommonHeaderOnce.Value;

internal static void initCommonHeader() {
    commonHeader = new map<@string, @string>();
    foreach (var (_, v) in new @string[]{
        "Accept"u8,
        "Accept-Charset"u8,
        "Accept-Encoding"u8,
        "Accept-Language"u8,
        "Accept-Ranges"u8,
        "Cache-Control"u8,
        "Cc"u8,
        "Connection"u8,
        "Content-Id"u8,
        "Content-Language"u8,
        "Content-Length"u8,
        "Content-Transfer-Encoding"u8,
        "Content-Type"u8,
        "Cookie"u8,
        "Date"u8,
        "Dkim-Signature"u8,
        "Etag"u8,
        "Expires"u8,
        "From"u8,
        "Host"u8,
        "If-Modified-Since"u8,
        "If-None-Match"u8,
        "In-Reply-To"u8,
        "Last-Modified"u8,
        "Location"u8,
        "Message-Id"u8,
        "Mime-Version"u8,
        "Pragma"u8,
        "Received"u8,
        "Return-Path"u8,
        "Server"u8,
        "Set-Cookie"u8,
        "Subject"u8,
        "To"u8,
        "User-Agent"u8,
        "Via"u8,
        "X-Forwarded-For"u8,
        "X-Imforwards"u8,
        "X-Powered-By"u8
    }.slice()) {
        commonHeader[v] = v;
    }
}

} // end textproto_package
