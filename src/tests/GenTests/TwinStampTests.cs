// TwinStampTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// M:N item 7d (DESIGN: docs/phase4/DATA-mn-origin-table.md): every twin TwinGenerator writes, and every async literal
/// inside one, carries a [GoTwinOf] stamp naming the function it was generated from (host, name, parameter types) and
/// the converted-file line of each await in source order. The runtime reads a parked twin's frame from that stamp, with
/// no lookup by name and no scan of IL.
/// </summary>
/// <remarks>
/// <para>
/// The ORACLE below holds the stamp to the compiler: it scans each state machine's MoveNext for the call awaited at each
/// state number (the scan the runtime used to do, now test-only) and reads that call's line from the portable PDB, then
/// requires the stamp's await list to say the same thing at every state. A mismatch is named by machine and await.
/// </para>
/// <para>
/// RED before the stamp existed: no twin carried one.
/// </para>
/// </remarks>
[TestClass]
public class TwinStampTests
{
    private const string StampName = "go.golib.GoTwinOfAttribute";

    private const string Path = @"C:\twinstamp\main.cs";

    // Two types' methods of one name in ONE class: the shape package main's converter emits after train 1's
    // main-holders rule (methods in main_package, a holder only on a real clash); a by-name lookup reads both twins'
    // state machines as the first method of the name.
    private const string SameName = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class main_package
        {
            internal static channel<nint> ch;

            public partial struct T { public nint x; }

            public partial struct U { public nint y; }

            internal static nint wait() { return ᐸꟷ(ch); }

            internal static nint b(this ref T t) { var v = wait(); t.x = v; return v; }

            internal static nint b(this ref U u) { u.y = 1; var v = wait(); u.y += v; return u.y; }

            public static void runT(ж<T> Ꮡt) { ref var t = ref Ꮡt.DerefOrNull(); t.b(); }

            public static void runU(ж<U> Ꮡu) { ref var u = ref Ꮡu.DerefOrNull(); u.b(); }
        }
        """;

    // Await shapes: straight-line, in a loop, in a try with a deferred call, in both arms of a branch, and a go
    // statement's literal, which is an async lambda with a state machine of its own.
    private const string Shapes = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class shapes_package
        {
            internal static channel<nint> ch;

            internal static nint two() { var a = ᐸꟷ(ch); var b = ᐸꟷ(ch); return a + b; }

            internal static nint loop(nint n)
            {
                nint s = 0;
                for (nint i = 0; i < n; i++)
                    s += ᐸꟷ(ch);
                return s;
            }

            internal static nint guarded()
            {
                GoFrame ᒐ = default;
                try
                {
                    defer(() => { }, ref ᒐ);
                    return ᐸꟷ(ch) + two();
                }
                finally { ᒐ.Run(); }
            }

            internal static nint branch(bool left)
            {
                if (left)
                    return two();

                return loop(2) + ᐸꟷ(ch);
            }

            public static void starts()
            {
                goǃ(() => { var v = two(); ch.ᐸꟷ(v); });
                _ = guarded() + branch(true);
            }
        }
        """;

    // A literal the converter writes with attributes of its own (a caller-observing literal's NoInlining, and the literal
    // name a GoFunc attribute will carry): its twin keeps them beside the [GoTwinOf] stamp. StampLiteral replaced the
    // literal's attribute lists with the stamp, so the twin lost both.
    private const string AttributedLiteral = """
        using System;
        using System.Runtime.CompilerServices;
        using static go.builtin;

        namespace go;

        public static partial class attributed_package
        {
            internal static channel<nint> ch;

            internal static nint two() { var a = ᐸꟷ(ch); var b = ᐸꟷ(ch); return a + b; }

            public static void starts()
            {
                goǃ([MethodImpl(MethodImplOptions.NoInlining), System.ComponentModel.Description("kept")] () => { var v = two(); ch.ᐸꟷ(v); });
                _ = two();
            }
        }
        """;

