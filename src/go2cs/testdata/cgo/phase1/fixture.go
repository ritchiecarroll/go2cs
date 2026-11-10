// fixture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package cgofixture is the OQ-1 probe fixture: one cgo file with the five preamble kinds
// (a function, a mixed struct, a #define, an enum, a typedef'd pointer) used from Go, and the
// three ways cmd/cgo spells a C name it does not declare as a plain function, variable, type or
// constant: a two-result call (r, err := C.f()), a C function used as a value, and a #define
// that is not a constant.
package cgofixture

/*
#include <errno.h>
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

int counter = 7;
#define NEXT (counter + 1)

static int halve(int v) { if (v % 2 != 0) { errno = EINVAL; return -1; } return v / 2; }
int twice(int v) { return 2 * v; }
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

// Halve calls a C function in Go's two-result form, which reads errno.
func Halve(n int) (int, error) {
	r, err := C.halve(C.int(n))
	return int(r), err
}

// Twice is a C function's address, used as a value rather than called.
func Twice() unsafe.Pointer { return C.twice }

// Next reads a #define that is not a constant: cmd/cgo evaluates it in C on each use.
func Next() int { return int(C.NEXT) }
