namespace go;

using fmt = fmt_package;
using time = time_package;

// M:N PROTOTYPE (A3): the context rule and the no-bleed test. A hand project in the converted shape, because Go has
// no AsyncLocal to write a Go oracle program against. The values and the counters live in ctx_impl.cs.
//
// The rule under test: what a goroutine writes to an AsyncLocal is the goroutine's. It follows the goroutine from
// carrier to carrier, a callee's write is seen by its caller, a child gets its creator's value at the go statement,
// and nothing one goroutine wrote is ever read by another that shares its carrier.
//
// An arm's value lives in the fixture's own AsyncLocal, except the labels arm's, which lives in golib's profile
// labels (the one AsyncLocal the runtime owns). Standard output holds only the count of wrong reads in each arm, so
// it is the same text in every lane. No go statement here passes more than four arguments: the prototype's twin
// launch stops there, and a longer one starts a goroutine on a thread of its own.
partial class main_package {

internal const nint armFollow = 0;
internal const nint armCallee = 1;
internal const nint armBleed = 2;
internal const nint armInherit = 3;
internal const nint armInjected = 4;
internal const nint armHandoff = 5;
internal const nint armSleep = 6;
internal const nint armLabels = 7;
internal const nint armMain = 8;

internal const nint slotLocal = 0;
internal const nint slotLabels = 1;

// partner writes its own value, then answers a goroutine's round trips until it is sent a negative value, and
// reads its value back after every park.
internal static void partner(channel<nint> req, channel<nint> resp, nint arm, nint own) {
    tag(arm, own);

    while (true) {
        nint v = ᐸꟷ(req);

        read(arm, own);

        if (v < 0) {
            return;
        }

        resp.ᐸꟷ(v);
    }
}

// listener is a partner that never writes: after every park it reads what its creator held when it started it.
internal static void listener(channel<nint> req, channel<nint> resp, nint arm, nint want) {
    while (true) {
        nint v = ᐸꟷ(req);

        read(arm, want);

        if (v < 0) {
            return;
        }

        resp.ᐸꟷ(v);
    }
}

// echo is the partner of the cost mode: no read, so the round trip holds the park and nothing else.
internal static void echo(channel<nint> req, channel<nint> resp, nint own, bool write) {
    if (write) {
        setTag(slotLocal, own);
    }

    while (true) {
        nint v = ᐸꟷ(req);

        if (v < 0) {
            return;
        }

        resp.ᐸꟷ(v);
    }
}

// trip is one round trip to the partner: the goroutine parks on the way.
internal static nint trip(channel<nint> req, channel<nint> resp, nint v) {
    req.ᐸꟷ(v);

    return ᐸꟷ(resp);
}

// follow: a writer sets its value once and reads it back after every park, until main says stop.
internal static void writer(nint arm, nint id, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(partner, req, resp, arm, id + 1000);
    tag(arm, id);

    while (!stopped()) {
        trip(req, resp, 1);
        read(arm, id);
    }

    req.ᐸꟷ(-1);
    done.ᐸꟷ(id);
}

// bleed: a reader never writes. It reads what its creator held when it started it, at every step.
internal static void reader(nint arm, nint want, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(listener, req, resp, arm, want);
    read(arm, want);

    while (!stopped()) {
        trip(req, resp, 1);
        read(arm, want);
    }

    req.ᐸꟷ(-1);
    done.ᐸꟷ(want);
}

// The three callee shapes: a write between two parks, a write after the last park, a write with no park at all.
internal static void writeBetween(channel<nint> req, channel<nint> resp, nint arm, nint v) {
    trip(req, resp, 1);
    tag(arm, v);
    trip(req, resp, 2);
}

internal static void writeLast(channel<nint> req, channel<nint> resp, nint arm, nint v) {
    trip(req, resp, 1);
    tag(arm, v);
}

internal static void writeOnly(channel<nint> req, channel<nint> resp, nint arm, nint v) {
    if (v < 0) {
        trip(req, resp, 1);
    }

    tag(arm, v);
}

// callee write: the caller reads what its callee wrote, straight after the return and after its own next park.
internal static void caller(nint arm, nint id, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);
    nint k = 0;

    goǃ(partner, req, resp, arm, id + 1000);
    tag(arm, id);

    while (!stopped()) {
        k = (k + 1) % 5;

        writeBetween(req, resp, arm, id + k + 1);
        read(arm, id + k + 1);
        trip(req, resp, 3);
        read(arm, id + k + 1);

        writeLast(req, resp, arm, id + k + 11);
        read(arm, id + k + 11);
        trip(req, resp, 3);
        read(arm, id + k + 11);

        writeOnly(req, resp, arm, id + k + 21);
        read(arm, id + k + 21);
        trip(req, resp, 3);
        read(arm, id + k + 21);
    }

