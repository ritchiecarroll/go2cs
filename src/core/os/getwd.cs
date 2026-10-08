// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using runtime = runtime_package;
using Δsync = sync_package;
using syscall = syscall_package;

partial class os_package {


partial struct getwdCacheᴛ1 /*dyn*/ {
    public partial ref sync_package.Mutex Mutex { get; }
    internal @string dir;
}
internal static ж<getwdCacheᴛ1> ᏑgetwdCache = new StandardBox<getwdCacheᴛ1>(new getwdCacheᴛ1(nil));
internal static ref getwdCacheᴛ1 getwdCache => ref ᏑgetwdCache.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getwdˢ = "getwd"u8;
internal static readonly @string pwdˢ = "PWD"u8;

// Getwd returns an absolute path name corresponding to the
// current directory. If the current directory can be
// reached via multiple paths (due to symbolic links),
// Getwd may return any one of them.
//
// On Unix platforms, if the environment variable PWD
// provides an absolute name, and it is a name of the
// current directory, it is returned.
public static (@string dir, error err) Getwd() {
    @string dir = default!;
    error err = default!;

    if (runtime.GOOS == "windows"u8 || runtime.GOOS == "plan9"u8) {
        // Use syscall.Getwd directly for
        //   - plan9: see reasons in CL 89575;
        //   - windows: syscall implementation is sufficient,
        //     and we should not rely on $PWD.
        (dir, err) = syscall.Getwd();
        return (dir, NewSyscallError(getwdˢ, err));
    }
    // Clumsy but widespread kludge:
    // if $PWD is set and matches ".", use it.
    FileInfo dot = default!;
    dir = Getenv(pwdˢ);
    if (len(dir) > 0 && dir[0] == (rune)'/') {
        (dot, err) = statNolog("."u8);
        if (err != default!) {
            return ("", err);
        }
        var (d, errΔ1) = statNolog(dir);
        if (errΔ1 == default! && SameFile(dot, d)) {
            return (dir, default!);
        }
    }
    // If err is ENAMETOOLONG here, the syscall.Getwd below will
    // fail with the same error, too, but let's give it a try
    // anyway as the fallback code is much slower.
    // If the operating system provides a Getwd call, use it.
    if (syscall.ImplementsGetwd) {
        (dir, err) = ignoringEINTR2<@string>(syscall.Getwd);
        // Linux returns ENAMETOOLONG if the result is too long.
        // Some BSD systems appear to return EINVAL.
        // FreeBSD systems appear to use ENOMEM
        // Solaris appears to use ERANGE.
        if (!AreEqual(err, syscall.ENAMETOOLONG) && !AreEqual(err, syscall.EINVAL) && !AreEqual(err, errERANGE) && !AreEqual(err, errENOMEM)) {
            return (dir, NewSyscallError(getwdˢ, err));
        }
    }
    // We're trying to find our way back to ".".
    if (dot == default!) {
        (dot, err) = statNolog("."u8);
        if (err != default!) {
            return ("", err);
        }
    }
    // Apply same kludge but to cached dir instead of $PWD.
    ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Lock();
    dir = getwdCache.dir;
    ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Unlock();
    if (len(dir) > 0) {
        var (d, errΔ2) = statNolog(dir);
        if (errΔ2 == default! && SameFile(dot, d)) {
            return (dir, default!);
        }
    }
    // Root is a special case because it has no parent
    // and ends in a slash.
    (var root, err) = statNolog("/"u8);
    if (err != default!) {
        // Can't stat root - no hope.
        return ("", err);
    }
    if (SameFile(root, dot)) {
        return ("/", default!);
    }
    // General algorithm: find name in parent
    // and then find name of parent. Each iteration
    // adds /name to the beginning of dir.
    dir = ""u8;
    for (@string parent = ".."u8; ᐧ ; parent = "../"u8 + parent) {
        if (len(parent) >= 1024) {
            // Sanity check
            return ("", NewSyscallError(getwdˢ, syscall.ENAMETOOLONG));
        }
        var (fd, errΔ3) = openDirNolog(parent);
        if (errΔ3 != default!) {
            return ("", errΔ3);
        }
        while (ᐧ) {
            var (names, errΔ4) = fd.Readdirnames(100);
            if (errΔ4 != default!) {
                fd.Close();
                // Readdirnames can return io.EOF or other error.
                // In any case, we're here because syscall.Getwd
                // is not implemented or failed with ENAMETOOLONG,
                // so return the most sensible error.
                if (syscall.ImplementsGetwd) {
                    return ("", NewSyscallError(getwdˢ, syscall.ENAMETOOLONG));
                }
                return ("", NewSyscallError(getwdˢ, errENOSYS));
            }
            foreach (var (_, name) in names) {
                var (d, _) = lstatNolog(parent + "/"u8 + name);
                if (SameFile(d, dot)) {
                    dir = "/"u8 + name + dir;
                    goto Found;
                }
            }
        }
Found:
        (var pd, errΔ3) = fd.Stat();
        fd.Close();
        if (errΔ3 != default!) {
            return ("", errΔ3);
        }
        if (SameFile(pd, root)) {
            break;
        }
        // Set up for next round.
        dot = pd;
    }
    // Save answer as hint to avoid the expensive path next time.
    ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Lock();
    getwdCache.dir = dir;
    ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Unlock();
    return (dir, default!);
}

} // end os_package
