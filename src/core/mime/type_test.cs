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
    once = new Δsync.Once();
    testInitMime = fn;
    return () => {
        testInitMime = default;
        once = new Δsync.Once();
    };
}

internal static void clearMimeTypes() {
    setMimeTypes(new map<@string, @string>{}, new map<@string, @string>{});
}

internal static void setType(@string ext, @string typ) {
    if (!strings.HasPrefix(ext, ".")) {
        throw panic("missing leading dot");
    }
    var err = setExtensionType(ext, typ); if (err != nil) {
        throw panic("bad test data: " + err.Error());
    }
}

public static void TestTypeByExtension(ж<testing.T> t) {
    once = new Δsync.Once();
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
            t.Errorf("TypeByExtension(%q) = %q, want %q", ext, val, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = ".foo"u8;
internal static ref readonly @string xFooˢ => ref xFooˢᵇ;
private static readonly @string xFooˢᵇ = "x/foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = ".bar"u8;
internal static ref readonly @string xBarˢ => ref xBarˢᵇ;
private static readonly @string xBarˢᵇ = "x/bar"u8;
internal static ref readonly @string barˢ2 => ref barˢ2ᵇ;
private static readonly @string barˢ2ᵇ = ".Bar"u8;
internal static ref readonly @string xBarCapital1ˢ => ref xBarCapital1ˢᵇ;
private static readonly @string xBarCapital1ˢᵇ = "x/bar; capital=1"u8;

public static void TestTypeByExtension_LocalData(ж<testing.T> t) {
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
            [".foo"] = "x/foo",
            [".bar"] = "x/bar",
            [".Bar"] = "x/bar; capital=1",
            [".sdlkfjskdlfj"] = "",
            [".t1"] = "" // testdata shouldn't be used
        };

        foreach (var (ext, want) in tests) {
            @string val = TypeByExtension(ext);
            if (val != want) {
                t.Errorf("TypeByExtension(%q) = %q, want %q", ext, val, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = ".TEST"u8;
internal static ref readonly @string tesTˢ => ref tesTˢᵇ;
private static readonly @string tesTˢᵇ = ".tesT"u8;
internal static ref readonly @string tesTˢ2 => ref tesTˢ2ᵇ;
private static readonly @string tesTˢ2ᵇ = ".TesT"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string customᶜ => ref customᶜᵇ;
private static readonly @string customᶜᵇ = "test/test; charset=iso-8859-1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string capsᶜ => ref capsᶜᵇ;
private static readonly @string capsᶜᵇ = "test/test; WAS=ALLCAPS"u8;

public static void TestTypeByExtensionCase(ж<testing.T> t) {
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
                t.Fatalf("for .tesT, got %q; want %q", got, custom);
            }
        }
        {
            @string got = TypeByExtension(testˢ); if (got != caps) {
                t.Fatalf("for .TEST, got %q; want %s", got, caps);
            }
        }

        // case-insensitive
        {
            @string got = TypeByExtension(tesTˢ2); if (got != custom) {
                t.Fatalf("for .TesT, got %q; want %q", got, custom);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gifˢ => ref gifˢᵇ;
private static readonly @string gifˢᵇ = ".gif"u8;
internal static ref readonly @string imageGifˢ => ref imageGifˢᵇ;
private static readonly @string imageGifˢᵇ = "image/gif"u8;
internal static ref readonly @string fooLetterˢ => ref fooLetterˢᵇ;
private static readonly @string fooLetterˢᵇ = "foo/letter"u8;
internal static ref readonly @string pngˢ => ref pngˢᵇ;
private static readonly @string pngˢᵇ = ".PNG"u8;
internal static ref readonly @string imagePngˢ => ref imagePngˢᵇ;
private static readonly @string imagePngˢᵇ = "image/png"u8;

internal partial struct TestExtensionsByType_tests /*dyn*/ {
    internal @string typ;
    internal slice<@string> want;
    internal @string wantErr;
}

public static void TestExtensionsByType(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            setType(gifˢ, imageGifˢ);
            setType(".a", fooLetterˢ);
            setType(".b", fooLetterˢ);
            setType(".B", fooLetterˢ);
            setType(pngˢ, imagePngˢ);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        var tests = slice<TestExtensionsByType_tests>([
            new(typ: "image/gif", want: [".gif"]),
            new(typ: "image/png", want: [".png"]), // lowercase
            new(typ: "foo/letter", want: [".a", ".b"]),
            new(typ: "x/unknown", want: default)
        ]);

        foreach (var (_, tt) in range(tests)) {
            var (got, err) = ExtensionsByType(tt.typ);
            if (err != nil && tt.wantErr != ""u8 && strings.Contains(err.Error(), tt.wantErr)) {
                continue;
            }
            if (err != nil) {
                t.Errorf("ExtensionsByType(%q) error: %v", tt.typ, err);
                continue;
            }
            if (tt.wantErr != ""u8) {
                t.Errorf("ExtensionsByType(%q) = %q, %v; want error substring %q", tt.typ, got, err, tt.wantErr);
                continue;
            }
            if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
                t.Errorf("ExtensionsByType(%q) = %q; want %q", tt.typ, got, tt.want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static ref readonly @string htmlˢ => ref htmlˢᵇ;
private static readonly @string htmlˢᵇ = ".html"u8;
internal static ref readonly @string htMLˢ => ref htMLˢᵇ;
private static readonly @string htMLˢᵇ = ".HtML"u8;

public static void TestLookupMallocs(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    var n = testing.AllocsPerRun(10000, () => {
        TypeByExtension(htmlˢ);
        TypeByExtension(htMLˢ);
    });
    if (n > 0D) {
        t.Errorf("allocs = %v; want 0", n);
    }
}

public static void BenchmarkTypeByExtension(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initMime();
    b.ResetTimer();

    foreach (var (_, ext) in range(slice<@string>([
        ".html",
        ".HTML",
        ".unused"
    ]))) {
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

    foreach (var (_, typ) in range(slice<@string>([
        "text/html",
        "text/html; charset=utf-8",
        "application/octet-stream"
    ]))) {
        Ꮡb.Run(typ, (ж<testing.B> bΔ1) => {
            bΔ1.RunParallel((ж<testing.PB> pb) => {
                while (pb.Next()) {
                    var (_, err) = ExtensionsByType(typ); if (err != nil) {
                        bΔ1.Fatal(err);
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

public static void TestExtensionsByType2(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            // Initialize built-in types like in type.go before osInitMime.
            setMimeTypes(builtinTypesLower, builtinTypesLower);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);

        var tests = slice<TestExtensionsByType2_tests>([
            new(typ: "application/postscript", want: [".ai", ".eps", ".ps"]),
            new(typ: "application/vnd.android.package-archive", want: [".apk"]),
            new(typ: "image/apng", want: [".apng"]),
            new(typ: "image/avif", want: [".avif"]),
            new(typ: "application/octet-stream", want: [".bin", ".com", ".exe"]),
            new(typ: "image/bmp", want: [".bmp"]),
            new(typ: "text/css; charset=utf-8", want: [".css"]),
            new(typ: "text/csv; charset=utf-8", want: [".csv"]),
            new(typ: "application/msword", want: [".doc"]),
            new(typ: "application/vnd.openxmlformats-officedocument.wordprocessingml.document", want: [".docx"]),
            new(typ: "text/html; charset=utf-8", want: [".ehtml", ".htm", ".html", ".shtml"]),
            new(typ: "message/rfc822", want: [".eml"]),
            new(typ: "audio/flac", want: [".flac"]),
            new(typ: "image/gif", want: [".gif"]),
            new(typ: "application/gzip", want: [".gz"]),
            new(typ: "image/vnd.microsoft.icon", want: [".ico"]),
            new(typ: "text/calendar; charset=utf-8", want: [".ics"]),
            new(typ: "image/jpeg", want: [".jfif", ".jpeg", ".jpg", ".pjp", ".pjpeg"]),
            new(typ: "text/javascript; charset=utf-8", want: [".js", ".mjs"]),
            new(typ: "application/json", want: [".json"]),
            new(typ: "audio/mp4", want: [".m4a"]),
            new(typ: "audio/mpeg", want: [".mp3"]),
            new(typ: "video/mp4", want: [".mp4"]),
            new(typ: "audio/ogg", want: [".oga", ".ogg", ".opus"]),
            new(typ: "video/ogg", want: [".ogv"]),
            new(typ: "application/pdf", want: [".pdf"]),
            new(typ: "image/png", want: [".png"]),
            new(typ: "application/vnd.ms-powerpoint", want: [".ppt"]),
            new(typ: "application/vnd.openxmlformats-officedocument.presentationml.presentation", want: [".pptx"]),
            new(typ: "application/rdf+xml", want: [".rdf"]),
            new(typ: "application/rtf", want: [".rtf"]),
            new(typ: "image/svg+xml", want: [".svg"]),
            new(typ: "text/plain; charset=utf-8", want: [".text", ".txt"]),
            new(typ: "image/tiff", want: [".tif", ".tiff"]),
            new(typ: "text/vtt; charset=utf-8", want: [".vtt"]),
            new(typ: "application/wasm", want: [".wasm"]),
            new(typ: "audio/wav", want: [".wav"]),
            new(typ: "audio/webm", want: [".webm"]),
            new(typ: "image/webp", want: [".webp"]),
            new(typ: "text/xml; charset=utf-8", want: [".xbl", ".xml", ".xsl"]),
            new(typ: "image/x-xbitmap", want: [".xbm"]),
            new(typ: "application/xhtml+xml", want: [".xht", ".xhtml"]),
            new(typ: "application/vnd.ms-excel", want: [".xls"]),
            new(typ: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", want: [".xlsx"]),
            new(typ: "application/zip", want: [".zip"])
        ]);

        foreach (var (_, tt) in range(tests)) {
            var (got, err) = ExtensionsByType(tt.typ);
            if (err != nil) {
                t.Errorf("ExtensionsByType(%q): %v", tt.typ, err);
                continue;
            }
            if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
                t.Errorf("ExtensionsByType(%q) = %q; want %q", tt.typ, got, tt.want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end mime_internal_test_package
