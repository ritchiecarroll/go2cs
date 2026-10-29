namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

partial struct words /*[]@string*/;

internal static nint Len(this words w) {
    return len(w);
}

internal static bool Less(this words w, nint i, nint j) {
    return w[i] < w[j];
}

internal static void Swap(this words w, nint i, nint j) {
    (w[i], w[j]) = (w[j], w[i]);
}

public static void Sort(sort.Interface data) {
    sort.Sort(data);
}

internal static void Sort(this words w) {
    Sort((sort.Interface)(w));
}

internal static void Main() {
    var ints = new sort.IntSlice(new nint[]{3, 1, 2}.slice());
    ints.Sort();
    fmt.Println("IntSlice.Sort:", ints);

    var floats = new sort.Float64Slice(new float64[]{2.5D, -1D, 0.5D}.slice());
    floats.Sort();
    fmt.Println("Float64Slice.Sort:", floats);

    var strs = new sort.StringSlice(new @string[]{"pear", "apple", "fig"}.slice());
    strs.Sort();
    fmt.Println("StringSlice.Sort:", strs);

    var names = slice<@string>(["carol", "alice", "bob"]);
    sort.Sort((sort.Interface)((sort.StringSlice)names));
    fmt.Println("sort.Sort(sort.StringSlice(v)):", names);

    var w = new words(new @string[]{"delta", "alpha", "charlie"}.slice());
    w.Sort();
    fmt.Println("user words.Sort:", w);
}

} // end main_package
