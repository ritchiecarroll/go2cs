// nugetFeed_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"go2cs/internal/sourcemeta"
)

// -nuget-map-feed (the nugetgo rehearsal, 2026-10-09, gap 2): package selection reads a configured feed instead of
// nuget.org, so owner ruling B7's rehearsal feed (int.nugettest.org) and a local folder can exercise the consumer
// half. Opt-in; nuget.org stays the default.

const feedPackageID = "nugetgo.example.test.shape"

func setNuGetMapFeed(t *testing.T, feed string) {
	t.Helper()
	saved := nugetPackageFeed
	nugetPackageFeed = feed
	t.Cleanup(func() { nugetPackageFeed = saved })
}

func shapePackage(t *testing.T, mod thirdPartyModule) []byte {
	t.Helper()
	return buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, mod.path, mod.version, corpusRelease(), nil)})
}

// A v3 service index (int.nugettest.org's shape): its PackageBaseAddress answers, and nuget.org's flat container is
// never asked.
func TestNuGetMapFeedServiceIndexAnswersSelection(t *testing.T) {
	f := newNuGetMapFixture(t)
	mod := thirdPartyModule{path: modShape.path, version: "v1.300.0"}
	pkg := shapePackage(t, mod)
	f.set("/feed/v3/index.json", `{"version":"3.0.0","resources":[{"@id":"`+f.url("/feed/v3/search")+`","@type":"SearchQueryService"},`+
		`{"@id":"`+f.url("/feedflat/")+`","@type":"PackageBaseAddress/3.0.0"}]}`)
	f.set("/feedflat/"+feedPackageID+"/index.json", `{"versions":["1.300.0"]}`)
	f.set("/feedflat/"+feedPackageID+"/1.300.0/"+feedPackageID+".1.300.0.nupkg", string(pkg))
	setNuGetMapFeed(t, f.url("/feed/v3/index.json"))

	choice, notes, err := selectNuGetPackage(feedPackageID, mod, nugetLockEntry{}, false, false)

	if err != nil || choice.packageVersion != "1.300.0" || choice.contentHash != nupkgHash(pkg) {
		t.Fatalf("choice %+v, notes %q (err %v); want 1.300.0 from the configured feed", choice, notes, err)
	}

	if n := f.count("/flat/" + feedPackageID + "/index.json"); n != 0 {
		t.Errorf("nuget.org's flat container was asked %d times; a configured feed replaces it", n)
	}
}

// A local folder feed, flat (<id>.<version>.nupkg) as nugetgo-pack.ps1 writes one: a REHEARSAL version
// (-RehearsalSuffix) is admitted from a configured feed, which is where a rehearsal lives.
func TestNuGetMapFeedLocalFolderAdmitsARehearsalVersion(t *testing.T) {
	newNuGetMapFixture(t)
	mod := thirdPartyModule{path: modShape.path, version: "v1.300.0"}
	pkg := shapePackage(t, mod)
	dir := t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, feedPackageID+".1.300.0-local.1.nupkg"), pkg, 0o644); err != nil {
		t.Fatal(err)
	}

	setNuGetMapFeed(t, dir)

	choice, notes, err := selectNuGetPackage(feedPackageID, mod, nugetLockEntry{}, false, false)

	if err != nil || choice.packageVersion != "1.300.0-local.1" || choice.contentHash != nupkgHash(pkg) {
		t.Fatalf("choice %+v, notes %q (err %v); want the rehearsal 1.300.0-local.1 from the folder", choice, notes, err)
	}
}

// A release (and its rebuilds) outranks a rehearsal on the same feed; without a configured feed a rehearsal is never
// a candidate (B3, TestNuGetPackageCandidatesFollowB3).
func TestNuGetMapFeedPrefersAReleaseToARehearsal(t *testing.T) {
	setNuGetMapFeed(t, t.TempDir())
	published := []string{"1.300.0-local.1", "1.300.0-local.2", "1.300.0", "1.300.0.1", "1.300.1-local.1"}
	got, err := nugetSelectionCandidates("v1.300.0", published)
	want := []string{"1.300.0.1", "1.300.0", "1.300.0-local.2", "1.300.0-local.1"}

	if err != nil || strings.Join(got, ",") != strings.Join(want, ",") {
		t.Errorf("candidates %v (err %v); want %v", got, err, want)
	}

	got, _ = nugetSelectionCandidates("v1.0.0-rc.1", []string{"1.0.0-rc.1.local.1", "1.0.0-rc.1.0.1", "1.0.0-rc.1"})
	want = []string{"1.0.0-rc.1.0.1", "1.0.0-rc.1", "1.0.0-rc.1.local.1"}

	if strings.Join(got, ",") != strings.Join(want, ",") {
		t.Errorf("prerelease candidates %v; want %v", got, want)
	}

	setNuGetMapFeed(t, "")
	got, _ = nugetSelectionCandidates("v1.300.0", published)

	if strings.Join(got, ",") != "1.300.0.1,1.300.0" {
		t.Errorf("without a configured feed the candidates are %v; want no rehearsal", got)
	}
}

// The flag is refused where it would do nothing or could be rewritten in transit.
func TestNuGetMapFeedIsRefusedByName(t *testing.T) {
	missing := filepath.Join(t.TempDir(), "absent")
	cases := []struct {
		name     string
		given    []string
		o        nugetMapOptions
		recurse  bool
		fragment string
	}{
		{"without -recurse=nuget", []string{"-nuget-map-feed"}, nugetMapOptions{feed: t.TempDir()}, false, "-nuget-map-feed applies only with -recurse=nuget"},
		{"with -nuget-map off", []string{"-nuget-map", "-nuget-map-feed"}, nugetMapOptions{off: true, feed: t.TempDir()}, true, "-nuget-map off"},
		{"a plain http URL", []string{"-nuget-map-feed"}, nugetMapOptions{feed: "http://example.test/v3/index.json"}, true, "only https://"},
		{"a folder that does not exist", []string{"-nuget-map-feed"}, nugetMapOptions{feed: missing}, true, "is not a folder"},
	}

	for _, c := range cases {
		err := validateNuGetMapFlags(c.given, c.o, c.recurse)

		if err == nil || !strings.Contains(err.Error(), c.fragment) {
			t.Errorf("%s: %v; want a refusal naming %q", c.name, err, c.fragment)
		}
	}

	if err := validateNuGetMapFlags([]string{"-nuget-map-feed"}, nugetMapOptions{feed: "https://apiint.nugettest.org/v3/index.json"}, true); err != nil {
		t.Errorf("an https service index was refused: %v", err)
	}
}
