// nugetPackage.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/zip"
	"bytes"
	"crypto/sha512"
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"

	"go2cs/internal/sourcemeta"
)

// Package SELECTION for a mapped module (docs/PLAN-nugetgo.md 4.3 and 4.6, stage S3b).
//
// A mapping names a package ID; this file decides which published VERSION of it, if any, the module is
// referenced as. A version qualifies only when its self-description (go2cs/source-metadata.txt, stage S2)
// names this module at exactly the version the build selected AND was built for this converter's own corpus
// release (corpusRelease(), the strict default). Among the qualifying versions the highest revision wins
// (owner ruling B3's tie-break). Nothing near is ever substituted: a version that does not qualify is passed
// over with its reason, and when none qualifies the module converts locally and the reasons are the warning.
//
// The lock pins the selected version and the SHA-512 of its bytes, so a later run selects nothing: it reads
// that version from the cache or from NuGet's global packages folder (a restore already downloaded it), with
// no request at all, and REFUSES by name a package whose bytes no longer match the pin -- nuget.org never
// lets a version be replaced, so a mismatch is an integrity failure, not an update.

// nugetFlatContainerURL is NuGet's flat-container base. A variable so a test can point it at a fixture.
var nugetFlatContainerURL = "https://api.nuget.org/v3-flatcontainer"

// nugetPackageMaxBytes caps one downloaded nupkg; a variable only so a test can lower it.
var nugetPackageMaxBytes int64 = 256 << 20

// nugetSelfDescriptionMaxBytes caps the self-description read out of a nupkg.
const nugetSelfDescriptionMaxBytes = 16 << 20

// nugetGlobalPackagesRoot is NuGet's global packages folder; a variable so a test can inject one.
var nugetGlobalPackagesRoot = func() string {
	if root := os.Getenv("NUGET_PACKAGES"); root != "" {
		return root
	}

	home, err := os.UserHomeDir()

	if err != nil {
		return ""
	}

	return filepath.Join(home, ".nuget", "packages")
}

var goModuleVersionPattern = regexp.MustCompile(`^v(\d+\.\d+\.\d+)(?:-([0-9A-Za-z.-]+))?(?:\+incompatible)?$`)

// nugetPackageCandidates returns, highest revision first, the published versions a conversion of
// moduleVersion can carry (owner ruling B3): a release vX.Y.Z is published as X.Y.Z and rebuilt as X.Y.Z.N; a
// prerelease or pseudo-version L as L and rebuilt as L.0.N; +incompatible is dropped. Every other version --
// another patch, a rehearsal's suffix, a prerelease of the same core -- is not a candidate.
func nugetPackageCandidates(moduleVersion string, published []string) ([]string, error) {
	match := goModuleVersionPattern.FindStringSubmatch(moduleVersion)

	if match == nil {
		return nil, fmt.Errorf("%s is not a Go module version", moduleVersion)
	}

	core, prerelease := match[1], match[2]

	if strings.ToLower(prerelease) != prerelease {
		return nil, fmt.Errorf("%s has an uppercase prerelease label, which no package version carries (owner ruling B3: converted locally)", moduleVersion)
	}

	base, rebuild := core, core+"."

	if prerelease != "" {
		base = core + "-" + prerelease
		rebuild = base + ".0."
	}

	type candidate struct {
		version  string
		revision int
	}

	var candidates []candidate

	for _, version := range published {
		lower := strings.ToLower(version)

		switch {
		case lower == base:
			candidates = append(candidates, candidate{lower, 0})
		case strings.HasPrefix(lower, rebuild):
			if revision, err := strconv.Atoi(strings.TrimPrefix(lower, rebuild)); err == nil && revision > 0 {
				candidates = append(candidates, candidate{lower, revision})
			}
		}
	}

	sort.SliceStable(candidates, func(i, j int) bool { return candidates[i].revision > candidates[j].revision })

	var versions []string

	for _, c := range candidates {
		versions = append(versions, c.version)
	}

	return versions, nil
}

// nugetPackageChoice is the package version selected for a mapped module.
type nugetPackageChoice struct {
	nugetID        string
	packageVersion string
	contentHash    string
	description    sourcemeta.Description
}

