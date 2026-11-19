// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains printing support for ASTs.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using global::go.go;
using ꓸꓸꓸany = Span<any>;
using static ast_package;

partial class ast_package {

// A FieldFilter may be provided to [Fprint] to control the output.
// type FieldFilter is a methodless func type — rendered inline as its base delegate

// NotNilFilter is a [FieldFilter] that returns true for field values
// that are not nil; it returns false otherwise.
public static bool NotNilFilter(@string _, reflect.Value v) {
    switch (v.Kind()) {
    case var ᴛ when ᴛ == reflect.Chan || ᴛ == reflect.Func || ᴛ == reflect.Interface || ᴛ == reflect.Map || ᴛ == reflect.Pointer || ᴛ == reflect.Slice: {
        return !v.IsNil();
    }}
    return true;
}

// Fprint prints the (sub-)tree starting at AST node x to w.
// If fset != nil, position information is interpreted relative
// to that file set. Otherwise positions are printed as integer
// values (file set specific offsets).
//
// A non-nil [FieldFilter] f may be provided to control the output:
// struct fields for which f(fieldname, fieldvalue) is true are
// printed; all others are filtered from the output. Unexported
// struct fields are never printed.
public static error Fprint(io.Writer w, ж<token.FileSet> fset, any x, Func<@string, reflect.Value, bool> f) {
    return fprint(w, fset, x, f);
}

internal static error /*err*/ fprint(io.Writer w, ж<token.FileSet> fset, any x, Func<@string, reflect.Value, bool> f) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        // setup printer
        ref var p = ref heap<printer>(new printer(
            output: w,
            fset: fset,
            filter: f,
            ptrmap: new map<any, nint>(),
            last: (byte)'\n' // force printing of line number on first line
        ), out var Ꮡp);

        // install error handler
        defer(() => {
            var e = recover(); if (e != nil) {
                err = e._<localError>().err; // re-panics if it's not a localError
            }
        }, ref ᒐ);

