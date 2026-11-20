// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using color = image.color_package;
using image;
using static image_package;

partial class image_package {

// Black is an opaque black uniform image.
public static ж<Uniform> Black = NewUniform(color.Black);
// White is an opaque white uniform image.
public static ж<Uniform> White = NewUniform(color.White);
// Transparent is a fully transparent uniform image.
public static ж<Uniform> Transparent = NewUniform(color.Transparent);
// Opaque is a fully opaque uniform image.
public static ж<Uniform> Opaque = NewUniform(color.Opaque);

// Uniform is an infinite-sized [Image] of uniform color.
// It implements the [color.Color], [color.Model], and [Image] interfaces.
partial struct Uniform {
    public color.Color C;
}

} partial class image_package {

internal static (uint32 r, uint32 g, uint32 b, uint32 a) RGBAᴍ(ref Uniform c) {
    return c.C.RGBA();
}

internal static color.Model ColorModelᴍ(ж<Uniform> Ꮡc) {
    return new UniformжModel(Ꮡc);
}

internal static color.Color Convertᴍ(ref Uniform c, color.Color _) {
    return c.C;
}

internal static Rectangle Boundsᴍ(ref Uniform c) { return new Rectangle(new Point(-1000000000, -1000000000), new Point(1000000000, 1000000000)); }

internal static color.Color Atᴍ(ref Uniform c, nint x, nint y) { return c.C; }

internal static color.RGBA64 RGBA64Atᴍ(ref Uniform c, nint x, nint y) {
    var (r, g, b, a) = c.C.RGBA();
    return new color.RGBA64((uint16)r, (uint16)g, (uint16)b, (uint16)a);
}

// Opaque scans the entire image and reports whether it is fully opaque.
internal static bool Opaqueᴍ(ref Uniform c) {
    var (_, _, _, a) = c.C.RGBA();
    return a == 0xffff;
}

} partial class image_package {

// NewUniform returns a new [Uniform] image of the given color.
public static ж<Uniform> NewUniform(color.Color c) {
    return Ꮡ(new Uniform(c));
}

} // end image_package

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class imageᴛUniform {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ref Uniform c) => image_package.RGBAᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ж<Uniform> Ꮡc) => image_package.ColorModelᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color Convert(this ref Uniform c, color.Color _) => image_package.Convertᴍ(ref c, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Uniform c) => image_package.Boundsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Uniform c, nint x, nint y) => image_package.Atᴍ(ref c, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Uniform c, nint x, nint y) => image_package.RGBA64Atᴍ(ref c, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Uniform c) => image_package.Opaqueᴍ(ref c);
}
// </GlyphForwarders>
