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

    internal readonly struct @char(int8 value)
    {
        private readonly int8 m_value = value;
        public static implicit operator @char(int8 value) => new(value);
        public static implicit operator int8(@char value) => value.m_value;
    }

    internal struct struct_in_addr
    {
        public @uint s_addr;
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
