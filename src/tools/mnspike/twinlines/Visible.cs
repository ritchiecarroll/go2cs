// The VISIBLE converted code: synchronous and Go-shaped, as a reader sees it. The M:N spike's G5 asks whether a
// breakpoint set on a line here binds when the program runs this function's async TWIN (Twin.cs).
namespace TwinLines;

public static partial class Pkg
{
    public static int Work(Chan c)
    {
        int v = c.Receive();
        return v + 1;
    }
}
