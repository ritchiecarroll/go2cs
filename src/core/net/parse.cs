// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple file i/o and string manipulation, to avoid
// depending on strconv and bufio and strings.
namespace go;

using bytealg = @internal.bytealg_package;
using Δio = io_package;
using os = os_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;

partial class net_package {

partial struct Δfile {
    internal ж<os.File> ΔΔfile;
    internal slice<byte> data;
    internal bool atEOF;
}

internal static void close(this ref Δfile f) {
    f.ΔΔfile.Close();
}

internal static (@string s, bool ok) getLineFromData(this ref Δfile f) {
    @string s = default!;
    bool ok = default!;

    var data = f.data;
    nint i = 0;
    for (i = 0; i < len(data); i++) {
        if (data[i] == (rune)'\n') {
            s = ((@string)(data.slice(0, i)));
            ok = true;
            // move data
            i++;
            nint n = len(data) - i;
            copy(data[0..], data.slice(i));
            f.data = data.slice(0, n);
            return (s, ok);
        }
    }
    if (f.atEOF && len(f.data) > 0) {
        // EOF, return all we have
        s = ((@string)data);
        f.data = f.data[0..0];
        ok = true;
    }
    return (s, ok);
}

internal static (@string s, bool ok) readLine(this ref Δfile f) {
    @string s = default!;
    bool ok = default!;

    {
        (s, ok) = f.getLineFromData(); if (ok) {
            return (s, ok);
        }
    }
    if (len(f.data) < cap(f.data)) {
        nint ln = len(f.data);
        var (n, err) = Δio.ReadFull(new os_FileжReader(f.ΔΔfile), f.data.slice(ln, cap(f.data)));
        if (n >= 0) {
            f.data = f.data.slice(0, ln + n);
        }
        if (AreEqual(err, Δio.EOF) || AreEqual(err, Δio.ErrUnexpectedEOF)) {
            f.atEOF = true;
        }
    }
    (s, ok) = f.getLineFromData();
    return (s, ok);
}

internal static (time.Time mtime, int64 size, error err) stat(this ref Δfile f) {
    error err = default!;

    (var st, err) = f.ΔΔfile.Stat();
    if (err != default!) {
        return (new time.Time(nil), 0, err);
    }
    return (st.ModTime(), st.Size(), default!);
}

internal static (ж<Δfile>, error) open(@string name) {
    var (fd, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new Δfile(fd, new slice<byte>(0, 64 * 1024), false)), default!);
}

internal static (time.Time mtime, int64 size, error err) stat(@string name) {
    error err = default!;

    (var st, err) = os.Stat(name);
    if (err != default!) {
        return (new time.Time(nil), 0, err);
    }
    return (st.ModTime(), st.Size(), default!);
}

// Count occurrences in s of any bytes in t.
internal static nint countAnyByte(@string s, @string t) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (bytealg.IndexByteString(t, s[i]) >= 0) {
            n++;
        }
    }
    return n;
}

// Split s at any bytes in t.
internal static slice<@string> splitAtBytes(@string s, @string t) {
    var a = new slice<@string>(1 + countAnyByte(s, t));
    nint n = 0;
    nint last = 0;
    for (nint i = 0; i < len(s); i++) {
        if (bytealg.IndexByteString(t, s[i]) >= 0) {
            if (last < i) {
                a[n] = s.slice(last, i);
                n++;
            }
            last = i + 1;
        }
    }
    if (last < len(s)) {
        a[n] = s.slice(last);
        n++;
    }
    return a.slice(0, n);
}

internal static slice<@string> getFields(@string s) {
    return splitAtBytes(s, " \r\t\n"u8);
}

// Bigger than we need, not too big to worry about overflow
internal static UntypedInt big => 0xFFFFFF;

// Decimal to integer.
// Returns number, characters consumed, success.
internal static (nint n, nint i, bool ok) dtoi(@string s) {
    nint n = default!;
    nint i = default!;

    n = 0;
    for (i = 0; i < len(s) && (rune)'0' <= s[i] && s[i] <= (rune)'9'; i++) {
        n = n * 10 + (nint)((byte)(s[i] - (rune)'0'));
        if (n >= big) {
            return (big, i, false);
        }
    }
    if (i == 0) {
        return (0, 0, false);
    }
    return (n, i, true);
}

