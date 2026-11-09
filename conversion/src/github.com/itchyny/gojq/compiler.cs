namespace go.github.com.itchyny;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸжcode = Span<ж<gojq_package.code>>;

partial class gojq_package {

public partial struct compiler {
    internal ModuleLoader moduleLoader;
    internal Func<slice<@string>> environLoader;
    internal slice<@string> variables;
    internal map<@string, function> customFuncs;
    internal Iter inputIter;
    internal slice<ж<code>> codes;
    internal slice<codeinfo> codeinfos;
    internal ж<scopeinfo> builtinScope;
    internal slice<ж<scopeinfo>> scopes;
    internal nint scopecnt;
    internal sync.Map regexpCache;
}

partial struct Code {
    internal slice<@string> variables;
    internal slice<ж<code>> codes;
    internal slice<codeinfo> codeinfos;
}

public static Iter Run(this ж<Code> Ꮡc, any v, params ꓸꓸꓸany valuesʗp) {
    var values = valuesʗp.sslice();

    return Ꮡc.RunWithContext(context.Background(), v, values.ꓸꓸꓸ);
}

public static Iter RunWithContext(this ж<Code> Ꮡc, context.Context ctx, any v, params ꓸꓸꓸany valuesʗp) {
    var values = valuesʗp.sslice();
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(values) > len(c.variables)) {
        return NewIter(Ꮡ(new tooManyVariableValuesError()));
    } else if (len(values) < len(c.variables)) {
        return NewIter(Ꮡ(new expectedVariableError(c.variables[len(values)])));
    }
    return newEnv(ctx).execute(Ꮡc, v, values.ꓸꓸꓸ);
}

partial struct scopeinfo {
    internal slice<ж<varinfo>> variables;
    internal slice<ж<funcinfo>> funcs;
    internal nint id;
    internal nint depth;
    internal nint variablecnt;
}

partial struct varinfo {
    internal @string name;
    internal array<nint> index = new(2);
    internal nint depth;
}

partial struct funcinfo {
    internal @string name;
    internal nint pc;
    internal nint argcnt;
}

internal partial interface Compile_type /*dyn*/ {
    (slice<ж<Query>>, error) LoadInitModules();
}

public static (ж<Code>, error) Compile(ж<Query> q, params Span<Action<ж<compiler>>> optionsʗp) {
    var options = optionsʗp.sslice();

    var c = Ꮡ(new compiler());
    foreach (var (_, opt) in range(options)) {
        opt(c);
    }
    c.builtinScope = c.newScope();
    var scope = c.newScope();
    c.scopes = [scope];
    var scopeʗ1 = scope;
    var setscope = c.lazy(() => Ꮡ(new code(op: opscope, v: new nint[]{scopeʗ1.id, scopeʗ1.variablecnt, 0}.array())));
    foreach (var (_, vᴛ1) in range(c.variables)) {
        ref var name = ref heap(new @string(), out var Ꮡname);
        name = vᴛ1;

        if (!newLexer(name).validVarName()) {
            return (default, new variableNameErrorжerror(Ꮡ(new variableNameError(name))));
        }
        c.appendCodeInfo(name);
        c.append(Ꮡ(new code(op: opstore, v: c.pushVariable(name))));
    }
    if (c.moduleLoader != nil) {
        var (moduleLoader, ok) = c.moduleLoader._<Compile_type>(ᐧ); if (ok) {
            var (qs, err) = moduleLoader.LoadInitModules();
            if (err != nil) {
                return (default, err);
            }
            foreach (var (_, qΔ1) in range(qs)) {
                var errΔ1 = c.compileModule(qΔ1, ""); if (errΔ1 != nil) {
                    return (default, errΔ1);
                }
            }
        }
    }
    {
        var err = c.compile(q); if (err != nil) {
            return (default, err);
        }
    }
    setscope();
    c.optimizeTailRec();
    c.optimizeCodeOps();
    return (Ꮡ(new Code(
        variables: c.variables,
        codes: c.codes,
        codeinfos: c.codeinfos
    )), default);
}

internal static error compile(this ж<compiler> Ꮡc, ж<Query> Ꮡq) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var q = ref Ꮡq.DerefOrNull();

    foreach (var (_, i) in range(q.Imports)) {
        var err = Ꮡc.compileImport(i); if (err != nil) {
            return err;
        }
    }
    {
        var err = Ꮡc.compileQuery(Ꮡq); if (err != nil) {
            return err;
        }
    }
    c.append(Ꮡ(new code(op: opret)));
    return default;
}

internal partial interface compileImport_type /*dyn*/ {
    (any, error) LoadJSONWithMeta(@string _Δp0, map<@string, any> _Δp1);
}

internal partial interface compileImport_typeᴛ1 /*dyn*/ {
    (any, error) LoadJSON(@string _);
}

internal partial interface compileImport_typeᴛ2 /*dyn*/ {
    (ж<Query>, error) LoadModuleWithMeta(@string _Δp0, map<@string, any> _Δp1);
}

internal partial interface compileImport_typeᴛ3 /*dyn*/ {
    (ж<Query>, error) LoadModule(@string _);
}

internal static error compileImport(this ж<compiler> Ꮡc, ж<Import> Ꮡi) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var i = ref Ꮡi.DerefOrNull();

    @string path = default;
    @string alias = default;
    error err = default;
    if (i.ImportPath != ""u8) {
        (path, alias) = (i.ImportPath, i.ImportAlias);
    } else {
        path = i.IncludePath;
    }
    if (c.moduleLoader == nil) {
        return fmt.Errorf("cannot load module: %q"u8, path);
    }
    if (strings.HasPrefix(alias, "$")) {
        any vals = default;
        var (moduleLoader, ok) = c.moduleLoader._<compileImport_type>(ᐧ); if (ok) {
            (vals, err) = moduleLoader.LoadJSONWithMeta(path, i.Meta.ToValue()); if (err != nil) {
                return err;
            }
        } else if (c.moduleLoader._<compileImport_typeᴛ1>(out var moduleLoaderΔ1)) {
            (vals, err) = moduleLoaderΔ1.LoadJSON(path); if (err != nil) {
                return err;
            }
        } else {
            return fmt.Errorf("module not found: %q"u8, path);
        }
        c.append(Ꮡ(new code(op: oppush, v: vals)));
        c.append(Ꮡ(new code(op: opstore, v: c.pushVariable(alias))));
        c.append(Ꮡ(new code(op: oppush, v: vals)));
        c.append(Ꮡ(new code(op: opstore, v: c.pushVariable(alias + "::" + alias[1..]))));
        return default;
    }
    ж<Query> q = default;
    {
        var (moduleLoader, ok) = c.moduleLoader._<compileImport_typeᴛ2>(ᐧ); if (ok) {
            (q, err) = moduleLoader.LoadModuleWithMeta(path, i.Meta.ToValue()); if (err != nil) {
                return err;
            }
        } else if (c.moduleLoader._<compileImport_typeᴛ3>(out var moduleLoaderΔ1)) {
            (q, err) = moduleLoaderΔ1.LoadModule(path); if (err != nil) {
                return err;
            }
        }
    }
    c.appendCodeInfo("module " + path);
    err = Ꮡc.compileModule(q, alias); if (err != nil) {
        return err;
    }
    c.appendCodeInfo("end of module " + path);
    return default;
}

