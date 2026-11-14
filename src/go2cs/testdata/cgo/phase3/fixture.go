// fixture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package cgophase3 is cgo Phase 3's fixture for increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16):
// cmd/cgo's own helpers (C.CString, C.CBytes, C.GoString, C.GoStringN, C.GoBytes), C.malloc and C.free, a void pointer
// as a parameter and as a result, and the deferred call through cmd/cgo's pointer-check closure. It hands C no Go
// memory. The probe library's functions (src/tests/GolibTests/cgo-probe/probe.c) are declared here as its header would
// declare them.
package cgophase3

/*
#include <stdlib.h>

const char *go2cs_probe_text(void);
char *go2cs_probe_text_copy(const char *text);
int go2cs_probe_text_fill(char *buffer, int size);
long go2cs_probe_sum(const void *bytes, int count);
void *go2cs_probe_same(void *pointer);
char *go2cs_probe_log_new(void);
void go2cs_probe_log_put(char *log, int value);
void go2cs_probe_release(void *pointer);
*/
import "C"

import "unsafe"

// Text reads a C string the library owns.
func Text() string { return C.GoString(C.go2cs_probe_text()) }

// RoundTrip copies a Go string into C memory and reads it back. The free is deferred.
func RoundTrip(s string) string {
	cs := C.CString(s)
	defer C.free(unsafe.Pointer(cs))

	return C.GoString(cs)
}

// Counted reads the first n bytes of a C copy of s, as a string and as bytes.
func Counted(s string, n int) (string, []byte) {
	cs := C.CString(s)
	defer C.free(unsafe.Pointer(cs))

	return C.GoStringN(cs, C.int(n)), C.GoBytes(unsafe.Pointer(cs), C.int(n))
}

// Bytes copies a Go byte slice into C memory, has C sum it behind a void pointer, and reads it back.
func Bytes(b []byte) (int64, []byte) {
	p := C.CBytes(b)
	defer C.free(p)

	return int64(C.go2cs_probe_sum(p, C.int(len(b)))), C.GoBytes(p, C.int(len(b)))
}

// Copy has C make its own copy of a C string. The library that allocated the copy releases it.
func Copy(s string) string {
	cs := C.CString(s)
	defer C.free(unsafe.Pointer(cs))

	copied := C.go2cs_probe_text_copy(cs)
	defer C.go2cs_probe_release(unsafe.Pointer(copied))

	return C.GoString(copied)
}

// Fill has C write a string into a buffer from C.malloc.
func Fill(size int) string {
	buffer := (*C.char)(C.malloc(C.size_t(size)))
	defer C.free(unsafe.Pointer(buffer))

	n := C.go2cs_probe_text_fill(buffer, C.int(size))

	return C.GoStringN(buffer, n)
}

// Same hands C a void pointer and takes it back: nil, C memory, and C memory through a uintptr held in a variable.
func Same() (bool, bool, bool) {
	p := C.malloc(8)
	defer C.free(p)

	number := uintptr(p)

	return C.go2cs_probe_same(nil) == nil, C.go2cs_probe_same(p) == p, uintptr(C.go2cs_probe_same(unsafe.Pointer(number))) == number
}

// Deferred defers two C calls around a deferred Go function, and returns the order C saw them in. The first deferred C
// call's argument is assigned again after its defer: C is handed the value it had at the defer.
func Deferred() string {
	log := C.go2cs_probe_log_new()
	defer C.go2cs_probe_release(unsafe.Pointer(log))

	func() {
		value := C.int(1)
		defer C.go2cs_probe_log_put(log, value)
		value = 9

		defer func() { C.go2cs_probe_log_put(log, 2) }()
		defer C.go2cs_probe_log_put(log, 3)
	}()

	return C.GoString(log)
}
