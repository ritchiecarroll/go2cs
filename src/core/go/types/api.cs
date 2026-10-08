// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package types declares the data types and implements
// the algorithms for type-checking of Go packages. Use
// [Config.Check] to invoke the type checker for a package.
// Alternatively, create a new type checker with [NewChecker]
// and invoke it incrementally by calling [Checker.Files].
//
// Type-checking consists of several interdependent phases:
//
// Name resolution maps each identifier ([ast.Ident]) in the program
// to the symbol ([Object]) it denotes. Use the Defs and Uses fields
// of [Info] or the [Info.ObjectOf] method to find the symbol for an
// identifier, and use the Implicits field of [Info] to find the
// symbol for certain other kinds of syntax node.
//
// Constant folding computes the exact constant value
// ([constant.Value]) of every expression ([ast.Expr]) that is a
// compile-time constant. Use the Types field of [Info] to find the
// results of constant folding for an expression.
//
// Type deduction computes the type ([Type]) of every expression
// ([ast.Expr]) and checks for compliance with the language
// specification. Use the Types field of [Info] for the results of
// type deduction.
//
// For a tutorial, see https://go.dev/s/types-tutorial.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// An Error describes a type-checking error; it implements the error interface.
// A "soft" error is an error that still permits a valid interpretation of a
// package (such as "unused variable"); "hard" errors may lead to unpredictable
// behavior if ignored.
partial struct ΔError {
    public ж<token.FileSet> Fset; // file set for interpretation of Pos
    public tokenꓸPos Pos;      // error position
    public @string Msg;        // error message
    public bool Soft;           // if set, error is "soft"
    // go116code is a future API, unexported as the set of error codes is large
    // and likely to change significantly during experimentation. Tools wishing
    // to preview this feature may read go116code using reflection (see
    // errorcodes_test.go), but beware that there is no guarantee of future
    // compatibility.
    internal errors.Code go116code;
    internal tokenꓸPos go116start;
    internal tokenꓸPos go116end;
}

// Error returns an error string formatted as follows:
// filename:line:column: message
public static @string Error(this ΔError err) {
    return fmt.Sprintf("%s: %s"u8, err.Fset.Position(err.Pos), err.Msg);
}

// An ArgumentError holds an error associated with an argument index.
partial struct ArgumentError {
    public nint Index;
    public error Err;
}

public static @string Error(this ref ArgumentError e) {
    return e.Err.Error();
}

public static error Unwrap(this ref ArgumentError e) {
    return e.Err;
}

// An Importer resolves import paths to Packages.
//
// CAUTION: This interface does not support the import of locally
// vendored packages. See https://golang.org/s/go15vendor.
// If possible, external implementations should implement [ImporterFrom].
partial interface Importer {
    // Import returns the imported package for the given import path.
    // The semantics is like for ImporterFrom.ImportFrom except that
    // dir and mode are ignored (since they are not present).
    (ж<Package>, error) Import(@string path);
}

partial struct ImportMode /*num:nint*/;

// An ImporterFrom resolves import paths to packages; it
// supports vendoring per https://golang.org/s/go15vendor.
// Use go/importer to obtain an ImporterFrom implementation.
partial interface ImporterFrom :
    Importer
{
    // ImportFrom returns the imported package for the given import
    // path when imported by a package file located in dir.
    // If the import failed, besides returning an error, ImportFrom
    // is encouraged to cache and return a package anyway, if one
    // was created. This will reduce package inconsistencies and
    // follow-on type checker errors due to the missing package.
    // The mode value must be 0; it is reserved for future use.
    // Two calls to ImportFrom with the same path and dir must
    // return the same package.
    (ж<Package>, error) ImportFrom(@string path, @string dir, ImportMode mode);
}

