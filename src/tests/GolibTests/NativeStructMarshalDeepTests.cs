// NativeStructMarshalDeepTests.cs - Gbtc
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
/// The windows funnel's deep form (<see cref="NativeStructMarshal.CallDeep{TState,TResult}"/>), driven through a FAKE
/// native callee: a reference-bearing pointee crosses as a Go-layout native copy with its pointer fields followed,
/// element runs and aliases share one copy, an OUT word comes back as the address the callee wrote, and what has no
/// native image is refused by name before the callee runs.
/// </summary>
/// <remarks>
/// The shapes are Go 1.26's live funnel callers, cut down: <c>WSADuplicateSocket</c>'s <c>*WSAProtocolInfo</c> (array
/// fields, written by the callee) and <c>SetEntriesInAcl</c>'s <c>&amp;entries[0]</c> with a count (a run, a pointer
/// field inside each entry) and <c>**ACL</c> (an OUT word). The real calls are read on windows by net TestFileError
/// and internal/syscall/windows TestDeleteAt.
/// </remarks>
[TestClass]
public unsafe class NativeStructMarshalDeepTests
{
    // WSAProtocolChain and the head of WSAProtocolInfo. Go layout: Flags 0; Chain at 4 (Len 4, Entries 8..36);
    // Version 36; Name 40..56; size 56.
    public struct Chain
    {
        public int Len;
        public array<uint> Entries;

        public Chain() => Entries = new array<uint>(7);
    }

    public struct ProtoInfo
    {
        public uint Flags;
        public Chain Chain;
        public int Version;
        public array<ushort> Name;

        public ProtoInfo()
        {
            Chain = new Chain();
            Name = new array<ushort>(8);
        }
    }

    // TRUSTEE: a pointer to its own type first. Go layout: MultipleTrustee 0, Operation 8, Form 12, Type 16,
    // Value 24; size 32.
    public struct Trustee
    {
        public ж<Trustee> MultipleTrustee;
        public int Operation;
        public int Form;
        public int Type;
        public nuint Value;
    }

    // EXPLICIT_ACCESS. Go layout: Permissions 0, Mode 4, Inheritance 8, Trustee at 16; size 48.
    public struct ExplicitAccess
    {
        public uint Permissions;
        public int Mode;
        public uint Inheritance;
        public Trustee Trustee;
    }

    // ACL: blittable, so a pointer to one is an address, never copied.
    public struct Acl
    {
        public byte Revision;
        public byte Sbz1;
        public ushort Size;
        public ushort AceCount;
        public ushort Sbz2;
    }

    // WSABuf: a byte pointer beside a length (reference-bearing by its pointer field).
    public struct Buf
    {
        public uint Len;
        public ж<byte> Data;
    }

    // Two pointer fields that may name the same pointee.
    public struct Pair
    {
        public ж<Trustee> A;
        public ж<Trustee> B;
    }

    // No native image: a string beside an array.
    public struct Named
    {
        public @string Name;
        public array<byte> B;

        public Named() => B = new array<byte>(4);
    }

    // No native image: a slice field.
    public struct Listed
    {
        public slice<byte> Items;
        public array<byte> B;

        public Listed() => B = new array<byte>(4);
    }

    private sealed class Probe
    {
        public Func<ReadOnlySpan<nuint>, nuint>? Body;
        public bool Invoked;
    }

    private delegate nuint Callee(ReadOnlySpan<nuint> args);

    private static nuint invoke(Probe probe, ReadOnlySpan<nuint> args)
    {
        probe.Invoked = true;
        return probe.Body!(args);
    }

    private static nuint call(Probe probe, params nuint[] args) => NativeStructMarshal.CallDeep(args, probe, &invoke);

