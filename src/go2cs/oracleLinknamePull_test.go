// oracleLinknamePull_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// oracleLinknamePull_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"testing"
	"time"
)

// linknamePullFixture is the shape github.com/remyoudompheng/bigfft has, cut to one symbol so the arm
// needs no network: a package that pulls math/big's subVV through //go:linkname. math/big pushes
// subVV (and addVV, shlVU, mulAddVWW, addMulVVW) only from arith_decl.go, which is
// `//go:build !math_big_pure_go`. Under that tag the pull has nothing to bind to and `go test` fails
// at link time: `invalid reference to math/big.subVV`. Every module reaching bigfft
// (modernc.org/mathutil, hence modernc.org/sqlite) reaches this.
var linknamePullFixture = map[string]string{
	"go.mod": "module example.test/bigpull\n\ngo 1.23\n",
	"pull.go": `package bigpull

import (
	"math/big"
	_ "unsafe"
)

//go:linkname subVV math/big.subVV
func subVV(z, x, y []big.Word) (c big.Word)

func Sub(x, y []big.Word) ([]big.Word, big.Word) {
	z := make([]big.Word, len(x))
	c := subVV(z, x, y)
	return z, c
}
`,
	"pull_test.go": `package bigpull

import (
	"math/big"
	"testing"
)

func TestSub(t *testing.T) {
	z, c := Sub([]big.Word{5, 0}, []big.Word{3, 1})
	if z[0] != 2 || c != 1 {
		t.Fatalf("got %v carry %d", z, c)
	}
}
`,
}

// TestOracleBuildsALinknamePullOfMathBig runs the -tests ORACLE's own command line, for a -recurse
// module row, over a package that pulls a math/big symbol by linkname. The oracle must build it: a
// module's test that Go itself cannot link compares nothing, and every verdict in the row reads as
// Go="" against the converted side.
func TestOracleBuildsALinknamePullOfMathBig(t *testing.T) {
	if testing.Short() {
		t.Skip("runs go test on a fixture module")
	}
	if _, err := exec.LookPath("go"); err != nil {
		t.Skip("no go on PATH")
	}

	dir := t.TempDir()
	for name, text := range linknamePullFixture {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(text), 0o644); err != nil {
			t.Fatal(err)
		}
	}
	t.Setenv("GOWORK", "off")
	t.Setenv("GOFLAGS", "")

	// resolveBuildTags' own answer for `-recurse -tests` with the module tags on, the row shape a
	// bigfft-reaching module is validated under. Not a spelled-out tag list: the arm must follow the
	// converter's defaults wherever they move.
	options := Options{
		testTimeout: 2 * time.Minute,
		buildTags:   resolveBuildTags(false, true, true, true, false, nil),
	}

	args := oracleTestArgs(options, "")
	output, err := runCommandWithTimeout(3*time.Minute, dir, options, "go", args...)
	if failure := oracleBuildFailure(output); failure != "" || err != nil {
		t.Fatalf("the oracle cannot build a module that pulls math/big by linkname.\n"+
			"  oracle args : %v\n  build line  : %s\n  exit        : %v\n"+
			"  Every verdict of such a row reads Go=\"\" against the converted side.", args, failure, err)
	}
}
