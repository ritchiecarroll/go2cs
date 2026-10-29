// FieldAccessGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Pins <see cref="FieldAccessGenerator"/>: the field properties it emits on <c>ж&lt;T&gt;</c> for the struct types
/// the <c>[GoFieldAccess]</c> records list, and the collision list it shares with the converter.
/// </summary>
/// <remarks>
/// The generated text is read as text: this Roslyn cannot compile a C# 14 extension block, and the end-to-end
/// compile and run is the FieldAccessReinterpret behavioral row.
/// </remarks>
[TestClass]
public class FieldAccessGeneratorTests
{
    private const string Stubs = """
        namespace go;

        public delegate ref TField FieldRefFunc<T, TField>(ref T instance);
        public class ж<T> { public ref TF field<TF>(FieldRefFunc<T, TF> f) => throw null!; public ref T Value => throw null!; }
        public sealed class NativeBox<T> : ж<T> { }
        public struct array<T> { }
        public sealed class GoFieldAccessAttribute(params System.Type[] types) : System.Attribute { }
        public interface IUnsafePointer { }
        public static partial class unsafe_package { public class Pointer : IUnsafePointer { } }
        public struct uintptr { public nuint Value; }
        """;

    private const string Package = """
        namespace go;

        [GoFieldAccess(typeof(record))]
        partial class main_package
        {
            public partial struct tail { public ushort A, B; }

            public partial struct record
            {
                public uint Kind;
                public ushort @base;
                public array<ushort> Path;
                public uint Value;
                public ushort _;
                internal partial ref tail tail { get; }
            }

            public partial struct unrecorded { public uint X; }
        }

        [GoFieldAccess(typeof(main_package.pair<>))]
        partial class main_package
        {
            public partial struct pair<T> where T : struct { public T First; public int Count; }
        }
        """;

    // A FOREIGN struct, compiled to metadata the way a referenced package's assembly reaches the generator: its
    // Ꮡ accessors are visible and decide which fields get a property.
    private const string Foreign = """
        namespace go;

        public static partial class syscall_package
        {
            public partial struct Sockaddr
            {
                public ushort Family;
                public ushort Port;
                public uint NoAccessor;
                internal uint hidden;

                public static ref ushort ᏑFamily(ref Sockaddr instance) => ref instance.Family;
                public static ref ushort ᏑPort(ref Sockaddr instance) => ref instance.Port;
                internal static ref uint Ꮡhidden(ref Sockaddr instance) => ref instance.hidden;
            }
        }
        """;

    private const string User = """
        namespace go;

        [GoFieldAccess(typeof(syscall_package.Sockaddr))]
        partial class user_package { }
        """;

