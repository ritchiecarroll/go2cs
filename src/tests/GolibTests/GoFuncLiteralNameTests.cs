// GoFuncLiteralNameTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests
{
    /// <summary>
    /// A function literal's frame is named from the [GoFunc] attribute the converter writes on its lambda (golib
    /// GoFuncAttribute, runtime goFrameName), with no symbol file: the stand-in for Native AOT, where there is none. The
    /// fixture is its own assembly, compiled here WITHOUT a PDB, holding the shape the converter emits for two types'
    /// same-named methods — one package class, two `Name` overloads, a literal in each — which no PDB-based record could
    /// tell apart: the literal's `&lt;Name&gt;b__X_Y` names neither receiver. The control is the same literal without the
    /// attribute, which takes the name the runtime derives from the frame. The end-to-end guard is the behavioral
    /// LiteralFrameNames.
    /// </summary>
    [TestClass]
    public class GoFuncLiteralNameTests
    {
        private const string Source = """
            namespace go
            {
                public static class litfixture_package
                {
                    public static System.Func<string>? probe;

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static string Name(int c)
                    {
                        System.Func<string> f = [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining), GoFunc("(*counter).Name.func1")] () => probe!();
                        return f();
                    }

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static string Name(string g)
                    {
                        System.Func<string> f = [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining), GoFunc("gauge.Name.func1")] () => probe!();
                        return f();
                    }

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static string unattributed()
                    {
                        System.Func<string> f = [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)] () => probe!();
                        return f();
                    }
                }
            }
            """;

        private static Type s_fixture = null!;

        [ClassInitialize]
        public static void LoadFixture(TestContext _)
        {
            string name = "litfixture_" + Guid.NewGuid().ToString("N");
            string directory = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(directory);

            string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            MetadataReference[] references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(GoFrame).Assembly.Location),
            ];

            SyntaxTree tree = CSharpSyntaxTree.ParseText(SourceText.From(Source, Encoding.UTF8), path: Path.Combine(directory, "litfixture.cs"));
            CSharpCompilation compilation = CSharpCompilation.Create(name, [tree], references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

            string dll = Path.Combine(directory, name + ".dll");
            EmitResult emitted;

            using (FileStream image = File.Create(dll))
                emitted = compilation.Emit(image);

            Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
            Assert.IsFalse(File.Exists(Path.Combine(directory, name + ".pdb")), "control: the fixture has no symbol file");

            s_fixture = Assembly.LoadFrom(dll).GetType("go.litfixture_package")!;

            // The probe reports the first Go frame above it, the literal that calls it (this assembly's frames are not Go's).
            s_fixture.GetField("probe")!.SetValue(null, (Func<string>)CallerFrameTestVariantNamingTests.CallerFunctionName);
        }

        private static string Call(string method, object argument) =>
            (string)s_fixture.GetMethod(method, [argument.GetType()])!.Invoke(null, [argument])!;

        [TestMethod]
        public void APointerReceiversLiteralIsNamedFromItsAttribute()
        {
            Assert.AreEqual("litfixture.(*counter).Name.func1", Call("Name", 0));
        }

        [TestMethod]
        public void AValueReceiversSameNamedLiteralIsNamedFromItsOwn()
        {
            Assert.AreEqual("litfixture.gauge.Name.func1", Call("Name", ""));
        }

        [TestMethod]
        public void ControlALiteralWithoutTheAttributeTakesTheDerivedName()
        {
            string name = (string)s_fixture.GetMethod("unattributed")!.Invoke(null, null)!;

            StringAssert.StartsWith(name, "litfixture.unattributed.func", "the fallback names a literal by its outer method and an ordinal");
        }
    }
}
