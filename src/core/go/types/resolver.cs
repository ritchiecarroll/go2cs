// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using cmp = cmp_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// A declInfo describes a package-level const, type, var, or func declaration.
partial struct declInfo {
    internal ж<ΔScope> @file;     // scope of file containing this declaration
    internal goVersion version;     // Go version of file containing this declaration
    internal slice<ж<Var>> lhs; // lhs of n:1 variable declarations, or nil
    internal ast.Expr vtyp;      // type, or nil (for const and var declarations only)
    internal ast.Expr init;      // init/orig expression, or nil (for const and var declarations only)
    internal bool inherited;          // if set, the init expression is inherited from a previous constant declaration
    internal ж<ast.TypeSpec> tdecl; // type declaration, or nil
    internal ж<ast.FuncDecl> fdecl; // func declaration, or nil
    // The deps field tracks initialization expression dependencies.
    internal map<Object, bool> deps; // lazily initialized
}

// hasInitializer reports whether the declared object has an initialization
// expression or function body.
internal static bool hasInitializer(this ref declInfo d) {
    return d.init != default! || d.fdecl != nil && (~d.fdecl).Body != nil;
}

// addDep adds obj to the set of objects d's init expression depends on.
internal static void addDep(this ref declInfo d, Object obj) {
    var m = d.deps;
    if (m == default!) {
        m = new map<Object, bool>();
        d.deps = m;
    }
    m[obj] = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingTypeOrInitExprˢ = "missing type or init expr"u8;

// arityMatch checks that the lhs and rhs of a const or var decl
// have the appropriate number of names and init exprs. For const
// decls, init is the value spec providing the init exprs; for
// var decls, init is nil (the init exprs are in s in this case).
internal static void arityMatch(this ж<Checker> Ꮡcheck, ж<ast.ValueSpec> Ꮡs, ж<ast.ValueSpec> Ꮡinit) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();
    ref var init = ref Ꮡinit.DerefOrNull();

    nint l = len(s.Names);
    nint r = len(s.Values);
    if (Ꮡinit != nil) {
        r = len(init.Values);
    }
    errors.Code code = /* WrongAssignCount */ 17;
    switch (ᐧ) {
    case {} when Ꮡinit == nil && r == 0: {
        if (s.Type == default!) {
            // var decl w/o init expr
            Ꮡcheck.error(new ast_ValueSpecжpositioner(Ꮡs), code, missingTypeOrInitExprˢ);
        }
        break;
    }
    case {} when l < r: {
        if (l < len(s.Values)){
            // init exprs from s
            var n = s.Values[l];
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(n), code, "extra init expr %s"u8, n);
        } else {
            // TODO(gri) avoid declared and not used error here
            // init exprs "inherited"
            Ꮡcheck.errorf(new ast_ValueSpecжpositioner(Ꮡs), code, "extra init expr at %s"u8, check.fset.Position(init.Pos()));
        }
        break;
    }
    case {} when l > r && (Ꮡinit != nil || r != 1): {
        var n = s.Names[r];
        Ꮡcheck.errorf(new ast_Identжpositioner(n), // TODO(gri) avoid declared and not used error here
 code, "missing init expr for %s"u8, n.OrTypedNil());
        break;
    }}

}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string illegalCharsᶜ = "!\"#$%&'()*,:;<=>?[\\]^{|}`�";

internal static (@string, error) validatedImportPath(@string path) {
    var (s, err) = strconv.Unquote(path);
    if (err != default!) {
        return ("", err);
    }
    if (s == ""u8) {
        return ("", fmt.Errorf("empty string"u8));
    }
    @string illegalChars = illegalCharsᶜ;
    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return (s, fmt.Errorf("invalid character %#U"u8, r));
        }
    }
    return (s, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotDeclareInitMustBeˢ = "cannot declare init - must be func"u8;
internal static readonly @string cannotDeclareMainMustBeˢ = "cannot declare main - must be func"u8;

// declarePkgObj declares obj in the package scope, records its ident -> obj mapping,
// and updates check.objMap. The object must not be a function or method.
internal static void declarePkgObj(this ж<Checker> Ꮡcheck, ж<ast.Ident> Ꮡident, Object obj, ж<declInfo> Ꮡd) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var ident = ref Ꮡident.DerefOrNull();

    assert(ident.Name == obj.Name());
    // spec: "A package-scope or file-scope identifier with name init
    // may only be declared to be a function with this (func()) signature."
    if (ident.Name == "init"u8) {
        Ꮡcheck.error(new ast_Identжpositioner(Ꮡident), InvalidInitDecl, cannotDeclareInitMustBeˢ);
        return;
    }
    // spec: "The main package must have package name main and declare
    // a function main that takes no arguments and returns no value."
    if (ident.Name == "main"u8 && (~check.pkg).name == "main"u8) {
        Ꮡcheck.error(new ast_Identжpositioner(Ꮡident), InvalidMainDecl, cannotDeclareMainMustBeˢ);
        return;
    }
    Ꮡcheck.declare((~check.pkg).scope, Ꮡident, obj, nopos);
    check.objMap[obj] = Ꮡd;
    obj.setOrder((uint32)len(check.objMap));
}

