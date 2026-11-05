using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

static class P
{
    static string LinePass(string s)
    {
        var lines = s.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder(s.Length + 64);
        bool prevBlank = false;
        int n = lines.Length;
        while (n > 0 && lines[n - 1].Trim().Length == 0) n--;
        for (int i = 0; i < n; i++)
        {
            string l = lines[i].TrimEnd();
            bool blank = l.Length == 0;
            if (blank && prevBlank) continue;
            sb.Append(l).Append("\r\n");
            prevBlank = blank;
        }
        return sb.ToString();
    }

    static void Main(string[] a)
    {
        var files = Directory.EnumerateFiles(a[0], "*.g.cs", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal).ToList();
        var texts = files.Select(File.ReadAllText).ToList();
        string Gen(string f) { var m = Regex.Match(f, @"go2cs\.(\w+?)Generator"); return m.Success ? m.Groups[1].Value : "?"; }
        string filter = a.Length > 1 ? a[1] : null;
        var idx = Enumerable.Range(0, files.Count).Where(i => filter == null || files[i].Contains(filter)).ToList();
        var parse = new Dictionary<string, double>(); var norm = new Dictionary<string, double>(); var line = new Dictionary<string, double>();
        var cnt = new Dictionary<string, int>(); var bytes = new Dictionary<string, long>();
        for (int iter = 0; iter < 4; iter++)
        {
            bool record = iter > 0;
            foreach (var i in idx)
            {
                string g = Gen(files[i]); string t = texts[i];
                var sw = Stopwatch.StartNew();
                var tree = CSharpSyntaxTree.ParseText(t);
                var root = tree.GetRoot();
                double p = sw.Elapsed.TotalMilliseconds;
                string o = root.NormalizeWhitespace("    ", "\r\n").ToFullString();
                double q = sw.Elapsed.TotalMilliseconds - p;
                sw.Restart();
                string l = LinePass(t);
                double r = sw.Elapsed.TotalMilliseconds;
                if (!record) continue;
                parse[g] = parse.GetValueOrDefault(g) + p / 3; norm[g] = norm.GetValueOrDefault(g) + q / 3; line[g] = line.GetValueOrDefault(g) + r / 3;
                if (iter == 1) { cnt[g] = cnt.GetValueOrDefault(g) + 1; bytes[g] = bytes.GetValueOrDefault(g) + t.Length; }
            }
        }
        Console.WriteLine("generator\tfiles\tchars\tparse_ms\tnormalize_ms\tlinepass_ms");
        foreach (var g in cnt.Keys.OrderByDescending(k => bytes[k]))
            Console.WriteLine($"{g}\t{cnt[g]}\t{bytes[g]}\t{parse[g]:F1}\t{norm[g]:F1}\t{line[g]:F1}");
        Console.WriteLine($"TOTAL\t{cnt.Values.Sum()}\t{bytes.Values.Sum()}\t{parse.Values.Sum():F1}\t{norm.Values.Sum():F1}\t{line.Values.Sum():F1}");
    }
}
