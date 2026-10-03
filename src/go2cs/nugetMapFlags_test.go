// nugetMapFlags_test.go - Gbtc
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

func TestNuGetMapFlagsKeepTheirListedOrder(t *testing.T) {
	o, err := newNuGetMapOptions([]string{"mine.txt", "https://example.com/team.txt"}, []string{"example.com/x"}, true, true, true)

	if err != nil {
		t.Fatal(err)
	}

	if strings.Join(o.sources, " ") != "mine.txt https://example.com/team.txt" {
		t.Errorf("sources lost their listed order (which is precedence): %q", o.sources)
	}

	if !o.only || !o.refresh || !o.canonicalOnly || len(o.exclude) != 1 || o.off {
		t.Errorf("flag values not carried: %+v", o)
	}
}

func TestNuGetMapOffIsItsOwnValue(t *testing.T) {
	o, err := newNuGetMapOptions([]string{"off"}, nil, false, false, false)

	if err != nil {
		t.Fatal(err)
	}

	if !o.off || len(o.sources) != 0 {
		t.Errorf("-nuget-map off parsed as %+v; want off with no source", o)
	}

	if _, err := newNuGetMapOptions([]string{"off", "mine.txt"}, nil, false, false, false); err == nil || !strings.Contains(err.Error(), "off") {
		t.Errorf("-nuget-map off combined with a source was accepted (err %v)", err)
	}

	if _, err := newNuGetMapOptions([]string{""}, nil, false, false, false); err == nil {
		t.Errorf("an empty -nuget-map value was accepted")
	}

	if _, err := newNuGetMapOptions(nil, []string{""}, false, false, false); err == nil {
		t.Errorf("an empty -nuget-map-exclude value was accepted")
	}
}

// THE FLIP (S3b, COORD ruling 2026-10-02): under -recurse=nuget the registry is consulted by DEFAULT, with
// no -nuget-map flag at all; -nuget-map off is the one way to opt out. S3a pinned the opposite (dormant
// until S3b), and this is that pin, inverted in the same change that applies the substitution.
func TestNuGetMapIsOnByDefault(t *testing.T) {
	if !(nugetMapOptions{}).active() {
		t.Errorf("no -nuget-map flag does not resolve mappings; S3b's default is on")
	}

	if (nugetMapOptions{off: true}).active() {
		t.Errorf("-nuget-map off resolves mappings")
	}

	if !(nugetMapOptions{sources: []string{"mine.txt"}}).active() {
		t.Errorf("a named -nuget-map source does not resolve mappings")
	}
}

// A -nuget-map* flag without -recurse=nuget would do nothing at all, so it is refused BY NAME (COORD
// ruling 2026-10-02) rather than ignored.
func TestNuGetMapFlagsAreRefusedWithoutRecurseNuGet(t *testing.T) {
	for _, flagName := range []string{"-nuget-map", "-nuget-map-only", "-nuget-map-exclude", "-nuget-map-refresh", "-nuget-map-canonical-only"} {
		err := validateNuGetMapFlags([]string{flagName}, nugetMapOptions{sources: []string{"mine.txt"}}, false)

		if err == nil {
			t.Errorf("%s without -recurse=nuget was accepted", flagName)
			continue
		}

		if !strings.Contains(err.Error(), flagName) || !strings.Contains(err.Error(), "-recurse=nuget") {
			t.Errorf("%s refusal does not name the flag and -recurse=nuget: %v", flagName, err)
		}
	}

	if err := validateNuGetMapFlags(nil, nugetMapOptions{}, false); err != nil {
		t.Errorf("no -nuget-map flag at all was refused: %v", err)
	}
}

func TestNuGetMapOnlyNeedsASource(t *testing.T) {
	err := validateNuGetMapFlags([]string{"-nuget-map-only"}, nugetMapOptions{only: true}, true)

	if err == nil || !strings.Contains(err.Error(), "-nuget-map-only") {
		t.Errorf("-nuget-map-only with no source was accepted (err %v)", err)
	}

	err = validateNuGetMapFlags([]string{"-nuget-map", "-nuget-map-only"}, nugetMapOptions{sources: []string{"mine.txt"}, only: true}, true)

	if err != nil {
		t.Errorf("-nuget-map-only with a source was refused: %v", err)
	}
}

// Only https:// is fetched: a plain http:// source could be rewritten in transit, and any other scheme is
// not a source this converter knows how to read.
func TestNuGetMapRefusesEveryURLButHTTPS(t *testing.T) {
	for _, src := range []string{"http://example.com/m.txt", "ftp://example.com/m.txt", "file:///tmp/m.txt"} {
		err := validateNuGetMapFlags([]string{"-nuget-map"}, nugetMapOptions{sources: []string{src}}, true)

		if err == nil || !strings.Contains(err.Error(), src) {
			t.Errorf("source %s was accepted, or its refusal does not name it (err %v)", src, err)
		}
	}

	if err := validateNuGetMapFlags([]string{"-nuget-map"}, nugetMapOptions{sources: []string{"https://example.com/m.txt", "local/m.txt"}}, true); err != nil {
		t.Errorf("an https URL and a local file were refused: %v", err)
	}
}

func TestStringListFlagAccumulates(t *testing.T) {
	var list stringListFlag

	for _, v := range []string{"a", "b", "a"} {
		if err := list.Set(v); err != nil {
			t.Fatal(err)
		}
	}

	if strings.Join(list, ",") != "a,b,a" || list.String() != "a,b,a" {
		t.Errorf("stringListFlag = %q (String %q); want every value in order", []string(list), list.String())
	}
}
