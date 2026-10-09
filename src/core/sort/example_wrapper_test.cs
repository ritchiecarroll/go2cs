// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using sort = sort_package;
using static go.sort_internal_test_package;

partial class sort_test_package {

partial struct Grams /*num:nint*/;

public static @string String(this Grams g) {
    return fmt.Sprintf("%dg"u8, (nint)g);
}

partial struct Organ {
    public @string Name;
    public Grams Weight;
}

partial struct Organs /*[]ж<Organ>*/;

public static nint Len(this Organs s) {
    return len(s);
}

public static void Swap(this Organs s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

// ByName implements sort.Interface by providing Less and using the Len and
// Swap methods of the embedded Organs value.
partial struct ByName {
    public partial ref Organs Organs { get; }
}

public static bool Less(this ByName s, nint i, nint j) {
    return (~s.Organs[i]).Name < (~s.Organs[j]).Name;
}

// ByWeight implements sort.Interface by providing Less and using the Len and
// Swap methods of the embedded Organs value.
partial struct ByWeight {
    public partial ref Organs Organs { get; }
}

public static bool Less(this ByWeight s, nint i, nint j) {
    return (~s.Organs[i]).Weight < (~s.Organs[j]).Weight;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object organsByWeightˢ = (@string)"Organs by weight:"u8;
internal static readonly object organsByNameˢ = (@string)"Organs by name:"u8;

public static void Example_sortWrapper() {
    var s = new ж<Organ>[]{
        Ꮡ(new Organ("brain"u8, 1340)),
        Ꮡ(new Organ("heart"u8, 290)),
        Ꮡ(new Organ("liver"u8, 1494)),
        Ꮡ(new Organ("pancreas"u8, 131)),
        Ꮡ(new Organ("prostate"u8, 62)),
        Ꮡ(new Organ("spleen"u8, 162))
    }.slice();
    sort.Sort(new ByWeight(s));
    fmt.Println(organsByWeightˢ);
    printOrgans(s);
    sort.Sort(new ByName(s));
    fmt.Println(organsByNameˢ);
    printOrgans(s);
}

// Output:
// Organs by weight:
// prostate (62g)
// pancreas (131g)
// spleen   (162g)
// heart    (290g)
// brain    (1340g)
// liver    (1494g)
// Organs by name:
// brain    (1340g)
// heart    (290g)
// liver    (1494g)
// pancreas (131g)
// prostate (62g)
// spleen   (162g)
internal static void printOrgans(slice<ж<Organ>> s) {
    foreach (var (_, o) in s) {
        fmt.Printf("%-8s (%v)\n"u8, (~o).Name, (~o).Weight);
    }
}

} // end sort_test_package
