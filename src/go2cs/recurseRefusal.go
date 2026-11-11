// recurseRefusal.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"fmt"
	"log"
	"strings"
)

// A package refused at a pre-emission gate (GO2CS2001 cgo, GO2CS2002, GO2CS2003, GO2CS2005) gets no project. A
// single-package conversion has nothing else to convert, so the refusal is fatal there, as it always was. Under
// -recurse one refused package used to end the whole run, and every package queued behind it went unconverted. So
// -recurse skips it instead: its diagnostics print as errors, the rest of the module converts, and the run ends
// nonzero naming the refused package and each converted package that imports it (those cannot build until it
// converts). A code downgraded by -warn never reaches a gate, so its package is not refused at all. A package that
// fails to load or panics was already skipped; it now makes the run end nonzero too, where it used to exit 0.
// -tests -recurse keeps both behaviours it had: a refusal is fatal there, and a failed package does not stop it.

// packageRefusal is a pre-emission refusal returned to the -recurse driver instead of ending the process.
type packageRefusal struct {
	code string // the refusal's diagnostic code
	err  error  // the gate's refusal, whose text names the package and the sites
}

func (r *packageRefusal) Error() string { return fmt.Sprintf("Refusing to convert: %s", r.err) }

func (r *packageRefusal) Unwrap() error { return r.err }

// refusePackage ends a single-package conversion with the refusal, as every gate always did. Under -recurse
// (options.skipRefusedPackages) it returns the refusal instead, for convertAll to record and skip.
func refusePackage(options Options, code string, err error) error {
	if !options.skipRefusedPackages {
		log.Fatalf("Refusing to convert: %s\n", err)
	}

	return &packageRefusal{code: code, err: err}
}

// refusedPackage is one package a -recurse run refused.
type refusedPackage struct {
	path string
	code string
}

// reportRefusedPackages prints the refused packages and then each converted package that imports one directly. Both
// lists are in conversion (dependency) order. A package that imports a refused one only indirectly is not named: it
// fails to build through the direct importer that is named.
func (m *ModuleConverter) reportRefusedPackages(refused []refusedPackage) {
	if len(refused) == 0 {
		return
	}

	refusedCode := make(map[string]string, len(refused))

	fmt.Printf("\nRefused %d %s (no project written):\n", len(refused), plural(len(refused), "package", "packages"))

	for _, r := range refused {
		refusedCode[r.path] = r.code
		fmt.Printf("  %s  %s\n", r.path, r.code)
	}

	var importers []string

	for _, pkgPath := range m.graph.sortedQueue {
		if _, converted := m.convertedCsproj[pkgPath]; !converted {
			continue
		}

		var imports []string

		for _, dep := range m.graph.packages[pkgPath].Dependencies {
			if _, isRefused := refusedCode[dep]; isRefused {
				imports = append(imports, dep)
			}
		}

		if len(imports) > 0 {
			importers = append(importers, fmt.Sprintf("  %s  imports %s", pkgPath, strings.Join(imports, ", ")))
		}
	}

	if len(importers) > 0 {
		fmt.Printf("%d converted %s a refused package and will not build until it converts:\n%s\n", len(importers),
			plural(len(importers), "package imports", "packages import"), strings.Join(importers, "\n"))
	}
}

// runError is ConvertModule's error when a package was refused or failed (did not load, or panicked), raised after
// the rest of the run's output is written; nil when every package converted. -tests -recurse (includeTestClosure)
// goes on past a failed package, as it always did: its phase A converts the closure for phase B, and a failed
// third-party package there is not a reason to stop (ruled out of scope with that driver's refusals).
func (m *ModuleConverter) runError() error {
	var parts []string

	if len(m.refused) > 0 {
		paths := make([]string, len(m.refused))

		for i, r := range m.refused {
			paths[i] = r.path
		}

		parts = append(parts, fmt.Sprintf("%d %s refused: %s", len(m.refused), plural(len(m.refused), "package", "packages"), strings.Join(paths, ", ")))
	}

	if len(m.failed) > 0 && !m.includeTestClosure {
		parts = append(parts, fmt.Sprintf("%d %s failed: %s", len(m.failed), plural(len(m.failed), "package", "packages"), strings.Join(m.failed, ", ")))
	}

	if len(parts) == 0 {
		return nil
	}

	return errors.New(strings.Join(parts, "; "))
}

func plural(n int, one, many string) string {
	if n == 1 {
		return one
	}

	return many
}
