// FieldMetadataGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A type whose field metadata was trimmed does not fail reflection: GetFields answers "no fields".
// Three golib facts derived from a struct's fields turned that answer into SILENT misbehaviour:
//
//   GoZeroSizeFacts.Classify   -- no fields means every one is zero-size, so the struct is: a slice of
//                                 it would share one element and skip its length ceilings.
//   GoLibcCall.DispatchArgsStruct -- no fields means no arguments: libc is called with none, and
//                                 nothing is written back.
//   SliceHeaderBox / HeaderSliceBox -- "not three fields" means "not a slice header", so the pair is
//                                 declined and the header goes down the address route.
//
// All three now read the fields through GoFieldMetadata.InstanceFields, which refuses by name a struct
// that occupies storage and reports none. Each arm withholds one probe type's fields the way trimming
// would (GoFieldMetadata.WithheldForTest) and requires the refusal. MSTest runs this assembly
// serially (no [Parallelize]), which is what makes setting that seam safe for the length of an arm;
// each arm has its OWN probe type, so no per-type static cached by an earlier arm can answer for it.
[TestClass]
public class FieldMetadataGuardTests
{
    private struct Empty;

    private struct ControlProbe { internal long a; internal int b; }

    private struct ZeroSizeProbe { internal long a; }

    private struct LibcArgsProbe { internal int fd, cmd; internal int ret, errno; }

    private struct SliceHeaderProbe { internal nint data, len, cap; }

    private struct HeaderSliceProbe { internal nint data, len, cap; }

    private static InvalidOperationException Refusal(Type withheld, Action act)
    {
        GoFieldMetadata.WithheldForTest = withheld;

        try
        {
            act();
        }
        catch (TypeInitializationException ex) when (ex.InnerException is InvalidOperationException inner)
        {
            return inner;
        }
        catch (InvalidOperationException ex)
        {
            return ex;
        }
        finally
        {
            GoFieldMetadata.WithheldForTest = null;
        }

        Assert.Fail($"{withheld.Name} occupies storage and reported no fields, and nothing refused it");
        return null!;
    }

    private static void AssertNamesTheType(InvalidOperationException refusal, Type withheld)
    {
        StringAssert.Contains(refusal.Message, withheld.Name, "the refusal must name the type whose metadata is gone");
        StringAssert.Contains(refusal.Message, "reports no instance fields");
    }

    [TestMethod]
    public void ControlAStructThatReallyHasNoFieldsIsNotRefused()
    {
        Assert.AreEqual(0, GoFieldMetadata.InstanceFields(typeof(Empty)).Length, "a genuinely field-less struct is one byte and answers no fields");
        Assert.IsTrue(GoZeroSizeFacts.Classify(typeof(Empty)), "and it is zero-size in Go");
    }

    [TestMethod]
    public void ControlAStructWithFieldsAnswersThem()
    {
        Assert.AreEqual(2, GoFieldMetadata.InstanceFields(typeof(ControlProbe)).Length);
        Assert.IsFalse(GoZeroSizeFacts.Classify(typeof(ControlProbe)));
    }

    [TestMethod]
    public void ZeroSizeClassificationRefusesAStructWhoseFieldsAreGone()
    {
        // Unguarded, the walk over no fields falls through to `return true`: zero-size.
        InvalidOperationException refusal = Refusal(typeof(ZeroSizeProbe), () => GoZeroSizeFacts.Classify(typeof(ZeroSizeProbe)));

        AssertNamesTheType(refusal, typeof(ZeroSizeProbe));
    }

    [TestMethod]
    public void LibcDispatchRefusesAnArgsStructWhoseFieldsAreGone()
    {
        ref LibcArgsProbe args = ref heap<LibcArgsProbe>(out ж<LibcArgsProbe> Ꮡargs);
        args = new LibcArgsProbe { fd = 1, cmd = 3 };

        // The function pointer is never reached: the refusal comes from reading the fields. Unguarded,
        // the dispatch goes on with zero arguments and fails later for a different reason (a null
        // function pointer here; a real call with no arguments in a program).
        InvalidOperationException refusal = Refusal(typeof(LibcArgsProbe), () => GoLibcCall.DispatchArgsStruct(0, Ꮡargs, 0, "probe"));

        AssertNamesTheType(refusal, typeof(LibcArgsProbe));
    }

    [TestMethod]
    public void TheSliceToHeaderBoxRefusesAHeaderWhoseFieldsAreGone()
    {
        // Unguarded, `fields.Length != 3` answers Applies = false and the pair is silently declined.
        InvalidOperationException refusal = Refusal(typeof(SliceHeaderProbe), () => _ = SliceHeaderBox<slice<byte>, SliceHeaderProbe>.Applies);

        AssertNamesTheType(refusal, typeof(SliceHeaderProbe));
    }

    [TestMethod]
    public void TheHeaderToSliceBoxRefusesAHeaderWhoseFieldsAreGone()
    {
        InvalidOperationException refusal = Refusal(typeof(HeaderSliceProbe), () => _ = HeaderSliceBox<HeaderSliceProbe, slice<byte>>.Applies);

        AssertNamesTheType(refusal, typeof(HeaderSliceProbe));
    }
}
