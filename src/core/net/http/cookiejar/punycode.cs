// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

// This file implements the Punycode algorithm from RFC 3492.
using fmt = fmt_package;
using ascii = go.net.http.@internal.ascii_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using go.net.http.@internal;
using unicode;

partial class cookiejar_package {

// These parameter values are specified in section 5.
//
// All computation is done with int32s, so that overflow behavior is identical
// regardless of whether int is 32-bit or 64-bit.
internal const int32 @base = 36;

internal const int32 damp = 700;

internal const int32 initialBias = 72;

internal const int32 initialN = 128;

internal const int32 skew = 38;

internal const int32 tmax = 26;

internal const int32 tmin = 1;

// encode encodes a string as specified in section 6.3 and prepends prefix to
// the result.
//
// The "while h < length(input)" line in the specification becomes "for
// remaining != 0" in the Go code, because len(s) in Go is in bytes, not runes.
internal static (@string, error) encode(@string prefix, @string s) {
    var output = new slice<byte>(len(prefix), len(prefix) + 1 + 2 * len(s));
    copy(output, prefix);
    var (delta, n, bias) = ((int32)0, initialN, initialBias);
    var (b, remaining) = ((int32)0, (int32)0);
    foreach (var (_, r) in s) {
        if (r < utf8.RuneSelf){
            b++;
            output = append(output, (byte)r);
        } else {
            remaining++;
        }
    }
    var h = b;
    if (b > 0) {
        output = append(output, (byte)((rune)'-'));
    }
    while (remaining != 0) {
        var m = (int32)0x7fffffff;
        foreach (var (_, r) in s) {
            if (m > r && r >= n) {
                m = r;
            }
        }
        delta += (m - n) * (h + 1);
        if (delta < 0) {
            return ("", fmt.Errorf("cookiejar: invalid label %q"u8, s));
        }
        n = m;
        foreach (var (_, r) in s) {
            if (r < n) {
                delta++;
                if (delta < 0) {
                    return ("", fmt.Errorf("cookiejar: invalid label %q"u8, s));
                }
                continue;
            }
            if (r > n) {
                continue;
            }
            var q = delta;
            for (var k = @base; ᐧ ; k += @base) {
                var t = k - bias;
                if (t < tmin){
                    t = tmin;
                } else 
                if (t > tmax) {
                    t = tmax;
                }
                if (q < t) {
                    break;
                }
                output = append(output, encodeDigit(t + rem((q - t), (@base - t))));
                q = quo((q - t), (@base - t));
            }
            output = append(output, encodeDigit(q));
            bias = adapt(delta, h + 1, h == b);
            delta = 0;
            h++;
            remaining--;
        }
        delta++;
        n++;
    }
    return (((@string)output), default!);
}

internal static byte encodeDigit(int32 digit) {
    switch (ᐧ) {
    case {} when 0 <= digit && digit < 26: {
        return (byte)(digit + (rune)'a');
    }
    case {} when 26 <= digit && digit < 36: {
        return (byte)(digit + ((rune)'0' - 26));
    }}

    throw panic("cookiejar: internal error in punycode encoding");
}

// adapt is the bias adaptation function specified in section 6.1.
internal static int32 adapt(int32 delta, int32 numPoints, bool firstTime) {
    if (firstTime){
        delta /= damp;
    } else {
        delta /= 2;
    }
    delta += quo(delta, numPoints);
    var k = (int32)0;
    while (delta > ((@base - tmin) * tmax) / 2) {
        delta /= @base - tmin;
        k += @base;
    }
    return k + quo((@base - tmin + 1) * delta, (delta + skew));
}

// Strictly speaking, the remaining code below deals with IDNA (RFC 5890 and
// friends) and not Punycode (RFC 3492) per se.

// acePrefix is the ASCII Compatible Encoding prefix.
internal static readonly @string acePrefix = "xn--"u8;

// toASCII converts a domain or domain label to its ASCII form. For example,
// toASCII("bücher.example.com") is "xn--bcher-kva.example.com", and
// toASCII("golang") is "golang".
internal static (@string, error) toASCII(@string s) {
    if (ascii.Is(s)) {
        return (s, default!);
    }
    var labels = strings.Split(s, "."u8);
    foreach (var (i, label) in labels) {
        if (!ascii.Is(label)) {
            var (a, err) = encode(acePrefix, label);
            if (err != default!) {
                return ("", err);
            }
            labels[i] = a;
        }
    }
    return (strings.Join(labels, "."u8), default!);
}

} // end cookiejar_package