// A Config specifies the configuration for type checking.
// The zero value for Config is a ready-to-use default configuration.
partial struct Config {
    // Context is the context used for resolving global identifiers. If nil, the
    // type checker will initialize this field with a newly created context.
    public ж<Context> Context;
    // GoVersion describes the accepted Go language version. The string must
    // start with a prefix of the form "go%d.%d" (e.g. "go1.20", "go1.21rc1", or
    // "go1.21.0") or it must be empty; an empty string disables Go language
    // version checks. If the format is invalid, invoking the type checker will
    // result in an error.
    public @string GoVersion;
    // If IgnoreFuncBodies is set, function bodies are not
    // type-checked.
    public bool IgnoreFuncBodies;
    // If FakeImportC is set, `import "C"` (for packages requiring Cgo)
    // declares an empty "C" package and errors are omitted for qualified
    // identifiers referring to package C (which won't find an object).
    // This feature is intended for the standard library cmd/api tool.
    //
    // Caution: Effects may be unpredictable due to follow-on errors.
    //          Do not use casually!
    public bool FakeImportC;
    // If go115UsesCgo is set, the type checker expects the
    // _cgo_gotypes.go file generated by running cmd/cgo to be
    // provided as a package source file. Qualified identifiers
    // referring to package C will be resolved to cgo-provided
    // declarations within _cgo_gotypes.go.
    //
    // It is an error to set both FakeImportC and go115UsesCgo.
    internal bool go115UsesCgo;
    // If _Trace is set, a debug trace is printed to stdout.
    internal bool _Trace;
    // If Error != nil, it is called with each error found
    // during type checking; err has dynamic type Error.
    // Secondary errors (for instance, to enumerate all types
    // involved in an invalid recursive type declaration) have
    // error strings that start with a '\t' character.
    // If Error == nil, type-checking stops with the first
    // error found.
    public Action<error> Error;
    // An importer is used to import packages referred to from
    // import declarations.
    // If the installed importer implements ImporterFrom, the type
    // checker calls ImportFrom instead of Import.
    // The type checker reports an error if an importer is needed
    // but none was installed.
    public Importer Importer;
    // If Sizes != nil, it provides the sizing functions for package unsafe.
    // Otherwise SizesFor("gc", "amd64") is used instead.
    public Sizes Sizes;
    // If DisableUnusedImportCheck is set, packages are not checked
    // for unused imports.
    public bool DisableUnusedImportCheck;
    // If a non-empty _ErrorURL format string is provided, it is used
    // to format an error URL link that is appended to the first line
    // of an error message. ErrorURL must be a format string containing
    // exactly one "%s" format, e.g. "[go.dev/e/%s]".
    internal @string _ErrorURL;
    // If EnableAlias is set, alias declarations produce an Alias type. Otherwise
    // the alias information is only in the type name, which points directly to
    // the actual (aliased) type.
    //
    // This setting must not differ among concurrent type-checking operations,
    // since it affects the behavior of Universe.Lookup("any").
    //
    // This flag will eventually be removed (with Go 1.24 at the earliest).
    internal bool _EnableAlias;
}

// Linkname for use from srcimporter.
//go:linkname srcimporter_setUsesCgo
public static void srcimporter_setUsesCgo(ж<Config> Ꮡconf) {
    ref var conf = ref Ꮡconf.DerefOrNull();

    conf.go115UsesCgo = true;
}

