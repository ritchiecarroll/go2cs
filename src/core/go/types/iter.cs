// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using iter = iter_package;
using static types_package;

partial class types_package {

} partial class typesᴛInterface {

// This file defines go1.23 iterator methods for a variety of data
// types. They are not mirrored to cmd/compile/internal/types2, as
// there is no point doing so until the bootstrap compiler it at least
// go1.23; therefore go1.23-style range statements should not be used
// in code common to types and types2, though clients of go/types are
// free to use them.

// Methods returns a go1.23 iterator over all the methods of an
// interface, ordered by Id.
//
// Example: for m := range t.Methods() { ... }
public static iter.Seq<ж<Func>> Methods(this ж<Interface> Ꮡt) {
    return (Func<ж<Func>, bool> yield) => {
        foreach (var i in range(Ꮡt.NumMethods())) {
            if (!yield(Ꮡt.Method(i))) {
                break;
            }
        }
    };
}

// ExplicitMethods returns a go1.23 iterator over the explicit methods of
// an interface, ordered by Id.
//
// Example: for m := range t.ExplicitMethods() { ... }
public static iter.Seq<ж<Func>> ExplicitMethods(this ж<Interface> Ꮡt) {
    return (Func<ж<Func>, bool> yield) => {
        foreach (var i in range(Ꮡt.Value.NumExplicitMethods())) {
            if (!yield(Ꮡt.Value.ExplicitMethod(i))) {
                break;
            }
        }
    };
}

// EmbeddedTypes returns a go1.23 iterator over the types embedded within an interface.
//
// Example: for e := range t.EmbeddedTypes() { ... }
public static iter.Seq<Type> EmbeddedTypes(this ж<Interface> Ꮡt) {
    return (Func<Type, bool> yield) => {
        foreach (var i in range(Ꮡt.Value.NumEmbeddeds())) {
            if (!yield(Ꮡt.Value.EmbeddedType(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛNamed {

// Methods returns a go1.23 iterator over the declared methods of a named type.
//
// Example: for m := range t.Methods() { ... }
public static iter.Seq<ж<Func>> Methods(this ж<Named> Ꮡt) {
    return (Func<ж<Func>, bool> yield) => {
        foreach (var i in range(Ꮡt.NumMethods())) {
            if (!yield(Ꮡt.Method(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛScope {

// Children returns a go1.23 iterator over the child scopes nested within scope s.
//
// Example: for child := range scope.Children() { ... }
public static iter.Seq<ж<Scope>> Children(this ж<Scope> Ꮡs) {
    return (Func<ж<Scope>, bool> yield) => {
        foreach (var i in range(Ꮡs.Value.NumChildren())) {
            if (!yield(Ꮡs.Value.Child(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛStruct {

// Fields returns a go1.23 iterator over the fields of a struct type.
//
// Example: for field := range s.Fields() { ... }
public static iter.Seq<ж<Var>> Fields(this ж<Struct> Ꮡs) {
    return (Func<ж<Var>, bool> yield) => {
        foreach (var i in range(Ꮡs.Value.NumFields())) {
            if (!yield(Ꮡs.Value.Field(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛTuple {

// Variables returns a go1.23 iterator over the variables of a tuple type.
//
// Example: for v := range tuple.Variables() { ... }
public static iter.Seq<ж<Var>> Variables(this ж<Tuple> Ꮡt) {
    return (Func<ж<Var>, bool> yield) => {
        foreach (var i in range(Ꮡt.Len())) {
            if (!yield(Ꮡt.Value.At(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛMethodSet {

// Methods returns a go1.23 iterator over the methods of a method set.
//
// Example: for method := range s.Methods() { ... }
public static iter.Seq<ж<Selection>> Methods(this ж<MethodSet> Ꮡs) {
    return (Func<ж<Selection>, bool> yield) => {
        foreach (var i in range(Ꮡs.Value.Len())) {
            if (!yield(Ꮡs.Value.At(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛUnion {

// Terms returns a go1.23 iterator over the terms of a union.
//
// Example: for term := range union.Terms() { ... }
public static iter.Seq<ж<Term>> Terms(this ж<Union> Ꮡu) {
    return (Func<ж<Term>, bool> yield) => {
        foreach (var i in range(Ꮡu.Value.Len())) {
            if (!yield(Ꮡu.Value.Term(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛTypeParamList {

// TypeParams returns a go1.23 iterator over a list of type parameters.
//
// Example: for tparam := range l.TypeParams() { ... }
public static iter.Seq<ж<TypeParam>> TypeParams(this ж<TypeParamList> Ꮡl) {
    return (Func<ж<TypeParam>, bool> yield) => {
        foreach (var i in range(Ꮡl.Len())) {
            if (!yield(Ꮡl.Value.At(i))) {
                break;
            }
        }
    };
}

} partial class typesᴛTypeList {

// Types returns a go1.23 iterator over the elements of a list of types.
//
// Example: for t := range l.Types() { ... }
public static iter.Seq<Type> Types(this ж<TypeList> Ꮡl) {
    return (Func<Type, bool> yield) => {
        foreach (var i in range(Ꮡl.Len())) {
            if (!yield(Ꮡl.Value.At(i))) {
                break;
            }
        }
    };
}

} // end typesᴛTypeList
