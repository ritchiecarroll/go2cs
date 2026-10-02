// nugetMapFetch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func TestNuGetMapFetchesAnHTTPSSource(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/team.txt", mapRow(modA.path, "team.a", "community"))

	src, warnings, err := loadNuGetMapSource(f.url("/team.txt"), false)

	if err != nil {
		t.Fatal(err)
	}

	if src.name != f.url("/team.txt") || src.rows[modA.path].nugetID != "team.a" || len(warnings) != 0 || f.count("/team.txt") != 1 {
		t.Errorf("source %+v, warnings %q, requests %d; want team.a from one request", src, warnings, f.count("/team.txt"))
	}
}

// The cache is REVALIDATED every run: the second read sends If-None-Match, the fixture answers 304, and
// the rows come from the cached body.
func TestNuGetMapRevalidatesItsCache(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/team.txt", mapRow(modA.path, "team.a", "community"))

	if _, _, err := loadNuGetMapSource(f.url("/team.txt"), false); err != nil {
		t.Fatal(err)
	}

	src, _, err := loadNuGetMapSource(f.url("/team.txt"), false)

	if err != nil {
		t.Fatal(err)
	}

	if f.count("/team.txt") != 2 || f.notMod["/team.txt"] != 1 || src.rows[modA.path].nugetID != "team.a" {
		t.Errorf("requests %d, 304s %d, rows %+v; want a conditional second request answered 304 and the cached rows",
			f.count("/team.txt"), f.notMod["/team.txt"], src.rows)
	}
}

func TestNuGetMapRefreshBypassesTheCache(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/team.txt", mapRow(modA.path, "team.a", "community"))

	if _, _, err := loadNuGetMapSource(f.url("/team.txt"), false); err != nil {
		t.Fatal(err)
	}

	if _, _, err := loadNuGetMapSource(f.url("/team.txt"), true); err != nil {
		t.Fatal(err)
	}

	if f.count("/team.txt") != 2 || f.sawINM["/team.txt"] != 0 {
		t.Errorf("requests %d, conditional %d; -nuget-map-refresh must fetch unconditionally", f.count("/team.txt"), f.sawINM["/team.txt"])
	}
}

func TestNuGetMapFallsBackToTheCacheWhenOffline(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/team.txt", mapRow(modA.path, "team.a", "community"))
	url := f.url("/team.txt")

	if _, _, err := loadNuGetMapSource(url, false); err != nil {
		t.Fatal(err)
	}

	f.server.Close()
	src, warnings, err := loadNuGetMapSource(url, false)

	if err != nil {
		t.Fatalf("offline with a cached copy failed: %v", err)
	}

	if src.rows[modA.path].nugetID != "team.a" || len(warnings) != 1 || !strings.Contains(warnings[0], "cached") || !strings.Contains(warnings[0], url) {
		t.Errorf("rows %+v, warnings %q; want the cached rows and a warning naming the source and the cache", src.rows, warnings)
	}
}

// A source the USER named and that cannot be read fails the run by name; the default registry instead
// warns and leaves every module it would have answered unmapped (local conversion is the fallback).
func TestNuGetMapUnreachableUserSourceFailsTheRun(t *testing.T) {
	f := newNuGetMapFixture(t)
	url := f.url("/team.txt")
	f.server.Close()

	if _, _, err := loadNuGetMapSource(url, false); err == nil || !strings.Contains(err.Error(), url) {
		t.Errorf("an unreachable user source with no cache did not fail by name (err %v)", err)
	}
}

func TestNuGetMapUnreachableRegistryLeavesModulesUnmapped(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.server.Close()

	src, warnings, err := loadNuGetMapSource(nugetMapRegistryURL, false)

	if err != nil {
		t.Fatalf("an unreachable registry failed the run: %v", err)
	}

	if len(src.rows) != 0 || len(warnings) != 1 || !strings.Contains(warnings[0], "unmapped") {
		t.Errorf("rows %+v, warnings %q; want no rows and one warning that the modules stay unmapped", src.rows, warnings)
	}
}

func TestNuGetMapRefusesAnOversizedSource(t *testing.T) {
	f := newNuGetMapFixture(t)
	saved := nugetMapMaxBytes
	nugetMapMaxBytes = 64
	t.Cleanup(func() { nugetMapMaxBytes = saved })

	body := strings.Repeat("# padding padding padding\n", 4)
	f.set("/big.txt", body)

	if _, _, err := loadNuGetMapSource(f.url("/big.txt"), false); err == nil || !strings.Contains(err.Error(), "64") {
		t.Errorf("a %d-byte remote source passed a 64-byte cap (err %v)", len(body), err)
	}

	path := writeMapFile(t, "big.txt", body)

	if _, _, err := loadNuGetMapSource(path, false); err == nil || !strings.Contains(err.Error(), "64") {
		t.Errorf("a %d-byte local source passed a 64-byte cap (err %v)", len(body), err)
	}
}

// A redirect may not downgrade the fetch to plain http.
func TestNuGetMapRefusesARedirectAwayFromHTTPS(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(modA.path, "reg.a", "canonical"))

	_, _, err := loadNuGetMapSource(f.url("/redirect-to-http"), false)

	if err == nil || !strings.Contains(err.Error(), "https") {
		t.Errorf("a redirect to http:// was followed (err %v)", err)
	}

	if f.count("/registry") != 0 {
		t.Errorf("the plain-http target was requested")
	}
}

func TestNuGetMapReadsALocalFile(t *testing.T) {
	path := writeMapFile(t, "mine.txt", mapRow(modA.path, "mine.a", "community"))
	src, _, err := loadNuGetMapSource(path, false)

	if err != nil || src.rows[modA.path].nugetID != "mine.a" || src.name != path {
		t.Errorf("local source %+v (err %v); want mine.a named by its path", src, err)
	}

	missing := filepath.Join(t.TempDir(), "missing.txt")

	if _, _, err := loadNuGetMapSource(missing, false); err == nil || !strings.Contains(err.Error(), missing) {
		t.Errorf("a missing local source did not fail by name (err %v)", err)
	}
}

func TestNuGetMapMalformedRemoteSourceIsRefusedByURLAndLine(t *testing.T) {
	f := newNuGetMapFixture(t)
	f.set("/bad.txt", mapRow(modA.path, "a", "community")+"only\ttwo\n")

	_, _, err := loadNuGetMapSource(f.url("/bad.txt"), false)

	if err == nil || !strings.Contains(err.Error(), f.url("/bad.txt")+":2") {
		t.Errorf("a malformed remote source was not refused by URL:line (err %v)", err)
	}
}

func TestNuGetMapSourceListAppendsTheRegistryLast(t *testing.T) {
	list := nugetMapSourceList(nugetMapOptions{sources: []string{"mine.txt", "https://example.com/team.txt"}})

	if strings.Join(list, " ") != "mine.txt https://example.com/team.txt "+nugetMapRegistryURL {
		t.Errorf("source list %q; want the listed sources, then the registry", list)
	}

	only := nugetMapSourceList(nugetMapOptions{sources: []string{"mine.txt"}, only: true})

	if strings.Join(only, " ") != "mine.txt" {
		t.Errorf("-nuget-map-only source list %q; want the listed source alone", only)
	}

	if off := nugetMapSourceList(nugetMapOptions{off: true}); len(off) != 0 {
		t.Errorf("-nuget-map off source list %q; want none", off)
	}
}
