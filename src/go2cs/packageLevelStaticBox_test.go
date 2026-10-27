// packageLevelStaticBox_test.go - Gbtc
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

// TestPackageLevelBoxIsAStaticBox pins the WaitGroup bubble sync's emission (BOARD 2026-10-15). Go leaves a
// package-level WaitGroup Unbubbled in a synctest bubble, because a global is outside every heap span, and golib can
// answer that only if the global's box says so: the package-level declaration constructs golib's StaticBox<T>, never
// the heap kind. A local that escapes stays a heap box.
func TestPackageLevelBoxIsAStaticBox(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	files, err := convertLazyVarModule(t, map[string]string{
		"main.go": `package main

import "sync"

var wg sync.WaitGroup

func local() *int {
	x := 1
	return &x
}

func main() {
	wg.Add(1)
	wg.Done()
	println(*local())
}
`,
	})

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	main := files["main.cs"]

	if want := "internal static readonly ж<sync.WaitGroup> Ꮡwg = new StaticBox<sync.WaitGroup>("; !strings.Contains(main, want) {
		t.Errorf("the package-level WaitGroup's box is not a StaticBox; want %q in:\n%s", want, main)
	}

	if strings.Contains(main, "new StandardBox<sync.WaitGroup>") {
		t.Errorf("the package-level WaitGroup is boxed as heap storage:\n%s", main)
	}

	if strings.Contains(main, "new StaticBox<nint>") {
		t.Errorf("an escaping local is boxed as package-level storage:\n%s", main)
	}
}
