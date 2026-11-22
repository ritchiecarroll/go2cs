namespace go.MnGlyphMethodFrames;

using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;

partial class glyphlib_package {

partial struct Box {
    internal channel<nint> ch;
    internal slice<@string> trail;
}

public static ж<Box> New() {
    return Ꮡ(new Box(ch: new channel<nint>(0)));
}

internal static void Put⁔(/*this*/ ref Box b, nint v) {
    b.ch.ᐸꟷ(v);
}

internal static partial nint Take⁔(/*this*/ ref Box b) {
    nint v = ᐸꟷ(b.ch);
    b.trail = append(b.trail, Frames("Take"));

    return v;
}

internal static slice<@string> Trail⁔(/*this*/ ref Box b) {
    return b.trail;
}

internal static partial @string Pass⁔(/*this*/ Box b, nint n) {
    return Frames("Pass "u8 + strconv.Itoa(n));
}

public static partial @string Frames(@string tag) {
    var namesᴷ = new stackAppend<@string>();

    var pcs = new slice<uintptr>(32);
    nint n = runtime.Callers(2, pcs);
    var frames = runtime.CallersFrames(pcs.slice(0, n));

    slice<@string> names = default;

    for (;;) {
        var (frame, more) = frames.Next();

        if (strings.HasPrefix(frame.Function, "runtime.")) {
            break;
        }

        names = append(namesᴷ, names, frame.Function + ":"u8 + strconv.Itoa(frame.Line));

        if (!more || frame.Function == "main.main"u8) {
            break;
        }
    }

    return tag + ": "u8 + strings.Join(names, " <- ");
}

} // end glyphlib_package
