// manualTypeUnwrapBox_test.go - Gbtc
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

// TestManualTypeUnwrapPassesAPointerParametersBox guards the manual-type unwrap of
// `guintptr(unsafe.Pointer(x))` (runtime proc.go's runqput). The manual type stores the referent
// itself, so the constructor must receive the BOX Go's pointer names. A pointer PARAMETER is emitted
// as a deref'd value alias beside its box `Ꮡgp`, and rendering the alias handed the constructor the
// pointed-to VALUE, which compiled only through a generated self-to-box implicit operator that boxes
// a COPY — so runnext held a copy of the caller's g where Go holds the caller's g. A pointer LOCAL
// already holds its box and is the control.
func TestManualTypeUnwrapPassesAPointerParametersBox(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	displacedTypeFixture(t, "example.com/app", "guintptr")

	lines := convertWithComments(t, `package main

import "unsafe"

type g struct{ id int }

type guintptr uintptr

type holder struct{ next guintptr }

func put(h *holder, gp *g) {
	h.next = guintptr(unsafe.Pointer(gp))
	println(gp.id)
}

func local(h *holder) {
	gp := &g{id: 1}
	h.next = guintptr(unsafe.Pointer(gp))
}

func main() {
	h := &holder{}
	put(h, &g{id: 2})
	local(h)
}
`)

	var sites []string

	for _, line := range lines {
		if strings.Contains(line, "new guintptr(") {
			sites = append(sites, strings.TrimSpace(line))
		}
	}

	want := []string{"h.next = new guintptr(Ꮡgp);", "h.next = new guintptr(gp);"}

	if strings.Join(sites, "\n") != strings.Join(want, "\n") {
		t.Fatalf("manual-type unwrap sites:\n got: %q\nwant: %q", sites, want)
	}
}
