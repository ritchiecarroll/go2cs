namespace go.MethodHolderAlias;

partial class tone_package {

partial struct tone /*num:nint*/;

internal static nint Pitch⁔(/*this*/ tone t) { return Pitch((nint)t) + (nint)Base; }

internal static nint Base⁔(/*this*/ tone t) { return Base * 2; }

public static UntypedInt Base => 100;

public static nint Pitch(nint n) { return n * 11; }

public static (nint, nint) Play(nint n) { return (((tone)n).Pitch(), ((tone)n).Base()); }

} // end tone_package