    [TestMethod]
    public void ATwinnedLiteralKeepsItsOwnAttributesBesideTheStamp()
    {
        using Built built = Build(AttributedLiteral);

        (Type Machine, System.Reflection.MethodInfo Kickoff)[] literals = [.. built.Twins().Where(twin => twin.Machine.Name.Contains(">b__", StringComparison.Ordinal))];
        Assert.IsTrue(literals.Length > 0, "the go statement's literal has a twin: " + string.Join(", ", built.Twins().Select(twin => twin.Machine.Name)));

        foreach ((Type machine, System.Reflection.MethodInfo kickoff) in literals)
        {
            Assert.IsNotNull(Stamp(kickoff), $"{machine.Name}: the twin's stamp");
            Assert.IsTrue(kickoff.GetCustomAttributesData().Any(attribute => attribute.AttributeType.FullName == "System.ComponentModel.DescriptionAttribute"),
                $"{machine.Name}: the literal's own attribute is kept beside the stamp");
            Assert.IsTrue(kickoff.MethodImplementationFlags.HasFlag(MethodImplAttributes.NoInlining), $"{machine.Name}: and so is its NoInlining");
        }
    }

    [TestMethod]
    public void SameNamedTwinsInOneClassStampTheirOwnOrigins()
    {
        using Built built = Build(SameName);

        foreach ((Type machine, System.Reflection.MethodInfo kickoff) in built.Twins())
        {
            if (!machine.Name.StartsWith("<bꞏtwin>", StringComparison.Ordinal))
                continue;

            CustomAttributeData stamp = Stamp(kickoff) ?? throw new AssertFailedException($"{machine.Name} (kickoff {Sig(kickoff)}) carries no {StampName}");
            Type host = (Type)stamp.ConstructorArguments[0].Value!;
            string name = (string)stamp.ConstructorArguments[1].Value!;
            Type?[] parameters = [.. ((IEnumerable<CustomAttributeTypedArgument>)stamp.ConstructorArguments[2].Value!).Select(argument => (Type?)argument.Value)];

            System.Reflection.MethodInfo[] matches = [.. host.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => method.Name == name && Matches(method, parameters))];

            Assert.AreEqual(1, matches.Length, $"{machine.Name}: the stamp ({host.Name}.{name}) matches {matches.Length} methods: {string.Join(", ", matches.Select(Sig))}");

            // The twin takes its receiver as a box, ж<T> or ж<U>: its origin takes that receiver by ref.
            Type receiver = kickoff.GetParameters()[0].ParameterType.GetGenericArguments()[0];
            Assert.AreEqual(receiver, matches[0].GetParameters()[0].ParameterType.GetElementType(), $"{machine.Name} resolves to {Sig(matches[0])}");
        }

