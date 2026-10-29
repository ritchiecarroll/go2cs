// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using url = net.url_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using net;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸreflectꓸValue = Span<reflectꓸValue>;

partial class template_package {

// FuncMap is the type of the map defining the mapping from names to functions.
// Each function must have either a single return value, or two return values of
// which the second has type error. In that case, if the second (error)
// return value evaluates to non-nil during execution, execution terminates and
// Execute returns that error.
//
// Errors returned by Execute wrap the underlying error; call [errors.AsType] to
// unwrap them.
//
// When template execution invokes a function with an argument list, that list
// must be assignable to the function's parameter types. Functions meant to
// apply to arguments of arbitrary type can use parameters of type interface{} or
// of type [reflect.Value]. Similarly, functions meant to return a result of arbitrary
// type can return interface{} or [reflect.Value].
partial struct FuncMap /*map[@string, any]*/;

// builtins returns the FuncMap.
// It is not a global variable so the linker can dead code eliminate
// more when this isn't called. See golang.org/issue/36021.
// TODO: revert this back to a global map once golang.org/issue/2559 is fixed.
internal static FuncMap builtins() {
    return new FuncMap(new map<@string, any>{
        ["and"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, reflectꓸValue>)(and)),
        ["call"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, reflectꓸValue>)(emptyCall)),
        ["html"u8] = ((Funcꓸꓸꓸ<any, @string>)(HTMLEscaper)),
        ["index"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, (reflectꓸValue, error)>)(index)),
        ["slice"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, (reflectꓸValue, error)>)(Δslice)),
        ["js"u8] = ((Funcꓸꓸꓸ<any, @string>)(JSEscaper)),
        ["len"u8] = length,
        ["not"u8] = not,
        ["or"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, reflectꓸValue>)(or)),
        ["print"u8] = ((Funcꓸꓸꓸ<any, @string>)(fmt.Sprint)),
        ["printf"u8] = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintfᶠ)),
        ["println"u8] = ((Funcꓸꓸꓸ<any, @string>)(fmt.Sprintln)),
        ["urlquery"u8] = ((Funcꓸꓸꓸ<any, @string>)(URLQueryEscaper)),

        // Comparisons
        ["eq"u8] = ((Funcꓸꓸꓸ<reflectꓸValue, reflectꓸValue, (bool, error)>)(eq)), // ==
        ["ge"u8] = ge,                                                           // >=
        ["gt"u8] = gt,                                                           // >
        ["le"u8] = le,                                                           // <=
        ["lt"u8] = lt,                                                           // <
        ["ne"u8] = ne                                                            // !=
    });
}

// builtinFuncs lazily computes & caches the builtinFuncs map.
internal static Func<map<@string, reflectꓸValue>> builtinFuncs;
internal static void initᴛbuiltinFuncs() { builtinFuncs = sync.OnceValue(map<@string, reflectꓸValue> () => {
    var funcMap = builtins();
    var m = new map<@string, reflectꓸValue>(len(funcMap));
    addValueFuncs(m, funcMap);
    return m;
}); }

// addValueFuncs adds to values the functions in funcs, converting them to reflect.Values.
internal static void addValueFuncs(map<@string, reflectꓸValue> @out, FuncMap @in) {
    foreach (var (name, fn) in @in) {
        if (!goodName(name)) {
            throw panic(fmt.Errorf("function name %q is not a valid identifier"u8, name));
        }
        var v = reflect.ValueOf(fn);
        if (v.Kind() != reflect.Func) {
            throw panic("value for " + name + " not a function");
        }
        {
            var err = goodFunc(name, v.Type()); if (err != nil) {
                throw panic(err);
            }
        }
        @out[name] = v;
    }
}

// addFuncs adds to values the functions in funcs. It does no checking of the input -
// call addValueFuncs first.
internal static void addFuncs(FuncMap @out, FuncMap @in) {
    foreach (var (name, fn) in @in) {
        @out[name] = fn;
    }
}

