// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;
using static windows_package;

partial class windows_package {

public static (@string value, bool found) Getenv(@string key) {
    return syscall.Getenv(key);
}

public static error Setenv(@string key, @string value) {
    return syscall.Setenv(key, value);
}

public static void Clearenv() {
    syscall.Clearenv();
}

public static slice<@string> Environ() {
    return syscall.Environ();
}

} partial class windowsᴛToken {

public static (slice<@string> env, error err) Environ(this Token token, bool inheritExisting) {
    slice<@string> env = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        var envᴷ = new stackAppend<@string>(moves: true);

        ref var block = ref heap<ж<uint16>>(out var Ꮡblock);
        err = CreateEnvironmentBlock(Ꮡblock, token, inheritExisting);
        if (err != nil) {
            (env, err) = (default, err); goto ᒐdone;
        }
        defer(DestroyEnvironmentBlock, block, ref ᒐ);
        var size = /* unsafe.Sizeof(*block) */ (uintptr)2;
        while (block.Value != 0) {
            @unsafe.Pointer end = @unsafe.Pointer.FromPinnedBox(block);
            while (~@unsafe.Pointer.To<uint16>(end) != 0) {
                end = @unsafe.Add(end, size);
            }

            var entry = @unsafe.Slice(block, ((uintptr)end - (uintptr)block) / size);
            env = append(envᴷ, env, UTF16ToString(entry));
            block = @unsafe.Pointer.To<uint16>(@unsafe.Add(end, size));
        }
        env = move2heap(envᴷ, env);
        (env, err) = (env, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (env, err);
}

} partial class windows_package {

public static error Unsetenv(@string key) {
    return syscall.Unsetenv(key);
}

} // end windows_package
