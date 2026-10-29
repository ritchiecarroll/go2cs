namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var linesᴷ = new stackAppend<slice<nint>>();
    var mapsᴷ = new stackAppend<map<@string, nint>>();
    var ptrsᴷ = new stackAppend<ж<nint>>();

    slice<slice<nint>> lines = default;
    lines = append(linesᴷ, lines, (slice<nint>)(default));
    lines = append(lines, (slice<nint>)(default));
    lines[0] = append(lines[0], (nint)7);
    fmt.Println(len(lines), lines[0], lines[1] == nil);

    slice<map<@string, nint>> maps = default;
    maps = append(mapsᴷ, maps, (map<@string, nint>)(default));
    fmt.Println(len(maps), maps[0] == nil);

    slice<ж<nint>> ptrs = default;
    ptrs = append(ptrsᴷ, ptrs, (ж<nint>)(nil));
    fmt.Println(len(ptrs), ptrs[0] == nil);
}

} // end main_package