internal static error compileModule(this ж<compiler> Ꮡc, ж<Query> Ꮡq, @string alias) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var q = ref Ꮡq.DerefOrNull();

        var scope = c.scopes[len(c.scopes) - 1];
        scope.depth++;
        var scopeʗ1 = scope;
        defer((nint l) => {
            scopeʗ1.depth--;
            scopeʗ1.variables = scopeʗ1.variables.slice(0, l);
        }, len(scope.variables), ref ᒐ);
        if (alias != ""u8) {
            var scopeʗ2 = scope;
            defer((nint l) => {
                foreach (var (_, f) in range(scopeʗ2.funcs.slice(l))) {
                    f.name = alias + "::"u8 + f.name;
                }
            }, len(scope.funcs), ref ᒐ);
        }
        foreach (var (_, i) in range(q.Imports)) {
            var err = Ꮡc.compileImport(i); if (err != nil) {
                return err;
            }
        }
        foreach (var (_, fd) in range(q.FuncDefs)) {
            var err = Ꮡc.compileFuncDef(fd, false); if (err != nil) {
                return err;
            }
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static array<nint> newVariable(this ref compiler c) {
    return c.createVariable("");
}

internal static array<nint> pushVariable(this ref compiler c, @string name) {
    var s = c.scopes[len(c.scopes) - 1];
    foreach (var (_, v) in range(s.variables)) {
        if (v.name == name && v.depth == s.depth) {
            return v.index.Clone();
        }
    }
    return c.createVariable(name);
}

internal static array<nint> createVariable(this ref compiler c, @string name) {
    var s = c.scopes[len(c.scopes) - 1];
    ref var v = ref heap<array<nint>>(new nint[]{s.id, s.variablecnt}.array(), out var Ꮡv);
    s.variablecnt++;
    s.variables = builtin.append(s.variables, Ꮡ(new varinfo(name, v.Clone(), s.depth)));
    return v.Clone();
}

internal static (array<nint>, error) lookupVariable(this ref compiler c, @string name) {
    for (nint i = len(c.scopes) - 1; i >= 0; i--) {
        var s = c.scopes[i];
        for (nint j = len(s.variables) - 1; j >= 0; j--) {
            var w = s.variables[j]; if (w.name == name) {
                return (w.index.Clone(), default);
            }
        }
    }
    return (new nint[]{}.array(2), new variableNotFoundErrorжerror(Ꮡ(new variableNotFoundError(name))));
}

internal static (ж<funcinfo>, ж<varinfo>) lookupFuncOrVariable(this ref compiler c, @string name) {
    for ((nint i, var isFunc) = (len(c.scopes) - 1, name[0] != '$'); i >= 0; i--) {
        var s = c.scopes[i];
        if (isFunc) {
            for (nint j = len(s.funcs) - 1; j >= 0; j--) {
                var f = s.funcs[j]; if (f.name == name && f.argcnt == 0) {
                    return (f, default);
                }
            }
        }
        for (nint j = len(s.variables) - 1; j >= 0; j--) {
            var v = s.variables[j]; if (v.name == name) {
                return (default, v);
            }
        }
    }
    return (default, default);
}

internal static ж<funcinfo> lookupBuiltin(this ref compiler c, @string name, nint argcnt) {
    var s = c.builtinScope;
    for (nint i = len(s.funcs) - 1; i >= 0; i--) {
        var f = s.funcs[i]; if (f.name == name && f.argcnt == argcnt) {
            return f;
        }
    }
    return default;
}

internal static Action appendBuiltin(this ж<compiler> Ꮡc, @string name, nint argcnt) {
    ref var c = ref Ꮡc.DerefOrNull();

    var setjump = Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes))));
    c.appendCodeInfo(name);
    c.builtinScope.funcs = builtin.append(
        c.builtinScope.funcs,
        Ꮡ(new funcinfo(name, len(c.codes), argcnt)));
    var setjumpʗ1 = setjump;
    return () => {
        setjumpʗ1();
        Ꮡc.Value.appendCodeInfo("end of " + name);
    };
}

internal static ж<scopeinfo> newScope(this ref compiler c) {
    ref var i = ref heap<nint>(c.scopecnt, out var Ꮡi);
    c.scopecnt++;
    return Ꮡ(new scopeinfo(id: i));
}

internal static Action newScopeDepth(this ref compiler c) {
    var scope = c.scopes[len(c.scopes) - 1];
    nint l = len(scope.variables);
    nint m = len(scope.funcs);
    scope.depth++;
    var scopeʗ1 = scope;
    return () => {
        scopeʗ1.depth--;
        scopeʗ1.variables = scopeʗ1.variables.slice(0, l);
        scopeʗ1.funcs = scopeʗ1.funcs.slice(0, m);
    };
}

internal partial struct compileFuncDef_varIndex /*dyn*/ {
    internal @string name;
    internal array<nint> index = new(2);
}

internal static error compileFuncDef(this ж<compiler> Ꮡc, ж<FuncDef> Ꮡe, bool Δbuiltin) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();
        var visᴷ = new stackAppend<compileFuncDef_varIndex>();

        ж<scopeinfo> scope = default;
        if (Δbuiltin) {
            scope = c.builtinScope;
        } else {
            scope = c.scopes[len(c.scopes) - 1];
        }
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes)))), ref ᒐ);
        c.appendCodeInfo(e.Name);
        scope.funcs = builtin.append(scope.funcs, Ꮡ(new funcinfo(e.Name, len(c.codes), len(e.Args))));
        defer((slice<ж<scopeinfo>> scopes, slice<@string> variables) => {
            (Ꮡc.Value.scopes, Ꮡc.Value.variables) = (scopes, variables);
        }, Ꮡc.Value.scopes, Ꮡc.Value.variables, ref ᒐ);
        c.variables = c.variables.slice(len(c.variables));
        scope = c.newScope();
        if (Δbuiltin) {
            c.scopes = [c.builtinScope, scope];
        } else {
            c.scopes = builtin.append(c.scopes, scope);
        }
        var scopeʗ1 = scope;
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opscope, v: new nint[]{scopeʗ1.id, scopeʗ1.variablecnt, len(Ꮡe.Value.Args)}.array()))), ref ᒐ);
        if (len(e.Args) > 0) {
            var vis = new slice<compileFuncDef_varIndex>(0, () => new(), len(e.Args));
            ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
            c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
            foreach (var (_, arg) in range(e.Args)) {
                if (arg[0] == '$') {
                    c.appendCodeInfo(arg[1..]);
                    ref var w = ref heap<array<nint>>(c.createVariable(arg[1..]), out var Ꮡw);
                    c.append(Ꮡ(new code(op: opstore, v: w.Clone())));
                    vis = builtin.append(visᴷ, vis, new compileFuncDef_varIndex(arg, w.Clone()));
                } else {
                    c.appendCodeInfo(arg);
                    c.append(Ꮡ(new code(op: opstore, v: c.createVariable(arg))));
                }
            }
            foreach (var (_, vᴛ1) in range(vis)) {
                var w = vᴛ1.ΔClone();

                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: opexpbegin)));
                c.append(Ꮡ(new code(op: opload, v: w.index.Clone())));
                c.append(Ꮡ(new code(op: opcallpc)));
                c.appendCodeInfo(w.name);
                c.append(Ꮡ(new code(op: opstore, v: c.pushVariable(w.name))));
                c.append(Ꮡ(new code(op: opexpend)));
            }
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        }
        var err = Ꮡc.compile(e.Body); if (err != nil) {
            return err;
        }
        c.appendCodeInfo("end of " + e.Name);
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileQuery(this ж<compiler> Ꮡc, ж<Query> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    foreach (var (_, fd) in range(e.FuncDefs)) {
        var err = Ꮡc.compileFuncDef(fd, false); if (err != nil) {
            return err;
        }
    }
    if (e.Term != nil) {
        return Ꮡc.compileTerm(e.Term);
    }
    switch (e.Op) {
    case var ᴛ when ᴛ == ((Operator)0): {
        return errors.New(@"missing query (try ""."")");
    }
    case var ᴛ when ᴛ == OpPipe: {
        if (len(e.Patterns) > 0) {
            return Ꮡc.compileBind(e.Left, e.Right, e.Patterns);
        }
        var err = Ꮡc.compileQuery(e.Left); if (err != nil) {
            return err;
        }
        return Ꮡc.compileQuery(e.Right);
    }
    case var ᴛ when ᴛ == OpComma: {
        return Ꮡc.compileComma(e.Left, e.Right);
    }
    case var ᴛ when ᴛ == OpAlt: {
        return Ꮡc.compileAlt(e.Left, e.Right);
    }
    case var ᴛ when ᴛ == OpAssign || ᴛ == OpModify || ᴛ == OpUpdateAdd || ᴛ == OpUpdateSub || ᴛ == OpUpdateMul || ᴛ == OpUpdateDiv || ᴛ == OpUpdateMod || ᴛ == OpUpdateAlt: {
        return Ꮡc.compileQueryUpdate(e.Left, e.Right, e.Op);
    }
    case var ᴛ when ᴛ == OpOr: {
        return Ꮡc.compileIf(
            Ꮡ(new If(
                Cond: e.Left,
                Then: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeTrue)))),
                Else: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeIf, If: Ꮡ(new If(
                    Cond: e.Right,
                    Then: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeTrue)))),
                    Else: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeFalse))))
                ))
                ))
                ))
            )));
    }
    case var ᴛ when ᴛ == OpAnd: {
        return Ꮡc.compileIf(
            Ꮡ(new If(
                Cond: e.Left,
                Then: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeIf, If: Ꮡ(new If(
                    Cond: e.Right,
                    Then: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeTrue)))),
                    Else: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeFalse))))
                ))
                ))
                )),
                Else: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeFalse))))
            )));
    }
    default: {
        return Ꮡc.compileCall(
            e.Op.getFunc(),
            [e.Left, e.Right]);
    }}

}