    req.ᐸꟷ(-1);
    done.ᐸꟷ(id);
}

// inherit: a parent that has parked many times starts a child and then writes again. The child keeps the value
// the parent held at the go statement; the parent reads its own new one.
internal static void parent(nint arm, nint id, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);
    var childDone = new channel<nint>(1);

    goǃ(partner, req, resp, arm, id + 1000);
    tag(arm, id);

    for (nint k = 0; k < 100; k++) {
        trip(req, resp, k);
        read(arm, id);
    }

    goǃ(reader, arm, id, childDone);
    tag(arm, id + 1);

    while (!stopped()) {
        trip(req, resp, 1);
        read(arm, id + 1);
    }

    req.ᐸꟷ(-1);
    ᐸꟷ(childDone);
    done.ᐸꟷ(id);
}

// holder keeps its carrier for a while each round. The injected arm spins past the preempt threshold while others
// are queued, so the monitor adds carriers. The hand-off arm blocks its thread in a park scope, so a carrier is
// started from inside its step. The sleep arm sleeps on a timer. The goroutines that the added carriers pick up
// belong to the other arms: that is where a bleed from a carrier's starter would be read.
internal static void holder(nint arm, nint id, nint rounds, channel<nint> done) {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);

    goǃ(partner, req, resp, arm, id + 1000);
    tag(arm, id);

    for (nint k = 0; k < rounds; k++) {
        if (arm == armInjected) {
            spin(30);
        } else if (arm == armHandoff) {
            block(20);
        } else {
            time.Sleep(2 * time.Millisecond);
        }

        read(arm, id);
        trip(req, resp, k);
        read(arm, id);
    }

    req.ᐸꟷ(-1);
    done.ᐸꟷ(id);
}

// cost is the reading: round trips between two goroutines, with or without a value written by each.
internal static void cost() {
    var req = new channel<nint>(0);
    var resp = new channel<nint>(0);
    nint n = costN();
    nint want = 0;
    nint other = 5;

    if (costTagged()) {
        want = 6;
        setTag(slotLocal, want);
    }

    goǃ(echo, req, resp, other, costTagged());

    for (nint rep = 0; rep < 5; rep++) {
        begin();

        for (nint k = 0; k < n; k++) {
            trip(req, resp, k);
        }

        end(rep, n);
    }

    check(armMain, slotLocal, want);
    req.ᐸꟷ(-1);
    report();
}

internal static void main() {
    if (costMode()) {
        cost();
        return;
    }

    var done = new channel<nint>(0);
    nint running = 0;
    nint held = 0;
    nint none = 0;
    nint mainTag = 7;
    nint mainLabels = 9;
    nint rounds = 6;
    nint naps = 20;

    // Started before main writes anything: these read zero for as long as they run.
    for (nint i = 0; i < 4; i++) {
        goǃ(reader, armBleed, none, done);
        goǃ(reader, armLabels, none, done);
        running += 2;
    }

    setTag(slotLocal, mainTag);
    setTag(slotLabels, mainLabels);

    for (nint i = 0; i < 4; i++) {
        goǃ(reader, armBleed, mainTag, done);
        goǃ(reader, armLabels, mainLabels, done);
        goǃ(writer, armLabels, 300 + 40 * i, done);
        goǃ(caller, armCallee, 500 + 40 * i, done);
        goǃ(parent, armInherit, 700 + 40 * i, done);
        goǃ(parent, armLabels, 900 + 40 * i, done);
        running += 6;
    }

    for (nint i = 0; i < 8; i++) {
        goǃ(writer, armFollow, 100 + 40 * i, done);
        running++;
    }

    for (nint i = 0; i < 4; i++) {
        if (enabled(armInjected)) {
            goǃ(holder, armInjected, 1300 + 40 * i, rounds, done);
            held++;
        }

        if (enabled(armHandoff)) {
            goǃ(holder, armHandoff, 1500 + 40 * i, rounds, done);
            held++;
        }

        if (enabled(armSleep)) {
            goǃ(holder, armSleep, 1700 + 40 * i, naps, done);
            held++;
        }
    }

    time.Sleep(50 * time.Millisecond);

    for (nint i = 0; i < held; i++) {
        ᐸꟷ(done);
    }

    stop();

    for (nint i = 0; i < running; i++) {
        ᐸꟷ(done);
    }

    check(armMain, slotLocal, mainTag);
    check(armMain, slotLabels, mainLabels);
    report();
}

} // end main_package
