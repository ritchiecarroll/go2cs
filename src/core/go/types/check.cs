// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements the Check function, which drives type-checking.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using godebug = @internal.godebug_package;
using static @internal.types.errors_package;
using atomic = global::go.sync.atomic_package;
using @internal;
using errors = @internal.types.errors_package;
using global::go.go;
using global::go.sync;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

// nopos, noposn indicate an unknown position
internal static tokenꓸPos nopos;

internal static atPos noposn = ((atPos)nopos);

// debugging/development support
internal const bool debug = false; // leave on during development

// gotypesalias controls the use of Alias types.
// As of Apr 16 2024 they are used by default.
// To disable their use, set GODEBUG to gotypesalias=0.
// This GODEBUG flag will be removed in the near future (tentatively Go 1.24).
internal static ж<godebug.Setting> gotypesalias = godebug.New("gotypesalias"u8);

// _aliasAny changes the behavior of [Scope.Lookup] for "any" in the
// [Universe] scope.
//
// This is necessary because while Alias creation is controlled by
// [Config._EnableAlias], based on the gotypealias variable, the representation
// of "any" is a global. In [Scope.Lookup], we select this global
// representation based on the result of [aliasAny], but as a result need to
// guard against this behavior changing during the type checking pass.
// Therefore we implement the following rule: any number of goroutines can type
// check concurrently with the same EnableAlias value, but if any goroutine
// tries to type check concurrently with a different EnableAlias value, we
// panic.
//
// To achieve this, _aliasAny is a state machine:
//
//	0:        no type checking is occurring
//	negative: type checking is occurring without _EnableAlias set
//	positive: type checking is occurring with _EnableAlias set
internal static ж<int32> Ꮡ_aliasAny = new StandardBox<int32>(default(int32));
internal static ref int32 _aliasAny => ref Ꮡ_aliasAny.Value;

internal static bool aliasAny() {
    @string v = gotypesalias.Value();
    var useAlias = v != "0"u8;
    var inuse = atomic.LoadInt32(Ꮡ_aliasAny);
    if (inuse != 0 && useAlias != (inuse > 0)) {
        throw panic(fmt.Sprintf("gotypealias mutated during type checking, gotypesalias=%s, inuse=%d"u8, v, inuse));
    }
    return useAlias;
}

// exprInfo stores information about an untyped expression.
[GoType] partial struct exprInfo {
    internal bool isLhs; // expression is lhs operand of a shift with delayed type-check
    internal operandMode mode;
    internal ж<Basic> typ;
    internal constant.Value val; // constant value; or nil (if not a constant)
}

// An environment represents the environment within which an object is
// type-checked.
[GoType] partial struct environment {
    internal ж<declInfo> decl;           // package-level declaration whose init expression/function body is checked
    internal ж<ΔScope> scope;              // top-most scope for lookups
    internal goVersion version;              // current accepted language version; changes across files
    internal constant.Value iota;         // value of iota in a constant declaration; nil otherwise
    internal positioner errpos;             // if set, identifier position of a constant with inherited initializer
    internal bool inTParamList;                   // set if inside a type parameter list
    internal ж<ΔSignature> sig;          // function signature if inside a function; nil otherwise
    internal map<ж<ast.CallExpr>, bool> isPanic; // set of panic call expressions (used for termination check)
    internal bool hasLabel;                   // set if a function makes use of labels (only ~1% of functions); unused outside functions
    internal bool hasCallOrRecv;                   // set if an expression contains a function call or channel receive operation
    // go/types only
    internal tokenꓸPos exprPos; // if valid, identifiers are looked up as if at position pos (used by CheckExpr, Eval)
}

// lookupScope looks up name in the current environment and if an object
// is found it returns the scope containing the object and the object.
// Otherwise it returns (nil, nil).
//
// Note that obj.Parent() may be different from the returned scope if the
// object was inserted into the scope and already had a parent at that
// time (see Scope.Insert). This can only happen for dot-imported objects
// whose parent is the scope of the package that exported them.
[GoRecv] internal static (ж<ΔScope>, Object) lookupScope(this ref environment env, @string name) {
    for (var s = env.scope; s != nil; s = s.Value.parent) {
        {
            var obj = s.Lookup(name); if (obj != default! && (!env.exprPos.IsValid() || cmpPos(obj.scopePos(), env.exprPos) <= 0)) {
                return (s, obj);
            }
        }
    }
    return (default!, default!);
}

