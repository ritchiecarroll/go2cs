// sanitization_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// TestReplaceInvalidIdentifierChars covers the import-path characters (hyphen, tilde) that are legal
// in a Go path element but illegal in a C# identifier; dots and valid characters are preserved.
func TestReplaceInvalidIdentifierChars(t *testing.T) {
	cases := map[string]string{
		"go-cmp":      "go_cmp",
		"go-isatty":   "go_isatty",
		"foo~bar":     "foo_bar",
		"a-b~c":       "a_b_c",
		"clean":       "clean",
		"github.com":  "github.com", // dot (separator) preserved
		"under_score": "under_score",
	}

	for in, want := range cases {
		if got := replaceInvalidIdentifierChars(in); got != want {
			t.Errorf("replaceInvalidIdentifierChars(%q) = %q, want %q", in, got, want)
		}
	}
}

// TestGetCoreSanitizedIdentifierPaths locks the namespace-segment sanitization for real-world import
// paths: a hyphen in any segment becomes an underscore, and a keyword embedded after a dot in a
// segment (the `in` of gopkg.in) is @-escaped. Stdlib-style segments are unchanged.
func TestGetCoreSanitizedIdentifierPaths(t *testing.T) {
	cases := map[string]string{
		"go-cmp":        "go_cmp",        // intermediate-segment hyphen (github.com/google/go-cmp/...)
		"go-sql-driver": "go_sql_driver", // multiple hyphens
		"gopkg.in":      "gopkg.@in",     // embedded C# keyword after a dot -> gopkg.@in
		"github.com":    "github.com",    // no keyword sub-token
		"internal":      "@internal",     // `internal` is a C# keyword -> escaped (stdlib internal/abi)
		"fmt":           "fmt",
	}

	for in, want := range cases {
		if got := getCoreSanitizedIdentifier(in); got != want {
			t.Errorf("getCoreSanitizedIdentifier(%q) = %q, want %q", in, got, want)
		}
	}
}

// TestGetSanitizedImportKeywordSegments is the CONSUMER half of the same invariant, and the reason
// issue #33's gopkg.in dependency converted but did not compile: getCoreSanitizedIdentifier (which
// composes the dependency's OWN `namespace go.gopkg.@in;`) has always split a path element on its
// dots, while getSanitizedImport — which every importer's `using yaml = …;` and `using gopkg.…;`
// render through — measured the element whole, so `in` was not a keyword and passed through
// unescaped. The two sides must agree on every namespace segment, whatever the keyword.
//
// The `_package` cases pin the reason the two functions are still distinct: a caller appends the
// class suffix to the FINAL segment, and only getSanitizedImport leaves it alone —
// getCoreSanitizedIdentifier Δ-prefixes anything ending in `_package`, naming a class no producer
// emits. Escaping is also idempotent, since a namespace string can be re-sanitized downstream.
func TestGetSanitizedImportKeywordSegments(t *testing.T) {
	cases := map[string]string{
		"gopkg.in":         "gopkg.@in",          // the reported case: `in` is a C# keyword
		"example.lock":     "example.@lock",      // keyword last  — not `in`-specific
		"object.example":   "@object.example",    // keyword first — position does not matter
		"a.ref.b.params.c": "a.@ref.b.@params.c", // every level is escaped independently
		"github.com":       "github.com",         // no keyword sub-token — unchanged
		"golang.org":       "golang.org",         // the only dotted segment the stdlib corpus has
		"go-isatty":        "go_isatty",          // hyphen handling is unchanged
		"in":               "@in",                // dot-free keyword — the pre-existing behavior
		"yaml_package":     "yaml_package",       // the class suffix is NEVER Δ-prefixed here…
		"gopkg.in_package": "gopkg.in_package",   // …not even when it follows a keyword-looking stem
		"gopkg.@in":        "gopkg.@in",          // already escaped — idempotent
	}

	for in, want := range cases {
		if got := getSanitizedImport(in); got != want {
			t.Errorf("getSanitizedImport(%q) = %q, want %q", in, got, want)
		}
	}

	// The agreement itself, stated directly: for a namespace SEGMENT (no class suffix), the
	// consumer-side sanitizer must render exactly what the declaration-side one does.
	for _, segment := range []string{"gopkg.in", "example.lock", "object.example", "github.com", "golang.org", "internal"} {
		if consumer, declaration := getSanitizedImport(segment), getCoreSanitizedIdentifier(segment); consumer != declaration {
			t.Errorf("segment %q renders %q for a consumer but %q for its declaration", segment, consumer, declaration)
		}
	}

	// …and end to end through the namespace renderer every consumer emission goes through.
	if got, want := convertImportPathToNamespace("gopkg.in/yaml.v3", PackageSuffix), "gopkg.@in.yaml.v3"+PackageSuffix; got != want {
		t.Errorf("convertImportPathToNamespace(gopkg.in/yaml.v3) = %q, want %q", got, want)
	}
}
