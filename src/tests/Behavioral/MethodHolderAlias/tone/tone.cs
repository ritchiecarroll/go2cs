namespace go.MethodHolderAlias;

using static tone_package;

partial class tone_package {

partial struct tone /*num:nint*/;

} partial class toneᴛtone {

internal static nint Pitch(this tone t) { return tone_package.Pitch((nint)t) + (nint)tone_package.Base; }

internal static nint Base(this tone t) { return tone_package.Base * 2; }

} partial class tone_package {

public static UntypedInt Base => 100;

public static nint Pitch(nint n) { return n * 11; }

public static (nint, nint) Play(nint n) { return (((tone)n).Pitch(), ((tone)n).Base()); }

} // end tone_package
