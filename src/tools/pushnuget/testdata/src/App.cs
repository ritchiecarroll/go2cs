// App: a platform-neutral package. ELEM is Alpha or Bravo, the two equal-length names, so the two builds have the
// same length and different type references -- the go.internal.coverage.decodemeta shape (1.24.13.6 rehearsal).
namespace App;

public static class Registry
{
    public static System.Type Element() => typeof(Dep.ELEM[]);

    public static string Label() => "neutral";
}
