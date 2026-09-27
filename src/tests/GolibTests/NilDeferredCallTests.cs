using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Go's `var f func(); defer f()`: the defer statement registers the nil func, and the CALL at
/// function exit raises the nil-dereference runtime error (runtime's TestCallersDeferNilFuncPanic and
/// its loop form). GoFrame.Push used to IGNORE a null registration, so the function returned normally
/// and the deferred recover() read nil.
/// </summary>
[TestClass]
public class NilDeferredCallTests
{
    private delegate void FrameBody(ref GoFrame frame);

    private static PanicException? RunInFrame(FrameBody body)
    {
        try
        {
            GoFrame frame = default;

            try
            {
                body(ref frame);
            }
            catch (Exception ex) when (GoFrame.IsPanic(ex, out PanicException? p))
            {
                GoFrame.Capture(p);
            }
            finally
            {
                frame.Run();
            }
        }
        catch (PanicException escaped)
        {
            GoFuncRoot.CapturedPanicValue = null;
            return escaped;
        }

        return null;
    }

    [TestMethod]
    public void ANilDeferredFuncPanicsAtFunctionExitNotAtTheDeferStatement()
    {
        int state = 1;
        int stateWhenRecovered = 0;
        object? recovered = null;

        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            frame.Push(() =>
            {
                recovered = recover();
                stateWhenRecovered = state;
            });

            Action f = null!;
            defer(f, ref frame);
            state = 2;
        });

        Assert.IsNull(escaped, "the deferred recover() must stop the panic");
        Assert.IsNotNull(recovered, "a nil deferred func must panic when it is called");
        StringAssert.Contains(recovered.ToString(), "nil pointer dereference");
        Assert.AreEqual(2, stateWhenRecovered, "it must panic at function EXIT, not at the defer statement");
    }

    [TestMethod]
    public void AnUnrecoveredNilDeferredCallEscapesTheFrame()
    {
        PanicException? escaped = RunInFrame((ref GoFrame frame) =>
        {
            for (int i = 0; i < 1; i++)
            {
                Action f = null!;
                defer(f, ref frame);
            }
        });

        Assert.IsNotNull(escaped, "a nil deferred func with no recover must panic out of the frame");
        StringAssert.Contains(escaped.Message, "nil pointer dereference");
    }
}
