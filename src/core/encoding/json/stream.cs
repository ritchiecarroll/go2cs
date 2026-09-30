// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using ΔToken = object;

namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;

partial class json_package {

// A Decoder reads and decodes JSON values from an input stream.
[GoType] partial struct Decoder {
    internal io.Reader r;
    internal slice<byte> buf;
    internal decodeState d;
    internal nint scanp;  // start of unread data in buf
    internal int64 scanned; // amount of data already scanned
    internal scanner scan;
    internal error err;
    internal nint tokenState;
    internal slice<nint> tokenStack;
}

// NewDecoder returns a new decoder that reads from r.
//
// The decoder introduces its own buffering and may
// read data from r beyond the JSON values requested.
public static ж<Decoder> NewDecoder(io.Reader r) {
    return Ꮡ(new Decoder(r: r));
}

// UseNumber causes the Decoder to unmarshal a number into an
// interface value as a [Number] instead of as a float64.
[GoRecv] public static void UseNumber(this ref Decoder dec) {
    dec.d.useNumber = true;
}

// DisallowUnknownFields causes the Decoder to return an error when the destination
// is a struct and the input contains object keys which do not match any
// non-ignored, exported fields in the destination.
[GoRecv] public static void DisallowUnknownFields(this ref Decoder dec) {
    dec.d.disallowUnknownFields = true;
}

// Decode reads the next JSON-encoded value from its
// input and stores it in the value pointed to by v.
//
// See the documentation for [Unmarshal] for details about
// the conversion of JSON into a Go value.
public static error Decode(this ж<Decoder> Ꮡdec, any v) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    if (dec.err != default!) {
        return dec.err;
    }
    {
        var errΔ1 = dec.tokenPrepareForDecode(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    if (!dec.tokenValueAllowed()) {
        return new SyntaxErrorжerror(Ꮡ(new SyntaxError(msg: "not at beginning of value"u8, Offset: dec.InputOffset())));
    }
    // Read whole value into buffer.
    var (n, err) = Ꮡdec.readValue();
    if (err != default!) {
        return err;
    }
    Ꮡdec.of(Decoder.Ꮡd).init(dec.buf.slice(dec.scanp, dec.scanp + n));
    dec.scanp += n;
    // Don't save err from unmarshal into dec.err:
    // the connection is still usable since we read a complete JSON
    // object from it before the error happened.
    err = Ꮡdec.of(Decoder.Ꮡd).unmarshal(v);
    // fixup token streaming state
    dec.tokenValueEnd();
    return err;
}

// Buffered returns a reader of the data remaining in the Decoder's
// buffer. The reader is valid until the next call to [Decoder.Decode].
[GoRecv] public static io.Reader Buffered(this ref Decoder dec) {
    return new bytes_ReaderжReader(bytes.NewReader(dec.buf.slice(dec.scanp)));
}

// readValue reads a JSON value into dec.buf.
// It returns the length of the encoding.
internal static (nint, error) readValue(this ж<Decoder> Ꮡdec) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    dec.scan.reset();
    nint scanp = dec.scanp;
    error err = default!;
Input:
    while (scanp >= 0) {
        // help the compiler see that scanp is never negative, so it can remove
        // some bounds checks below.
        // Look in the buffer for a new value.
        for (; scanp < len(dec.buf); scanp++) {
            var c = dec.buf[scanp];
            dec.scan.bytes++;
            var exprᴛ1 = dec.scan.step(Ꮡdec.of(Decoder.Ꮡscan), c);
            if (exprᴛ1 == scanEnd) {
                dec.scan.bytes--;
                goto break_Input;
            }
            else if (exprᴛ1 == scanEndObject || exprᴛ1 == scanEndArray) {
                if (stateEndValue(Ꮡdec.of(Decoder.Ꮡscan), // scanEnd is delayed one byte so we decrement
 // the scanner bytes count by 1 to ensure that
 // this value is correct in the next call of Decode.
 // scanEnd is delayed one byte.
 // We might block trying to get that byte from src,
 // so instead invent a space byte.
 (rune)' ') == scanEnd) {
                    scanp++;
                    goto break_Input;
                }
            }
            else if (exprᴛ1 == scanError) {
                dec.err = dec.scan.err;
                return (0, dec.scan.err);
            }

        }
        // Did the last read have an error?
        // Delayed until now to allow buffer scan.
        if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                if (dec.scan.step(Ꮡdec.of(Decoder.Ꮡscan), (rune)' ') == scanEnd) {
                    goto break_Input;
                }
                if (nonSpace(dec.buf)) {
                    err = io.ErrUnexpectedEOF;
                }
            }
            dec.err = err;
            return (0, err);
        }
        nint n = scanp - dec.scanp;
        err = dec.refill();
        scanp = dec.scanp + n;
continue_Input:;
    }
