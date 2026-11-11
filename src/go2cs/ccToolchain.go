// ccToolchain.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"context"
	"fmt"
	"os"
	"os/exec"
	"runtime"
	"strings"
	"sync"
	"time"
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
	// ccFlagPinnedPrefix names one pinned toolchain explicitly: pinned:zig or pinned:llvm-mingw. A bare `pinned` takes
	// the host OS's default (defaultCCToolchain).
	ccFlagPinnedPrefix = "pinned:"
)

// ccFlags holds -cc and -cc-download as main parsed them; resolveCToolchain reads them.
var ccFlags = struct {
	cmd      string
	download bool
}{download: true}

// ccResolution is the process's one resolution: a download happens at most once, and every -cgo load (and increment
// 4's load-name capture) links with the same compiler and environment.
var ccResolution struct {
	once    sync.Once
	cc      string
	env     []string
	version string
	err     error
}

// resolveCToolchain returns the C compiler for a -cgo load: the CC value (already quoted the way cmd/go splits CC), the
// extra environment the load carries (nil for a host compiler; zig's caches for the pin), one provenance line for the
// log, and a refusal naming every step it tried. It resolves once per process and is safe to call from anywhere.
//
// A host compiler is not reproducible (PLAN-cgo-interop §5.2): every byte-comparing gate (converter-cgo, G1/G2) passes
// -cc pinned.
func resolveCToolchain() (cc string, env []string, version string, err error) {
	ccResolution.once.Do(func() {
		r := newCCResolver(ccFlags.cmd, ccFlags.download)

		choice, err := r.resolve()
		if err != nil {
			ccResolution.err = err
			return
		}

		ccResolution.cc, ccResolution.version = choice.cc, r.version(choice)

		// choice.env opens with the CC= entry the caller writes itself; what follows is the extra environment.
		if len(choice.env) > 1 {
			ccResolution.env = choice.env[1:]
		}
	})

	return ccResolution.cc, ccResolution.env, ccResolution.version, ccResolution.err
}

// resetCToolchainResolution forgets the process's resolution, for tests that resolve under different conditions.
func resetCToolchainResolution() {
	ccResolution.once = sync.Once{}
	ccResolution.cc, ccResolution.env, ccResolution.version, ccResolution.err = "", nil, "", nil
}

// version is the provenance line: the pin's release and hash prefix, or a host compiler's first `--version` line.
func (r *ccResolver) version(choice ccChoice) string {
	if z, ok := r.pinned.(*pinnedProvisioner); ok && choice.how == "the pinned toolchain" {
		return fmt.Sprintf("%s (pinned, sha256 %s)", z.label(), z.pin.sha256[:8])
	}

	fields := splitCC(choice.cc)

	if r.runVersion == nil || len(fields) == 0 {
		return fmt.Sprintf("%s (%s)", choice.cc, choice.how)
	}

	line, err := r.runVersion(fields[0], append(fields[1:], "--version")...)
	if err != nil || line == "" {
		return fmt.Sprintf("%s (%s; --version: unavailable)", choice.cc, choice.how)
	}

	return fmt.Sprintf("%s (%s)", line, choice.how)
}

// splitCC mirrors cmd/go's quoted.Split (cmd/internal/quoted), which the -cgo load applies to CC: whitespace
// separates fields, and a field that opens with a quote runs to the matching quote. An unclosed quote takes the rest.
func splitCC(s string) []string {
	var fields []string

	for {
		s = strings.TrimLeft(s, " \t\n\r")

		if s == "" {
			return fields
		}

		if quote := s[0]; quote == '\'' || quote == '"' {
			end := strings.IndexByte(s[1:], quote)

			if end < 0 {
				return append(fields, s[1:])
			}

			fields = append(fields, s[1:1+end])
			s = s[2+end:]

			continue
		}

		end := strings.IndexAny(s, " \t\n\r")

		if end < 0 {
			end = len(s)
		}

		fields = append(fields, s[:end])
		s = s[end:]
	}
}

// firstVersionLine runs a compiler's --version and returns its first line.
func firstVersionLine(name string, args ...string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	out, err := exec.CommandContext(ctx, name, args...).Output()
	if err != nil {
		return "", err
	}

	line, _, _ := strings.Cut(string(out), "\n")

	return strings.TrimSpace(line), nil
}

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
	flag     string // -cc: "", ccFlagPinned, pinned:<name>, ccFlagHost, or a compiler command
	download bool   // -cc-download
	goos     string
	getenv   func(string) string
	lookPath func(string) (string, error)
	pinned   ccPinnedToolchain // nil when this build or platform has no pin
	// runVersion runs a host compiler's --version for the provenance line; nil leaves the line to the command alone.
	runVersion func(name string, args ...string) (string, error)
}

func newCCResolver(flag string, download bool) *ccResolver {
	return &ccResolver{flag: strings.TrimSpace(flag), download: download, goos: runtime.GOOS, getenv: os.Getenv, lookPath: exec.LookPath, pinned: newPinnedCC(pinnedToolchainName(flag)), runVersion: firstVersionLine}
}

// platformDefaultCC is the compiler cmd/go uses when CC is unset (cmd/go/internal/cfg/zdefaultcc.go).
func platformDefaultCC(goos string) string {
	if goos == "darwin" || goos == "ios" {
		return "clang"
	}

	return "gcc"
}

// pinnedToolchainName is the toolchain a -cc value names: the one after pinned:, or "" for the host OS's default.
func pinnedToolchainName(flag string) string {
	name, _ := strings.CutPrefix(strings.TrimSpace(flag), ccFlagPinnedPrefix)

	if name == strings.TrimSpace(flag) {
		return ""
	}

	return name
}

// newPinnedCC returns this platform's pinned C toolchain (ccPinnedToolchain.go): the named one, or for "" the host OS's
// default, llvm-mingw on windows and zig elsewhere; nil when this go2cs carries no pin of it for the platform. The nil is
// returned bare: a nil *pinnedProvisioner inside the interface would read as a pin.
func newPinnedCC(name string) ccPinnedToolchain {
	if z := newPinnedProvisioner(name); z != nil {
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
	pinnedOnly := r.flag == ccFlagPinned || strings.HasPrefix(r.flag, ccFlagPinnedPrefix)

	switch {
	case r.flag == "" || r.flag == ccFlagHost:
	case pinnedOnly:
		if name := pinnedToolchainName(r.flag); r.flag != ccFlagPinned && name != ccToolchainZig && name != ccToolchainLLVMMingw {
			return ccChoice{}, fmt.Errorf("-cc %s names no pinned C toolchain; the pinned ones are pinned:%s and pinned:%s, and `pinned` takes this OS's default",
				r.flag, ccToolchainZig, ccToolchainLLVMMingw)
		}
	default:
		return ccChoice{cc: r.flag, env: []string{"CC=" + r.flag}, how: "-cc"}, nil
	}

	if !pinnedOnly {
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
		name := pinnedToolchainName(r.flag)

		if name == "" {
			name = defaultCCToolchain(r.goos)
		}

		return ccChoice{}, fmt.Errorf("-cgo needs a C compiler: none of %s resolved, and this go2cs has no pinned C toolchain (%s) for %s/%s",
			hostSteps, name, r.goos, runtime.GOARCH)
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
