// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

partial class syscall_package {

// TimespecToNsec returns the time stored in ts as nanoseconds.
public static int64 TimespecToNsec(Timespec ts) { return ts.Nano(); }

// NsecToTimespec converts a number of nanoseconds into a [Timespec].
public static Timespec NsecToTimespec(int64 nsec) {
    var sec = nsec / 1000000000;
    nsec = nsec % 1000000000;
    if (nsec < 0) {
        nsec += 1000000000;
        sec--;
    }
    return setTimespec(sec, nsec);
}

// TimevalToNsec returns the time stored in tv as nanoseconds.
public static int64 TimevalToNsec(Timeval tv) {
    tv = tv.ΔClone();

    return tv.Nano();
}

// NsecToTimeval converts a number of nanoseconds into a [Timeval].
public static Timeval NsecToTimeval(int64 nsec) {
    nsec += 999; // round up to microsecond
    var usec = nsec % 1000000000 / 1000;
    var sec = nsec / 1000000000;
    if (usec < 0) {
        usec += 1000000;
        sec--;
    }
    return setTimeval(sec, usec);
}

} // end syscall_package
