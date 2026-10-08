// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using rand = crypto.rand_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using maps = maps_package;
using textproto = net.textproto_package;
using slices = slices_package;
using strings = strings_package;
using crypto;
using iter = iter_package;
using net;

partial class multipart_package {

// A Writer generates multipart messages.
partial struct Writer {
    internal io.Writer w;
    internal @string boundary;
    internal ж<part> lastpart;
}

// NewWriter returns a new multipart [Writer] with a random boundary,
// writing to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(
        w: w,
        boundary: randomBoundary()
    ));
}

// Boundary returns the [Writer]'s boundary.
public static @string Boundary(this ref Writer w) {
    return w.boundary;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mimeSetBoundaryCalledˢ = "mime: SetBoundary called after write"u8;
internal static readonly @string mimeInvalidBoundaryˢ = "mime: invalid boundary length"u8;
internal static readonly @string mimeInvalidBoundaryˢ2 = "mime: invalid boundary character"u8;

// SetBoundary overrides the [Writer]'s default randomly-generated
// boundary separator with an explicit value.
//
// SetBoundary must be called before any parts are created, may only
// contain certain ASCII characters, and must be non-empty and
// at most 70 bytes long.
public static error SetBoundary(this ref Writer w, @string boundary) {
    if (w.lastpart != nil) {
        return errors.New(mimeSetBoundaryCalledˢ);
    }
    // rfc2046#section-5.1.1
    if (len(boundary) < 1 || len(boundary) > 70) {
        return errors.New(mimeInvalidBoundaryˢ);
    }
    nint end = len(boundary) - 1;
    foreach (var (i, b) in boundary) {
        if ((rune)'A' <= b && b <= (rune)'Z' || (rune)'a' <= b && b <= (rune)'z' || (rune)'0' <= b && b <= (rune)'9') {
            continue;
        }
        switch (b) {
        case (rune)'\'' or (rune)'(' or (rune)')' or (rune)'+' or (rune)'_' or (rune)',' or (rune)'-' or (rune)'.' or (rune)'/' or (rune)':' or (rune)'=' or (rune)'?': {
            continue;
            break;
        }
        case (rune)' ': {
            if (i != end) {
                continue;
            }
            break;
        }}

        return errors.New(mimeInvalidBoundaryˢ2);
    }
    w.boundary = boundary;
    return default!;
}

// FormDataContentType returns the Content-Type for an HTTP
// multipart/form-data with this [Writer]'s Boundary.
public static @string FormDataContentType(this ref Writer w) {
    @string b = w.boundary;
    // We must quote the boundary if it contains any of the
    // tspecials characters defined by RFC 2045, or space.
    if (strings.ContainsAny(b, @"()<>@,;:\""/[]?= "u8)) {
        b = @""""u8 + b + @""""u8;
    }
    return "multipart/form-data; boundary="u8 + b;
}

internal static @string randomBoundary() {
    array<byte> buf = new(30);
    var (_, err) = io.ReadFull(rand.Reader, buf[..]);
    if (err != default!) {
        throw panic(err);
    }
    return fmt.Sprintf("%x"u8, buf[..]);
}

// CreatePart creates a new multipart section with the provided
// header. The body of the part should be written to the returned
// [Writer]. After calling CreatePart, any previous part may no longer
// be written to.
public static (io.Writer, error) CreatePart(this ж<Writer> Ꮡw, textproto.MIMEHeader header) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (w.lastpart != nil) {
        {
            var errΔ1 = w.lastpart.close(); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    if (w.lastpart != nil){
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "\r\n--%s\r\n"u8, w.boundary);
    } else {
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "--%s\r\n"u8, w.boundary);
    }
    foreach (var (_, k) in slices.Sorted(maps.Keys<textproto.MIMEHeader, @string, slice<@string>>(header))) {
        foreach (var (_, v) in header[k]) {
            fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "%s: %s\r\n"u8, k, v);
        }
    }
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "\r\n"u8);
    var (_, err) = io.Copy(w.w, new bytes_BufferжReader(Ꮡb));
    if (err != default!) {
        return (default!, err);
    }
    var p = Ꮡ(new part(
        mw: Ꮡw
    ));
    w.lastpart = p;
    return (new partжWriter(p), default!);
}

internal static ж<strings.Replacer> quoteEscaper = strings.NewReplacer("\\"u8, "\\\\", @"""", "\\\"");

internal static @string escapeQuotes(@string s) {
    return quoteEscaper.Replace(s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeˢ = "Content-Type"u8;
internal static readonly @string applicationOctetStreamˢ = "application/octet-stream"u8;

// CreateFormFile is a convenience wrapper around [Writer.CreatePart]. It creates
// a new form-data header with the provided field name and file name.
public static (io.Writer, error) CreateFormFile(this ж<Writer> Ꮡw, @string fieldname, @string filename) {
    var h = new textproto.MIMEHeader(0);
    h.Set(contentDispositionˢ,
        fmt.Sprintf(@"form-data; name=""%s""; filename=""%s"""u8,
            escapeQuotes(fieldname), escapeQuotes(filename)));
    h.Set(contentTypeˢ, applicationOctetStreamˢ);
    return Ꮡw.CreatePart(h);
}

// CreateFormField calls [Writer.CreatePart] with a header using the
// given field name.
public static (io.Writer, error) CreateFormField(this ж<Writer> Ꮡw, @string fieldname) {
    var h = new textproto.MIMEHeader(0);
    h.Set(contentDispositionˢ,
        fmt.Sprintf(@"form-data; name=""%s"""u8, escapeQuotes(fieldname)));
    return Ꮡw.CreatePart(h);
}

// WriteField calls [Writer.CreateFormField] and then writes the given value.
public static error WriteField(this ж<Writer> Ꮡw, @string fieldname, @string value) {
    var (p, err) = Ꮡw.CreateFormField(fieldname);
    if (err != default!) {
        return err;
    }
    (_, err) = p.Write(slice<byte>(value));
    return err;
}

// Close finishes the multipart message and writes the trailing
// boundary end line to the output.
public static error Close(this ref Writer w) {
    if (w.lastpart != nil) {
        {
            var errΔ1 = w.lastpart.close(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        w.lastpart = default!;
    }
    var (_, err) = fmt.Fprintf(w.w, "\r\n--%s--\r\n"u8, w.boundary);
    return err;
}

partial struct part {
    internal ж<Writer> mw;
    internal bool closed;
    internal error we; // last error that occurred writing
}

internal static error close(this ref part p) {
    p.closed = true;
    return p.we;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string multipartCanTWriteToˢ = "multipart: can't write to finished part"u8;

internal static (nint n, error err) Write(this ref part p, slice<byte> d) {
    nint n = default!;
    error err = default!;

    if (p.closed) {
        return (0, errors.New(multipartCanTWriteToˢ));
    }
    (n, err) = (~p.mw).w.Write(d);
    if (err != default!) {
        p.we = err;
    }
    return (n, err);
}

} // end multipart_package
