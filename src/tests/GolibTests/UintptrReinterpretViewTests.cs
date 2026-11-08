// UintptrReinterpretViewTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A pointer reflect handed out as a number, converted back as ANOTHER layout-identical type, answers the view the
/// direct conversion builds: gojq's <c>*(*int)(unsafe.Pointer(v.FieldByName("op").UnsafeAddr()))</c> over
/// <c>op opcode</c> (BOARD 2026-11-07, gojq class E).
/// </summary>
/// <remarks>
/// <para>
/// reflect's <c>UnsafeAddr</c> registers the field box's ORDER TOKEN (<c>reflectPointerToken</c>), and these arms
/// register it the same way. Before the reinterpret step in <c>explicit operator ж&lt;T&gt;(uintptr)</c>, a
/// reference-free field read as another type fell through to a native box over the token and panicked at its first
/// dereference, while <c>Reinterpret</c> of the same pointer aliased the field.
/// </para>
/// <para>
/// <b>Registration order, measured.</b> A root and its FIRST field share one token, and the table holds whichever box
/// registered last. Field last: the step answers the field's view. Root last, with a reference-free root: the root's
/// prefix read as the type, the same storage. Root last, with a reference-bearing root: the direct form refuses that
/// pair (<c>ReinterpretAliasing.HoldsForRoot</c>), so the answer is today's carrier and its dereference refusal. Only a
/// FIRST field shares its root's token; a later field's token is its own.
/// </para>
/// </remarks>
[TestClass]
public class UintptrReinterpretViewTests
{
    // `type opcode int` in the shape go2cs-gen emits for a defined basic type: one m_value field and the conversions.
    [GoType("num:int")]
    public readonly struct Opcode(nint value)
    {
        private readonly nint m_value = value;

        public nint V => m_value;

        public static implicit operator Opcode(nint value) => new(value);
    }

    [GoType]
    public struct Plain
    {
        public Opcode A;
        public nint B;
    }

    // gojq's record: an interface beside the opcode, so the root is reference-bearing and `op` is not first.
    [GoType]
    public struct Record
    {
        public object? Value;
        public Opcode Op;
    }

    [GoType]
    public struct OpFirst
    {
        public Opcode Op;
        public object? Value;
    }

    // Reference-free, so it can live in native memory; the opcode is NOT first.
    [GoType]
    public struct NativeWide
    {
        public nint A;
        public Opcode Op;
    }

    // reflect's own field box, the one `v.FieldByName(name).Addr()` answers: GoReflect.FieldAliasBox.
    private static ж<Opcode> field<S>(ж<S> box, string name) =>
        (ж<Opcode>)GoReflect.FieldAliasBox(box, Array.Find(GoReflect.GoFields(typeof(S)), f => f.Name == name));

    // reflectPointerToken's registration: the box's order token, remembered against the box.
    private static nuint register(object box)
    {
        nuint token = ManagedPointerTokens.CurrentToken(box);
        ManagedPointerTokens.Register(token, box);
        return token;
    }

    [TestMethod]
    public void AFieldTokenReadAsALayoutIdenticalTypeAliasesTheField()
    {
        builtin.heap(new Record { Value = 7, Op = (Opcode)3 }, out ж<Record> root);
        ж<Opcode> op = field(root, "Op");
        nuint token = register(op);

        ж<nint> asInt = (ж<nint>)(uintptr)token;

        Assert.IsFalse(asInt.IsNative, "a view over the field, never a native box over the token");
        Assert.AreEqual(token, ManagedPointerTokens.CurrentToken(asInt), "one Go address, one number: the view carries the field's token");
        Assert.AreEqual((nint)3, asInt.Value, "it reads the field");

        asInt.Value = 42;
        Assert.AreEqual((nint)42, root.Value.Op.V, "and a write through it lands in the record");
        GC.KeepAlive(op);
    }

    [TestMethod]
    public void TheDirectFormAndTheNumberAgree()
    {
        builtin.heap(new Record { Op = (Opcode)5 }, out ж<Record> root);
        ж<Opcode> op = field(root, "Op");
        nuint token = register(op);

        ж<nint> direct = op.Reinterpret<Opcode, nint>();
        ж<nint> viaNumber = (ж<nint>)(uintptr)token;

        Assert.AreEqual(direct, viaNumber, "(*int)(unsafe.Pointer(&r.op)) and the number it converts to are one pointer");
        GC.KeepAlive(op);
    }

    [TestMethod]
    public void ARootAndItsFirstFieldShareOneToken_TheFieldRegisteredLastAnswersTheField()
    {
        builtin.heap(new Plain { A = (Opcode)6, B = 9 }, out ж<Plain> root);
        ж<Opcode> a = field(root, "A");

        Assert.AreEqual(ManagedPointerTokens.CurrentToken(root), ManagedPointerTokens.CurrentToken(a), "the premise: one token for the root and its first field");

        register(root);
        nuint token = register(a);

        Assert.AreEqual((nint)6, ((ж<nint>)(uintptr)token).Value);
        GC.KeepAlive(root);
        GC.KeepAlive(a);
    }

