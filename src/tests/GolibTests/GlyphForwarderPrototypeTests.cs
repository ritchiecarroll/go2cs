// GlyphForwarderPrototypeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// SCRATCH PROTOTYPE (COORD 20261120T173455Z, an owner-requested scout; never for master).
//
// The shape under test: a Go method's body stays in the PACKAGE class under a glyph-suffixed name, as a plain static
// (not an extension), and its method holder carries the Go-named extension method as a one-line forwarder marked
// AggressiveInlining, StackTraceHidden and DebuggerStepThrough. Go:
//
//     type Kind int
//     type T struct{ k Kind; n int }
//     type words []string
//     func (t T) Kind() Kind      { return t.k }
//     func (t *T) Bump()          { t.n++ }
//     func (w words) len() int    { return len(w) }   // bare len is the builtin
//     func total(t T, w words) int { return int(t.Kind()) + w.len() }
//
// A package-level type Kind beside a method Kind, and a method len beside the builtin len, are the two clash classes
// that today send a type of package main to a holder (the queued main-holders rule).

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace go.glyphproto
{
    public static partial class glyphproto_package
    {
        public partial struct Kind
        {
            public nint v;
        }

        public partial struct T
        {
            public Kind k;
            public nint n;
        }

        public partial struct words
        {
            public slice<@string> s;
        }

        // The bodies, glyph-suffixed, beside their types. NoInlining is a PROTOTYPE DEVICE only: it keeps each body a
        // call target, so the caller's JIT listing shows whether the forwarder in front of it was inlined.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Kind Kind⁀(T t) => t.k;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Bump⁀(ref T t) => t.n++;

        // The bare `len` is golib's builtin: no member of this class is named len.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static nint len⁀(words w) => len(w.s);

        // A body calling methods writes them as Go does; each binds to the holder's forwarder.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static nint GlyphProtoTotal(T t, words w) => t.Kind().v + w.len();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static nint GlyphProtoBumpTwice(ref T t)
        {
            t.Bump();
            t.Bump();
            return t.n;
        }

        // The body records the frames above it; the forwarder's must not be among them.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string[] Frames⁀(T t) =>
            new StackTrace(0, false).GetFrames().Select(frame => frame.GetMethod() is { } m ? $"{m.DeclaringType?.Name}.{m.Name}" : "?").ToArray();

        // Every glyph candidate, as an identifier the C# compiler must accept (the build is the assertion).
        public static nint Size⁀(T t) => 1;   // U+2040 CHARACTER TIE (Pc)
        public static nint Size⁔(T t) => 2;   // U+2054 INVERTED UNDERTIE (Pc)
        public static nint Sizeᐟ(T t) => 3;   // U+141F CANADIAN SYLLABICS FINAL ACUTE (Lo)
        public static nint Sizeˈ(T t) => 4;   // U+02C8 MODIFIER LETTER VERTICAL LINE (Lm)
        public static nint Size͙(T t) => 5;   // U+0359 COMBINING ASTERISK BELOW (Mn)
        public static nint Size﹏(T t) => 6;  // U+FE4F WAVY LOW LINE (Pc, wide)
    }

    // The holders: Go-named extension methods, each a one-line forwarder.
    public static class glyphprotoᴛT
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough]
        public static glyphproto_package.Kind Kind(this glyphproto_package.T t) => glyphproto_package.Kind⁀(t);

        [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough]
        public static void Bump(this ref glyphproto_package.T t) => glyphproto_package.Bump⁀(ref t);

        // NOT inlined on purpose: the frame-hiding arm needs the forwarder's frame to exist.
        [MethodImpl(MethodImplOptions.NoInlining), StackTraceHidden, DebuggerStepThrough]
        public static string[] Frames(this glyphproto_package.T t) => glyphproto_package.Frames⁀(t);
    }

    public static class glyphprotoᴛwords
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough]
        public static nint len(this glyphproto_package.words w) => glyphproto_package.len⁀(w);
    }
}

namespace GolibTests
{
    using go.glyphproto;
    using static go.glyphproto.glyphproto_package;

    [TestClass]
    public class GlyphForwarderPrototypeTests
    {
        private static T Sample() => new() { k = new Kind { v = 40 }, n = 0 };

        private static words Words() => new() { s = new slice<@string>(new @string[] { "a"u8, "b"u8 }) };

        [TestMethod]
        public void ABodyCallsItsSiblingsThroughTheForwardersAndReadsTheBuiltinLen()
        {
            Assert.AreEqual((nint)42, GlyphProtoTotal(Sample(), Words()), "t.Kind() is 40 and w.len() is the builtin len of two");
        }

        [TestMethod]
        public void APointerReceiverForwarderWritesThroughTheCallersStorage()
        {
            T t = Sample();

            Assert.AreEqual((nint)2, GlyphProtoBumpTwice(ref t));
            Assert.AreEqual((nint)2, t.n, "the ref forwarder wrote the caller's T");
        }

        [TestMethod]
        public void TheForwardersFrameIsHiddenFromStackTrace()
        {
            string[] frames = Sample().Frames();

            Assert.AreEqual("glyphproto_package.Frames⁀", frames[0], string.Join(" | ", frames));
            Assert.IsFalse(frames.Any(frame => frame.StartsWith("glyphprotoᴛ", StringComparison.Ordinal)), "the forwarder's frame: " + string.Join(" | ", frames));
            Assert.AreEqual($"{nameof(GlyphForwarderPrototypeTests)}.{nameof(TheForwardersFrameIsHiddenFromStackTrace)}", frames[1], string.Join(" | ", frames));
        }

        // C# makes no delegate from an extension method on a value type (CS1113), so the converter writes a method
        // expression as a static method group under a cast, today naming the method where it lives. With the glyph
        // layout it can name either: the forwarder (the Go name, one more call) or the body (the glyph name, direct).
        [TestMethod]
        public void AMethodExpressionCanBindToTheForwarderOrToTheBody()
        {
            Func<T, Kind> viaForwarder = glyphprotoᴛT.Kind;
            Func<T, Kind> viaBody = Kind⁀;

            Assert.AreEqual("Kind", viaForwarder.Method.Name);
            Assert.AreEqual("Kind⁀", viaBody.Method.Name);
            Assert.AreEqual((nint)40, viaForwarder(Sample()).v);
            Assert.AreEqual((nint)40, viaBody(Sample()).v);
        }

        [TestMethod]
        public void EveryGlyphCandidateIsAnIdentifier()
        {
            T t = Sample();

            Assert.AreEqual((nint)21, Size⁀(t) + Size⁔(t) + Sizeᐟ(t) + Sizeˈ(t) + Size͙(t) + Size﹏(t));
        }
    }
}
