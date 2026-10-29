// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package impl is a registry of alternative implementations of cryptographic
// primitives, to allow selecting them for testing.
namespace go.crypto.@internal;

using strings = strings_package;

partial class impl_package {

partial struct implementation {
    public @string Package;
    public @string Name;
    public bool Available;
    public ж<bool> Toggle;
}

internal static slice<implementation> allImplementations;

// Register records an alternative implementation of a cryptographic primitive.
// The implementation might be available or not based on CPU support. If
// available is false, the implementation is unavailable and can't be tested on
// this machine. If available is true, it can be set to false to disable the
// implementation. If all alternative implementations but one are disabled, the
// remaining one must be used (i.e. disabling one implementation must not
// implicitly disable any other). Each package has an implicit base
// implementation that is selected when all alternatives are unavailable or
// disabled. pkg must be the package name, not path (e.g. "aes" not "crypto/aes").
public static void Register(@string pkg, @string name, ж<bool> Ꮡavailable) {
    ref var available = ref Ꮡavailable.DerefOrNull();

    if (strings.Contains(pkg, "/"u8)) {
        throw panic("impl: package name must not contain slashes");
    }
    allImplementations = append(allImplementations, new implementation(
        Package: pkg,
        Name: name,
        Available: available,
        Toggle: Ꮡavailable
    ));
}

// Packages returns the list of all packages for which alternative
// implementations are registered.
public static slice<@string> Packages() {
    var pkgsᴷ = new stackAppend<@string>(moves: true);

    slice<@string> pkgs = default;
    var seen = new map<@string, bool>();
    foreach (var (_, i) in allImplementations) {
        if (!seen[i.Package]) {
            pkgs = append(pkgsᴷ, pkgs, i.Package);
            seen[i.Package] = true;
        }
    }
    pkgs = move2heap(pkgsᴷ, pkgs);
    return pkgs;
}

// List returns the names of all alternative implementations registered for the
// given package, whether available or not. The implicit base implementation is
// not included.
public static slice<@string> List(@string pkg) {
    var namesᴷ = new stackAppend<@string>(moves: true);

    slice<@string> names = default;
    foreach (var (_, i) in allImplementations) {
        if (i.Package == pkg) {
            names = append(namesᴷ, names, i.Name);
        }
    }
    names = move2heap(namesᴷ, names);
    return names;
}

internal static bool available(@string pkg, @string name) {
    foreach (var (_, i) in allImplementations) {
        if (i.Package == pkg && i.Name == name) {
            return i.Available;
        }
    }
    throw panic("unknown implementation");
}

// Select disables all implementations for the given package except the one
// with the given name. If name is empty, the base implementation is selected.
// It returns whether the selected implementation is available.
public static bool Select(@string pkg, @string name) {
    if (name == ""u8) {
        foreach (var (_, i) in allImplementations) {
            if (i.Package == pkg) {
                i.Toggle.Value = false;
            }
        }
        return true;
    }
    if (!available(pkg, name)) {
        return false;
    }
    foreach (var (_, i) in allImplementations) {
        if (i.Package == pkg) {
            i.Toggle.Value = i.Name == name;
        }
    }
    return true;
}

public static void Reset(@string pkg) {
    foreach (var (_, i) in allImplementations) {
        if (i.Package == pkg) {
            i.Toggle.Value = i.Available;
            return;
        }
    }
}

} // end impl_package
