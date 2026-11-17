using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go; // ж<T> — the probe references it by open generic to locate golib's assembly

namespace GolibTests;

/// <summary>
/// RecvGenerator's accessibility guard: a PUBLICIZED receiver type's <c>ж&lt;T&gt;</c> overload must
/// bind from a CONSUMING assembly, and the narrowing that keeps a public overload off a non-public
/// receiver must survive saying so.
/// </summary>
/// <remarks>
/// <para>
/// The converter's publicize pass emits an UNEXPORTED Go type as <c>public partial struct</c> when an
/// exported signature reaches it — <c>crypto/internal/fips140/ecdsa</c>'s <c>hmacDRBG</c>, handed out
/// by the exported <c>TestingOnlyNewDRBG</c>. <c>RecvGenerator</c> used to decide the generated
/// <c>ж&lt;T&gt;</c> overload's accessibility from the Go EXPORT CASE of the receiver's NAME, which
/// answers "internal" for such a type however the emission actually declares it. The overload was
/// therefore minted <c>internal</c> over a genuinely <c>public</c> struct, a consuming assembly saw
/// only the <c>ref T</c> primary, and <c>box.Method(…)</c> there was CS1929 — two sites in
/// <c>crypto/internal/fips140test</c>'s <c>acvp_test</c>, which is what this class was minted on.
/// The twin partials <see cref="TypeGenerator"/>-style generators emit for that same type were
/// already public (the struct itself, and the implicit <c>ж&lt;hmacDRBG&gt;</c> conversion), so
/// RecvGenerator alone disagreed with the emission it was decorating.
/// </para>
/// <para>
/// These probes need no fixture and no corpus regen, for the same reason
/// <see cref="ZhBoxSelectionProbeTests"/> needs none: the shape ALREADY EXISTS IN PRODUCTION. The
/// subject pair is <c>ecdsa.hmacDRBG</c> (publicized, unexported in Go) and the control pair is
/// <c>ecdsa.PrivateKey</c> (exported in Go) — the SAME package, the SAME generator, the SAME
/// compilation, differing on the one axis under test. The verdict instrument is the compiler itself:
/// a snippet compiled into a SEPARATE assembly that references the real <c>ecdsa</c> DLL, so
/// "cross-assembly" is true by construction rather than by assertion (this test assembly is not
/// <c>crypto.internal.fips140.ecdsa.tests</c>, so the package's <c>InternalsVisibleTo</c> grant does
/// not reach the probe, exactly as it does not reach <c>fips140test</c>).
/// </para>
/// <para>
/// ⚠ The matrix is self-falsifying. The obvious WRONG fix — publish every <c>ж&lt;T&gt;</c> overload
/// — passes the two binding arms and is caught by <see cref="NoGeneratedBoxOverloadIsPublicOverANonPublicReceiver"/>
/// (CS0051 by construction) and by <see cref="AnInternalMethodOnAPublicReceiverKeepsAnInternalOverload"/>
/// (the converter's own W3a narrowing, which the generator must not re-widen). Both of those are
/// GREEN before the fix and must stay green after it; only the binding arm and the shape anchor move.
/// </para>
/// </remarks>
[TestClass]
public class PublicizedRecvOverloadTests
{
    private const string EcdsaNamespace = "go.crypto.@internal.fips140";
    private const string EcdsaPackage = "ecdsa_package";

    // The generator stamps every file it mints with this tool name; it is how an overload RecvGenerator
    // produced is told apart from a hand-written or converter-emitted member on the same class.
    private const string GeneratorToolName = "go2cs-gen";

    /// <summary>The real converted ecdsa package class — the assembly every arm below measures.</summary>
    private static Type EcdsaPackageType => typeof(go.crypto.@internal.fips140.ecdsa_package);

    #region [ Arm 1/2 — cross-assembly binding, by the compiler's own verdict ]

    // One scaffold, two call sites: the probe substitutes the statement so exactly one invocation
    // exists to resolve, and asks the SemanticModel which overload it bound.
    // ⚠ The plain `using go.crypto.@internal.fips140;` is LOAD-BEARING and is not decoration beside the
    // alias: C# finds an extension method by searching the namespaces named in `using` DIRECTIVES, and
    // a `using X = …` ALIAS imports a name without importing its namespace. Dropping it refuses BOTH
    // arms — including the exported control — for a reason that has nothing to do with accessibility,
    // which is precisely the false red a control exists to catch. This mirrors what the converter's own
    // test half writes at the CS1929 site (fips140test/acvp_test.cs carries the alias AND the namespace).
    private const string Scaffold = """
        using System;
        using go;
        using go.crypto.@internal.fips140;
        using ecdsa = go.crypto.@internal.fips140.ecdsa_package;

        public class Probe
        {
            public ж<ecdsa.hmacDRBG> Drbg = default!;
            public ж<ecdsa.PrivateKey> Key = default!;

            public void Run()
            {
                {{STATEMENT}}
            }
        }
        """;