        // print x
        if (x == nil) {
            Ꮡp.printf("nil\n");
            goto ᒐdone;
        }
        Ꮡp.print(reflect.ValueOf(x));
        Ꮡp.printf("\n");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// Print prints x to standard output, skipping nil fields.
// Print(fset, x) is the same as Fprint(os.Stdout, fset, x, NotNilFilter).
public static error Print(ж<token.FileSet> fset, any x) {
    return Fprint(new os.FileжWriter(os.Stdout), fset, x, new Func<@string, reflect.Value, bool>(NotNilFilter));
}

partial struct printer {
    internal io.Writer output;
    internal ж<token.FileSet> fset;
    internal Func<@string, reflect.Value, bool> filter;
    internal map<any, nint> ptrmap; // *T -> line number
    internal nint indent;           // current indentation level
    internal byte last;             // the last byte processed by Write
    internal nint line;             // current line number
}

internal static slice<byte> indent = slice<byte>(".  "u8);

} partial class astᴛprinter {

internal static (nint n, error err) Write(this ref printer p, slice<byte> data) {
    nint n = default;
    error err = default;

    nint m = default;
    foreach (var (i, b) in range(data)) {
        // invariant: data[0:n] has been written
        if (b == '\n') {
            (m, err) = p.output.Write(data.slice(n, i + 1));
            n += m;
            if (err != nil) {
                return (n, err);
            }
            p.line++;
        } else if (p.last == '\n') {
            (_, err) = fmt.Fprintf(p.output, "%6d  "u8, p.line);
            if (err != nil) {
                return (n, err);
            }
            for (nint j = p.indent; j > 0; j--) {
                (_, err) = p.output.Write(indent);
                if (err != nil) {
                    return (n, err);
                }
            }
        }
        p.last = b;
    }
    if (len(data) > n) {
        (m, err) = p.output.Write(data.slice(n));
        n += m;
    }
    return (n, err);
}

} partial class ast_package {

// localError wraps locally caught errors so we can distinguish
// them from genuine panics which we don't want to return as errors.
partial struct localError {
    internal error err;
}

} partial class astᴛprinter {

// printf is a convenience wrapper that takes care of print errors.
internal static void printf(this ж<printer> Ꮡp, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (_, err) = fmt.Fprintf(new printerжWriter(Ꮡp), format, args.ꓸꓸꓸ); if (err != nil) {
        throw panic(new localError(err));
    }
}

// Implementation note: Print is written for AST nodes but could be
// used to print arbitrary data structures; such a version should
// probably be in a different package.
//
// Note: This code detects (some) cycles created via pointers but
// not cycles that are created via slices or maps containing the
// same slice or map. Code for general data structures probably
// should catch those as well.
internal static void print(this ж<printer> Ꮡp, reflect.Value x) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (!NotNilFilter(""u8, x)) {
        Ꮡp.printf("nil");
        return;
    }

    switch (x.Kind()) {
    case var ᴛ when ᴛ == reflect.Interface: {
        Ꮡp.print(x.Elem());
        break;
    }
    case var ᴛ when ᴛ == reflect.Map: {
        Ꮡp.printf("%s (len = %d) {", x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            Ꮡp.printf("\n");
            foreach (var (_, key) in range(x.MapKeys())) {
                Ꮡp.print(key);
                Ꮡp.printf(": ");
                Ꮡp.print(x.MapIndex(key));
                Ꮡp.printf("\n");
            }
            p.indent--;
        }
        Ꮡp.printf("}");
        break;
    }
    case var ᴛ when ᴛ == reflect.Pointer: {
        Ꮡp.printf("*");
        // type-checked ASTs may contain cycles - use ptrmap
        // to keep track of objects that have been printed
        // already and print the respective line number instead
        var ptr = x.Interface();
        var (line, exists) = p.ptrmap[ptr, ꟷ]; if (exists) {
            Ꮡp.printf("(obj @ %d)", line);
        } else {
            p.ptrmap[ptr] = p.line;
            Ꮡp.print(x.Elem());
        }
        break;
    }
    case var ᴛ when ᴛ == reflect.Array: {
        Ꮡp.printf("%s {", x.Type());
        if (x.Len() > 0) {
            p.indent++;
            Ꮡp.printf("\n");
            for ((nint i, nint n) = (0, x.Len()); i < n; i++) {
                Ꮡp.printf("%d: ", i);
                Ꮡp.print(x.Index(i));
                Ꮡp.printf("\n");
            }
            p.indent--;
        }
        Ꮡp.printf("}");
        break;
    }
    case var ᴛ when ᴛ == reflect.Slice: {
        var (s, ok) = x.Interface()._<slice<byte>>(ᐧ); if (ok) {
            Ꮡp.printf("%#q", s);
            return;
        }
        Ꮡp.printf("%s (len = %d) {", x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            Ꮡp.printf("\n");
            for ((nint i, nint n) = (0, x.Len()); i < n; i++) {
                Ꮡp.printf("%d: ", i);
                Ꮡp.print(x.Index(i));
                Ꮡp.printf("\n");
            }
            p.indent--;
        }
        Ꮡp.printf("}");
        break;
    }
    case var ᴛ when ᴛ == reflect.Struct: {
        var t = x.Type();
        Ꮡp.printf("%s {", t);
        p.indent++;
        var first = true;
        for ((nint i, nint n) = (0, t.NumField()); i < n; i++) {
            // exclude non-exported fields because their
            // values cannot be accessed via reflection
            @string name = t.Field(i).Name; if (IsExported(name)) {
                var value = x.Field(i);
                if (p.filter == nil || p.filter(name, value)) {
                    if (first) {
                        Ꮡp.printf("\n");
                        first = false;
                    }
                    Ꮡp.printf("%s: ", name);
                    Ꮡp.print(value);
                    Ꮡp.printf("\n");
                }
            }
        }
        p.indent--;
        Ꮡp.printf("}");
        break;
    }
    default: {
        var v = x.Interface();
        switch (v.type()) {
        case @string vΔ1: {
            // print strings in quotes
            Ꮡp.printf("%q", vΔ1);
            return;
        }
        case token.Pos vΔ1: {
            // position values can be printed nicely if we have a file set
            if (p.fset != nil) {
                Ꮡp.printf("%s", p.fset.Position(vΔ1));
                return;
            }
            break;
        }}
        // default
        Ꮡp.printf("%v", v);
        break;
    }}
}

} // end astᴛprinter
