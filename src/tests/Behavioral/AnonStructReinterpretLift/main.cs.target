global using Tuint32_t = uint;
global using Tuint64_t = ulong;

namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using @unsafe = unsafe_package;
using System.Runtime.InteropServices;
using go.golib;

partial class main_package {

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct fpclassify_u /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint64_t>))] public readonly long Fi;
    [FieldOffset(0)] public float64 Ff;
}

internal partial struct fpclassify_type /*dyn*/ {
    internal float64 f;
}

internal static int32 fpclassify(float64 x) {
    int32 e = default;
    ref var u = ref heap(new fpclassify_u(), out var Ꮡu);
    _ = u;
    u = (Ꮡ(new fpclassify_type(f: x)).Reinterpret<fpclassify_type, fpclassify_u>()).Value;
    e = (int32)((Tuint64_t)((~Ꮡu.Reinterpret<fpclassify_u, Tuint64_t>() >> (int)((int32)52)) & (uint64)0x7ff));
    return e;
}

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] internal partial struct copysignf_ux /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint32_t>))] public readonly int Fi;
    [FieldOffset(0)] public float32 Ff;
}

internal partial struct copysignf_type /*dyn*/ {
    internal float32 f;
}

internal static float32 copysignf(float32 x, float32 y) {
    ref var ux = ref heap(new copysignf_ux(), out var Ꮡux);
    ref var uy = ref heap(new copysignf_ux(), out var Ꮡuy);
    _ = ux;
    _ = uy;
    ux = (Ꮡ(new copysignf_type(f: x)).Reinterpret<copysignf_type, copysignf_ux>()).Value;
    uy = (Ꮡ(new copysignf_type(f: y)).Reinterpret<copysignf_type, copysignf_ux>()).Value;
    (Ꮡux.Reinterpret<copysignf_ux, Tuint32_t>()).Value &= (uint32)0x7fffffff;
    (Ꮡux.Reinterpret<copysignf_ux, Tuint32_t>()).Value |= (Tuint32_t)(~Ꮡuy.Reinterpret<copysignf_ux, Tuint32_t>() & (uint32)0x80000000U);
    return ~Ꮡux.Reinterpret<copysignf_ux, float32>();
}

internal partial struct pair_p /*dyn*/ {
    public uint32 A;
}

internal static uint32 pair(uint32 a, uint32 b) {
    pair_p p = default;
    pair_p q = default;
    (p.A, q.A) = (a, b);
    return p.A * 10 + q.A;
}

internal static void main() {
    fmt.Println(fpclassify(1.5D), fpclassify(0D), fpclassify(Δmath.Inf(1)));
    fmt.Println(copysignf(3F, -1F), copysignf(-2.5F, 1F));
    fmt.Println(pair(4, 2));
}

} // end main_package
