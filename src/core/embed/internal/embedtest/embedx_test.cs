// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.embed.@internal;

using embed = embed_package;
using os = os_package;
using testing = testing_package;
using static go.embed.@internal.embedtest_internal_test_package;

partial class embedtest_test_package {

internal static embed.FS global2;
internal static void initᴛglobal2() { global2 = global; }
internal static @string concurrency2;
internal static void initᴛconcurrency2() { concurrency2 = concurrency; }
internal static slice<byte> glass2;
internal static void initᴛglass2() { glass2 = glass; }
internal static @string sbig2;
internal static void initᴛsbig2() { sbig2 = sbig; }
internal static slice<byte> bbig2;
internal static void initᴛbbig2() { bbig2 = bbig; }

//go:embed testdata/*.txt
internal static embed.FS global = go.embed_package.ΔEmbedFS(typeof(embedtest_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", ["testdata/", "testdata/ascii.txt", "testdata/glass.txt", "testdata/hello.txt", "testdata/ken.txt"]);

//go:embed c*txt
internal static @string concurrency = go.embed_package.ΔEmbedString(typeof(embedtest_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "concurrency.txt");

//go:embed testdata/g*.txt
internal static slice<byte> glass = go.embed_package.ΔEmbedBytes<byte>(typeof(embedtest_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/glass.txt");

//go:embed testdata/ascii.txt
internal static @string sbig = go.embed_package.ΔEmbedString(typeof(embedtest_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/ascii.txt");

//go:embed testdata/ascii.txt
internal static slice<byte> bbig = go.embed_package.ΔEmbedBytes<byte>(typeof(embedtest_test_package).Assembly, "go.embed/embed/internal/embedtest_test/", "testdata/ascii.txt");

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataHelloTxtˢ = "testdata/hello.txt"u8;
internal static readonly @string helloWorldˢ = "hello, world\n"u8;
internal static readonly @string concurrencyˢ = "concurrency"u8;
internal static readonly @string concurrencyIsNotˢ = "Concurrency is not parallelism.\n"u8;
internal static readonly @string glassˢ = "glass"u8;
internal static readonly @string iCanEatGlassAndItDoesnTˢ = "I can eat glass and it doesn't hurt me.\n"u8;
internal static readonly @string concurrency2ˢ = "concurrency2"u8;
internal static readonly @string glass2ˢ = "glass2"u8;
internal static readonly @string testdataAsciiTxtˢ = "testdata/ascii.txt"u8;
internal static readonly @string sbigˢ = "sbig"u8;
internal static readonly @string sbig2ˢ = "sbig2"u8;
internal static readonly @string bbigˢ = "bbig"u8;

public static void TestXGlobal(ж<testing.T> Ꮡt) {
    testFiles(Ꮡt, global, testdataHelloTxtˢ, helloWorldˢ);
    testString(Ꮡt, concurrency, concurrencyˢ, concurrencyIsNotˢ);
    testString(Ꮡt, (@string)glass, glassˢ, iCanEatGlassAndItDoesnTˢ);
    testString(Ꮡt, concurrency2, concurrency2ˢ, concurrencyIsNotˢ);
    testString(Ꮡt, (@string)glass2, glass2ˢ, iCanEatGlassAndItDoesnTˢ);

    var (big, err) = os.ReadFile(testdataAsciiTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testString(Ꮡt, sbig, sbigˢ, (@string)big);
    testString(Ꮡt, sbig2, sbig2ˢ, (@string)big);
    testString(Ꮡt, (@string)bbig, bbigˢ, (@string)big);
    testString(Ꮡt, (@string)bbig2, bbigˢ, (@string)big);

    if (Ꮡt.Failed()) {
        return;
    }

    // Could check &glass[0] == &glass2[0] but also want to make sure write does not fault
    // (data must not be in read-only memory).
    var old = glass[0];
    glass[0]++;
    if (glass2[0] != glass[0]) {
        Ꮡt.Fatalf("glass and glass2 do not share storage"u8);
    }
    glass[0] = old;

    // Could check &bbig[0] == &bbig2[0] but also want to make sure write does not fault
    // (data must not be in read-only memory).
    old = bbig[0];
    bbig[0]++;
    if (bbig2[0] != bbig[0]) {
        Ꮡt.Fatalf("bbig and bbig2 do not share storage"u8);
    }
    bbig[0] = old;
}

} // end embedtest_test_package
