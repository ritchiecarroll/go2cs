// ByteSeqPreMemberImplementerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards go.lib's binary compatibility with packages built against 1.26.10.3: an <see cref="IByteSeq{T}"/>
/// implementer that predates <see cref="IByteSeq{T}.TryGetSlice"/> and <see cref="IByteSeq{T}.TryGetString"/> still
/// loads and converts by value.
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen emits an IByteSeq&lt;byte&gt; implementation into every converted package that declares a named []byte
/// or named string. One built against .3 has neither member. The two structs below are that shape: the members .3's
/// generator wrote, and nothing else. If the two members were abstract, this file would not compile (CS0535), and a
/// .3-built package would fail to load its type under a later go.lib, the break the binary-compatibility guard
/// refused as CP0006. IByteSeq.cs gives both a default body that answers false, so the conversions copy, which is what
/// .3 did for these types.
/// </para>
/// <para>
/// What a .3-built named []byte loses until it is rebuilt is the C16 sharing only: <c>[]byte(v)</c> is a copy rather
/// than v's own window. ByteSeqNamedMemberTests covers the shape today's generator writes, which shares.
/// </para>
/// </remarks>
[TestClass]
public class ByteSeqPreMemberImplementerTests
{
    // A named []byte as go2cs-gen 1.26.10.3 implemented IByteSeq: no TryGetSlice, no TryGetString.
    private readonly struct PreMemberNamedBytes : IByteSeq<PreMemberNamedBytes, byte>
    {
        private readonly slice<byte> m_value;

        public PreMemberNamedBytes(slice<byte> value) => m_value = value;

        public slice<byte> Value => m_value;

        public nint Length => m_value.Length;

        public PreMemberNamedBytes this[Range range] => new(m_value[range]);

        public PreMemberNamedBytes slice(nint low) => new(m_value[(int)low..]);

        public PreMemberNamedBytes slice(nint low, nint high) => new(m_value[(int)low..(int)high]);

        byte IByteSeq<byte>.this[nint index] => m_value[index];

        ReadOnlySpan<byte> IByteSeq<byte>.ꓸꓸꓸ => m_value.ToSpan();
    }

    // A named string as go2cs-gen 1.26.10.3 implemented IByteSeq: no TryGetSlice, no TryGetString.
    private readonly struct PreMemberNamedString : IByteSeq<PreMemberNamedString, byte>
    {
        private readonly @string m_value;

        public PreMemberNamedString(@string value) => m_value = value;

        public nint Length => m_value.Length;

        public byte this[nint index] => m_value[index];

        public PreMemberNamedString this[Range range] => new(m_value[range]);

        public PreMemberNamedString slice(nint low) => new(m_value.slice(low));

        public PreMemberNamedString slice(nint low, nint high) => new(m_value.slice(low, high));

        ReadOnlySpan<byte> IByteSeq<byte>.ꓸꓸꓸ => m_value.ꓸꓸꓸ;
    }

    // What a union-constrained Go body emits for `[]byte(b)` and `string(b)`.
    private static slice<byte> ToBytes<TSeq>(TSeq b) where TSeq : IByteSeq<TSeq, byte> => b.ToSlice();

    private static @string ToText<TSeq>(TSeq b) where TSeq : IByteSeq<TSeq, byte> => b.ToGoString();

    private static slice<byte> Bytes(string text) => new(System.Text.Encoding.ASCII.GetBytes(text));

    private static string Ascii(slice<byte> b) => System.Text.Encoding.ASCII.GetString(b.ToSpan());

    [TestMethod]
    public void BothMembersDefaultToFalse()
    {
        IByteSeq<byte> bytes = new PreMemberNamedBytes(Bytes("ab"));
        IByteSeq<byte> text = new PreMemberNamedString(new @string("ab"));

        Assert.IsFalse(bytes.TryGetSlice(out slice<byte> window), "a .3-built implementer has no window to hand out");
        Assert.AreEqual(0, window.Length);
        Assert.IsFalse(text.TryGetString(out @string value), "a .3-built implementer has no @string to hand out");
        Assert.AreEqual(0, value.Length);
    }

    [TestMethod]
    public void APreMemberNamedByteSliceConvertsByValue()
    {
        PreMemberNamedBytes v = new(Bytes("abcd"));

        slice<byte> b = ToBytes(v);
        Assert.AreEqual("abcd", Ascii(b), "[]byte(v) carries v's bytes");
        Assert.AreEqual("cd", Ascii(ToBytes(v[2..])), "[]byte(v[2:]) carries the sub-slice's bytes");
        Assert.AreEqual("abcd", ToText(v).ToString(), "string(v) carries v's bytes");

        // The fallback copies, as .3's conversions did for this type: the result is not v's backing.
        b[0] = (byte)'A';
        Assert.AreEqual((byte)'a', v.Value[0], "the fallback is a copy; the C16 sharing waits for a rebuild");
    }

    [TestMethod]
    public void APreMemberNamedStringConvertsByValue()
    {
        PreMemberNamedString p = new(new @string("wxyz"));

        Assert.AreEqual("wxyz", ToText(p).ToString(), "string(p) carries p's bytes");
        Assert.AreEqual("yz", ToText(p[2..]).ToString(), "string(p[2:]) carries the sub-string's bytes");

        slice<byte> b = ToBytes(p);
        Assert.AreEqual("wxyz", Ascii(b), "[]byte(p) carries p's bytes");

        b[0] = (byte)'W';
        Assert.AreEqual("wxyz", ToText(p).ToString(), "[]byte(p) over a string is a copy, as in Go");
    }
}
