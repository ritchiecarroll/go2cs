// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build arm64
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class runtime_package {

internal static void archauxv(uintptr tag, uintptr val) {
    switch (tag) {
    case var ᴛ when ᴛ == _AT_HWCAP: {
        cpu.HWCap = (nuint)val;
        break;
    }}
}

//go:nosplit
internal static int64 cputicks() {
    // nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();
}

} // end runtime_package
