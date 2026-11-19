namespace go;

using fmt = fmt_package;
using bits = math.bits_package;
using sort = sort_package;
using math;
using main = main_package;
using static main_package;

partial class main_package {

partial struct words /*[]uint64*/;

} partial class mainᴛwords {

internal static nint Len(this words w) { return len(w); }
internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }

internal static nint cap(this words w) { return builtin.cap(w) + 1; }
internal static nint bits(this words w) { return math.bits_package.Len64(w[0]); }

internal static void Sort(this words w) { main.Sort(w); }

internal static nint Run(this words w) { return len(w); }

} partial class main_package {

partial struct gauge {
    internal nint n;
}

} partial class mainᴛgauge {

internal static nint size(this gauge g, words w) { return cap(w) + bits.Len64((uint64)g.n); }

} partial class main_package {

public static void Sort(sort.Interface data) { sort.Sort(data); }

public static nint Run() { return 4; }

internal static void others() {
    var w = new words(new uint64[]{3, 1, 2}.slice());
    w.Sort();

    var run = Run;
    any boxed = Run;
    var (_, isFunc) = boxed._<Func<nint>>(ᐧ);

    fmt.Println("others", w, w.cap(), cap(w), w.bits(), bits.Len64(w[0]), new gauge(n: 5).size(w), run(), isFunc, w.Run());
}

} // end main_package
