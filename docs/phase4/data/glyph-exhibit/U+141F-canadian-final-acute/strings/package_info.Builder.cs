// EXCERPT of src/core/strings/package_info.cs under the global-glyph prototype (scratch, never for master):
// the namespace line, the using directives the forwarders read, and strings.Builder's forwarder block.
// The rest of the file (type accessibility, import initializers, position maps) is unchanged and omitted.

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using @internal;
using go.unicode;
using static strings_package;
using io = io_package;
using math;
// </GlyphForwarderUsings>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᐟ bodies in the package class)
partial class stringsᴛBuilder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyCheck(this ж<Builder> Ꮡb) => strings_package.copyCheckᐟ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Builder b) => strings_package.Stringᐟ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Builder b) => strings_package.Lenᐟ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Cap(this ref Builder b) => strings_package.Capᐟ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Builder b) => strings_package.Resetᐟ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void grow(this ref Builder b, nint n) => strings_package.growᐟ(ref b, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Grow(this ж<Builder> Ꮡb, nint n) => strings_package.Growᐟ(Ꮡb, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Write(this ж<Builder> Ꮡb, slice<byte> p) => strings_package.Writeᐟ(Ꮡb, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteByte(this ж<Builder> Ꮡb, byte c) => strings_package.WriteByteᐟ(Ꮡb, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteRune(this ж<Builder> Ꮡb, rune r) => strings_package.WriteRuneᐟ(Ꮡb, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s) => strings_package.WriteStringᐟ(Ꮡb, s);
}
// </GlyphForwarders>
