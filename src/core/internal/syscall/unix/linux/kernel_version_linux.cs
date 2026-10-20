// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;

partial class unix_package {

// KernelVersion returns major and minor kernel version numbers
// parsed from the syscall.Uname's Release field, or (0, 0) if
// the version can't be obtained or parsed.
public static (nint major, nint minor) KernelVersion() {
    nint major = default!;
    nint minor = default!;

    ref var uname = ref heap(new syscall.Utsname(), out var Ꮡuname);
    {
        var err = syscall.Uname(Ꮡuname); if (err != default!) {
            return (major, minor);
        }
    }

    array<nint> values = new(2);
    nint value = default!;
    nint vi = default!;
    foreach (var (_, c) in uname.Release.ΔRangeSnapshot()) {
        if ((rune)'0' <= c && c <= (rune)'9'){
            value = (value * 10) + (nint)((int8)(c - (rune)'0'));
        } else {
            // Note that we're assuming N.N.N here.
            // If we see anything else, we are likely to mis-parse it.
            values[vi] = value;
            vi++;
            if (vi >= len(values)) {
                break;
            }
            value = 0;
        }
    }

    return (values[0], values[1]);
}

} // end unix_package
