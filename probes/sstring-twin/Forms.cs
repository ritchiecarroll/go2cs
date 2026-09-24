using System;
using go;

namespace SStringTwinProbe;

// One method per call form, each under its own symbol so a compiler error names exactly one form
// (by the FORM_x line it lands on) and pass 2 can run the forms that compiled.
static partial class Probe
{
#if FORM_A
    // (a) a u8 literal
    public static void FormA() { F.Sprintf("x %d"u8, 1); }
#endif

#if FORM_B
    // (b) an @string variable
    public static void FormB() { @string s = "x %d"; F.Sprintf(s, 1); }
#endif

#if FORM_C
    // (c) a C# string literal
    public static void FormC() { F.Sprintf("x %d", 1); }
#endif

#if FORM_D
    // (d) an sstring variable
    public static void FormD() { sstring v = "x %d"; F.Sprintf(v, 1); }
#endif

#if FORM_E
    // (e) a method-group conversion to the golib variadic delegate; must bind the @string member
    public static void FormE() { Funcꓸꓸꓸ<@string, any, @string> f = F.Sprintf; f("x %d", 1); }
#endif

#if FORM_E2
    // (e2, control) the same conversion from the pair WITHOUT the priority attribute
    public static void FormE2() { Funcꓸꓸꓸ<@string, any, @string> f = G.Sprintf; f("x %d", 1); }
#endif

#if FORM_E3
    // (e3, control) an explicit lambda wrapper in place of the method group
    public static void FormE3() { Funcꓸꓸꓸ<@string, any, @string> f = (@string format, Span<any> a) => F.Sprintf(format, a); f("x %d", 1); }
#endif

#if FORM_E4
    // (e4, control) an explicit cast of the method group
    public static void FormE4() { var f = (Funcꓸꓸꓸ<@string, any, @string>)F.Sprintf; f("x %d", 1); }
#endif

#if FORM_F
    // (f) a method group's natural type
    public static void FormF() { var g = F.Sprintf; Bound = "natural type: " + g.GetType().Name; }
#endif

#if FORM_G
    // (g) a lambda capturing an sstring parameter (expected: an error, the ref-struct rule)
    public static void FormG() { Capture("x"); }

    static int Capture(sstring p) { Func<int> f = () => (int)p.Length; return f(); }
#endif

#if FORM_H
    // (h, extra) a u8 literal assigned straight to an sstring local
    public static void FormH() { sstring v = "x %d"u8; Bound = "assigned, len " + v.Length; }
#endif

#if FORM_N1
    // (n1) the pair WITHOUT the priority attribute, called with a u8 literal
    public static void FormN1() { G.Sprintf("x %d"u8, 1); }
#endif

#if FORM_N2
    // (n2) a u8 literal into a parameter typed object
    public static void FormN2() { H.TakeObject("x"u8); }
#endif

#if FORM_N3
    // (n3) a u8 literal into a parameter typed any
    public static void FormN3() { H.TakeAny("x"u8); }
#endif
}