// filename returns a filename suitable for debugging output.
internal static @string filename(this ref Checker check, nint fileNo) {
    var @file = check.files[fileNo];
    {
        tokenꓸPos pos = @file.Pos(); if (pos.IsValid()) {
            return check.fset.File(pos).Name();
        }
    }
    return fmt.Sprintf("file[%d]"u8, fileNo);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotUseFakeImportCAndˢ = "cannot use FakeImportC and go115UsesCgo together"u8;

internal static ж<Package> importPackage(this ж<Checker> Ꮡcheck, positioner at, @string path, @string dir) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // If we already have a package for the given (path, dir)
    // pair, use it instead of doing a full import.
    // Checker.impMap only caches packages that are marked Complete
    // or fake (dummy packages for failed imports). Incomplete but
    // non-fake packages do require an import to complete them.
    var key = new importKey(path, dir);
    var imp = check.impMap[key];
    if (imp != nil) {
        return imp;
    }
    // no package yet => import it
    if (path == "C"u8 && ((~check.conf).FakeImportC || (~check.conf).go115UsesCgo)){
        if ((~check.conf).FakeImportC && (~check.conf).go115UsesCgo) {
            Ꮡcheck.error(at, BadImportPath, cannotUseFakeImportCAndˢ);
        }
        imp = NewPackage("C"u8, "C"u8);
        imp.Value.fake = true; // package scope is not populated
        imp.Value.cgo = check.conf.Value.go115UsesCgo;
    } else {
        // ordinary import
        error err = default!;
        {
            var importer = check.conf.Value.Importer; if (importer == default!){
                err = fmt.Errorf("Config.Importer not installed"u8);
            } else 
            {
                var (importerFrom, ok) = importer._<ImporterFrom>(ᐧ); if (ok){
                    (imp, err) = importerFrom.ImportFrom(path, dir, 0);
                    if (imp == nil && err == default!) {
                        err = fmt.Errorf("Config.Importer.ImportFrom(%s, %s, 0) returned nil but no error"u8, path, dir);
                    }
                } else {
                    (imp, err) = importer.Import(path);
                    if (imp == nil && err == default!) {
                        err = fmt.Errorf("Config.Importer.Import(%s) returned nil but no error"u8, path);
                    }
                }
            }
        }
        // make sure we have a valid package name
        // (errors here can only happen through manipulation of packages after creation)
        if (err == default! && imp != nil && ((~imp).name == "_"u8 || (~imp).name == ""u8)) {
            err = fmt.Errorf("invalid package name: %q"u8, (~imp).name);
            imp = default!; // create fake package below
        }
        if (err != default!) {
            Ꮡcheck.errorf(at, BrokenImport, "could not import %s (%s)"u8, path, err);
            if (imp == nil) {
                // create a new fake package
                // come up with a sensible package name (heuristic)
                @string name = path;
                {
                    nint i = len(name); if (i > 0 && name[i - 1] == (rune)'/') {
                        name = name.slice(0, i - 1);
                    }
                }
                {
                    nint i = strings.LastIndex(name, "/"u8); if (i >= 0) {
                        name = name.slice(i + 1);
                    }
                }
                imp = NewPackage(path, name);
            }
            // continue to use the package as best as we can
            imp.Value.fake = true; // avoid follow-up lookup failures
        }
    }
    // package should be complete or marked fake, but be cautious
    if ((~imp).complete || (~imp).fake) {
        check.impMap[key] = imp;
        // Once we've formatted an error message, keep the pkgPathMap
        // up-to-date on subsequent imports. It is used for package
        // qualification in error messages.
        if (check.pkgPathMap != default!) {
            check.markImports(imp);
        }
        return imp;
    }
    // something went wrong (importer may have returned incomplete package without error)
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotRenameImportCˢ = @"cannot rename import ""C"""u8;
internal static readonly @string cannotImportPackageAsˢ = "cannot import package as init - init must be a func"u8;
internal static readonly @string methodHasNoReceiverˢ = "method has no receiver"u8;

internal partial struct collectObjects_methodInfo /*dyn*/ {
    internal ж<Func> obj;   // method
    internal bool ptr;       // true if pointer receiver
    internal ж<ast.Ident> recv; // receiver type name
}

// collectObjects collects all file and package objects and inserts them
// into their respective scopes. It also performs imports and associates
// methods with receiver base type names.
internal static void collectObjects(this ж<Checker> Ꮡcheck) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var pkg = check.pkg;
    // pkgImports is the set of packages already imported by any package file seen
    // so far. Used to avoid duplicate entries in pkg.imports. Allocate and populate
    // it (pkg.imports may not be empty if we are checking test files incrementally).
    // Note that pkgImports is keyed by package (and thus package path), not by an
    // importKey value. Two different importKey values may map to the same package
    // which is why we cannot use the check.impMap here.
    map<ж<Package>, bool> pkgImports = new map<ж<Package>, bool>();
    foreach (var (_, imp) in (~pkg).imports) {
        pkgImports[imp] = true;
    }
    ref var methods = ref heap<slice<collectObjects_methodInfo>>(out var Ꮡmethods);                                  // collected methods with valid receivers and non-blank _ names
    var fileScopes = new slice<ж<ΔScope>>(len(check.files)); // fileScopes[i] corresponds to check.files[i]
    foreach (var (fileNo, @file) in check.files) {
        check.version = asGoVersion(check.versions[@file]);
        // The package identifier denotes the current package,
        // but there is no corresponding package object.
        check.recordDef((~@file).Name, default!);
        // Use the actual source file extent rather than *ast.File extent since the
        // latter doesn't include comments which appear at the start or end of the file.
        // Be conservative and use the *ast.File extent if we don't have a *token.File.
        tokenꓸPos pos = @file.Pos();
        tokenꓸPos end = @file.End();
        {
            var f = check.fset.File(@file.Pos()); if (f != nil) {
                (pos, end) = (((tokenꓸPos)f.Base()), ((tokenꓸPos)(f.Base() + f.Size())));
            }
        }
        var fileScope = NewScope((~pkg).scope, pos, end, check.filename(fileNo));
        fileScopes[fileNo] = fileScope;
        check.recordScope(new ast.FileжNode(@file), fileScope);
        // determine file directory, necessary to resolve imports
        // FileName may be "" (typically for tests) in which case
        // we get "." as the directory which is what we would want.
        @string fileDir = dir(check.fset.Position((~@file).Name.Pos()).Filename);
        var fileScopeʗ1 = fileScope;
        var pkgʗ1 = pkg;
        var pkgImportsʗ1 = pkgImports;
        Ꮡcheck.walkDecls((~@file).Decls, (decl d) => {
            switch (d.type()) {
            case importDecl dΔ1: {
                if ((~(~dΔ1.spec).Path).Value == ""u8) {
                    // import package
                    return; // error reported by parser
                }
                var (path, err) = validatedImportPath((~(~dΔ1.spec).Path).Value);
                if (err != default!) {
                    Ꮡcheck.errorf(new ast_BasicLitжpositioner((~dΔ1.spec).Path), BadImportPath, "invalid import path (%s)"u8, err);
                    return;
                }
                var imp = Ꮡcheck.importPackage(new ast_BasicLitжpositioner((~dΔ1.spec).Path), path, fileDir);
                if (imp == nil) {
                    return;
                }
                @string name = imp.Value.name;
                if ((~dΔ1.spec).Name != nil) {
                    // local name overrides imported package name
                    name = dΔ1.spec.Value.Name.Value.Name;
                    if (path == "C"u8) {
                        // match 1.17 cmd/compile (not prescribed by spec)
                        Ꮡcheck.error(new ast_Identжpositioner((~dΔ1.spec).Name), ImportCRenamed, cannotRenameImportCˢ);
                        return;
                    }
                }
                if (name == "init"u8) {
                    Ꮡcheck.error(new ast_ImportSpecжpositioner(dΔ1.spec), InvalidInitDecl, cannotImportPackageAsˢ);
                    return;
                }
                if (!pkgImportsʗ1[imp]) {
                    // add package to list of explicit imports
                    // (this functionality is provided as a convenience
                    // for clients; it is not needed for type-checking)
                    pkgImportsʗ1[imp] = true;
                    pkgʗ1.Value.imports = append((~pkgʗ1).imports, imp);
                }
                var pkgName = NewPkgName(dΔ1.spec.Pos(), pkgʗ1, name, imp);
                if ((~dΔ1.spec).Name != nil){
                    // in a dot-import, the dot represents the package
                    Ꮡcheck.Value.recordDef((~dΔ1.spec).Name, new PkgNameжObject(pkgName));
                } else {
                    Ꮡcheck.Value.recordImplicit(new ast.ImportSpecжNode(dΔ1.spec), new PkgNameжObject(pkgName));
                }
                if ((~imp).fake) {
                    // match 1.17 cmd/compile (not prescribed by spec)
                    Ꮡcheck.Value.usedPkgNames[pkgName] = true;
                }
                Ꮡcheck.Value.imports = append(Ꮡcheck.Value.imports, // add import to file scope
 pkgName);
                if (name == "."u8){
                    // dot-import
                    if (Ꮡcheck.Value.dotImportMap == default!) {
                        Ꮡcheck.Value.dotImportMap = new map<dotImportKey, ж<PkgName>>();
                    }
                    // merge imported scope with file scope
                    foreach (var (nameΔ1, obj) in (~(~imp).scope).elems) {
                        // Note: Avoid eager resolve(name, obj) here, so we only
                        // resolve dot-imported objects as needed.
                        // A package scope may contain non-exported objects,
                        // do not import them!
                        if (token.IsExported(nameΔ1)) {
                            // declare dot-imported object
                            // (Do not use check.declare because it modifies the object
                            // via Object.setScopePos, which leads to a race condition;
                            // the object may be imported into more than one file scope
                            // concurrently. See go.dev/issue/32154.)
                            {
                                var alt = fileScopeʗ1.Lookup(nameΔ1); if (alt != default!){
                                    var errΔ1 = Ꮡcheck.newError(DuplicateDecl);
                                    errΔ1.addf(new ast_Identжpositioner((~dΔ1.spec).Name), "%s redeclared in this block"u8, alt.Name());
                                    errΔ1.addAltDecl(alt);
                                    errΔ1.report();
                                } else {
                                    fileScopeʗ1.insert(nameΔ1, obj);
                                    Ꮡcheck.Value.dotImportMap[new dotImportKey(fileScopeʗ1, nameΔ1)] = pkgName;
                                }
                            }
                        }
                    }
                } else {
                    // declare imported package object in file scope
                    // (no need to provide s.Name since we called check.recordDef earlier)
                    Ꮡcheck.declare(fileScopeʗ1, nil, new PkgNameжObject(pkgName), nopos);
                }
                break;
            }
            case ΔconstDecl dΔ1: {
                foreach (var (i, name) in (~dΔ1.spec).Names) {
                    // declare all constants
                    var obj = NewConst(name.Pos(), pkgʗ1, (~name).Name, default!, constant.MakeInt64((int64)dΔ1.iota));
                    ast.Expr init = default!;
                    if (i < len(dΔ1.init)) {
                        init = dΔ1.init[i];
                    }
                    var dΔ2 = Ꮡ(new declInfo(@file: fileScopeʗ1, version: Ꮡcheck.Value.version, vtyp: dΔ1.typ, init: init, inherited: dΔ1.inherited));
                    Ꮡcheck.declarePkgObj(name, new ConstжObject(obj), dΔ2);
                }
                break;
            }
            case ΔvarDecl dΔ1: {
                var lhs = new slice<ж<Var>>(len((~dΔ1.spec).Names));
                // If there's exactly one rhs initializer, use
                // the same declInfo d1 for all lhs variables
                // so that each lhs variable depends on the same
                // rhs initializer (n:1 var declaration).
                ж<declInfo> d1 = default!;
                if (len((~dΔ1.spec).Values) == 1) {
                    // The lhs elements are only set up after the for loop below,
                    // but that's ok because declareVar only collects the declInfo
                    // for a later phase.
                    d1 = Ꮡ(new declInfo(@file: fileScopeʗ1, version: Ꮡcheck.Value.version, lhs: lhs, vtyp: (~dΔ1.spec).Type, init: (~dΔ1.spec).Values[0]));
                }
                foreach (var (i, name) in (~dΔ1.spec).Names) {
                    // declare all variables
                    var obj = NewVar(name.Pos(), pkgʗ1, (~name).Name, default!);
                    lhs[i] = obj;
                    var di = d1;
                    if (di == nil) {
                        // individual assignments
                        ast.Expr init = default!;
                        if (i < len((~dΔ1.spec).Values)) {
                            init = (~dΔ1.spec).Values[i];
                        }
                        di = Ꮡ(new declInfo(@file: fileScopeʗ1, version: Ꮡcheck.Value.version, vtyp: (~dΔ1.spec).Type, init: init));
                    }
                    Ꮡcheck.declarePkgObj(name, new VarжObject(obj), di);
                }
                break;
            }
            case ΔtypeDecl dΔ1: {
                var obj = NewTypeName((~dΔ1.spec).Name.Pos(), pkgʗ1, (~(~dΔ1.spec).Name).Name, default!);
                Ꮡcheck.declarePkgObj((~dΔ1.spec).Name, new TypeNameжObject(obj), Ꮡ(new declInfo(@file: fileScopeʗ1, version: Ꮡcheck.Value.version, tdecl: dΔ1.spec)));
                break;
            }
            case ΔfuncDecl dΔ1: {
                @string name = dΔ1.decl.Value.Name.Value.Name;
                var obj = NewFunc((~dΔ1.decl).Name.Pos(), pkgʗ1, name, nil); // signature set later
                var hasTParamError = false; // avoid duplicate type parameter errors
                if ((~dΔ1.decl).Recv.NumFields() == 0){
                    // regular function
                    if ((~dΔ1.decl).Recv != nil) {
                        Ꮡcheck.error(new ast_FieldListжpositioner((~dΔ1.decl).Recv), BadRecv, methodHasNoReceiverˢ);
                    }
                    // treat as function
                    if (name == "init"u8 || (name == "main"u8 && (~Ꮡcheck.Value.pkg).name == "main"u8)) {
                        errors.Code code = InvalidInitDecl;
                        if (name == "main"u8) {
                            code = InvalidMainDecl;
                        }
                        if ((~(~dΔ1.decl).Type).TypeParams.NumFields() != 0) {
                            Ꮡcheck.softErrorf(new ast_Fieldжpositioner((~(~(~dΔ1.decl).Type).TypeParams).List[0]), code, "func %s must have no type parameters"u8, name);
                            hasTParamError = true;
                        }
                        {
                            var t = dΔ1.decl.Value.Type; if ((~t).Params.NumFields() != 0 || (~t).Results != nil) {
                                // TODO(rFindley) Should this be a hard error?
                                Ꮡcheck.softErrorf(new ast_Identжpositioner((~dΔ1.decl).Name), code, "func %s must have no arguments and no return values"u8, name);
                            }
                        }
                    }
                    if (name == "init"u8){
                        // don't declare init functions in the package scope - they are invisible
                        obj.Value.parent = pkgʗ1.Value.scope;
                        Ꮡcheck.Value.recordDef((~dΔ1.decl).Name, new FuncжObject(obj));
                        // init functions must have a body
                        if ((~dΔ1.decl).Body == nil) {
                            // TODO(gri) make this error message consistent with the others above
                            Ꮡcheck.softErrorf(new Funcжpositioner(obj), MissingInitBody, "missing function body"u8);
                        }
                    } else {
                        Ꮡcheck.declare((~pkgʗ1).scope, (~dΔ1.decl).Name, new FuncжObject(obj), nopos);
                    }
                } else {
                    // method
                    // TODO(rFindley) earlier versions of this code checked that methods
                    //                have no type parameters, but this is checked later
                    //                when type checking the function type. Confirm that
                    //                we don't need to check tparams here.
                    var (ptr, @base, _) = Ꮡcheck.unpackRecv((~(~(~dΔ1.decl).Recv).List[0]).Type, false);
                    // (Methods with invalid receiver cannot be associated to a type, and
                    // methods with blank _ names are never found; no need to collect any
                    // of them. They will still be type-checked with all the other functions.)
                    {
                        var (recv, _) = @base._<ж<ast.Ident>>(ᐧ); if (recv != nil && name != "_"u8) {
                            Ꮡmethods.ValueSlot = append(Ꮡmethods.ValueSlot, new collectObjects_methodInfo(obj, ptr, recv));
                        }
                    }
                    Ꮡcheck.Value.recordDef((~dΔ1.decl).Name, new FuncжObject(obj));
                }
                _ = (~(~dΔ1.decl).Type).TypeParams.NumFields() != 0 && !hasTParamError && Ꮡcheck.verifyVersionf(new ast_Fieldжpositioner((~(~(~dΔ1.decl).Type).TypeParams).List[0]), go1_18, "type parameter"u8);
                var info = Ꮡ(new declInfo(@file: fileScopeʗ1, version: Ꮡcheck.Value.version, fdecl: dΔ1.decl));
                Ꮡcheck.Value.objMap[new FuncжObject(obj)] = info;
                obj.of(Func.Ꮡobject).setOrder((uint32)len(Ꮡcheck.Value.objMap));
                break;
            }}
        });
    }
    // Methods are not package-level objects but we still track them in the
    // object map so that we can handle them like regular functions (if the
    // receiver is invalid); also we need their fdecl info when associating
    // them with their receiver base type, below.
    // verify that objects in package and file scopes have different names
    foreach (var (_, scope) in fileScopes) {
        foreach (var (name, vᴛ1) in (~scope).elems) {
            var obj = vᴛ1;

            {
                var alt = (~pkg).scope.Lookup(name); if (alt != default!) {
                    obj = resolve(name, obj);
                    var err = Ꮡcheck.newError(DuplicateDecl);
                    {
                        var (pkgΔ1, ok) = obj._<ж<PkgName>>(ᐧ); if (ok){
                            err.addf(new Objectᴠpositioner(alt), "%s already declared through import of %s"u8, alt.Name(), pkgΔ1.Imported().OrTypedNil());
                            err.addAltDecl(new PkgNameжObject(pkgΔ1));
                        } else {
                            err.addf(new Objectᴠpositioner(alt), "%s already declared through dot-import of %s"u8, alt.Name(), obj.Pkg().OrTypedNil());
                            // TODO(gri) dot-imported objects don't have a position; addAltDecl won't print anything
                            err.addAltDecl(obj);
                        }
                    }
                    err.report();
                }
            }
        }
    }
    // Now that we have all package scope objects and all methods,
    // associate methods with receiver base type name where possible.
    // Ignore methods that have an invalid receiver. They will be
    // type-checked later, with regular functions.
    if (methods == default!) {
        return;
    }
    check.methods = new map<ж<TypeName>, slice<ж<Func>>>();
    foreach (var (i, _) in methods) {
        var m = Ꮡ(methods, i);
        // Determine the receiver base type and associate m with it.
        var (ptr, @base) = check.resolveBaseTypeName((~m).ptr, (~m).recv);
        if (@base != nil) {
            m.Value.obj.Value.hasPtrRecv_ = ptr;
            check.methods[@base] = append(check.methods[@base], (~m).obj);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parameterizedReceiverˢ = "parameterized receiver contains nil parameters"u8;

// unpackRecv unpacks a receiver type expression and returns its components: ptr indicates
// whether rtyp is a pointer receiver, base is the receiver base type expression stripped
// of its type parameters (if any), and tparams are its type parameter names, if any. The
// type parameters are only unpacked if unpackParams is set. For instance, given the rtyp
//
//	*T[A, _]
//
// ptr is true, base is T, and tparams is [A, _] (assuming unpackParams is set).
// Note that base may not be a *ast.Ident for erroneous programs.
internal static (bool ptr, ast.Expr @base, slice<ж<ast.Ident>> tparams) unpackRecv(this ж<Checker> Ꮡcheck, ast.Expr rtyp, bool unpackParams) {
    bool ptr = default!;
    ast.Expr @base = default!;
    slice<ж<ast.Ident>> tparams = default!;

    // unpack receiver type
    @base = ast.Unparen(rtyp);
    {
        var (t, _) = @base._<ж<ast.StarExpr>>(ᐧ); if (t != nil) {
            ptr = true;
            @base = ast.Unparen((~t).X);
        }
    }
    // unpack type parameters, if any
    switch (@base.type()) {
    case ж<ast.IndexExpr> _:
    case ж<ast.IndexListExpr> _: {
        var ix = unpackIndexedExpr(@base);
        @base = ix.Value.x;
        if (unpackParams) {
            foreach (var (_, arg) in (~ix).indices) {
                ж<ast.Ident> par = default!;
                switch (arg.type()) {
                case ж<ast.Ident> argΔ1: {
                    par = argΔ1;
                    break;
                }
                case ж<ast.BadExpr> argΔ1: {
                    break;
                }
                case null: {
                    Ꮡcheck.error(new ast_Exprᴠpositioner((~ix).orig), // ignore - error already reported by parser
 InvalidSyntaxTree, parameterizedReceiverˢ);
                    break;
                }
                default: {
                    var argΔ1 = arg;
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(argΔ1), BadDecl, "receiver type parameter %s must be an identifier"u8, argΔ1);
                    break;
                }}
                if (par == nil) {
                    par = Ꮡ(new ast.Ident(NamePos: arg.Pos(), Name: "_"u8));
                }
                tparams = append(tparams, par);
            }
        }
        break;
    }}

    return (ptr, @base, tparams);
}

// resolveBaseTypeName returns the non-alias base type name for the given name, and whether
// there was a pointer indirection to get to it. The base type name must be declared
// in package scope, and there can be at most one pointer indirection. Traversals
// through generic alias types are not permitted. If no such type name exists, the
// returned base is nil.
internal static (bool ptr_, ж<TypeName> @base) resolveBaseTypeName(this ref Checker check, bool ptr, ж<ast.Ident> Ꮡname) {
    ref var name = ref Ꮡname.DerefOrNull();

    // Algorithm: Starting from name, which is expected to denote a type,
    // we follow that type through non-generic alias declarations until
    // we reach a non-alias type name.
    map<ж<TypeName>, bool> seen = default!;
    while (Ꮡname != nil) {
        // name must denote an object found in the current package scope
        // (note that dot-imported objects are not in the package scope!)
        var obj = (~check.pkg).scope.Lookup(name.Name);
        if (obj == default!) {
            break;
        }
        // the object must be a type name...
        var (tname, _) = obj._<ж<TypeName>>(ᐧ);
        if (tname == nil) {
            break;
        }
        // ... which we have not seen before
        if (seen[tname]) {
            break;
        }
        // we're done if tdecl describes a defined type (not an alias)
        var tdecl = check.objMap[new TypeNameжObject(tname)].Value.tdecl; // must exist for objects in package scope
        if (!(~tdecl).Assign.IsValid()) {
            return (ptr, tname);
        }
        // an alias must not be generic
        // (importantly, we must not collect such methods - was https://go.dev/issue/70417)
        if ((~tdecl).TypeParams != nil) {
            break;
        }
        // otherwise, remember this type name and continue resolving
        if (seen == default!) {
            seen = new map<ж<TypeName>, bool>();
        }
        seen[tname] = true;
        // The go/parser keeps parentheses; strip them, if any.
        var typ = ast.Unparen((~tdecl).Type);
        // dereference a pointer type
        {
            var (pexpr, _) = typ._<ж<ast.StarExpr>>(ᐧ); if (pexpr != nil) {
                // if we've already seen a pointer, we're done
                if (ptr) {
                    break;
                }
                ptr = true;
                typ = ast.Unparen((~pexpr).X); // continue with pointer base type
            }
        }
        // After dereferencing, typ must be a locally defined type name.
        // Referring to other packages (qualified identifiers) or going
        // through instantiated types (index expressions) is not permitted,
        // so we can ignore those.
        (Ꮡname, _) = typ._<ж<ast.Ident>>(ᐧ); name = ref Ꮡname.DerefOrNull();
        if (Ꮡname == nil) {
            break;
        }
    }
    // no base type found
    return (false, default!);
}

// packageObjects typechecks all package objects, but not function bodies.
internal static void packageObjects(this ж<Checker> Ꮡcheck) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // process package objects in source order for reproducible results
    var objList = new slice<Object>(len(check.objMap));
    nint i = 0;
    foreach (var (obj, _) in check.objMap) {
        objList[i] = obj;
        i++;
    }
    slices.SortFunc(objList, (Object a, Object b) => cmp_package.Compare(a.order(), b.order()));
    // add new methods to already type-checked types (from a prior Checker.Files call)
    foreach (var (_, obj) in objList) {
        {
            var (objΔ1, _) = obj._<ж<TypeName>>(ᐧ); if (objΔ1 != nil && (~objΔ1).typ != default!) {
                Ꮡcheck.collectMethods(objΔ1);
            }
        }
    }
    if (false && (~check.conf)._EnableAlias){
        // With Alias nodes we can process declarations in any order.
        //
        // TODO(adonovan): unfortunately, Alias nodes
        // (GODEBUG=gotypesalias=1) don't entirely resolve
        // problems with cycles. For example, in
        // GOROOT/test/typeparam/issue50259.go,
        //
        // 	type T[_ any] struct{}
        // 	type A T[B]
        // 	type B = T[A]
        //
        // TypeName A has Type Named during checking, but by
        // the time the unified export data is written out,
        // its Type is Invalid.
        //
        // Investigate and reenable this branch.
        foreach (var (_, obj) in objList) {
            Ꮡcheck.objDecl(obj, nil);
        }
    } else {
        // Without Alias nodes, we process non-alias type declarations first, followed by
        // alias declarations, and then everything else. This appears to avoid most situations
        // where the type of an alias is needed before it is available.
        // There may still be cases where this is not good enough (see also go.dev/issue/25838).
        // In those cases Checker.ident will report an error ("invalid use of type alias").
        slice<ж<TypeName>> aliasList = default!;
        slice<Object> othersList = default!;        // everything that's not a type
        // phase 1: non-alias type declarations
        foreach (var (_, obj) in objList) {
            {
                var (tname, _) = obj._<ж<TypeName>>(ᐧ); if (tname != nil){
                    if ((~(~check.objMap[new TypeNameжObject(tname)]).tdecl).Assign.IsValid()){
                        aliasList = append(aliasList, tname);
                    } else {
                        Ꮡcheck.objDecl(obj, nil);
                    }
                } else {
                    othersList = append(othersList, obj);
                }
            }
        }
        // phase 2: alias type declarations
        foreach (var (_, obj) in aliasList) {
            Ꮡcheck.objDecl(new TypeNameжObject(obj), nil);
        }
        // phase 3: all other declarations
        foreach (var (_, obj) in othersList) {
            Ꮡcheck.objDecl(obj, nil);
        }
    }
    // At this point we may have a non-empty check.methods map; this means that not all
    // entries were deleted at the end of typeDecl because the respective receiver base
    // types were not found. In that case, an error was reported when declaring those
    // methods. We can now safely discard this map.
    check.methods = default!;
}

// unusedImports checks for unused imports.
internal static void unusedImports(this ж<Checker> Ꮡcheck) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // If function bodies are not checked, packages' uses are likely missing - don't check.
    if ((~check.conf).IgnoreFuncBodies) {
        return;
    }
    // spec: "It is illegal (...) to directly import a package without referring to
    // any of its exported identifiers. To import a package solely for its side-effects
    // (initialization), use the blank identifier as explicit package name."
    foreach (var (_, obj) in check.imports) {
        if ((~obj).name != "_"u8 && !check.usedPkgNames[obj]) {
            Ꮡcheck.errorUnusedPkg(obj);
        }
    }
}

