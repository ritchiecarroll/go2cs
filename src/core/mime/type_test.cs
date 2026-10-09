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
internal static readonly @string fooˢ2 = ".foo"u8;
internal static readonly @string xFooˢ = "x/foo"u8;
internal static readonly @string barˢ2 = ".bar"u8;
internal static readonly @string xBarˢ = "x/bar"u8;
internal static readonly @string barˢ3 = ".Bar"u8;
internal static readonly @string xBarCapital1ˢ = "x/bar; capital=1"u8;

public static void TestTypeByExtension_LocalData(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var cleanup = setMimeInit(() => {
            clearMimeTypes();
            setType(fooˢ2, xFooˢ);
            setType(barˢ2, xBarˢ);
            setType(barˢ3, xBarCapital1ˢ);
        });
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        var tests = new map<@string, @string>{
            [".foo"u8] = "x/foo"u8,
            [".bar"u8] = "x/bar"u8,
            [".Bar"u8] = "x/bar; capital=1"u8,
            [".sdlkfjskdlfj"u8] = ""u8,
            [".t1"u8] = ""u8
        };
        // testdata shouldn't be used
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
internal static readonly @string htmlˢ3 = ".html"u8;
internal static readonly @string htMLˢ = ".HtML"u8;

public static void TestLookupMallocs(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    var n = testing.AllocsPerRun(10000, () => {
        TypeByExtension(htmlˢ3);
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
            new(typ: "image/jpeg"u8, want: new @string[]{".jpeg"u8, ".jpg"u8}.slice())
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
