// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using os = os_package;
using strings = strings_package;
using syscall = syscall_package;

partial class filepath_package {

// HasPrefix exists for historical compatibility and should not be used.
//
// Deprecated: HasPrefix does not respect path boundaries and
// does not ignore case when required.
public static bool HasPrefix(@string p, @string prefix) {
    if (strings.HasPrefix(p, prefix)) {
        return true;
    }
    return strings.HasPrefix(strings.ToLower(p), strings.ToLower(prefix));
}

internal static slice<@string> splitList(@string path) {
    // The same implementation is used in LookPath in os/exec;
    // consider changing os/exec when changing this.
    if (path == ""u8) {
        return new @string[]{}.slice();
    }
    // Split path, respecting but preserving quotes.
    var list = new @string[]{}.slice();
    nint start = 0;
    var quo = false;
    for (nint i = 0; i < len(path); i++) {
        {
            var c = path[i];
            switch (ᐧ) {
            case {} when c is (rune)'"': {
                quo = !quo;
                break;
            }
            case {} when c == ListSeparator && !quo: {
                list = append(list, path.slice(start, i));
                start = i + 1;
                break;
            }}
        }

    }
    list = append(list, path.slice(start));
    // Remove quotes.
    foreach (var (i, s) in list) {
        list[i] = strings.ReplaceAll(s, @""""u8, @""u8);
    }
    return list;
}

internal static (@string, error) abs(@string path) {
    if (path == ""u8) {
        // syscall.FullPath returns an error on empty path, because it's not a valid path.
        // To implement Abs behavior of returning working directory on empty string input,
        // special-case empty path by changing it to "." path. See golang.org/issue/24441.
        path = "."u8;
    }
    var (fullPath, err) = syscall.FullPath(path);
    if (err != default!) {
        return ("", err);
    }
    return (Clean(fullPath), default!);
}

internal static @string join(slice<@string> elem) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    byte lastChar = default!;
    foreach (var (_, vᴛ1) in elem) {
        var e = vᴛ1;

        switch (ᐧ) {
        case {} when b.Len() is 0: {
            break;
        }
        case {} when os.IsPathSeparator(lastChar): {
            while (len(e) > 0 && os.IsPathSeparator(e[0])) {
                // Add the first non-empty path element unchanged.
                // If the path ends in a slash, strip any leading slashes from the next
                // path element to avoid creating a UNC path (any path starting with "\\")
                // from non-UNC elements.
                //
                // The correct behavior for Join when the first element is an incomplete UNC
                // path (for example, "\\") is underspecified. We currently join subsequent
                // elements so Join("\\", "host", "share") produces "\\host\share".
                e = e[1..];
            }
            if (b.Len() == 1 && strings.HasPrefix(e, // If the path is \ and the next path element is ??,
 // add an extra .\ to create \.\?? rather than \??\
 // (a Root Local Device path).
 "??"u8) && (len(e) == len("??") || os.IsPathSeparator(e[2]))) {
                Ꮡb.WriteString(@".\"u8);
            }
            break;
        }
        case {} when lastChar is (rune)':': {
            break;
        }
        default: {
            Ꮡb.WriteByte((rune)'\\');
            lastChar = (rune)'\\';
            break;
        }}

        // If the path ends in a colon, keep the path relative to the current directory
        // on a drive and don't add a separator. Preserve leading slashes in the next
        // path element, which may make the path absolute.
        //
        // 	Join(`C:`, `f`) = `C:f`
        //	Join(`C:`, `\f`) = `C:\f`
        // In all other cases, add a separator between elements.
        if (len(e) > 0) {
            Ꮡb.WriteString(e);
            lastChar = e[len(e) - 1];
        }
    }
    if (b.Len() == 0) {
        return ""u8;
    }
    return Clean(b.String());
}

internal static bool sameWord(@string a, @string b) {
    return strings.EqualFold(a, b);
}

} // end filepath_package
