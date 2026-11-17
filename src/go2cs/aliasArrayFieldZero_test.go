// aliasArrayFieldZero_test.go - Gbtc
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

// A struct field spelled through an ALIAS of an unnamed array (`FP TBF_key` with `type TBF_key = [18]TBF_word`, libc's
// crypt_blowfish) is an array field like `FP [18]TBF_word`, and its Go zero is 18 zero words. The converter emitted the
// `= new(N)` that carries the length only for the syntactic form, so the alias-spelled field kept default(array<T>),
// length 0: Go's `c.Fs.FP[0]` read a zero-length array, and golib measured the field's dims as [0] (B2's probe, the
// decoded TBF_ctx's FP empty). A DEFINED array type is a go2cs-gen wrapper that carries its own length, and keeps no
// initializer.
func TestAliasArrayFieldTakesItsLength(t *testing.T) {
	cs := convertInlineArrayFixture(t, "alias", `package alias

type word = uint32

type key = [18]word

type grid = [2][3]int

type Row [4]int

type ctx struct {
	FP  key
	G   grid
	R   Row
	Raw [5]word
}

var c ctx
`)

	for _, want := range []string{"public key FP = new(18);", "public grid G = new(2, () => new(3));", "public array<word> Raw = new(5);"} {
		if !strings.Contains(cs, want) {
			t.Errorf("missing %q:\n%s", want, cs)
		}
	}

	if strings.Contains(cs, "public Row R = new") {
		t.Errorf("a defined array type took an initializer:\n%s", cs)
	}
}
