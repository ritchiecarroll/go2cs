// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && !(386 || arm || mips || mipsle || (gccgo && (ppc || s390)))
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static void configure64bitsTimeOn32BitsArchitectures() {
}

//go:noescape
internal static partial int32 futex(@unsafe.Pointer addr, int32 op, uint32 val, ж<timespec> ts, @unsafe.Pointer addr2, uint32 val3);

//go:noescape
internal static partial int32 timer_settime(int32 timerid, int32 flags, ж<itimerspec> @new, ж<itimerspec> old);

} // end runtime_package