internal static error compileComma(this ж<compiler> Ꮡc, ж<Query> l, ж<Query> r) {
    GoFrame ᒐ = default;
    try {
        var setfork = Ꮡc.lazy(() => Ꮡ(new code(op: opfork, v: len(Ꮡc.Value.codes))));
        var err = Ꮡc.compileQuery(l); if (err != nil) {
            return err;
        }
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes)))), ref ᒐ);
        setfork();
        return Ꮡc.compileQuery(r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileAlt(this ж<compiler> Ꮡc, ж<Query> l, ж<Query> r) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.append(Ꮡ(new code(op: oppush, v: false)));
        ref var found = ref heap<array<nint>>(c.newVariable(), out var Ꮡfound);
        c.append(Ꮡ(new code(op: opstore, v: found.Clone())));
        var setfork = Ꮡc.lazy(() => Ꮡ(new code(op: opfork, v: len(Ꮡc.Value.codes))));
        var err = Ꮡc.compileQuery(l); if (err != nil) {
            return err;
        }
        c.append(Ꮡ(new code(op: opdup)));
        c.append(Ꮡ(new code(op: opjumpifnot, v: len(c.codes) + 4)));
        c.append(Ꮡ(new code(op: oppush, v: true)));
        c.append(Ꮡ(new code(op: opstore, v: found.Clone())));
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes)))), ref ᒐ);
        c.append(Ꮡ(new code(op: oppop)));
        c.append(Ꮡ(new code(op: opbacktrack)));
        setfork();
        c.append(Ꮡ(new code(op: opload, v: found.Clone())));
        c.append(Ꮡ(new code(op: opjumpifnot, v: len(c.codes) + 3)));
        c.append(Ꮡ(new code(op: opbacktrack)));
        c.append(Ꮡ(new code(op: oppop)));
        return Ꮡc.compileQuery(r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object setpathˢ = (@string)"setpath"u8;

internal static error compileQueryUpdate(this ж<compiler> Ꮡc, ж<Query> Ꮡl, ж<Query> r, Operator op) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var l = ref Ꮡl.DerefOrNull();

    var exprᴛ1 = op;
    var matchᴛ1 = false;
    if (exprᴛ1 == OpAssign) { matchᴛ1 = true;
        var xs = l.toIndices(default); if (xs != nil) {
            ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
            c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            var err = Ꮡc.compileQuery(r); if (err != nil) {
                return err;
            }
            c.append(Ꮡ(new code(op: oppush, v: xs)));
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            c.append(Ꮡ(new code(op: opcall, v: new any[]{(internalFuncs["setpath"].callback).OrTypedNilFunc(), (nint)2, setpathˢ}.array())));
            return default;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == OpModify) {
        return Ꮡc.compileFunc(
            Ꮡ(new Func(
                Name: op.getFunc(),
                Args: [Ꮡl, r]
            )));
    }
    if (!matchᴛ1) { /* default: */
        ref var name = ref heap<@string>("$%0", out var Ꮡname);
        c.append(Ꮡ(new code(op: opdup)));
        {
            var err = Ꮡc.compileQuery(r); if (err != nil) {
                return err;
            }
        }
        c.append(Ꮡ(new code(op: opstore, v: c.pushVariable(name))));
        return Ꮡc.compileFunc(
            Ꮡ(new Func(
                Name: "_modify",
                Args: slice<ж<Query>>([
                    Ꮡl,
                    Ꮡ(new Query(Term: Ꮡ(new Term(
                        Type: TermTypeFunc,
                        Func: Ꮡ(new Func(
                            Name: op.getFunc(),
                            Args: [
                                Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeIdentity)))),
                                Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeFunc, Func: Ꮡ(new Func(Name: name))))))
                            ]
                        ))
                    ))))
                ])
            )));
    }
    return default;

}

internal static error compileBind(this ж<compiler> Ꮡc, ж<Query> l, ж<Query> r, slice<ж<Pattern>> patterns) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
        c.append(Ꮡ(new code(op: opdup)));
        c.append(Ꮡ(new code(op: opexpbegin)));
        {
            var err = Ꮡc.compileQuery(l); if (err != nil) {
                return err;
            }
        }
        ref var pc = ref heap(new nint(), out var Ꮡpc);
        slice<array<nint>> vs = default;
        foreach (var (i, p) in range(patterns)) {
            ref var pcc = ref heap(new nint(), out var Ꮡpcc);
            error err = default;
            if (i < len(patterns) - 1) {
                defer(Ꮡc.lazy(() => Ꮡ(new code(op: opforkalt, v: Ꮡpcc.Value))), ref ᒐ);
            }
            if (0 < i) {
                foreach (var (_, vᴛ1) in range(vs)) {
                    ref var v = ref heap(new array<nint>(2), out var Ꮡv);
                    v = vᴛ1.Clone();

                    c.append(Ꮡ(new code(op: oppush, v: default)));
                    c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
                }
            }
            (vs, err) = Ꮡc.compilePattern(GoReflect.WithElemDims(vs[..0], 2), p); if (err != nil) {
                return err;
            }
            if (i < len(patterns) - 1) {
                defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: Ꮡpc.Value))), ref ᒐ);
                pcc = len(c.codes);
            }
        }
        if (len(patterns) > 1) {
            pc = len(c.codes);
        }
        if (len(patterns) == 1 && c.codes[len(c.codes) - 2].op == opexpbegin) {
            c.codes[len(c.codes) - 2].op = opnop;
        } else {
            c.append(Ꮡ(new code(op: opexpend)));
        }
        return Ꮡc.compileQuery(r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object indexˢ = (@string)"_index"u8;

internal static (slice<array<nint>>, error) compilePattern(this ж<compiler> Ꮡc, slice<array<nint>> vs, ж<Pattern> Ꮡp) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var p = ref Ꮡp.DerefOrNull();

    error err = default;
    c.appendCodeInfo(Ꮡp.OrTypedNil());
    if (p.Name != ""u8) {
        ref var v = ref heap<array<nint>>(c.pushVariable(p.Name), out var Ꮡv);
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        return (builtin.append(vs, v.Clone()), default);
    } else if (len(p.Array) > 0) {
        ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        foreach (var (iᴛ1, pΔ1) in range(p.Array)) {
            ref var i = ref heap(new nint(), out var Ꮡi);
            i = iᴛ1;

            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            c.append(Ꮡ(new code(op: opindexarray, v: i)));
            (vs, err) = Ꮡc.compilePattern(vs, pΔ1); if (err != nil) {
                return (default, err);
            }
        }
        return (vs, default);
    } else if (len(p.Object) > 0) {
        ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        foreach (var (_, kv) in range(p.Object)) {
            ref var key = ref heap(new @string(), out var Ꮡkey);
            ref var name = ref heap(new @string(), out var Ꮡname);
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            key = kv.Key; if (key != ""u8) {
                if (key[0] == '$') {
                    (key, name) = (key[1..], key);
                }
            } else if (kv.KeyString != nil) {
                key = kv.KeyString.Str; if (key == ""u8) {
                    var errΔ1 = Ꮡc.compileString(kv.KeyString, nil); if (errΔ1 != nil) {
                        return (default, errΔ1);
                    }
                }
            } else if (kv.KeyQuery != nil) {
                var errΔ2 = Ꮡc.compileQuery(kv.KeyQuery); if (errΔ2 != nil) {
                    return (default, errΔ2);
                }
            }
            if (key != ""u8) {
                c.append(Ꮡ(new code(op: opindex, v: key)));
            } else {
                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: oppush, v: default)));
                c.append(Ꮡ(new code(op: opcall, v: new any[]{(internalFuncs["_index"].callback).OrTypedNilFunc(), (nint)2, indexˢ}.array())));
            }
            if (name != ""u8) {
                if (kv.Val != nil) {
                    c.append(Ꮡ(new code(op: opdup)));
                }
                (vs, err) = Ꮡc.compilePattern(vs, Ꮡ(new Pattern(Name: name))); if (err != nil) {
                    return (default, err);
                }
            }
            if (kv.Val != nil) {
                (vs, err) = Ꮡc.compilePattern(vs, kv.Val); if (err != nil) {
                    return (default, err);
                }
            }
        }
        return (vs, default);
    } else {
        return (default, fmt.Errorf("invalid pattern: %s"u8, Ꮡp.OrTypedNil()));
    }
}

