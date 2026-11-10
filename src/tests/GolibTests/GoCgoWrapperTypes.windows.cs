// C long on windows (LLP64): 32 bits. Hand-declared until cgo Phase 2 increment 3d (see GoCgoWrapperTypes.cs).

namespace go;

internal static partial class cgotestꓸC_package
{
    internal readonly struct @long(int32 value)
    {
        private readonly int32 m_value = value;
        public static implicit operator @long(int32 value) => new(value);
        public static implicit operator int32(@long value) => value.m_value;
    }
}
