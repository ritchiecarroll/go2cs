// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using asan = @internal.asan_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using @internal;
using static go.mime_package;

partial class mime_internal_test_package {

internal static Action /*cleanup*/ setMimeInit(Action fn) {
    once = new Δsync.Once(nil);
    testInitMime = fn;
    return () => {
        testInitMime = default!;
        once = new Δsync.Once(nil);
    };
}

internal static void clearMimeTypes() {
    setMimeTypes(new map<@string, @string>{}, new map<@string, @string>{});
}

internal static void setType(@string ext, @string typ) {
    if (!strings.HasPrefix(ext, "."u8)) {
        throw panic("missing leading dot");
    }
    {
        var err = setExtensionType(ext, typ); if (err != default!) {
            throw panic("bad test data: " + err.Error());
        }
    }
}

public static void TestTypeByExtension(ж<testing.T> Ꮡt) {
    once = new Δsync.Once(nil);
    // initMimeForTests returns the platform-specific extension =>
    // type tests. On Unix and Plan 9, this also tests the parsing
    // of MIME text files (in testdata/*). On Windows, we test the
    // real registry on the machine and assume that ".png" exists
    // there, which empirically it always has, for all versions of
    // Windows.
    var typeTests = initMimeForTests();

    foreach (var (ext, want) in typeTests) {
        @string val = TypeByExtension(ext);
        if (val != want) {
            Ꮡt.Errorf("TypeByExtension(%q) = %q, want %q"u8, ext, val, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = ".foo"u8;
internal static readonly @string xFooˢ = "x/foo"u8;
internal static readonly @string barˢ = ".bar"u8;
internal static readonly @string xBarˢ = "x/bar"u8;
internal static readonly @string barˢ2 = ".Bar"u8;
internal static readonly @string xBarCapital1ˢ = "x/bar; capital=1"u8;

public static void TestTypeByExtension_LocalData(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            setType(fooˢ, xFooˢ);
            setType(barˢ, xBarˢ);
            setType(barˢ2, xBarCapital1ˢ);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        var tests = new map<@string, @string>{
            [".foo"u8] = "x/foo"u8,
            [".bar"u8] = "x/bar"u8,
            [".Bar"u8] = "x/bar; capital=1"u8,
            [".sdlkfjskdlfj"u8] = ""u8,
            [".t1"u8] = ""u8 // testdata shouldn't be used
        };

        foreach (var (ext, want) in tests) {
            @string val = TypeByExtension(ext);
            if (val != want) {
                Ꮡt.Errorf("TypeByExtension(%q) = %q, want %q"u8, ext, val, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = ".TEST"u8;
internal static readonly @string tesTˢ = ".tesT"u8;
internal static readonly @string tesTˢ2 = ".TesT"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string customᶜ = "test/test; charset=iso-8859-1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string capsᶜ = "test/test; WAS=ALLCAPS"u8;

public static void TestTypeByExtensionCase(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string custom = customᶜ;
        @string caps = capsᶜ;

        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            setType(testˢ, caps);
            setType(tesTˢ, custom);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        // case-sensitive lookup
        {
            @string got = TypeByExtension(tesTˢ); if (got != custom) {
                Ꮡt.Fatalf("for .tesT, got %q; want %q"u8, got, custom);
            }
        }
        {
            @string got = TypeByExtension(testˢ); if (got != caps) {
                Ꮡt.Fatalf("for .TEST, got %q; want %s"u8, got, caps);
            }
        }

        // case-insensitive
        {
            @string got = TypeByExtension(tesTˢ2); if (got != custom) {
                Ꮡt.Fatalf("for .TesT, got %q; want %q"u8, got, custom);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifˢ = ".gif"u8;
internal static readonly @string imageGifˢ = "image/gif"u8;
internal static readonly @string fooLetterˢ = "foo/letter"u8;
internal static readonly @string pngˢ = ".PNG"u8;
internal static readonly @string imagePngˢ = "image/png"u8;

internal partial struct TestExtensionsByType_tests /*dyn*/ {
    internal @string typ;
    internal slice<@string> want;
    internal @string wantErr;
}

public static void TestExtensionsByType(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            setType(gifˢ, imageGifˢ);
            setType(".a"u8, fooLetterˢ);
            setType(".b"u8, fooLetterˢ);
            setType(".B"u8, fooLetterˢ);
            setType(pngˢ, imagePngˢ);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        var tests = new TestExtensionsByType_tests[]{
            new(typ: "image/gif"u8, want: new @string[]{".gif"u8}.slice()),
            new(typ: "image/png"u8, want: new @string[]{".png"u8}.slice()), // lowercase
            new(typ: "foo/letter"u8, want: new @string[]{".a"u8, ".b"u8}.slice()),
            new(typ: "x/unknown"u8, want: default!)
        }.slice();

        foreach (var (_, tt) in tests) {
            var (got, err) = ExtensionsByType(tt.typ);
            if (err != default! && tt.wantErr != ""u8 && strings.Contains(err.Error(), tt.wantErr)) {
                continue;
            }
            if (err != default!) {
                Ꮡt.Errorf("ExtensionsByType(%q) error: %v"u8, tt.typ, err);
                continue;
            }
            if (tt.wantErr != ""u8) {
                Ꮡt.Errorf("ExtensionsByType(%q) = %q, %v; want error substring %q"u8, tt.typ, got, err, tt.wantErr);
                continue;
            }
            if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
                Ꮡt.Errorf("ExtensionsByType(%q) = %q; want %q"u8, tt.typ, got, tt.want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly @string htmlˢ = ".html"u8;
internal static readonly @string htMLˢ = ".HtML"u8;

public static void TestLookupMallocs(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    var n = testing.AllocsPerRun(10000, () => {
        TypeByExtension(htmlˢ);
        TypeByExtension(htMLˢ);
    });
    if (n > 0D) {
        Ꮡt.Errorf("allocs = %v; want 0"u8, n);
    }
}

public static void BenchmarkTypeByExtension(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initMime();
    b.ResetTimer();

    foreach (var (_, ext) in new @string[]{
        ".html"u8,
        ".HTML"u8,
        ".unused"u8
    }.slice()) {
        Ꮡb.Run(ext, (ж<testing.B> bΔ1) => {
            bΔ1.RunParallel((ж<testing.PB> pb) => {
                while (pb.Next()) {
                    TypeByExtension(ext);
                }
            });
        });
    }
}

public static void BenchmarkExtensionsByType(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initMime();
    b.ResetTimer();

    foreach (var (_, typ) in new @string[]{
        "text/html"u8,
        "text/html; charset=utf-8"u8,
        "application/octet-stream"u8
    }.slice()) {
        Ꮡb.Run(typ, (ж<testing.B> bΔ1) => {
            bΔ1.RunParallel((ж<testing.PB> pb) => {
                while (pb.Next()) {
                    {
                        var (_, err) = ExtensionsByType(typ); if (err != default!) {
                            bΔ1.Fatal(err);
                        }
                    }
                }
            });
        });
    }
}

internal partial struct TestExtensionsByType2_tests /*dyn*/ {
    internal @string typ;
    internal slice<@string> want;
}

public static void TestExtensionsByType2(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            // Initialize built-in types like in type.go before osInitMime.
            setMimeTypes(builtinTypesLower, builtinTypesLower);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        var tests = new TestExtensionsByType2_tests[]{
            new(typ: "application/postscript"u8, want: new @string[]{".ai"u8, ".eps"u8, ".ps"u8}.slice()),
            new(typ: "application/vnd.android.package-archive"u8, want: new @string[]{".apk"u8}.slice()),
            new(typ: "image/apng"u8, want: new @string[]{".apng"u8}.slice()),
            new(typ: "image/avif"u8, want: new @string[]{".avif"u8}.slice()),
            new(typ: "application/octet-stream"u8, want: new @string[]{".bin"u8, ".com"u8, ".exe"u8}.slice()),
            new(typ: "image/bmp"u8, want: new @string[]{".bmp"u8}.slice()),
            new(typ: "text/css; charset=utf-8"u8, want: new @string[]{".css"u8}.slice()),
            new(typ: "text/csv; charset=utf-8"u8, want: new @string[]{".csv"u8}.slice()),
            new(typ: "application/msword"u8, want: new @string[]{".doc"u8}.slice()),
            new(typ: "application/vnd.openxmlformats-officedocument.wordprocessingml.document"u8, want: new @string[]{".docx"u8}.slice()),
            new(typ: "text/html; charset=utf-8"u8, want: new @string[]{".ehtml"u8, ".htm"u8, ".html"u8, ".shtml"u8}.slice()),
            new(typ: "message/rfc822"u8, want: new @string[]{".eml"u8}.slice()),
            new(typ: "audio/flac"u8, want: new @string[]{".flac"u8}.slice()),
            new(typ: "image/gif"u8, want: new @string[]{".gif"u8}.slice()),
            new(typ: "application/gzip"u8, want: new @string[]{".gz"u8}.slice()),
            new(typ: "image/vnd.microsoft.icon"u8, want: new @string[]{".ico"u8}.slice()),
            new(typ: "text/calendar; charset=utf-8"u8, want: new @string[]{".ics"u8}.slice()),
            new(typ: "image/jpeg"u8, want: new @string[]{".jfif"u8, ".jpeg"u8, ".jpg"u8, ".pjp"u8, ".pjpeg"u8}.slice()),
            new(typ: "text/javascript; charset=utf-8"u8, want: new @string[]{".js"u8, ".mjs"u8}.slice()),
            new(typ: "application/json"u8, want: new @string[]{".json"u8}.slice()),
            new(typ: "audio/mp4"u8, want: new @string[]{".m4a"u8}.slice()),
            new(typ: "audio/mpeg"u8, want: new @string[]{".mp3"u8}.slice()),
            new(typ: "video/mp4"u8, want: new @string[]{".mp4"u8}.slice()),
            new(typ: "audio/ogg"u8, want: new @string[]{".oga"u8, ".ogg"u8, ".opus"u8}.slice()),
            new(typ: "video/ogg"u8, want: new @string[]{".ogv"u8}.slice()),
            new(typ: "application/pdf"u8, want: new @string[]{".pdf"u8}.slice()),
            new(typ: "image/png"u8, want: new @string[]{".png"u8}.slice()),
            new(typ: "application/vnd.ms-powerpoint"u8, want: new @string[]{".ppt"u8}.slice()),
            new(typ: "application/vnd.openxmlformats-officedocument.presentationml.presentation"u8, want: new @string[]{".pptx"u8}.slice()),
            new(typ: "application/rdf+xml"u8, want: new @string[]{".rdf"u8}.slice()),
            new(typ: "application/rtf"u8, want: new @string[]{".rtf"u8}.slice()),
            new(typ: "image/svg+xml"u8, want: new @string[]{".svg"u8}.slice()),
            new(typ: "text/plain; charset=utf-8"u8, want: new @string[]{".text"u8, ".txt"u8}.slice()),
            new(typ: "image/tiff"u8, want: new @string[]{".tif"u8, ".tiff"u8}.slice()),
            new(typ: "text/vtt; charset=utf-8"u8, want: new @string[]{".vtt"u8}.slice()),
            new(typ: "application/wasm"u8, want: new @string[]{".wasm"u8}.slice()),
            new(typ: "audio/wav"u8, want: new @string[]{".wav"u8}.slice()),
            new(typ: "audio/webm"u8, want: new @string[]{".webm"u8}.slice()),
            new(typ: "image/webp"u8, want: new @string[]{".webp"u8}.slice()),
            new(typ: "text/xml; charset=utf-8"u8, want: new @string[]{".xbl"u8, ".xml"u8, ".xsl"u8}.slice()),
            new(typ: "image/x-xbitmap"u8, want: new @string[]{".xbm"u8}.slice()),
            new(typ: "application/xhtml+xml"u8, want: new @string[]{".xht"u8, ".xhtml"u8}.slice()),
            new(typ: "application/vnd.ms-excel"u8, want: new @string[]{".xls"u8}.slice()),
            new(typ: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"u8, want: new @string[]{".xlsx"u8}.slice()),
            new(typ: "application/zip"u8, want: new @string[]{".zip"u8}.slice())
        }.slice();

        foreach (var (_, tt) in tests) {
            var (got, err) = ExtensionsByType(tt.typ);
            if (err != default!) {
                Ꮡt.Errorf("ExtensionsByType(%q): %v"u8, tt.typ, err);
                continue;
            }
            if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
                Ꮡt.Errorf("ExtensionsByType(%q) = %q; want %q"u8, tt.typ, got, tt.want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end mime_internal_test_package