// lookup is like lookupScope but it only returns the object (or nil).
[GoRecv] internal static Object lookup(this ref environment env, @string name) {
    var (_, obj) = env.lookupScope(name);
    return obj;
}

// An importKey identifies an imported package by import path and source directory
// (directory containing the file containing the import). In practice, the directory
// may always be the same, or may not matter. Given an (import path, directory), an
// importer must always return the same package (but given two different import paths,
// an importer may still return the same package by mapping them to the same package
// paths).
[GoType] partial struct importKey {
    internal @string path, dir;
}

// A dotImportKey describes a dot-imported object in the given scope.
[GoType] partial struct dotImportKey {
    internal ж<ΔScope> scope;
    internal @string name;
}

// An action describes a (delayed) action.
[GoType] partial struct action {
    internal goVersion version;   // applicable language version
    internal Action f;      // action to be executed
    internal ж<actionDesc> desc; // action description; may be nil, requires debug to be set
}

// If debug is set, describef sets a printf-formatted description for action a.
// Otherwise, it is a no-op.
[GoRecv] internal static void describef(this ref action a, positioner pos, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (debug) {
        a.desc = Ꮡ(new actionDesc(pos, format, args));
    }
}

// An actionDesc provides information on an action.
// For debugging only.
[GoType] partial struct actionDesc {
    internal positioner pos;
    internal @string format;
    internal slice<any> args;
}

// A Checker maintains the state of the type checker.
// It must be created with [NewChecker].
[GoType] partial struct Checker {
    // package information
    // (initialized by NewChecker, valid for the life-time of checker)
    internal ж<Config> conf;
    internal ж<Context> ctxt; // context for de-duplicating instances
    internal ж<token.FileSet> fset;
    internal ж<Package> pkg;
    public partial ref ж<ΔInfo> Info { get; }
    internal uint64 nextID;                 // unique Id for type parameters (first valid Id is 1)
    internal map<Object, ж<declInfo>> objMap; // maps package-level objects and (non-interface) methods to declaration info
    internal map<importKey, ж<Package>> impMap; // maps (import path, source directory) to (complete or fake) package
// see TODO in validtype.go
// valids instanceLookup // valid *Named (incl. instantiated) types per the validType check

    // pkgPathMap maps package names to the set of distinct import paths we've
    // seen for that name, anywhere in the import graph. It is used for
    // disambiguating package names in error messages.
    //
    // pkgPathMap is allocated lazily, so that we don't pay the price of building
    // it on the happy path. seenPkgMap tracks the packages that we've already
    // walked.
    internal map<@string, map<@string, bool>> pkgPathMap;
    internal map<ж<Package>, bool> seenPkgMap;
    // information collected during type-checking of a set of package files
    // (initialized by Files, valid only for the duration of check.Files;
    // maps and lists are allocated on demand)
    internal slice<ж<ast.File>> files;       // package files
    internal map<ж<ast.File>, @string> versions; // maps files to goVersion strings (each file has an entry); shared with Info.FileVersions if present; may be unaltered Config.GoVersion
    internal slice<ж<PkgName>> imports;        // list of imported packages
    internal map<dotImportKey, ж<PkgName>> dotImportMap; // maps dot-imported objects to the package they were dot-imported through
    internal map<ж<TypeName>, bool> brokenAliases;   // set of aliases with broken (not yet determined) types
    internal map<ж<Union>, ж<_TypeSet>> unionTypeSets; // computed type sets for union types
    internal map<ж<Var>, bool> usedVars;        // set of used variables
    internal map<ж<PkgName>, bool> usedPkgNames;    // set of used package names
    internal monoGraph mono;                 // graph for detecting non-monomorphizable instantiation loops
    internal error firstErr;                 // first error encountered
    internal map<ж<TypeName>, slice<ж<Func>>> methods; // maps package scope type names to associated non-blank (non-interface) methods
    internal map<ast.Expr, exprInfo> untyped; // map of expressions without final type
    internal slice<action> delayed;         // stack of delayed action segments; segments are processed in FIFO order
    internal slice<Object> objPath;         // path of object dependencies during type inference (for cycle reporting)
    internal slice<cleaner> cleaners;        // list of types that may need a final cleanup at the end of type-checking
    // environment within which the current object is type-checked (valid only
    // for the duration of type-checking a specific object)
    internal partial ref environment environment { get; }
    // debugging
    internal nint indent; // indentation for tracing
}

