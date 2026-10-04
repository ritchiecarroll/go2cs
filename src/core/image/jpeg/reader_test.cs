// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using fmt = fmt_package;
using image = image_package;
using color = go.image.color_package;
using io = io_package;
using rand = go.math.rand_package;
using os = os_package;
using debug = runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using encoding;
using go.image;
using go.math;
using runtime;
using static go.image.jpeg_package;

partial class jpeg_internal_test_package {

// TestDecodeProgressive tests that decoding the baseline and progressive
// versions of the same image result in exactly the same pixel data, in YCbCr
// space for color images, and Y space for grayscale images.
public static void TestDecodeProgressive(ж<testing.T> Ꮡt) {
    var testCases = new @string[]{
        "../testdata/video-001"u8,
        "../testdata/video-001.q50.410"u8,
        "../testdata/video-001.q50.411"u8,
        "../testdata/video-001.q50.420"u8,
        "../testdata/video-001.q50.422"u8,
        "../testdata/video-001.q50.440"u8,
        "../testdata/video-001.q50.444"u8,
        "../testdata/video-005.gray.q50"u8,
        "../testdata/video-005.gray.q50.2x2"u8,
        "../testdata/video-001.separate.dc.progression"u8
    }.slice();
    foreach (var (_, tc) in testCases) {
        var (m0, err) = decodeFile(tc + ".jpeg"u8);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, tc + ".jpeg", err);
            continue;
        }
        (var m1, err) = decodeFile(tc + ".progressive.jpeg"u8);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, tc + ".progressive.jpeg", err);
            continue;
        }
        if (m0.Bounds() != m1.Bounds()) {
            Ꮡt.Errorf("%s: bounds differ: %v and %v"u8, tc, m0.Bounds(), m1.Bounds());
            continue;
        }
        // All of the video-*.jpeg files are 150x103.
        if (m0.Bounds() != image.Rect(0, 0, 150, 103)) {
            Ꮡt.Errorf("%s: bad bounds: %v"u8, tc, m0.Bounds());
            continue;
        }
        switch (m0.type()) {
        case ж<image.YCbCr> m0Δ1: {
            var m1Δ1 = m1._<ж<image.YCbCr>>();
            {
                var errΔ1 = check(m0Δ1.Bounds(), (~m0Δ1).Y, (~m1Δ1).Y, (~m0Δ1).YStride, (~m1Δ1).YStride); if (errΔ1 != default!) {
                    Ꮡt.Errorf("%s (Y): %v"u8, tc, errΔ1);
                    continue;
                }
            }
            {
                var errΔ2 = check(m0Δ1.Bounds(), (~m0Δ1).Cb, (~m1Δ1).Cb, (~m0Δ1).CStride, (~m1Δ1).CStride); if (errΔ2 != default!) {
                    Ꮡt.Errorf("%s (Cb): %v"u8, tc, errΔ2);
                    continue;
                }
            }
            {
                var errΔ3 = check(m0Δ1.Bounds(), (~m0Δ1).Cr, (~m1Δ1).Cr, (~m0Δ1).CStride, (~m1Δ1).CStride); if (errΔ3 != default!) {
                    Ꮡt.Errorf("%s (Cr): %v"u8, tc, errΔ3);
                    continue;
                }
            }
            break;
        }
        case ж<image.Gray> m0Δ1: {
            var m1Δ2 = m1._<ж<image.Gray>>();
            {
                var errΔ4 = check(m0Δ1.Bounds(), (~m0Δ1).Pix, (~m1Δ2).Pix, (~m0Δ1).Stride, (~m1Δ2).Stride); if (errΔ4 != default!) {
                    Ꮡt.Errorf("%s: %v"u8, tc, errΔ4);
                    continue;
                }
            }
            break;
        }
        default: {
            var m0Δ1 = m0;
            Ꮡt.Errorf("%s: unexpected image type %T"u8, tc, m0Δ1);
            continue;
            break;
        }}
    }
}

