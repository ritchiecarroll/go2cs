// PlainEmptyLiteralTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A converted user package now emits Go's `""` in a `string` slot as the bare C# literal `""`, which
/// reaches @string through <c>implicit operator @string(string)</c> and StringLiteralCache, where it
/// used to be <c>""u8</c> (plain string literals, owner ruling 2026-10-17). Go's empty string has one
/// value, so the plain form must be indistinguishable from the zero @string by everything a Go program
/// can observe: equality, length, and the bytes conversion, which Go defines to yield a NON-nil slice
/// for any string (<c>[]byte("")</c> is an empty, non-nil slice).
/// </summary>
[TestClass]
public class PlainEmptyLiteralTests
{
    [TestMethod]
    public void APlainEmptyLiteralIsTheZeroString()
    {
        @string plain = "";
        @string zero = default;
        @string fromSpan = ""u8;

        Assert.IsTrue(plain == zero, "plain \"\" must equal the zero @string");
        Assert.IsTrue(plain == fromSpan, "plain \"\" must equal the former \"\"u8 form");
        Assert.IsFalse(plain != zero);
        Assert.AreEqual((nint)0, builtin.len(plain));
        Assert.AreEqual(builtin.len(zero), builtin.len(plain));
        Assert.AreEqual(zero.GetHashCode(), plain.GetHashCode(), "equal strings must hash equal (map keys)");

        // The emitted form of Go's []byte(s) (see any behavioral golden: `slice<byte>(s)`).
        slice<byte> fromPlain = builtin.slice<byte>(plain);
        slice<byte> fromZero = builtin.slice<byte>(zero);

        Assert.IsFalse(fromPlain.IsNil, "Go: []byte(\"\") is a non-nil empty slice");
        Assert.AreEqual(fromZero.IsNil, fromPlain.IsNil, "plain \"\" and the zero @string must convert alike");
        Assert.AreEqual((nint)0, builtin.len(fromPlain));
    }
}
