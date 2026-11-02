// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.embed.@internal;

using embed = embed_package;
using io = io_package;
using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.io;
using go.testing;
using ꓸꓸꓸstring = Span<@string>;

partial class embedtest_internal_test_package {

//go:embed testdata/h*.txt
//go:embed c*.txt testdata/g*.txt
internal static embed.FS global = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["concurrency.txt", "testdata/", "testdata/glass.txt", "testdata/hello.txt"]);

//go:embed c*txt
internal static @string concurrency = go.embed_package.ΔEmbedString(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "concurrency.txt");

//go:embed testdata/g*.txt
internal static slice<byte> glass = go.embed_package.ΔEmbedBytes<byte>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/glass.txt");

internal static void testFiles(ж<testing.T> t, embed.FS f, @string name, @string data) {
    t.Helper();
    var (d, err) = f.ReadFile(name);
    if (err != nil) {
        t.Error(err);
        return;
    }
    if (((sstring)d) != data) {
        t.Errorf("read %v = %q, want %q", name, d, data);
    }
}

internal static partial void testString(ж<testing.T> t, @string s, @string name, @string data) {
    t.Helper();
    if (s != data) {
        t.Errorf("%v = %q, want %q", name, s, data);
    }
}

internal static void testDir(ж<testing.T> t, embed.FS f, @string name, params ꓸꓸꓸstring expectʗp) {
    var expect = expectʗp.slice();

    t.Helper();
    var (dirs, err) = f.ReadDir(name);
    if (err != nil) {
        t.Error(err);
        return;
    }
    slice<@string> names = default;
    foreach (var (_, d) in dirs) {
        @string nameΔ1 = d.Name();
        if (d.IsDir()) {
            nameΔ1 += "/"u8;
        }
        names = append(names, nameΔ1);
    }
    if (!slices.Equal<slice<@string>, @string>(names, expect)) {
        t.Errorf("readdir %v = %v, want %v", name, names, expect);
    }
}

// Tests for issue 49514.
internal static rune _ᴛ1ʗ = (rune)'"';

internal static rune _ᴛ2ʗ = (rune)'\'';

internal static rune _ᴛ3ʗ = (rune)0x1F986;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string concurrencyTxtˢ => ref concurrencyTxtˢᵇ;
private static readonly @string concurrencyTxtˢᵇ = "concurrency.txt"u8;
internal static ref readonly @string concurrencyIsNotˢ => ref concurrencyIsNotˢᵇ;
private static readonly @string concurrencyIsNotˢᵇ = "Concurrency is not parallelism.\n"u8;
internal static ref readonly @string testdataHelloTxtˢ => ref testdataHelloTxtˢᵇ;
private static readonly @string testdataHelloTxtˢᵇ = "testdata/hello.txt"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello, world\n"u8;
internal static ref readonly @string testdataGlassTxtˢ => ref testdataGlassTxtˢᵇ;
private static readonly @string testdataGlassTxtˢᵇ = "testdata/glass.txt"u8;
internal static ref readonly @string iCanEatGlassAndItDoesnTˢ => ref iCanEatGlassAndItDoesnTˢᵇ;
private static readonly @string iCanEatGlassAndItDoesnTˢᵇ = "I can eat glass and it doesn't hurt me.\n"u8;
internal static ref readonly @string concurrencyˢ => ref concurrencyˢᵇ;
private static readonly @string concurrencyˢᵇ = "concurrency"u8;
internal static ref readonly @string glassˢ => ref glassˢᵇ;
private static readonly @string glassˢᵇ = "glass"u8;

public static void TestGlobal(ж<testing.T> t) {
    testFiles(t, global, concurrencyTxtˢ, concurrencyIsNotˢ);
    testFiles(t, global, testdataHelloTxtˢ, helloWorldˢ);
    testFiles(t, global, testdataGlassTxtˢ, iCanEatGlassAndItDoesnTˢ);

    {
        var err = fstest.TestFS(new embedtest_test_package.embed_FSᴠFS(global), concurrencyTxtˢ, testdataHelloTxtˢ); if (err != nil) {
            t.Fatal(err);
        }
    }

    testString(t, concurrency, concurrencyˢ, concurrencyIsNotˢ);
    testString(t, (@string)glass, glassˢ, iCanEatGlassAndItDoesnTˢ);
}

