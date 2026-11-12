namespace go.DotImportRenamedType;

using fmt = fmt_package;
using static renamedlib_package;

partial class renamedlib_package {

partial struct Marker {
    public @string Name;
    public nint Size;
}

} partial class renamedlibᴛMarker {

public static @string Marker(this Marker m) {
    return fmt.Sprintf("%s/%d"u8, m.Name, m.Size);
}

} partial class renamedlib_package {

partial struct Detail {
    public @string Label;
    public nint Rank;
}

} partial class renamedlibᴛDetail {

public static @string Show(this Detail d) {
    return fmt.Sprintf("%s#%d"u8, d.Label, d.Rank);
}

} partial class renamedlibᴛMarker {

public static Detail Detail(this Marker m) {
    return new Detail(Label: m.Name, Rank: m.Size);
}

} partial class renamedlib_package {

partial struct Plain {
    public @string Note;
}

public static any Describe(@string name, nint size) {
    return new Marker(Name: name, Size: size);
}

public static any Wrap(@string label, nint rank) {
    return new Detail(Label: label, Rank: rank);
}

public static any Boxed(@string note) {
    return new Plain(Note: note);
}

} // end renamedlib_package
