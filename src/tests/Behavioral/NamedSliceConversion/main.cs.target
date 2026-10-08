namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct CaseRange {
    public uint32 Lo, Hi;
}

partial struct SpecialCase /*[]CaseRange*/;

partial struct ints /*[]nint*/;

internal static nint count(slice<CaseRange> rs) {
    return len(rs);
}

internal static nint /*total*/ sum(slice<nint> s) {
    nint total = default!;

    foreach (var (_, v) in s) {
        total += v;
    }
    return total;
}

internal static void Main() {
    var special = new SpecialCase(new CaseRange[]{new(1, 2), new(3, 4), new(5, 6)}.slice());
    fmt.Println(count(((slice<CaseRange>)special)));
    var n = new ints(new nint[]{10, 20, 30}.slice());
    fmt.Println(sum(((slice<nint>)n)));
    slice<CaseRange> rs = ((slice<CaseRange>)special);
    fmt.Println(len(rs), rs[2].Lo);
}

} // end main_package
