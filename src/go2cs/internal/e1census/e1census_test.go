// e1census_test.go - Gbtc
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

// The census's controls, over the fixture module in testdata/fixture: a non-generic parker is NOT counted, a generic
// that parks only transitively IS counted, a method of a generic type that parks through a wait primitive IS counted,
// and a generic that only calls a non-parking function is NOT counted. Each named function must also have been seen,
// so no control can pass on an empty load.
func TestE1CensusControls(t *testing.T) {
	dir, err := filepath.Abs(filepath.Join("testdata", "fixture"))
	if err != nil {
		t.Fatal(err)
	}
	roots, err := load(dir, []string{"./..."})
	if err != nil {
		t.Fatal(err)
	}
	followRuntime = false
	c := build(roots)

	for _, control := range []struct {
		name string
		want bool
	}{
		{"e1fixture.NonGenericParker", false},
		{"e1fixture.GenericParker", true},
		{"e1fixture.(*Box).Get", true},
		{"e1fixture.GenericQuiet", false},
		{"e1fixture.GenericDynamic", false},
		{"e1fixture.GenericFinalizer", false}, // its only park is inside package runtime, not followed by default
	} {
		if !c.declared(control.name) {
			t.Errorf("%s: not seen by the census (the load or the key spelling is broken)", control.name)
			continue
		}
		if got := c.counted(control.name); got != control.want {
			t.Errorf("%s: counted=%v, want %v", control.name, got, control.want)
		}
	}

	// The generator-aligned reading does not count a lock: (*Box).Get parks only through sync.Mutex.Lock, while
	// GenericParker's path ends in a channel send.
	if !c.countedNoLocks("e1fixture.GenericParker") {
		t.Errorf("e1fixture.GenericParker: not counted without locks, want counted (its park is a chan send)")
	}
	if c.countedNoLocks("e1fixture.(*Box).Get") {
		t.Errorf("e1fixture.(*Box).Get: counted without locks, want not counted (its only park is sync.Mutex.Lock)")
	}

	r, parkers := c.summarize("fixture")
	if r.Generic != 5 || r.GenericDirect != 1 || r.GenericTransitive != 1 || r.GenericNoParkDynamic != 1 || r.GenericNoParkStatic != 2 {
		t.Errorf("fixture row: %+v; want generic 5, direct 1 (Box.Get), transitive 1 (GenericParker), dynamic 1, static 2 (GenericQuiet, GenericFinalizer)", r)
	}
	// Exported generic functions (the C# population's E1 members): GenericParker, GenericQuiet, GenericDynamic and
	// GenericFinalizer; of them GenericParker parks, in both readings. (*Box).Get is a method, so not among them.
	if r.ExportedFuncs != 4 || r.ExportedFuncParkers != 1 || r.ExportedFuncParkersNoLocks != 1 {
		t.Errorf("fixture exported generic funcs: %d, parkers %d, parkers without locks %d; want 4, 1, 1", r.ExportedFuncs, r.ExportedFuncParkers, r.ExportedFuncParkersNoLocks)
	}
	if len(parkers) != 2 {
		t.Errorf("fixture parkers: %+v; want exactly GenericParker and (*Box).Get", parkers)
	}

	// The runtime boundary, crossed: GenericFinalizer parks through runtime.SetFinalizer's finalizer goroutine.
	followRuntime = true
	defer func() { followRuntime = false }()
	if !build(roots).counted("e1fixture.GenericFinalizer") {
		t.Errorf("e1fixture.GenericFinalizer: not counted with the runtime followed, want counted (SetFinalizer starts a goroutine)")
	}
}
