// The C types GoCgoWrappers.<os>.golden.cs names, HAND-DECLARED. Increment 3d emits cmd/cgo's own type declarations into
// the pseudo-package (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.17), but in the shape go2cs-gen expands, which
// GolibTests (no go2cs-gen analyzer) cannot compile; these retire at increment 5 or G1, where a converted project compiles
// the converter's own types and these wrappers together. Each is the shape a converted
// numeric type takes: a value wrapper over its Go type, converting implicitly both ways. C long's width differs by data
// model, so it is declared per OS (GoCgoWrapperTypes.windows.cs and GoCgoWrapperTypes.posix.cs). Increment 3b2a's C
// structs are the shape a converted Go struct takes: a C# struct with the C names as fields, each of a C type above, and
// an inline array as golib's array<T> sized at construction. Each OS's structs are with its C long.

namespace go;

internal static partial class cgotestꓸC_package
{
    internal readonly struct @int(int32 value)
    {
        private readonly int32 m_value = value;
        public static implicit operator @int(int32 value) => new(value);
        public static implicit operator int32(@int value) => value.m_value;
    }

    internal readonly struct @uint(uint32 value)
    {
        private readonly uint32 m_value = value;
        public static implicit operator @uint(uint32 value) => new(value);
        public static implicit operator uint32(@uint value) => value.m_value;
    }

    internal readonly struct @char(int8 value)
    {
        private readonly int8 m_value = value;
        public static implicit operator @char(int8 value) => new(value);
        public static implicit operator int8(@char value) => value.m_value;
    }

    internal readonly struct @double(float64 value)
    {
        private readonly float64 m_value = value;
        public static implicit operator @double(float64 value) => new(value);
        public static implicit operator float64(@double value) => value.m_value;
    }

    // C void: cmd/cgo's [0]byte, a value with nothing in it.
    internal readonly struct @void;

    // div_t, which every C library has.
    internal struct div_t
    {
        public @int quot;
        public @int rem;
    }

    // The probe library's structs (increment 4d, src/tests/GolibTests/cgo-probe/probe.c), the same on every OS. The
    // tagged struct's union is the bytes cmd/cgo gives it; its padding is a blank field, which a converted struct omits.
    internal struct struct_go2cs_probe_pair
    {
        public @int a;
        public @int b;
    }

    internal struct struct_go2cs_probe_tagged
    {
        public @int tag;
        public array<byte> u = new(8);

        public struct_go2cs_probe_tagged() { }
    }

    internal struct struct_go2cs_probe_hfa
    {
        public @double x;
        public @double y;
    }
}