internal static void errorUnusedPkg(this ж<Checker> Ꮡcheck, ж<PkgName> Ꮡobj) {
    ref var obj = ref Ꮡobj.DerefOrNull();

    // If the package was imported with a name other than the final
    // import path element, show it explicitly in the error message.
    // Note that this handles both renamed imports and imports of
    // packages containing unconventional package declarations.
    // Note that this uses / always, even on Windows, because Go import
    // paths always use forward slashes.
    @string path = obj.imported.Value.path;
    @string elem = path;
    {
        nint i = strings.LastIndex(elem, "/"u8); if (i >= 0) {
            elem = elem.slice(i + 1);
        }
    }
    if (obj.name == ""u8 || obj.name == "."u8 || obj.name == elem){
        Ꮡcheck.softErrorf(new PkgNameжpositioner(Ꮡobj), UnusedImport, "%q imported and not used"u8, path);
    } else {
        Ꮡcheck.softErrorf(new PkgNameжpositioner(Ꮡobj), UnusedImport, "%q imported as %s and not used"u8, path, obj.name);
    }
}

// dir makes a good-faith attempt to return the directory
// portion of path. If path is empty, the result is ".".
// (Per the go/build package dependency tests, we cannot import
// path/filepath and simply use filepath.Dir.)
internal static @string dir(@string path) {
    {
        nint i = strings.LastIndexAny(path, @"/\"u8); if (i > 0) {
            return path.slice(0, i);
        }
    }
    // i <= 0
    return "."u8;
}

} // end types_package