        Assert.AreEqual(2, built.Twins().Count(twin => twin.Machine.Name.StartsWith("<bꞏtwin>", StringComparison.Ordinal)), "both b twins were generated");
    }

    [TestMethod]
    public void EveryStampSaysWhatTheCompilerSaysAtEveryAwait()
    {
        foreach (string source in new[] { SameName, Shapes })
        {
            using Built built = Build(source);
            string[] problems = Oracle(built);
            Assert.AreEqual(0, problems.Length, string.Join("\n", problems));
        }
    }

    [TestMethod]
    public void TheOracleNamesAnAwaitMissingFromAStamp()
    {
        // Made to fail: the stamp of `two` loses its last await site; the oracle must name the twin and the await.
        using Built built = Build(Shapes, twins => Regex.Replace(twins, @"(GoTwinOf\(typeof\([^)]*\), ""two"", [^;]*?new int\[\] \{ \d+), \d+ \}", "$1 }"));
        string[] problems = Oracle(built);
        Assert.IsTrue(problems.Any(problem => problem.Contains("<twoꞏtwin>", StringComparison.Ordinal) && problem.Contains("await #1", StringComparison.Ordinal)),
            "the oracle did not name the missing await:\n" + (problems.Length == 0 ? "(no problem reported)" : string.Join("\n", problems)));
    }

    // A call that parks on the SECOND line of its statement, beside a same-name call on the first: Go reports the call's
    // own line, so the stamp names the call by its place among the statement's calls of its name (G's second array).
    private const string MultiLine = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class multi_package
        {
            internal static channel<nint> ch;

            internal static nint leaf() { return ᐸꟷ(ch); }

            internal static nint add(nint a, nint b) => a + b;

            internal static nint mid()
            {
                nint v = add(leaf(),
                    leaf());
                return v;
            }
        }
        """;

    [TestMethod]
    public void AStampNamesALaterLineCallByItsPlaceInTheStatement()
    {
        using Built built = Build(MultiLine);
        (Type machine, System.Reflection.MethodInfo kickoff) = built.Twins().Single(twin => twin.Machine.Name.StartsWith("<midꞏtwin>", StringComparison.Ordinal));
        CustomAttributeData stamp = Stamp(kickoff) ?? throw new AssertFailedException($"{machine.Name} carries no stamp");

        int[] sites = [.. ((IEnumerable<CustomAttributeTypedArgument>)stamp.ConstructorArguments[3].Value!).Select(argument => (int)argument.Value!)];
        int[] calls = [.. ((IEnumerable<CustomAttributeTypedArgument>)stamp.ConstructorArguments[4].Value!).Select(argument => (int)argument.Value!)];
        int statement = Array.FindIndex(MultiLine.Replace("\r\n", "\n").Split('\n'), line => line.Contains("add(leaf(),", StringComparison.Ordinal)) + 1;

        // Both leaf calls park: the first, then the second, each at the statement's line, named 1 of 2 and 2 of 2.
        CollectionAssert.AreEqual(new[] { statement, statement }, sites, "the await lines");
        CollectionAssert.AreEqual(new[] { (1 << 16) | 2, (2 << 16) | 2 }, calls, "each leaf call's place among the statement's two");
    }

    // ---- the oracle ----

    // Every state machine of a twin or of an async literal in one: its stamp's await list against the compiler's.
    private static string[] Oracle(Built built)
    {
        List<string> problems = [];

        foreach ((Type machine, System.Reflection.MethodInfo kickoff) in built.Twins())
        {
            if (Stamp(kickoff) is not { } stamp)
            {
                problems.Add($"{machine.Name} (kickoff {Sig(kickoff)}): no {StampName}");
                continue;
            }

            int[] sites = [.. ((IEnumerable<CustomAttributeTypedArgument>)stamp.ConstructorArguments[3].Value!).Select(argument => (int)argument.Value!)];
            Dictionary<int, int> compiled = built.AwaitLines(machine);

            foreach ((int state, int line) in compiled.OrderBy(pair => pair.Key))
            {
                if (state >= sites.Length)
                    problems.Add($"{machine.Name} await #{state}: missing from the stamp (the compiler awaits the call at line {line})");
                else if (sites[state] != line)
                    problems.Add($"{machine.Name} await #{state}: the stamp says line {sites[state]}, the compiler awaits the call at line {line}");
            }

            if (sites.Length > compiled.Count)
                problems.Add($"{machine.Name}: the stamp lists {sites.Length} awaits, the compiler numbers {compiled.Count}");
        }

        return [.. problems];
    }

    private static CustomAttributeData? Stamp(System.Reflection.MethodInfo kickoff) => kickoff.GetCustomAttributesData().FirstOrDefault(data => data.AttributeType.FullName == StampName);

    private static bool Matches(System.Reflection.MethodInfo method, Type?[] parameters)
    {
        ParameterInfo[] actual = method.GetParameters();

        if (actual.Length != parameters.Length)
            return false;

        for (int i = 0; i < actual.Length; i++)
        {
            Type type = actual[i].ParameterType.IsByRef ? actual[i].ParameterType.GetElementType()! : actual[i].ParameterType;

            if (parameters[i] is null ? !type.IsGenericMethodParameter : (type.IsGenericType ? type.GetGenericTypeDefinition() : type) != parameters[i])
                return false;
        }

        return true;
    }

    private static string Sig(System.Reflection.MethodInfo method) =>
        $"{method.Name}({string.Join(", ", method.GetParameters().Select(parameter => (parameter.ParameterType.IsByRef ? "ref " : "") + parameter.ParameterType.Name.TrimEnd('&')))})";

    // ---- building ----

    private static Built Build(string source, Func<string, string>? editTwins = null)
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        MetadataReference[] references = [.. trusted.Split(System.IO.Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location)];

        CSharpCompilation compilation = CSharpCompilation.Create("twinstamp", [CSharpSyntaxTree.ParseText(source, options, Path)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));

        CSharpGeneratorDriver.Create([new RecvGenerator(), new FieldAccessGenerator(), new TypeGenerator(), new TwinGenerator()], parseOptions: options,
                optionsProvider: new BuildProperties(new() { ["build_property.GoTwins"] = "true" }))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        if (editTwins is not null)
        {
            foreach (SyntaxTree tree in output.SyntaxTrees.Where(tree => tree.FilePath.EndsWith(".twins.g.cs", StringComparison.Ordinal)).ToArray())
                output = output.ReplaceSyntaxTree(tree, CSharpSyntaxTree.ParseText(editTwins(tree.ToString()), options, tree.FilePath));
        }

        Built? built = Load(output, out string errors);
        Assert.IsNotNull(built, errors);
        return built!;
    }

    // Emits a compilation with its portable PDB and loads it; null (with the errors) where it does not emit.
    private static Built? Load(Compilation output, out string errors, byte[]? library = null)
    {
        // A PDB needs each source's encoding, which an in-memory text and a generator's output do not carry.
        foreach (SyntaxTree tree in output.SyntaxTrees.Where(tree => tree.Encoding is null).ToArray())
            output = output.ReplaceSyntaxTree(tree, CSharpSyntaxTree.ParseText(Microsoft.CodeAnalysis.Text.SourceText.From(tree.ToString(), System.Text.Encoding.UTF8), (CSharpParseOptions)tree.Options, tree.FilePath));

        using MemoryStream image = new();
        using MemoryStream pdb = new();
        EmitResult emitted = output.Emit(image, pdb, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
        errors = string.Join("\n", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        return emitted.Success ? new Built(image.ToArray(), pdb.ToArray(), library) : null;
    }

    /// <summary>
    /// Holds every twin a test compilation generated to the oracle: GenTests' twin suites call it on each compilation they
    /// build, so the stamp is checked against the compiler on every twin shape they write. A compilation that does not emit
    /// (an arm that expects an error) is that arm's to judge, and is skipped.
    /// </summary>
    internal static void AssertStampsHold(Compilation output, byte[]? library = null)
    {
        using Built? built = Load(output, out _, library);

        if (built is null)
            return;

        string[] problems = Oracle(built);
        Assert.AreEqual(0, problems.Length, "a [GoTwinOf] stamp disagrees with the compiler:\n" + string.Join("\n", problems));
    }

    private sealed class Built : IDisposable
    {
        private readonly AssemblyLoadContext m_context = new("twinstamp", isCollectible: true);
        private readonly MetadataReaderProvider m_pdb;
        private readonly Assembly m_assembly;

        public Built(byte[] image, byte[] pdb, byte[]? library = null)
        {
            // An in-memory library the compilation references (TwinBoxTests' cross-assembly arms) loads first.
            if (library is not null)
                m_context.LoadFromStream(new MemoryStream(library));

            m_context.Resolving += (context, name) => name.Name == "golib" ? context.LoadFromAssemblyPath(typeof(golib::go.IGoEquatable).Assembly.Location) : null;
            m_assembly = m_context.LoadFromStream(new MemoryStream(image));
            m_pdb = MetadataReaderProvider.FromPortablePdbStream(new MemoryStream(pdb));
        }

        // Every async state machine of a twin (`<Xꞏtwin>d__N`) or of a literal inside one, with its kickoff method: the
        // method of the machine's declaring type whose AsyncStateMachineAttribute names exactly this machine.
        public IEnumerable<(Type Machine, System.Reflection.MethodInfo Kickoff)> Twins()
        {
            const BindingFlags declared = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (Type machine in m_assembly.GetTypes().Where(type => typeof(IAsyncStateMachine).IsAssignableFrom(type) && type.Name.Contains("ꞏtwin>", StringComparison.Ordinal)))
            {
                System.Reflection.MethodInfo? kickoff = machine.DeclaringType?.GetMethods(declared)
                    .FirstOrDefault(method => method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == machine);

                if (kickoff is not null)
                    yield return (machine, kickoff);
            }
        }

        // The compiler's await numbering for one machine: state -> the converted-file line of the call awaited there.
        public Dictionary<int, int> AwaitLines(Type machine)
        {
            const BindingFlags members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            System.Reflection.MethodInfo moveNext = machine.GetMethod("MoveNext", members)!;
            byte[] il = moveNext.GetMethodBody()!.GetILAsByteArray()!;
            Type[]? typeArguments = machine.IsGenericType ? machine.GetGenericArguments() : null;
            Dictionary<int, int> lines = [];
            int lastCall = -1, awaited = -1, constant = -1, constantAge = int.MaxValue, i = 0;

            while (i < il.Length)
            {
                int start = i;
                OpCode op = il[i] == 0xFE ? s_twoByte[il[i + 1]] : s_oneByte[il[i]];
                i += op.Size;

                if (constantAge < int.MaxValue)
                    constantAge++;

                switch (il[start])
                {
                    case 0x15: (constant, constantAge) = (-1, 0); break;
                    case >= 0x16 and <= 0x1E: (constant, constantAge) = (il[start] - 0x16, 0); break;
                    case 0x1F: (constant, constantAge) = ((sbyte)il[i], 0); break;
                    case 0x20: (constant, constantAge) = (BitConverter.ToInt32(il, i), 0); break;
                    case 0x28 or 0x6F:
                        MethodBase? target = machine.Module.ResolveMethod(BitConverter.ToInt32(il, i), typeArguments, null);

                        if (target?.Name == "GetAwaiter")
                            awaited = lastCall;
                        else
                            lastCall = start;

                        break;
                    case 0x7D when constantAge <= 3 && constant >= 0:
                        if (machine.Module.ResolveField(BitConverter.ToInt32(il, i), typeArguments, null)?.Name == "<>1__state")
                        {
                            Assert.IsTrue(awaited >= 0 && lines.TryAdd(constant, LineAt(moveNext, awaited)), $"{machine.Name}: state {constant} has no single awaited call");
                            awaited = -1;
                        }

                        break;
                }

                i += op.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, i),
                    _ => 4
                };
            }

            return lines;
        }

        // The line of the sequence point that holds an IL offset: the twin's #line directive maps it into the converted file.
        private int LineAt(System.Reflection.MethodInfo method, int ilOffset)
        {
            MetadataReader reader = m_pdb.GetMetadataReader();
            MethodDebugInformation info = reader.GetMethodDebugInformation(MetadataTokens.MethodDefinitionHandle(method.MetadataToken));

            return info.GetSequencePoints().Where(point => !point.IsHidden && point.Offset <= ilOffset).OrderBy(point => point.Offset).Last().StartLine;
        }

        public void Dispose()
        {
            m_pdb.Dispose();
            m_context.Unload();
        }
    }

    private static readonly Dictionary<byte, OpCode> s_oneByte = [];
    private static readonly Dictionary<byte, OpCode> s_twoByte = [];

    static TwinStampTests()
    {
        foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            OpCode op = (OpCode)field.GetValue(null)!;
            ushort value = unchecked((ushort)op.Value);

            if (op.Size == 1)
                s_oneByte[(byte)value] = op;
            else
                s_twoByte[(byte)(value & 0xFF)] = op;
        }
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
