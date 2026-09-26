// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using context = context_package;
using os = os_package;
using slices = slices_package;
using sync = sync_package;
using ꓸꓸꓸosꓸSignal = Span<osꓸSignal>;

partial class signal_package {


[GoType("dyn")] partial struct handlersᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    // Map a channel to the signals that should be sent to it.
    internal map<channel/*<-*/<osꓸSignal>, ж<handler>> m;
    // Map a signal to the number of channels receiving it.
    internal array<int64> @ref = new(65);
    // Map channels to signals while the channel is being stopped.
    // Not a map because entries live here only very briefly.
    // We need a separate container because we need m to correspond to ref
    // at all times, and we also need to keep track of the *handler
    // value for a channel being stopped. See the Stop function.
    internal slice<stopping> stopping;
}
internal static ж<handlersᴛ1> Ꮡhandlers = new StandardBox<handlersᴛ1>(new handlersᴛ1(nil));
internal static ref handlersᴛ1 handlers => ref Ꮡhandlers.Value;

[GoType] partial struct stopping {
    internal channel/*<-*/<osꓸSignal> c = channel/*<-*/<osꓸSignal>.SendOnly;
    internal ж<handler> h;
}

[GoType] partial struct handler {
    internal array<uint32> mask = new((numSig + 31) / 32);
}

[GoRecv] internal static bool want(this ref handler h, nint sig) {
    return (uint32)((h.mask[sig / 32].Rsh((nuint)((nint)(sig & 31)))) & 1) != 0;
}

[GoRecv] internal static void set(this ref handler h, nint sig) {
    h.mask[sig / 32] |= (uint32)(((uint32)1).Lsh((nuint)((nint)(sig & 31))));
}

[GoRecv] internal static void clear(this ref handler h, nint sig) {
    h.mask[sig / 32] &= unchecked((uint32)~(uint32)(((uint32)1).Lsh((nuint)((nint)(sig & 31)))));
}

