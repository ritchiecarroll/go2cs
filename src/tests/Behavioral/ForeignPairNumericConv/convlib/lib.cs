namespace go.ForeignPairNumericConv;

using static convlib_package;

partial class convlib_package {

partial struct Handle /*num:uintptr*/;

partial struct Token /*num:uintptr*/;

public static Token NewToken(uintptr v) { return (Token)v; }

} partial class convlibᴛHandle {

public static @string String(this Handle h) { return "handle"; }

} partial class convlibᴛToken {

public static uintptr Raw(this Token t) { return (uintptr)t; }

} // end convlibᴛToken
