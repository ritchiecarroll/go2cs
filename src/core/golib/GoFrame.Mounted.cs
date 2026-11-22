//******************************************************************************************************
//  GoFrame.Mounted.cs - M:N SPIKE (R5b, B1). Spike code: it stays on the spike branch and never lands.
//
//  GoFrame's share of Goroutine.Mount / Unmount. It sits in its own file so that no line of GoFrame.cs
//  moves under the documents that cite it.
//******************************************************************************************************

namespace go;

public partial struct GoFrame
{
    // Exchanges this thread's activation counter, token and refusal with a goroutine's saved ones.
    internal static void SwapMounted(ref long activation, ref object? token, ref bool refused)
    {
        (activation, t_lastActivation) = (t_lastActivation, activation);
        (token, t_threadToken) = (t_threadToken, token);
        (refused, t_splicesRefused) = (t_splicesRefused, refused);
    }
}
