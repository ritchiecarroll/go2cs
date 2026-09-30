using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace go
{
    // A Go-source GENERIC function for the symbol-name guard: a method counts as a Go frame when its top-level
    // type is a `*_package` class in namespace go, so this prints as `symprobe.genericHere[...]`.
    internal static class symprobe_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static List<(string function, string symbol)> genericHere<T>()
        {
            slice<uintptr> pcs = new(32);
            pcs = pcs[..(int)Callers(0, pcs)];
            List<(string function, string symbol)> frames = [];
            var iterator = CallersFrames(pcs);
            while (true)
            {
                var (frame, more) = iterator.Next();
                if (frame.PC == 0 && !more)
                    break;
                frames.Add(((string)frame.Function, (string)GoFrameSymbolName(frame)));
                if (!more)
                    break;
            }
            return frames;
        }
    }
}

namespace GolibTests
{
    /// <summary>
    /// Guards the name runtime/pprof symbolizes a frame by (runtime_FrameSymbolName). Go's traceback prints a
    /// generic function as <c>fn[...]</c> (funcNameForPrint) while pprof reads the RAW symbol, so the two
    /// names differ for a generic frame; the CLR cannot recover Go's shape arguments, so the managed symbol
    /// name is the UNDECORATED one. Red while the symbol name is the print name: the decoration leaked into
    /// every generic heap-profile location (runtime/pprof's TestGenericsHashKeyInPprofBuilder and
    /// TestGenericsInlineLocations pins, TRAIN J 603f51490d).
    /// </summary>
    [TestClass]
    public class FrameSymbolNameTests
    {
        [TestMethod]
        public void AGenericFunctionsSymbolNameIsItsUndecoratedName()
        {
            List<(string function, string symbol)> frames = symprobe_package.genericHere<int>();
            (string function, string symbol) generic = frames.Find(f => f.function.StartsWith("symprobe.genericHere"));

            Assert.AreEqual("symprobe.genericHere[...]", generic.function, "the PRINT name keeps Go's [...] (funcNameForPrint)");
            Assert.AreEqual("symprobe.genericHere", generic.symbol, "the SYMBOL name pprof reads carries no print decoration");
        }

        [TestMethod]
        public void ANonGenericFramesSymbolNameIsItsFunctionName()
        {
            List<(string function, string symbol)> frames = symprobe_package.genericHere<int>();

            Assert.AreEqual("runtime.Callers", frames[0].function);
            Assert.AreEqual("runtime.Callers", frames[0].symbol, "a frame with no print decoration has one name");
        }
    }
}
