// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mime implements parts of the MIME spec.
namespace go;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;

partial class mime_package {

internal static readonly ж<Δsync.Map> ᏑmimeTypes = new StaticBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map mimeTypes => ref ((StaticBox<Δsync.Map>)ᏑmimeTypes).ValueSlot;  // map[string]string; ".Z" => "application/x-compress"
internal static readonly ж<Δsync.Map> ᏑmimeTypesLower = new StaticBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map mimeTypesLower => ref ((StaticBox<Δsync.Map>)ᏑmimeTypesLower).ValueSlot; // map[string]string; ".z" => "application/x-compress"

// extensions maps from MIME type to list of lowercase file
// extensions: "image/jpeg" => [".jfif", ".jpg", ".jpeg", ".pjp", ".pjpeg"]
internal static readonly ж<Δsync.Mutex> ᏑextensionsMu = new StaticBox<Δsync.Mutex>(default(Δsync.Mutex));
internal static ref Δsync.Mutex extensionsMu => ref ((StaticBox<Δsync.Mutex>)ᏑextensionsMu).ValueSlot; // Guards stores (but not loads) on extensions.
internal static readonly ж<Δsync.Map> Ꮡextensions = new StaticBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map extensions => ref ((StaticBox<Δsync.Map>)Ꮡextensions).ValueSlot; // map[string][]string; slice values are append-only.

// setMimeTypes is used by initMime's non-test path, and by tests.
internal static void setMimeTypes(map<@string, @string> lowerExt, map<@string, @string> mixExt) {
    GoFrame ᒐ = default;
    try {
        ᏑmimeTypes.Clear();
        ᏑmimeTypesLower.Clear();
        Ꮡextensions.Clear();

        foreach (var (k, v) in lowerExt) {
            ᏑmimeTypesLower.Store(k, v);
        }
        foreach (var (k, v) in mixExt) {
            ᏑmimeTypes.Store(k, v);
        }

        ᏑextensionsMu.Lock();
        defer(ᏑextensionsMu.Unlock, ref ᒐ);
        foreach (var (k, v) in lowerExt) {
            var (justType, _, err) = ParseMediaType(v);
            if (err != default!) {
                throw panic(err);
            }
            slice<@string> exts = default!;
            {
                var (ei, ok) = Ꮡextensions.Load(justType); if (ok) {
                    exts = ei._<slice<@string>>();
                }
            }
            Ꮡextensions.Store(justType, append(exts, k));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// A type is listed here if both Firefox and Chrome included them in their own
// lists.  In the case where they contradict they are deconflicted using IANA's
// listed media types https://www.iana.org/assignments/media-types/media-types.xhtml
//
// Chrome's MIME mappings to file extensions are defined at
// https://chromium.googlesource.com/chromium/src.git/+/refs/heads/main/net/base/mime_util.cc
//
// Firefox's MIME types can be found at
// https://github.com/mozilla-firefox/firefox/blob/main/netwerk/mime/nsMimeTypes.h
// and the mappings to file extensions at
// https://github.com/mozilla-firefox/firefox/blob/main/uriloader/exthandler/nsExternalHelperAppService.cpp
[GoLazy] partial class lazyᴛbuiltinTypesLower { internal static map<@string, @string> builtinTypesLower = new map<@string, @string>{
    [".ai"u8] = "application/postscript"u8,
    [".apk"u8] = "application/vnd.android.package-archive"u8,
    [".apng"u8] = "image/apng"u8,
    [".avif"u8] = "image/avif"u8,
    [".bin"u8] = "application/octet-stream"u8,
    [".bmp"u8] = "image/bmp"u8,
    [".com"u8] = "application/octet-stream"u8,
    [".css"u8] = "text/css; charset=utf-8"u8,
    [".csv"u8] = "text/csv; charset=utf-8"u8,
    [".doc"u8] = "application/msword"u8,
    [".docx"u8] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"u8,
    [".ehtml"u8] = "text/html; charset=utf-8"u8,
    [".eml"u8] = "message/rfc822"u8,
    [".eps"u8] = "application/postscript"u8,
    [".exe"u8] = "application/octet-stream"u8,
    [".flac"u8] = "audio/flac"u8,
    [".gif"u8] = "image/gif"u8,
    [".gz"u8] = "application/gzip"u8,
    [".htm"u8] = "text/html; charset=utf-8"u8,
    [".html"u8] = "text/html; charset=utf-8"u8,
    [".ico"u8] = "image/vnd.microsoft.icon"u8,
    [".ics"u8] = "text/calendar; charset=utf-8"u8,
    [".jfif"u8] = "image/jpeg"u8,
    [".jpeg"u8] = "image/jpeg"u8,
    [".jpg"u8] = "image/jpeg"u8,
    [".js"u8] = "text/javascript; charset=utf-8"u8,
    [".json"u8] = "application/json"u8,
    [".m4a"u8] = "audio/mp4"u8,
    [".mjs"u8] = "text/javascript; charset=utf-8"u8,
    [".mp3"u8] = "audio/mpeg"u8,
    [".mp4"u8] = "video/mp4"u8,
    [".oga"u8] = "audio/ogg"u8,
    [".ogg"u8] = "audio/ogg"u8,
    [".ogv"u8] = "video/ogg"u8,
    [".opus"u8] = "audio/ogg"u8,
    [".pdf"u8] = "application/pdf"u8,
    [".pjp"u8] = "image/jpeg"u8,
    [".pjpeg"u8] = "image/jpeg"u8,
    [".png"u8] = "image/png"u8,
    [".ppt"u8] = "application/vnd.ms-powerpoint"u8,
    [".pptx"u8] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"u8,
    [".ps"u8] = "application/postscript"u8,
    [".rdf"u8] = "application/rdf+xml"u8,
    [".rtf"u8] = "application/rtf"u8,
    [".shtml"u8] = "text/html; charset=utf-8"u8,
    [".svg"u8] = "image/svg+xml"u8,
    [".text"u8] = "text/plain; charset=utf-8"u8,
    [".tif"u8] = "image/tiff"u8,
    [".tiff"u8] = "image/tiff"u8,
    [".txt"u8] = "text/plain; charset=utf-8"u8,
    [".vtt"u8] = "text/vtt; charset=utf-8"u8,
    [".wasm"u8] = "application/wasm"u8,
    [".wav"u8] = "audio/wav"u8,
    [".webm"u8] = "audio/webm"u8,
    [".webp"u8] = "image/webp"u8,
    [".xbl"u8] = "text/xml; charset=utf-8"u8,
    [".xbm"u8] = "image/x-xbitmap"u8,
    [".xht"u8] = "application/xhtml+xml"u8,
    [".xhtml"u8] = "application/xhtml+xml"u8,
    [".xls"u8] = "application/vnd.ms-excel"u8,
    [".xlsx"u8] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"u8,
    [".xml"u8] = "text/xml; charset=utf-8"u8,
    [".xsl"u8] = "text/xml; charset=utf-8"u8,
    [".zip"u8] = "application/zip"u8
}; }

internal static readonly ж<Δsync.Once> Ꮡonce = new StaticBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once once => ref ((StaticBox<Δsync.Once>)Ꮡonce).ValueSlot;    // guards initMime

internal static Action testInitMime;
internal static Action osInitMime;

internal static void initMime() {
    {
        var fn = testInitMime; if (fn != default!){
            fn();
        } else {
            setMimeTypes(builtinTypesLower, builtinTypesLower);
            osInitMime();
        }
    }
}

// TypeByExtension returns the MIME type associated with the file extension ext.
// The extension ext should begin with a leading dot, as in ".html".
// When ext has no associated type, TypeByExtension returns "".
//
// Extensions are looked up first case-sensitively, then case-insensitively.
//
// The built-in table is small but on unix it is augmented by the local
// system's MIME-info database or mime.types file(s) if available under one or
// more of these names:
//
//	/usr/local/share/mime/globs2
//	/usr/share/mime/globs2
//	/etc/mime.types
//	/etc/apache2/mime.types
//	/etc/apache/mime.types
//	/etc/httpd/conf/mime.types
//
// On Windows, MIME types are extracted from the registry.
//
// Text types have the charset parameter set to "utf-8" by default.
public static @string TypeByExtension(@string ext) {
    var lowerᴷ = new stackAppend<byte>();

    Ꮡonce.Do(initMime);

    // Case-sensitive lookup.
    {
        var (v, ok) = ᏑmimeTypes.Load(ext); if (ok) {
            return v._<@string>();
        }
    }

    // Case-insensitive lookup.
    // Optimistically assume a short ASCII extension and be
    // allocation-free in that case.
    array<byte> buf = new(10);
    var lower = buf[..0];
    const byte utf8RuneSelf = 0x80; // from utf8 package, but not importing it.
    for (nint i = 0; i < len(ext); i++) {
        var c = ext[i];
        if (c >= utf8RuneSelf) {
            // Slow path.
            var (siΔ1, _) = ᏑmimeTypesLower.Load(strings.ToLower(ext));
            var (sΔ1, _) = siΔ1._<@string>(ᐧ);
            return sΔ1;
        }
        if ((rune)'A' <= c && c <= (rune)'Z'){
            lower = append(lowerᴷ, lower, (byte)(c + ((rune)'a' - (rune)'A')));
        } else {
            lower = append(lower, c);
        }
    }
    var (si, _) = ᏑmimeTypesLower.Load((@string)lower);
    var (s, _) = si._<@string>(ᐧ);
    return s;
}

// ExtensionsByType returns the extensions known to be associated with the MIME
// type typ. The returned extensions will each begin with a leading dot, as in
// ".html". When typ has no associated extensions, ExtensionsByType returns an
// nil slice.
//
// The built-in table is small but on unix it is augmented by the local
// system's MIME-info database or mime.types file(s) if available under one or
// more of these names:
//
//	/usr/local/share/mime/globs2
//	/usr/share/mime/globs2
//	/etc/mime.types
//	/etc/apache2/mime.types
//	/etc/apache/mime.types
//	/etc/httpd/conf/mime.types
//
// On Windows, extensions are extracted from the registry.
public static (slice<@string>, error) ExtensionsByType(@string typ) {
    var (justType, _, err) = ParseMediaType(typ);
    if (err != default!) {
        return (default!, err);
    }

    Ꮡonce.Do(initMime);
    var (s, ok) = Ꮡextensions.Load(justType);
    if (!ok) {
        return (default!, default!);
    }
    var ret = appendꓸꓸꓸ(slice<@string>(default!), s._<slice<@string>>());
    slices.Sort<slice<@string>, @string>(ret);
    return (ret, default!);
}

// AddExtensionType sets the MIME type associated with
// the extension ext to typ. The extension should begin with
// a leading dot, as in ".html".
public static error AddExtensionType(@string ext, @string typ) {
    if (!strings.HasPrefix(ext, "."u8)) {
        return fmt.Errorf("mime: extension %q missing leading dot"u8, ext);
    }
    Ꮡonce.Do(initMime);
    return setExtensionType(ext, typ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textˢ = "text/"u8;
internal static readonly @string charsetˢ = "charset"u8;

internal static error setExtensionType(@string extension, @string mimeType) {
    GoFrame ᒐ = default;
    try {
        var (justType, param, err) = ParseMediaType(mimeType);
        if (err != default!) {
            return err;
        }
        if (strings.HasPrefix(mimeType, textˢ) && param[charsetˢ] == "") {
            param[charsetˢ] = utf8ˢ2;
            mimeType = FormatMediaType(mimeType, param);
        }
        @string extLower = strings.ToLower(extension);

        ᏑmimeTypes.Store(extension, mimeType);
        ᏑmimeTypesLower.Store(extLower, mimeType);

        ᏑextensionsMu.Lock();
        defer(ᏑextensionsMu.Unlock, ref ᒐ);
        slice<@string> exts = default!;
        {
            var (ei, ok) = Ꮡextensions.Load(justType); if (ok) {
                exts = ei._<slice<@string>>();
            }
        }
        foreach (var (_, v) in exts) {
            if (v == extLower) {
                return default!;
            }
        }
        Ꮡextensions.Store(justType, append(exts, extLower));
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end mime_package
