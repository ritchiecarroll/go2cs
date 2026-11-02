// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

// IsNonblock returns whether the file descriptor fd is opened
// in non-blocking mode, that is, the [windows.O_FILE_FLAG_OVERLAPPED] flag
// was set when the file was opened.
public static (bool nonblocking, error err) IsNonblock(syscallꓸHandle fd) {
    ref var info = ref heap(new FILE_MODE_INFORMATION(), out var Ꮡinfo);
    {
        var errΔ1 = NtQueryInformationFile(fd, Ꮡ(new IO_STATUS_BLOCK()), @unsafe.Pointer.FromPinnedBox(Ꮡinfo), (uint32)/* unsafe.Sizeof(info) */ (uintptr)4, FileModeInformation); if (errΔ1 != nil) {
            return (false, errΔ1);
        }
    }
    return ((uint32)(info.Mode & ((uint32)((uint32)FILE_SYNCHRONOUS_IO_ALERT | (uint32)FILE_SYNCHRONOUS_IO_NONALERT))) == 0, default);
}

} // end windows_package
