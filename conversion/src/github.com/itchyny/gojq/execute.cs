namespace go.github.com.itchyny;

using context = context_package;
using math = math_package;
using reflect = reflect_package;
using sort = sort_package;
using ꓸꓸꓸany = Span<any>;

partial class gojq_package {

internal static Iter execute(this ж<env> Ꮡenv, ж<Code> Ꮡbc, any v, params ꓸꓸꓸany varsʗp) {
    var vars = varsʗp.sslice();
    ref var env = ref Ꮡenv.DerefOrNull();
    ref var bc = ref Ꮡbc.DerefOrNull();

    env.codes = bc.codes;
    env.codeinfos = bc.codeinfos;
    env.push(v);
    for (nint i = len(vars) - 1; i >= 0; i--) {
        env.push(vars[i]);
    }
    env.debugCodes();
    return new envжIter(Ꮡenv);
}

internal static (any, bool) Next(this ж<env> Ꮡenv) {
    GoFrame ᒐ = default;
    try {
        ref var env = ref Ꮡenv.DerefOrNull();

        error err = default;
        nint pc = env.pc;
        nint callpc = len(env.codes) - 1;
        nint index = -1;
        var (backtrack, hasCtx) = (env.backtrack, !AreEqual(env.ctx, context.Background()));
        defer(() => {
            (Ꮡenv.Value.pc, Ꮡenv.Value.backtrack) = (pc, true);
        }, ref ᒐ);
loop:
        for (; pc < len(env.codes); pc++) {
            env.debugState(pc, backtrack);
            var code = env.codes[pc];
            if (hasCtx) {
                var selᴛ1 = env.ctx.Done();
                switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
                case 0 when selᴛ1.ꟷᐳ(out _): {
                    (pc, env.forks) = (len(env.codes), default);
                    return (env.ctx.Err(), true);
                }
                default: {
                    break;
                }}
            }
            switch (code.op) {
            case var ᴛ when ᴛ == opnop: {
                break;
            }
            case var ᴛ when ᴛ == oppush: {
                env.push(code.v);
                break;
            }
            case var ᴛ when ᴛ == oppop: {
                env.pop();
                break;
            }
            case var ᴛ when ᴛ == opdup: {
                var v = env.pop();
                env.push(v);
                env.push(v);
                break;
            }
            case var ᴛ when ᴛ == opconst: {
                env.pop();
                env.push(code.v);
                break;
            }
            case var ᴛ when ᴛ == opload: {
                env.push(env.values[env.index(code.v._<array<nint>>())]);
                break;
            }
            case var ᴛ when ᴛ == opstore: {
                env.values[env.index(code.v._<array<nint>>())] = env.pop();
                break;
            }
            case var ᴛ when ᴛ == opobject: {
                if (backtrack) {
                    goto break_loop;
                }
                nint n = code.v._<nint>();
                var m = new map<@string, any>(n);
                foreach (var _ᴛ1 in range(n)) {
                    var (v, k) = (env.pop(), env.pop());
                    var (s, ok) = k._<@string>(ᐧ);
                    if (!ok) {
                        err = new objectKeyNotStringErrorжerror(Ꮡ(new objectKeyNotStringError(k)));
                        goto break_loop;
                    }
                    var (_, okΔ1) = m[s, ꟷ]; if (!okΔ1) {
                        m[s] = v;
                    }
                }
                env.push(m);
                break;
            }
            case var ᴛ when ᴛ == opappend: {
                nint i = env.index(code.v._<array<nint>>());
                env.values[i] = builtin.append(env.values[i]._<slice<any>>(), env.pop());
                break;
            }
            case var ᴛ when ᴛ == opfork: {
                if (backtrack) {
                    if (err != nil) {
                        goto break_loop;
                    }
                    (pc, backtrack) = (code.v._<nint>(), false);
                    goto loop;
                }
                env.pushfork(pc);
                break;
            }
            case var ᴛ when ᴛ == opforktrybegin: {
                if (backtrack) {
                    if (err == nil) {
                        goto break_loop;
                    }
                    switch (err.type()) {
                    case ж<tryEndError> e: {
                        err = e.err;
                        goto break_loop;
                        break;
                    }
                    case ж<breakError> _:
                    case ж<HaltError> _: {
                        var e = err;
                        goto break_loop;
                        break;
                    }
                    case {} Δe when Δe._<ValueError>(out var e): {
                        env.pop();
                        env.push(e.Value());
                        break;
                    }
                    default: {
                        var e = err;
                        env.pop();
                        env.push(err.Error());
                        break;
                    }}
                    (pc, backtrack, err) = (code.v._<nint>(), false, default);
                    goto loop;
                }
                env.pushfork(pc);
                break;
            }
            case var ᴛ when ᴛ == opforktryend: {
                if (backtrack) {
                    if (err != nil) {
                        err = new tryEndErrorжerror(Ꮡ(new tryEndError(err)));
                    }
                    goto break_loop;
                }
                env.pushfork(pc);
                break;
            }
            case var ᴛ when ᴛ == opforkalt: {
                if (backtrack) {
                    if (err == nil) {
                        goto break_loop;
                    }
                    (pc, backtrack, err) = (code.v._<nint>(), false, default);
                    goto loop;
                }
                env.pushfork(pc);
                break;
            }
            case var ᴛ when ᴛ == opforklabel: {
                if (backtrack) {
                    var label = env.pop();
                    var (e, ok) = err._<ж<breakError>>(ᐧ); if (ok && AreEqual(e.v, label)) {
                        err = default;
                    }
                    goto break_loop;
                }
                env.push(env.label);
                env.pushfork(pc);
                env.pop();
                env.values[env.index(code.v._<array<nint>>())] = env.label;
                env.label++;
                break;
            }
            case var ᴛ when ᴛ == opbacktrack: {
                goto break_loop;
                break;
            }
            case var ᴛ when ᴛ == opjump: {
                pc = code.v._<nint>();
                goto loop;
                break;
            }
            case var ᴛ when ᴛ == opjumpifnot: {
                {
                    var v = env.pop(); if (v == nil || AreEqual(v, false)) {
                        pc = code.v._<nint>();
                        goto loop;
                    }
                }
                break;
            }
            case var ᴛ when ᴛ == opindex || ᴛ == opindexarray: {
                if (backtrack) {
                    goto break_loop;
                }
                var (p, v) = (code.v, env.pop());
                if (code.op == opindexarray && v != nil) {
                    var (_, ok) = v._<slice<any>>(ᐧ); if (!ok) {
                        err = new expectedArrayErrorжerror(Ꮡ(new expectedArrayError(v)));
                        goto break_loop;
                    }
                }
                var w = funcIndex2(default, v, p);
                {
                    var (e, ok) = w._<error>(ᐧ); if (ok) {
                        err = e;
                        goto break_loop;
                    }
                }
                env.push(w);
                if (!env.paths.empty() && env.expdepth == 0) {
                    if (!env.pathIntact(v)) {
                        err = new invalidPathErrorжerror(Ꮡ(new invalidPathError(v)));
                        goto break_loop;
                    }
                    env.paths.push(new pathValue(path: p, value: w));
                }
                break;
            }
            case var ᴛ when ᴛ == opcall: {
                if (backtrack) {
                    goto break_loop;
                }
                switch (code.v.type()) {
                case nint vΔ3: {
                    (pc, callpc, index) = (vΔ3, pc, env.scopes.index);
                    goto loop;
                    break;
                }
                case array<any> vΔ3: {
                    nint argcnt = vΔ3[1]._<nint>();
                    var (xΔ1, args) = (env.pop(), env.args.slice(0, argcnt));
                    foreach (var i in range(argcnt)) {
                        args[i] = env.pop();
                    }
                    var w = vΔ3[0]._<Func<any, slice<any>, any>>()(xΔ1, args);
                    var (e, ok) = w._<error>(ᐧ); if (ok) {
                        err = e;
                        goto break_loop;
                    }
                    env.push(w);
                    if (!env.paths.empty() && env.expdepth == 0) {
                        var exprᴛ2 = vΔ3[2]._<@string>();
                        switch (ᐧ) {
                        case {} when exprᴛ2 == "_index"u8: {
                            xΔ1 = args[0]; if (!env.pathIntact(xΔ1)) {
                                err = new invalidPathErrorжerror(Ꮡ(new invalidPathError(xΔ1)));
                                goto break_loop;
                            }
                            env.paths.push(new pathValue(path: args[1], value: w));
                            break;
                        }
                        case {} when exprᴛ2 == "_slice"u8: {
                            xΔ1 = args[0]; if (!env.pathIntact(xΔ1)) {
                                err = new invalidPathErrorжerror(Ꮡ(new invalidPathError(xΔ1)));
                                goto break_loop;
                            }
                            env.paths.push(new pathValue(
                                path: new map<@string, any>{["start"] = args[2], ["end"] = args[1]},
                                value: w
                            ));
                            break;
                        }
                        case {} when exprᴛ2 == "getpath"u8: {
                            if (!env.pathIntact(xΔ1)) {
                                err = new invalidPathErrorжerror(Ꮡ(new invalidPathError(xΔ1)));
                                goto break_loop;
                            }
                            foreach (var (_, p) in range(args[0]._<slice<any>>())) {
                                env.paths.push(new pathValue(path: p, value: w));
                            }
                            break;
                        }}

                    }
                    break;
                }
                default: {
                    var vΔ3 = code.v;
                    throw panic(vΔ3);
                    break;
                }}
                break;
            }
            case var ᴛ when ᴛ == opcallrec: {
                (pc, callpc, index) = (code.v._<nint>(), -1, env.scopes.index);
                goto loop;
                break;
            }
            case var ᴛ when ᴛ == oppushpc: {
                env.push(new nint[]{code.v._<nint>(), env.scopes.index}.array());
                break;
            }
            case var ᴛ when ᴛ == opcallpc: {
                var xs = env.pop()._<array<nint>>();
                (pc, callpc, index) = (xs[0], pc, xs[1]);
                goto loop;
                break;
            }
            case var ᴛ when ᴛ == opscope: {
                var xs = code.v._<array<nint>>();
                nint saveindex = default;
                nint outerindexΔ1 = default;
                if (index == env.scopes.index) {
                    if (callpc >= 0) {
                        saveindex = index;
                    } else {
                        (callpc, saveindex) = env.popscope();
                    }
                } else {
                    saveindex = env.scopes.index;
                }
                outerindexΔ1 = index; if (outerindexΔ1 >= 0) {
                    var s = env.scopes.data[outerindexΔ1].value; if (s.id == xs[0]) {
                        outerindexΔ1 = s.outerindex;
                    }
                }
                env.scopes.push(new scope(xs[0], env.offset, callpc, saveindex, outerindexΔ1));
                env.offset += xs[1];
                if (env.offset > len(env.values)) {
                    var vs = new slice<any>(env.offset * 2);
                    copy(vs, env.values);
                    env.values = vs;
                }
                break;
            }
            case var ᴛ when ᴛ == opret: {
                if (backtrack) {
                    goto break_loop;
                }
                (pc, env.scopes.index) = env.popscope();
                if (env.scopes.empty()) {
                    return (env.pop(), true);
                }
                break;
            }
            case var ᴛ when ᴛ == opiter: {
                if (err != nil) {
                    goto break_loop;
                }
                backtrack = false;
                ref var xs = ref heap<slice<pathValue>>(out var Ꮡxs);
                var switchᴛ1 = env.pop();
                switch (switchᴛ1.type()) {
                case slice<pathValue> vΔ4: {
                    xs = vΔ4;
                    break;
                }
                case slice<any> vΔ4: {
                    if (!env.paths.empty() && env.expdepth == 0 && !env.pathIntact(vΔ4)) {
                        err = new invalidPathIterErrorжerror(Ꮡ(new invalidPathIterError(vΔ4)));
                        goto break_loop;
                    }
                    if (len(vΔ4) == 0) {
                        goto break_loop;
                    }
                    xs = new slice<pathValue>(len(vΔ4));
                    foreach (var (i, vΔ1) in range(vΔ4)) {
                        xs[i] = new pathValue(path: i, value: vΔ1);
                    }
                    break;
                }
                case map<@string, any> vΔ4: {
                    if (!env.paths.empty() && env.expdepth == 0 && !env.pathIntact(vΔ4)) {
                        err = new invalidPathIterErrorжerror(Ꮡ(new invalidPathIterError(vΔ4)));
                        goto break_loop;
                    }
                    if (len(vΔ4) == 0) {
                        goto break_loop;
                    }
                    xs = new slice<pathValue>(len(vΔ4));
                    nint i = default;
                    foreach (var (k, vΔ2) in vΔ4) {
                        xs[i] = new pathValue(path: k, value: vΔ2);
                        i++;
                    }
                    sort.Slice(xs, (nint iΔ1, nint j) => Ꮡxs.ValueSlot[iΔ1].path._<@string>() < Ꮡxs.ValueSlot[j].path._<@string>());
                    break;
                }
                case {} ΔvΔ4 when ΔvΔ4._<Iter>(out var vΔ4): {
                    var (w, ok) = vΔ4.Next(); if (ok) {
                        env.push(vΔ4);
                        env.pushfork(pc);
                        env.pop();
                        var (e, okΔ1) = w._<error>(ᐧ); if (okΔ1) {
                            err = e;
                            goto break_loop;
                        }
                        env.push(w);
                        continue;
                    }
                    goto break_loop;
                    break;
                }
                default: {
                    var vΔ4 = switchᴛ1;
                    err = new iteratorErrorжerror(Ꮡ(new iteratorError(vΔ4)));
                    env.push(new emptyIter());
                    goto break_loop;
                    break;
                }}
                if (len(xs) > 1) {
                    env.push(xs[1..]);
                    env.pushfork(pc);
                    env.pop();
                }
                env.push(xs[0].value);
                if (!env.paths.empty() && env.expdepth == 0) {
                    env.paths.push(xs[0]);
                }
                break;
            }
            case var ᴛ when ᴛ == opexpbegin: {
                env.expdepth++;
                break;
            }
            case var ᴛ when ᴛ == opexpend: {
                env.expdepth--;
                break;
            }
            case var ᴛ when ᴛ == oppathbegin: {
                env.paths.push(env.expdepth);
                env.paths.push(new pathValue(value: env.stack.top()));
                env.expdepth = 0;
                break;
            }
            case var ᴛ when ᴛ == oppathend: {
                if (backtrack) {
                    goto break_loop;
                }
                env.pop();
                {
                    var v = env.pop(); if (!env.pathIntact(v)) {
                        err = new invalidPathErrorжerror(Ꮡ(new invalidPathError(v)));
                        goto break_loop;
                    }
                }
                env.push(env.poppaths());
                env.expdepth = env.paths.pop()._<nint>();
                break;
            }
            default: {
                throw panic(code.op);
                break;
            }}

continue_loop:;
        }
break_loop:;
        if (len(env.forks) > 0) {
            (pc, backtrack) = (env.popfork(), true);
            goto loop;
        }
        if (err != nil) {
            return (err, true);
        }
        return (default, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void push(this ref env env, any v) {
    env.stack.push(v);
}

internal static any pop(this ref env env) {
    return env.stack.pop();
}

internal static (nint, nint) popscope(this ref env env) {
    var free = env.scopes.index > env.scopes.limit;
    var s = env.scopes.pop();
    if (free) {
        env.offset = s.offset;
    }
    return (s.pc, s.saveindex);
}

internal static void pushfork(this ref env env, nint pc) {
    var f = new fork(pc: pc, offset: env.offset, expdepth: env.expdepth);
    (f.stackindex, f.stacklimit) = env.stack.save();
    (f.scopeindex, f.scopelimit) = env.scopes.save();
    (f.pathindex, f.pathlimit) = env.paths.save();
    env.forks = builtin.append(env.forks, f);
    env.debugForks(pc, ">>>");
}

internal static nint popfork(this ref env env) {
    var f = env.forks[len(env.forks) - 1];
    env.debugForks(f.pc, "<<<");
    (env.forks, env.offset, env.expdepth) = (env.forks.slice(0, len(env.forks) - 1), f.offset, f.expdepth);
    env.stack.restore(f.stackindex, f.stacklimit);
    env.scopes.restore(f.scopeindex, f.scopelimit);
    env.paths.restore(f.pathindex, f.pathlimit);
    return f.pc;
}

internal static nint index(this ref env env, /*[2]*/ array<nint> v) {
    v = v.Clone();

    for ((nint id, nint i) = (v[0], env.scopes.index); i >= 0; ) {
        var s = env.scopes.data[i].value;
        if (s.id == id) {
            return s.offset + v[1];
        }
        i = s.outerindex;
    }
    throw panic("env.index");
}

partial struct pathValue {
    internal any path, value;
}

internal static bool pathIntact(this ref env env, any v) {
    var w = env.paths.top()._<pathValue>().value;
    switch (v.type()) {
    case slice<any> _:
    case map<@string, any> _: {
        var vΔ1 = v;
        switch (w.type()) {
        case slice<any> _:
        case map<@string, any> _: {
            var (vΔ2, wΔ1) = (reflect.ValueOf(vΔ1), reflect.ValueOf(w));
            return vΔ2.Pointer() == wΔ1.Pointer() && vΔ2.Len() == wΔ1.Len();
        }}

        break;
    }
    case float64 vΔ1: {
        var (wΔ2, ok) = w._<float64>(ᐧ); if (ok) {
            return vΔ1 == wΔ2 || math.IsNaN(vΔ1) && math.IsNaN(wΔ2);
        }
        break;
    }}
    return AreEqual(v, w);
}

internal static slice<any> poppaths(this ref env env) {
    var xsᴷ = new stackAppend<any>(moves: true);

    var xs = slice<any>([]);
    for (;;) {
        var p = env.paths.pop()._<pathValue>();
        if (p.path == nil) {
            break;
        }
        xs = builtin.append(xsᴷ, xs, p.path);
    }
    for ((nint i, nint j) = (0, len(xs) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (xs[i], xs[j]) = (xs[j], xs[i]);
    }
    xs = move2heap(xsᴷ, xs);
    return xs;
}

} // end gojq_package
