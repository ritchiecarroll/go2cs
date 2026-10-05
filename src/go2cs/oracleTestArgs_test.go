// oracleTestArgs_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// oracleTestArgs_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"slices"
	"strings"
	"testing"
	"time"
)

// TestOracleTestArgsCarryResolvedBuildTags guards the ORACLE HALF of the corpus flavour.
//
// The defect it locks out, measured on crypto/internal/fips140/nistec at 1.24.13: the conversion
// side loads packages under resolveBuildTags' answer (the corpus defaults purego,math_big_pure_go
// for every -stdlib/-tests run, whether or not -tags was passed), while this oracle ran `go test`
// with NO tags. The two sides therefore selected DIFFERENT FILE SETS -- p256_table_test.go is
// `//go:build ... || purego` and is selected only when tagged -- so the converted side ran and
// passed a test the oracle never compiled, and the comparison reported 44 entries of
// Go="" C#="pass". That reads as a corpus divergence and is nothing of the kind.
//
// The asymmetry was UNCONDITIONAL, not opt-in: commandLineOptions.go's resolveBuildTags applies the
// corpus defaults when -tags is absent, so omitting the flag did not avoid it.
func TestOracleTestArgsCarryResolvedBuildTags(t *testing.T) {
	// resolveBuildTags' OWN answer for a -tests run with no explicit -tags -- not a copy of the
	// expected tag list. A test that spelled the tags itself would go green against a converter
	// whose defaults had drifted, which is the failure mode the corpus cannot afford.
	resolved := resolveBuildTags(false, true, false, false, false, nil)
	if len(resolved) == 0 {
		t.Fatalf("precondition: a -tests run resolves to no build tags; this arm would be vacuous")
	}

	options := Options{testTimeout: 2 * time.Minute, buildTags: resolved}
	args := oracleTestArgs(options, "")

	want := "-tags=" + strings.Join(resolved, ",")
	if !slices.Contains(args, want) {
		t.Fatalf("the go test oracle does not carry the resolved build tags.\n"+
			"  want argument: %s\n  oracle args  : %v\n"+
			"  Every tag-gated test in every row reads as a false divergence without it.", want, args)
	}

	// The SAME renderer the conversion side uses, not a second spelling of it. conversionDriver,
	// stdLibConverter and this file's own packages.Load calls all take loaderBuildFlags(); if the
	// oracle rendered its own, the two could drift apart silently and the comparison would again be
	// measuring two different questions.
	loader := options.loaderBuildFlags()
	if len(loader) != 1 || loader[0] != want {
		t.Fatalf("oracle and loader renderings disagree: loader %v, oracle wants %s", loader, want)
	}

	// go test takes flags BEFORE the package argument; a -tags after the trailing "." is passed to
	// the compiled test binary instead and silently selects nothing.
	tagAt := slices.Index(args, want)
	dotAt := slices.Index(args, ".")
	if dotAt < 0 {
		t.Fatalf("oracle args carry no package argument: %v", args)
	}
	if tagAt > dotAt {
		t.Fatalf("-tags appears AFTER the package argument (%d > %d): %v", tagAt, dotAt, args)
	}
}

// TestOracleTestArgsUntaggedRunIsUnchanged is the zero arm. A conversion that resolves to no tags
// (a -recurse or single-file run, which stay tag-neutral by design) must produce the command line
// it produced before this fix existed -- no empty `-tags=` flag, which go test reads as a request
// for the tag set containing the empty string.
func TestOracleTestArgsUntaggedRunIsUnchanged(t *testing.T) {
	options := Options{testTimeout: 2 * time.Minute}
	args := oracleTestArgs(options, "")

	for _, a := range args {
		if strings.HasPrefix(a, "-tags") {
			t.Fatalf("an untagged run emitted a tag flag %q: %v", a, args)
		}
	}
}

// TestOracleTestArgsPreservesFilterAndSkip holds the two arguments that were already handed to both
// sides verbatim, so the extraction that made this list testable cannot have dropped one.
func TestOracleTestArgsPreservesFilterAndSkip(t *testing.T) {
	options := Options{testTimeout: 90 * time.Second, testFilter: "TestFoo", buildTags: []string{"purego"}}
	args := oracleTestArgs(options, "TestBar/sub")

	for _, want := range [][2]string{{"-run", "TestFoo"}, {"-skip", "TestBar/sub"}, {"-timeout", "1m30s"}} {
		i := slices.Index(args, want[0])
		if i < 0 || i+1 >= len(args) || args[i+1] != want[1] {
			t.Fatalf("oracle args lost %s %s: %v", want[0], want[1], args)
		}
	}
}