// Info holds result type information for a type-checked package.
// Only the information for which a map is provided is collected.
// If the package has type errors, the collected information may
// be incomplete.
partial struct ΔInfo {
    // Types maps expressions to their types, and for constant
    // expressions, also their values. Invalid expressions are
    // omitted.
    //
    // For (possibly parenthesized) identifiers denoting built-in
    // functions, the recorded signatures are call-site specific:
    // if the call result is not a constant, the recorded type is
    // an argument-specific signature. Otherwise, the recorded type
    // is invalid.
    //
    // The Types map does not record the type of every identifier,
    // only those that appear where an arbitrary expression is
    // permitted. For instance:
    // - an identifier f in a selector expression x.f is found
    //   only in the Selections map;
    // - an identifier z in a variable declaration 'var z int'
    //   is found only in the Defs map;
    // - an identifier p denoting a package in a qualified
    //   identifier p.X is found only in the Uses map.
    //
    // Similarly, no type is recorded for the (synthetic) FuncType
    // node in a FuncDecl.Type field, since there is no corresponding
    // syntactic function type expression in the source in this case
    // Instead, the function type is found in the Defs.map entry for
    // the corresponding function declaration.
    public map<ast.Expr, TypeAndValue> Types;
    // Instances maps identifiers denoting generic types or functions to their
    // type arguments and instantiated type.
    //
    // For example, Instances will map the identifier for 'T' in the type
    // instantiation T[int, string] to the type arguments [int, string] and
    // resulting instantiated *Named type. Given a generic function
    // func F[A any](A), Instances will map the identifier for 'F' in the call
    // expression F(int(1)) to the inferred type arguments [int], and resulting
    // instantiated *Signature.
    //
    // Invariant: Instantiating Uses[id].Type() with Instances[id].TypeArgs
    // results in an equivalent of Instances[id].Type.
    public map<ж<ast.Ident>, Instance> Instances;
    // Defs maps identifiers to the objects they define (including
    // package names, dots "." of dot-imports, and blank "_" identifiers).
    // For identifiers that do not denote objects (e.g., the package name
    // in package clauses, or symbolic variables t in t := x.(type) of
    // type switch headers), the corresponding objects are nil.
    //
    // For an embedded field, Defs returns the field *Var it defines.
    //
    // Invariant: Defs[id] == nil || Defs[id].Pos() == id.Pos()
    public map<ж<ast.Ident>, Object> Defs;
    // Uses maps identifiers to the objects they denote.
    //
    // For an embedded field, Uses returns the *TypeName it denotes.
    //
    // Invariant: Uses[id].Pos() != id.Pos()
    public map<ж<ast.Ident>, Object> Uses;
    // Implicits maps nodes to their implicitly declared objects, if any.
    // The following node and object types may appear:
    //
    //     node               declared object
    //
    //     *ast.ImportSpec    *PkgName for imports without renames
    //     *ast.CaseClause    type-specific *Var for each type switch case clause (incl. default)
    //     *ast.Field         anonymous parameter *Var (incl. unnamed results)
    //
    public map<ast.Node, Object> Implicits;
    // Selections maps selector expressions (excluding qualified identifiers)
    // to their corresponding selections.
    public map<ж<ast.SelectorExpr>, ж<Selection>> Selections;
    // Scopes maps ast.Nodes to the scopes they define. Package scopes are not
    // associated with a specific node but with all files belonging to a package.
    // Thus, the package scope can be found in the type-checked Package object.
    // Scopes nest, with the Universe scope being the outermost scope, enclosing
    // the package scope, which contains (one or more) files scopes, which enclose
    // function scopes which in turn enclose statement and function literal scopes.
    // Note that even though package-level functions are declared in the package
    // scope, the function scopes are embedded in the file scope of the file
    // containing the function declaration.
    //
    // The Scope of a function contains the declarations of any
    // type parameters, parameters, and named results, plus any
    // local declarations in the body block.
    // It is coextensive with the complete extent of the
    // function's syntax ([*ast.FuncDecl] or [*ast.FuncLit]).
    // The Scopes mapping does not contain an entry for the
    // function body ([*ast.BlockStmt]); the function's scope is
    // associated with the [*ast.FuncType].
    //
    // The following node types may appear in Scopes:
    //
    //     *ast.File
    //     *ast.FuncType
    //     *ast.TypeSpec
    //     *ast.BlockStmt
    //     *ast.IfStmt
    //     *ast.SwitchStmt
    //     *ast.TypeSwitchStmt
    //     *ast.CaseClause
    //     *ast.CommClause
    //     *ast.ForStmt
    //     *ast.RangeStmt
    //
    public map<ast.Node, ж<ΔScope>> Scopes;
    // InitOrder is the list of package-level initializers in the order in which
    // they must be executed. Initializers referring to variables related by an
    // initialization dependency appear in topological order, the others appear
    // in source order. Variables without an initialization expression do not
    // appear in this list.
    public slice<ж<Initializer>> InitOrder;
    // FileVersions maps a file to its Go version string.
    // If the file doesn't specify a version, the reported
    // string is Config.GoVersion.
    // Version strings begin with “go”, like “go1.21”, and
    // are suitable for use with the [go/version] package.
    public map<ж<ast.File>, @string> FileVersions;
}

internal static bool recordTypes(this ref ΔInfo info) {
    return info.Types != default!;
}

// TypeOf returns the type of expression e, or nil if not found.
// Precondition: the Types, Uses and Defs maps are populated.
public static ΔType TypeOf(this ref ΔInfo info, ast.Expr e) {
    {
        var (t, ok) = info.Types[e, ꟷ]; if (ok) {
            return t.Type;
        }
    }
    {
        var (id, _) = e._<ж<ast.Ident>>(ᐧ); if (id != nil) {
            {
                var obj = info.ObjectOf(id); if (obj != default!) {
                    return obj.Type();
                }
            }
        }
    }
    return default!;
}

// ObjectOf returns the object denoted by the specified id,
// or nil if not found.
//
// If id is an embedded struct field, [Info.ObjectOf] returns the field (*[Var])
// it defines, not the type (*[TypeName]) it uses.
//
// Precondition: the Uses and Defs maps are populated.
public static Object ObjectOf(this ref ΔInfo info, ж<ast.Ident> Ꮡid) {
    {
        var obj = info.Defs[Ꮡid]; if (obj != default!) {
            return obj;
        }
    }
    return info.Uses[Ꮡid];
}

