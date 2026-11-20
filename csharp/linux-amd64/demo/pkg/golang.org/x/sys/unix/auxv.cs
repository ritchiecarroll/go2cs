// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class unix_package {

internal static partial slice<uintptr> runtime_getAuxv();

public static (slice<array<uintptr>>, error) Auxv() {
    var vec = runtime_getAuxv();
    nint vecLen = len(vec);

    if (vecLen == 0) {
        return (default, syscall.ENOENT);
    }

    if (vecLen % 2 != 0) {
        return (default, syscall.EINVAL);
    }

    var result = new slice<uintptr>(vecLen);
    copy(result, vec);
    return (@unsafe.Slice(array<uintptr>.AliasPointer(Ꮡ(result, 0), 2), vecLen / 2), default);
}

} // end unix_package
