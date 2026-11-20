// inlineArrayOverCap_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// W14 (COORD 2026-11-15, sqlite-cs .4 gate wall, linux libc init): crypt_blowfish's TBF_ctx is
//
//	struct { FPS [0][1042]TBF_word; Fs struct { FP [18]TBF_word; FS [4][256]TBF_word } }
//
// and libc reads its initial state as a VALUE through a reinterpret, *(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{...})).
// It is kept out of inline arrays, so its arrays are array<T> references, and the read takes golib's address route: a
// native box over the [1042]uint32's storage whose whole-value read fabricates array<T> references (an access
// violation, measured; COORD's B0/B2, not this file's arms).
//
// COORD's question (a): is the size count right? Two separate counts trip, and only one is wrong:
//   - the [0][1042] carrier was measured at its ELEMENT's size: inlineArrayCaps recursed into a zero-length array's
//     element, [1042]uint32, 4168 bytes, over the 4096-byte ARRAY cap. A zero-length array holds no element and
//     occupies 0 bytes, so that count is wrong;
//   - the struct itself is 4168 bytes all the same: Fs alone is 72 + 4096. The 4096-byte STRUCT cap (7.7.1, ruling 3)
//     keeps TBF_ctx out by itself, so fixing the carrier count does not admit it.

// (a), isolated from the struct cap: a [0][1042]uint32 field in a 4-byte struct is admitted.
func TestInlineArrayZeroLengthArrayCountsNoBytes(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type Small struct {
	PS [0][1042]uint32
	N  uint32
}

func read(p unsafe.Pointer) *Small { return (*Small)(p) }
`})

	if got := admittedKeys(a); !strings.Contains(got, "Small") {
		t.Fatalf("admitted %q, want Small: a [0][1042]uint32 occupies 0 bytes; warnings %v", got, a.warnings)
	}
}

// TBF_ctx's own shape: it stays out, and the warning names the STRUCT cap, which it is genuinely over.
func TestInlineArrayTBFContextIsOverTheStructCap(t *testing.T) {
	a, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", `package lib

import "unsafe"

type TBF_word = uint32

type TBF_key = [18]TBF_word

type TBF_ctx = struct {
	FPS [0][1042]TBF_word
	Fs  struct {
		FP TBF_key
		FS [4][256]TBF_word
	}
}

var _BF_init_state = *(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{1, 2, 3}))
`})

	if len(a.keyOrder) != 0 {
		t.Fatalf("admitted %q, want TBF_ctx kept out (4168 bytes, over the 4096-byte struct cap)", admittedKeys(a))
	}

	if len(a.warnings) != 1 || !strings.Contains(a.warnings[0].text, "4168 bytes, over the 4096-byte struct cap") {
		t.Fatalf("warnings %v, want one naming the struct cap (4168 bytes)", a.warnings)
	}
}