internal static (image.Image, error) decodeFile(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return Decode(new jpeg_internal_test_package.os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct eofReader {
    internal slice<byte> data; // deliver from Read without EOF
    internal slice<byte> dataEOF; // then deliver from Read with EOF on last chunk
    internal nint lenAtEOF;
}

[GoRecv] internal static (nint n, error err) Read(this ref eofReader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (len(r.data) > 0){
        n = copy(b, r.data);
        r.data = r.data.slice(n);
    } else {
        n = copy(b, r.dataEOF);
        r.dataEOF = r.dataEOF.slice(n);
        if (len(r.dataEOF) == 0) {
            err = io.EOF;
            if (r.lenAtEOF == -1) {
                r.lenAtEOF = n;
            }
        }
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001Jpegˢ = "../testdata/video-001.jpeg"u8;

public static void TestDecodeEOF(ж<testing.T> Ꮡt) {
    // Check that if reader returns final data and EOF at same time, jpeg handles it.
    var (data, err) = os.ReadFile(testdataVideo001Jpegˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    nint n = len(data);
    for (nint i = 0; i < n; ) {
        var r = Ꮡ(new eofReader(data.slice(0, n - i), data.slice(n - i), -1));
        var (_, errΔ1) = Decode(new jpeg_internal_test_package.eofReaderжReader(r));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Decode with Read() = %d, EOF: %v"u8, (~r).lenAtEOF, errΔ1);
        }
        if (i == 0){
            i = 1;
        } else {
            i *= 2;
        }
    }
}

// check checks that the two pix data are equal, within the given bounds.
internal static error check(image.Rectangle bounds, slice<byte> pix0, slice<byte> pix1, nint stride0, nint stride1) {
    if (stride0 <= 0 || stride0 % 8 != 0) {
        return fmt.Errorf("bad stride %d"u8, stride0);
    }
    if (stride1 <= 0 || stride1 % 8 != 0) {
        return fmt.Errorf("bad stride %d"u8, stride1);
    }
    // Compare the two pix data, one 8x8 block at a time.
    for (nint y = 0; y < len(pix0) / stride0 && y < len(pix1) / stride1; y += 8) {
        for (nint x = 0; x < stride0 && x < stride1; x += 8) {
            if (x >= bounds.Max.X || y >= bounds.Max.Y) {
                // We don't care if the two pix data differ if the 8x8 block is
                // entirely outside of the image's bounds. For example, this can
                // occur with a 4:2:0 chroma subsampling and a 1x1 image. Baseline
                // decoding works on the one 16x16 MCU as a whole; progressive
                // decoding's first pass works on that 16x16 MCU as a whole but
                // refinement passes only process one 8x8 block within the MCU.
                continue;
            }
            for (nint j = 0; j < 8; j++) {
                for (nint i = 0; i < 8; i++) {
                    nint index0 = (y + j) * stride0 + (x + i);
                    nint index1 = (y + j) * stride1 + (x + i);
                    if (pix0[index0] != pix1[index1]) {
                        return fmt.Errorf("blocks at (%d, %d) differ:\n%sand\n%s"u8, x, y,
                            pixString(pix0, stride0, x, y),
                            pixString(pix1, stride1, x, y));
                    }
                }
            }
        }
    }
    return default!;
}

internal static @string pixString(slice<byte> pix, nint stride, nint x, nint y) {
    var s = Ꮡ(new strings.Builder(nil));
    for (nint j = 0; j < 8; j++) {
        fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "\t"u8);
        for (nint i = 0; i < 8; i++) {
            fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "%02x "u8, pix[(y + j) * stride + (x + i)]);
        }
        fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "\n"u8);
    }
    return s.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo005GrayQ50ˢ = "../testdata/video-005.gray.q50.jpeg"u8;
internal static readonly object sosMarkerNotFoundˢ = (@string)"SOS marker not found"u8;

public static void TestTruncatedSOSDataDoesntPanic(ж<testing.T> Ꮡt) {
    var (b, err) = os.ReadFile(testdataVideo005GrayQ50ˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var sosMarker = new byte[]{0xff, 0xda}.slice();
    nint i = bytes.Index(b, sosMarker);
    if (i < 0) {
        Ꮡt.Fatal(sosMarkerNotFoundˢ);
    }
    i += len(sosMarker);
    nint j = i + 10;
    if (j > len(b)) {
        j = len(b);
    }
    for (; i < j; i++) {
        Decode(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b.slice(0, i))));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string allˢ = "all"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ = ((@string)(new byte[]{0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xff, 0xdb, 0x00, 0x43, 0x00, 0x10, 0x0b, 0x0c, 0x0e, 0x0c, 0x0a, 0x10, 0x0e, 0x89, 0x0e, 0x12, 0x11, 0x10, 0x13, 0x18, 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xff, 0xdb, 0x00, 0x43, 0x00, 0x10, 0x0b, 0x0c, 0x0e, 0x0c, 0x0a, 0x10, 0x0e, 0x0d, 0x0e, 0x12, 0x11, 0x10, 0x13, 0x18, 0x28, 0x1a, 0x18, 0x16, 0x16, 0x18, 0x31, 0x23, 0x25, 0x1d, 0x28, 0x3a, 0x33, 0x3d, 0x3c, 0x39, 0x33, 0x38, 0x37, 0x40, 0x48, 0x5c, 0x4e, 0x40, 0x44, 0x57, 0x45, 0x37, 0x38, 0x50, 0x6d, 0x51, 0x57, 0x5f, 0x62, 0x67, 0x68, 0x67, 0x3e, 0x4d, 0x71, 0x79, 0x70, 0x64, 0x78, 0x5c, 0x65, 0x67, 0x63, 0xff, 0xc0, 0x00, 0x0b, 0x08, 0x20, 0x00, 0x20, 0x00, 0x01, 0x01, 0x11, 0x00, 0xff, 0xc4, 0x00, 0x1f, 0x00, 0x00, 0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0xff, 0xc4, 0x00, 0xb5, 0x10, 0x00, 0x02, 0x01, 0x03, 0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7d, 0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x01, 0x06, 0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08, 0x23, 0xd8, 0xff, 0xdd, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0, 0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x00, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xff, 0xdb, 0x00, 0x43, 0x00, 0x10, 0x0b, 0x0c, 0x0e, 0x0c, 0x0a, 0x10, 0x0e, 0x0d, 0x0e, 0x12, 0x11, 0x10, 0x13, 0x18, 0x28, 0x1a, 0x18, 0x16, 0x16, 0x18, 0x31, 0x23, 0x25, 0x1d, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa, 0xff, 0xda, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3f, 0x00, 0xb9, 0xeb, 0x50, 0xb0, 0xdb, 0xc8, 0xa8, 0xe4, 0x63, 0x80, 0xdd, 0x31, 0xd6, 0x9d, 0xbb, 0xf2, 0xc5, 0x42, 0x1f, 0x6c, 0x6f, 0xf4, 0x34, 0xdd, 0x3c, 0xfc, 0xac, 0xe7, 0x3d, 0x80, 0xa9, 0xcc, 0x87, 0x34, 0xb3, 0x37, 0xfa, 0x2b, 0x9f, 0x6a, 0xad, 0x63, 0x20, 0x36, 0x9f, 0x78, 0x64, 0x75, 0xe6, 0xab, 0x7d, 0xb2, 0xde, 0x29, 0x70, 0xd3, 0x20, 0x27, 0xde, 0xaf, 0xa4, 0xf0, 0xca, 0x9f, 0x24, 0xa8, 0xdf, 0x46, 0xa8, 0x24, 0x84, 0x96, 0xe3, 0x77, 0xf9, 0x2e, 0xe0, 0x0a, 0x62, 0x7f, 0xdf, 0xd9}));

public static void TestLargeImageWithShortData(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // This input is an invalid JPEG image, based on the fuzzer-generated image
        // in issue 10413. It is only 504 bytes, and shouldn't take long for Decode
        // to return an error. The Start Of Frame marker gives the image dimensions
        // as 8192 wide and 8192 high, so even if an unreadByteStuffedByte bug
        // doesn't technically lead to an infinite loop, such a bug can still cause
        // an unreasonably long loop for such a short input.
        @string input = inputᶜ;
        var timer = time.AfterFunc((time.Duration)(30000000000L), () => {
            debug.SetTraceback(allˢ);
            throw panic("TestLargeImageWithShortData stuck in Decode");
        });
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        var (_, err) = Decode(new jpeg_internal_test_package.strings_ReaderжReader(strings.NewReader(input)));
        if (err == default!) {
            Ꮡt.Fatalf("got nil error, want non-nil"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string base64EncodedImageᶜ = """

/9j/4AAhQVZJMQABAQEAeAB4AAAAAAAAAAAAAAAAAAAAAAAAAP/bAEMABAIDAwMCBAMDAwQEBAQGCgYG
BQUGDAgJBwoODA8PDgwODxASFxMQERURDQ4UGhQVFxgZGhkPExweHBkeFxkZGP/bAEMBBAQEBgUGCwYG
CxgQDhAYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGP/EAaIA
AAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKCxAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1Fh
ByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNk
ZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT
1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6AQADAQEBAQEBAQEBAAAAAAAAAQIDBAUGBwgJCgsRAAIB
AgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBka
JicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZ
mqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/dAAQA
Cv/gAAQAAP/AABEIALABQAMBIQACEQEDEQH/2gAMAwEAAhEDEQA/APnCFTk5BPPGKliAB718W7H2j3Ip
VUuwJxzTfKXacde9VBhYRUBAyO3pTmUAbSMU5WGmybywzHGAMdelPVFC+n1qXZCuyaJADxjj2qzbBMAP
xz1rKaVib6ltLcFvlIx2pLy0dwuAMMBnH1rFON9RNsszAZPFEYHldPzrOy3KewmBk9qUABugxjtTVmiW
xWRcjp+VJtHXgVL3K6AgBDdM9eRTNzAZViOe1VyxaJavuf/Q8aW4mUcSGpo764AyHz+FfnnJBvVH1UsN
CS1Q/wDte4Trip49ecA7g3FSqMW9zlqZandxJ4/EKADcSPqKni8QQMT865qOSUNjiqZdNbFiHWYXz84N
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
Sqm7JH//2Q==

"""u8;

public static void TestPaddedRSTMarker(ж<testing.T> Ꮡt) {
    // This test image comes from golang.org/issue/28717
    @string base64EncodedImage = base64EncodedImageᶜ;
    var (data, err) = base64.StdEncoding.DecodeString(base64EncodedImage);
    if (err != default!) {
        Ꮡt.Fatalf("base64 DecodeString: %v"u8, err);
    }
    {
        (_, err) = Decode(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data))); if (err != default!) {
            Ꮡt.Fatalf("Decode: %v"u8, err);
        }
    }
}

public static void TestExtraneousData(ж<testing.T> Ꮡt) {
    // Encode a 1x1 red image.
    var src = image.NewRGBA(image.Rect(0, 0, 1, 1));
    src.Set(0, 0, new colorꓸRGBA(0xff, 0x00, 0x00, 0xff));
    var buf = @new<bytes.Buffer>();
    {
        var err = Encode(new jpeg_internal_test_package.bytes_BufferжWriter(buf), new image.ΔRGBAжImage(src), nil); if (err != default!) {
            Ꮡt.Fatalf("encode: %v"u8, err);
        }
    }
    @string enc = buf.String();
    // Sanity check that the encoded JPEG is long enough, that it ends in a
    // "\xff\xd9" EOI marker, and that it contains a "\xff\xda" SOS marker
    // somewhere in the final 64 bytes.
    if (len(enc) < 64) {
        Ꮡt.Fatalf("encoded JPEG is too short: %d bytes"u8, len(enc));
    }
    {
        @string got = enc.slice(len(enc) - 2);
        @string want = ((@string)(new byte[]{0xff, 0xd9})); if (got != want) {
            Ꮡt.Fatalf("encoded JPEG ends with %q, want %q"u8, got, want);
        }
    }
    {
        @string s = enc.slice(len(enc) - 64); if (!strings.Contains(s, ((@string)(new byte[]{0xff, 0xda})))) {
            Ꮡt.Fatalf("encoded JPEG does not contain a SOS marker (ff da) near the end: % x"u8, s);
        }
    }
    // Test that adding some random junk between the SOS marker and the
    // EOI marker does not affect the decoding.
    var rnd = rand.New(rand.NewSource(1));
    for ((nint i, nint nerr) = (0, 0); i < 1000 && nerr < 10; i++) {
        buf.Reset();
        // Write all but the trailing "\xff\xd9" EOI marker.
        buf.WriteString(enc.slice(0, len(enc) - 2));
        // Write some random extraneous data.
        for (nint n = rnd.Intn(10); n > 0; n--) {
            {
                var x = (byte)rnd.Intn(256); if (x != 0xff){
                    buf.WriteByte(x);
                } else {
                    // The JPEG format escapes a SOS 0xff data byte as "\xff\x00".
                    buf.WriteString(((@string)(new byte[]{0xff, 0x00})));
                }
            }
        }
        // Write the "\xff\xd9" EOI marker.
        buf.WriteString(((@string)(new byte[]{0xff, 0xd9})));
        // Check that we can still decode the resultant image.
        var (got, err) = Decode(new jpeg_internal_test_package.bytes_BufferжReader(buf));
        if (err != default!) {
            Ꮡt.Errorf("could not decode image #%d: %v"u8, i, err);
            nerr++;
            continue;
        }
        if (got.Bounds() != src.Bounds()) {
            Ꮡt.Errorf("image #%d, bounds differ: %v and %v"u8, i, got.Bounds(), src.Bounds());
            nerr++;
            continue;
        }
        if (averageDelta(got, new image.ΔRGBAжImage(src)) > ((int64)2 << (int)(8))) {
            Ꮡt.Errorf("image #%d changed too much after a round trip"u8, i);
            nerr++;
            continue;
        }
    }
}

public static void TestIssue56724(ж<testing.T> Ꮡt) {
    var (b, err) = os.ReadFile(testdataVideo001Jpegˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    b = b[..24]; // truncate image data
    (_, err) = Decode(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
    if (!AreEqual(err, io.ErrUnexpectedEOF)) {
        Ꮡt.Errorf("got: %v, want: %v"u8, err, io.ErrUnexpectedEOF);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001Restart2ˢ = "../testdata/video-001.restart2.jpeg"u8;
internal static readonly object testImageHadUnexpectedˢ = (@string)"test image had unexpected length"u8;
internal static readonly object testImageDidNotHaveFfD1ˢ = (@string)"test image did not have FF D1 restart marker at expected offset"u8;

public static void TestBadRestartMarker(ж<testing.T> Ꮡt) {
    var (b, err) = os.ReadFile(testdataVideo001Restart2ˢ);
    if (err != default!){
        Ꮡt.Fatal(err);
    } else 
    if (len(b) != 4855){
        Ꮡt.Fatal(testImageHadUnexpectedˢ);
    } else 
    if ((b[2816] != 0xff) || (b[2817] != 0xd1)) {
        Ꮡt.Fatal(testImageDidNotHaveFfD1ˢ);
    }
    var (prefix, suffix) = (b[..2816], b[2816..]);
    var testCases = new @string[]{
        "PASS:"u8,
        "PASS:\x00"u8,
        "PASS:\x61"u8,
        ((@string)(new byte[]{0x50, 0x41, 0x53, 0x53, 0x3a, 0x61, 0x62, 0x63, 0xff, 0x00, 0x64})),
        ((@string)(new byte[]{0x50, 0x41, 0x53, 0x53, 0x3a, 0xff})),
        ((@string)(new byte[]{0x50, 0x41, 0x53, 0x53, 0x3a, 0xff, 0x00})),
        ((@string)(new byte[]{0x50, 0x41, 0x53, 0x53, 0x3a, 0xff, 0xff, 0xff, 0x00, 0xff, 0x00, 0x00, 0xff, 0xff, 0xff})),
        ((@string)(new byte[]{0x46, 0x41, 0x49, 0x4c, 0x3a, 0xff, 0x03})),
        ((@string)(new byte[]{0x46, 0x41, 0x49, 0x4c, 0x3a, 0xff, 0xd5})),
        ((@string)(new byte[]{0x46, 0x41, 0x49, 0x4c, 0x3a, 0xff, 0xff, 0xd5}))
    }.slice();
    foreach (var (_, tc) in testCases) {
        var want = tc[..5] == "PASS:";
        @string infix = tc[5..];
        var data = slice<byte>(default!);
        data = appendꓸꓸꓸ(data, prefix);
        data = append(data, infix.ꓸꓸꓸ);
        data = appendꓸꓸꓸ(data, suffix);
        var (_, errΔ1) = Decode(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
        var got = errΔ1 == default!;
        if (got != want) {
            Ꮡt.Errorf("%q: got %v, want %v"u8, tc, got, want);
        }
    }
}

internal static void benchmarkDecode(ж<testing.B> Ꮡb, @string filename) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (data, err) = os.ReadFile(filename);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    (var cfg, err) = DecodeConfig(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    b.SetBytes((int64)(cfg.Width * cfg.Height * 4));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Decode(new jpeg_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    }
}

public static void BenchmarkDecodeBaseline(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataVideo001Jpegˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001ˢ = "../testdata/video-001.progressive.jpeg"u8;

public static void BenchmarkDecodeProgressive(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataVideo001ˢ);
}

} // end jpeg_internal_test_package
