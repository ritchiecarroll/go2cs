// tsbench: armab (C:\go2cs\scratch\p1262\armab, the 1.26.10.2 perf run's A/B harness) for COORD's item 2 and item 4 on
// 2026-11-20 (the typescript-go-cs and sqlite-cs performance tables). Changes from armab, and only these:
//   - a 6th column, args: the arm's command-line arguments, space-separated; "@<file>" reads one argument per line;
//   - CPU time (user + kernel, Process.TotalProcessorTime) recorded beside wall time and peak working set;
//   - the verify step compares the SHA-256 of each arm's stdout (no timing filter: these programs print no timings),
//     writes it to <out.tsv>.sha256, and the TSV is appended after every run, so a long case keeps what it measured;
//   - the run timeout is 3 hours.
// Per program: verify every arm's output equal and exit 0, one discarded warmup per arm, then N rounds whose arm order
// alternates forward/reverse. Usage: tsbench <arms.tsv> <rounds> <out.tsv>;
// arms.tsv rows: program<TAB>arm<TAB>exe<TAB>workdir<TAB>env<TAB>args (env "K=V;K=V", may be empty).
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

const int RunTimeoutMs = 3 * 3600_000;
string[] rows = File.ReadAllLines(args[0]).Where(l => l.Length > 0 && !l.StartsWith('#')).ToArray();
int rounds = int.Parse(args[1], CultureInfo.InvariantCulture);
string outPath = args[2];
File.WriteAllText(outPath, "program\tarm\tround\twall_ms\tcpu_ms\tpeak_bytes\n");
var shaLog = new StringBuilder();
var arms = rows.Select(r => r.Split('\t')).Select(c => (Program: c[0], Arm: c[1], Exe: c[2], Work: c[3],
    Env: c.Length > 4 ? c[4] : "", Args: ParseArgs(c.Length > 5 ? c[5] : ""))).ToList();

foreach (var group in arms.GroupBy(a => a.Program))
{
    var list = group.ToList();
    string? reference = null;
    foreach (var a in list)
    {
        var (code, stdout, wall, cpu, peak) = RunMeasured(a.Exe, a.Work, a.Env, a.Args);
        string sha = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stdout.Replace("\r", "")))).ToLowerInvariant();
        shaLog.Append(CultureInfo.InvariantCulture, $"{a.Program}\t{a.Arm}\t{sha}\t{stdout.Length}\texit {code}\tverify-run wall {wall:F0} ms\n");
        File.WriteAllText(outPath + ".sha256", shaLog.ToString());
        if (code != 0) { Console.WriteLine($"VERIFY FAIL {a.Program}/{a.Arm}: exit {code}"); return 1; }
        if (reference is null) reference = sha;
        else if (sha != reference) { Console.WriteLine($"VERIFY FAIL {a.Program}/{a.Arm}: output differs"); return 1; }
    }
    Console.WriteLine($"{group.Key}: verified {list.Count} arms, output equal ({reference})");
    foreach (var a in list) RunMeasured(a.Exe, a.Work, a.Env, a.Args);   // warmup, discarded
    for (int i = 0; i < rounds; i++)
    {
        var order = i % 2 == 0 ? list : Enumerable.Reverse(list).ToList();
        foreach (var a in order)
        {
            var (code, _, wall, cpu, peak) = RunMeasured(a.Exe, a.Work, a.Env, a.Args);
            if (code != 0) { Console.WriteLine($"MEASURE FAIL {a.Program}/{a.Arm} round {i}: exit {code}"); return 1; }
            File.AppendAllText(outPath, string.Create(CultureInfo.InvariantCulture, $"{a.Program}\t{a.Arm}\t{i}\t{wall:F3}\t{cpu:F3}\t{peak}\n"));
            // Item 4 (2026-11-20): a program's own "phase <name> <ns>" stderr lines, kept per run in <out>.phases.
            foreach (string line in ErrSink.Last.Replace("\r", "").Split('\n').Where(l => l.StartsWith("phase ", StringComparison.Ordinal)))
                File.AppendAllText(outPath + ".phases", $"{a.Program}\t{a.Arm}\t{i}\t{line["phase ".Length..].Replace(' ', '\t')}\n");
        }
        Console.WriteLine($"{group.Key}: round {i} done");
    }
    Console.WriteLine($"{group.Key}: {rounds} rounds done");
}
return 0;

static string[] ParseArgs(string spec)
{
    if (spec.Length == 0)
        return [];

    if (spec.StartsWith('@'))
        return File.ReadAllLines(spec[1..]).Where(l => l.Length > 0).ToArray();

    return spec.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}

static (int exitCode, string stdOut, double wallMs, double cpuMs, long peakBytes) RunMeasured(string exe, string workDir, string env, string[] args)
{
    ProcessStartInfo startInfo = new()
    {
        FileName = exe,
        WorkingDirectory = workDir,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    foreach (string arg in args)
        startInfo.ArgumentList.Add(arg);

    foreach (string kv in env.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        int eq = kv.IndexOf('=');
        startInfo.Environment[kv[..eq]] = kv[(eq + 1)..];
    }

    StringBuilder outBuf = new();

    using Process process = new();
    process.StartInfo = startInfo;
    process.OutputDataReceived += (_, e) => { if (e.Data is not null) outBuf.AppendLine(e.Data); };
    StringBuilder errBuf = new();
    process.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (errBuf) errBuf.AppendLine(e.Data); };

    Stopwatch sw = Stopwatch.StartNew();
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    long peak = 0;

    try
    {
        peak = process.PeakWorkingSet64;
    }
    catch
    {
    }

    while (!process.WaitForExit(1))
    {
        try
        {
            process.Refresh();
            long ws = process.PeakWorkingSet64;

            if (ws > peak)
                peak = ws;
        }
        catch
        {
        }

        if (sw.ElapsedMilliseconds > RunTimeoutMs)
        {
            try { process.Kill(entireProcessTree: true); }
            catch { }

            process.WaitForExit(5000);
            return (-1, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, 0, peak);
        }
    }

    sw.Stop();
    process.WaitForExit();

    double cpu = 0;

    try
    {
        cpu = process.TotalProcessorTime.TotalMilliseconds;
    }
    catch
    {
    }

    lock (errBuf) ErrSink.Last = errBuf.ToString();
    return (process.ExitCode, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, cpu, peak);
}

static class ErrSink
{
    public static string Last = "";
}
