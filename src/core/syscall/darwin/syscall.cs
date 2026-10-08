// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package syscall contains an interface to the low-level operating system
// primitives. The details vary depending on the underlying system, and
// by default, godoc will display the syscall documentation for the current
// system. If you want godoc to display syscall documentation for another
// system, set $GOOS and $GOARCH to the desired system. For example, if
// you want to view documentation for freebsd/arm on linux/amd64, set $GOOS
// to freebsd and $GOARCH to arm.
// The primary use of syscall is inside other packages that provide a more
// portable interface to the system, such as "os", "time" and "net".  Use
// those packages rather than this one if you can.
// For details of the functions and data types in this package consult
// the manuals for the appropriate operating system.
// These calls return err == nil to indicate success; otherwise
// err is an operating system error describing the failure.
// On most systems, that error has type [Errno].
//
// NOTE: Most of the functions, types, and constants defined in
// this package are also available in the [golang.org/x/sys] package.
// That package has more system call support than this one,
// and most new code should prefer that package where possible.
// See https://golang.org/s/go1.4-syscall for more information.
namespace go;

using bytealg = @internal.bytealg_package;
using @internal;

partial class syscall_package {

//go:generate go run ./mksyscall_windows.go -systemdll -output zsyscall_windows.go syscall_windows.go security_windows.go

// StringByteSlice converts a string to a NUL-terminated []byte,
// If s contains a NUL byte this function panics instead of
// returning an error.
//
// Deprecated: Use ByteSliceFromString instead.
public static slice<byte> StringByteSlice(@string s) {
    var (a, err) = ByteSliceFromString(s);
    if (err != default!) {
        throw panic("syscall: string with NUL passed to StringByteSlice");
    }
    return a;
}

// ByteSliceFromString returns a NUL-terminated slice of bytes
// containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, [EINVAL]).
public static (slice<byte>, error) ByteSliceFromString(@string s) {
    if (bytealg.IndexByteString(s, 0) != -1) {
        return (default!, EINVAL);
    }
    var a = new slice<byte>(len(s) + 1);
    copy(a, s);
    return (a, default!);
}

// StringBytePtr returns a pointer to a NUL-terminated array of bytes.
// If s contains a NUL byte this function panics instead of returning
// an error.
//
// Deprecated: Use [BytePtrFromString] instead.
public static ж<byte> StringBytePtr(@string s) {
    return Ꮡ(StringByteSlice(s), 0);
}

// BytePtrFromString returns a pointer to a NUL-terminated array of
// bytes containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, [EINVAL]).
public static (ж<byte>, error) BytePtrFromString(@string s) {
    var (a, err) = ByteSliceFromString(s);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(a, 0), default!);
}

// Single-word zero for use when we need a valid pointer to 0 bytes.
// See mksyscall.pl.
internal static ж<uintptr> Ꮡ_zero = new StandardBox<uintptr>(default(uintptr));
internal static ref uintptr _zero => ref Ꮡ_zero.Value;

// Unix returns the time stored in ts as seconds plus nanoseconds.
public static (int64 sec, int64 nsec) Unix(this ref Timespec ts) {
    return ((int64)ts.Sec, (int64)ts.Nsec);
}

// Unix returns the time stored in tv as seconds plus nanoseconds.
public static (int64 sec, int64 nsec) Unix(this ref Timeval tv) {
    return ((int64)tv.Sec, (int64)tv.Usec * 1000);
}

// Nano returns the time stored in ts as nanoseconds.
public static int64 Nano(this ref Timespec ts) {
    return (int64)ts.Sec * 1000000000 + (int64)ts.Nsec;
}

// Nano returns the time stored in tv as nanoseconds.
public static int64 Nano(this ref Timeval tv) {
    return (int64)tv.Sec * 1000000000 + (int64)tv.Usec * 1000;
}

// Getpagesize and Exit are provided by the runtime.
public static partial nint Getpagesize();

public static partial void Exit(nint code);

// runtimeSetenv and runtimeUnsetenv are provided by the runtime.
internal static partial void runtimeSetenv(@string k, @string v);

internal static partial void runtimeUnsetenv(@string k);

} // end syscall_package
