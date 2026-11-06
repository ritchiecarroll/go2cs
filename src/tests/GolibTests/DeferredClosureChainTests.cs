// DeferredClosureChainTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests
{
    /// <summary>
    /// A panic a deferred Go CLOSURE raises (B1, the converter-marked deferred closure; BOARD 2026-10-24, option B).
    /// Go prints the closure's frame, the gopanic that called it as <c>panic(...)</c>, then the deferring function:
    /// <c>main.main.func1</c>, <c>panic</c>, <c>main.main</c>. The closure has no deferring frame of its own, so the
    /// deferring function's <c>GoFrame.Run</c> catches its panic first, as it catches a panic a converter thunk raises,
    /// and Go elides the thunk. The converter's DEFERRED-CLOSURE record (GoPositionMapAttribute.Closures) is what tells
    /// the two apart, so runtime splices a recorded closure's chain and keeps refusing everything else.
    /// </summary>
    /// <remarks>
    /// The fixture is its own assembly, compiled here, so its position map and its symbol file are exactly what each arm
    /// says: the closure recorded by its lambda's first sequence point, not recorded, or recorded with no symbol file
    /// to read the position from. CrashReportTests.ARefusedChainPrintsExactlyWhatItAlwaysHas keeps pinning the
    /// unrecorded probe (an older artifact's output). RED before runtime's isRecordedDeferredClosure: the recorded arm
    /// printed the deferring function alone.
    /// </remarks>
    [TestClass]
    public class DeferredClosureChainTests
    {
        // func deferClosure() { defer func() { r := recover(); panic(r) }(); panic("first") }
        // func deferThunk() { defer raise(); panic("first") }, with raise a function that panics, deferred through the
        // converter's thunk shape (`defer(() => raise(), ref ᒐ)`).
        private const string Source = """
            [assembly: go.GoPositionMap("crashclosure/fixture.go", "crashclosure.cs", "", "", "", "", "", "CLOSURESITE")]

            namespace go
            {
                public static class crashclosure_package
                {
                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void deferClosure()
                    {
                        GoFrame ᒐ = default;
                        try {
                            builtin.defer(() => {
                                var r = builtin.recover();
                                throw builtin.panic(r);
                            }, ref ᒐ);
                            throw builtin.panic("first");
                        }
                        catch (System.Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    }

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void raise() => throw builtin.panic("second");

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void deferThunk()
                    {
                        GoFrame ᒐ = default;
                        try {
                            builtin.defer(() => raise(), ref ᒐ);
                            throw builtin.panic("first");
                        }
                        catch (System.Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    }

                    public static slice<uintptr> pcs;

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void capture()
                    {
                        slice<uintptr> taken = new(64);
                        pcs = taken[..(int)runtime_package.Callers(0, taken)];
                    }

                    // Review S09 AR, as panicframesprobe's panicAfterRecovery: a deferred closure panics after a recovery
                    // completed, and an older deferred closure captures runtime.Callers. Go: AR.func1 | gopanic | AR.func2 | AR.
                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    public static void callersAfterClosurePanic()
                    {
                        GoFrame ᒐ = default;
                        try {
                            builtin.defer(() => { builtin.recover(); capture(); }, ref ᒐ);
                            builtin.defer(() => { throw builtin.panic("p3"); }, ref ᒐ);
                            builtin.defer(() => { builtin.recover(); }, ref ᒐ);
                            throw builtin.panic("p1");
                        }
                        catch (System.Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    }
                }
            }
            """;

        // A deferred closure's first sequence point is its body's `{` in a Debug build and its first statement in a Release
        // build, so the converter lists both, as here, for every Go func literal handed straight to defer.
        private const string Opening = "builtin.defer(() => {";

        private enum Record { Closure, None }

        // The fixture's text with CLOSURESITE replaced by the sites the arm records: every deferred closure's `{` and first
        // statement, or nothing.
        private static string FixtureText(Record record)
        {
            string[] lines = Source.Replace("\r\n", "\n").Split('\n');
            List<string> sites = [];

            for (int index = 0; record == Record.Closure && index < lines.Length; index++)
            {
                int at = lines[index].IndexOf(Opening, StringComparison.Ordinal);

                if (at < 0)
                    continue;

                int brace = at + Opening.Length;
                sites.Add($"{index + 1}:{brace}");

                // The first statement: on the brace's own line when the body is, else the next line's first character.
                string rest = lines[index][brace..];
                int statementLine = rest.Trim().Length > 0 ? index : index + 1;
                int statementColumn = statementLine == index
                    ? brace + rest.Length - rest.TrimStart().Length + 1
                    : lines[statementLine].Length - lines[statementLine].TrimStart().Length + 1;

                sites.Add($"{statementLine + 1}:{statementColumn}");
            }

            return string.Join("\n", lines).Replace("CLOSURESITE", string.Join(",", sites));
        }

        // The function names of a runtime.Callers capture, as CallersFrames reads them.
        private static List<string> NamesOf(slice<uintptr> pcs)
        {
            List<string> names = [];
            var iterator = runtime_package.CallersFrames(pcs);

            while (true)
            {
                var (frame, more) = iterator.Next();

                if (frame.PC == 0 && !more)
                    break;

                names.Add((string)frame.Function);

                if (!more)
                    break;
            }

            return names;
        }

        // Compiles the fixture under a unique assembly name into a fresh directory, with or without its symbol file, in a
        // Debug or Release build, and loads it from disk, so the runtime finds (or misses) the PDB beside it.
        private static Type LoadFixture(Record record, bool withPdb, OptimizationLevel optimization = OptimizationLevel.Debug)
        {
            string name = "crashclosure_" + Guid.NewGuid().ToString("N");
            string directory = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(directory);

            string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            MetadataReference[] references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(GoFrame).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(runtime_package).Assembly.Location),
            ];

            SyntaxTree tree = CSharpSyntaxTree.ParseText(SourceText.From(FixtureText(record), Encoding.UTF8), path: Path.Combine(directory, "crashclosure.cs"));
            CSharpCompilation compilation = CSharpCompilation.Create(name, [tree], references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable, optimizationLevel: optimization));

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

            return Assembly.LoadFrom(dll).GetType("go.crashclosure_package")!;
        }

        // Runs the fixture's method, which must end in an escaping panic, and returns the function names of runtime's crash
        // traceback for it (Go's `name()` lines; the tab-indented position lines are dropped).
        private static string[] CrashFramesOf(Type fixture, string method)
        {
            Exception? escaped = null;

            try
            {
                fixture.GetMethod(method)!.Invoke(null, null);
            }
            catch (TargetInvocationException ex)
            {
                escaped = ex.InnerException;
            }

            Assert.IsNotNull(escaped, "the probe's panic must escape");
            Assert.IsTrue(CrashReport.TryUnwrapPanic(escaped, out PanicException panic, out Exception thrown), "the escape must be a panic");

            string traceback = runtime_package.GoCrashTraceback(panic, thrown);
            string[] lines = traceback.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual("goroutine 1 [running]:", lines[0], traceback);

            return Array.ConvertAll(Array.FindAll(lines[1..], line => !line.StartsWith('\t')), line => line[..line.IndexOf('(')]);
        }

        [TestMethod]
        [DataRow(OptimizationLevel.Debug, DisplayName = "Debug: the first sequence point is the body's {")]
        [DataRow(OptimizationLevel.Release, DisplayName = "Release: the first sequence point is the body's first statement")]
        public void ARecordedDeferredClosuresPanicPrintsTheClosureThenPanicThenTheDeferringFunction(OptimizationLevel optimization)
        {
            string[] frames = CrashFramesOf(LoadFixture(Record.Closure, withPdb: true, optimization), "deferClosure");

            Assert.AreEqual(3, frames.Length, "expected Go's chain, closure | panic | deferring function -- read: " + string.Join(" | ", frames));
            StringAssert.StartsWith(frames[0], "crashclosure.deferClosure.func", "the closure's frame, as Go names a func literal -- read: " + string.Join(" | ", frames));
            Assert.AreEqual("panic", frames[1], string.Join(" | ", frames));
            Assert.AreEqual("crashclosure.deferClosure", frames[2], string.Join(" | ", frames));
        }

        // An older artifact: the same closure with no record keeps exactly what it always printed.
        [TestMethod]
        public void AnUnrecordedDeferredClosureKeepsTheRefusal()
        {
            CollectionAssert.AreEqual(new[] { "crashclosure.deferClosure" }, CrashFramesOf(LoadFixture(Record.None, withPdb: true), "deferClosure"));
        }

        // No symbol file, no position to match: the record cannot be read, and the chain keeps its refusal.
        [TestMethod]
        public void WithNoSymbolFileARecordedDeferredClosureKeepsTheRefusal()
        {
            CollectionAssert.AreEqual(new[] { "crashclosure.deferClosure" }, CrashFramesOf(LoadFixture(Record.Closure, withPdb: false), "deferClosure"));
        }

        // A converter thunk is never spliced as a closure: with the closure recorded, the thunk's panic keeps the refusal.
        [TestMethod]
        public void AThunksPanicIsStillRefusedBesideARecordedClosure()
        {
            CollectionAssert.AreEqual(new[] { "crashclosure.deferThunk" }, CrashFramesOf(LoadFixture(Record.Closure, withPdb: true), "deferThunk"));
        }

        // runtime.Callers reads the same rule: below the capturing closure (Callers, capture, func1), Go's list is gopanic,
        // the raising closure, then the deferring function. Unrecorded, the closure's site splices nothing: the owner alone.
        [TestMethod]
        [DataRow(true, DisplayName = "recorded: gopanic | the raising closure | the deferring function")]
        [DataRow(false, DisplayName = "unrecorded: the deferring function alone")]
        public void CallersBelowADeferredCallSpliceARecordedClosuresPanic(bool recorded)
        {
            Type fixture = LoadFixture(recorded ? Record.Closure : Record.None, withPdb: true);
            fixture.GetMethod("callersAfterClosurePanic")!.Invoke(null, null);

            List<string> names = NamesOf((slice<uintptr>)fixture.GetField("pcs")!.GetValue(null)!);
            string read = string.Join(" | ", names);

            Assert.IsTrue(names.Count > 3, read);
            Assert.AreEqual("runtime.Callers", names[0], read);
            Assert.AreEqual("crashclosure.capture", names[1], read);

            int owner = names.IndexOf("crashclosure.callersAfterClosurePanic", 3);
            Assert.IsTrue(owner >= 3, "the owner is missing: " + read);

            List<string> below = names.GetRange(3, owner - 3 + 1);

            if (!recorded)
            {
                CollectionAssert.AreEqual(new[] { "crashclosure.callersAfterClosurePanic" }, below, read);
                return;
            }

            Assert.AreEqual(3, below.Count, "expected gopanic | closure | owner -- read: " + read);
            Assert.AreEqual("runtime.gopanic", below[0], read);
            StringAssert.StartsWith(below[1], "crashclosure.callersAfterClosurePanic.func", read);
            Assert.AreEqual("crashclosure.callersAfterClosurePanic", below[2], read);
        }
    }
}
