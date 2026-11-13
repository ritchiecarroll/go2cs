// alloctool <file.nettrace> [top]: allocation samples (GCAllocationTick, about one per 100 KB allocated) by type and
// by allocating frame, plus GC counts and pause time, from a trace taken with the gc-verbose profile.
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

string path = args[0];
int top = args.Length > 1 ? int.Parse(args[1]) : 40;
string etlx = TraceLog.CreateFromEventPipeDataFile(path);
using TraceLog log = new(etlx);
var byType = new Dictionary<string, long>();
var byFrame = new Dictionary<string, long>();
var byTypeFrame = new Dictionary<string, long>();
long total = 0;
int gcs = 0, gen2 = 0;
double pauseMs = 0;
var startTimes = new Dictionary<int, double>();
TraceLogEventSource source = log.Events.GetSource();
source.Clr.GCAllocationTick += (GCAllocationTickTraceData d) =>
{
    long amount = d.AllocationAmount64;
    total += amount;
    string type = d.TypeName ?? "?";
    byType[type] = byType.GetValueOrDefault(type) + amount;
    TraceCallStack? cs = d.CallStack();
    string frame = "?";
    var chain = new List<string>();
    for (TraceCallStack? s = cs; s != null && chain.Count < 12; s = s.Caller)
    {
        string m = s.CodeAddress.FullMethodName ?? "";
        if (m.Length == 0) continue;
        chain.Add(m);
    }
    // The allocating frame: the first frame outside the runtime, golib and the BCL.
    foreach (string m in chain)
    {
        if (m.StartsWith("System.") || m.StartsWith("Microsoft.")) continue;
        frame = m;
        break;
    }
    string golibFrame = chain.FirstOrDefault(m => m.StartsWith("go.") && !m.StartsWith("go.github")) ?? "";
    string key = frame.Length > 140 ? frame[..140] : frame;
    byFrame[key] = byFrame.GetValueOrDefault(key) + amount;
    string tk = type + "  @  " + (golibFrame.Length > 0 ? golibFrame[..Math.Min(90, golibFrame.Length)] + "  <-  " : "") + key[..Math.Min(90, key.Length)];
    byTypeFrame[tk] = byTypeFrame.GetValueOrDefault(tk) + amount;
};
source.Clr.GCStart += d => { gcs++; if (d.Depth == 2) gen2++; };
source.Clr.GCSuspendEEStart += d => startTimes[d.ThreadID] = d.TimeStampRelativeMSec;
source.Clr.GCRestartEEStop += d =>
{
    if (startTimes.Count > 0) { pauseMs += d.TimeStampRelativeMSec - startTimes.Values.Min(); startTimes.Clear(); }
};
source.Process();
Console.WriteLine($"sampled allocation {total / 1048576.0:N0} MB; GCs {gcs} (gen2 {gen2}); suspended {pauseMs:N0} ms");
void Print(string title, Dictionary<string, long> d)
{
    Console.WriteLine($"-- {title}");
    foreach (var kv in d.OrderByDescending(kv => kv.Value).Take(top))
        Console.WriteLine($"  {100.0 * kv.Value / Math.Max(total, 1),5:F1}% {kv.Value / 1048576.0,9:N0} MB  {kv.Key}");
}
Print("by type", byType);
Print("by allocating frame (first frame outside System/Microsoft)", byFrame);
Print("by type @ golib frame <- allocating frame", byTypeFrame);
