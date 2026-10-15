// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using errors = errors_package;
using ꓸꓸꓸHandler = Span<slog_package.ΔHandler>;

partial class slog_package {

// NewMultiHandler creates a [MultiHandler] with the given Handlers.
public static ж<MultiHandler> NewMultiHandler(params ꓸꓸꓸHandler handlersʗp) {
    var handlers = handlersʗp.sslice();

    var h = new slice<ΔHandler>(len(handlers));
    copy(h, handlers);
    return Ꮡ(new MultiHandler(multi: h));
}

// MultiHandler is a [Handler] that invokes all the given Handlers.
// Its Enabled method reports whether any of the handlers' Enabled methods return true.
// Its Handle, WithAttrs and WithGroup methods call the corresponding method on each of the enabled handlers.
partial struct MultiHandler {
    internal slice<ΔHandler> multi;
}

public static bool Enabled(this ref MultiHandler h, context.Context ctx, ΔLevel l) {
    foreach (var (i, _) in h.multi) {
        if (h.multi[i].Enabled(ctx, l)) {
            return true;
        }
    }
    return false;
}

public static error Handle(this ref MultiHandler h, context.Context ctx, Record r) {
    r = r.ΔClone();

    slice<error> errs = default!;
    foreach (var (i, _) in h.multi) {
        if (h.multi[i].Enabled(ctx, r.Level)) {
            {
                var err = h.multi[i].Handle(ctx, r.Clone()); if (err != default!) {
                    errs = builtin.append(errs, err);
                }
            }
        }
    }
    return errors.Join(errs.ꓸꓸꓸ);
}

public static ΔHandler WithAttrs(this ref MultiHandler h, slice<Attr> attrs) {
    var handlers = new slice<ΔHandler>(0, len(h.multi));
    foreach (var (i, _) in h.multi) {
        handlers = builtin.append(handlers, h.multi[i].WithAttrs(attrs));
    }
    return new MultiHandlerжΔHandler(Ꮡ(new MultiHandler(multi: handlers)));
}

public static ΔHandler WithGroup(this ref MultiHandler h, @string name) {
    var handlers = new slice<ΔHandler>(0, len(h.multi));
    foreach (var (i, _) in h.multi) {
        handlers = builtin.append(handlers, h.multi[i].WithGroup(name));
    }
    return new MultiHandlerжΔHandler(Ꮡ(new MultiHandler(multi: handlers)));
}

} // end slog_package
