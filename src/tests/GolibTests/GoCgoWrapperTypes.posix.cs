// C long on linux and darwin (LP64): 64 bits. Hand-declared until cgo Phase 2 increment 3d (see GoCgoWrapperTypes.cs).

namespace go;

internal static partial class cgotestꓸC_package
{
    internal readonly struct @long(int64 value)
    {
        private readonly int64 m_value = value;
        public static implicit operator @long(int64 value) => new(value);
        public static implicit operator int64(@long value) => value.m_value;
    }
}
