// C long on linux and darwin (LP64): 64 bits, and the C types of their struct arms. Hand-declared until cgo Phase 2
// increment 3d (see GoCgoWrapperTypes.cs).

namespace go;

internal static partial class cgotestꓸC_package
{
    internal readonly struct @long(int64 value)
    {
        private readonly int64 m_value = value;
        public static implicit operator @long(int64 value) => new(value);
        public static implicit operator int64(@long value) => value.m_value;
    }

    internal struct struct_in_addr
    {
        public @uint s_addr;
    }

    // struct tm with glibc's and darwin's tm_gmtoff and the pointer member tm_zone (increment 3b2b).
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
        public @long tm_gmtoff;
        public ж<@char> tm_zone;
    }

#if GOCGO_FLAVOUR_LINUX
    // glibc: six char[65].
    internal struct struct_utsname
    {
        public array<@char> sysname = new(65);
        public array<@char> nodename = new(65);
        public array<@char> release = new(65);
        public array<@char> version = new(65);
        public array<@char> machine = new(65);
        public array<@char> domainname = new(65);

        public struct_utsname() { }
    }
#else
    // darwin: five char[256].
    internal struct struct_utsname
    {
        public array<@char> sysname = new(256);
        public array<@char> nodename = new(256);
        public array<@char> release = new(256);
        public array<@char> version = new(256);
        public array<@char> machine = new(256);

        public struct_utsname() { }
    }
#endif
}
