// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

public static (slice<byte>, error) SysvShmAttach(nint id, uintptr addr, nint flag) {
    (addr, var errno) = shmat(id, addr, flag);
    if (errno != nil) {
        return (default, errno);
    }

    ref var info = ref heap(new SysvShmDesc(), out var Ꮡinfo);

    var (_, err) = SysvShmCtl(id, IPC_STAT, Ꮡinfo);
    if (err != nil) {
        shmdt(addr);
        return (default, err);
    }

    var b = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)addr), (nint)info.Segsz);
    return (b, default);
}

public static error SysvShmDetach(slice<byte> data) {
    if (len(data) == 0) {
        return EINVAL;
    }

    return shmdt((uintptr)Ꮡ(data, 0));
}

public static (nint id, error err) SysvShmGet(nint key, nint size, nint flag) {
    return shmget(key, size, flag);
}

} // end unix_package
