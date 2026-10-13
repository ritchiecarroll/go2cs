// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.sys;

using os = os_package;

partial class cpu_package {

internal static UntypedInt _AT_HWCAP => 16;
internal static UntypedInt _AT_HWCAP2 => 26;

internal static readonly @string procAuxv = "/proc/self/auxv"u8;

internal const nint uintSize = /* int(32 << (^uint(0) >> 63)) */ 64;

// For those platforms don't have a 'cpuid' equivalent we use HWCAP/HWCAP2
// These are initialized in cpu_$GOARCH.go
// and should not be changed after they are initialized.
internal static nuint hwCap;

internal static nuint hwCap2;

internal static error readHWCAP() {
    // For Go 1.21+, get auxv from the Go runtime.
    {
        var a = getAuxv(); if (len(a) > 0) {
            while (len(a) >= 2) {
                var tag = a[0];
                nuint val = (nuint)a[1];
                a = a[2..];
                var exprᴛ1 = tag;
                if (exprᴛ1 == _AT_HWCAP) {
                    hwCap = val;
                }
                else if (exprᴛ1 == _AT_HWCAP2) {
                    hwCap2 = val;
                }
            }
            return default!;
        }
    }

    var (buf, err) = os.ReadFile(procAuxv);
    if (err != default!) {
        // e.g. on android /proc/self/auxv is not accessible, so silently
        // ignore the error and leave Initialized = false. On some
        // architectures (e.g. arm64) doinit() implements a fallback
        // readout and will set Initialized = true again.
        return err;
    }
    var bo = hostByteOrder();
    while (len(buf) >= 2 * (uintSize / 8)) {
        nuint tag = default!;
        nuint val = default!;
        switch (uintSize) {
        case 32: {
            tag = (nuint)bo.Uint32(buf[0..]);
            val = (nuint)bo.Uint32(buf[4..]);
            buf = buf[8..];
            break;
        }
        case 64: {
            tag = (nuint)bo.Uint64(buf[0..]);
            val = (nuint)bo.Uint64(buf[8..]);
            buf = buf[16..];
            break;
        }}
        var exprᴛ2 = tag;
        if (exprᴛ2 == _AT_HWCAP) {
            hwCap = val;
        }
        else if (exprᴛ2 == _AT_HWCAP2) {
            hwCap2 = val;
        }
    }
    return default!;
}

} // end cpu_package
