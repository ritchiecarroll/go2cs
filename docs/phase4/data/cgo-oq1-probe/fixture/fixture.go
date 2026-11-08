// Package cgofixture is the OQ-1 probe fixture: one cgo file with the five preamble kinds
// (a function, a mixed struct, a #define, an enum, a typedef'd pointer) used from Go.
package cgofixture

/*
#include <stdlib.h>

#define CONST 42
#define FCONST 2.5
#define SCONST "hi"

enum color { RED, GREEN = 5, BLUE };

struct x {
	int a;
	double b;
	char name[8];
	unsigned char flag;
	struct x *next;
};

typedef struct x *xptr;

static int add(int a, int b) { return a + b; }
static double scale(struct x *p) { return p->b * CONST; }
*/
import "C"

import "unsafe"

func Add(a, b int) int { return int(C.add(C.int(a), C.int(b))) }

func Use() (int, float64, string) {
	var s C.struct_x
	s.a = C.CONST
	s.b = C.FCONST
	var p C.xptr = &s
	var col C.enum_color = C.GREEN
	cs := C.CString(C.SCONST)
	defer C.free(unsafe.Pointer(cs))
	return int(s.a) + int(col) + int(C.BLUE), float64(C.scale(p)), C.GoString(cs)
}
