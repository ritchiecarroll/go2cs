using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = go.cgotestꓸC_package;

namespace GolibTests;

// cgo Phase 3, .5 cgo items 1(a) and 1(b) (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.12-18.12.3): the routes by which
// C hands a pointer BACK to Go, run against the probe library (cgo-probe/probe.c) through the generated golden.
//
// 1(a): a pointer member C writes into a mirrored struct, or returns inside a struct result, is checked against the Go
// memory the call pinned. Go's answer there is a pointer into that Go memory, which go2cs cannot represent, so the result
// is refused by name; the near-miss is the same call over C memory, which crosses as a native pointer.
// 1(b): a word C writes back that is the address of a stack copy go2cs made of a Go argument maps back to that argument's
// box, which is Go's answer (out == &a, node->next == node). An address INSIDE a copy that is not the copy itself has no
// Go-side box to map to and is refused by name.
//
// Before the cure every arm below copied the word back as a native pointer at the address C wrote: into the pinned Go
// array (a native alias of Go memory) or into the wrapper's own stack frame (dangling the moment the wrapper returned).
[TestClass]
[DoNotParallelize]
public unsafe class CgoReturnRoutesTests
{
    private static T OnAGoroutine<T>(Func<T> call) => CgoScalarWrapperTests.OnAGoroutine(call);

    private static string RefusalOf(Action call)
    {
        string? message = OnAGoroutine(() =>
        {
            try
            {
                call();
            }
            catch (PanicException refused)
            {
                return refused.Message;
            }

            return null;
        });

        Assert.IsNotNull(message, "the call was not refused");
        return message;
    }

    // 1(a), parameter side: cursor->cur = buffer + 3 over a pinned Go array. Go's answer is &buffer[3]; refused by name.
    [TestMethod]
    public void AMemberCWritesIntoTheGoMemoryItWasHandedIsRefused()
    {
        CgoProbe.Require();

        array<C.@char> buffer = new(8);
        ж<C.struct_go2cs_probe_cursor> cursor = new StandardBox<C.struct_go2cs_probe_cursor>(new C.struct_go2cs_probe_cursor());

        string message = RefusalOf(() => C.go2cs_probe_cursor_set(cursor, new ElemRefBox<C.@char>(buffer, 0), 3));

        StringAssert.Contains(message, "C.go2cs_probe_cursor_set");
        StringAssert.Contains(message, "returned an address inside the Go memory it was handed");
    }

