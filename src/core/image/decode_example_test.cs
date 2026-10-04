// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This example demonstrates decoding a JPEG image and examining its pixels.
namespace go;

using base64 = encoding.base64_package;
using fmt = fmt_package;
using Δimage = image_package;
using Δlog = log_package;
using strings = strings_package;
// blank import: go.image.jpeg_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using color = go.image.color_package;
using encoding;
using go.image;
using static go.image_internal_test_package;
using Δio = io_package;

partial class image_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object widthˢ = (@string)"Width:"u8;
internal static readonly object heightˢ = (@string)"Height:"u8;
internal static readonly object formatˢ = (@string)"Format:"u8;

public static void Example_decodeConfig() {
    var reader = base64.NewDecoder(base64.StdEncoding, new image_test_package.strings_ReaderжReader(strings.NewReader(data)));
    var (config, format, err) = Δimage.DecodeConfig(reader);
    if (err != default!) {
        Δlog.Fatal(err);
    }
    fmt.Println(widthˢ, config.Width, heightˢ, config.Height, formatˢ, format);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object binˢ = (@string)"bin"u8;
internal static readonly object redˢ = (@string)"red"u8;
internal static readonly object greenˢ = (@string)"green"u8;
internal static readonly object blueˢ = (@string)"blue"u8;
internal static readonly object alphaˢ = (@string)"alpha"u8;

public static void Example() {
    // Decode the JPEG data. If reading from file, create a reader with
    //
    // reader, err := os.Open("testdata/video-001.q50.420.jpeg")
    // if err != nil {
    //     log.Fatal(err)
    // }
    // defer reader.Close()
    var reader = base64.NewDecoder(base64.StdEncoding, new image_test_package.strings_ReaderжReader(strings.NewReader(data)));
    var (m, _, err) = Δimage.Decode(reader);
    if (err != default!) {
        Δlog.Fatal(err);
    }
    var bounds = m.Bounds();
    // Calculate a 16-bin histogram for m's red, green, blue and alpha components.
    //
    // An image's bounds do not necessarily start at (0, 0), so the two loops start
    // at bounds.Min.Y and bounds.Min.X. Looping over Y first and X second is more
    // likely to result in better memory access patterns than X first and Y second.
    array<array<nint>> histogram = new(16, () => new(4));
    for (nint y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
            var (r, g, b, a) = m.At(x, y).RGBA();
            // A color's RGBA method returns values in the range [0, 65535].
            // Shifting by 12 reduces this to the range [0, 15].
            histogram[(r >> (int)(12))][0]++;
            histogram[(g >> (int)(12))][1]++;
            histogram[(b >> (int)(12))][2]++;
            histogram[(a >> (int)(12))][3]++;
        }
    }
    // Print the results.
    fmt.Printf("%-14s %6s %6s %6s %6s\n"u8, binˢ, redˢ, greenˢ, blueˢ, alphaˢ);
    foreach (var (i, vᴛ1) in histogram.ΔRangeSnapshot()) {
        var x = vᴛ1.Clone();

        fmt.Printf("0x%04x-0x%04x: %6d %6d %6d %6d\n"u8, (i << (int)(12)), ((i + 1) << (int)(12)) - 1, x[0], x[1], x[2], x[3]);
    }
}

// Output:
// bin               red  green   blue  alpha
// 0x0000-0x0fff:    364    790   7242      0
// 0x1000-0x1fff:    645   2967   1039      0
// 0x2000-0x2fff:   1072   2299    979      0
// 0x3000-0x3fff:    820   2266    980      0
// 0x4000-0x4fff:    537   1305    541      0
// 0x5000-0x5fff:    319    962    261      0
// 0x6000-0x6fff:    322    375    177      0
// 0x7000-0x7fff:    601    279    214      0
// 0x8000-0x8fff:   3478    227    273      0
// 0x9000-0x9fff:   2260    234    329      0
// 0xa000-0xafff:    921    282    373      0
// 0xb000-0xbfff:    321    335    397      0
// 0xc000-0xcfff:    229    388    298      0
// 0xd000-0xdfff:    260    414    277      0
// 0xe000-0xefff:    516    428    298      0
// 0xf000-0xffff:   2785   1899   1772  15450
internal static readonly @string data = """

/9j/4AAQSkZJRgABAQIAHAAcAAD/2wBDABALDA4MChAODQ4SERATGCgaGBYWGDEjJR0oOjM9PDkzODdA
SFxOQERXRTc4UG1RV19iZ2hnPk1xeXBkeFxlZ2P/2wBDARESEhgVGC8aGi9jQjhCY2NjY2NjY2NjY2Nj
Y2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2NjY2P/wAARCABnAJYDASIAAhEBAxEB/8QA
HwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIh
MUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVW
V1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXG
x8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQF
BgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAV
YnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOE
hYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq
8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDlwKMD0pwzSiuK57QzGDxS7D6in8Y5ximnAPUfSlcq4m3ilUYp
2OKXHvRcVxnTtS7c07HNFK4DQPakC4PNOA+tOx70XAjK/So5gBGP94fzqfvUVx/qxx/EP51UXqRP4WSE
cmgjilP3jSEZqS0IO/NGDnpUiocDg/McDjvV6HTPOdVWYgsM5KcfzzQ2JySM2jp6VYu7SWzmMUwG4cgj
kMPUVBjjtTGtRu0Zopw+lFFxhinrGzuqqMsxAA9yaXFSRv5cqSEcIwYj6GpuZ30O30fSLKzhUpbpNMv3
5XGTn29BV28jt7pPLuIVljPBBFVreYx+VbqAjycgt3x14zRcNOxGyVFHQkIc/wA61exyKLbuzjdZ046d
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

"""u8;

} // end image_test_package
