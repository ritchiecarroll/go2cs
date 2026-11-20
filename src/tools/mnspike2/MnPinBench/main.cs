namespace go;

using runtime = runtime_package;
using time = time_package;

// M:N PROTOTYPE, THE PIN'S ARMS (docs/phase4/DESIGN-mn-pin.md 6, 10.5, 11, 12): one mode per process, chosen by
// MN_BENCH, timed in bench_impl.cs. Results go to standard error as MNBENCH lines; standard output stays empty.
//
//   pp      unbuffered ping-pong, MN_LOCK_A / MN_LOCK_B lock either side            (M1, M2: P1, P2, P3)
//   np      buffered send + receive that never parks                              (M1: P1)
//   m3      K pinned goroutines parked, MN_PAIRS ping-pong pairs run MN_DURATION_MS  (M3: P4, P5)
//   wake    a goroutine woken from a plain thread every MN_INTERVAL_MS, with MN_SPINNERS CPU-bound twins in
//           MN_SPIN_MS steps and K pinned parked; MN_LOCKED locks the woken one     (M3b, M7: P10, P11)
//   m1b     ping-pong beside MN_SPINNERS CPU twins, with or without one pinned goroutine parked all run (M1b: P12)
partial class main_package {

internal static void echo(channel<nint> req, channel<nint> resp, bool locked) {
    if (locked) {
        runtime.LockOSThread();
    }

    while (true) {
        nint v = ᐸꟷ(req);

        if (v < 0) {
            return;
        }

        resp.ᐸꟷ(v);
    }
}

internal static nint trip(channel<nint> req, channel<nint> resp, nint v) {
    req.ᐸꟷ(v);

    return ᐸꟷ(resp);
}

// pingpong times n round trips.
internal static void pingpong(nint n, bool lockA, bool lockB) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(echo, req, resp, lockB);

    if (lockA) {
        runtime.LockOSThread();
    }

    for (nint i = 0; i < n / 10; i++) {
        trip(req, resp, i);
    }

    startClock();

    for (nint i = 0; i < n; i++) {
        trip(req, resp, i);
    }

    stopClock(n, "round-trip-ns");
    req.ᐸꟷ(-1);
}

// neverPark times n send/receive pairs on a buffered channel that never blocks.
internal static void neverPark(nint n) {
    var ch = new channel<nint>(1);

    for (nint i = 0; i < n / 10; i++) {
        ch.ᐸꟷ(i);
        ᐸꟷ(ch);
    }

    startClock();

    for (nint i = 0; i < n; i++) {
        ch.ᐸꟷ(i);
        ᐸꟷ(ch);
    }

    stopClock(n, "iteration-ns");
}

// parkedPinned locks, then parks until it is released.
internal static void parkedPinned(channel<nint> release, channel<nint> ready) {
    runtime.LockOSThread();
    ready.ᐸꟷ(1);
    ᐸꟷ(release);
    runtime.UnlockOSThread();
}

// pinK starts k pinned goroutines and waits until each is locked and about to park.
internal static channel<nint> pinK(nint k) {
    var release = new channel<nint>(0);
    var ready = new channel<nint>(0);

    for (nint i = 0; i < k; i++) {
        goǃ(parkedPinned, release, ready);
    }

    for (nint i = 0; i < k; i++) {
        ᐸꟷ(ready);
    }

    return release;
}

internal static void unpinK(channel<nint> release, nint k) {
    for (nint i = 0; i < k; i++) {
        release.ᐸꟷ(1);
    }
}

// pair runs round trips until stopped and counts them.
internal static void pair(channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);
    nint n = 0;

    goǃ(echo, req, resp, false);

    while (!stopped()) {
        trip(req, resp, n);
        n++;
    }

    req.ᐸꟷ(-1);
    countTrips(n);
    done.ᐸꟷ(1);
}

// spinner is CPU-bound: each step spins ms milliseconds, then parks once, until stopped.
internal static void spinner(nint ms, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(echo, req, resp, false);

    while (!stopped()) {
        spin(ms);
        trip(req, resp, 1);
    }

    req.ᐸꟷ(-1);
    done.ᐸꟷ(1);
}

// woken parks on wakes, a channel a plain thread sends its send time on, and records each wake-to-run latency.
internal static void woken(channel<long> wakes, bool locked, channel<nint> done) {
    if (locked) {
        runtime.LockOSThread();
    }

    while (true) {
        long sent = ᐸꟷ(wakes);

        if (sent < 0) {
            break;
        }

        noteWake(sent);
    }

    done.ᐸꟷ(1);
}

// DESIGN-mn-pin.md 15.2 (P3's SHOULD on the floor fix): a pinned goroutine in a long plain block. It locks, parks once (so its
// carrier pins), then sleeps through time.Sleep's plain path, a block the scheduler sees (EnterBlocked) on a pinned carrier.
internal static void pinnedSleeper(channel<nint> ready, channel<nint> release, nint ms) {
    runtime.LockOSThread();
    ready.ᐸꟷ(1);
    ᐸꟷ(release);
    time.Sleep(ms * time.Millisecond);
}

internal static void main() {
    @string mode = benchMode();
    nint k = param("MN_K", 0);
    var done = new channel<nint>(0);

    if (mode == "pb") {
        var ready = new channel<nint>(0);
        var release = new channel<nint>(0);
        nint pairs = param("MN_PAIRS", 5000);

        for (nint i = 0; i < k; i++) {
            goǃ(pinnedSleeper, ready, release, param("MN_SLEEP_MS", 3000));
        }

        for (nint i = 0; i < k; i++) {
            ᐸꟷ(ready);
        }

        for (nint i = 0; i < k; i++) {
            release.ᐸꟷ(1);
        }

        for (nint i = 0; i < pairs; i++) {
            goǃ(pair, done);
        }

        runFor(param("MN_DURATION_MS", 2000));

        for (nint i = 0; i < pairs; i++) {
            ᐸꟷ(done);
        }

        reportTrips(param("MN_DURATION_MS", 2000));
    } else if (mode == "pp") {
        pingpong(param("MN_N", 200000), param("MN_LOCK_A", 0) != 0, param("MN_LOCK_B", 0) != 0);
    } else if (mode == "np") {
        neverPark(param("MN_N", 2000000));
    } else if (mode == "m3") {
        var release = pinK(k);
        nint pairs = param("MN_PAIRS", 5000);

        for (nint i = 0; i < pairs; i++) {
            goǃ(pair, done);
        }

        runFor(param("MN_DURATION_MS", 2000));

        for (nint i = 0; i < pairs; i++) {
            ᐸꟷ(done);
        }

        reportTrips(param("MN_DURATION_MS", 2000));
        unpinK(release, k);
    } else if (mode == "wake" || mode == "m1b") {
        var release = pinK(k);
        nint spinners = param("MN_SPINNERS", 0);

        for (nint i = 0; i < spinners; i++) {
            goǃ(spinner, param("MN_SPIN_MS", 200), done);
        }

        if (mode == "wake") {
            var wakes = new channel<long>(0);
            var wdone = new channel<nint>(0);

            goǃ(woken, wakes, param("MN_LOCKED", 0) != 0, wdone);
            driveWakes(wakes, param("MN_INTERVAL_MS", 25), param("MN_DURATION_MS", 10000));
            ᐸꟷ(wdone);
            reportWakes();
        } else {
            pingpong(param("MN_N", 200000), false, false);
        }

        stop();

        for (nint i = 0; i < spinners; i++) {
            ᐸꟷ(done);
        }

        unpinK(release, k);
    }

    reportScheduler();
}

} // end main_package
