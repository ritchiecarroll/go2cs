// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using sort = sort_package;
using static go.sort_internal_test_package;

partial class sort_test_package {

// A Change is a record of source code changes, recording user, language, and delta size.
partial struct Change {
    internal @string user;
    internal @string language;
    internal nint lines;
}

// type lessFunc is a methodless func type — rendered inline as its base delegate

// multiSorter implements the Sort interface, sorting the changes within.
public partial struct multiSorter {
    internal slice<Change> changes;
    internal slice<Func<ж<Change>, ж<Change>, bool>> less;
}

// Sort sorts the argument slice according to the less functions passed to OrderedBy.
public static void ΔSort(this ж<multiSorter> Ꮡms, slice<Change> changes) {
    ref var ms = ref Ꮡms.DerefOrNull();

    ms.changes = changes;
    sort.Sort(new sort_test_package.multiSorterжInterface(Ꮡms));
}

// OrderedBy returns a Sorter that sorts using the less functions, in order.
// Call its Sort method to sort the data.
public static ж<multiSorter> OrderedBy(params Span<Func<ж<Change>, ж<Change>, bool>> lessʗp) {
    var less = lessʗp.slice();

    return Ꮡ(new multiSorter(
        less: less
    ));
}

// Len is part of sort.Interface.
public static nint Len(this ref multiSorter ms) {
    return len(ms.changes);
}

// Swap is part of sort.Interface.
public static void Swap(this ref multiSorter ms, nint i, nint j) {
    (ms.changes[i], ms.changes[j]) = (ms.changes[j], ms.changes[i]);
}

// Less is part of sort.Interface. It is implemented by looping along the
// less functions until it finds a comparison that discriminates between
// the two items (one is less than the other). Note that it can call the
// less functions twice per call. We could change the functions to return
// -1, 0, 1 and reduce the number of calls for greater efficiency: an
// exercise for the reader.
public static bool Less(this ref multiSorter ms, nint i, nint j) {
    var (p, q) = (Ꮡ(ms.changes, i), Ꮡ(ms.changes, j));
    // Try all but the last comparison.
    nint k = default!;
    for (k = 0; k < len(ms.less) - 1; k++) {
        var less = ms.less[k];
        switch (ᐧ) {
        case {} when less(p, q): {
            return true;
        }
        case {} when less(q, // p < q, so we have a decision.
 p): {
            return false;
        }}

    }
    // p > q, so we have a decision.
    // p == q; try the next comparison.
    // All comparisons to here said "equal", so just return whatever
    // the final comparison reports.
    return ms.less[k](p, q);
}

internal static slice<Change> changes = new Change[]{
    new("gri"u8, "Go"u8, 100),
    new("ken"u8, "C"u8, 150),
    new("glenda"u8, "Go"u8, 200),
    new("rsc"u8, "Go"u8, 200),
    new("r"u8, "Go"u8, 100),
    new("ken"u8, "Go"u8, 200),
    new("dmr"u8, "C"u8, 100),
    new("r"u8, "C"u8, 150),
    new("gri"u8, "Smalltalk"u8, 80)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object byUserˢ = (@string)"By user:"u8;
internal static readonly object byUserLinesˢ = (@string)"By user,<lines:"u8;
internal static readonly object byUserLinesˢ2 = (@string)"By user,>lines:"u8;
internal static readonly object byLanguageLinesˢ = (@string)"By language,<lines:"u8;
internal static readonly object byLanguageLinesUserˢ = (@string)"By language,<lines,user:"u8;

// Example_sortMultiKeys demonstrates a technique for sorting a struct type using different
// sets of multiple fields in the comparison. We chain together "Less" functions, each of
// which compares a single field.
public static void Example_sortMultiKeys() {
    // Closures that order the Change structure.
    var user = (ж<Change> c1, ж<Change> c2) => (~c1).user < (~c2).user;
    var language = (ж<Change> c1, ж<Change> c2) => (~c1).language < (~c2).language;
    var increasingLines = (ж<Change> c1, ж<Change> c2) => (~c1).lines < (~c2).lines;
    var decreasingLines = (ж<Change> c1, ж<Change> c2) => {
        return (~c1).lines > (~c2).lines; // Note: > orders downwards.
    };
    // Simple use: Sort by user.
    OrderedBy(new Func<ж<Change>, ж<Change>, bool>(user)).ΔSort(changes);
    fmt.Println(byUserˢ, changes);
    // More examples.
    OrderedBy(new Func<ж<Change>, ж<Change>, bool>(user), increasingLines).ΔSort(changes);
    fmt.Println(byUserLinesˢ, changes);
    OrderedBy(new Func<ж<Change>, ж<Change>, bool>(user), decreasingLines).ΔSort(changes);
    fmt.Println(byUserLinesˢ2, changes);
    OrderedBy(new Func<ж<Change>, ж<Change>, bool>(language), increasingLines).ΔSort(changes);
    fmt.Println(byLanguageLinesˢ, changes);
    OrderedBy(new Func<ж<Change>, ж<Change>, bool>(language), increasingLines, user).ΔSort(changes);
    fmt.Println(byLanguageLinesUserˢ, changes);
}

// Output:
// By user: [{dmr C 100} {glenda Go 200} {gri Go 100} {gri Smalltalk 80} {ken C 150} {ken Go 200} {r Go 100} {r C 150} {rsc Go 200}]
// By user,<lines: [{dmr C 100} {glenda Go 200} {gri Smalltalk 80} {gri Go 100} {ken C 150} {ken Go 200} {r Go 100} {r C 150} {rsc Go 200}]
// By user,>lines: [{dmr C 100} {glenda Go 200} {gri Go 100} {gri Smalltalk 80} {ken Go 200} {ken C 150} {r C 150} {r Go 100} {rsc Go 200}]
// By language,<lines: [{dmr C 100} {ken C 150} {r C 150} {gri Go 100} {r Go 100} {glenda Go 200} {ken Go 200} {rsc Go 200} {gri Smalltalk 80}]
// By language,<lines,user: [{dmr C 100} {ken C 150} {r C 150} {gri Go 100} {r Go 100} {glenda Go 200} {ken Go 200} {rsc Go 200} {gri Smalltalk 80}]

} // end sort_test_package
