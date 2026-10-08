// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using binary = encoding.binary_package;
using encoding;

partial class http_package {

// The algorithm uses at most sniffLen bytes to make its decision.
internal static UntypedInt sniffLen => 512;

// DetectContentType implements the algorithm described
// at https://mimesniff.spec.whatwg.org/ to determine the
// Content-Type of the given data. It considers at most the
// first 512 bytes of data. DetectContentType always returns
// a valid MIME type: if it cannot determine a more specific one, it
// returns "application/octet-stream".
public static @string DetectContentType(slice<byte> data) {
    if (builtin.len(data) > sniffLen) {
        data = data[..(int)(sniffLen)];
    }
    // Index of the first non-whitespace byte in data.
    nint firstNonWS = 0;
    for (; firstNonWS < builtin.len(data) && isWS(data[firstNonWS]); firstNonWS++) {
    }
    foreach (var (_, sig) in sniffSignatures) {
        {
            @string ct = sig.match(data, firstNonWS); if (ct != ""u8) {
                return ct;
            }
        }
    }
    return applicationOctetStreamˢ; // fallback
}

// isWS reports whether the provided byte is a whitespace byte (0xWS)
// as defined in https://mimesniff.spec.whatwg.org/#terminology.
internal static bool isWS(byte b) {
    switch (b) {
    case (rune)'\t' or (rune)'\n' or (rune)'\x0c' or (rune)'\r' or (rune)' ': {
        return true;
    }}

    return false;
}

// isTT reports whether the provided byte is a tag-terminating byte (0xTT)
// as defined in https://mimesniff.spec.whatwg.org/#terminology.
internal static bool isTT(byte b) {
    switch (b) {
    case (rune)' ' or (rune)'>': {
        return true;
    }}

    return false;
}

partial interface sniffSig {
    // match returns the MIME type of the data, or "" if unknown.
    @string match(slice<byte> data, nint firstNonWS);
}

// UTF BOMs.
// Image types
// For posterity, we originally returned "image/vnd.microsoft.icon" from
// https://tools.ietf.org/html/draft-ietf-websec-mime-sniff-03#section-7
// https://codereview.appspot.com/4746042
// but that has since been replaced with "image/x-icon" in Section 6.2
// of https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern
// Audio and Video types
// Enforce the pattern match ordering as prescribed in
// https://mimesniff.spec.whatwg.org/#matching-an-audio-or-video-type-pattern
// 6.2.0.2. video/mp4
// 6.2.0.3. video/webm
// Font types
// 34 NULL bytes followed by the string "LP"
// 34 NULL bytes followed by \xF\xF
// Archive types
// RAR's signatures are incorrectly defined by the MIME spec as per
//    https://github.com/whatwg/mimesniff/issues/63
// However, RAR Labs correctly defines it at:
//    https://www.rarlab.com/technote.htm#rarsign
// so we use the definition from RAR Labs.
// TODO: do whatever the spec ends up doing.
// RAR v1.5-v4.0
// RAR v5+
// should be last
// Data matching the table in section 6.
internal static slice<sniffSig> sniffSignatures = new sniffSig[]{((htmlSig)slice<byte>((@string)"<!DOCTYPE HTML"u8)), ((htmlSig)slice<byte>((@string)"<HTML"u8)), ((htmlSig)slice<byte>((@string)"<HEAD"u8)), ((htmlSig)slice<byte>((@string)"<SCRIPT"u8)), ((htmlSig)slice<byte>((@string)"<IFRAME"u8)), ((htmlSig)slice<byte>((@string)"<H1"u8)), ((htmlSig)slice<byte>((@string)"<DIV"u8)), ((htmlSig)slice<byte>((@string)"<FONT"u8)), ((htmlSig)slice<byte>((@string)"<TABLE"u8)), ((htmlSig)slice<byte>((@string)"<A"u8)), ((htmlSig)slice<byte>((@string)"<STYLE"u8)), ((htmlSig)slice<byte>((@string)"<TITLE"u8)), ((htmlSig)slice<byte>((@string)"<B"u8)), ((htmlSig)slice<byte>((@string)"<BODY"u8)), ((htmlSig)slice<byte>((@string)"<BR"u8)), ((htmlSig)slice<byte>((@string)"<P"u8)), ((htmlSig)slice<byte>((@string)"<!--"u8)), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>("<?xml"u8),
    skipWS: true,
    ct: "text/xml; charset=utf-8"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("%PDF-"u8), "application/pdf"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("%!PS-Adobe-"u8), "application/postscript"u8))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0x00, 0x00}))),
    pat: slice<byte>(((@string)(new byte[]{0xfe, 0xff, 0x00, 0x00}))),
    ct: "text/plain; charset=utf-16be"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0x00, 0x00}))),
    pat: slice<byte>(((@string)(new byte[]{0xff, 0xfe, 0x00, 0x00}))),
    ct: "text/plain; charset=utf-16le"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0x00}))),
    pat: slice<byte>(((@string)(new byte[]{0xef, 0xbb, 0xbf, 0x00}))),
    ct: "text/plain; charset=utf-8"u8
))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("\x00\x00\x01\x00"u8), "image/x-icon"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("\x00\x00\x02\x00"u8), "image/x-icon"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("BM"u8), "image/bmp"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("GIF87a"u8), "image/gif"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("GIF89a"u8), "image/gif"u8))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>("RIFF\x00\x00\x00\x00WEBPVP"u8),
    ct: "image/webp"u8
))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>(((@string)(new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a}))), "image/png"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>(((@string)(new byte[]{0xff, 0xd8, 0xff}))), "image/jpeg"u8))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>(((@string)(new byte[]{0x46, 0x4f, 0x52, 0x4d, 0x00, 0x00, 0x00, 0x00, 0x41, 0x49, 0x46, 0x46}))),
    ct: "audio/aiff"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff}))),
    pat: slice<byte>("ID3"u8),
    ct: "audio/mpeg"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>("OggS\x00"u8),
    ct: "application/ogg"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>("MThd\x00\x00\x00\x06"u8),
    ct: "audio/midi"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>(((@string)(new byte[]{0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x41, 0x56, 0x49, 0x20}))),
    ct: "video/avi"u8
))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    mask: slice<byte>(((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff}))),
    pat: slice<byte>("RIFF\x00\x00\x00\x00WAVE"u8),
    ct: "audio/wave"u8
))), new mp4Sig(nil), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>(((@string)(new byte[]{0x1a, 0x45, 0xdf, 0xa3}))), "video/webm"u8))), new maskedSigжsniffSig(Ꮡ(new maskedSig(
    pat: slice<byte>("\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00LP"u8),
    mask: slice<byte>(((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff}))),
    ct: "application/vnd.ms-fontobject"u8
))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("\x00\x01\x00\x00"u8), "font/ttf"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("OTTO"u8), "font/otf"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("ttcf"u8), "font/collection"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("wOFF"u8), "font/woff"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("wOF2"u8), "font/woff2"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>(((@string)(new byte[]{0x1f, 0x8b, 0x08}))), "application/x-gzip"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("PK\x03\x04"u8), "application/zip"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("Rar!\x1A\x07\x00"u8), "application/x-rar-compressed"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("Rar!\x1A\x07\x01\x00"u8), "application/x-rar-compressed"u8))), new exactSigжsniffSig(Ꮡ(new exactSig(slice<byte>("\x00\x61\x73\x6D"u8), "application/wasm"u8))), new textSig(nil)
}.slice();