// goodFunc reports whether the function or method has the right result signature.
internal static error goodFunc(@string name, reflectꓸType typ) {
    // We allow functions with 1 result or 2 results where the second is an error.
    {
        nint numOut = typ.NumOut();
        switch (ᐧ) {
        case {} when numOut is 1: {
            return default;
        }
        case {} when numOut == 2 && AreEqual(typ.Out(1), errorType): {
            return default;
        }
        case {} when numOut is 2: {
            return fmt.Errorf("invalid function signature for %s: second return value should be error; is %s"u8, name, typ.Out(1));
        }
        default: {
            return fmt.Errorf("function %s has %d return values; should be 1 or 2"u8, name, typ.NumOut());
        }}
    }
}

// goodName reports whether the function name is a valid identifier.
internal static bool goodName(@string name) {
    if (name == ""u8) {
        return false;
    }
    foreach (var (i, r) in name) {
        switch (ᐧ) {
        case {} when r is '_': {
            break;
        }
        case {} when i == 0 && !unicode.IsLetter(r): {
            return false;
        }
        case {} when !unicode.IsLetter(r) && !unicode.IsDigit(r): {
            return false;
        }}
    }
    return true;
}

// findFunction looks for a function in the template, and global map.
internal static (reflectꓸValue v, bool isBuiltin, bool ok) findFunction(@string name, ж<Template> Ꮡtmpl) {
    reflectꓸValue v = new(nil);
    bool isBuiltin = default;
    bool ok = default;
    GoFrame ᒐ = default;
    try {
        ref var tmpl = ref Ꮡtmpl.DerefOrNull();

        if (Ꮡtmpl != nil && tmpl.common != nil) {
            Ꮡtmpl.of(Template.ᏑmuFuncs).RLock();
            defer(Ꮡtmpl.of(Template.ᏑmuFuncs).RUnlock, ref ᒐ);
            {
                var fn = tmpl.execFuncs[name]; if (fn.IsValid()) {
                    (v, isBuiltin, ok) = (fn, false, true); goto ᒐdone;
                }
            }
        }
        {
            var fn = builtinFuncs()[name]; if (fn.IsValid()) {
                (v, isBuiltin, ok) = (fn, true, true); goto ᒐdone;
            }
        }
        (v, isBuiltin, ok) = (new reflectꓸValue(), false, false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (v, isBuiltin, ok);
}

// prepareArg checks if value can be used as an argument of type argType, and
// converts an invalid value to appropriate zero if possible.
internal static (reflectꓸValue, error) prepareArg(reflectꓸValue value, reflectꓸType argType) {
    if (!value.IsValid()) {
        if (!canBeNil(argType)) {
            return (new reflectꓸValue(), fmt.Errorf("value is nil; should be of type %s"u8, argType));
        }
        value = reflect.Zero(argType);
    }
    if (value.Type().AssignableTo(argType)) {
        return (value, default);
    }
    if (intLike(value.Kind()) && intLike(argType.Kind()) && value.Type().ConvertibleTo(argType)) {
        value = value.Convert(argType);
        return (value, default);
    }
    return (new reflectꓸValue(), fmt.Errorf("value has type %s; should be %s"u8, value.Type(), argType));
}

internal static bool intLike(reflectꓸKind typ) {
    switch (typ) {
    case var ᴛ when ᴛ == reflect.ΔInt || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        return true;
    }
    case var ᴛ when ᴛ == reflect.ΔUint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64 || ᴛ == reflect.Uintptr: {
        return true;
    }}
    return false;
}

// indexArg checks if a reflect.Value can be used as an index, and converts it to int if possible.
internal static (nint, error) indexArg(reflectꓸValue index, nint cap) {
    int64 x = default;
    switch (index.Kind()) {
    case var ᴛ when ᴛ == reflect.ΔInt || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        x = index.Int();
        break;
    }
    case var ᴛ when ᴛ == reflect.ΔUint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64 || ᴛ == reflect.Uintptr: {
        x = (int64)index.Uint();
        break;
    }
    case var ᴛ when ᴛ == reflect.Invalid: {
        return (0, fmt.Errorf("cannot index slice/array with nil"u8));
    }
    default: {
        return (0, fmt.Errorf("cannot index slice/array with type %s"u8, index.Type()));
    }}
    if (x < 0 || (nint)x < 0 || (nint)x > cap) {
        return (0, fmt.Errorf("index out of range: %d"u8, x));
    }
    return ((nint)x, default);
}