// PkgNameOf returns the local package name defined by the import,
// or nil if not found.
//
// For dot-imports, the package name is ".".
//
// Precondition: the Defs and Implicts maps are populated.
public static ж<PkgName> PkgNameOf(this ref ΔInfo info, ж<ast.ImportSpec> Ꮡimp) {
    ref var imp = ref Ꮡimp.DerefOrNull();

    Object obj = default!;
    if (imp.Name != nil){
        obj = info.Defs[imp.Name];
    } else {
        obj = info.Implicits[new ast.ImportSpecжNode(Ꮡimp)];
    }
    var (pkgname, _) = obj._<ж<PkgName>>(ᐧ);
    return pkgname;
}

// TypeAndValue reports the type and value (for constants)
// of the corresponding expression.
partial struct TypeAndValue {
    internal operandMode mode;
    public ΔType Type;
    public constant.Value Value;
}

// IsVoid reports whether the corresponding expression
// is a function call without results.
public static bool IsVoid(this TypeAndValue tv) {
    return tv.mode == novalue;
}

// IsType reports whether the corresponding expression specifies a type.
public static bool IsType(this TypeAndValue tv) {
    return tv.mode == typexpr;
}

// IsBuiltin reports whether the corresponding expression denotes
// a (possibly parenthesized) built-in function.
public static bool IsBuiltin(this TypeAndValue tv) {
    return tv.mode == Δbuiltinᴛ;
}

// IsValue reports whether the corresponding expression is a value.
// Builtins are not considered values. Constant values have a non-
// nil Value.
public static bool IsValue(this TypeAndValue tv) {
    var exprᴛ1 = tv.mode;
    if (exprᴛ1 == constant_ || exprᴛ1 == variable || exprᴛ1 == mapindex || exprᴛ1 == value || exprᴛ1 == commaok || exprᴛ1 == commaerr) {
        return true;
    }

    return false;
}

// IsNil reports whether the corresponding expression denotes the
// predeclared value nil.
public static bool IsNil(this TypeAndValue tv) {
    return tv.mode == value && AreEqual(tv.Type, Typ[UntypedNil]);
}

// Addressable reports whether the corresponding expression
// is addressable (https://golang.org/ref/spec#Address_operators).
public static bool Addressable(this TypeAndValue tv) {
    return tv.mode == variable;
}

// Assignable reports whether the corresponding expression
// is assignable to (provided a value of the right type).
public static bool Assignable(this TypeAndValue tv) {
    return tv.mode == variable || tv.mode == mapindex;
}

// HasOk reports whether the corresponding expression may be
// used on the rhs of a comma-ok assignment.
public static bool HasOk(this TypeAndValue tv) {
    return tv.mode == commaok || tv.mode == mapindex;
}

// Instance reports the type arguments and instantiated type for type and
// function instantiations. For type instantiations, [Type] will be of dynamic
// type *[Named]. For function instantiations, [Type] will be of dynamic type
// *Signature.
partial struct Instance {
    public ж<TypeList> TypeArgs;
    public ΔType Type;
}

// An Initializer describes a package-level variable, or a list of variables in case
// of a multi-valued initialization expression, and the corresponding initialization
// expression.
partial struct Initializer {
    public slice<ж<Var>> Lhs; // var Lhs = Rhs
    public ast.Expr Rhs;
}

public static @string String(this ref Initializer init) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, lhs) in init.Lhs) {
        if (i > 0) {
            buf.WriteString(", "u8);
        }
        buf.WriteString(lhs.of(Var.Ꮡobject).Name());
    }
    buf.WriteString(" = "u8);
    WriteExpr(Ꮡbuf, init.Rhs);
    return Ꮡbuf.String();
}

// Check type-checks a package and returns the resulting package object and
// the first error if any. Additionally, if info != nil, Check populates each
// of the non-nil maps in the [Info] struct.
//
// The package is marked as complete if no errors occurred, otherwise it is
// incomplete. See [Config.Error] for controlling behavior in the presence of
// errors.
//
// The package is specified by a list of *ast.Files and corresponding
// file set, and the package path the package is identified with.
// The clean path must not be empty or dot (".").
public static (ж<Package>, error) Check(this ж<Config> Ꮡconf, @string path, ж<token.FileSet> Ꮡfset, slice<ж<ast.File>> files, ж<ΔInfo> Ꮡinfo) {
    var pkg = NewPackage(path, ""u8);
    return (pkg, NewChecker(Ꮡconf, Ꮡfset, pkg, Ꮡinfo).Files(files));
}

} // end types_package
