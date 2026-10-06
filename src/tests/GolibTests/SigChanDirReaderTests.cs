using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// GoReflect.MethodSigChanDirs / FuncChanDirs (NEW-1b, the generator-side route): the [GoSigChanDir]
// go2cs-gen emits beside a method's declaring type is read back by method name and parameter types, a
// `ref` parameter by its element type, through a delegate rebound over another delegate's Invoke, and an
// entry that cannot describe the signature it names is refused BY NAME rather than applied.
[TestClass]
public class SigChanDirReaderTests
{
    public struct Receiver { public int n; }

    public delegate void NamedRecv(channel<nint> c);

    [GoSigChanDir("Recv", new[] { typeof(channel<nint>) }, new[] { GoChanDir.Recv }, new GoChanDir[0])]
    [GoSigChanDir("Results", new Type[0], new GoChanDir[0], new[] { GoChanDir.Send, GoChanDir.Unstamped })]
    [GoSigChanDir("ByRef", new[] { typeof(Receiver), typeof(channel<nint>) }, new[] { GoChanDir.Unstamped, GoChanDir.Send }, new GoChanDir[0])]
    [GoSigChanDir("NotAChannel", new[] { typeof(nint) }, new[] { GoChanDir.Recv }, new GoChanDir[0])]
    [GoSigChanDir("WrongCount", new[] { typeof(channel<nint>) }, new[] { GoChanDir.Recv }, new[] { GoChanDir.Recv })]
    [GoSigChanDir("Bidirectional", new[] { typeof(channel<nint>) }, new[] { GoChanDir.Unstamped }, new GoChanDir[0])]
    public static class Holder
    {
        public static void Recv(channel<nint> c) { }
        public static (channel<nint>, error) Results() => default;
        public static void ByRef(ref Receiver r, channel<nint> c) { }
        public static void NotAChannel(nint x) { }
        public static void WrongCount(channel<nint> c) { }
        public static void Bidirectional(channel<nint> c) { }
        public static void Unlisted(channel<nint> c) { }
    }

    private static MethodInfo M(string name) => typeof(Holder).GetMethod(name)!;

    [TestMethod]
    public void ReadsAParameterDirectionByMethod()
    {
        (GoChanDir[]? ins, GoChanDir[]? outs) = GoReflect.MethodSigChanDirs(M("Recv"));
        CollectionAssert.AreEqual(new[] { GoChanDir.Recv }, ins);
        Assert.IsNull(outs, "no result is stamped, so the result vector is absent");
    }

    [TestMethod]
    public void ReadsMultiValueResultDirections()
    {
        (GoChanDir[]? ins, GoChanDir[]? outs) = GoReflect.MethodSigChanDirs(M("Results"));
        Assert.IsNull(ins);
        CollectionAssert.AreEqual(new[] { GoChanDir.Send, GoChanDir.Unstamped }, outs);
    }

    [TestMethod]
    public void MatchesARefParameterByItsElementType()
    {
        (GoChanDir[]? ins, _) = GoReflect.MethodSigChanDirs(M("ByRef"));
        CollectionAssert.AreEqual(new[] { GoChanDir.Unstamped, GoChanDir.Send }, ins);
    }

    [TestMethod]
    public void AnUnlistedOrBidirectionalMethodCarriesNothing()
    {
        Assert.AreEqual((null, null), GoReflect.MethodSigChanDirs(M("Unlisted")));
        Assert.AreEqual((null, null), GoReflect.MethodSigChanDirs(M("Bidirectional")));
    }

    [TestMethod]
    public void AFuncValueReadsItsTargetMethod()
    {
        Action<channel<nint>> value = Holder.Recv;
        CollectionAssert.AreEqual(new[] { GoChanDir.Recv }, GoReflect.FuncChanDirs(value).ins);
    }

    [TestMethod]
    public void AReboundDelegateIsChasedToTheMethodItWraps()
    {
        Action<channel<nint>> inner = Holder.Recv;
        NamedRecv rebound = (NamedRecv)Delegate.CreateDelegate(typeof(NamedRecv), inner, inner.GetType().GetMethod("Invoke")!);
        CollectionAssert.AreEqual(new[] { GoChanDir.Recv }, GoReflect.FuncChanDirs(rebound).ins,
            "a delegate rebound over another delegate's Invoke targets Invoke, which carries nothing");
    }

    [TestMethod]
    public void AStampOnANonChannelIsRefusedByName()
    {
        InvalidOperationException refused = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.MethodSigChanDirs(M("NotAChannel")));
        StringAssert.Contains(refused.Message, "NotAChannel");
        StringAssert.Contains(refused.Message, "is not a channel");
    }

    [TestMethod]
    public void AStampWhoseCountDisagreesIsRefusedByName()
    {
        InvalidOperationException refused = Assert.ThrowsException<InvalidOperationException>(() => GoReflect.MethodSigChanDirs(M("WrongCount")));
        StringAssert.Contains(refused.Message, "WrongCount");
        StringAssert.Contains(refused.Message, "0 results");
    }
}
