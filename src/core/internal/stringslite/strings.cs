// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package stringslite implements a subset of strings,
// only using packages that may be imported by "os".
//
// Tests for these functions are in the strings package.
namespace go.@internal;

using bytealg = go.@internal.bytealg_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class stringslite_package {

public static bool HasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s.slice(0, len(prefix)) == prefix;
}

public static bool HasSuffix(@string s, @string suffix) {
    return len(s) >= len(suffix) && s.slice(len(s) - len(suffix)) == suffix;
}

public static nint IndexByte(@string s, byte c) {
    return bytealg.IndexByteString(s, c);
}

public static nint Index(@string s, @string substr) {
    nint n = len(substr);
    switch (ᐧ) {
    case {} when n is 0: {
        return 0;
    }
    case {} when n is 1: {
        return IndexByte(s, substr[0]);
    }
    case {} when n == len(s): {
        if (substr == s) {
            return 0;
        }
        return -1;
    }
    case {} when n > len(s): {
        return -1;
    }
    case {} when n <= bytealg.MaxLen: {
        if (len(s) <= bytealg.MaxBruteForce) {
            // Use brute force when s and substr both are small
            return bytealg.IndexString(s, substr);
        }
        var c0Δ2 = substr[0];
        var c1Δ2 = substr[1];
        nint iΔ2 = 0;
        nint tΔ2 = len(s) - n + 1;
        nint failsΔ2 = 0;
        while (iΔ2 < tΔ2) {
            if (s[iΔ2] != c0Δ2) {
                // IndexByte is faster than bytealg.IndexString, so use it as long as
                // we're not getting lots of false positives.
                nint o = IndexByte(s.slice(iΔ2 + 1, tΔ2), c0Δ2);
                if (o < 0) {
                    return -1;
                }
                iΔ2 += o + 1;
            }
            if (s[iΔ2 + 1] == c1Δ2 && s.slice(iΔ2, iΔ2 + n) == substr) {
                return iΔ2;
            }
            failsΔ2++;
            iΔ2++;
            // Switch to bytealg.IndexString when IndexByte produces too many false positives.
            if (failsΔ2 > bytealg.Cutover(iΔ2)) {
                nint r = bytealg.IndexString(s.slice(iΔ2), substr);
                if (r >= 0) {
                    return r + iΔ2;
                }
                return -1;
            }
        }
        return -1;
    }}

    var c0 = substr[0];
    var c1 = substr[1];
    nint i = 0;
    nint t = len(s) - n + 1;
    nint fails = 0;
    while (i < t) {
        if (s[i] != c0) {
            nint o = IndexByte(s.slice(i + 1, t), c0);
            if (o < 0) {
                return -1;
            }
            i += o + 1;
        }
        if (s[i + 1] == c1 && s.slice(i, i + n) == substr) {
            return i;
        }
        i++;
        fails++;
        if (fails >= 4 + (i >> (int)(4)) && i < t) {
            // See comment in ../bytes/bytes.go.
            nint j = bytealg.IndexRabinKarp(s.slice(i), substr);
            if (j < 0) {
                return -1;
            }
            return i + j;
        }
    }
    return -1;
}

public static (@string before, @string after, bool found) Cut(@string s, @string sep) {
    {
        nint i = Index(s, sep); if (i >= 0) {
            return (s.slice(0, i), s.slice(i + len(sep)), true);
        }
    }
    return (s, "", false);
}

public static (@string after, bool found) CutPrefix(@string s, @string prefix) {
    if (!HasPrefix(s, prefix)) {
        return (s, false);
    }
    return (s.slice(len(prefix)), true);
}

public static (@string before, bool found) CutSuffix(@string s, @string suffix) {
    if (!HasSuffix(s, suffix)) {
        return (s, false);
    }
    return (s.slice(0, len(s) - len(suffix)), true);
}

public static @string TrimPrefix(@string s, @string prefix) {
    if (HasPrefix(s, prefix)) {
        return s.slice(len(prefix));
    }
    return s;
}

public static @string TrimSuffix(@string s, @string suffix) {
    if (HasSuffix(s, suffix)) {
        return s.slice(0, len(s) - len(suffix));
    }
    return s;
}

public static @string Clone(@string s) {
    if (len(s) == 0) {
        return ""u8;
    }
    var b = new slice<byte>(len(s));
    copy(b, s);
    return @unsafe.String(Ꮡ(b, 0), len(b));
}

} // end stringslite_package
