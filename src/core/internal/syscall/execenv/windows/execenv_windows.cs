// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go.@internal.syscall;

using Δwindows = go.@internal.syscall.windows_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal.syscall;

partial class execenv_package {

// Default will return the default environment
// variables based on the process attributes
// provided.
//
// If the process attributes contain a token, then
// the environment variables will be sourced from
// the defaults for that user token, otherwise they
// will be sourced from syscall.Environ().
public static (slice<@string> env, error err) Default(ж<syscall.SysProcAttr> Ꮡsys) {
    slice<@string> env = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var sys = ref Ꮡsys.DerefOrNull();

        if (Ꮡsys == nil || sys.Token == 0) {
            (env, err) = (syscall.Environ(), default); goto ᒐdone;
        }
        ref var blockp = ref heap<ж<uint16>>(out var Ꮡblockp);
        err = Δwindows.CreateEnvironmentBlock(Ꮡblockp, sys.Token, false);
        if (err != nil) {
            (env, err) = (default, err); goto ᒐdone;
        }
        defer(Δwindows.DestroyEnvironmentBlock, blockp, ref ᒐ);

        uintptr size = /* unsafe.Sizeof(*blockp) */ 2;
        while (blockp.Value != 0) { // environment block ends with empty string
            // find NUL terminator
            @unsafe.Pointer end = (uintptr)@unsafe.Add(@unsafe.Pointer.FromPinnedBox(blockp), size);
            while (~(ж<uint16>)(uintptr)(end) != 0) {
                end = (uintptr)@unsafe.Add(end, size);
            }

            var entry = @unsafe.Slice(blockp, ((uintptr)end - (uintptr)blockp) / 2);
            env = append(env, syscall.UTF16ToString(entry));
            blockp = (ж<uint16>)(uintptr)(@unsafe.Add(end, size));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (env, err);
}

} // end execenv_package