    [TestMethod]
    public void APublicizedReceiverTypesBoxOverloadBindsFromAConsumingAssembly()
    {
        ProbeResult result = Probe("Drbg.Generate(default(slice<byte>));", "Generate");

        // The failure this arm was minted on, named verbatim so a red says which defect returned.
        Assert.AreEqual(0, result.Errors.Length,
            $"`ж<hmacDRBG>.Generate(…)` was refused cross-assembly ({result.ErrorIds}). This is the " +
            "fips140test acvp_test CS1929: RecvGenerator narrowed the ж overload of a PUBLICIZED " +
            "receiver type to `internal` by reading the Go export case of its name.");

        Assert.IsNotNull(result.Bound, "no symbol bound for `Drbg.Generate(…)`");

        Assert.IsTrue(result.BoundIsBoxOverload,
            $"`Drbg.Generate(…)` bound `{result.BoundSignature}` rather than the generated " +
            "`this ж<hmacDRBG>` overload — a box receiver cannot reach the `ref T` primary.");

        Assert.AreEqual(Accessibility.Public, result.BoundAccessibility,
            "the generated overload bound, but not as public — a consuming assembly outside the " +
            "package's InternalsVisibleTo grant could not have reached it.");
    }

    [TestMethod]
    public void AnExportedReceiverTypesBoxOverloadStillBindsFromAConsumingAssembly()
    {
        // CONTROL: the corpus's ordinary case, same package and same generator, differing from the
        // arm above ONLY in the Go export case of the receiver type. Green before the fix and after;
        // it is what proves a red above is the publicize axis and not the probe's own plumbing.
        ProbeResult result = Probe("var b = Key.Bytes();", "Bytes");

        Assert.AreEqual(0, result.Errors.Length,
            $"the ORDINARY cross-assembly case regressed ({result.ErrorIds}): `ж<PrivateKey>.Bytes()` " +
            "must keep binding. A red here is not about publicized types at all.");

        Assert.IsTrue(result.BoundIsBoxOverload,
            $"`Key.Bytes()` bound `{result.BoundSignature}` rather than the generated ж overload.");

        Assert.AreEqual(Accessibility.Public, result.BoundAccessibility,
            "the exported type's generated ж overload must be public.");
    }

    #endregion

    #region [ Arm 3 — the corpus shape the arms above rest on ]

    [TestMethod]
    public void TheExportedFactoryHandsOutABoxOfAPublicizedUnexportedType()
    {
        // If the corpus stops publicizing hmacDRBG, or stops returning a box of it, the binding arm
        // above silently changes meaning. This arm fails FIRST and says which premise moved.
        MethodInfo? factory = EcdsaPackageType.GetMethod("TestingOnlyNewDRBG",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(factory, "ecdsa.TestingOnlyNewDRBG is gone from the converted package");
        Assert.IsTrue(factory!.IsPublic, "ecdsa.TestingOnlyNewDRBG is no longer public");

        Type returnType = factory.ReturnType;

        Assert.IsTrue(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ж<>),
            $"TestingOnlyNewDRBG no longer returns a heap box; it returns {returnType}");

        Type boxed = returnType.GetGenericArguments()[0];

        Assert.AreEqual("hmacDRBG", boxed.Name, "the factory's boxed type is no longer hmacDRBG");

        Assert.IsTrue(IsPubliclyVisible(boxed),
            "hmacDRBG is no longer PUBLIC in the emission — the converter's publicize pass is the " +
            "premise of this whole class; without it there is no publicized-receiver case to guard.");

        // ...and it is unexported in Go, which is the entire reason the name-based read got it wrong.
        Assert.IsTrue(char.IsLower(boxed.Name[0]),
            "hmacDRBG is no longer an UNEXPORTED Go name; the subject/control pair has lost its axis.");
    }

    #endregion

    #region [ Arm 4/5 — the narrowing the fix must not trade away ]

