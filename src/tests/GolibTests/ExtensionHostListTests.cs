using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// The extension-host list (docs/phase4/DESIGN-extension-host-list.md): go2cs-gen lists, as assembly attributes, the
// classes of a converted assembly that declare extension methods, and golib's scan reads that list where an assembly
// has no raw metadata (Native AOT). Under-listing would lose Go methods under Native AOT ALONE, where nothing else
// runs, so the list is held here, under the JIT, to the metadata walk that is exact by construction: for every
// converted src/core assembly this project can reach, not only the ones a test happened to load.
[TestClass]
public class ExtensionHostListTests
{
    // Assemblies that reference golib but are hand-written .NET projects built without go2cs-gen, so carry no list.
    private static readonly HashSet<string> s_notConverted = ["GolibTests", "go2cs.CpuProfiler", "go.arrays"];

    // Every assembly in the test's output directory that references golib, loaded. The output directory holds the
    // whole reference closure, so this is every converted assembly the project can reach.
    internal static List<Assembly> ReachableConvertedAssemblies()
    {
        List<Assembly> assemblies = [];

        foreach (string path in Directory.GetFiles(AppContext.BaseDirectory, "*.dll").Order(StringComparer.Ordinal))
        {
            AssemblyName name;

            try
            {
                name = AssemblyName.GetAssemblyName(path);
            }
            catch (BadImageFormatException)
            {
                continue; // a native library
            }

            if (name.Name is null || name.Name == "golib" || s_notConverted.Contains(name.Name))
                continue;

            Assembly assembly = Assembly.Load(name);

            if (assembly.GetReferencedAssemblies().Any(reference => reference.Name == "golib"))
                assemblies.Add(assembly);
        }

        return assemblies;
    }

    [TestMethod]
    public void EveryConvertedAssemblyCarriesACompleteList()
    {
        List<Assembly> assemblies = ReachableConvertedAssemblies();

        string[] unlisted = [.. assemblies
            .Where(assembly => !assembly.IsDefined(typeof(GoExtensionHostListAttribute), false))
            .Select(assembly => assembly.GetName().Name!)];

        Assert.AreEqual(0, unlisted.Length, $"converted assemblies with no extension-host list: {string.Join(", ", unlisted)}");

        // The receiver-home scope (docs/phase4/DESIGN-extension-receiver-scope.md): each listing names its homes.
        string[] homesUnlisted = [.. assemblies
            .Where(assembly => assembly.GetCustomAttribute<GoExtensionHostListAttribute>() is { HomesListed: false })
            .Select(assembly => assembly.GetName().Name!)];

        Assert.AreEqual(0, homesUnlisted.Length, $"converted assemblies whose list does not name its hosts' homes: {string.Join(", ", homesUnlisted)}");

        // Control: the population is the converted closure, not the handful a test loaded.
        Assert.IsTrue(assemblies.Count > 100, $"control: reached {assemblies.Count} converted assemblies");
    }

    [TestMethod]
    public void TheListYieldsExactlyTheMetadataWalksExtensionMethods()
    {
        int compared = 0, methods = 0;
        List<string> failures = [];

        foreach (Assembly assembly in ReachableConvertedAssemblies())
        {
            IReadOnlyList<Type>? listed = TypeExtensions.ListedExtensionHosts(assembly);

            if (listed is null)
                continue; // no list: EveryConvertedAssemblyCarriesACompleteList names it

            IReadOnlyList<Type>? walked = TypeExtensions.ExtensionHostTypes(assembly);
            Assert.IsNotNull(walked, $"{assembly.GetName().Name}: a JIT-loaded assembly must expose its raw metadata");

            HashSet<MethodInfo> viaList = [.. listed.SelectMany(TypeExtensions.DeclaredExtensionMethods)];
            HashSet<MethodInfo> viaWalk = [.. walked.SelectMany(TypeExtensions.DeclaredExtensionMethods)];

            string[] missing = [.. viaWalk.Except(viaList).Select(method => method.DeclaringType!.FullName!).Distinct().Order(StringComparer.Ordinal)];
            string[] extra = [.. viaList.Except(viaWalk).Select(method => method.DeclaringType!.FullName!).Distinct().Order(StringComparer.Ordinal)];

            if (missing.Length > 0)
                failures.Add($"{assembly.GetName().Name}: hosts the walk finds and the list omits: {string.Join(", ", missing)}");

            if (extra.Length > 0)
                failures.Add($"{assembly.GetName().Name}: methods the list yields and the walk does not: {string.Join(", ", extra)}");

            compared++;
            methods += viaWalk.Count;
        }

        Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        Assert.IsTrue(compared > 100 && methods > 5000, $"control: compared {compared} assemblies and {methods} methods");
    }

