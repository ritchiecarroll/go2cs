// A .NET start-up hook (DOTNET_STARTUP_HOOKS) that attributes a converted program's package initialization.
//   F1_APP   the program's assembly name (required)
//   F1_OUT   file to write (default: standard error)
//   F1_MODE  "init" (default): run each assembly's module initializer in dependency order, leaf first, and record
//            wall, JIT time, methods and IL bytes compiled, and bytes allocated for each;
//            "jit": the same, with every method's JIT duration from the runtime's JIT events;
//            "off": load nothing (the hook's own floor).
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

internal static class StartupHook
{
    private static readonly StringBuilder s_out = new();
    private static JitListener? s_listener;

    public static void Initialize()
    {
        string mode = Environment.GetEnvironmentVariable("F1_MODE") ?? "init";

        if (mode == "off" || Environment.GetEnvironmentVariable("F1_APP") is not { } app)
            return;

        long hookStart = Stopwatch.GetTimestamp();

        if (mode == "jit")
            s_listener = new JitListener();

        List<Assembly> order = [];
        HashSet<string> seen = new(StringComparer.Ordinal);
        Dictionary<string, double> loadMs = [];

        Visit(new AssemblyName(app), order, seen, loadMs);

        s_out.AppendLine("row\tassembly\twall_ms\tjit_ms\tmethods\til_bytes\talloc_bytes\tload_ms\tassemblies_loaded");

        foreach (Assembly assembly in order)
        {
            int loaded = AppDomain.CurrentDomain.GetAssemblies().Length;
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            TimeSpan jit = JitInfo.GetCompilationTime(true);
            long methods = JitInfo.GetCompiledMethodCount(true), il = JitInfo.GetCompiledILBytes(true);
            long start = Stopwatch.GetTimestamp();

            s_listener?.Mark($"begin {assembly.GetName().Name}");

            try
            {
                RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
            }
            catch (Exception error)
            {
                s_out.AppendLine($"error\t{assembly.GetName().Name}\t{error.GetType().Name}: {error.InnerException?.Message ?? error.Message}");
            }

            double wall = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            s_out.AppendLine(string.Join('\t', "init", assembly.GetName().Name, wall.ToString("F3"),
                (JitInfo.GetCompilationTime(true) - jit).TotalMilliseconds.ToString("F3"),
                JitInfo.GetCompiledMethodCount(true) - methods, JitInfo.GetCompiledILBytes(true) - il,
                GC.GetAllocatedBytesForCurrentThread() - allocated, loadMs[assembly.GetName().Name!].ToString("F3"),
                AppDomain.CurrentDomain.GetAssemblies().Length - loaded));
        }

        s_listener?.Mark("end");
        s_out.AppendLine($"total\thook\t{Stopwatch.GetElapsedTime(hookStart).TotalMilliseconds:F3}\t{JitInfo.GetCompilationTime(true).TotalMilliseconds:F3}\t{JitInfo.GetCompiledMethodCount(true)}\t{JitInfo.GetCompiledILBytes(true)}\t{GC.GetAllocatedBytesForCurrentThread()}\t\t{AppDomain.CurrentDomain.GetAssemblies().Length}");
        s_out.AppendLine($"total\tprocess-to-hook-end\t{(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds:F0}");

        // The converted program may leave through a native exit that runs no managed exit handler: write now.
        Flush();
    }

    // Loads the converted closure of `name` (the program, golib and every go.* package), dependencies first.
    private static void Visit(AssemblyName name, List<Assembly> order, HashSet<string> seen, Dictionary<string, double> loadMs)
    {
        if (!seen.Add(name.Name!))
            return;

        long start = Stopwatch.GetTimestamp();
        Assembly assembly;

        try
        {
            assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(name);
        }
        catch (Exception error)
        {
            s_out.AppendLine($"error\t{name.Name}\tload {error.GetType().Name}");
            return;
        }

        loadMs[name.Name!] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
        {
            string simple = reference.Name!;

            if (simple == "golib" || simple.StartsWith("go.", StringComparison.Ordinal))
                Visit(reference, order, seen, loadMs);
        }

        order.Add(assembly);
    }

    private static void Flush()
    {
        if (s_listener is { } listener)
        {
            Thread.Sleep(300); // the runtime's events arrive on a dispatch thread
            listener.Write(s_out);
        }

        if (Environment.GetEnvironmentVariable("F1_OUT") is { } path)
            File.WriteAllText(path, s_out.ToString());
        else
            Console.Error.Write(s_out.ToString());
    }

    // Every method's JIT duration: MethodJittingStarted to MethodLoadVerbose, by method id.
    private sealed class JitListener : EventListener
    {
        private readonly Dictionary<ulong, (DateTime start, string name, uint il)> m_started = [];
        private readonly List<(DateTime at, string text)> m_rows = [];

        protected override void OnEventSourceCreated(EventSource source)
        {
            if (source.Name == "Microsoft-Windows-DotNETRuntime")
                EnableEvents(source, EventLevel.Verbose, (EventKeywords)0x10);
        }

        public void Mark(string text)
        {
            lock (m_rows)
                m_rows.Add((DateTime.UtcNow, $"mark\t{text}"));
        }

        protected override void OnEventWritten(EventWrittenEventArgs data)
        {
            if (data.Payload is not { } payload || data.PayloadNames is not { } names)
                return;

            object? field(string name)
            {
                int index = names.IndexOf(name);
                return index < 0 ? null : payload[index];
            }

            lock (m_rows)
            {
                switch (data.EventName)
                {
                    case "MethodJittingStarted_V1" or "MethodJittingStarted":
                        m_started[(ulong)field("MethodID")!] = (data.TimeStamp, $"{field("MethodNamespace")}\t{field("MethodName")}", (uint)field("MethodILSize")!);
                        break;
                    case "MethodLoadVerbose_V1" or "MethodLoadVerbose_V2" or "MethodLoadVerbose":
                        if (m_started.Remove((ulong)field("MethodID")!, out (DateTime start, string name, uint il) started))
                            m_rows.Add((started.start, $"jit\t{started.name}\t{(data.TimeStamp - started.start).TotalMilliseconds:F3}\t{started.il}\t{data.OSThreadId}"));
                        break;
                }
            }
        }

        public void Write(StringBuilder output)
        {
            lock (m_rows)
            {
                output.AppendLine("row\tnamespace\tmethod\tjit_ms\til_bytes\tthread");

                foreach ((DateTime at, string text) in m_rows.OrderBy(static row => row.at))
                    output.AppendLine(text);
            }
        }
    }
}
