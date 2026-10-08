namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

partial struct Names /*map[nint, @string]*/;

partial struct Scores /*[]float64*/;

partial struct Feed /*chan nint*/;

// type Pred is a methodless func type — rendered inline as its base delegate

partial struct Grid /*[2]nint*/;

internal static T first<T>(/*[2]*/ array<T> a) {
    a = a.Clone();

    return a[0];
}

internal static slice<nint> keys<V>(map<nint, V> m) {
    var @out = new slice<nint>(0, len(m));
    foreach (var (k, _) in m) {
        @out = append(@out, k);
    }
    sort.Ints(@out);
    return @out;
}

internal static E total<E>(slice<E> s)
    where E : /* int | float64 */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IIncrementOperators<E>, IDecrementOperators<E>, IUnaryNegationOperators<E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    E t = GoZero<E>();
    foreach (var (_, v) in s) {
        t += v;
    }
    return t;
}

internal static slice<T> drain<T>(channel<T> c) {
    close(c);
    slice<T> @out = default!;
    foreach (var v in c) {
        @out = append(@out, v);
    }
    return @out;
}

internal static nint count<T>(slice<T> items, Func<T, bool> p) {
    nint n = 0;
    foreach (var (_, it) in items) {
        if (p(it)) {
            n++;
        }
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedMapˢ = (@string)"named map:"u8;
private static readonly object namedSliceˢ = (@string)"named slice:"u8;
private static readonly object namedChanˢ = (@string)"named chan:"u8;
private static readonly object namedFuncˢ = (@string)"named func:"u8;
private static readonly object namedArrayˢ = (@string)"named array:"u8;
private static readonly object controlsˢ = (@string)"controls:"u8;

internal static void Main() {
    var names = new Names(new map<nint, @string>{[2] = "b"u8, [1] = "a"u8});
    fmt.Println(namedMapˢ, keys<@string>(names));
    var scores = new Scores(new float64[]{1.5D, 2.5D}.slice());
    fmt.Println(namedSliceˢ, total<float64>(scores));
    var feed = new Feed(2);
    feed.ᐸꟷ(7);
    fmt.Println(namedChanˢ, drain<nint>(feed));
    Func<nint, bool> even = (nint v) => v % 2 == 0;
    fmt.Println(namedFuncˢ, count(new nint[]{1, 2, 4}.slice(), even));
    var grid = new Grid(new nint[]{4, 5}.array());
    fmt.Println(namedArrayˢ, first<nint>(grid));
    fmt.Println(controlsˢ, keys(new map<nint, @string>{[3] = "c"u8}), keys<@string>(names));
}

} // end main_package
