// The same async TWIN as Twin.cs, WITH #line: each line of the twin names the visible line it came from, braces
// included, so the twin's sequence points land on Visible.cs's lines and a breakpoint set there binds in both bodies.
// This is the mapping go2cs-gen would emit (DESIGN-mn-scheduler §5 O6, the debugging row).
using System.Threading.Tasks;

namespace TwinLines;

public static partial class Pkg
{
#line 7 "Visible.cs"
    public static async ValueTask<int> Work_twin(Chan c)
#line 8 "Visible.cs"
    {
#line 9 "Visible.cs"
        int v = await c.ReceiveAsync();
#line 10 "Visible.cs"
        return v + 1;
#line 11 "Visible.cs"
    }
#line default
}
