// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sync;

using context = context_package;
using fmt = fmt_package;
using sync = sync_package;
using System.Runtime.CompilerServices;

partial class errgroup_package {

partial struct token {
}

partial struct Group {
    internal Action<error> cancel;
    internal sync.WaitGroup wg;
    internal channel<token> sem;
    internal sync.Once errOnce;
    internal error err;
}

internal static void done(this ж<Group> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (g.sem != default!) {
        ᐸꟷ(g.sem);
    }
    Ꮡg.of(Group.Ꮡwg).Done();
}

public static (ж<Group>, context.Context) WithContext(context.Context ctx) {
    (ctx, var cancel) = context.WithCancelCause(ctx);
    return (Ꮡ(new Group(cancel: cancel)), ctx);
}

public static error Wait(this ж<Group> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();

    Ꮡg.of(Group.Ꮡwg).Wait();
    if (g.cancel != default!) {
        g.cancel(g.err);
    }
    return g.err;
}

public static partial void Go(this ж<Group> Ꮡg, Func<error> f) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (g.sem != default!) {
        g.sem.ᐸꟷ(new token(nil));
    }

    Ꮡg.of(Group.Ꮡwg).Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡg.done, ref ᒐ);

            {
                var err = f(); if (err != default!) {
                    var errʗ1 = err;
                    Ꮡg.of(Group.ᏑerrOnce).Do(() => {
                        Ꮡg.Value.err = errʗ1;
                        if (Ꮡg.Value.cancel != default!) {
                            Ꮡg.Value.cancel(Ꮡg.Value.err);
                        }
                    });
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static partial bool TryGo(this ж<Group> Ꮡg, Func<error> f) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (g.sem != default!) {
        var selᴛ1 = g.sem.ᐸꟷ(new token(nil), ꓸꓸꓸ);
        switch (trySelect(selᴛ1)) {
        case 0: {
            break;
        }
        default: {
            return false;
        }}
    }

    Ꮡg.of(Group.Ꮡwg).Add(1);
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡg.done, ref ᒐ);

            {
                var err = f(); if (err != default!) {
                    var errʗ1 = err;
                    Ꮡg.of(Group.ᏑerrOnce).Do(() => {
                        Ꮡg.Value.err = errʗ1;
                        if (Ꮡg.Value.cancel != default!) {
                            Ꮡg.Value.cancel(Ꮡg.Value.err);
                        }
                    });
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return true;
}

public static void SetLimit(this ref Group g, nint n) {
    if (n < 0) {
        g.sem = default!;
        return;
    }
    {
        nint active = len(g.sem); if (active != 0) {
            throw panic(fmt.Errorf("errgroup: modify limit while %v goroutines in the group are still active"u8, active));
        }
    }
    g.sem = new channel<token>(n);
}

} // end errgroup_package