// addDeclDep adds the dependency edge (check.decl -> to) if check.decl exists
[GoRecv] internal static void addDeclDep(this ref Checker check, Object to) {
    var from = check.decl;
    if (from == nil) {
        return; // not in a package-level init expression
    }
    {
        var (_, found) = check.objMap[to, ꟷ]; if (!found) {
            return; // to is not a package-level object
        }
    }
    from.addDep(to);
}

// Note: The following three alias-related functions are only used
//       when Alias types are not enabled.

// brokenAlias records that alias doesn't have a determined type yet.
// It also sets alias.typ to Typ[Invalid].
// Not used if check.conf._EnableAlias is set.
[GoRecv] internal static void brokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) {
    ref var alias = ref Ꮡalias.DerefOrNull();

    assert(!(~check.conf)._EnableAlias);
    if (check.brokenAliases == default!) {
        check.brokenAliases = new map<ж<TypeName>, bool>();
    }
    check.brokenAliases[Ꮡalias] = true;
    alias.typ = new BasicжΔType(Typ[Invalid]);
}

// validAlias records that alias has the valid type typ (possibly Typ[Invalid]).
[GoRecv] internal static void validAlias(this ref Checker check, ж<TypeName> Ꮡalias, ΔType typ) {
    ref var alias = ref Ꮡalias.DerefOrNull();

    assert(!(~check.conf)._EnableAlias);
    delete(check.brokenAliases, Ꮡalias);
    alias.typ = typ;
}

// isBrokenAlias reports whether alias doesn't have a determined type yet.
[GoRecv] internal static bool isBrokenAlias(this ref Checker check, ж<TypeName> Ꮡalias) {
    assert(!(~check.conf)._EnableAlias);
    return check.brokenAliases[Ꮡalias];
}

[GoRecv] internal static void rememberUntyped(this ref Checker check, ast.Expr e, bool lhs, operandMode mode, ж<Basic> Ꮡtyp, constant.Value val) {
    var m = check.untyped;
    if (m == default!) {
        m = new map<ast.Expr, exprInfo>();
        check.untyped = m;
    }
    m[e] = new exprInfo(lhs, mode, Ꮡtyp, val);
}

// later pushes f on to the stack of actions that will be processed later;
// either at the end of the current statement, or in case of a local constant
// or variable declaration, before the constant or variable is in scope
// (so that f still sees the scope before any new declarations).
// later returns the pushed action so one can provide a description
// via action.describef for debugging, if desired.
[GoRecv] internal static ж<action> later(this ref Checker check, Action f) {
    nint i = len(check.delayed);
    check.delayed = append(check.delayed, new action(version: check.version, f: f));
    return Ꮡ(check.delayed, i);
}

// push pushes obj onto the object path and returns its index in the path.
[GoRecv] internal static nint push(this ref Checker check, Object obj) {
    check.objPath = append(check.objPath, obj);
    return len(check.objPath) - 1;
}

// pop pops and returns the topmost object from the object path.
[GoRecv] internal static Object pop(this ref Checker check) {
    nint i = len(check.objPath) - 1;
    var obj = check.objPath[i];
    check.objPath[i] = default!;
    check.objPath = check.objPath.slice(0, i);
    return obj;
}

[GoType] partial interface cleaner {
    void cleanup();
}

// needsCleanup records objects/types that implement the cleanup method
// which will be called at the end of type-checking.
[GoRecv] internal static void needsCleanup(this ref Checker check, cleaner c) {
    check.cleaners = append(check.cleaners, c);
}

