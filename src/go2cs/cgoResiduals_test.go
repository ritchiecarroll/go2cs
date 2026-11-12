// cgoResiduals_test.go - Gbtc
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
	"regexp"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// cgo Phase 3 increment 3.0 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §15): what the focused re-check of Phase 2's
// increment 5b left open on a real host. Both tests need the host's C compiler, so they run where the real route's own
// test does (cgoRealProbeDir) and are NOT MEASURED elsewhere.

// R2: a link line that does not link is the package's own fault, and no symbol's. The capture links a probe that
// references nothing to tell the two apart, and then returned the FIRST probe's error, the one that references every
// symbol: on a linker that traces symbols, that text names each of them beside the library that is not there. The
// refusal is the baseline probe's own error, the linker's words about the link line.
func TestALinkLineThatDoesNotLinkIsRefusedWithoutNamingASymbol(t *testing.T) {
	cgoRealProbeDir(t)

	load := cgoLoad{env: append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1"), dir: t.TempDir()}
	flags := cgoLinkFlags{libraries: []string{"m", "go2csnosuchlib"}}
	_, _, refusals, err := cgoCapture(load, runtime.GOOS, runtime.GOARCH, flags, []string{"cos", "frexp"}, nil)

	if err == nil {
		t.Fatalf("a link line naming a library that is not there was captured; refusals: %v", refusals)
	}

	text := err.Error()
	t.Logf("READING %s/%s: %s", runtime.GOOS, runtime.GOARCH, text)

	if !strings.Contains(text, "go2csnosuchlib") {
		t.Errorf("the refusal does not name the library that did not link:\n%s", text)
	}

	for _, symbol := range []string{"frexp", "cos"} {
		if regexp.MustCompile(`\b` + symbol + `\b`).MatchString(text) {
			t.Errorf("the refusal of a link line names the symbol %s, which is not to blame:\n%s", symbol, text)
		}
	}
}

// R4, a READING: the verified-names rule (§11.21, M3) refuses a call whose C name cmd/cgo's own binary does not
// import. On glibc that is wider than the fixture's abs and labs: a header may inline a function at -O2, or rename it
// (strtol to an __isoc23_ symbol), and each such call is refused by name. Which ones is the C library's and the
// compiler's to decide, so this asserts only the rule's invariant, per function: the call is bound, or it is refused
// by name with nothing written. It prints the set, which docs/Limitations.md records as measured.
func TestTheVerifiedNamesRuleBindsOrRefusesEachLibcCallByName(t *testing.T) {
	probeDir := cgoRealProbeDir(t)

	if runtime.GOOS != "linux" {
		t.Skip("a reading of glibc's headers: NOT MEASURED off linux")
	}

	calls := []struct{ name, include, call string }{
		{"atoi", "stdlib.h", `int(C.atoi(text))`},
		{"atol", "stdlib.h", `int(C.atol(text))`},
		{"toupper", "ctype.h", `int(C.toupper(C.int(n)))`},
		{"tolower", "ctype.h", `int(C.tolower(C.int(n)))`},
		{"fabs", "math.h", `int(C.fabs(C.double(n)))`},
		{"strtol", "stdlib.h", `int(C.strtol(text, nil, 10))`},
	}

	notImported := regexp.MustCompile(`C\.(\w+): cmd/cgo's own binary does not import`)
	var bound, refused, other []string

	for _, call := range calls {
		source := fmt.Sprintf("package cgoreal\n\n/*\n#cgo LDFLAGS: -lm\n#include <%s>\n*/\nimport \"C\"\n\n// Call makes the one C call this package reads.\nfunc Call(text *C.char, n int) int { return %s }\n", call.include, call.call)
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{call.name + ".go": source})

		if err == nil {
			emitted := emittedProjectFiles(t, output)
			named := false

			for _, file := range emitted {
				if strings.HasSuffix(file, ".cgo.cs") && strings.Contains(readConverted(t, output, file), `EntryPoint = "`+call.name+`"`) {
					named = true
				}
			}

			if !named {
				t.Errorf("C.%s converted, and no bindings file binds it by that name: %v\n%s", call.name, emitted, stderr)
			}

			bound = append(bound, call.name)
			continue
		}

		requireNothingWritten(t, output, err, stderr)
		byName, named := false, false

		for _, line := range strings.Split(strings.ReplaceAll(stderr, "\r\n", "\n"), "\n") {
			if match := notImported.FindStringSubmatch(line); match != nil && match[1] == call.name {
				byName = true
			} else if strings.Contains(line, "C."+call.name) {
				// Refused by name for another reason (a parameter Phase 2 does not cross): its own line is the reading.
				named = true
				t.Logf("READING C.%s: %s", call.name, strings.TrimSpace(line))
			}
		}

		switch {
		case byName:
			refused = append(refused, call.name)
		case named:
			other = append(other, call.name)
		default:
			t.Errorf("C.%s is neither bound nor refused by name:\n%s", call.name, stderr)
		}
	}

	slices.Sort(bound)
	slices.Sort(refused)
	slices.Sort(other)
	toolchain, _ := cgoRealToolchainLine(t)
	t.Logf("READING linux/%s under %q: refused as not imported: %v; refused by name for another reason: %v; bound: %v", runtime.GOARCH, toolchain, refused, other, bound)
}
