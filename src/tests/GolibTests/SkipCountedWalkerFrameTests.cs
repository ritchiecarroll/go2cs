using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A SKIP-COUNTED WALKER keeps its own frame.
//
// The converter's computeNoInliningClosure (src/go2cs/callerInliningAnalysis.go) names the functions whose
// frame a stack walk COUNTS -- skipCountedWalkers: callers, saveblockevent, unlock2, Stack, ... -- and marks
// [MethodImpl(NoInlining)] on the converted hops that call them. It never marks a HAND-OWNED file, and the
// walker's own frame is load-bearing too: a skip counts from the walker (callers' skip 0 is its caller), so a
// walker the JIT inlines into its caller shifts every recorded stack one real frame too high.
//
// Measured: the hand-owned runtime.saveblockevent (mprof_impl.cs) carried no attribute. Once a full run
// made it hot, Tier-1 inlined it into mutexevent/blockevent, callers' skip 0 became mutexevent, and every
// mutex and block event's stack started one frame past sync.(*Mutex).Unlock / Lock -- SyncMutexProfileTests
// red in full Release runs only (filtered runs stay at Tier-0; Debug never inlines).
//
// This guard reads the walker set from the converter's own source, finds each walker's declaration, and
// requires NoInlining on every one declared in a hand-owned file (a *_impl.cs companion or a whole-file
// [module: go.GoManualConversion] replacement). Converted walkers are the converter's to mark.
[TestClass]
public class SkipCountedWalkerFrameTests
{
    private static string SrcRoot([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));

    private static readonly Regex s_walkerEntry = new("\"([A-Za-z0-9_/]+)\\.([A-Za-z0-9_]+)\":\\s*true", RegexOptions.Compiled);
    private static readonly Regex s_moduleHandOwn = new(@"^\[module: go\.GoManualConversion\]", RegexOptions.Compiled | RegexOptions.Multiline);

    // The walker set, parsed from the map literal so a walker added there is guarded here without an edit.
    private static List<(string Package, string Function)> Walkers(string src)
    {
        string analysis = File.ReadAllText(Path.Combine(src, "go2cs", "callerInliningAnalysis.go"));
        int start = analysis.IndexOf("var skipCountedWalkers = map[string]bool{", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "callerInliningAnalysis.go no longer declares skipCountedWalkers where this guard reads it");
        int end = analysis.IndexOf('}', start);

        return [.. s_walkerEntry.Matches(analysis[start..end]).Select(m => (m.Groups[1].Value, m.Groups[2].Value))];
    }

    // The files that declare `function` as a plain static (not a `this` extension, which is a METHOD of a
    // record type that merely shares the name, e.g. (*StackRecord).Stack), in the package's directory and
    // its per-OS subdirectories.
    private static List<string> DeclaringFiles(string src, string package, string function)
    {
        string dir = Path.Combine(src, "core", package.Replace('/', Path.DirectorySeparatorChar));
        var declaration = new Regex($@"\bstatic\s+[^=;(){{}}]+?\s{Regex.Escape(function)}\s*\((?!\s*this\b)");
        List<string> files = [];

        foreach (string sub in new[] { "", "windows", "linux", "darwin" })
        {
            string path = Path.Combine(dir, sub);

            if (!Directory.Exists(path))
                continue;

            foreach (string file in Directory.EnumerateFiles(path, "*.cs"))
            {
                if (file.EndsWith("_test.cs", StringComparison.Ordinal))
                    continue;

                if (declaration.IsMatch(File.ReadAllText(file)))
                    files.Add(file);
            }
        }

        return files;
    }

    private static bool IsHandOwned(string file) =>
        file.EndsWith("_impl.cs", StringComparison.Ordinal) || s_moduleHandOwn.IsMatch(File.ReadAllText(file));

    private static Type PackageClass(string package) => package switch
    {
        "runtime" => typeof(runtime_package),
        _ => AppDomain.CurrentDomain.GetAssemblies()
                 .Select(a => a.GetType("go." + package.Replace('/', '.') + "_package", throwOnError: false))
                 .FirstOrDefault(t => t is not null)
             ?? throw new AssertFailedException($"a hand-owned walker lives in {package}, whose assembly this project does not load; reference it so the guard can read the attribute")
    };

    [TestMethod]
    public void EveryHandOwnedSkipCountedWalkerKeepsItsFrame()
    {
        string src = SrcRoot();

        if (!File.Exists(Path.Combine(src, "go2cs", "callerInliningAnalysis.go")))
            Assert.Inconclusive($"source not found under {src}; this guard reads the tree it was built from");

        List<(string Package, string Function)> walkers = Walkers(src);

        // The parser's positive control: the set is non-trivial and names the walker every other one feeds.
        Assert.IsTrue(walkers.Count >= 5 && walkers.Contains(("runtime", "callers")),
            $"parsed only [{string.Join(", ", walkers)}] from skipCountedWalkers: the reader, not the tree, is broken");

        List<string> handOwned = [], unmarked = [];

        foreach ((string package, string function) in walkers)
        {
            List<string> files = DeclaringFiles(src, package, function);
            Assert.IsTrue(files.Count > 0, $"no declaration of {package}.{function} found under core/{package}");

            if (!files.Any(IsHandOwned))
                continue;

            handOwned.Add($"{package}.{function}");

            MethodInfo[] methods = [.. PackageClass(package)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == function && !m.IsDefined(typeof(ExtensionAttribute), false))];

            Assert.IsTrue(methods.Length > 0, $"{package}.{function} is declared in a hand-owned file but not found by reflection");

            if (methods.Any(m => (m.MethodImplementationFlags & MethodImplAttributes.NoInlining) == 0))
                unmarked.Add($"{package}.{function}");
        }

        // The second control: callers is hand-owned (managed_impl.cs), so the classification is live.
        Assert.IsTrue(handOwned.Contains("runtime.callers"), $"runtime.callers was not classified hand-owned; hand-owned set: [{string.Join(", ", handOwned)}]");

        Assert.AreEqual(0, unmarked.Count,
            $"hand-owned skip-counted walker(s) without [MethodImpl(MethodImplOptions.NoInlining)]: {string.Join(", ", unmarked)} " +
            $"-- an inlined walker shifts every stack it records one frame too high (hand-owned set checked: {string.Join(", ", handOwned)})");
    }
}
