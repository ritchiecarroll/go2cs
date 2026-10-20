// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

using bufio = bufio_package;
using os = os_package;
using strings = strings_package;
using Δio = io_package;

partial class mime_package {

[GoInit] internal static void init() {
    osInitMime = initMimeUnix;
}

// See https://specifications.freedesktop.org/shared-mime-info-spec/shared-mime-info-spec-0.21.html
// for the FreeDesktop Shared MIME-info Database specification.
internal static slice<@string> mimeGlobs = new @string[]{
    "/usr/local/share/mime/globs2"u8,
    "/usr/share/mime/globs2"u8
}.slice();

// Common locations for mime.types files on unix.
internal static slice<@string> typeFiles = new @string[]{
    "/etc/mime.types"u8,
    "/etc/apache2/mime.types"u8,
    "/etc/apache/mime.types"u8,
    "/etc/httpd/conf/mime.types"u8
}.slice();

internal static error loadMimeGlobsFile(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return err;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var scanner = bufio.NewScanner(new os_FileжReader(f));
        while (scanner.Scan()) {
            // Each line should be of format: weight:mimetype:glob[:morefields...]
            var fields = strings.Split(scanner.Text(), ":"u8);
            if (len(fields) < 3 || len(fields[0]) < 1 || len(fields[2]) < 3){
                continue;
            } else 
            if (fields[0][0] == (rune)'#' || fields[2][0] != (rune)'*' || fields[2][1] != (rune)'.') {
                continue;
            }

            @string extension = fields[2][1..];
            if (strings.ContainsAny(extension, "?*["u8)) {
                // Not a bare extension, but a glob. Ignore for now:
                // - we do not have an implementation for this glob
                //   syntax (translation to path/filepath.Match could
                //   be possible)
                // - support for globs with weight ordering would have
                //   performance impact to all lookups to support the
                //   rarely seen glob entries
                // - trying to match glob metacharacters literally is
                //   not useful
                continue;
            }
            {
                var (_, ok) = ᏑmimeTypes.Load(extension); if (ok) {
                    // We've already seen this extension.
                    // The file is in weight order, so we keep
                    // the first entry that we see.
                    continue;
                }
            }

            setExtensionType(extension, fields[1]);
        }
        {
            var errΔ1 = scanner.Err(); if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void loadMimeFile(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var scanner = bufio.NewScanner(new os_FileжReader(f));
        while (scanner.Scan()) {
            var fields = strings.Fields(scanner.Text());
            if (len(fields) <= 1 || fields[0][0] == (rune)'#') {
                continue;
            }
            @string mimeType = fields[0];
            foreach (var (_, ext) in fields[1..]) {
                if (ext[0] == (rune)'#') {
                    break;
                }
                setExtensionType("."u8 + ext, mimeType);
            }
        }
        {
            var errΔ1 = scanner.Err(); if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void initMimeUnix() {
    foreach (var (_, filename) in mimeGlobs) {
        {
            var err = loadMimeGlobsFile(filename); if (err == default!) {
                return; // Stop checking more files if mimetype database is found.
            }
        }
    }

    // Fallback if no system-generated mimetype database exists.
    foreach (var (_, filename) in typeFiles) {
        loadMimeFile(filename);
    }
}

internal static map<@string, @string> initMimeForTests() {
    mimeGlobs = new @string[]{""u8}.slice();
    typeFiles = new @string[]{"testdata/test.types"u8}.slice();
    return new map<@string, @string>{
        [".T1"u8] = "application/test"u8,
        [".t2"u8] = "text/test; charset=utf-8"u8,
        [".png"u8] = "image/png"u8
    };
}

} // end mime_package
