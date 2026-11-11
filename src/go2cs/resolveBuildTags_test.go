// resolveBuildTags_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// resolveBuildTags_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"reflect"
	"testing"
)

// TestResolveBuildTags guards the purego default decision. The regression it locks in: a `-tests`
// run must apply the same purego default as `-stdlib`, so the reconverted PRODUCTION sources select
// the same files the committed converted stdlib tree was built from. Before the fix, `-tests` left
// build tags empty and converted asm variants (crypto/subtle's xor_amd64.go) alongside the pure-Go
// ones, producing a CS0111 duplicate-member collision and a production .cs that diverged from the
// committed purego emission.
func TestResolveBuildTags(t *testing.T) {
	explicit := []string{"foo", "bar"}

	// -recurse -tests is the stdlib default followed by the module default with the tags they share dropped
	// (purego is in both), so the oracle and the conversion carry one set with no repeated tag.
	moduleTestTags := []string{"purego", "math_big_pure_go", "safe", "noasm"}

	tests := []struct {
		name          string
		convertStdLib bool
		convertTests  bool
		recurse       bool
		moduleSafeTag bool
		tagsExplicit  bool
		explicit      []string
		want          []string
	}{
		{"stdlib default -> purego", true, false, false, true, false, nil, defaultStdLibBuildTags},
		{"tests default -> purego", false, true, false, true, false, nil, defaultStdLibBuildTags},
		{"stdlib+tests default -> purego", true, true, false, true, false, nil, defaultStdLibBuildTags},
		{"tests with explicit -tags -> explicit honored", false, true, false, true, true, explicit, explicit},
		{"stdlib with explicit -tags -> explicit honored", true, false, false, true, true, explicit, explicit},
		{"tests with -tags= (explicit clear) -> empty", false, true, false, true, true, nil, nil},
		{"neither (single-file) -> tag-neutral", false, false, false, true, false, explicit, explicit},
		{"recurse default -> safe,purego,noasm", false, false, true, true, false, nil, []string{"safe", "purego", "noasm"}},
		{"recurse+tests default -> purego, math_big_pure_go, safe, noasm", false, true, true, true, false, nil, moduleTestTags},
		{"recurse with -module-safe-tag=false -> tag-neutral", false, false, true, false, false, nil, nil},
		{"recurse+tests with -module-safe-tag=false -> purego only", false, true, true, false, false, nil, defaultStdLibBuildTags},
		{"recurse with explicit -tags -> explicit honored", false, true, true, true, true, explicit, explicit},
		{"recurse (no -tests) with explicit -tags safe -> safe only", false, false, true, true, true, []string{"safe"}, []string{"safe"}},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got := resolveBuildTags(tt.convertStdLib, tt.convertTests, tt.recurse, tt.moduleSafeTag, tt.tagsExplicit, tt.explicit)
			if !reflect.DeepEqual(got, tt.want) {
				t.Fatalf("resolveBuildTags(%v, %v, %v, %v, %v, %v) = %v, want %v",
					tt.convertStdLib, tt.convertTests, tt.recurse, tt.moduleSafeTag, tt.tagsExplicit, tt.explicit, got, tt.want)
			}
		})
	}
}

// TestDefaultModuleBuildTagsContent pins the module default to exactly `safe`, `purego` and `noasm`, in
// that order. `safe` selects a library's fallback for unsafe pointer arithmetic into Go's runtime
// internals; `purego` and `noasm` select a library's pure-Go fallback for its assembly, which converts
// only to stubs that throw (klauspost/cpuid/v2 gates its asm detector on `!noasm`). `appengine` is the
// tempting fourth spelling and must stay out: libraries switch behavior beyond unsafe on it (logrus
// selects a different terminal check), so adding it would change what a converted module does rather
// than only which fallback it takes.
func TestDefaultModuleBuildTagsContent(t *testing.T) {
	if want := []string{"safe", "purego", "noasm"}; !reflect.DeepEqual(defaultModuleBuildTags, want) {
		t.Fatalf("defaultModuleBuildTags = %v, want %v", defaultModuleBuildTags, want)
	}
}

// TestDefaultStdLibBuildTagsContent pins the CONTENT of the default tag set, which the table above
// cannot: every default case there compares against defaultStdLibBuildTags itself, so dropping a tag
// keeps the table green while silently changing which stdlib source files the corpus is built from.
//
// `math_big_pure_go` is load-bearing for the same reason `purego` is: it selects math/big's
// arith_decl_pure.go (pure-Go forwarders to the `_g` implementations) over arith_decl.go, whose eight
// declarations are bodyless because their bodies are arith_$GOARCH.s assembly. Without the tag every
// big.Int / big.Float / big.Rat arithmetic path converts to a throwing partial stub — code that
// compiles clean and panics on first use.
func TestDefaultStdLibBuildTagsContent(t *testing.T) {
	want := map[string]bool{"purego": false, "math_big_pure_go": false}

	for _, tag := range defaultStdLibBuildTags {
		if _, ok := want[tag]; !ok {
			t.Errorf("defaultStdLibBuildTags carries an undocumented tag %q — document why the corpus needs it", tag)
			continue
		}
		want[tag] = true
	}

	for tag, seen := range want {
		if !seen {
			t.Errorf("defaultStdLibBuildTags is missing %q; the stdlib corpus would select the assembly-backed variant", tag)
		}
	}
}