// NewChecker returns a new [Checker] instance for a given package.
// [Package] files may be added incrementally via checker.Files.
public static ж<Checker> NewChecker(ж<Config> Ꮡconf, ж<token.FileSet> Ꮡfset, ж<Package> Ꮡpkg, ж<ΔInfo> Ꮡinfo) {
    ref var conf = ref Ꮡconf.DerefOrNull();
    ref var pkg = ref Ꮡpkg.DerefOrNull();
    ref var info = ref Ꮡinfo.DerefOrNull();

    // make sure we have a configuration
    if (Ꮡconf == nil) {
        Ꮡconf = @new<Config>(); conf = ref Ꮡconf.DerefOrNull();
    }
    // make sure we have an info struct
    if (Ꮡinfo == nil) {
        Ꮡinfo = @new<ΔInfo>(); info = ref Ꮡinfo.DerefOrNull();
    }
    // Note: clients may call NewChecker with the Unsafe package, which is
    // globally shared and must not be mutated. Therefore NewChecker must not
    // mutate *pkg.
    //
    // (previously, pkg.goVersion was mutated here: go.dev/issue/61212)
    // In go/types, conf._EnableAlias is controlled by gotypesalias.
    conf._EnableAlias = gotypesalias.Value() != "0"u8;
    return Ꮡ(new Checker(
        conf: Ꮡconf,
        ctxt: conf.Context,
        fset: Ꮡfset,
        pkg: Ꮡpkg,
        Info: Ꮡinfo,
        objMap: new map<Object, ж<declInfo>>(),
        impMap: new map<importKey, ж<Package>>(),
        usedVars: new map<ж<Var>, bool>(),
        usedPkgNames: new map<ж<PkgName>, bool>()
    ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPackageNameˢ = "invalid package name _"u8;

// initFiles initializes the files-specific portion of checker.
// The provided files must all belong to the same package.
internal static void initFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // start with a clean slate (check.Files may be called multiple times)
    // TODO(gri): what determines which fields are zeroed out here, vs at the end
    // of checkFiles?
    check.files = default!;
    check.imports = default!;
    check.dotImportMap = default!;
    check.firstErr = default!;
    check.methods = default!;
    check.untyped = default!;
    check.delayed = default!;
    check.objPath = default!;
    check.cleaners = default!;
    // We must initialize usedVars and usedPkgNames both here and in NewChecker,
    // because initFiles is not called in the CheckExpr or Eval codepaths, yet we
    // want to free this memory at the end of Files ('used' predicates are
    // only needed in the context of a given file).
    check.usedVars = new map<ж<Var>, bool>();
    check.usedPkgNames = new map<ж<PkgName>, bool>();
    // determine package name and collect valid files
    var pkg = check.pkg;
    foreach (var (_, @file) in files) {
        {
            @string name = @file.Value.Name.Value.Name;
            var exprᴛ1 = (~pkg).name;
            var matchᴛ1 = false;
            if (exprᴛ1 == ""u8) { matchᴛ1 = true;
                if (name != "_"u8){
                    pkg.Value.name = name;
                } else {
                    Ꮡcheck.error(new ast_Identжpositioner((~@file).Name), BlankPkgName, invalidPackageNameˢ);
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == name) {
                check.files = append(check.files, @file);
            }
            else if (!matchᴛ1) { /* default: */
                Ꮡcheck.errorf(((atPos)(~@file).Package), MismatchedPkgName, "package %s; expected package %s"u8, name, (~pkg).name);
            }
        }

    }
    // ignore this file
    // reuse Info.FileVersions if provided
    var versions = check.Info.Value.FileVersions;
    if (versions == default!) {
        versions = new map<ж<ast.File>, @string>();
    }
    check.versions = versions;
    goVersion pkgVersion = asGoVersion((~check.conf).GoVersion);
    if (pkgVersion.isValid() && len(files) > 0 && pkgVersion.cmp(go_current) > 0) {
        Ꮡcheck.errorf(new ast_Fileжpositioner(files[0]), TooNew, "package requires newer Go version %v (application built with %v)"u8,
            pkgVersion, go_current);
    }
    // determine Go version for each file
    foreach (var (_, @file) in check.files) {
        // use unaltered Config.GoVersion by default
        // (This version string may contain dot-release numbers as in go1.20.1,
        // unlike file versions which are Go language versions only, if valid.)
        @string v = check.conf.Value.GoVersion;
        // If the file specifies a version, use max(fileVersion, go1.21).
        {
            goVersion fileVersion = asGoVersion((~@file).GoVersion); if (fileVersion.isValid()) {
                // Go 1.21 introduced the feature of setting the go.mod
                // go line to an early version of Go and allowing //go:build lines
                // to set the Go version in a given file. Versions Go 1.21 and later
                // can be set backwards compatibly as that was the first version
                // files with go1.21 or later build tags could be built with.
                //
                // Set the version to max(fileVersion, go1.21): That will allow a
                // downgrade to a version before go1.22, where the for loop semantics
                // change was made, while being backwards compatible with versions of
                // go before the new //go:build semantics were introduced.
                v = ((@string)versionMax(fileVersion, go1_21));
                // Report a specific error for each tagged file that's too new.
                // (Normally the build system will have filtered files by version,
                // but clients can present arbitrary files to the type checker.)
                if (fileVersion.cmp(go_current) > 0) {
                    // Use position of 'package [p]' for types/types2 consistency.
                    // (Ideally we would use the //build tag itself.)
                    Ꮡcheck.errorf(new ast_Identжpositioner((~@file).Name), TooNew, "file requires newer Go version %v (application built with %v)"u8, fileVersion, go_current);
                }
            }
        }
        versions[@file] = v;
    }
}

internal static goVersion versionMax(goVersion a, goVersion b) {
    if (a.cmp(b) < 0) {
        return b;
    }
    return a;
}

// A bailout panic is used for early termination.
[GoType] partial struct bailout {
}

internal static void handleBailout(this ж<Checker> Ꮡcheck, ж<error> Ꮡerr) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();
        ref var err = ref Ꮡerr.DerefOrNull();

        var switchᴛ5 = recover();
        switch (switchᴛ5.type()) {
        case null:
        case bailout _: {
            var p = switchᴛ5;
            err = check.firstErr;
            break;
        }
        default: {
            var p = switchᴛ5;
            throw panic(p);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// normal return or early exit
// re-panic

// Files checks the provided files as part of the checker's package.
public static error /*err*/ Files(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        if (check.pkg == Unsafe) {
            // Defensive handling for Unsafe, which cannot be type checked, and must
            // not be mutated. See https://go.dev/issue/61212 for an example of where
            // Unsafe is passed to NewChecker.
            err = default!; goto ᒐdone;
        }
        // Avoid early returns here! Nearly all errors can be
        // localized to a piece of syntax and needn't prevent
        // type-checking of the rest of the package.
        defer(Ꮡcheck.handleBailout, Ꮡerr, ref ᒐ);
        Ꮡcheck.checkFiles(files);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string initFilesˢ = "== initFiles =="u8;
internal static readonly @string collectObjectsˢ = "== collectObjects =="u8;
internal static readonly @string packageObjectsˢ = "== packageObjects =="u8;
internal static readonly @string processDelayedˢ = "== processDelayed =="u8;
internal static readonly @string cleanupˢ = "== cleanup =="u8;
internal static readonly @string initOrderˢ = "== initOrder =="u8;
internal static readonly @string unusedImportsˢ = "== unusedImports =="u8;
internal static readonly @string recordUntypedˢ = "== recordUntyped =="u8;

// checkFiles type-checks the specified files. Errors are reported as
// a side effect, not by returning early, to ensure that well-formed
// syntax is properly type annotated even in a package containing
// errors.
internal static void checkFiles(this ж<Checker> Ꮡcheck, slice<ж<ast.File>> files) {
    GoFrame ᒐ = default;
    try {
        ref var check = ref Ꮡcheck.DerefOrNull();

        // Ensure that _EnableAlias is consistent among concurrent type checking
        // operations. See the documentation of [_aliasAny] for details.
        if ((~check.conf)._EnableAlias){
            if (atomic.AddInt32(Ꮡ_aliasAny, 1) <= 0) {
                throw panic("EnableAlias set while !EnableAlias type checking is ongoing");
            }
            defer(atomic.AddInt32, Ꮡ_aliasAny, (int32)(-1), ref ᒐ);
        } else {
            if (atomic.AddInt32(Ꮡ_aliasAny, -1) >= 0) {
                throw panic("!EnableAlias set while EnableAlias type checking is ongoing");
            }
            defer(atomic.AddInt32, Ꮡ_aliasAny, (int32)(1), ref ᒐ);
        }
        void print(@string msg) {
            if ((~Ꮡcheck.Value.conf)._Trace) {
                fmt.Println();
                fmt.Println(msg);
            }
        }
        print(initFilesˢ);
        Ꮡcheck.initFiles(files);
        print(collectObjectsˢ);
        Ꮡcheck.collectObjects();
        print(packageObjectsˢ);
        Ꮡcheck.packageObjects();
        print(processDelayedˢ);
        Ꮡcheck.processDelayed(0); // incl. all functions
        print(cleanupˢ);
        check.cleanup();
        print(initOrderˢ);
        Ꮡcheck.initOrder();
        if (!(~check.conf).DisableUnusedImportCheck) {
            print(unusedImportsˢ);
            Ꮡcheck.unusedImports();
        }
        print(recordUntypedˢ);
        Ꮡcheck.recordUntyped();
        if (check.firstErr == default!) {
            // TODO(mdempsky): Ensure monomorph is safe when errors exist.
            Ꮡcheck.monomorph();
        }
        check.pkg.Value.goVersion = check.conf.Value.GoVersion;
        check.pkg.Value.complete = true;
        // no longer needed - release memory
        check.imports = default!;
        check.dotImportMap = default!;
        check.pkgPathMap = default!;
        check.seenPkgMap = default!;
        check.brokenAliases = default!;
        check.unionTypeSets = default!;
        check.usedVars = default!;
        check.usedPkgNames = default!;
        check.ctxt = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string delayedPˢ = "-- delayed %p"u8;

// TODO(gri): shouldn't the cleanup above occur after the bailout?
// TODO(gri) There's more memory we should release at this point.

// processDelayed processes all delayed actions pushed after top.
internal static void processDelayed(this ж<Checker> Ꮡcheck, nint top) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    // If each delayed action pushes a new action, the
    // stack will continue to grow during this loop.
    // However, it is only processing functions (which
    // are processed in a delayed fashion) that may
    // add more actions (such as nested functions), so
    // this is a sufficiently bounded process.
    goVersion savedVersion = check.version;
    for (nint i = top; i < len(check.delayed); i++) {
        var a = Ꮡ(check.delayed, i);
        if ((~check.conf)._Trace) {
            if ((~a).desc != nil){
                Ꮡcheck.trace((~(~a).desc).pos.Pos(), "-- "u8 + (~(~a).desc).format, (~(~a).desc).args.ꓸꓸꓸ);
            } else {
                Ꮡcheck.trace(nopos, delayedPˢ, ((~a).f).OrTypedNilFunc());
            }
        }
        check.version = a.Value.version; // reestablish the effective Go version captured earlier
        (~a).f(); // may append to check.delayed
        if ((~check.conf)._Trace) {
            fmt.Println();
        }
    }
    assert(top <= len(check.delayed)); // stack must not have shrunk
    check.delayed = check.delayed.slice(0, top);
    check.version = savedVersion;
}

// cleanup runs cleanup for all collected cleaners.
[GoRecv] internal static void cleanup(this ref Checker check) {
    // Don't use a range clause since Named.cleanup may add more cleaners.
    for (nint i = 0; i < len(check.cleaners); i++) {
        check.cleaners[i].cleanup();
    }
    check.cleaners = default!;
}

// go/types doesn't support recording of types directly in the AST.
// dummy function to match types2 code.
[GoRecv] internal static void recordTypeAndValueInSyntax(this ref Checker check, ast.Expr x, operandMode mode, ΔType typ, constant.Value val) {
}

// nothing to do

// go/types doesn't support recording of types directly in the AST.
// dummy function to match types2 code.
[GoRecv] internal static void recordCommaOkTypesInSyntax(this ref Checker check, ast.Expr x, ΔType t0, ΔType t1) {
}

// nothing to do

// instantiatedIdent determines the identifier of the type instantiated in expr.
// Helper function for recordInstance in recording.go.
internal static ж<ast.Ident> instantiatedIdent(ast.Expr expr) {
    ast.Expr selOrIdent = default!;
    switch (expr.type()) {
    case ж<ast.IndexExpr> e: {
        selOrIdent = e.Value.X;
        break;
    }
    case ж<ast.IndexListExpr> e: {
        selOrIdent = e.Value.X;
        break;
    }
    case ж<ast.SelectorExpr> _:
    case ж<ast.Ident> _: {
        var e = expr;
        selOrIdent = e;
        break;
    }}
    // only exists in go/ast, not syntax
    switch (selOrIdent.type()) {
    case ж<ast.Ident> x: {
        return x;
    }
    case ж<ast.SelectorExpr> x: {
        return (~x).Sel;
    }}
    // extra debugging of go.dev/issue/63933
    throw panic(sprintf(nil, default!, true, "instantiated ident not found; please report: %s"u8, expr));
}

} // end types_package