    // The receiver-home scope (docs/phase4/DESIGN-extension-receiver-scope.md). Under Native AOT a question reads a
    // listed host only when the homes it names, or its own assembly, can match, so a home the listing omits loses Go
    // methods there alone. Held here to the homes the metadata walk reads from the signatures, and both to the homes
    // of the methods' receivers as reflection reads them, per host, for every converted assembly this project reaches.
    [TestMethod]
    public void EveryListingNamesTheHomesItsMethodsExtend()
    {
        int hosts = 0;
        List<string> failures = [];

        foreach (Assembly assembly in ReachableConvertedAssemblies())
        {
            IReadOnlyList<TypeExtensions.ExtensionHost>? listed = TypeExtensions.ListedExtensionHostEntries(assembly);

            if (listed is null)
                continue; // no list: EveryConvertedAssemblyCarriesACompleteList names it

            Dictionary<Type, TypeExtensions.ExtensionHost> listedByType = listed.ToDictionary(host => host.Type);

            foreach (TypeExtensions.ExtensionHost walked in TypeExtensions.WalkedExtensionHosts(assembly)!)
            {
                Assembly?[] receiverHomes = [.. TypeExtensions.DeclaredExtensionMethods(walked.Type)
                    .Select(method => TypeExtensions.ExtensionHome(TypeExtensions.GetExtensionTargetType(method))).Distinct()];

                string truth = homes(receiverHomes.OfType<Assembly>(), receiverHomes.Contains(null));
                string decoded = homes(walked.Homes, walked.AnyHome);

                if (decoded != truth)
                    failures.Add($"{walked.Type.FullName}: the walk reads [{decoded}], the receivers are homed in [{truth}]");

                if (!listedByType.TryGetValue(walked.Type, out TypeExtensions.ExtensionHost? entry))
                    continue; // unlisted: TheListYieldsExactlyTheMetadataWalksExtensionMethods names it

                // A listing always holds its own assembly; it names a home beyond it only where a receiver has one.
                string listing = homes(entry.Homes.Where(home => home != assembly), entry.AnyHome);
                string foreign = homes(receiverHomes.OfType<Assembly>().Where(home => home != assembly), receiverHomes.Contains(null));

                if (listing != foreign)
                    failures.Add($"{walked.Type.FullName}: the list names [{listing}] beyond its own assembly, the receivers [{foreign}]");

                hosts++;
            }
        }

        Assert.AreEqual(0, failures.Count, string.Join("\n", failures.Take(20)));
        Assert.IsTrue(hosts > 100, $"control: compared {hosts} hosts");

        static string homes(IEnumerable<Assembly> assemblies, bool anyHome) =>
            string.Join(", ", assemblies.Select(home => home.GetName().Name).Order(StringComparer.Ordinal)) + (anyHome ? " +any" : "");
    }

    // The list's order is not the walk's (attribute order against metadata order), and the scan's order is observable
    // only where GetExtensionMethod breaks a tie: two extension methods of one name whose receivers rank equally for
    // some target. Within one class both sources read the same GetMethods order, so a tie can matter only between two
    // classes: one name declared by two classes of an assembly over the SAME receiver type, or over two interfaces (one
    // type can implement both at the same distance). None may exist, so the order cannot matter.
    [TestMethod]
    public void NoSameNameEqualPrecedenceTieSpansTwoHosts()
    {
        List<string> ties = [];

        foreach (Assembly assembly in ReachableConvertedAssemblies())
        {
            IReadOnlyList<Type>? walked = TypeExtensions.ExtensionHostTypes(assembly);

            if (walked is null || TypeExtensions.ListedExtensionHosts(assembly) is null)
                continue;

            var byName = walked
                .SelectMany(TypeExtensions.DeclaredExtensionMethods)
                .Select(method => (method, receiver: TypeExtensions.GetExtensionTargetType(method)))
                .GroupBy(entry => entry.method.Name);

            foreach (var group in byName)
            {
                var entries = group.ToArray();

                for (int i = 0; i < entries.Length; i++)
                for (int j = i + 1; j < entries.Length; j++)
                {
                    (MethodInfo a, Type ra) = entries[i];
                    (MethodInfo b, Type rb) = entries[j];

                    if (a.DeclaringType == b.DeclaringType)
                        continue;

                    if (ra == rb || ra.IsInterface && rb.IsInterface)
                        ties.Add($"{assembly.GetName().Name}: {group.Key} on {ra.Name} in {a.DeclaringType!.Name} and on {rb.Name} in {b.DeclaringType!.Name}");
                }
            }
        }

        Assert.AreEqual(0, ties.Count, $"{ties.Count} ties, first 20:\n{string.Join("\n", ties.Take(20))}");
    }
}
