//******************************************************************************************************
//  builtin.Twins.cs - M:N SPIKE (R5b, B1). Spike code: it stays on the spike branch and never lands.
//
//  The hidden twins of builtin's parking members. They sit in their own file so that no line of builtin.cs
//  moves under the documents that cite it.
//******************************************************************************************************

using System.Threading.Tasks;
using go.golib;

namespace go;

public static partial class builtin
{
    // The hidden twin of select, which a twin awaits.
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ValueTask<int> selectꞏtwin(params SelectOp[] ops)
    {
        return SelectRuntime.RunTwin(ops);
    }
}
