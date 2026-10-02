// nugetPackage.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"path/filepath"

	"go2cs/internal/sourcemeta"
)

// nugetFlatContainerURL is NuGet's flat-container base. A variable so a test can point it at a fixture.
var nugetFlatContainerURL = "https://api.nuget.org/v3-flatcontainer"

// nugetPackageMaxBytes caps one downloaded nupkg; a variable only so a test can lower it.
var nugetPackageMaxBytes int64 = 256 << 20

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

// nugetPackageCandidates returns the published versions a conversion of moduleVersion can carry.
func nugetPackageCandidates(moduleVersion string, published []string) ([]string, error) {
	return nil, errors.New("not implemented")
}

// nugetPackageChoice is the package selected for a mapped module.
type nugetPackageChoice struct {
	nugetID        string
	packageVersion string
	contentHash    string
	description    sourcemeta.Description
}

// selectNuGetPackage selects the published package version a mapped module is substituted by.
func selectNuGetPackage(nugetID string, mod thirdPartyModule, locked nugetLockEntry, haveLock bool, refresh bool) (nugetPackageChoice, []string, error) {
	return nugetPackageChoice{}, nil, errors.New("not implemented")
}
