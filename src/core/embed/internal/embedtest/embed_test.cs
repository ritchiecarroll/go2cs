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

internal static void testFiles(ж<testing.T> Ꮡt, embed.FS f, @string name, @string data) {
    Ꮡt.Helper();
    var (d, err) = f.ReadFile(name);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (((sstring)d) != data) {
        Ꮡt.Errorf("read %v = %q, want %q"u8, name, d, data);
    }
}

internal static void testString(ж<testing.T> Ꮡt, @string s, @string name, @string data) {
    Ꮡt.Helper();
    if (s != data) {
        Ꮡt.Errorf("%v = %q, want %q"u8, name, s, data);
    }
}

internal static void testDir(ж<testing.T> Ꮡt, embed.FS f, @string name, params ꓸꓸꓸstring expectʗp) {
    var expect = expectʗp.slice();

    Ꮡt.Helper();
    var (dirs, err) = f.ReadDir(name);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    slice<@string> names = default!;
    foreach (var (_, d) in dirs) {
        @string nameΔ1 = d.Name();
        if (d.IsDir()) {
            nameΔ1 += "/"u8;
        }
        names = append(names, nameΔ1);
    }
    if (!slices.Equal<slice<@string>, @string>(names, expect)) {
        Ꮡt.Errorf("readdir %v = %v, want %v"u8, name, names, expect);
    }
}

// Tests for issue 49514.
internal static rune _ᴛ1ʗ = (rune)'"';

internal static rune _ᴛ2ʗ = (rune)'\'';

internal static rune _ᴛ3ʗ = (rune)0x1F986;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrencyTxtˢ = "concurrency.txt"u8;
internal static readonly @string concurrencyIsNotˢ = "Concurrency is not parallelism.\n"u8;
internal static readonly @string testdataHelloTxtˢ = "testdata/hello.txt"u8;
internal static readonly @string helloWorldˢ = "hello, world\n"u8;
internal static readonly @string testdataGlassTxtˢ = "testdata/glass.txt"u8;
internal static readonly @string iCanEatGlassAndItDoesnTˢ = "I can eat glass and it doesn't hurt me.\n"u8;
internal static readonly @string concurrencyˢ = "concurrency"u8;
internal static readonly @string glassˢ = "glass"u8;

