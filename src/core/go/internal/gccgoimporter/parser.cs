// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using scanner = text.scanner_package;
using utf8 = unicode.utf8_package;
using global::go.go;
using text;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class gccgoimporter_package {

partial struct parser {
    internal ж<scanner.Scanner> scanner;
    internal @string version;                        // format version
    internal rune tok;                               // current token
    internal @string lit;                            // literal string; only valid for Ident, Int, String tokens
    internal @string pkgpath;                        // package path of imported package
    internal @string pkgname;                        // name of imported package
    internal ж<types.Package> pkg;                   // reference to imported package
    internal map<@string, ж<types.Package>> imports; // package path -> package object
    internal slice<typesꓸType> typeList;             // type number -> type
    internal slice<@string> typeData;                // unparsed type data (v3 and later)
    internal slice<fixupRecord> fixups;              // fixups to apply at end of parsing
    internal InitData initdata;                      // package init priority data
    internal map<nint, @string> aliases;             // maps saved type number to alias name
}

// When reading export data it's possible to encounter a defined type
// N1 with an underlying defined type N2 while we are still reading in
// that defined type N2; see issues #29006 and #29198 for instances
// of this. Example:
//
//   type N1 N2
//   type N2 struct {
//      ...
//      p *N1
//   }
//
// To handle such cases, the parser generates a fixup record (below) and
// delays setting of N1's underlying type until parsing is complete, at
// which point fixups are applied.
partial struct fixupRecord {
    internal ж<types.Named> toUpdate; // type to modify when fixup is processed
    internal typesꓸType target;       // type that was incomplete when fixup was created
}

internal static void init(this ж<parser> Ꮡp, @string filename, io.Reader src, map<@string, ж<types.Package>> imports) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.scanner = @new<scanner.Scanner>();
    Ꮡp.initScanner(filename, src);
    p.imports = imports;
    p.aliases = new map<nint, @string>();
    p.typeList = new slice<typesꓸType>(1, 16);
}

internal static void initScanner(this ж<parser> Ꮡp, @string filename, io.Reader src) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.scanner.Init(src);
    p.scanner.Error = (ж<scanner.Scanner> _, @string msg) => {
        Ꮡp.Value.error(msg);
    };
    p.scanner.Mode = (nuint)((nuint)(UntypedInt)((UntypedInt)(scanner.ScanIdents | scanner.ScanInts) | scanner.ScanFloats) | (nuint)scanner.ScanStrings);
    p.scanner.Whitespace = (uint64)(1 << (int)((rune)'\t') | 4294967296L);
    p.scanner.Value.Filename = filename; // for good error messages
    p.next();
}

partial struct importError {
    internal scanner.Position pos;
    internal error err;
}

internal static @string Error(this importError e) {
    return fmt.Sprintf("import error %s (byte offset = %d): %s"u8, e.pos, e.pos.Offset, e.err);
}

internal static void error(this ref parser p, any err) {
    {
        var (s, ok) = err._<@string>(ᐧ); if (ok) {
            err = errors.New(s);
        }
    }
    // panic with a runtime.Error if err is not an error
    throw panic(new importError(p.scanner.Pos(), err._<error>()));
}

internal static void errorf(this ref parser p, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    p.error(fmt.Errorf(format, args.ꓸꓸꓸ));
}

internal static @string expect(this ref parser p, rune tok) {
    @string lit = p.lit;
    if (p.tok != tok) {
        p.errorf("expected %s, got %s (%s)"u8, scanner.TokenString(tok), scanner.TokenString(p.tok), lit);
    }
    p.next();
    return lit;
}

internal static void expectEOL(this ref parser p) {
    if (p.version == "v1"u8 || p.version == "v2"u8) {
        p.expect(';');
    }
    p.expect('\n');
}

internal static void expectKeyword(this ref parser p, @string keyword) {
    @string lit = p.expect(scanner.Ident);
    if (lit != keyword) {
        p.errorf("expected keyword %s, got %q"u8, keyword, lit);
    }
}

