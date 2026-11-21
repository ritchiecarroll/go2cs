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
using static multipart_package;

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

} partial class multipart_package {

// Boundary returns the [Writer]'s boundary.
[GoMethodBody("Boundary")] internal static @string Boundaryᴍ(ref Writer w) {
    return w.boundary;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mimeSetBoundaryCalledˢᴍWriter => ref mimeSetBoundaryCalledˢᵇᴍWriter;
private static readonly @string mimeSetBoundaryCalledˢᵇᴍWriter = "mime: SetBoundary called after write"u8;
internal static ref readonly @string mimeInvalidBoundaryˢᴍWriter => ref mimeInvalidBoundaryˢᵇᴍWriter;
private static readonly @string mimeInvalidBoundaryˢᵇᴍWriter = "mime: invalid boundary length"u8;
internal static ref readonly @string mimeInvalidBoundaryˢ2ᴍWriter => ref mimeInvalidBoundaryˢ2ᵇᴍWriter;
private static readonly @string mimeInvalidBoundaryˢ2ᵇᴍWriter = "mime: invalid boundary character"u8;

// SetBoundary overrides the [Writer]'s default randomly-generated
// boundary separator with an explicit value.
//
// SetBoundary must be called before any parts are created, may only
// contain certain ASCII characters, and must be non-empty and
// at most 70 bytes long.
[GoMethodBody("SetBoundary")] internal static error SetBoundaryᴍ(ref Writer w, @string boundary) {
    if (w.lastpart != nil) {
        return errors.New(mimeSetBoundaryCalledˢᴍWriter);
    }
    // rfc2046#section-5.1.1
    if (len(boundary) < 1 || len(boundary) > 70) {
        return errors.New(mimeInvalidBoundaryˢᴍWriter);
    }
    nint end = len(boundary) - 1;
    foreach (var (i, b) in boundary) {
        if ('A' <= b && b <= 'Z' || 'a' <= b && b <= 'z' || '0' <= b && b <= '9') {
            continue;
        }
        switch (b) {
        case '\'' or '(' or ')' or '+' or '_' or ',' or '-' or '.' or '/' or ':' or '=' or '?': {
            continue;
            break;
        }
        case ' ': {
            if (i != end) {
                continue;
            }
            break;
        }}
        return errors.New(mimeInvalidBoundaryˢ2ᴍWriter);
    }
    w.boundary = boundary;
    return default;
}

// FormDataContentType returns the Content-Type for an HTTP
// multipart/form-data with this [Writer]'s Boundary.
[GoMethodBody("FormDataContentType")] internal static @string FormDataContentTypeᴍ(ref Writer w) {
    @string b = w.boundary;
    // We must quote the boundary if it contains any of the
    // tspecials characters defined by RFC 2045, or space.
    if (strings.ContainsAny(b, @"()<>@,;:\""/[]?= ")) {
        b = @""""u8 + b + @""""u8;
    }
    return "multipart/form-data; boundary="u8 + b;
}

} partial class multipart_package {

internal static @string randomBoundary() {
    array<byte> buf = new(30);
    var (_, err) = io.ReadFull(rand.Reader, buf[..]);
    if (err != nil) {
        throw panic(err);
    }
    return fmt.Sprintf("%x"u8, buf[..]);
}

} partial class multipart_package {

// CreatePart creates a new multipart section with the provided
// header. The body of the part should be written to the returned
// [Writer]. After calling CreatePart, any previous part may no longer
// be written to.
[GoMethodBody("CreatePart")] internal static (io.Writer, error) CreatePartᴍ(ж<Writer> Ꮡw, textproto.MIMEHeader header) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (w.lastpart != nil) {
        var errΔ1 = w.lastpart.close(); if (errΔ1 != nil) {
            return (default, errΔ1);
        }
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    if (w.lastpart != nil) {
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "\r\n--%s\r\n"u8, w.boundary);
    } else {
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "--%s\r\n"u8, w.boundary);
    }

    foreach (var (_, k) in range(slices.Sorted(maps.Keys<textproto.MIMEHeader, @string, slice<@string>>(header)))) {
        foreach (var (_, v) in range(header[k])) {
            fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "%s: %s\r\n"u8, k, v);
        }
    }
    fmt.Fprintf(new bytes_BufferжWriter(Ꮡb), "\r\n"u8);
    var (_, err) = io.Copy(w.w, new bytes_BufferжReader(Ꮡb));
    if (err != nil) {
        return (default, err);
    }
    var p = Ꮡ(new part(
        mw: Ꮡw
    ));
    w.lastpart = p;
    return (new partжWriter(p), default);
}

} partial class multipart_package {

internal static ж<strings.Replacer> quoteEscaper = strings.NewReplacer("\\", "\\\\", @"""", "\\\"", "\r", "%0D", "\n", "%0A");

// escapeQuotes escapes special characters in field parameter values.
//
// For historical reasons, this uses \ escaping for " and \ characters,
// and percent encoding for CR and LF.
//
// The WhatWG specification for form data encoding suggests that we should
// use percent encoding for " (%22), and should not escape \.
// https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#multipart/form-data-encoding-algorithm
//
// Empirically, as of the time this comment was written, it is necessary
// to escape \ characters or else Chrome (and possibly other browsers) will
// interpet the unescaped \ as an escape.
internal static @string escapeQuotes(@string s) {
    return quoteEscaper.Replace(s);
}

} partial class multipart_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentDispositionˢᴍWriter => ref contentDispositionˢᵇᴍWriter;
private static readonly @string contentDispositionˢᵇᴍWriter = "Content-Disposition"u8;
internal static ref readonly @string contentTypeˢᴍWriter => ref contentTypeˢᵇᴍWriter;
private static readonly @string contentTypeˢᵇᴍWriter = "Content-Type"u8;
internal static ref readonly @string applicationOctetStreamˢᴍWriter => ref applicationOctetStreamˢᵇᴍWriter;
private static readonly @string applicationOctetStreamˢᵇᴍWriter = "application/octet-stream"u8;

// CreateFormFile is a convenience wrapper around [Writer.CreatePart]. It creates
// a new form-data header with the provided field name and file name.
[GoMethodBody("CreateFormFile")] internal static (io.Writer, error) CreateFormFileᴍ(ж<Writer> Ꮡw, @string fieldname, @string filename) {
    var h = new textproto.MIMEHeader(0);
    h.Set(contentDispositionˢᴍWriter, FileContentDisposition(fieldname, filename));
    h.Set(contentTypeˢᴍWriter, applicationOctetStreamˢᴍWriter);
    return Ꮡw.CreatePart(h);
}

// CreateFormField calls [Writer.CreatePart] with a header using the
// given field name.
[GoMethodBody("CreateFormField")] internal static (io.Writer, error) CreateFormFieldᴍ(ж<Writer> Ꮡw, @string fieldname) {
    var h = new textproto.MIMEHeader(0);
    h.Set(contentDispositionˢᴍWriter,
        fmt.Sprintf(@"form-data; name=""%s"""u8, escapeQuotes(fieldname)));
    return Ꮡw.CreatePart(h);
}

} partial class multipart_package {

// FileContentDisposition returns the value of a Content-Disposition header
// with the provided field name and file name.
public static @string FileContentDisposition(@string fieldname, @string filename) {
    return fmt.Sprintf(@"form-data; name=""%s""; filename=""%s"""u8,
        escapeQuotes(fieldname), escapeQuotes(filename));
}

} partial class multipart_package {

// WriteField calls [Writer.CreateFormField] and then writes the given value.
[GoMethodBody("WriteField")] internal static error WriteFieldᴍ(ж<Writer> Ꮡw, @string fieldname, @string value) {
    var (p, err) = Ꮡw.CreateFormField(fieldname);
    if (err != nil) {
        return err;
    }
    (_, err) = p.Write(slice<byte>(value));
    return err;
}

// Close finishes the multipart message and writes the trailing
// boundary end line to the output.
[GoMethodBody("Close")] internal static error Closeᴍ(ref Writer w) {
    if (w.lastpart != nil) {
        var errΔ1 = w.lastpart.close(); if (errΔ1 != nil) {
            return errΔ1;
        }
        w.lastpart = default;
    }
    var (_, err) = fmt.Fprintf(w.w, "\r\n--%s--\r\n"u8, w.boundary);
    return err;
}

} partial class multipart_package {

partial struct part {
    internal ж<Writer> mw;
    internal bool closed;
    internal error we; // last error that occurred writing
}

} partial class multipart_package {

[GoMethodBody("close")] internal static error closeᴍ(ref part p) {
    p.closed = true;
    return p.we;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string multipartCanTWriteToˢᴍpart => ref multipartCanTWriteToˢᵇᴍpart;
private static readonly @string multipartCanTWriteToˢᵇᴍpart = "multipart: can't write to finished part"u8;

[GoMethodBody("Write")] internal static (nint n, error err) Writeᴍ(ref part p, slice<byte> d) {
    nint n = default;
    error err = default;

    if (p.closed) {
        return (0, errors.New(multipartCanTWriteToˢᴍpart));
    }
    (n, err) = p.mw.w.Write(d);
    if (err != nil) {
        p.we = err;
    }
    return (n, err);
}

}
