// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements method sets.
namespace go.go;

using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using io = io_package;

partial class types_package {

// A MethodSet is an ordered set of concrete or abstract (interface) methods;
// a method is a [MethodVal] selection, and they are ordered by ascending m.Obj().Id().
// The zero value for a MethodSet is a ready-to-use empty method set.
partial struct MethodSet {
    internal slice<ж<Selection>> list;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodSetˢ = "MethodSet {}"u8;
internal static readonly object methodSetˢ2 = (@string)"MethodSet {"u8;

public static @string String(this ref MethodSet s) {
    if (s.Len() == 0) {
        return methodSetˢ;
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡbuf), methodSetˢ2);
    foreach (var (_, f) in s.list) {
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡbuf), "\t%s\n"u8, f.OrTypedNil());
    }
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡbuf), (@string)"}"u8);
    return buf.String();
}

// Len returns the number of methods in s.
public static nint Len(this ref MethodSet s) {
    return len(s.list);
}

// At returns the i'th method in s for 0 <= i < s.Len().
public static ж<Selection> At(this ref MethodSet s, nint i) {
    return s.list[i];
}

// Lookup returns the method with matching package and name, or nil if not found.
public static ж<Selection> Lookup(this ж<MethodSet> Ꮡs, ж<Package> Ꮡpkg, @string name) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.Len() == 0) {
        return default!;
    }
    @string key = Id(Ꮡpkg, name);
    nint i = sort.Search(len(s.list), (nint iΔ1) => {
        var m = Ꮡs.Value.list[iΔ1];
        return (~m).obj.Id() >= key;
    });
    if (i < len(s.list)) {
        var m = s.list[i];
        if ((~m).obj.Id() == key) {
            return m;
        }
    }
    return default!;
}

// Shared empty method set.
internal static ж<MethodSet> ᏑemptyMethodSet = new StandardBox<MethodSet>(default(MethodSet));
internal static ref MethodSet emptyMethodSet => ref ᏑemptyMethodSet.Value;

// Note: NewMethodSet is intended for external use only as it
//       requires interfaces to be complete. It may be used
//       internally if LookupFieldOrMethod completed the same
//       interfaces beforehand.

