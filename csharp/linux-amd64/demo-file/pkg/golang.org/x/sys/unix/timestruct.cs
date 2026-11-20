// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using time = time_package;
using static unix_package;

partial class unix_package {

public static int64 TimespecToNsec(Timespec ts) { return ts.Nano(); }

public static Timespec NsecToTimespec(int64 nsec) {
    var sec = nsec / 1000000000;
    nsec = nsec % 1000000000;
    if (nsec < 0) {
        nsec += 1000000000;
        sec--;
    }
    return setTimespec(sec, nsec);
}

public static (Timespec, error) TimeToTimespec(time.Time t) {
    var sec = t.Unix();
    var nsec = (int64)t.Nanosecond();
    var ts = setTimespec(sec, nsec);

    if ((int64)ts.Sec != sec) {
        return (new Timespec(), ERANGE);
    }
    return (ts, default);
}

public static int64 TimevalToNsec(Timeval tv) { return tv.Nano(); }

public static Timeval NsecToTimeval(int64 nsec) {
    nsec += 999;
    var usec = nsec % 1000000000 / 1000;
    var sec = nsec / 1000000000;
    if (usec < 0) {
        usec += 1000000;
        sec--;
    }
    return setTimeval(sec, usec);
}

} partial class unixᴛTimespec {

public static (int64 sec, int64 nsec) Unix(this ref Timespec ts) {
    return ((int64)ts.Sec, (int64)ts.Nsec);
}

} partial class unixᴛTimeval {

public static (int64 sec, int64 nsec) Unix(this ref Timeval tv) {
    return ((int64)tv.Sec, (int64)tv.Usec * 1000);
}

} partial class unixᴛTimespec {

public static int64 Nano(this ref Timespec ts) {
    return (int64)ts.Sec * 1000000000 + (int64)ts.Nsec;
}

} partial class unixᴛTimeval {

public static int64 Nano(this ref Timeval tv) {
    return (int64)tv.Sec * 1000000000 + (int64)tv.Usec * 1000;
}

} // end unixᴛTimeval
