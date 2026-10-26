// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || windows || wasip1
namespace go;

using fs = go.io.fs_package;
using runtime = runtime_package;
using slices = slices_package;
using Δsync = sync_package;
using syscall = syscall_package;
using time = time_package;
using go.io;

partial class os_package {

// root implementation for platforms with a function to open a file
// relative to a directory.
partial struct root {
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

internal static @string Name(this ref root r) {
    return r.name;
}

internal static error rootChmod(ref Root r, @string name, FileMode mode) {
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), chmodat(parent, nameΔ1, mode)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chmodat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootChown(ref Root r, @string name, nint uid, nint gid) {
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), chownat(parent, nameΔ1, uid, gid)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chownat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootLchown(ref Root r, @string name, nint uid, nint gid) {
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), lchownat(parent, nameΔ1, uid, gid)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "lchownat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootChtimes(ref Root r, @string name, time.Time atime, time.Time mtime) {
    var atimeʗ1 = atime;
    var mtimeʗ1 = mtime;
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), chtimesat(parent, nameΔ1, atimeʗ1, mtimeʗ1)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chtimesat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootMkdir(ref Root r, @string name, FileMode perm) {
    nuint flags = (nuint)doInRootCreatingDirectory;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "openbsd"u8) {
        // These platforms do not follow "symlink" on "mkdir symlink/".
        // (POSIX.1-2024 4.16 says that the trailing slash should cause
        // resolution to follow the symlink, but we're trying to match
        // platform semantics, not implement POSIX.)
        flags |= (nuint)(doInRootNoHandleTerminalSlash);
    }
    var (_, err) = doInRoot(ref r, name, flags, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), mkdirat(parent, nameΔ1, perm)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootMkdirAll(ж<Root> Ꮡr, @string fullname, FileMode perm) {
    // doInRoot opens each path element in turn.
    //
    // openDirFunc opens all but the last path component.
    // The usual default openDirFunc just opens directories with O_DIRECTORY.
    // We replace it here with one that creates missing directories along the way.
    var openDirFunc = (sysfdType, error) (sysfdType parent, @string name) => {
        foreach (var @try in range(2)) {
            var (fd, errΔ1) = rootOpenDir(parent, name);
            switch (errΔ1.type()) {
            case null:
            case errSymlink _: {
                return (fd, errΔ1);
            }}
            if (@try > 0 || !IsNotExist(errΔ1)) {
                return (0, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Err: errΔ1))));
            }
            // Try again on EEXIST, because the directory may have been created
            // by another process or thread between the rootOpenDir and mkdirat calls.
            {
                var errΔ2 = mkdirat(parent, name, perm); if (errΔ2 != default! && !AreEqual(errΔ2, syscall.EEXIST)) {
                    return (0, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Err: errΔ2))));
                }
            }
        }
        throw panic("unreachable");
    };
    // openLastComponentFunc opens the last path component.
    var openLastComponentFunc = (sysfdType parent, @string name, bool endsInSlash) => {
        var errΔ3 = mkdirat(parent, name, perm);
        if (AreEqual(errΔ3, syscall.EEXIST)) {
            var (fi, e) = lstatat(parent, name);
            if (e == default!) {
                if (fi.Mode().IsDir()){
                    // The target of MkdirAll is an existing directory.
                    errΔ3 = default!;
                } else 
                if (isLink(fi)) {
                    // The target of MkdirAll is a symlink or junction.
                    // For consistency with os.MkdirAll,
                    // succeed if the link resolves to a directory.
                    // We don't return errSymlink here, because we don't
                    // want to create the link target if it doesn't exist.
                    var (fiΔ1, eΔ1) = Ꮡr.Stat(fullname);
                    if (eΔ1 == default! && fiΔ1.Mode().IsDir()){
                        errΔ3 = default!;
                    } else 
                    if (eΔ1 == default!){
                        errΔ3 = syscall.ENOTDIR;
                    } else 
                    if (!IsNotExist(eΔ1)) {
                        // EPERM, ELOOP, etc.,
                        // probably more useful than EEXIST.
                        errΔ3 = eΔ1;
                    }
                }
            }
        }
        switch (errΔ3.type()) {
        case null:
        case errSymlink _: {
            return (new EmptyStruct(), errΔ3);
        }}
        return (new EmptyStruct(), new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Err: errΔ3))));
    };
    nuint flags = (nuint)doInRootCreatingDirectory;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "openbsd"u8) {
        flags |= (nuint)(doInRootNoHandleTerminalSlash); // see rootMkdir
    }
    var (_, err) = doInRoot(ref (Ꮡr).DerefOrNull(), fullname, flags, openDirFunc, openLastComponentFunc);
    if (err != default!) {
        {
            var (_, ok) = err._<ж<PathError>>(ᐧ); if (!ok) {
                err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Path: fullname, Err: err)));
            }
        }
    }
    return err;
}

