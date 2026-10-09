// nugetFeed.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"sync"
)

// -nuget-map-feed: the feed package SELECTION reads (the nugetgo rehearsal, 2026-10-09, gap 2). By default
// selection lists and downloads a mapped package's versions from nuget.org's flat container; with the flag it reads
// the named feed INSTEAD, so owner ruling B7's rehearsal gallery (int.nugettest.org, named by its v3 service index)
// or a local folder of nupkgs can exercise the consumer half. It is opt-in, and it changes only where versions are
// read: the restore of the emitted projects reads the user's own package sources, so the feed must be one of them.
//
// A REHEARSAL version -- nugetgo-pack.ps1's -RehearsalSuffix, X.Y.Z-<label> for a release and L.<label> for a
// prerelease L -- is a candidate only from a configured feed, ranked below a release and its rebuilds: a rehearsal is
// never published to nuget.org, and the self-description check still requires the exact module version.

// nugetPackageFeed is the -nuget-map-feed source selection reads; "" is nuget.org. Set once per conversion
// (ModuleConverter), as the other selection globals are.
var nugetPackageFeed string

// nugetFeedIsFolder reports whether the configured feed is a local folder rather than an https service index.
func nugetFeedIsFolder() bool {
	return nugetPackageFeed != "" && !strings.Contains(nugetPackageFeed, "://")
}

var (
	nugetFeedBasesMu sync.Mutex
	nugetFeedBases   = map[string]string{}
)

// nugetFeedFlatContainer is the flat-container base selection reads: nuget.org's, or the PackageBaseAddress/3.0.0
// resource a configured v3 service index names (read once per feed).
func nugetFeedFlatContainer() (string, error) {
	if nugetPackageFeed == "" {
		return nugetFlatContainerURL, nil
	}

	nugetFeedBasesMu.Lock()
	defer nugetFeedBasesMu.Unlock()

	if base, ok := nugetFeedBases[nugetPackageFeed]; ok {
		return base, nil
	}

	data, err := nugetHTTPGet(nugetPackageFeed, 4<<20)

	if err != nil {
		return "", fmt.Errorf("-nuget-map-feed %s: %v", nugetPackageFeed, err)
	}

	var index struct {
		Resources []struct {
			ID   string `json:"@id"`
			Type string `json:"@type"`
		} `json:"resources"`
	}

	if err := json.Unmarshal(data, &index); err != nil {
		return "", fmt.Errorf("-nuget-map-feed %s is not a NuGet v3 service index: %v", nugetPackageFeed, err)
	}

	for _, resource := range index.Resources {
		if strings.HasPrefix(resource.Type, "PackageBaseAddress/3.0.0") && strings.HasPrefix(resource.ID, "https://") {
			base := strings.TrimSuffix(resource.ID, "/")
			nugetFeedBases[nugetPackageFeed] = base
			return base, nil
		}
	}

	return "", fmt.Errorf("-nuget-map-feed %s names no https PackageBaseAddress/3.0.0 resource", nugetPackageFeed)
}

// nugetFolderFeedVersions lists a package's versions in a folder feed, flat (<id>.<version>.nupkg, as
// nugetgo-pack.ps1 writes one) or hierarchical (<id>/<version>/<id>.<version>.nupkg, as `nuget add` does).
func nugetFolderFeedVersions(nugetID string) ([]string, error) {
	lowerID := strings.ToLower(nugetID)
	entries, err := os.ReadDir(nugetPackageFeed)

	if err != nil {
		return nil, fmt.Errorf("-nuget-map-feed %s: %v", nugetPackageFeed, err)
	}

	seen := map[string]bool{}
	var versions []string

	add := func(version string) {
		if version != "" && !seen[version] {
			seen[version] = true
			versions = append(versions, version)
		}
	}

	for _, entry := range entries {
		name := strings.ToLower(entry.Name())

		if !entry.IsDir() && strings.HasPrefix(name, lowerID+".") && strings.HasSuffix(name, ".nupkg") {
			add(strings.TrimSuffix(strings.TrimPrefix(name, lowerID+"."), ".nupkg"))
		}

		if entry.IsDir() && name == lowerID {
			children, _ := os.ReadDir(filepath.Join(nugetPackageFeed, entry.Name()))

			for _, child := range children {
				if child.IsDir() {
					add(strings.ToLower(child.Name()))
				}
			}
		}
	}

	return versions, nil
}

// nugetFolderFeedRead reads one package version from a folder feed, either layout.
func nugetFolderFeedRead(nugetID, version string) ([]byte, error) {
	lowerID, lowerVersion := strings.ToLower(nugetID), strings.ToLower(version)
	fileName := lowerID + "." + lowerVersion + ".nupkg"
	entries, err := os.ReadDir(nugetPackageFeed)

	if err != nil {
		return nil, fmt.Errorf("-nuget-map-feed %s: %v", nugetPackageFeed, err)
	}

	for _, entry := range entries {
		if !entry.IsDir() && strings.EqualFold(entry.Name(), fileName) {
			return os.ReadFile(filepath.Join(nugetPackageFeed, entry.Name()))
		}
	}

	if data, err := os.ReadFile(filepath.Join(nugetPackageFeed, lowerID, lowerVersion, fileName)); err == nil {
		return data, nil
	}

	return nil, errNuGetNotFound
}

var nugetRehearsalLabelPattern = regexp.MustCompile(`^[0-9a-z-]+(\.[0-9a-z-]+)*$`)

// nugetSelectionCandidates is the candidate list selection walks: owner ruling B3's (nugetPackageCandidates), then,
// from a configured feed only, the module version's rehearsals, highest first.
func nugetSelectionCandidates(moduleVersion string, published []string) ([]string, error) {
	candidates, err := nugetPackageCandidates(moduleVersion, published)

	if err != nil || nugetPackageFeed == "" {
		return candidates, err
	}

	match := goModuleVersionPattern.FindStringSubmatch(moduleVersion)
	base, separator := match[1], "-"

	if match[2] != "" {
		base, separator = match[1]+"-"+match[2], "."
	}

	var rehearsals []string

	for _, version := range published {
		lower := strings.ToLower(version)
		label, ok := strings.CutPrefix(lower, base+separator)

		// A prerelease's rebuild L.0.N is B3's, not a rehearsal.
		if !ok || !nugetRehearsalLabelPattern.MatchString(label) || (separator == "." && regexp.MustCompile(`^0\.\d+$`).MatchString(label)) {
			continue
		}

		rehearsals = append(rehearsals, lower)
	}

	sort.SliceStable(rehearsals, func(i, j int) bool { return nugetLabelAfter(rehearsals[i], rehearsals[j]) })
	return append(candidates, rehearsals...), nil
}

// nugetLabelAfter orders two versions dot-segment by segment, numbers numerically, so local.10 follows local.9.
func nugetLabelAfter(left, right string) bool {
	a, b := strings.FieldsFunc(left, isVersionSeparator), strings.FieldsFunc(right, isVersionSeparator)

	for i := 0; i < len(a) && i < len(b); i++ {
		if a[i] == b[i] {
			continue
		}

		x, errX := strconv.Atoi(a[i])
		y, errY := strconv.Atoi(b[i])

		if errX == nil && errY == nil {
			return x > y
		}

		return a[i] > b[i]
	}

	return len(a) > len(b)
}

func isVersionSeparator(r rune) bool {
	return r == '.' || r == '-'
}