// Indexing.

// index returns the result of indexing its first argument by the following
// arguments. Thus "index x 1 2 3" is, in Go syntax, x[1][2][3]. Each
// indexed item must be a map, slice, or array.
internal static (reflectꓸValue, error) index(reflectꓸValue item, params ꓸꓸꓸreflectꓸValue indexesʗp) {
    var indexes = indexesʗp.sslice();

    item = indirectInterface(item);
    if (!item.IsValid()) {
        return (new reflectꓸValue(), fmt.Errorf("index of untyped nil"u8));
    }
    foreach (var (_, vᴛ1) in indexes) {
        var index = vᴛ1;

        index = indirectInterface(index);
        bool isNil = default;
        {
            (item, isNil) = indirect(item); if (isNil) {
                return (new reflectꓸValue(), fmt.Errorf("index of nil pointer"u8));
            }
        }
        switch (item.Kind()) {
        case var ᴛ when ᴛ == reflect.Array || ᴛ == reflect.ΔSlice || ᴛ == reflect.ΔString: {
            var (x, err) = indexArg(index, item.Len());
            if (err != nil) {
                return (new reflectꓸValue(), err);
            }
            item = item.Index(x);
            break;
        }
        case var ᴛ when ᴛ == reflect.Map: {
            var (indexΔ2, err) = prepareArg(index, item.Type().Key());
            if (err != nil) {
                return (new reflectꓸValue(), err);
            }
            {
                var x = item.MapIndex(indexΔ2); if (x.IsValid()) {
                    item = x;
                } else {
                    item = reflect.Zero(item.Type().Elem());
                }
            }
            break;
        }
        case var ᴛ when ᴛ == reflect.Invalid: {
            // the loop holds invariant: item.IsValid()
            throw panic("unreachable");
            break;
        }
        default: {
            return (new reflectꓸValue(), fmt.Errorf("can't index item of type %s"u8, item.Type()));
        }}
    }
    return (item, default);
}

// Slicing.

// slice returns the result of slicing its first argument by the remaining
// arguments. Thus "slice x 1 2" is, in Go syntax, x[1:2], while "slice x"
// is x[:], "slice x 1" is x[1:], and "slice x 1 2 3" is x[1:2:3]. The first
// argument must be a string, slice, or array.
internal static (reflectꓸValue, error) Δslice(reflectꓸValue item, params ꓸꓸꓸreflectꓸValue indexesʗp) {
    var indexes = indexesʗp.sslice();

    item = indirectInterface(item);
    if (!item.IsValid()) {
        return (new reflectꓸValue(), fmt.Errorf("slice of untyped nil"u8));
    }
    bool isNil = default;
    {
        (item, isNil) = indirect(item); if (isNil) {
            return (new reflectꓸValue(), fmt.Errorf("slice of nil pointer"u8));
        }
    }
    if (len(indexes) > 3) {
        return (new reflectꓸValue(), fmt.Errorf("too many slice indexes: %d"u8, len(indexes)));
    }
    nint cap = default;
    switch (item.Kind()) {
    case var ᴛ when ᴛ == reflect.ΔString: {
        if (len(indexes) == 3) {
            return (new reflectꓸValue(), fmt.Errorf("cannot 3-index slice a string"u8));
        }
        cap = item.Len();
        break;
    }
    case var ᴛ when ᴛ == reflect.Array || ᴛ == reflect.ΔSlice: {
        cap = item.Cap();
        break;
    }
    default: {
        return (new reflectꓸValue(), fmt.Errorf("can't slice item of type %s"u8, item.Type()));
    }}
    // set default values for cases item[:], item[i:].
    var idx = new nint[]{0, item.Len()}.array(3);
    foreach (var (i, index) in indexes) {
        var (x, err) = indexArg(index, cap);
        if (err != nil) {
            return (new reflectꓸValue(), err);
        }
        idx[i] = x;
    }
    // given item[i:j], make sure i <= j.
    if (idx[0] > idx[1]) {
        return (new reflectꓸValue(), fmt.Errorf("invalid slice index: %d > %d"u8, idx[0], idx[1]));
    }
    if (len(indexes) < 3) {
        return (item.Slice(idx[0], idx[1]), default);
    }
    // given item[i:j:k], make sure i <= j <= k.
    if (idx[1] > idx[2]) {
        return (new reflectꓸValue(), fmt.Errorf("invalid slice index: %d > %d"u8, idx[1], idx[2]));
    }
    return (item.Slice3(idx[0], idx[1], idx[2]), default);
}