internal static (@string, error) rootReadlink(ref Root r, @string name) {
    var (target, err) = doInRoot(ref r, name, 0, default!, (@string, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => readlinkat(parent, nameΔ1));
    if (err != default!) {
        return ("", new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readlinkat"u8, Path: name, Err: err))));
    }
    return (target, default!);
}

internal static error rootRemove(ref Root r, @string name) {
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), removeat(parent, nameΔ1)));
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "removeat"u8, Path: name, Err: err)));
    }
    return default!;
}

internal static error rootRemoveAll(ref Root r, @string name) {
    // Consistency with os.RemoveAll: Strip trailing /s from the name,
    // so RemoveAll("not_a_directory/") succeeds.
    while (len(name) > 0 && IsPathSeparator(name[len(name) - 1])) {
        name = name.slice(0, len(name) - 1);
    }
    if (endsWithDot(name)) {
        // Consistency with os.RemoveAll: Return EINVAL when trying to remove .
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "RemoveAll"u8, Path: name, Err: syscall.EINVAL)));
    }
    var (_, err) = doInRoot(ref r, name, 0, default!, (EmptyStruct, error) (sysfdType parent, @string nameΔ1, bool endsInSlash) => (new EmptyStruct(), removeAllFrom(parent, nameΔ1)));
    if (IsNotExist(err)) {
        return default!;
    }
    if (err != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "RemoveAll"u8, Path: name, Err: underlyingError(err))));
    }
    return err;
}

internal static error rootRename(ж<Root> Ꮡr, @string oldname, @string newname) {
    var (_, err) = doInRoot(ref (Ꮡr).DerefOrNull(), oldname, 0, default!, (EmptyStruct, error) (sysfdType oldparent, @string oldnameΔ1, bool oldEndsInSlash) => {
        nuint flags = (nuint)doInRootCreatingDirectory;
        if (runtime.GOOS == "windows"u8) {
            flags |= (nuint)(doInRootNoHandleTerminalSlash);
        }
        var (_, errΔ1) = doInRoot(ref (Ꮡr).DerefOrNull(), newname, flags, default!, (EmptyStruct, error) (sysfdType newparent, @string newnameΔ1, bool newEndsInSlash) => {
            if (runtime.GOOS != "windows"u8 && newEndsInSlash) {
                var (oldMode, errΔ2) = modeAt(oldparent, oldnameΔ1);
                if (errΔ2 != default!) {
                    return (new EmptyStruct(), errΔ2);
                }
                if (oldMode.Type() != fs.ModeDir) {
                    return (new EmptyStruct(), syscall.ENOTDIR);
                }
            }
            // Same checks as applied by rename (in file_unix.go):
            var (fi, errΔ3) = lstatat(newparent, newnameΔ1);
            if (errΔ3 == default! && fi.IsDir()) {
                {
                    var (ofi, errΔ4) = lstatat(oldparent, oldnameΔ1); if (errΔ4 != default!){
                        return (new EmptyStruct(), errΔ4);
                    } else 
                    if (newnameΔ1 == oldnameΔ1 || !SameFile(fi, ofi)) {
                        return (new EmptyStruct(), syscall.EEXIST);
                    }
                }
            }
            return (new EmptyStruct(), renameat(oldparent, oldnameΔ1, newparent, newnameΔ1));
        });
        return (new EmptyStruct(), errΔ1);
    });
    if (err != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("renameat"u8, oldname, newname, err)));
    }
    return err;
}

