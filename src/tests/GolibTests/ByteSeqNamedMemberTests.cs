// ByteSeqNamedMemberTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the two conversions out of Go's `~[]byte | ~string` union (<see cref="ByteSeqExtensions"/>) when the type
/// argument is a NAMED []byte or a NAMED string: Go's sharing rules, and their cost.
/// </summary>
/// <remarks>
/// <para>
/// Go's <c>[]byte(b)</c> over a value whose type is a []byte, named or not, is that same slice. A write through the
/// result is seen through <c>b</c>. Over a string it copies. jsontext passes its <c>type Value []byte</c> to
/// <c>~[]byte | ~string</c> functions (TS readiness C16). Before <see cref="IByteSeq{T}.TryGetSlice"/>,
/// <c>ToSlice</c> recognised only golib's own <c>slice&lt;byte&gt;</c> by type test and COPIED every other member, a
/// named []byte included. The result was correct to read, and a write through it was silently lost. The behavioral
/// project NamedBytesByteSeq asserts the same rule end to end, through go2cs-gen's real wrappers.
/// </para>
/// <para>
/// The two structs below stand in for those wrappers. They declare the same members the generator emits
/// (ISliceTypeTemplate.ByteSeqMembers, and InheritedTypeTemplate's string surface), so these tests pin golib's half of
/// the contract without go2cs-gen. Measured: the conversions allocate nothing over the byte-slice wrapper, and nothing
/// for <c>string(b)</c> over the string wrapper.
/// </para>
/// </remarks>
[TestClass]
public class ByteSeqNamedMemberTests
{
    private const int LoopCount = 1000;

    // A named []byte, as go2cs-gen's slice wrapper implements IByteSeq.
    private readonly struct NamedBytes : IByteSeq<NamedBytes, byte>
    {
        private readonly slice<byte> m_value;

        public NamedBytes(slice<byte> value) => m_value = value;

        public slice<byte> Value => m_value;

        public nint Length => m_value.Length;

        public NamedBytes this[Range range] => new(m_value[range]);

        public NamedBytes slice(nint low) => new(m_value[(int)low..]);

        public NamedBytes slice(nint low, nint high) => new(m_value[(int)low..(int)high]);

        byte IByteSeq<byte>.this[nint index] => m_value[index];

        ReadOnlySpan<byte> IByteSeq<byte>.ꓸꓸꓸ => m_value.ToSpan();

        bool IByteSeq<byte>.TryGetSlice(out slice<byte> window)
        {
            window = m_value;
            return true;
        }

        bool IByteSeq<byte>.TryGetString(out @string value)
        {
            value = default;
            return false;
        }
    }

    // A named string, as go2cs-gen's string wrapper implements IByteSeq.
    private readonly struct NamedString : IByteSeq<NamedString, byte>
    {
        private readonly @string m_value;

        public NamedString(@string value) => m_value = value;

        public nint Length => m_value.Length;

        public byte this[nint index] => m_value[index];

        public NamedString this[Range range] => new(m_value[range]);

        public NamedString slice(nint low) => new(m_value.slice(low));

        public NamedString slice(nint low, nint high) => new(m_value.slice(low, high));

        ReadOnlySpan<byte> IByteSeq<byte>.ꓸꓸꓸ => m_value.ꓸꓸꓸ;

        bool IByteSeq<byte>.TryGetSlice(out slice<byte> window)
        {
            window = default;
            return false;
        }

        bool IByteSeq<byte>.TryGetString(out @string value)
        {
            value = m_value;
            return true;
        }
    }

    // What a union-constrained Go body emits for `[]byte(b)` and `string(b)`.
    private static slice<byte> ToBytes<TSeq>(TSeq b) where TSeq : IByteSeq<TSeq, byte>, new() => b.ToSlice();

    private static @string ToText<TSeq>(TSeq b) where TSeq : IByteSeq<TSeq, byte>, new() => b.ToGoString();

    private static slice<byte> Bytes(string text) => new(System.Text.Encoding.ASCII.GetBytes(text));

    [TestMethod]
    public void ToSliceOverNamedByteSliceSharesItsBacking()
    {
        NamedBytes v = new(Bytes("abcd"));

        slice<byte> b = ToBytes(v);
        b[0] = (byte)'A';

        Assert.AreEqual((byte)'A', v.Value[0], "[]byte(v) over a named []byte must share v's backing, as Go's does");

        // A sub-slice shares too, at its own offset.
        b = ToBytes(v[2..]);
        b[0] = (byte)'C';

        Assert.AreEqual((byte)'C', v.Value[2], "[]byte(v[2:]) must share v's backing at the sub-slice's offset");
        Assert.AreEqual("AbCd", System.Text.Encoding.ASCII.GetString(v.Value.ToSpan()));
    }

    [TestMethod]
    public void ToSliceOverNamedStringCopies()
    {
        NamedString p = new(new @string("wxyz"));

        slice<byte> b = ToBytes(p);
        b[0] = (byte)'W';

        Assert.AreEqual("wxyz", ToText(p).ToString(), "[]byte(p) over a named string must copy its bytes");
        Assert.AreEqual((byte)'W', b[0]);
    }

    [TestMethod]
    public void NamedMemberConversionsAllocateNothing()
    {
        NamedBytes v = new(Bytes("abcd"));
        NamedString p = new(new @string("wxyz"));
        nint acc = 0;

        for (int warm = 0; warm < 32; warm++)
            acc += ToBytes(v).Length + ToText(p).Length;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            acc += ToBytes(v).Length + ToText(p).Length;

        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(8L * (32 + LoopCount), (long)acc);
        Assert.AreEqual(0L, bytes,
            $"[]byte(v) over a named []byte and string(p) over a named string allocated {bytes} bytes across " +
            $"{LoopCount} pairs: the first shares v's window and the second is the identity, both through a " +
            "constrained call on the value type, so neither may copy or box.");
    }

    [TestMethod]
    public void CopyingControlStillAllocates()
    {
        // The measurement's positive control: []byte(p) over a named STRING must copy, so it must allocate.
        NamedString p = new(new @string("wxyz"));
        nint acc = 0;

        for (int warm = 0; warm < 32; warm++)
            acc += ToBytes(p).Length;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            acc += ToBytes(p).Length;

        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(4L * (32 + LoopCount), (long)acc);
        Assert.IsTrue(bytes >= 4L * LoopCount,
            $"the copying control allocated only {bytes} bytes across {LoopCount} copies; a probe that cannot see " +
            "a copy cannot validate the zero-allocation assertion.");
    }
}
