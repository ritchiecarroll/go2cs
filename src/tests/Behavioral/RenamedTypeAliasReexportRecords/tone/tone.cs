namespace go.RenamedTypeAliasReexportRecords;

using fmt = fmt_package;

partial class tone_package {

partial struct Tone /*num:nint*/;

internal static nint Tone⁔(/*this*/ Tone t) { return (nint)t * 10; }

internal static @string String⁔(/*this*/ Tone t) { return fmt.Sprintf("tone %d"u8, (nint)t); }

partial struct Pitch {
    public nint Hz;
}

internal static Pitch Pitch⁔(/*this*/ Tone t) { return new Pitch(Hz: 400 + (nint)t * 10); }

internal static void Raise⁔(/*this*/ ref Pitch p, nint by) { p.Hz += by; }

internal static @string Label⁔(/*this*/ ref Pitch p) { return fmt.Sprintf("pitch %d Hz"u8, p.Hz); }

} // end tone_package
