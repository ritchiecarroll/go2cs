// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using flag = flag_package;
using fmt = fmt_package;
using static go.@internal.strconv_package;
using io = io_package;
using http = net.http_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using fs = go.io.fs_package;
using iter = iter_package;
using net;
using static go.@internal.strconv_internal_test_package;

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

// Wrapper around ParseFloat(x, 64).  Handles dddddp+ddd (binary exponent)
// itself, passes the rest on to ParseFloat.
internal static (float64 f, bool ok) myatof64(@string s) {
    {
        var (mant, exp, okΔ1) = strings.Cut(s, "p"u8); if (okΔ1) {
            var (n, errΔ1) = ParseInt(mant, 10, 64);
            if (errΔ1 != default!) {
                return (0D, false);
            }
            var (e, err1) = Atoi(exp);
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
    var (f1, err) = ParseFloat(s, 64);
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
            var (n, err) = Atoi(mant);
            if (err != default!) {
                println((@string)"bad n"u8, mant);
                return (0F, false);
            }
            var (e, err1Δ1) = Atoi(exp);
            if (err1Δ1 != default!) {
                println((@string)"bad p"u8, exp);
                return (0F, false);
            }
            return ((float32)((float64)n * pow2(e)), true);
        }
    }
    var (f64, err1) = ParseFloat(s, 32);
    var f1 = (float32)f64;
    if (err1 != default!) {
        return (0F, false);
    }
    return (f1, true);
}

//go:embed testdata/testfp.txt
internal static @string testfp = go.embed_package.ΔEmbedString(typeof(strconv_test_package).Assembly, "go.embed/internal/strconv_test/", "testdata/testfp.txt");

public static void TestFp(ж<testing.T> Ꮡt) {
    nint lineno = 0;
    foreach (var iᴛ1 in range<@string>(strings.Lines(testfp).Invoke)) {
        var line = iᴛ1;
        lineno++;
        (line, _, _) = strings.Cut(line, "#"u8);
        line = strings.TrimSpace(line);
        if (line == ""u8) {
            continue;
        }
        var a = strings.Split(line, " "u8);
        if (len(a) != 4) {
            Ꮡt.Errorf("testdata/testfp.txt:%d: wrong field count"u8, lineno);
            continue;
        }
        @string s = default!;
        float64 v = default!;
        var exprᴛ1 = a[0];
        if (exprᴛ1 == "float64"u8) {
            bool ok = default!;
            (v, ok) = myatof64(a[2]);
            if (!ok) {
                Ꮡt.Errorf("testdata/testfp.txt:%d: cannot atof64 %s"u8, lineno, a[2]);
                continue;
            }
            s = fmt.Sprintf(a[1], v);
        }
        else if (exprᴛ1 == "float32"u8) {
            var (v1, ok) = myatof32(a[2]);
            if (!ok) {
                Ꮡt.Errorf("testdata/testfp.txt:%d: cannot atof32 %s"u8, lineno, a[2]);
                continue;
            }
            s = fmt.Sprintf(a[1], v1);
            v = (float64)v1;
        }
        if (s != a[3]) {
            Ꮡt.Errorf("testdata/testfp.txt:%d: %s %s %s %s: have %s want %s"u8, lineno, a[0], a[1], a[2], a[3], s, a[3]);
        }
    }
}

// The -testbase flag runs the full testbase input set instead of the
// random sample in testdata/*1k.txt. See testdata/README for details.
internal static ж<bool> testbase = flag.Bool("testbase"u8, false, "download and test full testbase testdata"u8);

// testbaseURL is the URL for downloading the full testbase testdata.
// There is also a copy on "https://swtch.com/testbase/".
internal static @string testbaseURL = "https://gist.githubusercontent.com/rsc/606b378b0bf95c24a6fd6cef99e262e1/raw/128a03890e536bdf403e6cc768b0737405c6734d/"u8;

//go:embed testdata/atof1k.txt
internal static @string atof1ktxt = go.embed_package.ΔEmbedString(typeof(strconv_test_package).Assembly, "go.embed/internal/strconv_test/", "testdata/atof1k.txt");

//go:embed testdata/ftoa1k.txt
internal static @string ftoa1ktxt = go.embed_package.ΔEmbedString(typeof(strconv_test_package).Assembly, "go.embed/internal/strconv_test/", "testdata/ftoa1k.txt");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataAtof1kTxtˢ = "testdata/atof1k.txt"u8;
internal static readonly @string testdataFtoa1kTxtˢ = "testdata/ftoa1k.txt"u8;

