// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using sync = go.sync_package;
using fs = io.fs_package;
using go;
using path;

partial class testenv_package {

internal static Func<(bool, @string)> hasSymlink = sync.OnceValues((bool ok, @string reason) () => {
    bool ok = default;
    @string reason = default;
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            (ok, reason) = (false, ""); goto ᒐdone;
        }
        case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "wasip1"u8: {
            // For wasip1, some runtimes forbid absolute symlinks,
            // or symlinks that escape the current working directory.
            // Perform a simple test to see whether the runtime
            // supports symlinks or not. If we get a permission
            // error, the runtime does not support symlinks.
            var (dir, err) = os.MkdirTemp(""u8, ""u8);
            if (err != nil) {
                (ok, reason) = (false, ""); goto ᒐdone;
            }
            defer(() => {
                _ = os.RemoveAll(dir);
            }, ref ᒐ);
            @string fpath = filepath.Join(dir, "testfile.txt");
            {
                var errΔ1 = os.WriteFile(fpath, default, 420); if (errΔ1 != nil) {
                    (ok, reason) = (false, ""); goto ᒐdone;
                }
            }
            {
                var errΔ2 = os.Symlink(fpath, filepath.Join(dir, "testlink")); if (errΔ2 != nil) {
                    if (SyscallIsNotSupported(errΔ2)) {
                        (ok, reason) = (false, fmt.Sprintf("symlinks unsupported: %s"u8, errΔ2.Error())); goto ᒐdone;
                    }
                    (ok, reason) = (false, ""); goto ᒐdone;
                }
            }
            break;
        }}

        (ok, reason) = (true, ""); goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (ok, reason);
});

} // end testenv_package
