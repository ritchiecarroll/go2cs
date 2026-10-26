// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.net.textproto_package;

partial class textproto_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooDˢ = "foo %d"u8;

public static void TestPrintfLine(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(bufio.NewWriter(new textproto_internal_test_package.strings_BuilderжWriter(Ꮡbuf)));
    var err = w.PrintfLine(fooDˢ, (nint)123);
    {
        @string s = buf.String(); if (s != "foo 123\r\n"u8 || err != default!) {
            Ꮡt.Fatalf("s=%q; err=%s"u8, s, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcDefGhiJklˢ = "abc\r\n..def\r\n...ghi\r\n..jkl\r\n..\r\n.\r\n"u8;

public static void TestDotWriter(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(bufio.NewWriter(new textproto_internal_test_package.strings_BuilderжWriter(Ꮡbuf)));
    var d = w.DotWriter();
    var (n, err) = d.Write(slice<byte>("abc\n.def\n..ghi\n.jkl\n."u8));
    if (n != 21 || err != default!) {
        Ꮡt.Fatalf("Write: %d, %s"u8, n, err);
    }
    d.Close();
    @string want = abcDefGhiJklˢ;
    {
        @string s = buf.String(); if (s != want) {
            Ꮡt.Fatalf("wrote %q"u8, s);
        }
    }
}

public static void TestDotWriterCloseEmptyWrite(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(bufio.NewWriter(new textproto_internal_test_package.strings_BuilderжWriter(Ꮡbuf)));
    var d = w.DotWriter();
    var (n, err) = d.Write(new byte[]{}.slice());
    if (n != 0 || err != default!) {
        Ꮡt.Fatalf("Write: %d, %s"u8, n, err);
    }
    d.Close();
    @string want = "\r\n.\r\n"u8;
    {
        @string s = buf.String(); if (s != want) {
            Ꮡt.Fatalf("wrote %q; want %q"u8, s, want);
        }
    }
}

public static void TestDotWriterCloseNoWrite(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(bufio.NewWriter(new textproto_internal_test_package.strings_BuilderжWriter(Ꮡbuf)));
    var d = w.DotWriter();
    d.Close();
    @string want = "\r\n.\r\n"u8;
    {
        @string s = buf.String(); if (s != want) {
            Ꮡt.Fatalf("wrote %q; want %q"u8, s, want);
        }
    }
}

} // end textproto_internal_test_package