// Length

// length returns the length of the item, with an error if it has no defined length.
internal static (nint, error) length(reflectꓸValue item) {
    (item, var isNil) = indirect(item);
    if (isNil) {
        return (0, fmt.Errorf("len of nil pointer"u8));
    }
    switch (item.Kind()) {
    case var ᴛ when ᴛ == reflect.Array || ᴛ == reflect.Chan || ᴛ == reflect.Map || ᴛ == reflect.ΔSlice || ᴛ == reflect.ΔString: {
        return (item.Len(), default);
    }}
    return (0, fmt.Errorf("len of type %s"u8, item.Type()));
}

// Function invocation
internal static reflectꓸValue emptyCall(reflectꓸValue fn, params ꓸꓸꓸreflectꓸValue argsʗp) {
    var args = argsʗp.sslice();

    throw panic("unreachable"); // implemented as a special case in evalCall
}

// call returns the result of evaluating the first argument as a function.
// The function must return 1 result, or 2 results, the second of which is an error.
internal static (reflectꓸValue, error) call(@string name, reflectꓸValue fn, params ꓸꓸꓸreflectꓸValue argsʗp) {
    var args = argsʗp.sslice();

    fn = indirectInterface(fn);
    if (!fn.IsValid()) {
        return (new reflectꓸValue(), fmt.Errorf("call of nil"u8));
    }
    var typ = fn.Type();
    if (typ.Kind() != reflect.Func) {
        return (new reflectꓸValue(), fmt.Errorf("non-function %s of type %s"u8, name, typ));
    }

    {
        var err = goodFunc(name, typ); if (err != nil) {
            return (new reflectꓸValue(), err);
        }
    }
    nint numIn = typ.NumIn();
    reflectꓸType dddType = default;
    if (typ.IsVariadic()) {
        if (len(args) < numIn - 1) {
            return (new reflectꓸValue(), fmt.Errorf("wrong number of args for %s: got %d want at least %d"u8, name, len(args), numIn - 1));
        }
        dddType = typ.In(numIn - 1).Elem();
    } else {
        if (len(args) != numIn) {
            return (new reflectꓸValue(), fmt.Errorf("wrong number of args for %s: got %d want %d"u8, name, len(args), numIn));
        }
    }
    var argv = new slice<reflectꓸValue>(len(args), () => new(nil));
    foreach (var (i, vᴛ1) in args) {
        var arg = vᴛ1;

        arg = indirectInterface(arg);
        // Compute the expected type. Clumsy because of variadics.
        var argType = dddType;
        if (!typ.IsVariadic() || i < numIn - 1) {
            argType = typ.In(i);
        }

        error err = default;
        {
            (argv[i], err) = prepareArg(arg, argType); if (err != nil) {
                return (new reflectꓸValue(), fmt.Errorf("arg %d: %w"u8, i, err));
            }
        }
    }
    return safeCall(fn, argv);
}

// safeCall runs fun.Call(args), and returns the resulting value and error, if
// any. If the call panics, the panic value is returned as an error.
internal static (reflectꓸValue val, error err) safeCall(reflectꓸValue fun, slice<reflectꓸValue> args) {
    reflectꓸValue val = new(nil);
    error err = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    {
                        var (e, ok) = r._<error>(ᐧ); if (ok) {
                            err = e;
                        } else {
                            err = fmt.Errorf("%v"u8, r);
                        }
                    }
                }
            }
        }, ref ᒐ);
        var ret = fun.Call(args);
        if (len(ret) == 2 && !ret[1].IsNil()) {
            (val, err) = (ret[0], ret[1].Interface()._<error>()); goto ᒐdone;
        }
        (val, err) = (ret[0], default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (val, err);
}

