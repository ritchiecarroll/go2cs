// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pem implements the PEM data encoding, which originated in Privacy
// Enhanced Mail. The most common use of PEM encoding today is in TLS keys and
// certificates. See RFC 1421.
namespace go.encoding;

using bytes = bytes_package;
using base64 = go.encoding.base64_package;
using errors = errors_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using go.encoding;

partial class pem_package {

// A Block represents a PEM encoded structure.
//
// The encoded form is:
//
//	-----BEGIN Type-----
//	Headers
//	base64-encoded Bytes
//	-----END Type-----
//
// where [Block.Headers] is a possibly empty sequence of Key: Value lines.
partial struct Block {
    public @string Type;           // The type, taken from the preamble (i.e. "RSA PRIVATE KEY").
    public map<@string, @string> Headers; // Optional headers.
    public slice<byte> Bytes;       // The decoded bytes of the contents. Typically a DER encoded ASN.1 structure.
}

// getLine results the first \r\n or \n delineated line from the given byte
// array. The line does not include trailing whitespace or the trailing new
// line bytes. The remainder of the byte array (also not including the new line
// bytes) is also returned and this will always be smaller than the original
// argument.
internal static (slice<byte> line, slice<byte> rest, nint consumed) getLine(slice<byte> data) {
    nint i = bytes.IndexByte(data, (rune)'\n');
    nint j = default!;
    if (i < 0){
        i = len(data);
        j = i;
    } else {
        j = i + 1;
        if (i > 0 && data[i - 1] == (rune)'\r') {
            i--;
        }
    }
    return (bytes.TrimRight(data.slice(0, i), " \t"u8), data.slice(j), j);
}

// removeSpacesAndTabs returns a copy of its input with all spaces and tabs
// removed, if there were any. Otherwise, the input is returned unchanged.
//
// The base64 decoder already skips newline characters, so we don't need to
// filter them out here.
internal static slice<byte> removeSpacesAndTabs(slice<byte> data) {
    if (!bytes.ContainsAny(data, " \t"u8)) {
        // Fast path; most base64 data within PEM contains newlines, but
        // no spaces nor tabs. Skip the extra alloc and work.
        return data;
    }
    var result = new slice<byte>(len(data));
    nint n = 0;
    foreach (var (_, b) in data) {
        if (b == (rune)' ' || b == (rune)'\t') {
            continue;
        }
        result[n] = b;
        n++;
    }
    return result.slice(0, n);
}

internal static slice<byte> pemStart = slice<byte>("\n-----BEGIN "u8);

internal static slice<byte> pemEnd = slice<byte>("\n-----END "u8);

internal static slice<byte> pemEndOfLine = slice<byte>("-----"u8);

internal static slice<byte> colon = slice<byte>(":"u8);

// Decode will find the next PEM formatted block (certificate, private key
// etc) in the input. It returns that block and the remainder of the input. If
// no PEM data is found, p is nil and the whole of the input is returned in
// rest.
public static (ж<Block> p, slice<byte> rest) Decode(slice<byte> data) {
    ж<Block> p = default!;
    slice<byte> rest = default!;

    // pemStart begins with a newline. However, at the very beginning of
    // the byte array, we'll accept the start string without it.
    rest = data;
    nint endTrailerIndex = 0;
    while (ᐧ) {
        // If we've already tried parsing a block, skip past the END we already
        // saw.
        if (endTrailerIndex < 0 || endTrailerIndex > len(rest)) {
            return (default!, data);
        }
        rest = rest.slice(endTrailerIndex);
        // Find the first END line, and then find the last BEGIN line before
        // the end line. This lets us skip any repeated BEGIN lines that don't
        // have a matching END.
        nint endIndex = bytes.Index(rest, pemEnd);
        if (endIndex < 0) {
            return (default!, data);
        }
        endTrailerIndex = endIndex + len(pemEnd);
        nint beginIndex = bytes.LastIndex(rest.slice(0, endIndex), pemStart[1..]);
        if (beginIndex < 0 || (beginIndex > 0 && rest[beginIndex - 1] != (rune)'\n')) {
            continue;
        }
        rest = rest.slice(beginIndex + len(pemStart) - 1);
        endIndex -= beginIndex + len(pemStart) - 1;
        endTrailerIndex -= beginIndex + len(pemStart) - 1;
        slice<byte> typeLine = default!;
        nint consumed = default!;
        (typeLine, rest, consumed) = getLine(rest);
        endIndex -= consumed;
        endTrailerIndex -= consumed;
        if (!bytes.HasSuffix(typeLine, pemEndOfLine)) {
            continue;
        }
        typeLine = typeLine.slice(0, len(typeLine) - len(pemEndOfLine));
        p = Ꮡ(new Block(
            Headers: new map<@string, @string>(),
            Type: ((@string)typeLine)
        ));
        while (ᐧ) {
            // This loop terminates because getLine's second result is
            // always smaller than its argument.
            if (len(rest) == 0) {
                return (default!, data);
            }
            var (line, next, consumedΔ1) = getLine(rest);
            var (key, val, ok) = bytes.Cut(line, colon);
            if (!ok) {
                break;
            }
            // TODO(agl): need to cope with values that spread across lines.
            key = bytes.TrimSpace(key);
            val = bytes.TrimSpace(val);
            p.Value.Headers[((@string)key)] = ((@string)val);
            rest = next;
            endIndex -= consumedΔ1;
            endTrailerIndex -= consumedΔ1;
        }
        // If there were headers, there must be a newline between the headers
        // and the END line, so endIndex should be >= 0.
        if (len((~p).Headers) > 0 && endIndex < 0) {
            continue;
        }
        // After the "-----" of the ending line, there should be the same type
        // and then a final five dashes.
        var endTrailer = rest.slice(endTrailerIndex);
        nint endTrailerLen = len(typeLine) + len(pemEndOfLine);
        if (len(endTrailer) < endTrailerLen) {
            continue;
        }
        var restOfEndLine = endTrailer.slice(endTrailerLen);
        endTrailer = endTrailer.slice(0, endTrailerLen);
        if (!bytes.HasPrefix(endTrailer, typeLine) || !bytes.HasSuffix(endTrailer, pemEndOfLine)) {
            continue;
        }
        // The line must end with only whitespace.
        {
            var (s, _, _) = getLine(restOfEndLine); if (len(s) != 0) {
                continue;
            }
        }
        p.Value.Bytes = new byte[]{}.slice();
        if (endIndex > 0) {
            var base64Data = removeSpacesAndTabs(rest.slice(0, endIndex));
            p.Value.Bytes = new slice<byte>(base64.StdEncoding.DecodedLen(len(base64Data)));
            var (n, err) = base64.StdEncoding.Decode((~p).Bytes, base64Data);
            if (err != default!) {
                continue;
            }
            p.Value.Bytes = (~p).Bytes.slice(0, n);
        }
        // the -1 is because we might have only matched pemEnd without the
        // leading newline if the PEM block was empty.
        (_, rest, _) = getLine(rest.slice(endIndex + len(pemEnd) - 1));
        return (p, rest);
    }
}

internal static UntypedInt pemLineLength => 64;

partial struct lineBreaker {
    internal array<byte> line = new(pemLineLength);
    internal nint used;
    internal io.Writer @out;
}

internal static slice<byte> nl = new byte[]{(rune)'\n'}.slice();

internal static (nint n, error err) Write(this ref lineBreaker l, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (l.used + len(b) < pemLineLength) {
        copy(l.line.slice(l.used), b);
        l.used += len(b);
        return (len(b), default!);
    }
    (n, err) = l.@out.Write(l.line.slice(0, l.used));
    if (err != default!) {
        return (n, err);
    }
    nint excess = (nint)pemLineLength - l.used;
    l.used = 0;
    (n, err) = l.@out.Write(b.slice(0, excess));
    if (err != default!) {
        return (n, err);
    }
    (n, err) = l.@out.Write(nl);
    if (err != default!) {
        return (n, err);
    }
    return l.Write(b.slice(excess));
}

internal static error /*err*/ Close(this ref lineBreaker l) {
    error err = default!;

    if (l.used > 0) {
        (_, err) = l.@out.Write(l.line.slice(0, l.used));
        if (err != default!) {
            return err;
        }
        (_, err) = l.@out.Write(nl);
    }
    return err;
}

internal static error writeHeader(io.Writer @out, @string k, @string v) {
    var (_, err) = @out.Write(slice<byte>(k + ": " + v + "\n"));
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pemCannotEncodeAHeaderˢ = "pem: cannot encode a header key that contains a colon"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string procTypeᶜ = "Proc-Type"u8;

// Encode writes the PEM encoding of b to out.
public static error Encode(io.Writer @out, ж<Block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Check for invalid block before writing any output.
    foreach (var (k, _) in b.Headers) {
        if (strings.Contains(k, ":"u8)) {
            return errors.New(pemCannotEncodeAHeaderˢ);
        }
    }
    // All errors below are relayed from underlying io.Writer,
    // so it is now safe to write data.
    {
        var (_, errΔ1) = @out.Write(pemStart[1..]); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = @out.Write(slice<byte>(b.Type + "-----\n")); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    if (len(b.Headers) > 0) {
        @string procType = procTypeᶜ;
        var h = new slice<@string>(0, len(b.Headers));
        var hasProcType = false;
        foreach (var (k, _) in b.Headers) {
            if (k == procType) {
                hasProcType = true;
                continue;
            }
            h = append(h, k);
        }
        // The Proc-Type header must be written first.
        // See RFC 1421, section 4.6.1.1
        if (hasProcType) {
            {
                var errΔ3 = writeHeader(@out, procType, b.Headers[procType]); if (errΔ3 != default!) {
                    return errΔ3;
                }
            }
        }
        // For consistency of output, write other headers sorted by key.
        slices.Sort<slice<@string>, @string>(h);
        foreach (var (_, k) in h) {
            {
                var errΔ4 = writeHeader(@out, k, b.Headers[k]); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
        }
        {
            var (_, errΔ5) = @out.Write(nl); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
    }
    ref var breaker = ref heap(new lineBreaker(), out var Ꮡbreaker);
    breaker.@out = @out;
    var b64 = base64.NewEncoder(base64.StdEncoding, new lineBreakerжWriter(Ꮡbreaker));
    {
        var (_, errΔ6) = b64.Write(b.Bytes); if (errΔ6 != default!) {
            return errΔ6;
        }
    }
    b64.Close();
    breaker.Close();
    {
        var (_, errΔ7) = @out.Write(pemEnd[1..]); if (errΔ7 != default!) {
            return errΔ7;
        }
    }
    var (_, err) = @out.Write(slice<byte>(b.Type + "-----\n"));
    return err;
}

// EncodeToMemory returns the PEM encoding of b.
//
// If b has invalid headers and cannot be encoded,
// EncodeToMemory returns nil. If it is important to
// report details about this error case, use [Encode] instead.
public static slice<byte> EncodeToMemory(ж<Block> Ꮡb) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Encode(new bytes_BufferжWriter(Ꮡbuf), Ꮡb); if (err != default!) {
            return default!;
        }
    }
    return buf.Bytes();
}

} // end pem_package
