// FieldAccessGeneratorTests.cs - Gbtc
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

    private const string Record = "global::go.main_package.record";

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
