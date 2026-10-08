using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// golib binds a `this ref X` receiver marked [GoCopyBound] as a Go value-receiver method, through a
// copy (see CopyBoundReceiverTests), and reads every other by-ref receiver as a POINTER-set method.
// The mark is right for exactly one emitted shape — the forwarder of a pointer-receiver method
// promoted through an embedded POINTER — and both ways of getting it wrong are silent: a pointer-set
// method marked would be counted in a value method set Go does not give it and called on a copy, and
// a value-set forwarder left unmarked would be missing from the value set, so an assertion Go accepts
// would fail. So the population is PINNED. Every [GoCopyBound] by-ref receiver in a converted package
// this test can load must be listed in CopyBoundReceivers.allowlist.txt, where each row was classified
// VALUE independently, from the Go sources, and every listed receiver it meets must carry the mark.
//
// What it reads is what the PROCESS can load: the converted packages GolibTests references, directly
// and transitively. The count is asserted, so the guard cannot pass by reading nothing. The whole
// standard library's population is measured when the list is regenerated (see the list's header); the
// committed, hand-written by-ref receivers are pinned by the corpus guard
// TestByRefReceiversFollowTheReceiverRule.
[TestClass]
public class CopyBoundReceiverAllowlistTests
{
    [TestMethod]
    public void EveryCopyBoundReceiverInALoadedPackageIsAllowlisted()
    {
        HashSet<string> allowlist = ReadAllowlist();

        Assert.IsTrue(allowlist.Count >= 201, $"the allowlist holds {allowlist.Count} rows; it lists at least the 201 windows rows");

        List<string> unlisted = [];
        List<string> unmarked = [];
        HashSet<string> seen = new(StringComparer.Ordinal);
        int packages = 0;

        foreach (Assembly assembly in LoadReferencedClosure())
        {
            foreach (Type packageClass in SafeTypes(assembly))
            {
                // A converted package class: `go[.…].<pkg>_package`, static (abstract + sealed).
                if (!packageClass.IsAbstract || !packageClass.IsSealed || packageClass.Namespace is not { } ns ||
                    !(ns == "go" || ns.StartsWith("go.", StringComparison.Ordinal)) ||
                    !packageClass.Name.EndsWith("_package", StringComparison.Ordinal))
                    continue;

                packages++;

                string package = packageClass.FullName!["go.".Length..^"_package".Length];

                foreach (MethodInfo method in packageClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!method.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    ParameterInfo[] parameters = method.GetParameters();

                    if (parameters.Length == 0 || !parameters[0].ParameterType.IsByRef)
                        continue;

                    string receiver = parameters[0].ParameterType.GetElementType()!.Name;
                    int arity = receiver.IndexOf('`');

                    if (arity >= 0)
                        receiver = receiver[..arity];

                    string key = $"{package}\t{receiver}\t{method.Name}";

                    if (!method.IsDefined(typeof(GoCopyBoundAttribute), false))
                    {
                        // Read as POINTER-set: a listed (value-set) receiver must not land here.
                        if (allowlist.Contains(key))
                            unmarked.Add($"{package}.{receiver}.{method.Name}");

                        continue;
                    }

                    if (allowlist.Contains(key))
                        seen.Add(key);
                    else
                        unlisted.Add($"{package}.{receiver}.{method.Name}");
                }
            }
        }

        Assert.AreEqual(0, unlisted.Count,
            "[GoCopyBound] by-ref receiver(s) that the allowlist does not hold — a pointer-set method must not carry " +
            "the mark; a value-set forwarder must be added to CopyBoundReceivers.allowlist.txt with its go/types class: " +
            string.Join(", ", unlisted.Take(20)));

        Assert.AreEqual(0, unmarked.Count,
            "allowlisted value-set receiver(s) without [GoCopyBound] — golib reads an unmarked by-ref receiver as a " +
            "POINTER-set method, so the value set loses it: " + string.Join(", ", unmarked.Take(20)));

        // Not vacuous: runtime alone carries dozens of these, and it is always loaded here.
        Assert.IsTrue(packages >= 20 && seen.Count >= 40,
            $"the guard read {packages} package classes and matched {seen.Count} allowlisted receivers: too few to be the corpus");

        Console.WriteLine($"package classes read: {packages}; allowlisted copy-bound receivers seen: {seen.Count} of {allowlist.Count}");
    }