// selectNuGetPackage selects the published version a mapped module is referenced as. It returns no choice
// (empty packageVersion) with the reasons every candidate was passed over when none qualifies, and an ERROR
// only for an integrity failure: a locked package whose bytes no longer hash to the pin.
func selectNuGetPackage(nugetID string, mod thirdPartyModule, locked nugetLockEntry, haveLock bool, refresh bool) (nugetPackageChoice, []string, error) {
	if haveLock && !refresh && locked.packageVersion != "" && locked.packageVersion != "-" && locked.version == mod.version {
		data, err := readNuGetPackage(nugetID, locked.packageVersion)

		if err != nil {
			return nugetPackageChoice{}, []string{fmt.Sprintf("%s@%s (locked): %v", nugetID, locked.packageVersion, err)}, nil
		}

		hash := nugetContentHash(data)

		if locked.contentHash != "" && locked.contentHash != "-" && hash != locked.contentHash {
			return nugetPackageChoice{}, nil, fmt.Errorf("%s pins %s@%s with %s, but the package now hashes %s: a published version never changes on nuget.org, "+
				"so this is refused -- investigate it, or pass -nuget-map-refresh to select again", nugetLockFileName, nugetID, locked.packageVersion, locked.contentHash, hash)
		}

		choice, reason := qualifyNuGetPackage(nugetID, locked.packageVersion, data, mod)

		if reason != "" {
			return nugetPackageChoice{}, []string{reason + " (the locked version)"}, nil
		}

		return choice, nil, nil
	}

	published, err := fetchNuGetVersions(nugetID)

	if err != nil {
		return nugetPackageChoice{}, []string{fmt.Sprintf("%s: %v", nugetID, err)}, nil
	}

	candidates, err := nugetPackageCandidates(mod.version, published)

	if err != nil {
		return nugetPackageChoice{}, []string{fmt.Sprintf("%s: %v", nugetID, err)}, nil
	}

	if len(candidates) == 0 {
		return nugetPackageChoice{}, []string{fmt.Sprintf("%s publishes no version for %s %s (published: %s)", nugetID, mod.path, mod.version, strings.Join(published, ", "))}, nil
	}

	var notes []string

	for _, version := range candidates {
		data, err := readNuGetPackage(nugetID, version)

		if err != nil {
			notes = append(notes, fmt.Sprintf("%s@%s: %v", nugetID, version, err))
			continue
		}

		choice, reason := qualifyNuGetPackage(nugetID, version, data, mod)

		if reason != "" {
			notes = append(notes, reason)
			continue
		}

		return choice, notes, nil
	}

	return nugetPackageChoice{}, notes, nil
}

// qualifyNuGetPackage reads a nupkg's self-description and checks it describes this module, at this version,
// built for this converter's corpus release; it returns the reason when it does not.
func qualifyNuGetPackage(nugetID, version string, data []byte, mod thirdPartyModule) (nugetPackageChoice, string) {
	label := nugetID + "@" + version
	archive, err := zip.NewReader(bytes.NewReader(data), int64(len(data)))

	if err != nil {
		return nugetPackageChoice{}, fmt.Sprintf("%s: not a readable nupkg: %v", label, err)
	}

	var entry *zip.File

	for _, file := range archive.File {
		if file.Name == sourcemeta.EntryPath {
			entry = file
			break
		}
	}

	if entry == nil {
		return nugetPackageChoice{}, fmt.Sprintf("%s carries no %s (a package packed before stage S2)", label, sourcemeta.EntryPath)
	}

	reader, err := entry.Open()

	if err != nil {
		return nugetPackageChoice{}, fmt.Sprintf("%s: %s: %v", label, sourcemeta.EntryPath, err)
	}

	text, err := io.ReadAll(io.LimitReader(reader, nugetSelfDescriptionMaxBytes+1))
	reader.Close()

	if err != nil || len(text) > nugetSelfDescriptionMaxBytes {
		return nugetPackageChoice{}, fmt.Sprintf("%s: %s is unreadable or over %d bytes", label, sourcemeta.EntryPath, nugetSelfDescriptionMaxBytes)
	}

	description, err := sourcemeta.Parse(text)

	if err != nil {
		return nugetPackageChoice{}, fmt.Sprintf("%s: malformed %s: %v", label, sourcemeta.EntryPath, err)
	}

	if description.Module != mod.path || description.ModuleVersion != mod.version {
		return nugetPackageChoice{}, fmt.Sprintf("%s describes %s %s, not %s %s", label, description.Module, description.ModuleVersion, mod.path, mod.version)
	}

	if description.Go2csRelease != corpusRelease() {
		return nugetPackageChoice{}, fmt.Sprintf("%s was built for go2cs %s, and this converter is %s (the strict match: a package is rebuilt for each release)",
			label, description.Go2csRelease, corpusRelease())
	}

	return nugetPackageChoice{nugetID: nugetID, packageVersion: version, contentHash: nugetContentHash(data), description: description}, ""
}

