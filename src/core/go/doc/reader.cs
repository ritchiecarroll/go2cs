// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using cmp = cmp_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using lazyregexp = @internal.lazyregexp_package;
using path = path_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using @internal;
using global::go.go;
using global::go.unicode;

partial class doc_package {

partial struct methodSet /*map[@string, ж<Func>]*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badrecvˢ = "BADRECV"u8;

// ----------------------------------------------------------------------------
// function/method sets
//
// Internally, we treat functions like methods and collect them in method sets.

// recvString returns a string representation of recv of the form "T", "*T",
// "T[A, ...]", "*T[A, ...]" or "BADRECV" (if not a proper receiver type).
internal static @string recvString(ast.Expr recv) {
    switch (recv.type()) {
    case ж<ast.Ident> t: {
        return (~t).Name;
    }
    case ж<ast.StarExpr> t: {
        return "*"u8 + recvString((~t).X);
    }
    case ж<ast.IndexExpr> t: {
        return fmt.Sprintf("%s[%s]"u8, // Generic type with one parameter.
 recvString((~t).X), recvParam((~t).Index));
    }
    case ж<ast.IndexListExpr> t: {
        if (len((~t).Indices) > 0) {
            // Generic type with multiple parameters.
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            Ꮡb.WriteString(recvString((~t).X));
            Ꮡb.WriteByte((rune)'[');
            Ꮡb.WriteString(recvParam((~t).Indices[0]));
            foreach (var (_, e) in (~t).Indices[1..]) {
                Ꮡb.WriteString(", "u8);
                Ꮡb.WriteString(recvParam(e));
            }
            Ꮡb.WriteByte((rune)']');
            return b.String();
        }
        break;
    }}
    return badrecvˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badparamˢ = "BADPARAM"u8;

internal static @string recvParam(ast.Expr p) {
    {
        var (id, ok) = p._<ж<ast.Ident>>(ᐧ); if (ok) {
            return (~id).Name;
        }
    }
    return badparamˢ;
}

// set creates the corresponding Func for f and adds it to mset.
// If there are multiple f's with the same name, set keeps the first
// one with documentation; conflicts are ignored. The boolean
// specifies whether to leave the AST untouched.
internal static void set(this methodSet mset, ж<ast.FuncDecl> Ꮡf, bool preserveAST) {
    ref var f = ref Ꮡf.DerefOrNull();

    ref var name = ref heap<@string>(out var Ꮡname);
    name = f.Name.Value.Name;
    {
        var g = mset[name]; if (g != nil && (~g).Doc != ""u8) {
            // A function with the same name has already been registered;
            // since it has documentation, assume f is simply another
            // implementation and ignore it. This does not happen if the
            // caller is using go/build.ScanDir to determine the list of
            // files implementing a package.
            return;
        }
    }
    // function doesn't exist or has no documentation; use f
    ref var recv = ref heap<@string>(out var Ꮡrecv);
    recv = ""u8;
    if (f.Recv != nil) {
        ast.Expr typ = default!;
        // be careful in case of incorrect ASTs
        {
            var list = f.Recv.Value.List; if (len(list) == 1) {
                typ = list[0].Value.Type;
            }
        }
        recv = recvString(typ);
    }
    mset[name] = Ꮡ(new Func(
        Doc: f.Doc.Text(),
        Name: name,
        Decl: Ꮡf,
        Recv: recv,
        Orig: recv
    ));
    if (!preserveAST) {
        f.Doc = default!; // doc consumed - remove from AST
    }
}

// add adds method m to the method set; m is ignored if the method set
// already contains a method with the same name at the same or a higher
// level than m.
internal static void add(this methodSet mset, ж<Func> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    var old = mset[m.Name];
    if (old == nil || m.Level < (~old).Level) {
        mset[m.Name] = Ꮡm;
        return;
    }
    if (m.Level == (~old).Level) {
        // conflict - mark it using a method with nil Decl
        mset[m.Name] = Ꮡ(new Func(
            Name: m.Name,
            Level: m.Level
        ));
    }
}

// ----------------------------------------------------------------------------
// Named types

// baseTypeName returns the name of the base type of x (or "")
// and whether the type is imported or not.
internal static (@string name, bool imported) baseTypeName(ast.Expr x) {
    switch (x.type()) {
    case ж<ast.Ident> t: {
        return ((~t).Name, false);
    }
    case ж<ast.IndexExpr> t: {
        return baseTypeName((~t).X);
    }
    case ж<ast.IndexListExpr> t: {
        return baseTypeName((~t).X);
    }
    case ж<ast.SelectorExpr> t: {
        {
            var (_, ok) = (~t).X._<ж<ast.Ident>>(ᐧ); if (ok) {
                // only possible for qualified type names;
                // assume type is imported
                return ((~(~t).Sel).Name, true);
            }
        }
        break;
    }
    case ж<ast.ParenExpr> t: {
        return baseTypeName((~t).X);
    }
    case ж<ast.StarExpr> t: {
        return baseTypeName((~t).X);
    }}
    return ("", false);
}

partial struct embeddedSet /*map[ж<namedType>, bool]*/;

// A namedType represents a named unqualified (package local, or possibly
// predeclared) type. The namedType for a type name is always found via
// reader.lookupType.
partial struct namedType {
    internal @string doc;      // doc comment for type
    internal @string name;      // type name
    internal ж<ast.GenDecl> decl; // nil if declaration hasn't been seen yet
    internal bool isEmbedded;        // true if this type is embedded
    internal bool isStruct;        // true if this type is a struct
    internal embeddedSet embedded; // true if the embedded type is a pointer
    // associated declarations
    internal slice<ж<Value>> values; // consts and vars
    internal methodSet funcs;
    internal methodSet methods;
}

// ----------------------------------------------------------------------------
// AST reader

// reader accumulates documentation for a single package.
// It modifies the AST: Comments (declaration documentation)
// that have been collected by the reader are set to nil
// in the respective AST nodes so that they are not printed
// twice (once when printing the documentation and once when
// printing the corresponding AST node).
partial struct reader {
    internal Mode mode;
    // package properties
    internal @string doc; // package documentation, if any
    internal slice<@string> filenames;
    internal map<@string, slice<ж<Note>>> notes;
    // imports
    internal map<@string, nint> imports;
    internal bool hasDotImp; // if set, package contains a dot import
    internal map<@string, @string> importByName;
    // declarations
    internal slice<ж<Value>> values; // consts and vars
    internal nint order;     // sort order of const and var declarations (when we can't use a name)
    internal map<@string, ж<namedType>> types;
    internal methodSet funcs;
    // support for package-local shadowing of predeclared types
    internal map<@string, bool> shadowedPredecl;
    internal map<@string, slice<ж<ast.InterfaceType>>> fixmap;
}

internal static bool isVisible(this ref reader r, @string name) {
    return (Mode)(r.mode & AllDecls) != 0 || token.IsExported(name);
}

// lookupType returns the base type with the given name.
// If the base type has not been encountered yet, a new
// type with the given name but no associated declaration
// is added to the type map.
internal static ж<namedType> lookupType(this ref reader r, @string name) {
    if (name == ""u8 || name == "_"u8) {
        return default!; // no type docs for anonymous types
    }
    {
        var (typΔ1, found) = r.types[name, ꟷ]; if (found) {
            return typΔ1;
        }
    }
    // type not found - add one without declaration
    var typ = Ꮡ(new namedType(
        name: name,
        embedded: new embeddedSet(0),
        funcs: new methodSet(0),
        methods: new methodSet(0)
    ));
    r.types[name] = typ;
    return typ;
}

// recordAnonymousField registers fieldType as the type of an
// anonymous field in the parent type. If the field is imported
// (qualified name) or the parent is nil, the field is ignored.
// The function returns the field name.
internal static @string /*fname*/ recordAnonymousField(this ref reader r, ж<namedType> Ꮡparent, ast.Expr fieldType) {
    @string fname = default!;

    ref var parent = ref Ꮡparent.DerefOrNull();
    (fname, var imp) = baseTypeName(fieldType);
    if (Ꮡparent == nil || imp) {
        return fname;
    }
    {
        var ftype = r.lookupType(fname); if (ftype != nil) {
            ftype.Value.isEmbedded = true;
            var (_, ptr) = fieldType._<ж<ast.StarExpr>>(ᐧ);
            parent.embedded[ftype] = ptr;
        }
    }
    return fname;
}

internal static void readDoc(this ref reader r, ж<ast.CommentGroup> Ꮡcomment) {
    ref var comment = ref Ꮡcomment.DerefOrNull();

    // By convention there should be only one package comment
    // but collect all of them if there are more than one.
    @string text = Ꮡcomment.Text();
    if (r.doc == ""u8) {
        r.doc = text;
        return;
    }
    r.doc += "\n"u8 + text;
}

internal static void remember(this ref reader r, @string predecl, ж<ast.InterfaceType> Ꮡtyp) {
    if (r.fixmap == default!) {
        r.fixmap = new map<@string, slice<ж<ast.InterfaceType>>>();
    }
    r.fixmap[predecl] = append(r.fixmap[predecl], Ꮡtyp);
}

internal static slice<@string> specNames(slice<ast.Spec> specs) {
    var names = new slice<@string>(0, len(specs)); // reasonable estimate
    foreach (var (_, s) in specs) {
        // s guaranteed to be an *ast.ValueSpec by readValue
        foreach (var (_, ident) in (~s._<ж<ast.ValueSpec>>()).Names) {
            names = append(names, (~ident).Name);
        }
    }
    return names;
}

// readValue processes a const or var declaration.
internal static void readValue(this ж<reader> Ꮡr, ж<ast.GenDecl> Ꮡdecl) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var decl = ref Ꮡdecl.DerefOrNull();

