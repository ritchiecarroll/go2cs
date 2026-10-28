// NilHeaderEqualityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The proof 1.26.10.2 parity seat #24 (N1 stage 2) is gated on. Today a Go <c>x == nil</c> on a slice, map or chan
/// emits <c>x == default</c>, which binds the type's header operator; the seat makes it <c>x == nil</c>, which binds
/// the <see cref="NilType"/> operator, and for a slice and a map that operator reads <c>IsNil</c>. The two must agree
/// for every way a value is made, or the seat changes behavior. So each arm builds one construction kind and asserts
/// both Go's own answer and <c>IsNil == (x == default)</c>; a chan has no <c>IsNil</c> and its nil operator already
/// defers to the header, so its arms check the two operators against Go's answer.
/// </summary>
[TestClass]
public class NilHeaderEqualityTests
{
    private static void Agrees<T>(slice<T> s, bool goNil, string kind)
    {
        Assert.AreEqual(goNil, s.IsNil, $"{kind}: IsNil is Go's answer");
        Assert.AreEqual(s.IsNil, s == default(slice<T>), $"{kind}: IsNil agrees with == default");
        Assert.AreEqual(s.IsNil, s == nil, $"{kind}: IsNil agrees with == nil");
        Assert.AreEqual(!s.IsNil, s != default(slice<T>), $"{kind}: != default is the negation");
    }

    private static void Agrees<TKey, TValue>(map<TKey, TValue> m, bool goNil, string kind) where TKey : notnull
    {
        Assert.AreEqual(goNil, m.IsNil, $"{kind}: IsNil is Go's answer");
        Assert.AreEqual(m.IsNil, m == default(map<TKey, TValue>), $"{kind}: IsNil agrees with == default");
        Assert.AreEqual(m.IsNil, m == nil, $"{kind}: IsNil agrees with == nil");
        Assert.AreEqual(!m.IsNil, m != default(map<TKey, TValue>), $"{kind}: != default is the negation");
    }

    private static void Agrees<T>(channel<T> c, bool goNil, string kind)
    {
        Assert.AreEqual(goNil, c == default(channel<T>), $"{kind}: == default is Go's answer");
        Assert.AreEqual(goNil, c == nil, $"{kind}: == nil is Go's answer");
        Assert.AreEqual(!goNil, c != default(channel<T>), $"{kind}: != default is the negation");
    }

    private static ж<byte> NativePointer(IntPtr buffer)
    {
        unsafe
        {
            ж<byte> pointer = (void*)buffer;
            return pointer;
        }
    }

    [TestMethod]
    public void TheNilSliceIsNilEveryWayItIsWritten()
    {
        Agrees(default(slice<byte>), true, "default");
        slice<byte> fromNil = nil;
        Agrees(fromNil, true, "the nil literal");
        Agrees(new slice<byte>((byte[]?)null), true, "[]T(nil array)");
        Agrees(new slice<byte>((byte[]?)null, 0, 0), true, "nil[0:0]");
        Agrees(new slice<byte>((byte[]?)null, 0, 0, 0), true, "nil[0:0:0]");
        Agrees(default(slice<@string>), true, "default, reference element");
        Agrees(append(default(slice<byte>)), true, "append(nil) with nothing");
    }

    [TestMethod]
    public void AManagedSliceIsNeverNilEvenWhenEmpty()
    {
        Agrees(new slice<byte>(0), false, "make([]byte, 0)");
        Agrees(new slice<byte>(0, 8), false, "make([]byte, 0, 8)");
        Agrees(new slice<byte>(4), false, "make([]byte, 4)");
        Agrees(new slice<@string>(0), false, "make([]string, 0)");
        Agrees(new byte[] { }.slice(), false, "[]byte{}");
        Agrees(new byte[] { 1, 2 }.slice(), false, "[]byte{1, 2}");
        Agrees(new byte[] { 1, 2 }.slice()[0..0], false, "s[0:0] of a non-nil slice");
        Agrees(slice<byte>(""u8), false, "[]byte(\"\")");
        Agrees(new slice<byte>(), false, "the parameterless constructor");
    }

    [TestMethod]
    public void ANativeBackedSliceIsNeverNil()
    {
        IntPtr buffer = Marshal.AllocHGlobal(16);

        try
        {
            ж<byte> pointer = NativePointer(buffer);
            Agrees(@unsafe.Slice(pointer, 4), false, "unsafe.Slice over native memory, len 4");
            Agrees(@unsafe.Slice(pointer, 0), false, "unsafe.Slice over native memory, len 0");
            Agrees(@unsafe.Slice(pointer, 4)[0..0], false, "a native slice resliced to zero");
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [TestMethod]
    public void UnsafeSliceOverAManagedElementIsNotNil()
    {
        slice<byte> backing = new byte[] { 1, 2, 3 }.slice();
        Agrees(@unsafe.Slice(Ꮡ(backing, 0), 3), false, "unsafe.Slice(&s[0], 3)");
        Agrees(@unsafe.Slice(Ꮡ(backing, 0), 0), false, "unsafe.Slice(&s[0], 0)");
    }

    // Go: "as a special case, if ptr is nil and len is zero, Slice returns nil".
    [TestMethod]
    public void UnsafeSliceOfANilPointerAndZeroLengthIsNil()
    {
        Agrees(@unsafe.Slice(default(ж<byte>)!, 0), true, "unsafe.Slice(nil, 0)");
    }

    // *(*[]T)(unsafe.Pointer(&hdr)) over a runtime-shaped header: a nil array word with zero len and cap is the nil
    // slice, and a real one is a native window.
    private struct NativeHeaderShape
    {
        public ж<byte> array;
        public nint len;
        public nint cap;
    }

    [TestMethod]
    public void AReinterpretedHeaderViewIsNilExactlyWhenItsArrayWordIs()
    {
        ref NativeHeaderShape empty = ref heap(new NativeHeaderShape(), out ж<NativeHeaderShape> Ꮡempty);
        Agrees(~Ꮡempty.Reinterpret<NativeHeaderShape, slice<byte>>(), true, "a header whose words are all zero");

        IntPtr buffer = Marshal.AllocHGlobal(16);

        try
        {
            NativeHeaderShape header = new() { array = NativePointer(buffer), len = 0, cap = 4 };
            ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);
            Agrees(~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>(), false, "a header over real memory, len 0");
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [TestMethod]
    public void AMapIsNilOnlyWithoutABackingStore()
    {
        Agrees(default(map<@string, nint>), true, "default");
        map<@string, nint> fromNil = nil;
        Agrees(fromNil, true, "the nil literal");
        Agrees(new map<@string, nint>(), false, "make(map[string]int)");
        Agrees(new map<@string, nint>(8), false, "make(map[string]int, 8)");
        Agrees(new map<@string, nint> { }, false, "map[string]int{}");
        Agrees(new map<@string, nint> { ["a"] = 1 }, false, "map[string]int{\"a\": 1}");
    }

    [TestMethod]
    public void AChanIsNilOnlyWithoutACore()
    {
        Agrees(default(channel<nint>), true, "default");
        channel<nint> fromNil = nil;
        Agrees(fromNil, true, "the nil literal");
        Agrees(channel<nint>.SendOnly, true, "the nil chan<- int");
        Agrees(channel<nint>.RecvOnly, true, "the nil <-chan int");
        Agrees(new channel<nint>(0), false, "make(chan int)");
        Agrees(new channel<nint>(4), false, "make(chan int, 4)");
        Agrees(new channel<nint>(1).WithDirection(GoChanDir.Send), false, "a chan narrowed to chan<- int");
    }
}
