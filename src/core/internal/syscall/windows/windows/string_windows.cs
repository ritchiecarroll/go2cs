// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class windows_package {

// NTUnicodeString is a UTF-16 string for NT native APIs, corresponding to UNICODE_STRING.
partial struct NTUnicodeString {
    public uint16 Length;
    public uint16 MaximumLength;
    public ж<uint16> Buffer;
}

// NewNTUnicodeString returns a new NTUnicodeString structure for use with native
// NT APIs that work over the NTUnicodeString type. Note that most Windows APIs
// do not use NTUnicodeString, and instead UTF16PtrFromString should be used for
// the more common *uint16 string type.
public static (ж<NTUnicodeString>, error) NewNTUnicodeString(@string s) {
    var (s16, err) = syscall.UTF16FromString(s);
    if (err != default!) {
        return (default!, err);
    }
    ref var n = ref heap<uint16>(out var Ꮡn);
    n = (uint16)(len(s16) * 2);
    // https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdmsec/nf-wdmsec-wdmlibrtlinitunicodestringex
    return (Ꮡ(new NTUnicodeString(
        Length: (uint16)(n - 2), // subtract 2 bytes for the NUL terminator

        MaximumLength: n,
        Buffer: Ꮡ(s16, 0)
    )), default!);
}

} // end windows_package
