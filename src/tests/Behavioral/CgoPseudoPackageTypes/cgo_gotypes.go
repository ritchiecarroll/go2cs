package main

// What cmd/cgo declares for a package's C names in its _cgo_gotypes.go, written here as plain Go so that the
// converter's C pseudo-package emission (cgo Phase 2 increment 3d) runs on every lane with no C compiler: each C
// type and constant is converted into the package's `mainꓸC_package` under its C name, and reached as `C.<name>`.

type _Ctype_int int32
type _Ctype_uint uint32
type _Ctype_char int8
type _Ctype_schar int8
type _Ctype_uchar uint8
type _Ctype_short int16
type _Ctype_ushort uint16
type _Ctype_long int64
type _Ctype_ulong uint64
type _Ctype_longlong int64
type _Ctype_ulonglong uint64
type _Ctype_float float32
type _Ctype_double float64
type _Ctype_size_t = _Ctype_ulong
type _Ctype_void [0]byte

// typedef struct { int quot; int rem; } div_t;
type _Ctype_struct___0 struct {
	quot _Ctype_int
	rem  _Ctype_int
}

type _Ctype_div_t = _Ctype_struct___0

// struct point { int x, y; char name[8]; struct point *next; };
type _Ctype_struct_point struct {
	x    _Ctype_int
	y    _Ctype_int
	name [8]_Ctype_char
	next *_Ctype_struct_point
}

// struct segment { struct point from, to; };
type _Ctype_struct_segment struct {
	from _Ctype_struct_point
	to   _Ctype_struct_point
}

// union value { long l; double d; };
type _Ctype_union_value [8]byte

// enum color { RED, GREEN, BLUE };
type _Ctype_enum_color uint32

const _Ciconst_RED = 0x0
const _Ciconst_GREEN = 0x1
const _Ciconst_BLUE = 0x2
const _Ciconst_LIMIT = 0x10
const _Cfconst_HALF = 0.5
const _Csconst_GREETING = "hi"
