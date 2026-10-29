// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin && amd64 && gc
namespace go.vendor.golang.org.x.sys;

partial class cpu_package {

// darwinSupportsAVX512 checks Darwin kernel for AVX512 support via sysctl
// call (see issue 43089). It also restricts AVX512 support for Darwin to
// kernel version 21.3.0 (MacOS 12.2.0) or later (see issue 49233).
//
// Background:
// Darwin implements a special mechanism to economize on thread state when
// AVX512 specific registers are not in use. This scheme minimizes state when
// preempting threads that haven't yet used any AVX512 instructions, but adds
// special requirements to check for AVX512 hardware support at runtime (e.g.
// via sysctl call or commpage inspection). See issue 43089 and link below for
// full background:
// https://github.com/apple-oss-distributions/xnu/blob/xnu-11215.1.10/osfmk/i386/fpu.c#L214-L240
//
// Additionally, all versions of the Darwin kernel from 19.6.0 through 21.2.0
// (corresponding to MacOS 10.15.6 - 12.1) have a bug that can cause corruption
// of the AVX512 mask registers (K0-K7) upon signal return. For this reason
// AVX512 is considered unsafe to use on Darwin for kernel versions prior to
// 21.3.0, where a fix has been confirmed. See issue 49233 for full background.
internal static bool darwinSupportsAVX512() {
    return darwinSysctlEnabled(slice<byte>("hw.optional.avx512f\x00"u8)) && darwinKernelVersionCheck(21, 3, 0);
}

// Ensure Darwin kernel version is at least major.minor.patch, avoiding dependencies
internal static bool darwinKernelVersionCheck(nint major, nint minor, nint patch) {
    ref var release = ref heap(new array<byte>(256), out var Ꮡrelease);
    var err = darwinOSRelease(Ꮡrelease);
    if (err != nil) {
        return false;
    }

    array<nint> mmp = new(3);
    nint c = 0;
Loop:
    foreach (var (_, b) in release[..]) {
        switch (ᐧ) {
        case {} when b >= (byte)'0' && b <= (byte)'9': {
            mmp[c] = 10 * mmp[c] + (nint)((byte)(b - '0'));
            break;
        }
        case {} when b is (byte)'.': {
            c++;
            if (c > 2) {
                return false;
            }
            break;
        }
        case {} when b is 0: {
            goto break_Loop;
            break;
        }
        default: {
            return false;
        }}
continue_Loop:;
    }
break_Loop:;
    if (c != 2) {
        return false;
    }
    return mmp[0] > major || mmp[0] == major && (mmp[1] > minor || mmp[1] == minor && mmp[2] >= patch);
}

} // end cpu_package
