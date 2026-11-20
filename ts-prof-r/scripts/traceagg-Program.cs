using System.Text.RegularExpressions;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

// traceagg <trace.nettrace> [--phase <frame-substring>] [--top N] [--query <frame-regex>]... [--alloc-type <type-regex>]...
//
// Every table is computed over the events whose stack contains the --phase frame (default: every event). Frames are
// classed by module: FRAMEWORK (System.*, Microsoft.*, coreclr), GOLIB (golib / go.lib), STD (other go.* modules: the
// converted Go standard library), APP (everything else: the converted program).
//
// CPU (EventPipe sample profiler): exclusive and inclusive samples by method; inclusive limited to GOLIB frames.
// Allocations (GCAllocationTick, ~100 KB sampled): bytes by type; by allocating method (first non-FRAMEWORK frame); and
// for each --alloc-type regex, by (type, first GOLIB frame, nearest APP frame).
// --query regex: inclusive samples whose stack holds a matching frame, split by the nearest map`2[K,...] frame's K and by
// the nearest APP caller.
// GC: counts by generation, pause total (SuspendEE start -> RestartEE stop), and peak heap from GCHeapStats.

var a = args.ToList();
string path = a[0];
string phase = Opt("--phase"); var phaseRe = phase == null ? null : new Regex(phase); int top = int.Parse(Opt("--top") ?? "40");
var queries = Opts("--query").Select(q => new Regex(q)).ToList();
var allocTypes = Opts("--alloc-type").Select(q => new Regex(q)).ToList();

string Opt(string name) { int i = a.IndexOf(name); return i >= 0 && i + 1 < a.Count ? a[i + 1] : null; }
IEnumerable<string> Opts(string name) { for (int i = 0; i < a.Count - 1; i++) if (a[i] == name) yield return a[i + 1]; }

string etlx = TraceLog.CreateFromEventPipeDataFile(path);
using var log = new TraceLog(etlx);

static string Name(TraceCallStack s) => s.CodeAddress.FullMethodName is { Length: > 0 } m ? $"{Mod(s)}!{m}" : $"{Mod(s)}!?";
static string Mod(TraceCallStack s) { var m = s.CodeAddress.ModuleName ?? "?"; return m.EndsWith(".il") ? m[..^3] : m; }
static string Class(TraceCallStack s)
{
    string m = Mod(s);
    if (m.StartsWith("System") || m.StartsWith("Microsoft") || m == "coreclr" || m == "?" || m.StartsWith("clr")) return "FRAMEWORK";
    if (m == "golib" || m == "go.lib") return "GOLIB";
    if (m.StartsWith("go.")) return "STD";
    return "APP";
}

var excl = new Dictionary<string, long>(); var incl = new Dictionary<string, long>(); var inclGolib = new Dictionary<string, long>();
long samples = 0, phaseSamples = 0;
var byType = new Dictionary<string, (double b, long n)>(); var bySite = new Dictionary<string, (double b, long n)>();
var byTypeSite = new Dictionary<string, (double b, long n)>(); double allocTotal = 0, allocAll = 0;
var qTotal = new Dictionary<string, long>(); var qByKey = new Dictionary<string, long>(); var qByCaller = new Dictionary<string, long>();
var mapKey = new Regex(@"map`2\[([^,\]]+(?:\[[^\]]*\])?)");
int[] gcByGen = new int[3]; double pauseTotal = 0, suspendStart = -1, peakHeap = 0; double gcFirst = -1, gcLast = -1;

