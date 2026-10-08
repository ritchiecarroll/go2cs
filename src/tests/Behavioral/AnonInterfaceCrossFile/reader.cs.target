namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Sizer {
    nint Size();
}

partial interface Namer {
    @string Name();
}

internal partial interface describe_thing /*dyn*/ :
    Sizer,
    Namer
{
}

internal static @string describe(describe_thing thing) {
    return fmt.Sprintf("%s(%d)"u8, thing.Name(), thing.Size());
}

} // end main_package
