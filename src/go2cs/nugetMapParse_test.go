// nugetMapParse_test.go - Gbtc
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

// Schema v1 (docs/PLAN-nugetgo.md section 1): '#' comments, blank lines, and exactly six TAB-separated
// fields -- module-path, nuget-id, status, source-repo, registered, contact.
func TestNuGetMapParsesSchemaV1(t *testing.T) {
	body := "# module-path\tnuget-id\tstatus\tsource-repo\tregistered\tcontact\n" +
		"\n" +
		mapRow("github.com/google/uuid", "nugetgo.github.com.google.uuid", "community") +
		strings.TrimSuffix(mapRow("github.com/ritchiecarroll/hashset", "nugetgo.github.com.ritchiecarroll.hashset", "canonical"), "\n") + "\r\n" +
		mapRow("example.com/gone/v2", "nugetgo.example.com.gone.v2", "withdrawn")

	src, _, err := parseNuGetMap("m.txt", []byte(body))

	if err != nil {
		t.Fatal(err)
	}

	if src.name != "m.txt" || len(src.rows) != 3 {
		t.Fatalf("parsed %d rows from %q; want 3", len(src.rows), src.name)
	}

	row := src.rows["github.com/ritchiecarroll/hashset"]

	if row.nugetID != "nugetgo.github.com.ritchiecarroll.hashset" || row.status != "canonical" || row.line != 4 {
		t.Errorf("CRLF row parsed as %+v; want the id, canonical, line 4", row)
	}

	if src.rows["example.com/gone/v2"].status != "withdrawn" {
		t.Errorf("a withdrawn row was not kept as withdrawn: %+v", src.rows["example.com/gone/v2"])
	}
}

// A malformed row REFUSES THE SOURCE by name with file:line (COORD ruling 2026-10-02): the registry's CI
// guarantees well-formed rows, so a bad one signals corruption, and skipping it would silently change
// what the source answers.
func TestNuGetMapRefusesAMalformedSourceByFileAndLine(t *testing.T) {
	good := mapRow("github.com/a/b", "nugetgo.github.com.a.b", "community")

	cases := []struct {
		name string
		row  string
		want string
	}{
		{"five fields", "github.com/c/d\tnugetgo.github.com.c.d\tcommunity\thttps://x\t2026-10-02\n", "6 TAB-separated fields"},
		{"seven fields", strings.TrimSuffix(mapRow("github.com/c/d", "nugetgo.github.com.c.d", "community"), "\n") + "\textra\n", "6 TAB-separated fields"},
		{"unknown status", mapRow("github.com/c/d", "nugetgo.github.com.c.d", "official"), "status"},
		{"empty module", mapRow("", "nugetgo.github.com.c.d", "community"), "module"},
		{"invalid module path", mapRow("github.com/c d", "nugetgo.github.com.c.d", "community"), "module"},
		{"empty id", mapRow("github.com/c/d", "", "community"), "nuget-id"},
		{"id with a space", mapRow("github.com/c/d", "nugetgo github", "community"), "nuget-id"},
		{"padded field", mapRow(" github.com/c/d", "nugetgo.github.com.c.d", "community"), "module"},
	}

	for _, c := range cases {
		_, _, err := parseNuGetMap("bad.txt", []byte(good+c.row))

		if err == nil {
			t.Errorf("%s: the source was accepted", c.name)
			continue
		}

		if !strings.Contains(err.Error(), "bad.txt:2") || !strings.Contains(err.Error(), c.want) {
			t.Errorf("%s: the refusal does not name bad.txt:2 and %q: %v", c.name, c.want, err)
		}
	}

	// A malformed go. ID is refused for its malformation ALONE, as the registry's lint refuses it: the prefix rule
	// runs after the validity checks, so one row yields one nuget-id problem (and a well-formed go. row is skipped,
	// not refused: TestNuGetMapSkipsARowTheIDPatternRefusesWithAWarning).
	_, _, err := parseNuGetMap("bad.txt", []byte(good+mapRow("github.com/c/d", "go.github com", "community")))

	if err == nil || !strings.Contains(err.Error(), "non-empty, no whitespace") || strings.Contains(err.Error(), `uses the "go." prefix`) {
		t.Errorf("a malformed go. ID must be refused for its malformation alone: %v", err)
	}
}

func TestNuGetMapRefusesADuplicateModule(t *testing.T) {
	body := mapRow("github.com/a/b", "nugetgo.github.com.a.b", "community") +
		mapRow("github.com/c/d", "nugetgo.github.com.c.d", "community") +
		mapRow("github.com/a/b", "nugetgo.github.com.a.b.other", "canonical")

	_, _, err := parseNuGetMap("dup.txt", []byte(body))

	if err == nil || !strings.Contains(err.Error(), "dup.txt:3") || !strings.Contains(err.Error(), "line 1") {
		t.Errorf("a duplicate module row was accepted, or the refusal does not name both lines: %v", err)
	}
}

// Major versions are distinct modules: one row per module path INCLUDING its /vN suffix.
func TestNuGetMapKeysOnTheFullModulePath(t *testing.T) {
	body := mapRow("github.com/golang-jwt/jwt/v4", "nugetgo.github.com.golang-jwt.jwt.v4", "community") +
		mapRow("github.com/golang-jwt/jwt/v5", "nugetgo.github.com.golang-jwt.jwt.v5", "community")

	src, _, err := parseNuGetMap("v.txt", []byte(body))

	if err != nil {
		t.Fatalf("two major versions of one module were refused as duplicates: %v", err)
	}

	if len(src.rows) != 2 {
		t.Errorf("parsed %d rows; want 2", len(src.rows))
	}
}

// The "go." refusal requires nothing else: an otherwise valid ID with another prefix -- including one that merely
// BEGINS with the letters "go" -- is accepted, as the registry's lint accepts it.
func TestNuGetMapAcceptsAnyIDThatDoesNotTakeTheGoPrefix(t *testing.T) {
	body := mapRow("github.com/a/b", "nugetgo.github.com.a.b", "canonical") +
		mapRow("github.com/c/d", "golang.c.d", "community") +
		mapRow("github.com/e/f", "Acme.go.F", "community") +
		mapRow("github.com/g/h", "gopher.g.h", "withdrawn") +
		mapRow("github.com/i/j", "go", "community") +
		mapRow("github.com/k/l", "Example.HashSet", "canonical")

	src, _, err := parseNuGetMap("ok.txt", []byte(body))

	if err != nil {
		t.Fatalf("a source with no go.-prefixed ID was refused: %v", err)
	}

	if len(src.rows) != 6 {
		t.Errorf("want 6 rows, got %d", len(src.rows))
	}
}
