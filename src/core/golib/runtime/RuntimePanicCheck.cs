// RuntimePanicCheck.cs - Gbtc (COUNT-ONLY MEASUREMENT BUILD -- not for commit)

using System;
using System.Diagnostics;
using System.IO;

namespace go.golib;

internal static class RuntimePanicCheck
{
    private static readonly string? s_log = Environment.GetEnvironmentVariable("GO2CS_A8_COUNT_LOG");

    internal static void Check(string throwText)
    {
        if (s_log is null)
            return;

        bool hit = RaisedInRuntimePackage(out string frame);

        try
        {
            File.AppendAllText(s_log, $"{(hit ? "HIT" : "MISS")}\t{throwText}\t{frame}\t{Environment.ProcessId}\n");
        }
        catch (Exception)
        {
        }
    }

    internal static bool RaisedInRuntimePackage(out string frameName)
    {
        frameName = "";
        StackTrace trace = new(1, false);

        for (int i = 0; i < trace.FrameCount; i++)
        {
            System.Reflection.MethodBase? method = trace.GetFrame(i)?.GetMethod();
            Type? type = method?.DeclaringType;

            if (type is null || type.Assembly == typeof(RuntimePanicCheck).Assembly)
                continue;

            frameName = $"{type.FullName}.{method!.Name}";

            for (Type? outer = type; outer is not null; outer = outer.DeclaringType)
            {
                if (outer.FullName == "go.runtime_package")
                    return true;
            }

            return false;
        }

        return false;
    }
}