// Boolean logic.
internal static bool truth(reflectꓸValue arg) {
    var (t, _) = isTrue(indirectInterface(arg));
    return t;
}

// and computes the Boolean AND of its arguments, returning
// the first false argument it encounters, or the last argument.
internal static reflectꓸValue and(reflectꓸValue arg0, params ꓸꓸꓸreflectꓸValue argsʗp) {
    var args = argsʗp.sslice();

    throw panic("unreachable"); // implemented as a special case in evalCall
}

// or computes the Boolean OR of its arguments, returning
// the first true argument it encounters, or the last argument.
internal static reflectꓸValue or(reflectꓸValue arg0, params ꓸꓸꓸreflectꓸValue argsʗp) {
    var args = argsʗp.sslice();

    throw panic("unreachable"); // implemented as a special case in evalCall
}

// not returns the Boolean negation of its argument.
internal static bool not(reflectꓸValue arg) {
    return !truth(arg);
}

// Comparison.
// TODO: Perhaps allow comparison between signed and unsigned integers.
internal static error errBadComparisonType = errors.New("invalid type for comparison"u8);
internal static error errNoComparison = errors.New("missing argument for comparison"u8);

partial struct kind /*num:nint*/;

internal static kind invalidKind => /* iota */ 0;
internal static kind boolKind => 1;
internal static kind complexKind => 2;
internal static kind intKind => 3;
internal static kind floatKind => 4;
internal static kind stringKind => 5;
internal static kind uintKind => 6;

internal static (kind, error) basicKind(reflectꓸValue v) {
    switch (v.Kind()) {
    case var ᴛ when ᴛ == reflect.ΔBool: {
        return (boolKind, default);
    }
    case var ᴛ when ᴛ == reflect.ΔInt || ᴛ == reflect.Int8 || ᴛ == reflect.Int16 || ᴛ == reflect.Int32 || ᴛ == reflect.Int64: {
        return (intKind, default);
    }
    case var ᴛ when ᴛ == reflect.ΔUint || ᴛ == reflect.Uint8 || ᴛ == reflect.Uint16 || ᴛ == reflect.Uint32 || ᴛ == reflect.Uint64 || ᴛ == reflect.Uintptr: {
        return (uintKind, default);
    }
    case var ᴛ when ᴛ == reflect.Float32 || ᴛ == reflect.Float64: {
        return (floatKind, default);
    }
    case var ᴛ when ᴛ == reflect.Complex64 || ᴛ == reflect.Complex128: {
        return (complexKind, default);
    }
    case var ᴛ when ᴛ == reflect.ΔString: {
        return (stringKind, default);
    }}
    return (invalidKind, errBadComparisonType);
}

// isNil returns true if v is the zero reflect.Value, or nil of its type.
internal static bool isNil(reflectꓸValue v) {
    if (!v.IsValid()) {
        return true;
    }
    switch (v.Kind()) {
    case var ᴛ when ᴛ == reflect.Chan || ᴛ == reflect.Func || ᴛ == reflect.ΔInterface || ᴛ == reflect.Map || ᴛ == reflect.ΔPointer || ᴛ == reflect.ΔSlice: {
        return v.IsNil();
    }}
    return false;
}

// canCompare reports whether v1 and v2 are both the same kind, or one is nil.
// Called only when dealing with nillable types, or there's about to be an error.
internal static bool canCompare(reflectꓸValue v1, reflectꓸValue v2) {
    reflectꓸKind k1 = v1.Kind();
    reflectꓸKind k2 = v2.Kind();
    if (k1 == k2) {
        return true;
    }
    // We know the type can be compared to nil.
    return k1 == reflect.Invalid || k2 == reflect.Invalid;
}

