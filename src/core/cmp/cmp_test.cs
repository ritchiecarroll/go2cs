// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using fmt = fmt_package;
using Δmath = math_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;

partial class cmp_test_package {

internal static ж<float64> Ꮡnegzero = new StandardBox<float64>(Δmath.Copysign(0D, -1D));
internal static ref float64 negzero => ref Ꮡnegzero.Value;

internal static uintptr nonnilptr = (uintptr)Ꮡnegzero;

internal static uintptr nilptr = (uintptr)(@unsafe.Pointer)default!;


partial struct testsᴛ1 /*dyn*/ {
    internal any x, y;
    internal nint compare;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new((nint)(1), (nint)(2), -1),
    new((nint)(1), (nint)(1), 0),
    new((nint)(2), (nint)(1), +1),
    new((@string)"a"u8, (@string)"aa"u8, -1),
    new((@string)"a"u8, (@string)"a"u8, 0),
    new((@string)"aa"u8, (@string)"a"u8, +1),
    new(1.0D, 1.1D, -1),
    new(1.1D, 1.1D, 0),
    new(1.1D, 1.0D, +1),
    new(Δmath.Inf(1), Δmath.Inf(1), 0),
    new(Δmath.Inf(-1), Δmath.Inf(-1), 0),
    new(Δmath.Inf(-1), 1.0D, -1),
    new(1.0D, Δmath.Inf(-1), +1),
    new(Δmath.Inf(1), 1.0D, +1),
    new(1.0D, Δmath.Inf(1), -1),
    new(Δmath.NaN(), Δmath.NaN(), 0),
    new(0.0D, Δmath.NaN(), +1),
    new(Δmath.NaN(), 0.0D, -1),
    new(Δmath.NaN(), Δmath.Inf(-1), -1),
    new(Δmath.Inf(-1), Δmath.NaN(), +1),
    new(0.0D, 0.0D, 0),
    new(negzero, negzero, 0),
    new(negzero, 0.0D, 0),
    new(0.0D, negzero, 0),
    new(negzero, 1.0D, -1),
    new(negzero, -1.0D, +1),
    new(nilptr, nonnilptr, -1),
    new(nonnilptr, nilptr, 1),
    new(nonnilptr, nonnilptr, 0)
}.slice();

public static void TestLess(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in tests) {
        bool b = default!;
        switch (test.x.type()) {
        case nint: {
            b = cmp.Less(test.x._<nint>(), test.y._<nint>());
            break;
        }
        case int32: {
            b = cmp.Less(test.x._<nint>(), test.y._<nint>());
            break;
        }
        case @string: {
            b = cmp.Less(test.x._<@string>(), test.y._<@string>());
            break;
        }
        case float64: {
            b = cmp.Less(test.x._<float64>(), test.y._<float64>());
            break;
        }
        case uintptr: {
            b = cmp.Less(test.x._<uintptr>(), test.y._<uintptr>());
            break;
        }}

        if (b != (test.compare < 0)) {
            Ꮡt.Errorf("Less(%v, %v) == %t, want %t"u8, test.x, test.y, b, test.compare < 0);
        }
    }
}

public static void TestCompare(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in tests) {
        nint c = default!;
        switch (test.x.type()) {
        case nint: {
            c = cmp.Compare(test.x._<nint>(), test.y._<nint>());
            break;
        }
        case int32: {
            c = cmp.Compare(test.x._<nint>(), test.y._<nint>());
            break;
        }
        case @string: {
            c = cmp.Compare(test.x._<@string>(), test.y._<@string>());
            break;
        }
        case float64: {
            c = cmp.Compare(test.x._<float64>(), test.y._<float64>());
            break;
        }
        case uintptr: {
            c = cmp.Compare(test.x._<uintptr>(), test.y._<uintptr>());
            break;
        }}

        if (c != test.compare) {
            Ꮡt.Errorf("Compare(%v, %v) == %d, want %d"u8, test.x, test.y, c, test.compare);
        }
    }
}

public static void TestSort(ж<testing.T> Ꮡt) {
    // Test that our comparison function is consistent with
    // sort.Float64s.
    var input = new float64[]{1.0D, 0.0D, negzero, Δmath.Inf(1), Δmath.Inf(-1), Δmath.NaN()}.slice();
    sort.Float64s(input);
    for (nint i = 0; i < len(input) - 1; i++) {
        if (cmp.Less(input[i + 1], input[i])) {
            Ꮡt.Errorf("Less sort mismatch at %d in %v"u8, i, input);
        }
        if (cmp.Compare(input[i], input[i + 1]) > 0) {
            Ꮡt.Errorf("Compare sort mismatch at %d in %v"u8, i, input);
        }
    }
}

internal partial struct TestOr_cases /*dyn*/ {
    internal slice<nint> @in;
    internal nint want;
}

public static void TestOr(ж<testing.T> Ꮡt) {
    var cases = new TestOr_cases[]{
        new(default!, 0),
        new(new nint[]{0}.slice(), 0),
        new(new nint[]{1}.slice(), 1),
        new(new nint[]{0, 2}.slice(), 2),
        new(new nint[]{3, 0}.slice(), 3),
        new(new nint[]{4, 5}.slice(), 4),
        new(new nint[]{0, 6, 7}.slice(), 6)
    }.slice();
    foreach (var (_, tc) in cases) {
        {
            nint got = cmp.Or(tc.@in.ꓸꓸꓸ); if (got != tc.want) {
                Ꮡt.Errorf("cmp.Or(%v) = %v; want %v"u8, tc.@in, got, tc.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string someTextˢ = "some text"u8;
private static readonly @string defaultˢ = "default"u8;

public static void ExampleOr() {
    // Suppose we have some user input
    // that may or may not be an empty string
    @string userInput1 = ""u8;
    @string userInput2 = someTextˢ;
    fmt.Println(cmp.Or(userInput1, defaultˢ));
    fmt.Println(cmp.Or(userInput2, defaultˢ));
    fmt.Println(cmp.Or(userInput1, userInput2, defaultˢ));
}

internal partial struct ExampleOr_sort_Order /*dyn*/ {
    public @string Product;
    public @string Customer;
    public float64 Price;
}

// Output:
// default
// some text
// some text
public static void ExampleOr_sort() {
    var orders = new ExampleOr_sort_Order[]{
        new("foo"u8, "alice"u8, 1.00D),
        new("bar"u8, "bob"u8, 3.00D),
        new("baz"u8, "carol"u8, 4.00D),
        new("foo"u8, "alice"u8, 2.00D),
        new("bar"u8, "carol"u8, 1.00D),
        new("foo"u8, "bob"u8, 4.00D)
    }.slice();
    // Sort by customer first, product second, and last by higher price
    slices.SortFunc(orders, (ExampleOr_sort_Order a, ExampleOr_sort_Order b) => cmp.Or(
            strings.Compare(a.Customer, b.Customer),
            strings.Compare(a.Product, b.Product),
            cmp.Compare(b.Price, a.Price)));
    foreach (var (_, order) in orders) {
        fmt.Printf("%s %s %.2f\n"u8, order.Product, order.Customer, order.Price);
    }
}

// Output:
// foo alice 2.00
// foo alice 1.00
// bar bob 3.00
// foo bob 4.00
// bar carol 1.00
// baz carol 4.00

} // end cmp_test_package
