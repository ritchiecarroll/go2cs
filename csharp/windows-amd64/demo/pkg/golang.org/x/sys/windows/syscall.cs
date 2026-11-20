// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using bytes = bytes_package;
using strings = strings_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using static windows_package;

partial class windows_package {

public static (slice<byte>, error) ByteSliceFromString(@string s) {
    if (strings.IndexByte(s, 0) != -1) {
        return (default, syscall.EINVAL);
    }
    var a = new slice<byte>(len(s) + 1);
    copy(a, s);
    return (a, default);
}

public static (ж<byte>, error) BytePtrFromString(@string s) {
    var (a, err) = ByteSliceFromString(s);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(a, 0), default);
}

public static @string ByteSliceToString(slice<byte> s) {
    nint i = bytes.IndexByte(s, 0); if (i != -1) {
        s = s.slice(0, i);
    }
    return (@string)s;
}

public static @string BytePtrToString(ж<byte> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return "";
    }
    if (p == 0) {
        return "";
    }

    nint n = 0;
    for (@unsafe.Pointer ptr = @unsafe.Pointer.FromPinnedBox(Ꮡp); ~@unsafe.Pointer.To<byte>(ptr) != 0; n++) {
        ptr = @unsafe.Add(ptr, 1);
    }

    return (@string)@unsafe.Slice(Ꮡp, n);
}

internal static uintptr _zero;

} partial class windowsᴛTimespec {

public static (int64 sec, int64 nsec) Unix(this ref Timespec ts) {
    return ((int64)ts.Sec, (int64)ts.Nsec);
}

} partial class windowsᴛTimeval {

public static (int64 sec, int64 nsec) Unix(this ref Timeval tv) {
    return ((int64)tv.Sec, (int64)tv.Usec * 1000);
}

} partial class windowsᴛTimespec {

public static int64 Nano(this ref Timespec ts) {
    return (int64)ts.Sec * 1000000000 + (int64)ts.Nsec;
}

} partial class windowsᴛTimeval {

public static int64 Nano(this ref Timeval tv) {
    return (int64)tv.Sec * 1000000000 + (int64)tv.Usec * 1000;
}

} // end windowsᴛTimeval
