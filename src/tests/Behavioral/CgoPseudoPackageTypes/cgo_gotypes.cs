global using Cꓸsize_t = go.mainꓸC_package.@ulong;
global using Cꓸdiv_t = go.mainꓸC_package.struct___0;

namespace go;

using C = mainꓸC_package;

partial class main_package {

} // end main_package

partial class mainꓸC_package {

partial struct @int /*num:int32*/;
partial struct @uint /*num:uint32*/;
partial struct @char /*num:int8*/;
partial struct schar /*num:int8*/;
partial struct uchar /*num:uint8*/;
partial struct @short /*num:int16*/;
partial struct @ushort /*num:uint16*/;
partial struct @long /*num:int64*/;
partial struct @ulong /*num:uint64*/;
partial struct longlong /*num:int64*/;
partial struct ulonglong /*num:uint64*/;
partial struct @float /*num:float32*/;
partial struct @double /*num:float64*/;

partial struct @void /*[0]byte*/;

partial struct struct___0 {
    internal @int quot;
    internal @int rem;
}

partial struct struct_point {
    internal @int x;
    internal @int y;
    internal array<@char> name = new(8);
    internal ж<struct_point> next;
}

partial struct struct_segment {
    internal struct_point from;
    internal struct_point to;
}

partial struct union_value /*[8]byte*/;

partial struct enum_color /*num:uint32*/;

internal static UntypedInt RED => 0x0;
internal static UntypedInt GREEN => 0x1;
internal static UntypedInt BLUE => 0x2;
internal static UntypedInt LIMIT => 0x10;
internal static UntypedFloat HALF => 0.5;
internal static readonly @string GREETING = "hi"u8;

} // end mainꓸC_package
