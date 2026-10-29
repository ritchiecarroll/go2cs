// DeferThunkFrameTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
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
    /// A go/defer THUNK is elided from a live walk as Go elides its deferwrap/gowrap closure (runtime's
    /// isRecordedThunk, seat #42): the converter's `defer(() => f(...), ref ᒐ)` lambda, listed by its body's
    /// (line, column) in the file's GoPositionMap record. The position is a PDB property, so this is the one
    /// symbol read a capture makes, and these pin its cost. The fixture is its own assembly, compiled here, so
    /// its symbol file is an untouched witness: a recorded thunk opens exactly that assembly, once; with no
    /// symbol file the thunk shows, as before seat #42, without a throw and with one attempt per assembly; and
    /// an unrecorded block literal (a Go func literal) is a frame either way. The end-to-end guards are the
    /// behavioral LineMarkerDeferEndTC0 and DeferThunkFrames.
    /// </summary>
    [TestClass]
    public class DeferThunkFrameTests
    {
        private const string Source = """
            [assembly: go.GoPositionMap("thunkfixture/fixture.go", "thunkfixture.cs", "", "", "", "", "THUNKSITE")]

            namespace go
            {
                public static class thunkfixture_package
                {
                    public static System.Action? probe;

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void viaThunk()
                    {
                        GoFrame ᒐ = default;

                        try
                        {
                            builtin.defer(() => probe!(), ref ᒐ);
                        }
                        finally { ᒐ.Run(); }
                    }

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void viaLiteral()
                    {
                        GoFrame ᒐ = default;

                        try
                        {
                            builtin.defer([System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)] () =>
                            {
                                probe!();
                            }, ref ᒐ);
                        }
                        finally { ᒐ.Run(); }
                    }
                }
            }
            """;

        private const string ThunkLine = "builtin.defer(() => probe!(), ref ᒐ);";

        private static string[] Opens() =>
            (string[]?)typeof(runtime_package).GetMethod("symbolFileOpens", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null) ?? [];

        // The fixture's text with THUNKSITE replaced by the thunk body's 1-based line and column, the units a
        // portable PDB counts in (the line is ASCII up to the body, so characters are UTF-16 units).
        private static (string text, int line, int column) FixtureText()
        {
            string[] lines = Source.Replace("\r\n", "\n").Split('\n');
            int index = Array.FindIndex(lines, l => l.Contains(ThunkLine));
            int column = lines[index].IndexOf("probe!()", StringComparison.Ordinal) + 1;

            return (string.Join("\n", lines).Replace("THUNKSITE", $"{index + 1}:{column}"), index + 1, column);
        }

        // Compiles the fixture under a unique assembly name into a fresh directory, with or without its symbol
        // file, and loads it from disk, so the runtime finds (or misses) the PDB beside it.
        private static (Type type, string name) LoadFixture(bool withPdb)
        {
            (string text, _, _) = FixtureText();
            string name = "thunkfixture_" + Guid.NewGuid().ToString("N");
            string directory = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(directory);

            string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            MetadataReference[] references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(GoFrame).Assembly.Location),
            ];

            SyntaxTree tree = CSharpSyntaxTree.ParseText(SourceText.From(text, Encoding.UTF8), path: Path.Combine(directory, "thunkfixture.cs"));
            CSharpCompilation compilation = CSharpCompilation.Create(name, [tree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            string dll = Path.Combine(directory, name + ".dll");
            EmitResult emitted;

            using (FileStream image = File.Create(dll))
            {
                if (withPdb)
                {
                    using FileStream pdb = File.Create(Path.Combine(directory, name + ".pdb"));
                    emitted = compilation.Emit(image, pdb, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb, pdbFilePath: name + ".pdb"));
                }
                else
                {
                    emitted = compilation.Emit(image);
                }
            }

            Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
            Assert.AreEqual(withPdb, File.Exists(Path.Combine(directory, name + ".pdb")), "control: the symbol file is present exactly when asked for");

            return (Assembly.LoadFrom(dll).GetType("go.thunkfixture_package")!, name);
        }

        // runtime.Callers from inside the deferred call: the Go frames above the capture, counted.
        private static nint Capture(Type fixture, string method)
        {
            nint n = 0;
            slice<uintptr> pcs = new uintptr[64];

            fixture.GetField("probe")!.SetValue(null, (Action)(() => n = runtime_package.Callers(0, pcs)));
            fixture.GetMethod(method)!.Invoke(null, null);

            return n;
        }

        [TestMethod]
        public void ARecordedThunkIsElidedAndOpensExactlyItsAssemblyOnce()
        {
            (Type fixture, string name) = LoadFixture(withPdb: true);
            (_, int line, int column) = FixtureText();

            // Control: the recorded site IS the thunk lambda's first sequence point, as the PDB reports it.
            MethodInfo thunk = fixture.GetNestedTypes(BindingFlags.NonPublic).SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Single(m => m.Name.Contains("viaThunk") && m.Name.Contains(">b__"));

            string[] before = Opens();
            nint viaThunk = Capture(fixture, "viaThunk");
            nint again = Capture(fixture, "viaThunk");
            string[] added = Opens().Skip(before.Length).ToArray();

            CollectionAssert.AreEqual(new[] { name }, added, "a capture over a recorded thunk must open exactly the thunk's assembly, once");

            nint viaLiteral = Capture(fixture, "viaLiteral");

            Assert.AreEqual(viaThunk, again, "the same stack captures the same depth");
            Assert.AreEqual(viaLiteral - 1, viaThunk, "the thunk is elided, a block literal is a frame of its own");
            CollectionAssert.AreEqual(new[] { name }, Opens().Skip(before.Length).ToArray(), "the literal's classification opens nothing more");

            var (_, startLine, startColumn) = ((string?, int, int))typeof(runtime_package).GetMethod("methodSourceStart", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [thunk])!;
            Assert.AreEqual((line, column), (startLine, startColumn), "control: the recorded site is the thunk's first sequence point");
        }

        [TestMethod]
        public void WithNoSymbolFileTheThunkShowsAndIsReadOnce()
        {
            (Type fixture, string name) = LoadFixture(withPdb: false);

            string[] before = Opens();
            nint viaThunk = Capture(fixture, "viaThunk");
            nint again = Capture(fixture, "viaThunk");
            nint viaLiteral = Capture(fixture, "viaLiteral");

            Assert.AreEqual(viaLiteral, viaThunk, "with no symbol file the thunk cannot be recognised, and counts as a frame, as before seat #42");
            Assert.AreEqual(viaThunk, again);
            CollectionAssert.AreEqual(new[] { name }, Opens().Skip(before.Length).ToArray(), "the missing symbol file is looked for once per assembly, and the answer kept");
        }
    }
}
