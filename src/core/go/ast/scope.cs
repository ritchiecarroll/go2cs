// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements scopes and the objects they contain.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using io = io_package;

partial class ast_package {

// A Scope maintains the set of named language entities declared
// in the scope and a link to the immediately surrounding (outer)
// scope.
//
// Deprecated: use the type checker [go/types] instead; see [Object].
partial struct Scope {
    public ж<Scope> Outer;
    public map<@string, ж<Object>> Objects;
}

// NewScope creates a new scope nested in the outer scope.
public static ж<Scope> NewScope(ж<Scope> Ꮡouter) {
    const nint n = 4; // initial scope capacity
    return Ꮡ(new Scope(Ꮡouter, new map<@string, ж<Object>>(n)));
}

// Lookup returns the object with the given name if it is
// found in scope s, otherwise it returns nil. Outer scopes
// are ignored.
public static ж<Object> Lookup(this ref Scope s, @string name) {
    return s.Objects[name];
}

// Insert attempts to insert a named object obj into the scope s.
// If the scope already contains an object alt with the same name,
// Insert leaves the scope unchanged and returns alt. Otherwise
// it inserts obj and returns nil.
public static ж<Object> /*alt*/ Insert(this ref Scope s, ж<Object> Ꮡobj) {
    ж<Object> alt = default!;

    ref var obj = ref Ꮡobj.DerefOrNull();
    {
        alt = s.Objects[obj.Name]; if (alt == nil) {
            s.Objects[obj.Name] = Ꮡobj;
        }
    }
    return alt;
}

// Debugging support
public static @string String(this ж<Scope> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "scope %p {"u8, Ꮡs.OrTypedNil());
    if (Ꮡs != nil && len(s.Objects) > 0) {
        fmt.Fprintln(new strings_BuilderжWriter(Ꮡbuf));
        foreach (var (_, obj) in s.Objects) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "\t%s %s\n"u8, (~obj).Kind, (~obj).Name);
        }
    }
    fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "}\n"u8);
    return buf.String();
}

// ----------------------------------------------------------------------------
// Objects

// An Object describes a named language entity such as a package,
// constant, type, variable, function (incl. methods), or label.
//
// The Data fields contains object-specific data:
//
//	Kind    Data type         Data value
//	Pkg     *Scope            package scope
//	Con     int               iota for the respective declaration
//
// Deprecated: The relationship between Idents and Objects cannot be
// correctly computed without type information. For example, the
// expression T{K: 0} may denote a struct, map, slice, or array
// literal, depending on the type of T. If T is a struct, then K
// refers to a field of T, whereas for the other types it refers to a
// value in the environment.
//
// New programs should set the [parser.SkipObjectResolution] parser
// flag to disable syntactic object resolution (which also saves CPU
// and memory), and instead use the type checker [go/types] if object
// resolution is desired. See the Defs, Uses, and Implicits fields of
// the [types.Info] struct for details.
partial struct Object {
    public ObjKind Kind;
    public @string Name; // declared name
    public any Decl;    // corresponding Field, XxxSpec, FuncDecl, LabeledStmt, AssignStmt, Scope; or nil
    public any Data;    // object-specific data; or nil
    public any Type;    // placeholder for type information; may be nil
}

// NewObj creates a new object of a given kind and name.
public static ж<Object> NewObj(ObjKind kind, @string name) {
    return Ꮡ(new Object(Kind: kind, Name: name));
}

// Pos computes the source position of the declaration of an object name.
// The result may be an invalid position if it cannot be computed
// (obj.Decl may be nil or not correct).
public static tokenꓸPos Pos(this ref Object obj) {
    @string name = obj.Name;
    switch (obj.Decl.type()) {
    case ж<Field> d: {
        foreach (var (_, n) in (~d).Names) {
            if ((~n).Name == name) {
                return n.Pos();
            }
        }
        break;
    }
    case ж<ImportSpec> d: {
        if ((~d).Name != nil && (~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        return (~d).Path.Pos();
    }
    case ж<ValueSpec> d: {
        foreach (var (_, n) in (~d).Names) {
            if ((~n).Name == name) {
                return n.Pos();
            }
        }
        break;
    }
    case ж<TypeSpec> d: {
        if ((~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        break;
    }
    case ж<FuncDecl> d: {
        if ((~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        break;
    }
    case ж<LabeledStmt> d: {
        if ((~(~d).Label).Name == name) {
            return (~d).Label.Pos();
        }
        break;
    }
    case ж<AssignStmt> d: {
        foreach (var (_, x) in (~d).Lhs) {
            {
                var (ident, isIdent) = x._<ж<Ident>>(ᐧ); if (isIdent && (~ident).Name == name) {
                    return ident.Pos();
                }
            }
        }
        break;
    }
    case ж<Scope> d: {
        break;
    }}
    // predeclared object - nothing to do for now
    return token.NoPos;
}

partial struct ObjKind /*num:nint*/;

// The list of possible [Object] kinds.
public static ObjKind Bad => /* iota */ 0;  // for error handling

public static ObjKind Pkg => 1;  // package

public static ObjKind Con => 2;  // constant

public static ObjKind Typ => 3;  // type

public static ObjKind Var => 4;  // variable

public static ObjKind Fun => 5;  // function or method

public static ObjKind Lbl => 6;  // label

internal static array<@string> objKindStrings = new golib.SparseArray<@string>{
    [(int)Bad] = "bad"u8,
    [(int)Pkg] = "package"u8,
    [(int)Con] = "const"u8,
    [(int)Typ] = "type"u8,
    [(int)Var] = "var"u8,
    [(int)Fun] = "func"u8,
    [(int)Lbl] = "label"u8
}.array();

public static @string String(this ObjKind kind) {
    return objKindStrings[kind];
}

} // end ast_package
