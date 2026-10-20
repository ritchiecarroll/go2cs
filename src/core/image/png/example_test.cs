// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using base64 = encoding.base64_package;
using fmt = fmt_package;
using image = image_package;
using color = go.image.color_package;
using png = go.image.png_package;
using io = io_package;
using log = log_package;
using os = os_package;
using strings = strings_package;
using encoding;
using go.image;
using static go.image.png_internal_test_package;

partial class png_test_package {

internal static readonly @string gopher = @"iVBORw0KGgoAAAANSUhEUgAAAEsAAAA8CAAAAAALAhhPAAAFfUlEQVRYw62XeWwUVRzHf2+OPbo9d7tsWyiyaZti6eWGAhISoIGKECEKCAiJJkYTiUgTMYSIosYYBBIUIxoSPIINEBDi2VhwkQrVsj1ESgu9doHWdrul7ba73WNm3vOPtsseM9MdwvvrzTs+8/t95ze/33sI5BqiabU6m9En8oNjduLnAEDLUsQXFF8tQ5oxK3vmnNmDSMtrncks9Hhtt/qeWZapHb1ha3UqYSWVl2ZmpWgaXMXGohQAvmeop3bjTRtv6SgaK/Pb9/bFzUrYslbFAmHPp+3WhAYdr+7GN/YnpN46Opv55VDsJkoEpMrY/vO2BIYQ6LLvm0ThY3MzDzzeSJeeWNyTkgnIE5ePKsvKlcg/0T9QMzXalwXMlj54z4c0rh/mzEfr+FgWEz2w6uk8dkzFAgcARAgNp1ZYef8bH2AgvuStbc2/i6CiWGj98y2tw2l4FAXKkQBIf+exyRnteY83LfEwDQAYCoK+P6bxkZm/0966LxcAAILHB56kgD95PPxltuYcMtFTWw/FKkY/6Opf3GGd9ZF+Qp6mzJxzuRSractOmJrH1u8XTvWFHINNkLQLMR+XHXvfPPHw967raE1xxwtA36IMRfkAAG29/7mLuQcb2WOnsJReZGfpiHsSBX81cvMKywYZHhX5hFPtOqPGWZCXnhWGAu6lX91ElKXSalcLXu3UaOXVay57ZSe5f6Gpx7J2MXAsi7EqSp09b/MirKSyJfnfEEgeDjl8FgDAfvewP03zZ+AJ0m9aFRM8eEHBDRKjfcreDXnZdQuAxXpT2NRJ7xl3UkLBhuVGU16gZiGOgZmrSbRdqkILuL/yYoSXHHkl9KXgqNu3PB8oRg0geC5vFmLjad6mUyTKLmF3OtraWDIfACyXqmephaDABawfpi6tqqBZytfQMqOz6S09iWXhktrRaB8Xz4Yi/8gyABDm5NVe6qq/3VzPrcjELWrebVuyY2T7ar4zQyybUCtsQ5Es1FGaZVrRVQwAgHGW2ZCRZshI5bGQi7HesyE972pOSeMM0dSktlzxRdrlqb3Osa6CCS8IJoQQQgBAbTAa5l5epO34rJszibJI8rxLfGzcp1dRosutGeb2VDNgqYrwTiPNsLxXiPi3dz7LiS1WBRBDBOnqEjyy3aQb+/bLiJzz9dIkscVBBLxMfSEac7kO4Fpkngi0ruNBeSOal+u8jgOuqPz12nryMLCniEjtOOOmpt+KEIqsEdocJjYXwrh9OZqWJQyPCTo67LNS/TdxLAv6R5ZNK9npEjbYdT33gRo4o5oTqR34R+OmaSzDBWsAIPhuRcgyoteNi9gF0KzNYWVItPf2TLoXEg+7isNC7uJkgo1iQWOfRSP9NR11RtbZZ3OMG/VhL6jvx+J1m87+RCfJChAtEBQkSBX2PnSiihc/Twh3j0h7qdYQAoRVsRGmq7HU2QRbaxVGa1D6nIOqaIWRjyRZpHMQKWKpZM5feA+lzC4ZFultV8S6T0mzQGhQohi5I8iw+CsqBSxhFMuwyLgSwbghGb0AiIKkSDmGZVmJSiKihsiyOAUs70UkywooYP0bii9GdH4sfr1UNysd3fUyLLMQN+rsmo3grHl9VNJHbbwxoa47Vw5gupIqrZcjPh9R4Nye3nRDk199V+aetmvVtDRE8/+cbgAAgMIWGb3UA0MGLE9SCbWX670TDy1y98c3D27eppUjsZ6fql3jcd5rUe7+ZIlLNQny3Rd+E5Tct3WVhTM5RBCEdiEK0b6B+/ca2gYU393nFj/n1AygRQxPIUA043M42u85+z2SnssKrPl8Mx76NL3E6eXc3be7OD+H4WHbJkKI8AU8irbITQjZ+0hQcPEgId/Fn/pl9crKH02+5o2b9T/eMx7pKoskYgAAAABJRU5ErkJggg=="u8;

// gopherPNG creates an io.Reader by decoding the base64 encoded image data string in the gopher constant.
internal static io.Reader gopherPNG() {
    return base64.NewDecoder(base64.StdEncoding, new png_test_package.strings_ReaderжReader(strings.NewReader(gopher)));
}

public static void ExampleDecode() {
    // This example uses png.Decode which can only decode PNG images.
    // Consider using the general image.Decode as it can sniff and decode any registered image format.
    var (img, err) = png.Decode(gopherPNG());
    if (err != default!) {
        log.Fatal(err);
    }

    var levels = new @string[]{" "u8, "░"u8, "▒"u8, "▓"u8, "█"u8}.slice();

    for (nint y = img.Bounds().Min.Y; y < img.Bounds().Max.Y; y++) {
        for (nint x = img.Bounds().Min.X; x < img.Bounds().Max.X; x++) {
            var c = color.GrayModel.Convert(img.At(x, y))._<color.Gray>();
            var level = (uint8)(c.Y / 51); // 51 * 5 = 255
            if (level == 5) {
                level--;
            }
            fmt.Print(levels[level]);
        }
        fmt.Print((@string)"\n"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string imagePngˢ = "image.png"u8;

public static void ExampleEncode() {
    const nint width = 256;
    const nint height = 256;

    // Create a colored image of the given width and height.
    var img = image.NewNRGBA(image.Rect(0, 0, width, height));

    for (nint y = 0; y < height; y++) {
        for (nint x = 0; x < width; x++) {
            img.Set(x, y, new color.NRGBA(
                R: (uint8)((x + y) & 255),
                G: (uint8)((x + y) << 1 & 255),
                B: (uint8)((x + y) << 2 & 255),
                A: 255
            ));
        }
    }

    var (f, err) = os.Create(imagePngˢ);
    if (err != default!) {
        log.Fatal(err);
    }

    {
        var errΔ1 = png.Encode(new os.FileжWriter(f), new image.NRGBAжImage(img)); if (errΔ1 != default!) {
            f.Close();
            log.Fatal(errΔ1);
        }
    }

    {
        var errΔ2 = f.Close(); if (errΔ2 != default!) {
            log.Fatal(errΔ2);
        }
    }
}

} // end png_test_package
