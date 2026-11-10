// C long on windows (LLP64): 32 bits, and the C types of windows' struct arms. Hand-declared until cgo Phase 2 increment
// 3d (see GoCgoWrapperTypes.cs).

namespace go;

internal static partial class cgotestꓸC_package
{
    internal readonly struct @long(int32 value)
    {
        private readonly int32 m_value = value;
        public static implicit operator @long(int32 value) => new(value);
        public static implicit operator int32(@long value) => value.m_value;
    }

    internal readonly struct @ushort(uint16 value)
    {
        private readonly uint16 m_value = value;
        public static implicit operator @ushort(uint16 value) => new(value);
        public static implicit operator uint16(@ushort value) => value.m_value;
    }

    internal struct POINT
    {
        public @long x;
        public @long y;
    }

    internal struct RECT
    {
        public @long left;
        public @long top;
        public @long right;
        public @long bottom;
    }

    internal struct SYSTEMTIME
    {
        public @ushort wYear;
        public @ushort wMonth;
        public @ushort wDayOfWeek;
        public @ushort wDay;
        public @ushort wHour;
        public @ushort wMinute;
        public @ushort wSecond;
        public @ushort wMilliseconds;
    }
}
