// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using PathError = go.io.fs_package.PathError;

namespace go;

using poll = @internal.poll_package;
using fs = go.io.fs_package;
using @internal;
using go.io;

partial class os_package {

// Portable analogs of some common system call errors.
//
// Errors returned from this package may be tested against these errors
// with [errors.Is].
public static error ErrInvalid = fs.ErrInvalid; // "invalid argument"

public static error ErrPermission = fs.ErrPermission; // "permission denied"

public static error ErrExist = fs.ErrExist; // "file already exists"

public static error ErrNotExist = fs.ErrNotExist; // "file does not exist"

public static error ErrClosed = fs.ErrClosed; // "file already closed"

public static error ErrNoDeadline = errNoDeadline(); // "file type does not support deadline"

public static error ErrDeadlineExceeded = errDeadlineExceeded(); // "i/o timeout"

internal static error errNoDeadline() {
    return poll.ErrNoDeadline;
}

// errDeadlineExceeded returns the value for os.ErrDeadlineExceeded.
// This error comes from the internal/poll package, which is also
// used by package net. Doing it this way ensures that the net
// package will return os.ErrDeadlineExceeded for an exceeded deadline,
// as documented by net.Conn.SetDeadline, without requiring any extra
// work in the net package and without requiring the internal/poll
// package to import os (which it can't, because that would be circular).
internal static error errDeadlineExceeded() {
    return poll.ErrDeadlineExceeded;
}

partial interface timeout {
    bool Timeout();
}

// SyscallError records an error from a specific system call.
partial struct SyscallError {
    public @string Syscall;
    public error Err;
}

public static @string Error(this ref SyscallError e) {
    return e.Syscall + ": "u8 + e.Err.Error();
}

public static error Unwrap(this ref SyscallError e) {
    return e.Err;
}

// Timeout reports whether this error represents a timeout.
public static bool Timeout(this ref SyscallError e) {
    var (t, ok) = e.Err._<timeout>(ᐧ);
    return ok && t.Timeout();
}

// NewSyscallError returns, as an error, a new [SyscallError]
// with the given system call name and error details.
// As a convenience, if err is nil, NewSyscallError returns nil.
public static error NewSyscallError(@string syscall, error err) {
    if (err == default!) {
        return default!;
    }
    return new SyscallErrorжerror(Ꮡ(new SyscallError(syscall, err)));
}

// IsExist returns a boolean indicating whether its argument is known to report
// that a file or directory already exists. It is satisfied by [ErrExist] as
// well as some syscall errors.
//
// This function predates [errors.Is]. It only supports errors returned by
// the os package. New code should use errors.Is(err, fs.ErrExist).
public static bool IsExist(error err) {
    return underlyingErrorIs(err, ErrExist);
}

// IsNotExist returns a boolean indicating whether its argument is known to
// report that a file or directory does not exist. It is satisfied by
// [ErrNotExist] as well as some syscall errors.
//
// This function predates [errors.Is]. It only supports errors returned by
// the os package. New code should use errors.Is(err, fs.ErrNotExist).
public static bool IsNotExist(error err) {
    return underlyingErrorIs(err, ErrNotExist);
}

// IsPermission returns a boolean indicating whether its argument is known to
// report that permission is denied. It is satisfied by [ErrPermission] as well
// as some syscall errors.
//
// This function predates [errors.Is]. It only supports errors returned by
// the os package. New code should use errors.Is(err, fs.ErrPermission).
public static bool IsPermission(error err) {
    return underlyingErrorIs(err, ErrPermission);
}

// IsTimeout returns a boolean indicating whether its argument is known
// to report that a timeout occurred.
//
// This function predates [errors.Is], and the notion of whether an
// error indicates a timeout can be ambiguous. For example, the Unix
// error EWOULDBLOCK sometimes indicates a timeout and sometimes does not.
// New code should use errors.Is with a value appropriate to the call
// returning the error, such as [os.ErrDeadlineExceeded].
public static bool IsTimeout(error err) {
    var (terr, ok) = underlyingError(err)._<timeout>(ᐧ);
    return ok && terr.Timeout();
}

internal static bool underlyingErrorIs(error err, error target) {
    // Note that this function is not errors.Is:
    // underlyingError only unwraps the specific error-wrapping types
    // that it historically did, not all errors implementing Unwrap().
    err = underlyingError(err);
    if (AreEqual(err, target)) {
        return true;
    }
    // To preserve prior behavior, only examine syscall errors.
    var (e, ok) = err._<syscallErrorType>(ᐧ);
    return ok && e.Is(target);
}

// underlyingError returns the underlying error for known os error types.
internal static error underlyingError(error err) {
    switch (err.type()) {
    case ж<PathError> errΔ1: {
        return (~errΔ1).Err;
    }
    case ж<LinkError> errΔ1: {
        return (~errΔ1).Err;
    }
    case ж<SyscallError> errΔ1: {
        return (~errΔ1).Err;
    }}
    return err;
}

} // end os_package
