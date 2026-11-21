// EXCERPT of src/core/strings/package_info.cs under the final-form prototype (scratch, never for master): the
// namespace line, the using directives, strings.Builder's method-holder declaration (in <MethodHolders>) and its
// forwarder block. The rest of the file is unchanged and omitted.

namespace go;

// <PackageInfoUsings> (scratch prototype: the using directives this file's holders and forwarders read)
using @internal;
using go.unicode;
using static strings_package;
using io = io_package;
using math;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
// </PackageInfoUsings>

// <MethodHolders> (excerpt)
[GoMethodHost(typeof(strings_package.Builder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class stringsᴛBuilder;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ⁔ bodies in the package class)
partial class stringsᴛBuilder {
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyCheck(this ж<Builder> Ꮡb) => strings_package.copyCheck⁔(Ꮡb);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref Builder b) => strings_package.String⁔(ref b);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Builder b) => strings_package.Len⁔(ref b);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cap(this ref Builder b) => strings_package.Cap⁔(ref b);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Builder b) => strings_package.Reset⁔(ref b);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void grow(this ref Builder b, nint n) => strings_package.grow⁔(ref b, n);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Grow(this ж<Builder> Ꮡb, nint n) => strings_package.Grow⁔(Ꮡb, n);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ж<Builder> Ꮡb, slice<byte> p) => strings_package.Write⁔(Ꮡb, p);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteByte(this ж<Builder> Ꮡb, byte c) => strings_package.WriteByte⁔(Ꮡb, c);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteRune(this ж<Builder> Ꮡb, rune r) => strings_package.WriteRune⁔(Ꮡb, r);
[MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s) => strings_package.WriteString⁔(Ꮡb, s);
}
// </GlyphForwarders>