// NewMethodSet returns the method set for the given type T.
// It always returns a non-nil method set, even if it is empty.
public static ж<MethodSet> NewMethodSet(ΔType T) {
    // WARNING: The code in this function is extremely subtle - do not modify casually!
    //          This function and lookupFieldOrMethod should be kept in sync.
    // TODO(rfindley) confirm that this code is in sync with lookupFieldOrMethod
    //                with respect to type params.
    // Methods cannot be associated with a named pointer type.
    // (spec: "The type denoted by T is called the receiver base type;
    // it must not be a pointer or interface type and it must be declared
    // in the same package as the method.").
    {
        var t = asNamed(T); if (t != nil && isPointer(new NamedжΔType(t))) {
            return ᏑemptyMethodSet;
        }
    }
    // method set up to the current depth, allocated lazily
    methodSet @base = default!;
    var (typ, isPtr) = deref(T);
    // *typ where typ is an interface has no methods.
    if (isPtr && IsInterface(typ)) {
        return ᏑemptyMethodSet;
    }
    // Start with typ as single entry at shallowest depth.
    var current = new embeddedType[]{new(typ, default!, isPtr, false)}.slice();
    // seen tracks named types that we have seen already, allocated lazily.
    // Used to avoid endless searches in case of recursive types.
    //
    // We must use a lookup on identity rather than a simple map[*Named]bool as
    // instantiated types may be identical but not equal.
    instanceLookup seen = new();
    // collect methods at current depth
    while (len(current) > 0) {
        slice<embeddedType> next = default!;                          // embedded types found at current depth
        // field and method sets at current depth, indexed by names (Id's), and allocated lazily
        map<@string, bool> fset = default!;                            // we only care about the field names
        methodSet mset = default!;
        foreach (var (_, e) in current) {
            var typΔ1 = e.typ;
            // If we have a named type, we may have associated methods.
            // Look for those first.
            {
                var named = asNamed(typΔ1); if (named != nil) {
                    {
                        var alt = seen.lookup(named); if (alt != nil) {
                            // We have seen this type before, at a more shallow depth
                            // (note that multiples of this type at the current depth
                            // were consolidated before). The type at that depth shadows
                            // this same type at the current depth, so we can ignore
                            // this one.
                            continue;
                        }
                    }
                    seen.add(named);
                    for (nint i = 0; i < named.NumMethods(); i++) {
                        mset = mset.addOne(named.Method(i), concat(e.index, i), e.indirect, e.multiples);
                    }
                }
            }
            switch (under(typΔ1).type()) {
            case ж<Struct> t: {
                foreach (var (i, f) in (~t).fields) {
                    if (fset == default!) {
                        fset = new map<@string, bool>();
                    }
                    fset[f.of(Var.Ꮡobject).Id()] = true;
                    // Embedded fields are always of the form T or *T where
                    // T is a type name. If typ appeared multiple times at
                    // this depth, f.Type appears multiple times at the next
                    // depth.
                    if ((~f).embedded) {
                        var (typΔ2, isPtrΔ1) = deref((~f).typ);
                        // TODO(gri) optimization: ignore types that can't
                        // have fields or methods (only Named, Struct, and
                        // Interface types need to be considered).
                        next = append(next, new embeddedType(typΔ2, concat(e.index, i), e.indirect || isPtrΔ1, e.multiples));
                    }
                }
                break;
            }
            case ж<Interface> t: {
                mset = mset.add((~t.typeSet()).methods, e.index, true, e.multiples);
                break;
            }}
        }
        // Add methods and collisions at this depth to base if no entries with matching
        // names exist already.
        foreach (var (k, vᴛ1) in mset) {
            var m = vᴛ1;

            {
                var (_, found) = @base[k, ꟷ]; if (!found) {
                    // Fields collide with methods of the same name at this depth.
                    if (fset[k]) {
                        m = default!; // collision
                    }
                    if (@base == default!) {
                        @base = new methodSet(0);
                    }
                    @base[k] = m;
                }
            }
        }
        // Add all (remaining) fields at this depth as collisions (since they will
        // hide any method further down) if no entries with matching names exist already.
        foreach (var (k, _) in fset) {
            {
                var (_, found) = @base[k, ꟷ]; if (!found) {
                    if (@base == default!) {
                        @base = new methodSet(0);
                    }
                    @base[k] = default!; // collision
                }
            }
        }
        current = consolidateMultiples(next);
    }
    if (len(@base) == 0) {
        return ᏑemptyMethodSet;
    }
    // collect methods
    slice<ж<Selection>> list = default!;
    foreach (var (_, m) in @base) {
        if (m != nil) {
            m.Value.recv = T;
            list = append(list, m);
        }
    }
    // sort by unique name
    var listʗ1 = list;
    sort.Slice(list, (nint i, nint j) => (~listʗ1[i]).obj.Id() < (~listʗ1[j]).obj.Id());
    return Ꮡ(new MethodSet(list));
}

partial struct methodSet /*map[@string, ж<Selection>]*/;

// Add adds all functions in list to the method set s.
// If multiples is set, every function in list appears multiple times
// and is treated as a collision.
internal static methodSet add(this methodSet s, slice<ж<Func>> list, slice<nint> index, bool indirect, bool multiples) {
    if (len(list) == 0) {
        return s;
    }
    foreach (var (i, f) in list) {
        s = s.addOne(f, concat(index, i), indirect, multiples);
    }
    return s;
}

internal static methodSet addOne(this methodSet s, ж<Func> Ꮡf, slice<nint> index, bool indirect, bool multiples) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (s == default!) {
        s = new methodSet(0);
    }
    @string key = Ꮡf.of(Func.Ꮡobject).Id();
    // if f is not in the set, add it
    if (!multiples) {
        // TODO(gri) A found method may not be added because it's not in the method set
        // (!indirect && f.hasPtrRecv()). A 2nd method on the same level may be in the method
        // set and may not collide with the first one, thus leading to a false positive.
        // Is that possible? Investigate.
        {
            var (_, found) = s[key, ꟷ]; if (!found && (indirect || !f.hasPtrRecv())) {
                s[key] = Ꮡ(new Selection(MethodVal, default!, new FuncжObject(Ꮡf), index, indirect));
                return s;
            }
        }
    }
    s[key] = default!; // collision
    return s;
}

} // end types_package
