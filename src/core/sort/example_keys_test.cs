// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using sort = sort_package;
using static go.sort_internal_test_package;

partial class sort_test_package {

partial struct earthMass /*num:float64*/;

partial struct au /*num:float64*/;

// A Planet defines the properties of a solar system object.
partial struct Planet {
    internal @string name;
    internal earthMass mass;
    internal au distance;
}

public delegate bool By(ж<Planet> p1, ж<Planet> p2);

// Sort is a method on the function type, By, that sorts the argument slice according to the function.
public static void ΔSort(this By by, slice<Planet> planets) {
    var ps = Ꮡ(new planetSorter(
        planets: planets,
        by: new Func<ж<Planet>, ж<Planet>, bool>(by)
    ));
    // The Sort method's receiver is the function (closure) that defines the sort order.
    sort.Sort(new sort_test_package.planetSorterжInterface(ps));
}

// planetSorter joins a By function and a slice of Planets to be sorted.
partial struct planetSorter {
    internal slice<Planet> planets;
    internal Func<ж<Planet>, ж<Planet>, bool> by; // Closure used in the Less method.
}

// Len is part of sort.Interface.
internal static nint Len(this ref planetSorter s) {
    return len(s.planets);
}

// Swap is part of sort.Interface.
internal static void Swap(this ref planetSorter s, nint i, nint j) {
    (s.planets[i], s.planets[j]) = (s.planets[j], s.planets[i]);
}

// Less is part of sort.Interface. It is implemented by calling the "by" closure in the sorter.
internal static bool Less(this ref planetSorter s, nint i, nint j) {
    return s.by(Ꮡ(s.planets, i), Ꮡ(s.planets, j));
}

internal static slice<Planet> planets = new Planet[]{
    new("Mercury"u8, 0.055D, 0.4D),
    new("Venus"u8, 0.815D, 0.7D),
    new("Earth"u8, 1.0D, 1.0D),
    new("Mars"u8, 0.107D, 1.5D)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object byNameˢ = (@string)"By name:"u8;
internal static readonly object byMassˢ = (@string)"By mass:"u8;
internal static readonly object byDistanceˢ = (@string)"By distance:"u8;
internal static readonly object byDecreasingDistanceˢ = (@string)"By decreasing distance:"u8;

// Example_sortKeys demonstrates a technique for sorting a struct type using programmable sort criteria.
public static void Example_sortKeys() {
    // Closures that order the Planet structure.
    var name = (ж<Planet> p1, ж<Planet> p2) => (~p1).name < (~p2).name;
    var mass = (ж<Planet> p1, ж<Planet> p2) => (~p1).mass < (~p2).mass;
    var distance = (ж<Planet> p1, ж<Planet> p2) => (~p1).distance < (~p2).distance;
    var distanceʗ1 = distance;
    var decreasingDistance = (ж<Planet> p1, ж<Planet> p2) => distanceʗ1(p2, p1);
    // Sort the planets by the various criteria.
    NilSafeDelegateConversion<By, Func<ж<Planet>, ж<Planet>, bool>>(name).ΔSort(planets);
    fmt.Println(byNameˢ, planets);
    NilSafeDelegateConversion<By, Func<ж<Planet>, ж<Planet>, bool>>(mass).ΔSort(planets);
    fmt.Println(byMassˢ, planets);
    NilSafeDelegateConversion<By, Func<ж<Planet>, ж<Planet>, bool>>(distance).ΔSort(planets);
    fmt.Println(byDistanceˢ, planets);
    NilSafeDelegateConversion<By, Func<ж<Planet>, ж<Planet>, bool>>(decreasingDistance).ΔSort(planets);
    fmt.Println(byDecreasingDistanceˢ, planets);
}

// Output: By name: [{Earth 1 1} {Mars 0.107 1.5} {Mercury 0.055 0.4} {Venus 0.815 0.7}]
// By mass: [{Mercury 0.055 0.4} {Mars 0.107 1.5} {Venus 0.815 0.7} {Earth 1 1}]
// By distance: [{Mercury 0.055 0.4} {Venus 0.815 0.7} {Earth 1 1} {Mars 0.107 1.5}]
// By decreasing distance: [{Mars 0.107 1.5} {Earth 1 1} {Venus 0.815 0.7} {Mercury 0.055 0.4}]

} // end sort_test_package
