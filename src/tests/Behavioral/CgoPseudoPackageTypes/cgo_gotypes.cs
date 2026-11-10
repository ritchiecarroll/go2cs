global using _Ctype_size_t = go.main_package._Ctype_ulong;
global using _Ctype_div_t = go.main_package._Ctype_struct___0;

namespace go;

partial class main_package {

partial struct _Ctype_int /*num:int32*/;
partial struct _Ctype_uint /*num:uint32*/;
partial struct _Ctype_char /*num:int8*/;
partial struct _Ctype_schar /*num:int8*/;
partial struct _Ctype_uchar /*num:uint8*/;
partial struct _Ctype_short /*num:int16*/;
partial struct _Ctype_ushort /*num:uint16*/;
partial struct _Ctype_long /*num:int64*/;
partial struct _Ctype_ulong /*num:uint64*/;
partial struct _Ctype_longlong /*num:int64*/;
partial struct _Ctype_ulonglong /*num:uint64*/;
partial struct _Ctype_float /*num:float32*/;
partial struct _Ctype_double /*num:float64*/;

partial struct _Ctype_void /*[0]byte*/;

partial struct _Ctype_struct___0 {
    internal _Ctype_int quot;
    internal _Ctype_int rem;
}

partial struct _Ctype_struct_point {
    internal _Ctype_int x;
    internal _Ctype_int y;
    internal array<_Ctype_char> name = new(8);
    internal ж<_Ctype_struct_point> next;
}

partial struct _Ctype_struct_segment {
    internal _Ctype_struct_point from;
    internal _Ctype_struct_point to;
}

partial struct _Ctype_union_value /*[8]byte*/;

partial struct _Ctype_enum_color /*num:uint32*/;

internal static UntypedInt _Ciconst_RED => 0x0;
internal static UntypedInt _Ciconst_GREEN => 0x1;
internal static UntypedInt _Ciconst_BLUE => 0x2;
internal static UntypedInt _Ciconst_LIMIT => 0x10;
internal static UntypedFloat _Cfconst_HALF => 0.5;
internal static readonly @string _Csconst_GREETING = "hi"u8;

} // end main_package