    [TestMethod]
    public void NoGeneratedBoxOverloadIsPublicOverANonPublicReceiver()
    {
        // NEGATIVE CONTROL for the blanket-widening fix. A public extension method whose parameter
        // type is `ж<internalT>` is CS0051 ("inconsistent accessibility"), so this direction of the
        // narrowing is not a preference — it is what lets the corpus compile at all. Measured over
        // real assemblies rich in unexported receivers rather than argued.
        List<string> offenders = [];
        int measured = 0;

        foreach (Type packageClass in MeasuredPackageClasses())
        {
            foreach ((MethodInfo method, Type receiver) in GeneratedBoxOverloads(packageClass))
            {
                measured++;

                if (method.IsPublic && !IsPubliclyVisible(receiver))
                    offenders.Add($"{packageClass.Name}.{method.Name}(this ж<{receiver.Name}>)");
            }
        }

        // An empty sample is a broken instrument, not a pass — the same trap a filtered census hides.
        Assert.IsTrue(measured > 0,
            "no generated ж overloads were found in the measured packages; this arm measured NOTHING");

        Assert.AreEqual(0, offenders.Count,
            $"generated ж overload(s) public over a NON-public receiver (CS0051 by construction), " +
            $"out of {measured} measured: {string.Join(", ", offenders)}");
    }

    [TestMethod]
    public void AnInternalMethodOnAPublicReceiverKeepsAnInternalOverload()
    {
        // NEGATIVE CONTROL for the other half of the narrowing: the generated overload takes the
        // NARROWER of {the method's own scope, the receiver's scope}. flag.FlagSet is public and its
        // `set` is one the converter deliberately kept `internal`; re-widening it here would mint a
        // public overload over a signature the converter's own copy is not (CS0050/CS0051 the other
        // way). Green before the fix and after.
        Type flagPackage = typeof(go.flag_package);

        (MethodInfo method, Type receiver) = GeneratedBoxOverloads(flagPackage)
            .FirstOrDefault(item => item.method.Name == "set");

        Assert.IsNotNull(method,
            "flag_package has no generated ж overload named `set`; this control has lost its site " +
            "(pick another [GoRecv] internal method on an exported receiver type)");

        Assert.IsTrue(IsPubliclyVisible(receiver),
            $"the control's receiver {receiver.Name} is no longer public; it no longer varies the axis");

        Assert.IsFalse(method.IsPublic,
            $"flag.FlagSet's `set` overload was widened to public. The generator must never out-rank " +
            "the converter's own modifier on the method it is overloading.");
    }

    #endregion

    #region [ Probe plumbing ]

    private readonly record struct ProbeResult(
        IMethodSymbol? Bound,
        bool BoundIsBoxOverload,
        Accessibility BoundAccessibility,
        string BoundSignature,
        ImmutableArray<Diagnostic> Errors)
    {
        public string ErrorIds => string.Join(",", Errors.Select(static d => $"{d.Id}: {d.GetMessage()}").Distinct());
    }

    private static readonly Lazy<ImmutableArray<MetadataReference>> s_references = new(BuildReferences);

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        // Keyed by SIMPLE ASSEMBLY NAME: two MetadataReferences with equivalent identity are CS1703,
        // which would read as a refusal of the call under test rather than as a broken reference set.
        Dictionary<string, MetadataReference> byName = new(StringComparer.OrdinalIgnoreCase);