    // determine if decl should be associated with a type
    // Heuristic: For each typed entry, determine the type name, if any.
    //            If there is exactly one type name that is sufficiently
    //            frequent, associate the decl with the respective type.
    @string domName = ""u8;
    nint domFreq = 0;
    @string prev = ""u8;
    nint n = 0;
    foreach (var (_, spec) in decl.Specs) {
        var (s, ok) = spec._<ж<ast.ValueSpec>>(ᐧ);
        if (!ok) {
            continue; // should not happen, but be conservative
        }
        @string name = ""u8;
        switch (ᐧ) {
        case {} when (~s).Type != default!: {
            {
                var (nΔ2, imp) = baseTypeName((~s).Type); if (!imp) {
                    // a type is present; determine its name
                    name = nΔ2;
                }
            }
            break;
        }
        case {} when decl.Tok == token.CONST && len((~s).Values) == 0: {
            name = prev;
            break;
        }}

        // no type or value is present but we have a constant declaration;
        // use the previous type name (possibly the empty string)
        if (name != ""u8) {
            // entry has a named type
            if (domName != ""u8 && domName != name) {
                // more than one type name - do not associate
                // with any type
                domName = ""u8;
                break;
            }
            domName = name;
            domFreq++;
        }
        prev = name;
        n++;
    }
    // nothing to do w/o a legal declaration
    if (n == 0) {
        return;
    }
    // determine values list with which to associate the Value for this decl
    var values = Ꮡr.of(reader.Ꮡvalues);
    const float64 threshold = 0.75;
    if (domName != ""u8 && r.isVisible(domName) && domFreq >= (nint)((float64)len(decl.Specs) * threshold)) {
        // typed entries are sufficiently frequent
        {
            var typ = r.lookupType(domName); if (typ != nil) {
                values = typ.of(namedType.Ꮡvalues); // associate with that type
            }
        }
    }
    values.ValueSlot = append(values.ValueSlot, Ꮡ(new Value(
        Doc: decl.Doc.Text(),
        Names: specNames(decl.Specs),
        Decl: Ꮡdecl,
        order: r.order
    )));
    if ((Mode)(r.mode & PreserveAST) == 0) {
        decl.Doc = default!; // doc consumed - remove from AST
    }
    // Note: It's important that the order used here is global because the cleanupTypes
    // methods may move values associated with types back into the global list. If the
    // order is list-specific, sorting is not deterministic because the same order value
    // may appear multiple times (was bug, found when fixing #16153).
    r.order++;
}