internal static error compileIf(this ж<compiler> Ꮡc, ж<If> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        c.append(Ꮡ(new code(op: opdup)));
        c.append(Ꮡ(new code(op: opexpbegin)));
        nint pc = len(c.codes);
        var f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Cond); if (err != nil) {
                return err;
            }
        }
        f();
        if (pc == len(c.codes)) {
            c.codes = c.codes.slice(0, pc - 1);
        } else {
            c.append(Ꮡ(new code(op: opexpend)));
        }
        nint pcc = len(c.codes);
        var setjumpifnot = Ꮡc.lazy(() => Ꮡ(new code(op: opjumpifnot, v: len(Ꮡc.Value.codes))));
        f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Then); if (err != nil) {
                return err;
            }
        }
        f();
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes)))), ref ᒐ);
        setjumpifnot();
        if (len(e.Elif) > 0) {
            return Ꮡc.compileIf(Ꮡ(new If(e.Elif[0].Cond, e.Elif[0].Then, e.Elif[1..], e.Else)));
        }
        if (e.Else != nil) {
            defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
            defer(() => {
                if (pcc + 4 == len(Ꮡc.Value.codes) && Ꮡc.Value.codes[pcc + 1] != nil && Ꮡc.Value.codes[pcc + 1].op == opconst && Ꮡc.Value.codes[pcc + 3] != nil && Ꮡc.Value.codes[pcc + 3].op == opconst) {
                    Ꮡc.Value.codes[pc - 2].op = opnop;
                    Ꮡc.Value.codes[pcc + 1].op = oppush;
                    Ꮡc.Value.codes[pcc + 3].op = oppush;
                }
            }, ref ᒐ);
            return Ꮡc.compileQuery(e.Else);
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileTry(this ж<compiler> Ꮡc, ж<Try> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        var setforktrybegin = Ꮡc.lazy(() => Ꮡ(new code(op: opforktrybegin, v: len(Ꮡc.Value.codes))));
        var f = c.newScopeDepth();
        var err = Ꮡc.compileQuery(e.Body); if (err != nil) {
            return err;
        }
        f();
        c.append(Ꮡ(new code(op: opforktryend)));
        defer(Ꮡc.lazy(() => Ꮡ(new code(op: opjump, v: len(Ꮡc.Value.codes)))), ref ᒐ);
        setforktrybegin();
        if (e.Catch != nil) {
            defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
            return Ꮡc.compileQuery(e.Catch);
        }
        c.append(Ꮡ(new code(op: opbacktrack)));
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileReduce(this ж<compiler> Ꮡc, ж<Reduce> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
        c.append(Ꮡ(new code(op: opdup)));
        ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
        var f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Start); if (err != nil) {
                return err;
            }
        }
        f();
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        var setfork = Ꮡc.lazy(() => Ꮡ(new code(op: opfork, v: len(Ꮡc.Value.codes))));
        {
            var err = Ꮡc.compileQuery(e.Query); if (err != nil) {
                return err;
            }
        }
        {
            var (_, err) = Ꮡc.compilePattern(default, e.Pattern); if (err != nil) {
                return err;
            }
        }
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Update); if (err != nil) {
                return err;
            }
        }
        f();
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        c.append(Ꮡ(new code(op: opbacktrack)));
        setfork();
        c.append(Ꮡ(new code(op: oppop)));
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileForeach(this ж<compiler> Ꮡc, ж<Foreach> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
        c.append(Ꮡ(new code(op: opdup)));
        ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
        var f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Start); if (err != nil) {
                return err;
            }
        }
        f();
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        {
            var err = Ꮡc.compileQuery(e.Query); if (err != nil) {
                return err;
            }
        }
        {
            var (_, err) = Ꮡc.compilePattern(default, e.Pattern); if (err != nil) {
                return err;
            }
        }
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        f = c.newScopeDepth();
        {
            var err = Ꮡc.compileQuery(e.Update); if (err != nil) {
                return err;
            }
        }
        f();
        c.append(Ꮡ(new code(op: opdup)));
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        if (e.Extract != nil) {
            defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
            return Ꮡc.compileQuery(e.Extract);
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileLabel(this ж<compiler> Ꮡc, ж<Label> Ꮡe) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    c.appendCodeInfo(Ꮡe.OrTypedNil());
    ref var v = ref heap<array<nint>>(c.pushVariable("$%" + e.Ident[1..]), out var Ꮡv);
    c.append(Ꮡ(new code(op: opforklabel, v: v.Clone())));
    return Ꮡc.compileQuery(e.Body);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object breakˢ = (@string)"_break"u8;

internal static error compileBreak(this ref compiler c, @string label) {
    ref var v = ref heap<array<nint>>(out var Ꮡv);
    (v, var err) = c.lookupVariable("$%" + label[1..]);
    if (err != nil) {
        return new breakErrorжerror(Ꮡ(new breakError(label, default)));
    }
    c.append(Ꮡ(new code(op: oppop)));
    c.append(Ꮡ(new code(op: opload, v: v.Clone())));
    c.append(Ꮡ(new code(op: opcall, v: new any[]{(funcBreak(label)).OrTypedNilFunc(), (nint)0, breakˢ}.array())));
    return default;
}

internal static Func<any, slice<any>, any> funcBreak(@string label) {
    return (any v, slice<any> _) => Ꮡ(new breakError(label, v));
}

internal static error compileTerm(this ж<compiler> Ꮡc, ж<Term> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        if (len(e.SuffixList) > 0) {
            var s = e.SuffixList[len(e.SuffixList) - 1];
            ref var t = ref heap<Term>(e, out var Ꮡt);
            t.SuffixList = t.SuffixList.slice(0, len(e.SuffixList) - 1);
            return Ꮡc.compileTermSuffix(Ꮡt, s);
        }
        switch (e.Type) {
        case var ᴛ when ᴛ == TermTypeIdentity: {
            return default;
        }
        case var ᴛ when ᴛ == TermTypeRecurse: {
            return Ꮡc.compileFunc(Ꮡ(new Func(Name: "recurse")));
        }
        case var ᴛ when ᴛ == TermTypeNull: {
            c.append(Ꮡ(new code(op: opconst, v: default)));
            return default;
        }
        case var ᴛ when ᴛ == TermTypeTrue: {
            c.append(Ꮡ(new code(op: opconst, v: true)));
            return default;
        }
        case var ᴛ when ᴛ == TermTypeFalse: {
            c.append(Ꮡ(new code(op: opconst, v: false)));
            return default;
        }
        case var ᴛ when ᴛ == TermTypeIndex: {
            return Ꮡc.compileIndex(Ꮡ(new Term(Type: TermTypeIdentity)), e.Index);
        }
        case var ᴛ when ᴛ == TermTypeFunc: {
            return Ꮡc.compileFunc(e.Func);
        }
        case var ᴛ when ᴛ == TermTypeObject: {
            return Ꮡc.compileObject(e.Object);
        }
        case var ᴛ when ᴛ == TermTypeArray: {
            return Ꮡc.compileArray(e.Array);
        }
        case var ᴛ when ᴛ == TermTypeNumber: {
            c.append(Ꮡ(new code(op: opconst, v: toNumber(e.Number))));
            return default;
        }
        case var ᴛ when ᴛ == TermTypeUnary: {
            return Ꮡc.compileUnary(e.Unary);
        }
        case var ᴛ when ᴛ == TermTypeFormat: {
            return Ꮡc.compileFormat(e.Format, e.Str);
        }
        case var ᴛ when ᴛ == TermTypeString: {
            return Ꮡc.compileString(e.Str, nil);
        }
        case var ᴛ when ᴛ == TermTypeIf: {
            return Ꮡc.compileIf(e.If);
        }
        case var ᴛ when ᴛ == TermTypeTry: {
            return Ꮡc.compileTry(e.Try);
        }
        case var ᴛ when ᴛ == TermTypeReduce: {
            return Ꮡc.compileReduce(e.Reduce);
        }
        case var ᴛ when ᴛ == TermTypeForeach: {
            return Ꮡc.compileForeach(e.Foreach);
        }
        case var ᴛ when ᴛ == TermTypeLabel: {
            return Ꮡc.compileLabel(e.Label);
        }
        case var ᴛ when ᴛ == TermTypeBreak: {
            return c.compileBreak(e.Break);
        }
        case var ᴛ when ᴛ == TermTypeQuery: {
            defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
            return Ꮡc.compileQuery(e.Query);
        }
        default: {
            throw panic("invalid term: " + e.String());
            break;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileIndex(this ж<compiler> Ꮡc, ж<Term> e, ж<Index> Ꮡx) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    var k = x.toIndexKey(); if (k != nil) {
        var err = Ꮡc.compileTerm(e); if (err != nil) {
            return err;
        }
        c.appendCodeInfo(Ꮡx.OrTypedNil());
        c.append(Ꮡ(new code(op: opindex, v: k)));
        return default;
    }
    c.appendCodeInfo(Ꮡx.OrTypedNil());
    if (x.Str != nil) {
        return Ꮡc.compileCall("_index", [Ꮡ(new Query(Term: e)), Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeString, Str: x.Str))))]);
    }
    if (!x.IsSlice) {
        return Ꮡc.compileCall("_index", [Ꮡ(new Query(Term: e)), x.Start]);
    }
    if (x.Start == nil) {
        return Ꮡc.compileCall("_slice", [Ꮡ(new Query(Term: e)), x.End, Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeNull))))]);
    }
    if (x.End == nil) {
        return Ꮡc.compileCall("_slice", [Ꮡ(new Query(Term: e)), Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeNull)))), x.Start]);
    }
    return Ꮡc.compileCall("_slice", [Ꮡ(new Query(Term: e)), x.End, x.Start]);
}

