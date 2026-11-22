// GlyphProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using static go.builtin;
using static go.runtime_package;

// A converted package's suffix-form methods (owner ruling 2026-11-21), spelled as the converter writes them: each Go
// method's BODY is a plain static `<Name>⁔` in the package class, its receiver the first parameter (`/*this*/`), and the
// Go-named extension method in the type's holder is a [StackTraceHidden] one-line forwarder to it, which package_info.cs
// carries. RecvGenerator's `this ж<T>` twin of a pointer-receiver forwarder is written here by hand, as go2cs-gen mints it.
// The package class is a Go-source frame (a `*_package` class in namespace go), so the runtime names its frames.
namespace go;

internal static partial class glyphprobe_package
{
    internal partial struct counter
    {
        public nint n;
    }

    internal partial struct cell<T>
    {
        public T v;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> callersHere()
    {
        slice<uintptr> pcs = new(32);
        return pcs[..(int)Callers(0, pcs)];
    }

    // func (c counter) Walk() []uintptr
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> Walk⁔(/*this*/ counter c) => callersHere();

    // func (c *counter) Bump() []uintptr
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> Bump⁔(/*this*/ ref counter c)
    {
        c.n++;
        return callersHere();
    }

    // func (c counter) Fail()
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Fail⁔(/*this*/ counter c) => throw panic((@string)"glyph");

    // func (c *cell[T]) Get() T
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Get⁔<T>(/*this*/ ref cell<T> c) => c.v;

    // func (c counter) Spin() []uintptr { f := func() []uintptr { return callersHere() }; return f() }
    // The literal is a Roslyn lambda named for its enclosing body, `<Spin⁔>b__N_M`.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> Spin⁔(/*this*/ counter c)
    {
        Func<slice<uintptr>> f = [MethodImpl(MethodImplOptions.NoInlining)] () => callersHere();
        return f();
    }

    // func (c counter) Explode() { func() { panic("lit") }() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Explode⁔(/*this*/ counter c)
    {
        Action f = [MethodImpl(MethodImplOptions.NoInlining)] () => throw panic((@string)"lit");
        f();
    }

    // The callers, through the Go-named forwarders, as converted call sites read them.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> walkVia(counter c) => c.Walk();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> bumpVia(ж<counter> Ꮡc) => Ꮡc.Value.Bump();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void failVia(counter c) => c.Fail();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> spinVia(counter c) => c.Spin();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void explodeVia(counter c) => c.Explode();
}

[GoMethodHost(typeof(glyphprobe_package.counter))]
internal static partial class glyphprobeᴛcounter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static slice<uintptr> Walk(this glyphprobe_package.counter c) => glyphprobe_package.Walk⁔(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static slice<uintptr> Bump(this ref glyphprobe_package.counter c) => glyphprobe_package.Bump⁔(ref c);

    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static void Fail(this glyphprobe_package.counter c) => glyphprobe_package.Fail⁔(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static slice<uintptr> Spin(this glyphprobe_package.counter c) => glyphprobe_package.Spin⁔(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static void Explode(this glyphprobe_package.counter c) => glyphprobe_package.Explode⁔(c);

    // RecvGenerator's twin of the pointer-receiver forwarder.
    internal static slice<uintptr> Bump(this ж<glyphprobe_package.counter> Ꮡc) => Bump(ref Ꮡc.Value);
}

[GoMethodHost(typeof(glyphprobe_package.cell<>))]
internal static partial class glyphprobeᴛcell
{
    [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.StackTraceHidden, System.Diagnostics.DebuggerStepThrough]
    internal static T Get<T>(this ref glyphprobe_package.cell<T> c) => glyphprobe_package.Get⁔<T>(ref c);

    // RecvGenerator's twin of the generic pointer-receiver forwarder.
    internal static T Get<T>(this ж<glyphprobe_package.cell<T>> Ꮡc) => Get(ref Ꮡc.Value);
}