    [TestMethod]
    public void ARootAndItsFirstFieldShareOneToken_AReferenceFreeRootRegisteredLastAnswersItsPrefix()
    {
        builtin.heap(new Plain { A = (Opcode)6, B = 9 }, out ж<Plain> root);
        ж<Opcode> a = field(root, "A");

        register(a);
        nuint token = register(root);

        ж<nint> asInt = (ж<nint>)(uintptr)token;

        Assert.AreEqual((nint)6, asInt.Value, "the root's first word, which is the first field");
        asInt.Value = 60;
        Assert.AreEqual((nint)60, root.Value.A.V, "the same storage");
        GC.KeepAlive(a);
    }

    [TestMethod]
    public void ARootAndItsFirstFieldShareOneToken_AReferenceBearingRootRegisteredLastKeepsTodaysRefusal()
    {
        builtin.heap(new OpFirst { Op = (Opcode)4, Value = "x" }, out ж<OpFirst> root);
        ж<Opcode> op = field(root, "Op");

        Assert.AreEqual(ManagedPointerTokens.CurrentToken(root), ManagedPointerTokens.CurrentToken(op), "the premise: one token for the root and its first field");

        register(op);
        nuint token = register(root);

        // The direct form refuses a reference-bearing struct read as one word, so this keeps the carrier it had.
        ж<nint> asInt = (ж<nint>)(uintptr)token;

        Assert.IsTrue(asInt.IsNative, "the order-token carrier, unchanged");
        Assert.ThrowsException<PanicException>(() => asInt.Value, "and its dereference is refused by name, as before");
        GC.KeepAlive(op);
    }

    [TestMethod]
    public void ANonFirstFieldsOwnTokenReadsThatField()
    {
        builtin.heap(new Record { Value = "s", Op = (Opcode)11 }, out ж<Record> root);
        ж<Opcode> op = field(root, "Op");

        Assert.AreNotEqual(ManagedPointerTokens.CurrentToken(root), ManagedPointerTokens.CurrentToken(op), "the premise: a later field has a token of its own");

        nuint token = register(op);

        Assert.AreEqual((nint)11, ((ж<nint>)(uintptr)token).Value);
        GC.KeepAlive(op);
    }

    [TestMethod]
    public void TheSameFieldReachedByArithmeticOnTheRootIsStillRefusedByName()
    {
        builtin.heap(new Record { Value = "s", Op = (Opcode)11 }, out ж<Record> root);

        // The field's Go offset, read from the token a field box would carry; that box is never registered.
        nuint offset = ManagedPointerTokens.CurrentToken(field(root, "Op")) - ManagedPointerTokens.CurrentToken(root);
        nuint rootToken = register(root);

        Assert.AreNotEqual((nuint)0, offset, "the premise: the field is not at the root's offset");

        // `unsafe.Add(unsafe.Pointer(&r), offset)` read as another type: no box answers for the number, and the interior
        // walk matches exactly, so the arithmetic refusal stands.
        Assert.ThrowsException<PanicException>(() => (ж<nint>)(uintptr)(rootToken + offset));
    }

    [TestMethod]
    public unsafe void ARegisteredNativeBoxReadAsAnotherTypeKeepsTheNativeRoute()
    {
        nint memory = (nint)NativeMemory.AllocZeroed(16);

        try
        {
            NativeBox<Opcode> native = new((nuint)memory);
            nuint token = register(native);

            Assert.AreEqual((nuint)memory, token, "the premise: a native box's order token IS its address");

            ж<nint> asInt = (ж<nint>)(uintptr)token;

            Assert.IsTrue(asInt.IsNative, "a native box, never a view over one");
            Assert.AreEqual((nuint)memory, asInt.NativeAddress);

            asInt.Value = 0x5A;
            Assert.AreEqual((nint)0x5A, native.Value.V, "over the same memory");
            GC.KeepAlive(native);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public unsafe void ARegisteredFieldOfANativeElementReadAsAnotherTypeKeepsTheNativeRoute()
    {
        const int N = 2;
        nint memory = (nint)NativeMemory.AllocZeroed((nuint)(N * sizeof(NativeWide)));

        try
        {
            global::go.slice<NativeWide> native = global::go.slice<NativeWide>.OverNativeMemory((nuint)memory, N);

            // The element box ж<array<T>>.at() takes over native storage, then a NON-first field of it: a view
            // whose token is a REAL address (the element's plus the Go offset), though the view itself answers
            // NativeAddress 0. A field of a NativeBox root is not this shape: its token is tagged.
            ж<NativeWide> element = new ElemRefBox<NativeWide>((IArray)native, 1);
            ж<Opcode> op = field(element, "Op");
            nuint token = register(op);
            nuint address = (nuint)memory + (nuint)sizeof(NativeWide) + (nuint)sizeof(nint);

            Assert.IsFalse(op.IsNative, "the premise: a field view, not a native box");
            Assert.AreEqual(address, token, "the premise: its token is the field's real address");

            ж<nint> asInt = (ж<nint>)(uintptr)token;

            Assert.IsTrue(asInt.IsNative, "a native box over the address, never a view over the field view");
            Assert.AreEqual(address, asInt.NativeAddress);

            asInt.Value = 0x5A;
            Assert.AreEqual((nint)0x5A, native[1].Op.V, "over the same memory");
            GC.KeepAlive(op);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }
}
