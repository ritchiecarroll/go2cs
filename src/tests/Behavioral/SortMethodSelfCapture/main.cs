namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

[GoType("[]@string")] partial struct words;

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object intSliceSortˢ = (@string)"IntSlice.Sort:"u8;
private static readonly object float64SliceSortˢ = (@string)"Float64Slice.Sort:"u8;
private static readonly object stringSliceSortˢ = (@string)"StringSlice.Sort:"u8;
private static readonly object sortSortSortStringSliceVˢ = (@string)"sort.Sort(sort.StringSlice(v)):"u8;
private static readonly object userWordsSortˢ = (@string)"user words.Sort:"u8;

internal static void Main() {
    var ints = new sort.IntSlice(new nint[]{3, 1, 2}.slice());
    ints.Sort();
    fmt.Println(intSliceSortˢ, ints);
    var floats = new sort.Float64Slice(new float64[]{2.5D, -1D, 0.5D}.slice());
    floats.Sort();
    fmt.Println(float64SliceSortˢ, floats);
    var strs = new sort.StringSlice(new @string[]{"pear"u8, "apple"u8, "fig"u8}.slice());
    strs.Sort();
    fmt.Println(stringSliceSortˢ, strs);
    var names = new @string[]{"carol"u8, "alice"u8, "bob"u8}.slice();
    sort.Sort((sort.Interface)(((sort.StringSlice)names)));
    fmt.Println(sortSortSortStringSliceVˢ, names);
    var w = new words(new @string[]{"delta"u8, "alpha"u8, "charlie"u8}.slice());
    w.Sort();
    fmt.Println(userWordsSortˢ, w);
}

} // end main_package
