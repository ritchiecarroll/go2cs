// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements go/types-specific scope methods.
// These methods do not exist in types2.
namespace go.go;

using token = global::go.go.token_package;
using global::go.go;

partial class types_package {

// LookupParent follows the parent chain of scopes starting with s until
// it finds a scope where Lookup(name) returns a non-nil object, and then
// returns that scope and object. If a valid position pos is provided,
// only objects that were declared at or before pos are considered.
// If no such scope and object exists, the result is (nil, nil).
// The results are guaranteed to be valid only if the type-checked
// AST has complete position information.
//
// Note that obj.Parent() may be different from the returned scope if the
// object was inserted into the scope and already had a parent at that
// time (see Insert). This can only happen for dot-imported objects
// whose parent is the scope of the package that exported them.
public static (ж<ΔScope>, Object) LookupParent(this ж<ΔScope> Ꮡs, @string name, tokenꓸPos pos) {
    ref var s = ref Ꮡs.DerefOrNull();

    for (; Ꮡs != nil; Ꮡs = s.parent) {
        s = ref Ꮡs.DerefOrNull();
        {
            var obj = s.Lookup(name); if (obj != default! && (!pos.IsValid() || cmpPos(obj.scopePos(), pos) <= 0)) {
                return (Ꮡs, obj);
            }
        }
    }
    return (default!, default!);
}

// Pos and End describe the scope's source code extent [pos, end).
// The results are guaranteed to be valid only if the type-checked
// AST has complete position information. The extent is undefined
// for Universe and package scopes.
public static tokenꓸPos Pos(this ref ΔScope s) {
    return s.pos;
}

public static tokenꓸPos End(this ref ΔScope s) {
    return s.end;
}

// Contains reports whether pos is within the scope's extent.
// The result is guaranteed to be valid only if the type-checked
// AST has complete position information.
public static bool Contains(this ref ΔScope s, tokenꓸPos pos) {
    return cmpPos(s.pos, pos) <= 0 && cmpPos(pos, s.end) < 0;
}

// Innermost returns the innermost (child) scope containing
// pos. If pos is not within any scope, the result is nil.
// The result is also nil for the Universe scope.
// The result is guaranteed to be valid only if the type-checked
// AST has complete position information.
public static ж<ΔScope> Innermost(this ж<ΔScope> Ꮡs, tokenꓸPos pos) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Package scopes do not have extents since they may be
    // discontiguous, so iterate over the package's files.
    if (s.parent == Universe) {
        foreach (var (_, sΔ1) in s.children) {
            {
                var inner = sΔ1.Innermost(pos); if (inner != nil) {
                    return inner;
                }
            }
        }
    }
    if (s.Contains(pos)) {
        foreach (var (_, sΔ2) in s.children) {
            if (sΔ2.Contains(pos)) {
                return sΔ2.Innermost(pos);
            }
        }
        return Ꮡs;
    }
    return default!;
}

} // end types_package