break_Input:;
    return (scanp - dec.scanp, default!);
}

[GoRecv] internal static error refill(this ref Decoder dec) {
    // Make room to read more into the buffer.
    // First slide down data already consumed.
    if (dec.scanp > 0) {
        dec.scanned += (int64)dec.scanp;
        nint nΔ1 = copy(dec.buf, dec.buf.slice(dec.scanp));
        dec.buf = dec.buf.slice(0, nΔ1);
        dec.scanp = 0;
    }
    // Grow buffer if not large enough.
    const nint minRead = 512;
    if (cap(dec.buf) - len(dec.buf) < minRead) {
        var newBuf = new slice<byte>(len(dec.buf), 2 * cap(dec.buf) + minRead);
        copy(newBuf, dec.buf);
        dec.buf = newBuf;
    }
    // Read. Delay error for next iteration (after scan).
    var (n, err) = dec.r.Read(dec.buf.slice(len(dec.buf), cap(dec.buf)));
    dec.buf = dec.buf.slice(0, len(dec.buf) + n);
    return err;
}

internal static bool nonSpace(slice<byte> b) {
    foreach (var (_, c) in b) {
        if (!isSpace(c)) {
            return true;
        }
    }
    return false;
}

// An Encoder writes JSON values to an output stream.
[GoType] partial struct Encoder {
    internal io.Writer w;
    internal error err;
    internal bool escapeHTML;
    internal slice<byte> indentBuf;
    internal @string indentPrefix;
    internal @string indentValue;
}

// NewEncoder returns a new encoder that writes to w.
public static ж<Encoder> NewEncoder(io.Writer w) {
    return Ꮡ(new Encoder(w: w, escapeHTML: true));
}