    private static string Generate(string source, params MetadataReference[] references)
    {
        CSharpCompilation compilation = CSharpCompilation.Create("test",
            [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), .. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();

        Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

        return result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "";
    }

    private static MetadataReference ForeignReference()
    {
        CSharpCompilation foreign = CSharpCompilation.Create("syscall",
            [CSharpSyntaxTree.ParseText(Foreign)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        Assert.IsTrue(foreign.Emit(image).Success, "the foreign fixture compiles");

        return MetadataReference.CreateFromImage(image.ToArray());
    }

    // Roslyn 4.14 (go2cs-gen's floor) displays a type named `record`, a contextual keyword, escaped; 4.10 printed it
    // bare. Both spellings compile.
    private const string Record = "global::go.main_package.@record";

    // Every accessor's attributes: go2cs machinery, inlined, and no Go frame where the JIT keeps one (design Q4).
    private static readonly string Getter =
        $"[{Common.GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

    // A field's property, with the specialized body (design Q1): the field in place on a managed box, at its Go offset
    // through the accessor on a NativeBox<T>. `field` is the field's own name where the property's is escaped.
    private static string Ref(string fieldType, string name, string type, string? field = null) =>
        $"public ref {fieldType} {name} {{ {Getter} get => ref ʗp is global::go.NativeBox<{type}> ? ref ʗp.field({type}.Ꮡ{field ?? name}) : ref ʗp.Value.{name}; }}";

    // A header-shaped struct's field: get/set over the accessor, the setter committing the header.
    private static string Header(string fieldType, string name, string type) =>
        $"public {fieldType} {name} {{ {Getter} get => ʗp.field({type}.Ꮡ{name}); {Getter} set {{ ʗp.field({type}.Ꮡ{name}) = value; ʗp.OnHeaderStore(); }} }}";

    [TestMethod]
    public void ARecordedStructGetsARefPropertyPerField()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, $"extension(global::go.ж<{Record}> ʗp)");
        StringAssert.Contains(generated, Ref("uint", "Kind", Record));
        StringAssert.Contains(generated, Ref("global::go.array<ushort>", "Path", Record));
        Assert.IsFalse(generated.Contains("unrecorded"), "a struct no record lists gets no properties");
    }

    [TestMethod]
    public void AKeywordFieldKeepsItsEscapeAndItsAccessorDropsIt()
    {
        StringAssert.Contains(Generate(Package), Ref("ushort", "@base", Record, field: "base"));
    }

    [TestMethod]
    public void ACollisionNameAndABlankFieldGetNoProperty()
    {
        string generated = Generate(Package);

        // The control: the shape these negatives look for is the one every property takes.
        StringAssert.Contains(generated, " Kind { ");
        Assert.IsFalse(generated.Contains(" Value { "), "a field named like a ж<T> member would silently bind the member");
        Assert.IsFalse(generated.Contains(" _ { "), "a blank field has no address and no accessor");
    }

    [TestMethod]
    public void AnEmbeddedStructGetsItsRefPropertySoAPromotedPathReadsByName()
    {
        StringAssert.Contains(Generate(Package), Ref("global::go.main_package.tail", "tail", Record));
    }

    [TestMethod]
    public void AGenericStructGetsAGenericBlockWithItsConstraints()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, "extension<T>(global::go.ж<global::go.main_package.pair<T>> ʗp) where T : struct");
        StringAssert.Contains(generated, Ref("T", "First", "global::go.main_package.pair<T>"));
    }

    // A type another generator emits into the package class (the ImplementGenerator's constraint proxy, SelfRefGenerics'
    // `gnodeжGraphNode`) is unresolved where this generator runs, since generators do not see each other's output.
    // The field names it bare from inside the package class; the extension class sits outside it, so the property's
    // type roots every unresolved name at the struct's package class, at any depth of the type (go2cs.slnx: CS0246).
    private const string Proxied = """
        namespace go;

        [GoFieldAccess(typeof(gnode))]
        partial class main_package
        {
            public partial struct Graph<N> { public N Root; }

            public partial struct gnode { public ж<Graph<gnodeжGraphNode>> g; public gnodeжGraphNode self; public int id; }
        }
        """;

    [TestMethod]
    public void AFieldTypeNamingAnotherGeneratorsTypeIsRootedAtThePackageClass()
    {
        string generated = Generate(Proxied);
        const string Gnode = "global::go.main_package.gnode";

        StringAssert.Contains(generated, Ref("global::go.ж<global::go.main_package.Graph<global::go.main_package.gnodeжGraphNode>>", "g", Gnode));
        StringAssert.Contains(generated, Ref("global::go.main_package.gnodeжGraphNode", "self", Gnode));
        StringAssert.Contains(generated, Ref("int", "id", Gnode), "a resolved field type is rendered as before");
    }

    // A READONLY field is a zero-size layout field, laid over its neighbour at Go's offset: it has no writable slot of
    // its own, and its accessor answers golib's shared zero-size slot. Its property keeps the accessor's body, since
    // `ref ʗp.Value.f` would not compile (CS8329).
    private const string ZeroSize = """
        namespace go;

        [GoFieldAccess(typeof(counter))]
        partial class atomic_package
        {
            public partial struct counter { internal readonly noCopy _noCopy; public readonly noCopy pad; public int v; }

            public partial struct noCopy { }
        }
        """;

    [TestMethod]
    public void AReadonlyFieldKeepsTheAccessorsBody()
    {
        string generated = Generate(ZeroSize);
        const string Counter = "global::go.atomic_package.counter";

        StringAssert.Contains(generated, $"public ref global::go.atomic_package.noCopy pad {{ {Getter} get => ref ʗp.field({Counter}.Ꮡpad); }}");
        StringAssert.Contains(generated, Ref("int", "v", Counter), "the writable sibling takes the specialized body");
    }

    [TestMethod]
    public void EveryAccessorIsMachineryAndInlined()
    {
        string generated = Generate(Package);
        int properties = generated.Split(" get => ").Length - 1;

        Assert.IsTrue(properties > 0, "the fixture generates properties");
        Assert.AreEqual(properties, generated.Split($"{Getter} get => ").Length - 1, "every getter carries [GeneratedCode] and [MethodImpl(AggressiveInlining)]");
    }

    // abi.ArrayType's shape: a field that shadows a method an embedded type promotes. Go selects the field, but the box
    // carries the promoted method as an extension member, and a property beside it would make every `p.Len` ambiguous;
    // the converter keeps the box form for such a field (shadowsPromotedMethod), so the property would serve nothing.
    // The promoted method sits one embed down (Type), two embeds down (through outer's value embed of ArrayType's
    // shape), and behind a POINTER embed; a sibling field that shadows nothing keeps its property, and so does a field
    // whose name matches a method of an unrelated type.
    private const string Shadows = """
        namespace go;

        [GoFieldAccess(typeof(ArrayType), typeof(deeper), typeof(viaPointer), typeof(unrelated))]
        partial class abi_package
        {
            public partial struct Type { public nint Size_; }

            public partial struct ArrayType
            {
                /*embed*/ public Type Type;
                public nint Len;
                public nint Elem;
            }

            public partial struct deeper
            {
                /*embed*/ public ArrayType ArrayType;
                public nint Kind;
                public nint Cap;
            }

            public partial struct viaPointer
            {
                /*embed*/ public ж<Type> Type;
                public nint Len;
            }

            public partial struct other { }

            public partial struct unrelated { public nint Len; }

            public static nint Len(this ж<Type> t) => 0;

            public static nint Kind(this ref Type t) => 0;

            public static nint Len(this ж<other> o) => 0;
        }
        """;

    [TestMethod]
    public void AFieldThatShadowsAPromotedMethodGetsNoProperty()
    {
        string generated = Generate(Shadows);
        const string Package = "global::go.abi_package";

        Assert.IsFalse(generated.Contains($"{Package}.ArrayType.ᏑLen"), "ArrayType.Len shadows the Len its embedded Type promotes");
        Assert.IsFalse(generated.Contains($"{Package}.deeper.ᏑKind"), "deeper.Kind shadows the Kind promoted two embeds down");
        Assert.IsFalse(generated.Contains($"{Package}.viaPointer.ᏑLen"), "a pointer embed promotes its type's methods too");

        StringAssert.Contains(generated, Ref("nint", "Elem", $"{Package}.ArrayType"), "a sibling that shadows nothing keeps its property");
        StringAssert.Contains(generated, Ref("nint", "Cap", $"{Package}.deeper"));
        StringAssert.Contains(generated, Ref("nint", "Len", $"{Package}.unrelated"), "a method of an unrelated type shadows nothing");
    }

    // HEADER-SHAPED structs: an unsafe.Pointer field, then one or two ints, in declaration order. Go's slice and string
    // headers (internal/unsafeheader's Slice and String, runtime's slice and stringStruct, internal/runtime/maps'
    // stringStruct), which golib's header boxes serve over a slice or string variable. Beside them, a struct that only
    // resembles one (a third field that is not an int) and one whose first field is a typed pointer.
    private const string Headers = """
        namespace go;

        [GoFieldAccess(typeof(Slice), typeof(String), typeof(notHeader), typeof(typedHeader))]
        partial class unsafeheader_package
        {
            public partial struct Slice { public unsafe_package.Pointer Data; public nint Len; public nint Cap; }

            public partial struct String { public unsafe_package.Pointer Data; public nint Len; }

            public partial struct notHeader { public unsafe_package.Pointer Data; public nint Len; public uint Cap; }

            public partial struct typedHeader { public ж<byte> Data; public nint Len; public nint Cap; }
        }
        """;

    [TestMethod]
    public void AHeaderShapedStructGetsGetSetPropertiesWhoseSetterCommitsTheHeader()
    {
        // A ref cannot be intercepted, and the header's last store is followed by no access of the box, so the setter is
        // the one point at which golib's header box sees the finished header (ж<T>.OnHeaderStore).
        string generated = Generate(Headers);
        const string Slice = "global::go.unsafeheader_package.Slice";
        const string String = "global::go.unsafeheader_package.String";

        StringAssert.Contains(generated, Header("global::go.unsafe_package.Pointer", "Data", Slice));
        StringAssert.Contains(generated, Header("nint", "Len", Slice));
        StringAssert.Contains(generated, Header("nint", "Cap", Slice));
        StringAssert.Contains(generated, Header("nint", "Len", String));
    }

    // The predicate's arity and order checks (G, review of 94ec9ea487): a header is exactly two or three fields, the
    // pointer FIRST, and every later field a native int. golib's uintptr is a struct, not nint, so a Go uintptr word
    // does not make a header either.
    private const string NearMisses = """
        namespace go;

        [GoFieldAccess(typeof(fourWords), typeof(pointerSecond), typeof(pointerAlone), typeof(uintptrWord))]
        partial class nearmiss_package
        {
            public partial struct fourWords { public unsafe_package.Pointer Data; public nint Len; public nint Cap; public nint Extra; }

            public partial struct pointerSecond { public nint Len; public unsafe_package.Pointer Data; }

            public partial struct pointerAlone { public unsafe_package.Pointer Data; }

            public partial struct uintptrWord { public unsafe_package.Pointer Data; public uintptr Len; }
        }
        """;

    [TestMethod]
    public void ANearMissOfTheHeaderShapeKeepsItsRefProperties()
    {
        string generated = Generate(NearMisses);
        const string Package = "global::go.nearmiss_package";

        StringAssert.Contains(generated, Ref("nint", "Extra", $"{Package}.fourWords"), "four words: one too many");
        StringAssert.Contains(generated, Ref("global::go.unsafe_package.Pointer", "Data", $"{Package}.pointerSecond"), "the pointer second: the order is checked");
        StringAssert.Contains(generated, Ref("global::go.unsafe_package.Pointer", "Data", $"{Package}.pointerAlone"), "the pointer alone: one too few");
        StringAssert.Contains(generated, Ref("global::go.uintptr", "Len", $"{Package}.uintptrWord"), "a uintptr word is golib's struct, not an int");
        Assert.IsFalse(generated.Contains("OnHeaderStore"), "no near miss of the header shape commits anything");
    }

    [TestMethod]
    public void AStructThatIsNotHeaderShapedKeepsItsRefProperties()
    {
        string generated = Generate(Headers);

        StringAssert.Contains(generated, Ref("uint", "Cap", "global::go.unsafeheader_package.notHeader"));
        StringAssert.Contains(generated, Ref("nint", "Len", "global::go.unsafeheader_package.notHeader"));
        StringAssert.Contains(generated, Ref("global::go.ж<byte>", "Data", "global::go.unsafeheader_package.typedHeader"));

        // And every property of an ordinary record is still a ref.
        Assert.IsFalse(Generate(Package).Contains("OnHeaderStore"), "no ordinary struct's property commits anything");
    }

    [TestMethod]
    public void AForeignStructGetsAPropertyOnlyWhereItsAccessorIsVisible()
    {
        string generated = Generate(User, ForeignReference());
        const string Sockaddr = "global::go.syscall_package.Sockaddr";

        StringAssert.Contains(generated, Ref("ushort", "Port", Sockaddr));
        StringAssert.Contains(generated, Ref("ushort", "Family", Sockaddr));
        Assert.IsFalse(generated.Contains("NoAccessor"), "a foreign field with no Ꮡ accessor has nothing to reach it through");
        Assert.IsFalse(generated.Contains("hidden"), "an internal foreign field is not accessible here");
    }

    // A test assembly that REFERENCES its package's assembly sees the package's internal GoFieldAccess class through
    // InternalsVisibleTo. Two extension properties of one name on one receiver are ambiguous, which C# reports as no
    // member at all (CS9286): runtime's export_test.cs MemclrBytes, `s.Δarray` on a ж<Δsliceᴛ> that alg.cs records
    // too. The package's class is the metadata a C# 14 `extension(ж<slice> ʗp) { public ref nint len => … }` block
    // leaves behind, its implementation method, since this Roslyn cannot compile the block itself.
    private static string GenerateBeside(string package, string user)
    {
        CSharpCompilation packageCompilation = CSharpCompilation.Create("runtime",
            [CSharpSyntaxTree.ParseText(package), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        Assert.IsTrue(packageCompilation.Emit(image).Success, "the package fixture compiles");

        CSharpCompilation compilation = CSharpCompilation.Create("test",
            [CSharpSyntaxTree.ParseText(user)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromImage(image.ToArray())],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();

        Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

        return result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "";
    }

    private const string RuntimePackage = """
        [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

        namespace go;

        public static partial class runtime_package
        {
            internal partial struct slice
            {
                internal nint len;
                internal nint cap;

                internal static ref nint Ꮡlen(ref slice instance) => ref instance.len;
                internal static ref nint Ꮡcap(ref slice instance) => ref instance.cap;
            }
        }

        internal static class GoFieldAccess
        {
            public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;
        }
        """;

    private const string RuntimeTest = """
        namespace go;

        [GoFieldAccess(typeof(runtime_package.slice))]
        partial class runtime_test_package { }
        """;

    [TestMethod]
    public void APropertyTheReferencedPackageAlreadyGivesIsNotDeclaredAgain()
    {
        string generated = GenerateBeside(RuntimePackage, RuntimeTest);

        StringAssert.Contains(generated, Ref("nint", "cap", "global::go.runtime_package.slice"));
        Assert.IsFalse(generated.Contains(" len { "), "a second `len` on ж<slice> makes the package's own ambiguous: CS9286 at every site");
    }

    [TestMethod]
    public void APropertyTheReferencedPackageHidesIsDeclared()
    {
        // The control: with no InternalsVisibleTo grant the package's class is out of reach, so nothing is shadowed.
        // The fixture's struct and accessors are internal too, so they take the grant's place as public.
        string package = RuntimePackage
            .Replace("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"test\")]", "")
            .Replace("internal partial struct slice", "public partial struct slice")
            .Replace("internal nint", "public nint")
            .Replace("internal static ref", "public static ref");

        string generated = GenerateBeside(package, RuntimeTest);

        StringAssert.Contains(generated, Ref("nint", "len", "global::go.runtime_package.slice"));
        StringAssert.Contains(generated, Ref("nint", "cap", "global::go.runtime_package.slice"));
    }

    // The real thing, end to end: the package's block is a C# 14 extension compiled by the SDK's own compiler, the
    // generator runs over a test assembly that references it, and the SDK compiler then compiles that assembly with
    // its generated class and a `p.len` site. The arms above read the generated text; this one shows CS9286 itself
    // when the property is declared twice. Inconclusive only where no SDK compiler resolves.
    [TestMethod]
    public void ASiteOnAPropertyThePackageGivesCompilesInTheTestAssembly()
    {
        if (SdkCompiler() is not { } compiler)
        {
            Assert.Inconclusive("no SDK csc.dll resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): nothing here can compile a C# 14 extension block");
            return;
        }

        string directory = Directory.CreateTempSubdirectory("go2cs-fieldaccess-").FullName;

        try
        {
            string package = RuntimePackage.Replace(
                "public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;",
                "extension(ж<runtime_package.slice> ʗp) { public ref nint len => ref ʗp.field(runtime_package.slice.Ꮡlen); }");

            Assert.AreNotEqual(RuntimePackage, package, "the fixture's metadata stand-in was replaced by a real block");

            File.WriteAllText(Path.Combine(directory, "package.cs"), package);
            File.WriteAllText(Path.Combine(directory, "stubs.cs"), Stubs);

            (int code, string output) = Compile(compiler, directory, "runtime.dll", ["package.cs", "stubs.cs"], []);
            Assert.AreEqual(0, code, $"the package fixture compiles under the SDK compiler:\n{output}");

            CSharpCompilation compilation = CSharpCompilation.Create("test",
                [CSharpSyntaxTree.ParseText(RuntimeTest)],
                [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(Path.Combine(directory, "runtime.dll"))],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();
            Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

            File.WriteAllText(Path.Combine(directory, "test.cs"), RuntimeTest);
            File.WriteAllText(Path.Combine(directory, "generated.cs"), result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "");
            File.WriteAllText(Path.Combine(directory, "site.cs"), """
                namespace go;

                static class site
                {
                    static nint read(ж<runtime_package.slice> s) => s.len + s.cap;
                }
                """);

            (code, output) = Compile(compiler, directory, "test.dll", ["test.cs", "generated.cs", "site.cs"], ["runtime.dll"]);
            Assert.AreEqual(0, code, $"`s.len` and `s.cap` bind in the test assembly:\n{output}");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // The SDK's csc.dll beside the dotnet host, newest SDK first: (host, csc.dll).
    private static (string Host, string Csc)? SdkCompiler()
    {
        string exe = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        List<string> hosts = [];

        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host)
            hosts.Add(host);

        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { Length: > 0 } root)
            hosts.Add(Path.Combine(root, exe));

        hosts.AddRange((Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Where(dir => dir.Length > 0).Select(dir => Path.Combine(dir, exe)));

        foreach (string candidate in hosts.Where(File.Exists))
        {
            string sdk = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(candidate))!, "sdk");

            if (!Directory.Exists(sdk))
                continue;

            string? csc = Directory.GetDirectories(sdk)
                .Select(version => Path.Combine(version, "Roslyn", "bincore", "csc.dll"))
                .Where(File.Exists)
                .OrderByDescending(path => Version.TryParse(Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)))!.Split('-')[0]), out Version? version) ? version : new Version())
                .FirstOrDefault();

            if (csc is not null)
                return (candidate, csc);
        }

        return null;
    }

    // Compiles a library with the SDK compiler against this runtime's own framework assemblies.
    private static (int Code, string Output) Compile((string Host, string Csc) compiler, string directory, string output, string[] sources, string[] references)
    {
        string framework = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        List<string> arguments = [compiler.Csc, "-nologo", "-noconfig", "-nostdlib+", "-target:library", "-langversion:14", "-nullable:annotations", $"-out:{output}",
            $"-r:{Path.Combine(framework, "System.Private.CoreLib.dll")}", $"-r:{Path.Combine(framework, "System.Runtime.dll")}"];

        arguments.AddRange(references.Select(reference => $"-r:{reference}"));
        arguments.AddRange(sources);

        ProcessStartInfo start = new(compiler.Host, arguments)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = Process.Start(start)!;
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(120_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"csc {output} did not finish in 120 s");
        }

        return (process.ExitCode, stdout + stderr);
    }

    // The converter writes the records in package_info.cs, on the assembly (parity seat #31); the class form above is
    // the committed corpus's until its regeneration. Both are read, and a type both list gets one block.
    private const string AssemblyRecords = """
        [assembly: global::go.GoFieldAccess(typeof(global::go.main_package.point))]
        [assembly: global::go.GoFieldAccess(typeof(global::go.main_package.pair<>))]

        namespace go;

        partial class main_package
        {
            public partial struct point { public int X, Y; }

            public partial struct pair<T> where T : struct { public T First; public int Count; }
        }

        [GoFieldAccess(typeof(main_package.point))]
        partial class main_package { }
        """;

    [TestMethod]
    public void AnAssemblyRecordGetsItsPropertiesBesideTheClassForm()
    {
        string generated = Generate(AssemblyRecords);

        StringAssert.Contains(generated, Ref("int", "X", "global::go.main_package.point"));
        StringAssert.Contains(generated, Ref("T", "First", "global::go.main_package.pair<T>"));
        Assert.AreEqual(1, generated.Split("extension(global::go.ж<global::go.main_package.point> ʗp)").Length - 1, "a type both forms list gets one block");

        string assemblyOnly = AssemblyRecords.Replace("[GoFieldAccess(typeof(main_package.point))]", "");
        Assert.AreNotEqual(AssemblyRecords, assemblyOnly, "the fixture's class record was removed");
        StringAssert.Contains(Generate(assemblyOnly), Ref("int", "Y", "global::go.main_package.point"), "the assembly record alone generates");
    }

    [TestMethod]
    public void TheClassIsInternalSoTwoPackagesExtendingOneTypeNeverMeet()
    {
        StringAssert.Contains(Generate(Package), "internal static class GoFieldAccess");
    }

    [TestMethod]
    public void NoRecordGeneratesNothing()
    {
        Assert.AreEqual("", Generate(Package.Replace("[GoFieldAccess(typeof(record))]", "").Replace("[GoFieldAccess(typeof(main_package.pair<>))]", "")));
    }

    /// <summary>
    /// The guard on the collision list: every public instance member of golib's <c>ж&lt;T&gt;</c>, and every golib
    /// extension member whose receiver is a <c>ж&lt;…&gt;</c>, must be on it. C# binds an instance member before an
    /// extension property, so a Go field named like a member the list lacks would silently bind the member.
    /// </summary>
    [TestMethod]
    public void TheCollisionListNamesEveryMemberOfTheBox()
    {
        Type box = typeof(golib::go.ж<>);
        HashSet<string> names = [];

        foreach (MemberInfo member in box.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            if (member is System.Reflection.MethodInfo { IsSpecialName: true })
                continue;

            names.Add(member.Name);
        }

        foreach (Type type in box.Assembly.GetExportedTypes().Where(type => type.IsAbstract && type.IsSealed))
        {
            foreach (System.Reflection.MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!method.IsDefined(typeof(ExtensionAttribute), false) || method.GetParameters() is not [var receiver, ..])
                    continue;

                if (IsBox(receiver.ParameterType))
                    names.Add(method.Name);
            }
        }

        string[] missing = names.Where(name => !FieldAccessGenerator.CollisionNames.Contains(name)).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.AreEqual(0, missing.Length, $"src/go2cs/fieldAccessCollisions.txt lacks: {string.Join(", ", missing)}");
        Assert.IsTrue(names.Contains("Value") && names.Contains("of"), "the guard must see the box's own members");

        static bool IsBox(Type type)
        {
            for (Type? current = type; current is not null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(golib::go.ж<>))
                    return true;
            }

            return false;
        }
    }
}
