using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// A Go-source fixture (namespace go, a *_package class) that captures program counters from INSIDE the
// converted `sort` package: sort.Slice calls back into less, so sort's own frames are on the stack at
// every capture, and nothing else in this test assembly ever symbolizes a frame of sort.dll. That makes
// sort's symbol file an untouched witness: whoever opens it, opened it because of this fixture.
namespace go
{
    internal static class lazycallers_package
    {
        internal static nint frames;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void capture(slice<uintptr> pcs)
        {
            slice<nint> data = new nint[] { 3, 1, 2 };

            sort_package.Slice(data, (i, j) =>
            {
                frames = runtime_package.Callers(0, pcs);
                return data[i] < data[j];
            });
        }

        // One call site, reached twice: Go gives one site one pc.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static uintptr site()
        {
            slice<uintptr> pc = new uintptr[1];
            runtime_package.Callers(1, pc);
            return pc[0];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static (uintptr, uintptr) twice()
        {
            uintptr first = 0, second = 0;

            for (int i = 0; i < 2; i++)
            {
                uintptr pc = site();

                if (i == 0)
                    first = pc;
                else
                    second = pc;
            }

            return (first, second);
        }
    }
}

namespace GolibTests
{
    // CAPTURE IS NOT SYMBOLIZATION. Go's runtime.Callers records program counters; the name, file and
    // line are found later, by CallersFrames or FuncForPC, for the pcs somebody asks about. The managed
    // runtime used to symbolize at capture -- a StackTrace built with file info makes the CLR open the PDB
    // of every assembly on the stack -- so log/slog, which captures a pc for every record, paid a symbol
    // read per assembly whether or not anything ever printed a source (measured on a busy disk as seconds
    // of waiting inside log/slog's tests).
    //
    // The guard is a COUNT, not a timing: the runtime's symbol-file door (symbolFileOpens) and an
    // exclusive-open probe on the witness file, which a reader of EITHER kind holds once it has opened it.
    [TestClass]
    public class LazyCallersTests
    {
        // The runtime's own door (runtime_package.symbolFileOpens). A runtime without the door answers
        // "none", and the witness probe below still speaks for the CLR's reader.
        private static string[] Opens() =>
            (string[]?)typeof(runtime_package).GetMethod("symbolFileOpens", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null) ?? [];

        private static string WitnessPdb => Path.Combine(AppContext.BaseDirectory, "sort.pdb");

        // Whether ANY reader in this process holds the witness open: the runtime's, or the CLR's own
        // (which a capture with file info would have used). An exclusive open succeeds only if nobody does.
        private static bool WitnessIsHeld()
        {
            try
            {
                using FileStream probe = new(WitnessPdb, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        [TestMethod]
        public void ACaptureOpensNoSymbolFileAndSymbolizingOpensTheAssembliesItNames()
        {
            Assert.IsTrue(File.Exists(WitnessPdb), "control: sort's symbol file must sit beside the test assembly, or the witness proves nothing");

            if (Opens().Contains("sort") || WitnessIsHeld())
                Assert.Inconclusive("another test already symbolized a frame of sort.dll in this process; the witness is spent");

            string[] before = Opens();
            slice<uintptr> pcs = new uintptr[32];

            for (int i = 0; i < 50; i++)
                lazycallers_package.capture(pcs);

            nint n = lazycallers_package.frames;
            Assert.IsTrue(n >= 3, $"control: the capture must hold the fixture's and sort's frames, got {n}");

            // CAPTURE: nothing opened, by anyone.
            CollectionAssert.AreEqual(before, Opens(), "runtime.Callers opened a symbol file; a capture records pcs and nothing else");
            Assert.IsFalse(WitnessIsHeld(), "runtime.Callers left sort's symbol file open: the stack was symbolized at capture");

            // SYMBOLIZING: exactly the assemblies the frames name.
            ж<runtime_package.Frames> frames = runtime_package.CallersFrames(pcs[..(int)n]);
            bool sawSort = false, more = true;
            System.Collections.Generic.HashSet<string> named = [];

            while (more)
            {
                (runtime_package.Frame frame, bool next) = frames.Value.Next();
                more = next;

                // The assembly a frame names: its Go package is the name's first segment, and the
                // fixture's own package lives in this test assembly.
                string function = (string)frame.Function;
                string package = function.Split('.')[0];
                named.Add(package == "lazycallers" ? "GolibTests" : package);

                if (package == "sort")
                {
                    sawSort = true;
                    Assert.IsTrue(frame.Line > 0, $"sort's frame {function} must resolve to a line once it is asked for");
                }
            }

            Assert.IsTrue(sawSort, "control: the symbolized stack must name a sort frame");

            string[] opened = Opens().Skip(before.Length).ToArray();

            CollectionAssert.Contains(opened, "sort", "symbolizing a sort frame must open sort's symbol file");
            CollectionAssert.IsSubsetOf(opened, named.ToArray(), $"symbolizing opened an assembly no frame names: opened [{string.Join(",", opened)}], named [{string.Join(",", named)}]");
            Assert.IsTrue(WitnessIsHeld(), "control: the probe must see the runtime's own reader holding the file it just opened");
        }

        // The caveat of a pc keyed by the REPORTED IL offset: one call site has one pc for one compilation
        // of its method. Under tiering a promotion recompiles the method and may report a different offset.
        [TestMethod]
        public void ACallSiteHasOnePcAcrossCalls()
        {
            (uintptr first, uintptr second) = lazycallers_package.twice();

            Assert.AreNotEqual((uintptr)0, first, "control: the site must yield a pc");
            Assert.AreEqual(first, second, "one call site reached twice must yield one pc");
        }
    }
}
