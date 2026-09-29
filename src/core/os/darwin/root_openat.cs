// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || windows || wasip1
namespace go;

using runtime = runtime_package;
using slices = slices_package;
using Δsync = sync_package;
using syscall = syscall_package;
using fs = go.io.fs_package;

partial class os_package {

// root implementation for platforms with a function to open a file
// relative to a directory.
[GoType] partial struct root {
    internal @string name;
    // refs is incremented while an operation is using fd.
    // closed is set when Close is called.
    // fd is closed when closed is true and refs is 0.
    internal Δsync.Mutex mu;
    internal sysfdType fd;
    internal nint refs; // number of active operations
    internal bool closed; // set when closed
}

internal static error Close(this ж<root> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        if (!r.closed && r.refs == 0) {
            syscall.Close(r.fd);
        }
        r.closed = true;
        runtime.SetFinalizer(Ꮡr.OrTypedNil(), default!); // no need for a finalizer any more
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error incref(this ж<root> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        if (r.closed) {
            return ErrClosed;
        }
        r.refs++;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void decref(this ж<root> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        if (r.refs <= 0) {
            throw panic("bad Root refcount");
        }
        r.refs--;
        if (r.closed && r.refs == 0) {
            syscall.Close(r.fd);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static @string Name(this ref root r) {
    return r.name;
}

internal static error rootMkdir(ref Root r, @string name, FileMode perm) {
    var (_, err) = doInRoot(ref r, name, (EmptyStruct, error) (sysfdType parent, @string nameΔ1) => (new EmptyStruct(), mkdirat(parent, nameΔ1, perm)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Path: name, Err: err)));
    }
    return err;
}

internal static error rootRemove(ref Root r, @string name) {
    var (_, err) = doInRoot(ref r, name, (EmptyStruct, error) (sysfdType parent, @string nameΔ1) => (new EmptyStruct(), removeat(parent, nameΔ1)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "removeat"u8, Path: name, Err: err)));
    }
    return err;
}

// doInRoot performs an operation on a path in a Root.
//
// It opens the directory containing the final element of the path,
// and calls f with the directory FD and name of the final element.
//
// If the path refers to a symlink which should be followed,
// then f must return errSymlink.
// doInRoot will follow the symlink and call f again.
internal static (T ret, error err) doInRoot<T>(ref Root r, @string name, Func<sysfdType, @string, (T, error)> f) {
    T ret = GoZero<T>();
    error err = default!;
    GoFrame ᒐ = default;
    try {
        {
            var errΔ1 = r.root.incref(); if (errΔ1 != default!) {
                (ret, err) = (ret, errΔ1); goto ᒐdone;
            }
        }
        defer(r.root.decref, ref ᒐ);
        (var parts, var suffixSep, err) = splitPathInRoot(name, default!, default!);
        if (err != default!) {
            goto ᒐdone;
        }
        sysfdType rootfd = r.root.Value.fd;
        sysfdType dirfd = rootfd;
        defer(() => {
            if (dirfd != rootfd) {
                syscall.Close(dirfd);
            }
        }, ref ᒐ);
        // When resolving .. path components, we restart path resolution from the root.
        // (We can't openat(dir, "..") to move up to the parent directory,
        // because dir may have moved since we opened it.)
        // To limit how many opens a malicious path can cause us to perform, we set
        // a limit on the total number of path steps and the total number of restarts
        // caused by .. components. If *both* limits are exceeded, we halt the operation.
        const nint maxSteps = 255;
        const nint maxRestarts = 8;
        nint i = 0;
        nint steps = 0;
        nint restarts = 0;
        nint symlinks = 0;
        while (ᐧ) {
            steps++;
            if (steps > maxSteps && restarts > maxRestarts) {
                (ret, err) = (ret, syscall.ENAMETOOLONG); goto ᒐdone;
            }
            if (parts[i] == "..") {
                // Resolve one or more parent ("..") path components.
                //
                // Rewrite the original path,
                // removing the elements eliminated by ".." components,
                // and start over from the beginning.
                restarts++;
                nint end = i + 1;
                while (end < len(parts) && parts[end] == "..") {
                    end++;
                }
                nint count = end - i;
                if (count > i) {
                    (ret, err) = (ret, errPathEscapes); goto ᒐdone;
                }
                parts = slices.Delete<slice<@string>, @string>(parts, i - count, end);
                if (len(parts) == 0) {
                    parts = new @string[]{"."u8}.slice();
                }
                i = 0;
                if (dirfd != rootfd) {
                    syscall.Close(dirfd);
                }
                dirfd = rootfd;
                continue;
            }
            if (i == len(parts) - 1){
                // This is the last path element.
                // Call f to decide what to do with it.
                // If f returns errSymlink, this element is a symlink
                // which should be followed.
                // suffixSep contains any trailing separator characters
                // which we rejoin to the final part at this time.
                (ret, err) = f(dirfd, parts[i] + suffixSep);
                {
                    var (_, ok) = err._<errSymlink>(ᐧ); if (!ok) {
                        goto ᒐdone;
                    }
                }
            } else {
                sysfdType fd = default!;
                (fd, err) = rootOpenDir(dirfd, parts[i]);
                if (err == default!){
                    if (dirfd != rootfd) {
                        syscall.Close(dirfd);
                    }
                    dirfd = fd;
                } else 
                {
                    var (_, ok) = err._<errSymlink>(ᐧ); if (!ok) {
                        goto ᒐdone;
                    }
                }
            }
            {
                var (e, ok) = err._<errSymlink>(ᐧ); if (ok) {
                    symlinks++;
                    if (symlinks > rootMaxSymlinks) {
                        (ret, err) = (ret, syscall.ELOOP); goto ᒐdone;
                    }
                    var (newparts, newSuffixSep, errΔ2) = splitPathInRoot(((@string)e), parts[..(int)(i)], parts[(int)(i + 1)..]);
                    if (errΔ2 != default!) {
                        (ret, err) = (ret, errΔ2); goto ᒐdone;
                    }
                    if (i == len(parts) - 1) {
                        // suffixSep contains any trailing path separator characters
                        // in the link target.
                        // If we are replacing the remainder of the path, retain these.
                        // If we're replacing some intermediate component of the path,
                        // ignore them, since intermediate components must always be
                        // directories.
                        suffixSep = newSuffixSep;
                    }
                    if (len(newparts) < i || !slices.Equal<slice<@string>, @string>(parts[..(int)(i)], newparts[..(int)(i)])) {
                        // Some component in the path which we have already traversed
                        // has changed. We need to restart parsing from the root.
                        i = 0;
                        if (dirfd != rootfd) {
                            syscall.Close(dirfd);
                        }
                        dirfd = rootfd;
                    }
                    parts = newparts;
                    continue;
                }
            }
            i++;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (ret, err);
}

[GoType("@string")] partial struct errSymlink;

internal static @string Error(this errSymlink _) {
    throw panic("errSymlink is not user-visible");
}

} // end os_package
