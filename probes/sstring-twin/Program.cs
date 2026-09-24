using System;

namespace SStringTwinProbe;

static partial class Probe
{
    static void Run(string name, Action form)
    {
        Bound = "(none)";
        try
        {
            form();
            Console.WriteLine($"{name}: {Bound}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name}: THREW {ex.GetType().Name}");
        }
    }

    static void Main()
    {
#if FORM_A
        Run("a", FormA);
#endif
#if FORM_B
        Run("b", FormB);
#endif
#if FORM_C
        Run("c", FormC);
#endif
#if FORM_D
        Run("d", FormD);
#endif
#if FORM_E
        Run("e", FormE);
#endif
#if FORM_E2
        Run("e2", FormE2);
#endif
#if FORM_E3
        Run("e3", FormE3);
#endif
#if FORM_E4
        Run("e4", FormE4);
#endif
#if FORM_F
        Run("f", FormF);
#endif
#if FORM_G
        Run("g", FormG);
#endif
#if FORM_H
        Run("h", FormH);
#endif
#if FORM_N1
        Run("n1", FormN1);
#endif
#if FORM_N2
        Run("n2", FormN2);
#endif
#if FORM_N3
        Run("n3", FormN3);
#endif
        Console.WriteLine("done");
    }
}
