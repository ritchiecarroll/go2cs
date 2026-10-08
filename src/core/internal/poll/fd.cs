// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package poll supports non-blocking I/O on file descriptors with polling.
// This supports I/O operations that block only a goroutine, not a thread.
// This is used by the net and os packages.
// It uses a poller built into the runtime, with support from the
// runtime scheduler.
namespace go.@internal;

using errors = errors_package;

partial class poll_package {

// errNetClosing is the type of the variable ErrNetClosing.
// This is used to implement the net.Error interface.
public partial struct errNetClosing {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string useOfClosedNetworkˢ = "use of closed network connection"u8;

// Error returns the error message for ErrNetClosing.
// Keep this string consistent because of issue #4373:
// since historically programs have not been able to detect
// this error, they look for the string.
public static @string Error(this errNetClosing e) {
    return useOfClosedNetworkˢ;
}

public static bool Timeout(this errNetClosing e) {
    return false;
}

public static bool Temporary(this errNetClosing e) {
    return false;
}

// ErrNetClosing is returned when a network descriptor is used after
// it has been closed.
public static errNetClosing ErrNetClosing = new errNetClosing(nil);

// ErrFileClosing is returned when a file descriptor is used after it
// has been closed.
public static error ErrFileClosing = errors.New("use of closed file"u8);

// ErrNoDeadline is returned when a request is made to set a deadline
// on a file type that does not use the poller.
public static error ErrNoDeadline = errors.New("file type does not support deadline"u8);

// Return the appropriate closing error based on isFile.
internal static error errClosing(bool isFile) {
    if (isFile) {
        return ErrFileClosing;
    }
    return ErrNetClosing;
}

// ErrDeadlineExceeded is returned for an expired deadline.
// This is exported by the os package as os.ErrDeadlineExceeded.
public static error ErrDeadlineExceeded = new DeadlineExceededErrorжerror(Ꮡ(new DeadlineExceededError(nil)));

// DeadlineExceededError is returned for an expired deadline.
partial struct DeadlineExceededError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iOTimeoutˢ = "i/o timeout"u8;

// Implement the net.Error interface.
// The string is "i/o timeout" because that is what was returned
// by earlier Go versions. Changing it may break programs that
// match on error strings.
public static @string Error(this ref DeadlineExceededError e) {
    return iOTimeoutˢ;
}

public static bool Timeout(this ref DeadlineExceededError e) {
    return true;
}

public static bool Temporary(this ref DeadlineExceededError e) {
    return true;
}

// ErrNotPollable is returned when the file or socket is not suitable
// for event notification.
public static error ErrNotPollable = errors.New("not pollable"u8);

// consume removes data from a slice of byte slices, for writev.
internal static void consume(ref slice<slice<byte>> v, int64 n) {
    while (len(v) > 0) {
        var ln0 = (int64)len((v)[0]);
        if (ln0 > n) {
            (v)[0] = (v)[0].slice((nint)(n));
            return;
        }
        n -= ln0;
        (v)[0] = default!;
        v = (v)[1..];
    }
}

// TestHookDidWritev is a hook for testing writev.
public static Action<nint> TestHookDidWritev = (nint wrote) => {
};

partial struct String /*@string*/;

} // end poll_package