internal static error rootLink(ж<Root> Ꮡr, @string oldname, @string newname) {
    var (_, err) = doInRoot(ref (Ꮡr).DerefOrNull(), oldname, 0, default!, (EmptyStruct, error) (sysfdType oldparent, @string oldnameΔ1, bool oldEndsInSlash) => {
        nuint flags = (nuint)0;
        if (runtime.GOOS == "windows"u8) {
            // Windows doesn't pay attention to trailing slashes in the link target.
            flags |= (nuint)(doInRootNoHandleTerminalSlash);
        }
        var (_, errΔ1) = doInRoot(ref (Ꮡr).DerefOrNull(), newname, flags, default!, (EmptyStruct, error) (sysfdType newparent, @string newnameΔ1, bool newEndsInSlash) => (new EmptyStruct(), linkat(oldparent, oldnameΔ1, newparent, newnameΔ1)));
        return (new EmptyStruct(), errΔ1);
    });
    if (err != default!) {
        return new LinkErrorжerror(Ꮡ(new LinkError("linkat"u8, oldname, newname, err)));
    }
    return err;
}

// Flags for doInRoot.
// doInRootNoHandleTerminalSlash prevents doInRoot from applying special handling
// for paths which end in one or more slashes.
internal static UntypedInt doInRootNoHandleTerminalSlash => /* 1 << iota */ 1;

// doInRootCreatingDirectory indicates that the operation is creating a directory.
// When a path ends in /, the last path component may name a file which does not exist.
internal static UntypedInt doInRootCreatingDirectory => 2;

// doInRootAlwaysResolveTerminalSlash causes doInRoot to resolve symlinks in the last
// path component when a path ends in /, even on Windows. For example, this causes
// doInRoot to resolve "symlink/" as the link target of "symlink".
//
// POSIX path operations resolve symlinks in this case.
// Most Windows operations do not.
// This flag enforces the POSIX behavior.
internal static UntypedInt doInRootAlwaysResolveTerminalSlash => 4;

