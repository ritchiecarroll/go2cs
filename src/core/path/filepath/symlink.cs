// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using fs = io.fs_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;
using io;

partial class filepath_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string evalSymlinksTooManyLinksˢ = "EvalSymlinks: too many links"u8;

internal static (@string, error) walkSymlinks(@string path) {
    nint volLen = filepathlite.VolumeNameLen(path);
    @string pathSeparator = ((@string)(rune)os.PathSeparator);
    if (volLen < len(path) && os.IsPathSeparator(path[volLen])) {
        volLen++;
    }
    @string vol = path.slice(0, volLen);
    @string dest = vol;
    nint linksWalked = 0;
    for ((nint start, nint end) = (volLen, volLen); start < len(path); start = end) {
        while (start < len(path) && os.IsPathSeparator(path[start])) {
            start++;
        }
        end = start;
        while (end < len(path) && !os.IsPathSeparator(path[end])) {
            end++;
        }
        // On Windows, "." can be a symlink.
        // We look it up, and use the value if it is absolute.
        // If not, we just return ".".
        var isWindowsDot = runtime.GOOS == "windows"u8 && path.slice(filepathlite.VolumeNameLen(path)) == ".";
        // The next path component is in path[start:end].
        if (end == start){
            // No more path components.
            break;
        } else 
        if (path.slice(start, end) == "." && !isWindowsDot){
            // Ignore path component ".".
            continue;
        } else 
        if (path.slice(start, end) == "..") {
            // Back up to previous component if possible.
            // Note that volLen includes any leading slash.
            // Set r to the index of the last slash in dest,
            // after the volume.
            nint r = default!;
            for (r = len(dest) - 1; r >= volLen; r--) {
                if (os.IsPathSeparator(dest[r])) {
                    break;
                }
            }
            if (r < volLen || dest.slice(r + 1) == ".."){
                // Either path has no slashes
                // (it's empty or just "C:")
                // or it ends in a ".." we had to keep.
                // Either way, keep this "..".
                if (len(dest) > volLen) {
                    dest += pathSeparator;
                }
                dest += ".."u8;
            } else {
                // Discard everything since the last slash.
                dest = dest.slice(0, r);
            }
            continue;
        }
        // Ordinary path component. Add it to result.
        if (len(dest) > filepathlite.VolumeNameLen(dest) && !os.IsPathSeparator(dest[len(dest) - 1])) {
            dest += pathSeparator;
        }
        dest += path.slice(start, end);
        // Resolve symlink.
        var (fi, err) = os.Lstat(dest);
        if (err != default!) {
            return ("", err);
        }
        if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) == 0) {
            if (!fi.Mode().IsDir() && end < len(path)) {
                return ("", syscall.ENOTDIR);
            }
            continue;
        }
        // Found symlink.
        linksWalked++;
        if (linksWalked > 255) {
            return ("", errors.New(evalSymlinksTooManyLinksˢ));
        }
        (var link, err) = os.Readlink(dest);
        if (err != default!) {
            return ("", err);
        }
        if (isWindowsDot && !IsAbs(link)) {
            // On Windows, if "." is a relative symlink,
            // just return ".".
            break;
        }
        path = link + path.slice(end);
        nint v = filepathlite.VolumeNameLen(link);
        if (v > 0){
            // Symlink to drive name is an absolute path.
            if (v < len(link) && os.IsPathSeparator(link[v])) {
                v++;
            }
            vol = link.slice(0, v);
            dest = vol;
            end = len(vol);
        } else 
        if (len(link) > 0 && os.IsPathSeparator(link[0])){
            // Symlink to absolute path.
            dest = link[..1];
            end = 1;
            vol = link[..1];
            volLen = 1;
        } else {
            // Symlink to relative path; replace last
            // path component in dest.
            nint r = default!;
            for (r = len(dest) - 1; r >= volLen; r--) {
                if (os.IsPathSeparator(dest[r])) {
                    break;
                }
            }
            if (r < volLen){
                dest = vol;
            } else {
                dest = dest.slice(0, r);
            }
            end = 0;
        }
    }
    return (Clean(dest), default!);
}

} // end filepath_package
