// stackAppendOption.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The -stack-append option: whether a conversion emits Go's stack-append backing store (stackAppendHints.go).
//
// The hint reproduces what Go 1.25+ makes observable about a non-escaping append's 32-byte stack buffer, cap()
// after the first appends and the allocation count testing.AllocsPerRun reads. It is a fidelity feature, not a
// speed one: golib's buffer is a heap array, so the hint removes no allocation, and reading the sites costs one
// `go build -gcflags=-m` per package and target into the probe cache. The owner's ruling (2026-11-24): on by
// default only where a converted test compares against Go's own (-stdlib, whose corpus the banked rows test, and
// -tests), off for plain and -recurse user conversions, with an explicit opt-in either way.

package main

import (
	"fmt"
	"strings"
)

// stackAppendMode is -stack-append's value.
type stackAppendMode int

const (
	// stackAppendAuto is the default: on for -stdlib and -tests, off otherwise.
	stackAppendAuto stackAppendMode = iota
	stackAppendOn
	stackAppendOff
)

// parseStackAppendMode reads -stack-append: on, off or auto (case-insensitive). Anything else is an error.
func parseStackAppendMode(value string) (stackAppendMode, error) {
	switch strings.ToLower(strings.TrimSpace(value)) {
	case "", "auto":
		return stackAppendAuto, nil
	case "on":
		return stackAppendOn, nil
	case "off":
		return stackAppendOff, nil
	}

	return stackAppendAuto, fmt.Errorf("-stack-append: %q is not on, off or auto", value)
}

// stackAppendEnabled resolves the mode for a conversion. With the hint off no append site reads the escape probe,
// so no `go build -gcflags=-m` runs and the probe cache is never created: every append is plain `append(x, v)`.
func stackAppendEnabled(options Options) bool {
	switch options.stackAppend {
	case stackAppendOn:
		return true
	case stackAppendOff:
		return false
	}

	return options.convertStdLib || options.convertTests
}