    // The near-miss: the same call over C memory. The member crosses back as a native pointer at buffer + 3.
    [TestMethod]
    public void AMemberCWritesIntoCMemoryCrossesBackAsANativePointer()
    {
        CgoProbe.Require();

        byte* memory = (byte*)NativeMemory.AllocZeroed(8);

        try
        {
            ж<C.struct_go2cs_probe_cursor> cursor = new StandardBox<C.struct_go2cs_probe_cursor>(new C.struct_go2cs_probe_cursor());
            int32 k = OnAGoroutine(() => (int32)C.go2cs_probe_cursor_set(cursor, new NativeBox<C.@char>((nuint)memory), 3));

            Assert.AreEqual(3, k);
            Assert.AreEqual(3, (int32)cursor.Value.n, "C wrote n");
            Assert.AreEqual((nuint)memory + 3, ((NativeBox<C.@char>)cursor.Value.cur).NativeAddress, "and cur, a C pointer");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // 1(a), result side: the same cursor returned BY VALUE, its member pointing into the pinned Go array.
    [TestMethod]
    public void AResultMemberPointingIntoTheGoMemoryItWasHandedIsRefused()
    {
        CgoProbe.Require();

        array<C.@char> buffer = new(8);

        string message = RefusalOf(() => C.go2cs_probe_cursor_at(new ElemRefBox<C.@char>(buffer, 0), 5));

        StringAssert.Contains(message, "C.go2cs_probe_cursor_at");
        StringAssert.Contains(message, "returned an address inside the Go memory it was handed");
    }

    [TestMethod]
    public void AResultMemberPointingIntoCMemoryCrossesBackAsANativePointer()
    {
        CgoProbe.Require();

        byte* memory = (byte*)NativeMemory.AllocZeroed(8);

        try
        {
            C.struct_go2cs_probe_cursor cursor = OnAGoroutine(() => C.go2cs_probe_cursor_at(new NativeBox<C.@char>((nuint)memory), 5));

            Assert.AreEqual(5, (int32)cursor.n);
            Assert.AreEqual((nuint)memory + 5, ((NativeBox<C.@char>)cursor.cur).NativeAddress);
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // 1(b): *out = a, where a crossed as a stack copy. Go's answer is out == &a: the out-slot holds a's own box.
    [TestMethod]
    public void AnOutWordThatIsTheArgumentsStackCopyIsTheArgumentItself()
    {
        CgoProbe.Require();

        ж<C.@int> a = new StandardBox<C.@int>(41);
        ж<ж<C.@int>> slot = new StandardBox<ж<C.@int>>((ж<C.@int>)null!);

        int32 r = OnAGoroutine(() => (int32)C.go2cs_probe_pick(a, slot));

        Assert.AreEqual(41, r);
        Assert.IsTrue(ReferenceEquals(a, slot.Value), $"out == &a: the slot holds a's own box, not {slot.Value?.GetType().Name ?? "nil"}");

        slot.Value.Value = 42;
        Assert.AreEqual(42, (int32)a.Value, "a write through out is a write to a");
    }

    // The near-miss: a in C memory crosses by its own address, so *out is that native pointer.
    [TestMethod]
    public void AnOutWordThatIsACPointerCrossesBackAsThatPointer()
    {
        CgoProbe.Require();

        int* memory = (int*)NativeMemory.Alloc(sizeof(int));
        *memory = 7;

        try
        {
            ж<ж<C.@int>> slot = new StandardBox<ж<C.@int>>((ж<C.@int>)null!);
            int32 r = OnAGoroutine(() => (int32)C.go2cs_probe_pick(new NativeBox<C.@int>((nuint)memory), slot));

            Assert.AreEqual(7, r);
            Assert.AreEqual((nuint)memory, ((NativeBox<C.@int>)slot.Value).NativeAddress);
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // H's near-miss for 1(b): C stores a's stack-copy address through another pointee type (char * from int *). The
    // address is the copy's own, but no box of a's names a char there, so it is refused rather than mapped.
    [TestMethod]
    public void AnOutWordAtAnArgumentsStackCopyThroughAnotherPointeeIsRefused()
    {
        CgoProbe.Require();

        ж<C.@int> a = new StandardBox<C.@int>(41);
        ж<ж<C.@char>> slot = new StandardBox<ж<C.@char>>((ж<C.@char>)null!);

        string message = RefusalOf(() => C.go2cs_probe_pick_as_char(a, slot));

        StringAssert.Contains(message, "C.go2cs_probe_pick_as_char");
        StringAssert.Contains(message, "stack copy");
    }

    // 1(b), interior: *out = &pair->b, inside the pair's stack copy but not the copy itself. No Go-side box names it.
    [TestMethod]
    public void AnOutWordInsideAnArgumentsStackCopyIsRefused()
    {
        CgoProbe.Require();

        ж<C.struct_go2cs_probe_pair> pair = new StandardBox<C.struct_go2cs_probe_pair>(new C.struct_go2cs_probe_pair { a = 1, b = 2 });
        ж<ж<C.@int>> slot = new StandardBox<ж<C.@int>>((ж<C.@int>)null!);

        string message = RefusalOf(() => C.go2cs_probe_pick_b(pair, slot));

        StringAssert.Contains(message, "C.go2cs_probe_pick_b");
        StringAssert.Contains(message, "stack copy");
    }

    // A member that is its struct's own address: node->next = node over the node's stack copy. Go's answer is
    // node.next == node, the node's own box.
    [TestMethod]
    public void AMemberThatIsItsStructsOwnStackCopyIsTheStructItself()
    {
        CgoProbe.Require();

        ж<C.struct_go2cs_probe_node> node = new StandardBox<C.struct_go2cs_probe_node>(new C.struct_go2cs_probe_node { value = 9 });

        OnAGoroutine(() => C.go2cs_probe_node_ring(node));

        Assert.AreEqual(9, (int32)node.Value.value);
        Assert.IsTrue(ReferenceEquals(node, node.Value.next), $"node->next == node: the member holds the node's own box, not {node.Value.next?.GetType().Name ?? "nil"}");
    }
}