        // The framework surface, taken from THIS test host's own trusted-platform set so the probe
        // compilation targets exactly the runtime the corpus is built against.
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted)
        {
            foreach (string path in trusted.Split(Path.PathSeparator))
            {
                if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                    Add(byName, path);
            }
        }

        // golib supplies the real ж<T> and slice<T>; ecdsa supplies the real publicized/exported pair.
        // Their transitive closure comes along because hmacDRBG's own field types (hmac.HMAC, …) live
        // in sibling assemblies, and a missing one is CS0012 — which would masquerade as the CS1929
        // this class exists to detect.
        AddClosure(byName, typeof(ж<>).Assembly);
        AddClosure(byName, EcdsaPackageType.Assembly);

        return byName.Values.ToImmutableArray();
    }

    private static void AddClosure(Dictionary<string, MetadataReference> byName, Assembly assembly)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        Queue<Assembly> pending = new();
        pending.Enqueue(assembly);

        while (pending.Count > 0)
        {
            Assembly current = pending.Dequeue();

            if (!seen.Add(current.FullName ?? current.ToString()))
                continue;

            if (!string.IsNullOrEmpty(current.Location) && File.Exists(current.Location))
                Add(byName, current.Location);

            foreach (AssemblyName reference in current.GetReferencedAssemblies())
            {
                try
                {
                    pending.Enqueue(Assembly.Load(reference));
                }
                catch (Exception)
                {
                    // A reference this host cannot load is either already in the trusted-platform set
                    // or genuinely absent; either way the compiler says so as CS0012 and the arm's
                    // message prints the id, so swallowing it here cannot turn into a silent pass.
                }
            }
        }
    }

    private static void Add(Dictionary<string, MetadataReference> byName, string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);

        if (!byName.ContainsKey(name))
            byName[name] = MetadataReference.CreateFromFile(path);
    }

    private static ProbeResult Probe(string statement, string memberName)
    {
        string source = Scaffold.Replace("{{STATEMENT}}", statement);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "publicized-recv-overload-probe",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            s_references.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        ImmutableArray<Diagnostic> errors = compilation
            .GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        SyntaxTree tree = compilation.SyntaxTrees[0];
        SemanticModel model = compilation.GetSemanticModel(tree);

        MemberAccessExpressionSyntax? site = tree.GetRoot()
            .DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault(m => m.Name.Identifier.ValueText == memberName);

        // A probe with no call site is a broken probe, not a finding.
        Assert.IsNotNull(site, $"probe statement contains no `.{memberName}` site to resolve: {statement}");

        SymbolInfo info = model.GetSymbolInfo(site!);

        // ⚠ Only Symbol counts as BOUND. CandidateSymbols holds what the compiler CONSIDERED and
        // REJECTED — folding it in would report the exact refusal under test as a success.
        IMethodSymbol? bound = info.Symbol as IMethodSymbol;

        // An extension method invoked in reduced form reports the reduced symbol; ReducedFrom recovers
        // the static declaration whose FIRST parameter is the receiver, which is the only place the
        // `ref T` primary and the `ж<T>` box overload differ.
        IMethodSymbol? declared = bound?.ReducedFrom ?? bound;

        bool isBoxOverload = declared is { Parameters.Length: > 0 } &&
            declared.Parameters[0].Type is INamedTypeSymbol { IsGenericType: true } first &&
            first.ConstructedFrom.Name == "ж";

        return new ProbeResult(
            bound,
            isBoxOverload,
            declared?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            declared?.ToDisplayString() ?? "<none>",
            errors);
    }

    #endregion

    #region [ Reflection plumbing ]

    // The packages arms 4/5 measure: ecdsa carries the subject pair; fmt is the corpus's densest
    // supply of UNEXPORTED receiver types (pp, ss, readRune, …), which is what arm 4 needs to have
    // anything to measure; flag carries arm 5's internal-method-on-public-receiver site.
    private static IEnumerable<Type> MeasuredPackageClasses()
    {
        yield return EcdsaPackageType;
        yield return typeof(go.fmt_package);
        yield return typeof(go.flag_package);
    }

    // A Go method and its generated overloads are members of the receiver type's method holder, the
    // [GoMethodHost] class beside the package class, so the package's overloads are read from every holder
    // whose receiver type the package class declares.
    private static IEnumerable<Type> MethodHolders(Type packageClass) =>
        packageClass.Assembly.GetTypes().Where(type =>
            type.GetCustomAttribute<GoMethodHostAttribute>() is { } host &&
            (host.Receiver.IsGenericType ? host.Receiver.GetGenericTypeDefinition() : host.Receiver).DeclaringType == packageClass);

    private static IEnumerable<(MethodInfo method, Type receiver)> GeneratedBoxOverloads(Type packageClass)
    {
        foreach (MethodInfo method in MethodHolders(packageClass).SelectMany(holder => holder.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)))
        {
            if (method.GetCustomAttribute<GeneratedCodeAttribute>() is not { Tool: GeneratorToolName })
                continue;

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
                continue;

            Type first = parameters[0].ParameterType;

            if (!first.IsGenericType || first.GetGenericTypeDefinition() != typeof(ж<>))
                continue;

            yield return (method, first.GetGenericArguments()[0]);
        }
    }

    // A NESTED type reports IsPublic false however it is declared, so "public in the emission" is
    // IsNestedPublic for the package-class members every converted Go type is, and IsPublic for a
    // top-level one.
    private static bool IsPubliclyVisible(Type type) =>
        type.IsNested ? type.IsNestedPublic : type.IsPublic;

    #endregion
}
