namespace go.github.com.itchyny;

using fmt = fmt_package;

partial class gojq_package {

// type CompilerOption is a methodless func type — rendered inline as its base delegate

public static Action<ж<compiler>> WithModuleLoader(ModuleLoader moduleLoader) {
    return (ж<compiler> c) => {
        c.moduleLoader = moduleLoader;
    };
}

public static Action<ж<compiler>> WithEnvironLoader(Func<slice<@string>> environLoader) {
    return (ж<compiler> c) => {
        c.environLoader = environLoader;
    };
}

public static Action<ж<compiler>> WithVariables(slice<@string> variables) {
    var variablesʗ1 = variables;
    return (ж<compiler> c) => {
        c.variables = variablesʗ1;
    };
}

public static Action<ж<compiler>> WithFunction(@string name, nint minarity, nint maxarity, Func<any, slice<any>, any> f) {
    return withFunction(name, minarity, maxarity, false, f);
}

public static Action<ж<compiler>> WithIterFunction(@string name, nint minarity, nint maxarity, Func<any, slice<any>, Iter> f) {
    return withFunction(name, minarity, maxarity, true,
        (any v, slice<any> args) => f(v, args));
}

internal static Action<ж<compiler>> withFunction(@string name, nint minarity, nint maxarity, bool iter, Func<any, slice<any>, any> f) {
    if (!(0 <= minarity && minarity <= maxarity && maxarity <= 30)) {
        throw panic(fmt.Sprintf("invalid arity for %q: %d, %d"u8, name, minarity, maxarity));
    }
    nint argcount = ((nint)1).Lsh((int64)(maxarity + 1)) - ((nint)1).Lsh((int64)(minarity));
    return (ж<compiler> c) => {
        if (c.customFuncs == nil) {
            c.customFuncs = new map<@string, function>();
        }
        {
            ref var fn = ref heap<function>(out var Ꮡfn);
            (fn, var ok) = c.customFuncs[name, ꟷ]; if (ok) {
                if (fn.iter != iter) {
                    throw panic(fmt.Sprintf("cannot define both iterator and non-iterator functions for %q"u8, name));
                }
                    var fnʗ1 = fn;
                c.customFuncs[name] = new function(
                    argcount | fn.argcount, iter,
                    (any x, slice<any> xs) => {
                        if ((argcount & (((nint)1).Lsh((int64)(len(xs))))) != 0) {
                            return f(x, xs);
                        }
                        return fnʗ1.callback(x, xs);
                    }
                );
            } else {
                c.customFuncs[name] = new function(argcount, iter, f);
            }
        }
    };
}

public static Action<ж<compiler>> WithInputIter(Iter inputIter) {
    return (ж<compiler> c) => {
        c.inputIter = inputIter;
    };
}

} // end gojq_package
