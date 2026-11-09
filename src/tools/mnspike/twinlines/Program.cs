// G5's automatable proxy (docs/phase4/DESIGN-mn-scheduler.md §8 item 5): a breakpoint on a visible line binds inside
// a twin exactly when the twin's state-machine MoveNext has sequence points in the visible file's PDB document, on
// lines of the visible method. This reads this assembly's own portable PDB and prints one VERDICT line.
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace TwinLines;

public sealed class Chan
{
    public int Receive() => 41;

    public ValueTask<int> ReceiveAsync() => new(Task.Run(() => 41));
}

public static class Program
{
    public static int Main()
    {
        // Premise: both bodies run and agree.
        int sync = Pkg.Work(new Chan());
        int twin = Pkg.Work_twin(new Chan()).AsTask().GetAwaiter().GetResult();
        if (sync != 42 || twin != 42) { Console.WriteLine($"PREMISE FAILED sync={sync} twin={twin}"); return 2; }

        string dll = typeof(Program).Assembly.Location;
        using var pe = new PEReader(File.OpenRead(dll));
        using var pdbProvider = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(Path.ChangeExtension(dll, ".pdb")));
        MetadataReader md = pe.GetMetadataReader();
        MetadataReader pdb = pdbProvider.GetMetadataReader();

        MethodDefinitionHandle Find(string typeName, string methodName) =>
            md.MethodDefinitions.First(h =>
            {
                MethodDefinition m = md.GetMethodDefinition(h);
                TypeDefinition t = md.GetTypeDefinition(m.GetDeclaringType());
                return md.GetString(m.Name) == methodName && md.GetString(t.Name).Contains(typeName, StringComparison.Ordinal);
            });

        (string doc, int line)[] Points(MethodDefinitionHandle h) =>
            pdb.GetMethodDebugInformation(h.ToDebugInformationHandle()).GetSequencePoints()
                .Where(sp => !sp.IsHidden)
                .Select(sp => (pdb.GetString(pdb.GetDocument(sp.Document).Name), sp.StartLine))
                .ToArray();

        var visible = Points(Find("Pkg", "Work"));
        var twinPoints = Points(Find("<Work_twin>", "MoveNext"));
        string visibleDoc = visible.Select(p => p.doc).Distinct().Single();
        int[] visibleLines = visible.Select(p => p.line).Distinct().OrderBy(l => l).ToArray();

        Console.WriteLine($"VISIBLE {visibleDoc} breakpoint lines {string.Join(",", visibleLines)}");
        Console.WriteLine($"TWIN    {string.Join(", ", twinPoints.Select(p => $"{p.doc}:{p.line}"))}");

        // A breakpoint on a visible line binds in the twin when the twin has a sequence point at the SAME document (the
        // same PDB document name, so the same file) and line. The gate: every breakpoint-able visible line does.
        int[] unbound = visibleLines.Where(l => !twinPoints.Any(p => p.doc == visibleDoc && p.line == l)).ToArray();
        bool binds = unbound.Length == 0;
        Console.WriteLine(binds
            ? $"VERDICT BINDS: all {visibleLines.Length} breakpoint-able visible lines have a twin sequence point in the visible document"
            : $"VERDICT MISSES: {unbound.Length} of {visibleLines.Length} visible lines have no twin sequence point there ({string.Join(",", unbound)})");
        return binds ? 0 : 1;
    }
}