foreach (var ev in log.Events)
{
    if (ev.ProviderName == "Microsoft-DotNETCore-SampleProfiler" && ev.EventName.StartsWith("Thread/Sample"))
    {
        var cs = ev.CallStack(); if (cs == null) continue; samples++;
        var frames = new List<TraceCallStack>(); for (var s = cs; s != null; s = s.Caller) frames.Add(s);
        var names = frames.Select(Name).ToList();
        if (phaseRe != null && !names.Any(n => phaseRe.IsMatch(n))) continue;
        phaseSamples++;
        excl[names[0]] = excl.GetValueOrDefault(names[0]) + 1;
        foreach (var (n, i) in names.Select((n, i) => (n, i)).DistinctBy(x => x.n))
        {
            incl[n] = incl.GetValueOrDefault(n) + 1;
            if (Class(frames[i]) == "GOLIB") inclGolib[n] = inclGolib.GetValueOrDefault(n) + 1;
        }
        foreach (var q in queries)
        {
            int hit = names.FindIndex(n => q.IsMatch(n)); if (hit < 0) continue;
            string qk = q.ToString(); qTotal[qk] = qTotal.GetValueOrDefault(qk) + 1;
            string key = "-"; for (int i = hit; i < names.Count; i++) { var m = mapKey.Match(names[i]); if (m.Success) { key = m.Groups[1].Value; break; } }
            string caller = "-"; for (int i = hit; i < frames.Count; i++) if (Class(frames[i]) == "APP") { caller = names[i]; break; }
            qByKey[$"{qk} | K={key}"] = qByKey.GetValueOrDefault($"{qk} | K={key}") + 1;
            qByCaller[$"{qk} | {caller}"] = qByCaller.GetValueOrDefault($"{qk} | {caller}") + 1;
        }
    }
    else if (ev is GCAllocationTickTraceData at)
    {
        double b = at.AllocationAmount64; allocAll += b;
        var frames = new List<TraceCallStack>(); for (var s = ev.CallStack(); s != null; s = s.Caller) frames.Add(s);
        var names = frames.Select(Name).ToList();
        if (phaseRe != null && !names.Any(n => phaseRe.IsMatch(n))) continue;
        allocTotal += b; string t = at.TypeName ?? "?";
        var v = byType.GetValueOrDefault(t); byType[t] = (v.b + b, v.n + 1);
        int si = frames.FindIndex(f => Class(f) != "FRAMEWORK"); string site = si >= 0 ? names[si] : "?";
        var w = bySite.GetValueOrDefault(site); bySite[site] = (w.b + b, w.n + 1);
        if (allocTypes.Any(r => r.IsMatch(t)))
        {
            int gi = frames.FindIndex(f => Class(f) == "GOLIB"); int ai = frames.FindIndex(f => Class(f) == "APP");
            string k = $"{t} | golib: {(gi >= 0 ? names[gi] : "-")} | app: {(ai >= 0 ? names[ai] : "-")}";
            var x = byTypeSite.GetValueOrDefault(k); byTypeSite[k] = (x.b + b, x.n + 1);
        }
    }
    else if (ev is GCStartTraceData gs) { if (gs.Depth is >= 0 and <= 2) gcByGen[gs.Depth]++; if (gcFirst < 0) gcFirst = gs.TimeStampRelativeMSec; gcLast = gs.TimeStampRelativeMSec; }
    else if (ev is GCSuspendEETraceData) suspendStart = ev.TimeStampRelativeMSec;
    else if (ev.EventName == "GC/RestartEEStop" && suspendStart >= 0) { pauseTotal += ev.TimeStampRelativeMSec - suspendStart; suspendStart = -1; }
    else if (ev is GCHeapStatsTraceData hs) { double h = hs.GenerationSize0 + hs.GenerationSize1 + hs.GenerationSize2 + hs.GenerationSize3 + hs.GenerationSize4; if (h > peakHeap) peakHeap = h; }
}

double traceMs = log.SessionDuration.TotalMilliseconds;
Console.WriteLine($"# trace {path}; duration {traceMs:F0} ms; phase filter: {phase ?? "(none: whole trace)"}");
Console.WriteLine($"# CPU samples {phaseSamples} of {samples}; sampled allocation {allocTotal / 1e6:F1} MB of {allocAll / 1e6:F1} MB (ticks are ~100 KB samples, so MB are estimates)");
Console.WriteLine($"## GC (whole trace, not phase-filtered): gen0 {gcByGen[0]}, gen1 {gcByGen[1]}, gen2 {gcByGen[2]}; pause total {pauseTotal:F0} ms = {100 * pauseTotal / Math.Max(1, traceMs):F1}% of the trace; peak heap (GCHeapStats sum) {peakHeap / 1e6:F0} MB");
Table("CPU exclusive", excl, phaseSamples);
Table("CPU inclusive", incl, phaseSamples);
Table("CPU inclusive, GOLIB frames only", inclGolib, phaseSamples);
ATable("Allocations by type", byType);
ATable("Allocations by allocating method (first non-framework frame)", bySite);
if (allocTypes.Count > 0) ATable("Allocations of --alloc-type by (type | first golib frame | nearest app frame)", byTypeSite);
foreach (var q in queries)
{
    string qk = q.ToString();
    Console.WriteLine($"## QUERY /{qk}/: inclusive samples {qTotal.GetValueOrDefault(qk)} ({100.0 * qTotal.GetValueOrDefault(qk) / Math.Max(1, phaseSamples):F2}%)");
    foreach (var kv in qByKey.Where(k => k.Key.StartsWith(qk + " |")).OrderByDescending(k => k.Value).Take(15)) Console.WriteLine($"{kv.Value,8}  {kv.Key[(qk.Length + 3)..]}");
    Console.WriteLine($"   -- by nearest APP caller:");
    foreach (var kv in qByCaller.Where(k => k.Key.StartsWith(qk + " |")).OrderByDescending(k => k.Value).Take(10)) Console.WriteLine($"{kv.Value,8}  {kv.Key[(qk.Length + 3)..]}");
}

void Table(string title, Dictionary<string, long> d, long total)
{
    Console.WriteLine($"## {title} (top {top}): samples  pct  method");
    foreach (var kv in d.OrderByDescending(k => k.Value).Take(top)) Console.WriteLine($"{kv.Value,8} {100.0 * kv.Value / Math.Max(1, total),6:F2}%  {kv.Key}");
}
void ATable(string title, Dictionary<string, (double b, long n)> d)
{
    Console.WriteLine($"## {title} (top {top}): MB  pct  ticks  key");
    foreach (var kv in d.OrderByDescending(k => k.Value.b).Take(top)) Console.WriteLine($"{kv.Value.b / 1e6,10:F1} {100 * kv.Value.b / Math.Max(1, allocTotal),6:F2}% {kv.Value.n,8}  {kv.Key}");
}
