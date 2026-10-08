// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements (error and trace) message formatting support.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using strconv = strconv_package;
using strings = strings_package;
using global::go.go;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

internal static @string sprintf(ж<token.FileSet> Ꮡfset, Func<ж<Package>, @string> qf, bool tpSubscripts, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    foreach (var (i, vᴛ1) in args) {
        var arg = vᴛ1;

        switch (arg.type()) {
        case null: {
            arg = nilˢ;
            break;
        }
        case operand a: {
            throw panic("got operand instead of *operand");
            break;
        }
        case ж<operand> a: {
            arg = operandString(ref (a).DerefOrNull(), qf);
            break;
        }
        case tokenꓸPos a: {
            if (Ꮡfset != nil) {
                arg = Ꮡfset.Position(a).String();
            }
            break;
        }
        case {} Δa when Δa._<ast.Expr>(out var a): {
            arg = ExprString(a);
            break;
        }
        case slice<ast.Expr> a: {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            buf.WriteByte((rune)'[');
            writeExprList(Ꮡbuf, a);
            buf.WriteByte((rune)']');
            arg = Ꮡbuf.String();
            break;
        }
        case {} Δa when Δa._<Object>(out var a): {
            arg = ObjectString(a, qf);
            break;
        }
        case {} Δa when Δa._<ΔType>(out var a): {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var w = newTypeWriter(Ꮡbuf, qf);
            w.Value.tpSubscripts = tpSubscripts;
            w.typ(a);
            arg = Ꮡbuf.String();
            break;
        }
        case slice<ΔType> a: {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var w = newTypeWriter(Ꮡbuf, qf);
            w.Value.tpSubscripts = tpSubscripts;
            buf.WriteByte((rune)'[');
            foreach (var (iΔ1, x) in a) {
                if (iΔ1 > 0) {
                    buf.WriteString(", "u8);
                }
                w.typ(x);
            }
            buf.WriteByte((rune)']');
            arg = Ꮡbuf.String();
            break;
        }
        case slice<ж<TypeParam>> a: {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var w = newTypeWriter(Ꮡbuf, qf);
            w.Value.tpSubscripts = tpSubscripts;
            buf.WriteByte((rune)'[');
            foreach (var (iΔ2, x) in a) {
                if (iΔ2 > 0) {
                    buf.WriteString(", "u8);
                }
                w.typ(new TypeParamжΔType(x));
            }
            buf.WriteByte((rune)']');
            arg = Ꮡbuf.String();
            break;
        }}
        args[i] = arg;
    }
    return fmt.Sprintf(format, args.ꓸꓸꓸ);
}

// check may be nil.
internal static @string sprintf(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var check = ref Ꮡcheck.DerefOrNull();
    ж<token.FileSet> fset = default!;
    Func<ж<Package>, @string> qf = default!;
    if (Ꮡcheck != nil) {
        fset = check.fset;
        qf = (ж<Package> p1) => Ꮡcheck.qualifier(p1);
    }
    return sprintf(fset, qf, false, format, args.ꓸꓸꓸ);
}

internal static void trace(this ж<Checker> Ꮡcheck, tokenꓸPos pos, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var check = ref Ꮡcheck.DerefOrNull();
    fmt.Printf("%s:\t%s%s\n"u8,
        check.fset.Position(pos),
        strings.Repeat(".  "u8, check.indent),
        sprintf(check.fset, new Func<ж<Package>, @string>(Ꮡcheck.qualifier), true, format, args.ꓸꓸꓸ));
}

// dump is only needed for debugging
internal static void dump(this ж<Checker> Ꮡcheck, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    ref var check = ref Ꮡcheck.DerefOrNull();
    fmt.Println(sprintf(check.fset, new Func<ж<Package>, @string>(Ꮡcheck.qualifier), true, format, args.ꓸꓸꓸ));
}

internal static @string qualifier(this ref Checker check, ж<Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.DerefOrNull();

    // Qualify the package unless it's the package being type-checked.
    if (Ꮡpkg != check.pkg) {
        if (check.pkgPathMap == default!) {
            check.pkgPathMap = new map<@string, map<@string, bool>>();
            check.seenPkgMap = new map<ж<Package>, bool>();
            check.markImports(check.pkg);
        }
        // If the same package name was used by multiple packages, display the full path.
        if (len(check.pkgPathMap[pkg.name]) > 1) {
            return strconv.Quote(pkg.path);
        }
        return pkg.name;
    }
    return ""u8;
}

// markImports recursively walks pkg and its imports, to record unique import
// paths in pkgPathMap.
internal static void markImports(this ref Checker check, ж<Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.DerefOrNull();

    if (check.seenPkgMap[Ꮡpkg]) {
        return;
    }
    check.seenPkgMap[Ꮡpkg] = true;
    var (forName, ok) = check.pkgPathMap[pkg.name, ꟷ];
    if (!ok) {
        forName = new map<@string, bool>();
        check.pkgPathMap[pkg.name] = forName;
    }
    forName[pkg.path] = true;
    foreach (var (_, imp) in pkg.imports) {
        check.markImports(imp);
    }
}

// stripAnnotations removes internal (type) annotations from s.
internal static @string stripAnnotations(@string s) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, r) in s) {
        // strip #'s and subscript digits
        if (r < (rune)'₀' || (rune)'₀' + 10 <= r) {
            // '₀' == U+2080
            Ꮡbuf.WriteRune(r);
        }
    }
    if (buf.Len() < len(s)) {
        return buf.String();
    }
    return s;
}

} // end types_package
