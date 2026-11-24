namespace go;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

partial interface Named {
    @string Name();
}

partial struct item {
    internal @string n;
}

internal static @string Name(this ref item i) { return i.n; }

internal static nint compareItems(ж<item> Ꮡa, ж<item> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    return strings.Compare(a.n, b.n);
}

internal static slice<ж<item>> wrap(params ꓸꓸꓸstring nsʗp) {
    var ns = nsʗp.sslice();

    var @out = new slice<ж<item>>(len(ns));
    foreach (var (i, vᴛ1) in range(ns)) {
        ref var n = ref heap(new @string(), out var Ꮡn);
        n = vᴛ1;

        @out[i] = Ꮡ(new item(n));
    }
    return @out;
}

internal static @string join<T>(slice<T> xs, Func<T, T, nint> cmp)
    where T : Named
{
    return strings.Join(sortedNames(xs, cmp), ",");
}

internal static slice<@string> sortedNames<T>(slice<T> xs, Func<T, T, nint> cmp)
    where T : Named
{
    var ys = slices.Clone<slice<T>, T>(xs);
    slices.SortFunc(ys, cmp);
    var names = new slice<@string>(len(ys));
    foreach (var (i, y) in range(ys)) {
        names[i] = y.Name();
    }
    return names;
}

internal static (T, bool) find<T>(slice<T> xs, @string name)
    where T : Named
{
    foreach (var (_, x) in range(xs)) {
        if (x.Name() == name) {
            return (x, true);
        }
    }
    T zero = GoZero<T>();
    return (zero, false);
}

internal static T firstOr<T>(slice<T> xs, Func<T> fallback) {
    if (len(xs) == 0) {
        return fallback();
    }
    return xs[0];
}

internal static nint countLike<T>(slice<T> xs, T like)
    where T : Named
{
    nint n = 0;
    foreach (var (_, x) in range(xs)) {
        if (x.Name() == like.Name()) {
            n++;
        }
    }
    return n;
}

internal static void main() {
    var items = wrap("b", "a");

    fmt.Println("join:", join<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(items, elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), (Δp0, Δp1) => compareItems(Δp0, Δp1)), len(items), items[0].n);

    fmt.Println("literal:", join<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(items, elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), [GoFunc("main.func1")] (itemжNamedᴾ aΔp, itemжNamedᴾ bΔp) => {
        var a = (ж<item>)aΔp;
        var b = (ж<item>)bΔp;
        return strings.Compare(b.n, a.n);
    }));

    var (got, ok) = (find<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(items, elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), "a") switch { var ᴛr => ((ж<item>)ᴛr.Item1, ᴛr.Item2) });
    fmt.Println("find:", ok, got == items[1]);
    got.n = "a2";
    fmt.Println("write-through:", items[1].n);
    (var missing, ok) = (find<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(items, elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), "zz") switch { var ᴛr => ((ж<item>)ᴛr.Item1, ᴛr.Item2) });
    fmt.Println("missing:", ok, missing == nil);

    var spare = Ꮡ(new item("spare"));
    var spareʗ1 = spare;

    var spareʗ2 = spare;
    fmt.Println("firstOr:", firstOr(items, [GoFunc("main.func2")] ж<item> () => spareʗ1) == items[0], firstOr(default, [GoFunc("main.func3")] ж<item> () => spareʗ2) == spare);

    fmt.Println("countLike:", countLike<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(items, elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), Ꮡ(new item("b"))), countLike<itemжNamedᴾ>(widen<ж<item>, itemжNamedᴾ>(wrap("b", "c", "b"), elemᴛ0 => new itemжNamedᴾ(elemᴛ0)), items[0]));
}

} // end main_package