internal static @string parseString(this ref parser p) {
    var (str, err) = strconv.Unquote(p.expect(scanner.ΔString));
    if (err != nil) {
        p.error(err);
    }
    return str;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedEofˢ = (@string)"unexpected EOF"u8;

// unquotedString     = { unquotedStringChar } .
// unquotedStringChar = <neither a whitespace nor a ';' char> .
internal static @string parseUnquotedString(this ref parser p) {
    if (p.tok == scanner.EOF) {
        p.error(unexpectedEofˢ);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Ꮡb.WriteString(p.scanner.TokenText());
    // This loop needs to examine each character before deciding whether to consume it. If we see a semicolon,
    // we need to let it be consumed by p.next().
    for (var ch = p.scanner.Peek(); ch != '\n' && ch != ';' && ch != scanner.EOF && (p.scanner.Whitespace & (((uint64)1).Lsh((nuint)ch))) == 0; ch = p.scanner.Peek()) {
        Ꮡb.WriteRune(ch);
        p.scanner.Next();
    }
    p.next();
    return b.String();
}

internal static void next(this ref parser p) {
    p.tok = p.scanner.Scan();
    switch (p.tok) {
    case var ᴛ when ᴛ == scanner.Ident || ᴛ == scanner.Int || ᴛ == scanner.Float || ᴛ == scanner.ΔString || ᴛ == '·': {
        p.lit = p.scanner.TokenText();
        break;
    }
    default: {
        p.lit = ""u8;
        break;
    }}
}

internal static (@string path, @string name) parseQualifiedName(this ref parser p) {
    return p.parseQualifiedNameStr(p.parseString());
}

internal static (@string path, @string name) parseUnquotedQualifiedName(this ref parser p) {
    return p.parseQualifiedNameStr(p.parseUnquotedString());
}

// qualifiedName = [ ["."] unquotedString "." ] unquotedString .
//
// The above production uses greedy matching.
internal static (@string pkgpath, @string name) parseQualifiedNameStr(this ref parser p, @string unquotedName) {
    @string pkgpath = default;
    @string name = default;

    var parts = strings.Split(unquotedName, "."u8);
    if (parts[0] == "") {
        parts = parts[1..];
    }

    switch (len(parts)) {
    case 0: {
        p.errorf("malformed qualified name: %q"u8, unquotedName);
        break;
    }
    case 1: {
        // unqualified name
        pkgpath = p.pkgpath;
        name = parts[0];
        break;
    }
    default: {
        // qualified name, which may contain periods
        pkgpath = strings.Join(parts.slice(0, len(parts) - 1), "."u8);
        name = parts[len(parts) - 1];
        break;
    }}

    return (pkgpath, name);
}

// getPkg returns the package for a given path. If the package is
// not found but we have a package name, create the package and
// add it to the p.imports map.
internal static ж<types.Package> getPkg(this ref parser p, @string pkgpath, @string name) {
    // package unsafe is not in the imports map - handle explicitly
    if (pkgpath == "unsafe"u8) {
        return types.Unsafe;
    }
    var pkg = p.imports[pkgpath];
    if (pkg == nil && name != ""u8) {
        pkg = types.NewPackage(pkgpath, name);
        p.imports[pkgpath] = pkg;
    }
    return pkg;
}

// parseExportedName is like parseQualifiedName, but
// the package path is resolved to an imported *types.Package.
//
// ExportedName = string [string] .
internal static (ж<types.Package> pkg, @string name) parseExportedName(this ref parser p) {
    ж<types.Package> pkg = default;
    @string name = default;

    (var path, name) = p.parseQualifiedName();
    @string pkgname = default;
    if (p.tok == scanner.ΔString) {
        pkgname = p.parseString();
    }
    pkg = p.getPkg(path, pkgname);
    if (pkg == nil) {
        p.errorf("package %s (path = %q) not found"u8, name, path);
    }
    return (pkg, name);
}

// Name = QualifiedName | "?" .
internal static @string parseName(this ref parser p) {
    if (p.tok == '?') {
        // Anonymous.
        p.next();
        return ""u8;
    }
    // The package path is redundant for us. Don't try to parse it.
    var (_, name) = p.parseUnquotedQualifiedName();
    return name;
}

internal static typesꓸType deref(typesꓸType typ) {
    {
        var (p, _) = typ._<ж<types.Pointer>>(ᐧ); if (p != nil) {
            typ = p.Elem();
        }
    }
    return typ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object embeddedFieldExpectedˢ = (@string)"embedded field expected"u8;

// Field = Name Type [string] .
internal static (ж<types.Var> field, @string tag) parseField(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    ж<types.Var> field = default;
    @string tag = default;
    ref var p = ref Ꮡp.DerefOrNull();

    @string name = p.parseName();
    var (typ, n) = Ꮡp.parseTypeExtended(pkg);
    var anon = false;
    if (name == ""u8) {
        anon = true;
        // Alias?
        {
            var (aname, ok) = p.aliases[n, ꟷ]; if (ok) {
                name = aname;
            } else {
                var switchᴛ1 = deref(typ);
                switch (switchᴛ1.type()) {
                case ж<types.Basic> typΔ1: {
                    name = typΔ1.Name();
                    break;
                }
                case ж<types.Named> typΔ1: {
                    name = typΔ1.Obj().Name();
                    break;
                }
                default: {
                    var typΔ1 = switchᴛ1;
                    p.error(embeddedFieldExpectedˢ);
                    break;
                }}
            }
        }
    }
    field = types.NewField(token.NoPos, pkg, name, typ, anon);
    if (p.tok == scanner.ΔString) {
        tag = p.parseString();
    }
    return (field, tag);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string retˢ = "$ret"u8;
internal static readonly @string escˢ = "esc"u8;

// Param = Name ["..."] Type .
internal static (ж<types.Var> param, bool isVariadic) parseParam(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> pkg) {
    ж<types.Var> param = default;
    bool isVariadic = default;
    ref var p = ref Ꮡp.DerefOrNull();

    @string name = p.parseName();
    // Ignore names invented for inlinable functions.
    if (strings.HasPrefix(name, "p."u8) || strings.HasPrefix(name, "r."u8) || strings.HasPrefix(name, retˢ)) {
        name = ""u8;
    }
    if (p.tok == '<' && p.scanner.Peek() == 'e') {
        // EscInfo = "<esc:" int ">" . (optional and ignored)
        p.next();
        p.expectKeyword(escˢ);
        p.expect(':');
        p.expect(scanner.Int);
        p.expect('>');
    }
    if (p.tok == '.') {
        p.next();
        p.expect('.');
        p.expect('.');
        isVariadic = true;
    }
    var typ = Ꮡp.parseType(pkg);
    if (isVariadic) {
        typ = new types.SliceжΔType(types.NewSlice(typ));
    }
    param = types.NewParam(token.NoPos, pkg, name, typ);
    param.SetKind(kind);
    return (param, isVariadic);
}

// Var = Name Type .
internal static ж<types.Var> parseVar(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    ref var p = ref Ꮡp.DerefOrNull();

    @string name = p.parseName();
    var v = types.NewVar(token.NoPos, pkg, name, Ꮡp.parseType(pkg)); // (types.PackageVar)
    if (name[0] == '.' || name[0] == '<') {
        // This is an unexported variable,
        // or a variable defined in a different package.
        // We only want to record exported variables.
        return default;
    }
    return v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string convertˢ = "convert"u8;

// Conversion = "convert" "(" Type "," ConstValue ")" .
internal static (constant.Value val, typesꓸType typ) parseConversion(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    constant.Value val = default;
    typesꓸType typ = default;
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(convertˢ);
    p.expect('(');
    typ = Ꮡp.parseType(pkg);
    p.expect(',');
    (val, _) = Ꮡp.parseConstValue(pkg);
    p.expect(')');
    return (val, typ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldNotParseIntegerˢ = (@string)"could not parse integer literal"u8;
internal static readonly object couldNotParseFloatˢ = (@string)"could not parse float literal"u8;
internal static readonly object couldNotParseRealˢ = (@string)"could not parse real component of complex literal"u8;
internal static readonly object couldNotParseImagˢ = (@string)"could not parse imag component of complex literal"u8;

// ConstValue     = string | "false" | "true" | ["-"] (int ["'"] | FloatOrComplex) | Conversion .
// FloatOrComplex = float ["i" | ("+"|"-") float "i"] .
internal static (constant.Value val, typesꓸType typ) parseConstValue(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    constant.Value val = default;
    typesꓸType typ = default;
    ref var p = ref Ꮡp.DerefOrNull();

    // v3 changed to $false, $true, $convert, to avoid confusion
    // with variable names in inline function bodies.
    if (p.tok == '$') {
        p.next();
        if (p.tok != scanner.Ident) {
            p.errorf("expected identifier after '$', got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        }
    }

    switch (p.tok) {
    case var ᴛ when ᴛ == scanner.ΔString: {
        @string str = p.parseString();
        val = constant.MakeString(str);
        typ = new types.BasicжΔType(types.Typ[types.UntypedString]);
        return (val, typ);
    }
    case var ᴛ when ᴛ == scanner.Ident: {
        var b = false;
        var exprᴛ2 = p.lit;
        switch (ᐧ) {
        case {} when exprᴛ2 == "false"u8: {
            break;
        }
        case {} when exprᴛ2 == "true"u8: {
            b = true;
            break;
        }
        case {} when exprᴛ2 == "convert"u8: {
            return Ꮡp.parseConversion(pkg);
        }
        default: {
            p.errorf("expected const value, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
            break;
        }}

        p.next();
        val = constant.MakeBool(b);
        typ = new types.BasicжΔType(types.Typ[types.UntypedBool]);
        return (val, typ);
    }}

    @string sign = ""u8;
    if (p.tok == '-') {
        p.next();
        sign = "-"u8;
    }

    switch (p.tok) {
    case var ᴛ when ᴛ == scanner.Int: {
        val = constant.MakeFromLiteral(sign + p.lit, token.INT, 0);
        if (val == nil) {
            p.error(couldNotParseIntegerˢ);
        }

        p.next();
        if (p.tok == '\'') {
            p.next();
            typ = new types.BasicжΔType(types.Typ[types.UntypedRune]);
        } else {
            typ = new types.BasicжΔType(types.Typ[types.ΔUntypedInt]);
        }
        break;
    }
    case var ᴛ when ᴛ == scanner.Float: {
        @string re = sign + p.lit;
        p.next();

        @string im = default;
        switch (p.tok) {
        case '+': {
            p.next();
            im = p.expect(scanner.Float);
            break;
        }
        case '-': {
            p.next();
            im = "-"u8 + p.expect(scanner.Float);
            break;
        }
        case var ᴛ2 when ᴛ2 == scanner.Ident: {
            // re is in fact the imaginary component. Expect "i" below.
            im = re;
            re = "0"u8;
            break;
        }
        default: {
            val = constant.MakeFromLiteral(re, token.FLOAT, 0);
            if (val == nil) {
                p.error(couldNotParseFloatˢ);
            }
            typ = new types.BasicжΔType(types.Typ[types.ΔUntypedFloat]);
            return (val, typ);
        }}

        p.expectKeyword("i"u8);
        var reval = constant.MakeFromLiteral(re, token.FLOAT, 0);
        if (reval == nil) {
            p.error(couldNotParseRealˢ);
        }
        var imval = constant.MakeFromLiteral(im + "i"u8, token.IMAG, 0);
        if (imval == nil) {
            p.error(couldNotParseImagˢ);
        }
        val = constant.BinaryOp(reval, token.ADD, imval);
        typ = new types.BasicжΔType(types.Typ[types.ΔUntypedComplex]);
        break;
    }
    default: {
        p.errorf("expected const value, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        break;
    }}

    return (val, typ);
}

// Const = Name [Type] "=" ConstValue .
internal static ж<types.Const> parseConst(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    ref var p = ref Ꮡp.DerefOrNull();

    @string name = p.parseName();
    typesꓸType typ = default;
    if (p.tok == '<') {
        typ = Ꮡp.parseType(pkg);
    }
    p.expect('=');
    var (val, vtyp) = Ꮡp.parseConstValue(pkg);
    if (typ == nil) {
        typ = vtyp;
    }
    return types.NewConst(token.NoPos, pkg, name, typ, val);
}

// reserved is a singleton type used to fill type map slots that have
// been reserved (i.e., for which a type number has been parsed) but
// which don't have their actual type yet. When the type map is updated,
// the actual type must replace a reserved entry (or we have an internal
// error). Used for self-verification only - not required for correctness.
partial struct reservedᴛ1 /*dyn*/ {
    /*embed*/ public global::go.go.types_package.ΔType Type;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();

// reserve reserves the type map entry n for future use.
internal static void reserve(this ref parser p, nint n) {
    // Notes:
    // - for pre-V3 export data, the type numbers we see are
    //   guaranteed to be in increasing order, so we append a
    //   reserved entry onto the list.
    // - for V3+ export data, type numbers can appear in
    //   any order, however the 'types' section tells us the
    //   total number of types, hence typeList is pre-allocated.
    if (len(p.typeData) == 0) {
        if (n != len(p.typeList)) {
            p.errorf("invalid type number %d (out of sync)"u8, n);
        }
        p.typeList = append(p.typeList, (typesꓸType)(new reservedᴛ1жΔType(reserved)));
    } else {
        if (p.typeList[n] != nil) {
            p.errorf("previously visited type number %d"u8, n);
        }
        p.typeList[n] = new reservedᴛ1жΔType(reserved);
    }
}

// update sets the type map entries for the entries in nlist to t.
// An entry in nlist can be a type number in p.typeList,
// used to resolve named types, or it can be a *types.Pointer,
// used to resolve pointers to named types in case they are referenced
// by embedded fields.
internal static void update(this ref parser p, typesꓸType t, slice<any> nlist) {
    if (AreEqual(t, reserved)) {
        p.errorf("internal error: update(%v) invoked on reserved"u8, (any)(nlist));
    }
    if (t == nil) {
        p.errorf("internal error: update(%v) invoked on nil"u8, (any)(nlist));
    }
    foreach (var (_, n) in nlist) {
        switch (n.type()) {
        case nint nΔ1: {
            if (AreEqual(p.typeList[nΔ1], t)) {
                continue;
            }
            if (!AreEqual(p.typeList[nΔ1], reserved)) {
                p.errorf("internal error: update(%v): %d not reserved"u8, nlist, nΔ1);
            }
            p.typeList[nΔ1] = t;
            break;
        }
        case ж<types.Pointer> nΔ1: {
            if (nΔ1.Value != (new types.Pointer())) {
                var elem = nΔ1.Elem();
                if (AreEqual(elem, t)) {
                    continue;
                }
                p.errorf("internal error: update: pointer already set to %v, expected %v"u8, elem, t);
            }
            nΔ1.Value = types.NewPointer(t).Value;
            break;
        }
        default: {
            var nΔ1 = n;
            p.errorf("internal error: %T on nlist"u8, nΔ1);
            break;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedUnderlyingTypeˢ = (@string)"unexpected underlying type for non-named TypeName"u8;
internal static readonly @string funcˢ = "func"u8;

// NamedType = TypeName [ "=" ] Type { Method } .
// TypeName  = ExportedName .
// Method    = "func" "(" Param ")" Name ParamList ResultList [InlineBody] ";" .
internal static typesꓸType parseNamedType(this ж<parser> Ꮡp, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    var (pkg, name) = p.parseExportedName();
    var scope = pkg.Scope();
    var obj = scope.Lookup(name);
    if (obj != nil && obj.Type() == nil) {
        p.errorf("%v has nil type"u8, obj);
    }

    if (p.tok == scanner.Ident && p.lit == "notinheap"u8) {
        p.next();
        // The go/types package has no way of recording that
        // this type is marked notinheap. Presumably no user
        // of this package actually cares.
    }

    // type alias
    if (p.tok == '=') {
        p.next();
        p.aliases[nlist[len(nlist) - 1]._<nint>()] = name;
        if (obj != nil) {
            // use the previously imported (canonical) type
            var tΔ1 = obj.Type();
            p.update(tΔ1, nlist);
            Ꮡp.parseType(pkg); // discard
            return tΔ1;
        }
        var tΔ2 = Ꮡp.parseType(pkg, nlist.ꓸꓸꓸ);
        obj = new types.TypeNameжObject(types.NewTypeName(token.NoPos, pkg, name, tΔ2));
        scope.Insert(obj);
        return tΔ2;
    }

    // defined type
    if (obj == nil) {
        // A named type may be referred to before the underlying type
        // is known - set it up.
        var tname = types.NewTypeName(token.NoPos, pkg, name, default);
        types.NewNamed(tname, default, default);
        scope.Insert(new types.TypeNameжObject(tname));
        obj = new types.TypeNameжObject(tname);
    }

    // use the previously imported (canonical), or newly created type
    var t = obj.Type();
    p.update(t, nlist);

    var (nt, ok) = t._<ж<types.Named>>(ᐧ);
    if (!ok) {
        // This can happen for unsafe.Pointer, which is a TypeName holding a Basic type.
        var pt = Ꮡp.parseType(pkg);
        if (!AreEqual(pt, t)) {
            p.error(unexpectedUnderlyingTypeˢ);
        }
        return t;
    }

    var underlying = Ꮡp.parseType(pkg);
    if (nt.Underlying() == nil) {
        if (underlying.Underlying() == nil) {
            var fix = new fixupRecord(toUpdate: nt, target: underlying);
            p.fixups = append(p.fixups, fix);
        } else {
            nt.SetUnderlying(underlying.Underlying());
        }
    }

    if (p.tok == '\n') {
        p.next();
        // collect associated methods
        while (p.tok == scanner.Ident) {
            p.expectKeyword(funcˢ);
            if (p.tok == '/') {
                // Skip a /*nointerface*/ or /*asm ID */ comment.
                p.expect('/');
                p.expect('*');
                if (p.expect(scanner.Ident) == "asm"u8) {
                    p.parseUnquotedString();
                }
                p.expect('*');
                p.expect('/');
            }
            p.expect('(');
            var (receiver, _) = Ꮡp.parseParam(types.RecvVar, pkg);
            p.expect(')');
            @string nameΔ1 = p.parseName();
            var (@params, isVariadic) = Ꮡp.parseParamList(types.ParamVar, pkg);
            var results = Ꮡp.parseResultList(pkg);
            Ꮡp.skipInlineBody();
            p.expectEOL();

            var sig = types.NewSignatureType(receiver, default, default, @params, results, isVariadic);
            nt.AddMethod(types.NewFunc(token.NoPos, pkg, nameΔ1, sig));
        }
    }

    return new types.NamedжΔType(nt);
}

internal static int64 parseInt64(this ref parser p) {
    @string lit = p.expect(scanner.Int);
    var (n, err) = strconv.ParseInt(lit, 10, 64);
    if (err != nil) {
        p.error(err);
    }
    return n;
}

internal static nint parseInt(this ref parser p) {
    @string lit = p.expect(scanner.Int);
    var (n, err) = strconv.ParseInt(lit, 10, 0 /* int */);
    if (err != nil) {
        p.error(err);
    }
    return (nint)n;
}

// ArrayOrSliceType = "[" [ int ] "]" Type .
internal static typesꓸType parseArrayOrSliceType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expect('[');
    if (p.tok == ']') {
        p.next();

        var tΔ1 = @new<types.Slice>();
        p.update(new types.SliceжΔType(tΔ1), nlist);

        tΔ1.Value = types.NewSlice(Ꮡp.parseType(pkg)).Value;
        return new types.SliceжΔType(tΔ1);
    }

    var t = @new<types.Array>();
    p.update(new types.ArrayжΔType(t), nlist);

    var len = p.parseInt64();
    p.expect(']');

    t.Value = types.NewArray(Ꮡp.parseType(pkg), len).Value;
    return new types.ArrayжΔType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapˢ = "map"u8;

// MapType = "map" "[" Type "]" Type .
internal static typesꓸType parseMapType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(mapˢ);

    var t = @new<types.Map>();
    p.update(new types.MapжΔType(t), nlist);

    p.expect('[');
    var key = Ꮡp.parseType(pkg);
    p.expect(']');
    var elem = Ꮡp.parseType(pkg);

    t.Value = types.NewMap(key, elem).Value;
    return new types.MapжΔType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chanˢ = "chan"u8;

// ChanType = "chan" ["<-" | "-<"] Type .
internal static typesꓸType parseChanType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(chanˢ);

    var t = @new<types.Chan>();
    p.update(new types.ChanжΔType(t), nlist);

    types.ChanDir dir = types.SendRecv;
    switch (p.tok) {
    case '-': {
        p.next();
        p.expect('<');
        dir = types.SendOnly;
        break;
    }
    case '<': {
        // don't consume '<' if it belongs to Type
        if (p.scanner.Peek() == '-') {
            p.next();
            p.expect('-');
            dir = types.RecvOnly;
        }
        break;
    }}

    t.Value = types.NewChan(dir, Ꮡp.parseType(pkg)).Value;
    return new types.ChanжΔType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string structˢ = "struct"u8;

// StructType = "struct" "{" { Field } "}" .
internal static typesꓸType parseStructType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(structˢ);

    var t = @new<types.Struct>();
    p.update(new types.StructжΔType(t), nlist);

    slice<ж<types.Var>> fields = default;
    slice<@string> tags = default;

    p.expect('{');
    while (p.tok != '}' && p.tok != scanner.EOF) {
        var (field, tag) = Ꮡp.parseField(pkg);
        p.expect(';');
        fields = append(fields, field);
        tags = append(tags, tag);
    }
    p.expect('}');

    t.Value = types.NewStruct(fields, tags).Value;
    return new types.StructжΔType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notOnFinalArgumentˢ = (@string)"... not on final argument"u8;

// ParamList = "(" [ { Parameter "," } Parameter ] ")" .
internal static (ж<types.Tuple>, bool) parseParamList(this ж<parser> Ꮡp, types.VarKind kind, ж<types.Package> pkg) {
    ref var p = ref Ꮡp.DerefOrNull();

    slice<ж<types.Var>> list = default;
    var isVariadic = false;

    p.expect('(');
    while (p.tok != ')' && p.tok != scanner.EOF) {
        if (len(list) > 0) {
            p.expect(',');
        }
        var (par, variadic) = Ꮡp.parseParam(kind, pkg);
        list = append(list, par);
        if (variadic) {
            if (isVariadic) {
                p.error(notOnFinalArgumentˢ);
            }
            isVariadic = true;
        }
    }
    p.expect(')');

    return (types.NewTuple(list.ꓸꓸꓸ), isVariadic);
}

// ResultList = Type | ParamList .
internal static ж<types.Tuple> parseResultList(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (p.tok) {
    case '<': {
        p.next();
        if (p.tok == scanner.Ident && p.lit == "inl"u8) {
            return default;
        }
        var (taa, _) = Ꮡp.parseTypeAfterAngle(pkg);
        var param = types.NewParam(token.NoPos, pkg, ""u8, taa);
        param.SetKind(types.ResultVar);
        return types.NewTuple(param);
    }
    case '(': {
        var (@params, _) = Ꮡp.parseParamList(types.ResultVar, pkg);
        return @params;
    }
    default: {
        return default;
    }}
}

// FunctionType = ParamList ResultList .
internal static ж<typesꓸSignature> parseFunctionType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    var t = @new<typesꓸSignature>();
    p.update(new types.ΔSignatureжΔType(t), nlist);

    var (@params, isVariadic) = Ꮡp.parseParamList(types.ParamVar, pkg);
    var results = Ꮡp.parseResultList(pkg);

    t.Value = types.NewSignatureType(nil, default, default, @params, results, isVariadic).Value;
    return t;
}

// Func = Name FunctionType [InlineBody] .
internal static ж<types.Func> parseFunc(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.tok == '/') {
        // Skip an /*asm ID */ comment.
        p.expect('/');
        p.expect('*');
        if (p.expect(scanner.Ident) == "asm"u8) {
            p.parseUnquotedString();
        }
        p.expect('*');
        p.expect('/');
    }

    @string name = p.parseName();
    var f = types.NewFunc(token.NoPos, pkg, name, Ꮡp.parseFunctionType(pkg, default));
    Ꮡp.skipInlineBody();

    if (name[0] == '.' || name[0] == '<' || strings.ContainsRune(name, '$')) {
        // This is an unexported function,
        // or a function defined in a different package,
        // or a type$equal or type$hash function.
        // We only want to record exported functions.
        return default;
    }

    return f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string interfaceˢ = "interface"u8;

// InterfaceType = "interface" "{" { ("?" Type | Func) ";" } "}" .
internal static typesꓸType parseInterfaceType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(interfaceˢ);

    var t = @new<types.Interface>();
    p.update(new types.InterfaceжΔType(t), nlist);

    slice<ж<types.Func>> methods = default;
    slice<typesꓸType> embeddeds = default;

    p.expect('{');
    while (p.tok != '}' && p.tok != scanner.EOF) {
        if (p.tok == '?') {
            p.next();
            embeddeds = append(embeddeds, Ꮡp.parseType(pkg));
        } else {
            var method = Ꮡp.parseFunc(pkg);
            if (method != nil) {
                methods = append(methods, method);
            }
        }
        p.expect(';');
    }
    p.expect('}');

    t.Value = types.NewInterfaceType(methods, embeddeds).Value;
    return new types.InterfaceжΔType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string anyˢ = "any"u8;

// PointerType = "*" ("any" | Type) .
internal static typesꓸType parsePointerType(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.expect('*');
    if (p.tok == scanner.Ident) {
        p.expectKeyword(anyˢ);
        var tΔ1 = types.Typ[types.UnsafePointer];
        p.update(new types.BasicжΔType(tΔ1), nlist);
        return new types.BasicжΔType(tΔ1);
    }

    var t = @new<types.Pointer>();
    p.update(new types.PointerжΔType(t), nlist);

    t.Value = types.NewPointer(Ꮡp.parseType(pkg, t.OrTypedNil())).Value;

    return new types.PointerжΔType(t);
}

// TypeSpec = NamedType | MapType | ChanType | StructType | InterfaceType | PointerType | ArrayOrSliceType | FunctionType .
internal static typesꓸType parseTypeSpec(this ж<parser> Ꮡp, ж<types.Package> pkg, slice<any> nlist) {
    ref var p = ref Ꮡp.DerefOrNull();

    switch (p.tok) {
    case var ᴛ when ᴛ == scanner.ΔString: {
        return Ꮡp.parseNamedType(nlist);
    }
    case var ᴛ when ᴛ == scanner.Ident: {
        var exprᴛ2 = p.lit;
        switch (ᐧ) {
        case {} when exprᴛ2 == "map"u8: {
            return Ꮡp.parseMapType(pkg, nlist);
        }
        case {} when exprᴛ2 == "chan"u8: {
            return Ꮡp.parseChanType(pkg, nlist);
        }
        case {} when exprᴛ2 == "struct"u8: {
            return Ꮡp.parseStructType(pkg, nlist);
        }
        case {} when exprᴛ2 == "interface"u8: {
            return Ꮡp.parseInterfaceType(pkg, nlist);
        }}
        break;
    }
    case '*': {
        return Ꮡp.parsePointerType(pkg, nlist);
    }
    case '[': {
        return Ꮡp.parseArrayOrSliceType(pkg, nlist);
    }
    case '(': {
        return new types.ΔSignatureжΔType(Ꮡp.parseFunctionType(pkg, nlist));
    }}

    p.errorf("expected type name or literal, got %s"u8, scanner.TokenString(p.tok));
    return default;
}

// From gofrontend/go/export.h
// Note that these values are negative in the gofrontend and have been made positive
// in the gccgoimporter.
internal static UntypedInt gccgoBuiltinINT8 => 1;
internal static UntypedInt gccgoBuiltinINT16 => 2;
internal static UntypedInt gccgoBuiltinINT32 => 3;
internal static UntypedInt gccgoBuiltinINT64 => 4;
internal static UntypedInt gccgoBuiltinUINT8 => 5;
internal static UntypedInt gccgoBuiltinUINT16 => 6;
internal static UntypedInt gccgoBuiltinUINT32 => 7;
internal static UntypedInt gccgoBuiltinUINT64 => 8;
internal static UntypedInt gccgoBuiltinFLOAT32 => 9;
internal static UntypedInt gccgoBuiltinFLOAT64 => 10;
internal static UntypedInt gccgoBuiltinINT => 11;
internal static UntypedInt gccgoBuiltinUINT => 12;
internal static UntypedInt gccgoBuiltinUINTPTR => 13;
internal static UntypedInt gccgoBuiltinBOOL => 15;
internal static UntypedInt gccgoBuiltinSTRING => 16;
internal static UntypedInt gccgoBuiltinCOMPLEX64 => 17;
internal static UntypedInt gccgoBuiltinCOMPLEX128 => 18;
internal static UntypedInt gccgoBuiltinERROR => 19;
internal static UntypedInt gccgoBuiltinBYTE => 20;
internal static UntypedInt gccgoBuiltinRUNE => 21;
internal static UntypedInt gccgoBuiltinANY => 22;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "error"u8;
internal static readonly @string byteˢ = "byte"u8;
internal static readonly @string runeˢ = "rune"u8;

internal static typesꓸType lookupBuiltinType(nint typ) {
    return new golib.SparseArray<typesꓸType>{[gccgoBuiltinINT8] = new types.BasicжΔType(types.Typ[types.Int8]), [gccgoBuiltinINT16] = new types.BasicжΔType(types.Typ[types.Int16]), [gccgoBuiltinINT32] = new types.BasicжΔType(types.Typ[types.Int32]), [gccgoBuiltinINT64] = new types.BasicжΔType(types.Typ[types.Int64]), [gccgoBuiltinUINT8] = new types.BasicжΔType(types.Typ[types.Uint8]), [gccgoBuiltinUINT16] = new types.BasicжΔType(types.Typ[types.Uint16]), [gccgoBuiltinUINT32] = new types.BasicжΔType(types.Typ[types.Uint32]), [gccgoBuiltinUINT64] = new types.BasicжΔType(types.Typ[types.Uint64]), [gccgoBuiltinFLOAT32] = new types.BasicжΔType(types.Typ[types.Float32]), [gccgoBuiltinFLOAT64] = new types.BasicжΔType(types.Typ[types.Float64]), [gccgoBuiltinINT] = new types.BasicжΔType(types.Typ[types.Int]), [gccgoBuiltinUINT] = new types.BasicжΔType(types.Typ[types.Uint]), [gccgoBuiltinUINTPTR] = new types.BasicжΔType(types.Typ[types.Uintptr]), [gccgoBuiltinBOOL] = new types.BasicжΔType(types.Typ[types.Bool]), [gccgoBuiltinSTRING] = new types.BasicжΔType(types.Typ[types.ΔString]), [gccgoBuiltinCOMPLEX64] = new types.BasicжΔType(types.Typ[types.Complex64]), [gccgoBuiltinCOMPLEX128] = new types.BasicжΔType(types.Typ[types.Complex128]), [gccgoBuiltinERROR] = types.Universe.Lookup(errorˢ).Type(), [gccgoBuiltinBYTE] = types.Universe.Lookup(byteˢ).Type(), [gccgoBuiltinRUNE] = types.Universe.Lookup(runeˢ).Type(), [gccgoBuiltinANY] = types.Universe.Lookup(anyˢ).Type()
    }.array(23)[typ];
}

// Type = "<" "type" ( "-" int | int [ TypeSpec ] ) ">" .
//
// parseType updates the type map to t for all type numbers n.
internal static typesꓸType parseType(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) {
    var n = nʗp.sslice();
    ref var p = ref Ꮡp.DerefOrNull();

    p.expect('<');
    var (t, _) = Ꮡp.parseTypeAfterAngle(pkg, n.ꓸꓸꓸ);
    return t;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeˢ = "type"u8;

// (*parser).Type after reading the "<".
internal static (typesꓸType t, nint n1) parseTypeAfterAngle(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) {
    typesꓸType t = default;
    nint n1 = default;
    var n = nʗp.slice();
    ref var p = ref Ꮡp.DerefOrNull();

    p.expectKeyword(typeˢ);

    n1 = 0;
    switch (p.tok) {
    case var ᴛ when ᴛ == scanner.Int: {
        n1 = p.parseInt();
        if (p.tok == '>') {
            if (len(p.typeData) > 0 && p.typeList[n1] == nil) {
                Ꮡp.parseSavedType(pkg, n1, n);
            }
            t = p.typeList[n1];
            if (len(p.typeData) == 0 && AreEqual(t, reserved)) {
                p.errorf("invalid type cycle, type %d not yet defined (nlist=%v)"u8, n1, n);
            }
            p.update(t, n);
        } else {
            p.reserve(n1);
            t = Ꮡp.parseTypeSpec(pkg, append(n, (any)(n1)));
        }
        break;
    }
    case '-': {
        p.next();
        nint n1Δ2 = p.parseInt();
        t = lookupBuiltinType(n1Δ2);
        p.update(t, n);
        break;
    }
    default: {
        p.errorf("expected type number, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        return (default, 0);
    }}

    if (t == nil || AreEqual(t, reserved)) {
        p.errorf("internal error: bad return from parseType(%v)"u8, (any)(n));
    }

    p.expect('>');
    return (t, n1);
}

// parseTypeExtended is identical to parseType, but if the type in
// question is a saved type, returns the index as well as the type
// pointer (index returned is zero if we parsed a builtin).
internal static (typesꓸType t, nint n1) parseTypeExtended(this ж<parser> Ꮡp, ж<types.Package> pkg, params ꓸꓸꓸany nʗp) {
    typesꓸType t = default;
    nint n1 = default;
    var n = nʗp.sslice();
    ref var p = ref Ꮡp.DerefOrNull();

    p.expect('<');
    (t, n1) = Ꮡp.parseTypeAfterAngle(pkg, n.ꓸꓸꓸ);
    return (t, n1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inlˢ = "inl"u8;

// InlineBody = "<inl:NN>" .{NN}
// Reports whether a body was skipped.
internal static void skipInlineBody(this ж<parser> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        // We may or may not have seen the '<' already, depending on
        // whether the function had a result type or not.
        if (p.tok == '<') {
            p.next();
            p.expectKeyword(inlˢ);
        } else if (p.tok != scanner.Ident || p.lit != "inl"u8) {
            return;
        } else {
            p.next();
        }

        p.expect(':');
        nint want = p.parseInt();
        p.expect('>');

        defer((uint64 w) => {
            Ꮡp.Value.scanner.Whitespace = w;
        }, Ꮡp.Value.scanner.Whitespace, ref ᒐ);
        p.scanner.Whitespace = 0;

        nint got = 0;
        while (got < want) {
            var r = p.scanner.Next();
            if (r == scanner.EOF) {
                p.error(unexpectedEofˢ);
            }
            got += utf8.RuneLen(r);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct parseTypes_typeOffset /*dyn*/ {
    internal nint offset;
    internal nint length;
}

// Types = "types" maxp1 exportedp1 (offset length)* .
internal static void parseTypes(this ж<parser> Ꮡp, ж<types.Package> pkg) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();
        var typeOffsetsᴷ = new stackAppend<parseTypes_typeOffset>(moves: true);

        nint maxp1 = p.parseInt();
        nint exportedp1 = p.parseInt();
        p.typeList = new slice<typesꓸType>(maxp1, maxp1);
        slice<parseTypes_typeOffset> typeOffsets = default;

        nint total = 0;
        for (nint i = 1; i < maxp1; i++) {
            nint len = p.parseInt();
            typeOffsets = append(typeOffsetsᴷ, typeOffsets, new parseTypes_typeOffset(total, len));
            total += len;
        }

        defer((uint64 w) => {
            Ꮡp.Value.scanner.Whitespace = w;
        }, Ꮡp.Value.scanner.Whitespace, ref ᒐ);
        p.scanner.Whitespace = 0;

        // We should now have p.tok pointing to the final newline.
        // The next runes from the scanner should be the type data.

        ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
        while (sb.Len() < total) {
            var r = p.scanner.Next();
            if (r == scanner.EOF) {
                p.error(unexpectedEofˢ);
            }
            Ꮡsb.WriteRune(r);
        }
        @string allTypeData = sb.String();

        p.typeData = slice<@string>([""u8]); // type 0, unused
        typeOffsets = move2heap(typeOffsetsᴷ, typeOffsets);
        foreach (var (_, to) in typeOffsets) {
            p.typeData = append(p.typeData, allTypeData.slice(to.offset, to.offset + to.length));
        }

        for (nint i = 1; i < exportedp1; i++) {
            Ꮡp.parseSavedType(pkg, i, default);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// parseSavedType parses one saved type definition.
internal static void parseSavedType(this ж<parser> Ꮡp, ж<types.Package> pkg, nint i, slice<any> nlist) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer((ж<scanner.Scanner> s, rune tok, @string lit) => {
            Ꮡp.Value.scanner = s;
            Ꮡp.Value.tok = tok;
            Ꮡp.Value.lit = lit;
        }, Ꮡp.Value.scanner, Ꮡp.Value.tok, Ꮡp.Value.lit, ref ᒐ);

        p.scanner = @new<scanner.Scanner>();
        Ꮡp.initScanner((~p.scanner).Filename, new strings_ReaderжReader(strings.NewReader(p.typeData[i])));
        p.expectKeyword(typeˢ);
        nint id = p.parseInt();
        if (id != i) {
            p.errorf("type ID mismatch: got %d, want %d"u8, id, i);
        }
        if (AreEqual(p.typeList[i], reserved)) {
            p.errorf("internal error: %d already reserved in parseSavedType"u8, i);
        }
        if (p.typeList[i] == nil) {
            p.reserve(i);
            Ꮡp.parseTypeSpec(pkg, append(nlist, (any)(i)));
        }
        if (p.typeList[i] == nil || AreEqual(p.typeList[i], reserved)) {
            p.errorf("internal error: parseSavedType(%d,%v) reserved/nil"u8, i, nlist);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// PackageInit = unquotedString unquotedString int .
internal static PackageInit parsePackageInit(this ref parser p) {
    @string name = p.parseUnquotedString();
    @string initfunc = p.parseUnquotedString();
    nint priority = -1;
    if (p.version == "v1"u8) {
        priority = p.parseInt();
    }
    return new PackageInit(Name: name, InitFunc: initfunc, Priority: priority);
}

// Create the package if we have parsed both the package path and package name.
internal static void maybeCreatePackage(this ref parser p) {
    if (p.pkgname != ""u8 && p.pkgpath != ""u8) {
        p.pkg = p.getPkg(p.pkgpath, p.pkgname);
    }
}

// InitDataDirective = ( "v1" | "v2" | "v3" ) ";" |
//
//	"priority" int ";" |
//	"init" { PackageInit } ";" |
//	"checksum" unquotedString ";" .
internal static void parseInitDataDirective(this ж<parser> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        if (p.tok != scanner.Ident) {
            // unexpected token kind; panic
            p.expect(scanner.Ident);
        }

        var exprᴛ1 = p.lit;
        switch (ᐧ) {
        case {} when exprᴛ1 == "v1"u8 || exprᴛ1 == "v2"u8 || exprᴛ1 == "v3"u8: {
            p.version = p.lit;
            p.next();
            p.expect(';');
            p.expect('\n');
            break;
        }
        case {} when exprᴛ1 == "priority"u8: {
            p.next();
            p.initdata.Priority = p.parseInt();
            p.expectEOL();
            break;
        }
        case {} when exprᴛ1 == "init"u8: {
            p.next();
            while (p.tok != '\n' && p.tok != ';' && p.tok != scanner.EOF) {
                p.initdata.Inits = append(p.initdata.Inits, p.parsePackageInit());
            }
            p.expectEOL();
            break;
        }
        case {} when exprᴛ1 == "init_graph"u8: {
            p.next();
            // The graph data is thrown away for now.
            while (p.tok != '\n' && p.tok != ';' && p.tok != scanner.EOF) {
                p.parseInt64();
                p.parseInt64();
            }
            p.expectEOL();
            break;
        }
        case {} when exprᴛ1 == "checksum"u8: {
            // Don't let the scanner try to parse the checksum as a number.
            defer((nuint mode) => {
                Ꮡp.Value.scanner.Mode = mode;
            }, Ꮡp.Value.scanner.Mode, ref ᒐ);
            p.scanner.Mode &= unchecked((nuint)~(nuint)((nuint)((nuint)scanner.ScanInts | (nuint)scanner.ScanFloats)));
            p.next();
            p.parseUnquotedString();
            p.expectEOL();
            break;
        }
        default: {
            p.errorf("unexpected identifier: %q"u8, p.lit);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Directive = InitDataDirective |
//
//	"package" unquotedString [ unquotedString ] [ unquotedString ] ";" |
//	"pkgpath" unquotedString ";" |
//	"prefix" unquotedString ";" |
//	"import" unquotedString unquotedString string ";" |
//	"indirectimport" unquotedString unquotedstring ";" |
//	"func" Func ";" |
//	"type" Type ";" |
//	"var" Var ";" |
//	"const" Const ";" .
internal static void parseDirective(this ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.tok != scanner.Ident) {
        // unexpected token kind; panic
        p.expect(scanner.Ident);
    }

    var exprᴛ1 = p.lit;
    switch (ᐧ) {
    case {} when exprᴛ1 == "v1"u8 || exprᴛ1 == "v2"u8 || exprᴛ1 == "v3"u8 || exprᴛ1 == "priority"u8 || exprᴛ1 == "init"u8 || exprᴛ1 == "init_graph"u8 || exprᴛ1 == "checksum"u8: {
        Ꮡp.parseInitDataDirective();
        break;
    }
    case {} when exprᴛ1 == "package"u8: {
        p.next();
        p.pkgname = p.parseUnquotedString();
        p.maybeCreatePackage();
        if (p.version != "v1"u8 && p.tok != '\n' && p.tok != ';') {
            p.parseUnquotedString();
            p.parseUnquotedString();
        }
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "pkgpath"u8: {
        p.next();
        p.pkgpath = p.parseUnquotedString();
        p.maybeCreatePackage();
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "prefix"u8: {
        p.next();
        p.pkgpath = p.parseUnquotedString();
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "import"u8: {
        p.next();
        @string pkgname = p.parseUnquotedString();
        @string pkgpath = p.parseUnquotedString();
        p.getPkg(pkgpath, pkgname);
        p.parseString();
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "indirectimport"u8: {
        p.next();
        @string pkgname = p.parseUnquotedString();
        @string pkgpath = p.parseUnquotedString();
        p.getPkg(pkgpath, pkgname);
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "types"u8: {
        p.next();
        Ꮡp.parseTypes(p.pkg);
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "func"u8: {
        p.next();
        var fun = Ꮡp.parseFunc(p.pkg);
        if (fun != nil) {
            p.pkg.Scope().Insert(new types.FuncжObject(fun));
        }
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "type"u8: {
        p.next();
        Ꮡp.parseType(p.pkg);
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "var"u8: {
        p.next();
        var v = Ꮡp.parseVar(p.pkg);
        if (v != nil) {
            p.pkg.Scope().Insert(new types.VarжObject(v));
        }
        p.expectEOL();
        break;
    }
    case {} when exprᴛ1 == "const"u8: {
        p.next();
        var c = Ꮡp.parseConst(p.pkg);
        p.pkg.Scope().Insert(new types.ConstжObject(c));
        p.expectEOL();
        break;
    }
    default: {
        p.errorf("unexpected identifier: %q"u8, p.lit);
        break;
    }}
}

// Package = { Directive } .
internal static ж<types.Package> parsePackage(this ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    while (p.tok != scanner.EOF) {
        Ꮡp.parseDirective();
    }
    foreach (var (_, f) in p.fixups) {
        if (f.target.Underlying() == nil) {
            p.errorf("internal error: fixup can't be applied, loop required"u8);
        }
        f.toUpdate.SetUnderlying(f.target.Underlying());
    }
    p.fixups = default;
    foreach (var (_, typ) in p.typeList) {
        {
            var (it, ok) = typ._<ж<types.Interface>>(ᐧ); if (ok) {
                it.Complete();
            }
        }
    }
    p.pkg.MarkComplete();
    return p.pkg;
}

} // end gccgoimporter_package