partial struct exactSig {
    internal slice<byte> sig;
    internal @string ct;
}

internal static @string match(this ref exactSig e, slice<byte> data, nint firstNonWS) {
    if (bytes.HasPrefix(data, e.sig)) {
        return e.ct;
    }
    return ""u8;
}

partial struct maskedSig {
    internal slice<byte> mask, pat;
    internal bool skipWS;
    internal @string ct;
}

internal static @string match(this ref maskedSig m, slice<byte> data, nint firstNonWS) {
    // pattern matching algorithm section 6
    // https://mimesniff.spec.whatwg.org/#pattern-matching-algorithm
    if (m.skipWS) {
        data = data.slice(firstNonWS);
    }
    if (builtin.len(m.pat) != builtin.len(m.mask)) {
        return ""u8;
    }
    if (builtin.len(data) < builtin.len(m.pat)) {
        return ""u8;
    }
    foreach (var (i, pb) in m.pat) {
        var maskedData = (byte)(data[i] & m.mask[i]);
        if (maskedData != pb) {
            return ""u8;
        }
    }
    return m.ct;
}

partial struct htmlSig /*[]byte*/;

internal static @string match(this htmlSig h, slice<byte> data, nint firstNonWS) {
    data = data.slice(firstNonWS);
    if (builtin.len(data) < builtin.len(h) + 1) {
        return ""u8;
    }
    foreach (var (i, b) in h) {
        var db = data[i];
        if ((rune)'A' <= b && b <= (rune)'Z') {
            db &= (byte)(0xDF);
        }
        if (b != db) {
            return ""u8;
        }
    }
    // Next byte must be a tag-terminating byte(0xTT).
    if (!isTT(data[builtin.len(h)])) {
        return ""u8;
    }
    return textHtmlCharsetUtf8ˢ;
}

internal static slice<byte> mp4ftype = slice<byte>("ftyp"u8);

internal static slice<byte> mp4 = slice<byte>("mp4"u8);

partial struct mp4Sig {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string videoMp4ˢ = "video/mp4"u8;

internal static @string match(this mp4Sig _, slice<byte> data, nint firstNonWS) {
    // https://mimesniff.spec.whatwg.org/#signature-for-mp4
    // c.f. section 6.2.1
    if (builtin.len(data) < 12) {
        return ""u8;
    }
    nint boxSize = (nint)binary.BigEndian.Uint32(data[..4]);
    if (builtin.len(data) < boxSize || boxSize % 4 != 0) {
        return ""u8;
    }
    if (!bytes.Equal(data[4..8], mp4ftype)) {
        return ""u8;
    }
    for (nint st = 8; st < boxSize; st += 4) {
        if (st == 12) {
            // Ignores the four bytes that correspond to the version number of the "major brand".
            continue;
        }
        if (bytes.Equal(data.slice(st, st + 3), mp4)) {
            return videoMp4ˢ;
        }
    }
    return ""u8;
}

partial struct textSig {
}

internal static @string match(this textSig _, slice<byte> data, nint firstNonWS) {
    // c.f. section 5, step 4.
    foreach (var (_, b) in data.slice(firstNonWS)) {
        switch (ᐧ) {
        case {} when (b <= 0x08) || (b == 0x0B) || (0x0E <= b && b <= 0x1A) || (0x1C <= b && b <= 0x1F): {
            return ""u8;
        }}

    }
    return textPlainCharsetUtf8ˢ;
}

} // end http_package
