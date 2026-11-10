// The C types GoCgoWrappers.<os>.golden.cs names, HAND-DECLARED until cgo Phase 2 increment 3d emits cmd/cgo's own type
// declarations into the pseudo-package (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.17). Each is the shape a converted
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
}