// Stop relaying the signals, sigs, to any channels previously registered to
// receive them and either reset the signal handlers to their original values
// (action=disableSignal) or ignore the signals (action=ignoreSignal).
internal static void cancel(slice<osꓸSignal> sigs, Action<nint> action) {
    GoFrame ᒐ = default;
    try {
        Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Lock();
        defer(Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock, ref ᒐ);
        void remove(nint n) {
            handler zerohandler = new();
            foreach (var (c, h) in handlers.m) {
                if (h.want(n)) {
                    handlers.@ref[n]--;
                    h.clear(n);
                    if ((~h).mask == zerohandler.mask) {
                        delete(handlers.m, c);
                    }
                }
            }
            action(n);
        }
        if (len(sigs) == 0){
            for (nint n = 0; n < numSig; n++) {
                remove(n);
            }
        } else {
            foreach (var (_, s) in sigs) {
                remove(signum(s));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Ignore causes the provided signals to be ignored. If they are received by
// the program, nothing will happen. Ignore undoes the effect of any prior
// calls to [Notify] for the provided signals.
// If no signals are provided, all incoming signals will be ignored.
public static void Ignore(params ꓸꓸꓸosꓸSignal sigʗp) {
    var sig = sigʗp.slice();

    cancel(sig, ignoreSignal);
}

// Ignored reports whether sig is currently ignored.
public static bool Ignored(osꓸSignal sig) {
    nint sn = signum(sig);
    return sn >= 0 && signalIgnored(sn);
}

internal static ж<sync.Once> ᏑwatchSignalLoopOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once watchSignalLoopOnce => ref ᏑwatchSignalLoopOnce.Value;
internal static Action watchSignalLoop;

// Notify causes package signal to relay incoming signals to c.
// If no signals are provided, all incoming signals will be relayed to c.
// Otherwise, just the provided signals will.
//
// Package signal will not block sending to c: the caller must ensure
// that c has sufficient buffer space to keep up with the expected
// signal rate. For a channel used for notification of just one signal value,
// a buffer of size 1 is sufficient.
//
// It is allowed to call Notify multiple times with the same channel:
// each call expands the set of signals sent to that channel.
// The only way to remove signals from the set is to call [Stop].
//
// It is allowed to call Notify multiple times with different channels
// and the same signals: each channel receives copies of incoming
// signals independently.
public static void Notify(channel/*<-*/<osꓸSignal> c, params ꓸꓸꓸosꓸSignal sigʗp) {
    GoFrame ᒐ = default;
    try {
        var sig = sigʗp.sslice();

        if (c == default!) {
            throw panic("os/signal: Notify using nil channel");
        }
        Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Lock();
        defer(Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock, ref ᒐ);
        var h = handlers.m[c];
        if (h == nil) {
            if (handlers.m == default!) {
                handlers.m = new map<channel/*<-*/<osꓸSignal>, ж<handler>>();
            }
            h = @new<handler>();
            handlers.m[c] = h;
        }
        var hʗ1 = h;
        void add(nint n) {
            if (n < 0) {
                return;
            }
            if (!hʗ1.want(n)) {
                hʗ1.set(n);
                if (handlers.@ref[n] == 0) {
                    enableSignal(n);
                    // The runtime requires that we enable a
                    // signal before starting the watcher.
                    ᏑwatchSignalLoopOnce.Do(() => {
                        if (watchSignalLoop != default!) {
                            goǃ(watchSignalLoop);
                        }
                    });
                }
                handlers.@ref[n]++;
            }
        }
        if (len(sig) == 0){
            for (nint n = 0; n < numSig; n++) {
                add(n);
            }
        } else {
            foreach (var (_, s) in sig) {
                add(signum(s));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Reset undoes the effect of any prior calls to [Notify] for the provided
// signals.
// If no signals are provided, all signal handlers will be reset.
public static void Reset(params ꓸꓸꓸosꓸSignal sigʗp) {
    var sig = sigʗp.slice();

    cancel(sig, disableSignal);
}

// Stop causes package signal to stop relaying incoming signals to c.
// It undoes the effect of all prior calls to [Notify] using c.
// When Stop returns, it is guaranteed that c will receive no more signals.
public static void Stop(channel/*<-*/<osꓸSignal> c) {
    Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Lock();
    var h = handlers.m[c];
    if (h == nil) {
        Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock();
        return;
    }
    delete(handlers.m, c);
    for (nint n = 0; n < numSig; n++) {
        if (h.want(n)) {
            handlers.@ref[n]--;
            if (handlers.@ref[n] == 0) {
                disableSignal(n);
            }
        }
    }
    // Signals will no longer be delivered to the channel.
    // We want to avoid a race for a signal such as SIGINT:
    // it should be either delivered to the channel,
    // or the program should take the default action (that is, exit).
    // To avoid the possibility that the signal is delivered,
    // and the signal handler invoked, and then Stop deregisters
    // the channel before the process function below has a chance
    // to send it on the channel, put the channel on a list of
    // channels being stopped and wait for signal delivery to
    // quiesce before fully removing it.
    handlers.stopping = append(handlers.stopping, new stopping(c, h));
    Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock();
    signalWaitUntilIdle();
    Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Lock();
    foreach (var (i, s) in handlers.stopping) {
        if (s.c == c) {
            handlers.stopping = slices.Delete<slice<stopping>, stopping>(handlers.stopping, i, i + 1);
            break;
        }
    }
    Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock();
}

// Wait until there are no more signals waiting to be delivered.
// Defined by the runtime package.
[global::System.Diagnostics.StackTraceHidden] internal static void signalWaitUntilIdle() {
    go.runtime_package.signalWaitUntilIdle();
}

internal static void process(osꓸSignal sig) {
    GoFrame ᒐ = default;
    try {
        nint n = signum(sig);
        if (n < 0) {
            return;
        }
        Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Lock();
        defer(Ꮡhandlers.of(handlersᴛ1.ᏑMutex).Unlock, ref ᒐ);
        foreach (var (c, h) in handlers.m) {
            if (h.want(n)) {
                // send but do not block for it
                var selᴛ1 = c.ᐸꟷ(sig, ꓸꓸꓸ);
                switch (trySelect(selᴛ1)) {
                case 0: {
                    break;
                }
                default: {
                    break;
                }}
            }
        }
        // Avoid the race mentioned in Stop.
        foreach (var (_, d) in handlers.stopping) {
            if (d.h.want(n)) {
                var selᴛ2 = d.c.ᐸꟷ(sig, ꓸꓸꓸ);
                switch (trySelect(selᴛ2)) {
                case 0: {
                    break;
                }
                default: {
                    break;
                }}
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// NotifyContext returns a copy of the parent context that is marked done
// (its Done channel is closed) when one of the listed signals arrives,
// when the returned stop function is called, or when the parent context's
// Done channel is closed, whichever happens first.
//
// The stop function unregisters the signal behavior, which, like [signal.Reset],
// may restore the default behavior for a given signal. For example, the default
// behavior of a Go program receiving [os.Interrupt] is to exit. Calling
// NotifyContext(parent, os.Interrupt) will change the behavior to cancel
// the returned context. Future interrupts received will not trigger the default
// (exit) behavior until the returned stop function is called.
//
// The stop function releases resources associated with it, so code should
// call stop as soon as the operations running in this Context complete and
// signals no longer need to be diverted to the context.
public static (context.Context ctx, Action stop) NotifyContext(context.Context parent, params ꓸꓸꓸosꓸSignal signalsʗp) {
    context.Context ctx = default!;
    var signals = signalsʗp.slice();

    (ctx, var cancel) = context.WithCancel(parent);
    var c = Ꮡ(new signalCtx(
        Context: ctx,
        cancel: cancel,
        signals: signals
    ));
    c.Value.ch = new channel<osꓸSignal>(1);
    Notify((~c).ch.WithDirection(GoChanDir.Send), (~c).signals.ꓸꓸꓸ);
    if (ctx.Err() == default!) {
        var cʗ1 = c;
        goǃ(() => {
            var selᴛ3 = (~cʗ1).ch;
            var selᴛ4 = (~cʗ1).Context.Done();
            switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                (~cʗ1).cancel();
                break;
            }
            case 1 when selᴛ4.ꟷᐳ(out _): {
                break;
            }}
        });
    }
    return (new signalCtxжContext(c), c.stop);
}

[GoType] partial struct signalCtx {
    public context_package.Context Context;
    internal Action cancel;
    internal slice<osꓸSignal> signals;
    internal channel<osꓸSignal> ch;
}

[GoRecv] internal static void stop(this ref signalCtx c) {
    c.cancel();
    Stop(c.ch.WithDirection(GoChanDir.Send));
}

[GoType] partial interface stringer {
    @string String();
}

[GoRecv] internal static @string String(this ref signalCtx c) {
    slice<byte> buf = default!;
    // We know that the type of c.Context is context.cancelCtx, and we know that the
    // String method of cancelCtx returns a string that ends with ".WithCancel".
    @string name = c.Context._<stringer>().String();
    name = name[..(int)(len(name) - len(".WithCancel"))];
    buf = append(buf, ((@string)("signal.NotifyContext("u8 + name)).ꓸꓸꓸ);
    if (len(c.signals) != 0) {
        buf = append(buf, ((@string)", ["u8).ꓸꓸꓸ);
        foreach (var (i, s) in c.signals) {
            buf = append(buf, s.String().ꓸꓸꓸ);
            if (i != len(c.signals) - 1) {
                buf = append(buf, (byte)((rune)' '));
            }
        }
        buf = append(buf, (byte)((rune)']'));
    }
    buf = append(buf, (byte)((rune)')'));
    return ((@string)buf);
}

} // end signal_package