    // A method promoted through ANOTHER package's embed is forwarded from a sibling class,
    // `{pkg}ᴛ{Struct}ᴛxpkg`, and the generator settles its method set where it mints it: a
    // pointer-receiver method through value hops is `[GoRecv] this ref` (pointer set only), and one
    // through a pointer hop is by value. So NO sibling-class forwarder is a by-ref receiver marked
    // [GoCopyBound], or a by-ref receiver without [GoRecv] — the allowlist has no rows for these
    // classes, and none may be added.
    [TestMethod]
    public void NoCrossPackageForwarderIsACopyBoundReceiver()
    {
        List<string> unmarked = [];
        int classes = 0, forwarders = 0, pointerSetOnly = 0;

        foreach (Assembly assembly in LoadReferencedClosure())
        {
            foreach (Type siblingClass in SafeTypes(assembly))
            {
                if (!siblingClass.IsAbstract || !siblingClass.IsSealed || siblingClass.IsNested ||
                    !siblingClass.Name.EndsWith("ᴛxpkg", StringComparison.Ordinal))
                    continue;

                classes++;

                foreach (MethodInfo method in siblingClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!method.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    forwarders++;

                    if (!method.GetParameters()[0].ParameterType.IsByRef)
                        continue;

                    if (method.IsDefined(typeof(GoRecvAttribute), false) && !method.IsDefined(typeof(GoCopyBoundAttribute), false))
                        pointerSetOnly++;
                    else
                        unmarked.Add($"{siblingClass.FullName}.{method.Name}");
                }
            }
        }

        Assert.AreEqual(0, unmarked.Count,
            "cross-package forwarder(s) with a by-ref receiver that is not [GoRecv] alone — the generator forwards a value-set one by value: " +
            string.Join(", ", unmarked.Take(20)));

        // Not vacuous: the loaded closure holds sibling classes, with forwarders of both kinds.
        Assert.IsTrue(classes >= 1 && forwarders > pointerSetOnly && pointerSetOnly >= 1,
            $"the guard read {classes} sibling classes, {forwarders} forwarders, {pointerSetOnly} of them [GoRecv] by-ref: too few to be the corpus");

        Console.WriteLine($"sibling classes read: {classes}; forwarders: {forwarders}; [GoRecv] by-ref: {pointerSetOnly}");
    }

    private static HashSet<string> ReadAllowlist()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string path = Path.Combine(dir.FullName, "tests", "GolibTests", "CopyBoundReceivers.allowlist.txt");

            if (!File.Exists(path))
                continue;

            HashSet<string> rows = new(StringComparer.Ordinal);

            foreach (string line in File.ReadLines(path))
            {
                if (line.Length == 0 || line[0] == '#')
                    continue;

                string[] columns = line.Split('\t');

                Assert.IsTrue(columns.Length >= 3, $"malformed allowlist row: '{line}'");
                rows.Add($"{columns[0]}\t{columns[1]}\t{columns[2]}");
            }

            return rows;
        }

        Assert.Fail("CopyBoundReceivers.allowlist.txt was not found above the test binary");
        return [];
    }

    // The test assembly's referenced assemblies, transitively — a converted package loads lazily, so
    // the ones no other test has touched yet are loaded here by name.
    private static List<Assembly> LoadReferencedClosure()
    {
        Assembly self = typeof(CopyBoundReceiverAllowlistTests).Assembly;
        Dictionary<string, Assembly> loaded = new(StringComparer.Ordinal);
        Queue<Assembly> pending = new();

        pending.Enqueue(self);

        while (pending.Count > 0)
        {
            foreach (AssemblyName reference in pending.Dequeue().GetReferencedAssemblies())
            {
                if (reference.Name is not { } name || loaded.ContainsKey(name) ||
                    name.StartsWith("System", StringComparison.Ordinal) || name.StartsWith("Microsoft", StringComparison.Ordinal) ||
                    name is "netstandard" or "mscorlib")
                    continue;

                try
                {
                    Assembly assembly = Assembly.Load(reference);
                    loaded[name] = assembly;
                    pending.Enqueue(assembly);
                }
                catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
                {
                    // Not deployed beside the tests (a platform-conditional reference): nothing to read.
                }
            }
        }

        // The test assembly itself is left out on purpose: its fixtures declare unmarked by-ref
        // receivers to exercise the binding.
        return [.. loaded.Values];
    }

    private static Type[] SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return [.. ex.Types.Where(static type => type is not null).Cast<Type>()];
        }
    }
}
