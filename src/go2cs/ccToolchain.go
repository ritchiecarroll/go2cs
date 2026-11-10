// ccToolchain.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"os/exec"
	"runtime"
	"strings"
)

// THE C COMPILER FOR -cgo (seat Z; docs/phase4/DESIGN-cgo-zig-provision.md, PLAN-cgo-interop §5.1)
//
// A -cgo load runs cmd/cgo, which needs a C compiler on the converting machine. The owner's default is to
// provision a pinned one on demand, with overrides. The first that applies wins:
//  1. -cc CMD, as given (it may carry arguments, as `zig cc` does). It never downloads.
//  2. CC from the environment, as the go command reads it.
//  3. The platform's default compiler on PATH: clang on darwin, gcc elsewhere, the names cmd/go defaults to.
//  4. The pinned toolchain, downloaded into the user cache on first use.
//
// Two reserved -cc values steer it: `pinned` goes straight to step 4 (the reproducible choice), and `host` refuses
// step 4. -cc-download=false turns step 4 into a refusal that names the other three. No step ever falls back to a
// cgo-off load: that is the silent failure PLAN-cgo-interop §2 Finding 1 measured, and -cgo exists to refuse it.

const (
	ccFlagPinned = "pinned"
	ccFlagHost   = "host"
)

// ccChoice is a resolved compiler: the CC value, the extra environment its load carries, and which step chose it.
type ccChoice struct {
	cc  string
	env []string
	how string
}

// ccPinnedToolchain provides step 4. cached reports an install already in the user cache, which is used even under
// -cc-download=false since nothing is fetched; install downloads, verifies and unpacks it.
type ccPinnedToolchain interface {
	platform() string
	cached() (cc string, env []string, ok bool)
	install() (cc string, env []string, err error)
}

type ccResolver struct {
	flag     string // -cc: "", ccFlagPinned, ccFlagHost, or a compiler command
	download bool   // -cc-download
	goos     string
	getenv   func(string) string
	lookPath func(string) (string, error)
	pinned   ccPinnedToolchain // nil when this build or platform has no pin
}

func newCCResolver(flag string, download bool) *ccResolver {
	return &ccResolver{flag: strings.TrimSpace(flag), download: download, goos: runtime.GOOS, getenv: os.Getenv, lookPath: exec.LookPath, pinned: newPinnedCC()}
}

// platformDefaultCC is the compiler cmd/go uses when CC is unset (cmd/go/internal/cfg/zdefaultcc.go).
func platformDefaultCC(goos string) string {
	if goos == "darwin" || goos == "ios" {
		return "clang"
	}

	return "gcc"
}

// newPinnedCC returns this platform's pinned C toolchain, the zig provisioner (ccZigProvision.go), or nil when this
// go2cs carries no pin for the platform. The nil is returned bare: a nil *zigProvisioner inside the interface would
// read as a pin.
func newPinnedCC() ccPinnedToolchain {
	if z := newZigProvisioner(); z != nil {
		return z
	}

	return nil
}

// checkCCFlags refuses -cc and -cc-download without -cgo: they steer the -cgo load's compiler and nothing else.
func checkCCFlags(cgo bool, cc string, download bool) error {
	if !cgo && (strings.TrimSpace(cc) != "" || !download) {
		return fmt.Errorf("-cc and -cc-download choose the C compiler of a -cgo load; add -cgo or drop them")
	}

	return nil
}

func (r *ccResolver) resolve() (ccChoice, error) {
	switch r.flag {
	case "", ccFlagPinned, ccFlagHost:
	default:
		return ccChoice{cc: r.flag, env: []string{"CC=" + r.flag}, how: "-cc"}, nil
	}

	if r.flag != ccFlagPinned {
		if cc := strings.TrimSpace(r.getenv("CC")); cc != "" {
			return ccChoice{cc: cc, env: []string{"CC=" + cc}, how: "CC from the environment"}, nil
		}

		name := platformDefaultCC(r.goos)

		if _, err := r.lookPath(name); err == nil {
			return ccChoice{cc: name, env: []string{"CC=" + name}, how: name + " on PATH"}, nil
		}
	}

	hostSteps := fmt.Sprintf("-cc CMD, CC in the environment, or %s on PATH", platformDefaultCC(r.goos))

	if r.flag == ccFlagHost {
		return ccChoice{}, fmt.Errorf("-cgo needs a C compiler and -cc host refuses the pinned one: none of %s resolved", hostSteps)
	}

	if r.pinned == nil {
		return ccChoice{}, fmt.Errorf("-cgo needs a C compiler: none of %s resolved, and this go2cs has no pinned C toolchain for %s/%s",
			hostSteps, r.goos, runtime.GOARCH)
	}

	if cc, env, ok := r.pinned.cached(); ok {
		return ccChoice{cc: cc, env: append([]string{"CC=" + cc}, env...), how: "the pinned toolchain"}, nil
	}

	if !r.download {
		return ccChoice{}, fmt.Errorf("-cgo needs a C compiler: none of %s resolved, and -cc-download=false refuses to download the pinned one for %s; "+
			"install it from a copied archive with `go2cs toolchain install -cc -from FILE`", hostSteps, r.pinned.platform())
	}

	cc, env, err := r.pinned.install()
	if err != nil {
		return ccChoice{}, err
	}

	return ccChoice{cc: cc, env: append([]string{"CC=" + cc}, env...), how: "the pinned toolchain"}, nil
}
