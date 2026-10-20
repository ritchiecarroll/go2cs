// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.strconv_internal_test_package;
using Δio = io_package;

partial class strconv_test_package {

internal static float64 pow2(nint i) {
    switch (ᐧ) {
    case {} when i is < 0: {
        return 1D / pow2(-i);
    }
    case {} when i is 0: {
        return 1D;
    }
    case {} when i is 1: {
        return 2D;
    }}
    return pow2(i / 2) * pow2(i - i / 2);
}

// Wrapper around strconv.ParseFloat(x, 64).  Handles dddddp+ddd (binary exponent)
// itself, passes the rest on to strconv.ParseFloat.
internal static (float64 f, bool ok) myatof64(@string s) {
    {
        var (mant, exp, okΔ1) = strings.Cut(s, "p"u8); if (okΔ1) {
            var (n, errΔ1) = strconv.ParseInt(mant, 10, 64);
            if (errΔ1 != default!) {
                return (0D, false);
            }
            var (e, err1) = strconv.Atoi(exp);
            if (err1 != default!) {
                println((@string)"bad e"u8, exp);
                return (0D, false);
            }
            var v = (float64)n;
            // We expect that v*pow2(e) fits in a float64,
            // but pow2(e) by itself may not. Be careful.
            if (e <= -1000) {
                v *= pow2(-1000);
                e += 1000;
                while (e < 0) {
                    v /= 2D;
                    e++;
                }
                return (v, true);
            }
            if (e >= 1000) {
                v *= pow2(1000);
                e -= 1000;
                while (e > 0) {
                    v *= 2D;
                    e--;
                }
                return (v, true);
            }
            return (v * pow2(e), true);
        }
    }
    var (f1, err) = strconv.ParseFloat(s, 64);
    if (err != default!) {
        return (0D, false);
    }
    return (f1, true);
}

// Wrapper around strconv.ParseFloat(x, 32).  Handles dddddp+ddd (binary exponent)
// itself, passes the rest on to strconv.ParseFloat.
internal static (float32 f, bool ok) myatof32(@string s) {
    {
        var (mant, exp, okΔ1) = strings.Cut(s, "p"u8); if (okΔ1) {
            var (n, err) = strconv.Atoi(mant);
            if (err != default!) {
                println((@string)"bad n"u8, mant);
                return (0F, false);
            }
            var (e, err1Δ1) = strconv.Atoi(exp);
            if (err1Δ1 != default!) {
                println((@string)"bad p"u8, exp);
                return (0F, false);
            }
            return ((float32)((float64)n * pow2(e)), true);
        }
    }
    var (f64, err1) = strconv.ParseFloat(s, 32);
    var f1 = (float32)f64;
    if (err1 != default!) {
        return (0F, false);
    }
    return (f1, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestfpTxtˢ = "testdata/testfp.txt"u8;
internal static readonly object testfpOpenTestdataTestfpˢ = (@string)"testfp: open testdata/testfp.txt:"u8;
internal static readonly object testdataTestfpTxtˢ2 = (@string)"testdata/testfp.txt:"u8;
internal static readonly object wrongFieldCountˢ = (@string)": wrong field count"u8;
internal static readonly object cannotAtof64ˢ = (@string)": cannot atof64 "u8;
internal static readonly object cannotAtof32ˢ = (@string)": cannot atof32 "u8;
internal static readonly object wantˢ = (@string)"want "u8;
internal static readonly object gotˢ = (@string)" got "u8;
internal static readonly object testfpReadTestdataTestfpˢ = (@string)"testfp: read testdata/testfp.txt: "u8;

public static void TestFp(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataTestfpTxtˢ);
        if (err != default!) {
            Ꮡt.Fatal(testfpOpenTestdataTestfpˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var s = bufio.NewScanner(new strconv_test_package.os_FileжReader(f));

        for (nint lineno = 1; s.Scan(); lineno++) {
            @string line = s.Text();
            if (len(line) == 0 || line[0] == (rune)'#') {
                continue;
            }
            var a = strings.Split(line, " "u8);
            if (len(a) != 4) {
                Ꮡt.Error(testdataTestfpTxtˢ2, lineno, wrongFieldCountˢ);
                continue;
            }
            @string sΔ1 = default!;
            float64 v = default!;
            var exprᴛ1 = a[0];
            if (exprᴛ1 == "float64"u8) {
                bool ok = default!;
                (v, ok) = myatof64(a[2]);
                if (!ok) {
                    Ꮡt.Error(testdataTestfpTxtˢ2, lineno, cannotAtof64ˢ, a[2]);
                    continue;
                }
                sΔ1 = fmt.Sprintf(a[1], v);
            }
            else if (exprᴛ1 == "float32"u8) {
                var (v1, ok) = myatof32(a[2]);
                if (!ok) {
                    Ꮡt.Error(testdataTestfpTxtˢ2, lineno, cannotAtof32ˢ, a[2]);
                    continue;
                }
                sΔ1 = fmt.Sprintf(a[1], v1);
                v = (float64)v1;
            }
            if (sΔ1 != a[3]) {
                Ꮡt.Error(testdataTestfpTxtˢ2, lineno, (@string)": "u8, a[0], (@string)" "u8, a[1], (@string)" "u8, a[2], (@string)" ("u8, v, (@string)") "u8,
                    wantˢ, a[3], gotˢ, sΔ1);
            }
        }
        if (s.Err() != default!) {
            Ꮡt.Fatal(testfpReadTestdataTestfpˢ, s.Err());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end strconv_test_package
