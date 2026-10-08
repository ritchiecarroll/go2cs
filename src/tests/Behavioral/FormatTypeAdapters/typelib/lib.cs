namespace go.FormatTypeAdapters;

partial class typelib_package {

partial struct Mark {
    public @string Tag;
}

public static Mark NewMark(@string tag) {
    return new Mark(Tag: tag);
}

public static @string Stamp(this Mark m) {
    return "mark:"u8 + m.Tag;
}

} // end typelib_package