internal static error compileFunc(this ж<compiler> Ꮡc, ж<Func> Ꮡe) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    if (len(e.Args) == 0) {
        var (f, v) = c.lookupFuncOrVariable(e.Name); if (f != nil) {
            return Ꮡc.compileCallPc(f, e.Args);
        } else if (v != nil) {
            if (e.Name[0] == '$') {
                c.append(Ꮡ(new code(op: oppop)));
                c.append(Ꮡ(new code(op: opload, v: v.index.Clone())));
            } else {
                c.append(Ꮡ(new code(op: opload, v: v.index.Clone())));
                c.append(Ꮡ(new code(op: opcallpc)));
            }
            return default;
        } else if (e.Name == "$ENV"u8 || e.Name == "env"u8) {
            var env = new map<@string, any>();
            if (c.environLoader != nil) {
                foreach (var (_, kv) in range(c.environLoader())) {
                    var (k, vΔ1, ok) = strings.Cut(kv, "="); if (ok && k != ""u8) {
                        env[k] = vΔ1;
                    }
                }
            }
            c.append(Ꮡ(new code(op: opconst, v: env)));
            return default;
        } else if (e.Name[0] == '$') {
            return new variableNotFoundErrorжerror(Ꮡ(new variableNotFoundError(e.Name)));
        }
    } else {
        for (nint i = len(c.scopes) - 1; i >= 0; i--) {
            var s = c.scopes[i];
            for (nint j = len(s.funcs) - 1; j >= 0; j--) {
                var f = s.funcs[j]; if (f.name == e.Name && f.argcnt == len(e.Args)) {
                    return Ꮡc.compileCallPc(f, e.Args);
                }
            }
        }
    }
    {
        var f = c.lookupBuiltin(e.Name, len(e.Args)); if (f != nil) {
            return Ꮡc.compileCallPc(f, e.Args);
        }
    }
    if (builtinFuncDefs[e.Name, ꟷ] is (var fds, true)) {
        bool compiled = default;
        foreach (var (_, fd) in range(fds)) {
            if (len(fd.Args) == len(e.Args)) {
                var err = Ꮡc.compileFuncDef(fd, true); if (err != nil) {
                    return err;
                }
                compiled = true;
                break;
            }
        }
        if (!compiled) {
            var exprᴛ1 = e.Name;
            switch (ᐧ) {
            case {} when exprᴛ1 == "_assign"u8: {
                Ꮡc.compileAssign();
                break;
            }
            case {} when exprᴛ1 == "_modify"u8: {
                Ꮡc.compileModify();
                break;
            }
            case {} when exprᴛ1 == "_last"u8: {
                Ꮡc.compileLast();
                break;
            }}

        }
        var f = c.lookupBuiltin(e.Name, len(e.Args)); if (f != nil) {
            return Ꮡc.compileCallPc(f, e.Args);
        }
    }
    {
        var (fn, ok) = internalFuncs[e.Name, ꟷ]; if (ok && fn.accept(len(e.Args))) {
            var exprᴛ2 = e.Name;
            switch (ᐧ) {
            case {} when exprᴛ2 == "empty"u8: {
                c.append(Ꮡ(new code(op: opbacktrack)));
                return default;
            }
            case {} when exprᴛ2 == "path"u8: {
                c.append(Ꮡ(new code(op: oppathbegin)));
                {
                    var err = Ꮡc.compileCall(e.Name, e.Args); if (err != nil) {
                        return err;
                    }
                }
                c.codes[len(c.codes) - 1] = Ꮡ(new code(op: oppathend));
                return default;
            }
            case {} when exprᴛ2 == "builtins"u8: {
                return Ꮡc.compileCallInternal(
                    new any[]{Ꮡc.funcBuiltins, (nint)0, e.Name}.array(),
                    e.Args,
                    true,
                    -1);
            }
            case {} when exprᴛ2 == "input"u8: {
                if (c.inputIter == nil) {
                    return new inputNotAllowedErrorжerror(Ꮡ(new inputNotAllowedError()));
                }
                return Ꮡc.compileCallInternal(
                    new any[]{Ꮡc.funcInput, (nint)0, e.Name}.array(),
                    e.Args,
                    true,
                    -1);
            }
            case {} when exprᴛ2 == "modulemeta"u8: {
                return Ꮡc.compileCallInternal(
                    new any[]{Ꮡc.funcModulemeta, (nint)0, e.Name}.array(),
                    e.Args,
                    true,
                    -1);
            }
            case {} when exprᴛ2 == "debug"u8: {
                var setfork = Ꮡc.lazy(() => Ꮡ(new code(op: opfork, v: len(Ꮡc.Value.codes))));
                {
                    var err = Ꮡc.compileQuery(e.Args[0]); if (err != nil) {
                        return err;
                    }
                }
                {
                    var err = Ꮡc.compileFunc(Ꮡ(new Func(Name: "debug"))); if (err != nil) {
                        var (_, okΔ2) = err._<ж<funcNotFoundError>>(ᐧ); if (okΔ2) {
                            err = new funcNotFoundErrorжerror(Ꮡ(new funcNotFoundError(Ꮡe)));
                        }
                        return err;
                    }
                }
                c.append(Ꮡ(new code(op: opbacktrack)));
                setfork();
                return default;
            }
            case {} when exprᴛ2 == "_match"u8: {
                return Ꮡc.compileCallInternal(
                    new any[]{Ꮡc.funcMatch, len(e.Args), e.Name}.array(),
                    e.Args,
                    true,
                    -1);
            }
            default: {
                return Ꮡc.compileCall(e.Name, e.Args);
            }}

        }
    }
    {
        var (fn, ok) = c.customFuncs[e.Name, ꟷ]; if (ok && fn.accept(len(e.Args))) {
            var err = Ꮡc.compileCallInternal(
                new any[]{(fn.callback).OrTypedNilFunc(), len(e.Args), e.Name}.array(),
                e.Args,
                true,
                -1); if (err != nil) {
                return err;
            }
            if (fn.iter) {
                c.append(Ꮡ(new code(op: opiter)));
            }
            return default;
        }
    }
    return new funcNotFoundErrorжerror(Ꮡ(new funcNotFoundError(Ꮡe)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object allocatorˢ = (@string)"_allocator"u8;
private static readonly object setpathˢ2 = (@string)"_setpath"u8;

internal static void compileAssign(this ж<compiler> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(Ꮡc.appendBuiltin("_assign", 2), ref ᒐ);
        var scope = c.newScope();
        ref var v = ref heap<array<nint>>(out var Ꮡv);
        v = new nint[]{scope.id, 0}.array();
        ref var p = ref heap<array<nint>>(out var Ꮡp);
        p = new nint[]{scope.id, 1}.array();
        ref var x = ref heap<array<nint>>(out var Ꮡx);
        x = new nint[]{scope.id, 2}.array();
        ref var a = ref heap<array<nint>>(out var Ꮡa);
        a = new nint[]{scope.id, 3}.array();
        ref var w = ref heap<array<nint>>(out var Ꮡw);
        w = new nint[]{scope.id, 4}.array();
        ref var q = ref heap<array<nint>>(out var Ꮡq);
        q = new nint[]{scope.id, 5}.array();
        c.appends(
            Ꮡ(new code(op: opscope, v: new nint[]{scope.id, 6, 2}.array())),
            Ꮡ(new code(op: opstore, v: v.Clone())),
            Ꮡ(new code(op: opstore, v: p.Clone())),
            Ꮡ(new code(op: opstore, v: x.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opexpbegin)),
            Ꮡ(new code(op: opload, v: x.Clone())),
            Ꮡ(new code(op: opcallpc)),
            Ꮡ(new code(op: opstore, v: x.Clone())),
            Ꮡ(new code(op: opexpend)),
            Ꮡ(new code(op: oppush, v: default)),
            Ꮡ(new code(op: opcall, v: new any[]{funcAllocator, (nint)0, allocatorˢ}.array())),
            Ꮡ(new code(op: opstore, v: a.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opfork, v: len(c.codes) + 30)),
            Ꮡ(new code(op: opdup)),
            Ꮡ(new code(op: opstore, v: w.Clone())),
            Ꮡ(new code(op: oppathbegin)),
            Ꮡ(new code(op: opload, v: p.Clone())),
            Ꮡ(new code(op: opcallpc)),
            Ꮡ(new code(op: opload, v: w.Clone())),
            Ꮡ(new code(op: oppathend)),
            Ꮡ(new code(op: opstore, v: q.Clone())),
            Ꮡ(new code(op: opload, v: a.Clone())),
            Ꮡ(new code(op: opload, v: x.Clone())),
            Ꮡ(new code(op: opload, v: q.Clone())),
            Ꮡ(new code(op: opload, v: w.Clone())),
            Ꮡ(new code(op: opcall, v: new any[]{funcSetpathWithAllocator, (nint)3, setpathˢ2}.array())),
            Ꮡ(new code(op: opstore, v: w.Clone())),
            Ꮡ(new code(op: opbacktrack)),
            Ꮡ(new code(op: oppop)),
            Ꮡ(new code(op: opload, v: w.Clone())),
            Ꮡ(new code(op: opret)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object getpathˢ = (@string)"getpath"u8;
private static readonly object delpathsˢ = (@string)"_delpaths"u8;

internal static void compileModify(this ж<compiler> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(Ꮡc.appendBuiltin("_modify", 2), ref ᒐ);
        var scope = c.newScope();
        ref var v = ref heap<array<nint>>(out var Ꮡv);
        v = new nint[]{scope.id, 0}.array();
        ref var p = ref heap<array<nint>>(out var Ꮡp);
        p = new nint[]{scope.id, 1}.array();
        ref var f = ref heap<array<nint>>(out var Ꮡf);
        f = new nint[]{scope.id, 2}.array();
        ref var d = ref heap<array<nint>>(out var Ꮡd);
        d = new nint[]{scope.id, 3}.array();
        ref var a = ref heap<array<nint>>(out var Ꮡa);
        a = new nint[]{scope.id, 4}.array();
        ref var l = ref heap<array<nint>>(out var Ꮡl);
        l = new nint[]{scope.id, 5}.array();
        c.appends(
            Ꮡ(new code(op: opscope, v: new nint[]{scope.id, 6, 2}.array())),
            Ꮡ(new code(op: opstore, v: v.Clone())),
            Ꮡ(new code(op: opstore, v: p.Clone())),
            Ꮡ(new code(op: opstore, v: f.Clone())),
            Ꮡ(new code(op: oppush, v: slice<any>([]))),
            Ꮡ(new code(op: opstore, v: d.Clone())),
            Ꮡ(new code(op: oppush, v: default)),
            Ꮡ(new code(op: opcall, v: new any[]{funcAllocator, (nint)0, allocatorˢ}.array())),
            Ꮡ(new code(op: opstore, v: a.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opfork, v: len(c.codes) + 39)),
            Ꮡ(new code(op: oppathbegin)),
            Ꮡ(new code(op: opload, v: p.Clone())),
            Ꮡ(new code(op: opcallpc)),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: oppathend)),
            Ꮡ(new code(op: opstore, v: p.Clone())),
            Ꮡ(new code(op: opforklabel, v: l.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opfork, v: len(c.codes) + 36)),
            Ꮡ(new code(op: oppop)),
            Ꮡ(new code(op: opload, v: a.Clone())),
            Ꮡ(new code(op: opload, v: p.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opcall, v: new any[]{(internalFuncs["getpath"].callback).OrTypedNilFunc(), (nint)1, getpathˢ}.array())),
            Ꮡ(new code(op: opload, v: f.Clone())),
            Ꮡ(new code(op: opcallpc)),
            Ꮡ(new code(op: opload, v: p.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opcall, v: new any[]{funcSetpathWithAllocator, (nint)3, setpathˢ2}.array())),
            Ꮡ(new code(op: opstore, v: v.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opfork, v: len(c.codes) + 34)),
            Ꮡ(new code(op: opjump, v: len(c.codes) + 38)),
            Ꮡ(new code(op: opload, v: l.Clone())),
            Ꮡ(new code(op: opcall, v: new any[]{(funcBreak("")).OrTypedNilFunc(), (nint)0, breakˢ}.array())),
            Ꮡ(new code(op: opload, v: p.Clone())),
            Ꮡ(new code(op: opappend, v: d.Clone())),
            Ꮡ(new code(op: opbacktrack)),
            Ꮡ(new code(op: oppop)),
            Ꮡ(new code(op: opload, v: a.Clone())),
            Ꮡ(new code(op: opload, v: d.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opcall, v: new any[]{funcDelpathsWithAllocator, (nint)2, delpathsˢ}.array())),
            Ꮡ(new code(op: opret)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void compileLast(this ж<compiler> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(Ꮡc.appendBuiltin("_last", 1), ref ᒐ);
        var scope = c.newScope();
        ref var v = ref heap<array<nint>>(out var Ꮡv);
        v = new nint[]{scope.id, 0}.array();
        ref var g = ref heap<array<nint>>(out var Ꮡg);
        g = new nint[]{scope.id, 1}.array();
        ref var x = ref heap<array<nint>>(out var Ꮡx);
        x = new nint[]{scope.id, 2}.array();
        c.appends(
            Ꮡ(new code(op: opscope, v: new nint[]{scope.id, 3, 1}.array())),
            Ꮡ(new code(op: opstore, v: v.Clone())),
            Ꮡ(new code(op: opstore, v: g.Clone())),
            Ꮡ(new code(op: oppush, v: true)),
            Ꮡ(new code(op: opstore, v: x.Clone())),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opfork, v: len(c.codes) + 13)),
            Ꮡ(new code(op: opload, v: g.Clone())),
            Ꮡ(new code(op: opcallpc)),
            Ꮡ(new code(op: opstore, v: v.Clone())),
            Ꮡ(new code(op: oppush, v: false)),
            Ꮡ(new code(op: opstore, v: x.Clone())),
            Ꮡ(new code(op: opbacktrack)),
            Ꮡ(new code(op: oppop)),
            Ꮡ(new code(op: opload, v: x.Clone())),
            Ꮡ(new code(op: opjumpifnot, v: len(c.codes) + 17)),
            Ꮡ(new code(op: opbacktrack)),
            Ꮡ(new code(op: opload, v: v.Clone())),
            Ꮡ(new code(op: opret)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct funcBuiltins_funcNameArity /*dyn*/ {
    internal @string name;
    internal nint arity;
}

internal static any funcBuiltins(this ref compiler c, any _Δp1, slice<any> _Δp2) {
    slice<ж<funcBuiltins_funcNameArity>> xs = default;
    foreach (var (_, fds) in builtinFuncDefs) {
        foreach (var (_, fd) in range(fds)) {
            if (fd.Name[0] != '_') {
                xs = builtin.append(xs, Ꮡ(new funcBuiltins_funcNameArity(fd.Name, len(fd.Args))));
            }
        }
    }
    foreach (var (kᴛ1, fn) in internalFuncs) {
        ref var name = ref heap(new @string(), out var Ꮡname);
        name = kᴛ1;

        if (name[0] != '_') {
            for ((nint iᴛ1, nint cnt) = (0, fn.argcount); cnt > 0; (iᴛ1, cnt) = (iᴛ1 + 1, cnt >> 1)) {
                ref var i = ref heap<nint>(out var Ꮡi);
                i = iᴛ1;
                if ((cnt & 1) > 0) {
                    xs = builtin.append(xs, Ꮡ(new funcBuiltins_funcNameArity(name, i)));
                }
                iᴛ1 = i;
            }
        }
    }
    foreach (var (kᴛ2, fn) in c.customFuncs) {
        ref var name = ref heap(new @string(), out var Ꮡname);
        name = kᴛ2;

        if (name[0] != '_') {
            for ((nint iᴛ2, nint cnt) = (0, fn.argcount); cnt > 0; (iᴛ2, cnt) = (iᴛ2 + 1, cnt >> 1)) {
                ref var i = ref heap<nint>(out var Ꮡi);
                i = iᴛ2;
                if ((cnt & 1) > 0) {
                    xs = builtin.append(xs, Ꮡ(new funcBuiltins_funcNameArity(name, i)));
                }
                iᴛ2 = i;
            }
        }
    }
    var xsʗ1 = xs;
    sort.Slice(xs, (nint i, nint j) => xsʗ1[i].name < xsʗ1[j].name || xsʗ1[i].name == xsʗ1[j].name && xsʗ1[i].arity < xsʗ1[j].arity);
    var ys = new slice<any>(len(xs));
    foreach (var (i, x) in range(xs)) {
        ys[i] = x.name + "/" + strconv.Itoa(x.arity);
    }
    return ys;
}

internal static any funcInput(this ref compiler c, any _Δp1, slice<any> _Δp2) {
    var (v, ok) = c.inputIter.Next();
    if (!ok) {
        return errors.New("break");
    }
    return v;
}

internal static any funcModulemeta(this ref compiler c, any v, slice<any> _) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("modulemeta", v));
    }
    if (c.moduleLoader == nil) {
        return fmt.Errorf("cannot load module: %q"u8, s);
    }
    ж<Query> q = default;
    error err = default;
    var (moduleLoader, okΔ1) = c.moduleLoader._<compileImport_typeᴛ2>(ᐧ); if (okΔ1) {
        (q, err) = moduleLoader.LoadModuleWithMeta(s, default); if (err != nil) {
            return err;
        }
    } else if (c.moduleLoader._<compileImport_typeᴛ3>(out var moduleLoaderΔ1)) {
        (q, err) = moduleLoaderΔ1.LoadModule(s); if (err != nil) {
            return err;
        }
    }
    var meta = q.Meta.ToValue();
    if (meta == nil) {
        meta = new map<@string, any>();
    }
    meta["defs"] = listModuleDefs(ref (q).DerefOrNull());
    meta["deps"] = listModuleDeps(ref (q).DerefOrNull());
    return meta;
}

internal partial struct listModuleDefs_funcNameArity /*dyn*/ {
    internal @string name;
    internal nint arity;
}

internal static slice<any> listModuleDefs(ref Query q) {
    slice<ж<listModuleDefs_funcNameArity>> xs = default;
    foreach (var (_, fd) in range(q.FuncDefs)) {
        if (fd.Name[0] != '_') {
            xs = builtin.append(xs, Ꮡ(new listModuleDefs_funcNameArity(fd.Name, len(fd.Args))));
        }
    }
    var xsʗ1 = xs;
    sort.Slice(xs, (nint i, nint j) => xsʗ1[i].name < xsʗ1[j].name || xsʗ1[i].name == xsʗ1[j].name && xsʗ1[i].arity < xsʗ1[j].arity);
    var defs = new slice<any>(len(xs));
    foreach (var (i, x) in range(xs)) {
        defs[i] = x.name + "/" + strconv.Itoa(x.arity);
    }
    return defs;
}

internal static slice<any> listModuleDeps(ref Query q) {
    var deps = new slice<any>(len(q.Imports));
    foreach (var (j, i) in range(q.Imports)) {
        var v = i.Meta.ToValue();
        if (v == nil) {
            v = new map<@string, any>();
        }
        @string relpath = i.ImportPath;
        if (relpath == ""u8) {
            relpath = i.IncludePath;
        }
        v["relpath"] = relpath;
        if (i.ImportAlias != ""u8) {
            v["as"] = strings.TrimPrefix(i.ImportAlias, "$");
        }
        v["is_data"] = strings.HasPrefix(i.ImportAlias, "$");
        deps[j] = v;
    }
    return deps;
}

internal static any funcMatch(this ж<compiler> Ꮡc, any v, slice<any> args) {
    return funcMatch(v, args[0], args[1], args[2], Ꮡc.ᏑregexpCache);
}

internal static error compileObject(this ж<compiler> Ꮡc, ж<Object> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        if (len(e.KeyVals) == 0) {
            c.append(Ꮡ(new code(op: opconst, v: new map<@string, any>{})));
            return default;
        }
        defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
        ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
        c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
        nint pc = len(c.codes);
        foreach (var (_, kv) in range(e.KeyVals)) {
            var err = Ꮡc.compileObjectKeyVal(v, kv); if (err != nil) {
                return err;
            }
        }
        c.append(Ꮡ(new code(op: opobject, v: len(e.KeyVals))));
        nint l = len(e.KeyVals);
        if (pc + l * 3 + 1 != len(c.codes)) {
            return default;
        }
        foreach (var i in range(l)) {
            if (c.codes[pc + i * 3].op != oppush || c.codes[pc + i * 3 + 1].op != opload || c.codes[pc + i * 3 + 2].op != opconst) {
                return default;
            }
        }
        var w = new map<@string, any>(l);
        foreach (var i in range(l)) {
            w[c.codes[pc + i * 3].v._<@string>()] = c.codes[pc + i * 3 + 2].v;
        }
        c.codes[pc - 1] = Ꮡ(new code(op: opconst, v: w));
        c.codes = c.codes.slice(0, pc);
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileObjectKeyVal(this ж<compiler> Ꮡc, /*[2]*/ array<nint> v, ж<ObjectKeyVal> Ꮡkv) {
    v = v.Clone();
    ref var c = ref Ꮡc.DerefOrNull();
    ref var kv = ref Ꮡkv.DerefOrNull();

    ref var key = ref heap<@string>(kv.Key, out var Ꮡkey); if (key != ""u8) {
        if (key[0] == '$') {
            if (kv.Val == nil) {
                c.append(Ꮡ(new code(op: oppush, v: key[1..])));
            }
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            var err = Ꮡc.compileFunc(Ꮡ(new Func(Name: key))); if (err != nil) {
                return err;
            }
        } else {
            c.append(Ꮡ(new code(op: oppush, v: key)));
            if (kv.Val == nil) {
                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: opindex, v: key)));
            }
        }
    } else if (kv.KeyString is var keyΔ1 && keyΔ1 != nil) {
        if (keyΔ1.Queries == nil) {
            c.append(Ꮡ(new code(op: oppush, v: keyΔ1.Str)));
            if (kv.Val == nil) {
                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: opindex, v: keyΔ1.Str)));
            }
        } else {
            c.append(Ꮡ(new code(op: opload, v: v.Clone())));
            var err = Ꮡc.compileString(keyΔ1, nil); if (err != nil) {
                return err;
            }
            if (kv.Val == nil) {
                c.append(Ꮡ(new code(op: opdup)));
                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: oppush, v: default)));
                c.append(Ꮡ(new code(op: opcall, v: new any[]{(internalFuncs["_index"].callback).OrTypedNilFunc(), (nint)2, indexˢ}.array())));
            }
        }
    } else if (kv.KeyQuery != nil) {
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        var f = c.newScopeDepth();
        var err = Ꮡc.compileQuery(kv.KeyQuery); if (err != nil) {
            return err;
        }
        f();
    }
    if (kv.Val != nil) {
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
        var err = Ꮡc.compileQuery(kv.Val); if (err != nil) {
            return err;
        }
    }
    return default;
}