//go:embed testdata
internal static embed.FS testDirAll = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataII18nTxtˢ => ref testdataII18nTxtˢᵇ;
private static readonly @string testdataII18nTxtˢᵇ = "testdata/i/i18n.txt"u8;
internal static ref readonly @string internationalizationˢ => ref internationalizationˢᵇ;
private static readonly @string internationalizationˢᵇ = "internationalization\n"u8;
internal static ref readonly @string testdataIJKK8sTxtˢ => ref testdataIJKK8sTxtˢᵇ;
private static readonly @string testdataIJKK8sTxtˢᵇ = "testdata/i/j/k/k8s.txt"u8;
internal static ref readonly @string kubernetesˢ => ref kubernetesˢᵇ;
private static readonly @string kubernetesˢᵇ = "kubernetes\n"u8;
internal static ref readonly @string testdataKenTxtˢ => ref testdataKenTxtˢᵇ;
private static readonly @string testdataKenTxtˢᵇ = "testdata/ken.txt"u8;
internal static ref readonly @string ifAProgramIsTooSlowItˢ => ref ifAProgramIsTooSlowItˢᵇ;
private static readonly @string ifAProgramIsTooSlowItˢᵇ = "If a program is too slow, it must have a loop.\n"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata/"u8;
internal static ref readonly @string testdataIˢ => ref testdataIˢᵇ;
private static readonly @string testdataIˢᵇ = "testdata/i"u8;
internal static ref readonly @string i18nTxtˢ => ref i18nTxtˢᵇ;
private static readonly @string i18nTxtˢᵇ = "i18n.txt"u8;
internal static ref readonly @string testdataIJˢ => ref testdataIJˢᵇ;
private static readonly @string testdataIJˢᵇ = "testdata/i/j"u8;
internal static ref readonly @string testdataIJKˢ => ref testdataIJKˢᵇ;
private static readonly @string testdataIJKˢᵇ = "testdata/i/j/k"u8;
internal static ref readonly @string k8sTxtˢ => ref k8sTxtˢᵇ;
private static readonly @string k8sTxtˢᵇ = "k8s.txt"u8;

public static void TestDir(ж<testing.T> t) {
    var all = testDirAll;
    testFiles(t, all, testdataHelloTxtˢ, helloWorldˢ);
    testFiles(t, all, testdataII18nTxtˢ, internationalizationˢ);
    testFiles(t, all, testdataIJKK8sTxtˢ, kubernetesˢ);
    testFiles(t, all, testdataKenTxtˢ, ifAProgramIsTooSlowItˢ);

    testDir(t, all, ".", testdataˢ);
    testDir(t, all, testdataIˢ, i18nTxtˢ, "j/");
    testDir(t, all, testdataIJˢ, "k/");
    testDir(t, all, testdataIJKˢ, k8sTxtˢ);
}

//go:embed testdata
internal static embed.FS testHiddenDir = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);

//go:embed testdata/*
internal static embed.FS testHiddenStar = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/.hidden/", "testdata/_hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/.hidden/fortune.txt", "testdata/.hidden/more/", "testdata/.hidden/more/tip.txt", "testdata/_hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ2 => ref testdataˢ2ᵇ;
private static readonly @string testdataˢ2ᵇ = "testdata"u8;
internal static ref readonly @string notHiddenˢ => ref notHiddenˢᵇ;
private static readonly @string notHiddenˢᵇ = "-not-hidden/"u8;
internal static ref readonly @string asciiTxtˢ => ref asciiTxtˢᵇ;
private static readonly @string asciiTxtˢᵇ = "ascii.txt"u8;
internal static ref readonly @string glassTxtˢ => ref glassTxtˢᵇ;
private static readonly @string glassTxtˢᵇ = "glass.txt"u8;
internal static ref readonly @string helloTxtˢ => ref helloTxtˢᵇ;
private static readonly @string helloTxtˢᵇ = "hello.txt"u8;
internal static ref readonly @string kenTxtˢ => ref kenTxtˢᵇ;
private static readonly @string kenTxtˢᵇ = "ken.txt"u8;
internal static ref readonly @string hiddenˢ => ref hiddenˢᵇ;
private static readonly @string hiddenˢᵇ = ".hidden/"u8;
internal static ref readonly @string hiddenˢ2 => ref hiddenˢ2ᵇ;
private static readonly @string hiddenˢ2ᵇ = "_hidden/"u8;
internal static ref readonly @string testdataHiddenˢ => ref testdataHiddenˢᵇ;
private static readonly @string testdataHiddenˢᵇ = "testdata/.hidden"u8;
internal static ref readonly @string fortuneTxtˢ => ref fortuneTxtˢᵇ;
private static readonly @string fortuneTxtˢᵇ = "fortune.txt"u8;
internal static ref readonly @string moreˢ => ref moreˢᵇ;
private static readonly @string moreˢᵇ = "more/"u8;

public static void TestHidden(ж<testing.T> t) {
    var dir = testHiddenDir;
    var star = testHiddenStar;

    t.Logf("//go:embed testdata");

    testDir(t, dir, testdataˢ2,
        notHiddenˢ, asciiTxtˢ, glassTxtˢ, helloTxtˢ, "i/", kenTxtˢ);

    t.Logf("//go:embed testdata/*");

    testDir(t, star, testdataˢ2,
        notHiddenˢ, hiddenˢ, hiddenˢ2, asciiTxtˢ, glassTxtˢ, helloTxtˢ, "i/", kenTxtˢ);

    testDir(t, star, testdataHiddenˢ,
        fortuneTxtˢ, moreˢ); // but not .more or _more
}

