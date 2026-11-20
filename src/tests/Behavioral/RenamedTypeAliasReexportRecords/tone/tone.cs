namespace go.RenamedTypeAliasReexportRecords;

using fmt = fmt_package;
using static tone_package;

partial class tone_package {

partial struct Tone /*num:nint*/;

} partial class toneᴛTone {

public static nint Tone(this Tone t) { return (nint)t * 10; }

public static @string String(this Tone t) { return fmt.Sprintf("tone %d"u8, (nint)t); }

} partial class tone_package {

partial struct Pitch {
    public nint Hz;
}

} partial class toneᴛTone {

public static Pitch Pitch(this Tone t) { return new Pitch(Hz: 400 + (nint)t * 10); }

} partial class toneᴛPitch {

public static void Raise(this ref Pitch p, nint by) { p.Hz += by; }

public static @string Label(this ref Pitch p) { return fmt.Sprintf("pitch %d Hz"u8, p.Hz); }

} // end toneᴛPitch
