// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

public static error IoctlSetInt(nint fd, nuint req, nint value) {
    return ioctl(fd, req, (uintptr)value);
}

public static error IoctlSetPointerInt(nint fd, nuint req, nint value) {
    ref var v = ref heap<int32>((int32)value, out var Ꮡv);
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡv));
}

public static error IoctlSetString(nint fd, nuint req, @string value) {
    var bs = append(slice<byte>(value), (byte)0);
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡ(bs, 0)));
}

public static error IoctlSetWinsize(nint fd, nuint req, ж<Winsize> value) {
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(value));
}

public static error IoctlSetTermios(nint fd, nuint req, ж<Termios> value) {
    return ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(value));
}

public static (nint, error) IoctlGetInt(nint fd, nuint req) {
    ref var value = ref heap(new nint(), out var Ꮡvalue);
    var err = ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (value, err);
}

public static (ж<Winsize>, error) IoctlGetWinsize(nint fd, nuint req) {
    ref var value = ref heap(new Winsize(), out var Ꮡvalue);
    var err = ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

public static (ж<Termios>, error) IoctlGetTermios(nint fd, nuint req) {
    ref var value = ref heap(new Termios(), out var Ꮡvalue);
    var err = ioctlPtr(fd, req, @unsafe.Pointer.FromPinnedBox(Ꮡvalue));
    return (Ꮡvalue, err);
}

} // end unix_package
