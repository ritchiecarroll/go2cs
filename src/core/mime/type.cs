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

internal static ж<Δsync.Map> ᏑmimeTypes = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map mimeTypes => ref ᏑmimeTypes.Value;  // map[string]string; ".Z" => "application/x-compress"
internal static ж<Δsync.Map> ᏑmimeTypesLower = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map mimeTypesLower => ref ᏑmimeTypesLower.Value; // map[string]string; ".z" => "application/x-compress"

// extensions maps from MIME type to list of lowercase file
// extensions: "image/jpeg" => [".jpg", ".jpeg"]
internal static ж<Δsync.Mutex> ᏑextensionsMu = new StandardBox<Δsync.Mutex>(default(Δsync.Mutex));
internal static ref Δsync.Mutex extensionsMu => ref ᏑextensionsMu.Value; // Guards stores (but not loads) on extensions.
internal static ж<Δsync.Map> Ꮡextensions = new StandardBox<Δsync.Map>(default(Δsync.Map));
internal static ref Δsync.Map extensions => ref Ꮡextensions.Value; // map[string][]string; slice values are append-only.

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

internal static map<@string, @string> builtinTypesLower = new map<@string, @string>{
    [".avif"u8] = "image/avif"u8,
    [".css"u8] = "text/css; charset=utf-8"u8,
    [".gif"u8] = "image/gif"u8,
    [".htm"u8] = "text/html; charset=utf-8"u8,
    [".html"u8] = "text/html; charset=utf-8"u8,
    [".jpeg"u8] = "image/jpeg"u8,
    [".jpg"u8] = "image/jpeg"u8,
    [".js"u8] = "text/javascript; charset=utf-8"u8,
    [".json"u8] = "application/json"u8,
    [".mjs"u8] = "text/javascript; charset=utf-8"u8,
    [".pdf"u8] = "application/pdf"u8,
    [".png"u8] = "image/png"u8,
    [".svg"u8] = "image/svg+xml"u8,
    [".wasm"u8] = "application/wasm"u8,
    [".webp"u8] = "image/webp"u8,
    [".xml"u8] = "text/xml; charset=utf-8"u8
};

internal static ж<Δsync.Once> Ꮡonce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once once => ref Ꮡonce.Value;    // guards initMime

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
//
// On Windows, MIME types are extracted from the registry.
//
// Text types have the charset parameter set to "utf-8" by default.
public static @string TypeByExtension(@string ext) {
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
            lower = append(lower, (byte)(c + ((rune)'a' - (rune)'A')));
        } else {
            lower = append(lower, c);
        }
    }
    var (si, _) = ᏑmimeTypesLower.Load(((@string)lower));
    var (s, _) = si._<@string>(ᐧ);
    return s;
}

// ExtensionsByType returns the extensions known to be associated with the MIME
// type typ. The returned extensions will each begin with a leading dot, as in
// ".html". When typ has no associated extensions, ExtensionsByType returns an
// nil slice.
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
