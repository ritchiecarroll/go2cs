// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static @internal.types.errors_package;
using errors = @internal.types.errors_package;
using global::go.go;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// An Interface represents an interface type.
partial struct Interface {
    internal ж<Checker> check;  // for error reporting; nil once type set is computed
    internal slice<ж<Func>> methods; // ordered list of explicitly declared methods
    internal slice<ΔType> embeddeds;  // ordered list of explicitly embedded elements
    internal ж<slice<tokenꓸPos>> embedPos; // positions of embedded elements; or nil (for error messages) - use pointer to save space
    internal bool @implicit;         // interface is wrapper for type set literal (non-interface T, ~T, or A|B)
    internal bool complete;         // indicates that obj, methods, and embeddeds are set and type set can be computed
    internal ж<_TypeSet> tset; // type set described by this interface, computed lazily
}

// typeSet returns the type set for interface t.
internal static ж<_TypeSet> typeSet(this ж<Interface> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return computeInterfaceTypeSet(t.check, nopos, Ꮡt);
}

// emptyInterface represents the empty (completed) interface
internal static ж<Interface> ᏑemptyInterface = new StandardBox<Interface>(default(Interface));
internal static ref Interface emptyInterface => ref ᏑemptyInterface.Value;
internal static void initᴛemptyInterface() { emptyInterface = new Interface(complete: true, tset: ᏑtopTypeSet); }

// NewInterface returns a new interface for the given methods and embedded types.
// NewInterface takes ownership of the provided methods and may modify their types
// by setting missing receivers.
//
// Deprecated: Use NewInterfaceType instead which allows arbitrary embedded types.
public static ж<Interface> NewInterface(slice<ж<Func>> methods, slice<ж<Named>> embeddeds) {
    var tnames = new slice<ΔType>(len(embeddeds));
    foreach (var (i, t) in embeddeds) {
        tnames[i] = new NamedжΔType(t);
    }
    return NewInterfaceType(methods, tnames);
}

// NewInterfaceType returns a new interface for the given methods and embedded
// types. NewInterfaceType takes ownership of the provided methods and may
// modify their types by setting missing receivers.
//
// To avoid race conditions, the interface's type set should be computed before
// concurrent use of the interface, by explicitly calling Complete.
public static ж<Interface> NewInterfaceType(slice<ж<Func>> methods, slice<ΔType> embeddeds) {
    if (len(methods) == 0 && len(embeddeds) == 0) {
        return ᏑemptyInterface;
    }
    // set method receivers if necessary
    var typ = (((ж<Checker>)nil)).newInterface();
    foreach (var (_, m) in methods) {
        {
            var sig = (~m).typ._<ж<ΔSignature>>(); if ((~sig).recv == nil) {
                sig.Value.recv = NewVar((~m).pos, (~m).pkg, ""u8, new InterfaceжΔType(typ));
            }
        }
    }
    // sort for API stability
    sortMethods(methods);
    typ.Value.methods = methods;
    typ.Value.embeddeds = embeddeds;
    typ.Value.complete = true;
    return typ;
}

// check may be nil
internal static ж<Interface> newInterface(this ж<Checker> Ꮡcheck) {
    ref var check = ref Ꮡcheck.DerefOrNull();

    var typ = Ꮡ(new Interface(check: Ꮡcheck));
    if (Ꮡcheck != nil) {
        check.needsCleanup(new Interfaceжcleaner(typ));
    }
    return typ;
}

// MarkImplicit marks the interface t as implicit, meaning this interface
// corresponds to a constraint literal such as ~T or A|B without explicit
// interface embedding. MarkImplicit should be called before any concurrent use
// of implicit interfaces.
public static void MarkImplicit(this ref Interface t) {
    t.@implicit = true;
}

// NumExplicitMethods returns the number of explicitly declared methods of interface t.
public static nint NumExplicitMethods(this ref Interface t) {
    return len(t.methods);
}

// ExplicitMethod returns the i'th explicitly declared method of interface t for 0 <= i < t.NumExplicitMethods().
// The methods are ordered by their unique [Id].
public static ж<Func> ExplicitMethod(this ref Interface t, nint i) {
    return t.methods[i];
}

// NumEmbeddeds returns the number of embedded types in interface t.
public static nint NumEmbeddeds(this ref Interface t) {
    return len(t.embeddeds);
}

// Embedded returns the i'th embedded defined (*[Named]) type of interface t for 0 <= i < t.NumEmbeddeds().
// The result is nil if the i'th embedded type is not a defined type.
//
// Deprecated: Use [Interface.EmbeddedType] which is not restricted to defined (*[Named]) types.
public static ж<Named> Embedded(this ref Interface t, nint i) {
    return asNamed(t.embeddeds[i]);
}

// EmbeddedType returns the i'th embedded type of interface t for 0 <= i < t.NumEmbeddeds().
public static ΔType EmbeddedType(this ref Interface t, nint i) {
    return t.embeddeds[i];
}

// NumMethods returns the total number of methods of interface t.
public static nint NumMethods(this ж<Interface> Ꮡt) {
    return Ꮡt.typeSet().NumMethods();
}

// Method returns the i'th method of interface t for 0 <= i < t.NumMethods().
// The methods are ordered by their unique Id.
public static ж<Func> Method(this ж<Interface> Ꮡt, nint i) {
    return Ꮡt.typeSet().Method(i);
}

// Empty reports whether t is the empty interface.
public static bool Empty(this ж<Interface> Ꮡt) {
    return Ꮡt.typeSet().IsAll();
}