// fields returns a struct's fields or an interface's methods.
internal static (slice<ж<ast.Field>> list, bool isStruct) fields(ast.Expr typ) {
    slice<ж<ast.Field>> list = default!;
    bool isStruct = default!;

    ж<ast.FieldList> fields = default!;
    switch (typ.type()) {
    case ж<ast.StructType> t: {
        fields = t.Value.Fields;
        isStruct = true;
        break;
    }
    case ж<ast.InterfaceType> t: {
        fields = t.Value.Methods;
        break;
    }}
    if (fields != nil) {
        list = fields.Value.List;
    }
    return (list, isStruct);
}

// readType processes a type declaration.
internal static void readType(this ref reader r, ж<ast.GenDecl> Ꮡdecl, ж<ast.TypeSpec> Ꮡspec) {
    ref var decl = ref Ꮡdecl.DerefOrNull();
    ref var spec = ref Ꮡspec.DerefOrNull();

    var typ = r.lookupType((~spec.Name).Name);
    if (typ == nil) {
        return; // no name or blank name - ignore the type
    }
    // A type should be added at most once, so typ.decl
    // should be nil - if it is not, simply overwrite it.
    typ.Value.decl = Ꮡdecl;
    // compute documentation
    var doc = spec.Doc;
    if (doc == nil) {
        // no doc associated with the spec, use the declaration doc, if any
        doc = decl.Doc;
    }
    if ((Mode)(r.mode & PreserveAST) == 0) {
        spec.Doc = default!; // doc consumed - remove from AST
        decl.Doc = default!; // doc consumed - remove from AST
    }
    typ.Value.doc = doc.Text();
    // record anonymous fields (they may contribute methods)
    // (some fields may have been recorded already when filtering
    // exports, but that's ok)
    slice<ж<ast.Field>> list = default!;
    (list, typ.Value.isStruct) = fields(spec.Type);
    foreach (var (_, field) in list) {
        if (len((~field).Names) == 0) {
            r.recordAnonymousField(typ, (~field).Type);
        }
    }
}

