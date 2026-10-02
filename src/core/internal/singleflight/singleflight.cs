// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package singleflight provides a duplicate function call suppression
// mechanism.
namespace go.@internal;

using sync = go.sync_package;
using System.Runtime.CompilerServices;
using go;

partial class singleflight_package {

// call is an in-flight or completed singleflight.Do call
[GoType] partial struct call {
    internal sync.WaitGroup wg;
    // These fields are written once before the WaitGroup is done
    // and are only read after the WaitGroup is done.
    internal any val;
    internal error err;
    // These fields are read and written with the singleflight
    // mutex held before the WaitGroup is done, and are read but
    // not written after the WaitGroup is done.
    internal nint dups;
    internal slice<channel/*<-*/<Result>> chans;
}

// Group represents a class of work and forms a namespace in
// which units of work can be executed with duplicate suppression.
[GoType] partial struct Group {
    internal sync.Mutex mu;       // protects m
    internal map<@string, ж<call>> m; // lazily initialized
}

// Result holds the results of Do, so they can be passed
// on a channel.
[GoType] partial struct Result {
    public any Val;
    public error Err;
    public bool Shared;
}

// Do executes and returns the results of the given function, making
// sure that only one execution is in-flight for a given key at a
// time. If a duplicate comes in, the duplicate caller waits for the
// original to complete and receives the same results.
// The return value shared indicates whether v was given to multiple callers.
public static (any v, error err, bool shared) Do(this ж<Group> Ꮡg, @string key, Func<(any, error)> fn) {
    ref var g = ref Ꮡg.DerefOrNull();

    g.mu.Lock();
    if (g.m == default!) {
        g.m = new map<@string, ж<call>>();
    }
    {
        var (cΔ1, ok) = g.m[key, ꟷ]; if (ok) {
            cΔ1.Value.dups++;
            g.mu.Unlock();
            cΔ1.of(call.Ꮡwg).Wait();
            return ((~cΔ1).val, (~cΔ1).err, true);
        }
    }
    var c = @new<call>();
    c.of(call.Ꮡwg).Add(1);
    g.m[key] = c;
    g.mu.Unlock();
    Ꮡg.doCall(c, key, fn);
    return ((~c).val, (~c).err, (~c).dups > 0);
}

// DoChan is like Do but returns a channel that will receive the
// results when they are ready.
[MethodImpl(MethodImplOptions.NoInlining)] public static /*<-*/channel<Result> DoChan(this ж<Group> Ꮡg, @string key, Func<(any, error)> fn) {
    ref var g = ref Ꮡg.DerefOrNull();

    var ch = new channel<Result>(1);
    g.mu.Lock();
    if (g.m == default!) {
        g.m = new map<@string, ж<call>>();
    }
    {
        var (cΔ1, ok) = g.m[key, ꟷ]; if (ok) {
            cΔ1.Value.dups++;
            cΔ1.Value.chans = append((~cΔ1).chans, ch.WithDirection(GoChanDir.Send));
            g.mu.Unlock();
            return ch.WithDirection(GoChanDir.Recv);
        }
    }
    var c = Ꮡ(new call(chans: new channel/*<-*/<Result>[]{ch}.slice()));
    c.of(call.Ꮡwg).Add(1);
    g.m[key] = c;
    g.mu.Unlock();
    goǃ(Ꮡg.doCall, c, key, fn);
    return ch.WithDirection(GoChanDir.Recv);
}

// doCall handles the single call for a key.
internal static void doCall(this ж<Group> Ꮡg, ж<call> Ꮡc, @string key, Func<(any, error)> fn) {
    ref var g = ref Ꮡg.DerefOrNull();
    ref var c = ref Ꮡc.DerefOrNull();

    (c.val, c.err) = fn();
    g.mu.Lock();
    Ꮡc.of(call.Ꮡwg).Done();
    if (g.m[key] == Ꮡc) {
        delete(g.m, key);
    }
    foreach (var (_, ch) in c.chans) {
        ch.ᐸꟷ(new Result(c.val, c.err, c.dups > 0));
    }
    g.mu.Unlock();
}

// ForgetUnshared tells the singleflight to forget about a key if it is not
// shared with any other goroutines. Future calls to Do for a forgotten key
// will call the function rather than waiting for an earlier call to complete.
// Returns whether the key was forgotten or unknown--that is, whether no
// other goroutines are waiting for the result.
public static bool ForgetUnshared(this ж<Group> Ꮡg, @string key) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var g = ref Ꮡg.DerefOrNull();

        g.mu.Lock();
        ᒐd1 = true;
        var (c, ok) = g.m[key, ꟷ];
        if (!ok) {
            return true;
        }
        if ((~c).dups == 0) {
            delete(g.m, key);
            return true;
        }
        return false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡg.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

} // end singleflight_package