// eq evaluates the comparison a == b || a == c || ...
internal static (bool, error) eq(reflectꓸValue arg1, params ꓸꓸꓸreflectꓸValue arg2ʗp) {
    var arg2 = arg2ʗp.sslice();

    arg1 = indirectInterface(arg1);
    if (len(arg2) == 0) {
        return (false, errNoComparison);
    }
    var (k1, _) = basicKind(arg1);
    foreach (var (_, vᴛ1) in arg2) {
        var arg = vᴛ1;

        arg = indirectInterface(arg);
        var (k2, _) = basicKind(arg);
        var truth = false;
        if (k1 != k2) {
            // Special case: Can compare integer values regardless of type's sign.
            switch (ᐧ) {
            case {} when k1 == intKind && k2 == uintKind: {
                truth = arg1.Int() >= 0 && (uint64)arg1.Int() == arg.Uint();
                break;
            }
            case {} when k1 == uintKind && k2 == intKind: {
                truth = arg.Int() >= 0 && arg1.Uint() == (uint64)arg.Int();
                break;
            }
            default: {
                if (arg1.IsValid() && arg.IsValid()) {
                    return (false, fmt.Errorf("incompatible types for comparison: %v and %v"u8, arg1.Type(), arg.Type()));
                }
                break;
            }}
        } else {
            switch (k1) {
            case var ᴛ when ᴛ == boolKind: {
                truth = arg1.Bool() == arg.Bool();
                break;
            }
            case var ᴛ when ᴛ == complexKind: {
                truth = arg1.Complex() == arg.Complex();
                break;
            }
            case var ᴛ when ᴛ == floatKind: {
                truth = arg1.Float() == arg.Float();
                break;
            }
            case var ᴛ when ᴛ == intKind: {
                truth = arg1.Int() == arg.Int();
                break;
            }
            case var ᴛ when ᴛ == stringKind: {
                truth = arg1.String() == arg.String();
                break;
            }
            case var ᴛ when ᴛ == uintKind: {
                truth = arg1.Uint() == arg.Uint();
                break;
            }
            default: {
                if (!canCompare(arg1, arg)) {
                    return (false, fmt.Errorf("non-comparable types %s: %v, %s: %v"u8, arg1, arg1.Type(), arg.Type(), arg));
                }
                if (isNil(arg1) || isNil(arg)) {
                    truth = isNil(arg) == isNil(arg1);
                } else {
                    if (!arg.Type().Comparable()) {
                        return (false, fmt.Errorf("non-comparable type %s: %v"u8, arg, arg.Type()));
                    }
                    truth = AreEqual(arg1.Interface(), arg.Interface());
                }
                break;
            }}
        }
        if (truth) {
            return (true, default);
        }
    }
    return (false, default);
}

// ne evaluates the comparison a != b.
internal static (bool, error) ne(reflectꓸValue arg1, reflectꓸValue arg2) {
    // != is the inverse of ==.
    var (equal, err) = eq(arg1, arg2);
    return (!equal, err);
}

// lt evaluates the comparison a < b.
internal static (bool, error) lt(reflectꓸValue arg1, reflectꓸValue arg2) {
    arg1 = indirectInterface(arg1);
    var (k1, err) = basicKind(arg1);
    if (err != nil) {
        return (false, err);
    }
    arg2 = indirectInterface(arg2);
    (var k2, err) = basicKind(arg2);
    if (err != nil) {
        return (false, err);
    }
    var truth = false;
    if (k1 != k2) {
        // Special case: Can compare integer values regardless of type's sign.
        switch (ᐧ) {
        case {} when k1 == intKind && k2 == uintKind: {
            truth = arg1.Int() < 0 || (uint64)arg1.Int() < arg2.Uint();
            break;
        }
        case {} when k1 == uintKind && k2 == intKind: {
            truth = arg2.Int() >= 0 && arg1.Uint() < (uint64)arg2.Int();
            break;
        }
        default: {
            return (false, fmt.Errorf("incompatible types for comparison: %v and %v"u8, arg1.Type(), arg2.Type()));
        }}
    } else {
        switch (k1) {
        case var ᴛ when ᴛ == boolKind || ᴛ == complexKind: {
            return (false, errBadComparisonType);
        }
        case var ᴛ when ᴛ == floatKind: {
            truth = arg1.Float() < arg2.Float();
            break;
        }
        case var ᴛ when ᴛ == intKind: {
            truth = arg1.Int() < arg2.Int();
            break;
        }
        case var ᴛ when ᴛ == stringKind: {
            truth = arg1.String() < arg2.String();
            break;
        }
        case var ᴛ when ᴛ == uintKind: {
            truth = arg1.Uint() < arg2.Uint();
            break;
        }
        default: {
            throw panic("invalid kind");
            break;
        }}
    }
    return (truth, default);
}

