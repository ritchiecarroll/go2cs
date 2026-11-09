// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sync;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using sync = sync_package;
using System.Runtime.CompilerServices;
using go.runtime;

partial class singleflight_package {

internal static error errGoexit = errors.New("runtime.Goexit was called");

partial struct panicError {
    internal any value;
    internal slice<byte> stack;
}

internal static @string Error(this ref panicError p) {
    return fmt.Sprintf("%v\n\n%s"u8, p.value, p.stack);
}

internal static error Unwrap(this ref panicError p) {
    var (err, ok) = p.value._<error>(ᐧ);
    if (!ok) {
        return default;
    }

    return err;
}

internal static partial error newPanicError(any v) {
    var stack = debug.Stack();

    nint line = bytes.IndexByte(stack[..], (byte)'\n'); if (line >= 0) {
        stack = stack.slice(line + 1);
    }
    return new panicErrorжerror(Ꮡ(new panicError(value: v, stack: stack)));
}

partial struct call {
    internal sync.WaitGroup wg;
    internal any val;
    internal error err;
    internal nint dups;
    internal slice<channel/*<-*/<Result>> chans;
}

partial struct Group {
    internal sync.Mutex mu;
    internal map<@string, ж<call>> m;
}

partial struct Result {
    public any Val;
    public error Err;
    public bool Shared;
}

public static (any v, error err, bool shared) Do(this ж<Group> Ꮡg, @string key, Func<(any, error)> fn) {
    ref var g = ref Ꮡg.DerefOrNull();

    g.mu.Lock();
    if (g.m == nil) {
        g.m = new map<@string, ж<call>>();
    }
    var (cΔ1, ok) = g.m[key, ꟷ]; if (ok) {
        cΔ1.dups++;
        g.mu.Unlock();
        cΔ1.Ꮡwg.Wait();

        var (e, okΔ1) = cΔ1.err._<ж<panicError>>(ᐧ); if (okΔ1) {
            throw panic(e.OrTypedNil());
        } else if (AreEqual(cΔ1.err, errGoexit)) {
            runtime.Goexit();
        }
        return (cΔ1.val, cΔ1.err, true);
    }
    var c = @new<call>();
    c.Ꮡwg.Add(1);
    g.m[key] = c;
    g.mu.Unlock();

    Ꮡg.doCall(c, key, fn);
    return (c.val, c.err, c.dups > 0);
}

public static partial /*<-*/channel<Result> DoChan(this ж<Group> Ꮡg, @string key, Func<(any, error)> fn) {
    ref var g = ref Ꮡg.DerefOrNull();

    var ch = new channel<Result>(1);
    g.mu.Lock();
    if (g.m == nil) {
        g.m = new map<@string, ж<call>>();
    }
    var (cΔ1, ok) = g.m[key, ꟷ]; if (ok) {
        cΔ1.dups++;
        cΔ1.chans = append(cΔ1.chans, ch.AsSendOnly());
        g.mu.Unlock();
        return ch.AsRecvOnly();
    }
    var c = Ꮡ(new call(chans: [ch]));
    c.Ꮡwg.Add(1);
    g.m[key] = c;
    g.mu.Unlock();

    goǃ(Ꮡg.doCall, c, key, fn);

    return ch.AsRecvOnly();
}

internal static void doCall(this ж<Group> Ꮡg, ж<call> c, @string key, Func<(any, error)> fn) {
    GoFrame ᒐ = default;
    try {
        var normalReturn = false;
        var recovered = false;

        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            GoFrame ᒐ = default;
            try {
                if (!normalReturn && !recovered) {
                    c.Value.err = errGoexit;
                }

                Ꮡg.Value.mu.Lock();
                defer(Ꮡg.Ꮡmu.Unlock, ref ᒐ);
                c.Ꮡwg.Done();
                if (Ꮡg.Value.m[key] == c) {
                    delete(Ꮡg.Value.m, key);
                }

                {
                    var (e, ok) = c.Value.err._<ж<panicError>>(ᐧ); if (ok) {
                        if (len(c.Value.chans) > 0) {
                            goǃ(ᴛ1 => throw panic(ᴛ1), e.OrTypedNil());
                            select();
                        } else {
                            throw panic(e.OrTypedNil());
                        }
                    } else if (AreEqual(c.Value.err, errGoexit)) {
                    } else {
                        foreach (var (_, ch) in range(c.Value.chans)) {
                            ch.ᐸꟷ(new Result(c.Value.val, c.Value.err, c.Value.dups > 0));
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);

        func(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    if (!normalReturn) {
                        var r = recover(); if (r != nil) {
                            c.Value.err = newPanicError(r);
                        }
                    }
                }, ref ᒐ);

                (c.Value.val, c.Value.err) = fn();
                normalReturn = true;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })();

        if (!normalReturn) {
            recovered = true;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void Forget(this ж<Group> Ꮡg, @string key) {
    ref var g = ref Ꮡg.DerefOrNull();

    g.mu.Lock();
    delete(g.m, key);
    g.mu.Unlock();
}

} // end singleflight_package
