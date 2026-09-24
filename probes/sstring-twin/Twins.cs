using System;
using System.Runtime.CompilerServices;
using go;

namespace SStringTwinProbe;

// The twin pair as COORD specified it: @string and sstring overloads, the sstring member given
// OverloadResolutionPriority(1). Each body reports which overload ran.
static class F
{
    public static @string Sprintf(@string format, params ꓸꓸꓸany a)
    {
        Probe.Bound = "@string";
        return format;
    }

    [OverloadResolutionPriority(1)]
    public static @string Sprintf(sstring format, params ꓸꓸꓸany a)
    {
        Probe.Bound = "sstring";
        return format;
    }
}

// The same pair WITHOUT the priority attribute (the section 8.2R.2 negative case).
static class G
{
    public static @string Sprintf(@string format, params ꓸꓸꓸany a)
    {
        Probe.Bound = "@string";
        return format;
    }

    public static @string Sprintf(sstring format, params ꓸꓸꓸany a)
    {
        Probe.Bound = "sstring";
        return format;
    }
}

// A single-parameter sink typed object / any (any is the object alias, as in the corpus).
static class H
{
    public static void TakeObject(object o) => Probe.Bound = "object";

    public static void TakeAny(any o) => Probe.Bound = "any";
}

static partial class Probe
{
    public static string Bound = "(none)";
}