// le evaluates the comparison <= b.
internal static (bool, error) le(reflectꓸValue arg1, reflectꓸValue arg2) {
    // <= is < or ==.
    var (lessThan, err) = lt(arg1, arg2);
    if (lessThan || err != nil) {
        return (lessThan, err);
    }
    return eq(arg1, arg2);
}

// gt evaluates the comparison a > b.
internal static (bool, error) gt(reflectꓸValue arg1, reflectꓸValue arg2) {
    // > is the inverse of <=.
    var (lessOrEqual, err) = le(arg1, arg2);
    if (err != nil) {
        return (false, err);
    }
    return (!lessOrEqual, default);
}

// ge evaluates the comparison a >= b.
internal static (bool, error) ge(reflectꓸValue arg1, reflectꓸValue arg2) {
    // >= is the inverse of <.
    var (lessThan, err) = lt(arg1, arg2);
    if (err != nil) {
        return (false, err);
    }
    return (!lessThan, default);
}

// HTML escaping.
internal static slice<byte> htmlQuot = slice<byte>("&#34;"u8); // shorter than "&quot;"
internal static slice<byte> htmlApos = slice<byte>("&#39;"u8); // shorter than "&apos;" and apos was not in HTML until HTML5
internal static slice<byte> htmlAmp = slice<byte>("&amp;"u8);
internal static slice<byte> htmlLt = slice<byte>("&lt;"u8);
internal static slice<byte> htmlGt = slice<byte>("&gt;"u8);
internal static slice<byte> htmlNull = slice<byte>("\uFFFD"u8);

// HTMLEscape writes to w the escaped HTML equivalent of the plain text data b.
public static void HTMLEscape(io.Writer w, slice<byte> b) {
    nint last = 0;
    foreach (var (i, c) in b) {
        slice<byte> html = default;
        switch (c) {
        case (byte)'\u0000': {
            html = htmlNull;
            break;
        }
        case (byte)'"': {
            html = htmlQuot;
            break;
        }
        case (byte)'\'': {
            html = htmlApos;
            break;
        }
        case (byte)'&': {
            html = htmlAmp;
            break;
        }
        case (byte)'<': {
            html = htmlLt;
            break;
        }
        case (byte)'>': {
            html = htmlGt;
            break;
        }
        default: {
            continue;
            break;
        }}
        w.Write(b.slice(last, i));
        w.Write(html);
        last = i + 1;
    }
    w.Write(b.slice(last));
}

// HTMLEscapeString returns the escaped HTML equivalent of the plain text data s.
public static @string HTMLEscapeString(@string s) {
    // Avoid allocation if we can.
    if (!strings.ContainsAny(s, "'\"&<>\u0000"u8)) {
        return s;
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    HTMLEscape(new strings_BuilderжWriter(Ꮡb), slice<byte>(s));
    return b.String();
}

// HTMLEscaper returns the escaped HTML equivalent of the textual
// representation of its arguments.
public static @string HTMLEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return HTMLEscapeString(evalArgs(args));
}

// JavaScript escaping.
internal static slice<byte> jsLowUni = slice<byte>(@"\u00"u8);
internal static slice<byte> hex = slice<byte>("0123456789ABCDEF"u8);

