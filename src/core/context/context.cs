// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package context defines the Context type, which carries deadlines,
// cancellation signals, and other request-scoped values across API boundaries
// and between processes.
//
// Incoming requests to a server should create a [Context], and outgoing
// calls to servers should accept a Context. The chain of function
// calls between them must propagate the Context, optionally replacing
// it with a derived Context created using [WithCancel], [WithDeadline],
// [WithTimeout], or [WithValue].
//
// A Context may be canceled to indicate that work done on its behalf should stop.
// A Context with a deadline is canceled after the deadline passes.
// When a Context is canceled, all Contexts derived from it are also canceled.
//
// The [WithCancel], [WithDeadline], and [WithTimeout] functions take a
// Context (the parent) and return a derived Context (the child) and a
// [CancelFunc]. Calling the CancelFunc directly cancels the child and its
// children, removes the parent's reference to the child, and stops
// any associated timers. Failing to call the CancelFunc leaks the
// child and its children until the parent is canceled. The go vet tool
// checks that CancelFuncs are used on all control-flow paths.
//
// The [WithCancelCause], [WithDeadlineCause], and [WithTimeoutCause] functions
// return a [CancelCauseFunc], which takes an error and records it as
// the cancellation cause. Calling [Cause] on the canceled context
// or any of its children retrieves the cause. If no cause is specified,
// Cause(ctx) returns the same value as ctx.Err().
//
// Programs that use Contexts should follow these rules to keep interfaces
// consistent across packages and enable static analysis tools to check context
// propagation:
//
// Do not store Contexts inside a struct type; instead, pass a Context
// explicitly to each function that needs it. This is discussed further in
// https://go.dev/blog/context-and-structs. The Context should be the first
// parameter, typically named ctx:
//
//	func DoSomething(ctx context.Context, arg Arg) error {
//		// ... use ctx ...
//	}
//
// Do not pass a nil [Context], even if a function permits it. Pass [context.TODO]
// if you are unsure about which Context to use.
//
// Use context Values only for request-scoped data that transits processes and
// APIs, not for passing optional parameters to functions.
//
// The same Context may be passed to functions running in different goroutines;
// Contexts are safe for simultaneous use by multiple goroutines.
//
// See https://go.dev/blog/context for example code for a server that uses
// Contexts.
namespace go;

using errors = errors_package;
using reflectlite = @internal.reflectlite_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @internal;
using System.Runtime.CompilerServices;
using go.sync;