// Encode writes the JSON encoding of v to the stream,
// with insignificant space characters elided,
// followed by a newline character.
//
// See the documentation for [Marshal] for details about the
// conversion of Go values to JSON.
public static error Encode(this ж<Encoder> Ꮡenc, any v) {
    GoFrame ᒐ = default;
    try {
        ref var enc = ref Ꮡenc.DerefOrNull();

        if (enc.err != default!) {
            return enc.err;
        }
        var e = newEncodeState();
        defer(ᏑencodeStatePool.Put, e.OrTypedNil(), ref ᒐ);
        var err = e.marshal(v, new encOpts(escapeHTML: enc.escapeHTML));
        if (err != default!) {
            return err;
        }
        // Terminate each value with a newline.
        // This makes the output look a little nicer
        // when debugging, and some kind of space
        // is required if the encoded value was a number,
        // so that the reader knows there aren't more
        // digits coming.
        e.of(encodeState.ᏑBuffer).WriteByte((rune)'\n');
        var b = e.of(encodeState.ᏑBuffer).Bytes();
        if (enc.indentPrefix != ""u8 || enc.indentValue != ""u8) {
            (enc.indentBuf, err) = appendIndent(enc.indentBuf[..0], b, enc.indentPrefix, enc.indentValue);
            if (err != default!) {
                return err;
            }
            b = enc.indentBuf;
        }
        {
            (_, err) = enc.w.Write(b); if (err != default!) {
                enc.err = err;
            }
        }
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// SetIndent instructs the encoder to format each subsequent encoded
// value as if indented by the package-level function Indent(dst, src, prefix, indent).
// Calling SetIndent("", "") disables indentation.
[GoRecv] public static void SetIndent(this ref Encoder enc, @string prefix, @string indent) {
    enc.indentPrefix = prefix;
    enc.indentValue = indent;
}

// SetEscapeHTML specifies whether problematic HTML characters
// should be escaped inside JSON quoted strings.
// The default behavior is to escape &, <, and > to \u0026, \u003c, and \u003e
// to avoid certain safety problems that can arise when embedding JSON in HTML.
//
// In non-HTML settings where the escaping interferes with the readability
// of the output, SetEscapeHTML(false) disables this behavior.
[GoRecv] public static void SetEscapeHTML(this ref Encoder enc, bool on) {
    enc.escapeHTML = on;
}

[GoType("[]byte")] partial struct RawMessage;

// MarshalJSON returns m as the JSON encoding of m.
public static (slice<byte>, error) MarshalJSON(this RawMessage m) {
    if (m == default!) {
        return (slice<byte>("null"u8), default!);
    }
    return (m, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jsonRawMessageˢ = "json.RawMessage: UnmarshalJSON on nil pointer"u8;

// UnmarshalJSON sets *m to a copy of data.
public static error UnmarshalJSON(this ж<RawMessage> Ꮡm, slice<byte> data) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        return errors.New(jsonRawMessageˢ);
    }
    m = appendꓸꓸꓸ((m)[0..0], data);
    return default!;
}

internal static Marshaler _ᴛ1ʗ = new RawMessageжMarshaler(((ж<RawMessage>)nil));

internal static Unmarshaler _ᴛ2ʗ = new RawMessageжUnmarshaler(((ж<RawMessage>)nil));
// Descriptor carrier for `Token` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("Token")] public interface ΔTokenᴅ { }


internal static UntypedInt tokenTopValue => iota;
internal static UntypedInt tokenArrayStart => 1;
internal static UntypedInt tokenArrayValue => 2;
internal static UntypedInt tokenArrayComma => 3;
internal static UntypedInt tokenObjectStart => 4;
internal static UntypedInt tokenObjectKey => 5;
internal static UntypedInt tokenObjectColon => 6;
internal static UntypedInt tokenObjectValue => 7;
internal static UntypedInt tokenObjectComma => 8;

// advance tokenstate from a separator state to a value state
[GoRecv] internal static error tokenPrepareForDecode(this ref Decoder dec) {
    // Note: Not calling peek before switch, to avoid
    // putting peek into the standard Decode path.
    // peek is only called when using the Token API.
    var exprᴛ1 = dec.tokenState;
    if (exprᴛ1 == tokenArrayComma) {
        var (c, err) = dec.peek();
        if (err != default!) {
            return err;
        }
        if (c != (rune)',') {
            return new SyntaxErrorжerror(Ꮡ(new SyntaxError("expected comma after array element"u8, dec.InputOffset())));
        }
        dec.scanp++;
        dec.tokenState = tokenArrayValue;
    }
    else if (exprᴛ1 == tokenObjectColon) {
        var (c, err) = dec.peek();
        if (err != default!) {
            return err;
        }
        if (c != (rune)':') {
            return new SyntaxErrorжerror(Ꮡ(new SyntaxError("expected colon after object key"u8, dec.InputOffset())));
        }
        dec.scanp++;
        dec.tokenState = tokenObjectValue;
    }

    return default!;
}

[GoRecv] internal static bool tokenValueAllowed(this ref Decoder dec) {
    var exprᴛ1 = dec.tokenState;
    if (exprᴛ1 == tokenTopValue || exprᴛ1 == tokenArrayStart || exprᴛ1 == tokenArrayValue || exprᴛ1 == tokenObjectValue) {
        return true;
    }

    return false;
}

[GoRecv] internal static void tokenValueEnd(this ref Decoder dec) {
    var exprᴛ1 = dec.tokenState;
    if (exprᴛ1 == tokenArrayStart || exprᴛ1 == tokenArrayValue) {
        dec.tokenState = tokenArrayComma;
    }
    else if (exprᴛ1 == tokenObjectValue) {
        dec.tokenState = tokenObjectComma;
    }

}

[GoType("num:rune")] partial struct Delim;

public static @string String(this Delim d) {
    return ((@string)(rune)d);
}

// Token returns the next JSON token in the input stream.
// At the end of the input stream, Token returns nil, [io.EOF].
//
// Token guarantees that the delimiters [ ] { } it returns are
// properly nested and matched: if Token encounters an unexpected
// delimiter in the input, it will return an error.
//
// The input stream consists of basic JSON values—bool, string,
// number, and null—along with delimiters [ ] { } of type [Delim]
// to mark the start and end of arrays and objects.
// Commas and colons are elided.
public static (ΔToken, error) Token(this ж<Decoder> Ꮡdec) {
    ref var dec = ref Ꮡdec.DerefOrNull();

    while (ᐧ) {
        var (c, err) = dec.peek();
        if (err != default!) {
            return (default!, err);
        }
        var exprᴛ1 = c;
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)'[') { matchᴛ1 = true;
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenStack = append(dec.tokenStack, dec.tokenState);
            dec.tokenState = tokenArrayStart;
            return (((Delim)(rune)'['), default!);
        }
        if (exprᴛ1 is (rune)']') { matchᴛ1 = true;
            if (dec.tokenState != tokenArrayStart && dec.tokenState != tokenArrayComma) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = dec.tokenStack[len(dec.tokenStack) - 1];
            dec.tokenStack = dec.tokenStack.slice(0, len(dec.tokenStack) - 1);
            dec.tokenValueEnd();
            return (((Delim)(rune)']'), default!);
        }
        if (exprᴛ1 is (rune)'{') { matchᴛ1 = true;
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenStack = append(dec.tokenStack, dec.tokenState);
            dec.tokenState = tokenObjectStart;
            return (((Delim)(rune)'{'), default!);
        }
        if (exprᴛ1 is (rune)'}') { matchᴛ1 = true;
            if (dec.tokenState != tokenObjectStart && dec.tokenState != tokenObjectComma) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = dec.tokenStack[len(dec.tokenStack) - 1];
            dec.tokenStack = dec.tokenStack.slice(0, len(dec.tokenStack) - 1);
            dec.tokenValueEnd();
            return (((Delim)(rune)'}'), default!);
        }
        if (exprᴛ1 is (rune)':') { matchᴛ1 = true;
            if (dec.tokenState != tokenObjectColon) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = tokenObjectValue;
            continue;
        }
        else if (exprᴛ1 is (rune)',') { matchᴛ1 = true;
            if (dec.tokenState == tokenArrayComma) {
                dec.scanp++;
                dec.tokenState = tokenArrayValue;
                continue;
            }
            if (dec.tokenState == tokenObjectComma) {
                dec.scanp++;
                dec.tokenState = tokenObjectKey;
                continue;
            }
            return dec.tokenError(c);
        }
        else if (exprᴛ1 is (rune)'"') { matchᴛ1 = true;
            if (dec.tokenState == tokenObjectStart || dec.tokenState == tokenObjectKey) {
                ref var x = ref heap(new @string(), out var Ꮡx);
                nint old = dec.tokenState;
                dec.tokenState = tokenTopValue;
                var errΔ3 = Ꮡdec.Decode(Ꮡx);
                dec.tokenState = old;
                if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
                dec.tokenState = tokenObjectColon;
                return (x, default!);
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            ref var x = ref heap<any>(out var Ꮡx);
            {
                var errΔ4 = Ꮡdec.Decode(Ꮡx); if (errΔ4 != default!) {
                    return (default!, errΔ4);
                }
            }
            return (x, default!);
        }

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lookingForBeginningOfˢ3 = " looking for beginning of value"u8;
internal static readonly @string afterArrayElementˢ2 = " after array element"u8;
internal static readonly @string lookingForBeginningOfˢ4 = " looking for beginning of object key string"u8;
internal static readonly @string afterObjectKeyˢ2 = " after object key"u8;
internal static readonly @string afterObjectKeyValuePairˢ2 = " after object key:value pair"u8;

[GoRecv] internal static (ΔToken, error) tokenError(this ref Decoder dec, byte c) {
    @string context = default!;
    var exprᴛ1 = dec.tokenState;
    if (exprᴛ1 == tokenTopValue) {
        context = lookingForBeginningOfˢ3;
    }
    else if (exprᴛ1 == tokenArrayStart || exprᴛ1 == tokenArrayValue || exprᴛ1 == tokenObjectValue) {
        context = lookingForBeginningOfˢ3;
    }
    else if (exprᴛ1 == tokenArrayComma) {
        context = afterArrayElementˢ2;
    }
    else if (exprᴛ1 == tokenObjectKey) {
        context = lookingForBeginningOfˢ4;
    }
    else if (exprᴛ1 == tokenObjectColon) {
        context = afterObjectKeyˢ2;
    }
    else if (exprᴛ1 == tokenObjectComma) {
        context = afterObjectKeyValuePairˢ2;
    }

    return (default!, new SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character "u8 + quoteChar(c) + context, dec.InputOffset()))));
}

// More reports whether there is another element in the
// current array or object being parsed.
[GoRecv] public static bool More(this ref Decoder dec) {
    var (c, err) = dec.peek();
    return err == default! && c != (rune)']' && c != (rune)'}';
}

[GoRecv] internal static (byte, error) peek(this ref Decoder dec) {
    error err = default!;
    while (ᐧ) {
        for (nint i = dec.scanp; i < len(dec.buf); i++) {
            var c = dec.buf[i];
            if (isSpace(c)) {
                continue;
            }
            dec.scanp = i;
            return (c, default!);
        }
        // buffer has been scanned, now report any error
        if (err != default!) {
            return (0, err);
        }
        err = dec.refill();
    }
}

// InputOffset returns the input stream byte offset of the current decoder position.
// The offset gives the location of the end of the most recently returned token
// and the beginning of the next token.
[GoRecv] public static int64 InputOffset(this ref Decoder dec) {
    return dec.scanned + (int64)dec.scanp;
}

} // end json_package
