// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string basicˢ = "basic"u8;
internal static readonly @string aBCDEF0123456789abcdefˢ = """

           0 1 2 3  4 5 6 7   8 9 a b  c d e f  0123456789abcdef
00000000: 10111213 14151617  18191a1b 1c1d1e1f  ................
00000010: 20212223 24252627  28292a2b 2c2d2e2f   !"#$%&'()*+,-./
"""u8.Raw();
internal static readonly @string word4ˢ = "word=4"u8;
internal static readonly @string bA98FEDC0123456789abcdefˢ = """

           3 2 1 0  7 6 5 4   b a 9 8  f e d c  0123456789abcdef
00000000: 13121110 17161514  1b1a1918 1f1e1d1c  ................
00000010: 23222120 27262524  2b2a2928 2f2e2d2c   !"#$%&'()*+,-./
"""u8.Raw();
internal static readonly @string word8ˢ = "word=8"u8;
internal static readonly @string fEDCBA980123456789abcdefˢ = """

           7 6 5 4  3 2 1 0   f e d c  b a 9 8  0123456789abcdef
00000000: 17161514 13121110  1f1e1d1c 1b1a1918  ................
00000010: 27262524 23222120  2f2e2d2c 2b2a2928   !"#$%&'()*+,-./
"""u8.Raw();
internal static readonly @string offset1ˢ = "offset=1"u8;
internal static readonly @string aBCDEF0123456789abcdefˢ2 = """

           0 1 2 3  4 5 6 7   8 9 a b  c d e f  0123456789abcdef
00000000:   101112 13141516  1718191a 1b1c1d1e   ...............
00000010: 1f202122 23242526  2728292a 2b2c2d2e  . !"#$%&'()*+,-.
00000020: 2f                                    /
"""u8.Raw();
internal static readonly @string offset1AndWord4ˢ = "offset=1 and word=4"u8;
internal static readonly @string bA98FEDC0123456789abcdefˢ2 = """

           3 2 1 0  7 6 5 4   b a 9 8  f e d c  0123456789abcdef
00000000: 121110   16151413  1a191817 1e1d1c1b   ...............
00000010: 2221201f 26252423  2a292827 2e2d2c2b  . !"#$%&'()*+,-.
00000020:       2f                              /
"""u8.Raw();
internal static readonly @string partialsˢ = "partials"u8;
internal static readonly @string marksˢ = "marks"u8;
internal static readonly @string aBCDEF0123456789abcdefˢ3 = """

           0 1 2 3  4 5 6 7   8 9 a b  c d e f  0123456789abcdef
00000000: 10111213 14151617  18191a1b 1c1d1e1f  ................
          ^ mark
                         ^ mark
                                          ^ mark
00000010: 20212223 24252627  28292a2b 2c2d2e2f   !"#$%&'()*+,-./
                     ^ mark
                                      ^ mark
"""u8.Raw();
internal static readonly @string marksAndWord4ˢ = "marks and word=4"u8;
internal static readonly @string bA98FEDC0123456789abcdefˢ3 = """

           3 2 1 0  7 6 5 4   b a 9 8  f e d c  0123456789abcdef
00000000: 13121110 17161514  1b1a1918 1f1e1d1c  ................
          ^ mark
00000010: 23222120 27262524  2b2a2928 2f2e2d2c   !"#$%&'()*+,-./
                                      ^ mark
"""u8.Raw();