partial class context_package {

// A Context carries a deadline, a cancellation signal, and other values across
// API boundaries.
//
// Context's methods may be called by multiple goroutines simultaneously.
[GoType] partial interface Context {
    // Deadline returns the time when work done on behalf of this context
    // should be canceled. Deadline returns ok==false when no deadline is
    // set. Successive calls to Deadline return the same results.
    (time.Time deadline, bool ok) Deadline();
    // Done returns a channel that's closed when work done on behalf of this
    // context should be canceled. Done may return nil if this context can
    // never be canceled. Successive calls to Done return the same value.
    // The close of the Done channel may happen asynchronously,
    // after the cancel function returns.
    //
    // WithCancel arranges for Done to be closed when cancel is called;
    // WithDeadline arranges for Done to be closed when the deadline
    // expires; WithTimeout arranges for Done to be closed when the timeout
    // elapses.
    //
    // Done is provided for use in select statements:
    //
    //  // Stream generates values with DoSomething and sends them to out
    //  // until DoSomething returns an error or ctx.Done is closed.
    //  func Stream(ctx context.Context, out chan<- Value) error {
    //  	for {
    //  		v, err := DoSomething(ctx)
    //  		if err != nil {
    //  			return err
    //  		}
    //  		select {
    //  		case <-ctx.Done():
    //  			return ctx.Err()
    //  		case out <- v:
    //  		}
    //  	}
    //  }
    //
    // See https://blog.golang.org/pipelines for more examples of how to use
    // a Done channel for cancellation.
    /*<-*/channel<EmptyStruct> Done();
    // If Done is not yet closed, Err returns nil.
    // If Done is closed, Err returns a non-nil error explaining why:
    // DeadlineExceeded if the context's deadline passed,
    // or Canceled if the context was canceled for some other reason.
    // After Err returns a non-nil error, successive calls to Err return the same error.
    error Err();
    // Value returns the value associated with this context for key, or nil
    // if no value is associated with key. Successive calls to Value with
    // the same key returns the same result.
    //
    // Use context values only for request-scoped data that transits
    // processes and API boundaries, not for passing optional parameters to
    // functions.
    //
    // A key identifies a specific value in a Context. Functions that wish
    // to store values in Context typically allocate a key in a global
    // variable then use that key as the argument to context.WithValue and
    // Context.Value. A key can be any type that supports equality;
    // packages should define keys as an unexported type to avoid
    // collisions.
    //
    // Packages that define a Context key should provide type-safe accessors
    // for the values stored using that key:
    //
    // 	// Package user defines a User type that's stored in Contexts.
    // 	package user
    //
    // 	import "context"
    //
    // 	// User is the type of value stored in the Contexts.
    // 	type User struct {...}
    //
    // 	// key is an unexported type for keys defined in this package.
    // 	// This prevents collisions with keys defined in other packages.
    // 	type key int
    //
    // 	// userKey is the key for user.User values in Contexts. It is
    // 	// unexported; clients use user.NewContext and user.FromContext
    // 	// instead of using this key directly.
    // 	var userKey key
    //
    // 	// NewContext returns a new Context that carries value u.
    // 	func NewContext(ctx context.Context, u *User) context.Context {
    // 		return context.WithValue(ctx, userKey, u)
    // 	}
    //
    // 	// FromContext returns the User value stored in ctx, if any.
    // 	func FromContext(ctx context.Context) (*User, bool) {
    // 		u, ok := ctx.Value(userKey).(*User)
    // 		return u, ok
    // 	}
    any Value(any key);
}

// Canceled is the error returned by [Context.Err] when the context is canceled
// for some reason other than its deadline passing.
public static error Canceled = errors.New("context canceled"u8);

// DeadlineExceeded is the error returned by [Context.Err] when the context is canceled
// due to its deadline passing.
public static error DeadlineExceeded = new deadlineExceededError(nil);

[GoType] partial struct deadlineExceededError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextDeadlineExceededˢ = "context deadline exceeded"u8;

internal static @string Error(this deadlineExceededError _) {
    return contextDeadlineExceededˢ;
}

internal static bool Timeout(this deadlineExceededError _) {
    return true;
}

internal static bool Temporary(this deadlineExceededError _) {
    return true;
}

// An emptyCtx is never canceled, has no values, and has no deadline.
// It is the common base of backgroundCtx and todoCtx.
[GoType] partial struct emptyCtx {
}

internal static (time.Time deadline, bool ok) Deadline(this emptyCtx _) {
    time.Time deadline = default!;
    bool ok = default!;

    return (deadline, ok);
}

internal static /*<-*/channel<EmptyStruct> Done(this emptyCtx _) {
    return default!;
}

internal static error Err(this emptyCtx _) {
    return default!;
}

internal static any Value(this emptyCtx _, any key) {
    return default!;
}

[GoType] partial struct backgroundCtx {
    internal partial ref emptyCtx emptyCtx { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextBackgroundˢ = "context.Background"u8;

internal static @string String(this backgroundCtx _) {
    return contextBackgroundˢ;
}

[GoType] partial struct todoCtx {
    internal partial ref emptyCtx emptyCtx { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextTodoˢ = "context.TODO"u8;

internal static @string String(this todoCtx _) {
    return contextTodoˢ;
}

// Background returns a non-nil, empty [Context]. It is never canceled, has no
// values, and has no deadline. It is typically used by the main function,
// initialization, and tests, and as the top-level Context for incoming
// requests.
public static Context Background() {
    return new backgroundCtx(nil);
}

// TODO returns a non-nil, empty [Context]. Code should use context.TODO when
// it's unclear which Context to use or it is not yet available (because the
// surrounding function has not yet been extended to accept a Context
// parameter).
public static Context TODO() {
    return new todoCtx(nil);
}

// type CancelFunc is a methodless func type — rendered inline as its base delegate

// WithCancel returns a derived context that points to the parent context
// but has a new Done channel. The returned context's Done channel is closed
// when the returned cancel function is called or when the parent context's
// Done channel is closed, whichever happens first.
//
// Canceling this context releases resources associated with it, so code should
// call cancel as soon as the operations running in this [Context] complete.
public static (Context ctx, Action cancel) WithCancel(Context parent) {
    var c = withCancel(parent);
    var cʗ1 = c;
    return (new cancelCtxжContext(c), () => {
        cʗ1.cancel(true, Canceled, default!);
    });
}

// type CancelCauseFunc is a methodless func type — rendered inline as its base delegate

// WithCancelCause behaves like [WithCancel] but returns a [CancelCauseFunc] instead of a [CancelFunc].
// Calling cancel with a non-nil error (the "cause") records that error in ctx;
// it can then be retrieved using Cause(ctx).
// Calling cancel with nil sets the cause to Canceled.
//
// Example use:
//
//	ctx, cancel := context.WithCancelCause(parent)
//	cancel(myError)
//	ctx.Err() // returns context.Canceled
//	context.Cause(ctx) // returns myError
public static (Context ctx, Action<error> cancel) WithCancelCause(Context parent) {
    var c = withCancel(parent);
    var cʗ1 = c;
    return (new cancelCtxжContext(c), (error cause) => {
        cʗ1.cancel(true, Canceled, cause);
    });
}

internal static ж<cancelCtx> withCancel(Context parent) {
    if (parent == default!) {
        throw panic("cannot create context from nil parent");
    }
    var c = Ꮡ(new cancelCtx(nil));
    c.propagateCancel(parent, new cancelCtxжcanceler(c));
    return c;
}

// Cause returns a non-nil error explaining why c was canceled.
// The first cancellation of c or one of its parents sets the cause.
// If that cancellation happened via a call to CancelCauseFunc(err),
// then [Cause] returns err.
// Otherwise Cause(c) returns the same value as c.Err().
// Cause returns nil if c has not been canceled yet.
public static error Cause(Context c) {
    GoFrame ᒐ = default;
    try {
        {
            var (cc, ok) = c.Value(ᏑcancelCtxKey)._<ж<cancelCtx>>(ᐧ); if (ok) {
                cc.of(cancelCtx.Ꮡmu).Lock();
                var ccʗ1 = cc;
                defer(ccʗ1.of(cancelCtx.Ꮡmu).Unlock, ref ᒐ);
                return (~cc).cause;
            }
        }
        // There is no cancelCtxKey value, so we know that c is
        // not a descendant of some Context created by WithCancelCause.
        // Therefore, there is no specific cause to return.
        // If this is not one of the standard Context types,
        // it might still have an error even though it won't have a cause.
        return c.Err();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// AfterFunc arranges to call f in its own goroutine after ctx is canceled.
// If ctx is already canceled, AfterFunc calls f immediately in its own goroutine.
//
// Multiple calls to AfterFunc on a context operate independently;
// one does not replace another.
//
// Calling the returned stop function stops the association of ctx with f.
// It returns true if the call stopped f from being run.
// If stop returns false,
// either the context is canceled and f has been started in its own goroutine;
// or f was already stopped.
// The stop function does not wait for f to complete before returning.
// If the caller needs to know whether f is completed,
// it must coordinate with f explicitly.
//
// If ctx has a "AfterFunc(func()) func() bool" method,
// AfterFunc will use it to schedule the call.
public static Func<bool> /*stop*/ AfterFunc(Context ctx, Action f) {
    var a = Ꮡ(new afterFuncCtx(
        f: f
    ));
    a.of(afterFuncCtx.ᏑcancelCtx).propagateCancel(ctx, new afterFuncCtxжcanceler(a));
    var aʗ1 = a;
    return () => {
        var stopped = false;
        aʗ1.of(afterFuncCtx.Ꮡonce).Do(() => {
            stopped = true;
        });
        if (stopped) {
            aʗ1.cancel(true, Canceled, default!);
        }
        return stopped;
    };
}

[GoType] partial interface afterFuncer {
    Func<bool> AfterFunc(Action _);
}

[GoType] partial struct afterFuncCtx {
    internal partial ref cancelCtx cancelCtx { get; }
    internal Δsync.Once once; // either starts running f or stops f from running
    internal Action f;
}

internal static void cancel(this ж<afterFuncCtx> Ꮡa, bool removeFromParent, error err, error cause) {
    ref var a = ref Ꮡa.DerefOrNull();

    Ꮡa.of(afterFuncCtx.ᏑcancelCtx).cancel(false, err, cause);
    if (removeFromParent) {
        removeChild(a.Context, new afterFuncCtxжcanceler(Ꮡa));
    }
    Ꮡa.of(afterFuncCtx.Ꮡonce).Do([MethodImpl(MethodImplOptions.NoInlining)] () => {
        goǃ(Ꮡa.Value.f);
    });
}

// A stopCtx is used as the parent context of a cancelCtx when
// an AfterFunc has been registered with the parent.
// It holds the stop function used to unregister the AfterFunc.
[GoType] partial struct stopCtx {
    [GoEmbedded] public Context Context;
    internal Func<bool> stop;
}

// goroutines counts the number of goroutines ever created; for testing.
internal static ж<atomic.Int32> Ꮡgoroutines = new StandardBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 goroutines => ref Ꮡgoroutines.Value;

// &cancelCtxKey is the key that a cancelCtx returns itself for.
internal static ж<nint> ᏑcancelCtxKey = new StandardBox<nint>(default(nint));
internal static ref nint cancelCtxKey => ref ᏑcancelCtxKey.Value;

// parentCancelCtx returns the underlying *cancelCtx for parent.
// It does this by looking up parent.Value(&cancelCtxKey) to find
// the innermost enclosing *cancelCtx and then checking whether
// parent.Done() matches that *cancelCtx. (If not, the *cancelCtx
// has been wrapped in a custom implementation providing a
// different done channel, in which case we should not bypass it.)
internal static (ж<cancelCtx>, bool) parentCancelCtx(Context parent) {
    var done = parent.Done();
    if (done == closedchan || done == default!) {
        return (default!, false);
    }
    var (p, ok) = parent.Value(ᏑcancelCtxKey)._<ж<cancelCtx>>(ᐧ);
    if (!ok) {
        return (default!, false);
    }
    var (pdone, _) = p.of(cancelCtx.Ꮡdone).Load()._<channel<EmptyStruct>>(ᐧ);
    if (pdone != done) {
        return (default!, false);
    }
    return (p, true);
}

// removeChild removes a context from its parent.
internal static void removeChild(Context parent, canceler child) {
    {
        var (s, okΔ1) = parent._<stopCtx>(ᐧ); if (okΔ1) {
            s.stop();
            return;
        }
    }
    var (p, ok) = parentCancelCtx(parent);
    if (!ok) {
        return;
    }
    p.of(cancelCtx.Ꮡmu).Lock();
    if ((~p).children != default!) {
        delete((~p).children, child);
    }
    p.of(cancelCtx.Ꮡmu).Unlock();
}

// A canceler is a context type that can be canceled directly. The
// implementations are *cancelCtx and *timerCtx.
[GoType] partial interface canceler {
    void cancel(bool removeFromParent, error err, error cause);
    /*<-*/channel<EmptyStruct> Done();
}

// closedchan is a reusable closed channel.
internal static channel<EmptyStruct> closedchan = new channel<EmptyStruct>(0);

[GoInit] internal static void init() {
    close(closedchan);
}

// A cancelCtx can be canceled. When canceled, it also cancels any children
// that implement canceler.
[GoType] partial struct cancelCtx {
    [GoEmbedded] public Context Context;
    internal Δsync.Mutex mu;            // protects following fields
    internal atomic.Value done;          // of chan struct{}, created lazily, closed by first cancel call
    internal map<canceler, EmptyStruct> children; // set to nil by the first cancel call
    internal error err;                 // set to non-nil by the first cancel call
    internal error cause;                 // set to non-nil by the first cancel call
}

// Go method set entry for the promoted 'Context.Deadline()' - provided ONLY by the embedded
// interface field in *cancelCtx's method set; see the pointer-only satisfaction record.
internal static (time.Time, bool) Deadline(this cancelCtx recvᴛ) => recvᴛ.Context.Deadline();

internal static any Value(this ж<cancelCtx> Ꮡc, any key) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (key == ᏑcancelCtxKey) {
        return Ꮡc;
    }
    return value(c.Context, key);
}

internal static /*<-*/channel<EmptyStruct> Done(this ж<cancelCtx> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var d = Ꮡc.of(cancelCtx.Ꮡdone).Load();
        if (d != default!) {
            return d._<channel<EmptyStruct>>().WithDirection(GoChanDir.Recv);
        }
        c.mu.Lock();
        ᒐd1 = true;
        d = Ꮡc.of(cancelCtx.Ꮡdone).Load();
        if (d == default!) {
            d = new channel<EmptyStruct>(0);
            Ꮡc.of(cancelCtx.Ꮡdone).Store(d);
        }
        return d._<channel<EmptyStruct>>().WithDirection(GoChanDir.Recv);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error Err(this ж<cancelCtx> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.mu.Lock();
    var err = c.err;
    c.mu.Unlock();
    return err;
}

// propagateCancel arranges for child to be canceled when parent is.
// It sets the parent context of cancelCtx.
[MethodImpl(MethodImplOptions.NoInlining)] internal static void propagateCancel(this ж<cancelCtx> Ꮡc, Context parent, canceler child) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.Context = parent;
    var done = parent.Done();
    if (done == default!) {
        return; // parent is never canceled
    }
    var selᴛ1 = done;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        child.cancel(false, // parent is already canceled
 parent.Err(), Cause(parent));
        return;
    }
    default: {
        break;
    }}
    {
        var (p, ok) = parentCancelCtx(parent); if (ok) {
            // parent is a *cancelCtx, or derives from one.
            p.of(cancelCtx.Ꮡmu).Lock();
            if ((~p).err != default!){
                // parent has already been canceled
                child.cancel(false, (~p).err, (~p).cause);
            } else {
                if ((~p).children == default!) {
                    p.Value.children = new map<canceler, EmptyStruct>();
                }
                p.Value.children[child] = new EmptyStruct();
            }
            p.of(cancelCtx.Ꮡmu).Unlock();
            return;
        }
    }
    {
        var (a, ok) = parent._<afterFuncer>(ᐧ); if (ok) {
            // parent implements an AfterFunc method.
            c.mu.Lock();
            var stop = a.AfterFunc(() => {
                child.cancel(false, parent.Err(), Cause(parent));
            });
            c.Context = new stopCtx(
                Context: parent,
                stop: stop
            );
            c.mu.Unlock();
            return;
        }
    }
    Ꮡgoroutines.Add(1);
    goǃ(() => {
        var selᴛ2 = parent.Done();
        var selᴛ3 = child.Done();
        switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            child.cancel(false, parent.Err(), Cause(parent));
            break;
        }
        case 1 when selᴛ3.ꟷᐳ(out _): {
            break;
        }}
    });
}

[GoType] partial interface stringer {
    @string String();
}

internal static @string contextName(Context c) {
    {
        var (s, ok) = c._<stringer>(ᐧ); if (ok) {
            return s.String();
        }
    }
    return reflectlite.TypeOf(c).String();
}

[GoRecv] internal static @string String(this ref cancelCtx c) {
    return contextName(c.Context) + ".WithCancel"u8;
}

// cancel closes c.done, cancels each of c's children, and, if
// removeFromParent is true, removes c from its parent's children.
// cancel sets c.cause to cause if this is the first time c is canceled.
internal static void cancel(this ж<cancelCtx> Ꮡc, bool removeFromParent, error err, error cause) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (err == default!) {
        throw panic("context: internal error: missing cancel error");
    }
    if (cause == default!) {
        cause = err;
    }
    c.mu.Lock();
    if (c.err != default!) {
        c.mu.Unlock();
        return; // already canceled
    }
    c.err = err;
    c.cause = cause;
    var (d, _) = Ꮡc.of(cancelCtx.Ꮡdone).Load()._<channel<EmptyStruct>>(ᐧ);
    if (d == default!){
        Ꮡc.of(cancelCtx.Ꮡdone).Store(closedchan);
    } else {
        close(d);
    }
    foreach (var (child, _) in c.children) {
        // NOTE: acquiring the child's lock while holding parent's lock.
        child.cancel(false, err, cause);
    }
    c.children = default!;
    c.mu.Unlock();
    if (removeFromParent) {
        removeChild(c.Context, new cancelCtxжcanceler(Ꮡc));
    }
}

// WithoutCancel returns a derived context that points to the parent context
// and is not canceled when parent is canceled.
// The returned context returns no Deadline or Err, and its Done channel is nil.
// Calling [Cause] on the returned context returns nil.
public static Context WithoutCancel(Context parent) {
    if (parent == default!) {
        throw panic("cannot create context from nil parent");
    }
    return new withoutCancelCtx(parent);
}

[GoType] partial struct withoutCancelCtx {
    internal Context c;
}

internal static (time.Time deadline, bool ok) Deadline(this withoutCancelCtx _) {
    time.Time deadline = default!;
    bool ok = default!;

    return (deadline, ok);
}

internal static /*<-*/channel<EmptyStruct> Done(this withoutCancelCtx _) {
    return default!;
}

internal static error Err(this withoutCancelCtx _) {
    return default!;
}

internal static any Value(this withoutCancelCtx c, any key) {
    return value(c, key);
}

internal static @string String(this withoutCancelCtx c) {
    return contextName(c.c) + ".WithoutCancel"u8;
}

// WithDeadline returns a derived context that points to the parent context
// but has the deadline adjusted to be no later than d. If the parent's
// deadline is already earlier than d, WithDeadline(parent, d) is semantically
// equivalent to parent. The returned [Context.Done] channel is closed when
// the deadline expires, when the returned cancel function is called,
// or when the parent context's Done channel is closed, whichever happens first.
//
// Canceling this context releases resources associated with it, so code should
// call cancel as soon as the operations running in this [Context] complete.
public static (Context, Action) WithDeadline(Context parent, time.Time d) {
    return WithDeadlineCause(parent, d, default!);
}

// WithDeadlineCause behaves like [WithDeadline] but also sets the cause of the
// returned Context when the deadline is exceeded. The returned [CancelFunc] does
// not set the cause.
public static (Context, Action) WithDeadlineCause(Context parent, time.Time d, error cause) {
    GoFrame ᒐ = default;
    try {
        if (parent == default!) {
            throw panic("cannot create context from nil parent");
        }
        {
            var (cur, ok) = parent.Deadline(); if (ok && cur.Before(d)) {
                // The current deadline is already sooner than the new one.
                return WithCancel(parent);
            }
        }
        var c = Ꮡ(new timerCtx(
            deadline: d
        ));
        c.of(timerCtx.ᏑcancelCtx).propagateCancel(parent, new timerCtxжcanceler(c));
        var dur = time.Until(d);
        if (dur <= 0) {
            c.cancel(true, DeadlineExceeded, cause); // deadline has already passed
            var cʗ1 = c;
            return (new timerCtxжContext(c), () => {
                cʗ1.cancel(false, Canceled, default!);
            });
        }
        c.of(timerCtx.Ꮡmu).Lock();
        var cʗ2 = c;
        defer(cʗ2.of(timerCtx.Ꮡmu).Unlock, ref ᒐ);
        if ((~c).err == default!) {
            var cʗ3 = c;
            c.Value.timer = time.AfterFunc(dur, () => {
                cʗ3.cancel(true, DeadlineExceeded, cause);
            });
        }
        var cʗ4 = c;
        return (new timerCtxжContext(c), () => {
            cʗ4.cancel(true, Canceled, default!);
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// A timerCtx carries a timer and a deadline. It embeds a cancelCtx to
// implement Done and Err. It implements cancel by stopping its timer then
// delegating to cancelCtx.cancel.
[GoType] partial struct timerCtx {
    internal partial ref cancelCtx cancelCtx { get; }
    internal ж<time.Timer> timer; // Under cancelCtx.mu.
    internal time.Time deadline;
}

[GoRecv] internal static (time.Time deadline, bool ok) Deadline(this ref timerCtx c) {
    return (c.deadline, true);
}

[GoRecv] internal static @string String(this ref timerCtx c) {
    return contextName(c.cancelCtx.Context) + ".WithDeadline("u8 + c.deadline.String() + " ["u8 + time.Until(c.deadline).String() + "])"u8;
}

internal static void cancel(this ж<timerCtx> Ꮡc, bool removeFromParent, error err, error cause) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.of(timerCtx.ᏑcancelCtx).cancel(false, err, cause);
    if (removeFromParent) {
        // Remove this timerCtx from its parent cancelCtx's children.
        removeChild(c.cancelCtx.Context, new timerCtxжcanceler(Ꮡc));
    }
    c.mu.Lock();
    if (c.timer != nil) {
        c.timer.Stop();
        c.timer = default!;
    }
    c.mu.Unlock();
}

// WithTimeout returns WithDeadline(parent, time.Now().Add(timeout)).
//
// Canceling this context releases resources associated with it, so code should
// call cancel as soon as the operations running in this [Context] complete:
//
//	func slowOperationWithTimeout(ctx context.Context) (Result, error) {
//		ctx, cancel := context.WithTimeout(ctx, 100*time.Millisecond)
//		defer cancel()  // releases resources if slowOperation completes before timeout elapses
//		return slowOperation(ctx)
//	}
public static (Context, Action) WithTimeout(Context parent, time.Duration timeout) {
    return WithDeadline(parent, time.Now().Add(timeout));
}

// WithTimeoutCause behaves like [WithTimeout] but also sets the cause of the
// returned Context when the timeout expires. The returned [CancelFunc] does
// not set the cause.
public static (Context, Action) WithTimeoutCause(Context parent, time.Duration timeout, error cause) {
    return WithDeadlineCause(parent, time.Now().Add(timeout), cause);
}

// WithValue returns a derived context that points to the parent Context.
// In the derived context, the value associated with key is val.
//
// Use context Values only for request-scoped data that transits processes and
// APIs, not for passing optional parameters to functions.
//
// The provided key must be comparable and should not be of type
// string or any other built-in type to avoid collisions between
// packages using context. Users of WithValue should define their own
// types for keys. To avoid allocating when assigning to an
// interface{}, context keys often have concrete type
// struct{}. Alternatively, exported context key variables' static
// type should be a pointer or interface.
public static Context WithValue(Context parent, any key, any val) {
    if (parent == default!) {
        throw panic("cannot create context from nil parent");
    }
    if (key == default!) {
        throw panic("nil key");
    }
    if (!reflectlite.TypeOf(key).Comparable()) {
        throw panic("key is not comparable");
    }
    return new valueCtxжContext(Ꮡ(new valueCtx(parent, key, val)));
}

// A valueCtx carries a key-value pair. It implements Value for that key and
// delegates all other calls to the embedded Context.
[GoType] partial struct valueCtx {
    [GoEmbedded] public Context Context;
    internal any key, val;
}

// Go method set entry for the promoted 'Context.Deadline()' - provided ONLY by the embedded
// interface field in *valueCtx's method set; see the pointer-only satisfaction record.
internal static (time.Time, bool) Deadline(this valueCtx recvᴛ) => recvᴛ.Context.Deadline();

// Go method set entry for the promoted 'Context.Done()' - provided ONLY by the embedded
// interface field in *valueCtx's method set; see the pointer-only satisfaction record.
internal static /*<-*/channel<EmptyStruct> Done(this valueCtx recvᴛ) => recvᴛ.Context.Done();

// Go method set entry for the promoted 'Context.Err()' - provided ONLY by the embedded
// interface field in *valueCtx's method set; see the pointer-only satisfaction record.
internal static error Err(this valueCtx recvᴛ) => recvᴛ.Context.Err();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

// stringify tries a bit to stringify v, without using fmt, since we don't
// want context depending on the unicode tables. This is only used by
// *valueCtx.String().
internal static @string stringify(any v) {
    switch (v.type()) {
    case {} Δs when Δs._<stringer>(out var s): {
        return s.String();
    }
    case @string s: {
        return s;
    }
    case null: {
        return nilˢ;
    }}
    return reflectlite.TypeOf(v).String();
}

[GoRecv] internal static @string String(this ref valueCtx c) {
    return contextName(c.Context) + ".WithValue("u8 + stringify(c.key) + ", "u8 + stringify(c.val) + ")"u8;
}

[GoRecv] internal static any Value(this ref valueCtx c, any key) {
    if (AreEqual(c.key, key)) {
        return c.val;
    }
    return value(c.Context, key);
}

internal static any value(Context c, any key) {
    while (ᐧ) {
        switch (c.type()) {
        case ж<valueCtx> ctx: {
            if (AreEqual(key, (~ctx).key)) {
                return (~ctx).val;
            }
            c = ctx.Value.Context;
            break;
        }
        case ж<cancelCtx> ctx: {
            if (key == ᏑcancelCtxKey) {
                return c;
            }
            c = ctx.Value.Context;
            break;
        }
        case withoutCancelCtx ctx: {
            if (key == ᏑcancelCtxKey) {
                // This implements Cause(ctx) == nil
                // when ctx is created using WithoutCancel.
                return default!;
            }
            c = ctx.c;
            break;
        }
        case ж<timerCtx> ctx: {
            if (key == ᏑcancelCtxKey) {
                return ctx.of(timerCtx.ᏑcancelCtx);
            }
            c = ctx.Value.Context;
            break;
        }
        case backgroundCtx _:
        case todoCtx _: {
            var ctx = c;
            return default!;
        }
        default: {
            var ctx = c;
            return c.Value(key);
        }}
    }
}

} // end context_package
