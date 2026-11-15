namespace go.RenamedTypeAliasReexportRecords;

using fmt = fmt_package;

partial class tone_package {

partial struct ΔTone /*num:nint*/;

public static nint Tone(this ΔTone t) { return (nint)t * 10; }

public static @string String(this ΔTone t) { return fmt.Sprintf("tone %d"u8, (nint)t); }

partial struct ΔPitch {
    public nint Hz;
}

public static ΔPitch Pitch(this ΔTone t) { return new ΔPitch(Hz: 400 + (nint)t * 10); }

public static void Raise(this ref ΔPitch p, nint by) { p.Hz += by; }

public static @string Label(this ref ΔPitch p) { return fmt.Sprintf("pitch %d Hz"u8, p.Hz); }

} // end tone_package