// openTestbase opens the named testbase data file.
// By default it opens testdata/name1k.txt,
// but if the -testbase flag has been set,
// then it opens the full testdata/name.txt,
// downloading that file if necessary.
internal static (@string @file, @string data) openTestbase(ж<testing.T> Ꮡt, @string name) {
    @string @file = default!;

    if (!testbase.Value) {
        var exprᴛ1 = name;
        if (exprᴛ1 == "atof"u8) {
            return (testdataAtof1kTxtˢ, atof1ktxt);
        }
        if (exprᴛ1 == "ftoa"u8) {
            return (testdataFtoa1kTxtˢ, ftoa1ktxt);
        }
        Ꮡt.Fatalf("unknown file %s"u8, name);
    }

    // Use cached copy if present.
    @file = "testdata/"u8 + name + ".txt"u8;
    {
        var (dataΔ1, errΔ1) = os.ReadFile(@file); if (errΔ1 == default!) {
            return (@file, (@string)dataΔ1);
        }
    }

    // Download copy.
    @string url = testbaseURL + name + ".txt"u8;
    var (resp, err) = http.Get(url);
    if (err != default!) {
        Ꮡt.Fatalf("%s: %s"u8, url, err);
    }
    if ((~resp).StatusCode != 200) {
        Ꮡt.Fatalf("%s: %s"u8, url, (~resp).Status);
    }
    (var bytes, err) = io.ReadAll(new strconv_test_package.io_ReadCloserᴠReader((~resp).Body));
    (~resp).Body.Close();
    if (err != default!) {
        Ꮡt.Fatalf("%s: %s"u8, url, err);
    }
    {
        var errΔ2 = os.WriteFile(@file, bytes, 438); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    return (@file, (@string)bytes);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string atofˢ = "atof"u8;

public static void TestParseFloatTestdata(ж<testing.T> Ꮡt) {
    // Test testbase inputs, optimized against not.
    var (name, data) = openTestbase(Ꮡt, atofˢ);
    nint fail = 0;
    nint lineno = 0;
    foreach (var line in range<@string>(strings.Lines(data).Invoke)) {
        lineno++;
        @string s = strings.TrimSpace(line);
        if (strings.HasPrefix(s, "#"u8) || s == ""u8) {
            continue;
        }
        strconv_internal_test_package.SetOptimize(false);
        var (want, err1) = ParseFloat(s, 64);
        strconv_internal_test_package.SetOptimize(true);
        var (have, err2) = ParseFloat(s, 64);
        if (err1 != default!) {
            // Error in test data; should not happen.
            Ꮡt.Errorf("%s:%d: ParseFloat(%#q): %v"u8, name, lineno, s, err1);
            continue;
        }
        if (err2 != default!) {
            Ꮡt.Errorf("ParseFloat(%#q): %v"u8, s, err2);
            {
                fail++; if (fail > 100) {
                    Ꮡt.Fatalf("too many failures"u8);
                }
            }
            continue;
        }
        if (have != want) {
            Ꮡt.Errorf("ParseFloat(%#q) = %#x, want %#x"u8, s, have, want);
            {
                fail++; if (fail > 100) {
                    Ꮡt.Fatalf("too many failures"u8);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ftoaˢ = "ftoa"u8;

public static void TestFormatFloatTestdata(ж<testing.T> Ꮡt) {
    // Test testbase inputs, optimized against not.
    var (name, data) = openTestbase(Ꮡt, ftoaˢ);
    nint fail = 0;
    nint lineno = 0;
    foreach (var line in range<@string>(strings.Lines(data).Invoke)) {
        lineno++;
        @string s = strings.TrimSpace(line);
        if (strings.HasPrefix(s, "#"u8) || s == ""u8) {
            continue;
        }
        var (f, err) = ParseFloat(s, 64);
        if (err != default!) {
            // Error in test data; should not happen.
            Ꮡt.Errorf("%s:%d: ParseFloat(%#q): %v"u8, name, lineno, s, err);
            continue;
        }
        foreach (var i in range(19)) {
            strconv_internal_test_package.SetOptimize(false);
            @string want = FormatFloat(f, (rune)'e', i, 64);
            strconv_internal_test_package.SetOptimize(true);
            @string have = FormatFloat(f, (rune)'e', i, 64);
            if (have != want) {
                Ꮡt.Errorf("FormatFloat(%#x, 'e', %d) = %s, want %s"u8, f, i, have, want);
                {
                    fail++; if (fail > 100) {
                        Ꮡt.Fatalf("too many failures"u8);
                    }
                }
            }
        }
    }
}

} // end strconv_test_package