internal static error compileArray(this ж<compiler> Ꮡc, ж<Array> Ꮡe) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var e = ref Ꮡe.DerefOrNull();

        c.appendCodeInfo(Ꮡe.OrTypedNil());
        if (e.Query == nil) {
            c.append(Ꮡ(new code(op: opconst, v: slice<any>([]))));
            return default;
        }
        c.append(Ꮡ(new code(op: oppush, v: slice<any>([]))));
        ref var arr = ref heap<array<nint>>(c.newVariable(), out var Ꮡarr);
        c.append(Ꮡ(new code(op: opstore, v: arr.Clone())));
        nint pc = len(c.codes);
        var setfork = Ꮡc.lazy(() => Ꮡ(new code(op: opfork, v: len(Ꮡc.Value.codes))));
        defer(Ꮡc.Value.newScopeDepth(), ref ᒐ);
        var err = Ꮡc.compileQuery(e.Query); if (err != nil) {
            return err;
        }
        c.append(Ꮡ(new code(op: opappend, v: arr.Clone())));
        c.append(Ꮡ(new code(op: opbacktrack)));
        setfork();
        c.append(Ꮡ(new code(op: oppop)));
        c.append(Ꮡ(new code(op: opload, v: arr.Clone())));
        if (e.Query.Op == OpPipe) {
            return default;
        }
        if ((len(c.codes) - pc) % 3 != 0) {
            return default;
        }
        nint l = (len(c.codes) - pc - 3) / 3;
        foreach (var i in range(l)) {
            if (c.codes[pc + i].op != opfork || c.codes[pc + i * 2 + l].op != opconst || (i < l - 1 && c.codes[pc + i * 2 + l + 1].op != opjump)) {
                return default;
            }
        }
        var v = new slice<any>(l);
        foreach (var i in range(l)) {
            v[i] = c.codes[pc + i * 2 + l].v;
        }
        c.codes[pc - 2] = Ꮡ(new code(op: opconst, v: v));
        c.codes = c.codes.slice(0, pc - 1);
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error compileUnary(this ж<compiler> Ꮡc, ж<Unary> Ꮡe) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();

    c.appendCodeInfo(Ꮡe.OrTypedNil());
    var v = e.toNumber(); if (v != nil) {
        c.append(Ꮡ(new code(op: opconst, v: v)));
        return default;
    }
    var err = Ꮡc.compileTerm(e.Term); if (err != nil) {
        return err;
    }
    switch (e.Op) {
    case var ᴛ when ᴛ == OpAdd: {
        return Ꮡc.compileCall("_plus", default);
    }
    case var ᴛ when ᴛ == OpSub: {
        return Ꮡc.compileCall("_negate", default);
    }
    default: {
        return fmt.Errorf("unexpected operator in Unary: %s"u8, e.Op);
    }}

}

