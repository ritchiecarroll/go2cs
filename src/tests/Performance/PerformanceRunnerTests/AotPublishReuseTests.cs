// AotPublishReuseTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceRunner;

namespace PerformanceRunnerTests;

// PerformanceRunner reuses a completed Native AOT publish (hours of ILC each) when AotPublishReuse says it is current. A
// benchmark's own .cs, its csproj and go2cs.exe are not its only build inputs: golib, the go2cs-gen analyzer and every
// converted package it references compile into the program too. The reuse predicate read none of them, so a golib-only
// change's tip arm reused the base arm's binary and timed it (found by G setting up parity seat #35's A/B). Every edit
// below keeps its file's OLD write time: the gen dll's time is rewritten by unchanged builds and a farm-adopted binary
// arrives with foreign times, so the dependencies' identity is their content, carried in the stamp.
[TestClass]
public sealed class AotPublishReuseTests
{
    private const string Sdk = "10.0.100";
    private const string Config = "Release";

    private static readonly DateTime s_old = new(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private sealed class Tree : IDisposable
    {
        public readonly string Root = Path.Combine(Path.GetTempPath(), $"go2cs-aot-reuse-{Guid.NewGuid():N}");

        public string Src => Path.Combine(Root, "src");
        public string ProjPath => Path.Combine(Src, "tests", "Performance", "PerfX");
        public string Csproj => Path.Combine(ProjPath, "PerfX.csproj");
        public string Exe => Path.Combine(ProjPath, "bin", Config, "aot", "PerfX");
        public string StampPath => Path.Combine(ProjPath, "bin", Config, "aot", "publish.stamp");
        public string Converter => Path.Combine(Src, "go2cs", "bin", "go2cs");

        public Tree()
        {
            Write("Directory.Build.props", "<Project />");
            Write("version.props", "<Project />");
            Write("core/Directory.Build.props", "<Project />");
            Write("core/golib/golib.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <Import Project="..\go2cs\go2cs.projitems" Label="Shared" />
                </Project>
                """);
            Write("core/golib/slice.cs", "public struct slice { }");
            Write("core/go2cs/go2cs.projitems", "<Project />");
            Write("core/go2cs/Symbols.cs", "static class Symbols { }");
            Write("gen/go2cs-gen/go2cs-gen.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
            Write("gen/go2cs-gen/TypeGenerator.cs", "class TypeGenerator { }");
            Write("core/fmt/fmt.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="All" />
                    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
                  </ItemGroup>
                </Project>
                """);
            Write("core/fmt/print.cs", "static class fmt { }");
            Write("core/unused/unused.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\" />");
            Write("core/unused/unused.cs", "static class unused { }");
            Write("tests/Performance/Directory.Build.props", "<Project />");
            Write("tests/Performance/PerfX/PerfX.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="All" />
                    <ProjectReference Include="$(go2csPath)core/golib/golib.csproj" />
                    <ProjectReference Include="$(go2csPath)core/fmt/fmt.csproj" />
                  </ItemGroup>
                </Project>
                """);
            Write("tests/Performance/PerfX/main.cs", "static class main { }");
            Write("go2cs/bin/go2cs", "a converter");

            // The publish: newer than every input, stamped with what this tree reads as at publish time.
            Directory.CreateDirectory(Path.GetDirectoryName(Exe)!);
            File.WriteAllText(Exe, "a published program");
            File.SetLastWriteTimeUtc(Exe, s_old.AddHours(2));
            File.WriteAllText(StampPath, CurrentStamp());
        }

        // Every input keeps the old time, so only content can tell an edit apart.
        public void Write(string relative, string text)
        {
            string path = Path.Combine(Src, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            File.SetLastWriteTimeUtc(path, s_old);
        }

        public string CurrentStamp() => AotPublishReuse.Stamp(Sdk, Config, Csproj, Src);

        public bool Reused() => AotPublishReuse.IsUpToDate(Exe, StampPath, CurrentStamp(), ProjPath, Csproj, Converter, out _);

        public void Dispose() => Directory.Delete(Root, true);
    }

    [TestMethod]
    public void AnUnchangedTreeReusesThePublish()
    {
        using Tree tree = new();

        Assert.IsTrue(tree.Reused(), "a publish newer than every input, under an unchanged tree, must be reused or every run re-pays the ILC");
    }

    [TestMethod]
    public void AGolibOnlyEditForcesARepublish()
    {
        using Tree tree = new();

        tree.Write("core/golib/slice.cs", "public struct slice { int len; }");

        Assert.IsFalse(tree.Reused(), "a golib-only change reused the previous publish: the tip arm would time the base arm's binary");
    }

    [TestMethod]
    public void AGeneratorOnlyEditForcesARepublish()
    {
        using Tree tree = new();

        tree.Write("gen/go2cs-gen/TypeGenerator.cs", "class TypeGenerator { void Emit() { } }");

        Assert.IsFalse(tree.Reused(), "a go2cs-gen-only change reused the previous publish, though the generated C# it compiles may differ");
    }

    [TestMethod]
    public void AReferencedConvertedPackageEditForcesARepublish()
    {
        using Tree tree = new();

        tree.Write("core/fmt/print.cs", "static class fmt { static void Println() { } }");

        Assert.IsFalse(tree.Reused(), "a change to a converted package the program references reused the previous publish");
    }

    [TestMethod]
    public void ASharedProjectEditForcesARepublish()
    {
        using Tree tree = new();

        // golib compiles core/go2cs through its shared-project Import, not through a ProjectReference.
        tree.Write("core/go2cs/Symbols.cs", "static class Symbols { const int N = 1; }");

        Assert.IsFalse(tree.Reused(), "a change to golib's shared project reused the previous publish");
    }

    [TestMethod]
    public void ABuildFileEditForcesARepublish()
    {
        using Tree tree = new();

        tree.Write("core/Directory.Build.props", "<Project><PropertyGroup><Optimize>false</Optimize></PropertyGroup></Project>");

        Assert.IsFalse(tree.Reused(), "a change to a Directory.Build.props the dependencies import reused the previous publish");
    }

    [TestMethod]
    public void APublishStampedWithoutTheDependencyIdentityIsNotReused()
    {
        using Tree tree = new();

        // Every stamp written before the dependencies were part of it.
        File.WriteAllText(tree.StampPath, $"sdk={Sdk};config={Config};rid=win-x64;mode=PerfAot");

        Assert.IsFalse(tree.Reused(), "a stamp that never recorded the dependencies cannot vouch for them");
    }

    [TestMethod]
    public void BuildOutputUnderADependencyStillReuses()
    {
        using Tree tree = new();

        // Every build writes these; none is an input.
        tree.Write("core/golib/bin/Release/net10.0/golib.dll", "a rebuilt assembly");
        tree.Write("core/golib/obj/Release/net10.0/golib.AssemblyInfo.cs", "// written by every build");
        tree.Write("gen/go2cs-gen/bin/Release/netstandard2.0/go2cs-gen.dll", "a rebuilt analyzer");
        tree.Write("core/fmt/Generated/go2cs-gen/go2cs.TypeGenerator/fmt.g.cs", "// emitted by every compile");

        Assert.IsTrue(tree.Reused(), "build output under a dependency forced a publish: every run would re-pay the ILC");
    }

    [TestMethod]
    public void AnUnreferencedPackageEditStillReuses()
    {
        using Tree tree = new();

        tree.Write("core/unused/unused.cs", "static class unused { static int x; }");

        Assert.IsTrue(tree.Reused(), "a change to a package the program does not reference forced a publish");
    }

    [TestMethod]
    public void LineEndingsAloneStillReuse()
    {
        using Tree tree = new();

        // A windows checkout and a linux checkout of the same commit differ only here.
        tree.Write("core/golib/slice.cs", "public struct slice { }\r\n");
        string lf = tree.CurrentStamp();
        tree.Write("core/golib/slice.cs", "public struct slice { }\n");

        Assert.AreEqual(lf, tree.CurrentStamp(), "the dependency identity must not depend on the checkout's line endings");
    }
}
