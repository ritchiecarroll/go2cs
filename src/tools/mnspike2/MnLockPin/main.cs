namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using time = time_package;

// M:N PROTOTYPE, THE PIN (docs/phase4/DESIGN-mn-pin.md, M4 and M5). A hand project in the converted shape, because
// Go has no word for "the thread this goroutine is on": the thread record lives in pin_impl.cs.
//
// The rule under test: a goroutine locked with runtime.LockOSThread runs on one thread until it unlocks or ends, over
// every kind of park; no other goroutine ever runs on that thread meanwhile; a goroutine that ends locked takes its
// thread with it; and a goroutine that unlocks and parks at once is released, not lost (10.3).
//
// Standard output holds only counts that are the same in every lane: thread changes per shape (0), goroutines seen on
// a pinned thread (0), whether the ended-locked thread is gone (true), and that the unpin arm finished.
partial class main_package {

internal const nint shapeChan = 0;
internal const nint shapeSelect = 1;
internal const nint shapeNested = 2;
internal const nint shapeForeign = 3;
internal const nint shapeSleep = 4;
internal const nint shapeMutex = 5;
internal const nint shapeWait = 6;
internal const nint shapeNest = 7;
internal const nint shapeExit = 8;
internal const nint shapeUnpin = 9;
internal const nint shapeScenA = 10;
internal const nint shapeScenB = 11;
// The Gosched-park seat (P3's check (d)): a locked goroutine's runtime.Gosched is a yield park, delivered back to its
// own carrier; checkPinned after every yield asserts it never stepped on another thread.
internal const nint shapeGosched = 12;

// echo answers round trips until it is sent a negative value. It is never locked, so it checks after every park that
// it is not on a thread a locked goroutine holds.
internal static void echo(channel<nint> req, channel<nint> resp) {
    while (true) {
        nint v = ᐸꟷ(req);

        checkForeign();

        if (v < 0) {
            return;
        }

        resp.ᐸꟷ(v);
    }
}

// trip is one round trip to the partner: the goroutine parks on the way, and the call returns through a twin.
internal static nint trip(channel<nint> req, channel<nint> resp, nint v) {
    req.ᐸꟷ(v);

    return ᐸꟷ(resp);
}

// tripSelect parks in a select whose other case never fires.
internal static nint tripSelect(channel<nint> req, channel<nint> resp, channel<nint> never, nint v) {
    req.ᐸꟷ(v);
    nint got = 0;
    var respᴛ1 = resp;
    var neverᴛ1 = never;

    switch (select(ᐸꟷ(respᴛ1, ꓸꓸꓸ), ᐸꟷ(neverᴛ1, ꓸꓸꓸ))) {
    case 0 when respᴛ1.ꟷᐳ(out var r): {
        got = r;
        break;
    }
    case 1 when neverᴛ1.ꟷᐳ(out var _): {
        break;
    }}

    return got;
}

// twice returns through two twins, each of which parked.
internal static nint twice(channel<nint> req, channel<nint> resp, nint v) {
    trip(req, resp, v);

    return trip(req, resp, v + 1);
}

// holder takes the mutex, parks once, and lets it go, until it is told to stop.
internal static void holder(ж<sync_package.Mutex> Ꮡmu, channel<nint> req, channel<nint> resp, channel<nint> done) {
    ref var mu = ref Ꮡmu.DerefOrNull();

    while (!stopped()) {
        mu.Lock();
        trip(req, resp, 1);
        mu.Unlock();
    }

    done.ᐸꟷ(1);
}

// waiter's partner: one Done per round.
internal static void doner(ж<sync_package.WaitGroup> Ꮡwg, channel<nint> req, channel<nint> resp) {

    while (true) {
        nint v = trip(req, resp, 1);

        if (v < 0) {
            return;
        }

        Ꮡwg.Done();
    }
}

// locked runs one shape, locked to its thread for the whole run, and checks its thread after every park.
internal static void locked(nint shape, nint rounds, channel<nint> done) {
    armStart(shape);
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);
    var never = new channel<nint>(0);

    goǃ(echo, req, resp);
    lockMe(shape);

    if (shape == shapeMutex) {
        var Ꮡmu = Ꮡ(new sync_package.Mutex());
        ref var mu = ref Ꮡmu.DerefOrNull();
        var hreq = new channel<nint>(0);
        var hresp = new channel<nint>(0);
        var hdone = new channel<nint>(0);

        goǃ(echo, hreq, hresp);
        goǃ(holder, Ꮡmu, hreq, hresp, hdone);

        for (nint i = 0; i < rounds; i++) {
            mu.Lock();
            checkPinned(shape);
            trip(req, resp, i);
            checkPinned(shape);
            mu.Unlock();
        }

        stop();
        ᐸꟷ(hdone);
        hreq.ᐸꟷ(-1);
    } else if (shape == shapeWait) {
        var Ꮡwg = Ꮡ(new sync_package.WaitGroup());
        var wreq = new channel<nint>(0);
        var wresp = new channel<nint>(0);

        goǃ(doner, Ꮡwg, wreq, wresp);

        for (nint i = 0; i < rounds; i++) {
            Ꮡwg.Add(1);
            ᐸꟷ(wreq);
            wresp.ᐸꟷ(i);
            Ꮡwg.Wait();
            checkPinned(shape);
        }

        ᐸꟷ(wreq);
        wresp.ᐸꟷ(-1);
    } else {
        for (nint i = 0; i < rounds; i++) {
            if (shape == shapeChan) {
                trip(req, resp, i);
            } else if (shape == shapeSelect) {
                tripSelect(req, resp, never, i);
            } else if (shape == shapeNested) {
                twice(req, resp, i);
            } else if (shape == shapeForeign) {
                hop();
            } else if (shape == shapeSleep) {
                time.Sleep(time.Microsecond);
            } else if (shape == shapeGosched) {
                runtime.Gosched();
            }

            checkPinned(shape);
        }
    }