// isPredeclared reports whether n denotes a predeclared type.
internal static bool isPredeclared(this ref reader r, @string n) {
    return predeclaredTypes[n] && r.types[n] == nil;
}

// readFunc processes a func or method declaration.
internal static void readFunc(this ref reader r, ж<ast.FuncDecl> Ꮡfun) {
    ref var fun = ref Ꮡfun.DerefOrNull();

    // strip function body if requested.
    if ((Mode)(r.mode & PreserveAST) == 0) {
        fun.Body = default!;
    }
    // associate methods with the receiver type, if any
    if (fun.Recv != nil) {
        // method
        if (len((~fun.Recv).List) == 0) {
            // should not happen (incorrect AST); (See issue 17788)
            // don't show this method
            return;
        }
        var (recvTypeName, imp) = baseTypeName((~(~fun.Recv).List[0]).Type);
        if (imp) {
            // should not happen (incorrect AST);
            // don't show this method
            return;
        }
        {
            var typ = r.lookupType(recvTypeName); if (typ != nil) {
                (~typ).methods.set(Ꮡfun, (Mode)(r.mode & PreserveAST) != 0);
            }
        }
        // otherwise ignore the method
        // TODO(gri): There may be exported methods of non-exported types
        // that can be called because of exported values (consts, vars, or
        // function results) of that type. Could determine if that is the
        // case and then show those methods in an appropriate section.
        return;
    }
    // Associate factory functions with the first visible result type, as long as
    // others are predeclared types.
    if ((~fun.Type).Results.NumFields() >= 1) {
        ж<namedType> typ = default!;                   // type to associate the function with
        nint numResultTypes = 0;
        foreach (var (_, res) in (~(~fun.Type).Results).List) {
            var factoryType = res.Value.Type;
            {
                var (t, ok) = factoryType._<ж<ast.ArrayType>>(ᐧ); if (ok) {
                    // We consider functions that return slices or arrays of type
                    // T (or pointers to T) as factory functions of T.
                    factoryType = t.Value.Elt;
                }
            }
            {
                var (n, imp) = baseTypeName(factoryType); if (!imp && r.isVisible(n) && !r.isPredeclared(n)) {
                    if (lookupTypeParam(n, (~fun.Type).TypeParams) != nil) {
                        // Issue #49477: don't associate fun with its type parameter result.
                        // A type parameter is not a defined type.
                        continue;
                    }
                    {
                        var t = r.lookupType(n); if (t != nil) {
                            typ = t;
                            numResultTypes++;
                            if (numResultTypes > 1) {
                                break;
                            }
                        }
                    }
                }
            }
        }
        // If there is exactly one result type,
        // associate the function with that type.
        if (numResultTypes == 1) {
            (~typ).funcs.set(Ꮡfun, (Mode)(r.mode & PreserveAST) != 0);
            return;
        }
    }
    // just an ordinary function
    r.funcs.set(Ꮡfun, (Mode)(r.mode & PreserveAST) != 0);
}

// lookupTypeParam searches for type parameters named name within the tparams
// field list, returning the relevant identifier if found, or nil if not.
internal static ж<ast.Ident> lookupTypeParam(@string name, ж<ast.FieldList> Ꮡtparams) {
    ref var tparams = ref Ꮡtparams.DerefOrNull();

    if (Ꮡtparams == nil) {
        return default!;
    }
    foreach (var (_, field) in tparams.List) {
        foreach (var (_, id) in (~field).Names) {
            if ((~id).Name == name) {
                return id;
            }
        }
    }
    return default!;
}

internal static @string noteMarker = @"([A-Z][A-Z]+)\(([^)]+)\):?"u8; // MARKER(uid), MARKER at least 2 chars, uid at least 1 char
internal static ж<lazyregexp.Regexp> noteMarkerRx = lazyregexp.New(@"^[ \t]*"u8 + noteMarker);          // MARKER(uid) at text start
internal static ж<lazyregexp.Regexp> noteCommentRx = lazyregexp.New(@"^/[/*][ \t]*"u8 + noteMarker);     // MARKER(uid) at comment start

// clean replaces each sequence of space, \r, or \t characters
// with a single space and removes any trailing and leading spaces.
internal static @string clean(@string s) {
    slice<byte> b = default!;
    var p = (byte)(rune)' ';
    for (nint i = 0; i < len(s); i++) {
        var q = s[i];
        if (q == (rune)'\r' || q == (rune)'\t') {
            q = (rune)' ';
        }
        if (q != (rune)' ' || p != (rune)' ') {
            b = append(b, q);
            p = q;
        }
    }
    // remove trailing blank, if any
    {
        nint n = len(b); if (n > 0 && p == (rune)' ') {
            b = b.slice(0, n - 1);
        }
    }
    return ((@string)b);
}