internal static error compileFormat(this ж<compiler> Ꮡc, @string format, ж<ΔString> str) {
    var f = formatToFunc(format);
    if (f == nil) {
        f = Ꮡ(new Func(
            Name: "format",
            Args: [Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeString, Str: Ꮡ(new ΔString(Str: format[1..]))))))]
        ));
    }
    if (str == nil) {
        return Ꮡc.compileFunc(f);
    }
    return Ꮡc.compileString(str, f);
}

internal static ж<Func> formatToFunc(@string format) {
    var exprᴛ1 = format;
    switch (ᐧ) {
    case {} when exprᴛ1 == "@text"u8: {
        return Ꮡ(new Func(Name: "tostring"));
    }
    case {} when exprᴛ1 == "@json"u8: {
        return Ꮡ(new Func(Name: "tojson"));
    }
    case {} when exprᴛ1 == "@html"u8: {
        return Ꮡ(new Func(Name: "_tohtml"));
    }
    case {} when exprᴛ1 == "@uri"u8: {
        return Ꮡ(new Func(Name: "_touri"));
    }
    case {} when exprᴛ1 == "@urid"u8: {
        return Ꮡ(new Func(Name: "_tourid"));
    }
    case {} when exprᴛ1 == "@csv"u8: {
        return Ꮡ(new Func(Name: "_tocsv"));
    }
    case {} when exprᴛ1 == "@tsv"u8: {
        return Ꮡ(new Func(Name: "_totsv"));
    }
    case {} when exprᴛ1 == "@sh"u8: {
        return Ꮡ(new Func(Name: "_tosh"));
    }
    case {} when exprᴛ1 == "@base64"u8: {
        return Ꮡ(new Func(Name: "_tobase64"));
    }
    case {} when exprᴛ1 == "@base64d"u8: {
        return Ꮡ(new Func(Name: "_tobase64d"));
    }
    default: {
        return default;
    }}

}

internal static error compileString(this ж<compiler> Ꮡc, ж<ΔString> Ꮡs, ж<Func> Ꮡf) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();
    ref var f = ref Ꮡf.DerefOrNull();

    if (s.Queries == nil) {
        c.append(Ꮡ(new code(op: opconst, v: s.Str)));
        return default;
    }
    if (Ꮡf == nil) {
        Ꮡf = Ꮡ(new Func(Name: "tostring")); f = ref Ꮡf.DerefOrNull();
    }
    ж<Query> q = default;
    foreach (var (_, vᴛ1) in range(s.Queries)) {
        var e = vᴛ1;

        if (e.Term.Str == nil) {
            e = Ꮡ(new Query(Left: e, Op: OpPipe, Right: Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeFunc, Func: Ꮡf))))));
        }
        if (q == nil) {
            q = e;
        } else {
            q = Ꮡ(new Query(Left: q, Op: OpAdd, Right: e));
        }
    }
    return Ꮡc.compileQuery(q);
}

