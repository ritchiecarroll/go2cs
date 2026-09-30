// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytealg = go.@internal.bytealg_package;
using stringslite = go.@internal.stringslite_package;
using Δsyscall = syscall_package;
using go.@internal;

partial class filepathlite_package {

public static UntypedInt Separator => /* '\\' */ 92; // OS-specific path separator
public static UntypedInt ListSeparator => /* ';' */ 59; // OS-specific path list separator

public static bool IsPathSeparator(uint8 c) {
    return c == (rune)'\\' || c == (rune)'/';
}

internal static bool isLocal(@string path) {
    if (path == ""u8) {
        return false;
    }
    if (IsPathSeparator(path[0])) {
        // Path rooted in the current drive.
        return false;
    }
    if (stringslite.IndexByte(path, (rune)':') >= 0) {
        // Colons are only valid when marking a drive letter ("C:foo").
        // Rejecting any path with a colon is conservative but safe.
        return false;
    }
    var hasDots = false; // contains . or .. path elements
    for (@string p = path; p != ""u8; ) {
        @string part = default!;
        (part, p, _) = cutPath(p);
        if (part == "."u8 || part == ".."u8) {
            hasDots = true;
        }
        if (isReservedName(part)) {
            return false;
        }
    }
    if (hasDots) {
        path = Clean(path);
    }
    if (path == ".."u8 || stringslite.HasPrefix(path, @"..\"u8)) {
        return false;
    }
    return true;
}

internal static (@string, error) localize(@string path) {
    for (nint i = 0; i < len(path); i++) {
        switch (path[i]) {
        case (rune)':' or (rune)'\\' or 0: {
            return ("", errInvalidPath);
        }}

    }
    var containsSlash = false;
    for (@string p = path; p != ""u8; ) {
        // Find the next path element.
        @string element = default!;
        nint i = bytealg.IndexByteString(p, (rune)'/');
        if (i < 0){
            element = p;
            p = ""u8;
        } else {
            containsSlash = true;
            element = p.slice(0, i);
            p = p.slice(i + 1);
        }
        if (isReservedName(element)) {
            return ("", errInvalidPath);
        }
    }
    if (containsSlash) {
        // We can't depend on strings, so substitute \ for / manually.
        var buf = slice<byte>(path);
        foreach (var (i, b) in buf) {
            if (b == (rune)'/') {
                buf[i] = (rune)'\\';
            }
        }
        path = ((@string)buf);
    }
    return (path, default!);
}

// isReservedName reports if name is a Windows reserved device name.
// It does not detect names with an extension, which are also reserved on some Windows versions.
//
// For details, search for PRN in
// https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file.
internal static bool isReservedName(@string name) {
    // Device names can have arbitrary trailing characters following a dot or colon.
    @string @base = name;
    for (nint i = 0; i < len(@base); i++) {
        switch (@base[i]) {
        case (rune)':' or (rune)'.': {
            @base = @base.slice(0, i);
            break;
        }}

    }
    // Trailing spaces in the last path element are ignored.
    while (len(@base) > 0 && @base[len(@base) - 1] == (rune)' ') {
        @base = @base.slice(0, len(@base) - 1);
    }
    if (!isReservedBaseName(@base)) {
        return false;
    }
    if (len(@base) == len(name)) {
        return true;
    }
    // The path element is a reserved name with an extension.
    // Some Windows versions consider this a reserved name,
    // while others do not. Use FullPath to see if the name is
    // reserved.
    {
        var (p, _) = Δsyscall.FullPath(name); if (len(p) >= 4 && p[..4] == @"\\.\") {
            return true;
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string coninˢ = "CONIN$"u8;
private static readonly @string conoutˢ = "CONOUT$"u8;

internal static bool isReservedBaseName(@string name) {
    if (len(name) == 3) {
        var exprᴛ1 = ((sstring)new byte[]{toUpper(name[0]), toUpper(name[1]), toUpper(name[2])}.slice());
        if (exprᴛ1 == "CON"u8 || exprᴛ1 == "PRN"u8 || exprᴛ1 == "AUX"u8 || exprᴛ1 == "NUL"u8) {
            return true;
        }

    }
    if (len(name) >= 4) {
        var exprᴛ2 = ((sstring)new byte[]{toUpper(name[0]), toUpper(name[1]), toUpper(name[2])}.slice());
        if (exprᴛ2 == "COM"u8 || exprᴛ2 == "LPT"u8) {
            if (len(name) == 4 && (rune)'1' <= name[3] && name[3] <= (rune)'9') {
                return true;
            }
            var exprᴛ3 = name[3..];
            if (exprᴛ3 == "\u00b2"u8 || exprᴛ3 == "\u00b3"u8 || exprᴛ3 == "\u00b9"u8) {
                return true;
            }

            return false;
        }

    }
    // Superscript ¹, ², and ³ are considered numbers as well.
    // Passing CONIN$ or CONOUT$ to CreateFile opens a console handle.
    // https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilea#consoles
    //
    // While CONIN$ and CONOUT$ aren't documented as being files,
    // they behave the same as CON. For example, ./CONIN$ also opens the console input.
    if (len(name) == 6 && name[5] == (rune)'$' && equalFold(name, coninˢ)) {
        return true;
    }
    if (len(name) == 7 && name[6] == (rune)'$' && equalFold(name, conoutˢ)) {
        return true;
    }
    return false;
}

internal static bool equalFold(@string a, @string b) {
    if (len(a) != len(b)) {
        return false;
    }
    for (nint i = 0; i < len(a); i++) {
        if (toUpper(a[i]) != toUpper(b[i])) {
            return false;
        }
    }
    return true;
}

internal static byte toUpper(byte c) {
    if ((rune)'a' <= c && c <= (rune)'z') {
        return (byte)(c - ((rune)'a' - (rune)'A'));
    }
    return c;
}

// IsAbs reports whether the path is absolute.
public static bool /*b*/ IsAbs(@string path) {
    nint l = volumeNameLen(path);
    if (l == 0) {
        return false;
    }
    // If the volume name starts with a double slash, this is an absolute path.
    if (IsPathSeparator(path[0]) && IsPathSeparator(path[1])) {
        return true;
    }
    path = path.slice(l);
    if (path == ""u8) {
        return false;
    }
    return IsPathSeparator(path[0]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string uncˢ = @"\\.\UNC"u8;

// volumeNameLen returns length of the leading volume name on Windows.
// It returns 0 elsewhere.
//
// See:
// https://learn.microsoft.com/en-us/dotnet/standard/io/file-path-formats
// https://googleprojectzero.blogspot.com/2016/02/the-definitive-guide-on-win32-to-nt.html
internal static nint volumeNameLen(@string path) {
    switch (ᐧ) {
    case {} when len(path) >= 2 && path[1] == (rune)':': {
        return 2;
    }
    case {} when len(path) == 0 || !IsPathSeparator(path[0]): {
        return 0;
    }
    case {} when pathHasPrefixFold(path, // Path starts with a drive letter.
 //
 // Not all Windows functions necessarily enforce the requirement that
 // drive letters be in the set A-Z, and we don't try to here.
 //
 // We don't handle the case of a path starting with a non-ASCII character,
 // in which case the "drive letter" might be multiple bytes long.
 // Path does not have a volume component.
 uncˢ): {
        return uncLen(path, // We're going to treat the UNC host and share as part of the volume
 // prefix for historical reasons, but this isn't really principled;
 // Windows's own GetFullPathName will happily remove the first
 // component of the path in this space, converting
 // \\.\unc\a\b\..\c into \\.\unc\a\c.
 len(@"\\.\UNC\"));
    }
    case {} when pathHasPrefixFold(path, @"\\."u8) || pathHasPrefixFold(path, @"\\?"u8) || pathHasPrefixFold(path, @"\??"u8): {
        if (len(path) == 3) {
            // Path starts with \\.\, and is a Local Device path; or
            // path starts with \\?\ or \??\ and is a Root Local Device path.
            //
            // We treat the next component after the \\.\ prefix as
            // part of the volume name, which means Clean(`\\?\c:\`)
            // won't remove the trailing \. (See #64028.)
            return 3; // exactly \\.
        }
        var (_, rest, ok) = cutPath(path[4..]);
        if (!ok) {
            return len(path);
        }
        return len(path) - len(rest) - 1;
    }
    case {} when len(path) >= 2 && IsPathSeparator(path[1]): {
        return uncLen(path, // Path starts with \\, and is a UNC path.
 2);
    }}

    return 0;
}

// pathHasPrefixFold tests whether the path s begins with prefix,
// ignoring case and treating all path separators as equivalent.
// If s is longer than prefix, then s[len(prefix)] must be a path separator.
internal static bool pathHasPrefixFold(@string s, @string prefix) {
    if (len(s) < len(prefix)) {
        return false;
    }
    for (nint i = 0; i < len(prefix); i++) {
        if (IsPathSeparator(prefix[i])){
            if (!IsPathSeparator(s[i])) {
                return false;
            }
        } else 
        if (toUpper(prefix[i]) != toUpper(s[i])) {
            return false;
        }
    }
    if (len(s) > len(prefix) && !IsPathSeparator(s[len(prefix)])) {
        return false;
    }
    return true;
}

// uncLen returns the length of the volume prefix of a UNC path.
// prefixLen is the prefix prior to the start of the UNC host;
// for example, for "//host/share", the prefixLen is len("//")==2.
internal static nint uncLen(@string path, nint prefixLen) {
    nint count = 0;
    for (nint i = prefixLen; i < len(path); i++) {
        if (IsPathSeparator(path[i])) {
            count++;
            if (count == 2) {
                return i;
            }
        }
    }
    return len(path);
}

// cutPath slices path around the first path separator.
internal static (@string before, @string after, bool found) cutPath(@string path) {
    foreach (var (i, _) in path) {
        if (IsPathSeparator(path[i])) {
            return (path.slice(0, i), path.slice(i + 1), true);
        }
    }
    return (path, "", false);
}

// isUNC reports whether path is a UNC path.
internal static bool isUNC(@string path) {
    return len(path) > 1 && IsPathSeparator(path[0]) && IsPathSeparator(path[1]);
}

// postClean adjusts the results of Clean to avoid turning a relative path
// into an absolute or rooted one.
internal static void postClean(ж<lazybuf> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    if (@out.volLen != 0 || @out.buf == default!) {
        return;
    }
    // If a ':' appears in the path element at the start of a path,
    // insert a .\ at the beginning to avoid converting relative paths
    // like a/../c: into c:.
    foreach (var (_, c) in @out.buf) {
        if (IsPathSeparator(c)) {
            break;
        }
        if (c == (rune)':') {
            @out.prepend((rune)'.', Separator);
            return;
        }
    }
    // If a path begins with \??\, insert a \. at the beginning
    // to avoid converting paths like \a\..\??\c:\x into \??\c:\x
    // (equivalent to c:\x).
    if (len(@out.buf) >= 3 && IsPathSeparator(@out.buf[0]) && @out.buf[1] == (rune)'?' && @out.buf[2] == (rune)'?') {
        @out.prepend(Separator, (rune)'.');
    }
}

} // end filepathlite_package