// readNote collects a single note from a sequence of comments.
internal static void readNote(this ref reader r, slice<ж<ast.Comment>> list) {
    @string text = (Ꮡ(new ast.CommentGroup(List: list))).Text();
    {
        var m = noteMarkerRx.FindStringSubmatchIndex(text); if (m != default!) {
            // The note body starts after the marker.
            // We remove any formatting so that we don't
            // get spurious line breaks/indentation when
            // showing the TODO body.
            ref var body = ref heap<@string>(out var Ꮡbody);
            body = clean(text.slice(m[1]));
            if (body != ""u8) {
                @string marker = text.slice(m[2], m[3]);
                r.notes[marker] = append(r.notes[marker], Ꮡ(new Note(
                    Pos: list[0].Pos(),
                    End: list[len(list) - 1].End(),
                    UID: text.slice(m[4], m[5]),
                    Body: body
                )));
            }
        }
    }
}

// readNotes extracts notes from comments.
// A note must start at the beginning of a comment with "MARKER(uid):"
// and is followed by the note body (e.g., "// BUG(gri): fix this").
// The note ends at the end of the comment group or at the start of
// another note in the same comment group, whichever comes first.
internal static void readNotes(this ref reader r, slice<ж<ast.CommentGroup>> comments) {
    foreach (var (_, group) in comments) {
        nint i = -1; // comment index of most recent note start, valid if >= 0
        var list = group.Value.List;
        foreach (var (j, c) in list) {
            if (noteCommentRx.MatchString((~c).Text)) {
                if (i >= 0) {
                    r.readNote(list.slice(i, j));
                }
                i = j;
            }
        }
        if (i >= 0) {
            r.readNote(list.slice(i));
        }
    }
}