public static void TestHexdumper(ж<testing.T> Ꮡt) {
    void check(@string label, @string got, @string want) {
        got = strings.TrimRight(got, "\n"u8);
        want = strings.TrimPrefix(want, "\n"u8);
        want = strings.TrimRight(want, "\n"u8);
        if (got != want) {
            Ꮡt.Errorf("%s: got\n%s\nwant\n%s"u8, label, got, want);
        }
    }

    var data = new slice<byte>(32);
    foreach (var (i, _) in data) {
        data[i] = (byte)(0x10 + (byte)i);
    }

    check(basicˢ, runtime_internal_test_package.Hexdumper(0, 1, default!, data), aBCDEF0123456789abcdefˢ);

    if (!goarch.BigEndian) {
        // Different word sizes
        check(word4ˢ, runtime_internal_test_package.Hexdumper(0, 4, default!, data), bA98FEDC0123456789abcdefˢ);
        check(word8ˢ, runtime_internal_test_package.Hexdumper(0, 8, default!, data), fEDCBA980123456789abcdefˢ);
    }

    // Starting offset
    check(offset1ˢ, runtime_internal_test_package.Hexdumper(1, 1, default!, data), aBCDEF0123456789abcdefˢ2);
    if (!goarch.BigEndian) {
        // ... combined with a word size
        check(offset1AndWord4ˢ, runtime_internal_test_package.Hexdumper(1, 4, default!, data), bA98FEDC0123456789abcdefˢ2);
    }

    // Partial data full of annoying boundaries.
    var partials = new slice<slice<byte>>(0);
    for (nint i = 0; i < len(data); i += 2) {
        partials = append(partials, data.slice(i, i + 2));
    }
    check(partialsˢ, runtime_internal_test_package.Hexdumper(1, 1, default!, partials.ꓸꓸꓸ), aBCDEF0123456789abcdefˢ2);

    // Marks.
    check(marksˢ, runtime_internal_test_package.Hexdumper(0, 1, (uintptr addr, Action start) => {
        if (addr % 7 == 0) {
            start();
            println((@string)"mark"u8);
        }
    }, data), aBCDEF0123456789abcdefˢ3);
    if (!goarch.BigEndian) {
        check(marksAndWord4ˢ, runtime_internal_test_package.Hexdumper(0, 4, (uintptr addr, Action start) => {
            if (addr % 7 == 0) {
                start();
                println((@string)"mark"u8);
            }
        }, data), bA98FEDC0123456789abcdefˢ3);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object requires64BitLittleˢ = (@string)"requires 64-bit little endian"u8;

public static void TestHexdumpWords(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (goarch.BigEndian || goarch.PtrSize != 8) {
            // We could support these, but it's kind of a pain.
            Ꮡt.Skip(requires64BitLittleˢ);
        }

        // Most of this is in hexdumper. Here we just test the symbolizer.

        var pc = abi.FuncPCABIInternal(TestHexdumpWords);
        var pcs = slices.Repeat<slice<uintptr>, uintptr>(new uintptr[]{pc}.slice(), 3);

        // Make sure pcs doesn't move around on us.
        ref var p = ref heap(new Δruntime.Pinner(), out var Ꮡp);
        defer(Ꮡp.Unpin, ref ᒐ);
        p.Pin(Ꮡ(pcs, 0));
        // Get a 16 byte, 16-byte-aligned chunk of pcs so the hexdump is simple.
        var start = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(pcs, 0));
        start = (uintptr)((start + 15) & ~(uintptr)(uintptr)15);

        // Do the hex dump.
        @string got = runtime_internal_test_package.HexdumpWords(start, 16);

        // Construct the expected output.
        @string pcStr = fmt.Sprintf("%016x"u8, pc);
        pcStr = pcStr[..8] + " " + pcStr[8..]; // Add middle space
        var ascii = new slice<byte>(8);
        foreach (var (i, _) in ascii) {
            var b = (byte)(pc.Rsh((int64)(8 * i)));
            if (b >= (rune)' ' && b <= (rune)'~'){
                ascii[i] = b;
            } else {
                ascii[i] = (rune)'.';
            }
        }
        @string want = fmt.Sprintf("""

                   7 6 5 4  3 2 1 0   f e d c  b a 9 8  0123456789abcdef
%016x: %s  %s  %s%s
                  ^ <runtime_test.TestHexdumpWords+0x0>
                                     ^ <runtime_test.TestHexdumpWords+0x0>

"""u8.Raw(), start, pcStr, pcStr, ascii, ascii);
        want = strings.TrimPrefix(want, "\n"u8);

        if (got != want) {
            Ꮡt.Errorf("got\n%s\nwant\n%s"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
