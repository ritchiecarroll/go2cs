// The hand-written async TWIN of Pkg.Work, as go2cs-gen would emit it beside the visible function (O6), WITHOUT
// #line: its sequence points stay in this file, so a breakpoint set in Visible.cs cannot bind inside it (G5's red).
// TwinLined.cs is the same twin with #line; twinlines.csproj compiles one or the other (TWIN_LINE).
using System.Threading.Tasks;

namespace TwinLines;

public static partial class Pkg
{
    public static async ValueTask<int> Work_twin(Chan c)
    {
        int v = await c.ReceiveAsync();
        return v + 1;
    }
}
