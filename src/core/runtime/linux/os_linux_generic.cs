// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !mips && !mipsle && !mips64 && !mips64le && !s390x && !ppc64 && linux
namespace go;

partial class runtime_package {

internal static UntypedInt _SS_DISABLE => 2;
internal static UntypedInt _NSIG => 65;
internal static UntypedInt _SIG_BLOCK => 0;
internal static UntypedInt _SIG_UNBLOCK => 1;
internal static UntypedInt _SIG_SETMASK => 2;

partial struct sigset /*[2]uint32*/;

internal static ж<sigset> Ꮡsigset_all = new StandardBox<sigset>(new sigset(new uint32[]{~(uint32)0, ~(uint32)0}.array()));
internal static ref sigset sigset_all => ref Ꮡsigset_all.Value;

//go:nosplit
//go:nowritebarrierrec
internal static void sigaddset(ref sigset mask, nint i) {
    (mask)[(i - 1) / 32] |= (uint32)(((uint32)1 << (int)(((uint32)(((uint32)i - 1) & 31)))));
}

internal static void sigdelset(ref sigset mask, nint i) {
    (mask)[(i - 1) / 32] &= unchecked((uint32)~(uint32)(((uint32)1 << (int)(((uint32)(((uint32)i - 1) & 31))))));
}

//go:nosplit
internal static void sigfillset(ref uint64 mask) {
    mask = ~(uint64)0;
}

} // end runtime_package
