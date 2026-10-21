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
        public class ж<T> { public ref TF field<TF>(FieldRefFunc<T, TF> f) => throw null!; }
        public struct array<T> { }
        public sealed class GoFieldAccessAttribute(params System.Type[] types) : System.Attribute { }
        public interface IUnsafePointer { }
        public static partial class unsafe_package { public class Pointer : IUnsafePointer { } }
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

    [TestMethod]
    public void ARecordedStructGetsARefPropertyPerField()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, $"extension(global::go.ж<{Record}> ʗp)");
        StringAssert.Contains(generated, $"public ref uint Kind => ref ʗp.field({Record}.ᏑKind);");
        StringAssert.Contains(generated, $"public ref global::go.array<ushort> Path => ref ʗp.field({Record}.ᏑPath);");
        Assert.IsFalse(generated.Contains("unrecorded"), "a struct no record lists gets no properties");
    }

    [TestMethod]
    public void AKeywordFieldKeepsItsEscapeAndItsAccessorDropsIt()
    {
        StringAssert.Contains(Generate(Package), $"public ref ushort @base => ref ʗp.field({Record}.Ꮡbase);");
    }

    [TestMethod]
    public void ACollisionNameAndABlankFieldGetNoProperty()
    {
        string generated = Generate(Package);

        Assert.IsFalse(generated.Contains(" Value =>"), "a field named like a ж<T> member would silently bind the member");
        Assert.IsFalse(generated.Contains(" _ =>"), "a blank field has no address and no accessor");
    }

    [TestMethod]
    public void AnEmbeddedStructGetsItsRefPropertySoAPromotedPathReadsByName()
    {
        StringAssert.Contains(Generate(Package), $"public ref global::go.main_package.tail tail => ref ʗp.field({Record}.Ꮡtail);");
    }

    [TestMethod]
    public void AGenericStructGetsAGenericBlockWithItsConstraints()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, "extension<T>(global::go.ж<global::go.main_package.pair<T>> ʗp) where T : struct");
        StringAssert.Contains(generated, "public ref T First => ref ʗp.field(global::go.main_package.pair<T>.ᏑFirst);");
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

        StringAssert.Contains(generated, $"public global::go.unsafe_package.Pointer Data {{ get => ʗp.field({Slice}.ᏑData); set {{ ʗp.field({Slice}.ᏑData) = value; ʗp.OnHeaderStore(); }} }}");
        StringAssert.Contains(generated, $"public nint Len {{ get => ʗp.field({Slice}.ᏑLen); set {{ ʗp.field({Slice}.ᏑLen) = value; ʗp.OnHeaderStore(); }} }}");
        StringAssert.Contains(generated, $"public nint Cap {{ get => ʗp.field({Slice}.ᏑCap); set {{ ʗp.field({Slice}.ᏑCap) = value; ʗp.OnHeaderStore(); }} }}");
        StringAssert.Contains(generated, $"public nint Len {{ get => ʗp.field({String}.ᏑLen); set {{ ʗp.field({String}.ᏑLen) = value; ʗp.OnHeaderStore(); }} }}");
    }

    [TestMethod]
    public void AStructThatIsNotHeaderShapedKeepsItsRefProperties()
    {
        string generated = Generate(Headers);

        StringAssert.Contains(generated, "public ref uint Cap => ref ʗp.field(global::go.unsafeheader_package.notHeader.ᏑCap);");
        StringAssert.Contains(generated, "public ref nint Len => ref ʗp.field(global::go.unsafeheader_package.notHeader.ᏑLen);");
        StringAssert.Contains(generated, "public ref global::go.ж<byte> Data => ref ʗp.field(global::go.unsafeheader_package.typedHeader.ᏑData);");

        // And every property of an ordinary record is still a ref.
        Assert.IsFalse(Generate(Package).Contains("OnHeaderStore"), "no ordinary struct's property commits anything");
    }

    [TestMethod]
    public void AForeignStructGetsAPropertyOnlyWhereItsAccessorIsVisible()
    {
        string generated = Generate(User, ForeignReference());
        const string Sockaddr = "global::go.syscall_package.Sockaddr";

        StringAssert.Contains(generated, $"public ref ushort Port => ref ʗp.field({Sockaddr}.ᏑPort);");
        StringAssert.Contains(generated, $"public ref ushort Family => ref ʗp.field({Sockaddr}.ᏑFamily);");
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

        StringAssert.Contains(generated, "public ref nint cap => ref ʗp.field(global::go.runtime_package.slice.Ꮡcap);");
        Assert.IsFalse(generated.Contains(" len =>"), "a second `len` on ж<slice> makes the package's own ambiguous: CS9286 at every site");
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

        StringAssert.Contains(generated, "public ref nint len => ref ʗp.field(global::go.runtime_package.slice.Ꮡlen);");
        StringAssert.Contains(generated, "public ref nint cap => ref ʗp.field(global::go.runtime_package.slice.Ꮡcap);");
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
