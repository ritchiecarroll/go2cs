// wait_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"testing"
)

// The wait-twin reach census's controls: a function that parks through a channel is not W; one that parks only through
// sync.WaitGroup.Wait is W (directly, and one call down); one that parks only through sync.Cond.Wait is C; one that waits
// and also sends on a channel is neither. And the real golang.org/x/sync/errgroup.(*Group).Wait (the converter module's
// own x/sync, v0.24.0) is W: its other call, g.cancel, is through a func field and is not a park. Each named function
// must also be SEEN, so no control can pass on an empty load.
func TestWaitTwinControls(t *testing.T) {
	followRuntime = false
	dir, err := filepath.Abs(filepath.Join("testdata", "fixture"))
	if err != nil {
		t.Fatal(err)
	}
	roots, err := load(dir, []string{"./..."})
	if err != nil {
		t.Fatal(err)
	}
	fixture := build(roots)

	roots, err = load("", []string{"golang.org/x/sync/errgroup"})
	if err != nil {
		t.Fatal(err)
	}
	errgroup := build(roots)

	for _, control := range []struct {
		c          *census
		name, want string
	}{
		{fixture, "e1fixture.NonGenericParker", ""},
		{fixture, "e1fixture.WaitOnly", "W"},
		{fixture, "e1fixture.WaitViaHelper", "W"},
		{fixture, "e1fixture.CondOnly", "C"},
		{fixture, "e1fixture.WaitAndSend", ""},
		{errgroup, "golang.org/x/sync/errgroup.(*Group).Wait", "W"},
	} {
		got, seen := control.c.waitClass(control.name, false)
		if !seen {
			t.Errorf("%s: not seen by the census (the load or the key spelling is broken)", control.name)
			continue
		}
		if got != control.want {
			t.Errorf("%s: class %q, want %q", control.name, got, control.want)
		}
	}

	w, _ := fixture.waitSummary("fixture", false)
	if w.W != 3 || w.C != 1 || w.WC != 0 {
		t.Errorf("fixture wait row: %+v; want W 3 (WaitOnly, waitHelper, WaitViaHelper), C 1 (CondOnly), WC 0", w)
	}

	// The locks-aside reading: CondLocked holds a *sync.Mutex around its Cond.Wait. The literal reading counts the lock as
	// another park point (not C); set aside, as the generator sets a lock aside, it reads C. WaitAndSend stays neither.
	for _, control := range []struct {
		name           string
		literal, aside string
	}{
		{"e1fixture.CondLocked", "", "C"},
		{"e1fixture.WaitLocked", "", "W"},
		{"e1fixture.WaitAndSend", "", ""},
		{"e1fixture.CondOnly", "C", "C"},
	} {
		literal, seen := fixture.waitClass(control.name, false)
		aside, _ := fixture.waitClass(control.name, true)
		if !seen {
			t.Errorf("%s: not seen by the census", control.name)
			continue
		}
		if literal != control.literal || aside != control.aside {
			t.Errorf("%s: literal %q, locks-aside %q; want %q, %q", control.name, literal, aside, control.literal, control.aside)
		}
	}
}