    unlockMe(shape);
    req.ᐸꟷ(-1);
    armDone(shape);
    done.ᐸꟷ(shape);
}

// nest: locked twice and unlocked once still pins; unlocked to zero it is released.
internal static void nest(nint rounds, channel<nint> done) {
    armStart(shapeNest);
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(echo, req, resp);
    lockMe(shapeNest);
    runtime.LockOSThread();
    runtime.UnlockOSThread();

    for (nint i = 0; i < rounds; i++) {
        trip(req, resp, i);
        checkPinned(shapeNest);
    }

    unlockMe(shapeNest);

    for (nint i = 0; i < rounds; i++) {
        trip(req, resp, i);
    }

    req.ᐸꟷ(-1);
    armDone(shapeNest);
    done.ᐸꟷ(shapeNest);
}

// exitLocked ends while locked: its thread must end with it.
internal static void exitLocked(channel<nint> done) {
    armStart(shapeExit);
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(echo, req, resp);
    lockMe(shapeExit);

    for (nint i = 0; i < 10; i++) {
        trip(req, resp, i);
    }

    noteExitThread();
    req.ᐸꟷ(-1);
    armDone(shapeExit);
    done.ᐸꟷ(shapeExit);
}

// unpin: pinned, then unlocked, then a park whose wake comes before the park commits (10.3's deterministic form).
internal static void unpin(nint rounds, channel<nint> done) {
    armStart(shapeUnpin);
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(echo, req, resp);
    lockMe(shapeUnpin);

    for (nint i = 0; i < rounds; i++) {
        trip(req, resp, i);
        checkPinned(shapeUnpin);
    }

    unlockMe(shapeUnpin);
    prewoke();

    for (nint i = 0; i < rounds; i++) {
        trip(req, resp, i);
    }

    req.ᐸꟷ(-1);
    armDone(shapeUnpin);
    done.ᐸꟷ(shapeUnpin);
}

// P3's MUST (DESIGN-mn-pin.md 15), scenario A: goroutines that lock, compute and END locked without parking. Each
// takes its carrier with it; the unlocked goroutine readied afterwards must still find a carrier.
internal static void lockAndEnd(nint ms, channel<nint> done) {
    runtime.LockOSThread();
    spinMs(ms);
    done.ᐸꟷ(1);
}

internal static void answer(channel<nint> ch) {
    ch.ᐸꟷ(7);
}

internal static void scenarioA(nint k) {
    armStart(shapeScenA);
    nint ms = 50;
    var done = new channel<nint>(k);

    for (nint i = 0; i < k; i++) {
        goǃ(lockAndEnd, ms, done);
    }

    for (nint i = 0; i < k; i++) {
        ᐸꟷ(done);
    }

    var ch = new channel<nint>(0);
    goǃ(answer, ch);
    ᐸꟷ(ch);
    armDone(shapeScenA);
    fmt.Println("scenario A finished: true");
}

// Scenario B: K = P goroutines lock, park once (pinned), then compute over 100 ms at once and end locked; the last
// send readies main (a twin park, not a thread block), which must still find a carrier.
internal static void lockParkCompute(channel<nint> release, channel<nint> done, nint ms) {
    runtime.LockOSThread();
    ᐸꟷ(release);
    spinMs(ms);
    done.ᐸꟷ(1);
}

internal static void scenarioB(nint k) {
    armStart(shapeScenB);
    nint ms = 150;
    var release = new channel<nint>(0);
    var done = new channel<nint>(k);

    for (nint i = 0; i < k; i++) {
        goǃ(lockParkCompute, release, done, ms);
    }

    for (nint i = 0; i < k; i++) {
        release.ᐸꟷ(1);
    }

    for (nint i = 0; i < k; i++) {
        ᐸꟷ(done);
    }

    armDone(shapeScenB);
    fmt.Println("scenario B finished: true");
}

internal static void main() {
    if (scenario() == "A") {
        scenarioA(procs());
        return;
    }

    if (scenario() == "B") {
        scenarioB(procs());
        return;
    }

    controlForeign();
    var done = new channel<nint>(0);
    nint rounds = roundsOf(20000);
    nint running = 0;

    foreach (nint shape in new nint[] { shapeChan, shapeSelect, shapeNested, shapeForeign }) {
        goǃ(locked, shape, rounds, done);
        running++;
    }

    goǃ(locked, shapeSleep, roundsOf(200), done);
    goǃ(locked, shapeMutex, roundsOf(2000), done);
    goǃ(locked, shapeWait, roundsOf(2000), done);
    goǃ(nest, roundsOf(2000), done);
    goǃ(exitLocked, done);
    goǃ(unpin, roundsOf(2000), done);
    goǃ(locked, shapeGosched, roundsOf(2000), done);
    running += 7;

    for (nint i = 0; i < running; i++) {
        ᐸꟷ(done);
    }

    time.Sleep(300 * time.Millisecond);
    report();
}

} // end main_package
