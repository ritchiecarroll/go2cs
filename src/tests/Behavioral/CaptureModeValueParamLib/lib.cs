namespace go;

using fmt = fmt_package;

partial class CaptureModeValueParamLib_package {

partial struct Config {
    public nint Indent;
    internal @string trace;
}

internal static (@string @out, error err) fprint⁔(/*this*/ ж<Config> Ꮡcfg, @string label) {
    @string @out = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var cfg = ref Ꮡcfg.DerefOrNull();

        defer([GoFunc("(*Config).fprint.func1")] () => {
            var e = recover(); if (e != nil) {
                err = fmt.Errorf("panic: %v"u8, e);
            }
        }, ref ᒐ);
        cfg.trace = fmt.Sprintf("%s|%s"u8, cfg.trace, label);
        (@out, err) = (fmt.Sprintf("%s@%d"u8, label, cfg.Indent), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (@out, err);
}

internal static (@string, error) Fprint⁔(/*this*/ ж<Config> Ꮡcfg, @string label) {
    return Ꮡcfg.fprint(label);
}

internal static @string Trace⁔(/*this*/ ref Config cfg) {
    return cfg.trace;
}

} // end CaptureModeValueParamLib_package
