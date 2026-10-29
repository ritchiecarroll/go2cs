// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using os = os_package;
using filepath = path.filepath_package;
using sync = go.sync_package;
using Δsyscall = syscall_package;
using go;
using path;

partial class testenv_package {

internal static Func<(bool, @string)> hasSymlink = sync.OnceValues((bool, @string) () => {
    GoFrame ᒐ = default;
    try {
        var (tmpdir, err) = os.MkdirTemp(""u8, "symtest"u8);
        if (err != nil) {
            throw panic("failed to create temp directory: " + err.Error());
        }
        defer(os.RemoveAll, tmpdir, ref ᒐ);

        err = os.Symlink("target"u8, filepath.Join(tmpdir, "symlink"));
        switch (ᐧ) {
        case {} when err == nil: {
            return (true, "");
        }
        case {} when errors.Is(err, Δsyscall.EWINDOWS): {
            return (false, ": symlinks are not supported on your version of Windows");
        }
        case {} when errors.Is(err, Δsyscall.ERROR_PRIVILEGE_NOT_HELD): {
            return (false, ": you don't have enough privileges to create symlinks");
        }}
        return (false, "");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
});

} // end testenv_package
