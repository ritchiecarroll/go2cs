// manualTypeReceiverRefusal_test.go - Gbtc
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

// The manual-type unwrap of `guintptr(unsafe.Pointer(x))` must hand the manual type the BOX Go's
// pointer names. A pointer PARAMETER renders its box (TestManualTypeUnwrapPassesAPointerParametersBox);
// a pointer RECEIVER is a deref'd value alias with no box to pass, so the conversion is refused by name
// (GO2CS2003) before any file is written, rather than emitted with the value. runtime's one receiver
// form, g.guintptr, is hand-owned (`this ж<g>`), which a displaced method's exemption keeps unrefused.
const manualTypeReceiverFixture = `package rcv

import "unsafe"

type g struct{ id int }

type guintptr uintptr

func (gp *g) viaReceiver() guintptr {
	return guintptr(unsafe.Pointer(gp)) // REFUSED
}

func (gp *g) handOwned() guintptr {
	return guintptr(unsafe.Pointer(gp))
}

func (gp *g) shadowed() guintptr {
	gp = &g{}
	inner := gp
	return guintptr(unsafe.Pointer(inner))
}

func viaParameter(gp *g) guintptr {
	return guintptr(unsafe.Pointer(gp))
}

func (gp g) valueReceiver() uintptr {
	return uintptr(unsafe.Pointer(&gp))
}
`

func TestManualTypeReceiverOperandIsRefusedBySite(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	displacedTypeFixture(t, "example/rcv", "guintptr")
	displacedFixture(t, "example/rcv", "g.handOwned")

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/rcv\n\ngo 1.23\n",
		"rcv.go": manualTypeReceiverFixture,
	})

	refusal := refuseManualTypeReceiverOperands(loadProductionForDir(t, dir), "linux")

	if refusal == nil {
		t.Fatal("no conversion was refused; viaReceiver passes its pointer RECEIVER to the manual type guintptr")
	}

	line := 0

	for i, text := range strings.Split(manualTypeReceiverFixture, "\n") {
		if strings.Contains(text, "// REFUSED") {
			line = i + 1
		}
	}

	if len(refusal.sites) != 1 || refusal.sites[0].position.Line != line {
		t.Fatalf("want exactly the viaReceiver site at line %d; got %+v", line, refusal.sites)
	}

	if text := refusal.sites[0].text; !strings.Contains(text, "guintptr") || !strings.Contains(text, "gp") {
		t.Errorf("the refusal names neither the manual type nor the receiver: %q", text)
	}
}