internal static slice<byte> jsBackslash = slice<byte>(@"\\"u8);
internal static slice<byte> jsApos = slice<byte>(@"\'"u8);
internal static slice<byte> jsQuot = slice<byte>(@"\"""u8);
internal static slice<byte> jsLt = slice<byte>(@"\u003C"u8);
internal static slice<byte> jsGt = slice<byte>(@"\u003E"u8);
internal static slice<byte> jsAmp = slice<byte>(@"\u0026"u8);
internal static slice<byte> jsEq = slice<byte>(@"\u003D"u8);

// JSEscape writes to w the escaped JavaScript equivalent of the plain text data b.
public static void JSEscape(io.Writer w, slice<byte> b) {
    nint last = 0;
    for (nint i = 0; i < len(b); i++) {
        var c = b[i];

        if (!jsIsSpecial((rune)c)) {
            // fast path: nothing to do
            continue;
        }
        w.Write(b.slice(last, i));

        if (c < utf8.RuneSelf) {
            // Quotes, slashes and angle brackets get quoted.
            // Control characters get written as \u00XX.
            switch (c) {
            case (byte)'\\': {
                w.Write(jsBackslash);
                break;
            }
            case (byte)'\'': {
                w.Write(jsApos);
                break;
            }
            case (byte)'"': {
                w.Write(jsQuot);
                break;
            }
            case (byte)'<': {
                w.Write(jsLt);
                break;
            }
            case (byte)'>': {
                w.Write(jsGt);
                break;
            }
            case (byte)'&': {
                w.Write(jsAmp);
                break;
            }
            case (byte)'=': {
                w.Write(jsEq);
                break;
            }
            default: {
                w.Write(jsLowUni);
                var (t, bΔ2) = ((byte)((c >> 4)), (byte)(c & 0x0f));
                w.Write(hex.slice(t, (byte)(t + 1)));
                w.Write(hex.slice(bΔ2, (byte)(bΔ2 + 1)));
                break;
            }}
        } else {
            // Unicode rune.
            var (r, size) = utf8.DecodeRune(b.slice(i));
            if (unicode.IsPrint(r)) {
                w.Write(b.slice(i, i + size));
            } else {
                fmt.Fprintf(w, "\\u%04X"u8, r);
            }
            i += size - 1;
        }
        last = i + 1;
    }
    w.Write(b.slice(last));
}

// JSEscapeString returns the escaped JavaScript equivalent of the plain text data s.
public static @string JSEscapeString(@string s) {
    // Avoid allocation if we can.
    if (strings.IndexFunc(s, jsIsSpecial) < 0) {
        return s;
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    JSEscape(new strings_BuilderжWriter(Ꮡb), slice<byte>(s));
    return b.String();
}

internal static bool jsIsSpecial(rune r) {
    switch (r) {
    case '\\' or '\'' or '"' or '<' or '>' or '&' or '=': {
        return true;
    }}
    return r < ' ' || utf8.RuneSelf <= r;
}

// JSEscaper returns the escaped JavaScript equivalent of the textual
// representation of its arguments.
public static @string JSEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return JSEscapeString(evalArgs(args));
}

// URLQueryEscaper returns the escaped value of the textual representation of
// its arguments in a form suitable for embedding in a URL query.
public static @string URLQueryEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return url.QueryEscape(evalArgs(args));
}

// evalArgs formats the list of arguments into a string. It is therefore equivalent to
//
//	fmt.Sprint(args...)
//
// except that each argument is indirected (if a pointer), as required,
// using the same rules as the default string evaluation during template
// execution.
internal static @string evalArgs(slice<any> args) {
    var ok = false;
    @string s = default;
    // Fast path for simple common case.
    if (len(args) == 1) {
        (s, ok) = args[0]._<@string>(ᐧ);
    }
    if (!ok) {
        foreach (var (i, arg) in args) {
            var (a, okΔ1) = printableValue(reflect.ValueOf(arg));
            if (okΔ1) {
                args[i] = a;
            } // else let fmt do its thing
        }
        s = fmt.Sprint(args.ꓸꓸꓸ);
    }
    return s;
}

} // end template_package
