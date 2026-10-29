// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin && !ios
namespace go.@internal;

/* blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) */ // for linkname

partial class cpu_package {

// Pushed from runtime.
//
//go:noescape
internal static partial (int32, int32) sysctlbynameInt32(slice<byte> name);

// Pushed from runtime.
//
//go:noescape
internal static partial int32 sysctlbynameBytes(slice<byte> name, slice<byte> @out);

// sysctlEnabled should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sysctlEnabled
internal static bool sysctlEnabled(slice<byte> name) {
    var (ret, value) = sysctlbynameInt32(name);
    if (ret < 0) {
        return false;
    }
    return value > 0;
}

// darwinKernelVersionCheck reports if Darwin kernel version is at
// least major.minor.patch.
//
// Code borrowed from x/sys/cpu.
internal static bool darwinKernelVersionCheck(nint major, nint minor, nint patch) {
    array<byte> release = new(256);
    var ret = sysctlbynameBytes(slice<byte>("kern.osrelease\x00"u8), release[..]);
    if (ret < 0) {
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
