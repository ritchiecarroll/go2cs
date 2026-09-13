// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using windows = go.@internal.syscall.windows_package;
using io = io_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal.syscall;

partial class pprof_package {

internal static void addMaxRSS(io.Writer w) {
    ref var m = ref heap(new windows.PROCESS_MEMORY_COUNTERS(), out var Ꮡm);
    var (p, _) = syscall.GetCurrentProcess();
    var err = windows.GetProcessMemoryInfo(p, Ꮡm, (uint32)/* unsafe.Sizeof(m) */ (uintptr)72);
    if (err == default!) {
        fmt.Fprintf(w, "# MaxRSS = %d\n"u8, m.PeakWorkingSetSize);
    }
}

} // end pprof_package