// nugetContentHash is the SHA-512 of a nupkg's bytes, the form the lock records.
func nugetContentHash(data []byte) string {
	sum := sha512.Sum512(data)
	return "sha512-" + base64.StdEncoding.EncodeToString(sum[:])
}

// fetchNuGetVersions lists a package's published versions from the flat container.
func fetchNuGetVersions(nugetID string) ([]string, error) {
	data, err := nugetHTTPGet(fmt.Sprintf("%s/%s/index.json", nugetFlatContainerURL, strings.ToLower(nugetID)), 4<<20)

	if err != nil {
		return nil, err
	}

	var index struct {
		Versions []string `json:"versions"`
	}

	if err := json.Unmarshal(data, &index); err != nil {
		return nil, fmt.Errorf("the flat container's version list is not JSON: %v", err)
	}

	return index.Versions, nil
}

// readNuGetPackage returns a nupkg's bytes from the cache, then NuGet's global packages folder, then the flat
// container (caching what it downloads).
func readNuGetPackage(nugetID, version string) ([]byte, error) {
	lowerID, lowerVersion := strings.ToLower(nugetID), strings.ToLower(version)
	fileName := lowerID + "." + lowerVersion + ".nupkg"
	cachePath := ""

	if root, err := nugetMapCacheRoot(); err == nil {
		cachePath = filepath.Join(root, "packages", lowerID, lowerVersion, fileName)

		if data, err := os.ReadFile(cachePath); err == nil {
			return data, nil
		}
	}

	if gpf := nugetGlobalPackagesRoot(); gpf != "" {
		if data, err := os.ReadFile(filepath.Join(gpf, lowerID, lowerVersion, fileName)); err == nil {
			return data, nil
		}
	}

	data, err := nugetHTTPGet(fmt.Sprintf("%s/%s/%s/%s", nugetFlatContainerURL, lowerID, lowerVersion, fileName), nugetPackageMaxBytes)

	if err != nil {
		return nil, err
	}

	if cachePath != "" {
		if err := os.MkdirAll(filepath.Dir(cachePath), 0o755); err == nil {
			_ = os.WriteFile(cachePath, data, 0o644)
		}
	}

	return data, nil
}

// errNuGetNotFound marks a flat-container 404: the package or version is not published.
var errNuGetNotFound = errors.New("not published (HTTP 404)")

// nugetHTTPGet fetches one https:// resource under the -nuget-map client and redirect policy, capped.
func nugetHTTPGet(url string, maxBytes int64) ([]byte, error) {
	client := *nugetMapHTTPClient
	client.CheckRedirect = nugetMapCheckRedirect
	resp, err := client.Get(url)

	if err != nil {
		return nil, err
	}

	defer resp.Body.Close()

	if resp.StatusCode == http.StatusNotFound {
		return nil, errNuGetNotFound
	}

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("HTTP %s from %s", resp.Status, url)
	}

	if resp.ContentLength > maxBytes {
		return nil, fmt.Errorf("larger than the %d-byte cap", maxBytes)
	}

	data, err := io.ReadAll(io.LimitReader(resp.Body, maxBytes+1))

	if err != nil {
		return nil, err
	}

	if int64(len(data)) > maxBytes {
		return nil, fmt.Errorf("larger than the %d-byte cap", maxBytes)
	}

	return data, nil
}
