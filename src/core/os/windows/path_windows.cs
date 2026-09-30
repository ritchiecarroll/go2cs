// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using filepathlite = @internal.filepathlite_package;
using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal;
using @internal.syscall;

partial class os_package {

public static UntypedInt PathSeparator => /* '\\' */ 92; // OS-specific path separator
public static UntypedInt PathListSeparator => /* ';' */ 59; // OS-specific path list separator

// IsPathSeparator reports whether c is a directory separator character.
public static bool IsPathSeparator(uint8 c) {
    // NOTE: Windows accepts / as path separator.
    return c == (rune)'\\' || c == (rune)'/';
}

internal static @string dirname(@string path) {
    @string vol = filepathlite.VolumeName(path);
    nint i = len(path) - 1;
    while (i >= len(vol) && !IsPathSeparator(path[i])) {
        i--;
    }
    @string dir = path.slice(len(vol), i + 1);
    nint last = len(dir) - 1;
    if (last > 0 && IsPathSeparator(dir[last])) {
        dir = dir.slice(0, last);
    }
    if (dir == ""u8) {
        dir = "."u8;
    }
    return vol + dir;
}

// fixLongPath returns the extended-length (\\?\-prefixed) form of
// path when needed, in order to avoid the default 260 character file
// path limit imposed by Windows. If the path is short enough or already
// has the extended-length prefix, fixLongPath returns path unmodified.
// If the path is relative and joining it with the current working
// directory results in a path that is too long, fixLongPath returns
// the absolute path with the extended-length prefix.
//
// See https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file#maximum-path-length-limitation
internal static @string fixLongPath(@string path) {
    if (windows.CanUseLongPaths) {
        return path;
    }
    return addExtendedPrefix(path);
}

// addExtendedPrefix adds the extended path prefix (\\?\) to path.
internal static @string addExtendedPrefix(@string path) {
    if (len(path) >= 4) {
        if (path[..4] == @"\??\") {
            // Already extended with \??\
            return path;
        }
        if (IsPathSeparator(path[0]) && IsPathSeparator(path[1]) && path[2] == (rune)'?' && IsPathSeparator(path[3])) {
            // Already extended with \\?\ or any combination of directory separators.
            return path;
        }
    }
    // Do nothing (and don't allocate) if the path is "short".
    // Empirically (at least on the Windows Server 2013 builder),
    // the kernel is arbitrarily okay with < 248 bytes. That
    // matches what the docs above say:
    // "When using an API to create a directory, the specified
    // path cannot be so long that you cannot append an 8.3 file
    // name (that is, the directory name cannot exceed MAX_PATH
    // minus 12)." Since MAX_PATH is 260, 260 - 12 = 248.
    //
    // The MSDN docs appear to say that a normal path that is 248 bytes long
    // will work; empirically the path must be less then 248 bytes long.
    nint pathLength = len(path);
    if (!filepathlite.IsAbs(path)) {
        // If the path is relative, we need to prepend the working directory
        // plus a separator to the path before we can determine if it's too long.
        // We don't want to call syscall.Getwd here, as that call is expensive to do
        // every time fixLongPath is called with a relative path, so we use a cache.
        // Note that getwdCache might be outdated if the working directory has been
        // changed without using os.Chdir, i.e. using syscall.Chdir directly or cgo.
        // This is fine, as the worst that can happen is that we fail to fix the path.
        ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Lock();
        if (getwdCache.dir == ""u8) {
            // Init the working directory cache.
            (getwdCache.dir, _) = syscall.Getwd();
        }
        pathLength += len(getwdCache.dir) + 1;
        ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Unlock();
    }
    if (pathLength < 248) {
        // Don't fix. (This is how Go 1.7 and earlier worked,
        // not automatically generating the \\?\ form)
        return path;
    }
    bool isUNC = default!;
    bool isDevice = default!;
    if (len(path) >= 2 && IsPathSeparator(path[0]) && IsPathSeparator(path[1])) {
        if (len(path) >= 4 && path[2] == (rune)'.' && IsPathSeparator(path[3])){
            // Starts with //./
            isDevice = true;
        } else {
            // Starts with //
            isUNC = true;
        }
    }
    slice<uint16> prefix = default!;
    if (isUNC){
        // UNC path, prepend the \\?\UNC\ prefix.
        prefix = new uint16[]{(rune)'\\', (rune)'\\', (rune)'?', (rune)'\\', (rune)'U', (rune)'N', (rune)'C', (rune)'\\'}.slice();
    } else 
    if (isDevice){
    } else {
        // Don't add the extended prefix to device paths, as it would
        // change its meaning.
        prefix = new uint16[]{(rune)'\\', (rune)'\\', (rune)'?', (rune)'\\'}.slice();
    }
    var (p, err) = syscall.UTF16FromString(path);
    if (err != default!) {
        return path;
    }
    // Estimate the required buffer size using the path length plus the null terminator.
    // pathLength includes the working directory. This should be accurate unless
    // the working directory has changed without using os.Chdir.
    var n = (uint32)pathLength + 1;
    slice<uint16> buf = default!;
    while (ᐧ) {
        buf = new slice<uint16>((nint)(n + (uint32)len(prefix)));
        (n, err) = syscall.GetFullPathName(Ꮡ(p, 0), n, Ꮡ(buf, len(prefix)), nil);
        if (err != default!) {
            return path;
        }
        if (n <= (uint32)(len(buf) - len(prefix))) {
            buf = buf.slice(0, (nint)(n + (uint32)len(prefix)));
            break;
        }
    }
    if (isUNC) {
        // Remove leading \\.
        buf = buf[2..];
    }
    copy(buf, prefix);
    return syscall.UTF16ToString(buf);
}

} // end os_package
