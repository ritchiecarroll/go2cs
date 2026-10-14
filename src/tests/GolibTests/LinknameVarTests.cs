using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The late-bound //go:linkname var pull (golib LinknameVar; the converter emits it for a pull whose
// direct reference would close a project cycle -- math/bits pulling runtime.overflowError at go1.26.9).
// Its three promises: the value is the remote's OWN object, read through the remote's lazy static
// initializer exactly as a direct forwarding property would; a write reaches the remote's storage; and a
// pull it cannot resolve PANICS naming the target, never returns a silent null -- the null field is the
// bug the late-bound pull replaces.
public static class LinknameVarRemote
{
    public static int Initializations;

    // A field initializer with a side effect: the CLR runs it lazily, at first access to the class.
    public static object Value = initialize();

    private static object initialize()
    {
        Interlocked.Increment(ref Initializations);
        return new object();
    }
}

[TestClass]
public class LinknameVarTests
{
    private static readonly string s_remote = typeof(LinknameVarRemote).AssemblyQualifiedName!;

    [TestMethod]
    public void ReadsTheRemotesOwnObjectThroughItsStaticInitializer()
    {
        object pulled = LinknameVar<object>.Get(s_remote, nameof(LinknameVarRemote.Value));

        Assert.AreEqual(1, LinknameVarRemote.Initializations, "the remote's static initializer must have run exactly once");
        Assert.AreSame(LinknameVarRemote.Value, pulled, "the pull must be the remote's own object, Go's one storage");
    }

    [TestMethod]
    public void AWriteReachesTheRemoteStorage()
    {
        object previous = LinknameVar<object>.Get(s_remote, nameof(LinknameVarRemote.Value));
        object written = new();

        try
        {
            LinknameVar<object>.Set(s_remote, nameof(LinknameVarRemote.Value), written);
            Assert.AreSame(written, LinknameVarRemote.Value);
            Assert.AreSame(written, LinknameVar<object>.Get(s_remote, nameof(LinknameVarRemote.Value)));
        }
        finally
        {
            LinknameVarRemote.Value = previous;
        }
    }

    [TestMethod]
    public void AMissingTargetPanicsNamingIt()
    {
        const string absent = "go.no_such_package, no.such.assembly";

        PanicException panic = Assert.ThrowsException<PanicException>(() => LinknameVar<object>.Get(absent, "overflowError"));
        StringAssert.Contains(panic.Message, absent);
    }

    [TestMethod]
    public void AMissingFieldPanicsNamingIt()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => LinknameVar<object>.Get(s_remote, "NoSuchField"));
        StringAssert.Contains(panic.Message, "NoSuchField");
    }
}
