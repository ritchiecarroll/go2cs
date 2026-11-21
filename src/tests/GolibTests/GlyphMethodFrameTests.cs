// GlyphMethodFrameTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// A suffix-form method (owner ruling 2026-11-21) is named as the Go method it is: its body <c>&lt;Name&gt;⁔</c> in the
/// package class (GlyphProbe.cs) is the method's frame, named with its receiver and without the suffix, and the
/// Go-named forwarder in its holder has no frame. U+2054 is connector punctuation no Go identifier can spell, so the
/// trailing suffix alone marks a body. Read by golib's <see cref="GoSyntheticPC.GoNameOf"/> and by the runtime's frame
/// naming (managed_impl.cs: goFunctionName, goReceiverName, goRecvForwardee).
/// </summary>
/// <remarks>
/// Failing-first record: before the runtime read the suffix, a body's frame was <c>glyphprobe.Walk⁔</c>, with no
/// receiver, and a pointer-receiver twin's function value resolved to the hidden forwarder and lost its receiver.
/// </remarks>
[TestClass]
public class GlyphMethodFrameTests
{
    private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static List<string> FunctionsOf(slice<uintptr> pcs)
    {
        List<string> functions = [];
        var frames = CallersFrames(pcs);

        while (true)
        {
            var (frame, more) = frames.Next();
            functions.Add((string)frame.Function);

            if (!more)
                break;
        }

        return functions;
    }

    [TestMethod]
    public void GolibNamesABodyByItsGoName()
    {
        MethodInfo body = typeof(glyphprobe_package).GetMethod("Walk⁔", AnyStatic)!;

        Assert.IsNotNull(body, "the fixture's body");
        Assert.AreEqual("glyphprobe.Walk", GoSyntheticPC.GoNameOf(body));
    }

    [TestMethod]
    public void FuncForPCNamesAValueReceiverBodyAsItsMethod()
    {
        // A method expression of a value receiver binds the body directly (convSelectorExpr's MethodExpr arm).
        uintptr pc = reflect_package.ValueOf((Func<glyphprobe_package.counter, slice<uintptr>>)glyphprobe_package.Walk⁔).Pointer();

        Assert.AreEqual("glyphprobe.counter.Walk", (string)FuncForPC(pc).Name());
    }

    [TestMethod]
    public void CallersNamesTheBodyFrameAndNeverTheForwarder()
    {
        List<string> functions = FunctionsOf(glyphprobe_package.walkVia(new glyphprobe_package.counter()));
        string all = string.Join(" | ", functions);

        Assert.IsTrue(functions.Count >= 4, all);
        Assert.AreEqual("glyphprobe.callersHere", functions[1], all);
        Assert.AreEqual("glyphprobe.counter.Walk", functions[2], all);
        Assert.AreEqual("glyphprobe.walkVia", functions[3], all);
        Assert.IsFalse(all.Contains('⁔'), all);
    }

    [TestMethod]
    public void CallersNamesAPointerReceiverBodyWithItsPointerReceiver()
    {
        ж<glyphprobe_package.counter> box = Ꮡ(new glyphprobe_package.counter());
        List<string> functions = FunctionsOf(glyphprobe_package.bumpVia(box));
        string all = string.Join(" | ", functions);

        Assert.IsTrue(functions.Count >= 4, all);
        Assert.AreEqual("glyphprobe.(*counter).Bump", functions[2], all);
        Assert.AreEqual("glyphprobe.bumpVia", functions[3], all);
        Assert.IsFalse(all.Contains('⁔'), all);
        Assert.AreEqual((nint)1, box.Value.n, "the body ran on the boxed value");
    }

    [TestMethod]
    public void APanicTraceNamesTheBodyAsItsMethod()
    {
        Exception escaped = null;

        try
        {
            glyphprobe_package.failVia(new glyphprobe_package.counter());
        }
        catch (Exception ex)
        {
            escaped = ex;
        }

        Assert.IsNotNull(escaped, "the probe's panic must escape");
        Assert.IsTrue(CrashReport.TryUnwrapPanic(escaped, out PanicException panic, out Exception thrown), "the escape must be a panic");

        string traceback = GoCrashTraceback(panic, thrown);
        string[] lines = traceback.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string[] frames = Array.ConvertAll(Array.FindAll(lines[1..], line => !line.StartsWith('\t')), line => line[..line.IndexOf('(')]);
        string all = string.Join(" | ", frames);

        Assert.AreEqual("glyphprobe.counter.Fail", frames[0], traceback);
        CollectionAssert.Contains(frames, "glyphprobe.failVia", all);
        Assert.IsFalse(traceback.Contains('⁔'), traceback);
    }

    [TestMethod]
    public void APointerReceiverTwinsFunctionValueIsNamedAsTheMethod()
    {
        // RecvGenerator's `this ж<T>` twin forwards to the hidden `this ref T` forwarder, which forwards to the body: the
        // function is the Go method, named with its pointer receiver.
        uintptr pc = reflect_package.ValueOf((Func<ж<glyphprobe_package.counter>, slice<uintptr>>)glyphprobeᴛcounter.Bump).Pointer();

        Assert.AreEqual("glyphprobe.(*counter).Bump", (string)FuncForPC(pc).Name());
    }

    [TestMethod]
    public void AGenericPointerReceiverTwinFindsItsBodyByShape()
    {
        // The twin's forwardee and the body each declare their own T: the body is matched by the shape of its
        // parameters, not by type identity.
        uintptr pc = reflect_package.ValueOf((Func<ж<glyphprobe_package.cell<nint>>, nint>)glyphprobeᴛcell.Get<nint>).Pointer();

        Assert.AreEqual("glyphprobe.(*cell[...]).Get", (string)FuncForPC(pc).Name());
    }

    [TestMethod]
    public void AMethodValueThroughATwinKeepsItsName()
    {
        // Control: a method value's `-fm` name was already read through the holder, and stays.
        ж<glyphprobe_package.counter> box = Ꮡ(new glyphprobe_package.counter());
        Func<slice<uintptr>> value = box.Bump;
        uintptr pc = reflect_package.ValueOf(value).Pointer();

        Assert.AreEqual("glyphprobe.(*counter).Bump-fm", (string)FuncForPC(pc).Name());
    }
}
