// fixture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package cgophase2 is cgo Phase 2's extern-only fixture: every C function it calls is declared by a system header
// and defined by a system library, so each is one the converter binds rather than one it would have to compile. Each
// function is one shape Phase 2 admits (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §7, §11.7, as amended).
package cgophase2

/*
#cgo LDFLAGS: -lm
#include <math.h>
#include <stdlib.h>
#include <time.h>
*/
import "C"

// Abs passes and returns a C int.
func Abs(x int) int { return int(C.abs(C.int(x))) }

// Labs passes and returns a C long, whose width is the target's.
func Labs(x int64) int64 { return int64(C.labs(C.long(x))) }

// Div returns a struct of integers by value.
func Div(a, b int) (int, int) {
	r := C.div(C.int(a), C.int(b))
	return int(r.quot), int(r.rem)
}

// Cos passes and returns a C double.
func Cos(x float64) float64 { return float64(C.cos(C.double(x))) }

// Exp calls a C function in Go's two-result form, which reads errno.
func Exp(x float64) (float64, error) {
	r, err := C.exp(C.double(x))
	return float64(r), err
}

// Frexp passes a pointer to one C int. Its pointee holds no pointer, so cmd/cgo checks nothing.
func Frexp(x float64) (float64, int) {
	var e C.int
	f := C.frexp(C.double(x), &e)
	return float64(f), int(e)
}

// Mktime passes a pointer to a struct that holds a pointer (glibc's tm_zone), so cmd/cgo wraps the call in its
// pointer-check closure.
func Mktime(year, month, day int) int64 {
	var tm C.struct_tm
	tm.tm_year = C.int(year - 1900)
	tm.tm_mon = C.int(month - 1)
	tm.tm_mday = C.int(day)
	tm.tm_isdst = -1
	return int64(C.mktime(&tm))
}
