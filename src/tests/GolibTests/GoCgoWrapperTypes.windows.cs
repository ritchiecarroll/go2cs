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

    internal readonly struct @longlong(int64 value)
    {
        private readonly int64 m_value = value;
        public static implicit operator @longlong(int64 value) => new(value);
        public static implicit operator int64(@longlong value) => value.m_value;
    }

    // windows' struct tm: the nine ints alone (increment 3b2b).
    internal struct struct_tm
    {
        public @int tm_sec;
        public @int tm_min;
        public @int tm_hour;
        public @int tm_mday;
        public @int tm_mon;
        public @int tm_year;
        public @int tm_wday;
        public @int tm_yday;
        public @int tm_isdst;
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