public static void TestGlobal(ж<testing.T> Ꮡt) {
    testFiles(Ꮡt, global, concurrencyTxtˢ, concurrencyIsNotˢ);
    testFiles(Ꮡt, global, testdataHelloTxtˢ, helloWorldˢ);
    testFiles(Ꮡt, global, testdataGlassTxtˢ, iCanEatGlassAndItDoesnTˢ);
    {
        var err = fstest.TestFS(new embedtest_test_package.embed_FSᴠFS(global), concurrencyTxtˢ, testdataHelloTxtˢ); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testString(Ꮡt, concurrency, concurrencyˢ, concurrencyIsNotˢ);
    testString(Ꮡt, ((@string)glass), glassˢ, iCanEatGlassAndItDoesnTˢ);
}

//go:embed testdata
internal static embed.FS testDirAll = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataII18nTxtˢ = "testdata/i/i18n.txt"u8;
internal static readonly @string internationalizationˢ = "internationalization\n"u8;
internal static readonly @string testdataIJKK8sTxtˢ = "testdata/i/j/k/k8s.txt"u8;
internal static readonly @string kubernetesˢ = "kubernetes\n"u8;
internal static readonly @string testdataKenTxtˢ = "testdata/ken.txt"u8;
internal static readonly @string ifAProgramIsTooSlowItˢ = "If a program is too slow, it must have a loop.\n"u8;
internal static readonly @string testdataˢ = "testdata/"u8;
internal static readonly @string testdataIˢ = "testdata/i"u8;
internal static readonly @string i18nTxtˢ = "i18n.txt"u8;
internal static readonly @string testdataIJˢ = "testdata/i/j"u8;
internal static readonly @string testdataIJKˢ = "testdata/i/j/k"u8;
internal static readonly @string k8sTxtˢ = "k8s.txt"u8;

public static void TestDir(ж<testing.T> Ꮡt) {
    var all = testDirAll;
    testFiles(Ꮡt, all, testdataHelloTxtˢ, helloWorldˢ);
    testFiles(Ꮡt, all, testdataII18nTxtˢ, internationalizationˢ);
    testFiles(Ꮡt, all, testdataIJKK8sTxtˢ, kubernetesˢ);
    testFiles(Ꮡt, all, testdataKenTxtˢ, ifAProgramIsTooSlowItˢ);
    testDir(Ꮡt, all, "."u8, testdataˢ);
    testDir(Ꮡt, all, testdataIˢ, i18nTxtˢ, "j/");
    testDir(Ꮡt, all, testdataIJˢ, "k/"u8);
    testDir(Ꮡt, all, testdataIJKˢ, k8sTxtˢ);
}

internal static embed.FS testHiddenDir = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);
internal static embed.FS testHiddenStar = go.embed_package.ΔEmbedFS(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/-not-hidden/", "testdata/.hidden/", "testdata/_hidden/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/i/", "testdata/ken.txt", "testdata/-not-hidden/fortune.txt", "testdata/.hidden/fortune.txt", "testdata/.hidden/more/", "testdata/.hidden/more/tip.txt", "testdata/_hidden/fortune.txt", "testdata/i/i18n.txt", "testdata/i/j/", "testdata/i/j/k/", "testdata/i/j/k/k8s.txt"]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ2 = "testdata"u8;
internal static readonly @string notHiddenˢ = "-not-hidden/"u8;
internal static readonly @string asciiTxtˢ = "ascii.txt"u8;
internal static readonly @string glassTxtˢ = "glass.txt"u8;
internal static readonly @string helloTxtˢ = "hello.txt"u8;
internal static readonly @string kenTxtˢ = "ken.txt"u8;
internal static readonly @string hiddenˢ = ".hidden/"u8;
internal static readonly @string hiddenˢ2 = "_hidden/"u8;
internal static readonly @string testdataHiddenˢ = "testdata/.hidden"u8;
internal static readonly @string fortuneTxtˢ = "fortune.txt"u8;
internal static readonly @string moreˢ = "more/"u8;

public static void TestHidden(ж<testing.T> Ꮡt) {
    var dir = testHiddenDir;
    var star = testHiddenStar;
    Ꮡt.Logf("//go:embed testdata"u8);
    testDir(Ꮡt, dir, testdataˢ2,
        notHiddenˢ, asciiTxtˢ, glassTxtˢ, helloTxtˢ, "i/", kenTxtˢ);
    Ꮡt.Logf("//go:embed testdata/*"u8);
    testDir(Ꮡt, star, testdataˢ2,
        notHiddenˢ, hiddenˢ, hiddenˢ2, asciiTxtˢ, glassTxtˢ, helloTxtˢ, "i/", kenTxtˢ);
    testDir(Ꮡt, star, testdataHiddenˢ,
        fortuneTxtˢ, moreˢ); // but not .more or _more
}

public static void TestUninitialized(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        embed.FS uninitialized = default!;
        testDir(Ꮡt, uninitialized, "."u8);
        var (f, err) = uninitialized.Open("."u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!fi.IsDir()) {
            Ꮡt.Errorf("in uninitialized embed.FS, . is not a directory"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<T> helloT = go.embed_package.ΔEmbedBytes<T>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
internal static slice<uint8> helloUint8 = go.embed_package.ΔEmbedBytes<uint8>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
internal static slice<EmbedUint8> helloEUint8 = go.embed_package.ΔEmbedBytes<EmbedUint8>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt");
internal static EmbedBytes helloBytes = (EmbedBytes)(go.embed_package.ΔEmbedBytes<byte>(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt"));
internal static EmbedString helloString = (EmbedString)(go.embed_package.ΔEmbedString(typeof(embedtest_internal_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/hello.txt"));

public partial struct T /*num:byte*/;

public partial struct EmbedUint8 /*num:uint8*/;

public partial struct EmbedBytes /*[]byte*/;

public partial struct EmbedString /*@string*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFileˢ = (@string)"ReadFile:"u8;

// golang.org/issue/47735
public static void TestAliases(ж<testing.T> Ꮡt) {
    var all = testDirAll;
    var (want, e) = all.ReadFile(testdataHelloTxtˢ);
    if (e != default!) {
        Ꮡt.Fatal(readFileˢ, e);
    }
    var wantʗ1 = want;
    void check(any g) {
        var got = reflect.ValueOf(g);
        for (nint i = 0; i < got.Len(); i++) {
            if ((byte)got.Index(i).Uint() != wantʗ1[i]) {
                Ꮡt.Fatalf("got %v want %v"u8, got.Bytes(), wantʗ1);
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

public static void TestOffset(ж<testing.T> Ꮡt) {
    var (@file, err) = testDirAll.Open(testdataHelloTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(openˢ, err);
    }
    @string want = helloWorldˢ;
    // Read the entire file.
    var got = new slice<byte>(len(want));
    (var n, err) = @file.Read(got);
    if (err != default!) {
        Ꮡt.Fatal(readˢ, err);
    }
    if (n != len(want)) {
        Ꮡt.Fatal(readˢ, n);
    }
    if (((sstring)got) != want) {
        Ꮡt.Fatalf("Read: %q"u8, got);
    }
    // Try to read one byte; confirm we're at the EOF.
    array<byte> buf = new(1);
    (n, err) = @file.Read(buf[..]);
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Fatal(readˢ, err);
    }
    if (n != 0) {
        Ꮡt.Fatal(readˢ, n);
    }
    // Use seek to get the offset at the EOF.
    var seeker = @file._<io.Seeker>();
    (var off, err) = seeker.Seek(0, io.SeekCurrent);
    if (err != default!) {
        Ꮡt.Fatal(seekˢ, err);
    }
    if (off != (int64)len(want)) {
        Ꮡt.Fatal(seekˢ, off);
    }
    // Use ReadAt to read the entire file, ignoring the offset.
    var at = @file._<io.ReaderAt>();
    got = new slice<byte>(len(want));
    (n, err) = at.ReadAt(got, 0);
    if (err != default!) {
        Ꮡt.Fatal(readAtˢ, err);
    }
    if (n != len(want)) {
        Ꮡt.Fatalf("ReadAt: got %d bytes, want %d bytes"u8, n, len(want));
    }
    if (((sstring)got) != want) {
        Ꮡt.Fatalf("ReadAt: got %q, want %q"u8, got, want);
    }
    // Use ReadAt with non-zero offset.
    off = (int64)7;
    want = want.slice((nint)(off));
    got = new slice<byte>(len(want));
    (n, err) = at.ReadAt(got, off);
    if (err != default!) {
        Ꮡt.Fatal(readAtˢ, err);
    }
    if (n != len(want)) {
        Ꮡt.Fatalf("ReadAt: got %d bytes, want %d bytes"u8, n, len(want));
    }
    if (((sstring)got) != want) {
        Ꮡt.Fatalf("ReadAt: got %q, want %q"u8, got, want);
    }
}

} // end embedtest_internal_test_package