// Hexadecimal to integer.
// Returns number, characters consumed, success.
internal static (nint n, nint i, bool ok) xtoi(@string s) {
    nint n = default!;
    nint i = default!;

    n = 0;
    for (i = 0; i < len(s); i++) {
        if ((rune)'0' <= s[i] && s[i] <= (rune)'9'){
            n *= 16;
            n += (nint)((byte)(s[i] - (rune)'0'));
        } else 
        if ((rune)'a' <= s[i] && s[i] <= (rune)'f'){
            n *= 16;
            n += (nint)((byte)(s[i] - (rune)'a')) + 10;
        } else 
        if ((rune)'A' <= s[i] && s[i] <= (rune)'F'){
            n *= 16;
            n += (nint)((byte)(s[i] - (rune)'A')) + 10;
        } else {
            break;
        }
        if (n >= big) {
            return (0, i, false);
        }
    }
    if (i == 0) {
        return (0, i, false);
    }
    return (n, i, true);
}

// xtoi2 converts the next two hex digits of s into a byte.
// If s is longer than 2 bytes then the third byte must be e.
// If the first two bytes of s are not hex digits or the third byte
// does not match e, false is returned.
internal static (byte, bool) xtoi2(@string s, byte e) {
    if (len(s) > 2 && s[2] != e) {
        return (0, false);
    }
    var (n, ei, ok) = xtoi(s[..2]);
    return ((byte)n, ok && ei == 2);
}

// hasUpperCase tells whether the given string contains at least one upper-case.
internal static bool hasUpperCase(@string s) {
    foreach (var (i, _) in s) {
        if ((rune)'A' <= s[i] && s[i] <= (rune)'Z') {
            return true;
        }
    }
    return false;
}

// lowerASCIIBytes makes x ASCII lowercase in-place.
internal static void lowerASCIIBytes(slice<byte> x) {
    foreach (var (i, b) in x) {
        if ((rune)'A' <= b && b <= (rune)'Z') {
            x[i] += (byte)((rune)'a' - (rune)'A');
        }
    }
}

// lowerASCII returns the ASCII lowercase version of b.
internal static byte lowerASCII(byte b) {
    if ((rune)'A' <= b && b <= (rune)'Z') {
        return (byte)(b + ((rune)'a' - (rune)'A'));
    }
    return b;
}

// trimSpace returns x without any leading or trailing ASCII whitespace.
internal static @string trimSpace(@string x) {
    while (len(x) > 0 && isSpace(x[0])) {
        x = x[1..];
    }
    while (len(x) > 0 && isSpace(x[len(x) - 1])) {
        x = x.slice(0, len(x) - 1);
    }
    return x;
}

// isSpace reports whether b is an ASCII space character.
internal static bool isSpace(byte b) {
    return b == (rune)' ' || b == (rune)'\t' || b == (rune)'\n' || b == (rune)'\r';
}

// removeComment returns line, removing any '#' byte and any following
// bytes.
internal static @string removeComment(@string line) {
    {
        nint i = bytealg.IndexByteString(line, (rune)'#'); if (i != -1) {
            return line.slice(0, i);
        }
    }
    return line;
}

// foreachField runs fn on each non-empty run of non-space bytes in x.
// It returns the first non-nil error returned by fn.
internal static error foreachField(@string x, Func<@string, error> fn) {
    x = trimSpace(x);
    while (len(x) > 0) {
        nint sp = bytealg.IndexByteString(x, (rune)' ');
        if (sp == -1) {
            return fn(x);
        }
        {
            @string field = trimSpace(x.slice(0, sp)); if (len(field) > 0) {
                {
                    var err = fn(field); if (err != default!) {
                        return err;
                    }
                }
            }
        }
        x = trimSpace(x.slice(sp + 1));
    }
    return default!;
}

// stringsHasSuffixFold reports whether s ends in suffix,
// ASCII-case-insensitively.
internal static bool stringsHasSuffixFold(@string s, @string suffix) {
    return len(s) >= len(suffix) && stringsEqualFold(s.slice(len(s) - len(suffix)), suffix);
}

// stringsEqualFold is strings.EqualFold, ASCII only. It reports whether s and t
// are equal, ASCII-case-insensitively.
internal static bool stringsEqualFold(@string s, @string t) {
    if (len(s) != len(t)) {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        if (lowerASCII(s[i]) != lowerASCII(t[i])) {
            return false;
        }
    }
    return true;
}

} // end net_package