// readFile adds the AST for a source file to the reader.
internal static void readFile(this ж<reader> Ꮡr, ж<ast.File> Ꮡsrc) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    // add package documentation
    if (src.Doc != nil) {
        r.readDoc(src.Doc);
        if ((Mode)(r.mode & PreserveAST) == 0) {
            src.Doc = default!; // doc consumed - remove from AST
        }
    }
    // add all declarations but for functions which are processed in a separate pass
    foreach (var (_, decl) in src.Decls) {
        switch (decl.type()) {
        case ж<ast.GenDecl> d: {
            var exprᴛ1 = (~d).Tok;
            if (exprᴛ1 == token.IMPORT) {
                foreach (var (_, spec) in (~d).Specs) {
                    // imports are handled individually
                    {
                        var (s, ok) = spec._<ж<ast.ImportSpec>>(ᐧ); if (ok) {
                            {
                                var (import_, err) = strconv.Unquote((~(~s).Path).Value); if (err == default!) {
                                    r.imports[import_] = 1;
                                    @string name = default!;
                                    if ((~s).Name != nil) {
                                        name = s.Value.Name.Value.Name;
                                        if (name == "."u8) {
                                            r.hasDotImp = true;
                                        }
                                    }
                                    if (name != "."u8) {
                                        if (name == ""u8) {
                                            name = assumedPackageName(import_);
                                        }
                                        var (old, okΔ1) = r.importByName[name, ꟷ];
                                        if (!okΔ1){
                                            r.importByName[name] = import_;
                                        } else 
                                        if (old != import_ && old != ""u8) {
                                            r.importByName[name] = ""u8; // ambiguous
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (exprᴛ1 == token.CONST || exprᴛ1 == token.VAR) {
                Ꮡr.readValue(d);
            }
            else if (exprᴛ1 == token.TYPE) {
                do {
                    if (len((~d).Specs) == 1 && !(~d).Lparen.IsValid()) {
                        // constants and variables are always handled as a group
                        // types are handled individually
                        // common case: single declaration w/o parentheses
                        // (if a single declaration is parenthesized,
                        // create a new fake declaration below, so that
                        // go/doc type declarations always appear w/o
                        // parentheses)
                        {
                            var (s, ok) = (~d).Specs[0]._<ж<ast.TypeSpec>>(ᐧ); if (ok) {
                                r.readType(d, s);
                            }
                        }
                        break;
                    }
                    foreach (var (_, spec) in (~d).Specs) {
                        {
                            var (s, ok) = spec._<ж<ast.TypeSpec>>(ᐧ); if (ok) {
                                // use an individual (possibly fake) declaration
                                // for each type; this also ensures that each type
                                // gets to (re-)use the declaration documentation
                                // if there's none associated with the spec itself
                                var fake = Ꮡ(new ast.GenDecl(
                                    Doc: (~d).Doc, // don't use the existing TokPos because it
 // will lead to the wrong selection range for
 // the fake declaration if there are more
 // than one type in the group (this affects
 // src/cmd/godoc/godoc.go's posLink_urlFunc)

                                    TokPos: s.Pos(),
                                    Tok: token.TYPE,
                                    Specs: new ast.Spec[]{new ast.TypeSpecжSpec(s)}.slice()
                                ));
                                r.readType(fake, s);
                            }
                        }
                    }
                } while (false);
            }

            break;
        }}
    }
    // collect MARKER(...): annotations
    r.readNotes(src.Comments);
    if ((Mode)(r.mode & PreserveAST) == 0) {
        src.Comments = default!; // consumed unassociated comments - remove from AST
    }
}

internal static void readPackage(this ж<reader> Ꮡr, ж<ast.Package> Ꮡpkg, Mode mode) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var pkg = ref Ꮡpkg.DerefOrNull();

    // initialize reader
    r.filenames = new slice<@string>(len(pkg.Files));
    r.imports = new map<@string, nint>();
    r.mode = mode;
    r.types = new map<@string, ж<namedType>>();
    r.funcs = new methodSet(0);
    r.notes = new map<@string, slice<ж<Note>>>();
    r.importByName = new map<@string, @string>();
    // sort package files before reading them so that the
    // result does not depend on map iteration order
    nint i = 0;
    foreach (var (filename, _) in pkg.Files) {
        r.filenames[i] = filename;
        i++;
    }
    slices.Sort<slice<@string>, @string>(r.filenames);
    // process files in sorted order
    foreach (var (_, filename) in r.filenames) {
        var f = pkg.Files[filename];
        if ((Mode)(mode & AllDecls) == 0) {
            r.fileExports(f);
        }
        Ꮡr.readFile(f);
    }
    foreach (var (name, path) in r.importByName) {
        if (path == ""u8) {
            delete(r.importByName, name);
        }
    }
    // process functions now that we have better type information
    foreach (var (_, f) in pkg.Files) {
        foreach (var (_, decl) in (~f).Decls) {
            {
                var (d, ok) = decl._<ж<ast.FuncDecl>>(ᐧ); if (ok) {
                    r.readFunc(d);
                }
            }
        }
    }
}

// ----------------------------------------------------------------------------
// Types
internal static ж<Func> customizeRecv(ж<Func> Ꮡf, @string recvTypeName, bool embeddedIsPtr, nint level) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == nil || f.Decl == nil || (~f.Decl).Recv == nil || len((~(~f.Decl).Recv).List) != 1) {
        return Ꮡf; // shouldn't happen, but be safe
    }
    // copy existing receiver field and set new type
    ref var newField = ref heap<ast.Field>(out var ᏑnewField);
    newField = (~(~f.Decl).Recv).List[0].Value;
    ref var origPos = ref heap<tokenꓸPos>(out var ᏑorigPos);
    origPos = newField.Type.Pos();
    var (_, origRecvIsPtr) = newField.Type._<ж<ast.StarExpr>>(ᐧ);
    var newIdent = Ꮡ(new ast.Ident(NamePos: origPos, Name: recvTypeName));
    ast.Expr typ = new ast.IdentжExpr(newIdent);
    if (!embeddedIsPtr && origRecvIsPtr) {
        newIdent.Value.NamePos++; // '*' is one character
        typ = new ast.StarExprжExpr(Ꮡ(new ast.StarExpr(Star: origPos, X: new ast.IdentжExpr(newIdent))));
    }
    newField.Type = typ;
    // copy existing receiver field list and set new receiver field
    ref var newFieldList = ref heap<ast.FieldList>(out var ᏑnewFieldList);
    newFieldList = (~f.Decl).Recv.Value;
    newFieldList.List = new ж<ast.Field>[]{ᏑnewField}.slice();
    // copy existing function declaration and set new receiver field list
    ref var newFuncDecl = ref heap<ast.FuncDecl>(out var ᏑnewFuncDecl);
    newFuncDecl = f.Decl.Value;
    newFuncDecl.Recv = ᏑnewFieldList;
    // copy existing function documentation and set new declaration
    ref var newF = ref heap<Func>(out var ᏑnewF);
    newF = f;
    newF.Decl = ᏑnewFuncDecl;
    newF.Recv = recvString(typ);
    // the Orig field never changes
    newF.Level = level;
    return ᏑnewF;
}

// collectEmbeddedMethods collects the embedded methods of typ in mset.
internal static void collectEmbeddedMethods(this ref reader r, methodSet mset, ж<namedType> Ꮡtyp, @string recvTypeName, bool embeddedIsPtr, nint level, embeddedSet visited) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    visited[Ꮡtyp] = true;
    foreach (var (embedded, isPtr) in typ.embedded) {
        // Once an embedded type is embedded as a pointer type
        // all embedded types in those types are treated like
        // pointer types for the purpose of the receiver type
        // computation; i.e., embeddedIsPtr is sticky for this
        // embedding hierarchy.
        var thisEmbeddedIsPtr = embeddedIsPtr || isPtr;
        foreach (var (_, m) in (~embedded).methods) {
            // only top-level methods are embedded
            if ((~m).Level == 0) {
                mset.add(customizeRecv(m, recvTypeName, thisEmbeddedIsPtr, level));
            }
        }
        if (!visited[embedded]) {
            r.collectEmbeddedMethods(mset, embedded, recvTypeName, thisEmbeddedIsPtr, level + 1, visited);
        }
    }
    delete(visited, Ꮡtyp);
}

// computeMethodSets determines the actual method sets for each type encountered.
internal static void computeMethodSets(this ref reader r) {
    foreach (var (_, t) in r.types) {
        // collect embedded methods for t
        if ((~t).isStruct){
            // struct
            r.collectEmbeddedMethods((~t).methods, t, (~t).name, false, 1, new embeddedSet(0));
        } else {
        }
    }
    // interface
    // TODO(gri) fix this
    // For any predeclared names that are declared locally, don't treat them as
    // exported fields anymore.
    foreach (var (predecl, _) in r.shadowedPredecl) {
        foreach (var (_, ityp) in r.fixmap[predecl]) {
            removeAnonymousField(predecl, ref (ityp).DerefOrNull());
        }
    }
}

// cleanupTypes removes the association of functions and methods with
// types that have no declaration. Instead, these functions and methods
// are shown at the package level. It also removes types with missing
// declarations or which are not visible.
internal static void cleanupTypes(this ref reader r) {
    foreach (var (_, t) in r.types) {
        var visible = r.isVisible((~t).name);
        var predeclared = predeclaredTypes[(~t).name];
        if ((~t).decl == nil && (predeclared || visible && ((~t).isEmbedded || r.hasDotImp))) {
            // t.name is a predeclared type (and was not redeclared in this package),
            // or it was embedded somewhere but its declaration is missing (because
            // the AST is incomplete), or we have a dot-import (and all bets are off):
            // move any associated values, funcs, and methods back to the top-level so
            // that they are not lost.
            // 1) move values
            r.values = appendꓸꓸꓸ(r.values, (~t).values);
            // 2) move factory functions
            foreach (var (name, f) in (~t).funcs) {
                // in a correct AST, package-level function names
                // are all different - no need to check for conflicts
                r.funcs[name] = f;
            }
            // 3) move methods
            if (!predeclared) {
                foreach (var (name, m) in (~t).methods) {
                    // don't overwrite functions with the same name - drop them
                    {
                        var (_, found) = r.funcs[name, ꟷ]; if (!found) {
                            r.funcs[name] = m;
                        }
                    }
                }
            }
        }
        // remove types w/o declaration or which are not visible
        if ((~t).decl == nil || !visible) {
            delete(r.types, (~t).name);
        }
    }
}

// ----------------------------------------------------------------------------
// Sorting
internal static slice<@string> sortedKeys(map<@string, nint> m) {
    var list = new slice<@string>(len(m));
    nint i = 0;
    foreach (var (key, _) in m) {
        list[i] = key;
        i++;
    }
    slices.Sort<slice<@string>, @string>(list);
    return list;
}

// sortingName returns the name to use when sorting d into place.
internal static @string sortingName(ref ast.GenDecl d) {
    if (len(d.Specs) == 1) {
        {
            var (s, ok) = d.Specs[0]._<ж<ast.ValueSpec>>(ᐧ); if (ok) {
                return (~(~s).Names[0]).Name;
            }
        }
    }
    return ""u8;
}

internal static slice<ж<Value>> sortedValues(slice<ж<Value>> m, token.Token tok) {
    var list = new slice<ж<Value>>(len(m)); // big enough in any case
    nint i = 0;
    foreach (var (_, val) in m) {
        if ((~(~val).Decl).Tok == tok) {
            list[i] = val;
            i++;
        }
    }
    list = list.slice(0, i);
    slices.SortFunc(list, (ж<Value> a, ж<Value> b) => {
        nint r = strings.Compare(sortingName(ref ((~a).Decl).DerefOrNull()), sortingName(ref ((~b).Decl).DerefOrNull()));
        if (r != 0) {
            return r;
        }
        return cmp.Compare((~a).order, (~b).order);
    });
    return list;
}

internal static slice<ж<Type>> sortedTypes(map<@string, ж<namedType>> m, bool allMethods) {
    var list = new slice<ж<Type>>(len(m));
    nint i = 0;
    foreach (var (_, t) in m) {
        list[i] = Ꮡ(new Type(
            Doc: (~t).doc,
            Name: (~t).name,
            Decl: (~t).decl,
            Consts: sortedValues((~t).values, token.CONST),
            Vars: sortedValues((~t).values, token.VAR),
            Funcs: sortedFuncs((~t).funcs, true),
            Methods: sortedFuncs((~t).methods, allMethods)
        ));
        i++;
    }
    slices.SortFunc(list, (ж<Type> a, ж<Type> b) => strings.Compare((~a).Name, (~b).Name));
    return list;
}

internal static @string removeStar(@string s) {
    if (len(s) > 0 && s[0] == (rune)'*') {
        return s[1..];
    }
    return s;
}

internal static slice<ж<Func>> sortedFuncs(methodSet m, bool allMethods) {
    var list = new slice<ж<Func>>(len(m));
    nint i = 0;
    foreach (var (_, mΔ1) in m) {
        // determine which methods to include
        switch (ᐧ) {
        case {} when (~mΔ1).Decl == nil: {
            break;
        }
        case {} when (allMethods) || ((~mΔ1).Level == 0) || (!token.IsExported(removeStar((~mΔ1).Orig))): {
            list[i] = mΔ1;
            i++;
            break;
        }}

    }
    // exclude conflict entry
    // forced inclusion, method not embedded, or method
    // embedded but original receiver type not exported
    list = list.slice(0, i);
    slices.SortFunc(list, (ж<Func> a, ж<Func> b) => strings.Compare((~a).Name, (~b).Name));
    return list;
}

// noteBodies returns a list of note body strings given a list of notes.
// This is only used to populate the deprecated Package.Bugs field.
internal static slice<@string> noteBodies(slice<ж<Note>> notes) {
    slice<@string> list = default!;
    foreach (var (_, n) in notes) {
        list = append(list, (~n).Body);
    }
    return list;
}

// ----------------------------------------------------------------------------
// Predeclared identifiers

// IsPredeclared reports whether s is a predeclared identifier.
public static bool IsPredeclared(@string s) {
    return predeclaredTypes[s] || predeclaredFuncs[s] || predeclaredConstants[s];
}

internal static map<@string, bool> predeclaredTypes = new map<@string, bool>{
    ["any"u8] = true,
    ["bool"u8] = true,
    ["byte"u8] = true,
    ["comparable"u8] = true,
    ["complex64"u8] = true,
    ["complex128"u8] = true,
    ["error"u8] = true,
    ["float32"u8] = true,
    ["float64"u8] = true,
    ["int"u8] = true,
    ["int8"u8] = true,
    ["int16"u8] = true,
    ["int32"u8] = true,
    ["int64"u8] = true,
    ["rune"u8] = true,
    ["string"u8] = true,
    ["uint"u8] = true,
    ["uint8"u8] = true,
    ["uint16"u8] = true,
    ["uint32"u8] = true,
    ["uint64"u8] = true,
    ["uintptr"u8] = true
};

internal static map<@string, bool> predeclaredFuncs = new map<@string, bool>{
    ["append"u8] = true,
    ["cap"u8] = true,
    ["clear"u8] = true,
    ["close"u8] = true,
    ["complex"u8] = true,
    ["copy"u8] = true,
    ["delete"u8] = true,
    ["imag"u8] = true,
    ["len"u8] = true,
    ["make"u8] = true,
    ["max"u8] = true,
    ["min"u8] = true,
    ["new"u8] = true,
    ["panic"u8] = true,
    ["print"u8] = true,
    ["println"u8] = true,
    ["real"u8] = true,
    ["recover"u8] = true
};

internal static map<@string, bool> predeclaredConstants = new map<@string, bool>{
    ["false"u8] = true,
    ["iota"u8] = true,
    ["nil"u8] = true,
    ["true"u8] = true
};

// assumedPackageName returns the assumed package name
// for a given import path. This is a copy of
// golang.org/x/tools/internal/imports.ImportPathToAssumedName.
internal static @string assumedPackageName(@string importPath) {
    var notIdentifier = (rune ch) => !((rune)'a' <= ch && ch <= (rune)'z' || (rune)'A' <= ch && ch <= (rune)'Z' || (rune)'0' <= ch && ch <= (rune)'9' || ch == (rune)'_' || ch >= utf8.RuneSelf && (unicode.IsLetter(ch) || unicode.IsDigit(ch)));
    @string @base = path.Base(importPath);
    if (strings.HasPrefix(@base, "v"u8)) {
        {
            var (_, err) = strconv.Atoi(@base[1..]); if (err == default!) {
                @string dir = path.Dir(importPath);
                if (dir != "."u8) {
                    @base = path.Base(dir);
                }
            }
        }
    }
    @base = strings.TrimPrefix(@base, "go-"u8);
    {
        nint i = strings.IndexFunc(@base, notIdentifier); if (i >= 0) {
            @base = @base.slice(0, i);
        }
    }
    return @base;
}

} // end doc_package