    private static nuint token<T>(ж<T> box)
    {
        nuint number = ((uintptr)box).Value;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(number), $"precondition: a {typeof(T).Name} box answers a token");
        return number;
    }

    [TestMethod]
    public void AStructWithArrayFieldsIsFilledByTheCallee()
    {
        // WSADuplicateSocket(s, pid, &info): the callee fills the record.
        ref ProtoInfo info = ref heap(new ProtoInfo(), out ж<ProtoInfo> box);
        info.Version = 3;

        Probe probe = new()
        {
            Body = args =>
            {
                Assert.AreEqual((nuint)7, args[0], "an ordinary word passes unchanged");
                byte* p = (byte*)args[2];
                Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(args[2]), "the callee receives an address, not the token");
                Assert.AreEqual(3, *(int*)(p + 36), "Version at 36");

                *(uint*)(p + 0) = 0x20066;
                *(int*)(p + 4) = 1;
                *(uint*)(p + 8 + 4 * 6) = 0xBEEF;
                *(ushort*)(p + 40 + 2 * 7) = 'z';
                return 0;
            }
        };

        call(probe, 7, 1234, token(box));

        Assert.AreEqual(0x20066u, box.Value.Flags);
        Assert.AreEqual(1, box.Value.Chain.Len, "a nested struct decodes in place");
        Assert.AreEqual(0xBEEFu, box.Value.Chain.Entries[6], "a nested array's last element decodes back");
        Assert.AreEqual((ushort)'z', box.Value.Name[7]);
        Assert.AreEqual(3, box.Value.Version, "a field the callee did not touch keeps its value");
    }

    [TestMethod]
    public void AnElementRunAPointerFieldAndAnOutWordCrossTogether()
    {
        // SetEntriesInAcl(2, &entries[0], nil, &newACL).
        ref Trustee group = ref heap(new Trustee(), out ж<Trustee> groupBox);
        group.Form = 5;

        slice<ExplicitAccess> entries = new slice<ExplicitAccess>(2);
        entries[0].Permissions = 0x1F01FF;
        entries[0].Trustee.MultipleTrustee = groupBox;
        entries[1].Mode = 1;

        ж<ExplicitAccess> first = new ElemRefBox<ExplicitAccess>(entries, 0);
        heap<ж<Acl>>(nil, out ж<ж<Acl>> newAcl);

        Acl* written = (Acl*)NativeMemory.AllocZeroed((nuint)sizeof(Acl));

        try
        {
            nuint groupWord = 0;

            Probe probe = new()
            {
                Body = args =>
                {
                    Assert.AreEqual((nuint)2, args[0]);
                    Assert.AreEqual((nuint)0, args[2], "a nil pointer argument is 0");

                    byte* run = (byte*)args[1];
                    Assert.AreEqual(0x1F01FFu, *(uint*)run, "entries[0].Permissions at 0");
                    Assert.AreEqual(1, *(int*)(run + 48 + 4), "entries[1] follows at Go's stride, 48: the run reaches the count");

                    groupWord = *(nuint*)(run + 16);
                    Assert.AreNotEqual((nuint)0, groupWord, "a non-nil pointer field crosses as an address");
                    Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(groupWord), "a reference-bearing pointee is copied, never a token");
                    Assert.AreEqual(5, *(int*)((byte*)groupWord + 12), "the pointee's copy carries its fields (Form at 12)");
                    Assert.AreEqual((nuint)0, *(nuint*)(run + 48 + 16), "a nil pointer field is 0");

                    *(int*)((byte*)groupWord + 16) = 9;
                    *(uint*)(run + 48 + 8) = 3;
                    *(nuint*)args[3] = (nuint)written;
                    return 0;
                }
            };

            call(probe, 2, token(first), 0, token(newAcl));

            Assert.AreSame(groupBox, entries[0].Trustee.MultipleTrustee, "an unchanged pointer word keeps the caller's own box");
            Assert.AreEqual(9, groupBox.Value.Type, "the pointee's copy decodes back into its own box");
            Assert.AreEqual(3u, entries[1].Inheritance, "the run's second element decodes back into the slice");
            Assert.AreNotEqual(nil, newAcl.Value, "nil before and non-zero after is the address the callee wrote");
            Assert.AreEqual((nuint)written, ((uintptr)newAcl.Value).Value, "the OUT word is a native box over that exact address");

            written->AceCount = 4;
            Assert.AreEqual((ushort)4, newAcl.Value.Value.AceCount, "the native box reads the callee's memory");
        }
        finally
        {
            NativeMemory.Free(written);
        }
    }

    [TestMethod]
    public void AliasesShareOneCopy()
    {
        slice<ExplicitAccess> entries = new slice<ExplicitAccess>(3);
        ж<ExplicitAccess> e0 = new ElemRefBox<ExplicitAccess>(entries, 0);
        ж<ExplicitAccess> e2 = new ElemRefBox<ExplicitAccess>(entries, 2);

        heap(new Trustee(), out ж<Trustee> shared);
        ref Pair pair = ref heap(new Pair(), out ж<Pair> pairBox);
        pair.A = shared;
        pair.B = shared;

        Probe probe = new()
        {
            Body = args =>
            {
                Assert.AreEqual(args[0] + 2 * 48, args[1], "another element of a run already copied is that run's element");

                nuint a = *(nuint*)args[2];
                Assert.AreEqual(a, *(nuint*)(args[2] + 8), "two pointers to one pointee store one address");

                *(int*)((byte*)args[1] + 4) = 6;
                return 0;
            }
        };

        call(probe, token(e0), token(e2), token(pairBox));

        Assert.AreEqual(6, entries[2].Mode);
        Assert.AreSame(shared, pairBox.Value.A);
        Assert.AreSame(shared, pairBox.Value.B);
    }

    [TestMethod]
    public void AnEarlierElementCannotJoinARunAlreadyCopied()
    {
        slice<ExplicitAccess> entries = new slice<ExplicitAccess>(3);
        ж<ExplicitAccess> e0 = new ElemRefBox<ExplicitAccess>(entries, 0);
        ж<ExplicitAccess> e1 = new ElemRefBox<ExplicitAccess>(entries, 1);
        Probe probe = new() { Body = _ => 0 };

        PanicException refused = Assert.ThrowsException<PanicException>(() => call(probe, token(e1), token(e0)));

        StringAssert.Contains(refused.Message, "argument 1");
        StringAssert.Contains(refused.Message, "element 0");
        Assert.IsFalse(probe.Invoked, "the call must not run");
    }

    [TestMethod]
    public void ABlittablePointeeCrossesAsItsOwnPinnedAddress()
    {
        // A WSABuf's Data: the byte buffer is not copied. The callee gets the caller's own storage, and the pin
        // is the box's, so a write after the call returns (an overlapped completion) still lands in the buffer.
        byte[] storage = new byte[16];
        slice<byte> buffer = storage.slice();
        ж<byte> data = new ElemRefBox<byte>(buffer, 0);

        ref Buf buf = ref heap(new Buf(), out ж<Buf> bufBox);
        buf.Len = 16;
        buf.Data = data;

        nuint seen = 0;

        Probe probe = new()
        {
            Body = args =>
            {
                seen = *(nuint*)(args[0] + 8);
                ((byte*)seen)[0] = 1;
                return 0;
            }
        };

        call(probe, token(bufBox));

        Assert.AreEqual(((uintptr)data).Value, seen, "the field carries the buffer's own pinned address");
        ((byte*)seen)[3] = 7;
        Assert.AreEqual((byte)1, storage[0]);
        Assert.AreEqual((byte)7, storage[3], "a write after the call lands in the caller's buffer");
        Assert.AreSame(data, bufBox.Value.Data);
    }

    [TestMethod]
    public void AFieldWithNoNativeImageIsRefusedByName()
    {
        heap(new Named(), out ж<Named> named);
        heap(new Listed(), out ж<Listed> listed);
        Probe probe = new() { Body = _ => 0 };

        PanicException byString = Assert.ThrowsException<PanicException>(() => call(probe, 0, token(named)));
        StringAssert.Contains(byString.Message, "argument 1");
        StringAssert.Contains(byString.Message, "Named");
        StringAssert.Contains(byString.Message, "field Name");

        PanicException bySlice = Assert.ThrowsException<PanicException>(() => call(probe, token(listed)));
        StringAssert.Contains(bySlice.Message, "field Items");

        Assert.IsFalse(probe.Invoked, "the call must not run");
    }

    [TestMethod]
    public void AnElementOfANonContiguousHolderIsRefusedByName()
    {
        // The foreign arm: a collection with no T[] backing, so no run to copy.
        ForeignArray<ExplicitAccess> foreign = new(2);
        ж<ExplicitAccess> e0 = new ElemRefBox<ExplicitAccess>(foreign, 0);
        Probe probe = new() { Body = _ => 0 };

        PanicException refused = Assert.ThrowsException<PanicException>(() => call(probe, token(e0)));

        StringAssert.Contains(refused.Message, "no contiguous backing array");
        Assert.IsFalse(probe.Invoked);
    }

    [TestMethod]
    public void AWordLeftInsideACopyIsRefusedRatherThanDangling()
    {
        heap(new Pair(), out ж<Pair> pairBox);
        slice<ExplicitAccess> entries = new slice<ExplicitAccess>(2);
        ж<ExplicitAccess> e0 = new ElemRefBox<ExplicitAccess>(entries, 0);

        Probe probe = new()
        {
            Body = args =>
            {
                *(nuint*)args[0] = args[1] + 48 + 16;
                return 0;
            }
        };

        PanicException refused = Assert.ThrowsException<PanicException>(() => call(probe, token(pairBox), token(e0)));
        StringAssert.Contains(refused.Message, "freed when the call returns");
    }

    [TestMethod]
    public void TheDeepProbeNamesTheRefusedField()
    {
        Assert.IsNull(NativeStructMarshal.DeepRefusal(typeof(ExplicitAccess)), "a pointer field is carried");
        Assert.IsNull(NativeStructMarshal.DeepRefusal(typeof(ProtoInfo)), "nested arrays are carried");
        Assert.AreEqual("Name", NativeStructMarshal.DeepRefusal(typeof(Named)));
        Assert.AreEqual("Items", NativeStructMarshal.DeepRefusal(typeof(Listed)));
    }

    // An IArray<T> that is not a slice, array or slice view: ElemRefBox takes its foreign arm (a PinnedBuffer's).
    private sealed class ForeignArray<T> : IArray<T>
    {
        private readonly T[] m_items;

        public ForeignArray(int length) => m_items = new T[length];

        public T[] Source => m_items;

        Array? IArray.Source => m_items;

        public nint Length => m_items.Length;

        public ref T this[nint index] => ref m_items[index];

        object? IArray.this[nint index]
        {
            get => m_items[index];
            set => m_items[index] = (T)value!;
        }

        public Span<T> ꓸꓸꓸ => m_items;

        public Span<T> ToSpan() => m_items;

        public System.Collections.Generic.IEnumerator<(nint, T)> GetEnumerator()
        {
            for (nint i = 0; i < m_items.Length; i++)
                yield return (i, m_items[i]);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public object Clone() => new ForeignArray<T>(m_items.Length);
    }
}