public static void TestUninitialized(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        embed.FS uninitialized = default;
        testDir(t, uninitialized, ".");
        var (f, err) = uninitialized.Open(".");
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != nil) {
            t.Fatal(err);
        }
        if (!fi.IsDir()) {
            t.Errorf("in uninitialized embed.FS, . is not a directory");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

//go:embed "testdata/hello.txt"
internal static slice<T> helloT = go.embed_package.ΔEmbedBytes<T>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
//go:embed "testdata/hello.txt"
internal static slice<uint8> helloUint8 = go.embed_package.ΔEmbedBytes<uint8>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
//go:embed "testdata/hello.txt"
internal static slice<EmbedUint8> helloEUint8 = go.embed_package.ΔEmbedBytes<EmbedUint8>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
//go:embed "testdata/hello.txt"
internal static EmbedBytes helloBytes = (EmbedBytes)(go.embed_package.ΔEmbedBytes<byte>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt"));
//go:embed "testdata/hello.txt"
internal static EmbedString helloString = (EmbedString)(go.embed_package.ΔEmbedString(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt"));

public partial struct T /*num:byte*/;

public partial struct EmbedUint8 /*num:uint8*/;

public partial struct EmbedBytes /*[]byte*/;

public partial struct EmbedString /*@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFileˢ = (@string)"ReadFile:"u8;

// golang.org/issue/47735
public static void TestAliases(ж<testing.T> t) {
    var all = testDirAll;
    var (want, e) = all.ReadFile(testdataHelloTxtˢ);
    if (e != nil) {
        t.Fatal(readFileˢ, e);
    }
    var wantʗ1 = want;
    void check(any g) {
        var got = reflect.ValueOf(g);
        for (nint i = 0; i < got.Len(); i++) {
            if ((byte)got.Index(i).Uint() != wantʗ1[i]) {
                t.Fatalf("got %v want %v", got.Bytes(), wantʗ1);
            }
        }
    }
    check(helloT);
    check(helloUint8);
    check(helloEUint8);
    check(helloBytes);
    check(helloString);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openˢ = (@string)"Open:"u8;
internal static readonly object readˢ = (@string)"Read:"u8;
internal static readonly object seekˢ = (@string)"Seek:"u8;
internal static readonly object readAtˢ = (@string)"ReadAt:"u8;

public static void TestOffset(ж<testing.T> t) {
    var (@file, err) = testDirAll.Open(testdataHelloTxtˢ);
    if (err != nil) {
        t.Fatal(openˢ, err);
    }

    @string want = helloWorldˢ;

    // Read the entire file.
    var got = new slice<byte>(len(want));
    (var n, err) = @file.Read(got);
    if (err != nil) {
        t.Fatal(readˢ, err);
    }
    if (n != len(want)) {
        t.Fatal(readˢ, n);
    }
    if (((sstring)got) != want) {
        t.Fatalf("Read: %q", got);
    }

    // Try to read one byte; confirm we're at the EOF.
    array<byte> buf = new(1);
    (n, err) = @file.Read(buf[..]);
    if (!AreEqual(err, io.EOF)) {
        t.Fatal(readˢ, err);
    }
    if (n != 0) {
        t.Fatal(readˢ, n);
    }

    // Use seek to get the offset at the EOF.
    var seeker = @file._<io.Seeker>();
    (var off, err) = seeker.Seek(0, io.SeekCurrent);
    if (err != nil) {
        t.Fatal(seekˢ, err);
    }
    if (off != (int64)len(want)) {
        t.Fatal(seekˢ, off);
    }

    // Use ReadAt to read the entire file, ignoring the offset.
    var at = @file._<io.ReaderAt>();
    got = new slice<byte>(len(want));
    (n, err) = at.ReadAt(got, 0);
    if (err != nil) {
        t.Fatal(readAtˢ, err);
    }
    if (n != len(want)) {
        t.Fatalf("ReadAt: got %d bytes, want %d bytes", n, len(want));
    }
    if (((sstring)got) != want) {
        t.Fatalf("ReadAt: got %q, want %q", got, want);
    }

    // Use ReadAt with non-zero offset.
    off = (int64)7;
    want = want.slice((nint)(off));
    got = new slice<byte>(len(want));
    (n, err) = at.ReadAt(got, off);
    if (err != nil) {
        t.Fatal(readAtˢ, err);
    }
    if (n != len(want)) {
        t.Fatalf("ReadAt: got %d bytes, want %d bytes", n, len(want));
    }
    if (((sstring)got) != want) {
        t.Fatalf("ReadAt: got %q, want %q", got, want);
    }
}

} // end embedtest_internal_test_package