// IsComparable reports whether each type in interface t's type set is comparable.
public static bool IsComparable(this ж<Interface> Ꮡt) {
    return Ꮡt.typeSet().IsComparable(default!);
}

// IsMethodSet reports whether the interface t is fully described by its method
// set.
public static bool IsMethodSet(this ж<Interface> Ꮡt) {
    return Ꮡt.typeSet().IsMethodSet();
}

// IsImplicit reports whether the interface t is a wrapper for a type set literal.
public static bool IsImplicit(this ref Interface t) {
    return t.@implicit;
}

// Complete computes the interface's type set. It must be called by users of
// [NewInterfaceType] and [NewInterface] after the interface's embedded types are
// fully defined and before using the interface type in any way other than to
// form other types. The interface must not contain duplicate methods or a
// panic occurs. Complete returns the receiver.
//
// Interface types that have been completed are safe for concurrent use.
public static ж<Interface> Complete(this ж<Interface> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!t.complete) {
        t.complete = true;
    }
    Ꮡt.typeSet(); // checks if t.tset is already set
    return Ꮡt;
}

public static ΔType Underlying(this ж<Interface> Ꮡt) {
    return new InterfaceжΔType(Ꮡt);
}

public static @string String(this ж<Interface> Ꮡt) {
    return TypeString(new InterfaceжΔType(Ꮡt), default!);
}

// ----------------------------------------------------------------------------
// Implementation
internal static void cleanup(this ж<Interface> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.typeSet(); // any interface that escapes type checking must be safe for concurrent use
    t.check = default!;
    t.embedPos = default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodsMustHaveAUniqueˢ = "methods must have a unique non-blank name"u8;
internal static readonly @string methodsCannotHaveTypeˢ = "methods cannot have type parameters"u8;

internal static void interfaceType(this ж<Checker> Ꮡcheck, ж<Interface> Ꮡityp, ж<ast.InterfaceType> Ꮡiface, ж<TypeName> Ꮡdef) {
    ref var check = ref Ꮡcheck.DerefOrNull();
    ref var ityp = ref Ꮡityp.DerefOrNull();
    ref var iface = ref Ꮡiface.DerefOrNull();
    ref var def = ref Ꮡdef.DerefOrNull();

    void addEmbedded(tokenꓸPos pos, ΔType typ) {
        Ꮡityp.Value.embeddeds = append(Ꮡityp.Value.embeddeds, typ);
        if (Ꮡityp.Value.embedPos == nil) {
            Ꮡityp.Value.embedPos = @new<slice<tokenꓸPos>>();
        }
        Ꮡityp.Value.embedPos.ValueSlot = append(Ꮡityp.Value.embedPos.ValueSlot, pos);
    }
    foreach (var (_, f) in (~iface.Methods).List) {
        if (len((~f).Names) == 0) {
            addEmbedded((~f).Type.Pos(), parseUnion(Ꮡcheck, (~f).Type));
            continue;
        }
        // f.Name != nil
        // We have a method with name f.Names[0].
        var name = (~f).Names[0];
        if ((~name).Name == "_"u8) {
            Ꮡcheck.error(new ast_Identжpositioner(name), BlankIfaceMethod, methodsMustHaveAUniqueˢ);
            continue; // ignore
        }
        var typ = Ꮡcheck.typ((~f).Type);
        var (sig, _) = typ._<ж<ΔSignature>>(ᐧ);
        if (sig == nil) {
            if (isValid(typ)) {
                Ꮡcheck.errorf(new ast_Exprᴠpositioner((~f).Type), InvalidSyntaxTree, "%s is not a method signature"u8, typ);
            }
            continue; // ignore
        }
        // The go/parser doesn't accept method type parameters but an ast.FuncType may have them.
        if ((~sig).tparams != nil) {
            positioner at = new ast_Exprᴠpositioner((~f).Type);
            {
                var (ftyp, _) = (~f).Type._<ж<ast.FuncType>>(ᐧ); if (ftyp != nil && (~ftyp).TypeParams != nil) {
                    at = new ast_FieldListжpositioner(ftyp.Value.TypeParams);
                }
            }
            Ꮡcheck.error(at, InvalidSyntaxTree, methodsCannotHaveTypeˢ);
        }
        // use named receiver type if available (for better error messages)
        ΔType recvTyp = new InterfaceжΔType(Ꮡityp);
        if (Ꮡdef != nil) {
            {
                var named = asNamed(def.typ); if (named != nil) {
                    recvTyp = new NamedжΔType(named);
                }
            }
        }
        sig.Value.recv = NewVar(name.Pos(), check.pkg, ""u8, recvTyp);
        var m = NewFunc(name.Pos(), check.pkg, (~name).Name, sig);
        check.recordDef(name, new FuncжObject(m));
        ityp.methods = append(ityp.methods, m);
    }
    // All methods and embedded elements for this interface are collected;
    // i.e., this interface may be used in a type set computation.
    ityp.complete = true;
    if (len(ityp.methods) == 0 && len(ityp.embeddeds) == 0) {
        // empty interface
        ityp.tset = ᏑtopTypeSet;
        return;
    }
    // sort for API stability
    sortMethods(ityp.methods);
    // (don't sort embeddeds: they must correspond to *embedPos entries)
    // Compute type set as soon as possible to report any errors.
    // Subsequent uses of type sets will use this computed type
    // set and won't need to pass in a *Checker.
    check.later(() => {
        computeInterfaceTypeSet(Ꮡcheck, Ꮡiface.Value.Pos(), Ꮡityp);
    }).describef(new ast_InterfaceTypeжpositioner(Ꮡiface), "compute type set for %s"u8, Ꮡityp.OrTypedNil());
}

} // end types_package
