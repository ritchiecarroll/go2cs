// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using synctest = global::go.@internal.synctest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.@internal;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

internal static error errStillRunning = errors.New("async op still running"u8);

[GoType] partial struct asyncResult<T> {
    internal channel<EmptyStruct> donec;
    internal T res;
    internal error err;
}

// runAsync runs f in a new goroutine.
// It returns an asyncResult which acts as a future.
//
// Must be called from within a synctest bubble.
[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<asyncResult<T>> runAsync<T>(Func<(T, error)> f) {
    var r = Ꮡ(new asyncResult<T>(
        donec: new channel<EmptyStruct>(0)
    ));
    var rʗ1 = r;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), (~rʗ1).donec, ref ᒐ);
            (rʗ1.Value.res, rʗ1.Value.err) = f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    synctest.Wait();
    return r;
}

// done reports whether the function has returned.
[GoRecv] internal static bool done<T>(this ref asyncResult<T> r) {
    var (_, err) = r.result();
    return !AreEqual(err, errStillRunning);
}

// result returns the result of the function.
// If the function hasn't completed yet, it returns errStillRunning.
[GoRecv] internal static (T, error) result<T>(this ref asyncResult<T> r) {
    var selᴛ1 = r.donec;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        return (r.res, r.err);
    }
    default: {
        T zero = GoZero<T>();
        return (zero, errStillRunning);
    }}
}

} // end http_test_package