internal static error compileTermSuffix(this ж<compiler> Ꮡc, ж<Term> Ꮡe, ж<Suffix> Ꮡs) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var e = ref Ꮡe.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.Index != nil) {
        return Ꮡc.compileIndex(Ꮡe, s.Index);
    } else if (s.Iter) {
        var err = Ꮡc.compileTerm(Ꮡe); if (err != nil) {
            return err;
        }
        c.append(Ꮡ(new code(op: opiter)));
        return default;
    } else if (s.Optional) {
        if (len(e.SuffixList) > 0) {
            var u = e.SuffixList[len(e.SuffixList) - 1].toTerm(); if (u != nil) {
                e.SuffixList = e.SuffixList.slice(0, len(e.SuffixList) - 1);
                var err = Ꮡc.compileTerm(Ꮡe); if (err != nil) {
                    return err;
                }
                Ꮡe = u; e = ref Ꮡe.DerefOrNull();
            }
        }
        return Ꮡc.compileTry(Ꮡ(new Try(Body: Ꮡ(new Query(Term: Ꮡe)))));
    } else {
        return fmt.Errorf("invalid suffix: %s"u8, Ꮡs.OrTypedNil());
    }
}

internal static error compileCall(this ж<compiler> Ꮡc, @string name, slice<ж<Query>> args) {
    ref var c = ref Ꮡc.DerefOrNull();

    var fn = internalFuncs[name];
    nint indexing = default;
    var exprᴛ1 = name;
    switch (ᐧ) {
    case {} when exprᴛ1 == "_index"u8 || exprᴛ1 == "_slice"u8: {
        indexing = 1;
        break;
    }
    case {} when exprᴛ1 == "getpath"u8: {
        indexing = 0;
        break;
    }
    default: {
        indexing = -1;
        break;
    }}

    var err = Ꮡc.compileCallInternal(
        new any[]{(fn.callback).OrTypedNilFunc(), len(args), name}.array(),
        args,
        true,
        indexing); if (err != nil) {
        return err;
    }
    if (fn.iter) {
        c.append(Ꮡ(new code(op: opiter)));
    }
    return default;
}

internal static error compileCallPc(this ж<compiler> Ꮡc, ж<funcinfo> Ꮡfn, slice<ж<Query>> args) {
    ref var fn = ref Ꮡfn.DerefOrNull();

    return Ꮡc.compileCallInternal(fn.pc, args, false, -1);
}

internal static error compileCallInternal(this ж<compiler> Ꮡc, any fn, slice<ж<Query>> args, bool @internal, nint indexing) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(args) == 0) {
        c.append(Ꮡ(new code(op: opcall, v: fn)));
        return default;
    }
    ref var v = ref heap<array<nint>>(c.newVariable(), out var Ꮡv);
    c.append(Ꮡ(new code(op: opstore, v: v.Clone())));
    if (indexing >= 0) {
        c.append(Ꮡ(new code(op: opexpbegin)));
    }
    for (nint i = len(args) - 1; i >= 0; i--) {
        ref var pc = ref heap<nint>(len(c.codes) + 1, out var Ꮡpc);
        ref var name = ref heap<@string>("lambda:"u8 + strconv.Itoa(pc), out var Ꮡname);
        var err = Ꮡc.compileFuncDef(Ꮡ(new FuncDef(Name: name, Body: args[i])), false); if (err != nil) {
            return err;
        }
        if (@internal) {
            switch (len(c.codes) - pc) {
            case 2: {
                nint j = len(c.codes) - 3;
                c.codes[j] = Ꮡ(new code(op: opload, v: v.Clone()));
                c.codes = c.codes.slice(0, j + 1);
                var s = c.scopes[len(c.scopes) - 1];
                s.funcs = s.funcs.slice(0, len(s.funcs) - 1);
                c.deleteCodeInfo(name);
                break;
            }
            case 3: {
                nint j = len(c.codes) - 4;
                if (c.codes[j + 2].op == opconst) {
                    c.codes[j] = Ꮡ(new code(op: oppush, v: c.codes[j + 2].v));
                    c.codes = c.codes.slice(0, j + 1);
                } else {
                    c.codes[j] = Ꮡ(new code(op: opload, v: v.Clone()));
                    c.codes[j + 1] = c.codes[j + 2];
                    c.codes = c.codes.slice(0, j + 2);
                }
                var s = c.scopes[len(c.scopes) - 1];
                s.funcs = s.funcs.slice(0, len(s.funcs) - 1);
                c.deleteCodeInfo(name);
                break;
            }
            default: {
                c.append(Ꮡ(new code(op: opload, v: v.Clone())));
                c.append(Ꮡ(new code(op: oppushpc, v: pc)));
                c.append(Ꮡ(new code(op: opcallpc)));
                break;
            }}

        } else {
            c.append(Ꮡ(new code(op: oppushpc, v: pc)));
        }
        if (i == indexing) {
            if (c.codes[len(c.codes) - 2].op == opexpbegin) {
                c.codes[len(c.codes) - 2] = c.codes[len(c.codes) - 1];
                c.codes = c.codes.slice(0, len(c.codes) - 1);
            } else {
                c.append(Ꮡ(new code(op: opexpend)));
            }
        }
    }
    if (indexing > 0) {
        c.append(Ꮡ(new code(op: oppush, v: default)));
    } else {
        c.append(Ꮡ(new code(op: opload, v: v.Clone())));
    }
    c.append(Ꮡ(new code(op: opcall, v: fn)));
    return default;
}

internal static void append(this ref compiler c, ж<code> code) {
    c.codes = builtin.append(c.codes, code);
}

internal static void appends(this ref compiler c, params ꓸꓸꓸжcode codesʗp) {
    var codes = codesʗp.sslice();

    c.codes = builtin.appendꓸꓸꓸ(c.codes, codes);
}

internal static Action lazy(this ж<compiler> Ꮡc, Func<ж<code>> f) {
    ref var c = ref Ꮡc.DerefOrNull();

    nint i = len(c.codes);
    c.codes = builtin.append(c.codes, (ж<code>)(nil));
    return () => {
        Ꮡc.Value.codes[i] = f();
    };
}

internal static void optimizeTailRec(this ref compiler c) {
    var pcsᴷ = new stackAppend<nint>();

    slice<nint> pcs = default;
    var scopes = new map<nint, bool>{};
L:
    foreach (var (i, code) in range(c.codes)) {
        switch (code.op) {
        case var ᴛ when ᴛ == opscope: {
            pcs = builtin.append(pcsᴷ, pcs, i);
            var v = code.v._<array<nint>>(); if (v[2] == 0) {
                scopes[i] = v[1] == 0;
            }
            break;
        }
        case var ᴛ when ᴛ == opcall: {
            bool canjumpΔ1 = default;
            {
                var (j, okΔ1) = code.v._<nint>(ᐧ); if (!okΔ1 || len(pcs) == 0 || pcs[len(pcs) - 1] != j) {
                    break;
                } else {
                    (canjumpΔ1, okΔ1) = scopes[j, ꟷ]; if (!okΔ1) {
                        break;
                    }
                }
            }
            for (nint j = i + 1; j < len(c.codes); ) {
                var exprᴛ2 = c.codes[j].op;
                var matchᴛ1 = false;
                if (exprᴛ2 == opjump) { matchᴛ1 = true;
                    j = c.codes[j].v._<nint>();
                }
                else if (exprᴛ2 == opret) { matchᴛ1 = true;
                    if (canjumpΔ1) {
                        code.op = opjump;
                        code.v = pcs[len(pcs) - 1] + 1;
                    } else {
                        code.op = opcallrec;
                    }
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1) { /* default: */
                    goto continue_L;
                }

            }
            break;
        }
        case var ᴛ when ᴛ == opret: {
            if (len(pcs) == 0) {
                goto break_L;
            }
            pcs = pcs.slice(0, len(pcs) - 1);
            break;
        }}

continue_L:;
    }
break_L:;
}

internal static void optimizeCodeOps(this ref compiler c) {
    for ((nint i, var next) = (len(c.codes) - 1, (ж<code>)nil); i >= 0; i--) {
        var code = c.codes[i];
        switch (code.op) {
        case var ᴛ when ᴛ == oppush || ᴛ == opdup || ᴛ == opload: {
            switch (next.op) {
            case var ᴛ2 when ᴛ2 == oppop: {
                code.op = opnop;
                next.op = opnop;
                break;
            }
            case var ᴛ2 when ᴛ2 == opconst: {
                code.op = opnop;
                next.op = oppush;
                break;
            }}

            break;
        }
        case var ᴛ when ᴛ == opjump || ᴛ == opjumpifnot: {
            nint j = code.v._<nint>(); if (j - 1 == i) {
                code.op = opnop;
            } else {
                next = c.codes[j]; if (next.op == opjump) {
                    code.v = next.v;
                }
            }
            break;
        }}

        next = code;
    }
}

} // end gojq_package