// doInRoot performs an operation on a path in a Root.
//
// It calls f with the FD or handle for the directory containing the last
// path element, the name of the last path element (not including slashes),
// and a boolean indicating whether the original path ended in one or more slashes.
//
// For example, given the path a/b/c it calls f with the FD for a/b and the name "c".
//
// It applies special handling for paths ending in a slash: When a path ends in a slash
// (for example "a/b/"), doInRoot will check the final component ("b") before calling f.
// If the final component is a symlink, doInRoot will resolve it.
// If the final component is neither a symlink nor a directory, doInRoot will return ENOTDIR.
// This behavior may be disabled by passing the doInRootNoHandleTerminalSlash flag.
//
// If openDirFunc is non-nil, it is called to open intermediate path elements.
// For example, given the path a/b/c openDirFunc will be called to open a and a/b in turn.
//
// f or openDirFunc may return errSymlink to indicate that the path element is a symlink
// which should be followed. Note that this can result in f being called multiple times
// with different names. For example, given the path "link" which is a symlink to "target",
// f is called with the path "link", returns errSymlink("target"), and is called again with
// the path "target".
//
// If f or openDirFunc return a *PathError, doInRoot will set PathError.Path to the
// full path which caused the error.
internal static (T ret, error err) doInRoot<T>(ref Root r, @string name, nuint flags, Func<sysfdType, @string, (sysfdType, error)> openDirFunc, Func<sysfdType, @string, bool, (T, error)> f) {
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

        (var parts, var endsInSlash, err) = splitPathInRoot(name, default!, default!);
        if (err != default!) {
            goto ᒐdone;
        }
        if (openDirFunc == default!) {
            openDirFunc = rootOpenDir;
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
Loop:
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
                err = default!;
                if (endsInSlash && (flags & (nuint)doInRootNoHandleTerminalSlash) == 0) {
                    FileInfo fi = default!;
                    (fi, err) = lstatat(dirfd, parts[i]);
                    switch (ᐧ) {
                    case {} when IsNotExist(err) && (flags & (nuint)doInRootCreatingDirectory) != 0: {
                        // The path ends in a slash, the last path component
                        // does not exist, and we creating a directory.
                        // This is fine.
                        err = default!;
                        break;
                    }
                    case {} when err != default!: {
                        goto ᒐdone;
                    }
                    case {} when fi.Mode().Type() == fs.ModeDir: {
                        break;
                    }
                    case {} when isLink(fi): {
                        if (runtime.GOOS != "windows"u8 || (flags & (nuint)doInRootAlwaysResolveTerminalSlash) != 0){
                            err = checkSymlink(dirfd, parts[i], syscall.ENOTDIR);
                        } else {
                            if (!isDirectoryLink(fi)) {
                                err = syscall.ENOTDIR;
                            }
                        }
                        break;
                    }
                    default: {
                        err = syscall.ENOTDIR;
                        goto ᒐdone;
                    }}
                }

                // This is the last path element.
                // Call f to decide what to do with it.
                // If f returns errSymlink, this element is a symlink
                // which should be followed.
                // suffixSep contains any trailing separator characters.
                if (err == default!) {
                    (ret, err) = f(dirfd, parts[i], endsInSlash);
                    if (err == default!) {
                        goto ᒐdone;
                    }
                }
            } else {
                sysfdType fd = default!;
                (fd, err) = openDirFunc(dirfd, parts[i]);
                if (err == default!) {
                    if (dirfd != rootfd) {
                        syscall.Close(dirfd);
                    }
                    dirfd = fd;
                }
            }

            switch (err.type()) {
            case null: {
                break;
            }
            case errSymlink e: {
                symlinks++;
                if (symlinks > rootMaxSymlinks) {
                    (ret, err) = (ret, syscall.ELOOP); goto ᒐdone;
                }
                var lastPart = i == len(parts) - 1;
                var (newparts, newEndsInSlash, errΔ2) = splitPathInRoot((@string)e, parts.slice(0, i), parts.slice(i + 1));
                if (errΔ2 != default!) {
                    (ret, err) = (ret, errΔ2); goto ᒐdone;
                }
                if (lastPart && newEndsInSlash) {
                    // If a link target in the final path component ends in a slash,
                    // then the path now ends in a slash.
                    endsInSlash = true;
                }
                if (len(newparts) < i || !slices.Equal<slice<@string>, @string>(parts.slice(0, i), newparts.slice(0, i))) {
                    // Some component in the path which we have already traversed
                    // has changed. We need to restart parsing from the root.
                    i = 0;
                    if (dirfd != rootfd) {
                        syscall.Close(dirfd);
                    }
                    dirfd = rootfd;
                }
                parts = newparts;
                goto continue_Loop;
                break;
            }
            case ж<PathError> e: {
                // This is strings.Join(parts[:i+1], PathSeparator).
                e.Value.Path = parts[0];
                foreach (var (_, part) in parts.slice(1, i + 1)) {
                    e.Value.Path += ((@string)(rune)PathSeparator) + part;
                }
                (ret, err) = (ret, new fs.PathErrorжerror(e)); goto ᒐdone;
            }
            default: {
                var e = err;
                goto ᒐdone;
            }}

            i++;
continue_Loop:;
        }
break_Loop:;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (ret, err);
}

internal static (FileMode, error) modeAt(sysfdType parent, @string name) {
    var (fi, err) = lstatat(parent, name);
    if (err != default!) {
        return (0, err);
    }
    return (fi.Mode(), default!);
}

// errSymlink reports that a file being operated on is actually a symlink,
// and the target of that symlink.
partial struct errSymlink /*@string*/;

internal static @string Error(this errSymlink _) {
    throw panic("errSymlink is not user-visible");
}

} // end os_package
