// valueDecodeSites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"regexp"
	"strings"
	"testing"
)

// B2's D2: every rvalue `*p` of a kept-out struct emits `(p).GoValue()`, and nothing else does. The fixture is the
// census's control module grown by the shapes the emission must tell apart: the reinterpret seed and a pointer stored
// in a local are read as values; an assignment target, a field selection, a constant operand and a pointer parameter
// keep today's emission.
const valueDecodeFixture = `package lib

import "unsafe"

type TBF_word = uint32

type TBF_ctx = struct {
	FPS [0][1042]TBF_word
	Fs  struct {
		FP [18]TBF_word
		FS [4][256]TBF_word
	}
}

var init_ = [1042]uint32{1, 2, 3}

var state = *(*TBF_ctx)(unsafe.Pointer(&init_)) // DECODE: the seed

func load(q *TBF_ctx) TBF_ctx {
	r := q
	return *r // DECODE: a pointer stored first
}

func store(q *TBF_ctx) {
	r := q
	*r = state // KEEP: an assignment target
}

func field(q *TBF_ctx) uint32 {
	r := q
	return (*r).Fs.FP[0] // KEEP: a field selection
}

func size(q *TBF_ctx) uintptr {
	r := q
	return unsafe.Sizeof(*r) // KEEP: a constant operand
}

func param(p *TBF_ctx) TBF_ctx {
	return *p // KEEP: a pointer parameter's ref-local alias
}
`

func TestValueDecodeEmissionReadsAKeptOutStructAsAValue(t *testing.T) {
	cs := convertInlineArrayFixture(t, "lib", valueDecodeFixture)
	decoded := regexp.MustCompile(`\.GoValue\(\)`)

	// The converter does not carry these trailing comments into C#, so each site is found by its own code.
	for name, site := range map[string]*regexp.Regexp{
		"the seed":               regexp.MustCompile(`TBF_ctx state = \(.*Reinterpret<.*\)\.GoValue\(\)`),
		"a pointer stored first": regexp.MustCompile(`return \(r\)\.GoValue\(\)`),
		"an assignment target":   regexp.MustCompile(`r\.Value\.AssignFrom\(state\)`),
		"a field selection":      regexp.MustCompile(`return .*\.Fs\.FP\[0\]`),
		"a constant operand":     regexp.MustCompile(`unsafe\.Sizeof\(\*r\)`),
		"a pointer parameter":    regexp.MustCompile(`TBF_ctx param\(ref TBF_ctx p\)`),
	} {
		if !site.MatchString(cs) {
			t.Errorf("%s: no emitted line matches %s", name, site)
		}
	}

	// Exactly the two DECODE sites read as values; every KEEP site keeps today's emission.
	if got := len(decoded.FindAllString(cs, -1)); got != 2 {
		t.Errorf("%d GoValue reads, want exactly the 2 DECODE sites:\n%s", got, cs)
	}
}

// An ADMITTED struct holds its arrays inline, so its CLR layout is Go's and a whole-value read is a byte read: no D2.
func TestValueDecodeEmissionLeavesAnAdmittedStructAlone(t *testing.T) {
	cs := convertInlineArrayFixture(t, "small", `package small

import "unsafe"

type Small struct {
	FP [18]uint32
	N  uint32
}

var words = [19]uint32{1, 2, 3}

var state = *(*Small)(unsafe.Pointer(&words))

func load(q *Small) Small {
	r := q
	return *r
}
`)

	if strings.Contains(cs, ".GoValue()") {
		t.Errorf("an admitted struct is read as a value:\n%s", cs)
	}
}
